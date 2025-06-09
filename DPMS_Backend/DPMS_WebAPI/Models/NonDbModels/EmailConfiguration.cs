namespace DPMS_WebAPI.Models.NonDbModels
{
    public abstract class EmailConfiguration
    {
        public string DefaultFromEmail { get; set; }
        public string DefaultFromName { get; set; }
    }

    public class GmailConfiguration : EmailConfiguration
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; } = true;
    }

    public class SendGridConfiguration : EmailConfiguration
    {
        public string ApiKey { get; set; }
    }
}
