# [RASM_APPUI_API_GH2_IO]

`GrasshopperIO` supplies GH2 archive, node, item, value, reader, writer, path, identity, and storage surfaces for host-aware UI state exchange.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `GrasshopperIO`
- package: `GrasshopperIO`
- assembly: `GrasshopperIO`
- namespace: `GrasshopperIO`
- namespace: `GrasshopperIO.DataBase`
- asset: host assembly
- rail: host-gh2

## [2]-[PUBLIC_TYPES]

[ARCHIVE_TYPES]: archive, identity, and path surface
- rail: host-gh2

| [INDEX] | [SYMBOL]         | [RAIL]             |
| :-----: | :--------------- | :----------------- |
|   [1]   | `Archive`        | archive root       |
|   [2]   | `IO`             | IO coordinator     |
|   [3]   | `Name`           | archive name       |
|   [4]   | `Path`           | archive path       |
|   [5]   | `FileSystemPath` | file-system path   |
|   [6]   | `IoIdAttribute`  | identity attribute |
|   [7]   | `ICustomIoId`    | identity contract  |

[NODE_TYPES]: node, item, value, and storage surface
- rail: host-gh2

| [INDEX] | [SYMBOL]                 | [RAIL]           |
| :-----: | :----------------------- | :--------------- |
|   [1]   | `Node`                   | archive node     |
|   [2]   | `Item`                   | archive item     |
|   [3]   | `Value`                  | typed value      |
|   [4]   | `DataType`               | value type       |
|   [5]   | `NamedList<T>`           | named collection |
|   [6]   | `SimpleImmutableList<T>` | immutable list   |
|   [7]   | `SharedStorage`          | shared state     |
|   [8]   | `NodeMatcher`            | node comparison  |

[CONTRACT_TYPES]: read/write contracts
- rail: host-gh2

| [INDEX] | [SYMBOL]       | [RAIL]           |
| :-----: | :------------- | :--------------- |
|   [1]   | `IReader`      | reader contract  |
|   [2]   | `IWriter`      | writer contract  |
|   [3]   | `IStorable`    | storage contract |
|   [4]   | `IBinary`      | binary contract  |
|   [5]   | `ISml`         | SML contract     |
|   [6]   | `ReadContext`  | read context     |
|   [7]   | `WriteContext` | write context    |
|   [8]   | `SmlWriter`    | SML writer       |

## [3]-[ENTRYPOINTS]

[NODE_ENTRYPOINTS]: archive node operations
- rail: host-gh2

| [INDEX] | [SURFACE]     | [SURFACE_ROOT] | [RAIL]       |
| :-----: | :------------ | :------------- | :----------- |
|   [1]   | `Create`      | `Node`         | node create  |
|   [2]   | `FindItem`    | `Node`         | item lookup  |
|   [3]   | `AddItem`     | `Node`         | item add     |
|   [4]   | `ReplaceItem` | `Node`         | item replace |
|   [5]   | `RemoveItem`  | `Node`         | item remove  |
|   [6]   | `FindNode`    | `Node`         | node lookup  |
|   [7]   | `EmitNode`    | `Node`         | node create  |
|   [8]   | `Select`      | `Node`         | path select  |

[READ_WRITE_ENTRYPOINTS]: archive contracts
- rail: host-gh2

| [INDEX] | [SURFACE]         | [SURFACE_ROOT] | [RAIL]       |
| :-----: | :---------------- | :------------- | :----------- |
|   [1]   | `Read`            | `IBinary`      | binary read  |
|   [2]   | `Write`           | `IBinary`      | binary write |
|   [3]   | `Write`           | `ISml`         | SML write    |
|   [4]   | `WriteLine`       | `SmlWriter`    | SML emit     |
|   [5]   | `TryFind`         | `NamedList<T>` | named lookup |
|   [6]   | `Add`             | `NamedList<T>` | named add    |
|   [7]   | `FindDifferences` | `Node`         | node diff    |
|   [8]   | `AreIndentical`   | `Node`         | node compare |

## [4]-[IMPLEMENTATION_LAW]

[ARCHIVE_LAW]:
- Package: `GrasshopperIO`
- Owns: GH2 archive interop, nodes, items, values, read/write contracts, paths, and identity records
- Accept: host persistence stays boundary-owned and projects through typed archive receipts
- Reject: archive fields as UI state

[BOUNDARY_LAW]:
- Package: `GrasshopperIO`
- Owns: GH2 serialization at the host boundary only
- Accept: archive data can seed or receive AppUi state through explicit projection records
- Reject: archive nodes as product UI model
