﻿using Dapper;
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
    private readonly AppState _appState;
    private readonly IDbConnectionFactory _connectionFactory;

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
            settings = new Settings
            {
                Path = ""
            };
            await con.InsertAsync(settings);
        }

        _appState.PathChanged(settings.Path);

        return settings;
    }
}

public class ColumnsClass
{
    public string name { get; set; }
}

public class InitializeDatabaseCommand : IRequest<GeneralStatus>
{
}

public class InitializeDatabaseCommandHandler : IRequestHandler<InitializeDatabaseCommand, GeneralStatus>
{
    private readonly AppState _appState;
    private readonly IDbConnectionFactory _connectionFactory;

    public InitializeDatabaseCommandHandler(IDbConnectionFactory connectionFactory, AppState appState)
    {
        _connectionFactory = connectionFactory;
        _appState = appState;
    }

    public async Task<GeneralStatus> Handle(InitializeDatabaseCommand request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        con.QueryAsync(@"create table if not exists Accolades
(
    Id            INTEGER not null
        primary key autoincrement
        unique,
    BloodlineXp   INTEGER,
    Bounty        INTEGER,
    Category      nvarchar,
    Header        nvarchar,
    EventPoints   INTEGER,
    Gems          INTEGER,
    GeneratedGems INTEGER,
    Gold          INTEGER,
    Hits          INTEGER,
    HunterPoints  INTEGER,
    HunterXp      INTEGER,
    Weighting     INTEGER,
    Xp            INTEGER,
    MatchId       INTEGER
);

create table if not exists Entries
(
    Id              INTEGER not null
        primary key autoincrement
        unique,
    Category        nvarchar,
    DescriptorName  nvarchar,
    DescriptorScore INTEGER,
    DescriptorType  INTEGER,
    Reward          INTEGER,
    RewardSize      INTEGER,
    UiName          nvarchar,
    UiName2         nvarchar,
    MatchId         INTEGER,
    Amount          INTEGER
);

create table if not exists Match
(
    Id        INTEGER not null
        primary key autoincrement,
    DateTime  INTEGER,
    Scrapbeak INTEGER,
    Assassin  INTEGER,
    Spider    INTEGER,
    Butcher   INTEGER
);

create table if not exists Settings
(
    Id   INTEGER
        primary key autoincrement,
    Path nvarchar(16)
);

create table if not exists Teams
(
    Id      INTEGER not null
        primary key autoincrement,
    Mmr     INTEGER,
    Players nvarchar,
    MatchId INTEGER not null
);

");
        var columnCheck = await con.QueryAsync<ColumnsClass>("pragma table_info(Settings)");
        var StartWorkerOnBoot = columnCheck.FirstOrDefault(x => x.name == "StartWorkerOnBoot");
        var PlayerProfileId = columnCheck.FirstOrDefault(x => x.name == "PlayerProfileId");
        if (StartWorkerOnBoot == null)
            con.QueryAsync(@"alter table Settings
                                    add StartWorkerOnBoot integer default 0 not null;");

        if (PlayerProfileId == null)
            con.QueryAsync(@"alter table Settings
                add PlayerProfileId nvarchar;
            ");

        return GeneralStatus.Succes;
    }
}

public class UpdateSettingsCommand : IRequest<GeneralStatus>
{
    public UpdateSettingsCommand(Settings settings)
    {
        Settings = settings;
    }

    public Settings Settings { get; set; }
}

public class UpdateSettingsCommandHandler : IRequestHandler<UpdateSettingsCommand, GeneralStatus>
{
    private readonly AppState _appState;
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateSettingsCommandHandler(IDbConnectionFactory connectionFactory, AppState appState)
    {
        _connectionFactory = connectionFactory;
        _appState = appState;
    }

    public async Task<GeneralStatus> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var huntFilePath = request.Settings.Path + @"\user\profiles\default\attributes.xml";
        var fileExists = File.Exists(huntFilePath);
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        var settings = await con.FirstOrDefaultAsync<Settings>(x => x.Id == 1);
        if (!fileExists) request.Settings.Path = "";
        if (settings == null)
        {
            await con.InsertAsync(new Settings
            {
                Path = request.Settings.Path,
                StartWorkerOnBoot = request.Settings.StartWorkerOnBoot
            });
        }
        else
        {
            settings.Path = request.Settings.Path;
            settings.PlayerProfileId = request.Settings.PlayerProfileId;
            settings.StartWorkerOnBoot = request.Settings.StartWorkerOnBoot;
            await con.UpdateAsync(settings);
        }

        _appState.PathChanged(request.Settings.Path);
        if (!fileExists) return GeneralStatus.Error;

        return GeneralStatus.Succes;
    }
}