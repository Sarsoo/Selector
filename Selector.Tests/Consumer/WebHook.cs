﻿using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using Xunit;

namespace Selector.Tests
{
    public class WebHookTest
    {
        [Fact(Skip = "Not working atm")]
        public async Task TestHttpClientUsed()
        {
            var msg = new HttpResponseMessage(HttpStatusCode.OK);

            var httpHandlerMock = new Mock<HttpMessageHandler>();
            httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(msg);

            var watcherMock = new Mock<ISpotifyPlayerWatcher>();
            watcherMock.SetupAdd(w => w.ItemChange += It.IsAny<EventHandler<SpotifyListeningChangeEventArgs>>());
            watcherMock.SetupRemove(w => w.ItemChange -= It.IsAny<EventHandler<SpotifyListeningChangeEventArgs>>());

            var link = "https://link";
            var content = new StringContent("");
            var config = new WebHookConfig()
            {
                Url = link,
                Content = content,
                Name = "test"
            };

            var http = new HttpClient(httpHandlerMock.Object);

            var webHook = new WebHook(watcherMock.Object, http, config, logger: NullLogger<WebHook>.Instance);

            webHook.Subscribe();
            watcherMock.Raise(w => w.ItemChange += null, this,
                new SpotifyListeningChangeEventArgs()
                    { Id = "test", SpotifyUsername = "test", Current = null, Timeline = null });

            await Task.Delay(100);

            httpHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData(200, true, true)]
        [InlineData(404, true, false)]
        [InlineData(500, true, false)]
        public async Task TestEventFiring(int code, bool predicate, bool successful)
        {
            var msg = new HttpResponseMessage(Enum.Parse<HttpStatusCode>(code.ToString()));

            var httpHandlerMock = new Mock<HttpMessageHandler>();
            httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(msg);

            var watcherMock = new Mock<ISpotifyPlayerWatcher>();

            var link = "https://link";
            var content = new StringContent("");
            var config = new WebHookConfig()
            {
                Url = link,
                Content = content,
                Name = "test"
            };

            var http = new HttpClient(httpHandlerMock.Object);

            bool predicateEvent = false, successfulEvent = false, failedEvent = false;

            var webHook = new WebHook(watcherMock.Object, http, config, logger: NullLogger<WebHook>.Instance);

            webHook.PredicatePass += (o, e) => { predicateEvent = predicate; };

            webHook.SuccessfulRequest += (o, e) => { successfulEvent = successful; };

            webHook.FailedRequest += (o, e) => { failedEvent = !successful; };

            await (Task)typeof(WebHook).GetMethod("ProcessEvent", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(webHook,
                    new object[]
                    {
                        SpotifyListeningChangeEventArgs.From(new(), new(), new() { EqualityChecker = null }, null,
                            "test")
                    });

            predicateEvent.Should().Be(predicate);
            successfulEvent.Should().Be(successful);
            failedEvent.Should().Be(!successful);
        }
    }
}