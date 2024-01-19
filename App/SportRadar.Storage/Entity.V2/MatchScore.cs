using Core.Api.Data;

namespace SportRadar.Storage.Entity.V2
{
    public record MatchScore(Guid Id, Guid MatchId, int HomeTeamGoalTotal, int AwayTeamGoalTotal, DateTime CreatedOn):IEntity<Guid>
    {
        public Guid Key => Id;
    }
}
