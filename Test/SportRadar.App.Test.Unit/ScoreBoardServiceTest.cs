using AutoFixture.Xunit2;
using Core.Api.Service;
using FluentAssertions;
using Moq;
using SportRadar.Api.Model;
using SportRadar.Api.Service.V2;
using SportRadar.App.Service.V2;
using SportRadar.Storage.Entity.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Match = SportRadar.Storage.Entity.V2.Match;

namespace SportRadar.App.Test.Unit
{
    public class ScoreBoardServiceTest
    {
        private readonly Mock<IEntityStorage<Guid, Match>> matchStorageMock = new();
        private readonly Mock<IEntityStorage<Guid, MatchScore>> matchScoreStorageMock = new();
        private readonly IScoreBoardService sut;

        public ScoreBoardServiceTest()
        {
            this.sut = new ScoreBoardService(this.matchStorageMock.Object, this.matchScoreStorageMock.Object);
        }

        [Fact]
        public void AnnonceMatch_GivenHomeTeamNameIsEmpty_ShouldThrow()
        {
            // act
            Action action = () => this.sut.StartMatch(string.Empty, "Away Team");

            // assert
            action.Should().Throw<ArgumentNullException>()
                           .WithParameterName("homeTeamName");
        }

        [Fact]
        public void AnnonceMatch_GivenAwayTeamNameIsEmpty_ShouldThrow()
        {
            // act
            Action action = () => this.sut.StartMatch("Home Team", string.Empty);

            // assert
            action.Should().Throw<ArgumentNullException>()
                           .WithParameterName("awayTeamName");
        }

        [Theory]
        [AutoData]
        public void AnnonceMatch_GivenTeamName_ShouldReturn(string homeTeamName, string awayTeamName)
        {
            // arrange
            this.matchStorageMock
                .Setup(service => service.Insert(It.IsAny<Match>()));

            // act
            _ = this.sut.StartMatch(homeTeamName, awayTeamName);

            // assert
            this.matchStorageMock.Verify(service => service.Insert(It.IsAny<Match>()), Times.Once);
        }

        [Fact]
        public void FinishMatch_GivenMatchNotFound_ShouldThrow()
        {
            // arrange
            Match? match = null;
            this.matchStorageMock.Setup(service => service.GetEntity(It.IsAny<Guid>()))
                                 .Returns(match);

            // act
            Action action = () => this.sut.FinishMatch(It.IsAny<Guid>());

            // assert
            action.Should()
                  .Throw<KeyNotFoundException>();
        }

        [Theory]
        [AutoData]
        public void FinishMatch_GivenMatchFound_ShouldDone(Match match)
        {
            // arrange
            this.matchStorageMock.Setup(service => service.GetEntity(match.Id))
                                 .Returns(match);

            // act
            this.sut.FinishMatch(match.Id);

            // assert
            this.matchStorageMock.Verify(service => service.GetEntity(match.Id), Times.Once);
            this.matchStorageMock.Verify(service => service.Update(It.IsAny<Match>()), Times.Once);
        }

        [Fact]
        public void UpdateScore_GivenMartchScore_ShouldDone()
        {
            // act
            this.sut.UpdateScore(Guid.NewGuid(), It.IsAny<int>(), It.IsAny<int>());

            // assert
            this.matchScoreStorageMock.Verify(service => service.Insert(It.IsAny<MatchScore>()), Times.Once);
        }

        [Fact]
        public void GetActiveMatchSummary_GivenNoMatch_ShouldBeEmpty()
        {
            // arrange
            this.matchStorageMock.Setup(service => service.GetAll())
                                 .Returns(Array.Empty<Match>());

            // act
            IEnumerable<IScoreBoardItem> scoreBoardItemList = this.sut.GetActiveMatchSummary();

            // assert
            scoreBoardItemList.Should()
                              .NotBeNull();
            scoreBoardItemList.Should()
                              .BeEmpty();
            this.matchStorageMock.Verify(service => service.GetAll(), Times.Once);
            this.matchScoreStorageMock.Verify(service => service.GetAll(), Times.Never);
        }

        [Fact]
        public void GetActiveMatchSummary_GivenFinishedMatchInfo_ShouldBeFilteredOut()
        {
            // arrange
            this.matchStorageMock.Setup(service => service.GetAll())
                                 .Returns(CreateaMatchList(3, "Home Team", "Away Team", DateTime.UtcNow.AddDays(-3), true));
            this.matchScoreStorageMock.Setup(service => service.GetAll())
                                      .Returns(Array.Empty<MatchScore>());

            // act
            IEnumerable<IScoreBoardItem> scoreBoardItemList = this.sut.GetActiveMatchSummary();

            // assert
            scoreBoardItemList.Should()
                              .NotBeNull();
            scoreBoardItemList.Should()
                              .BeEmpty();
            this.matchStorageMock.Verify(service => service.GetAll(), Times.Once);
            this.matchScoreStorageMock.Verify(service => service.GetAll(), Times.Once);
        }

