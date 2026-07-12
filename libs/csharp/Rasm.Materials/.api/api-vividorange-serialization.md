# [RASM_MATERIALS_API_VIVIDORANGE_SERIALIZATION]

`VividOrange.Serialization` is verified-transitive through direct package `VividOrange.InteractionDiagram` and owns the polymorphic JSON round-trip behind the `ITaxonomySerializable` marker. Every admitted VividOrange material, section, bar, section-property carrier, standard, catalogued profile, and N-M-M capacity mesh implements the marker. `JsonSerializationExtensions.ToJson<T>` and `FromJson<T>`, constrained by `where T: ITaxonomySerializable`, round-trip the taxonomy through Newtonsoft.Json and `UnitsNet.Serialization.JsonNet`; `TypeNameHandling.Objects` preserves concrete runtime types, and the quantity converter preserves SI scalars with their units. The interface-only `VividOrange.ISerialization` floor declares the empty marker, while this package binds its behavior. `UnitsNet.Serialization.JsonNet` owns the quantity-converter half ([TRANSITIVE_UNITSNET_JSONNET]), and `UnitsNet` owns the quantity types.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Serialization`

- package: `VividOrange.Serialization`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Serialization`
- namespace: `VividOrange.Serialization`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `netstandard2.0` / `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the
  bound TFM — no `net9.0`+ asset, so the `api resolve` primary `net8.0` IS the consumed surface).
- rail: serialization (the VividOrange-taxonomy JSON wire)
- ABI floor: a PRE-1.0 contract — the extension-method surface may break across a minor bump. The `ITaxonomySerializable` marker comes from the centrally pinned `VividOrange.ISerialization` floor. Hard dependencies are `Newtonsoft.Json`, `UnitsNet.Serialization.JsonNet`, and `VividOrange.ISerialization`; `UnitsNet` is the shared quantity floor consumed through the Json.NET converter.

[MARKER_FLOOR_SPLIT]: `VividOrange.Serialization.ITaxonomySerializable` is the empty marker interface declared in assembly `VividOrange.ISerialization`; it admits a type to the generic round-trip without carrying serialization behavior. `VividOrange.Serialization` owns `ToJson` and `FromJson`, so a serializing consumer references the marker floor and this implementation. `VividOrange.Taxonomy.Serialization.ITaxonomySerializable`, declared in assembly `VividOrange.Taxonomy.ISerialization`, has distinct CLR type identity and governs the uncertainty taxonomy. `TaxonomyJsonSerializer`, `ToJson`, and `FromJson` operate only on the first marker lane.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the round-trip extension surface (the only consumer-callable type)

- rail: serialization

`JsonSerializationExtensions` is the consumer-callable static extension owner. Both members constrain `T` with `ITaxonomySerializable`, accept optional `JsonSerializerSettings`, and use the taxonomy defaults when settings are `null`.

| [INDEX] | [MEMBER]      | [RECEIVER] | [RESULT] |
| :-----: | :------------ | :--------- | :------- |
|  [01]   | `ToJson<T>`   | `this T`   | `string` |
|  [02]   | `FromJson<T>` | `string`   | `T?`     |

[PUBLIC_TYPE_SCOPE]: the settings owner — NOT consumer-callable (`internal`)

- rail: serialization
- gate: `TaxonomyJsonSerializer` is internal, and its static `Settings` owns the default wire. Consumers customize the wire by passing their own `JsonSerializerSettings` to `ToJson` or `FromJson`; the internal settings remain immutable from the consumer boundary. `ToJson` writes with `Formatting.Indented`.

`TaxonomyJsonSerializer.Settings` binds these default values.

| [INDEX] | [SETTING]                        | [VALUE]                          |
| :-----: | :------------------------------- | :------------------------------- |
|  [01]   | `Converters[0]`                  | `StringEnumConverter`            |
|  [02]   | `Converters[1]`                  | `UnitsNetIQuantityJsonConverter` |
|  [03]   | `TypeNameHandling`               | `Objects`                        |
|  [04]   | `TypeNameAssemblyFormatHandling` | `Simple`                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: round-trip a taxonomy object

- rail: serialization

The extension pair constrains `T` with `ITaxonomySerializable`. Default serialization emits indented JSON with polymorphic `$type` discrimination and SI-scalar-plus-unit quantities, while default deserialization reconstructs the precise concrete runtime type behind `T`. A settings argument replaces the taxonomy defaults with the consumer-owned converter and handling policy.

| [INDEX] | [SURFACE]                    | [SETTINGS] | [RESULT] |
| :-----: | :--------------------------- | :--------- | :------- |
|  [01]   | `taxonomyObject.ToJson()`    | default    | `string` |
|  [02]   | `json.FromJson<T>()`         | default    | `T?`     |
|  [03]   | `taxonomyObject.ToJson(...)` | custom     | `string` |
|  [04]   | `json.FromJson<T>(...)`      | custom     | `T?`     |

## [04]-[IMPLEMENTATION_LAW]

[ROUNDTRIP_CONTRACT]:

- The round-trip is ONE generic pair constrained `where T: ITaxonomySerializable` — there is no per-type serializer.
  Every VividOrange DATA type implements the marker, so `ToJson`/`FromJson` is the SINGLE polymorphic rail for the whole
  taxonomy: a material, a reinforced section, a standard, a catalogued profile, and a capacity mesh all serialize
  through the same two methods.
- Newtonsoft.Json owns serialization through `JsonConvert.SerializeObject` and `JsonConvert.DeserializeObject`; System.Text.Json does not participate in this rail.
- `TaxonomyJsonSerializer.Settings` binds `TypeNameHandling.Objects`, `TypeNameAssemblyFormatHandling.Simple`, `StringEnumConverter`, and `UnitsNetIQuantityJsonConverter`. These settings emit `$type` for precise polymorphic reconstruction, shorten assembly-qualified names, encode enums as names, and encode every `UnitsNet.IQuantity` as `{ Value, Unit }`. `ToJson` writes `Formatting.Indented`.
- `FromJson<T>` returns `T?` (nullable) — a malformed/empty JSON deserializes to `null` rather than throwing a typed
  error; the boundary that round-trips traps the `null` (and any Newtonsoft `JsonException` the parse may throw) at the
  in-folder seam and lowers it onto the canonical typed wire-decode error rail.

[POLYMORPHIC_TYPE_PRESERVATION]:

- `TypeNameHandling.Objects` is load-bearing for the taxonomy: a `ConcreteSection` exposed through an `ISection`
  reference, an `EnConcreteMaterial` through an `IMaterial`, an `En1992` through an `IStandard`, and a `ForceMomentMesh`
  through an `IForceMomentMesh` all carry a `$type` tag, so the deserializer rebuilds the precise concrete type rather
  than the interface. A consumer relying on this MUST keep `TypeNameHandling.Objects` when supplying custom settings —
  dropping it collapses polymorphic fields to their declared interface and breaks reconstruction.
- `TypeNameHandling.Objects` is a known Newtonsoft.Json deserialization-gadget surface: `FromJson<T>` must only be fed
  JSON produced by a trusted VividOrange `ToJson` (the round-trip is producer=consumer here, the C# sole-producer
  minting and re-reading its own taxonomy wire), never an untrusted external document — an external/peer-decoded shape
  goes through the canonical Materials wire, not this `$type`-tagged round-trip.

[WIRE_FIREBREAK]:

- Newtonsoft.Json owns the internal VividOrange taxonomy round-trip. Thinktecture System.Text.Json and MessagePack generated codecs own the canonical cross-language wire and its SmartEnum, union, and value-object shapes.
- A VividOrange object retained inside C# uses `ToJson` and `FromJson`; a cross-language Materials concept lowers to the canonical shape and its Thinktecture codec. The VividOrange `$type` shape remains inside the C# boundary.
- The `UnitsNet.Serialization.JsonNet` converter ([TRANSITIVE_UNITSNET_JSONNET]) is the Json.NET sibling of
  the Materials STJ quantity handling — both serialize an `IQuantity` to `{ Value, Unit }`, so a quantity carried in a
  VividOrange round-trip and a quantity on the canonical wire agree on the SI-scalar+unit shape even across the two
  serializers.

[LOCAL_ADMISSION]:

- The round-trip persists or clones a VividOrange data object inside the C# layer; the Thinktecture System.Text.Json owner governs the cross-language wire ([WIRE_FIREBREAK]).
- The boundary traps `FromJson<T>` `null` results and Newtonsoft exceptions, then lowers them onto the typed wire-decode rail.
- A consumer that needs a non-default wire passes its own `JsonSerializerSettings` (preserving `TypeNameHandling.Objects`
  and the UnitsNet converter, [POLYMORPHIC_TYPE_PRESERVATION]); it never mutates the `internal` `TaxonomyJsonSerializer`.

[STACK]:

- taxonomy seam: `ToJson<T>` and `FromJson<T>` form one round-trip for every `ITaxonomySerializable`.
- admitted taxonomy: material grade records, reinforced sections and bars, section-property carriers, standards, catalogued profiles, and the N-M-M capacity mesh all use the same pair.
- quantity seam: the `UnitsNetIQuantityJsonConverter` ([TRANSITIVE_UNITSNET_JSONNET]) serializes every
  `UnitsNet.IQuantity` field (`Pressure`/`Area`/`Length`/`Ratio`/`Force`/`Torque`, `api-unitsnet.md`) as SI scalar +
  unit token — so a serialized material strength or section modulus carries its dimension across the round-trip.
- firebreak seam: distinct from the Materials canonical Thinktecture STJ/MessagePack wire ([WIRE_FIREBREAK]) — this is
  the C#-internal VividOrange round-trip, the canonical cross-language shape is the Thinktecture-generated codec.
- marker seam: the `ITaxonomySerializable` constraint is the `VividOrange.ISerialization` floor marker
  ([MARKER_FLOOR_SPLIT]) — the floor declares the tag, this package is the behavior; a serializing page references both.

[TRANSITIVE_UNITSNET_JSONNET]:

- direct package: `VividOrange.Serialization` brings `UnitsNet.Serialization.JsonNet` and registers the quantity converter stack for taxonomy JSON.

[CONVERTER_ALGEBRA]:

- base root: `UnitsNetBaseJsonConverter<T>: JsonConverter<T>` — owns the `ValueUnit`/`ExtendedValueUnit` DTO model,
  the `decimal`-quantity path (serialize as string to keep `100` vs), the `ConvertIQuantity` / `ConvertValueUnit` / `ReadValueUnit` bridges, and the `RegisterCustomType` registry. A custom converter subclasses
  it and overrides `ReadJson`/`WriteJson`.
- abbreviated root: `AbbreviatedUnitsConverter` — the compact `"value unit"` form, resolving the unit through a
  `UnitAbbreviationsCache` (default or injected). It is the standard converter: smaller payload, human-readable,
  registry-extensible.
- object root: `UnitsNetIQuantityJsonConverter` (`{ Value, Unit }` read+write) and `UnitsNetIComparableJsonConverter`
  (read-only `IComparable`) — the verbose object form for producer interop with producers that emit it.

[DETERMINISM_CONTRACT]:

- The abbreviated form's read/write resolves through a `UnitAbbreviationsCache`; the parameterless ctor uses
  `UnitAbbreviationsCache.Default` (culture-sensitive via `CurrentCulture`). For deterministic Materials wire the
  boundary INJECTS an invariant-configured abbreviations cache through the 3-arg ctor so the same JSON byte parses
  identically regardless of the host's ambient culture or loaded `UnitsNet` satellites (the determinism law of
  `api-unitsnet.md` extends to the wire).
- The object `{ Value, Unit }` form encodes the unit as the `"<QuantityType>.<Unit>"` enum path
  (`UnitsNet.Units.<enum>`), so it is culture-independent on the unit token but verbose; the value is a raw `double`
  except for `decimal` quantities, which carry `ValueString`/`ValueType` to preserve precision.

[LOCAL_ADMISSION]:

- The Materials wire boundary registers this Json.NET `JsonConverter` on `JsonSerializerSettings`, and Json.NET dispatches every quantity through the registered converter.
- `VividOrange.Serialization` requires this transitive floor. The interchange owner registers it once, and every `UnitsNet`-typed field round-trips through that setting.

[STACK]:

- serialization seam: `VividOrange.Serialization` is the `ITaxonomySerializable` JSON pipeline over `Newtonsoft.Json`; this converter handles its `UnitsNet` quantities.
- settings seam: registering the converter on the taxonomy settings admits serialization of a capacity mesh carrying `Force` and `Torque` coordinates.
- hull seam: the `InteractionDiagram` output mesh (`api-vividorange-iforcemomentinteraction.md` /
  `api-vividorange-forcemomentinteraction.md`) carries `IForceMomentVertex.X:Force`, `.Y/.Z:Torque` — this converter
  is what writes those vertex coordinates to JSON and reads them back, so a computed capacity surface persists/round-
  trips with its units intact.
- units seam: the converter is the wire side of the in-folder `UnitsNet` owner (`api-unitsnet.md`) — the SAME
  invariant-culture determinism the `MaterialUnits` boundary enforces on parse/abbreviation applies here via the
  injected `UnitAbbreviationsCache`, so a measured row and a serialized quantity agree across hosts.
- interchange seam: the Materials `interchange#MATERIAL_WIRE` projection registers `AbbreviatedUnitsConverter` so a
  quantity crosses to a TS/Python peer as the canonical scalar + unit token (not a localized string); the inverse
  path decodes the same form back to a typed `IQuantity`.

