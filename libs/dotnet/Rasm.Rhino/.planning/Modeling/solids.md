# [RASM_RHINO_MODELING_SOLIDS]

`Rasm.Rhino.Modeling` owns Brep solid construction. `SolidOp` carries booleans, edge treatments, offset, shell, pipe, seeding, tapered extrusion, editing, and the `Extrusion` lifecycle through `Solids.Build`. `ModelGate` borrows leased `GeometryHandle` inputs and owns fresh natives. `Built<TSlot>` preserves operation-correlated products and evidence. Native statics return command-fidelity geometry; intersections, mass properties, bounds, contours, and analysis remain kernel-owned. Every arm reads tolerance through `Context`.

## [01]-[INDEX]

- [02]-[MODEL_GATE]: `ModelGate`, `ModelRuntime`, `Built<TSlot>`, `BuildRun<TSlot>`, `BuildBody`, `BuildFact<TSlot>`, `BuildReceipt<TSlot>`, `BenchBand`, `BenchEvidence` — the folder spine, its effect runtime, and its bench harvest.
- [03]-[POLICY_FAMILY]: `CapEnd`, `ShapeGrant`, `OffsetGrant`, `SolidBooleanLaw`, `PlanarBooleanLaw`, `ArcDegree`, `ArcSlider`, `FilletShape`, `FilletLaw`, `FilletDegree`, `HigherFilletDegree`, `SectionFilletProfile`, `RadiusLaw`, `EdgeFillet`, `SectionFilletLaw`, `MatchLaw`, `MatchCapability`, `MatchRefinement`, `PipeLaw`, `SolidSeed`, `ExtrusionSeed`, `ExtrusionRead`, `SolidEdit`, `MergeSurfaceLaw`, `TrimCutter`, `ConnectSeed` — the grant vocabularies and the construction policies.
- [04]-[OPERATION_RAIL]: `SolidSlot`, `SolidOp`, and the `Solids.Build` entry.

## [02]-[MODEL_GATE]

