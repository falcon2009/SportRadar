using SportRadar.Api.Model;
using SportRadar.Api.Service.V1;
using SportRadar.App.Model.V1;
using SportRadar.Storage.Api.V1;

namespace SportRadar.App.Service.V1
{
    public sealed class ScoreBoardDataProvider : IScoreBoardDataProvider
    {
        private readonly IMatchDispatcher matchDispatcher;
        public ScoreBoardDataProvider(IMatchDispatcher matchDispatcher)
        {
            this.matchDispatcher = matchDispatcher;
        }

        public IEnumerable<IScoreBoardItem> GetActiveMatchSummary()
        {
            return this.matchDispatcher.GetMatchInfoList()
                                       .Where(item => item.IsStarted && !item.IsFinished)
                                       .OrderByDescending(item => item.GoalTotal)
                                       .ThenByDescending(item => item.StartedOn)
                                       .Select(item => CreateScoreBoardItem(item));
        }

        private static IScoreBoardItem CreateScoreBoardItem(IMatchInfo item)
        {
            return new ScoreBoardItem(item.HomeTeamName, item.AwayTeamName, item.HomeTeamGoalTotal, item.AwayTeamGoalTotal);
        }
    }
}
