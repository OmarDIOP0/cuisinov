using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;

namespace CantineBack.Helpers
{
    public class EmailManager
    {


        static ILogger<Program> GetLogger()
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging();
                var serviceProvider = serviceCollection.BuildServiceProvider();
                return serviceProvider.GetService<ILogger<Program>>();
            }
            catch (Exception)
            {

                return null;
            }

        }
        static ILogger<Program> _logger = GetLogger();

        /// <summary>
        /// SendEmail
        /// </summary>
        /// <param name="destinataire"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="arrayOfByte"></param>
        /// <param name="fileAttachmentName"></param>
        /// <returns></returns>
        public static bool SendEmail(
            string destinataire,
            string subject,
            string body,
            byte[] arrayOfByte,
            string fileAttachmentName)
        {

      
            try
            {
                string smtpUser = Common.SmtpSettings?.SmtpUser;
                string smtpPassword = Common.SmtpSettings?.SmtpPassword;

                //string smtpDomain = ConfigurationManager.AppSettings["smtpDomain"].ToString();
                var client = new SmtpClient
                {
                    Port = Common.SmtpSettings?.SmtpPort ?? 0,
                    Host = Common.SmtpSettings?.SmtpHost,
                    Timeout = 30000,
                    //TargetName = "STARTTLS",/*smtp.office365.com/*/
                    //DeliveryFormat = SmtpDeliveryFormat.International,
                    EnableSsl = Common.SmtpSettings?.SmtpEnableSsl ?? false,
                    //DeliveryMethod = SmtpDeliveryMethod.Network,
                    //Credentials = new NetworkCredential(smtpUser, smtpPassword, smtpDomain),
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    UseDefaultCredentials = Common.SmtpSettings?.SmtpUseDefaultCredentials ?? false
                };

                string emailRH = Common.SmtpSettings?.Email;

                var message = new MailMessage
                {
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess,

                    From = new MailAddress(smtpUser,emailRH),
                    Priority = MailPriority.High,
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                message.To.Add(new MailAddress(destinataire));

                if (arrayOfByte != null)
                {
                    Stream stream = new MemoryStream(arrayOfByte);
                    // Create  the file attachment for this e-mail message.
                    Attachment data = new Attachment(stream, fileAttachmentName, MediaTypeNames.Application.Pdf);
                    // Add the file attachment to this e-mail message.
                    message.Attachments.Add(data);
                }

                client.Send(message);
                client.Dispose();
                client = null;
                _logger.LogError("ENVOI EMAIL AVEC SUCCESS: ");
                return true;
            }
            catch (Exception exp)
            {
                _logger.LogError("ERREUR ENVOI EMAIL : " + exp.Message, exp);
                return false;
            }
        }


        public static void SendMultiple(string[] emails, string message, string subject, byte[] arrByteAttach = null, string filename = "")
        {
            try
            {
                if (emails != null)
                {
                    foreach (var email in emails)
                    {
                        //EmailManager.SendEmail("dkr.documentation@dpworld.com", subject, message, null, "");

                        if (!String.IsNullOrEmpty(email))
                        {
                            EmailManager.SendEmail(email.ToLower(), subject, message, arrByteAttach, filename);
                        }

                        //if (!String.IsNullOrEmpty(email))
                        //{
                        //    EmailManager.SendEmail(email.ToLower(), subject, message, null, "");
                        //}
                    }
                }
            }
            catch (Exception exp)
            {
                _logger.LogError("ERREUR ENVOI EMAIL : " + exp.Message, exp);

            }
        }
    }
}
