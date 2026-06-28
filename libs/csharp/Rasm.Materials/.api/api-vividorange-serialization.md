# [RASM_MATERIALS_API_VIVIDORANGE_SERIALIZATION]

`VividOrange.Serialization` supplies the polymorphic JSON round-trip IMPL for the entire VividOrange taxonomy —
the behavior behind the `ITaxonomySerializable` marker. The whole admitted VividOrange surface implements that
marker: every material (`EnConcreteMaterial`/`EnSteelMaterial`/`EnRebarMaterial`, the four constitutive analysis
materials, `api-vividorange-materials.md`), every section + bar (`Section`/`ConcreteSection`/`Rebar`/`Link`/the
reinforcement layers, `api-vividorange-sections.md`), every section-property carrier
(`api-vividorange-sections-sectionproperties.md`), every standard (`En1990`..`En1999`, the `IStandard` interface,
`api-vividorange-standards.md`), every catalogued profile (`api-vividorange-profiles-catalogue.md`), and the N-M-M
capacity `IForceMomentMesh` (`api-vividorange-iforcemomentinteraction.md`). This package is the SOLE generic round-trip
rail for all of them: `JsonSerializationExtensions.ToJson<T>` / `FromJson<T>`, constrained `where T :
ITaxonomySerializable`, over a Newtonsoft.Json + `UnitsNet.Serialization.JsonNet` settings stack with
`TypeNameHandling.Objects` polymorphic `$type` discrimination — so a runtime VividOrange object graph round-trips to
canonical SI-scalar+unit JSON preserving the exact concrete types. It is DISTINCT from the interface-only
`VividOrange.ISerialization` floor (which declares the empty `ITaxonomySerializable` marker and nothing else); this
package is the impl that gives the marker behavior. The quantity-converter half is `UnitsNet.Serialization.JsonNet`
(`api-unitsnet-serialization-jsonnet.md`); the quantity types are `UnitsNet` (`api-unitsnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Serialization`
- package: `VividOrange.Serialization`
- version: `0.1.0`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Serialization`
- namespace: `VividOrange.Serialization`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `netstandard2.0` / `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the
  bound TFM — no `net9.0`+ asset, so the `api resolve` primary `net8.0` IS the consumed surface).
- rail: serialization (the VividOrange-taxonomy JSON wire)
- ABI floor: a `0.1.0` PRE-1.0 contract — the extension-method surface may break across a minor bump. The
  `ITaxonomySerializable` marker comes from the transitive `VividOrange.ISerialization` `0.1.0` floor (centrally
  pinned). Hard dependencies: `Newtonsoft.Json 13.0.4` (the serializer — NOT System.Text.Json), `UnitsNet.Serialization.JsonNet`
  `5.50.0` (the `IQuantity` Json.NET converter), `VividOrange.ISerialization 0.1.0`. `Newtonsoft.Json` `13.0.4` is the
  centrally-pinned floor; `UnitsNet` `5.75.0` is the shared quantity floor consumed via the JsonNet converter.

[MARKER_FLOOR_SPLIT]: `ITaxonomySerializable` is the EMPTY MARKER interface (zero members) declared in the
interface-only `VividOrange.ISerialization` floor — it carries NO serialization behavior itself; it is the
constraint tag that admits a type to the round-trip generic. THIS package (`VividOrange.Serialization`) is the impl
that supplies the actual `ToJson`/`FromJson` behavior over the marker. A consumer references BOTH: every VividOrange
DATA package transitively pins `VividOrange.ISerialization` (so its types ARE `ITaxonomySerializable`), and a page
that serializes pins `VividOrange.Serialization` (the impl). The interface-only floor is NOT a substitute for this
impl — `ToJson`/`FromJson` live ONLY here. This marker's FQN is `VividOrange.Serialization.ITaxonomySerializable`
(declared in assembly `VividOrange.ISerialization` `0.1.0`) — a DISTINCT CLR type identity from the `0.2.0`
`VividOrange.Taxonomy.Serialization.ITaxonomySerializable` (assembly `VividOrange.Taxonomy.ISerialization`) the
`VividOrange.Uncertainties` packages ride (`api-vividorange-uncertainties.md`); `TaxonomyJsonSerializer` /
`ToJson` / `FromJson` here serialize ONLY the `0.1.0` lane and do NOT round-trip the `0.2.0` uncertainty types, so a
Materials design page never assumes one shared VividOrange serializer covers both taxonomy interfaces.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the round-trip extension surface (the only consumer-callable type)
- rail: serialization

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------- | :------------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `JsonSerializationExtensions`  | `static` extensions | `ToJson<T>(this T, JsonSerializerSettings? = null) where T : ITaxonomySerializable -> string` and `FromJson<T>(this string, JsonSerializerSettings? = null) where T : ITaxonomySerializable -> T?` — the generic round-trip over any taxonomy object, defaulting to the taxonomy settings |

