# [RASM_MATERIALS_API_UNITSNET_SERIALIZATION_JSONNET]

`UnitsNet.Serialization.JsonNet` is the `UnitsNet` ↔ Json.NET (`Newtonsoft.Json`) converter set — the wire owner
that round-trips any `IQuantity` (`Force`, `Torque`, `Length`, `Area`, `Pressure`, …) through a `JsonConverter` so a
`UnitsNet` quantity is a first-class JSON value with its value + unit preserved, never a lossy bare `double`. It is
the converter the `VividOrange.Serialization` `ITaxonomySerializable` pipeline (`api-vividorange-serialization.md`)
REQUIRES to serialize the `Force`/`Torque`-bearing capacity-hull mesh (`api-vividorange-iforcemomentinteraction.md` /
`api-vividorange-forcemomentinteraction.md`) the `InteractionDiagram` engine emits
(`api-vividorange-interactiondiagram.md`), and the same converter the Materials `interchange#MATERIAL_WIRE`
projection uses so a measured/computed quantity (`api-unitsnet.md`) crosses the wire as canonical scalar + unit token
to a TS/Python peer. It carries TWO format families: the modern `AbbreviatedUnitsConverter` (compact `"value unit"`
abbreviated form, culture/comparer/registry-aware) and the legacy `UnitsNetIQuantityJsonConverter` /
`UnitsNetIComparableJsonConverter` (`{ "Value", "Unit" }` object form), over the shared `UnitsNetBaseJsonConverter<T>`
base.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet.Serialization.JsonNet`
- package: `UnitsNet.Serialization.JsonNet`
- version: `5.50.0`
- license: MIT-0 (`licenses.nuget.org/MIT-0` — public-domain-equivalent, no attribution clause; same MIT-0 as the
  `UnitsNet` core)
- assembly: `UnitsNet.Serialization.JsonNet`
- namespace: `UnitsNet.Serialization.JsonNet`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. SINGLE-TFM `netstandard2.0` ONLY (no
  `net8.0`+ asset ships) — the consumer `net10.0` binds `lib/netstandard2.0/UnitsNet.Serialization.JsonNet.dll`, the
  bound and only surface (the `api resolve` primary `netstandard2.0` is the consumed asset, not an under-resolved
  fallback). Ships its XML doc (`UnitsNet.Serialization.JsonNet.xml`). NO satellite resource assemblies.
- rail: units (wire — the `UnitsNet` ↔ Json.NET converter)
- ABI floor: a stable `5.x` line tracking the `UnitsNet` `5.x` major. Hard dep on `Newtonsoft.Json` (the Rasm
  central pin `13.0.4` rides the existing row) and `UnitsNet` `5.75.0` (the in-folder quantity owner). All centrally
  pinned; this is the transitive floor `VividOrange.Serialization` requires for `UnitsNet`-typed taxonomy fields.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `UnitsNet` ↔ Json.NET converters
- rail: units
- base law: all three converters extend `UnitsNetBaseJsonConverter<T> : JsonConverter<T>` (the `Newtonsoft.Json`
  generic converter), so each registers via `JsonSerializerSettings.Converters` and dispatches on the JSON node
  shape — a Materials wire boundary adds the chosen converter to the Json.NET settings, never hand-parses a quantity.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]        | [CAPABILITY]                                                                  |
| :-----: | :-------------------------------- | :-------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `AbbreviatedUnitsConverter`       | converter (modern)    | `: JsonConverter<IQuantity>` — the compact `"value unit"` abbreviated form, read+write, culture/comparer/registry-aware (the recommended converter) |
|  [02]   | `UnitsNetIQuantityJsonConverter`  | converter (legacy)    | `: UnitsNetBaseJsonConverter<IQuantity?>` (sealed) — the `{ "Value", "Unit" }` object form, read+write |
|  [03]   | `UnitsNetIComparableJsonConverter`| converter (legacy)    | `: UnitsNetBaseJsonConverter<IComparable?>` (sealed) — the `IComparable` read-only mirror (`CanWrite => false`); deserializes legacy quantities typed as `IComparable` |
|  [04]   | `UnitsNetBaseJsonConverter<T>`    | converter base        | `abstract : JsonConverter<T>` — the shared `ValueUnit`/`ExtendedValueUnit` model, `decimal`-quantity handling, and the custom-type registry; subclass to add a quantity-typed converter |

[PUBLIC_TYPE_SCOPE]: the base converter's serialization model + helpers (`protected`)
- rail: units
- note: these `protected` members are the EXTENSION surface for a custom converter (a Materials-owned converter that
  narrows to a specific quantity family); a plain wire boundary uses the three concrete converters above and never
  touches these directly.

| [INDEX] | [SYMBOL]                                              | [PACKAGE_ROLE]      | [CAPABILITY]                                                                  |
| :-----: | :--------------------------------------------------- | :------------------ | :--------------------------------------------------------------------------- |
|  [01]   | `UnitsNetBaseJsonConverter<T>.ValueUnit`             | nested model (protected) | the `{ string Unit; double Value }` legacy DTO                          |
|  [02]   | `UnitsNetBaseJsonConverter<T>.ExtendedValueUnit`     | nested model (protected, sealed) | `: ValueUnit` + `string? ValueString` / `string? ValueType` — the `decimal`-quantity carrier (decimal serialized as string to preserve `100` vs `100.00`) |
|  [03]   | `RegisterCustomType(Type quantity, Type unit)`       | registry call       | register a custom `(quantity, unit)` pair the base resolves via `new T(double, unit)` |
|  [04]   | `ConvertIQuantity(IQuantity quantity)`               | helper (protected)  | `IQuantity` -> `ValueUnit`/`ExtendedValueUnit` (decimal-aware)               |
|  [05]   | `ConvertValueUnit(ValueUnit valueUnit)`              | helper (protected)  | `ValueUnit` -> `IQuantity` (resolves `UnitsNet.Units.<enum>` and `Quantity.From`) |
|  [06]   | `ReadValueUnit(JToken jsonToken)`                    | helper (protected)  | a `JToken` -> `ValueUnit`/`ExtendedValueUnit` parse                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `AbbreviatedUnitsConverter` — the modern compact form
- rail: units
- registration law: add to `JsonSerializerSettings.Converters`; it writes `IQuantity` as the abbreviated `"value
  unit"` string and reads it back by resolving the abbreviation through the (default or supplied)
  `UnitAbbreviationsCache`. The 3-arg ctor injects a custom quantity registry + abbreviations cache + property-name
  comparer, so the Materials boundary controls culture-determinism (pass an invariant abbreviations cache, per
  `api-unitsnet.md`).

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]   | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `new AbbreviatedUnitsConverter()`                                       | constructor    | the default abbreviated converter (default `Quantity.Infos` + `UnitAbbreviationsCache.Default`) |
|  [02]   | `new AbbreviatedUnitsConverter(IEqualityComparer<string?> comparer)`    | constructor    | as default with a custom property-name comparer                              |
|  [03]   | `new AbbreviatedUnitsConverter(IDictionary<string, QuantityInfo> quantities, UnitAbbreviationsCache abbreviations, IEqualityComparer<string?> propertyComparer)` | constructor | full control — the quantity registry, abbreviations cache (for invariant-culture determinism), and comparer |
|  [04]   | `.WriteJson(JsonWriter, IQuantity?, JsonSerializer)`                    | override       | writes the quantity as the abbreviated `"value unit"` form                   |
|  [05]   | `.ReadJson(JsonReader, Type, IQuantity?, bool, JsonSerializer)`         | override       | reads the abbreviated form back to a typed `IQuantity`                       |
|  [06]   | `.FindUnit(string abbr, out QuantityInfo)` / `.TryParse(string, QuantityInfo, out Enum)` (protected) | override hooks | the abbreviation→unit resolution a subclass can specialize           |

