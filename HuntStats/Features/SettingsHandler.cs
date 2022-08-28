using Dommel;
using HuntStats.Data;
using HuntStats.Models;
using HuntStats.State;
using MediatR;

namespace HuntStats.Features;

public class GetSettingsCommand : IRequest<Settings>
{
    
}

public class GetSettingsCommandHandler : IRequestHandler<GetSettingsCommand, Settings>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly AppState _appState;
    
    public GetSettingsCommandHandler(IDbConnectionFactory connectionFactory, AppState appState)
    {
        _connectionFactory = connectionFactory;
        _appState = appState;
    }
    
    public async Task<Settings> Handle(GetSettingsCommand request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        var settings = await con.FirstOrDefaultAsync<Settings>(x => x.Id == 1);
        if (settings == null)
        {
            await con.InsertAsync(new Settings()
            {
                Path = ""
            });
        }
        
        _appState.PathChanged(settings.Path);

        return settings;
    }
}

public class UpdateSettingsCommand : IRequest<GeneralStatus>
{
    public UpdateSettingsCommand(string path)
    {
        Path = path;
    }

    public string Path { get; set; }
}

public class UpdateSettingsCommandHandler : IRequestHandler<UpdateSettingsCommand, GeneralStatus>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly AppState _appState;
    
    public UpdateSettingsCommandHandler(IDbConnectionFactory connectionFactory, AppState appState)
    {
        _connectionFactory = connectionFactory;
        _appState = appState;
    }
    
    public async Task<GeneralStatus> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var huntFilePath = request.Path + @"\user\profiles\default\attributes.xml";
        var fileExists = File.Exists(huntFilePath);
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        var settings = await con.FirstOrDefaultAsync<Settings>(x => x.Id == 1);
        if (!fileExists) request.Path = "";
        if (settings == null)
        {
            await con.InsertAsync(new Settings()
            {
                Path = request.Path
            });
        }
        else
        {
            settings.Path = request.Path;
            await con.UpdateAsync(settings);
        }
        _appState.PathChanged(request.Path);
        if (!fileExists) return GeneralStatus.Error;

        return GeneralStatus.Succes;
    }
}