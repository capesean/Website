using System;
using WEB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http.Extensions;
using System.IO;

namespace WEB.Error
{
    public static class Logger
    {
        public static void Log(ExceptionContext context, ApplicationDbContext db, Settings settings, IEmailSender emailSender)
        {
            if (context.Exception == null) return;
            if (context.Exception.Message == "A task was canceled.") return;

            var request = context.HttpContext.Request;
            var url = request.HttpContext.Request.GetEncodedUrl();
            var userName = context.HttpContext.User.Identity.Name;
            var errorMessage = context.Exception.Message;
            //var form = context.Request.Content.ReadAsStringAsync().Result;

            string form = string.Empty;

            if (request.Method == "POST" && string.IsNullOrWhiteSpace(form))
            {
                foreach (var key in request.Form.Keys)
                    form += key + ":" + request.Form[key] + Environment.NewLine;

                using (StreamReader sr = new StreamReader(request.Body))
                {
                    if (request.Body.CanSeek) request.Body.Seek(0, SeekOrigin.Begin);
                    if (request.Body.CanRead) form = sr.ReadToEnd();
                }
            }

            if (!string.IsNullOrWhiteSpace(form) && form.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                form = "<REMOVED DUE TO PASSWORD SENSITIVITY>";
            }

            //var entityValidationError = (string)null;
            //try
            //{
            //    if (exc is DbEntityValidationException)
            //    {
            //        foreach (var eve in ((DbEntityValidationException)exc).EntityValidationErrors)
            //        {
            //            entityValidationError += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            //                eve.Entry.Entity.GetType().Name, eve.Entry.State) + Environment.NewLine;

            //            foreach (var ve in eve.ValidationErrors)
            //            {
            //                entityValidationError += ve.PropertyName + " : " + ve.ErrorMessage + Environment.NewLine;
            //            }
            //        }
            //    }
            //}
            //catch { }

            var error = new Models.Error
            {
                Id = Guid.NewGuid(),
                DateUtc = DateTime.UtcNow,
                Message = errorMessage,
                //EntityValidationErrors = entityValidationError,
                Url = url,
                UserName = userName,
                Form = form
            };

            error.Exception = ProcessExceptions(error, context.Exception);
            error.ExceptionId = error.Exception.Id;

            try
            {
                db.Entry(error).State = EntityState.Added;
                var exception = error.Exception;
                while (exception != null)
                {
                    db.Entry(exception).State = EntityState.Added;
                    exception = exception.InnerException;
                }
                db.SaveChanges();
            }
            catch(Exception e) {
            
            }


            if (!string.IsNullOrWhiteSpace(settings.EmailSettings.EmailToErrors))
            {
                var body = string.Empty;
                body += "URL: " + url + Environment.NewLine;
                body += "DATE: " + DateTime.Now.ToString("dd MMMM yyyy, HH:mm:ss") + Environment.NewLine;
                body += "USER: " + userName + Environment.NewLine;
                body += "MESSAGE: " + errorMessage + Environment.NewLine;// + Environment.NewLine + entityValidationError;
                body += Environment.NewLine;

                var exception = error.Exception;
                while (exception != null)
                {
                    body += "INNER EXCEPTION: " + exception.Message + Environment.NewLine;
                    body += Environment.NewLine;
                    exception = exception.InnerException;
                }

                body += settings.RootUrl + "api/errors/" + error.Id + Environment.NewLine;

                try
                {
                    emailSender.SendEmailAsync(settings.EmailSettings.EmailToErrors, settings.EmailSettings.EmailToErrors, settings.SiteName + " Error", body).Wait();
                }
                catch { }
            }
        }

        public static ErrorException ProcessExceptions(Models.Error error, Exception exception)
        {
            if (exception == null) return null;

            ErrorException innerException = null;

            if (exception.InnerException != null)
                innerException = ProcessExceptions(error, exception.InnerException);

            var errorException = new ErrorException
            {
                Id = Guid.NewGuid(),
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = innerException,
                InnerExceptionId = innerException?.InnerExceptionId
            };

            return errorException;
        }

    }
}
