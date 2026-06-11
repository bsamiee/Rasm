# [RASM_APPUI_API_GH2_IO]

`GrasshopperIO` supplies GH2 serialization and archive surfaces for host-aware UI state exchange.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `GrasshopperIO`
- host_assembly: `GrasshopperIO`
- assembly: `GrasshopperIO`
- namespace: `GrasshopperIO`
- asset: host assembly
- rail: host-gh2

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: GH2 IO family
- rail: host-gh2

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]  | [CAPABILITY]        |
| :-----: | :-------------------- | :-------------- | :------------------ |
|   [1]   | `GH_Archive`          | archive root    | stores GH2 data     |
|   [2]   | `GH_IReader`          | reader contract | reads archive data  |
|   [3]   | `GH_IWriter`          | writer contract | writes archive data |
|   [4]   | `GH_IO.Serialization` | namespace group | groups IO surfaces  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialization operations
- rail: host-gh2

| [INDEX] | [SURFACE]   | [CALL_SHAPE]   | [CAPABILITY]              |
| :-----: | :---------- | :------------- | :------------------------ |
|   [1]   | `Read`      | operation call | executes operation        |
|   [2]   | `Write`     | operation call | executes operation        |
|   [3]   | `SetString` | mutation call  | admits configured surface |
|   [4]   | `GetString` | lookup call    | resolves typed value      |
|   [5]   | `SetInt32`  | mutation call  | admits configured surface |
|   [6]   | `GetInt32`  | lookup call    | resolves typed value      |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `GrasshopperIO`
- Owns: GH2 archive interop
- Accept: host persistence stays boundary-owned
- Reject: archive fields as UI state
