using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.ExternalSystem
{
    public class UpdateSystemVM
    {
        public string Name { get; set; }
        [MaxLength(255)]
        public string Domain { get; set; }
        public string Description { get; set; }
    }
}