using System.Linq;

namespace WEB.Models
{
    public class Settings
    {
        public string RootUrl { get; set; }
        public string RootPath { get; set; }
        public string SiteName { get; set; }
        public bool IsDevelopment { get; set; }
        public EmailSettings EmailSettings { get; set; }
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryMinutes { get; set; }
        public string CertificatePassword { get; set; }
        public string ProxyToSpaDevelopmentServer { get; set; }

        public DbSettings GetDbSettings(ApplicationDbContext db)
        {
            return db.Settings.Single();
        }
    }

    public class EmailSettings
    {
        public string SenderName { get; set; }
        public string Sender { get; set; }
        public string SendGridKey { get; set; }
        public string SubstitutionEmailAddress { get; set; }
        public string EmailToErrors { get; set; }
        public bool SendEmails { get; set; }
        public bool SendErrorEmails { get; set; }
    }

}
