using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Consent
{
    public class ConsentBannerVM
    {
        public string? Header { get; set; }
        public string? Url { get; set; }
        public List<Models.Purpose>? Purposes { get; set; } // Purpose list
    }
}
