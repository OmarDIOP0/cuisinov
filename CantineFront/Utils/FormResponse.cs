namespace CantineFront.Utils
{
    public class FormResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Object { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}