        [Fact]
        public void GetActiveMatchSummary_GivenActiveMatchInfo_ShouldHighestScoreGoFirst()
        {
            // arrange
            Match[] matchArray = CreateaMatchList(3, "Home Team", "Away Team", DateTime.UtcNow.AddDays(-3)).ToArray();
            this.matchStorageMock.Setup(service => service.GetAll())
                                 .Returns(matchArray);
            MatchScore[] matchScoreArray = Enumerable.Concat(CreateMatchScore(matchArray[0], 2, 1), 
                                                             CreateMatchScore(matchArray[1], 0, 1))
                                                     .Concat(CreateMatchScore(matchArray[2], 2, 3))
                                                     .ToArray();
            this.matchScoreStorageMock.Setup(service => service.GetAll())
                                      .Returns(matchScoreArray);

            // act
            IScoreBoardItem[] scoreBoardItemList = this.sut.GetActiveMatchSummary()
                                                           .ToArray();

            // assert
            scoreBoardItemList.Should()
                              .NotBeEmpty();
            scoreBoardItemList.Should()
                              .HaveCount(3);
            ValidateScoreBoardItem(matchArray[2], scoreBoardItemList[0], 2, 3);
            ValidateScoreBoardItem(matchArray[0], scoreBoardItemList[1], 2, 1);
            ValidateScoreBoardItem(matchArray[1], scoreBoardItemList[2], 0, 1);
            this.matchStorageMock.Verify(service => service.GetAll(), Times.Once);
            this.matchScoreStorageMock.Verify(service => service.GetAll(), Times.Once);
        }

        [Fact]
        public void GetActiveMatchSummary_GivenActiveMatchWithTheSameTotalScore_ShouldMostRecentGoFirst()
        {
            // arrange
            Match[] matchArray = CreateaMatchList(3, "Home Team", "Away Team", DateTime.UtcNow.AddDays(-3)).ToArray();
            this.matchStorageMock.Setup(service => service.GetAll())
                                 .Returns(matchArray);
            MatchScore[] matchScoreArray = Enumerable.Concat(CreateMatchScore(matchArray[0], 2, 2),
                                                             CreateMatchScore(matchArray[1], 3, 1))
                                                     .Concat(CreateMatchScore(matchArray[2], 0, 4))
                                                     .ToArray();
            this.matchScoreStorageMock.Setup(service => service.GetAll())
                                      .Returns(matchScoreArray);
            // act
            IScoreBoardItem[] scoreBoardItemList = this.sut.GetActiveMatchSummary()
                                                           .ToArray();

            // assert
            scoreBoardItemList.Should()
                              .NotBeEmpty();
            scoreBoardItemList.Should()
                              .HaveCount(3);
            ValidateScoreBoardItem(matchArray[2], scoreBoardItemList[0], 0, 4);
            ValidateScoreBoardItem(matchArray[1], scoreBoardItemList[1], 3, 1);
            ValidateScoreBoardItem(matchArray[0], scoreBoardItemList[2], 2, 2);
            this.matchStorageMock.Verify(service => service.GetAll(), Times.Once);
            this.matchScoreStorageMock.Verify(service => service.GetAll(), Times.Once);
        }

        private static IEnumerable<Match> CreateaMatchList(int count, string homeTeamName, string awayTeamName, DateTime createdOn, bool isFinished = false)
        {
            for(int i = 0; i < count; i++)
            {
                yield return new(Guid.NewGuid(), $"{homeTeamName}{i}", $"{awayTeamName}{i}", createdOn.AddHours(i*5), isFinished);
            }
        }

        private static IEnumerable<MatchScore> CreateMatchScore(Match match, int homeTeamGoalTotal, int awayTeamGoalTotal)
        {
            List<MatchScore> matchScoreList = new List<MatchScore>();
            if (homeTeamGoalTotal > 0)
            {
                matchScoreList.Add(new(Guid.NewGuid(), match.Id, homeTeamGoalTotal, 0, match.CreatedOn.AddMinutes(30)));
            }
            if (awayTeamGoalTotal > 0)
            {
                matchScoreList.Add(new(Guid.NewGuid(), match.Id, homeTeamGoalTotal, awayTeamGoalTotal, match.CreatedOn.AddMinutes(60)));
            }

            return matchScoreList;
        }

        private static void ValidateScoreBoardItem(Match match, IScoreBoardItem item, int homeTeamGoalTotal, int awayTeamGoalTotal)
        {
            item.Should()
                .NotBeNull();
            item.HomeTeamName.Should().Be(match.HomeTeamName);
            item.AwayTeamName.Should().Be(match.AwayTeamName);
            item.HomeTeamGoalTotal.Should().Be(homeTeamGoalTotal);
            item.AwayTeamGoalTotal.Should().Be(awayTeamGoalTotal);
        }
    }
}
