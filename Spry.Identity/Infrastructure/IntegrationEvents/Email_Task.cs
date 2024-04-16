using Newtonsoft.Json;
using Spry.BuildingBlocks.EventBus.Events;
using Spry.Identity.Services;

namespace Spry.Identity.Infrastructure.IntegrationEvents
{
#nullable disable
    public enum SmsProviderToUse
    {
        BigMsgBox = 1, Deywuro = 2, Termii = 3
    }

    public class Sms_Task : IntegrationEvent
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        public SmsProviderToUse ProviderToUse { get; set; } = SmsProviderToUse.BigMsgBox;
    }

    public class Email_Task : IntegrationEvent
    {
        public string Sender { get; set; }
        public string SenderName { get; set; }
        public string Receiver { get; set; }
        public string ReceiverName { get; set; }

        public string Template { get; set; }
        public string TemplateLocale { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string ContentMimeType { get; set; }
        public string AttachmentContent { get; set; }
        public string AttachmentFileName { get; set; }
        public string AttachmentMimeType { get; set; }
        public string Ccs { get; set; }
        public void SetCcs<T>(List<T> CcList) where T : EmailIdentity
        {
            if (CcList != null)
            {
                Ccs = JsonConvert.SerializeObject(CcList);
            }
        }
    }
}
