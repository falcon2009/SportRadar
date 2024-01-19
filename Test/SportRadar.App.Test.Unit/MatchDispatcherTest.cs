using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using Core.Api.Service;
using FluentAssertions;
using Moq;
using SportRadar.Api.Model;
using SportRadar.App.Service.V1;
using SportRadar.Storage.Api.V1;
using Xunit;

namespace SportRadar.Storage.Test.Unit
{
    public class MatchDispatcherTest
    {
        private readonly Mock<IEntityStorage<Guid, Entity.V1.Match>> matchStorageMock = new();
        private readonly Mock<IEntityStorage<Guid, Entity.V1.MatchEvent>> matchEventStorageMock = new();
        private readonly IMatchDispatcher sut;

        public MatchDispatcherTest()
        {
            this.sut = new MatchDispatcher(matchStorageMock.Object, matchEventStorageMock.Object);
        }

        [Fact]
        public void AnnonceMatch_GivenHomeTeamNameIsEmpty_ShouldThrow()
        {
            // act
            Action action = () => this.sut.AnnonceMatch(string.Empty, "Away Team");

            // assert
            action.Should().Throw<ArgumentNullException>()
                           .WithParameterName("homeTeamName");
        }

        [Fact]
        public void AnnonceMatch_GivenAwayTeamNameIsEmpty_ShouldThrow()
        {
            // act
            Action action = () => this.sut.AnnonceMatch("Home Team", string.Empty);

            // assert
            action.Should().Throw<ArgumentNullException>()
                           .WithParameterName("awayTeamName");
        }

        [Theory]
        [AutoData]
        public void AnnonceMatch_GivenTeamName_ShouldDone(string homeTeamName, string awayTeamName)
        {
            // arrange
            this.matchStorageMock
                .Setup(service => service.Insert(It.IsAny<Entity.V1.Match>()));

            // act
            _ = this.sut.AnnonceMatch(homeTeamName, awayTeamName);

            // assert
            this.matchStorageMock.Verify(service => service.Insert(It.IsAny<Entity.V1.Match>()), Times.Once);
        }

        [Theory]
        [AutoData]
        public void StartMatch_GivenMatchId_ShouldDone(Guid matchId)
        {
            // arrange
            this.matchEventStorageMock
                .Setup(service => service.Insert(It.IsAny<Entity.V1.MatchEvent>()));

            // act
            _ = this.sut.StartMatch(matchId);

            // assert
            this.matchEventStorageMock.Verify(service => service.Insert(It.IsAny<Entity.V1.MatchEvent>()), Times.Once);
        }

        [Theory]
        [AutoData]
        public void FinishMatch_GivenMatchId_ShouldDone(Guid matchId)
        {
            // arrange
            this.matchEventStorageMock
                .Setup(service => service.Insert(It.IsAny<Entity.V1.MatchEvent>()));

            // act
            _ = this.sut.FinishMatch(matchId);

            // assert
            this.matchEventStorageMock.Verify(service => service.Insert(It.IsAny<Entity.V1.MatchEvent>()), Times.Once);
        }

        [Theory]
        [AutoData]
        public void AddGoal_GivenMatchIdAndTeam_ShouldDone(Guid matchId, string scoredTeamName)
        {
            // arrange
            this.matchEventStorageMock
                .Setup(service => service.Insert(It.IsAny<Entity.V1.MatchEvent>()));

            // act
            _ = this.sut.AddGoal(matchId, scoredTeamName);

            // assert
            this.matchEventStorageMock.Verify(service => service.Insert(It.IsAny<Entity.V1.MatchEvent>()), Times.Once);
        }

        [Fact]
        public void GetMatchInfoList_GivenNoMatch_ShouldReturnEmpty()
        {
            // arrange
            this.matchStorageMock.Setup(service => service.GetAll())
                                 .Returns(Array.Empty<Entity.V1.Match>);
            this.matchEventStorageMock.Setup(service => service.GetAll())
                                      .Returns(Array.Empty<Entity.V1.MatchEvent>);
            // act
            IEnumerable<IMatchInfo> matchInfoList = this.sut.GetMatchInfoList();

            // assert
            this.matchStorageMock.Verify(service => service.GetAll(), Times.Once);
            this.matchEventStorageMock.Verify(service => service.GetAll(), Times.Never);
            matchInfoList.Should()
                         .BeEmpty();
        }

