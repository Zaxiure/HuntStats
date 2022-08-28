namespace HuntStats;

public interface IFolderPicker
{
    Task<string> PickFolder();
}