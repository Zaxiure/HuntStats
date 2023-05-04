namespace HuntStats.Models;

public class Settings
{
    public int Id { get; set; }
    public string Path { get; set; }

    public string HighlightsTempPath { get; set; }
    public string HighlightsOutputPath { get; set; }

    public bool CopyHighlights { get; set; }

    public bool StartWorkerOnBoot { get; set; }

    public string PlayerProfileId { get; set; }
}