# [BIM_WIRE]

The host-free wire projection: one JSON wire surface over the generated `BimModel`/`BimElement`/`IfcClass`/`ElementPredicate`/`AssemblyRel`/`InterchangeFormat` owners through the `Thinktecture.Runtime.Extensions.Json` generated-owner converters and the `System.Text.Json` source-generated `JsonSerializerContext`, so each closed family serializes by its key or case discriminant rather than a hand-authored DTO mirror, and a `BimModel` snapshot crosses the wire as one content-keyed payload joined to the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity`. C# owns this wire vocabulary and the `python:geometry/ifc-companion` ifcopenshell peer and the TypeScript web peer decode it, never re-minting a parallel BIM shape; the `Review/issues#TS_PROJECTION` BCF topics, the `Review/diff#TS_PROJECTION` change-set, and the `Review/validation#IDS_FACETS` audit ride this same `BimWireContext`/`BimWireOptions` machinery, each carrying `[JsonSerializable]` rows on the one source-generated context rather than a second serializer. The page composes every generated owner and the Compute content-key as settled vocabulary; deserialization gates through each owner's `Validate` so a malformed wire payload faults at admission. The page is HOST-FREE.

## [1]-[INDEX]

- [1]-[WIRE_PROJECTION]: the `BimWire` snapshot payload, the `BimWireContext` source-generated serializer, and the generated-owner converter registration.

## [2]-[WIRE_PROJECTION]

- Owner: `BimWire` the content-keyed `BimModel` snapshot wire payload carrying the element/assembly/format projection and the Compute content-key; `BimWireContext` the `JsonSerializerContext` source-generated serializer over the record graph; `BimWireOptions` the one `JsonSerializerOptions` composition registering the Thinktecture generated-owner converter factory beside the source-generated context.
- Entry: `BimWire.Encode(BimModel model, UInt128 contentKey)` projects the snapshot onto the wire payload and `BimWire.Decode(ReadOnlyMemory<byte> json)` admits a wire payload back into the typed `BimModel` — `Fin<T>` aborts on a wire payload a generated owner's `Validate` rejects (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()` at the `Boundary` funnel, so a malformed `IfcClass` key or a half-built `ElementPredicate` case faults at admission rather than minting a half-built owner; the discriminant is the owner key or case, never a positional DTO.
- Auto: `Encode` serializes the `BimModel` graph through the `BimWireContext` source-generated metadata — the `IfcClass`/`Classification` `[SmartEnum<string>]` owners serialize by their string key through the Thinktecture converter, the `ElementPredicate`/`AssemblyRel` `[Union]` owners serialize by their case discriminant through the per-leaf `[JsonDerivedType]` polymorphic projection, and the `InterchangeFormat` row serializes by its key — and stamps the Compute content-key so a peer joins the snapshot to the same content-addressed artifact; `Decode` routes every owner through the `ThinktectureJsonConverterFactory` so each key/case re-admits through the owner's static `Validate`, a non-null validation error surfacing as a `JsonException` the `Boundary` funnel captures.
- Receipt: the `BimWire` payload is the one cross-runtime contract carrying THREE faces of one sealed model — the `Snapshot` full-model face, the `OpLog` incremental change-stream face (`Seq<ElementChange>` op rows the `Review/diff#MODEL_DIFF` fold produces), and the `Grpc` descriptor face (the `BimWireDescriptor` the service contract projects) — all emitted once through the one `BimWireContext`; the Python `ifcopenshell` companion and the TypeScript web peer decode the same `BimModel` vocabulary the C# branch mints, never re-minting a parallel BIM shape; the content-key joins the snapshot to the Compute `InterchangeIdentity` so a peer reads one content-addressed payload, and the `XxHash128`-keyed `WireFixture` golden bytes the C# branch host-validates pin the wire shape across runtimes.
- Packages: Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm, System.IO.Hashing, BCL `System.Text.Json`
- Growth: a new generated owner on the wire is one `[JsonSerializable]` row on `BimWireContext`; a new closed-family arm rides the same case discriminant through the generated converter; a new emission face is one `BimWireFace` arm folding the same model through the same context; the content-key is one column on `BimWire` and the golden-fixture key is one `XxHash128` row per wire type; admit no hand-authored DTO mirror — the discriminant is the owner key or case, never a positional DTO.
- Boundary: the wire serializes each closed family by its key or case discriminant through the `Thinktecture.Runtime.Extensions.Json` generated-owner converters and the `System.Text.Json` source-generated `JsonSerializerContext` — a hand-authored DTO mirror is the deleted form, the generated owners carry the wire converter so a peer runtime consumes the semantic model without a second mint; deserialization is admission and routes through each value-object/union/smart-enum `Validate` so a malformed wire payload faults at admission rather than minting a half-built owner — a reflection serializer that empty-object-constructs a private-keyed owner bypassing `Validate` is the named seam violation; the `ElementPredicate`/`AssemblyRel` unions are wire-capable only through the per-leaf `[JsonDerivedType]` discriminator the closed-union projection carries, never a positional case index; the snapshot joins the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` content-key and Bim mints no second identity; the `BimWire` is HOST-FREE — it carries no RhinoCommon type and no host-bound geometry, only the by-reference geometry-handle key the peer resolves through the content-addressed artifact; the `Review/issues#TS_PROJECTION` `BcfWire` topics, the `Review/diff#TS_PROJECTION` `DiffWire` change-set, and the `Review/validation#IDS_FACETS` `IdsAudit` audit are `[JsonSerializable]` rows on this one `BimWireContext` bound to the one `BimWireOptions.Json`, never a second serializer or a parallel wire vocabulary; the THREE emission faces (`Snapshot`/`OpLog`/`Grpc`) are one `BimWireFace` `[SmartEnum]` folding the same sealed model through the same context — the op-log face carries the `Review/diff#MODEL_DIFF` `ElementChange` op rows and the gRPC face the `BimWireDescriptor`, neither minting a second serializer — so the one sealed model emits snapshot+op-log+gRPC once and the three runtimes decode the one descriptor (`lib:ONE_MODEL_THREE_FACES`); the `WireFixture` `XxHash128`-keyed golden bytes the C# branch host-validates pin each wire type's byte shape (`lib:ONE_WIRE_FIXTURE_CORPUS`), the `System.IO.Hashing` `XxHash128.HashToUInt128` content key the one fixture identity, never a second hashing scheme.

