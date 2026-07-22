# [RASM_APPHOST_API_AUTHORIZATION]

`Microsoft.AspNetCore.Authorization` owns the host-neutral ABAC evaluation core: an injected `IAuthorizationService` folds a `ClaimsPrincipal`, an optional resource, and a requirement set through registered handlers into an `AuthorizationResult`. `AddAuthorizationCore` registers that core standalone — no `HttpContext`, no ASP.NET pipeline — serving the AppHost agent/policy rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.Authorization`
- package: `Microsoft.AspNetCore.Authorization`
- assembly: `Microsoft.AspNetCore.Authorization`
- namespace: `Microsoft.AspNetCore.Authorization`, `Microsoft.AspNetCore.Authorization.Infrastructure`, `Microsoft.Extensions.DependencyInjection`
- rail: authorization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: evaluation and result family

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :---------------------------- | :------------ | :--------------------------- |
|  [01]   | `IAuthorizationService`       | interface     | requirement-set evaluation   |
|  [02]   | `AuthorizationResult`         | class         | succeeded-or-failure outcome |
|  [03]   | `AuthorizationFailure`        | class         | explicit reasoned failure    |
|  [04]   | `AuthorizationFailureReason`  | class         | handler-attributed message   |
|  [05]   | `AuthorizationHandlerContext` | class         | pending-succeed-fail ledger  |
|  [06]   | `AuthorizationOptions`        | class         | named-policy registration    |

[PUBLIC_TYPE_SCOPE]: handler and requirement family

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :--------------------------------------------- | :------------ | :------------------------- |
|  [01]   | `IAuthorizationHandler`                        | interface     | requirement evaluation     |
|  [02]   | `AuthorizationHandler<TRequirement>`           | class         | single-requirement handler |
|  [03]   | `AuthorizationHandler<TRequirement,TResource>` | class         | resource-typed handler     |
|  [04]   | `IAuthorizationRequirement`                    | interface     | handler dispatch key       |
|  [05]   | `ClaimsAuthorizationRequirement`               | class         | claim-type-value match     |
|  [06]   | `RolesAuthorizationRequirement`                | class         | role membership            |
|  [07]   | `NameAuthorizationRequirement`                 | class         | user-name match            |
|  [08]   | `DenyAnonymousAuthorizationRequirement`        | class         | authenticated-user gate    |
|  [09]   | `OperationAuthorizationRequirement`            | class         | resource-operation match   |
|  [10]   | `AssertionRequirement`                         | class         | inline predicate           |

[PUBLIC_TYPE_SCOPE]: policy construction and provider family

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :----------------------------- | :------------ | :---------------------------- |
|  [01]   | `AuthorizationPolicy`          | class         | requirement set plus schemes  |
|  [02]   | `AuthorizationPolicyBuilder`   | class         | fluent requirement assembly   |
|  [03]   | `IAuthorizationPolicyProvider` | interface     | named-default-fallback lookup |
|  [04]   | `AuthorizationBuilder`         | class         | named-policy registration     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: evaluation operations

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `AuthorizeAsync(user, resource, requirements)` | instance | requirement-set evaluation     |
|  [02]   | `AuthorizeAsync(user, resource, policyName)`   | instance | named-policy evaluation        |
|  [03]   | `AuthorizeAsync(user, resource, requirement)`  | static   | single-requirement evaluation  |
|  [04]   | `AuthorizeAsync(user, resource, policy)`       | static   | policy-value evaluation        |
|  [05]   | `AuthorizeAsync(user, policy)`                 | static   | resourceless policy evaluation |
|  [06]   | `AuthorizeAsync(user, policyName)`             | static   | resourceless named evaluation  |

[ENTRYPOINT_SCOPE]: context and result operations

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                |
| :-----: | :---------------------------------------- | :------- | :-------------------------- |
|  [01]   | `HandleAsync(context)`                    | instance | requirement evaluation pass |
|  [02]   | `HandleRequirementAsync(context, req)`    | instance | typed requirement decision  |
|  [03]   | `Context.Succeed(requirement)`            | instance | mark requirement satisfied  |
|  [04]   | `Context.Fail(reason)`                    | instance | latch reasoned or bare fail |
|  [05]   | `Context.PendingRequirements`             | property | unsatisfied requirement set |
|  [06]   | `Context.HasSucceeded`                    | property | aggregate success state     |
|  [07]   | `Context.HasFailed`                       | property | aggregate failure state     |
|  [08]   | `AuthorizationResult.Success()`           | factory  | successful result           |
|  [09]   | `AuthorizationResult.Failed(failure)`     | factory  | failed-result carrier       |
|  [10]   | `AuthorizationFailure.FailedRequirements` | property | unmet requirement set       |
|  [11]   | `AuthorizationFailure.FailureReasons`     | property | attributed failure reasons  |

[ENTRYPOINT_SCOPE]: policy construction and registration

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :-------------------------------------- | :------- | :----------------------------- |
|  [01]   | `RequireClaim(type, values)`            | instance | claim requirement              |
|  [02]   | `RequireRole(roles)`                    | instance | role requirement               |
|  [03]   | `RequireUserName(name)`                 | instance | name requirement               |
|  [04]   | `RequireAuthenticatedUser()`            | instance | deny-anonymous requirement     |
|  [05]   | `RequireAssertion(handler)`             | instance | inline predicate requirement   |
|  [06]   | `AddRequirements(requirements)`         | instance | explicit requirement admission |
|  [07]   | `Combine(policy)`                       | instance | fold an existing policy in     |
|  [08]   | `Build()`                               | instance | assemble the policy value      |
|  [09]   | `AuthorizationPolicy.Combine(policies)` | static   | merged policy value            |
|  [10]   | `GetPolicyAsync(name)`                  | instance | named-policy resolution        |
|  [11]   | `GetDefaultPolicyAsync()`               | instance | default-policy resolution      |
|  [12]   | `GetFallbackPolicyAsync()`              | instance | fallback-policy resolution     |
|  [13]   | `AddAuthorizationCore()`                | static   | standalone evaluation core     |
|  [14]   | `AddAuthorizationCore(configure)`       | static   | options-configured core        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IAuthorizationService.AuthorizeAsync` accepts a `ClaimsPrincipal`, an optional `object?` resource, and either an `IEnumerable<IAuthorizationRequirement>` or a `string` policy name.
- `AuthorizationResult` carries `bool Succeeded` and a nullable `AuthorizationFailure?`; `[MemberNotNullWhen(false, "Failure")]` makes `Failure` non-null exactly when `Succeeded` is `false`.
- `AuthorizationFailure` distinguishes `ExplicitFail()`, `Failed(reasons)`, and `Failed(failedRequirements)`, exposing `FailedRequirements` (`IEnumerable<IAuthorizationRequirement>`) and `FailureReasons` (`IEnumerable<AuthorizationFailureReason>`); each `AuthorizationFailureReason` attributes a `Message` to its originating `IAuthorizationHandler`.
- `AuthorizationHandlerContext.HasSucceeded` holds only when `Succeed` was called, `Fail` was not, and no requirement remains pending; `Succeed(requirement)` drops from the pending set and `Fail` latches failure.
- Each built-in requirement is both the `IAuthorizationRequirement` and its own `IAuthorizationHandler`; `IAuthorizationRequirement` is an empty marker.
- `AuthorizationPolicy` is an immutable value of `IReadOnlyList<IAuthorizationRequirement> Requirements` and `IReadOnlyList<string> AuthenticationSchemes`; an empty requirement set throws `InvalidOperationException`, and `IAuthorizationPolicyProvider.AllowsCachingPolicies` defaults to `false`.
- `AddAuthorizationCore` registers the evaluation core — service, policy provider, handler provider, evaluator, and context factory — as transient services with no HTTP dependency.

