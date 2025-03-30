using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
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
            watcherMock.SetupAdd(w => w.ItemChange += It.IsAny<EventHandler<ListeningChangeEventArgs>>());
            watcherMock.SetupRemove(w => w.ItemChange -= It.IsAny<EventHandler<ListeningChangeEventArgs>>());

            var link = "https://link";
            var content = new StringContent("");
            var config = new WebHookConfig()
            {
                Url = link,
                Content = content,
            };

            var http = new HttpClient(httpHandlerMock.Object);

            var webHook = new WebHook(watcherMock.Object, http, config);

            webHook.Subscribe();
            watcherMock.Raise(w => w.ItemChange += null, this, new ListeningChangeEventArgs());

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
            };

            var http = new HttpClient(httpHandlerMock.Object);

            bool predicateEvent = false, successfulEvent = false, failedEvent = false;

            var webHook = new WebHook(watcherMock.Object, http, config);

            webHook.PredicatePass += (o, e) => { predicateEvent = predicate; };

            webHook.SuccessfulRequest += (o, e) => { successfulEvent = successful; };

            webHook.FailedRequest += (o, e) => { failedEvent = !successful; };

            await webHook.AsyncCallback(ListeningChangeEventArgs.From(new(), new(), new()));

            predicateEvent.Should().Be(predicate);
            successfulEvent.Should().Be(successful);
            failedEvent.Should().Be(!successful);
        }
    }
}