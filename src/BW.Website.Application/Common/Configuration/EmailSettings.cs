namespace BW.Website.Application.Common.Configuration
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; }
        public string FromAddress { get; set; } = string.Empty;
    }
}
