namespace DPMS.EmailEngine.EmailTemplates
{
    public class AccountCredentialsVM
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }

        public string DpmsLoginUrl { get; set; }
    }
}
