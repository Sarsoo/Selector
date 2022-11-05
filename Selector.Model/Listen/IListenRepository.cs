using System;
using System.Collections.Generic;

namespace Selector.Model
{
    public interface IListenRepository
    {
        IEnumerable<IListen> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null, bool tracking = true, bool orderTime = false);
        int Count(string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null);
    }
}

