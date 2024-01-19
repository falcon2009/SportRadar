using SportRadar.Storage.Entity.V1;
using SportRadar.Api.Enum;
using System;

namespace SportRadar.Storage.Test.Unit
{
    public sealed class MatchEventEntityStorageTest : InMemoryEntityStorageTest<Guid, MatchEvent>
    {
        protected override MatchEvent NewEntity => new(Guid.NewGuid(), Guid.NewGuid(), MatchEventType.Goal, DateTime.UtcNow, "Home Team");

        protected override Guid NewKey => Guid.NewGuid();

        protected override MatchEvent ChangeEntity(MatchEvent entity)
        {
            return entity with { TeamName = "Home Team Changed" };
        }
    }
}
