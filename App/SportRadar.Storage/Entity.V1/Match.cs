using Core.Api.Data;

namespace SportRadar.Storage.Entity.V1
{
    public record Match(Guid Id, string HomeTeamName, string AwayTeamName, DateTime CreatedOn) : IEntity<Guid>
    {
        public Guid Key => this.Id;
    }
}