        [Fact]
        public void GetMatchInfoList_GivenMatch_ShouldReturn()
        {
            // arrange
            Entity.V1.Match[] matchArray = CreateMatchList(5).ToArray();
            this.matchStorageMock
                .Setup(service => service.GetAll())
                .Returns(matchArray);

            List<Entity.V1.MatchEvent> matchEventList = new();
            matchEventList.AddRange(CreateMatchEventList(matchArray[0], false));
            matchEventList.AddRange(CreateMatchEventList(matchArray[1], true, false));
            matchEventList.AddRange(CreateMatchEventList(matchArray[2], true, false, 1, 1));
            matchEventList.AddRange(CreateMatchEventList(matchArray[3], true, false, 3, 1));
            matchEventList.AddRange(CreateMatchEventList(matchArray[4], true, true, 1, 3));
            this.matchEventStorageMock
                .Setup(service => service.GetAll())
                .Returns(matchEventList);

            // act
            IEnumerable<IMatchInfo> matchInfoList = this.sut.GetMatchInfoList();

            // assert
            this.matchStorageMock.Verify(service => service.GetAll(), Times.Once);
            this.matchEventStorageMock.Verify(service => service.GetAll(), Times.Once);
            matchInfoList.Count()
                         .Should()
                         .Be(5);

            IEnumerable<IMatchInfo> notStartedMatchInfoList = matchInfoList.Where(item => !item.IsStarted);
            notStartedMatchInfoList.Count()
                                   .Should()
                                   .Be(1);

            IEnumerable<IMatchInfo> startedMatchInfoList = matchInfoList.Where(item => item.IsStarted);
            startedMatchInfoList.Count()
                                .Should()
                                .Be(4);

            IEnumerable<IMatchInfo> finishedMatchListInfo = matchInfoList.Where(item => item.IsStarted && item.IsFinished);
            finishedMatchListInfo.Count()
                                 .Should()
                                 .Be(1);

            IMatchInfo[] activeMatchListInfo = matchInfoList.Where(item => item.IsStarted && !item.IsFinished)
                                                            .ToArray();
            activeMatchListInfo.Count()
                               .Should()
                               .Be(3);
            // not started
            ValidateMatchInfo(notStartedMatchInfoList.FirstOrDefault(), false, matchArray[0].HomeTeamName, matchArray[0].AwayTeamName, 0, 0, 0);

            // finished, away wins
            ValidateMatchInfo(finishedMatchListInfo.FirstOrDefault(), false, matchArray[4].HomeTeamName, matchArray[4].AwayTeamName, 1, 3, 4);

            // active, no goals
            ValidateMatchInfo(activeMatchListInfo[0], false, matchArray[1].HomeTeamName, matchArray[1].AwayTeamName, 0, 0, 0);

            // active, in a draw
            ValidateMatchInfo(activeMatchListInfo[1], false, matchArray[2].HomeTeamName, matchArray[2].AwayTeamName, 1, 1, 2);

            // active, home wins
            ValidateMatchInfo(activeMatchListInfo[2], false, matchArray[3].HomeTeamName, matchArray[3].AwayTeamName, 3, 1, 4);

        }

        private static IEnumerable<Entity.V1.Match> CreateMatchList(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                yield return new(Guid.NewGuid(), $"Home Team {i}", $"Away Team {i}", DateTime.UtcNow.AddHours(-1).AddMinutes(i));
            }
        }

        private static IEnumerable<Entity.V1.MatchEvent> CreateMatchEventList(Entity.V1.Match match,
            bool isStarted = true,
            bool isFinished = false,
            int homeTeamGoalTotal = 0,
            int awayTeamGoalTotal = 0)
        {
            List<Entity.V1.MatchEvent> matchEventList = new();
            if (!isStarted)
            {
                return matchEventList;
            }

            matchEventList.Add(new(Guid.NewGuid(), match.Id, SportRadar.Api.Enum.MatchEventType.Start, match.CreatedOn.AddMinutes(60)));

            if (isFinished)
            {
                matchEventList.Add(new(Guid.NewGuid(), match.Id, SportRadar.Api.Enum.MatchEventType.Finish, match.CreatedOn.AddMinutes(180)));
            }

            for (int i = 0; i < homeTeamGoalTotal; i++)
            {
                matchEventList.Add(new(Guid.NewGuid(), match.Id, SportRadar.Api.Enum.MatchEventType.Goal, match.CreatedOn.AddMinutes(60 + 5 * i), match.HomeTeamName));
            }

            for (int i = 0; i < awayTeamGoalTotal; i++)
            {
                matchEventList.Add(new(Guid.NewGuid(), match.Id, SportRadar.Api.Enum.MatchEventType.Goal, match.CreatedOn.AddMinutes(60 + 7 * (i + 1)), match.AwayTeamName));
            }

            return matchEventList;
        }

        private static void ValidateMatchInfo(IMatchInfo? matchInfo,
            bool shouldBeNull,
            string homeTeamName,
            string awayTeamName,
            int homeTeamGoalLotal,
            int awayTeamGoalTotal,
            int goalTotal)
        {
            if (shouldBeNull)
            {
                matchInfo.Should()
                         .BeNull();

                return;
            }

            matchInfo.Should()
                     .NotBeNull();
            matchInfo!.HomeTeamName
                      .Should()
                      .Be(homeTeamName);
            matchInfo.AwayTeamName
                     .Should()
                     .Be(awayTeamName);
            matchInfo.HomeTeamGoalTotal
                     .Should()
                     .Be(homeTeamGoalLotal);
            matchInfo.AwayTeamGoalTotal
                     .Should()
                     .Be(awayTeamGoalTotal);
            matchInfo.GoalTotal
                     .Should()
                     .Be(goalTotal);
        }
    }
}
