namespace CantineFront.Static
{
    public class AppSettings
    {
        public string? EnvironmentMode { get; set; }
        public string? ApiUrl { get; set; }
        public static int ShopID { get; set; }
        public EnvironmentModeEnum EnvMode => EnvironmentMode == "PROD" ? EnvironmentModeEnum.PROD : EnvironmentModeEnum.TEST;


    }
}
