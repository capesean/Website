using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message, string bodyHtml = null);
    }

    public class EmailSender : IEmailSender
    {
        private readonly Settings _settings;

        public EmailSender(Settings settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string email, string subject, string bodyText, string bodyHtml = null)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.To.Add(new MailboxAddress(email));
            mimeMessage.From.Add(new MailboxAddress(_settings.EmailSettings.SenderName, _settings.EmailSettings.Sender));
            mimeMessage.Subject = subject;

            var html = System.IO.File.ReadAllText(System.IO.Path.Join(_settings.RootPath, "templates/email.html"));
            html = html.Replace("{rootUrl}", _settings.RootUrl);
            html = html.Replace("{title}", subject);
            if (bodyHtml == null) bodyHtml = bodyText;
            while (bodyHtml.IndexOf(Environment.NewLine + Environment.NewLine) >= 0)
                bodyHtml = bodyHtml.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            var lines = "<p>" + string.Join("</p><p>", bodyHtml.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)) + "</p>";
            html = html.Replace("{body}", lines);

            var builder = new BodyBuilder();
            builder.TextBody = bodyText;
            builder.HtmlBody = html;
            //builder.Attachments.Add(...);

            mimeMessage.Body = builder.ToMessageBody();

            if (_settings.IsDevelopment || !string.IsNullOrWhiteSpace(_settings.EmailSettings.SubstitutionEmailAddress))
            {
                // substitute all TO emails
                var replacements = new List<MailboxAddress>();
                foreach (var address in mimeMessage.To) replacements.Add(new MailboxAddress(address.Name, _settings.EmailSettings.SubstitutionEmailAddress));
                mimeMessage.To.Clear();
                foreach (var address in replacements) mimeMessage.To.Add(address);
                // substitute all CC emails
                replacements = new List<MailboxAddress>();
                foreach (var address in mimeMessage.Cc) replacements.Add(new MailboxAddress(address.Name, _settings.EmailSettings.SubstitutionEmailAddress));
                mimeMessage.Cc.Clear();
                foreach (var address in replacements) mimeMessage.Cc.Add(address);
                // substitute all BCC emails
                replacements = new List<MailboxAddress>();
                foreach (var address in mimeMessage.Bcc) replacements.Add(new MailboxAddress(address.Name, _settings.EmailSettings.SubstitutionEmailAddress));
                mimeMessage.Bcc.Clear();
                foreach (var address in replacements) mimeMessage.Bcc.Add(address);
            }

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(_settings.EmailSettings.MailServer);

                    await client.AuthenticateAsync(_settings.EmailSettings.Sender, _settings.EmailSettings.Password);

                    await client.SendAsync(mimeMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch (System.Exception err)
            {
                //todo: log error
                throw err;
            }
        }
    }

}
