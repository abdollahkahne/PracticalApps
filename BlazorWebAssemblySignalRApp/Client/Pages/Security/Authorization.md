Authorization in Asp.Net Core
Authorization refers to the process that determines what a user is able to do. However, authorization requires an authentication mechanism, Authorization is orthogonal and independent from authentication. Authentication is the process of ascertaining who a user is. Authentication may create one or more identities for the current user.
Authorization is expressed in requirements, and handlers evaluate a user's claims against requirements. We have two type of Authorization:
* Imperative checks can be based on simple policies or policies which evaluate both the user identity and properties of the resource that the user is attempting to access (Using AuthorizationService).
* Declarative simple, role-based or policy-based model ([Authorize] and [AllowAnynomous] Attributes)
Note: [AllowAnonymous] bypasses all authorization statements. If you combine [AllowAnonymous] and any [Authorize] attribute, the [Authorize] attributes are ignored. 
Note: The AuthorizeAttribute can not be applied to Razor Page handlers.

Simple Authorization
In its most basic form, applying the [Authorize] attribute to a controller, action, or Razor Page, limits access to that component authenticated users.

Role-based Authorization
You can use the [Authorize(Roles="")] to declaratively implement role base authorization. You can also use ClaimPrincipal.IsInRole() method to impretively do it. 
Note: Multiple roles can be specified as a comma separated list
Note: If you apply multiple attributes then an accessing user must be a member of all the roles specified.
You can also do Role-based authorization through implementing a Role Policy and then using Policy-based implementation.
Note: If you are using Identity for authentication, you should add Role Service by following code:
services.AddDefaultIdentity<IdentityUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

Claim-Based Authorization
If you want to implement it declaratively, you should first define a policy which require that claim (Its existence and its existence and a specified value/ Value List) and then use Policy-based authoirzation.

Policy-based Authorization
You can implement a policy-based authorization using declarative [Authorize(Policy = "RequireAdministratorRole")] attribute. You should add policy by policy builder first. What happen underneath is an AuthorizationService which generate an AuthorizationContext using users and requirements to be passed by current policy and the resource which requirements should check against and then call authorization handler responsible for handling the requirement and at the end check if any requirement failed to be met.

Resource-Based Authorization
Authorization approach depends on the resource. For example, only the author of a document is authorized to update the document. Consequently, the document must be retrieved from the data store before authorization evaluation can occur. Declarative authorization with an [Authorize] attribute doesn't suffice since Attribute evaluation occurs before data binding and before execution of the page handler or action that loads the document. Instead, you can invoke a custom authorization method—a style known as imperative authorization using an IAuthorizationService instance injected by Dependency Injection. 

View-Based Authorization
A developer often wants to show, hide, or otherwise modify a UI based on the current user identity. You can access the authorization service within MVC views via dependency injection. To inject the authorization service into a Razor view, use the @inject directive. This is also is an imperative authorization.

Terminology
Underneath the declarative authorization like role-based authorization and claims-based authorization use a requirement, a requirement handler, and a pre-configured policy. These building blocks support the expression of authorization evaluations in code. The result is a richer, reusable, testable authorization structure.

Policy
An authorization policy consists of one or more requirements. It's registered as part of the authorization service configuration. To make a policy, we use Policy Builder which simply added some requirement as policy. The requirement may be added by extension methods like RequireRoles, RequireAssert or simply by calling builder.Requirements.Add(). We should register Requirement Handler befor we use them in Policies. 

Requirement
A requirement is what the user is required to be to the authorization process succeed. As examle consider the role-based authentication which we have a Role Requirement. 
To define it in .Net, make it marked using an interface IAuthorizationRequirement as Requirement and have data related to requirement (An authorization requirement is a collection of data parameters that a policy can use to evaluate the current user principal. Their value specified when we define the policy. We can have multiple instance of a requirement class as requirement with different data for different policies). For example we have an AgeRequirement and we want to make it dynamic. We simply add an int field (for age requirement) to it and use it as requirement. If an authorization policy contains multiple authorization requirements, all requirements must pass in order for the policy evaluation to succeed. In other words, multiple authorization requirements added to a single authorization policy are treated on an AND basis.
Note: A requirement doesn't need to have data or properties.

