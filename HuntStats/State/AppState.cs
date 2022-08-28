using System;
using Microsoft.AspNetCore.Components.Web;

namespace HuntStats.State;

public class AppState
{
    public event Action CloseSidebarEvent;

    public event Action<string> PathChangedEvent;

    public void CloseSidebar()
    {
        CloseSidebarEvent?.Invoke();
    }

    public event Action<MouseEventArgs> ClickOutside;

    public event Action<string> NavigateTo;

    public event Action NewMatchAdded;
    
    public event Action OpenedDropdownEvent;
    
    public event Action CloseOpenedDropdownEvent;

    public string OpenedUniqueId;

    public string LastOpenedUniqueId;

    public void NavigatingTo(string url)
    {
        NavigateTo?.Invoke(url);
    }

    public void MatchAdded()
    {
        NewMatchAdded?.Invoke();
    }

    public void PathChanged(string path)
    {
        PathChangedEvent?.Invoke(path);
    }

    public void OpenedDropdown(string UniqueId)
    {
        LastOpenedUniqueId = OpenedUniqueId;
        OpenedUniqueId = UniqueId;
        OpenedDropdownEvent?.Invoke();
    }

    public void CloseOpenedDropdown()
    {
        CloseOpenedDropdownEvent?.Invoke();
    }

    public void ClickedOutside(MouseEventArgs args)
    {
        ClickOutside?.Invoke(args);
    }
}