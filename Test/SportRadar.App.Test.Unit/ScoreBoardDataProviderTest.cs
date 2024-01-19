using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SportRadar.Api.Enum;
using SportRadar.Api.Model;
using SportRadar.Api.Service.V1;
using SportRadar.App.Model.V1;
using SportRadar.App.Service.V1;
using SportRadar.Storage.Api.V1;
using SportRadar.Storage.Entity.V1;
using Xunit;
using Match = SportRadar.Storage.Entity.V1.Match;

namespace SportRadar.App.Test.Unit
{
    public class ScoreBoardDataProviderTest
    {
        private readonly Mock<IMatchDispatcher> matchDispatcherMock = new();
        private readonly IScoreBoardDataProvider sut;

        public ScoreBoardDataProviderTest()
        {
            this.sut = new ScoreBoardDataProvider(this.matchDispatcherMock.Object);
        }

        [Fact]
        public void GetActiveMatchSummary_GivenNoMatchInfo_ShouldBeEmpty()
        {
            // arrange
            this.matchDispatcherMock
                .Setup(service => service.GetMatchInfoList())
                .Returns(Array.Empty<MatchInfo>);

            // act 
            IEnumerable<IScoreBoardItem> scoreBoardItemList = this.sut.GetActiveMatchSummary();

            // assert
            scoreBoardItemList.Should()
                              .NotBeNull();
            scoreBoardItemList.Should()
                              .BeEmpty();
        }

        [Theory]
        [AutoData]
        public void GetActiveMatchSummary_GivenNotStartedOrFinishedMatchInfo_ShouldBeFilteredOut(List<Match> notStartedMatchList, List<Match> finishedMatchList)
        {
            // arrange
            IEnumerable<IMatchInfo> notStartedMatchInfoList =
                notStartedMatchList.Select(item => CreateMatchInfo(item, false));
            IEnumerable<IMatchInfo> finishedMatchInfoList =
                finishedMatchList.Select(item => CreateMatchInfo(item, true, true));

            this.matchDispatcherMock
                .Setup(service => service.GetMatchInfoList())
                .Returns(Enumerable.Concat(notStartedMatchInfoList, finishedMatchInfoList));

            // act 
            IEnumerable<IScoreBoardItem> scoreBoardItemList = this.sut.GetActiveMatchSummary();

            // assert
            scoreBoardItemList.Should()
                              .NotBeNull();
            scoreBoardItemList.Should()
                              .BeEmpty();
        }

        [Fact]
        public void GetActiveMatchSummary_GivenActiveMatchInfo_ShouldHighestScoreGoFirst()
        {
            // arrange
            DateTime date = DateTime.UtcNow.AddDays(-3);
            Match[] matchArray = new[]
            {
                new Match(Guid.NewGuid(), "Home Team first", "Away Team first", date.AddMinutes(-60)),
                new Match(Guid.NewGuid(), "Home Team second", "Away Team second", date.AddMinutes(-30)),
                new Match(Guid.NewGuid(), "Home Team third", "Away Team third", date)
            };

            IMatchInfo[] matchInfoArray = new[]
            {
                CreateMatchInfo(matchArray[0], true, false),
                CreateMatchInfo(matchArray[1], true, false, 3, 1),
                CreateMatchInfo(matchArray[2], true, false, 1, 0),
            };

            this.matchDispatcherMock
                .Setup(service => service.GetMatchInfoList())
                .Returns(matchInfoArray);

            // act 
            IScoreBoardItem[] scoreBoardItemArray = this.sut.GetActiveMatchSummary()
                                                           .ToArray();

            // assert
            scoreBoardItemArray.Should()
                              .NotBeNull();
            scoreBoardItemArray.Should().HaveCount(3);

            // second match should go first
            ValidateScoreBoardItem(matchInfoArray[1], scoreBoardItemArray[0]);

            // third match should go second
            ValidateScoreBoardItem(matchInfoArray[2], scoreBoardItemArray[1]);

            // first match match should be the last
            ValidateScoreBoardItem(matchInfoArray[0], scoreBoardItemArray[2]);
        }

        [Fact]
        public void GetActiveMatchSummary_GivenActiveMatchInfoWithTheSameScore_ShouldMostRecentGoFirst()
        {
            // arrange
            DateTime date = DateTime.UtcNow.AddDays(-3);
            Match[] matchArray = new[]
            {
                new Match(Guid.NewGuid(), "Home Team first", "Away Team first", date.AddMinutes(-60)),
                new Match(Guid.NewGuid(), "Home Team second", "Away Team second", date.AddMinutes(-30)),
                new Match(Guid.NewGuid(), "Home Team third", "Away Team third", date)
            };

            IMatchInfo[] matchInfoArray = new[]
            {
                CreateMatchInfo(matchArray[0], true, false),
                CreateMatchInfo(matchArray[1], true, false, 1, 1),
                CreateMatchInfo(matchArray[2], true, false),
            };

            this.matchDispatcherMock
                .Setup(service => service.GetMatchInfoList())
                .Returns(matchInfoArray);

            // act 
            IScoreBoardItem[] scoreBoardItemArray = this.sut.GetActiveMatchSummary()
                                                            .ToArray();

            // assert
            scoreBoardItemArray.Should()
                              .NotBeNull();
            scoreBoardItemArray.Should().HaveCount(3);

            // second match should go first as it has highest total score
            ValidateScoreBoardItem(matchInfoArray[1], scoreBoardItemArray[0]);

            // third match should go second
            ValidateScoreBoardItem(matchInfoArray[2], scoreBoardItemArray[1]);

            // first match match should be the last
            ValidateScoreBoardItem(matchInfoArray[0], scoreBoardItemArray[2]);


        }

        private static IMatchInfo CreateMatchInfo(
            Match match,
            bool isStarted = true,
            bool isFinished = false,
            int homeTeamGoalTotal = 0,
            int awayTeamGoalTotal = 0)
        {
            if (!isStarted)
            {
                return new MatchInfo(match, Array.Empty<MatchEvent>());
            }

            List<MatchEvent> matchEventList = new();

            matchEventList.Add(new(Guid.NewGuid(), match.Id, MatchEventType.Start, match.CreatedOn.AddMinutes(60)));

            if (isFinished)
            {
                matchEventList.Add(new(Guid.NewGuid(), match.Id, MatchEventType.Finish, match.CreatedOn.AddMinutes(180)));
            }

            for (int i = 0; i < homeTeamGoalTotal; i++)
            {
                matchEventList.Add(new(Guid.NewGuid(), match.Id, MatchEventType.Goal, match.CreatedOn.AddMinutes(60 + 5 * i), match.HomeTeamName));
            }

            for (int i = 0; i < awayTeamGoalTotal; i++)
            {
                matchEventList.Add(new(Guid.NewGuid(), match.Id, MatchEventType.Goal, match.CreatedOn.AddMinutes(60 + 7 * (i + 1)), match.AwayTeamName));
            }

            return new MatchInfo(match, matchEventList); ;
        }

        private static void ValidateScoreBoardItem(IMatchInfo matchInfo, IScoreBoardItem item)
        {
            item.Should()
                .NotBeNull();
            item.HomeTeamName.Should().Be(matchInfo.HomeTeamName);
            item.AwayTeamName.Should().Be(matchInfo.AwayTeamName);
            item.HomeTeamGoalTotal.Should().Be(matchInfo.HomeTeamGoalTotal);
            item.AwayTeamGoalTotal.Should().Be(matchInfo.AwayTeamGoalTotal);
        }
    }
}