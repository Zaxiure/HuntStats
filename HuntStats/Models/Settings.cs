namespace HuntStats.Models;

public class Settings
{
    public int Id { get; set; }
    public string Path { get; set; }

    public bool StartWorkerOnBoot { get; set; }
}