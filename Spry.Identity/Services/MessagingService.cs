using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Spry.BuildingBlocks.EventBus.Abstractions;
using Spry.Identity.Infrastructure.IntegrationEvents;
using System.Dynamic;
using System.Text;

namespace Spry.Identity.Services
{
#nullable disable
    public class MessagingService(IEventBus eventBus,
        IConfiguration configuration,
        ILogger<MessagingService> logger)
    {
        public void SendMail(MailInfo info,
            CopiedMailInfo copiedMailInfo = null)
        {
            var emailTask = new Email_Task
            {
                Receiver = info.RxEmail,
                ReceiverName = info.RxName,
                Template = info.EmailTemplate,
                Subject = info.Subject,
                TemplateLocale = info.EmailTemplateLocale,
                Content = JsonConvert.SerializeObject(info.Content),
                Sender = configuration["EmailSender"],
                SenderName = configuration["EmailSenderName"],
                AttachmentContent = info.AttachmentContent,
                AttachmentFileName = info.AttachmentFileName,
                AttachmentMimeType = info.AttachmentMimeType,
            };

            if (info.Ccs.Any())
                emailTask.SetCcs(info.Ccs.ToList());

            eventBus.PublishTask(emailTask);
            logger.LogInformation("Email event to {Reciever} sent.", info.RxEmail);

            if (copiedMailInfo != null)
            {
                var copyEmailTask = new Email_Task
                {
                    Receiver = copiedMailInfo.RxEmail,
                    ReceiverName = copiedMailInfo.RxName,
                    Template = copiedMailInfo.EmailTemplate,
                    TemplateLocale = copiedMailInfo.EmailTemplateLocale,
                    Content = JsonConvert.SerializeObject(copiedMailInfo.Content),
                    Sender = configuration["EmailSender"],
                    SenderName = configuration["EmailSenderName"]
                };

                if (info.Ccs.Any())
                    emailTask.SetCcs(info.Ccs.ToList());

                eventBus.PublishTask(copyEmailTask);
                logger.LogInformation("Email event to {Reciever} sent.", info.RxEmail);
            }
        }

        public void SendMails(IEnumerable<MailInfo> infos)
        {
            foreach (var info in infos)
            {
                var emailTask = new Email_Task
                {
                    Receiver = info.RxEmail,
                    ReceiverName = info.RxName,
                    Template = info.EmailTemplate,
                    TemplateLocale = info.EmailTemplateLocale,
                    Content = JsonConvert.SerializeObject(info.Content),
                    Sender = configuration["EmailSender"],
                    SenderName = configuration["EmailSenderName"]
                };

                if (info.Ccs.Any())
                    emailTask.SetCcs(info.Ccs.ToList());

                eventBus.PublishTask(emailTask);
                logger.LogInformation("Email event to {Reciever} sent.", info.RxEmail);
            }
        }

        public void SendSms(Sms_Task sms_Task)
        {
            eventBus.PublishTask(sms_Task);
        }
    }

    #region classes
    public class MailInfo
    {
        /// <summary>
        /// Receiver email
        /// </summary>
        public string RxEmail { get; set; }

        /// <summary>
        /// Reciever name
        /// </summary>
        public string RxName { get; set; }
        public string Subject { get; set; }
        public string EmailTemplate { get; set; }
        public string EmailTemplateLocale { get; set; }
        public object Content { get; set; }

        public string AttachmentContent { get; set; }
        public string AttachmentFileName { get; set; }
        public string AttachmentMimeType { get; set; }
        public IEnumerable<EmailIdentity> Ccs { get; set; } = Enumerable.Empty<EmailIdentity>();
    }

    public class CopiedMailInfo : MailInfo
    {

    }

    /// <summary>
    /// Warning: Do not capitalize properties
    /// </summary>
    public class MailContent
    {
        public string first_name { get; set; }
        public string company_name { get; set; }
        public string username { get; set; }
        public string domain { get; set; }
        /// <summary>
        /// email of requester
        /// </summary>
        public string password { get; set; }
        public string role { get; set; }
        public string employer_token { get; set; }
    }

    /// <summary>
    /// Warning: Do not capitalize properties
    /// </summary>
    public class CopyMailContent : MailContent
    {
        public string requester { get; set; }
        public bool isCopy { get; private set; } = true;
    }

    public class EmailIdentity
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
    #endregion classes

}
