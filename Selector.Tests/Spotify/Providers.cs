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
    public class RefreshTokenFactoryProviderTests
    {
        [Fact]
        public void Init()
        {
            var provider = new RefreshTokenFactoryProvider();

            provider.Initialised.Should().BeFalse();

            provider.Initialise("a", "b");
            
            provider.Initialised.Should().BeTrue();

            provider.Initialise("a", "");

            provider.Initialised.Should().BeFalse();

            provider.Initialise(null, "b");

            provider.Initialised.Should().BeFalse();
        }

        [Fact]
        public void Get()
        {
            var provider = new RefreshTokenFactoryProvider();

            provider.Initialise("a", "b");
            
            var consumer = provider.GetFactory("a");

            consumer.Should().NotBeNull();
            consumer.Result.Should().NotBeNull();
        }
    }
}
