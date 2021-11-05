using System;
using System.Linq;

using Selector.Model;

namespace Selector.Web.Service
{
    public interface IUserMapping {
        public void FormAll();
    }

    public class NowPlayingUserMapping: IUserMapping
    {
        private readonly ApplicationDbContext Db;
        private readonly CacheHubProxy Proxy;
        private readonly INowPlayingMappingFactory NowPlayingMappingFactory;

        public NowPlayingUserMapping(
            ApplicationDbContext db,
            CacheHubProxy proxy,
            INowPlayingMappingFactory nowPlayingMappingFactory
        )
        {
            Db = db;
            Proxy = proxy;
            NowPlayingMappingFactory = nowPlayingMappingFactory;
        }

        public void FormAll()
        {
            foreach(var user in Db.Users)
            {
                Proxy.FormMapping(NowPlayingMappingFactory.Get(user.Id, user.UserName));
            }
        }
    }
}