Requirement Handler
An authorization handler is responsible for the evaluation of a requirement's properties. The authorization handler evaluates the requirements against a provided AuthorizationHandlerContext to determine if access is allowed.
A requirement can have multiple handlers. A handler may inherit AuthorizationHandler<TRequirement>, where TRequirement is the requirement to be handled. Alternatively, a handler may implement IAuthorizationHandler to handle more than one type of requirement.
Note: We should add AuthorizationHandler to Service Container before we use them.
Note: It's possible to bundle both a requirement and a handler in a single class implementing both IAuthorizationRequirement and IAuthorizationHandler. This bundling creates a tight coupling between the handler and requirement and is only recommended for simple requirements and handlers. Creating a class that implements both interfaces removes the need to register the handler in DI because of the built-in PassThroughAuthorizationHandler that allows requirements to handle themselves.
Note: If you require access to HttpContext in handler, study this section:
https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0#access-mvc-request-context-in-handlers

Authorization Service
The following code shows the simplified (and annotated with comments) default implementation of the authorization service (Default Authorization Service):
public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, 
             object resource, IEnumerable<IAuthorizationRequirement> requirements)
{
    // Create a tracking context from the authorization inputs.
    var authContext = _contextFactory.CreateContext(requirements, user, resource);

    // By default this returns an IEnumerable<IAuthorizationHandlers> from DI.
    var handlers = await _handlers.GetHandlersAsync(authContext);

    // Invoke all handlers.
    foreach (var handler in handlers)
    {
        await handler.HandleAsync(authContext);
    }

    // Check the context, by default success is when all requirements have been met.
    return _evaluator.Evaluate(authContext);
}

Authorization Type and Classes
The primary service that determines if authorization is successful is IAuthorizationService

IAuthorizationService
The authorization service responsible for checking policies (DefaultAuthorizationService) is implemented this interface. It has one main method AuthorizeAsync which has multiple overloads:
* AuthorizeAsync(ClaimsPrincipal user, Object resource, IEnumerable<IAuthorizationRequirement> requirements): Checks if a user meets a specific set of requirements for the specified resource
* AuthorizeAsync(ClaimsPrincipal user, Object resource, String policy): Checks if a user meets a specific authorization policy
And the following extension overloads:
* AuthorizeAsync(ClaimsPrincipal user, Object resource, IAuthorizationRequirement requirement): Checks if a user meets a specific requirement for the specified resource

IAuthorizationRequirement
IAuthorizationRequirement is a marker service with no methods, and the mechanism for tracking whether authorization is successful. Implement this interface to make the requirement usable in a method which takes an input with this type (or as generic type when it is required). There are some implemeted type for this interface which can be used for simple requirements:
OperationAuthorizationRequirement
A helper class to provide a useful IAuthorizationRequirement which contains a name. It has only one Property as below:
* Name (string): The name of this instance of IAuthorizationRequirement.
Use this class if you only require to make your requirement as soon as possible and do evaluation of requirement on  compare with a name field.
In the following we do not allow manager to do other operations than Approve or Reject:
// // Manager can only do Approve Or Reject (He can not do CRUD for other people)
if (!(requirement.Name==Constants.Approve  || requirement.Name==Constants.Reject)) {
    return Task.CompletedTask;
} else {
    if (context.User.IsInRole(Roles.Manager.ToString())) {
    context.Succeed(requirement);
    }
}

IAuthorizationHandler
Each IAuthorizationHandler is responsible for checking if requirements are met. Classes implementing this interface are able to make a decision if authorization is allowed. It has only one method to implement which return a Task:
* HandleAsync(AuthorizationHandlerContext)
Multiple class implemented this handler to add something to it and work similarly.

AuthorizationHandler<TRequirement>:IAuthorizationHandler
This is Base class for authorization handlers that need to be called for a specific requirement type. It has two methods which requires to implement one:
* HandleAsync(AuthorizationHandlerContext);
* HandleRequirementAsync(AuthorizationHandlerContext, TRequirement): Makes a decision if authorization is allowed based on a specific requirement.
Most of authorization handlers in Asp.Net framwork implement this class including
* RolesAuthorizationRequirement
* ClaimsAuthorizationRequirement
* NameAuthorizationRequirement
* DenyAnynomousAuthorizationRequirement

