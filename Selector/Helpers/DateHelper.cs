using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    public static class DateHelper
    {
        public static DateTime FromUnixMilli(long milli) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milli);
    }
}
