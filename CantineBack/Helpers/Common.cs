namespace CantineBack.Helpers
{
    public class Common
    {
        public static int ShopID { get; set; }
        public static int EntrepriseID { get; set; }
        //public static string BackendLink { get; set; }
        public static string FrontEndLink { get; set; }
        public static string? EnvironmentMode { get; set; }
        public static string? PasswordResetMessage { get; set; }
        public static string? CreateAccountMessage { get; set; }
        public static string? QrCodeEmailMessage { get; set; }
        public static string? QrCodeEmailMessageEn { get; set; }
        public static SmtpSettings? SmtpSettings { get; set; }

        public static string GetRandomAlphanumericString(int length)
        {

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            string password = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            Console.WriteLine(password);
            return password;    
           
        }

    }
}
