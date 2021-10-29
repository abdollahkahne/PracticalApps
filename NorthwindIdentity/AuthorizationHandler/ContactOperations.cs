using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace NorthwindIdentity.AuthorizationHandler
{
    public static class ContactOperationsRequirements
    {
        public static OperationAuthorizationRequirement Create=new OperationAuthorizationRequirement {Name=Constants.Create};
        public static OperationAuthorizationRequirement Read=new OperationAuthorizationRequirement {Name=Constants.Read};
        public static OperationAuthorizationRequirement Update=new OperationAuthorizationRequirement {Name=Constants.Update};
        public static OperationAuthorizationRequirement Delete=new OperationAuthorizationRequirement {Name=Constants.Delete};
        public static OperationAuthorizationRequirement Approve=new OperationAuthorizationRequirement {Name=Constants.Approve};
        public static OperationAuthorizationRequirement Reject=new OperationAuthorizationRequirement {Name=Constants.Reject};
    }
    public static class Constants {
        public static readonly string Create="Create";
        public static readonly string Read="Read";
        public static readonly string Update="Update";
        public static readonly string Delete="Delete";
        public static readonly string Approve="Approve";
        public static readonly string Reject="Reject";
    }
}