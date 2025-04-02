using System.Net;

namespace Selector.AppleMusic.Exceptions;

public class AppleMusicException : Exception
{
    public HttpStatusCode StatusCode { get; set; }
}