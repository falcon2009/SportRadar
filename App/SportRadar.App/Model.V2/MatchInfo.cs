using SportRadar.Api.Enum;
using SportRadar.Api.Model;
using SportRadar.Storage.Entity.V2;

namespace SportRadar.App.Model.V2
{
    public record MatchInfo(Match Match, MatchScore? MatchScore) : IMatchInfo
    {
        private Match Match { get; init; } = Match;
        private MatchScore? MatchScore { get; init; } = MatchScore;

        public string HomeTeamName => this.Match.HomeTeamName;

        public string AwayTeamName => this.Match.AwayTeamName;

        public DateTime? StartedOn => this.Match.CreatedOn;

        public bool IsStarted => true;

        public bool IsFinished => this.Match.IsFinished;

        public int GoalTotal => this.HomeTeamGoalTotal + AwayTeamGoalTotal;

        public int HomeTeamGoalTotal => this.MatchScore?.HomeTeamGoalTotal ?? 0;

        public int AwayTeamGoalTotal => this.MatchScore?.AwayTeamGoalTotal ?? 0;
    }
}