[ENTRYPOINT_SCOPE]: the legacy `{ Value, Unit }` converters
- rail: units
- registration law: add to `JsonSerializerSettings.Converters`; `UnitsNetIQuantityJsonConverter` round-trips the
  `{ "Value": <double>, "Unit": "<QuantityType>.<Unit>" }` object, `UnitsNetIComparableJsonConverter` only READS
  it (for fields typed `IComparable`). Use these only to interop with a producer that emits the legacy object form;
  new Materials wire uses `AbbreviatedUnitsConverter`.

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]   | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `new UnitsNetIQuantityJsonConverter()`                                  | constructor    | the legacy `{ Value, Unit }` read+write converter                            |
|  [02]   | `UnitsNetIQuantityJsonConverter.WriteJson` / `.ReadJson`               | override       | round-trips an `IQuantity` as the `{ Value, Unit }` object                    |
|  [03]   | `new UnitsNetIComparableJsonConverter()`                               | constructor    | the legacy read-only `IComparable` converter (`CanWrite => false`)           |
|  [04]   | `UnitsNetIComparableJsonConverter.ReadJson`                            | override       | reads a legacy `{ Value, Unit }` object into an `IComparable` quantity        |
|  [05]   | `converter.RegisterCustomType(typeof(MyQuantity), typeof(MyUnit))`     | registry call  | teach any base converter a custom `(quantity, unit)` pair (shared base method)|

