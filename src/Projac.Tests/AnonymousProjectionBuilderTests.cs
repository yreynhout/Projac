﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Projac.Tests
{
    [TestFixture]
    public class AnonymousProjectionBuilderTests
    {
        private AnonymousProjectionBuilder<CallRecordingConnection> _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new AnonymousProjectionBuilder<CallRecordingConnection>();
        }

        [Test]
        public void HandlersCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnonymousProjectionBuilder<object>(null));
        }

        [Test]
        public void HandlersAreCopiedOnConstruction()
        {
            var handler1 = new ProjectionHandler<object>(
                typeof (object),
                (_, __, ___) => TaskFactory());
            var handler2 = new ProjectionHandler<object>(
                typeof (object),
                (_, __, ___) => TaskFactory());
            var sut = new AnonymousProjectionBuilder<object>(new[]
            {
                handler1, 
                handler2
            });

            var result = sut.Build();

            Assert.That(result, Is.EquivalentTo(new[]
            {
                handler1, handler2
            }));

        }

        [Test]
        public void EmptyInstanceBuildReturnsExpectedResult()
        {
            var result = new AnonymousProjectionBuilder<object>().Build();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void InitialInstanceBuildReturnsExpectedResult()
        {
            var sut = new AnonymousProjectionBuilder<object>();

            var result = sut.Build();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void HandleHandlerWithoutTokenCanNotBeNull()
        {
            var sut = new AnonymousProjectionBuilder<object>();
            Assert.Throws<ArgumentNullException>(() => sut.Handle((Func<object, object, Task>)null));
        }

        [Test]
        public void HandleHandlerWithTokenCanNotBeNull()
        {
            var sut = new AnonymousProjectionBuilder<object>();
            Assert.Throws<ArgumentNullException>(() => sut.Handle((Func<object, object, CancellationToken, Task>)null));
        }

        [Test]
        public void HandleSyncHandlerCanNotBeNull()
        {
            var sut = new AnonymousProjectionBuilder<object>();
            Assert.Throws<ArgumentNullException>(() => sut.Handle((Action<object, object>)null));
        }

        [Test]
        public void HandleHandlerWithoutTokenReturnsExpectedResult()
        {
            var sut = new AnonymousProjectionBuilder<object>();

            var result = sut.Handle<object>((_, __) => TaskFactory());

            Assert.That(result, Is.InstanceOf<AnonymousProjectionBuilder<object>>());
        }

        [Test]
        public void HandleHandlerWithTokenReturnsExpectedResult()
        {
            var sut = new AnonymousProjectionBuilder<object>();

            var result = sut.Handle<object>((_, __, ___) => TaskFactory());

            Assert.That(result, Is.InstanceOf<AnonymousProjectionBuilder<object>>());
        }

        [Test]
        public void HandleSyncHandlerReturnsExpectedResult()
        {
            var sut = new AnonymousProjectionBuilder<object>();

            var result = sut.Handle<object>((_, __) => { });

            Assert.That(result, Is.InstanceOf<AnonymousProjectionBuilder<object>>());
        }

        [Test]
        public void HandleHandlerWithoutTokenIsPreservedUponBuild()
        {
            Func<CallRecordingConnection,object, Task> handler =
                (connection, message) =>
                {
                    connection.RecordCall(1, message, CancellationToken.None);
                    return TaskFactory();
                };
            
            var result = _sut.Handle(handler).Build();

            Assert.That(
                result.Count(_ =>
                {
                    if (_.Message == typeof (object))
                    {
                        var msg = new object();
                        var recorder = new CallRecordingConnection();
                        _.Handler(recorder, msg, CancellationToken.None).Wait();
                        return recorder.RecordedCalls.SequenceEqual(new[]
                        {
                            new Tuple<int, object, CancellationToken>(
                                1, msg, CancellationToken.None)
                        });
                    } 
                    return false;
                }),
                Is.EqualTo(1));
        }

        [Test]
        public void HandleSyncHandlerIsPreservedUponBuild()
        {
            Action<CallRecordingConnection, object> handler =
                (connection, message) =>
                {
                    connection.RecordCall(1, message, CancellationToken.None);
                };

            var result = _sut.Handle(handler).Build();

            Assert.That(
                result.Count(_ =>
                {
                    if (_.Message == typeof(object))
                    {
                        var msg = new object();
                        var recorder = new CallRecordingConnection();
                        _.Handler(recorder, msg, CancellationToken.None).Wait();
                        return recorder.RecordedCalls.SequenceEqual(new[]
                        {
                            new Tuple<int, object, CancellationToken>(
                                1, msg, CancellationToken.None)
                        });
                    }
                    return false;
                }),
                Is.EqualTo(1));
        }

        [Test]
        public void HandleHandlerWithTokenIsPreservedUponBuild()
        {
            Func<CallRecordingConnection, object, CancellationToken, Task> handler =
                (connection, message, token) =>
                {
                    connection.RecordCall(1, message, token);
                    return TaskFactory();
                };

            var result = _sut.Handle(handler).Build();

            Assert.That(
                result.Count(_ =>
                {
                    if (_.Message == typeof(object))
                    {
                        var msg = new object();
                        var recorder = new CallRecordingConnection();
                        var token = new CancellationToken();
                        _.Handler(recorder, msg, token).Wait();
                        return recorder.RecordedCalls.SequenceEqual(new[]
                        {
                            new Tuple<int, object, CancellationToken>(
                                1, msg, token)
                        });
                    }
                    return false;
                }),
                Is.EqualTo(1));
        }

        [Test]
        public void HandleHandlerWithTokenPreservesPreviouslyRegisteredHandlersUponBuild()
        {
            Func<CallRecordingConnection, object, CancellationToken, Task> handler1 =
                (connection, message, token) =>
                {
                    connection.RecordCall(1, message, token);
                    return TaskFactory();
                };

            Func<CallRecordingConnection, object, CancellationToken, Task> handler2 =
                (connection, message, token) =>
                {
                    connection.RecordCall(2, message, token);
                    return TaskFactory();
                };

            var result = _sut.Handle(handler1).Handle(handler2).Build();

            Assert.That(
                result.Count(_ =>
                {
                    if (_.Message == typeof(object))
                    {
                        var msg = new object();
                        var recorder = new CallRecordingConnection();
                        var token = new CancellationToken();
                        _.Handler(recorder, msg, token).Wait();
                        return recorder.RecordedCalls.SequenceEqual(new[]
                        {
                            new Tuple<int, object, CancellationToken>(
                                1, msg, token)
                        });
                    }
                    return false;
                }),
                Is.EqualTo(1));
        }

        [Test]
        public void HandleSyncHandlerPreservesPreviouslyRegisteredHandlersUponBuild()
        {
            Func<CallRecordingConnection, object, CancellationToken, Task> handler1 =
                (connection, message, token) =>
                {
                    connection.RecordCall(1, message, token);
                    return TaskFactory();
                };

            Action<CallRecordingConnection, object> handler2 =
                (connection, message) =>
                {
                    connection.RecordCall(2, message, CancellationToken.None);
                };

            var result = _sut.Handle(handler1).Handle(handler2).Build();

            Assert.That(
                result.Count(_ =>
                {
                    if (_.Message == typeof(object))
                    {
                        var msg = new object();
                        var recorder = new CallRecordingConnection();
                        var token = new CancellationToken();
                        _.Handler(recorder, msg, token).Wait();
                        return recorder.RecordedCalls.SequenceEqual(new[]
                        {
                            new Tuple<int, object, CancellationToken>(
                                1, msg, token)
                        });
                    }
                    return false;
                }),
                Is.EqualTo(1));
        }

        [Test]
        public void HandleHandlerWithoutTokenPreservesPreviouslyRegisteredHandlersUponBuild()
        {
            Func<CallRecordingConnection, object, Task> handler1 =
                (connection, message) =>
                {
                    connection.RecordCall(1, message, CancellationToken.None);
                    return TaskFactory();
                };

            Func<CallRecordingConnection, object, Task> handler2 =
                (connection, message) =>
                {
                    connection.RecordCall(2, message, CancellationToken.None);
                    return TaskFactory();
                };

            var result = _sut.Handle(handler1).Handle(handler2).Build();

            Assert.That(
                result.Count(_ =>
                {
                    if (_.Message == typeof(object))
                    {
                        var msg = new object();
                        var recorder = new CallRecordingConnection();
                        _.Handler(recorder, msg, CancellationToken.None).Wait();
                        return recorder.RecordedCalls.SequenceEqual(new[]
                        {
                            new Tuple<int, object, CancellationToken>(
                                1, msg, CancellationToken.None)
                        });
                    }
                    return false;
                }),
                Is.EqualTo(1));
        }

        private static Task TaskFactory()
        {
            return Task.FromResult<object>(null);
        }
    }
}
