# [RASM_GRASSHOPPER_API_GH2_IO]

`GrasshopperIO` is the host-document persistence archive the document owner reads and writes through: `IWriter` and `IReader` expose a typed-primitive surface over every BCL scalar, array, and nested sub-object, `IStorable` is the round-trip contract a domain value implements, and `IoIdAttribute` stamps the IO identity the component registration gate keys on. `DataType` discriminates the stored primitive; `Node` is an in-memory archive node that itself implements `IWriter`/`IReader`. Every member is catalog-verified against the installed RhinoWIP `GrasshopperIO.dll`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `GrasshopperIO`
- package: `GrasshopperIO` (Rhino 9 WIP Grasshopper2 plug-in persistence library; not a NuGet pin — the in-process `GrasshopperIO.dll` under `Grasshopper2Plugin.rhp` is the resolved asset)
- assembly: `GrasshopperIO`
- namespace: `GrasshopperIO` (`IWriter`, `IReader`, `IStorable`, `IoIdAttribute`, `Name`)
- namespace: `GrasshopperIO.DataBase` (`Item`, `Node`, `Value`, `DataType`)
- asset: host assembly; `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/Current/Resources/ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll` resolved from the installed RhinoWIP bundle
- rail: host-grasshopper-io

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive contracts
- namespace: `GrasshopperIO`
- rail: host-grasshopper-io

`IWriter` and `IReader` are the write and read seams; `IStorable` is the round-trip contract a value implements; `IoIdAttribute` carries the IO identity a component type declares.

| [INDEX] | [SYMBOL]         | [KIND]           | [CAPABILITY]                              |
| :-----: | :--------------- | :--------------- | :---------------------------------------- |
|  [01]   | `IWriter`        | interface        | typed-primitive archive write             |
|  [02]   | `IReader`        | interface        | typed-primitive archive read              |
|  [03]   | `IStorable`      | interface        | `Store(IWriter)` round-trip contract      |
|  [04]   | `IoIdAttribute`  | sealed attribute | `Guid` IO identity from a `string` id     |

[PUBLIC_TYPE_SCOPE]: archive data primitives
- namespace: `GrasshopperIO.DataBase`
- rail: host-grasshopper-io

Primitive archive types the reader and writer address: `Name` keys every item, `Item` and `Value` carry a stored scalar, `Node` is a nested archive sub-object, and `DataType` discriminates the stored kind.

| [INDEX] | [SYMBOL]   | [KIND]        | [CAPABILITY]                                       |
| :-----: | :--------- | :------------ | :------------------------------------------------- |
|  [01]   | `Name`     | class         | comparable, equatable item key                     |
|  [02]   | `Item`     | sealed class  | one addressed archive entry                        |
|  [03]   | `Value`    | sealed class  | a stored scalar value                              |
|  [04]   | `Node`     | sealed class  | nested archive node (`IWriter`/`IReader`/`ISml`)   |
|  [05]   | `DataType` | enum          | stored-primitive discriminant                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IWriter typed write
- namespace: `GrasshopperIO`
- rail: host-grasshopper-io

Every write member keys on a `Name`; the scalar family covers each BCL primitive, `Storable` writes a nested value, and `CreateWriter`/`AddItem`/`AddNode` grow the archive tree.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]              | [CAPABILITY]                |
| :-----: | :------------------------------------- | :----------------------- | :-------------------------- |
|  [01]   | `Boolean` · `Integer8` · `Integer32` · `Integer64` | `(Name, value)` → `void` | boolean and integral writes |
|  [02]   | `IntegerXx` · `Number32` · `Number64`  | `(Name, value)` → `void` | big-integer and real writes |
|  [03]   | `Decimal128` · `Complex128`            | `(Name, value)` → `void` | decimal and complex writes  |
|  [04]   | `DateTime` · `TimeSpan` · `Guid128`    | `(Name, value)` → `void` | temporal and guid writes    |
|  [05]   | `String` · `Version` · `Type`          | `(Name, value)` → `void` | text and metadata writes    |
|  [06]   | `FilePath`                             | `(Name, FileSystemPath)` | path write                  |
|  [07]   | `Storable`                             | `(Name, IStorable)`      | nested storable write       |
|  [08]   | `CreateWriter` · `AddItem` · `AddNode` | `(Name)`/`(Item)`/`(Node)` | grow the archive tree     |

[ENTRYPOINT_SCOPE]: IReader typed read
- namespace: `GrasshopperIO`
- rail: host-grasshopper-io

