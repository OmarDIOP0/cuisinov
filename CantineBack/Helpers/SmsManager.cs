using System.Net;
using System.Text;

namespace CantineBack.Helpers
{
    public class SmsManager
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

              return   null;            }
     
        }
      static  ILogger<Program> _logger= GetLogger();
        public SmsManager ()
        {

        }
        public enum DestinationSMS
        {
            NATIONAL,
            INTERNATIONAL
        }

        /// <summary>
        /// Send SMS
        /// </summary>
        /// <param name="destinataire"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool SendSMS(string destinataire, string message, DestinationSMS destinationSMS = DestinationSMS.NATIONAL)
        {
            if (destinationSMS == DestinationSMS.NATIONAL)
            {
                if (!destinataire.StartsWith("221")) destinataire = "221" + destinataire;
            }

            var request = (HttpWebRequest)WebRequest.Create("https://api.freebusiness.sn/sms/1/text/single");

            var postData = "{\n  \"from\": \"DPWORLD\",\n  \"to\": \"" + destinataire + "\",\n  \"text\": \"" + message + "\"\n}";
            var data = Encoding.ASCII.GetBytes(postData);

            string authInfo = "gdgdggdgdgg=="; //Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.Headers.Add("Authorization", "Basic " + authInfo);

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (responseString.Contains("PENDING_ENROUTE"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                if(_logger!=null)
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
