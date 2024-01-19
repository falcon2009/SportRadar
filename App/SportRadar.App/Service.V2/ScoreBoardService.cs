using Core.Api.Service;
using SportRadar.Api.Model;
using SportRadar.Api.Service.V2;
using SportRadar.App.Model.V2;
using SportRadar.Storage.Entity.V2;

namespace SportRadar.App.Service.V2
{
    public class ScoreBoardService : IScoreBoardService
    {
        private readonly IEntityStorage<Guid, Match> matchStorage;
        private readonly IEntityStorage<Guid, MatchScore> matchScoreStorage;

        public ScoreBoardService(
            IEntityStorage<Guid, Match> matchStorage,
            IEntityStorage<Guid, MatchScore> matchScoreStorage)
        {
            this.matchStorage = matchStorage;
            this.matchScoreStorage = matchScoreStorage;
        }

        public Guid StartMatch(string homeTeamName, string awayTeamName)
        {
            if (string.IsNullOrEmpty(homeTeamName))
            {
                throw new ArgumentNullException(nameof(homeTeamName));
            }

            if (string.IsNullOrEmpty(awayTeamName))
            {
                throw new ArgumentNullException(nameof(awayTeamName));
            }

            Match newMatch = new(Guid.NewGuid(), homeTeamName, awayTeamName, DateTime.UtcNow);
            this.matchStorage.Insert(newMatch);

            return newMatch.Id;
        }

        public void FinishMatch(Guid matchId)
        {
            Match? match = this.matchStorage.GetEntity(matchId);
            if (match == null)
            {
                throw new KeyNotFoundException();
            }

            match = match with { IsFinished = true };
            this.matchStorage.Update(match);
        }

        public void UpdateScore(Guid matchId, int homeTeamScore, int awayTeamScore)
        {
            MatchScore matchScore = new(Guid.NewGuid(), matchId, homeTeamScore, awayTeamScore, DateTime.UtcNow);
            this.matchScoreStorage.Insert(matchScore);
        }

        public IEnumerable<IScoreBoardItem> GetActiveMatchSummary()
        {
            return GetMatchInfoList().Where(item => item.IsStarted && !item.IsFinished)
                                     .OrderByDescending(item => item.GoalTotal)
                                     .ThenByDescending(item => item.StartedOn)
                                     .Select(item => CreateScoreBoardItem(item));
        }

        private IEnumerable<IMatchInfo> GetMatchInfoList()
        {
            IEnumerable<Match> matchList = this.matchStorage.GetAll();
            if (!matchList.Any())
            {
                return Array.Empty<MatchInfo>();
            }

            IEnumerable<MatchScore> matchScoreList = this.matchScoreStorage.GetAll();

            return matchList.Select(item => CreateMatchInfo(item, matchScoreList));
        }

        private static MatchInfo CreateMatchInfo(Match match, IEnumerable<MatchScore> matchScoreList)
        {
            MatchScore? matchScore = matchScoreList.Where(item => item.MatchId == match.Id)
                                                   .OrderByDescending(item => item.CreatedOn)
                                                   .FirstOrDefault();

            return new(match, matchScore);
        }

        private static IScoreBoardItem CreateScoreBoardItem(IMatchInfo item)
        {
            return new ScoreBoardItem(item.HomeTeamName, item.AwayTeamName, item.HomeTeamGoalTotal, item.AwayTeamGoalTotal);
        }
    }
}
