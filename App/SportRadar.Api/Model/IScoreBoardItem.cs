namespace SportRadar.Api.Model
{
    public interface IScoreBoardItem
    {
        string HomeTeamName { get; }

        string AwayTeamName { get; }

        int HomeTeamGoalTotal { get; }

        int AwayTeamGoalTotal { get; }
    }   
}
