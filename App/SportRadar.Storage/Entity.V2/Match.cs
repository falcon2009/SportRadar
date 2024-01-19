using Core.Api.Data;

namespace SportRadar.Storage.Entity.V2
{
    public record Match(Guid Id, string HomeTeamName, string AwayTeamName, DateTime CreatedOn, bool IsFinished = false) : IEntity<Guid>
    {
        public Guid Key => this.Id;
    }
}
