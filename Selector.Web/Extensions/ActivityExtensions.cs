using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Selector.Web.Extensions;

public static class ActivityExtensions
{
    public static void Enrich(this Activity activity, HttpContext context)
    {
        if (context.User.Identity is not null)
        {
            activity?.AddBaggage(TraceConst.Username, context.User.Identity?.Name);
        }

        try
        {
            activity?.AddBaggage(TraceConst.Username, context.Session.Id);
        }
        catch (InvalidOperationException)
        {
        }

        activity?.AddBaggage(TraceConst.ClientPort, context.Connection.RemotePort.ToString());
        if (context.Connection.RemoteIpAddress is not null)
        {
            activity?.AddBaggage(TraceConst.ClientAddress, context.Connection.RemoteIpAddress.ToString());
        }
    }

    public static void Enrich(this Activity activity, HubCallerContext context)
    {
        if (context.User?.Identity is not null)
        {
            activity?.AddBaggage(TraceConst.Username, context.User.Identity?.Name);
        }

        var httpContext = context.GetHttpContext();
        if (httpContext is not null)
        {
            try
            {
                activity?.AddBaggage(TraceConst.Username, httpContext.Session.Id);
            }
            catch (InvalidOperationException)
            {
            }

            activity?.AddBaggage(TraceConst.ClientPort, httpContext.Connection.RemotePort.ToString());
            if (httpContext.Connection.RemoteIpAddress is not null)
            {
                activity?.AddBaggage(TraceConst.ClientAddress, httpContext.Connection.RemoteIpAddress.ToString());
            }
        }
    }
}