- Owner: `ModelGate` — the one custody kernel under every Modeling arm: `Borrow` projects a live native of the demanded kind out of a leased handle, `BorrowMany` sequences borrow windows over a handle spread, `Own`/`OwnMany`/`OwnEach` mint owned leases for fresh natives, `Folded` is the batch fold, and `Entry` is the one operation spine ALL EIGHT Modeling pages run — capture, non-empty guard, accumulating admission, `Folded`, and the bench stamp in one member; `Built<TSlot>` — flat projections over operation-correlated `BuildRun<TSlot>` groups; `BuildBody` `[Union]` — the closed evidence payload vocabulary; `BuildReceipt<TSlot>` — the slot-generic fact-stream monoid; `ModelRuntime` — the folder runtime carrying the regime, session timeline, cancellation token, and the two progress reporters a `ProgressLease` produces.
- Law: `Entry` discriminates on the operation carrier alone — the `ReadOnlySpan<TOp>` entry materializes and delegates to the `Seq<TOp>` core, and a runtime-bound page enters at the core because a span cannot cross the `Eff.runtime<TRuntime>()` lambda that binds its runtime; admission is ALWAYS accumulating, so every page reports the whole rejection set and no page re-mints a fold with abort-on-first semantics nothing asked for.
- Law: minting is spine-owned — `Single` and `Many` mint the one-product and spread run with its slot tally, `Kept` and `Owned`/`OwnedMany` close duplicate-edit custody, `Staged` enumerates and owns every harvest inside one guarded custody scope, `Mapped` admits source maps against their declared axis cardinality, `Detached` disposes host-returned originals after duplication, and `Entry` accumulates every operation refusal before dispatch; failure-arm custody release rides kernel `Custody.Rollback` at every fold, and a page-local re-mint of any member is the deleted form.
- Law: release is fallible and never masks — kernel `Custody.Dispose`/`Rollback` own reverse-order, all-attempted cleanup and append every refusal onto the primary; `Staged` folds each admitted product roster through that kernel, so no local sweep, unwind, or disposer projection survives.
- Law: a construction result is an acquisition, never a crossing — the native static's return is this rail's own owned material, so `Own` mints the owned lease directly and `GeometryCrossing.Cross` remains the entry for foreign or document geometry; a null single result and a null-or-empty array are the native failure signal folded to `InvalidResult` unless the arm passes the `allowEmpty` grant because a declared diagnostic side-channel explains the empty spread — the boolean-union arm survives empty behind its naked/bad/non-manifold marks — and a mid-spread `OwnMany` failure disposes every handle it already minted.
- Law: `BuildReceipt<TSlot>` is a SECOND evidence machine beside `Document/facts.md`'s `FactStream<TSlot, TBody>`, and the discriminant is TIMING, stated at that owner and settled at the folder RULINGS: a stream fact accumulates inside one `DocumentCommit` and is sealed by an undo stamp, while a build fact is bound to a produced value minted OUTSIDE the commit and read by the builder that produced it, so conforming this receipt would put a commit-scoped undo column on a value that never enters a commit. `SolidSlot` and every Modeling sibling slot is therefore a BUILD-timing vocabulary riding `Built<TSlot>` and declares no `IFactSlot<TBody>`; a Modeling page aliasing its receipt to `FactStream` is the deleted form, and so is a mutation folder re-minting `BuildReceipt` beside the stream.
- Law: `BuildBody`'s scalar cases are slot-addressed evidence, not primitive wrappers — `Tally`, `Measure`, `Code`, `Flag`, and `Text` are read through `Project<T>(slot, select)`, so the fact's meaning is the (slot, case) pair and never the CLR type alone: `SolidSlot.SplitApart` + `Flag` IS the tolerance-escalation verdict and `MeshSlot.Booled` + `Code` IS the host command result. Splitting them into per-meaning cases forks one union per consuming page and buys a name the slot already carries.
- Law: one receipt algebra serves every Modeling page — `BuildReceipt<TSlot>` is generic over the page's slot vocabulary, so diagnostic points, uv rows, labels, axis-qualified source maps, segments, faces, components, region topology, tallies, codes, measures, flags, planes, bounds, motion, and texts are one `BuildBody` union with one `+` monoid; `Project<T>(slot, select)` is the only reader, and callers select the demanded body case instead of growing accessor rosters. Every case answers to a live producer — the unsigned-id case the SubD interpolate arm fed is deleted with that arm, because a payload no arm emits is decorative density however well the type reads.
- Law: every operation remains reconstructible — `BuildRun<TSlot>` groups the `Op`, exact product spread, and exact receipt generated by one arm; `Built<TSlot>.Products` and `.Evidence` are batch projections, while `.Runs` preserves correspondence across repeated slots and heterogeneous batches.
- Law: the batch fold is failure-symmetric — `Folded` sums products and receipts monoidally and releases every product accumulated by earlier operations the moment a later operation faults, so a batch never half-leaks custody.
- Law: `Borrow` is the type gate — a handle whose live native is not the demanded kind refuses through `Unsupported` with both types named, so no arm ever pattern-matches raw geometry beyond its own dispatch.
- Law: bench evidence is harvest-grade spine data — `BenchBand.Measured` brackets one entry through the injected session `MonotonicTimeline`, reads thread allocation, and converts synchronous runner exceptions through `Op.Catch`; `BenchEvidence` normalizes to the corpus-gate benchmark-receipt shape (operation-family identity, input scale, duration, allocation, outcome bit, host fingerprint), while the corpus gate owns aggregation and thresholds; the capture rail composes the same band.
- Law: benchmark evidence is RECEIPT data, never an error surrogate. `Entry` stamps it onto `Built<TSlot>.Bench` on success; failure preserves its exact cause without appending a generic text warning. Harnesses that need failed-run timing consume `BenchBand.Measured` directly, where the evidence remains beside either outcome.
- Boundary: `ModelRuntime` receives the load root's one `MonotonicTimeline`; every Modeling entry takes that runtime, so timing never mints a clock or reads `Stopwatch`. Capture receives the same timeline explicitly from its composing root.
- Growth: a new evidence payload is one `BuildBody` case; a new custody modality is one `ModelGate` member; a new bench dimension is one `BenchEvidence` field; sibling pages add zero spine surface.
- Packages: kernel `Domain/rails` (`Op`, `Fault`, `Fin`, `ValidityClaim`, `Custody`, `Op.Catch`/`Confirm`/`Need`), kernel `Domain/context` (`Context`, `Tolerance`), kernel `Parametric/projections` (`MonotonicTimeline`), `Rasm.Rhino.Document` (`GeometryHandle`, `Lease<T>`, `CrossingMode`, `GeometryCrossing`), `Modeling/curves.md` (`ModelClaim`, `ModelFact`), `Document/facts.md` (the ruled-plural `FactStream<TSlot, TBody>` sibling), RhinoCommon (`Rhino.Runtime.HostUtils`, `Rhino.Geometry` — `.api/api-rhinocommon-solids.md`, `.api/api-rhinocommon-geometry.md`), LanguageExt.Core (`Seq`, `FoldM`, `Traverse`, `Validation`), Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`, `[ComplexValueObject]` — `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
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

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SourceAxis {
    public static readonly SourceAxis Curve = new(key: "curve");
    public static readonly SourceAxis Brep = new(key: "brep");
    public static readonly SourceAxis Input = new(key: "input");
    public static readonly SourceAxis Region = new(key: "region");
    public static readonly SourceAxis Subject = new(key: "subject");
    public static readonly SourceAxis Kind = new(key: "kind");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BuildBody {
    private BuildBody() { }
    public sealed record Marks(Seq<Point3d> Points) : BuildBody;
    public sealed record UvRows(Seq<Point2d> Rows) : BuildBody;
    public sealed record Labels(Seq<(Point3d Location, string Text)> Rows) : BuildBody;
    public sealed record SourceMap(SourceAxis Axis, Seq<int> Rows) : BuildBody;
    public sealed record SourceGroups(SourceAxis Axis, Seq<Seq<int>> Groups) : BuildBody;
    public sealed record Tally(int Count) : BuildBody;
    public sealed record Measure(double Value) : BuildBody;
    public sealed record Code(int Value) : BuildBody;
    public sealed record Components(Seq<int> Indices) : BuildBody;
    public sealed record ComponentRows(Seq<ComponentIndex> Indices) : BuildBody;
    public sealed record Segments(Seq<Line> Lines) : BuildBody;
    public sealed record Faces(Seq<MeshFace> Rows) : BuildBody;
    public sealed record RegionSegments(Seq<(int Region, int Boundary, int Segment, int PlanarCurve, Interval Domain, bool Reversed)> Rows) : BuildBody;
    public sealed record Planes(Seq<Plane> Rows) : BuildBody;
    public sealed record Bounds(BoundingBox Value) : BuildBody;
    public sealed record Motion(Transform Value) : BuildBody;
    public sealed record Flag(bool Value) : BuildBody;
    public sealed record Text(string Value) : BuildBody;
}

// --- [MODELS] -----------------------------------------------------------------------------
// `ModelRuntime` carries the folder's complete execution band. The load root supplies its one timeline and every
// Modeling entry takes this same value, whether or not its native member consumes cancellation or progress.
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ModelRuntime {
    public Context Domain { get; }
    public MonotonicTimeline Timeline { get; }
    public CancellationToken Cancellation { get; }
    public Option<IProgress<int>> IntegerProgress { get; }
    public Option<IProgress<double>> ScalarProgress { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Context domain,
        ref MonotonicTimeline timeline,
        ref CancellationToken cancellation,
        ref Option<IProgress<int>> integerProgress,
        ref Option<IProgress<double>> scalarProgress) {
        if (domain is null || timeline is null) {
            validationError = new ValidationError("Model runtime requires a domain context and session timeline.");
        }
    }

    public static implicit operator Context(ModelRuntime runtime) => runtime.Domain;

    internal IProgress<int>? IntegerReporter => IntegerProgress.ValueUnsafe();

    internal IProgress<double>? ScalarReporter => ScalarProgress.ValueUnsafe();

    internal Fin<TOut> Await<TOut>(Func<Task<TOut>> work, Op key) => key.Catch(() => {
        Task<TOut> running = work();                                  // Exemption: the ONE async-to-rail collapse on the folder's synchronous spine
        running.Wait(Cancellation);
        return Fin.Succ(running.GetAwaiter().GetResult());
    }, token: Cancellation);
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

public readonly record struct BuildFact<TSlot>(TSlot Slot, BuildBody Body) where TSlot : notnull;

public readonly record struct BuildReceipt<TSlot> where TSlot : notnull {
    private readonly Seq<BuildFact<TSlot>> facts;

    private BuildReceipt(Seq<BuildFact<TSlot>> facts) => this.facts = facts;

    public static BuildReceipt<TSlot> Empty { get; } = new(facts: Seq<BuildFact<TSlot>>());

    public Seq<BuildFact<TSlot>> Facts => facts;

    public static BuildReceipt<TSlot> operator +(BuildReceipt<TSlot> left, BuildReceipt<TSlot> right) =>
        new(facts: left.facts + right.facts);

    public static BuildReceipt<TSlot> Of(TSlot slot, BuildBody body) =>
        new(facts: Seq(new BuildFact<TSlot>(Slot: slot, Body: body)));

    public Seq<T> Project<T>(TSlot slot, Func<BuildBody, Option<T>> select) =>
        facts.Filter(fact => fact.Slot.Equals(slot)).Choose(fact => select(fact.Body));
}

public readonly record struct BuildRun<TSlot>(Op Operation, Seq<GeometryHandle> Products, BuildReceipt<TSlot> Evidence)
    where TSlot : notnull;

public sealed record Built<TSlot> where TSlot : notnull {
    private Built(
        Seq<GeometryHandle> products, BuildReceipt<TSlot> evidence, Seq<BuildRun<TSlot>> runs, Option<BenchEvidence> bench) =>
        (Products, Evidence, Runs, Bench) = (products, evidence, runs, bench);

    // No slot carries an `init` accessor: `Stamped` is the one site that seats bench evidence and `Witnessed` the one
    // site that appends facts, so a call-site `with { … }` can neither fork the harvest nor desync `Runs` from the batch.
    public Seq<GeometryHandle> Products { get; }
    public BuildReceipt<TSlot> Evidence { get; }
    public Seq<BuildRun<TSlot>> Runs { get; }
    public Option<BenchEvidence> Bench { get; }

    internal Built<TSlot> Stamped(BenchEvidence bench) =>
        new(products: Products, evidence: Evidence, runs: Runs, bench: Some(bench));

    // Arm-local witness append: an arm that harvests a host out-channel AFTER minting its product folds the fact in
    // here so the batch projection and the run correspondence stay one truth.
    public Built<TSlot> Witnessed(BuildReceipt<TSlot> extra) =>
        new(products: Products,
            evidence: Evidence + extra,
            runs: Runs.Map(run => run with { Evidence = run.Evidence + extra }),
            bench: Bench);

    public static readonly Built<TSlot> Empty = new(
        products: Seq<GeometryHandle>(), evidence: BuildReceipt<TSlot>.Empty,
        runs: Seq<BuildRun<TSlot>>(), bench: Option<BenchEvidence>.None);

    public static Built<TSlot> Of(Op operation, Seq<GeometryHandle> Products, BuildReceipt<TSlot> Evidence) =>
        new(products: Products, evidence: Evidence,
            runs: Seq(new BuildRun<TSlot>(Operation: operation, Products: Products, Evidence: Evidence)),
            bench: Option<BenchEvidence>.None);

    public static Built<TSlot> operator +(Built<TSlot> left, Built<TSlot> right) =>
        new(products: left.Products + right.Products,
            evidence: left.Evidence + right.Evidence,
            runs: left.Runs + right.Runs,
            bench: left.Bench.IsSome ? left.Bench : right.Bench);

}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BenchBand {
    private static readonly Lazy<ProcessFingerprint> Fingerprint = new(static () => {
        HostUtils.GetCurrentProcessInfo(processName: out string process, processVersion: out Version version);
        return new ProcessFingerprint(
            Process: process,
            Version: version,
            PreRelease: HostUtils.IsPreRelease,
            // BCL process fact, never the `Rhino.UI` host static: an S1 Modeling owner reaching `PlatformServiceProvider`
            // is a strata breach, and `RuntimeInformation` answers the identical architecture with no host reach.
            Architecture: System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
            Processors: HostUtils.GetSystemProcessorCount());
    });

    public static Fin<(Fin<T> Outcome, BenchEvidence Evidence)> Measured<T>(
        MonotonicTimeline timeline, string operation, long inputScale, Func<Fin<T>> run) {
        Op op = Op.Of(name: operation);
        return from clock in op.Need(timeline)
               from opened in clock.Capture(key: op)
               let allocated = GC.GetAllocatedBytesForCurrentThread()
               let outcome = op.Catch(run)
               from closed in clock.Capture(key: op)
               from duration in clock.Elapsed(start: opened, end: closed, key: op)
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
    internal static Fin<TResult> Borrow<TNative, TResult>(GeometryHandle handle, Op key, Func<TNative, Fin<TResult>> body)
        where TNative : GeometryBase =>
        key.Need(handle).Bind(active => active.With(
            key: key,
            project: geometry => Optional(geometry as TNative)
                .ToFin(Fail: key.Unsupported(inputType: geometry.GetType(), outputType: typeof(TNative)))
                .Bind(body)));

    internal static Fin<TResult> BorrowMany<TNative, TResult>(
        Seq<GeometryHandle> handles, Op key, Func<Seq<TNative>, Fin<TResult>> body, bool allowEmpty = false)
        where TNative : GeometryBase =>
        handles.IsEmpty && !allowEmpty
            ? Fin.Fail<TResult>(error: key.InvalidInput())
            : Nested(handles: handles, borrowed: Seq<TNative>(), key: key, body: body);

    internal static Fin<GeometryHandle> Own(GeometryBase? built, Op key) =>
        Optional(built).ToFin(Fail: key.InvalidResult())
            .Map(fresh => new GeometryHandle(lease: new Lease<GeometryBase>.Owned(Value: fresh), mode: CrossingMode.Detach));

    internal static Fin<Seq<GeometryHandle>> OwnMany(IEnumerable<GeometryBase>? built, Op key, bool allowEmpty = false) =>
        Optional(built).Map(static values => toSeq(values)).ToFin(Fail: key.InvalidResult())
            .Bind(fresh => fresh.IsEmpty && !allowEmpty
                ? Fin.Fail<Seq<GeometryHandle>>(error: key.InvalidResult())
                : fresh.FoldM<Fin, Seq<GeometryHandle>>(Seq<GeometryHandle>(), (held, value) =>
                    Own(built: value, key: key)
                        .Map(handle => held.Add(value: handle))
                        .Rollback(release: () => Custody.Dispose(held: held, key: key), key: key)));

    internal static Fin<Seq<GeometryHandle>> OwnEach<TSource>(
        Seq<TSource> sources, Op key, Func<TSource, GeometryBase?> run, bool allowEmpty = false) =>
        sources.IsEmpty && !allowEmpty
            ? Fin.Fail<Seq<GeometryHandle>>(error: key.InvalidResult())
            : sources.FoldM<Fin, Seq<GeometryHandle>>(Seq<GeometryHandle>(), (held, source) =>
                key.Catch(() => Own(built: run(source), key: key))
                    .Map(handle => held.Add(value: handle))
                    .Rollback([.. held]));

    internal static Fin<Built<TSlot>> Folded<TSlot, TOp>(
        Context context, Seq<TOp> operations, Func<TOp, Context, Fin<Built<TSlot>>> apply)
        where TSlot : notnull =>
        operations.FoldM<Fin, Built<TSlot>>(Built<TSlot>.Empty, (held, operation) =>
            apply(operation, context)
                .Map(next => held + next)
                .Rollback([.. held.Products]));

    internal static Fin<Built<TSlot>> Entry<TSlot, TOp>(
        ModelRuntime runtime, Seq<TOp> operations,
        Func<TOp, Op, Fin<TOp>> admit,
        Func<TOp, Context, Fin<Built<TSlot>>> apply)
        where TSlot : notnull {
        Op op = Op.Of();
        return BenchBand.Measured(
            timeline: runtime.Timeline,
            operation: typeof(TOp).Name,
            inputScale: operations.Count,
            run: () =>
                from domain in Optional(runtime.Domain).ToFin(Fail: op.MissingContext())
                from _ in guard(!operations.IsEmpty, op.InvalidInput())
                from admitted in operations
                    .Traverse(operation => op.Need(operation)
                        .Bind(active => admit(active, op))
                        .ToValidation())
                    .As()
                    .ToFin()
                from built in Folded(context: domain, operations: admitted, apply: apply)
                select built)
            .Bind(measured => measured.Outcome.Map(built => built.Stamped(bench: measured.Evidence)));
    }

    internal static Fin<Built<TSlot>> Entry<TSlot, TOp>(
        ModelRuntime runtime, ReadOnlySpan<TOp> operations,
        Func<TOp, Op, Fin<TOp>> admit,
        Func<TOp, Context, Fin<Built<TSlot>>> apply)
        where TSlot : notnull =>
        Entry(runtime: runtime, operations: toSeq(operations.ToArray()), admit: admit, apply: apply);

    internal static Fin<Built<TSlot>> Single<TSlot>(
        Op op, TSlot slot, Func<GeometryBase?> run, CancellationToken token = default) where TSlot : notnull =>
        op.Catch(() => Own(built: run(), key: op).Map(owned => Built<TSlot>.Of(operation: op,
            Products: Seq(owned),
            Evidence: BuildReceipt<TSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)))), token: token);

    internal static Fin<Built<TSlot>> Many<TSlot>(
        Op op, TSlot slot, Func<System.Collections.Generic.IEnumerable<GeometryBase>?> run,
        bool allowEmpty = false, CancellationToken token = default)
        where TSlot : notnull =>
        op.Catch(() => OwnMany(built: run(), key: op, allowEmpty: allowEmpty).Map(owned => Built<TSlot>.Of(operation: op,
            Products: owned,
            Evidence: BuildReceipt<TSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)))), token: token);

    internal static Fin<Built<TSlot>> Kept<TSlot>(Op op, TSlot slot, GeometryBase working, BuildReceipt<TSlot> extra = default)
        where TSlot : notnull =>
        Own(built: working, key: op).Map(owned => Built<TSlot>.Of(operation: op,
            Products: Seq(owned),
            Evidence: BuildReceipt<TSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)) + extra));

    internal static Fin<Built<TSlot>> Owned<TSlot>(
        Op op, TSlot slot, GeometryBase working, Func<GeometryBase?> run, BuildReceipt<TSlot> extra = default)
        where TSlot : notnull =>
        op.Catch(() => Own(built: run(), key: op)).Bind(owned =>
            Relinquished(op: op, working: working, built: Built<TSlot>.Of(operation: op,
                Products: Seq(owned),
                Evidence: BuildReceipt<TSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)) + extra)));

    internal static Fin<Built<TSlot>> OwnedMany<TSlot>(
        Op op, TSlot slot, GeometryBase working, Func<IEnumerable<GeometryBase>?> run,
        bool allowEmpty = false, BuildReceipt<TSlot> extra = default)
        where TSlot : notnull =>
        op.Catch(() => OwnMany(built: run(), key: op, allowEmpty: allowEmpty)).Bind(owned =>
            Relinquished(op: op, working: working, built: Built<TSlot>.Of(operation: op,
                Products: owned,
                Evidence: BuildReceipt<TSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)) + extra)));

    internal static Fin<GeometryBase> Detached(GeometryBase? source, Op key) {
        GeometryBase? detached = null;
        Fin<GeometryBase> copied = from active in Optional(source).ToFin(Fail: key.InvalidResult())
                                   from copy in key.Catch(() => Optional(active.Duplicate())
                                       .ToFin(Fail: key.InvalidResult()))
                                   select (detached = copy)!;
        return copied
            .Settled(
                release: () => Custody.Dispose(held: Optional(source).ToSeq(), key: key),
                key: key)
            .BindFail(primary => Optional(detached).Match(
                Some: copy => Fin.Fail<GeometryBase>(error: primary).Rollback(
                    release: () => Custody.Dispose(held: Seq(copy), key: key), key: key),
                None: () => Fin.Fail<GeometryBase>(error: primary)));
    }

    internal static Fin<Seq<GeometryBase>> DetachedMany(IEnumerable<GeometryBase>? source, Op key) =>
        Optional(source).ToFin(Fail: key.InvalidResult()).Bind(rows => toSeq(rows).FoldM<Fin, Seq<GeometryBase>>(
            Seq<GeometryBase>(),
            (held, row) => Detached(source: row, key: key)
                .Map(held.Add)
                .Rollback(release: () => Custody.Dispose(held: held, key: key), key: key)));

    internal static Fin<Built<TSlot>> Mapped<TSlot>(
        Op op, TSlot slot, IEnumerable<GeometryBase>? built, int mapLength,
        params ReadOnlySpan<(SourceAxis Axis, int[]? Rows)> maps)
        where TSlot : notnull {
        Seq<GeometryBase> products = built is null ? Seq<GeometryBase>() : toSeq(built);
        Seq<(SourceAxis Axis, int[]? Rows)> captured = toSeq(maps.ToArray());
        return from _ in guard(
                       !products.IsEmpty && !captured.IsEmpty && captured.ForAll(row =>
                           row.Axis is not null && row.Rows is { Length: var length } && length == mapLength),
                       op.InvalidResult())
                   .ToFin()
                   .Rollback([.. products])
               from owned in OwnMany(built: products, key: op)
               let evidence = captured.Fold(BuildReceipt<TSlot>.Empty, (receipt, map) =>
                   receipt + BuildReceipt<TSlot>.Of(
                       slot: slot,
                       body: new BuildBody.SourceMap(Axis: map.Axis, Rows: toSeq(map.Rows!))))
               select Built<TSlot>.Of(
                   operation: op,
                   Products: owned,
                   Evidence: BuildReceipt<TSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)) + evidence);
    }

    internal static Fin<Built<TSlot>> Staged<TSlot>(
        Op op, params ReadOnlySpan<(TSlot Slot, System.Collections.Generic.IEnumerable<GeometryBase>? Built, bool AllowEmpty)> stages)
        where TSlot : notnull =>
        StageOwned(op: op, success: Option<bool>.None, extra: BuildReceipt<TSlot>.Empty, stages: stages.ToArray());

    internal static Fin<Built<TSlot>> Staged<TSlot>(
        Op op, bool success,
        params ReadOnlySpan<(TSlot Slot, System.Collections.Generic.IEnumerable<GeometryBase>? Built, bool AllowEmpty)> stages)
        where TSlot : notnull =>
        StageOwned(op: op, success: Some(success), extra: BuildReceipt<TSlot>.Empty, stages: stages.ToArray());

    internal static Fin<Built<TSlot>> Staged<TSlot>(
        Op op, bool success, BuildReceipt<TSlot> extra,
        params ReadOnlySpan<(TSlot Slot, System.Collections.Generic.IEnumerable<GeometryBase>? Built, bool AllowEmpty)> stages)
        where TSlot : notnull =>
        StageOwned(op: op, success: Some(success), extra: extra, stages: stages.ToArray());

    internal static Fin<Built<TSlot>> Staged<TSlot>(
        Op op, BuildReceipt<TSlot> extra,
        params ReadOnlySpan<(TSlot Slot, System.Collections.Generic.IEnumerable<GeometryBase>? Built, bool AllowEmpty)> stages)
        where TSlot : notnull =>
        StageOwned(op: op, success: Option<bool>.None, extra: extra, stages: stages.ToArray());

    private static Fin<Built<TSlot>> StageOwned<TSlot>(
        Op op, Option<bool> success, BuildReceipt<TSlot> extra,
        (TSlot Slot, System.Collections.Generic.IEnumerable<GeometryBase>? Built, bool AllowEmpty)[] stages)
        where TSlot : notnull {
        Fin<Seq<(TSlot Slot, Seq<GeometryHandle> Products)>> captured = toSeq(stages).FoldM<Fin, Seq<(TSlot, Seq<GeometryHandle>)>>(
            Seq<(TSlot, Seq<GeometryHandle>)>(),
            (held, stage) => op.Catch(() => OwnMany(
                    built: stage.Built ?? Seq<GeometryBase>().AsEnumerable(), key: op, allowEmpty: stage.AllowEmpty))
                .Map(products => held.Add((stage.Slot, products)))
                .Rollback(
                    release: () => Custody.Dispose(
                        held: held.Bind(static capturedStage => capturedStage.Item2), key: op),
                    key: op));
        return from rows in captured
               let products = rows.Bind(static stage => stage.Products)
               from _ in success.Match(
                       Some: verdict => op.Confirm(success: verdict),
                       None: static () => Fin.Succ(unit))
                   .Rollback(release: () => Custody.Dispose(held: products, key: op), key: op)
               let evidence = rows.Fold(BuildReceipt<TSlot>.Empty, (receipt, stage) =>
                   receipt + BuildReceipt<TSlot>.Of(
                       slot: stage.Slot,
                       body: new BuildBody.Tally(Count: stage.Products.Count)))
               select Built<TSlot>.Of(operation: op, Products: products, Evidence: evidence + extra);
    }

    private static Fin<Built<TSlot>> Relinquished<TSlot>(Op op, GeometryBase working, Built<TSlot> built)
        where TSlot : notnull =>
        op.Catch(() => {
            working.Dispose();
            return Fin.Succ(value: built);
        }).Rollback([.. built.Products]);

    private static Fin<TResult> Nested<TNative, TResult>(
        Seq<GeometryHandle> handles, Seq<TNative> borrowed, Op key, Func<Seq<TNative>, Fin<TResult>> body)
        where TNative : GeometryBase =>
        handles.Head.Case switch {
            GeometryHandle head => Borrow<TNative, TResult>(handle: head, key: key,
                body: native => Nested(handles: handles.Tail, borrowed: borrowed.Add(value: native), key: key, body: body)),
            _ => body(arg: borrowed),
        };
}
```

## [03]-[POLICY_FAMILY]

- Owner: `CapEnd`, `ShapeGrant`, and `OffsetGrant` are the page's three capability vocabularies; `SolidBooleanLaw` and `PlanarBooleanLaw` carry only the source arity and manifold policy consumed by each native boolean; `ArcDegree` and `ArcSlider` own the folder's non-rational arc approximation degree and slider bands; `FilletShape` closes the four `Brep.FilletSurfaceSettings` profile factories and `FilletLaw` binds one profile to its grant column; `SectionFilletProfile` closes the verified `SurfaceFilletBase` section family; `EdgeFillet` pairs an edge index with a constant or parameter-profiled radius law; `MatchLaw` carries the complete `MatchSrfSettings` policy; `PipeLaw` closes thin/thick constant and variable profiles; `SolidSeed` and `ExtrusionSeed` close heavy and lightweight construction; `ExtrusionRead` closes lightweight projections; `SolidEdit`, `TrimCutter`, and `ConnectSeed` close value-semantic editing.
- Law: a run of ADJACENT host bools is a grant set, never a payload run — `trim`/`extend` sit side by side in all four `FilletSurfaceSettings` factories and in every `SurfaceFilletBase` section static, `solid`/`extend`/`shrink` sit side by side in `CreateOffsetBrep`, `bothSides`/`createSolid` in `CreateFromOffsetFace`, and `capBottom`/`capTop` in four natives whose fifth spelling REVERSES the pair (`Extrusion.CreatePipeExtrusion` takes `capTop` first), so a call site transposes any of them in silence and the compiler never speaks. `ShapeGrant`, `OffsetGrant`, and `CapEnd` carry them as rank-ordered `CapabilitySet` columns read by name at the native, and `CapabilityLaw` states the corners a native cannot honour. A SOLITARY independent bool with no adjacent sibling stays a named bool on its owning case — `Setback`, `LocalBlending`, `FitRail`, `Flip`, `Smooth`, `TrimmedTriangles`, `SplitKinkyFaces`, and `ExtrusionSeed`'s single `Cap` are that form.
- Law: `CapEnd` is the folder's ONE cap vocabulary — the Brep cylinder, cone, and revolve seeds, both lightweight extrusion seeds, and the mesh rail's cylinder seed all name the same two sweep ends, so `Modeling/meshing.md` composes this roster and its own four-row `MeshCaps` mirror is deleted; `CapabilityLaw` carries each native's reachable corners, and the cone bars `Upper` because `Brep.CreateFromCone` publishes `capBottom` alone.
- Law: the fillet profile is the settings factory and the grant column is its sibling — every `Brep[]`-returning fillet/chamfer overload is obsolete, so `FilletLaw.Rig` is the only site naming `CreateRationalArcSettings`/`CreateNonRationalSettings`/`CreateG2BlendSettings`/`CreateChamferSettings`, the tolerance slot reads the regime, and `ContinueAcrossTangentFaces` is written once off the grant set. NAMED LOSS: the twelve per-case defaults the four profiles carried are gone and a caller now declares the grant set at construction; bought back by one reading of `Trim`/`Extend`/`AcrossTangents` for the whole family and by the four-arm `Switch` that existed only to re-read `AcrossTangents` off every case disappearing entirely.
- Law: the two boolean laws stay two owners and neither generalizes over the curve rail's — the host natives take a DIFFERENT operand arity per (operation, geometry class) cell: solid intersection and difference take two spreads, planar intersection and difference take exactly two breps, and the curve rail's difference takes one curve against a spread. A `BooleanLaw<TOperand>` would have to vary arity per case per instantiation, which a type parameter cannot express, so the arity IS the payload and the three unions survive with this discriminant named. `Split` additionally carries no manifold column because `Brep.CreateBooleanSplit` publishes no `manifoldOnly` parameter, and the three cases that do carry it declare it REQUIRED — a defaulted manifold policy chose a topology regime the caller never named.
- Law: the non-rational arc approximation has one folder vocabulary — `ArcDegree` rows the host's declared 3/4/5 degree space and `ArcSlider` admits the control-point displacement band once, so `CreateNonRationalSettings` and the curve rail's `CreateNonRationalArcBezier` read the same two owners and no arm carries a bare degree int or an unbounded slider double. Every native slider consumer — both fillet families and the arc bezier — refuses nothing and clamps to [-0.9, 0.9] silently, negatives producing distinct real geometry; the factories assign the arguments through without validating, so admission is this layer's alone and `ArcSlider`'s band is the effective band, keeping every admitted value one the host honors verbatim.
- Law: section fillets generate the degree space — `FilletDegree` and `HigherFilletDegree` rows carry their native constructor delegates and select which `SurfaceFilletBase` static runs, a distinct axis from `ArcDegree`'s argument-valued degree; `NonRationalCubic` carries tangent alone and `NonRationalHigher` requires tangent with inner slider, so invalid degree-payload combinations and nested degree dispatch are absent.
- Law: the section harvest is its own consequence — `SectionFillet` lands its fillet products on `SolidSlot.Sectioned` while the surface-fillet harvests land on `SolidSlot.Filleted`, so a consumer partitions section fillets from face and edge fillets off the receipt with no re-derivation.
- Law: parallel arrays are rows — an edge fillet enters as `(Edge, Law)` rows and the arm splits all-constant rows onto `CreateFilletEdges` and any-profiled rows onto `CreateFilletEdgesVariableRadius` with `BrepEdgeFilletDistance` rows minted per profile point, so equal-cardinality is proven by construction and the two native members stay one case.
- Law: `MatchLaw` collapses the host's split configuration — constructor continuities, combinable `MatchCapability` membership, and behavior-bearing `MatchRefinement` rig `EnableRefinement` once, so every policy has one native interpretation.
- Law: seeds carry no custody unless the source is geometry — analytic primitive cases hold value structs; `SolidSeed.CornerPoints` derives the triangular or quadrilateral native constructor from row cardinality; the surface, revolve, and mesh conversion cases hold leased handles borrowed only inside `Build`. `ExtrusionRead` projects the lightweight solid to brep, wireframe, detached cached mesh, station profile, wall geometry, or typed plane evidence through one operation.
- Law: admission is owner-local and evidence-shaped — every policy union and every policy value answers `IValidityEvidence.IsValid` off its generated `Switch` or the same fold its generated factory ran, so a new case breaks its owning owner's evidence at compile; the shape claims are the spine's `ModelClaim` and the kernel's `ValidityClaim` rows, never a page-local predicate class, and one `object?`-typed predicate switching over every policy type is the deleted form that let a new case pass unchecked.
- Law: `Rig` is a capability projection on the fault rail, NOT a `[Mapper]` transcription — the grant-column collapse consumed the field-for-field mirroring a Mapperly seat owns, so `FilletLaw.Rig` delegates the profile to `FilletShape.Bake`'s generated `Switch` over four host factories and writes one `Admits` read, and `MatchLaw.Rig` writes four of its five `MatchSrfSettings` slots off `MatchCapability` before handing the settings to `MatchRefinement.Apply`. Mapperly maps a declared source property onto a same-shaped target property on a pure signature and expresses neither the dispatch, the grant read, nor the `Fin` rail, so a mapper seated here carries a hand-written body per slot and maps nothing; the folder's `[Mapper]` seats stay on the pages transcribing a foreign record field-for-field.
- Growth: a new profile is one `FilletShape` case; a new primitive is one `SolidSeed` case; a new edit verb is one `SolidEdit` case; a new grant is one row on its owning vocabulary — the rail and every consumer read them with zero new surface.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `Brep.FilletSurfaceSettings` `[01]` and its four factories `:84`, `Brep.CreateFilletSurface`/`CreateFilletSurfaceCurve` `:80-81`, `SurfaceFilletBase` section family, `Brep.CreateBoolean*`, `Brep.CreatePlanar*`, `Brep.CreatePipe`/`CreateThickPipe`, `Brep.CreateOffsetBrep`, `MatchSrfSettings`), RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `Box`, `Cone`, `Cylinder`, `Torus`, `Sphere`, `ComponentIndex`, `Continuity`, `BlendType`, `RailType`, `PipeCapMode`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`), kernel `Domain/rails` (`ValidityClaim`, `IValidityEvidence`, `Op`, `Fin`), kernel `Domain/context` (`Context`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The two sweep ends five natives name with four different argument spellings — and one, `CreatePipeExtrusion`,
// with the pair REVERSED. Membership is read by name at every call, so the transposition is unspellable.
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
    // `Brep.CreateBooleanSplit` publishes no `manifoldOnly` parameter, so the column stops here rather than
    // riding a base a fourth native cannot honour.
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

// Sibling of `SolidBooleanLaw` by name and NOT by payload: the planar natives take exactly two breps where the
// solid natives take two spreads, so the operand arity is case payload and no generic operand parameter fits.
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
    // Live-proven effective band: every native slider consumer clamps to [-0.9, 0.9] silently, so wider admission passes dead input.
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
    // `smooth` is the one independent host bool on this native with no adjacent sibling, so it stays named.
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
    // `trimmedTriangles` is a conversion fidelity knob, not a cap end, and stands alone at the native.
    public sealed record FromMesh(GeometryHandle Source, bool TrimmedTriangles = true) : SolidSeed;

    // `Brep.CreateFromCone` publishes `capBottom` alone, so `Upper` names a corner no cone run can honour.
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

    internal Fin<GeometryHandle> Build(Context domain, Op key) =>
        Switch(
            context: (Domain: domain, Op: key),
            ofBox: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromBox(box: seed.Value), key: ctx.Op)),
            ofBounds: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromBox(box: seed.Value), key: ctx.Op)),
            ofCorners: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: Brep.CreateFromBox(corners: Seq(seed.A, seed.B, seed.C, seed.D, seed.E, seed.F, seed.G, seed.H).AsIterable()),
                key: ctx.Op)),
            ofCylinder: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: Brep.CreateFromCylinder(
                    cylinder: seed.Value,
                    capBottom: seed.Caps.Admits(capability: CapEnd.Lower),
                    capTop: seed.Caps.Admits(capability: CapEnd.Upper)),
                key: ctx.Op)),
            ofCone: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: Brep.CreateFromCone(cone: seed.Value, capBottom: seed.Caps.Admits(capability: CapEnd.Lower)), key: ctx.Op)),
            ofTorus: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromTorus(torus: seed.Value), key: ctx.Op)),
            ofSphere: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromSphere(sphere: seed.Value), key: ctx.Op)),
            quadSphere: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateQuadSphere(sphere: seed.Value), key: ctx.Op)),
            baseball: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: Brep.CreateBaseballSphere(center: seed.Center, radius: seed.Radius, tolerance: ctx.Domain.Absolute.Value), key: ctx.Op)),
            cornerPoints: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: seed.Values.Count switch {
                    3 => Brep.CreateFromCornerPoints(
                        corner1: seed.Values[0], corner2: seed.Values[1], corner3: seed.Values[2],
                        tolerance: ctx.Domain.Absolute.Value),
                    4 => Brep.CreateFromCornerPoints(
                        corner1: seed.Values[0], corner2: seed.Values[1], corner3: seed.Values[2], corner4: seed.Values[3],
                        tolerance: ctx.Domain.Absolute.Value),
                    _ => null,
                }, key: ctx.Op)),
            fromSurface: static (ctx, seed) => ModelGate.Borrow<Surface, GeometryHandle>(handle: seed.Source, key: ctx.Op,
                body: surface => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromSurface(surface: surface), key: ctx.Op))),
            fromRevolve: static (ctx, seed) => ModelGate.Borrow<RevSurface, GeometryHandle>(handle: seed.Source, key: ctx.Op,
                body: surface => ctx.Op.Catch(() => ModelGate.Own(
                    built: Brep.CreateFromRevSurface(
                        surface: surface,
                        capStart: seed.Caps.Admits(capability: CapEnd.Lower),
                        capEnd: seed.Caps.Admits(capability: CapEnd.Upper)),
                    key: ctx.Op))),
            fromMesh: static (ctx, seed) => ModelGate.Borrow<Mesh, GeometryHandle>(handle: seed.Source, key: ctx.Op,
                body: mesh => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromMesh(mesh: mesh, trimmedTriangles: seed.TrimmedTriangles), key: ctx.Op))));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrusionSeed : IValidityEvidence {
    private ExtrusionSeed() { }
    // The three profile and box natives publish ONE `cap` bool covering both ends, so a two-end set would spell
    // a distinction the host cannot receive; the cylinder and pipe natives publish the pair and take the column.
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

    internal Fin<GeometryHandle> Build(Op key) =>
        Switch(
            context: key,
            profile: static (op, seed) => ModelGate.Borrow<Curve, GeometryHandle>(handle: seed.PlanarProfile, key: op,
                body: profile => op.Catch(() => ModelGate.Own(
                    built: Extrusion.Create(planarCurve: profile, height: seed.Height, cap: seed.Cap),
                    key: op))),
            framedProfile: static (op, seed) => ModelGate.Borrow<Curve, GeometryHandle>(handle: seed.PlanarProfile, key: op,
                body: profile => op.Catch(() => ModelGate.Own(
                    built: Extrusion.Create(curve: profile, plane: seed.Frame, height: seed.Height, cap: seed.Cap), key: op))),
            ofBox: static (op, seed) => op.Catch(() => ModelGate.Own(built: Extrusion.CreateBoxExtrusion(box: seed.Value, cap: seed.Cap), key: op)),
            ofCylinder: static (op, seed) => op.Catch(() => ModelGate.Own(
                built: Extrusion.CreateCylinderExtrusion(
                    cylinder: seed.Value,
                    capBottom: seed.Caps.Admits(capability: CapEnd.Lower),
                    capTop: seed.Caps.Admits(capability: CapEnd.Upper)),
                key: op)),
            // This native REVERSES the pair the cylinder native declares — the set is read by name, so the
            // reversal is a call-site detail no caller can get wrong.
            ofPipe: static (op, seed) => op.Catch(() => ModelGate.Own(
                built: Extrusion.CreatePipeExtrusion(
                    cylinder: seed.Value, otherRadius: seed.OtherRadius,
                    capTop: seed.Caps.Admits(capability: CapEnd.Upper),
                    capBottom: seed.Caps.Admits(capability: CapEnd.Lower)),
                key: op)));
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
    public sealed record ProfilePlane(double Station) : ExtrusionRead;
    public sealed record PathPlane(double Station) : ExtrusionRead;

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
            Enum.IsDefined(read.Component.ComponentIndexType)),
        profilePlane: static read => ValidityClaim.Finite(value: read.Station),
        pathPlane: static read => ValidityClaim.Finite(value: read.Station));
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

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct EdgeFillet(int Edge, RadiusLaw Law) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Edge, floor: 0), ValidityClaim.Evidence(evidence: Optional(Law)));
}

