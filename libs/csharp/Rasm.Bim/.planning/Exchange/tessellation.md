# [BIM_TESSELLATION_BRIDGE]

`TessellationRequest` crosses imported IFC/STEP/IGES/native geometry to the IfcOpenShell companion — `IfcConvert` producing GLB — and re-imports that GLB through the `import#IMPORT_RAIL` glTF path, minting the `TessellationOutcome` receipt over the dual content keys, the mesh evidence, the origin, and the monotonic latency. This bridge owns the cache-before-cross-and-store-before-return policy over two injected ports, so this AEC-DOMAIN owner mints no `Rasm.Persistence` or `Rasm.Compute` reference, depends strictly upward, and stays HOST-LOCAL.

Two injected ports carry the policy: the content-addressed `ITessellationStore` over `Rasm.Persistence/Store` and the `ITessellationCompanion` cross over `Rasm.Compute/Runtime/tiles#TWO_HOP_TESSELLATION`. Composed as settled vocabulary: the `format#FORMAT_AXIS` `TessellationRequiresCompanion` gate, the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` content key, the `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` transport, the kernel `Rasm/Domain/validation#CAPABILITY` capability set the geometry flags ride, the kernel `Rasm/Domain/rails#REDRIVE` `RedrivePolicy` the cross publishes, and the `import#IMPORT_RAIL` `ImportedGeometry` re-entry. `import#EXPLICIT_TESSELLATION` owns the in-process decode of an ALREADY-tessellated IFC face set, so this page holds no decode fence and names that arm by anchor.

## [01]-[INDEX]

- [02]-[TESSELLATION_BRIDGE]: imported IFC/STEP/IGES geometry crossing to the companion rail and re-importing as GLB, the `GeomSetting` capability vocabulary, the published re-drive cadence, and the dual-key `TessellationOutcome` receipt.

