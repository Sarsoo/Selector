using System;
using System.Collections.Generic;
using QuikGraph;
using Selector.Net;
using Xunit;
using FluentAssertions;
using Moq;

namespace Selector.Tests.Net
{
    public class GraphTests
    {
        [Fact]
        public void Network()
        {
            var net = new Graph<string>();

            var trigger = new TriggerSource<string>()
            {
                Id = "trigger"
            };

            var sink = new EmptySink<string, object>()
            {
                Id = "sink"
            };

            net.AddEdge(
                trigger,
                sink
            );

            trigger.Trigger("payload");
        }

        [Fact]
        public void SourceReceivesPayload()
        {
            var net = new Graph<string>();

            var trigger = new TriggerSource<string>()
            {
                Id = "trigger"
            };

            var sink = new Mock<ISink<string>>();

            net.AddEdge(
                trigger,
                sink.Object
            );

            var payload = "payload";

            trigger.Trigger(payload);

            sink.Verify(a => a.Consume(payload), Times.Once());
            sink.VerifyNoOtherCalls();
        }

        [Fact]
        public void SourceReceivesGraphPayload()
        {
            var net = new Graph<string>();

            var sink = new Mock<ISink<string>>();
            sink.Setup(a => a.Topics).Returns(new[] {"topic1"});

            net.AddNode(
                sink.Object
            );

            var payload = "payload";

            net.Sink("topic1", payload);

            sink.Verify(a => a.Consume(payload), Times.Once());
            sink.Verify(a => a.Topics, Times.Once());
            sink.VerifyNoOtherCalls();
        }

        [Fact]
        public void SourceReceivesRepeatedPayload()
        {
            var net = new Graph<string>();

            var trigger = new TriggerSource<string>()
            {
                Id = "trigger"
            };

            var repeater = new Repeater<string>();
            var repeater2 = new Repeater<string>();
            var repeater3 = new Repeater<string>();

            var sink = new Mock<ISink<string>>();

            net.AddEdge(
                trigger,
                repeater
            );

            net.AddEdge(
                repeater,
                repeater2
            );

            net.AddEdge(
                repeater2,
                repeater3
            );

            net.AddEdge(
                repeater3,
                sink.Object
            );

            var payload = "payload";

            trigger.Trigger(payload);

            sink.Verify(a => a.Consume(payload), Times.Once());
            sink.VerifyNoOtherCalls();
        }
    }
}

