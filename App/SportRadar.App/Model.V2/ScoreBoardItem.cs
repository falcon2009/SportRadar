using SportRadar.Api.Model;

namespace SportRadar.App.Model.V2
{
    public record ScoreBoardItem (string HomeTeamName, string AwayTeamName, int HomeTeamGoalTotal, int AwayTeamGoalTotal) : IScoreBoardItem;
}
