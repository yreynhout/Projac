using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Raven.Client;

namespace Projac.RavenDB.Tests
{
    [TestFixture]
    public class RavenProjectionBuilderTests
    {
        private RavenProjectionBuilder _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new RavenProjectionBuilder();
        }

        [Test]
        public void DecoratedProjectionCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RavenProjectionBuilder(null));
        }

        [Test]
        public void DecoratedProjectionHandlersAreCopiedOnConstruction()
        {
            var handler1 = new RavenProjectionHandler(typeof(object), (connection, message, token) => Task.FromResult(false));
            var handler2 = new RavenProjectionHandler(typeof(object), (connection, message, token) => Task.FromResult(false));
            var projection = new RavenProjection(new[]
            {
                handler1, 
                handler2
            });
            var sut = new RavenProjectionBuilder(projection);

            var result = sut.Build();

            Assert.That(result.Handlers, Is.EquivalentTo(new[]
            {
                handler1, handler2
            }));

        }

        [Test]
        public void InitialInstanceBuildReturnsExpectedResult()
        {
            var result = _sut.Build();

            Assert.That(result.Handlers, Is.Empty);
        }

        [Test]
        public void WhenHandlerCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.When<object>((Func<IAsyncDocumentSession, object, Task>)null));
        }

        [Test]
        public void WhenHandlerWithTokenCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.When<object>((Func<IAsyncDocumentSession, object, CancellationToken, Task>)null));
        }

        [Test]
        public void WhenHandlerReturnsExpectedResult()
        {
            var result = _sut.When<object>((connection, message) => Task.FromResult(0));

            Assert.That(result, Is.InstanceOf<RavenProjectionBuilder>());
        }

        [Test]
        public void WhenHandlerRetainsMessageType()
        {
            _sut.When<Message>((connection, message) =>
            {
                Message _ = message;
                return Task.FromResult(0);
            });
        }

        class Message { }

        [Test]
        public void WhenHandlerWithTokenReturnsExpectedResult()
        {
            var result = _sut.When<object>((connection, message, token) => Task.FromResult(0));

            Assert.That(result, Is.InstanceOf<RavenProjectionBuilder>());
        }

        [Test]
        public void WhenHandlerWithTokenRetainsMessageType()
        {
            _sut.When<Message>((connection, message, token) =>
            {
                Message _ = message;
                return Task.FromResult(0);
            });
        }

        [Test]
        public void WhenHandlerIsPreservedUponBuild()
        {
            var task = Task.FromResult(false);
            Func<IAsyncDocumentSession, object, Task> handler = (connection, message) => task;
            var result = _sut.When<object>(handler).Build();

            Assert.That(
                result.Handlers.Count(_ => 
                    _.Message == typeof(object) && 
                    ReferenceEquals(_.Handler(null, null, CancellationToken.None), task)),
                Is.EqualTo(1));
        }

        [Test]
        public void WhenHandlerWithTokenIsPreservedUponBuild()
        {
            var task = Task.FromResult(false);
            Func<IAsyncDocumentSession, object, CancellationToken, Task> handler = (connection, message, token) => task;
            var result = _sut.When<object>(handler).Build();

            Assert.That(
                result.Handlers.Count(_ =>
                    _.Message == typeof(object) &&
                    ReferenceEquals(_.Handler(null, null, CancellationToken.None), task)),
                Is.EqualTo(1));
        }

        [Test]
        public void WhenHandlerPreservesPreviouslyCollectedHandlersUponBuild()
        {
            var task1 = Task.FromResult(false);
            var task2 = Task.FromResult(false);
            Func<IAsyncDocumentSession, object, Task> handler1 = (connection, message) => task1;
            Func<IAsyncDocumentSession, object, Task> handler2 = (connection, message) => task2;
            var result = _sut.When<object>(handler1).When<object>(handler2).Build();

            Assert.That(
                result.Handlers.Length,
                Is.EqualTo(2));

            Assert.That(
                result.Handlers.Count(_ => _.Message == typeof(object) && ReferenceEquals(_.Handler(null, null, CancellationToken.None), task1)),
                Is.EqualTo(1));

            Assert.That(
                result.Handlers.Count(_ => _.Message == typeof(object) && ReferenceEquals(_.Handler(null, null, CancellationToken.None), task2)),
                Is.EqualTo(1));
        }

        [Test]
        public void WhenHandlerWithTokenPreservesPreviouslyCollectedHandlersUponBuild()
        {
            var task1 = Task.FromResult(false);
            var task2 = Task.FromResult(false);
            Func<IAsyncDocumentSession, object, CancellationToken, Task> handler1 = (connection, message, token) => task1;
            Func<IAsyncDocumentSession, object, CancellationToken, Task> handler2 = (connection, message, token) => task2;
            var result = _sut.When<object>(handler1).When<object>(handler2).Build();

            Assert.That(
                result.Handlers.Length,
                Is.EqualTo(2));

            Assert.That(
                result.Handlers.Count(_ => _.Message == typeof(object) && ReferenceEquals(_.Handler(null, null, CancellationToken.None), task1)),
                Is.EqualTo(1));

            Assert.That(
                result.Handlers.Count(_ => _.Message == typeof(object) && ReferenceEquals(_.Handler(null, null, CancellationToken.None), task2)),
                Is.EqualTo(1));
        }
    }
}