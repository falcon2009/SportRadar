using SportRadar.Api.Model;

namespace SportRadar.Api.Service.V2
{
    public interface IScoreBoardService
    {
        Guid StartMatch(string homeTeamName, string awayTeamName);

        void FinishMatch(Guid matchId);

        void UpdateScore(Guid matchId, int homeTeamScore, int awayTeamScore);

        IEnumerable<IScoreBoardItem> GetActiveMatchSummary();
    }
}
