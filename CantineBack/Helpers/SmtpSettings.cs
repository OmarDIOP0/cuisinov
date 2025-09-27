namespace CantineBack.Helpers
{
    public class SmtpSettings
    {

        public string SmtpUser { get; set; }
        public string Email { get; set; }
        public string SmtpPassword { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public bool SmtpUseDefaultCredentials { get; set; }

    }
}
