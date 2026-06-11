# [RASM_PERSISTENCE_API_MESSAGEPACK_ANALYZER]

`MessagePackAnalyzer` supplies analyzer and generator assets for MessagePack object contracts and formatter seams.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePackAnalyzer`
- package: `MessagePackAnalyzer`
- assembly: analyzer assets
- namespace: Roslyn analyzer assets
- asset: analyzer assets
- rail: snapshot-codec

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: analyzer family
- rail: snapshot-codec

| [INDEX] | [ASSET]                               | [PACKAGE_ROLE]    | [CAPABILITY]           |
| :-----: | :------------------------------------ | :---------------- | :--------------------- |
|   [1]   | `analyzers/roslyn4.3/cs`              | analyzer path     | declares build input   |
|   [2]   | `MessagePack.SourceGenerator.dll`     | generator asset   | emits formatter source |
|   [3]   | `MessagePack.Analyzers.CodeFixes.dll` | analyzer asset    | guards codec contract  |
|   [4]   | diagnostic descriptor set             | analyzer contract | guards codec contract  |
|   [5]   | generated resolver output             | generated output  | selects formatter      |
|   [6]   | generated formatter output            | generated output  | serializes shape       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: analyzer surfaces
- rail: snapshot-codec

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]      | [CAPABILITY]           |
| :-----: | :------------------------- | :---------------- | :--------------------- |
|   [1]   | analyzer assembly          | build asset       | declares build input   |
|   [2]   | source generator assembly  | build asset       | emits formatter source |
|   [3]   | diagnostic categories      | analyzer contract | guards codec contract  |
|   [4]   | generated resolver output  | generated output  | selects formatter      |
|   [5]   | generated formatter output | generated output  | serializes shape       |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `MessagePackAnalyzer`
- Owns: codec analyzer and generator
- Accept: generated formatters enter codec rail
- Reject: runtime analyzer references
