using DPMS_WebAPI.AuthPolicies;
using DPMS_WebAPI.Builders;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.FileStorage;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Middlewares;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Models.NonDbModels;
using DPMS_WebAPI.Repositories;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Utils;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Quiz.EmailEngine;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Configuration;
using System.Text;
using System.Text.Json.Serialization;

#pragma warning disable CS1591
namespace DPMS_WebAPI
{

    public class Program
    {
        public static void Main(string[] args)
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration.WriteTo.Console();

            var appSettings = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var logDb = appSettings["ConnectionStrings:DpmsLogs"];
            var sinkOptions = new MSSqlServerSinkOptions
            {
                TableName = "SerilogEvents",
                AutoCreateSqlTable = true,
                AutoCreateSqlDatabase = true,
            };
            var columnOpts = new ColumnOptions();

            loggerConfiguration.WriteTo.MSSqlServer(connectionString: logDb, sinkOptions: sinkOptions);
            // loggerConfiguration.WriteTo.MSSqlServer(connectionString: logDb, sinkOptions: sinkOptions, columnOptions: columnOpts, appConfiguration: appSettings);
            loggerConfiguration.WriteTo.Seq("http://localhost:5341");

            Log.Logger = loggerConfiguration.CreateLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                var configuration = builder.Configuration;

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddMvcCore().AddRazorViewEngine(); // Add RazorViewEnginer for EmailEngine
                builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
                builder.Services.AddHttpClient();

                // services for logging all HTTP requests
                //builder.Services.AddHttpLogging(o => { });
                //builder.Services.AddHttpLoggingInterceptor<LoggingInterceptorService>();

                builder.Services.AddSerilog();

                //  Config hangfire to use same db with dpmslogs
                builder.Services.AddHangfire(config =>
                    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DpmsLogs")));
                // Register the Hangfire Server with DI
                builder.Services.AddHangfireServer(options =>
                {
                    options.WorkerCount = 5;  // Set the number of worker threads
                    options.Queues = new[] { "default", "critical" };  // Set queues to 
                });
                // Register repositories
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<IFileRepository, S3FileStorage>();
                builder.Services.AddScoped<IEventMessageBuilder, DPIAEventMessageBuilder>();


                // Register Repositories
                builder.Services.AddScoped<IGroupRepository, GroupRepository>();
                builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
                builder.Services.AddScoped<IIssueTicketRepository, IssueTicketRepository>();
                builder.Services.AddScoped<IExternalSystemRepository, ExternalSystemRepository>();
                builder.Services.AddScoped<IPurposeRepository, PurposeRepository>();
                builder.Services.AddScoped<IssueTicketDocumentRepository, IssueTicketDocumentRepository>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<IFormRepository, FormRepository>();
                builder.Services.AddScoped<IExternalSystemPurposeRepository, ExternalSystemPurposeRepository>();
                builder.Services.AddScoped<IDPIAMemberRepository, DPIAMemberRepository>();
                builder.Services.AddScoped<IDPIARepository, DPIARepository>();
                builder.Services.AddScoped<IConsentRepository, ConsentRepository>();
                builder.Services.AddScoped<IConsentPurposeRepository, ConsentPurposeRepository>();
                builder.Services.AddScoped<IConsentTokenRepository, ConsentTokenRepository>();
                builder.Services.AddScoped<IResponsibilityRepository, ResponsibilityRepository>();
                builder.Services.AddScoped<IPrivacyPolicyRepository, PrivacyPolicyRepository>();
                builder.Services.AddScoped<IDsarRepository, DsarRepository>();
                builder.Services.AddScoped<IMemberResponsibilityRepository, MemberResponsibilityRepository>();
                builder.Services.AddScoped<IDPIAResponsibilityRepository, DPIAResponsibilityRepository>();
                builder.Services.AddScoped<IRiskRepository, RiskRepository>();
                // Register Services
                builder.Services.AddScoped<IGroupService, GroupService>();
                builder.Services.AddScoped<IFeatureService, FeatureService>();
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                builder.Services.AddScoped<IExternalSystemService, ExternalSystemService>();
                builder.Services.AddScoped<IIssueTicketService, IssueTicketService>();
                builder.Services.AddScoped<IPurposeService, PurposeService>();
                builder.Services.AddScoped<IIssueTicketDocumentService, IssueTicketDocumentService>();
                builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
                builder.Services.AddScoped<IFormService, FormService>();
                builder.Services.AddScoped<IDPIAService, DPIAService>();
                builder.Services.AddScoped<IConsentService, ConsentService>();
                builder.Services.AddScoped<IConsentPurposeService, ConsentPurposeService>();
                builder.Services.AddScoped<IResponsibilityService, ResponsibilityService>();
                builder.Services.AddScoped<IPrivacyPolicyService, PrivacyPolicyService>();
                builder.Services.AddScoped<IDsarService, DsarService>();
                builder.Services.AddScoped<IRiskService, RiskService>();
                // Configure email sender
                // Register both configurations
                builder.Services.Configure<SendGridConfiguration>(builder.Configuration.GetSection("EmailConfig:SendGrid"));
                builder.Services.Configure<GmailConfiguration>(builder.Configuration.GetSection("EmailConfig:Gmail"));

                // Register concrete implementations
                builder.Services.AddScoped<SendGridConfiguration>(sp => sp.GetRequiredService<IOptions<SendGridConfiguration>>().Value);
                builder.Services.AddScoped<GmailConfiguration>(sp => sp.GetRequiredService<IOptions<GmailConfiguration>>().Value);

                // Choose which implementation to use for the abstract type
                builder.Services.AddScoped<EmailConfiguration>(sp => sp.GetRequiredService<SendGridConfiguration>());

                builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();
                builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

                // Register Other Dependencies
                builder.Services.AddScoped<AuthService>();
                builder.Services.AddScoped<IdentityService>();
                builder.Services.AddScoped<FileStorageService>();
                builder.Services.AddScoped<IAuthorizationHandler, PolicyAuthorizationHandler>();
                // Add services to the container.
                builder.Services.AddDbContext<DPMSContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DpmsDb")));
                builder.Services.AddDbContext<DPMSLoggingContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DpmsLogs"));
                }, ServiceLifetime.Singleton);

