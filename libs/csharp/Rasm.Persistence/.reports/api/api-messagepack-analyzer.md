# [RASM_PERSISTENCE_API_MESSAGEPACK_ANALYZER]

`MessagePackAnalyzer` supplies analyzer, code-fix, and source-generator assets
for MessagePack contracts, generated formatters, generated resolvers, and
contract diagnostics.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePackAnalyzer`
- package: `MessagePackAnalyzer`
- assembly: analyzer assets
- namespace: analyzer and generator assets
- asset: analyzer package
- rail: snapshot-codec

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: analyzer and generator assets
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]   | [CAPABILITY]             |
| :-----: | :------------------------------------ | :--------------- | :----------------------- |
|   [1]   | `MessagePack.SourceGenerator.dll`     | source generator | emits formatter code     |
|   [2]   | `MessagePack.Analyzers.CodeFixes.dll` | analyzer asset   | reports contract issues  |
|   [3]   | analyzer diagnostics                  | diagnostic asset | classifies contract gaps |
|   [4]   | generated formatter templates         | generator asset  | emits formatter shapes   |
|   [5]   | generated resolver templates          | generator asset  | emits resolver shapes    |

[RECOGNIZED_TYPES]: recognized contract markers
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]        | [CAPABILITY]             |
| :-----: | :-------------------------------------- | :-------------------- | :----------------------- |
|   [1]   | `MessagePackObjectAttribute`            | contract attribute    | marks object contracts   |
|   [2]   | `KeyAttribute`                          | contract attribute    | declares member keys     |
|   [3]   | `IgnoreMemberAttribute`                 | contract attribute    | excludes members         |
|   [4]   | `UnionAttribute`                        | contract attribute    | declares union cases     |
|   [5]   | `MessagePackFormatterAttribute`         | formatter attribute   | selects formatter        |
|   [6]   | `SerializationConstructorAttribute`     | constructor attribute | selects constructor      |
|   [7]   | `GeneratedMessagePackResolverAttribute` | resolver attribute    | marks generated resolver |
|   [8]   | `CompositeResolverAttribute`            | resolver attribute    | declares resolver inputs |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: diagnostics and generated surfaces
- rail: snapshot-codec

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :---------------------------------- | :--------------- | :----------------------- |
|   [1]   | `UseMessagePackObjectAttribute`     | diagnostic       | requires contract marker |
|   [2]   | `MessagePackObjectAllowPrivate`     | diagnostic       | checks private contract  |
|   [3]   | `AttributeMessagePackObjectMembers` | diagnostic       | checks member markers    |
|   [4]   | `CollidingFormatters`               | diagnostic       | checks resolver conflict |
|   [5]   | `InvalidMessagePackObject`          | diagnostic       | checks object contract   |
|   [6]   | `UnionAttributeRequired`            | diagnostic       | checks union contract    |
|   [7]   | generated formatter class           | generated output | formats snapshot values  |
|   [8]   | generated resolver class            | generated output | resolves snapshot values |
|   [9]   | `AddSource`                         | generator call   | emits generated source   |

## [4]-[IMPLEMENTATION_LAW]

[ANALYZER_ADMISSION]:
- dependency role: analyzer-only package
- generator root: formatter and resolver source generation
- diagnostic root: MessagePack contract correctness
- output root: generated formatter and resolver surfaces

[LOCAL_ADMISSION]:
- Analyzer and generator assets enforce snapshot contract shape before runtime use.
- Generated formatters and resolvers belong to snapshot codec implementation folders.
- Analyzer diagnostics block ambiguous, unkeyed, colliding, or invalid MessagePack contracts.
- Analyzer assets stay private and never become runtime dependencies.

[RAIL_LAW]:
- Package: `MessagePackAnalyzer`
- Owns: MessagePack contract analyzer and generator assets
- Accept: generated formatter and resolver proof
- Reject: runtime dependency on analyzer assets
