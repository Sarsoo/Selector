using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    public interface IEqual
    {
        public bool IsEqual<T>(T item, T other);
    }
}
