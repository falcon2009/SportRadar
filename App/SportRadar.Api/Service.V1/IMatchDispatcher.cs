using SportRadar.Api.Model;

namespace SportRadar.Storage.Api.V1
{
    public interface IMatchDispatcher
    {
        Guid AnnonceMatch(string homeTeamName, string awayTeamName);

        Guid StartMatch(Guid matchId);

        Guid FinishMatch(Guid matchId);

        Guid AddGoal(Guid matchId, string scoredTeamName);

        IEnumerable<IMatchInfo> GetMatchInfoList();
    }
}
