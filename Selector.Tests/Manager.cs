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
    public class ManagerTests
    {
        [Fact]
        public void Count()
        {
            var manager = new Manager();

            var watcherMock = new Mock<IWatcher>();

            manager.Add(watcherMock.Object);
            manager.Add(watcherMock.Object);
            manager.Add(watcherMock.Object);

            manager.Count.Should().Be(3);
        }

        [Fact]
        public void StartAndStop()
        {
            var manager = new Manager();

            var watcherMock = new Mock<IWatcher>();

            manager.Add(watcherMock.Object);
            manager.Count.Should().Be(1);

            manager.Start();

            manager.IsRunning.Should().BeTrue();
            manager.Running.Count().Should().Be(1);

            var context = manager.Running.First();

            manager.Stop();

            manager.IsRunning.Should().BeFalse();
            context.IsRunning.Should().BeFalse();

            manager.Running.Count().Should().Be(0);
            manager.TokenSources.First().IsCancellationRequested.Should().BeTrue();
        }
    }
}
