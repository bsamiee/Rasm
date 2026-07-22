# [RASM_MATERIALS_API_VIVIDORANGE_SERIALIZATION]

`VividOrange.Serialization` owns the polymorphic JSON round-trip for the VividOrange taxonomy: one generic `ToJson`/`FromJson` pair constrained to the `ITaxonomySerializable` marker preserves every data type's concrete runtime type through Newtonsoft.Json `$type` tags and its `UnitsNet` quantities through the transitive `UnitsNet.Serialization.JsonNet` converter stack. Every round-trip stays inside the C# layer for persist and clone, while the Materials Thinktecture System.Text.Json owner holds the cross-language wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Serialization`
- package: `VividOrange.Serialization` (MIT, Vivid Orange)
- assembly: `VividOrange.Serialization`
- namespace: `VividOrange.Serialization`
- asset: managed AnyCPU runtime library, no native asset; the consumer `net10.0` binds `lib/net8.0`
- depends: `Newtonsoft.Json`, `UnitsNet.Serialization.JsonNet`, and the `ITaxonomySerializable` marker floor `VividOrange.ISerialization`
- rail: serialization — the in-C# VividOrange-taxonomy JSON wire

[PACKAGE_SURFACE]: `UnitsNet.Serialization.JsonNet`
- package: `UnitsNet.Serialization.JsonNet` (MIT-0, Andreas Gullberg Larsen)
- assembly: `UnitsNet.Serialization.JsonNet`
- namespace: `UnitsNet.Serialization.JsonNet`
- asset: managed runtime library; the consumer `net10.0` binds `netstandard2.0`
- rail: quantity converter — the Json.NET side of the `IQuantity` SI-scalar+unit wire the taxonomy settings register

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the VividOrange round-trip surface

`JsonSerializationExtensions` is the sole consumer-callable type; `TaxonomyJsonSerializer` is `internal` and owns the default wire, customized only by passing a consumer-owned `JsonSerializerSettings`.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :---------------------------- | :------------ | :------------------------------------- |
|  [01]   | `JsonSerializationExtensions` | class         | the `ToJson`/`FromJson` extension pair |
|  [02]   | `TaxonomyJsonSerializer`      | class         | internal default-wire settings owner   |

[PUBLIC_TYPE_SCOPE]: the UnitsNet Json.NET quantity converters

`UnitsNetBaseJsonConverter<T>` roots the converter family over the `ValueUnit`/`ExtendedValueUnit` DTO model and the `RegisterCustomType` registry.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `UnitsNetBaseJsonConverter<T>`     | class         | DTO base, decimal-precision path, type registry |
|  [02]   | `AbbreviatedUnitsConverter`        | class         | compact `"value unit"` form, cache-resolved     |
|  [03]   | `UnitsNetIQuantityJsonConverter`   | class         | object `{ Value, Unit }` read and write         |
|  [04]   | `UnitsNetIComparableJsonConverter` | class         | read-only `IComparable` object form             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: round-trip a taxonomy object and inject a deterministic quantity wire

Both extension members constrain `T` with `ITaxonomySerializable` and fall back to `TaxonomyJsonSerializer.Settings` when `settings` is `null`.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------------------------- |
|  [01]   | `ToJson<T>(JsonSerializerSettings?) -> string`                | static   | serialize to indented `$type`-tagged JSON          |
|  [02]   | `FromJson<T>(JsonSerializerSettings?) -> T?`                  | static   | reconstruct the concrete type; `null` on malformed |
|  [03]   | `UnitsNetBaseJsonConverter<T>.RegisterCustomType(Type, Type)` | instance | register a custom quantity/unit pair               |

