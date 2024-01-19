using SportRadar.Storage.Entity.V2;
using System;

namespace SportRadar.Storage.Test.Unit
{
    public sealed class MatchScoreEntityStorageTest : InMemoryEntityStorageTest<Guid, MatchScore>
    {
        protected override MatchScore NewEntity => new(Guid.NewGuid(), Guid.NewGuid(), 1, 1, DateTime.UtcNow);

        protected override Guid NewKey => Guid.NewGuid();

        protected override MatchScore ChangeEntity(MatchScore entity)
        {
            return entity with { HomeTeamGoalTotal = 5, AwayTeamGoalTotal = 6 };
        }
    }
}