[PUBLIC_TYPE_SCOPE]: the settings owner — NOT consumer-callable (`internal`)
- rail: serialization
- gate: `TaxonomyJsonSerializer` is `internal` to this assembly — a consumer NEVER reads `TaxonomyJsonSerializer.Settings`
  directly. Its configured `JsonSerializerSettings` is the DEFAULT the extension methods apply when the optional
  `settings` argument is null; it is documented here for the wire-shape LAW, not as an entrypoint. To customize the
  wire, a consumer passes its OWN `JsonSerializerSettings` to `ToJson`/`FromJson` (rebuilding the taxonomy converters
  it needs), not a mutation of this internal owner.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]          | [CAPABILITY]                                                                  |
| :-----: | :------------------------ | :---------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `TaxonomyJsonSerializer`  | settings owner (internal) | `static JsonSerializerSettings Settings` — the default wire: `Converters = { StringEnumConverter, UnitsNetIQuantityJsonConverter }`, `TypeNameHandling = Objects`, `TypeNameAssemblyFormatHandling = Simple`; `ToJson` writes with `Formatting.Indented` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: round-trip a taxonomy object
- rail: serialization

| [INDEX] | [SURFACE]                                                                                     | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `taxonomyObject.ToJson()`                                                                      | extension      | serialize any `ITaxonomySerializable` (a material/section/standard/profile/mesh) to indented JSON with the polymorphic `$type` discriminator + SI-scalar+unit quantities |
|  [02]   | `json.FromJson<T>()` (`T : ITaxonomySerializable`)                                             | extension      | deserialize back to `T?`, reconstructing the EXACT concrete runtime type from the `$type` tag (e.g. a `ConcreteSection` deserialized through an `ISection` `T`) |
|  [03]   | `taxonomyObject.ToJson(settings)` / `json.FromJson<T>(settings)`                               | extension      | round-trip with a CUSTOM `JsonSerializerSettings` (the consumer supplies its own converters/handling) — the override for a non-default wire |

## [04]-[IMPLEMENTATION_LAW]

[ROUNDTRIP_CONTRACT]:
- The round-trip is ONE generic pair constrained `where T : ITaxonomySerializable` — there is no per-type serializer.
  Every VividOrange DATA type implements the marker, so `ToJson`/`FromJson` is the SINGLE polymorphic rail for the whole
  taxonomy: a material, a reinforced section, a standard, a catalogued profile, and a capacity mesh all serialize
  through the same two methods.
