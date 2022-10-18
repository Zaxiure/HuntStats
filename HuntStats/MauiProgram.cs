using System.Reflection;
using Windows.Graphics;
using Microsoft.AspNetCore.Components.WebView.Maui;
using HuntStats.Data;
using HuntStats.Services;
using HuntStats.State;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;

namespace HuntStats;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});
#if WINDOWS
			builder.ConfigureLifecycleEvents(events =>
			{
				events.AddWindows(wndLifeCycleBuilder =>
				{
					wndLifeCycleBuilder.OnWindowCreated(window =>
					{
						IntPtr nativeWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
						WindowId win32WindowsId = Win32Interop.GetWindowIdFromWindow(nativeWindowHandle);
						AppWindow winuiAppWindow = AppWindow.GetFromWindowId(win32WindowsId);    
						if(winuiAppWindow.Presenter is OverlappedPresenter p)
						{ 
							//p.Maximize();
							p.IsResizable=false;
							p.IsMaximizable = false;
							p.IsMinimizable=false;
						}                     
					});
				});
			});
#endif
		builder.Services.AddScoped<AppState>();
		builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
		builder.Services.AddTransient<IDbConnectionFactory, ConnectionFactory>();
#if WINDOWS
		builder.Services.AddTransient<IFolderPicker, Platforms.Windows.FolderPicker>();
#endif
		builder.Services.AddMauiBlazorWebView();
		#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		#endif
		
		
		
		return builder.Build();
	}
}
