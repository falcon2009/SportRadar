using SportRadar.Api.Enum;
using SportRadar.Api.Model;
using SportRadar.Storage.Entity.V1;

namespace SportRadar.App.Model.V1
{
    public record MatchInfo(Match Match, IEnumerable<MatchEvent> EventList) : IMatchInfo
    {
        private Match Match { get; init; } = Match;
        private IEnumerable<MatchEvent> EventList { get; init; } = EventList;

        public string HomeTeamName => Match.HomeTeamName;

        public string AwayTeamName => Match.AwayTeamName;

        public DateTime? StartedOn =>
            this.EventList.FirstOrDefault(item => item.Type == MatchEventType.Start)?.CreatedOn;

        public bool IsStarted =>
            this.EventList.Any(item => item.Type == MatchEventType.Start);

        public bool IsFinished =>
            this.EventList.Any(item => item.Type == MatchEventType.Finish);

        public int GoalTotal =>
            this.EventList.Where(item => item.Type == MatchEventType.Goal).Count();

        public int HomeTeamGoalTotal=>
            this.EventList.Where(item => string.Equals(item.TeamName, this.HomeTeamName, StringComparison.OrdinalIgnoreCase)  && item.Type == MatchEventType.Goal).Count();

        public int AwayTeamGoalTotal =>
            this.EventList.Where(item => string.Equals(item.TeamName, this.AwayTeamName, StringComparison.OrdinalIgnoreCase) && item.Type == MatchEventType.Goal).Count();
    }
}
