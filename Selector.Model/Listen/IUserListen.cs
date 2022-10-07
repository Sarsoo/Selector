using System;

namespace Selector.Model;

public interface IUserListen: IListen
{
    string UserId { get; set; }
    ApplicationUser User { get; set; }
}