- The serializer is Newtonsoft.Json (`JsonConvert.SerializeObject`/`DeserializeObject`), NOT System.Text.Json. The
  default settings (the `internal TaxonomyJsonSerializer.Settings`): `TypeNameHandling = Objects` (emits a `$type`
  discriminator on every object so a polymorphic `IMaterial`/`ISection`/`IStandard` field reconstructs its exact
  concrete type), `TypeNameAssemblyFormatHandling = Simple` (the short assembly-qualified name, not the full
  version/culture/token), `StringEnumConverter` (enums as names, not ordinals — so `EnConcreteGrade.C30_37` is a
  readable token), and the `UnitsNetIQuantityJsonConverter` (every `UnitsNet.IQuantity` as `{ Value, Unit }`, not a raw
  double). `ToJson` writes `Formatting.Indented`.
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
- This is the VividOrange-INTERNAL taxonomy round-trip (Newtonsoft.Json), DISTINCT from the Materials CANONICAL wire.
  The Materials canonical owner mints its SmartEnum/Union/ValueObject shapes through the Thinktecture
  System.Text.Json + MessagePack generated codecs (`Thinktecture.Runtime.Extensions.Json`/`.MessagePack`, the C#
  sole-producer the TS/Python peers decode). The two are different stacks: a VividOrange DATA object that survives only
  inside the C# computation (e.g. round-tripping a `ConcreteSection` to cache or to feed `ConcreteSectionProperties`)
  uses THIS `ToJson`/`FromJson`; a Materials concept projected onto the cross-language canonical wire is lowered to the
  Materials canonical SmartEnum/ValueObject shape and serialized through Thinktecture STJ — the VividOrange `$type`
  Newtonsoft shape NEVER crosses to the TS/Python peer.
- The `UnitsNet.Serialization.JsonNet` converter (`api-unitsnet-serialization-jsonnet.md`) is the Json.NET sibling of
  the Materials STJ quantity handling — both serialize an `IQuantity` to `{ Value, Unit }`, so a quantity carried in a
  VividOrange round-trip and a quantity on the canonical wire agree on the SI-scalar+unit shape even across the two
  serializers.

[LOCAL_ADMISSION]:
- The round-trip is admitted at the Materials boundary that needs to PERSIST or CLONE a VividOrange DATA object inside
  the C# layer — a computed `ConcreteSection`, a derived material, a standard reference — not as the cross-language wire
  (that is the Thinktecture STJ canonical owner, [WIRE_FIREBREAK]). The `FromJson<T>` `null` and any Newtonsoft throw is
  trapped at the in-folder seam and lowered onto the typed wire-decode rail.
- A consumer that needs a non-default wire passes its own `JsonSerializerSettings` (preserving `TypeNameHandling.Objects`
  and the UnitsNet converter, [POLYMORPHIC_TYPE_PRESERVATION]); it never mutates the `internal` `TaxonomyJsonSerializer`.

[STACK]:
- taxonomy seam: `ToJson<T>`/`FromJson<T>` is the round-trip for EVERY `ITaxonomySerializable` — the Materials grade
  records (`api-vividorange-materials.md`), the reinforced sections + bars (`api-vividorange-sections.md`), the
  section-property carriers (`api-vividorange-sections-sectionproperties.md`), the standards
  (`api-vividorange-standards.md`), the catalogued profiles (`api-vividorange-profiles-catalogue.md`), and the N-M-M
  capacity mesh (`api-vividorange-iforcemomentinteraction.md`) — one rail, every VividOrange type.
- quantity seam: the `UnitsNetIQuantityJsonConverter` (`api-unitsnet-serialization-jsonnet.md`) serializes every
  `UnitsNet.IQuantity` field (`Pressure`/`Area`/`Length`/`Ratio`/`Force`/`Torque`, `api-unitsnet.md`) as SI scalar +
  unit token — so a serialized material strength or section modulus carries its dimension across the round-trip.
- firebreak seam: distinct from the Materials canonical Thinktecture STJ/MessagePack wire ([WIRE_FIREBREAK]) — this is
  the C#-internal VividOrange round-trip, the canonical cross-language shape is the Thinktecture-generated codec.
- marker seam: the `ITaxonomySerializable` constraint is the `VividOrange.ISerialization` floor marker
  ([MARKER_FLOOR_SPLIT]) — the floor declares the tag, this package is the behavior; a serializing page references both.

[RAIL_LAW]:
- Package: `VividOrange.Serialization` `0.1.0` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the polymorphic JSON round-trip IMPL for the VividOrange taxonomy — `JsonSerializationExtensions.ToJson<T>` /
  `FromJson<T>` (`T : ITaxonomySerializable`) over a Newtonsoft.Json + `UnitsNet.Serialization.JsonNet` settings stack
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