Read members mirror the write scalar family, `FindReader` descends a sub-object, the `Has*` family probes presence, and the `Storable`/array forms round-trip typed values.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]                    | [CAPABILITY]                    |
| :-----: | :------------------------------------------------- | :------------------------------ | :------------------------------ |
|  [01]   | `Boolean` · `Integer32` · `Number64` · `Guid128` · `String` | `(Name)` → `value`     | typed scalar reads              |
|  [02]   | `HasItem` · `HasNode` · `HasItemOrNode`            | `(Name[, DataType])` → `bool`   | presence probe                  |
|  [03]   | `FindReader` · `FindItem`                          | `(Name)` → `IReader` / `Value`  | sub-object and item lookup      |
|  [04]   | `Storable` · `Storable<T>` · `StorableArray<T>`    | `(Name)` → `IStorable`/`T`/`T[]` | round-trip storable read       |
|  [05]   | `BooleanArray` · `Integer32Array` · `Number64Array` · `StringArray` | `(Name)` → `value[]` | typed array reads         |
|  [06]   | `SupportsArray<T>`                                 | `()` → `bool`                   | array-capability probe          |

[ENTRYPOINT_SCOPE]: storable round-trip and IO identity
- namespace: `GrasshopperIO`
- rail: host-grasshopper-io

`IStorable.Store` is the one member a value implements to serialize itself; `IoIdAttribute` mints a `Guid` identity from a `string` id the registration gate reads through `Attribute.IsDefined`.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]              | [CAPABILITY]                    |
| :-----: | :--------------------------- | :----------------------- | :------------------------------ |
|  [01]   | `IStorable.Store`            | `(IWriter)` → `void`     | serialize self into a writer    |
|  [02]   | `new IoIdAttribute`          | `(string id)`            | mint the IO identity            |
|  [03]   | `IoIdAttribute.Id`           | `Guid` property          | parsed identity read            |

## [04]-[IMPLEMENTATION_LAW]

[GH2_IO_TOPOLOGY]:
- `IWriter` and `IReader` are the symmetric write and read seams over one archive; every access keys on a `Name`, and a nested sub-object descends through `CreateWriter`/`FindReader`
- a domain value crosses the archive only as an `IStorable`: `Store(IWriter)` writes it and `IReader.Storable<T>` reconstructs it, so `Document/document.md`'s `StashCase(ValueShelf, string, IStorable)` and `StoreCase(IWriter, FileContents)` transport values through this one contract
- `IoIdAttribute` is the component-type IO identity; `Components/component.md`'s registration gate reads it through `Attribute.IsDefined(type, typeof(IoIdAttribute))`, failing registration when the stamp is absent
- `DataType` discriminates the stored primitive, and the `Has*` probe family gates a read against a missing or wrong-typed item before the scalar accessor runs

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a read that may miss lowers through `HasItem`/`FindItem` into `Option<Value>`; a `Storable<T>` round-trip folds to `Fin<T>` where a malformed archive maps to `Error`; the write side sequences through `Op.Side`/`Eff`, and a `StorableArray<T>` carries as a `Seq<T>`
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `DataType` is owned as a `[SmartEnum]` discriminant so a stored-kind branch dispatches through exhaustive `Switch`; a folder domain value object stores through `IStorable.Store` as its `[ValueObject<T>]` key and reads back through `Storable<T>`

[LOCAL_ADMISSION]:
- persistence enters through `IWriter`/`IReader` at `Document/document.md`'s `DocumentGate.StoreCase` and `StashCase`; a value crosses only as an `IStorable`, never as a raw scalar the folder re-serializes
- component IO identity is `IoIdAttribute`; the registration gate reads it once and a component without the stamp is not admitted
- typed-primitive members are the whole write and read surface; a hand-rolled serializer over `Value`/`Item` is the deleted form

[RAIL_LAW]:
- Package: `GrasshopperIO` (host-document persistence)
- Owns: the `IWriter`/`IReader` typed-primitive archive, the `IStorable` round-trip, the `GrasshopperIO.DataBase` `Name`/`Item`/`Node`/`Value`/`DataType` primitives, and the `IoIdAttribute` type identity
- Accept: typed document read and write, storable round-trip, archive-node traversal, and IO-id declaration
- Reject: document lifecycle and graph mutation (`api-gh2-document`), component declaration and pin typing (`api-gh2-components`)
