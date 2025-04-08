using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Selector.Spotify.ConfigFactory;
using Selector.Spotify.Consumer.Factory;
using Xunit;

namespace Selector.Tests
{
    [Obsolete]
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