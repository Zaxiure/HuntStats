using Dommel;
using HuntStats.Data;
using HuntStats.Models;
using MediatR;

namespace HuntStats.Features;

public class GetAccoladesByMatchIdCommand : IRequest<List<Accolade>>
{
    public GetAccoladesByMatchIdCommand(int matchId)
    {
        MatchId = matchId;
    }

    public int MatchId { get; set; }
}

public class GetAccoladesByMatchIdCommandHandler : IRequestHandler<GetAccoladesByMatchIdCommand, List<Accolade>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAccoladesByMatchIdCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Accolade>> Handle(GetAccoladesByMatchIdCommand request, CancellationToken cancellationToken)
    {
        var con = await _connectionFactory.GetOpenConnectionAsync();
        var accolades = await con.SelectAsync<Accolade>(x => x.MatchId == request.MatchId);
        return accolades.ToList();
    }
}