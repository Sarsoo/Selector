using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Selector.AppleMusic;
using Selector.AppleMusic.Watcher;
using Xunit;

namespace Selector.Tests.Apple;

public class AppleTimelineTests
{
    public static IEnumerable<object[]> MatchingData =>
        new List<object[]>
        {
            new object[]
            {
                new List<List<AppleMusicCurrentlyPlayingContext>>
                {
                    new()
                    {
                        Helper.AppleContext("1"),
                        Helper.AppleContext("2"),
                        Helper.AppleContext("3"),
                    }
                },
                new List<string>
                {
                    "1", "2", "3"
                },
                new List<List<string>>
                {
                    new()
                    {
                        // "1", "2", "3"
                    }
                }
            },
            new object[]
            {
                new List<List<AppleMusicCurrentlyPlayingContext>>
                {
                    new()
                    {
                        Helper.AppleContext("1"),
                        Helper.AppleContext("2"),
                        Helper.AppleContext("3"),
                    },
                    new()
                    {
                        Helper.AppleContext("3"),
                        Helper.AppleContext("4"),
                        Helper.AppleContext("5"),
                    }
                },
                new List<string>
                {
                    "1", "2", "3", "4", "5"
                },
                new List<List<string>>
                {
                    new()
                    {
                        // "1", "2", "3"
                    },
                    new()
                    {
                        "4", "5",
                    }
                }
            },
            new object[]
            {
                new List<List<AppleMusicCurrentlyPlayingContext>>
                {
                    new()
                    {
                        Helper.AppleContext("1"),
                        Helper.AppleContext("2"),
                        Helper.AppleContext("3"),
                    },
                    new()
                    {
                        Helper.AppleContext("3"),
                        Helper.AppleContext("4"),
                        Helper.AppleContext("5"),
                    },
                    new()
                    {
                        Helper.AppleContext("3"),
                        Helper.AppleContext("4"),
                        Helper.AppleContext("5"),
                    },
                    new()
                    {
                        Helper.AppleContext("5"),
                        Helper.AppleContext("6"),
                        Helper.AppleContext("7"),
                    }
                },
                new List<string>
                {
                    "1", "2", "3", "4", "5", "6", "7"
                },
                new List<List<string>>
                {
                    new()
                    {
                        // "1", "2", "3"
                    },
                    new()
                    {
                        "4", "5",
                    },
                    new()
                    {
                    },
                    new()
                    {
                        "6", "7",
                    }
                }
            },
            new object[]
            {
                new List<List<AppleMusicCurrentlyPlayingContext>>
                {
                    new()
                    {
                        Helper.AppleContext("1"),
                        Helper.AppleContext("2"),
                        Helper.AppleContext("3"),
                    },
                    new()
                    {
                        Helper.AppleContext("3"),
                        Helper.AppleContext("4"),
                        Helper.AppleContext("5"),
                    },
                    new()
                    {
                        Helper.AppleContext("3"),
                        Helper.AppleContext("4"),
                        Helper.AppleContext("5"),
                    },
                    new()
                    {
                        Helper.AppleContext("1"),
                        Helper.AppleContext("2"),
                        Helper.AppleContext("3"),
                    }
                },
                new List<string>
                {
                    "1", "2", "3", "4", "5"
                },
                new List<List<string>>
                {
                    new()
                    {
                        // "1", "2", "3"
                    },
                    new()
                    {
                        "4", "5",
                    },
                    new()
                    {
                    },
                    new()
                    {
                    }
                }
            }
        };

    [Theory]
    [MemberData(nameof(MatchingData))]
    public void Matching(List<List<AppleMusicCurrentlyPlayingContext>> currentlyPlaying, List<string> expectedContent,
        List<List<string>> expectedResult)
    {
        var timeline = new AppleTimeline();

        foreach (var (batch, expectedReturn) in currentlyPlaying.Zip(expectedResult))
        {
            var newItems = timeline.Add(batch);
            newItems.Select(x => x.Track.Id).Should().ContainInOrder(expectedReturn);
        }

        timeline
            .Select(x => x.Item.Track.Id)
            .Should()
            .ContainInOrder(expectedContent);
    }
}