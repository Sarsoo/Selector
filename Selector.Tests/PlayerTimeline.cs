using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using FluentAssertions;
using SpotifyAPI.Web;

using Selector;

namespace Selector.Tests
{
    public class PlayerTimelineTests
    {
        public static IEnumerable<object[]> CountData => 
        new List<object[]>
        {
            new object[] {
                new CurrentlyPlayingContext[]
                {
                    Helper.CurrentPlayback(Helper.FullTrack("uri1"))
                }
            },
            new object[] {
                new CurrentlyPlayingContext[]
                {
                    Helper.CurrentPlayback(Helper.FullTrack("uri1")),
                    Helper.CurrentPlayback(Helper.FullTrack("uri2")),
                    Helper.CurrentPlayback(Helper.FullTrack("uri3")),
                }
            },
        };

        [Theory]
        [MemberData(nameof(CountData))]
        public void Count(CurrentlyPlayingContext[] currentlyPlaying)
        {
            var timeline = new PlayerTimeline();

            foreach(var i in currentlyPlaying)
            {
                timeline.Add(i);
            }

            timeline.Count.Should().Be(currentlyPlaying.Length);
        }

        [Fact]
        public void Clear()
        {
            var timeline = new PlayerTimeline();
            var tracks = new CurrentlyPlayingContext[]
            {
                Helper.CurrentPlayback(Helper.FullTrack("uri1")),
                Helper.CurrentPlayback(Helper.FullTrack("uri2")),
                Helper.CurrentPlayback(Helper.FullTrack("uri3")),
            };

            foreach (var i in tracks)
            {
                timeline.Add(i);
            }

            timeline.Clear();

            timeline.Count.Should().Be(0);
        }

        [Fact]
        public void Sort()
        {
            var timeline = new PlayerTimeline(){
                SortOnBackDate = false
            };

            var earlier = Helper.CurrentPlayback(Helper.FullTrack("uri1"));
            var earlierDate = DateTime.Now;

            var later = Helper.CurrentPlayback(Helper.FullTrack("uri2"));
            var laterDate = DateTime.Now.AddDays(2);

            timeline.Add(later, laterDate);
            timeline.Add(earlier, earlierDate);

            timeline.Select(i => i.Item).Should().Equal(later, earlier);

            timeline.Sort();

            timeline.Select(i => i.Item).Should().Equal(earlier, later);
        }

        [Fact]
        public void Sort3()
        {
            var timeline = new PlayerTimeline()
            {
                SortOnBackDate = false
            };

            var earlier = Helper.CurrentPlayback(Helper.FullTrack("uri1"));
            var earlierDate = DateTime.Now;

            var middle= Helper.CurrentPlayback(Helper.FullTrack("uri3"));
            var middleDate = DateTime.Now.AddDays(1);

            var later = Helper.CurrentPlayback(Helper.FullTrack("uri2"));
            var laterDate = DateTime.Now.AddDays(2);

            timeline.Add(later, laterDate);
            timeline.Add(earlier, earlierDate);
            timeline.Add(middle, middleDate);

            timeline.Select(i => i.Item).Should().Equal(later, earlier, middle);

            timeline.Sort();

            timeline.Select(i => i.Item).Should().Equal(earlier, middle, later);
        }
    }
}
