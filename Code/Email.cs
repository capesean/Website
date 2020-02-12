using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string bodyText, string bodyHtml = null, bool isErrorEmail = false);
    }

    public class EmailSender : IEmailSender
    {
        private readonly Settings _settings;

        public EmailSender(Settings settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string bodyText, string bodyHtml = null, bool isErrorEmail = false)
        {
            if (!isErrorEmail && !_settings.EmailSettings.SendEmails) return;
            if (isErrorEmail && !_settings.EmailSettings.SendErrorEmails) return;

            var html = System.IO.File.ReadAllText(System.IO.Path.Join(_settings.RootPath, "wwwroot/templates/email.html"));
            html = html.Replace("{rootUrl}", _settings.RootUrl);
            html = html.Replace("{title}", subject);
            if (bodyHtml == null) bodyHtml = bodyText;
            while (bodyHtml.IndexOf(Environment.NewLine + Environment.NewLine) >= 0)
                bodyHtml = bodyHtml.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            var lines = "<p>" + string.Join("</p><p>", bodyHtml.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)) + "</p>";
            html = html.Replace("{body}", lines);

            var client = new SendGridClient(_settings.EmailSettings.SendGridKey);
            var from = new EmailAddress(_settings.EmailSettings.Sender, _settings.EmailSettings.SenderName);
            var to = new EmailAddress(toEmail, toName);
            if (!String.IsNullOrWhiteSpace(_settings.EmailSettings.SubstitutionEmailAddress)) to.Email = _settings.EmailSettings.SubstitutionEmailAddress;
            var plainTextContent = bodyText;
            var htmlContent = html;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                throw new Exception("Failed to send mail");

        }
    }

}
