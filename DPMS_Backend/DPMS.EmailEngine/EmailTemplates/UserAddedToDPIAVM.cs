namespace DPMS.EmailEngine.EmailTemplates
{
    public class UserAddedToDPIAVM
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; } // Bussiness Owner or Product Developer
        public string DPIATitle { get; set; }
        public string SystemName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}