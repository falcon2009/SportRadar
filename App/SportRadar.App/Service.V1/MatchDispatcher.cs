using Core.Api.Service;
using SportRadar.Api.Enum;
using SportRadar.Api.Model;
using SportRadar.App.Model.V1;
using SportRadar.Storage.Api.V1;
using SportRadar.Storage.Entity.V1;

namespace SportRadar.App.Service.V1
{
    public sealed class MatchDispatcher : IMatchDispatcher
    {
        private readonly IEntityStorage<Guid, Match> matchStorage;
        private readonly IEntityStorage<Guid, MatchEvent> matchEventStorage;

        public MatchDispatcher(
            IEntityStorage<Guid, Match> matchStorage,
            IEntityStorage<Guid, MatchEvent> matchEventStorage)
        {
            this.matchStorage = matchStorage;
            this.matchEventStorage = matchEventStorage;
        }

        public Guid AnnonceMatch(string homeTeamName, string awayTeamName)
        {
            if (string.IsNullOrEmpty(homeTeamName))
            {
                throw new ArgumentNullException(nameof(homeTeamName));
            }

            if (string.IsNullOrEmpty(awayTeamName))
            {
                throw new ArgumentNullException(nameof(awayTeamName));
            }

            Match match = new(Guid.NewGuid(), homeTeamName, awayTeamName, DateTime.UtcNow);
            this.matchStorage.Insert(match);

            return match.Id;
        }

        public Guid StartMatch(Guid matchId)
        {
            MatchEvent newEvent = new(Guid.NewGuid(), matchId, MatchEventType.Start, DateTime.UtcNow);
            this.matchEventStorage.Insert(newEvent);

            return newEvent.Id;
        }

        public Guid FinishMatch(Guid matchId)
        {
            MatchEvent newEvent = new(Guid.NewGuid(), matchId, MatchEventType.Finish, DateTime.UtcNow);
            matchEventStorage.Insert(newEvent);

            return newEvent.Id;
        }

        public Guid AddGoal(Guid matchId, string scoredTeamName)
        {
            MatchEvent newEvent = new(Guid.NewGuid(), matchId, MatchEventType.Goal, DateTime.UtcNow, scoredTeamName);
            matchEventStorage.Insert(newEvent);

            return newEvent.Id;
        }

        public IEnumerable<IMatchInfo> GetMatchInfoList()
        {
            IEnumerable<Match> matchList = this.matchStorage.GetAll();
            if (!matchList.Any())
            {
                return Array.Empty<MatchInfo>();
            }

            IEnumerable<MatchEvent> matchEventList = this.matchEventStorage.GetAll();

            return matchList.Select(match => CreateMatchInfo(match, matchEventList));
        }

        private static MatchInfo CreateMatchInfo(Match match, IEnumerable<MatchEvent> matchEventList)
        {
            IEnumerable<MatchEvent> matchEvents = matchEventList.Where(item => item.MatchId == match.Id);

            return new(match, matchEvents);
        }
    }
}
