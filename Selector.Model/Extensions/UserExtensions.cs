using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selector.Model.Extensions
{
    public static class UserExtensions
    {
        public static bool LastFmConnected(this ApplicationUser user)
            => !string.IsNullOrEmpty(user.LastFmUsername);

        public static bool ScrobbleSavingEnabled(this ApplicationUser user)
            => user.LastFmConnected() && user.SaveScrobbles;
    }
}
