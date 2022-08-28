using System.Reflection;
using Microsoft.AspNetCore.Components.WebView.Maui;
using HuntStats.Data;
using HuntStats.Services;
using HuntStats.State;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;

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
		builder.ConfigureLifecycleEvents(lifecycle =>
		{
#if WINDOWS
                
			lifecycle.AddWindows(windows => windows.OnWindowCreated((del) => {
				del.ExtendsContentIntoTitleBar = false;
			}));
#endif
		});
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
		
		builder.Services.AddSingleton<WeatherForecastService>();

		return builder.Build();
	}
}
