using System.Diagnostics;
using Dommel;
using HuntStats.Data;
using HuntStats.Models;
using HuntStats.State;
using HuntStats.WinUI;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HuntStats.Features;

public class HighlightsQuery : IRequest<GeneralStatus>
{

}

    public class HighlightsHandler : IRequestHandler<HighlightsQuery, GeneralStatus>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly AppState _appState;
    private Settings _settings;

    public HighlightsHandler(IDbConnectionFactory connectionFactory, AppState appState)
    {
        _connectionFactory = connectionFactory;
        _appState = appState;
    }

    public async Task<GeneralStatus> Handle(HighlightsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            using var con = await _connectionFactory.GetOpenConnectionAsync();
            _settings = await con.FirstOrDefaultAsync<Settings>(x => x.Id == 1);
            var huntHighlightsTempPath = _settings.HighlightsTempPath;
            var huntHighlightsOutputPath = _settings.HighlightsOutputPath + @"\Hunt Showdown\";
 
            if (!Directory.Exists(huntHighlightsTempPath)) return GeneralStatus.Error;

            if (huntHighlightsOutputPath != "" && !Directory.Exists(huntHighlightsOutputPath)) Directory.CreateDirectory(huntHighlightsOutputPath);

            if (!Directory.Exists(huntHighlightsOutputPath)) return GeneralStatus.Error;

            Debug.WriteLine("Highlight Handler running...");

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = huntHighlightsTempPath;
            watcher.NotifyFilter = NotifyFilters.FileName;
            watcher.Filter = "*.mp4";
            watcher.IncludeSubdirectories = true;

            Log.Information("Waiting for highlights...");
            while (!cancellationToken.IsCancellationRequested)
            {
                WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Created, 1000);
                if (result.TimedOut) continue;
                OnHighlightCreated(result);
            }
            Debug.WriteLine("Cancellation received...");

            return GeneralStatus.Succes;
        }
        catch (Exception e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("crashlog.txt").CreateLogger();
            Log.Information(e.ToString());
            Log.Information(e.StackTrace);
            return GeneralStatus.Error;
        }
    }

    private void OnHighlightCreated(WaitForChangedResult e)
    {
        Debug.WriteLine("Received new highlight");
        // Waiting for creation...
        Thread.Sleep(5000);

        string FullPath = Path.Combine(_settings.HighlightsTempPath, e.Name);
        string year = DateTime.Parse(DateTime.Now.ToString()).Year.ToString();
        string month = DateTime.Parse(DateTime.Now.ToString()).Month.ToString();
        string day = DateTime.Parse(DateTime.Now.ToString()).Day.ToString();
        string DestinationPath = Path.Combine(_settings.HighlightsOutputPath, "Hunt Showdown", year, month, day);
        if (!Directory.Exists(DestinationPath))
        {
            Directory.CreateDirectory(DestinationPath);
        }

        string Filename = Path.GetFileName(e.Name);
        string[] filePaths = Directory.GetFiles(FullPath.Replace(Filename, ""), "*.mp4");
        foreach (var sourcePath in filePaths)
        {
            string tempFilename = Path.GetFileName(sourcePath);
            if (!File.Exists(Path.Combine(DestinationPath, tempFilename))) { 
                File.Copy(sourcePath, Path.Combine(DestinationPath, tempFilename), false);
                Debug.WriteLine(tempFilename + @" has been copied");
            }
        }
    }
}