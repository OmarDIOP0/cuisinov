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
        /// SendEmail - Version asynchrone corrigée
        /// </summary>
        public static async Task<bool> SendEmail(
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

                using (var client = new SmtpClient
                {
                    Port = Common.SmtpSettings?.SmtpPort ?? 0,
                    Host = Common.SmtpSettings?.SmtpHost,
                    Timeout = 15000,
                    EnableSsl = Common.SmtpSettings?.SmtpEnableSsl ?? false,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    UseDefaultCredentials = Common.SmtpSettings?.SmtpUseDefaultCredentials ?? false
                })
                {
                    string emailRH = Common.SmtpSettings?.Email;

                    using (var message = new MailMessage
                    {
                        DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess,
                        From = new MailAddress(smtpUser, emailRH),
                        Priority = MailPriority.Normal,
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false,
                    })
                    {
                        message.To.Add(new MailAddress(destinataire));

                        if (arrayOfByte != null)
                        {
                            using (var stream = new MemoryStream(arrayOfByte))
                            {
                                var data = new Attachment(stream, fileAttachmentName, MediaTypeNames.Application.Pdf);
                                message.Attachments.Add(data);

                                // Utilisation de SendMailAsync pour la version asynchrone
                                await client.SendMailAsync(message);
                            }
                        }
                        else
                        {
                            // Utilisation de SendMailAsync pour la version asynchrone
                            await client.SendMailAsync(message);
                        }
                    }
                }

                _logger?.LogInformation("ENVOI EMAIL AVEC SUCCESS: " + destinataire);
                return true;
            }
            catch (Exception exp)
            {
                _logger?.LogError("ERREUR ENVOI EMAIL : " + exp.Message, exp);
                return false;
            }
        }

        /// <summary>
        /// SendMultiple - Version asynchrone corrigée
        /// </summary>
        public static async Task SendMultiple(string[] emails, string message, string subject, byte[] arrByteAttach = null, string filename = "")
        {
            try
            {
                if (emails != null)
                {
                    var tasks = new List<Task>();

                    foreach (var email in emails)
                    {
                        if (!String.IsNullOrEmpty(email))
                        {
                            // Lance tous les envois en parallèle
                            var task = SendEmail(email.ToLower(), subject, message, arrByteAttach, filename);
                            tasks.Add(task);
                        }
                    }

                    // Attend que tous les envois soient terminés
                    await Task.WhenAll(tasks);

                    _logger?.LogInformation($"ENVOI MULTIPLE TERMINÉ: {tasks.Count} emails");
                }
            }
            catch (Exception exp)
            {
                _logger?.LogError("ERREUR ENVOI EMAIL MULTIPLE : " + exp.Message, exp);
            }
        }
    }
}