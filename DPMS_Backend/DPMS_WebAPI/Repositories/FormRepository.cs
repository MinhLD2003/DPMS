using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Form;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DPMS_WebAPI.Repositories
{
    public class FormRepository : BaseRepository<Form>, IFormRepository
    {
        public FormRepository(DPMSContext context) : base(context)
        {
        }

        public async Task<List<ExportFICSubmissionVM>> GetFicExportData(Guid submissionId)
        {
            SqlConnection connection = _context.Database.GetDbConnection() as SqlConnection;
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand("dbo.[sp_dpms_export_fic_submission]", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // add parameters
                command.Parameters.Add(new SqlParameter { ParameterName = "SubmissionId", SqlDbType = SqlDbType.UniqueIdentifier, Value = submissionId });

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    List<ExportFICSubmissionVM> model = new List<ExportFICSubmissionVM>();
                    while (reader.Read())
                    {
                        model.Add(new ExportFICSubmissionVM()
                        {
                            Id = reader.GetGuid(0),
                            ParentId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                            FormId = reader.GetGuid(2),
                            Name = reader.GetString(3),
                            OrderIndex = reader.GetInt32(4),
                            HierarchyLevel = reader.GetInt32(5),
                            SoftPath = reader.GetString(6),
                            SubmissionId = reader.IsDBNull(7) ? null : reader.GetGuid(7),
                            FormElementValue = reader.IsDBNull(8) ? null : reader.GetString(8),
                        });
                    }

                    await connection.CloseAsync();
                    return model;
                }
            }
        }

        public async Task<Submission> GetSubmissionDetailsAsync(Guid submissionId)
        {
            Submission submission = await _context.Submissions
                .Include(s => s.Form)
                .Include(s => s.ExternalSystem)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
            if (submission == null)
            {
                throw new Exception("Submission does not exist");
            }

            return submission;
        }
    }
}
