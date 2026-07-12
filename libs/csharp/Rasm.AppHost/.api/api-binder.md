# [RASM_APPHOST_API_BINDER]

`Microsoft.Extensions.Configuration.Binder` supplies typed binding, scalar conversion, object-graph construction, binder options, and source-generated binding assets for AppHost policy values.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.Binder`

- package: `Microsoft.Extensions.Configuration.Binder`
- assembly: `Microsoft.Extensions.Configuration.Binder`
- namespace: `Microsoft.Extensions.Configuration`
- asset: runtime library
- rail: configuration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binding family

- rail: configuration

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :---------------------------- | :-------------- | :--------------------- |
|  [01]   | `ConfigurationBinder`         | binding facade  | binds configuration    |
|  [02]   | `BinderOptions`               | policy object   | carries binding policy |
|  [03]   | `BindNonPublicProperties`     | option property | widens member binding  |
|  [04]   | `ErrorOnUnknownConfiguration` | option property | fails unknown keys     |

[GENERATOR_ASSETS]:

- analyzer: `Microsoft.Extensions.Configuration.Binder.SourceGeneration.dll`
- target: `Microsoft.Extensions.Configuration.Binder.targets`
- switch: `EnableConfigurationBindingGenerator`
- rail: source-generated configuration binding

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binding operations

- rail: configuration

| [INDEX] | [SURFACE]              | [CALL_SHAPE]    | [CAPABILITY]          |
| :-----: | :--------------------- | :-------------- | :-------------------- |
|  [01]   | `Get<T>`               | allocation bind | creates typed value   |
|  [02]   | `Get(Type)`            | allocation bind | creates typed value   |
|  [03]   | `Bind`                 | object bind     | fills existing object |
|  [04]   | `Bind(string, object)` | section bind    | fills named section   |
|  [05]   | `GetValue<T>`          | scalar read     | converts scalar value |
|  [06]   | `GetValue(Type, key)`  | scalar read     | converts scalar value |

## [04]-[IMPLEMENTATION_LAW]

[BINDING_TOPOLOGY]:

- namespaces: `Microsoft.Extensions.Configuration`
- option knobs: `BindNonPublicProperties`, `ErrorOnUnknownConfiguration`
- value shapes: scalar, nullable, enum, array, collection, dictionary, object graph, constructor-bound object
- binding modes: allocate typed instance, bind existing instance, read scalar value
- trim boundary: reflection binding is policy material; generated binding is preferred where available

[GENERATOR_TOPOLOGY]:

- package asset: `Microsoft.Extensions.Configuration.Binder.SourceGeneration.dll`
- build switch: `EnableConfigurationBindingGenerator`
- generated rail: binder calls are intercepted into source-generated binding when enabled
- unsupported rail: dynamic or reflection-only shapes stay rejected for runtime policy records

[LOCAL_ADMISSION]:

- Policy records bind once at bootstrap and validate before runtime start.
- Unknown configuration keys fail closed when the policy surface is closed.
- Non-public property binding stays rejected unless a package-owned policy type explicitly owns that shape.
- Source-generated binding is the admitted path for trim-sensitive or AOT-sensitive policy records.

[RAIL_LAW]:

- Package: `Microsoft.Extensions.Configuration.Binder`
- Owns: typed policy binding
- Accept: bound values validate before runtime start
- Reject: late untyped config reads
