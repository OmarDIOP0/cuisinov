using System.Globalization;

namespace DpworldDkr.Helpers
{
    public class Functions
    {

        public static string FormatMonetaire(int? value)
        {
            if (value == null) return "";

            return value.Value.ToString("C3", CultureInfo.CreateSpecificCulture("fr-FR")).Replace(",000 €", "");
        }
      

    }
}
