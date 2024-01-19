using System;
using System.Collections.Concurrent;
using System.Linq;
using Core.Api.Data;
using Core.App.Service;
using FluentAssertions;
using Xunit;
using AutoFixture.Xunit2;
using System.Collections.Generic;
using Core.Api.Service;

namespace SportRadar.Storage.Test.Unit
{
    public abstract class InMemoryEntityStorageTest<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TEntity> storage = new();
        private readonly IEntityStorage<TKey, TEntity> sut;

        protected InMemoryEntityStorageTest()
        {
            this.sut = new InMemoryEntityStorage<TKey, TEntity>(storage);
        }

        [Fact]
        public void Insert_GivenEntityIsNull_ShouldThrow()
        {
            // arrange
            TEntity? entity = null;

            // act
            Action action = () => this.sut.Insert(entity);

            // assert
            action.Should()
                  .Throw<ArgumentNullException>()
                  .WithParameterName("entity");

        }

        [Theory]
        [AutoData]
        public void Insert_GivenEntity_ShouldInsert(TEntity entity)
        {
            // act
             bool result = this.sut.Insert(entity);

            // assert
            result.Should().BeTrue();
            this.storage.Count.Should()
                              .Be(1);
            TEntity item = this.storage[entity.Key];
            ValidateEntity(entity, item);
        }

        [Fact]
        public void Update_GivenEntityIsNull_ShouldThrow()
        {
            // arrange
            TEntity? entity = null;

            // act
            Action action = () => this.sut.Update(entity);

            // assert
            action.Should()
                  .Throw<ArgumentNullException>()
                  .WithParameterName("entity");

        }

        [Theory]
        [AutoData]
        public void Update_GivenKeyNotFound_ShouldThrow(TEntity entity)
        {
            // arrange
            this.sut.Insert(entity);
            TEntity otherEntity = NewEntity;

            // act
            Action action = () => this.sut.Update(otherEntity);

            // assert
            action.Should()
                  .Throw<KeyNotFoundException>();
        }

        [Theory]
        [AutoData]
        public void Update_GivenEntity_ShouldUpdate(TEntity entity)
        {
            // arrange
            this.sut.Insert(entity);
            entity = this.ChangeEntity(entity);

            // act
            this.sut.Update(entity);

            // assert
            TEntity item = this.storage[entity.Key];
            ValidateEntity(entity, item);
        }

        [Theory]
        [AutoData]
        public void Delete_GivenNoKey_ShouldNotDelete(TEntity entity)
        {
            // arrange
            this.sut.Insert(entity);

            // act
            bool result = this.sut.Delete(NewKey);

            // assert
            result.Should().BeFalse();
            this.storage.Count.Should().Be(1);
        }

        [Theory]
        [AutoData]
        public void Delete_GivenEntity_ShouldDelete(TEntity entity)
        {
            // arrange
            this.sut.Insert(entity);

            // act
            bool result = this.sut.Delete(entity.Key);

            // assert
            result.Should().BeTrue();
            this.storage.Count.Should().Be(0);
        }

        [Theory]
        [AutoData]
        public void GetEntity_GivenNoEntity_ShouldReturnNull(TEntity entity)
        {
            // arrange
            this.sut.Insert(entity);

            // act
            TEntity? item = this.sut.GetEntity(this.NewKey);

            // assert
            item.Should().BeNull();
        }

        [Theory]
        [AutoData]
        public void GetEntity_GivenEntity_ShouldReturn(TEntity entity)
        {
            // arrange
            this.sut.Insert(entity);

            // act
            TEntity? item = this.sut.GetEntity(entity.Key);

            // assert
            item.Should().NotBeNull();
            this.ValidateEntity(entity, item);
        }

        [Fact]
        public void GetAll_GivenStorageEmpty_ShouldReturnEmpty()
        { 
            // act
            IEnumerable<TEntity> itemList = this.sut.GetAll();

            // assert
            itemList.Should().NotBeNull();
            itemList.Should().BeEmpty();
        }

        [Theory]
        [AutoData]
        public void GetAll_GivenStorageHasEntity_ShouldReturn(List<TEntity> entityList)
        {
            // arrange
            foreach(TEntity entity in entityList)
            {
                this.storage.TryAdd(entity.Key, entity);
            }

            // act
            IEnumerable<TEntity> itemList = this.sut.GetAll();

            // assert
            itemList.Should().NotBeNull();
            itemList.Count().Should().Be(entityList.Count);
            foreach(TEntity item in itemList)
            {
                TEntity? entity = entityList.FirstOrDefault(entry => entry.Key.Equals(item.Key));
                entity.Should().NotBeNull();
                ValidateEntity(entity!, item);
            }
        }

        protected abstract TEntity NewEntity { get; }

        protected abstract TKey NewKey { get; }

        protected abstract TEntity ChangeEntity(TEntity entity);

        protected virtual void ValidateEntity(TEntity example, TEntity? result)
        {
            result.Should()
                  .NotBeNull();

            result!.Key.Should()
                   .Be(example.Key);
        }
    }
}