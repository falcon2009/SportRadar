namespace SportRadar.Api.Model
{
    public interface IMatchInfo : IScoreBoardItem
    {
        DateTime? StartedOn { get; }

        bool IsStarted { get; }

        bool IsFinished { get; }

        int GoalTotal { get; }
    }
}
