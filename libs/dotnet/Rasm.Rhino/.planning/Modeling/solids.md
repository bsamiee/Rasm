# [RASM_RHINO_MODELING_SOLIDS]

`Rasm.Rhino.Modeling` owns Brep solid construction. `SolidOp` carries booleans, edge treatments, offset, shell, pipe, seeding, tapered extrusion, editing, and the `Extrusion` lifecycle through `Solids.Build`. `ModelGate` borrows leased `GeometryHandle` inputs and owns fresh natives. Native statics return command-fidelity geometry; intersections, mass properties, bounds, contours, and analysis remain kernel-owned. Every arm reads tolerance through `Context`.

## [01]-[INDEX]

- [02]-[MODEL_GATE]: `ModelGate`, `ModelRuntime`, `BenchBand`, `BenchEvidence` — the folder spine, its effect runtime, and its benchmark observation.
- [03]-[POLICY_FAMILY]: `CapEnd`, `ShapeGrant`, `OffsetGrant`, `SolidBooleanLaw`, `PlanarBooleanLaw`, `ArcDegree`, `ArcSlider`, `FilletShape`, `FilletLaw`, `FilletDegree`, `HigherFilletDegree`, `SectionFilletProfile`, `RadiusLaw`, `EdgeFillet`, `SectionFilletLaw`, `MatchLaw`, `MatchCapability`, `MatchRefinement`, `PipeLaw`, `SolidSeed`, `ExtrusionSeed`, `ExtrusionRead`, `SolidEdit`, `MergeSurfaceLaw`, `TrimCutter`, `ConnectSeed` — the grant vocabularies and the construction policies.
- [04]-[OPERATION_PIPELINE]: `SolidOp` and the `Solids.Build` entry.

## [02]-[MODEL_GATE]

- Owner: `ModelGate` — the one custody kernel under every Modeling arm: `Borrow` projects a live native of the demanded kind out of a leased handle, `BorrowMany` sequences borrow windows over a handle spread, `Own`/`OwnMany`/`OwnEach` mint owned leases for fresh natives, `Folded` is the batch fold, and `Entry` is the one operation spine every Modeling page runs; `ModelRuntime` carries the regime, cancellation token, and the two progress reporters a `ProgressLease` produces.
- Law: `Entry` discriminates on the operation carrier alone — the `ReadOnlySpan<TOp>` entry materializes and delegates to the `Seq<TOp>` core, and a runtime-bound page enters at the core because a span cannot cross the `Eff.runtime<TRuntime>()` lambda that binds its runtime; admission is ALWAYS accumulating, so every page reports the whole rejection set and no page re-mints a fold with abort-on-first semantics nothing asked for.
- Law: minting is spine-owned — `Single` and `Many` mint one-product and spread results, `Kept` and `Owned`/`OwnedMany` close duplicate-edit custody, `Staged` enumerates and owns every harvest inside one guarded custody scope, `Detached` disposes host-returned originals after duplication, and `Entry` accumulates every operation refusal before dispatch; failure-arm custody release rides kernel `Custody.Rollback` at every fold.
- Law: release is fallible and never masks — kernel `Custody.Dispose`/`Rollback` own reverse-order, all-attempted cleanup and append every refusal onto the primary; `Staged` folds each admitted product roster through that kernel, so no local sweep, unwind, or disposer projection survives.
- Law: a construction result is an acquisition, never a crossing — `Own` mints native results directly, `OwnMany` refuses null and undeclared empty spreads, optional secondary stages admit empty explicitly, and a mid-spread failure disposes every handle already minted.
- Law: the batch fold is failure-symmetric — `Folded` concatenates owned products and releases every product accumulated by earlier operations the moment a later operation faults, so a batch never half-leaks custody.
- Law: `Borrow` is the type gate — a handle whose live native is not the demanded kind refuses through `Unsupported` with both types named, so no arm ever pattern-matches raw geometry beyond its own dispatch.
- Law: `BenchBand.Measured` brackets an observed operation through the injected session `MonotonicTimeline`, reads thread allocation, and converts synchronous runner exceptions through `Try.lift`; `BenchEvidence` carries the capture pipeline's benchmark shape while the corpus gate owns aggregation and thresholds.
- Growth: a new custody modality is one `ModelGate` member; a new bench dimension is one `BenchEvidence` field; sibling pages add zero spine surface.
- Packages: kernel `Domain/results` (`Fault`, `Fin`, `ValidityClaim`, `Custody`, `Try.lift`/`Confirm`/`Need`), kernel `Domain/context` (`Context`, `Tolerance`), kernel `Parametric/projections` (`MonotonicTimeline`), `Rasm.Rhino.Document` (`GeometryHandle`, `Lease<T>`, `CrossingMode`, `GeometryCrossing`), `Modeling/curves.md` (`ModelClaim`), RhinoCommon (`Rhino.Runtime.HostUtils`, `Rhino.Geometry` — `.api/api-rhinocommon-solids.md`, `.api/api-rhinocommon-geometry.md`), LanguageExt.Core (`Seq`, `FoldM`, `Traverse`, `Validation`), Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`, `[ComplexValueObject]` — `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Rasm.Domain;
using Rasm.Parametric;
using Rasm.Rhino.Document;
using Rhino.Geometry;
using Rhino.Runtime;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ModelRuntime {
    public Context Domain { get; }
    public CancellationToken Cancellation { get; }
    public Option<IProgress<int>> IntegerProgress { get; }
    public Option<IProgress<double>> ScalarProgress { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Context domain,
        ref CancellationToken cancellation,
        ref Option<IProgress<int>> integerProgress,
        ref Option<IProgress<double>> scalarProgress) {
        if (domain is null) {
            validationError = new ValidationError("Model runtime requires a domain context.");
        }
    }

    public static implicit operator Context(ModelRuntime runtime) => runtime.Domain;

    internal IProgress<int>? IntegerReporter => IntegerProgress.ValueUnsafe();

    internal IProgress<double>? ScalarReporter => ScalarProgress.ValueUnsafe();

    internal Fin<TOut> Await<TOut>(Func<Task<TOut>> work) => Try.lift(() => {
        Task<TOut> running = work();
        running.Wait(Cancellation);
        return Fin.Succ(running.GetAwaiter().GetResult());
    }).Run().Bind(static inner => inner);
}

public sealed record ProcessFingerprint(
    string Process, Version Version, bool PreRelease,
    System.Runtime.InteropServices.Architecture Architecture, int Processors);