## [02]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` crosses imported geometry to the IfcOpenShell companion and re-imports its GLB through the `import#IMPORT_RAIL` glTF path; `TessellationOutcome` the typed receipt, `ITessellationStore`/`ITessellationCompanion` the injected ports the app-platform binds, `TessellationScope`/`TessellationSettings`/`GeomSetting`/`TessellationOrigin` the request vocabulary. This bridge owns the cache-before-cross-and-store-before-return POLICY and the managed half of its governance; companion transport and durable GLB residence are bound port implementations carrying BCL `CancellationToken` currency, never types this AEC-DOMAIN owner mints. In-flight progress is absent from both port contracts because the two-hop transport publishes no progress channel — a sink there has no feeder.
- Law: geometry evaluation flags are ONE `CapabilitySet<GeomSetting>` over the ifcopenshell `geom.settings` key space, not a bool block. Two of the six are REQUIRED by their consumers — `ApplyDefaultMaterials` for the glTF serialisation and `UseElementGuids` for the per-element metadata join `export#TILE_METADATA` reads — so `Of` demands them through the kernel's ONE refusal door and the refuse arm names WHICH capabilities were missing rather than restating the demand. NAMED LOSS: per-flag compile-time presence; bought back by that demanded set at construction. Second named loss: `disable-opening-subtractions` is a NEGATIVELY named foreign key, so its row keeps that key while its NAME states what presence means — the preimage bit is `Has(KeepOpenings)`, byte-identical to the retired `.Bool(DisableOpeningSubtractions)` write, so no stored GLB re-keys and no inversion column is owed.
- Law: the preimage is the ROSTER's own declaration order. `Write` folds `Items` rather than spelling six calls, so the promise that a new geometry knob is one more write in declaration order holds STRUCTURALLY rather than by hand.
- Entry: `Plan` gates on `source.TessellationRequiresCompanion` and mints the dual-key request, defaulting `settings`/`scope` to `TessellationSettings.Canonical`/`TessellationScope.Whole` so the whole-model canonical case is one call; `Resolve` reads the content-addressed store by the request's `Address` BEFORE the companion cross — a hit re-imports the cached GLB (`TessellationOrigin.Cached`, no round-trip), a miss crosses the companion over `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS`, stores the fresh GLB write-blob-first, and re-imports it (`TessellationOrigin.Tessellated`).
- Auto: dual keys separate concerns. `SourceKey` is the PURE, tolerance-independent source identity — the cross-projection join the GLB row and the IFC-semantic-graph row of one source share, holding whatever deflection a tessellation runs at. `ContentKey` folds that `SourceKey` with every GLB-affecting dimension (the `InterchangePolicy` deflection/tolerance/angle and the order-stable settings and scope), so two tessellations differing only in a weld flag, a deflection, or an element filter never collide on the store address while source identity stays pure. `Address` is the `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` value object minted off the `ContentKey` and the `InterchangeFormat.Glb` row — the ONE object-plane grammar owner, so the store address a lookup, a store write, and the receipt carry is admitted rather than rendered here.
- Receipt: the in-flight fraction is the COMPANION's alone — it runs the work out of process — and this owner publishes none rather than fabricating a stage ratio for a store lookup, while the token is read at every boundary the policy owns so an abandoned request never pays for the next long leg. `TessellationOutcome` is typed tessellation evidence on the `Fin<T>` rail, never a generic `IReceipt`/ledger. One outcome shape surfaces both a cached reuse and a fresh cross, so a caller reads `TessellationOrigin` from the receipt rather than a side channel. Receipt columns carry coordinates, counts, and hashes — never payload bytes, the GLB riding the `ITessellationStore` port.
- Packages: Generator.Equals, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Rasm, Rasm.Element
- Growth: a new tessellation flag is one `GeomSetting` row carrying its own ifcopenshell key — the preimage fold, the request, and the companion mapping are untouched; a new non-flag parameter is one `TessellationSettings` column folded into `ContentKey`; a new scope modality is one `TessellationScope` case with one `--include`/`--exclude` mapping; a new companion-evaluated source format is one `InterchangeFormat` row carrying `TessellationRequiresCompanion=true`. Each extension is a row, case, or column on an existing owner, never a second bridge or in-process tessellator.
- Boundary: this companion bridge is the single imported-geometry-to-GLB path for anything requiring an evaluator — GeometryGym and the in-process `import#IMPORT_RAIL` `StepReader` carry no tessellation kernel, no in-process arm64 solid evaluator is admitted, and `IfcConvert`/ifcopenshell stays the permanent default because the only .NET web-ifc binding is Windows C++/CLI; an ALREADY-tessellated IFC face set never reaches here at all, decoding in process at `import#EXPLICIT_TESSELLATION` and handing its `Deferred` GlobalId set straight to `TessellationScope.Elements` so the cross narrows to the residue. Both content keys derive from the kernel `ContentHash` and the kernel `CanonicalWriter` (`Rasm/Domain/identity#CONTENT_KEY`), so this AEC-DOMAIN owner mints no `Rasm.Compute.InterchangeIdentity` call. `SourceKey` is the cross-projection join the app-platform artifact-index projection owns, so the IFC semantic graph and the tessellated geometry stay two projections of one content-keyed source. Store failures retain their original `Error`; a non-companion source format is the terminal `Refused`/`BimReason.Capability` verdict, while a failed companion cross is `BoundaryFailed(BimBoundary.TessellationCompanion, error)` and inherits the boundary's transient posture; direct or caught cancellation retains its kernel identity. RE-DRIVE is PUBLISHED, never executed: the root-bound executor reads that posture and drives the one declared cadence; a tier-local `Redrive.Run` here forks the estate's one schedule. Governance rides the PORT CONTRACT in BCL currency alone: the token is a `System` type the binding already speaks, so the out-of-process companion carries it into its own transport without this owner minting a cancellation type; an `IProgress<double>` beside it is the SINK WITH NO FEEDER the port equally forecloses, because the transport publishes no progress channel in either direction and a managed progress lane lands as a transport row FIRST. `import#IMPORT_RAIL` `FrameNormalization` coerces the glTF-canonical Y-up GLB to the kernel Z-up frame by the `InterchangeFormat.Glb` row, so this page mints no frame transform. This bridge reaches the `python:geometry/ifc-companion` IfcOpenShell package only through Compute's companion rpc, which owns the `geom.settings` argument mapping, the `IfcConvert` filter grammar, and the GLB stream-back.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Linq;
using System.Threading;
using GeneratorEquals;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Bim.Model;                       // BimFault and its compact scope/reason/boundary axes
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// ifcopenshell `dimensionality` value (CURVES=0, SURFACES_AND_SOLIDS=1 default, CURVES_SURFACES_AND_SOLIDS=2);
// byte value IS the companion key, so names track ifcopenshell — no solids-only mode.
public enum Dimensionality : byte { Curves = 0, SurfacesAndSolids = 1, CurvesSurfacesAndSolids = 2 }

