using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Selector.Model.Authorisation
{
    public static class WatcherOperations
    {
        public static OperationAuthorizationRequirement Create = new() { Name = Constants.CreateOpName };
        public static OperationAuthorizationRequirement Read = new() { Name = Constants.ReadOpName };
        public static OperationAuthorizationRequirement Update = new() { Name = Constants.UpdateOpName };
        public static OperationAuthorizationRequirement Delete = new() { Name = Constants.DeleteOpName };
    }

    public static class UserOperations
    {
        public static OperationAuthorizationRequirement Create = new() { Name = Constants.CreateOpName };
        public static OperationAuthorizationRequirement Read = new() { Name = Constants.ReadOpName };
        public static OperationAuthorizationRequirement Update = new() { Name = Constants.UpdateOpName };
        public static OperationAuthorizationRequirement Delete = new() { Name = Constants.DeleteOpName };
    }

    public class Constants
    {
        public const string CreateOpName = "Create";
        public const string ReadOpName = "Read";
        public const string UpdateOpName = "Update";
        public const string DeleteOpName = "Delete";

        public const string AdminRole = "Admin";
    }
}
