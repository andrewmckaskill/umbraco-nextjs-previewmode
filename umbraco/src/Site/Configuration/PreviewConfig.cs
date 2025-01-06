public class PreviewConfig
{
    public string? FrontendUrl { get; set; }
    public string? PreviewPath { get; set; } = "/api/preview";
    public string? PreviewExitPath { get; set; } = "/api/exit-preview";
    public string? PreviewSecret { get; set; }
}