```csharp signature
public sealed record BimWire(
    ReleaseVersion Schema,
    ModelView View,
    UInt128 ContentKey,
    Seq<BimWire.ElementWire> Elements,
    Seq<AssemblyRel> Relationships,
    double Tolerance,
    Instant At) {
    public sealed record ElementWire(
        string GlobalId,
        IfcClass Class,
        string Name,
        string GeometryKey,
        Seq<BimElement.PropertyBinding> Properties,
        Seq<BimElement.QuantityBinding> Quantities,
        Seq<ClassificationRef> Classifications,
        Option<string> SpatialContainerId);

    public static Fin<byte[]> Encode(BimModel model, UInt128 contentKey) => Encode(BimWireFace.Snapshot, model, contentKey);

    public static Fin<byte[]> Encode(BimWireFace face, BimModel model, UInt128 contentKey) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(Faced(face, model, contentKey), BimWireOptions.Json)).Run()
            .MapFail(static error => new BimFault.ModelRejected($"wire-encode:{face.Key}:{error.Message}").ToError());

    public static Fin<BimWire> Decode(ReadOnlyMemory<byte> json) =>
        Try.lift(() => JsonSerializer.Deserialize<BimWire>(json.Span, BimWireOptions.Json)!).Run()
            .MapFail(static error => new BimFault.ModelRejected($"wire-decode:{error.Message}").ToError());

    static object Faced(BimWireFace face, BimModel model, UInt128 contentKey) => face.Switch(
        snapshot: () => (object)Project(model, contentKey),
        opLog:    () => new OpLogWire(model.Schema, model.View, contentKey, ModelDiff.Between(BimModel.Empty, model).Changes, model.At),
        grpc:     () => new BimWireDescriptor(model.Schema, model.View, contentKey, model.Elements.Count, model.Types.Count, model.At));

    static BimWire Project(BimModel model, UInt128 contentKey) =>
        new(model.Schema, model.View, contentKey,
            model.Elements.Map(static e => new ElementWire(
                e.GlobalId, e.Class, e.Name, e.Geometry.Key, e.Properties, e.Quantities, e.Classifications, e.SpatialContainerId)),
            Seq<AssemblyRel>(), model.Tolerance, model.At);
}

[SmartEnum<string>]
public sealed partial class BimWireFace {
    public static readonly BimWireFace Snapshot = new("snapshot");
    public static readonly BimWireFace OpLog    = new("op-log");
    public static readonly BimWireFace Grpc     = new("grpc");
}

public sealed record OpLogWire(ReleaseVersion Schema, ModelView View, UInt128 ContentKey, Seq<ElementChange> Changes, Instant At);

public sealed record BimWireDescriptor(ReleaseVersion Schema, ModelView View, UInt128 ContentKey, int ElementCount, int TypeCount, Instant At);

public sealed record WireFixture(string WireType, UInt128 GoldenKey, int ByteCount) {
    public static WireFixture Of(string wireType, ReadOnlySpan<byte> wire) =>
        new(wireType, XxHash128.HashToUInt128(wire), wire.Length);

    public bool Matches(ReadOnlySpan<byte> wire) => XxHash128.HashToUInt128(wire) == GoldenKey && wire.Length == ByteCount;
}

public static class BimWireOptions {
    public static readonly JsonSerializerOptions Json = Compose();

    static JsonSerializerOptions Compose() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = BimWireContext.Default,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.Converters.Add(new ThinktectureJsonConverterFactory());
        options.MakeReadOnly();
        return options;
    }
}

[JsonSerializable(typeof(BimWire))]
[JsonSerializable(typeof(BimWire.ElementWire))]
[JsonSerializable(typeof(AssemblyRel))]
[JsonSerializable(typeof(ElementPredicate))]
[JsonSerializable(typeof(BimElement.PropertyBinding))]
[JsonSerializable(typeof(BimElement.QuantityBinding))]
[JsonSerializable(typeof(ClassificationRef))]
[JsonSerializable(typeof(BcfWire))]
[JsonSerializable(typeof(BcfTopicWire))]
[JsonSerializable(typeof(BcfCommentWire))]
[JsonSerializable(typeof(BcfViewpointWire))]
[JsonSerializable(typeof(DiffWire))]
[JsonSerializable(typeof(ElementChange))]
[JsonSerializable(typeof(OpLogWire))]
[JsonSerializable(typeof(BimWireDescriptor))]
[JsonSerializable(typeof(IdsAudit))]
[JsonSerializable(typeof(IdsAudit.FacetVerdict))]
[JsonSerializable(typeof(IdsFacet))]
public sealed partial class BimWireContext : JsonSerializerContext;
```