AuthorizationHandler<TRequirement,TResource>:IAuthorizationHandler
This is Base class for authorization handlers that need to be called for specific requirement and resource types. It has the following methods:
* HandleAsync(AuthorizationHandlerContext);
* HandleRequirementAsync(AuthorizationHandlerContext, TRequirement, TResource)

AssertionRequirement:IAuthorizationHandler, IAuthorizationRequirement
This classed Implements an IAuthorizationHandler and IAuthorizationRequirement that takes a user specified assertion and used by policyBuilder.RequireAssert to create an instance of Authorization Requirement and Handler. It has two constructor used for sync and async requirement assertion as below:
* AssertionRequirement(Func<AuthorizationHandlerContext,Boolean>)
* AssertionRequirement(Func<AuthorizationHandlerContext,Task<bool>>)
It has one property named Handler as below:
Func<AuthorizationHandlerContext,Task<bool>> Handler ( a delegate which set a handler by constructor and invoke by HandleAsync method)
And it has a method that do invoking handler:
HandleAsync (AuthorizationHandlerContext context): Calls Handler to see if authorization is allowed.
As you can see in this type its simulatenously both handler and requirement. This is not the only case for this pattern, all the policy builder methods implement the same pattern (For example RolesAuthorizationRequirement which is the used in policyBuilder.RequireRole which  Adds a RolesAuthorizationRequirement to the current instance of policy which enforces that the current user must have at least one of the specified roles.)

AuthorizationHandlerContext
This class Contains authorization information (user as claim principal, requirements as enumerable and resouce as an object) used by IAuthorizationHandler and used by HandleAsync and HandleRequirementAsync methods. It has following Properties:
* ClaimsPrincipal User
* IEnumerable<IAuthorizationRequirement> Requirements
* object? Resource
* IEnumerable<IAuthorizationRequirement> PendingRequirements: Gets the requirements that have not yet been marked as succeeded.
* bool HasSucceeded: Flag indicating whether the current authorization processing has succeeded.
* bool HasFailed: Flag indicating whether the current authorization processing has failed.
* IEnumerable<AuthorizationFailureReason> FailureReasons
And it has following methods (which are used to make current check success or fail):
* Fail(AuthorizationFailureReason): Called to indicate HasSucceeded will never return true, even if all requirements are met. A handler doesn't need to handle failures generally, as other handlers for the same requirement may succeed. So use it carefully!
* Succeed(IAuthorizationRequirement): Called to mark the specified requirement as being successfully evaluated.

PolicyBuilder
This class Used for building policies. It has following methods:
* RequireUserName(String): Adds a NameAuthorizationRequirement to the current instance which enforces that the current user matches the specified name.
* RequireRole(String[]), RequireRole(IEnumerable<String>): Adds a RolesAuthorizationRequirement to the current instance which enforces that the current user must have at least one of the specified roles.
* RequireClaim(String),RequireClaim(String, IEnumerable<String>),RequireClaim(String, String[]): Adds a ClaimsAuthorizationRequirement to the current instance which requires that the current user has the specified claim and/Or that the claim value must be one of the allowed values.
* RequireAssertion(Func<AuthorizationHandlerContext,Boolean>),RequireAssertion(Func<AuthorizationHandlerContext,Task<Boolean>>): Adds an AssertionRequirement to the current instance. That is used for building a handler class simply by writing a function. 
* RequireAuthenticatedUser(): Adds DenyAnonymousAuthorizationRequirement to the current instance which enforces that the current user is authenticated.
* AddRequirements(IAuthorizationRequirement[]): Adds the specified requirements to the Requirements for this instance.
* AddAuthenticationSchemes(String[]): Adds the specified authentication schemes to the AuthenticationSchemes for this instance.
And it has 2 property for requirements and authentication schemes:
* IList<IAuthorizationRequirement> Requirements: Gets or sets a list of IAuthorizationRequirements which must succeed for this policy to be successful.
* IList<String> AuthenticationSchemes: Gets or sets a list authentication schemes the Requirements are evaluated against. When not specified, the requirements are evaluated against default schemes. 

