using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using FluentAssertions;
using SpotifyAPI.Web;

using Selector;

namespace Selector.Tests
{
    public class AudioInjectorFactoryTests
    {
        [Fact]
        public void Get()
        {
            var logMock = new Mock<ILoggerFactory>();
            var configFactoryMock = new Mock<ISpotifyConfigFactory>();

            var factory = new AudioFeatureInjectorFactory(logMock.Object);

            var consumer = factory.Get(configFactoryMock.Object);

            configFactoryMock.Verify(m => m.GetConfig(), Times.Once);
            consumer.Should().NotBeNull();
        }
    }
}