[STACKING]:
- `api-di`(`.api/api-di.md`): `AddAuthorizationCore` mints the evaluation-core `ServiceDescriptor` set onto the `IServiceCollection`, and both `IAuthorizationService` and every custom `IAuthorizationHandler` resolve through `GetRequiredService`.
- `api-identitymodel-jwt`(`.api/api-identitymodel-jwt.md`): a validated `TokenValidationResult.ClaimsIdentity` becomes the `ClaimsPrincipal` argument to `AuthorizeAsync`, joining the token-validation leg to the access decision.
- within-lib: AppHost's agent/policy rail injects `IAuthorizationService`, evaluates a domain resource object under an `OperationAuthorizationRequirement` and a resource-typed `AuthorizationHandler<TRequirement, TResource>`, and projects `AuthorizationFailure.FailureReasons` onto the host failure rail.

[LOCAL_ADMISSION]:
- AppHost binds the evaluation core through `AddAuthorizationCore`; the HTTP-coupled `AddAuthorization` and middleware surface stay out of the host.
- A custom requirement implements `IAuthorizationRequirement` and pairs with an `AuthorizationHandler<TRequirement>` (or the resource-typed arity) registered as `IAuthorizationHandler`.
- Decisions read `AuthorizationResult.Succeeded` and project `AuthorizationFailure.FailureReasons`; the typed result flows through handler callbacks that never throw.
- AppHost's resource-bound rail binds explicit `AuthorizationPolicy` values and `OperationAuthorizationRequirement` ahead of string policy names, which register through `AuthorizationOptions`/`AuthorizationBuilder`.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.Authorization`
- Owns: host-neutral ABAC requirement and policy evaluation
- Accept: `IAuthorizationService` over claims, resources, and registered handlers
- Reject: HTTP-pipeline authorization middleware or hand-rolled claim/role checks