[RAIL_LAW]:

- Package: `UnitsNet.Serialization.JsonNet` (MIT-0, `netstandard2.0`-only, `net10.0` binds `netstandard2.0`)
- Owns: the `UnitsNet` ↔ Json.NET converter set — `AbbreviatedUnitsConverter` (compact `"value unit"` form,
  culture/registry-aware), `UnitsNetIQuantityJsonConverter` / `UnitsNetIComparableJsonConverter` (object form
  `{ Value, Unit }` object form), and the `UnitsNetBaseJsonConverter<T>` base with the `ValueUnit`/`ExtendedValueUnit`
  model, `decimal`-quantity handling, and the `RegisterCustomType` registry.
- Accept: a `UnitsNet` quantity round-tripped as a registered Json.NET `JsonConverter` on the Materials wire
  boundary; the abbreviations cache injected for invariant-culture determinism; the transitive floor
  `VividOrange.Serialization` requires
- Reject: imperatively hand-parsing a quantity instead of registering the converter; the culture-default abbreviated
  ctor on a deterministic boundary (inject an invariant cache); serializing a quantity as a bare `double` that drops
  the unit

[RAIL_LAW]:

- Package: `VividOrange.Serialization` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the polymorphic JSON round-trip IMPL for the VividOrange taxonomy — `JsonSerializationExtensions.ToJson<T>` /
  `FromJson<T>` (`T: ITaxonomySerializable`) over a Newtonsoft.Json + `UnitsNet.Serialization.JsonNet` settings stack
  (`TypeNameHandling.Objects` polymorphic `$type`, `StringEnumConverter`, the `IQuantity` SI-scalar+unit converter) —
  the SOLE generic round-trip rail for every VividOrange DATA type. The marker itself is the `VividOrange.ISerialization`
  floor ([MARKER_FLOOR_SPLIT]); the `TaxonomyJsonSerializer` settings owner is `internal`.
- Accept: a VividOrange DATA object round-tripped INSIDE the C# layer (persist/clone) through `ToJson`/`FromJson`,
  preserving the polymorphic concrete type via the `$type` tag and the quantities via the SI-scalar+unit converter; a
  custom wire supplied as a consumer-owned `JsonSerializerSettings` (keeping `TypeNameHandling.Objects` + the UnitsNet
  converter); the `FromJson` `null`/throw trapped at the in-folder boundary onto the typed wire-decode rail.
- Reject: treating this Newtonsoft `$type` round-trip as the cross-language canonical wire (that is the Materials
  Thinktecture STJ owner, [WIRE_FIREBREAK]); feeding `FromJson<T>` an untrusted external document under
  `TypeNameHandling.Objects` ([POLYMORPHIC_TYPE_PRESERVATION]); reading/mutating the `internal` `TaxonomyJsonSerializer`
  instead of passing custom settings; dropping `TypeNameHandling.Objects` from custom settings (collapses polymorphic
  fields); a `FromJson` `null` left unhandled where a typed decode rail is required.
