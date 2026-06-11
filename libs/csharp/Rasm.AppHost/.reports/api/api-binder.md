# [RASM_APPHOST_API_BINDER]

`Microsoft.Extensions.Configuration.Binder` supplies typed binding from configuration sections into AppHost policy values.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.Binder`
- package: `Microsoft.Extensions.Configuration.Binder`
- assembly: `Microsoft.Extensions.Configuration.Binder`
- namespace: `Microsoft.Extensions.Configuration`
- asset: runtime library
- rail: configuration

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binding family
- rail: configuration

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE] | [CAPABILITY]                   |
| :-----: | :-------------------- | :------------- | :----------------------------- |
|   [1]   | `ConfigurationBinder` | binding facade | anchors configuration contract |
|   [2]   | `BinderOptions`       | policy object  | carries policy input           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binding operations
- rail: configuration

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]           | [CAPABILITY]              |
| :-----: | :------------------------- | :--------------------- | :------------------------ |
|   [1]   | `Get<T>`                   | lookup call            | resolves typed value      |
|   [2]   | `Bind`                     | mutation call          | admits configured surface |
|   [3]   | `GetValue<T>`              | lookup call            | resolves typed value      |
|   [4]   | `Configure<BinderOptions>` | configuration delegate | applies policy value      |

## [4]-[IMPLEMENTATION_LAW]

[BINDING_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Configuration`
- option knobs: `BindNonPublicProperties`, `ErrorOnUnknownConfiguration`
- value shapes: scalar, nullable, enum, array, collection, dictionary, object graph
- binding modes: allocate typed instance, bind existing instance, read scalar value
- trim boundary: reflection binding is policy material; generated binding is preferred where available

[LOCAL_ADMISSION]:
- Policy records bind once at bootstrap and validate before runtime start.
- Unknown configuration keys fail closed when the policy surface is closed.
- Non-public property binding stays rejected unless a package-owned policy type explicitly owns that shape.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Configuration.Binder`
- Owns: typed policy binding
- Accept: bound values validate before runtime start
- Reject: late untyped config reads