// The ifcopenshell geom.settings key space as a capability vocabulary: the KEY is the foreign token the companion
// maps and the NAME is what PRESENCE means here. KeepOpenings carries the negatively-named foreign key
// `disable-opening-subtractions`, so its presence writes that flag true and the preimage bit is unchanged.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeomSetting : ICapability<GeomSetting> {
    public static readonly GeomSetting Weld = new("weld-vertices");
    public static readonly GeomSetting WorldCoords = new("use-world-coords");
    public static readonly GeomSetting DefaultMaterials = new("apply-default-materials");
    public static readonly GeomSetting GenerateUvs = new("generate-uvs");
    public static readonly GeomSetting KeepOpenings = new("disable-opening-subtractions");
    public static readonly GeomSetting ElementGuids = new("use-element-guids");

    // The two capabilities a legal corner ALWAYS holds: a GLB with no material serialises nothing a viewer can
    // shade, and a GLB whose nodes carry no GlobalId joins to no element metadata.
    public static readonly CapabilitySet<GeomSetting> Required =
        CapabilitySet<GeomSetting>.Of(DefaultMaterials, ElementGuids);
}

// Tessellation scope is the IfcConvert geometry filter: whole model, an explicit GlobalId set (--include attribute
// GlobalId, the per-element modality), an entity-type set to keep (--include entities), or one to drop (--exclude
// entities — IfcSpace/IfcOpeningElement/IfcAnnotation off the tessellation, the dominant IFC-to-GLB cull). Case IS
// the modality; a layer filter or an exclude-by-GlobalId set is one further case, never a filter-mode flag.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TessellationScope {
    private TessellationScope() { }

    public sealed record WholeModel : TessellationScope;
    public sealed record Elements(Seq<string> GlobalIds) : TessellationScope;
    public sealed record Entities(Seq<string> IfcTypes) : TessellationScope;
    public sealed record ExcludeEntities(Seq<string> IfcTypes) : TessellationScope;

    public static readonly TessellationScope Whole = new WholeModel();

    // Order-stable ContentKey contribution written STRAIGHT onto the one seam CanonicalWriter — a case ordinal, then
    // the count-prefixed ordered token run — so {a,b} and {b,a} key identically, include/exclude polarity rides the
    // ordinal, and no delimiter-joined intermediate string re-mints a second canonicalization scheme.
    public CanonicalWriter Write(CanonicalWriter w) =>
        Switch(
            state: w,
            wholeModel:      static (writer, _) => writer.Ordinal(0),
            elements:        static (writer, e) => Tokens(writer.Ordinal(1), e.GlobalIds),
            entities:        static (writer, e) => Tokens(writer.Ordinal(2), e.IfcTypes),
            excludeEntities: static (writer, e) => Tokens(writer.Ordinal(3), e.IfcTypes));

    static CanonicalWriter Tokens(CanonicalWriter w, Seq<string> raw) {
        var ordered = raw.OrderBy(static t => t, StringComparer.Ordinal).ToSeq();
        return ordered.Fold(w.Ordinal(ordered.Count), static (writer, token) => writer.String(token));
    }
}