## [3]-[RESEARCH]

- [GENERATED_OWNER_CONVERTERS]: the `Thinktecture.Runtime.Extensions.Json` `ThinktectureJsonConverterFactory` parameterless registration admits every metadata-bearing owner (`IfcClass`/`Classification` `[SmartEnum<string>]`, `ProjectedCrs`/`ClassificationCode` `[ValueObject<string>]`) so the key codec and the static `Validate` rail run inside the converter — the factory `CanConvert`/`CreateConverter` selection routes the string-keyed smart enums through `ThinktectureJsonConverter<T, TValidationError>` and the value-objects through their keyed converter, confirmed against the catalogued factory surface; the `ElementPredicate`/`AssemblyRel` closed unions are wire-capable through the per-leaf `[JsonDerivedType]` polymorphic discriminator the generated `[Union]` carries, whose source-generated `[JsonDerivedType]` emission confirms against the Thinktecture union JSON surface before the union wire projection is final.
- [SNAPSHOT_CONTENT_KEY]: the `BimWire.ContentKey` joins the snapshot to the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` so a peer reads one content-addressed payload; the content-key derivation public signature confirms against the Compute owner at cross-folder alignment, and the `JsonSerializerContext` source-generated metadata over the `Seq<T>`/`Option<T>`/`Map<K,V>` LanguageExt collection shapes grounds against the `System.Text.Json` source-generator's custom-converter resolution for the immutable collections at the codec admission gate.
- [PEER_DECODE]: the Python `python:geometry/ifc-companion` ifcopenshell peer and the TypeScript web peer decode the `BimWire` vocabulary at the boundary — the peer-side key/case discriminant decode mirrors the C# generated owners' wire form so a peer never re-mints a parallel BIM shape; the cross-runtime wire-shape parity grounds at the cross-`libs/` synthesis tier where the Python and TypeScript peer decoders align to this C# wire owner.
- [THREE_FACES_FIXTURE]: the `BimWireFace` three-face fold (`Snapshot`/`OpLog`/`Grpc`) emits one sealed model through the one `BimWireContext` — the op-log face projects the `Review/diff#MODEL_DIFF` `ModelDiff.Between(BimModel.Empty, model)` `ElementChange` rows and the gRPC face the `BimWireDescriptor`, so the `lib:ONE_MODEL_THREE_FACES` committed big idea folds as additive `[JsonSerializable]` rows, never a second wire owner; the `WireFixture` `XxHash128`-keyed golden-byte corpus (the eight wire types: `BimWire`/`ElementWire` plus `BcfWire`/`DiffWire`/`OpLogWire`/`BimWireDescriptor`/`IdsAudit`) the C# branch host-validates is the `lib:ONE_WIRE_FIXTURE_CORPUS` partially-authorable-now corpus, the `System.IO.Hashing` `XxHash128.HashToUInt128` content key catalogued at `.api/api-hashing`; the cross-runtime golden parity (the Python/TypeScript peers must reproduce the same `XxHash128` golden bytes) grounds at the cross-`libs/` synthesis tier where the peer encoders align to this C# fixture corpus.
