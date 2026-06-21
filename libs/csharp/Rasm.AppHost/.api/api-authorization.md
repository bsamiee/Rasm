# [RASM_APPHOST_API_AUTHORIZATION]

`Microsoft.AspNetCore.Authorization` supplies the host-neutral attribute-based access control core: an injected `IAuthorizationService` that evaluates a `ClaimsPrincipal`, an optional resource, and a set of requirements through registered handlers into an `AuthorizationResult`. The evaluation core runs standalone over `AddAuthorizationCore()` with no `HttpContext` and no ASP.NET pipeline, serving the AppHost agent/policy rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.Authorization`
- package: `Microsoft.AspNetCore.Authorization`
- assembly: `Microsoft.AspNetCore.Authorization`
- namespace: `Microsoft.AspNetCore.Authorization`
- namespace: `Microsoft.AspNetCore.Authorization.Infrastructure`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: authorization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: evaluation and result family
- rail: authorization

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [RAIL]                        |
| :-----: | :---------------------------- | :----------------- | :---------------------------- |
|  [01]   | `IAuthorizationService`       | evaluation service | policy/requirement evaluation |
|  [02]   | `AuthorizationResult`         | result value       | succeeded/failure outcome     |
|  [03]   | `AuthorizationFailure`        | failure value      | explicit/reasoned failure     |
|  [04]   | `AuthorizationFailureReason`  | failure detail     | handler-attributed message    |
|  [05]   | `AuthorizationHandlerContext` | evaluation context | pending/succeed/fail ledger   |
|  [06]   | `AuthorizationOptions`        | options            | named-policy registration     |

[PUBLIC_TYPE_SCOPE]: handler and requirement family
- rail: authorization

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]      | [RAIL]                     |
| :-----: | :--------------------------------------------- | :----------------- | :------------------------- |
|  [01]   | `IAuthorizationHandler`                        | handler contract   | requirement evaluation     |
|  [02]   | `AuthorizationHandler<TRequirement>`           | handler base       | single-requirement handler |
|  [03]   | `AuthorizationHandler<TRequirement,TResource>` | handler base       | resource-typed handler     |
|  [04]   | `IAuthorizationRequirement`                    | requirement marker | handler dispatch key       |
|  [05]   | `ClaimsAuthorizationRequirement`               | requirement        | claim-type/value match     |
|  [06]   | `RolesAuthorizationRequirement`                | requirement        | role membership            |
|  [07]   | `NameAuthorizationRequirement`                 | requirement        | user-name match            |
|  [08]   | `DenyAnonymousAuthorizationRequirement`        | requirement        | authenticated-user gate    |
|  [09]   | `OperationAuthorizationRequirement`            | requirement        | resource-operation match   |
|  [10]   | `AssertionRequirement`                         | requirement        | inline predicate           |

[PUBLIC_TYPE_SCOPE]: policy construction and provider family
- rail: authorization

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [RAIL]                        |
| :-----: | :----------------------------- | :-------------- | :---------------------------- |
|  [01]   | `AuthorizationPolicy`          | policy value    | requirement set plus schemes  |
|  [02]   | `AuthorizationPolicyBuilder`   | policy builder  | fluent requirement assembly   |
|  [03]   | `IAuthorizationPolicyProvider` | policy provider | named/default/fallback lookup |
|  [04]   | `AuthorizationBuilder`         | DI builder      | named-policy registration     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: evaluation operations
- rail: authorization

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]       | [RAIL]                         |
| :-----: | :--------------------------------------------- | :------------------- | :----------------------------- |
|  [01]   | `AuthorizeAsync(user, resource, requirements)` | service evaluation   | requirement-set evaluation     |
|  [02]   | `AuthorizeAsync(user, resource, policyName)`   | service evaluation   | named-policy evaluation        |
|  [03]   | `AuthorizeAsync(user, resource, requirement)`  | extension evaluation | single-requirement evaluation  |
|  [04]   | `AuthorizeAsync(user, resource, policy)`       | extension evaluation | policy-value evaluation        |
|  [05]   | `AuthorizeAsync(user, policy)`                 | extension evaluation | resourceless policy evaluation |
|  [06]   | `AuthorizeAsync(user, policyName)`             | extension evaluation | resourceless named evaluation  |

[ENTRYPOINT_SCOPE]: context and result operations
- rail: authorization

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]   | [RAIL]                      |
| :-----: | :------------------------------------- | :--------------- | :-------------------------- |
|  [01]   | `HandleAsync(context)`                 | handler dispatch | requirement evaluation pass |
|  [02]   | `HandleRequirementAsync(context, req)` | handler override | typed requirement decision  |
|  [03]   | `Context.Succeed(requirement)`         | context ledger   | mark requirement satisfied  |
|  [04]   | `Context.Fail()` / `Fail(reason)`      | context ledger   | explicit/reasoned failure   |
|  [05]   | `Context.PendingRequirements`          | context query    | unsatisfied requirements    |
|  [06]   | `Context.HasSucceeded` / `HasFailed`   | context query    | aggregate evaluation state  |
|  [07]   | `AuthorizationResult.Success()`        | result factory   | succeeded outcome           |
|  [08]   | `AuthorizationResult.Failed(failure)`  | result factory   | failed outcome carrier      |

[ENTRYPOINT_SCOPE]: policy construction and registration
- rail: authorization

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]    | [RAIL]                         |
| :-----: | :------------------------------------------------- | :---------------- | :----------------------------- |
|  [01]   | `RequireClaim(type, allowedValues)`                | builder extension | claim requirement              |
|  [02]   | `RequireRole(roles)`                               | builder extension | role requirement               |
|  [03]   | `RequireUserName(userName)`                        | builder extension | name requirement               |
|  [04]   | `RequireAuthenticatedUser()`                       | builder extension | deny-anonymous requirement     |
|  [05]   | `RequireAssertion(handler)`                        | builder extension | inline predicate requirement   |
|  [06]   | `AddRequirements(requirements)`                    | builder extension | explicit requirement admission |
|  [07]   | `Combine(policy)` / `Build()`                      | builder lifecycle | policy assembly                |
|  [08]   | `AuthorizationPolicy.Combine(policies)`            | policy combinator | merged policy value            |
|  [09]   | `GetPolicyAsync(name)`                             | provider lookup   | named-policy resolution        |
|  [10]   | `GetDefaultPolicyAsync` / `GetFallbackPolicyAsync` | provider lookup   | default/fallback resolution    |
|  [11]   | `AddAuthorizationCore()`                           | DI registration   | standalone evaluation core     |
|  [12]   | `AddAuthorizationCore(configure)`                  | DI registration   | options-configured core        |

## [04]-[IMPLEMENTATION_LAW]

[AUTHORIZATION_TOPOLOGY]:
- namespaces: evaluation core (`Microsoft.AspNetCore.Authorization`), built-in requirements (`Microsoft.AspNetCore.Authorization.Infrastructure`), DI registration (`Microsoft.Extensions.DependencyInjection`)
- evaluation surface: `IAuthorizationService.AuthorizeAsync` accepts a `ClaimsPrincipal`, an optional `object?` resource, and either an `IEnumerable<IAuthorizationRequirement>` or a `string` policy name
- result surface: `AuthorizationResult` carries `bool Succeeded` and a nullable `AuthorizationFailure?`; `[MemberNotNullWhen(false, "Failure")]` makes `Failure` non-null exactly when `Succeeded` is `false`
- failure surface: `AuthorizationFailure` distinguishes `ExplicitFail()`, `Failed(reasons)`, and `Failed(failedRequirements)`; `AuthorizationFailureReason` attributes a `Message` to the originating `IAuthorizationHandler`
- handler surface: `IAuthorizationHandler.HandleAsync(AuthorizationHandlerContext)` is the dispatch contract; `AuthorizationHandler<TRequirement>` and `AuthorizationHandler<TRequirement, TResource>` iterate matching requirements and dispatch to a typed `HandleRequirementAsync` override
- context surface: `AuthorizationHandlerContext` is the evaluation ledger carrying `Requirements`, `User`, `Resource`, `PendingRequirements`, and `FailureReasons`; `Succeed(requirement)` removes from the pending set, `Fail()`/`Fail(reason)` latches failure, and `HasSucceeded` holds only when `Succeed` was called, `Fail` was not, and no requirements remain pending
- requirement surface: `IAuthorizationRequirement` is an empty marker; built-in requirements (`ClaimsAuthorizationRequirement`, `RolesAuthorizationRequirement`, `NameAuthorizationRequirement`, `DenyAnonymousAuthorizationRequirement`, `OperationAuthorizationRequirement`, `AssertionRequirement`) are both the requirement and its self-handler
- policy surface: `AuthorizationPolicy` is an immutable value of `IReadOnlyList<IAuthorizationRequirement> Requirements` and `IReadOnlyList<string> AuthenticationSchemes`; construction with an empty requirement set throws `InvalidOperationException`
- builder surface: `AuthorizationPolicyBuilder` exposes fluent `Require*` methods that each append a requirement and return `this`, terminating in `Build()`
- provider surface: `IAuthorizationPolicyProvider` resolves `GetPolicyAsync(name)`, `GetDefaultPolicyAsync()`, and `GetFallbackPolicyAsync()`, with `AllowsCachingPolicies` defaulting to `false`
- registration surface: `AddAuthorizationCore()` registers `IAuthorizationService` (`DefaultAuthorizationServiceImpl`), `IAuthorizationPolicyProvider`, `IAuthorizationHandlerProvider`, `IAuthorizationEvaluator`, `IAuthorizationHandlerContextFactory`, and the enumerable `PassThroughAuthorizationHandler` as transient services with no HTTP dependency

[LOCAL_ADMISSION]:
- The AppHost agent/policy rail consumes the evaluation core through `AddAuthorizationCore()`; the HTTP-coupled `AddAuthorization()` and middleware surface stay out of the host.
- Authorization is an injected `IAuthorizationService` capability; evaluation passes a `ClaimsPrincipal`, a domain resource object, and requirements with no `HttpContext`.
- Custom requirements implement `IAuthorizationRequirement` and pair with an `AuthorizationHandler<TRequirement>` (or the resource-typed arity) registered as `IAuthorizationHandler`.
- Decisions read `AuthorizationResult.Succeeded` and project `AuthorizationFailure.FailureReasons` into the host failure rail; the boolean and nullable `Failure` flow through typed results without throwing from handler callbacks.
- Named policies register through `AuthorizationOptions`/`AuthorizationBuilder`; the resource-bound rail prefers explicit `AuthorizationPolicy` values and `OperationAuthorizationRequirement` over string policy names.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.Authorization`
- Owns: host-neutral ABAC requirement/policy evaluation
- Accept: `IAuthorizationService` over claims, resources, and registered handlers
- Reject: HTTP-pipeline authorization middleware or hand-rolled claim/role checks
