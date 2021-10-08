using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using FluentAssertions;
using SpotifyAPI.Web;

using Selector;
using System.Threading.Tasks;

namespace Selector.Tests
{
    public class WatcherCollectionTests
    {
        [Fact]
        public void Count()
        {
            var watchCollection = new WatcherCollection();

            var watcherMock = new Mock<IWatcher>();

            watchCollection.Add(watcherMock.Object);
            watchCollection.Add(watcherMock.Object);
            watchCollection.Add(watcherMock.Object);

            watchCollection.Count.Should().Be(3);
        }

        [Fact]
        public void StartAndStop()
        {
            var watchCollection = new WatcherCollection();

            var watcherMock = new Mock<IWatcher>();

            watchCollection.Add(watcherMock.Object);
            watchCollection.Count.Should().Be(1);

            watchCollection.Start();

            watchCollection.IsRunning.Should().BeTrue();
            watchCollection.First().IsRunning.Should().BeTrue();
            watchCollection.Running.Count().Should().Be(1);

            watchCollection.Stop();

            watchCollection.IsRunning.Should().BeFalse();
            watchCollection.First().IsRunning.Should().BeFalse();

            watchCollection.Running.Count().Should().Be(0);
            watchCollection.TokenSources.First().IsCancellationRequested.Should().BeTrue();
        }
    }
}
