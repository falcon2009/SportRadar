using System;
using SportRadar.Storage.Entity.V2;
using FluentAssertions;

namespace SportRadar.Storage.Test.Unit
{
    public sealed class MatchV2EntityStorageTest : InMemoryEntityStorageTest<Guid, Match>
    {
        protected override Match NewEntity => new(Guid.NewGuid(), "Home Team", "Away Team", DateTime.UtcNow);

        protected override Match ChangeEntity(Match entity)
        {
            return entity with { HomeTeamName = "Home Team Changed", AwayTeamName = "Away Team Changed" };
        }

        protected override Guid NewKey => Guid.NewGuid();

        protected override void ValidateEntity(Match example, Match? result)
        {
            base.ValidateEntity(example, result);
            example.HomeTeamName.Should().Be(result!.HomeTeamName);
            example.AwayTeamName.Should().Be(result.AwayTeamName);
            example.CreatedOn.Should().Be(result.CreatedOn);
            example.IsFinished.Should().Be(result.IsFinished);
        }
    }
}