## [04]-[IMPLEMENTATION_LAW]

[CONVERTER_ALGEBRA]:
- base root: `UnitsNetBaseJsonConverter<T> : JsonConverter<T>` — owns the `ValueUnit`/`ExtendedValueUnit` DTO model,
  the `decimal`-quantity path (serialize as string to keep `100` vs `100.00`), the `ConvertIQuantity` /
  `ConvertValueUnit` / `ReadValueUnit` bridges, and the `RegisterCustomType` registry. A custom converter subclasses
  it and overrides `ReadJson`/`WriteJson`.
- modern root: `AbbreviatedUnitsConverter` — the compact `"value unit"` form, resolving the unit through a
  `UnitAbbreviationsCache` (default or injected). It is the recommended converter: smaller payload, human-readable,
  registry-extensible.
- legacy root: `UnitsNetIQuantityJsonConverter` (`{ Value, Unit }` read+write) and `UnitsNetIComparableJsonConverter`
  (read-only `IComparable`) — the verbose object form for back-compat with producers that emit it.

[DETERMINISM_CONTRACT]:
- The abbreviated form's read/write resolves through a `UnitAbbreviationsCache`; the parameterless ctor uses
  `UnitAbbreviationsCache.Default` (culture-sensitive via `CurrentCulture`). For deterministic Materials wire the
  boundary INJECTS an invariant-configured abbreviations cache through the 3-arg ctor so the same JSON byte parses
  identically regardless of the host's ambient culture or loaded `UnitsNet` satellites (the determinism law of
  `api-unitsnet.md` extends to the wire).
- The legacy `{ Value, Unit }` form encodes the unit as the `"<QuantityType>.<Unit>"` enum path
  (`UnitsNet.Units.<enum>`), so it is culture-independent on the unit token but verbose; the value is a raw `double`
  except for `decimal` quantities, which carry `ValueString`/`ValueType` to preserve precision.

[LOCAL_ADMISSION]:
- This converter is admitted ONLY as a Json.NET `JsonConverter` registered on the Materials wire boundary's
  `JsonSerializerSettings`, never invoked imperatively — a quantity is serialized by adding the converter and
  letting Json.NET dispatch. It is the transitive floor `VividOrange.Serialization` (`api-vividorange-serialization.md`)
  requires; the Materials interchange owner registers the converter once and every `UnitsNet`-typed field
  (the hull's `Force`/`Torque`, a measured `Length`/`Area`/`Pressure`) round-trips through it.

[STACK]:
- serialization seam: `VividOrange.Serialization` (`api-vividorange-serialization.md`) is the `ITaxonomySerializable`
  JSON pipeline over `Newtonsoft.Json`; THIS converter is its required `UnitsNet`-quantity handler — registering it
  on the taxonomy serializer's settings is what lets a `Force`/`Torque`-bearing capacity mesh serialize at all.
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
- Package: `UnitsNet.Serialization.JsonNet` `5.50.0` (MIT-0, `netstandard2.0`-only, `net10.0` binds `netstandard2.0`)
- Owns: the `UnitsNet` ↔ Json.NET converter set — `AbbreviatedUnitsConverter` (modern compact `"value unit"` form,
  culture/registry-aware), `UnitsNetIQuantityJsonConverter` / `UnitsNetIComparableJsonConverter` (legacy
  `{ Value, Unit }` object form), and the `UnitsNetBaseJsonConverter<T>` base with the `ValueUnit`/`ExtendedValueUnit`
  model, `decimal`-quantity handling, and the `RegisterCustomType` registry.
- Accept: a `UnitsNet` quantity round-tripped as a registered Json.NET `JsonConverter` on the Materials wire
  boundary; the abbreviations cache injected for invariant-culture determinism; the transitive floor
  `VividOrange.Serialization` requires
- Reject: imperatively hand-parsing a quantity instead of registering the converter; the culture-default abbreviated
  ctor on a deterministic boundary (inject an invariant cache); serializing a quantity as a bare `double` that drops
  the unit
