﻿namespace WEB.Models
{
    public class Settings
    {
        public string RootUrl { get; set; }
    }
    public class EmailSettings
    {
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string SenderName { get; set; }
        public string Sender { get; set; }
        public string Password { get; set; }
        public string SubstitutionEmailAddress { get; set; }
    }
}