public sealed record BenchEvidence(
    string Operation, long InputScale, TimeSpan Duration, long AllocatedBytes, bool Succeeded, ProcessFingerprint Host) {
    internal string Band => string.Create(
        CultureInfo.InvariantCulture,
        $"{Operation} scale={InputScale} duration={Duration.TotalSeconds:R}s allocated={AllocatedBytes}B");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BenchBand {
    private static readonly Lazy<ProcessFingerprint> Fingerprint = new(static () => {
        HostUtils.GetCurrentProcessInfo(processName: out string process, processVersion: out Version version);
        return new ProcessFingerprint(
            Process: process,
            Version: version,
            PreRelease: HostUtils.IsPreRelease,
            Architecture: System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
            Processors: HostUtils.GetSystemProcessorCount());
    });

    public static Fin<(Fin<T> Outcome, BenchEvidence Evidence)> Measured<T>(
        MonotonicTimeline timeline, string operation, long inputScale, Func<Fin<T>> run) {
        return from clock in Admit.Need(timeline)
               from opened in clock.Capture()
               let allocated = GC.GetAllocatedBytesForCurrentThread()
               let outcome = Try.lift(run).Run().Bind(static inner => inner)
               from closed in clock.Capture()
               from duration in clock.Elapsed(start: opened, end: closed)
               select (outcome, new BenchEvidence(
                   Operation: operation,
                   InputScale: inputScale,
                   Duration: duration,
                   AllocatedBytes: GC.GetAllocatedBytesForCurrentThread() - allocated,
                   Succeeded: outcome.IsSucc,
                   Host: Fingerprint.Value));
    }
}

internal static class ModelGate {
    internal static Fin<TResult> Borrow<TNative, TResult>(GeometryHandle handle, Func<TNative, Fin<TResult>> body)
        where TNative : GeometryBase =>
        Admit.Need(handle).Bind(active => active.With(project: geometry => Optional(geometry as TNative)
                .ToFin(Fail: new KernelFault.Unsupported(InputType: geometry.GetType(), OutputType: typeof(TNative)))
                .Bind(body)));

    internal static Fin<TResult> BorrowMany<TNative, TResult>(
        Seq<GeometryHandle> handles, Func<Seq<TNative>, Fin<TResult>> body, bool allowEmpty = false)
        where TNative : GeometryBase =>
        handles.IsEmpty && !allowEmpty
            ? Fin.Fail<TResult>(error: new KernelFault.InvalidInput())
            : Nested(handles: handles, borrowed: Seq<TNative>(), body: body);

    internal static Fin<GeometryHandle> Own(GeometryBase? built) =>
        Optional(built).ToFin(Fail: new KernelFault.InvalidResult())
            .Map(fresh => new GeometryHandle(lease: new Lease<GeometryBase>.Owned(Value: fresh), mode: CrossingMode.Detach));

    internal static Fin<Seq<GeometryHandle>> OwnMany(IEnumerable<GeometryBase>? built, bool allowEmpty = false) =>
        Optional(built).Map(static values => toSeq(values)).ToFin(Fail: new KernelFault.InvalidResult())
            .Bind(fresh => fresh.IsEmpty && !allowEmpty
                ? Fin.Fail<Seq<GeometryHandle>>(error: new KernelFault.InvalidResult())
                : fresh.FoldM<Fin, Seq<GeometryHandle>>(Seq<GeometryHandle>(), (held, value) =>
                    Own(built: value)
                        .Map(handle => held.Add(value: handle))
                        .Rollback(release: () => Custody.Dispose(held: held))));

    internal static Fin<Seq<GeometryHandle>> OwnEach<TSource>(
        Seq<TSource> sources, Func<TSource, GeometryBase?> run, bool allowEmpty = false) =>
        sources.IsEmpty && !allowEmpty
            ? Fin.Fail<Seq<GeometryHandle>>(error: new KernelFault.InvalidResult())
            : sources.FoldM<Fin, Seq<GeometryHandle>>(Seq<GeometryHandle>(), (held, source) =>
                Try.lift(() => Own(built: run(source))).Run().Bind(static inner => inner)
                    .Map(handle => held.Add(value: handle))
                    .Rollback([.. held]));

    internal static Fin<Seq<GeometryHandle>> Folded<TOp>(
        Context context, Seq<TOp> operations, Func<TOp, Context, Fin<Seq<GeometryHandle>>> apply) =>
        operations.FoldM<Fin, Seq<GeometryHandle>>(Seq<GeometryHandle>(), (held, operation) =>
            apply(operation, context)
                .Map(next => held + next)
                .Rollback([.. held]));

    internal static Fin<Seq<GeometryHandle>> Entry<TOp>(
        ModelRuntime runtime, Seq<TOp> operations,
        Func<TOp, Fin<TOp>> admit,
        Func<TOp, Context, Fin<Seq<GeometryHandle>>> apply) {
        return from domain in Optional(runtime.Domain).ToFin(Fail: new KernelFault.MissingContext())
               from _ in guard(!operations.IsEmpty, new KernelFault.InvalidInput())
               from admitted in operations
                   .Traverse(operation => Admit.Need(operation)
                       .Bind(active => admit(active, op))
                       .ToValidation())
                   .As()
                   .ToFin()
               from products in Folded(context: domain, operations: admitted, apply: apply)
               select products;
    }

    internal static Fin<Seq<GeometryHandle>> Entry<TOp>(
        ModelRuntime runtime, ReadOnlySpan<TOp> operations,
        Func<TOp, Fin<TOp>> admit,
        Func<TOp, Context, Fin<Seq<GeometryHandle>>> apply) =>
        Entry(runtime: runtime, operations: toSeq(operations.ToArray()), admit: admit, apply: apply);

    internal static Fin<Seq<GeometryHandle>> Single(Func<GeometryBase?> run, CancellationToken token = default) =>
        Try.lift(() => Own(built: run()).Map(Seq)).Run().Bind(static inner => inner);

    internal static Fin<Seq<GeometryHandle>> Many(Func<System.Collections.Generic.IEnumerable<GeometryBase>?> run,
        bool allowEmpty = false, CancellationToken token = default) =>
        Try.lift(() => OwnMany(built: run(), allowEmpty: allowEmpty)).Run().Bind(static inner => inner);

    internal static Fin<Seq<GeometryHandle>> Kept(GeometryBase working) =>
        Own(built: working).Map(Seq);

    internal static Fin<Seq<GeometryHandle>> Owned(GeometryBase working, Func<GeometryBase?> run) =>
        Try.lift(() => Own(built: run())).Run().Bind(static inner => inner).Bind(owned =>
            Relinquished(working: working, built: Seq(owned)));

    internal static Fin<Seq<GeometryHandle>> OwnedMany(GeometryBase working, Func<IEnumerable<GeometryBase>?> run,
        bool allowEmpty = false) =>
        Try.lift(() => OwnMany(built: run(), allowEmpty: allowEmpty)).Run().Bind(static inner => inner).Bind(owned =>
            Relinquished(working: working, built: owned));

    internal static Fin<GeometryBase> Detached(GeometryBase? source) {
        GeometryBase? detached = null;
        Fin<GeometryBase> copied = from active in Optional(source).ToFin(Fail: new KernelFault.InvalidResult())
                                   from copy in Try.lift(() => Optional(active.Duplicate())
                                       .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
                                   select (detached = copy)!;
        return copied
            .Settled(
                release: () => Custody.Dispose(held: Optional(source).ToSeq()))
            .Rollback(
                release: () => Custody.Dispose(held: Optional(detached).ToSeq()));
    }

    internal static Fin<Seq<GeometryBase>> DetachedMany(IEnumerable<GeometryBase>? source) =>
        Optional(source).ToFin(Fail: new KernelFault.InvalidResult()).Bind(rows => toSeq(rows).FoldM<Fin, Seq<GeometryBase>>(
            Seq<GeometryBase>(),
            (held, row) => Detached(source: row)
                .Map(held.Add)
                .Rollback(release: () => Custody.Dispose(held: held))));

    internal static Fin<Seq<GeometryHandle>> Staged(params ReadOnlySpan<(System.Collections.Generic.IEnumerable<GeometryBase>? Values, bool AllowEmpty)> stages) =>
        StageOwned(success: Option<bool>.None, stages: stages.ToArray());

    internal static Fin<Seq<GeometryHandle>> Staged(bool success,
        params ReadOnlySpan<(System.Collections.Generic.IEnumerable<GeometryBase>? Values, bool AllowEmpty)> stages) =>
        StageOwned(success: Some(success), stages: stages.ToArray());

    private static Fin<Seq<GeometryHandle>> StageOwned(Option<bool> success,
        (System.Collections.Generic.IEnumerable<GeometryBase>? Values, bool AllowEmpty)[] stages) {
        Fin<Seq<Seq<GeometryHandle>>> captured = toSeq(stages).FoldM<Fin, Seq<Seq<GeometryHandle>>>(
            Seq<Seq<GeometryHandle>>(),
            (held, stage) => Try.lift(() => OwnMany(
                    built: stage.Values ?? Seq<GeometryBase>().AsEnumerable(), allowEmpty: stage.AllowEmpty)).Run().Bind(static inner => inner)
                .Map(held.Add)
                .Rollback(
                    release: () => Custody.Dispose(
                        held: held.Bind(static products => products))));
        return from rows in captured
               let products = rows.Bind(static stage => stage)
               from _ in success
                   .TraverseM(verdict => Admit.Confirm(success: verdict))
                   .As()
                   .Map(static _ => unit)
                   .Rollback(release: () => Custody.Dispose(held: products))
               select products;
    }

    private static Fin<Seq<GeometryHandle>> Relinquished(GeometryBase working, Seq<GeometryHandle> built) =>
        Try.lift(() => {
            working.Dispose();
            return Fin.Succ(value: built);
        }).Run().Bind(static inner => inner).Rollback([.. built]);

    private static Fin<TResult> Nested<TNative, TResult>(
        Seq<GeometryHandle> handles, Seq<TNative> borrowed, Func<Seq<TNative>, Fin<TResult>> body)
        where TNative : GeometryBase =>
        handles.Head.Case switch {
            GeometryHandle head => Borrow<TNative, TResult>(handle: head,
                body: native => Nested(handles: handles.Tail, borrowed: borrowed.Add(value: native), body: body)),
            _ => body(arg: borrowed),
        };
}
```

## [03]-[POLICY_FAMILY]

- Owner: `CapEnd`, `ShapeGrant`, and `OffsetGrant` are the page's three capability vocabularies; `SolidBooleanLaw` and `PlanarBooleanLaw` carry only the source arity and manifold policy consumed by each native boolean; `ArcDegree` and `ArcSlider` own the folder's non-rational arc approximation degree and slider bands; `FilletShape` closes the four `Brep.FilletSurfaceSettings` profile factories and `FilletLaw` binds one profile to its grant column; `SectionFilletProfile` closes the verified `SurfaceFilletBase` section family; `EdgeFillet` pairs an edge index with a constant or parameter-profiled radius law; `MatchLaw` carries the complete `MatchSrfSettings` policy; `PipeLaw` closes thin/thick constant and variable profiles; `SolidSeed` and `ExtrusionSeed` close heavy and lightweight construction; `ExtrusionRead` closes lightweight projections; `SolidEdit`, `TrimCutter`, and `ConnectSeed` close value-semantic editing.
- Law: `ShapeGrant`, `OffsetGrant`, and `CapEnd` carry adjacent host booleans as ordinal-key `CapabilitySet` columns read by name at each native; independent booleans stay on their owning cases.
- Law: `CapEnd` is the folder's ONE cap vocabulary — the Brep cylinder, cone, and revolve seeds, both lightweight extrusion seeds, and the mesh pipeline's cylinder seed all name the same two sweep ends, so `Modeling/meshing.md` composes this roster and its own four-row `MeshCaps` mirror is deleted; `CapabilityLaw` carries each native's reachable corners, and the cone bars `Upper` because `Brep.CreateFromCone` publishes `capBottom` alone.
- Law: the fillet profile is the settings factory and the grant column is its sibling — every `Brep[]`-returning fillet/chamfer overload is obsolete, so `FilletLaw.Rig` is the only site naming `CreateRationalArcSettings`/`CreateNonRationalSettings`/`CreateG2BlendSettings`/`CreateChamferSettings`, the tolerance slot reads the regime, and `ContinueAcrossTangentFaces` is written once off the grant set. NAMED LOSS: the twelve per-case defaults the four profiles carried are gone and a caller now declares the grant set at construction; bought back by one reading of `Trim`/`Extend`/`AcrossTangents` for the whole family and by the four-arm `Switch` that existed only to re-read `AcrossTangents` off every case disappearing entirely.
- Law: `SolidBooleanLaw`, `PlanarBooleanLaw`, and the curve boolean owner preserve their native operand arities; `Split` omits the manifold column published by no `Brep.CreateBooleanSplit` overload.
- Law: the non-rational arc approximation has one folder vocabulary — `ArcDegree` rows the host's declared 3/4/5 degree space and `ArcSlider` admits the control-point displacement band once, so `CreateNonRationalSettings` and the curve pipeline's `CreateNonRationalArcBezier` read the same two owners and no arm carries a bare degree int or an unbounded slider double. Every native slider consumer — both fillet families and the arc bezier — refuses nothing and clamps to [-0.9, 0.9] silently, negatives producing distinct real geometry; the factories assign the arguments through without validating, so admission is this layer's alone and `ArcSlider`'s band is the effective band, keeping every admitted value one the host honors verbatim.
- Law: section fillets generate the degree space — `FilletDegree` and `HigherFilletDegree` rows carry their native constructor delegates and select which `SurfaceFilletBase` static runs, a distinct axis from `ArcDegree`'s argument-valued degree; `NonRationalCubic` carries tangent alone and `NonRationalHigher` requires tangent with inner slider, so invalid degree-payload combinations and nested degree dispatch are absent.
- Law: `SectionFillet` and the surface-fillet arms return every owned fillet and trimmed surface in native result order.
- Law: parallel arrays are rows — an edge fillet enters as `(Edge, Law)` rows and the arm splits all-constant rows onto `CreateFilletEdges` and any-profiled rows onto `CreateFilletEdgesVariableRadius` with `BrepEdgeFilletDistance` rows minted per profile point, so equal-cardinality is proven by construction and the two native members stay one case.
- Law: `MatchLaw` collapses the host's split configuration — constructor continuities, combinable `MatchCapability` membership, and behavior-bearing `MatchRefinement` rig `EnableRefinement` once, so every policy has one native interpretation.
- Law: seeds carry no custody unless the source is geometry — analytic primitive cases hold value structs; `SolidSeed.CornerPoints` derives the triangular or quadrilateral native constructor from row cardinality; the surface, revolve, and mesh conversion cases hold leased handles borrowed only inside `Build`. `ExtrusionRead` projects the lightweight solid to brep, wireframe, detached cached mesh, station profile, or wall geometry.
- Law: admission is owner-local and evidence-shaped — every policy union and every policy value answers `IValidityEvidence.IsValid` off its generated `Switch` or the same fold its generated factory ran, so a new case breaks its owning owner's evidence at compile; the shape claims are the spine's `ModelClaim` and the kernel's `ValidityClaim` rows, never a page-local predicate class, and one `object?`-typed predicate switching over every policy type is the deleted form that let a new case pass unchecked.
- Law: `Rig` is a capability projection on the fault channel, NOT a `[Mapper]` transcription — the grant-column collapse consumed the field-for-field mirroring a Mapperly seat owns, so `FilletLaw.Rig` delegates the profile to `FilletShape.Bake`'s generated `Switch` over four host factories and writes one `Admits` read, and `MatchLaw.Rig` writes four of its five `MatchSrfSettings` slots off `MatchCapability` before handing the settings to `MatchRefinement.Apply`. Mapperly maps a declared source property onto a same-shaped target property on a pure signature and expresses neither the dispatch, the grant read, nor the `Fin` carrier, so a mapper seated here carries a hand-written body per slot and maps nothing; the folder's `[Mapper]` seats stay on the pages transcribing a foreign record field-for-field.
- Growth: a new profile is one `FilletShape` case; a new primitive is one `SolidSeed` case; a new edit verb is one `SolidEdit` case; a new grant is one row on its owning vocabulary — the pipeline and every consumer read them with zero new surface.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `Brep.FilletSurfaceSettings` `[01]` and its four factories `:84`, `Brep.CreateFilletSurface`/`CreateFilletSurfaceCurve` `:80-81`, `SurfaceFilletBase` section family, `Brep.CreateBoolean*`, `Brep.CreatePlanar*`, `Brep.CreatePipe`/`CreateThickPipe`, `Brep.CreateOffsetBrep`, `MatchSrfSettings`), RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `Box`, `Cone`, `Cylinder`, `Torus`, `Sphere`, `ComponentIndex`, `Continuity`, `BlendType`, `RailType`, `PipeCapMode`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`, `IValidityEvidence`), kernel `Domain/results` (`ValidityClaim`, `Fin`), kernel `Domain/context` (`Context`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CapEnd : ICapability<CapEnd> {
    public static readonly CapEnd Lower = new(key: "lower");
    public static readonly CapEnd Upper = new(key: "upper");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeGrant : ICapability<ShapeGrant> {
    public static readonly ShapeGrant Trim = new(key: "trim");
    public static readonly ShapeGrant Extend = new(key: "extend");
    public static readonly ShapeGrant AcrossTangents = new(key: "across-tangents");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OffsetGrant : ICapability<OffsetGrant> {
    public static readonly OffsetGrant Solid = new(key: "solid");
    public static readonly OffsetGrant Extend = new(key: "extend");
    public static readonly OffsetGrant Shrink = new(key: "shrink");
    public static readonly OffsetGrant BothSides = new(key: "both-sides");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidBooleanLaw : IValidityEvidence {
    private SolidBooleanLaw() { }
    public sealed record Union(Seq<GeometryHandle> Breps, bool ManifoldOnly) : SolidBooleanLaw;
    public sealed record Intersection(Seq<GeometryHandle> First, Seq<GeometryHandle> Second, bool ManifoldOnly) : SolidBooleanLaw;
    public sealed record Difference(Seq<GeometryHandle> First, Seq<GeometryHandle> Second, bool ManifoldOnly) : SolidBooleanLaw;
    public sealed record Split(Seq<GeometryHandle> First, Seq<GeometryHandle> Second) : SolidBooleanLaw;

    public bool IsValid => Switch(
        union: static law => ModelClaim.Handles(handles: law.Breps),
        intersection: static law => ValidityClaim.All(
            ModelClaim.Handles(handles: law.First), ModelClaim.Handles(handles: law.Second)),
        difference: static law => ValidityClaim.All(
            ModelClaim.Handles(handles: law.First), ModelClaim.Handles(handles: law.Second)),
        split: static law => ValidityClaim.All(
            ModelClaim.Handles(handles: law.First), ModelClaim.Handles(handles: law.Second)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanarBooleanLaw : IValidityEvidence {
    private PlanarBooleanLaw() { }
    public sealed record Union(Seq<GeometryHandle> Breps) : PlanarBooleanLaw;
    public sealed record Intersection(GeometryHandle First, GeometryHandle Second) : PlanarBooleanLaw;
    public sealed record Difference(GeometryHandle First, GeometryHandle Second) : PlanarBooleanLaw;

    public bool IsValid => Switch(
        union: static law => ModelClaim.Handles(handles: law.Breps),
        intersection: static law => ValidityClaim.All(
            ModelClaim.Handle(handle: law.First), ModelClaim.Handle(handle: law.Second)),
        difference: static law => ValidityClaim.All(
            ModelClaim.Handle(handle: law.First), ModelClaim.Handle(handle: law.Second)));
}

[SmartEnum<int>]
public sealed partial class ArcDegree {
    public static readonly ArcDegree Cubic = new(key: 3);
    public static readonly ArcDegree Quartic = new(key: 4);
    public static readonly ArcDegree Quintic = new(key: 5);
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct ArcSlider {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = ValidityClaim.All(ValidityClaim.Finite(value: value), value is >= -0.9 and <= 0.9)
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"ArcSlider must lie in [-0.9, 0.9] (got {value:R})."));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FilletShape : IValidityEvidence {
    private FilletShape() { }
    public sealed record RationalArc(double Radius) : FilletShape;
    public sealed record NonRational(double Radius, ArcDegree Degree, ArcSlider TanSlider, ArcSlider InnerSlider) : FilletShape;
    public sealed record G2Blend(double Radius) : FilletShape;
    public sealed record Chamfer(double Radius0, double Radius1) : FilletShape;

    public bool IsValid => Switch(
        rationalArc: static shape => ValidityClaim.Positive(value: shape.Radius),
        nonRational: static shape => ValidityClaim.All(
            ValidityClaim.Positive(value: shape.Radius), shape.Degree is not null),
        g2Blend: static shape => ValidityClaim.Positive(value: shape.Radius),
        chamfer: static shape => ValidityClaim.All(
            ValidityClaim.Positive(value: shape.Radius0), ValidityClaim.Positive(value: shape.Radius1)));

    internal Brep.FilletSurfaceSettings Bake(Context domain, CapabilitySet<ShapeGrant> grants) => Switch(
        context: (Domain: domain, Trim: grants.Admits(capability: ShapeGrant.Trim), Extend: grants.Admits(capability: ShapeGrant.Extend)),
        rationalArc: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateRationalArcSettings(
            radius: shape.Radius, tolerance: ctx.Domain.Absolute.Value, trim: ctx.Trim, extend: ctx.Extend),
        nonRational: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateNonRationalSettings(
            radius: shape.Radius, tolerance: ctx.Domain.Absolute.Value, degree: shape.Degree.Key,
            tanSlider: shape.TanSlider.Value, innerSlider: shape.InnerSlider.Value, trim: ctx.Trim, extend: ctx.Extend),
        g2Blend: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateG2BlendSettings(
            radius: shape.Radius, tolerance: ctx.Domain.Absolute.Value, trim: ctx.Trim, extend: ctx.Extend),
        chamfer: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateChamferSettings(
            radius0: shape.Radius0, radius1: shape.Radius1, tolerance: ctx.Domain.Absolute.Value, trim: ctx.Trim, extend: ctx.Extend));
}

[SmartEnum<int>]
public sealed partial class FilletDegree {
    public static readonly FilletDegree Cubic = new(key: 3, create: static (first, firstUv, second, secondUv, law, tolerance, trimmed0, trimmed1, fillets) =>
        SurfaceFilletBase.CreateNonRationalCubicArcsFilletSrf(
            first, firstUv, second, secondUv, law.Radius, tolerance,
            trimmed0, trimmed1, law.RailDegree, law.Trim, law.Extend, fillets));
    public static readonly FilletDegree Quartic = new(key: 4, create: static (first, firstUv, second, secondUv, law, tolerance, trimmed0, trimmed1, fillets) =>
        SurfaceFilletBase.CreateNonRationalQuarticArcsFilletSrf(
            first, firstUv, second, secondUv, law.Radius, tolerance,
            trimmed0, trimmed1, law.RailDegree, law.Trim, law.Extend, fillets));
    public static readonly FilletDegree Quintic = new(key: 5, create: static (first, firstUv, second, secondUv, law, tolerance, trimmed0, trimmed1, fillets) =>
        SurfaceFilletBase.CreateNonRationalQuinticArcsFilletSrf(
            first, firstUv, second, secondUv, law.Radius, tolerance,
            trimmed0, trimmed1, law.RailDegree, law.Trim, law.Extend, fillets));

    [UseDelegateFromConstructor]
    internal partial bool Create(
        BrepFace first, Point2d firstUv, BrepFace second, Point2d secondUv,
        SectionFilletLaw law, double tolerance,
        System.Collections.Generic.List<Brep> trimmed0,
        System.Collections.Generic.List<Brep> trimmed1,
        System.Collections.Generic.List<Brep> fillets);
}

[SmartEnum<int>]
public sealed partial class HigherFilletDegree {
    public static readonly HigherFilletDegree Quartic = new(key: 4, create: static (first, firstUv, second, secondUv, law, tangent, inner, tolerance, trimmed0, trimmed1, fillets) =>
        SurfaceFilletBase.CreateNonRationalQuarticFilletSrf(
            first, firstUv, second, secondUv, law.Radius, tolerance,
            trimmed0, trimmed1, law.RailDegree, tangent, inner, law.Trim, law.Extend, fillets));
    public static readonly HigherFilletDegree Quintic = new(key: 5, create: static (first, firstUv, second, secondUv, law, tangent, inner, tolerance, trimmed0, trimmed1, fillets) =>
        SurfaceFilletBase.CreateNonRationalQuinticFilletSrf(
            first, firstUv, second, secondUv, law.Radius, tolerance,
            trimmed0, trimmed1, law.RailDegree, tangent, inner, law.Trim, law.Extend, fillets));

    [UseDelegateFromConstructor]
    internal partial bool Create(
        BrepFace first, Point2d firstUv, BrepFace second, Point2d secondUv,
        SectionFilletLaw law, double tangent, double inner, double tolerance,
        System.Collections.Generic.List<Brep> trimmed0,
        System.Collections.Generic.List<Brep> trimmed1,
        System.Collections.Generic.List<Brep> fillets);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionFilletProfile : IValidityEvidence {
    private SectionFilletProfile() { }
    public sealed record RationalArcs : SectionFilletProfile;
    public sealed record NonRationalArcs(FilletDegree Degree) : SectionFilletProfile;
    public sealed record NonRationalCubic(ArcSlider TangentSlider) : SectionFilletProfile;
    public sealed record NonRationalHigher(HigherFilletDegree Degree, ArcSlider TangentSlider, ArcSlider InnerSlider) : SectionFilletProfile;
    public sealed record G2ChordalQuintic : SectionFilletProfile;

    public bool IsValid => Switch(
        rationalArcs: static () => (ValidityClaim)true,
        nonRationalArcs: static profile => (ValidityClaim)(profile.Degree is not null),
        nonRationalCubic: static () => (ValidityClaim)true,
        nonRationalHigher: static profile => (ValidityClaim)(profile.Degree is not null),
        g2ChordalQuintic: static () => (ValidityClaim)true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RadiusLaw : IValidityEvidence {
    private RadiusLaw() { }
    public sealed record Constant(double Start, double End) : RadiusLaw;
    public sealed record Profiled(Seq<(double Parameter, double Distance)> Rows) : RadiusLaw;

    public bool IsValid => Switch(
        constant: static law => ValidityClaim.All(
            ValidityClaim.Positive(value: law.Start), ValidityClaim.Positive(value: law.End)),
        profiled: static law => ModelClaim.Rows(rows: law.Rows, claim: static point => ValidityClaim.All(
            ValidityClaim.Finite(value: point.Parameter), ValidityClaim.Positive(value: point.Distance))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PipeLaw : IValidityEvidence {
    private PipeLaw() { }
    public sealed record Constant(double Radius) : PipeLaw;
    public sealed record Variable(Seq<(double Parameter, double Radius)> Rows) : PipeLaw;
    public sealed record Thick(double Radius0, double Radius1) : PipeLaw;
    public sealed record ThickVariable(Seq<(double Parameter, double Inner, double Outer)> Rows) : PipeLaw;

    public bool IsValid => Switch(
        constant: static law => ValidityClaim.Positive(value: law.Radius),
        variable: static law => ModelClaim.Rows(rows: law.Rows, claim: static point => ValidityClaim.All(
            ValidityClaim.Finite(value: point.Parameter), ValidityClaim.Positive(value: point.Radius))),
        thick: static law => ValidityClaim.All(
            ValidityClaim.Positive(value: law.Radius0), ValidityClaim.Positive(value: law.Radius1)),
        thickVariable: static law => ModelClaim.Rows(rows: law.Rows, claim: static point => ValidityClaim.All(
            ValidityClaim.Finite(value: point.Parameter), ValidityClaim.Positive(value: point.Inner),
            ValidityClaim.Positive(value: point.Outer), point.Inner < point.Outer)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TrimCutter : IValidityEvidence {
    private TrimCutter() { }
    public sealed record ByBrep(GeometryHandle Cutter) : TrimCutter;
    public sealed record ByPlane(Plane Cutter) : TrimCutter;

    public bool IsValid => Switch(
        byBrep: static cutter => ModelClaim.Handle(handle: cutter.Cutter),
        byPlane: static cutter => (ValidityClaim)cutter.Cutter.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConnectSeed : IValidityEvidence {
    private ConnectSeed() { }
    public sealed record AtEdges(int FirstEdge, int SecondEdge) : ConnectSeed;
    public sealed record AtPoints(Point3d First, Point3d Second) : ConnectSeed;

    public bool IsValid => Switch(
        atEdges: static at => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: at.FirstEdge, floor: 0),
            ValidityClaim.CountAtLeast(count: at.SecondEdge, floor: 0)),
        atPoints: static at => ValidityClaim.All(
            ValidityClaim.Finite(value: at.First), ValidityClaim.Finite(value: at.Second)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MergeSurfaceLaw : IValidityEvidence {
    private MergeSurfaceLaw() { }
    public sealed record Plain : MergeSurfaceLaw;
    public sealed record AtPoints(Point2d First, Point2d Second, double Roundness, bool Smooth) : MergeSurfaceLaw;

    public bool IsValid => Switch(
        plain: static () => (ValidityClaim)true,
        atPoints: static law => ValidityClaim.All(
            law.First.IsValid, law.Second.IsValid, ValidityClaim.Finite(value: law.Roundness)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidSeed : IValidityEvidence {
    private SolidSeed() { }
    public sealed record OfBox(Box Value) : SolidSeed;
    public sealed record OfBounds(BoundingBox Value) : SolidSeed;
    public sealed record OfCorners(
        Point3d A, Point3d B, Point3d C, Point3d D,
        Point3d E, Point3d F, Point3d G, Point3d H) : SolidSeed;
    public sealed record OfCylinder(Cylinder Value, CapabilitySet<CapEnd> Caps) : SolidSeed;
    public sealed record OfCone(Cone Value, CapabilitySet<CapEnd> Caps) : SolidSeed;
    public sealed record OfTorus(Torus Value) : SolidSeed;
    public sealed record OfSphere(Sphere Value) : SolidSeed;
    public sealed record QuadSphere(Sphere Value) : SolidSeed;
    public sealed record Baseball(Point3d Center, double Radius) : SolidSeed;
    public sealed record CornerPoints(Seq<Point3d> Values) : SolidSeed;
    public sealed record FromSurface(GeometryHandle Source) : SolidSeed;
    public sealed record FromRevolve(GeometryHandle Source, CapabilitySet<CapEnd> Caps) : SolidSeed;
    public sealed record FromMesh(GeometryHandle Source, bool TrimmedTriangles = true) : SolidSeed;

    private static readonly CapabilityLaw<CapEnd> ConeCaps =
        CapabilityLaw<CapEnd>.Forbidden(barred: Seq(CapabilitySet<CapEnd>.Of(CapEnd.Upper)));

    public bool IsValid => Switch(
        ofBox: static seed => (ValidityClaim)seed.Value.IsValid,
        ofBounds: static seed => (ValidityClaim)seed.Value.IsValid,
        ofCorners: static seed => ModelClaim.Points(
            points: Seq(seed.A, seed.B, seed.C, seed.D, seed.E, seed.F, seed.G, seed.H)),
        ofCylinder: static seed => (ValidityClaim)seed.Value.IsValid,
        ofCone: static seed => ValidityClaim.All(seed.Value.IsValid, ConeCaps.Admit(held: seed.Caps).IsSucc),
        ofTorus: static seed => (ValidityClaim)seed.Value.IsValid,
        ofSphere: static seed => (ValidityClaim)seed.Value.IsValid,
        quadSphere: static seed => (ValidityClaim)seed.Value.IsValid,
        baseball: static seed => ValidityClaim.All(
            ValidityClaim.Finite(value: seed.Center), ValidityClaim.Positive(value: seed.Radius)),
        cornerPoints: static seed => ValidityClaim.All(
            seed.Values.Count is 3 or 4, ModelClaim.Points(points: seed.Values)),
        fromSurface: static seed => ModelClaim.Handle(handle: seed.Source),
        fromRevolve: static seed => ModelClaim.Handle(handle: seed.Source),
        fromMesh: static seed => ModelClaim.Handle(handle: seed.Source));

    internal Fin<GeometryHandle> Build(Context domain) =>
        Switch(
            context: domain,
            ofBox: static (ctx, seed) => Try.lift(() => ModelGate.Own(built: Brep.CreateFromBox(box: seed.Value))).Run().Bind(static inner => inner),
            ofBounds: static (ctx, seed) => Try.lift(() => ModelGate.Own(built: Brep.CreateFromBox(box: seed.Value))).Run().Bind(static inner => inner),
            ofCorners: static (ctx, seed) => Try.lift(() => ModelGate.Own(
                built: Brep.CreateFromBox(corners: Seq(seed.A, seed.B, seed.C, seed.D, seed.E, seed.F, seed.G, seed.H).AsIterable()))).Run().Bind(static inner => inner),
            ofCylinder: static (ctx, seed) => Try.lift(() => ModelGate.Own(
                built: Brep.CreateFromCylinder(
                    cylinder: seed.Value,
                    capBottom: seed.Caps.Admits(capability: CapEnd.Lower),
                    capTop: seed.Caps.Admits(capability: CapEnd.Upper)))).Run().Bind(static inner => inner),
            ofCone: static (ctx, seed) => Try.lift(() => ModelGate.Own(
                built: Brep.CreateFromCone(cone: seed.Value, capBottom: seed.Caps.Admits(capability: CapEnd.Lower)))).Run().Bind(static inner => inner),
            ofTorus: static (ctx, seed) => Try.lift(() => ModelGate.Own(built: Brep.CreateFromTorus(torus: seed.Value))).Run().Bind(static inner => inner),
            ofSphere: static (ctx, seed) => Try.lift(() => ModelGate.Own(built: Brep.CreateFromSphere(sphere: seed.Value))).Run().Bind(static inner => inner),
            quadSphere: static (ctx, seed) => Try.lift(() => ModelGate.Own(built: Brep.CreateQuadSphere(sphere: seed.Value))).Run().Bind(static inner => inner),
            baseball: static (ctx, seed) => Try.lift(() => ModelGate.Own(
                built: Brep.CreateBaseballSphere(center: seed.Center, radius: seed.Radius, tolerance: ctx.Absolute.Value))).Run().Bind(static inner => inner),
            cornerPoints: static (ctx, seed) => Try.lift(() => ModelGate.Own(
                built: seed.Values.Count switch {
                    3 => Brep.CreateFromCornerPoints(
                        corner1: seed.Values[0], corner2: seed.Values[1], corner3: seed.Values[2],
                        tolerance: ctx.Absolute.Value),
                    4 => Brep.CreateFromCornerPoints(
                        corner1: seed.Values[0], corner2: seed.Values[1], corner3: seed.Values[2], corner4: seed.Values[3],
                        tolerance: ctx.Absolute.Value),
                    _ => null,
                })).Run().Bind(static inner => inner),
            fromSurface: static (ctx, seed) => ModelGate.Borrow<Surface, GeometryHandle>(handle: seed.Source,
                body: surface => Try.lift(() => ModelGate.Own(built: Brep.CreateFromSurface(surface: surface))).Run().Bind(static inner => inner)),
            fromRevolve: static (ctx, seed) => ModelGate.Borrow<RevSurface, GeometryHandle>(handle: seed.Source,
                body: surface => Try.lift(() => ModelGate.Own(
                    built: Brep.CreateFromRevSurface(
                        surface: surface,
                        capStart: seed.Caps.Admits(capability: CapEnd.Lower),
                        capEnd: seed.Caps.Admits(capability: CapEnd.Upper)))).Run().Bind(static inner => inner)),
            fromMesh: static (ctx, seed) => ModelGate.Borrow<Mesh, GeometryHandle>(handle: seed.Source,
                body: mesh => Try.lift(() => ModelGate.Own(built: Brep.CreateFromMesh(mesh: mesh, trimmedTriangles: seed.TrimmedTriangles))).Run().Bind(static inner => inner)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrusionSeed : IValidityEvidence {
    private ExtrusionSeed() { }
    public sealed record Profile(GeometryHandle PlanarProfile, double Height, bool Cap = true) : ExtrusionSeed;
    public sealed record FramedProfile(GeometryHandle PlanarProfile, Plane Frame, double Height, bool Cap = true) : ExtrusionSeed;
    public sealed record OfBox(Box Value, bool Cap = true) : ExtrusionSeed;
    public sealed record OfCylinder(Cylinder Value, CapabilitySet<CapEnd> Caps) : ExtrusionSeed;
    public sealed record OfPipe(Cylinder Value, double OtherRadius, CapabilitySet<CapEnd> Caps) : ExtrusionSeed;

    public bool IsValid => Switch(
        profile: static seed => ValidityClaim.All(
            ModelClaim.Handle(handle: seed.PlanarProfile),
            ValidityClaim.Finite(value: seed.Height), seed.Height != 0.0),
        framedProfile: static seed => ValidityClaim.All(
            ModelClaim.Handle(handle: seed.PlanarProfile), seed.Frame.IsValid,
            ValidityClaim.Finite(value: seed.Height), seed.Height != 0.0),
        ofBox: static seed => (ValidityClaim)seed.Value.IsValid,
        ofCylinder: static seed => (ValidityClaim)seed.Value.IsValid,
        ofPipe: static seed => ValidityClaim.All(
            seed.Value.IsValid, ValidityClaim.Positive(value: seed.OtherRadius)));

    internal Fin<GeometryHandle> Build() =>
        Switch(
            profile: static (seed) => ModelGate.Borrow<Curve, GeometryHandle>(handle: seed.PlanarProfile,
                body: profile => Try.lift(() => ModelGate.Own(
                    built: Extrusion.Create(planarCurve: profile, height: seed.Height, cap: seed.Cap))).Run().Bind(static inner => inner)),
            framedProfile: static (seed) => ModelGate.Borrow<Curve, GeometryHandle>(handle: seed.PlanarProfile,
                body: profile => Try.lift(() => ModelGate.Own(
                    built: Extrusion.Create(curve: profile, plane: seed.Frame, height: seed.Height, cap: seed.Cap))).Run().Bind(static inner => inner)),
            ofBox: static (seed) => Try.lift(() => ModelGate.Own(built: Extrusion.CreateBoxExtrusion(box: seed.Value, cap: seed.Cap))).Run().Bind(static inner => inner),
            ofCylinder: static (seed) => Try.lift(() => ModelGate.Own(
                built: Extrusion.CreateCylinderExtrusion(
                    cylinder: seed.Value,
                    capBottom: seed.Caps.Admits(capability: CapEnd.Lower),
                    capTop: seed.Caps.Admits(capability: CapEnd.Upper)))).Run().Bind(static inner => inner),
            ofPipe: static (seed) => Try.lift(() => ModelGate.Own(
                built: Extrusion.CreatePipeExtrusion(
                    cylinder: seed.Value, otherRadius: seed.OtherRadius,
                    capTop: seed.Caps.Admits(capability: CapEnd.Upper),
                    capBottom: seed.Caps.Admits(capability: CapEnd.Lower)))).Run().Bind(static inner => inner));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrusionRead : IValidityEvidence {
    private ExtrusionRead() { }
    public sealed record Heavy(bool SplitKinkyFaces = true) : ExtrusionRead;
    public sealed record Wireframe : ExtrusionRead;
    public sealed record Mesh(MeshType Kind) : ExtrusionRead;
    public sealed record Profile(int Index, double Station) : ExtrusionRead;
    public sealed record WallEdge(ComponentIndex Component) : ExtrusionRead;
    public sealed record WallSurface(ComponentIndex Component) : ExtrusionRead;

    public bool IsValid => Switch(
        heavy: static _ => (ValidityClaim)true,
        wireframe: static () => (ValidityClaim)true,
        mesh: static read => (ValidityClaim)Enum.IsDefined(read.Kind),
        profile: static read => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: read.Index, floor: 0), ValidityClaim.Finite(value: read.Station)),
        wallEdge: static read => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: read.Component.Index, floor: 0),
            Enum.IsDefined(read.Component.ComponentIndexType)),
        wallSurface: static read => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: read.Component.Index, floor: 0),
            Enum.IsDefined(read.Component.ComponentIndexType)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidEdit : IValidityEvidence {
    private SolidEdit() { }
    public sealed record Cap : SolidEdit;
    public sealed record JoinNaked : SolidEdit;
    public sealed record MergeCoplanar : SolidEdit;
    public sealed record MergeFace(int Face) : SolidEdit;
    public sealed record MergeFacePair(int First, int Second) : SolidEdit;
    public sealed record UnjoinEdges(Seq<int> Edges) : SolidEdit;
    public sealed record RemoveHoles(Seq<ComponentIndex> Loops) : SolidEdit;
    public sealed record RemoveFins : SolidEdit;
    public sealed record CullFaces : SolidEdit;
    public sealed record Repair : SolidEdit;
    public sealed record Reseam(int Face, ParametricAxis Axis, double Parameter) : SolidEdit;

    public bool IsValid => Switch(
        cap: static () => (ValidityClaim)true,
        joinNaked: static () => (ValidityClaim)true,
        mergeCoplanar: static () => (ValidityClaim)true,
        mergeFace: static edit => ValidityClaim.CountAtLeast(count: edit.Face, floor: 0),
        mergeFacePair: static edit => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: edit.First, floor: 0),
            ValidityClaim.CountAtLeast(count: edit.Second, floor: 0), edit.First != edit.Second),
        unjoinEdges: static edit => ModelClaim.Rows(
            rows: edit.Edges, claim: static edge => ValidityClaim.CountAtLeast(count: edge, floor: 0)),
        removeHoles: static edit => ModelClaim.Rows(
            rows: edit.Loops, claim: static loop => ValidityClaim.CountAtLeast(count: loop.Index, floor: 0)),
        removeFins: static () => (ValidityClaim)true,
        cullFaces: static () => (ValidityClaim)true,
        repair: static () => (ValidityClaim)true,
        reseam: static edit => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: edit.Face, floor: 0), edit.Axis is not null,
            ValidityClaim.Finite(value: edit.Parameter)));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct EdgeFillet(int Edge, RadiusLaw Law) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Edge, floor: 0), ValidityClaim.Evidence(evidence: Optional(Law)));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct FilletLaw : IValidityEvidence {
    public FilletShape Shape { get; }
    public CapabilitySet<ShapeGrant> Grants { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FilletShape shape,
        ref CapabilitySet<ShapeGrant> grants) {
        if (shape is not { IsValid: true }) {
            validationError = new ValidationError("Fillet policy requires an admitted profile shape.");
        }
    }

    public bool IsValid => Shape is { IsValid: true };

    internal Fin<Brep.FilletSurfaceSettings> Rig(Context domain) =>
        Try.lift(() => {
            Brep.FilletSurfaceSettings settings = Shape.Bake(domain: domain, grants: Grants);
            settings.ContinueAcrossTangentFaces = Grants.Admits(capability: ShapeGrant.AcrossTangents);
            return Fin.Succ(value: settings);
        }).Run().Bind(static inner => inner);
}

public sealed record SectionFilletLaw(
    double Radius,
    int RailDegree,
    SectionFilletProfile Profile,
    CapabilitySet<ShapeGrant> Grants) : IValidityEvidence {
    private static readonly CapabilityLaw<ShapeGrant> SectionGrants =
        CapabilityLaw<ShapeGrant>.Forbidden(barred: Seq(CapabilitySet<ShapeGrant>.Of(ShapeGrant.AcrossTangents)));

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: Radius),
        ValidityClaim.CountAtLeast(count: RailDegree, floor: 1),
        ValidityClaim.Evidence(evidence: Optional(Profile)),
        SectionGrants.Admit(held: Grants).IsSucc);

    internal bool Trim => Grants.Admits(capability: ShapeGrant.Trim);

    internal bool Extend => Grants.Admits(capability: ShapeGrant.Extend);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MatchCapability : ICapability<MatchCapability> {
    public static readonly MatchCapability Average = new(key: "average");
    public static readonly MatchCapability ClosestPoints = new(key: "closest-points");
    public static readonly MatchCapability ReverseMatch = new(key: "reverse-match");
    public static readonly MatchCapability ReverseTarget = new(key: "reverse-target");
}

[SmartEnum<int>]
public sealed partial class MatchRefinement {
    public static readonly MatchRefinement None = new(key: 0, apply: static (settings, _) => {
        settings.EnableRefinement(enabled: false);
        return unit;
    });
    public static readonly MatchRefinement Contextual = new(key: 1, apply: static (settings, domain) => {
        settings.EnableRefinement(
            enabled: true,
            positionalTolerance: domain.Absolute.Value,
            angleToleranceRadians: domain.Angle.Value,
            curvatureTolerance: domain.Fractional);
        return unit;
    });

    [UseDelegateFromConstructor]
    internal partial Unit Apply(MatchSrfSettings settings, Context domain);
}

public sealed record MatchLaw(
    Continuity Match,
    Continuity OtherEnd,
    CapabilitySet<MatchCapability> Capabilities,
    MatchRefinement Refinement,
    PreserveIsoCurveMethod PreserveIso = PreserveIsoCurveMethod.Automatic) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Enum.IsDefined(Match), Enum.IsDefined(OtherEnd), Refinement is not null, Enum.IsDefined(PreserveIso));

    internal Fin<MatchSrfSettings> Rig(Context domain) =>
        Try.lift(() => {
            MatchSrfSettings settings = new(match: Match, otherEnd: OtherEnd) {
                Average = Capabilities.Admits(capability: MatchCapability.Average),
                MatchClosestPoints = Capabilities.Admits(capability: MatchCapability.ClosestPoints),
                PreserveIso = PreserveIso,
                ReverseMatchDirection = Capabilities.Admits(capability: MatchCapability.ReverseMatch),
                ReverseAverageTargetDirection = Capabilities.Admits(capability: MatchCapability.ReverseTarget),
            };
            _ = Refinement.Apply(settings: settings, domain: domain);
            return Fin.Succ(value: settings);
        }).Run().Bind(static inner => inner);
}
```

## [04]-[OPERATION_PIPELINE]

- Owner: `SolidOp` `[Union]` — the whole verified solid-construction verb roster; `Solids` — the one entry folding any operation spread into owned geometry handles.
- Law: every construction arm returns the owned geometry produced by RhinoCommon directly; native diagnostics and correspondence outputs stay local when no caller consumes them.
- Law: the pipeline is value-semantic — `Edit` duplicates its borrowed brep, runs the in-place host member on the working copy, and owns the copy (or the member's returned brep, disposing the copy) as the product; no operation mutates the geometry behind an input handle.
- Law: boolean payload shape is admission — `SolidBooleanLaw` carries each set topology and manifold grant, and `PlanarBooleanLaw` carries union spread or exact pair.
- Law: `Solids.Build` admits the complete operation spread before dispatch and every refusal NAMES its axis — `SolidOp.Admitted` dispatches through the generated total `Switch` into `ModelClaim.Admits`, so a request breaching four constraints answers four `KernelFault.InvalidInput(Key, Axis)` errors on one carrier, a new case breaks the compile instead of falling through a catch-all, and live collection bounds remain inside their borrow windows.
- Law: `Brep.CreateBlendSurface` returns an edge-interval spread, `Brep.CreateBlendShape` returns one parameter-selected surface, and `PairPosture` owns reversal for both cases.
- Law: face-addressed operations index inside the borrow — a case carries the handle with face or edge indices, the arm guards the index against the live `Faces`/`Edges` count, and no `BrepFace` or `BrepEdge` ever crosses a case payload.
- Boundary: sweep, loft, and patch construction is the lofting page's pipeline; freeform surface and curve construction is the surfaces and curves pages'; this pipeline owns the solid-topology verbs alone.
- Boundary: this pipeline is unpaced because no Brep construction member consumes cancellation or progress; `ModelRuntime` supplies the shared context while meshing, lofting, and projection consume their pacing columns.
- Growth: a new host solid verb is one case with its arm; the spine and every consumer read it with zero new surface.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — the `Brep` boolean, fillet, blend, offset, shell, pipe, join, merge, match, split, trim, and extrusion rosters `:44-160`), RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `Extrusion`, `ComponentIndex`, `MeshType`, `ExtrudeCornerType`), kernel `Domain/results` (`KernelFault.InvalidInput(Key, Axis)`, `Fin`), kernel `Domain/validation` (`CapabilitySet`), kernel `Domain/context` (`Context`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidOp {
    private SolidOp() { }
    public sealed record Boolean(SolidBooleanLaw Law) : SolidOp;
    public sealed record PlanarBoolean(Plane Plane, PlanarBooleanLaw Law) : SolidOp;
    public sealed record Solidify(Seq<GeometryHandle> Open) : SolidOp;
    public sealed record FilletEdges(GeometryHandle Target, Seq<EdgeFillet> Edges, BlendType Blend, RailType Rail, bool Setback = false) : SolidOp;
    public sealed record FaceFillet(GeometryHandle First, int FirstFace, Point2d FirstUv, GeometryHandle Second, int SecondFace, Point2d SecondUv, FilletLaw Law) : SolidOp;
    public sealed record FaceCurveFillet(GeometryHandle Host, int Face, Point2d Uv, GeometryHandle Along, double Parameter, FilletLaw Law) : SolidOp;
    public sealed record SectionFillet(GeometryHandle First, int FirstFace, Point2d FirstUv, GeometryHandle Second, int SecondFace, Point2d SecondUv, SectionFilletLaw Law) : SolidOp;
    public sealed record BlendSurface(
        GeometryHandle First, int FirstFace, int FirstEdge, Interval FirstDomain, BlendContinuity FirstContinuity,
        GeometryHandle Second, int SecondFace, int SecondEdge, Interval SecondDomain, BlendContinuity SecondContinuity,
        PairPosture Reverse) : SolidOp;
    public sealed record BlendSection(
        GeometryHandle First, int FirstFace, int FirstEdge, double FirstT, BlendContinuity FirstContinuity,
        GeometryHandle Second, int SecondFace, int SecondEdge, double SecondT, BlendContinuity SecondContinuity,
        PairPosture Reverse) : SolidOp;
    public sealed record OffsetSolid(GeometryHandle Target, double Distance, CapabilitySet<OffsetGrant> Grants) : SolidOp;
    public sealed record FaceOffset(GeometryHandle Target, int Face, double Distance, CapabilitySet<OffsetGrant> Grants) : SolidOp;
    public sealed record Shell(GeometryHandle Target, Seq<int> FacesToRemove, double Distance) : SolidOp;
    public sealed record Pipe(GeometryHandle Rail, PipeLaw Law, bool LocalBlending, PipeCapMode Cap, bool FitRail = false) : SolidOp;
    public sealed record Seed(SolidSeed Value) : SolidOp;
    public sealed record TaperedExtrude(GeometryHandle Profile, double Distance, Vector3d Direction, Point3d BasePoint, double DraftAngleRadians, ExtrudeCornerType Corner) : SolidOp;
    public sealed record TaperedExtrudeRef(GeometryHandle Profile, Vector3d Direction, double Distance, double DraftAngleRadians, Plane Reference) : SolidOp;
    public sealed record PlanarFill(Seq<GeometryHandle> Loops) : SolidOp;
    public sealed record EdgeSurface(Seq<GeometryHandle> Rails) : SolidOp;
    public sealed record TrimmedPlane(Plane Frame, Seq<GeometryHandle> Curves) : SolidOp;
    public sealed record Join(Seq<GeometryHandle> Targets) : SolidOp;
    public sealed record JoinEdges(GeometryHandle First, int FirstEdge, GeometryHandle Second, int SecondEdge) : SolidOp;
    public sealed record Merge(Seq<GeometryHandle> Targets) : SolidOp;
    public sealed record MergeFaces(GeometryHandle First, GeometryHandle Second, MergeSurfaceLaw Law) : SolidOp;
    public sealed record Match(GeometryHandle Target, int Edge, Seq<GeometryHandle> TargetCurves, MatchLaw Law) : SolidOp;
    public sealed record ExtendToConnect(GeometryHandle First, int FirstFace, GeometryHandle Second, int SecondFace, ConnectSeed At) : SolidOp;
    public sealed record SplitPieces(GeometryHandle Target) : SolidOp;
    public sealed record SplitBy(GeometryHandle Target, Seq<GeometryHandle> Cutters) : SolidOp;
    public sealed record Trim(GeometryHandle Target, TrimCutter Cutter) : SolidOp;
    public sealed record CutUp(GeometryHandle Source, Seq<GeometryHandle> Curves, bool Flip) : SolidOp;
    public sealed record CopyTrims(GeometryHandle TrimSource, int Face, GeometryHandle SurfaceSource) : SolidOp;
    public sealed record Edit(GeometryHandle Target, SolidEdit Verb) : SolidOp;
    public sealed record Simplify(GeometryHandle Target) : SolidOp;
    public sealed record Lite(ExtrusionSeed Value) : SolidOp;
    public sealed record LiteProfiled(GeometryHandle Target, GeometryHandle Outer, Seq<GeometryHandle> Inners, bool Cap, Option<(Point3d A, Point3d B, Vector3d Up)> Path = default) : SolidOp;
    public sealed record LiteRead(GeometryHandle Target, ExtrusionRead Read) : SolidOp;

    private static readonly CapabilityLaw<OffsetGrant> SolidOffsetGrants =
        CapabilityLaw<OffsetGrant>.Forbidden(barred: Seq(CapabilitySet<OffsetGrant>.Of(OffsetGrant.BothSides)));

    private static readonly CapabilityLaw<OffsetGrant> FaceOffsetGrants = CapabilityLaw<OffsetGrant>.Forbidden(
        barred: Seq(CapabilitySet<OffsetGrant>.Of(OffsetGrant.Extend), CapabilitySet<OffsetGrant>.Of(OffsetGrant.Shrink)));

    internal Fin<SolidOp> Admitted() =>
        Switch(
            boolean: static (row) => ModelClaim.Admits(row,
                (nameof(row.Law), row.Law is { IsValid: true })),
            planarBoolean: static (row) => ModelClaim.Admits(row,
                (nameof(row.Plane), row.Plane.IsValid), (nameof(row.Law), row.Law is { IsValid: true })),
            solidify: static (row) => ModelClaim.Admits(row,
                (nameof(row.Open), ModelClaim.Handles(handles: row.Open))),
            filletEdges: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Edges), ModelClaim.Rows(rows: row.Edges, claim: static edge => edge.IsValid)),
                (nameof(row.Blend), Enum.IsDefined(row.Blend)), (nameof(row.Rail), Enum.IsDefined(row.Rail))),
            faceFillet: static (row) => ModelClaim.Admits(row,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.FirstUv), row.FirstUv.IsValid),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.SecondUv), row.SecondUv.IsValid), (nameof(row.Law), row.Law.IsValid)),
            faceCurveFillet: static (row) => ModelClaim.Admits(row,
                (nameof(row.Host), ModelClaim.Handle(handle: row.Host)),
                (nameof(row.Face), ValidityClaim.CountAtLeast(count: row.Face, floor: 0)),
                (nameof(row.Uv), row.Uv.IsValid), (nameof(row.Along), ModelClaim.Handle(handle: row.Along)),
                (nameof(row.Parameter), ValidityClaim.Finite(value: row.Parameter)),
                (nameof(row.Law), row.Law.IsValid)),
            sectionFillet: static (row) => ModelClaim.Admits(row,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.FirstUv), row.FirstUv.IsValid),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.SecondUv), row.SecondUv.IsValid), (nameof(row.Law), row.Law is { IsValid: true })),
            blendSurface: static (row) => ModelClaim.Admits(row,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.FirstEdge), ValidityClaim.CountAtLeast(count: row.FirstEdge, floor: 0)),
                (nameof(row.FirstDomain), row.FirstDomain.IsValid),
                (nameof(row.FirstContinuity), Enum.IsDefined(row.FirstContinuity)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.SecondEdge), ValidityClaim.CountAtLeast(count: row.SecondEdge, floor: 0)),
                (nameof(row.SecondDomain), row.SecondDomain.IsValid),
                (nameof(row.SecondContinuity), Enum.IsDefined(row.SecondContinuity)),
                (nameof(row.Reverse), row.Reverse is not null)),
            blendSection: static (row) => ModelClaim.Admits(row,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.FirstEdge), ValidityClaim.CountAtLeast(count: row.FirstEdge, floor: 0)),
                (nameof(row.FirstT), ValidityClaim.Finite(value: row.FirstT)),
                (nameof(row.FirstContinuity), Enum.IsDefined(row.FirstContinuity)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.SecondEdge), ValidityClaim.CountAtLeast(count: row.SecondEdge, floor: 0)),
                (nameof(row.SecondT), ValidityClaim.Finite(value: row.SecondT)),
                (nameof(row.SecondContinuity), Enum.IsDefined(row.SecondContinuity)),
                (nameof(row.Reverse), row.Reverse is not null)),
            offsetSolid: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.Grants), SolidOffsetGrants.Admit(held: row.Grants).IsSucc)),
            faceOffset: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Face), ValidityClaim.CountAtLeast(count: row.Face, floor: 0)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.Grants), FaceOffsetGrants.Admit(held: row.Grants).IsSucc)),
            shell: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.FacesToRemove), ModelClaim.Rows(
                    rows: row.FacesToRemove, claim: static face => ValidityClaim.CountAtLeast(count: face, floor: 0), allowEmpty: true)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0))),
            pipe: static (row) => ModelClaim.Admits(row,
                (nameof(row.Rail), ModelClaim.Handle(handle: row.Rail)),
                (nameof(row.Law), row.Law is { IsValid: true }), (nameof(row.Cap), Enum.IsDefined(row.Cap))),
            seed: static (row) => ModelClaim.Admits(row, (nameof(row.Value), row.Value is { IsValid: true })),
            taperedExtrude: static (row) => ModelClaim.Admits(row,
                (nameof(row.Profile), ModelClaim.Handle(handle: row.Profile)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.Direction), ValidityClaim.Direction(value: row.Direction)),
                (nameof(row.BasePoint), ValidityClaim.Finite(value: row.BasePoint)),
                (nameof(row.DraftAngleRadians), ValidityClaim.Finite(value: row.DraftAngleRadians)),
                (nameof(row.Corner), Enum.IsDefined(row.Corner))),
            taperedExtrudeRef: static (row) => ModelClaim.Admits(row,
                (nameof(row.Profile), ModelClaim.Handle(handle: row.Profile)),
                (nameof(row.Direction), ValidityClaim.Direction(value: row.Direction)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.DraftAngleRadians), ValidityClaim.Finite(value: row.DraftAngleRadians)),
                (nameof(row.Reference), row.Reference.IsValid)),
            planarFill: static (row) => ModelClaim.Admits(row,
                (nameof(row.Loops), ModelClaim.Handles(handles: row.Loops))),
            edgeSurface: static (row) => ModelClaim.Admits(row,
                (nameof(row.Rails), ModelClaim.Handles(handles: row.Rails))),
            trimmedPlane: static (row) => ModelClaim.Admits(row,
                (nameof(row.Frame), row.Frame.IsValid), (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves))),
            join: static (row) => ModelClaim.Admits(row,
                (nameof(row.Targets), ModelClaim.Handles(handles: row.Targets))),
            joinEdges: static (row) => ModelClaim.Admits(row,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstEdge), ValidityClaim.CountAtLeast(count: row.FirstEdge, floor: 0)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondEdge), ValidityClaim.CountAtLeast(count: row.SecondEdge, floor: 0))),
            merge: static (row) => ModelClaim.Admits(row,
                (nameof(row.Targets), ModelClaim.Handles(handles: row.Targets))),
            mergeFaces: static (row) => ModelClaim.Admits(row,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.Law), row.Law is { IsValid: true })),
            match: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Edge), ValidityClaim.CountAtLeast(count: row.Edge, floor: 0)),
                (nameof(row.TargetCurves), ModelClaim.Handles(handles: row.TargetCurves)),
                (nameof(row.Law), row.Law is { IsValid: true })),
            extendToConnect: static (row) => ModelClaim.Admits(row,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.At), row.At is { IsValid: true })),
            splitPieces: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            splitBy: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Cutters), ModelClaim.Handles(handles: row.Cutters))),
            trim: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Cutter), row.Cutter is { IsValid: true })),
            cutUp: static (row) => ModelClaim.Admits(row,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves))),
            copyTrims: static (row) => ModelClaim.Admits(row,
                (nameof(row.TrimSource), ModelClaim.Handle(handle: row.TrimSource)),
                (nameof(row.Face), ValidityClaim.CountAtLeast(count: row.Face, floor: 0)),
                (nameof(row.SurfaceSource), ModelClaim.Handle(handle: row.SurfaceSource))),
            edit: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Verb), row.Verb is { IsValid: true })),
            simplify: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            lite: static (row) => ModelClaim.Admits(row, (nameof(row.Value), row.Value is { IsValid: true })),
            liteProfiled: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Outer), ModelClaim.Handle(handle: row.Outer)),
                (nameof(row.Inners), ModelClaim.Handles(handles: row.Inners, allowEmpty: true)),
                (nameof(row.Path), ValidityClaim.WhenPresent(facet: row.Path, claim: static path => ValidityClaim.All(
                    ValidityClaim.Finite(value: path.A), ValidityClaim.Finite(value: path.B),
                    ValidityClaim.Direction(value: path.Up))))),
            liteRead: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Read), row.Read is { IsValid: true })));

    internal Fin<Seq<GeometryHandle>> Apply(Context domain) =>
        Switch(
            context: domain,
            boolean: static (model, edit) => {
                return edit.Law.Switch(
                    state: model,
                    union: static (ctx, law) => ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.Breps, body: breps =>
                        ModelGate.Many(() => Brep.CreateBooleanUnion(
                            breps: breps.AsIterable(), tolerance: ctx.Absolute.Value, manifoldOnly: law.ManifoldOnly))),
                    intersection: static (ctx, law) => ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.First, body: first =>
                        ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.Second, body: second =>
                            ModelGate.Many(() => Brep.CreateBooleanIntersection(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(),
                                tolerance: ctx.Absolute.Value, manifoldOnly: law.ManifoldOnly)))),
                    difference: static (ctx, law) => ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.First, body: first =>
                        ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.Second, body: second =>
                            ModelGate.Many(() => Brep.CreateBooleanDifference(
                                    firstSet: first.AsIterable(), secondSet: second.AsIterable(),
                                    tolerance: ctx.Absolute.Value, manifoldOnly: law.ManifoldOnly))),
                    split: static (ctx, law) => ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.First, body: first =>
                        ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.Second, body: second =>
                            ModelGate.Many(() => Brep.CreateBooleanSplit(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(), tolerance: ctx.Absolute.Value)))));
            },
            planarBoolean: static (model, edit) => {
                return edit.Law.Switch(
                    state: (Edit: edit, Model: model),
                    union: static (ctx, law) => ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: law.Breps, body: breps =>
                        ModelGate.Many(() => Brep.CreatePlanarUnion(
                            breps: breps.AsIterable(), plane: ctx.Edit.Plane, tolerance: ctx.Absolute.Value))),
                    intersection: static (ctx, law) => ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: law.First, body: first =>
                        ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: law.Second, body: second =>
                            ModelGate.Many(() => Brep.CreatePlanarIntersection(
                                b0: first, b1: second, plane: ctx.Edit.Plane, tolerance: ctx.Absolute.Value)))),
                    difference: static (ctx, law) => ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: law.First, body: first =>
                        ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: law.Second, body: second =>
                            ModelGate.Many(() => Brep.CreatePlanarDifference(
                                b0: first, b1: second, plane: ctx.Edit.Plane, tolerance: ctx.Absolute.Value)))));
            },
            solidify: static (model, edit) => {
                return ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: edit.Open, body: open =>
                    ModelGate.Many(() => Brep.CreateSolid(breps: open.AsIterable(), tolerance: model.Absolute.Value)));
            },
            filletEdges: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    from _ in guard(edit.Edges.ForAll(row => row.Edge < target.Edges.Count), new KernelFault.InvalidInput())
                    from built in edit.Edges.Exists(static row => row.Law is RadiusLaw.Profiled)
                        ? ModelGate.Many(op, () => Brep.CreateFilletEdgesVariableRadius(
                            brep: target,
                            edgeIndices: edit.Edges.Map(static row => row.Edge).AsIterable(),
                            edgeDistances: edit.Edges.AsEnumerable().ToDictionary(
                                static row => row.Edge,
                                row => row.Law.Switch<System.Collections.Generic.IList<BrepEdgeFilletDistance>>(
                                    constant: law => [
                                        new BrepEdgeFilletDistance(edgeParameter: target.Edges[row.Edge].Domain.Min, filletDistance: law.Start),
                                        new BrepEdgeFilletDistance(edgeParameter: target.Edges[row.Edge].Domain.Max, filletDistance: law.End)],
                                    profiled: static law => [.. law.Rows.Map(static point =>
                                        new BrepEdgeFilletDistance(edgeParameter: point.Parameter, filletDistance: point.Distance))])),
                            blendType: edit.Blend, railType: edit.Rail, setbackFillets: edit.Setback,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))
                        : ModelGate.Many(op, () => Brep.CreateFilletEdges(
                            brep: target,
                            edgeIndices: edit.Edges.Map(static row => row.Edge).AsIterable(),
                            startRadii: edit.Edges.Map(static row => ((RadiusLaw.Constant)row.Law).Start).AsIterable(),
                            endRadii: edit.Edges.Map(static row => ((RadiusLaw.Constant)row.Law).End).AsIterable(),
                            blendType: edit.Blend, railType: edit.Rail, setbackFillets: edit.Setback,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))
                    select built);
            },
            faceFillet: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.First, body: first =>
                    ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Second, body: second =>
                        from _ in guard(edit.FirstFace < first.Faces.Count && edit.SecondFace < second.Faces.Count, new KernelFault.InvalidInput())
                        from settings in edit.Law.Rig(domain: model)
                        from built in Try.lift(() =>
                            Admit.Confirm(success: Brep.CreateFilletSurface(
                                face0: first.Faces[edit.FirstFace], uv0: edit.FirstUv,
                                face1: second.Faces[edit.SecondFace], uv1: edit.SecondUv,
                                settings: settings, results: out Brep.FilletSurfaceResults results))
                            .Bind(_ => Harvested(results: results))).Run().Bind(static inner => inner)
                        select built));
            },
            faceCurveFillet: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Host, body: host =>
                    ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Along, body: along =>
                        from _ in guard(edit.Face < host.Faces.Count, new KernelFault.InvalidInput())
                        from settings in edit.Law.Rig(domain: model)
                        from built in Try.lift(() =>
                            Admit.Confirm(success: Brep.CreateFilletSurfaceCurve(
                                face: host.Faces[edit.Face], uv: edit.Uv, curve: along, t: edit.Parameter,
                                settings: settings, results: out Brep.FilletSurfaceResults results))
                            .Bind(_ => Harvested(results: results))).Run().Bind(static inner => inner)
                        select built));
            },
            sectionFillet: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.First, body: first =>
                    ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Second, body: second =>
                        from _ in guard(
                            edit.FirstFace < first.Faces.Count && edit.SecondFace < second.Faces.Count,
                            new KernelFault.InvalidInput())
                        from built in SectionFilleted(
                            first: first.Faces[edit.FirstFace], firstUv: edit.FirstUv,
                            second: second.Faces[edit.SecondFace], secondUv: edit.SecondUv,
                            law: edit.Law, tolerance: model.Absolute.Value)
                        select built));
            },
            blendSurface: static (_, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.First, body: first =>
                    ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Second, body: second =>
                        from _ in guard(
                            edit.FirstFace < first.Faces.Count && edit.FirstEdge < first.Edges.Count
                            && edit.SecondFace < second.Faces.Count && edit.SecondEdge < second.Edges.Count,
                            new KernelFault.InvalidInput())
                        from built in ModelGate.Many(op, () => Brep.CreateBlendSurface(
                            face0: first.Faces[edit.FirstFace], edge0: first.Edges[edit.FirstEdge], domain0: edit.FirstDomain, rev0: edit.Reverse.First, continuity0: edit.FirstContinuity,
                            face1: second.Faces[edit.SecondFace], edge1: second.Edges[edit.SecondEdge], domain1: edit.SecondDomain, rev1: edit.Reverse.Second, continuity1: edit.SecondContinuity))
                        select built));
            },
            blendSection: static (_, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.First, body: first =>
                    ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Second, body: second =>
                        from _ in guard(
                            edit.FirstFace < first.Faces.Count && edit.FirstEdge < first.Edges.Count
                            && edit.SecondFace < second.Faces.Count && edit.SecondEdge < second.Edges.Count,
                            new KernelFault.InvalidInput())
                        from built in ModelGate.Single(op, () => Brep.CreateBlendShape(
                            face0: first.Faces[edit.FirstFace], edge0: first.Edges[edit.FirstEdge], t0: edit.FirstT, rev0: edit.Reverse.First, continuity0: edit.FirstContinuity,
                            face1: second.Faces[edit.SecondFace], edge1: second.Edges[edit.SecondEdge], t1: edit.SecondT, rev1: edit.Reverse.Second, continuity1: edit.SecondContinuity))
                        select built));
            },
            offsetSolid: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    Try.lift(() => {
                        Brep[] offsets = Brep.CreateOffsetBrep(
                            brep: target, distance: edit.Distance,
                            solid: edit.Grants.Admits(capability: OffsetGrant.Solid),
                            extend: edit.Grants.Admits(capability: OffsetGrant.Extend),
                            shrink: edit.Grants.Admits(capability: OffsetGrant.Shrink),
                            tolerance: model.Absolute.Value, outBlends: out Brep[] blends, outWalls: out Brep[] walls);
                        return ModelGate.Staged((offsets, false),
                            (blends, true),
                            (walls, true));
                    }).Run().Bind(static inner => inner));
            },
            faceOffset: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    from _ in guard(edit.Face < target.Faces.Count, new KernelFault.InvalidInput())
                    from built in ModelGate.Single(() => Brep.CreateFromOffsetFace(
                        face: target.Faces[edit.Face], offsetDistance: edit.Distance,
                        offsetTolerance: model.Absolute.Value,
                        bothSides: edit.Grants.Admits(capability: OffsetGrant.BothSides),
                        createSolid: edit.Grants.Admits(capability: OffsetGrant.Solid)))
                    select built);
            },
            shell: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    from _ in guard(edit.FacesToRemove.ForAll(face => face < target.Faces.Count), new KernelFault.InvalidInput())
                    from built in ModelGate.Many(() => Brep.CreateShell(
                        brep: target, facesToRemove: edit.FacesToRemove.AsIterable(), distance: edit.Distance, tolerance: model.Absolute.Value))
                    select built);
            },
            pipe: static (model, edit) => {
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Rail, body: rail =>
                    edit.Law.Switch(
                        state: (Rail: rail, Edit: edit, Tolerance: model.Absolute.Value, Angle: model.Angle.Value),
                        constant: static (ctx, law) => ModelGate.Many(() => Brep.CreatePipe(
                            rail: ctx.Rail, radius: law.Radius, localBlending: ctx.Edit.LocalBlending, cap: ctx.Edit.Cap,
                            fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        variable: static (ctx, law) => ModelGate.Many(() => Brep.CreatePipe(
                            rail: ctx.Rail, railRadiiParameters: law.Rows.Map(static row => row.Parameter).AsIterable(),
                            radii: law.Rows.Map(static row => row.Radius).AsIterable(), localBlending: ctx.Edit.LocalBlending,
                            cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        thick: static (ctx, law) => ModelGate.Many(() => Brep.CreateThickPipe(
                            rail: ctx.Rail, radius0: law.Radius0, radius1: law.Radius1, localBlending: ctx.Edit.LocalBlending,
                            cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        thickVariable: static (ctx, law) => ModelGate.Many(() => Brep.CreateThickPipe(
                            rail: ctx.Rail, railRadiiParameters: law.Rows.Map(static row => row.Parameter).AsIterable(),
                            radii0: law.Rows.Map(static row => row.Inner).AsIterable(), radii1: law.Rows.Map(static row => row.Outer).AsIterable(),
                            localBlending: ctx.Edit.LocalBlending, cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail,
                            absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle))));
            },
            seed: static (model, edit) => {
                return edit.Value.Build(domain: model).Map(Seq);
            },
            taperedExtrude: static (model, edit) => {
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Profile, body: profile =>
                    ModelGate.Many(() => Brep.CreateFromTaperedExtrude(
                        curveToExtrude: profile, distance: edit.Distance, direction: edit.Direction, basePoint: edit.BasePoint,
                        draftAngleRadians: edit.DraftAngleRadians, cornerType: edit.Corner,
                        tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value)));
            },
            taperedExtrudeRef: static (model, edit) => {
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Profile, body: profile =>
                    ModelGate.Many(() => Brep.CreateFromTaperedExtrudeWithRef(
                        curve: profile, direction: edit.Direction, distance: edit.Distance,
                        draftAngle: edit.DraftAngleRadians, plane: edit.Reference, tolerance: model.Absolute.Value)));
            },
            planarFill: static (model, edit) => {
                return ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Loops, body: loops =>
                    ModelGate.Many(() => Brep.CreatePlanarBreps(inputLoops: loops.AsIterable(), tolerance: model.Absolute.Value)));
            },
            edgeSurface: static (_, edit) => {
                return ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Rails, body: rails =>
                    from _ in guard(rails.Count >= 2 && rails.Count <= 4, new KernelFault.InvalidInput())
                    from built in ModelGate.Single(() => Brep.CreateEdgeSurface(curves: rails.AsIterable()))
                    select built);
            },
            trimmedPlane: static (_, edit) => {
                return ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Curves, body: curves =>
                    ModelGate.Single(() => Brep.CreateTrimmedPlane(plane: edit.Frame, curves: curves.AsIterable())));
            },
            join: static (model, edit) => {
                return ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: edit.Targets, body: targets =>
                    Try.lift(() => {
                        Brep[] joined = Brep.JoinBreps(
                            brepsToJoin: targets.AsIterable(), tolerance: model.Absolute.Value,
                            angleTolerance: model.Angle.Value,
                            indexMap: out _);
                        return ModelGate.OwnMany(built: joined);
                    }).Run().Bind(static inner => inner));
            },
            joinEdges: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.First, body: first =>
                    ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Second, body: second =>
                        from _ in guard(
                            edit.FirstEdge < first.Edges.Count && edit.SecondEdge < second.Edges.Count,
                            new KernelFault.InvalidInput())
                        from built in ModelGate.Single(op, () => Brep.CreateFromJoinedEdges(
                            brep0: first, edgeIndex0: edit.FirstEdge, brep1: second, edgeIndex1: edit.SecondEdge, joinTolerance: model.Absolute.Value))
                        select built));
            },
            merge: static (model, edit) => {
                return ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: edit.Targets, body: targets =>
                    ModelGate.Single(() => Brep.MergeBreps(brepsToMerge: targets.AsIterable(), tolerance: model.Absolute.Value)));
            },
            mergeFaces: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.First, body: first =>
                    ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Second, body: second =>
                        edit.Law.Switch(
                            state: (First: first, Second: second, Model: model),
                            plain: static ctx => ModelGate.Single(() => Brep.MergeSurfaces(
                                brep0: ctx.First, brep1: ctx.Second,
                                tolerance: ctx.Absolute.Value, angleToleranceRadians: ctx.Angle.Value)),
                            atPoints: static (ctx, law) => ModelGate.Single(() => Brep.MergeSurfaces(
                                brep0: ctx.First, brep1: ctx.Second,
                                tolerance: ctx.Absolute.Value, angleToleranceRadians: ctx.Angle.Value,
                                point0: law.First, point1: law.Second, roundness: law.Roundness, smooth: law.Smooth)))));
            },
            match: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.TargetCurves, body: curves =>
                        from _ in guard(edit.Edge < target.Edges.Count, new KernelFault.InvalidInput())
                        from settings in edit.Law.Rig(domain: model)
                        from built in Try.lift(() =>
                            ModelGate.Staged(success: Brep.CreateFromMatch(
                                edge: target.Edges[edit.Edge], targetCurves: curves.AsIterable(), settings: settings,
                                matched: out Brep matched, target: out Brep matchTarget),
                                ((Brep[])[matched, matchTarget], false))).Run().Bind(static inner => inner)
                        select built));
            },
            extendToConnect: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.First, body: first =>
                    ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Second, body: second =>
                        from _ in guard(edit.FirstFace < first.Faces.Count && edit.SecondFace < second.Faces.Count, new KernelFault.InvalidInput())
                        from __ in edit.At.Switch(
                            atEdges: at => guard(
                                at.FirstEdge < first.Edges.Count && at.SecondEdge < second.Edges.Count,
                                new KernelFault.InvalidInput()),
                            atPoints: static _ => Fin.Succ(value: unit))
                        from built in Try.lift(() => {
                            (bool Connected, Brep First, Brep Second) result = edit.At.Switch(
                                state: (First: first.Faces[edit.FirstFace], Second: second.Faces[edit.SecondFace], Tol: model.Absolute.Value, Angle: model.Angle.Value),
                                atEdges: static (ctx, at) => {
                                    bool connected = Brep.ExtendBrepFacesToConnect(
                                        Face0: ctx.First, edgeIndex0: at.FirstEdge, Face1: ctx.Second, edgeIndex1: at.SecondEdge,
                                        tol: ctx.Tol, angleTol: ctx.Angle, outBrep0: out Brep firstResult, outBrep1: out Brep secondResult);
                                    return (Connected: connected, First: firstResult, Second: secondResult);
                                },
                                atPoints: static (ctx, at) => {
                                    bool connected = Brep.ExtendBrepFacesToConnect(
                                        Face0: ctx.First, f0_sel_pt: at.First, Face1: ctx.Second, f1_sel_pt: at.Second,
                                        tol: ctx.Tol, angleTol: ctx.Angle, outBrep0: out Brep firstResult, outBrep1: out Brep secondResult);
                                    return (Connected: connected, First: firstResult, Second: secondResult);
                                });
                            return ModelGate.Staged(success: result.Connected,
                                ((Brep[])[result.First, result.Second], false));
                        }).Run().Bind(static inner => inner)
                        select built));
            },
            splitPieces: static (_, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    Try.lift(() => {
                        Brep[] pieces = Brep.SplitDisjointPieces(brep: target);
                        return ModelGate.OwnMany(built: pieces);
                    }).Run().Bind(static inner => inner));
            },
            splitBy: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    ModelGate.BorrowMany<Brep, Seq<GeometryHandle>>(handles: edit.Cutters, body: cutters =>
                        cutters.Count == 1
                            ? Try.lift(() => {
                                Brep[] pieces = target.Split(cutter: cutters[0], intersectionTolerance: model.Absolute.Value, toleranceWasRaised: out _);
                                return ModelGate.OwnMany(built: pieces);
                            }).Run().Bind(static inner => inner)
                            : ModelGate.Many(op, () => target.Split(cutters: cutters.AsIterable(), intersectionTolerance: model.Absolute.Value))));
            },
            trim: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.Target, body: target =>
                    edit.Cutter.Switch(
                        state: (Target: target, Tolerance: model.Absolute.Value),
                        byBrep: static (ctx, cutter) => ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: cutter.Cutter,
                            body: blade => ModelGate.Many(() => ctx.Target.Trim(cutter: blade, intersectionTolerance: ctx.Tolerance))),
                        byPlane: static (ctx, cutter) => ModelGate.Many(() => ctx.Target.Trim(cutter: cutter.Cutter, intersectionTolerance: ctx.Tolerance))));
            },
            cutUp: static (model, edit) => {
                return ModelGate.Borrow<Surface, Seq<GeometryHandle>>(handle: edit.Source, body: surface =>
                    ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Curves, body: curves =>
                        ModelGate.Many(() => Brep.CutUpSurface(
                            surface: surface, curves: curves.AsIterable(), flip: edit.Flip,
                            fitTolerance: model.Absolute.Value, keepTolerance: model.Absolute.Value))));
            },
            copyTrims: static (model, edit) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: edit.TrimSource, body: source =>
                    ModelGate.Borrow<Surface, Seq<GeometryHandle>>(handle: edit.SurfaceSource, body: surface =>
                        from _ in guard(edit.Face < source.Faces.Count, new KernelFault.InvalidInput())
                        from built in ModelGate.Single(() => Brep.CopyTrimCurves(
                            trimSource: source.Faces[edit.Face], surfaceSource: surface, tolerance: model.Absolute.Value))
                        select built));
            },
            edit: static (model, request) => {
                return ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: request.Target, body: source =>
                    Try.lift(() => Optional(source.DuplicateBrep()).ToFin(Fail: new KernelFault.InvalidResult()).Bind(working =>
                        Edited(working: working, verb: request.Verb, domain: model).Rollback(working))).Run().Bind(static inner => inner));
            },
            simplify: static (_, edit) => {
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Target, body: source =>
                    ModelGate.Single(() => Brep.TryConvertBrep(geometry: source)));
            },
            lite: static (_, edit) => {
                return edit.Value.Build().Map(Seq);
            },
            liteProfiled: static (_, edit) => {
                return ModelGate.Borrow<Extrusion, Seq<GeometryHandle>>(handle: edit.Target, body: source =>
                    ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Outer, body: outer =>
                        ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Inners, allowEmpty: true, body: inners =>
                            Try.lift(() => Optional(source.Duplicate() as Extrusion).ToFin(Fail: new KernelFault.InvalidResult()).Bind(working => (
                                from _ in edit.Path.Case switch {
                                    (Point3d a, Point3d b, Vector3d up) => Admit.Confirm(success: working.SetPathAndUp(a: a, b: b, up: up)),
                                    _ => Fin.Succ(value: unit),
                                }
                                from __ in Admit.Confirm(success: working.SetOuterProfile(outerProfile: outer, cap: edit.Cap))
                                from ___ in inners.FoldM<Fin, Unit>(unit, (_, inner) =>
                                    Admit.Confirm(success: working.AddInnerProfile(innerProfile: inner)))
                                from built in ModelGate.Kept(op, working)
                                select built)
                                .Rollback(working))).Run().Bind(static inner => inner))));
            },
            liteRead: static (_, edit) => {
                return ModelGate.Borrow<Extrusion, Seq<GeometryHandle>>(handle: edit.Target, body: source =>
                    edit.Read.Switch(
                        state: source,
                        heavy: static (ctx, read) => ModelGate.Single(() =>
                            ctx.ToBrep(splitKinkyFaces: read.SplitKinkyFaces)),
                        wireframe: static ctx => Try.lift(() => ModelGate.DetachedMany(source: ctx.GetWireframe())).Run().Bind(static inner => inner)
                            .Bind(detached => ModelGate.Many(() => detached.AsEnumerable())),
                        mesh: static (ctx, read) => Try.lift(() => ModelGate.Detached(
                                source: ctx.GetMesh(meshType: read.Kind))).Run().Bind(static inner => inner)
                            .Bind(detached => ModelGate.Single(() => detached)),
                        profile: static (ctx, read) => Try.lift(() => ModelGate.Detached(
                                source: ctx.Profile3d(profileIndex: read.Index, s: read.Station))).Run().Bind(static inner => inner)
                            .Bind(detached => ModelGate.Single(() => detached)),
                        wallEdge: static (ctx, read) => Try.lift(() => ModelGate.Detached(
                                source: ctx.WallEdge(ci: read.Component))).Run().Bind(static inner => inner)
                            .Bind(detached => ModelGate.Single(() => detached)),
                        wallSurface: static (ctx, read) => Try.lift(() => ModelGate.Detached(
                                source: ctx.WallSurface(ci: read.Component))).Run().Bind(static inner => inner)
                            .Bind(detached => ModelGate.Single(() => detached))));
            });

    private static Fin<Seq<GeometryHandle>> Harvested(Brep.FilletSurfaceResults results) =>
        ModelGate.Staged((FaceDup(face: results.Face0), true),
            (FaceDup(face: results.Face1), true),
            (results.Fillets, false),
            (results.OutBreps0, true),
            (results.OutBreps1, true));

    private static Fin<Seq<GeometryHandle>> SectionFilleted(
        BrepFace first, Point2d firstUv, BrepFace second, Point2d secondUv,
        SectionFilletLaw law, double tolerance) =>
        Try.lift(() => {
            System.Collections.Generic.List<Brep> trimmed0 = [];
            System.Collections.Generic.List<Brep> trimmed1 = [];
            System.Collections.Generic.List<Brep> fillets = [];
            bool created = law.Profile.Switch(
                state: (First: first, FirstUv: firstUv, Second: second, SecondUv: secondUv, Law: law, Tolerance: tolerance, Trimmed0: trimmed0, Trimmed1: trimmed1, Fillets: fillets),
                rationalArcs: static ctx => SurfaceFilletBase.CreateRationalArcsFilletSrf(
                    ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                    ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                nonRationalArcs: static (ctx, profile) => profile.Degree.Create(
                    first: ctx.First, firstUv: ctx.FirstUv, second: ctx.Second, secondUv: ctx.SecondUv,
                    law: ctx.Law, tolerance: ctx.Tolerance,
                    trimmed0: ctx.Trimmed0, trimmed1: ctx.Trimmed1, fillets: ctx.Fillets),
                nonRationalCubic: static (ctx, profile) => SurfaceFilletBase.CreateNonRationalCubicFilletSrf(
                        ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                        ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, profile.TangentSlider.Value, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                nonRationalHigher: static (ctx, profile) => profile.Degree.Create(
                    first: ctx.First, firstUv: ctx.FirstUv, second: ctx.Second, secondUv: ctx.SecondUv,
                    law: ctx.Law, tangent: profile.TangentSlider.Value, inner: profile.InnerSlider.Value, tolerance: ctx.Tolerance,
                    trimmed0: ctx.Trimmed0, trimmed1: ctx.Trimmed1, fillets: ctx.Fillets),
                g2ChordalQuintic: static ctx => SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf(
                    ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                    ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets));
            return Admit.Confirm(success: created).Bind(_ => Harvested(
                fillets: fillets, trimmed0: trimmed0, trimmed1: trimmed1));
        }).Run().Bind(static inner => inner);

    private static Fin<Seq<GeometryHandle>> Harvested(
        System.Collections.Generic.IEnumerable<Brep> fillets,
        System.Collections.Generic.IEnumerable<Brep> trimmed0,
        System.Collections.Generic.IEnumerable<Brep> trimmed1) =>
        ModelGate.Staged((fillets, false),
            (trimmed0, true),
            (trimmed1, true));

    private static System.Collections.Generic.IEnumerable<GeometryBase> FaceDup(BrepFace? face) =>
        face is null ? [] : [face.Duplicate()];

    private static Fin<Seq<GeometryHandle>> Edited(Brep working, SolidEdit verb, Context domain) =>
        verb.Switch(
            state: (Working: working, Domain: domain),
            cap: static ctx => ModelGate.Owned(ctx.Working, () => ctx.Working.CapPlanarHoles(tolerance: ctx.Domain.Absolute.Value)),
            joinNaked: static ctx => Try.lift(() => ctx.Working.JoinNakedEdges(tolerance: ctx.Domain.Absolute.Value)).Run()
                .Bind(_ => ModelGate.Kept(ctx.Working)),
            mergeCoplanar: static ctx =>
                from _ in Admit.Confirm(success: ctx.Working.MergeCoplanarFaces(
                    tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                from built in ModelGate.Kept(ctx.Working)
                select built,
            mergeFace: static (ctx, edit) =>
                from _ in guard(edit.Face < ctx.Working.Faces.Count, new KernelFault.InvalidInput())
                from __ in Admit.Confirm(success: ctx.Working.MergeCoplanarFaces(
                    faceIndex: edit.Face,
                    tolerance: ctx.Domain.Absolute.Value,
                    angleTolerance: ctx.Domain.Angle.Value))
                from built in ModelGate.Kept(ctx.Working)
                select built,
            mergeFacePair: static (ctx, edit) =>
                from _ in guard(
                    edit.First < ctx.Working.Faces.Count && edit.Second < ctx.Working.Faces.Count,
                    new KernelFault.InvalidInput())
                from __ in Admit.Confirm(success: ctx.Working.MergeCoplanarFaces(
                    faceIndex0: edit.First, faceIndex1: edit.Second,
                    tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                from built in ModelGate.Kept(ctx.Working)
                select built,
            unjoinEdges: static (ctx, edit) =>
                from _ in guard(edit.Edges.ForAll(edge => edge < ctx.Working.Edges.Count), new KernelFault.InvalidInput())
                from built in ModelGate.OwnedMany(working: ctx.Working,
                    run: () => ctx.Working.UnjoinEdges(edgesToUnjoin: edit.Edges.AsIterable()))
                select built,
            removeHoles: static (ctx, edit) => ModelGate.Owned(ctx.Working, () => ctx.Working.RemoveHoles(loops: edit.Loops.AsIterable(), tolerance: ctx.Domain.Absolute.Value)),
            removeFins: static ctx => Admit.Confirm(success: ctx.Working.RemoveFins()).Bind(_ => ModelGate.Kept(ctx.Working)),
            cullFaces: static ctx => Admit.Confirm(success: ctx.Working.CullUnusedFaces()).Bind(_ => ModelGate.Kept(ctx.Working)),
            repair: static ctx => Admit.Confirm(success: ctx.Working.Repair(tolerance: ctx.Domain.Absolute.Value)).Bind(_ => ModelGate.Kept(ctx.Working)),
            reseam: static (ctx, edit) =>
                from _ in guard(edit.Face < ctx.Working.Faces.Count, new KernelFault.InvalidInput())
                from built in ModelGate.Owned(ctx.Working, () => Brep.ChangeSeam(
                    face: ctx.Working.Faces[edit.Face], direction: edit.Axis.Native,
                    parameter: edit.Parameter, tolerance: ctx.Domain.Absolute.Value))
                select built);

}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Solids {
    public static Eff<ModelRuntime, Seq<GeometryHandle>> Build(params ReadOnlySpan<SolidOp> operations) {
        Seq<SolidOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(),
                apply: static (operation, model) => operation.Apply(domain: model)).ToEff());
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
