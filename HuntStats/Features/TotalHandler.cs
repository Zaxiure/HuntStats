using HuntStats.Data;
using HuntStats.State;
using MediatR;

namespace HuntStats.Features;

public class TotalView
{
    public int Kills { get; set; }
    public int YourKills { get; set; }

    public int Assists { get; set; }

    public int Deaths { get; set; }
}

public class GetTotalsCommand : IRequest<TotalView>
{
    
}

public class GetTotalsCommandHandler : IRequestHandler<GetTotalsCommand, TotalView>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly AppState _appState;
    private readonly IMediator _mediator;
    
    public GetTotalsCommandHandler(IDbConnectionFactory connectionFactory, AppState appState, IMediator mediator)
    {
        _connectionFactory = connectionFactory;
        _appState = appState;
        _mediator = mediator;
    }
    
    public async Task<TotalView> Handle(GetTotalsCommand request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync(cancellationToken);
        var matches = await _mediator.Send(new GetAllMatchCommand());
        var Settings = await _mediator.Send(new GetSettingsCommand());
        var test = matches.Select(x => x.Teams.Select(x => x.Players.Select(x => x.KilledByMe).Sum()).Sum());

        
        var totals = matches.Select(async x =>
        {
            var accolades = await _mediator.Send(new GetAccoladesByMatchIdCommand(x.Id));
            var team = x.Teams.FirstOrDefault(x => x.Players.FirstOrDefault(y => y.ProfileId == Settings.PlayerProfileId) != null);
            if (team != null)
            {
                var Assists = 0;
                if (accolades.FirstOrDefault(x => x.Category == "accolade_players_killed_assist") != null) Assists = accolades.FirstOrDefault(x => x.Category == "accolade_players_killed_assist").Hits;
                var Kills = x.Teams.Select(x => x.Players.Select(y => y.KilledByMe + y.DownedByMe + y.KilledByTeammate + y.DownedByTeammate).Sum()).Sum();
                var YourKills = x.Teams.Select(x => x.Players.Select(y => y.KilledByMe + y.DownedByMe).Sum()).Sum();
                var Deaths = x.Teams.Select(x => x.Players.Select(y => y.KilledMe + y.DownedMe).Sum()).Sum();
                return new TotalView 
                {
                    YourKills = YourKills,
                    Deaths = Deaths,
                    Kills = Kills,
                    Assists = Assists
                };
            }
            return null;
        }).Select(x => x.Result).Where(x => x != null).ToList();

        return new TotalView()
        {
            Kills = totals.Select(x => x.Kills).Sum(),
            YourKills = totals.Select(x => x.YourKills).Sum(),
            Deaths = totals.Select(x => x.Deaths).Sum(),
            Assists = totals.Select(x => x.Assists).Sum(),
        };
    }
}
