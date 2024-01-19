using Core.Api.Data;
using SportRadar.Api.Enum;

namespace SportRadar.Storage.Entity.V1
{
    public record MatchEvent(Guid Id, Guid MatchId, MatchEventType Type, DateTime CreatedOn, string? TeamName = null) : IEntity<Guid>
    {
        public Guid Key => this.Id;
    }
}
