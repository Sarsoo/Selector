using System;
using System.Collections.Generic;
using System.Linq;

namespace Selector.Model
{
    public interface IListenRepository
    {
        IQueryable<IListen> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null, bool tracking = true, bool orderTime = false);
        int Count(string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null);
    }
}