[SmartEnum<string>]
public sealed partial class TessellationOrigin {
    public static readonly TessellationOrigin Cached = new("cached");
    public static readonly TessellationOrigin Tessellated = new("tessellated");
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record TessellationSettings {
    private TessellationSettings(CapabilitySet<GeomSetting> flags, Dimensionality dimensionality) =>
        (Flags, Dimensionality) = (flags, dimensionality);

    public CapabilitySet<GeomSetting> Flags { get; }
    public Dimensionality Dimensionality { get; }

    public static readonly TessellationSettings Canonical = new(
        CapabilitySet<GeomSetting>.Of(GeomSetting.Weld, GeomSetting.WorldCoords,
            GeomSetting.DefaultMaterials, GeomSetting.ElementGuids),
        Dimensionality.SurfacesAndSolids);

    // ONE refusal door: Require hands the refuse arm the MISSING set, so a caller learns which capabilities its
    // corner lacked instead of reading a bare label. A legal-corner law enumerating sixteen of sixty-four corners
    // would restate exactly this one demanded subset.
    public static Fin<TessellationSettings> Of(CapabilitySet<GeomSetting> flags, Dimensionality dimensionality, Op key) =>
        flags.Require(GeomSetting.Required, missing => new BimFault.Refused(key, BimScope.Tessellation, BimReason.Capability, string.Join(':', new object?[] { "tessellation-settings-incomplete", missing.Wire })))
            .Map(admitted => new TessellationSettings(admitted, dimensionality));

    // ContentKey contribution on the one seam writer: one bit per roster row in DECLARATION order, then the
    // dimensionality ordinal. Folding Items is what makes the declaration-order promise structural.
    public CanonicalWriter Write(CanonicalWriter w) =>
        GeomSetting.Items.Fold(w, (writer, row) => writer.Bool(Flags.Admits(row))).Ordinal((byte)Dimensionality);
}

// Mesh evidence (VertexCount/TriangleCount/GlbByteCount) rides typed receipt columns readable without the Geometry
// payload or the store-resident GLB; Took is the monotonic timestamp/elapsed pair over the whole Resolve.
public sealed record TessellationOutcome(
    ImportedGeometry Geometry,
    ArtifactKey Address,
    UInt128 ContentKey,
    UInt128 SourceKey,
    double Deflection,
    int VertexCount,
    int TriangleCount,
    long GlbByteCount,
    TessellationOrigin Origin,
    Duration Took,
    Instant At);

// SourceBytes under synthesized record equality compares the HANDLE — reference, offset, length — so two
// byte-identical requests would read UNEQUAL; the column seats the package's ONE ReadOnlyMemory<byte> comparer,
// span-equal over the payload and hashing through the same kernel digest the dual keys already mint.
[Equatable]
public sealed partial record TessellationRequest(
    UInt128 SourceKey,
    UInt128 ContentKey,
    InterchangeFormat Source,
    [property: CustomEquality(typeof(WireBytes))] ReadOnlyMemory<byte> SourceBytes,
    InterchangePolicy Policy,
    TessellationSettings Settings,
    TessellationScope Scope) {

    // The PUBLISHED re-drive cadence for the companion hop — a jittered exponential curve under a bound, declared as
    // ONE policy VALUE the root-bound executor drives. A tier-local Redrive.Run here would fork the estate's one
    // schedule, so this owner classifies (the unreachable cross publishes Transient below) and executes nothing.
    public static readonly RedrivePolicy CompanionRedrive = RedrivePolicy.Of(
        (Schedule.exponential(Duration.FromMilliseconds(250)) | Schedule.jitter(0.1)) & Schedule.spaced(Duration.FromSeconds(20)),
        bound: 4);

    public static Fin<TessellationRequest> Plan(
        InterchangeFormat source, ReadOnlyMemory<byte> sourceBytes, InterchangePolicy policy, Op key,
        Option<TessellationSettings> settings = default, Option<TessellationScope> scope = default) =>
        source.TessellationRequiresCompanion
            ? Fin.Succ(Keyed(source, sourceBytes, policy, settings.IfNone(TessellationSettings.Canonical), scope.IfNone(TessellationScope.Whole)))
            : Fin.Fail<TessellationRequest>(new BimFault.Refused(key, BimScope.Tessellation, BimReason.Capability, string.Join(':', new object?[] { "tessellation-not-required", source.Key })));

    // SourceKey is the PURE source-artifact identity — the kernel seed-zero content-hash over the source key and the
    // raw bytes, NO tolerances — so the GLB projection and the IFC-semantic-graph projection of one source re-derive
    // the identical join key independent of the deflection any tessellation runs at.
    static TessellationRequest Keyed(InterchangeFormat source, ReadOnlyMemory<byte> sourceBytes, InterchangePolicy policy, TessellationSettings settings, TessellationScope scope) {
        UInt128 sourceKey = ContentHash.Of((source, sourceBytes), static (s, writer) => writer.String(s.source.Key).Raw(s.sourceBytes.Span));
        return new(sourceKey, Fold(sourceKey, policy, settings, scope), source, sourceBytes, policy, settings, scope);
    }

    // Store addresses MINT through the grammar owner off the two facts each IS, so this bridge holds no separator
    // position, hex width, or format token of its own and a grammar change lands in exactly one place.
    public ArtifactKey Address => ArtifactKey.Of(ContentKey, InterchangeFormat.Glb);

    // Cache-before-cross-and-store-before-return. TWO time parameters BY KERNEL LAW (Parametric/projections
    // MonotonicTimeline Boundary): no joint invariant binds a wall instant to a monotonic mark, so a carrier fusing
    // IClock with the timeline is REFUSED — the clock gives the receipt's Instant, the timeline the monotonic leg,
    // and each answers its own kernel owner. A raw TimeProvider mark/elapsed pair is the deleted form.
    public Fin<TessellationOutcome> Resolve(
        ITessellationStore store, ITessellationCompanion companion,
        CancellationToken cancel, IClock clock, MonotonicTimeline timeline, Op key) {
        var at = clock.GetCurrentInstant();
        return timeline.Capture(key).Bind(mark => Live(cancel)
            .Bind(_ => store.Lookup(Address))
            .Bind(hit => hit.Match(
                Some: glb => Live(cancel).Bind(_ => Reenter(glb, TessellationOrigin.Cached, clock, timeline, at, mark, key)),
                // Store writes are the RELEASE half of the cross's bracket, so they run on the ABANDONMENT path too:
                // a caller who walked away still paid for the companion round trip, and discarding the GLB there
                // makes the next Resolve pay it again. Cancellation is read AFTER the write, so an abandoned request
                // lands its cache and then stops.
                None: () => Live(cancel)
                    .Bind(_ => Crossed(companion, cancel, key))
                    .Bind(glb => store.Store(Address, glb)
                        .Bind(_ => Live(cancel))
                        .Bind(_ => Reenter(glb, TessellationOrigin.Tessellated, clock, timeline, at, mark, key))))));
    }

    // The classification half of the re-drive contract: the companion boundary owns its TRANSIENT posture, the
    // returned Error remains its cause, and cancellation keeps the kernel identity established by the caller token.
    // The port is the declared companion boundary, so either its returned refusal or a captured invocation throw
    // becomes the same cause-bearing case; neither is re-rendered or mistaken for cancellation.
    Fin<ReadOnlyMemory<byte>> Crossed(ITessellationCompanion companion, CancellationToken cancel, Op key) =>
        key.Catch(() => companion.Cross(this, cancel), cancel)
            .MapFail(error => error is KernelFault.Cancelled || error.Is(Errors.Cancelled)
                ? error
                : (Error)new BimFault.BoundaryFailed(BimBoundary.TessellationCompanion, error));

    static Fin<Unit> Live(CancellationToken cancel) =>
        cancel.IsCancellationRequested ? Fin.Fail<Unit>(Errors.Cancelled) : Fin.Succ(unit);

    Fin<TessellationOutcome> Reenter(ReadOnlyMemory<byte> glb, TessellationOrigin origin, IClock clock, MonotonicTimeline timeline, Instant at, MonotonicStamp mark, Op key) =>
        BimIo.ImportGeometry(InterchangeFormat.Glb, glb, clock, key)
            .Bind(geometry => Sound(geometry, key).ToFin())
            .Bind(geometry => timeline.Elapsed(mark, key).Map(elapsed => new TessellationOutcome(
                geometry, Address, ContentKey, SourceKey, Policy.Deflection,
                geometry.VertexCount, geometry.TriangleCount, glb.Length, origin,
                elapsed.ToDuration(), at)));

    // Three INDEPENDENT claims accumulate, so a caller learns every way a re-imported GLB is unsound rather than the
    // first: emptiness, the arena's own validity claim, and coordinate finiteness. The arena's IsValid already proves
    // descriptor/payload agreement and a lossless witness, so the third conjunct adds only the check IsValid does not
    // make, reading the position lane through its typed view rather than a raw span. The boundary-validation scan is
    // the named statement exemption.
    static Validation<Error, ImportedGeometry> Sound(ImportedGeometry geometry, Op key) =>
        (Claim(geometry is { VertexCount: > 0, TriangleCount: > 0 }, key, "empty"),
         Claim(geometry.Lanes.IsValid, key, "arena-invalid"),
         Claim(Finite(geometry), key, "non-finite-coordinate"))
        .Apply(static (_, _, _) => geometry).As();

    static Validation<Error, Unit> Claim(bool held, Op key, string witness) =>
        held
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new GeometryFault.DegenerateInput(
                Kind.Mesh, None, string.Join(':', new object?[] { "tessellation-degenerate", witness })));

