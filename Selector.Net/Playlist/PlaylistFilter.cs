using System;

namespace Selector.Net.Playlist
{
    public class PlaylistFilterConfig
    {
        public IEnumerable<string> NameWhiteList { get; set; }
        public IEnumerable<string> NameBlackList { get; set; }
        public IEnumerable<string> UriWhiteList { get; set; }
        public IEnumerable<string> UriBlackList { get; set; }
    }

    public class PlaylistFilter<TNodeId> : BaseSinkSource<TNodeId, PlaylistChangeEventArgs>
    {
        public IEnumerable<string> NameWhiteList { get; set; }
        public IEnumerable<string> NameBlackList { get; set; }
        public IEnumerable<string> UriWhiteList { get; set; }
        public IEnumerable<string> UriBlackList { get; set; }

        public PlaylistFilter() { }

        public PlaylistFilter(PlaylistFilterConfig config)
        {
            NameWhiteList = config.NameWhiteList;
            NameBlackList = config.NameBlackList;
            UriWhiteList = config.UriWhiteList;
            UriBlackList = config.UriBlackList;
        }

        public override Task ConsumeType(PlaylistChangeEventArgs obj)
        {
            if (NameWhiteList is not null && NameWhiteList.Any())
            {
                if (NameWhiteList.Contains(obj.Current.Name))
                {
                    Emit(obj);
                }
            }

            if (NameBlackList is not null && NameBlackList.Any())
            {
                if (!NameBlackList.Contains(obj.Current.Name))
                {
                    Emit(obj);
                }
            }

            if (UriWhiteList is not null && UriWhiteList.Any())
            {
                if (UriWhiteList.Contains(obj.Current.Name))
                {
                    Emit(obj);
                }
            }

            if (UriBlackList is not null && UriBlackList.Any())
            {
                if (!UriBlackList.Contains(obj.Current.Name))
                {
                    Emit(obj);
                }
            }

            return Task.CompletedTask;
        }
    }
}

