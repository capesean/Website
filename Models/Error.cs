using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class Error
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime DateUtc { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Message { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string EntityValidationErrors { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Url { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Form { get; set; }

        [MaxLength(256)]
        public string UserName { get; set; }

        [MaxLength(10)]
        public string Method { get; set; }

        public Guid ExceptionId { get; set; }
        public virtual ErrorException Exception { get; set; }
    }

    public class ErrorException
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Message { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string StackTrace { get; set; }

        public Guid? InnerExceptionId { get; set; }
        public virtual ErrorException InnerException { get; set; }
    }

    public class ApiException : ExceptionFilterAttribute, IFilterMetadata
    {
        Settings _s;
        IEmailSender _es;
        ApplicationDbContext _db;

        public ApiException(Settings settings, IEmailSender emailSender, ApplicationDbContext db)
        {
            _s = settings;
            _es = emailSender;
            _db = db;
        }

        public override void OnException(ExceptionContext context)
        {
            WEB.Error.Logger.Log(context, _db, _s, _es);
            base.OnException(context);
        }
    }


}