// The surface-fillet policy: ONE profile bound to the grant column all four factories read. The grants left the
// four profile cases because they are identical on every one of them and because `trim`/`extend` are adjacent
// positional bools at each factory; `ContinueAcrossTangentFaces` is the post-factory knob, written here once.
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

    internal Fin<Brep.FilletSurfaceSettings> Rig(Context domain, Op key) =>
        key.Catch(() => {
            Brep.FilletSurfaceSettings settings = Shape.Bake(domain: domain, grants: Grants);
            settings.ContinueAcrossTangentFaces = Grants.Admits(capability: ShapeGrant.AcrossTangents);
            return Fin.Succ(value: settings);
        });
}

// The section family reads the same two adjacent bools off the same vocabulary; `AcrossTangents` is barred
// because no `SurfaceFilletBase` static publishes the knob.
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

    internal Fin<MatchSrfSettings> Rig(Context domain, Op key) =>
        key.Catch(() => {
            MatchSrfSettings settings = new(match: Match, otherEnd: OtherEnd) {
                Average = Capabilities.Admits(capability: MatchCapability.Average),
                MatchClosestPoints = Capabilities.Admits(capability: MatchCapability.ClosestPoints),
                PreserveIso = PreserveIso,
                ReverseMatchDirection = Capabilities.Admits(capability: MatchCapability.ReverseMatch),
                ReverseAverageTargetDirection = Capabilities.Admits(capability: MatchCapability.ReverseTarget),
            };
            _ = Refinement.Apply(settings: settings, domain: domain);
            return Fin.Succ(value: settings);
        });
}
```

## [04]-[OPERATION_RAIL]

- Owner: `SolidSlot` `[SmartEnum<int>]` — the consequence vocabulary; `SolidOp` `[Union]` `[GenerateUnionOps]` — the whole verified solid-construction verb roster, each case carrying its own generated `SelfOp`; `Solids` — the one entry folding any operation spread into one `Built<SolidSlot>`.
- Law: every native side-channel is a fact — union diagnostics land as three `Marks` facts (naked edges, bad intersections, non-manifold edges) and survive an empty union, so a union that yields no solid still carries the naked/bad/non-manifold points that explain the absence; join groups land as `SourceGroups`, split tolerance escalation lands as `Flag`, and offset blends and walls cross as products behind per-class tallies so the flat product seq partitions by count.
- Law: the rail is value-semantic — `Edit` duplicates its borrowed brep, runs the in-place host member on the working copy, and owns the copy (or the member's returned brep, disposing the copy) as the product; no operation mutates the geometry behind an input handle.
- Law: boolean payload shape is admission — `SolidBooleanLaw` carries each set topology and manifold grant, and `PlanarBooleanLaw` carries union spread or exact pair.
- Law: source correspondence is a UNIFORM receipt concern answered per arm by what the host publishes, never a per-operation policy: difference always takes `CreateBooleanDifferenceWithIndexMap` so its map is never discarded, and union, intersection, and split emit NO `SourceMap` because the Brep boolean family publishes an index map on that one member alone — the absence is a stated host ceiling and a derived, re-matched, or nearest-operand map standing in for it fabricates a correspondence nothing measured. `BuildReceipt` is a fact stream, so an unmeasured axis is a fact the arm never emits rather than a slot holding a stand-in; the mesh rail at `Modeling/meshing.md` is the counterpart where the host DOES publish uniformly, and its four arms all land `SourceGroups` off the one `MeshBooleanOptions` map.
- Law: `Solids.Build` admits the complete operation spread before dispatch and every refusal NAMES its axis — `SolidOp.Admitted` dispatches through the generated total `Switch` into `ModelClaim.Admits`, so a request breaching four constraints answers four `KernelFault.InvalidInput(Key, Axis)` errors on one rail, a new case breaks the compile instead of falling through a catch-all, and live collection bounds remain inside their borrow windows.
- Law: the two blend cases stay two cases — `Brep.CreateBlendSurface` spans an edge INTERVAL on each side and answers a spread, while `Brep.CreateBlendShape` reads a single parameter on each side and answers one surface, so a fused case would carry a payload half of every caller leaves unread and an arity the arm re-decides. The reversal PAIR on both is the spine's `PairPosture`, not two positional bools a call site transposes.
- Law: face-addressed operations index inside the borrow — a case carries the handle with face or edge indices, the arm guards the index against the live `Faces`/`Edges` count, and no `BrepFace` or `BrepEdge` ever crosses a case payload.
- Boundary: sweep, loft, and patch construction is the lofting page's rail; freeform surface and curve construction is the surfaces and curves pages'; this rail owns the solid-topology verbs alone.
- Boundary: this rail is unpaced because no Brep construction member consumes cancellation or progress; it still receives `ModelRuntime` for the shared context and timeline. Pacing columns are consumed only by meshing, lofting, and projection members that publish them.
- Growth: a new host solid verb is one case with its arm; the spine, the receipt, and every consumer read it with zero new surface.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — the `Brep` boolean, fillet, blend, offset, shell, pipe, join, merge, match, split, trim, and extrusion rosters `:44-160`), RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `Extrusion`, `ComponentIndex`, `MeshType`, `ExtrudeCornerType`), kernel `Domain/rails` (`Op`, `KernelFault.InvalidInput(Key, Axis)`, `[GenerateUnionOps]` + generated `SelfOp`, `Fin`), kernel `Domain/validation` (`CapabilitySet`), kernel `Domain/context` (`Context`), `Modeling/curves.md` (`ModelClaim`, `ModelFact`, `PairPosture`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SolidSlot {
    public static readonly SolidSlot Booled = new(key: 0);
    public static readonly SolidSlot NakedEdges = new(key: 1);
    public static readonly SolidSlot BadIntersections = new(key: 2);
    public static readonly SolidSlot NonManifoldEdges = new(key: 3);
    public static readonly SolidSlot Solidified = new(key: 4);
    public static readonly SolidSlot Filleted = new(key: 5);
    public static readonly SolidSlot Trimmed0 = new(key: 6);
    public static readonly SolidSlot Trimmed1 = new(key: 7);
    public static readonly SolidSlot Blended = new(key: 8);
    public static readonly SolidSlot Sectioned = new(key: 9);
    public static readonly SolidSlot Offsets = new(key: 10);
    public static readonly SolidSlot Blends = new(key: 11);
    public static readonly SolidSlot Walls = new(key: 12);
    public static readonly SolidSlot Shelled = new(key: 13);
    public static readonly SolidSlot Piped = new(key: 14);
    public static readonly SolidSlot Seeded = new(key: 15);
    public static readonly SolidSlot Tapered = new(key: 16);
    public static readonly SolidSlot Planar = new(key: 17);
    public static readonly SolidSlot EdgeSurfaced = new(key: 18);
    public static readonly SolidSlot PlaneTrimmed = new(key: 19);
    public static readonly SolidSlot Joined = new(key: 20);
    public static readonly SolidSlot Merged = new(key: 21);
    public static readonly SolidSlot Matched = new(key: 22);
    public static readonly SolidSlot Extended = new(key: 23);
    public static readonly SolidSlot SplitApart = new(key: 24);
    public static readonly SolidSlot Cut = new(key: 25);
    public static readonly SolidSlot Edited = new(key: 26);
    public static readonly SolidSlot Simplified = new(key: 27);
    public static readonly SolidSlot Extruded = new(key: 28);
    public static readonly SolidSlot FilletFace0 = new(key: 29);
    public static readonly SolidSlot FilletFace1 = new(key: 30);
    public static readonly SolidSlot Projected = new(key: 31);
}

[GenerateUnionOps]
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

    // `CreateOffsetBrep` reads solid/extend/shrink and `CreateFromOffsetFace` reads bothSides/createSolid, so each
    // site bars the rows its native cannot receive rather than accepting a grant nothing reads.
    private static readonly CapabilityLaw<OffsetGrant> SolidOffsetGrants =
        CapabilityLaw<OffsetGrant>.Forbidden(barred: Seq(CapabilitySet<OffsetGrant>.Of(OffsetGrant.BothSides)));

    private static readonly CapabilityLaw<OffsetGrant> FaceOffsetGrants = CapabilityLaw<OffsetGrant>.Forbidden(
        barred: Seq(CapabilitySet<OffsetGrant>.Of(OffsetGrant.Extend), CapabilitySet<OffsetGrant>.Of(OffsetGrant.Shrink)));

    // Every nested policy answers its OWN `IsValid` off its generated `Switch`, so a new case breaks its owning
    // owner's evidence at compile; this arm reads case-local shape and NAMES the axis of every refusal.
    internal Fin<SolidOp> Admitted(Op key) =>
        Switch(
            context: key,
            boolean: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Law), row.Law is { IsValid: true })),
            planarBoolean: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Plane), row.Plane.IsValid), (nameof(row.Law), row.Law is { IsValid: true })),
            solidify: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Open), ModelClaim.Handles(handles: row.Open))),
            filletEdges: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Edges), ModelClaim.Rows(rows: row.Edges, claim: static edge => edge.IsValid)),
                (nameof(row.Blend), Enum.IsDefined(row.Blend)), (nameof(row.Rail), Enum.IsDefined(row.Rail))),
            faceFillet: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.FirstUv), row.FirstUv.IsValid),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.SecondUv), row.SecondUv.IsValid), (nameof(row.Law), row.Law.IsValid)),
            faceCurveFillet: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Host), ModelClaim.Handle(handle: row.Host)),
                (nameof(row.Face), ValidityClaim.CountAtLeast(count: row.Face, floor: 0)),
                (nameof(row.Uv), row.Uv.IsValid), (nameof(row.Along), ModelClaim.Handle(handle: row.Along)),
                (nameof(row.Parameter), ValidityClaim.Finite(value: row.Parameter)),
                (nameof(row.Law), row.Law.IsValid)),
            sectionFillet: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.FirstUv), row.FirstUv.IsValid),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.SecondUv), row.SecondUv.IsValid), (nameof(row.Law), row.Law is { IsValid: true })),
            blendSurface: static (op, row) => ModelClaim.Admits(row, op,
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
            blendSection: static (op, row) => ModelClaim.Admits(row, op,
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
            offsetSolid: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.Grants), SolidOffsetGrants.Admit(held: row.Grants).IsSucc)),
            faceOffset: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Face), ValidityClaim.CountAtLeast(count: row.Face, floor: 0)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.Grants), FaceOffsetGrants.Admit(held: row.Grants).IsSucc)),
            shell: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.FacesToRemove), ModelClaim.Rows(
                    rows: row.FacesToRemove, claim: static face => ValidityClaim.CountAtLeast(count: face, floor: 0), allowEmpty: true)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0))),
            pipe: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail), ModelClaim.Handle(handle: row.Rail)),
                (nameof(row.Law), row.Law is { IsValid: true }), (nameof(row.Cap), Enum.IsDefined(row.Cap))),
            seed: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Value), row.Value is { IsValid: true })),
            taperedExtrude: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Profile), ModelClaim.Handle(handle: row.Profile)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.Direction), ValidityClaim.Direction(value: row.Direction)),
                (nameof(row.BasePoint), ValidityClaim.Finite(value: row.BasePoint)),
                (nameof(row.DraftAngleRadians), ValidityClaim.Finite(value: row.DraftAngleRadians)),
                (nameof(row.Corner), Enum.IsDefined(row.Corner))),
            taperedExtrudeRef: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Profile), ModelClaim.Handle(handle: row.Profile)),
                (nameof(row.Direction), ValidityClaim.Direction(value: row.Direction)),
                (nameof(row.Distance), ValidityClaim.All(ValidityClaim.Finite(value: row.Distance), row.Distance != 0.0)),
                (nameof(row.DraftAngleRadians), ValidityClaim.Finite(value: row.DraftAngleRadians)),
                (nameof(row.Reference), row.Reference.IsValid)),
            planarFill: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Loops), ModelClaim.Handles(handles: row.Loops))),
            edgeSurface: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rails), ModelClaim.Handles(handles: row.Rails))),
            trimmedPlane: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Frame), row.Frame.IsValid), (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves))),
            join: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Targets), ModelClaim.Handles(handles: row.Targets))),
            joinEdges: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstEdge), ValidityClaim.CountAtLeast(count: row.FirstEdge, floor: 0)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondEdge), ValidityClaim.CountAtLeast(count: row.SecondEdge, floor: 0))),
            merge: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Targets), ModelClaim.Handles(handles: row.Targets))),
            mergeFaces: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.Law), row.Law is { IsValid: true })),
            match: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Edge), ValidityClaim.CountAtLeast(count: row.Edge, floor: 0)),
                (nameof(row.TargetCurves), ModelClaim.Handles(handles: row.TargetCurves)),
                (nameof(row.Law), row.Law is { IsValid: true })),
            extendToConnect: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.At), row.At is { IsValid: true })),
            splitPieces: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            splitBy: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Cutters), ModelClaim.Handles(handles: row.Cutters))),
            trim: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Cutter), row.Cutter is { IsValid: true })),
            cutUp: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves))),
            copyTrims: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.TrimSource), ModelClaim.Handle(handle: row.TrimSource)),
                (nameof(row.Face), ValidityClaim.CountAtLeast(count: row.Face, floor: 0)),
                (nameof(row.SurfaceSource), ModelClaim.Handle(handle: row.SurfaceSource))),
            edit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Verb), row.Verb is { IsValid: true })),
            simplify: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            lite: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Value), row.Value is { IsValid: true })),
            liteProfiled: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Outer), ModelClaim.Handle(handle: row.Outer)),
                (nameof(row.Inners), ModelClaim.Handles(handles: row.Inners, allowEmpty: true)),
                (nameof(row.Path), ValidityClaim.WhenPresent(facet: row.Path, claim: static path => ValidityClaim.All(
                    ValidityClaim.Finite(value: path.A), ValidityClaim.Finite(value: path.B),
                    ValidityClaim.Direction(value: path.Up))))),
            liteRead: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Read), row.Read is { IsValid: true })));

    internal Fin<Built<SolidSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            boolean: static (model, edit) => {
                Op op = Boolean.SelfOp;
                return edit.Law.Switch(
                    state: (Model: model, Op: op),
                    union: static (ctx, law) => ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.Breps, key: ctx.Op, body: breps =>
                        ctx.Op.Catch(() => {
                                Brep[] products = Brep.CreateBooleanUnion(
                                    breps: breps.AsIterable(), tolerance: ctx.Model.Absolute.Value, manifoldOnly: law.ManifoldOnly,
                                    nakedEdgePoints: out Point3d[] naked, badIntersectionPoints: out Point3d[] bad, nonManifoldEdgePoints: out Point3d[] nonManifold);
                                // A silent diagnostic channel lands NO fact: an absent array and an empty one are
                                // different answers, and `?? []` certifies the first as the second.
                                Option<Seq<Brep>> productRows = ModelFact.Answered(channel: products);
                                Option<Seq<Point3d>> nakedRows = ModelFact.Answered(channel: naked);
                                Option<Seq<Point3d>> badRows = ModelFact.Answered(channel: bad);
                                Option<Seq<Point3d>> nonManifoldRows = ModelFact.Answered(channel: nonManifold);
                                return guard(
                                        Seq(productRows.Map(static rows => rows.Count), nakedRows.Map(static rows => rows.Count),
                                                badRows.Map(static rows => rows.Count), nonManifoldRows.Map(static rows => rows.Count))
                                            .Somes().Exists(static count => count > 0),
                                        ctx.Op.InvalidResult())
                                    .ToFin()
                                    .Bind(_ => ModelGate.OwnMany(built: productRows.IfNone(Seq<Brep>()), key: ctx.Op, allowEmpty: true))
                                    .Map(owned => Built<SolidSlot>.Of(operation: ctx.Op,
                                    Products: owned,
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Booled, body: new BuildBody.Tally(Count: owned.Count))
                                        + ModelFact.Channel(slot: SolidSlot.NakedEdges, value: nakedRows.Map(static rows => (BuildBody)new BuildBody.Marks(Points: rows)))
                                        + ModelFact.Channel(slot: SolidSlot.BadIntersections, value: badRows.Map(static rows => (BuildBody)new BuildBody.Marks(Points: rows)))
                                        + ModelFact.Channel(slot: SolidSlot.NonManifoldEdges, value: nonManifoldRows.Map(static rows => (BuildBody)new BuildBody.Marks(Points: rows)))));
                            })),
                    intersection: static (ctx, law) => ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.First, key: ctx.Op, body: first =>
                        ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.Second, key: ctx.Op, body: second =>
                            ModelGate.Many(ctx.Op, SolidSlot.Booled, () => Brep.CreateBooleanIntersection(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(),
                                tolerance: ctx.Model.Absolute.Value, manifoldOnly: law.ManifoldOnly)))),
                    difference: static (ctx, law) => ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.First, key: ctx.Op, body: first =>
                        ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.Second, key: ctx.Op, body: second =>
                            ctx.Op.Catch(() => {
                                Brep[] products = Brep.CreateBooleanDifferenceWithIndexMap(
                                    firstSet: first.AsIterable(), secondSet: second.AsIterable(),
                                    tolerance: ctx.Model.Absolute.Value, manifoldOnly: law.ManifoldOnly, indexMap: out int[] map);
                                return ModelGate.OwnMany(built: products, key: ctx.Op).Map(owned => Built<SolidSlot>.Of(operation: ctx.Op,
                                    Products: owned,
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Booled, body: new BuildBody.Tally(Count: owned.Count))
                                        + ModelFact.Channel(slot: SolidSlot.Booled, value: ModelFact.Answered(channel: map)
                                            .Map(static rows => (BuildBody)new BuildBody.SourceMap(Axis: SourceAxis.Input, Rows: rows)))));
                            }))),
                    split: static (ctx, law) => ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.First, key: ctx.Op, body: first =>
                        ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.Second, key: ctx.Op, body: second =>
                            ModelGate.Many(ctx.Op, SolidSlot.Booled, () => Brep.CreateBooleanSplit(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(), tolerance: ctx.Model.Absolute.Value)))));
            },
            planarBoolean: static (model, edit) => {
                Op op = PlanarBoolean.SelfOp;
                return edit.Law.Switch(
                    state: (Edit: edit, Model: model, Op: op),
                    union: static (ctx, law) => ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: law.Breps, key: ctx.Op, body: breps =>
                        ModelGate.Many(ctx.Op, SolidSlot.Booled, () => Brep.CreatePlanarUnion(
                            breps: breps.AsIterable(), plane: ctx.Edit.Plane, tolerance: ctx.Model.Absolute.Value))),
                    intersection: static (ctx, law) => ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: law.First, key: ctx.Op, body: first =>
                        ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: law.Second, key: ctx.Op, body: second =>
                            ModelGate.Many(ctx.Op, SolidSlot.Booled, () => Brep.CreatePlanarIntersection(
                                b0: first, b1: second, plane: ctx.Edit.Plane, tolerance: ctx.Model.Absolute.Value)))),
                    difference: static (ctx, law) => ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: law.First, key: ctx.Op, body: first =>
                        ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: law.Second, key: ctx.Op, body: second =>
                            ModelGate.Many(ctx.Op, SolidSlot.Booled, () => Brep.CreatePlanarDifference(
                                b0: first, b1: second, plane: ctx.Edit.Plane, tolerance: ctx.Model.Absolute.Value)))));
            },
            solidify: static (model, edit) => {
                Op op = Solidify.SelfOp;
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Open, key: op, body: open =>
                    ModelGate.Many(op, SolidSlot.Solidified, () => Brep.CreateSolid(breps: open.AsIterable(), tolerance: model.Absolute.Value)));
            },
            filletEdges: static (model, edit) => {
                Op op = FilletEdges.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    from _ in guard(edit.Edges.ForAll(row => row.Edge < target.Edges.Count), op.InvalidInput())
                    from built in edit.Edges.Exists(static row => row.Law is RadiusLaw.Profiled)
                        ? ModelGate.Many(op, SolidSlot.Filleted, () => Brep.CreateFilletEdgesVariableRadius(
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
                        : ModelGate.Many(op, SolidSlot.Filleted, () => Brep.CreateFilletEdges(
                            brep: target,
                            edgeIndices: edit.Edges.Map(static row => row.Edge).AsIterable(),
                            startRadii: edit.Edges.Map(static row => ((RadiusLaw.Constant)row.Law).Start).AsIterable(),
                            endRadii: edit.Edges.Map(static row => ((RadiusLaw.Constant)row.Law).End).AsIterable(),
                            blendType: edit.Blend, railType: edit.Rail, setbackFillets: edit.Setback,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))
                    select built);
            },
            faceFillet: static (model, edit) => {
                Op op = FaceFillet.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(edit.FirstFace < first.Faces.Count && edit.SecondFace < second.Faces.Count, op.InvalidInput())
                        from settings in edit.Law.Rig(domain: model, key: op)
                        from built in op.Catch(() =>
                            op.Confirm(success: Brep.CreateFilletSurface(
                                face0: first.Faces[edit.FirstFace], uv0: edit.FirstUv,
                                face1: second.Faces[edit.SecondFace], uv1: edit.SecondUv,
                                settings: settings, results: out Brep.FilletSurfaceResults results))
                            .Bind(_ => Harvested(results: results, op: op)))
                        select built));
            },
            faceCurveFillet: static (model, edit) => {
                Op op = FaceCurveFillet.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Host, key: op, body: host =>
                    ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Along, key: op, body: along =>
                        from _ in guard(edit.Face < host.Faces.Count, op.InvalidInput())
                        from settings in edit.Law.Rig(domain: model, key: op)
                        from built in op.Catch(() =>
                            op.Confirm(success: Brep.CreateFilletSurfaceCurve(
                                face: host.Faces[edit.Face], uv: edit.Uv, curve: along, t: edit.Parameter,
                                settings: settings, results: out Brep.FilletSurfaceResults results))
                            .Bind(_ => Harvested(results: results, op: op)))
                        select built));
            },
            sectionFillet: static (model, edit) => {
                Op op = SectionFillet.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstFace < first.Faces.Count && edit.SecondFace < second.Faces.Count,
                            op.InvalidInput())
                        from built in SectionFilleted(
                            first: first.Faces[edit.FirstFace], firstUv: edit.FirstUv,
                            second: second.Faces[edit.SecondFace], secondUv: edit.SecondUv,
                            law: edit.Law, tolerance: model.Absolute.Value, op: op)
                        select built));
            },
            blendSurface: static (_, edit) => {
                Op op = BlendSurface.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstFace < first.Faces.Count && edit.FirstEdge < first.Edges.Count
                            && edit.SecondFace < second.Faces.Count && edit.SecondEdge < second.Edges.Count,
                            op.InvalidInput())
                        from built in ModelGate.Many(op, SolidSlot.Blended, () => Brep.CreateBlendSurface(
                            face0: first.Faces[edit.FirstFace], edge0: first.Edges[edit.FirstEdge], domain0: edit.FirstDomain, rev0: edit.Reverse.First, continuity0: edit.FirstContinuity,
                            face1: second.Faces[edit.SecondFace], edge1: second.Edges[edit.SecondEdge], domain1: edit.SecondDomain, rev1: edit.Reverse.Second, continuity1: edit.SecondContinuity))
                        select built));
            },
            blendSection: static (_, edit) => {
                Op op = BlendSection.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstFace < first.Faces.Count && edit.FirstEdge < first.Edges.Count
                            && edit.SecondFace < second.Faces.Count && edit.SecondEdge < second.Edges.Count,
                            op.InvalidInput())
                        from built in ModelGate.Single(op, SolidSlot.Blended, () => Brep.CreateBlendShape(
                            face0: first.Faces[edit.FirstFace], edge0: first.Edges[edit.FirstEdge], t0: edit.FirstT, rev0: edit.Reverse.First, continuity0: edit.FirstContinuity,
                            face1: second.Faces[edit.SecondFace], edge1: second.Edges[edit.SecondEdge], t1: edit.SecondT, rev1: edit.Reverse.Second, continuity1: edit.SecondContinuity))
                        select built));
            },
            offsetSolid: static (model, edit) => {
                Op op = OffsetSolid.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    op.Catch(() => {
                        Brep[] offsets = Brep.CreateOffsetBrep(
                            brep: target, distance: edit.Distance,
                            solid: edit.Grants.Admits(capability: OffsetGrant.Solid),
                            extend: edit.Grants.Admits(capability: OffsetGrant.Extend),
                            shrink: edit.Grants.Admits(capability: OffsetGrant.Shrink),
                            tolerance: model.Absolute.Value, outBlends: out Brep[] blends, outWalls: out Brep[] walls);
                        return ModelGate.Staged(op: op,
                            (SolidSlot.Offsets, offsets, false),
                            (SolidSlot.Blends, blends, true),
                            (SolidSlot.Walls, walls, true));
                    }));
            },
            faceOffset: static (model, edit) => {
                Op op = FaceOffset.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    from _ in guard(edit.Face < target.Faces.Count, op.InvalidInput())
                    from built in ModelGate.Single(op, SolidSlot.Offsets, () => Brep.CreateFromOffsetFace(
                        face: target.Faces[edit.Face], offsetDistance: edit.Distance,
                        offsetTolerance: model.Absolute.Value,
                        bothSides: edit.Grants.Admits(capability: OffsetGrant.BothSides),
                        createSolid: edit.Grants.Admits(capability: OffsetGrant.Solid)))
                    select built);
            },
            shell: static (model, edit) => {
                Op op = Shell.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    from _ in guard(edit.FacesToRemove.ForAll(face => face < target.Faces.Count), op.InvalidInput())
                    from built in ModelGate.Many(op, SolidSlot.Shelled, () => Brep.CreateShell(
                        brep: target, facesToRemove: edit.FacesToRemove.AsIterable(), distance: edit.Distance, tolerance: model.Absolute.Value))
                    select built);
            },
            pipe: static (model, edit) => {
                Op op = Pipe.SelfOp;
                return ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Rail, key: op, body: rail =>
                    edit.Law.Switch(
                        state: (Rail: rail, Edit: edit, Tolerance: model.Absolute.Value, Angle: model.Angle.Value, Op: op),
                        constant: static (ctx, law) => ModelGate.Many(ctx.Op, SolidSlot.Piped, () => Brep.CreatePipe(
                            rail: ctx.Rail, radius: law.Radius, localBlending: ctx.Edit.LocalBlending, cap: ctx.Edit.Cap,
                            fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        variable: static (ctx, law) => ModelGate.Many(ctx.Op, SolidSlot.Piped, () => Brep.CreatePipe(
                            rail: ctx.Rail, railRadiiParameters: law.Rows.Map(static row => row.Parameter).AsIterable(),
                            radii: law.Rows.Map(static row => row.Radius).AsIterable(), localBlending: ctx.Edit.LocalBlending,
                            cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        thick: static (ctx, law) => ModelGate.Many(ctx.Op, SolidSlot.Piped, () => Brep.CreateThickPipe(
                            rail: ctx.Rail, radius0: law.Radius0, radius1: law.Radius1, localBlending: ctx.Edit.LocalBlending,
                            cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        thickVariable: static (ctx, law) => ModelGate.Many(ctx.Op, SolidSlot.Piped, () => Brep.CreateThickPipe(
                            rail: ctx.Rail, railRadiiParameters: law.Rows.Map(static row => row.Parameter).AsIterable(),
                            radii0: law.Rows.Map(static row => row.Inner).AsIterable(), radii1: law.Rows.Map(static row => row.Outer).AsIterable(),
                            localBlending: ctx.Edit.LocalBlending, cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail,
                            absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle))));
            },
            seed: static (model, edit) => {
                Op op = Seed.SelfOp;
                return edit.Value.Build(domain: model, key: op).Map(product => Built<SolidSlot>.Of(operation: op,
                    Products: Seq(product),
                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Seeded, body: new BuildBody.Tally(Count: 1))));
            },
            taperedExtrude: static (model, edit) => {
                Op op = TaperedExtrude.SelfOp;
                return ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Profile, key: op, body: profile =>
                    ModelGate.Many(op, SolidSlot.Tapered, () => Brep.CreateFromTaperedExtrude(
                        curveToExtrude: profile, distance: edit.Distance, direction: edit.Direction, basePoint: edit.BasePoint,
                        draftAngleRadians: edit.DraftAngleRadians, cornerType: edit.Corner,
                        tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value)));
            },
            taperedExtrudeRef: static (model, edit) => {
                Op op = TaperedExtrudeRef.SelfOp;
                return ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Profile, key: op, body: profile =>
                    ModelGate.Many(op, SolidSlot.Tapered, () => Brep.CreateFromTaperedExtrudeWithRef(
                        curve: profile, direction: edit.Direction, distance: edit.Distance,
                        draftAngle: edit.DraftAngleRadians, plane: edit.Reference, tolerance: model.Absolute.Value)));
            },
            planarFill: static (model, edit) => {
                Op op = PlanarFill.SelfOp;
                return ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Loops, key: op, body: loops =>
                    ModelGate.Many(op, SolidSlot.Planar, () => Brep.CreatePlanarBreps(inputLoops: loops.AsIterable(), tolerance: model.Absolute.Value)));
            },
            edgeSurface: static (_, edit) => {
                Op op = EdgeSurface.SelfOp;
                return ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Rails, key: op, body: rails =>
                    from _ in guard(rails.Count >= 2 && rails.Count <= 4, op.InvalidInput())
                    from built in ModelGate.Single(op, SolidSlot.EdgeSurfaced, () => Brep.CreateEdgeSurface(curves: rails.AsIterable()))
                    select built);
            },
            trimmedPlane: static (_, edit) => {
                Op op = TrimmedPlane.SelfOp;
                return ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Curves, key: op, body: curves =>
                    ModelGate.Single(op, SolidSlot.PlaneTrimmed, () => Brep.CreateTrimmedPlane(plane: edit.Frame, curves: curves.AsIterable())));
            },
            join: static (model, edit) => {
                Op op = Join.SelfOp;
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Targets, key: op, body: targets =>
                    op.Catch(() => {
                        Brep[] joined = Brep.JoinBreps(
                            brepsToJoin: targets.AsIterable(), tolerance: model.Absolute.Value,
                            angleTolerance: model.Angle.Value,
                            indexMap: out System.Collections.Generic.List<int[]> map);
                        return ModelGate.OwnMany(built: joined, key: op).Map(owned => Built<SolidSlot>.Of(operation: op,
                            Products: owned,
                            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Joined, body: new BuildBody.Tally(Count: owned.Count))
                                + ModelFact.Channel(slot: SolidSlot.Joined, value: ModelFact.Answered(channel: map)
                                    .Map(static groups => (BuildBody)new BuildBody.SourceGroups(
                                        Axis: SourceAxis.Input, Groups: groups.Map(static rows => toSeq(rows)))))));
                    }));
            },
            joinEdges: static (model, edit) => {
                Op op = JoinEdges.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstEdge < first.Edges.Count && edit.SecondEdge < second.Edges.Count,
                            op.InvalidInput())
                        from built in ModelGate.Single(op, SolidSlot.Joined, () => Brep.CreateFromJoinedEdges(
                            brep0: first, edgeIndex0: edit.FirstEdge, brep1: second, edgeIndex1: edit.SecondEdge, joinTolerance: model.Absolute.Value))
                        select built));
            },
            merge: static (model, edit) => {
                Op op = Merge.SelfOp;
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Targets, key: op, body: targets =>
                    ModelGate.Single(op, SolidSlot.Merged, () => Brep.MergeBreps(brepsToMerge: targets.AsIterable(), tolerance: model.Absolute.Value)));
            },
            mergeFaces: static (model, edit) => {
                Op op = MergeFaces.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        edit.Law.Switch(
                            state: (First: first, Second: second, Model: model, Op: op),
                            plain: static ctx => ModelGate.Single(ctx.Op, SolidSlot.Merged, () => Brep.MergeSurfaces(
                                brep0: ctx.First, brep1: ctx.Second,
                                tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value)),
                            atPoints: static (ctx, law) => ModelGate.Single(ctx.Op, SolidSlot.Merged, () => Brep.MergeSurfaces(
                                brep0: ctx.First, brep1: ctx.Second,
                                tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value,
                                point0: law.First, point1: law.Second, roundness: law.Roundness, smooth: law.Smooth)))));
            },
            match: static (model, edit) => {
                Op op = Match.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.TargetCurves, key: op, body: curves =>
                        from _ in guard(edit.Edge < target.Edges.Count, op.InvalidInput())
                        from settings in edit.Law.Rig(domain: model, key: op)
                        from built in op.Catch(() =>
                            ModelGate.Staged(op: op, success: Brep.CreateFromMatch(
                                edge: target.Edges[edit.Edge], targetCurves: curves.AsIterable(), settings: settings,
                                matched: out Brep matched, target: out Brep matchTarget),
                                (SolidSlot.Matched, (Brep[])[matched, matchTarget], false)))
                        select built));
            },
            extendToConnect: static (model, edit) => {
                Op op = ExtendToConnect.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(edit.FirstFace < first.Faces.Count && edit.SecondFace < second.Faces.Count, op.InvalidInput())
                        from __ in edit.At.Switch(
                            atEdges: at => guard(
                                at.FirstEdge < first.Edges.Count && at.SecondEdge < second.Edges.Count,
                                op.InvalidInput()),
                            atPoints: static _ => Fin.Succ(value: unit))
                        from built in op.Catch(() => {
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
                            return ModelGate.Staged(op: op, success: result.Connected,
                                (SolidSlot.Extended, (Brep[])[result.First, result.Second], false));
                        })
                        select built));
            },
            splitPieces: static (_, edit) => {
                Op op = SplitPieces.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    op.Catch(() => {
                        Brep[] pieces = Brep.SplitDisjointPieces(brep: target, indexMap: out System.Collections.Generic.List<int[]> map);
                        return ModelGate.OwnMany(built: pieces, key: op).Map(owned => Built<SolidSlot>.Of(operation: op,
                            Products: owned,
                            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.SplitApart, body: new BuildBody.Tally(Count: owned.Count))
                                + ModelFact.Channel(slot: SolidSlot.SplitApart, value: ModelFact.Answered(channel: map)
                                    .Map(static groups => (BuildBody)new BuildBody.SourceGroups(
                                        Axis: SourceAxis.Input, Groups: groups.Map(static rows => toSeq(rows)))))));
                    }));
            },
            splitBy: static (model, edit) => {
                Op op = SplitBy.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Cutters, key: op, body: cutters =>
                        cutters.Count == 1
                            ? op.Catch(() => {
                                Brep[] pieces = target.Split(cutter: cutters[0], intersectionTolerance: model.Absolute.Value, toleranceWasRaised: out bool raised);
                                return ModelGate.OwnMany(built: pieces, key: op).Map(owned => Built<SolidSlot>.Of(operation: op,
                                    Products: owned,
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.SplitApart, body: new BuildBody.Tally(Count: owned.Count))
                                        + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.SplitApart, body: new BuildBody.Flag(Value: raised))));
                            })
                            : ModelGate.Many(op, SolidSlot.SplitApart, () => target.Split(cutters: cutters.AsIterable(), intersectionTolerance: model.Absolute.Value))));
            },
            trim: static (model, edit) => {
                Op op = Trim.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    edit.Cutter.Switch(
                        state: (Target: target, Tolerance: model.Absolute.Value, Op: op),
                        byBrep: static (ctx, cutter) => ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: cutter.Cutter, key: ctx.Op,
                            body: blade => ModelGate.Many(ctx.Op, SolidSlot.Cut, () => ctx.Target.Trim(cutter: blade, intersectionTolerance: ctx.Tolerance))),
                        byPlane: static (ctx, cutter) => ModelGate.Many(ctx.Op, SolidSlot.Cut, () => ctx.Target.Trim(cutter: cutter.Cutter, intersectionTolerance: ctx.Tolerance))));
            },
            cutUp: static (model, edit) => {
                Op op = CutUp.SelfOp;
                return ModelGate.Borrow<Surface, Built<SolidSlot>>(handle: edit.Source, key: op, body: surface =>
                    ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Curves, key: op, body: curves =>
                        ModelGate.Many(op, SolidSlot.Cut, () => Brep.CutUpSurface(
                            surface: surface, curves: curves.AsIterable(), flip: edit.Flip,
                            fitTolerance: model.Absolute.Value, keepTolerance: model.Absolute.Value))));
            },
            copyTrims: static (model, edit) => {
                Op op = CopyTrims.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.TrimSource, key: op, body: source =>
                    ModelGate.Borrow<Surface, Built<SolidSlot>>(handle: edit.SurfaceSource, key: op, body: surface =>
                        from _ in guard(edit.Face < source.Faces.Count, op.InvalidInput())
                        from built in ModelGate.Single(op, SolidSlot.Cut, () => Brep.CopyTrimCurves(
                            trimSource: source.Faces[edit.Face], surfaceSource: surface, tolerance: model.Absolute.Value))
                        select built));
            },
            edit: static (model, request) => {
                Op op = Edit.SelfOp;
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: request.Target, key: op, body: source =>
                    op.Catch(() => Optional(source.DuplicateBrep()).ToFin(Fail: op.InvalidResult()).Bind(working =>
                        Edited(working: working, verb: request.Verb, domain: model, op: op).Rollback(working))));
            },
            simplify: static (_, edit) => {
                Op op = Simplify.SelfOp;
                return ModelGate.Borrow<GeometryBase, Built<SolidSlot>>(handle: edit.Target, key: op, body: source =>
                    ModelGate.Single(op, SolidSlot.Simplified, () => Brep.TryConvertBrep(geometry: source)));
            },
            lite: static (_, edit) => {
                Op op = Lite.SelfOp;
                return edit.Value.Build(key: op).Map(product => Built<SolidSlot>.Of(operation: op,
                    Products: Seq(product),
                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Extruded, body: new BuildBody.Tally(Count: 1))));
            },
            liteProfiled: static (_, edit) => {
                Op op = LiteProfiled.SelfOp;
                return ModelGate.Borrow<Extrusion, Built<SolidSlot>>(handle: edit.Target, key: op, body: source =>
                    ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Outer, key: op, body: outer =>
                        ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Inners, key: op, allowEmpty: true, body: inners =>
                            op.Catch(() => Optional(source.Duplicate() as Extrusion).ToFin(Fail: op.InvalidResult()).Bind(working => (
                                from _ in edit.Path.Case switch {
                                    (Point3d a, Point3d b, Vector3d up) => op.Confirm(success: working.SetPathAndUp(a: a, b: b, up: up)),
                                    _ => Fin.Succ(value: unit),
                                }
                                from __ in op.Confirm(success: working.SetOuterProfile(outerProfile: outer, cap: edit.Cap))
                                from ___ in inners.FoldM<Fin, Unit>(unit, (_, inner) =>
                                    op.Confirm(success: working.AddInnerProfile(innerProfile: inner)))
                                from built in ModelGate.Kept(op, SolidSlot.Extruded, working)
                                select built)
                                .Rollback(working))))));
            },
            liteRead: static (_, edit) => {
                Op op = LiteRead.SelfOp;
                return ModelGate.Borrow<Extrusion, Built<SolidSlot>>(handle: edit.Target, key: op, body: source =>
                    edit.Read.Switch(
                        state: (Source: source, Op: op),
                        heavy: static (ctx, read) => ModelGate.Single(ctx.Op, SolidSlot.Projected, () =>
                            ctx.Source.ToBrep(splitKinkyFaces: read.SplitKinkyFaces)),
                        // A null wireframe is the native's own failure signal and reaches `Many` unaltered, so the
                        // rail refuses through `InvalidResult` instead of an empty spread standing in for absence.
                        wireframe: static ctx => ctx.Op
                            .Catch(() => ModelGate.DetachedMany(source: ctx.Source.GetWireframe(), key: ctx.Op))
                            .Bind(detached => ModelGate.Many(
                                ctx.Op, SolidSlot.Projected, () => detached.AsEnumerable())),
                        mesh: static (ctx, read) => ctx.Op
                            .Catch(() => ModelGate.Detached(
                                source: ctx.Source.GetMesh(meshType: read.Kind), key: ctx.Op))
                            .Bind(detached => ModelGate.Single(ctx.Op, SolidSlot.Projected, () => detached)),
                        profile: static (ctx, read) => ctx.Op
                            .Catch(() => ModelGate.Detached(
                                source: ctx.Source.Profile3d(profileIndex: read.Index, s: read.Station), key: ctx.Op))
                            .Bind(detached => ModelGate.Single(ctx.Op, SolidSlot.Projected, () => detached)),
                        wallEdge: static (ctx, read) => ctx.Op
                            .Catch(() => ModelGate.Detached(
                                source: ctx.Source.WallEdge(ci: read.Component), key: ctx.Op))
                            .Bind(detached => ModelGate.Single(ctx.Op, SolidSlot.Projected, () => detached)),
                        wallSurface: static (ctx, read) => ctx.Op
                            .Catch(() => ModelGate.Detached(
                                source: ctx.Source.WallSurface(ci: read.Component), key: ctx.Op))
                            .Bind(detached => ModelGate.Single(ctx.Op, SolidSlot.Projected, () => detached)),
                        profilePlane: static (ctx, read) => ctx.Op.Catch(() => Fin.Succ(value: Built<SolidSlot>.Of(
                            operation: ctx.Op,
                            Products: Seq<GeometryHandle>(),
                            Evidence: BuildReceipt<SolidSlot>.Of(
                                slot: SolidSlot.Projected,
                                body: new BuildBody.Planes(Rows: Seq(ctx.Source.GetProfilePlane(s: read.Station))))))),
                        pathPlane: static (ctx, read) => ctx.Op.Catch(() => Fin.Succ(value: Built<SolidSlot>.Of(
                            operation: ctx.Op,
                            Products: Seq<GeometryHandle>(),
                            Evidence: BuildReceipt<SolidSlot>.Of(
                                slot: SolidSlot.Projected,
                                body: new BuildBody.Planes(Rows: Seq(ctx.Source.GetPathPlane(s: read.Station)))))))));
            });

    private static Fin<Built<SolidSlot>> Harvested(Brep.FilletSurfaceResults results, Op op) =>
        ModelGate.Staged(op: op,
            (SolidSlot.FilletFace0, FaceDup(face: results.Face0), true),
            (SolidSlot.FilletFace1, FaceDup(face: results.Face1), true),
            (SolidSlot.Filleted, results.Fillets, false),
            (SolidSlot.Trimmed0, results.OutBreps0, true),
            (SolidSlot.Trimmed1, results.OutBreps1, true));

    private static Fin<Built<SolidSlot>> SectionFilleted(
        BrepFace first, Point2d firstUv, BrepFace second, Point2d secondUv,
        SectionFilletLaw law, double tolerance, Op op) =>
        op.Catch(() => {
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
            return op.Confirm(success: created).Bind(_ => Harvested(
                fillets: fillets, trimmed0: trimmed0, trimmed1: trimmed1, op: op));
        });

    private static Fin<Built<SolidSlot>> Harvested(
        System.Collections.Generic.IEnumerable<Brep> fillets,
        System.Collections.Generic.IEnumerable<Brep> trimmed0,
        System.Collections.Generic.IEnumerable<Brep> trimmed1,
        Op op) =>
        ModelGate.Staged(op: op,
            (SolidSlot.Sectioned, fillets, false),
            (SolidSlot.Trimmed0, trimmed0, true),
            (SolidSlot.Trimmed1, trimmed1, true));

    private static System.Collections.Generic.IEnumerable<GeometryBase> FaceDup(BrepFace? face) =>
        face is null ? [] : [face.Duplicate()];

    private static Fin<Built<SolidSlot>> Edited(Brep working, SolidEdit verb, Context domain, Op op) =>
        verb.Switch(
            state: (Working: working, Domain: domain, Op: op),
            cap: static ctx => ModelGate.Owned(ctx.Op, SolidSlot.Edited, ctx.Working, () => ctx.Working.CapPlanarHoles(tolerance: ctx.Domain.Absolute.Value)),
            joinNaked: static ctx => ctx.Op.Catch(() => Fin.Succ(value: ctx.Working.JoinNakedEdges(tolerance: ctx.Domain.Absolute.Value)))
                .Bind(count => ModelGate.Kept(ctx.Op, SolidSlot.Edited, ctx.Working, extra: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Edited, body: new BuildBody.Tally(Count: count)))),
            mergeCoplanar: static ctx =>
                from _ in ctx.Op.Confirm(success: ctx.Working.MergeCoplanarFaces(
                    tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                from built in ModelGate.Kept(ctx.Op, SolidSlot.Edited, ctx.Working)
                select built,
            mergeFace: static (ctx, edit) =>
                from _ in guard(edit.Face < ctx.Working.Faces.Count, ctx.Op.InvalidInput())
                from __ in ctx.Op.Confirm(success: ctx.Working.MergeCoplanarFaces(
                    faceIndex: edit.Face,
                    tolerance: ctx.Domain.Absolute.Value,
                    angleTolerance: ctx.Domain.Angle.Value))
                from built in ModelGate.Kept(ctx.Op, SolidSlot.Edited, ctx.Working)
                select built,
            mergeFacePair: static (ctx, edit) =>
                from _ in guard(
                    edit.First < ctx.Working.Faces.Count && edit.Second < ctx.Working.Faces.Count,
                    ctx.Op.InvalidInput())
                from __ in ctx.Op.Confirm(success: ctx.Working.MergeCoplanarFaces(
                    faceIndex0: edit.First, faceIndex1: edit.Second,
                    tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                from built in ModelGate.Kept(ctx.Op, SolidSlot.Edited, ctx.Working)
                select built,
            unjoinEdges: static (ctx, edit) =>
                from _ in guard(edit.Edges.ForAll(edge => edge < ctx.Working.Edges.Count), ctx.Op.InvalidInput())
                from built in ModelGate.OwnedMany(
                    op: ctx.Op,
                    slot: SolidSlot.Edited,
                    working: ctx.Working,
                    run: () => ctx.Working.UnjoinEdges(edgesToUnjoin: edit.Edges.AsIterable()))
                select built,
            removeHoles: static (ctx, edit) => ModelGate.Owned(ctx.Op, SolidSlot.Edited, ctx.Working, () => ctx.Working.RemoveHoles(loops: edit.Loops.AsIterable(), tolerance: ctx.Domain.Absolute.Value)),
            removeFins: static ctx => ctx.Op.Confirm(success: ctx.Working.RemoveFins()).Bind(_ => ModelGate.Kept(ctx.Op, SolidSlot.Edited, ctx.Working)),
            cullFaces: static ctx => ctx.Op.Confirm(success: ctx.Working.CullUnusedFaces()).Bind(_ => ModelGate.Kept(ctx.Op, SolidSlot.Edited, ctx.Working)),
            repair: static ctx => ctx.Op.Confirm(success: ctx.Working.Repair(tolerance: ctx.Domain.Absolute.Value)).Bind(_ => ModelGate.Kept(ctx.Op, SolidSlot.Edited, ctx.Working)),
            reseam: static (ctx, edit) =>
                from _ in guard(edit.Face < ctx.Working.Faces.Count, ctx.Op.InvalidInput())
                from built in ModelGate.Owned(ctx.Op, SolidSlot.Edited, ctx.Working, () => Brep.ChangeSeam(
                    face: ctx.Working.Faces[edit.Face], direction: edit.Axis.Native,
                    parameter: edit.Parameter, tolerance: ctx.Domain.Absolute.Value))
                select built);

}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Solids {
    public static Fin<Built<SolidSlot>> Build(ModelRuntime runtime, params ReadOnlySpan<SolidOp> operations) =>
        ModelGate.Entry(
            runtime: runtime,
            operations: operations,
            admit: static (operation, key) => operation.Admitted(key: key),
            apply: static (operation, model) => operation.Apply(domain: model));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
