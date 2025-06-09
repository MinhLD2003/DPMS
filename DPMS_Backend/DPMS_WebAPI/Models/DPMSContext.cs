using AutoMapper.Execution;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels.Form;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Models
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class DPMSContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public DPMSContext(DbContextOptions<DPMSContext> options, IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<GroupFeature> GroupFeatures { get; set; }
        public DbSet<ExternalSystem> ExternalSystems { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<FormElement> FormElements { get; set; }
        public DbSet<DPIA> DPIAs { get; set; }
        public DbSet<IssueTicket> IssueTickets { get; set; }
        public DbSet<IssueTicketDocument> IssueTicketDocuments { get; set; }
        public DbSet<FormResponse> FormResponses { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Purpose> Purposes { get; set; }
        public DbSet<ExternalSystemPurpose> ExternalSystemPurposes { get; set; }
        public DbSet<Consent> Consents { get; set; }
        public DbSet<ConsentPurpose> ConsentPurposes { get; set; }
        public DbSet<ConsentToken> ConsentTokens { get; set; }
        public DbSet<DPIAMember> DPIAMembers { get; set; }
        public DbSet<DPIADocument> DPIADocuments { get; set; }
        public DbSet<MemberResponsibility> MemberResponsibilities { get; set; }
        public DbSet<DPIAResponsibility> DPIAResponsibilities { get; set; }
        public DbSet<Responsibility> Responsibilities { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<DPIAEvent> DPIAEvents { get; set; }
        public DbSet<PrivacyPolicy> PrivacyPolicies { get; set; }
        public DbSet<DSAR> DSARs { get; set; }
        public DbSet<Risk> Risks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UNCOMMENT THIS in case you want to configure AuditLogs for a specific table
            //modelBuilder.Entity<DPIA>()
            //    .ToTable(tb => tb.UseSqlOutputClause(false));

            // Define the relationships for CreatedUser and LastModifiedUser
            modelBuilder.Entity<User>()
                .HasOne(u => u.CreatedBy)  // A User is created by another User
                .WithMany()  // A User can create many Users
                .HasForeignKey(u => u.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);  // Restrict delete to maintain integrity

            modelBuilder.Entity<User>()
                .HasOne(u => u.LastModifiedBy)  // A User can be last modified by another User
                .WithMany()  // A User can modify many Users
                .HasForeignKey(u => u.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);  // Restrict delete to maintain integrity

            modelBuilder.Entity<Group>()
                .HasOne(u => u.CreatedBy)
                .WithMany()
                .HasForeignKey(u => u.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasOne(u => u.LastModifiedBy)
                .WithMany()
                .HasForeignKey(u => u.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasMany(u => u.Users)
                .WithMany(u => u.Groups)
                .UsingEntity<UserGroup>();

            modelBuilder.Entity<UserGroup>()
                .HasOne(u => u.CreatedBy)
                .WithMany()
                .HasForeignKey(u => u.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserGroup>()
                .HasOne(u => u.LastModifiedBy)
                .WithMany()
                .HasForeignKey(u => u.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserGroup>()
                .HasKey(ug => ug.Id);
            // modelBuilder.Entity<UserGroup>()
            // 	.HasKey(ug => new { ug.UserId, ug.GroupId }); // composite key
            modelBuilder.Entity<UserGroup>()
                .HasIndex(ug => new { ug.UserId, ug.GroupId })
                .IsUnique();

            modelBuilder.Entity<UserGroup>()
                .HasOne(uig => uig.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(uig => uig.UserId);

            modelBuilder.Entity<UserGroup>()
                .HasOne(uig => uig.Group)
                .WithMany(ug => ug.UserGroups)
                .HasForeignKey(uig => uig.GroupId);

            modelBuilder.Entity<Feature>()
                .HasOne(u => u.CreatedBy)
                .WithMany()
                .HasForeignKey(u => u.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feature>()
                .HasOne(u => u.LastModifiedBy)
                .WithMany()
                .HasForeignKey(u => u.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupFeature>()
                .HasOne(u => u.CreatedBy)
                .WithMany()
                .HasForeignKey(u => u.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupFeature>()
                .HasOne(u => u.LastModifiedBy)
                .WithMany()
                .HasForeignKey(u => u.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to maintain integrity 

            modelBuilder.Entity<Feature>()
                .HasOne(u => u.Parent)
                .WithMany(f => f.Children)
                .HasForeignKey(f => f.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupFeature>()
                .HasKey(gf => gf.Id);

            modelBuilder.Entity<GroupFeature>()
                .HasIndex(gf => new { gf.GroupId, gf.FeatureId })
                .IsUnique();

            modelBuilder.Entity<Group>()
                .HasMany(u => u.Features)
                .WithMany(u => u.Groups)
                .UsingEntity<GroupFeature>();

            modelBuilder.Entity<GroupFeature>()
                .HasOne(uig => uig.Group)
                .WithMany(u => u.GroupFeatures)
                .HasForeignKey(uig => uig.GroupId);

            modelBuilder.Entity<GroupFeature>()
                .HasOne(uig => uig.Feature)
                .WithMany(ug => ug.GroupFeatures)
                .HasForeignKey(uig => uig.FeatureId);

            modelBuilder.Entity<ExternalSystem>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExternalSystem>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExternalSystem>()
                .HasMany(s => s.FormSubmission)
                .WithOne(f => f.ExternalSystem)
                .HasForeignKey(f => f.SystemId)
                .IsRequired(false);

            // modelBuilder.Entity<ExternalSystem>()
            //     .HasMany<User>(s => s.Users) // one External System has many Users belong to it
            //     .WithMany(u => u.ExternalSystems) // one Users may belongs to many External System
            //     .UsingEntity<UserPermission>(
            //         j =>
            //         {
            //             j.Property(e => e.IsPic).HasDefaultValue(false);
            //         }

            modelBuilder.Entity<ExternalSystem>()
                .HasMany(e => e.Groups)
                .WithOne(g => g.System)
                .HasForeignKey(g => g.SystemId)
                .IsRequired(false);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.System)
                .WithMany(s => s.Groups)
                .HasForeignKey(g => g.SystemId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to maintain integrity

            // modelBuilder.Entity<DPIA>()
            //     .HasMany(d => d.Groups)
            //     .WithOne(g => g.DPIA)
            //     .HasForeignKey(g => g.DPIAId)
            //     .IsRequired(false);

            // modelBuilder.Entity<Group>()
            //     .HasOne(g => g.DPIA)
            //     .WithMany(d => d.Groups)
            //     .HasForeignKey(g => g.DPIAId)
            //     .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DPIAMember>()
                .HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIAMember>()
                .HasOne(d => d.LastModifiedBy)
                .WithMany()
                .HasForeignKey(d => d.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIAMember>()
                .HasKey(dm => dm.Id);

            modelBuilder.Entity<DPIAMember>()
                .HasIndex(dm => new { dm.DPIAId, dm.UserId })
                .IsUnique();

            modelBuilder.Entity<DPIAMember>()
                .HasOne(dm => dm.DPIA)
                .WithMany(d => d.DPIAMembers)
                .HasForeignKey(dm => dm.DPIAId);

            modelBuilder.Entity<DPIAMember>()
                .HasOne(dm => dm.User)
                .WithMany(u => u.DPIAs)
                .HasForeignKey(dm => dm.UserId);

            modelBuilder.Entity<DPIAMember>()
                .HasMany(dm => dm.Responsibilities)
                .WithOne(t => t.Member)
                .HasForeignKey(t => t.MemberId);

            //modelBuilder.Entity<DPIAMember>()
            //    .HasMany(dm => dm.DPIAResponsibility)
            //    .WithMany(dr => dr.DPIAMembers)
            //    .UsingEntity<MemberResponsibility>(
            //        l=>l.HasOne<DPIAMember>(e=>e.Member).WithMany(m=>m.DPIAResponsibility.AsEnumerable()),

            //    );

            // MemberTask and DPIAMember (Many-to-One)
            modelBuilder.Entity<MemberResponsibility>()
                .HasOne(mt => mt.Member)
                .WithMany(m => m.Responsibilities)
                .HasForeignKey(mt => mt.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MemberResponsibility>()
                .HasOne(mt => mt.DPIAResponsibility)
                .WithMany(dr => dr.MemberResponsibilities)
                .HasForeignKey(mt => mt.DPIAResponsibilityId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<DPIA>()
                .HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIA>()
                .HasOne(d => d.LastModifiedBy)
                .WithMany()
                .HasForeignKey(d => d.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIA>()
                .HasMany(d => d.Documents)
                .WithOne(d => d.DPIA)
                .HasForeignKey(d => d.DPIAId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DPIA>()
                .HasOne(d => d.ExternalSystem)
                .WithMany()
                .HasForeignKey(d => d.ExternalSystemId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIA>()
                .HasMany(d => d.DPIAMembers)
                .WithOne(d => d.DPIA)
                .HasForeignKey(d => d.DPIAId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove members when DPIA is deleted

            modelBuilder.Entity<DPIAResponsibility>()
                .HasOne(dr => dr.DPIA)
                .WithMany(d => d.Responsibilities)
                .HasForeignKey(dr => dr.DPIAId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove responsibilities when DPIA is deleted


            modelBuilder.Entity<DPIAResponsibility>()
                .HasOne(dr => dr.Responsibility)
                .WithMany()
                .HasForeignKey(dr => dr.ResponsibilityId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to maintain integrity

            modelBuilder.Entity<Form>()
                .HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Form>()
                .HasOne(f => f.LastModifiedBy)
                .WithMany()
                .HasForeignKey(f => f.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FormElement>()
                .HasOne(fe => fe.CreatedBy)
                .WithMany()
                .HasForeignKey(fe => fe.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FormElement>()
                .HasOne(fe => fe.LastModifiedBy)
                .WithMany()
                .HasForeignKey(fe => fe.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Submission>()
                .HasMany(fs => fs.SubmissionElements)
                .WithOne(fr => fr.Submission)
                .HasForeignKey(fr => fr.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove elements when submission is deleted

            modelBuilder.Entity<Submission>()
                .HasOne(fs => fs.CreatedBy)
                .WithMany()
                .HasForeignKey(fs => fs.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Submission>()
                .HasOne(fs => fs.LastModifiedBy)
                .WithMany()
                .HasForeignKey(fs => fs.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FormResponse>()
                .HasOne(fr => fr.CreatedBy)
                .WithMany()
                .HasForeignKey(fr => fr.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FormResponse>()
                .HasOne(fr => fr.LastModifiedBy)
                .WithMany()
                .HasForeignKey(fr => fr.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FormElement>()
                .HasOne(e => e.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<IssueTicket>(entity =>
            {
                entity.HasOne(e => e.ExternalSystem)
                       .WithMany()
                       .HasForeignKey(e => e.ExternalSystemId)
                       .IsRequired(false)
                       .OnDelete(DeleteBehavior.SetNull); // Set to null if the external system is deleted
            });

            modelBuilder.Entity<IssueTicket>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IssueTicket>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IssueTicketDocument>(entity =>
            {
                entity.HasOne(d => d.CreatedBy)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.LastModifiedBy)
                    .WithMany()
                    .HasForeignKey(d => d.LastModifiedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<IssueTicketDocument>(entity =>
            {
                entity.HasOne(d => d.CreatedBy)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.LastModifiedBy)
                    .WithMany()
                    .HasForeignKey(d => d.LastModifiedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.IssueTicket)
                    .WithMany(i => i.Documents)
                    .HasForeignKey(e => e.IssueTicketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DPIADocument>(entity =>
            {
                entity.HasOne(d => d.CreatedBy)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.LastModifiedBy)
                    .WithMany()
                    .HasForeignKey(d => d.LastModifiedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // modelBuilder.Entity<Document>()
            //       .HasOne<IssueTicket>()
            //       .WithMany(i => i.Documents)
            //       .HasForeignKey(d => d.RelatedId)
            //       .IsRequired(false);

            modelBuilder.Entity<Purpose>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purpose>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExternalSystemPurpose>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExternalSystemPurpose>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConsentPurpose>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConsentPurpose>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Consent>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consent>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            // modelBuilder.Entity<DPIAEvent>()
            //     .HasOne(e => e.DPIA)
            //     .WithMany()
            //     .HasForeignKey(e => e.DPIAId)
            //     .OnDelete(DeleteBehavior.Cascade);

            // User and Events (Many-to-One)
            modelBuilder.Entity<DPIAEvent>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete to maintain integrity

            modelBuilder.Entity<DPIAEvent>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIAEvent>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIAEvent>()
                .HasOne(e => e.DPIA)
                .WithMany(d => d.Events)
                .HasForeignKey(e => e.DPIAId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove events when DPIA is deleted

            modelBuilder.Entity<Comment>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(e => e.LastModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DPIAResponsibility>()
                .HasIndex(dr => new { dr.DPIAId, dr.ResponsibilityId })
                .IsUnique();

            modelBuilder.Entity<MemberResponsibility>()
                .HasIndex(mr => new { mr.MemberId, mr.DPIAResponsibilityId })
                .IsUnique();

        }
        public override int SaveChanges()
        {
            SetTimestamps();
            SetUser();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            SetUser();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseModel>(); // Gets all entities that inherit from BaseModel
            DateTime currentTime = DateTime.Now;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = currentTime;
                    entry.Entity.LastModifiedAt = currentTime;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedAt = currentTime;
                }
            }
        }

        private void SetUser()
        {
            var entries = ChangeTracker.Entries<BaseModel>(); // Gets all entities that inherit from BaseModel
            var identityService = _serviceProvider.GetRequiredService<IdentityService>();
            var userId = Guid.Empty;

            try
            {
                userId = identityService.GetCurrentUserId();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedById = userId;
                    entry.Entity.LastModifiedById = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedById = userId;
                }
            }
        }
    }
}
