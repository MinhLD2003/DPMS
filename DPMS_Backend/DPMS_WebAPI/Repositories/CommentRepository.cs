namespace DPMS_WebAPI.Repositories
{
    using DPMS_WebAPI.Interfaces.Repositories;
    using DPMS_WebAPI.Models;

    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(DPMSContext context) : base(context)
        {
        }
    }
}