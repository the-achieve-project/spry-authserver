using Newtonsoft.Json;
using Spry.BuildingBlocks.EventBus.Abstractions;
using Spry.AuthServer.Infrastructure.IntegrationEvents;

namespace Spry.AuthServer.Services
{
#nullable disable
    public class MessagingService(IEventBus eventBus,
        IConfiguration configuration,
        ILogger<MessagingService> logger)
    {

        public void SendNewLoginNotice(string email, string userAgent)
        {
            var mail = new MailInfo
            {
                RxEmail = email,
                EmailTemplate = configuration["EmailTemplates:NewLoginNotice"],
                EmailTemplateLocale = configuration["EmailTemplates:NewLoginNotice"],
                Content = new
                {
                    email,
                    userAgent,
                }
            };

            SendMail(mail);
        }

        public void SendSMSNewLoginNotice(string phone, string userAgent)
        {
            eventBus.PublishTask(new Sms_Task
            {
                //From = _configuration["DefaultSmsHeader"], // set in messaging service
                To = phone,
                Text = $"Security alert.\n We noticed a new sign-in to your achieve account, {phone} on a {userAgent} device. \n " +
                $"If this was use ignore this message. If not visit your account to reset password."
            });
        }

        public void SendPasswordResetSuccess(string email, string firstName)
        {
            var mail = new MailInfo
            {
                RxEmail = email,
                RxName = firstName,
                EmailTemplate = configuration["EmailTemplates:PasswordChanged"],
                EmailTemplateLocale = configuration["EmailTemplates:PasswordChanged"],
                Content = new
                {
                    first_name = firstName,
                }
            };

            SendMail(mail);
        }
        
        public void SendOtp(string email, string firstName, string code)
        {
            var mail = new MailInfo
            {
                RxEmail = email,
                RxName = firstName,
                EmailTemplate = configuration["EmailTemplates:2fa"],
                EmailTemplateLocale = configuration["EmailTemplates:2fa"],
                Content = new
                {
                    first_name = firstName,
                    code
                }
            };

            SendMail(mail);
        }


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
                Sender = configuration["Email:Sender"],
                SenderName = configuration["Email:SenderName"],
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
                    Sender = configuration["Email:Sender"],
                    SenderName = configuration["Email:SenderName"]
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

        public void SendSMS2faNotice(string phone, string code)
        {
            eventBus.PublishTask(new Sms_Task
            {
                //From = _configuration["DefaultSmsHeader"], // set in messaging service
                To = phone,
                Text = $"Use verification code {code} for Achieve authencation."
            });
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