    static bool Finite(ImportedGeometry geometry) =>
        geometry.Lanes.Descriptors.Find(static d => d.Channel == EncodingChannel.Position).Match(
            Some: descriptor => {
                float[] raw = new float[descriptor.Floats];
                descriptor.Dtype.Unpack(geometry.Lanes.Channel(EncodingChannel.Position).Span, raw);
                return raw.All(static coordinate => float.IsFinite(coordinate));
            },
            None: static () => false);

    // ContentKey folds the PURE SourceKey with EVERY GLB-affecting dimension — the InterchangePolicy
    // deflection/tolerance/angle AND the order-stable config — each written STRAIGHT onto the one kernel seed-zero
    // fold through its own Write, so the store address partitions on every input that changes the GLB while
    // SourceKey stays the tolerance-independent join.
    static UInt128 Fold(UInt128 sourceKey, InterchangePolicy policy, TessellationSettings settings, TessellationScope scope) =>
        ContentAddress.Of((sourceKey, policy, settings, scope), 0.0, static (s, writer) =>
            s.scope.Write(s.settings.Write(writer
                .U128(s.sourceKey)
                .Double(s.policy.Deflection).Double(s.policy.Tolerance).Double(s.policy.AngleTolerance)))).Value;
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Injected ports the app-platform binds at the composition edge. Bim mints no Persistence or Compute reference; it
// owns the cache/cross/store POLICY through these contracts while durable residence and transport are the bound
// implementations. Lookup separates a store fault (Fin failure) from a normal miss (None); Store is the
// write-blob-first put.
public interface ITessellationStore {
    Fin<Option<ReadOnlyMemory<byte>>> Lookup(ArtifactKey address);
    Fin<Unit> Store(ArtifactKey address, ReadOnlyMemory<byte> glb);
}

// Crosses carry their governance on the contract in BCL currency: the token the transport threads, and nothing else.
// A Bim-minted lane or cancellation type crossing here is the AEC-DOMAIN leak the port exists to foreclose.
public interface ITessellationCompanion {
    Fin<ReadOnlyMemory<byte>> Cross(TessellationRequest request, CancellationToken cancel);
}
```

## [03]-[RESEARCH]

(none)