- `AbbreviatedUnitsConverter(IDictionary, UnitAbbreviationsCache, IEqualityComparer)`: injects an invariant abbreviations cache through its three-arg ctor for a deterministic quantity wire.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One generic pair constrained `where T: ITaxonomySerializable` is the single polymorphic round-trip rail for the whole taxonomy; no per-type serializer exists.
- `ITaxonomySerializable` is the empty marker declared in `VividOrange.ISerialization`; the floor declares the tag and this package owns the `ToJson`/`FromJson` behavior.
- `TaxonomyJsonSerializer.Settings` binds the default wire: `StringEnumConverter`, `UnitsNetIQuantityJsonConverter`, `TypeNameHandling.Objects` emitting `$type` for concrete reconstruction, and `TypeNameAssemblyFormatHandling.Simple`; `ToJson` writes `Formatting.Indented`.
- `TypeNameHandling.Objects` is a Newtonsoft.Json deserialization-gadget surface, so `FromJson<T>` consumes only trusted VividOrange `ToJson` output; custom settings retain it or polymorphic fields collapse to their declared interface.
- `FromJson<T>` returns `T?`, so malformed JSON yields `null` rather than a typed error; the in-folder seam traps the `null` and any Newtonsoft `JsonException` onto the canonical typed wire-decode rail.
- `AbbreviatedUnitsConverter` resolves the compact `"value unit"` form through a `UnitAbbreviationsCache` that defaults to `UnitAbbreviationsCache.Default` (culture-sensitive); the boundary injects an invariant cache through the three-arg ctor so one JSON byte parses identically across host culture and loaded `UnitsNet` satellites.

[STACKING]:
- `UnitsNet`(`.api/api-unitsnet.md`): every `UnitsNet.IQuantity` field serializes as SI scalar and unit token through `UnitsNetIQuantityJsonConverter`, agreeing with the STJ quantity handling on the canonical wire.
- `VividOrange.InteractionDiagram`(`.api/api-vividorange-iforcemomentinteraction.md`): the capacity-mesh `IForceMomentVertex.X:Force` and `.Y`/`.Z:Torque` coordinates round-trip through the registered quantity converter, so a computed capacity surface persists with its dimensions intact.
- Materials interchange: the `interchange#MATERIAL_WIRE` projection registers `AbbreviatedUnitsConverter` so a quantity crosses to a TS or Python peer as the canonical scalar and unit token, the inverse path decoding it back to a typed `IQuantity`.

[LOCAL_ADMISSION]:
- `ToJson`/`FromJson` persists or clones a VividOrange data object inside the C# layer; the Thinktecture System.Text.Json owner governs the cross-language wire, and the Newtonsoft `$type` shape never leaves the C# boundary.
- Boundary code traps `FromJson<T>` `null` results and Newtonsoft exceptions, then lowers them onto the typed wire-decode rail.
- A non-default wire passes a consumer-owned `JsonSerializerSettings` retaining `TypeNameHandling.Objects` and the UnitsNet converter, never reading or mutating the `internal` `TaxonomyJsonSerializer`.

[RAIL_LAW]:
- Package: `VividOrange.Serialization`
- Owns: the polymorphic JSON round-trip for the VividOrange taxonomy — `JsonSerializationExtensions.ToJson<T>`/`FromJson<T>` over a Newtonsoft.Json and `UnitsNet.Serialization.JsonNet` settings stack — the sole generic round-trip rail for every VividOrange data type.
- Accept: a VividOrange data object round-tripped inside the C# layer preserving concrete type via the `$type` tag and quantities via the SI-scalar+unit converter; a consumer-owned `JsonSerializerSettings` keeping `TypeNameHandling.Objects` and the UnitsNet converter; the `FromJson` `null`-or-throw trapped onto the typed wire-decode rail; the abbreviations cache injected invariant for a deterministic wire.
- Reject: this Newtonsoft `$type` round-trip as the cross-language canonical wire, which the Materials Thinktecture STJ owner holds; an untrusted external document fed to `FromJson<T>` under `TypeNameHandling.Objects`; dropping `TypeNameHandling.Objects` from custom settings; a quantity hand-parsed instead of registered through the converter, serialized as a bare `double` dropping its unit, or the culture-default abbreviated ctor bound on a deterministic boundary.
