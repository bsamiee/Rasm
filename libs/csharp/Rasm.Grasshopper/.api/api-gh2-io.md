# [RASM_GRASSHOPPER_API_GH2_IO]

`GrasshopperIO` is the host-document persistence archive: `IWriter` and `IReader` are the symmetric typed-primitive write and read seams over every BCL scalar, array, and nested sub-object, `IStorable` is the round-trip contract a domain value implements, and `IoIdAttribute` stamps the IO identity the component-registration gate keys on. `DataType` discriminates the stored primitive, and the `Has*` probe family gates a read before its scalar accessor runs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GrasshopperIO` archive
- host: `GrasshopperIO.dll` inside `Grasshopper2Plugin.rhp`, loaded in-process by Rhino 9 WIP
- namespace: `GrasshopperIO`, `GrasshopperIO.DataBase`
- rail: host-grasshopper-io

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive contracts

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------- | :------------ | :------------------------------------ |
|  [01]   | `IWriter`       | interface     | typed-primitive archive write         |
|  [02]   | `IReader`       | interface     | typed-primitive archive read          |
|  [03]   | `IStorable`     | interface     | `Store(IWriter)` round-trip contract  |
|  [04]   | `IoIdAttribute` | attribute     | `Guid` IO identity from a `string` id |

[PUBLIC_TYPE_SCOPE]: archive data primitives

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :--------- | :------------ | :----------------------------------------------- |
|  [01]   | `Name`     | class         | comparable, equatable item key                   |
|  [02]   | `Item`     | class         | one addressed archive entry                      |
|  [03]   | `Value`    | class         | a stored scalar value                            |
|  [04]   | `Node`     | class         | nested archive node (`IWriter`/`IReader`/`ISml`) |
|  [05]   | `DataType` | enum          | stored-primitive discriminant                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IWriter typed write

| [INDEX] | [SURFACE]                                          | [SHAPE]                    | [CAPABILITY]                |
| :-----: | :------------------------------------------------- | :------------------------- | :-------------------------- |
|  [01]   | `Boolean` · `Integer8` · `Integer32` · `Integer64` | `(Name, value)` → `void`   | boolean and integral writes |
|  [02]   | `IntegerXx` · `Number32` · `Number64`              | `(Name, value)` → `void`   | big-integer and real writes |
|  [03]   | `Decimal128` · `Complex128`                        | `(Name, value)` → `void`   | decimal and complex writes  |
|  [04]   | `DateTime` · `TimeSpan` · `Guid128`                | `(Name, value)` → `void`   | temporal and guid writes    |
|  [05]   | `String` · `Version` · `Type`                      | `(Name, value)` → `void`   | text and metadata writes    |
|  [06]   | `FilePath`                                         | `(Name, FileSystemPath)`   | path write                  |
|  [07]   | `Storable`                                         | `(Name, IStorable)`        | nested storable write       |
|  [08]   | `CreateWriter` · `AddItem` · `AddNode`             | `(Name)`/`(Item)`/`(Node)` | grow the archive tree       |

[ENTRYPOINT_SCOPE]: IReader typed read

| [INDEX] | [SURFACE]                                                           | [SHAPE]                          | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------ | :------------------------------- | :------------------------- |
|  [01]   | `Boolean` · `Integer32` · `Number64` · `Guid128` · `String`         | `(Name)` → `value`               | typed scalar reads         |
|  [02]   | `HasItem` · `HasNode` · `HasItemOrNode`                             | `(Name[, DataType])` → `bool`    | presence probe             |
|  [03]   | `FindReader` · `FindItem`                                           | `(Name)` → `IReader` / `Value`   | sub-object and item lookup |
|  [04]   | `Storable` · `Storable<T>` · `StorableArray<T>`                     | `(Name)` → `IStorable`/`T`/`T[]` | round-trip storable read   |
|  [05]   | `BooleanArray` · `Integer32Array` · `Number64Array` · `StringArray` | `(Name)` → `value[]`             | typed array reads          |
|  [06]   | `SupportsArray<T>`                                                  | `()` → `bool`                    | array-capability probe     |

[ENTRYPOINT_SCOPE]: storable round-trip and IO identity

| [INDEX] | [SURFACE]           | [SHAPE]              | [CAPABILITY]                 |
| :-----: | :------------------ | :------------------- | :--------------------------- |
|  [01]   | `IStorable.Store`   | `(IWriter)` → `void` | serialize self into a writer |
|  [02]   | `new IoIdAttribute` | `(string id)`        | mint the IO identity         |
|  [03]   | `IoIdAttribute.Id`  | `Guid` property      | parsed identity read         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IWriter` and `IReader` are symmetric write and read seams over one archive; every access keys on a `Name`, and a nested sub-object descends through `CreateWriter`/`FindReader`.
- A domain value crosses the archive only as an `IStorable`: `Store(IWriter)` writes it and `IReader.Storable<T>` reconstructs it.
- `IoIdAttribute` is the component-type IO identity, read through `Attribute.IsDefined(type, typeof(IoIdAttribute))`; a type without the stamp fails registration.
- `DataType` discriminates the stored primitive, and the `Has*` probe family gates a read against a missing or wrong-typed item before the scalar accessor runs.

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a read that may miss lowers through `HasItem`/`FindItem` into `Option<Value>`; a `Storable<T>` round-trip folds to `Fin<T>` mapping a malformed archive to `Error`; the write side sequences through `Op.Side`/`Eff`, and a `StorableArray<T>` carries as `Seq<T>`.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `DataType` is owned as a `[SmartEnum]` discriminant so a stored-kind branch dispatches through exhaustive `Switch`; a folder value object stores through `IStorable.Store` as its `[ValueObject<T>]` key and reads back through `Storable<T>`.

[LOCAL_ADMISSION]:
- Persistence enters through `IWriter`/`IReader`; a value crosses only as an `IStorable`, never as a raw scalar the folder re-serializes.
- Component IO identity is `IoIdAttribute`; a type without the stamp is not admitted.
- Typed-primitive members are the whole write and read surface; a hand-rolled serializer over `Value`/`Item` is the deleted form.

[RAIL_LAW]:
- Package: `GrasshopperIO` (host-document persistence)
- Owns: the `IWriter`/`IReader` typed-primitive archive, the `IStorable` round-trip, the `GrasshopperIO.DataBase` `Name`/`Item`/`Node`/`Value`/`DataType` primitives, and the `IoIdAttribute` type identity
- Accept: typed document read and write, storable round-trip, archive-node traversal, and IO-id declaration
- Reject: document lifecycle and graph mutation (`api-gh2-document`), component declaration and pin typing (`api-gh2-components`)