                builder.Services.AddAutoMapper(typeof(Program));

                builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

                // builder.Services.AddAWSService<IAmazonS3>();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Data Privacy Management System",
                        Version = "v1",

                    });

                    var filePath = Path.Combine(AppContext.BaseDirectory, "DPMS_WebAPI.xml");
                    options.IncludeXmlComments(filePath);

                    var security = new OpenApiSecurityScheme
                    {
                        Name = HeaderNames.Authorization,
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header",
                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    };

                    options.AddSecurityDefinition(security.Reference.Id, security);
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        { security, Array.Empty<string>() }
                    });
                });

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowFrontend",
                        builder =>
                        {
                            builder.WithOrigins("http://localhost:5173", "https://g50-sep490-spr25-94a030.gitlab.io")
                                   .AllowCredentials() // Allow cookies (important for auth)
                                   .AllowAnyMethod()
                                   .AllowAnyHeader();
                        });
                    options.AddPolicy("DynamicCors", policy =>
                    policy.SetIsOriginAllowed(origin =>
                    {
                        using var scope = builder.Services.BuildServiceProvider().CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<DPMSContext>();
                        // Current not save in cache every time call api require dynamic cors
                        var domains = db.ExternalSystems.Select(es => es.Domain).ToList();
                        return domains.Contains(origin);
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
                });

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration["Jwt:Issuer"],
                            ValidAudience = configuration["Jwt:Issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty))
                        };
                    });

                builder.Services.AddAuthorization(options =>
                {
                    // 1 policy contains MANY requirements --> AND basis
                    options.AddPolicy(Policies.Authenticated, policy =>
                    {
                        policy.Requirements.Add(new PolicyRequirement(Policies.Authenticated));
                    });
                    options.AddPolicy(Policies.FeatureRequired, policy =>
                    {
                        policy.Requirements.Add(new PolicyRequirement(Policies.FeatureRequired));
                    });
                });

                // Initialize encryption utils with configuration
                ConsentUtils.Initialize(builder.Configuration);

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                // Apply the API Key middleware only for endpoints starting with "/api-consent".
                app.UseWhen(context => context.Request.Path.StartsWithSegments("/api-consent"), appBuilder =>
                {
                    appBuilder.UseMiddleware<ApiKeyMiddleware>();
                });
                app.UseWhen(context =>
                    context.Request.Path.StartsWithSegments("/api-cjs"),
                  branch =>
                  {
                      // Apply dynamiccors to /api-cjs
                      branch.UseCors("DynamicCors");
                  });
                //app.UseHttpLogging();

                //var mapper = app.Services.GetRequiredService<IMapper>();
                //mapper.ConfigurationProvider.AssertConfigurationIsValid();

                app.UseCors("AllowFrontend");

                //app.UseHttpsRedirection();

                app.UseAuthorization();

                app.UseHangfireDashboard();

                // Schedule the status update job to run every 12 hours
                RecurringJob.AddOrUpdate<DsarService>("update-dsar-status",
                    service => service.ChangeStatus(),
                    Cron.DayInterval(12));

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
