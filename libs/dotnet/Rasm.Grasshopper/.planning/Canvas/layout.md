# [RASM_GRASSHOPPER_CANVAS_LAYOUT]

`CanvasLayout` owns programmatic arrangement as document mutation with owned undo, and interactive snap solving as typed capsules over the host solver surfaces. Every arrangement is ONE `CanvasArrangement` case folded to per-object pivot deltas and settled as ONE document mutation: `IAttributes.Move` under pre-captured `PivotAction` undo rows, sealed through `Document/history.md`'s `HistoryLedger.Seal` with the caller's `VerbNoun` — a move without its undo record is unconstructible from this gate. Session clock arrives INJECTED and required; the mint-when-absent fallback this page carried was the folder's third clock posture and is deleted (folder RULINGS `[02]`).

Host absorption owns every solver: the `SnappingAction` factory family owns align, gap, ortho, and wire-straighten candidates, `SnappingConstraints` owns document-scoped snapping, `SnapSpace` owns numeric lattices, and `StretchLayoutSolver` owns min/ideal/max distribution. Snap-guide feedback is a `Seq<Mark>` producer over the kernel paint vocabulary — evidence records carry no paint intent.

## [01]-[INDEX]

- [02]-[CANDIDATES]: `CandidatePayload` + `CandidateRow` + `NudgeVector` — the snap-candidate factory rows, the winning-nudge fold, and the kernel-mark guide projection.
- [03]-[SOLVERS]: `SelectionSide` + `SnapScope` + `SnapField` + `Lattice` + `RoundingPosture` + `StretchPlan` — the document snap capsule, the numeric lattice, and the stretch distribution fold.
- [04]-[ARRANGE]: `Axis` + `CanvasArrangement` + `CanvasLayout` — the arrangement union and the one sealed-mutation gate.

## [02]-[CANDIDATES]

- Owner: `CandidateRow` `[SmartEnum<int>]` — the snap-candidate vocabulary over the `SnappingAction` factory family, twelve rows through ONE generic `Row<TCase>` factory: the row names its payload case as a type argument, a matching payload mints through the host factory, and a mismatched payload is a TYPED refusal naming both the row and the case — the four per-family factories and their four `_ => null` miss arms are unrepresentable. Five align rows own their misalignment arithmetic INSIDE the mint closure (the host factory consumes its delta verbatim), so the `Gauge` column — declared on twelve rows, meaningful on five, hard-zero on seven, read at one site — is DELETED whole: the case payload sheds its `Delta` and no caller re-derives edge math.
- Owner: `NudgeVector` — the candidate evidence, VALUE-ONLY: `Dx`/`Dy` with `Magnitude` derived; the guide lines and label triple the record once carried were paint intent on an evidence record, and they now project as `NudgeVector.Guides(action, ink)` → `Seq<Mark>` — kernel stroke and text marks a `Canvas/paint.md` plan transports — so the leak that reached `CanvasState` through this record is closed at the type. `Winner` folds a candidate set to the shortest nudge through the host's own `SmallerMagnitude`, total over the empty span as `None`.
- Law: candidate arithmetic is host arithmetic — a candidate the host family cannot mint lands as a NEW row composing kernel `Rasm.Numerics` atoms and minting through the public `SnappingAction` constructor; growth is a row, never a solver.
- Law: guide feedback is plan transport — `SnappingAction.Draw`/`DrawSnappingBoxes` remain host draw members a `MountRaw` window may run, but the DECLARATIVE route is `Guides`' kernel marks through the planned window; an inline feedback stroke outside either is the inline-paint defect.
- Packages: Grasshopper2 (the `SnappingAction` factory family, `SmallerMagnitude`, `TextAnchor`), `Rasm.Interaction` (`Mark`, `PathSpec`, `StrokeSpec`, `TypeFace`), `Rasm.Numerics` (`PerceptualColor`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new candidate is one row through the one factory; the fold and the evidence never fork.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record CandidatePayload {
    private CandidatePayload() { }
    public sealed record AlignCase(RectangleF Source, RectangleF Target) : CandidatePayload;
    public sealed record GapCase(RectangleF Source, RectangleF Target, int GapSize, float Delta) : CandidatePayload;
    public sealed record OrthoCase(PointF Origin, RectangleF Frame) : CandidatePayload;
    public sealed record WireCase(PointF Source, PointF Target) : CandidatePayload;
}

[SmartEnum<int>]
public sealed partial class CandidateRow {
    public static readonly CandidateRow AlignLeft = Row<CandidatePayload.AlignCase>(key: 0,
        mint: static c => SnappingAction.CreateLeftAlignAction(c.Source, c.Target, c.Target.Left - c.Source.Left));
    public static readonly CandidateRow AlignRight = Row<CandidatePayload.AlignCase>(key: 1,
        mint: static c => SnappingAction.CreateRightAlignAction(c.Source, c.Target, c.Target.Right - c.Source.Right));
    public static readonly CandidateRow AlignTop = Row<CandidatePayload.AlignCase>(key: 2,
        mint: static c => SnappingAction.CreateTopAlignAction(c.Source, c.Target, c.Target.Top - c.Source.Top));
    public static readonly CandidateRow AlignBottom = Row<CandidatePayload.AlignCase>(key: 3,
        mint: static c => SnappingAction.CreateBottomAlignAction(c.Source, c.Target, c.Target.Bottom - c.Source.Bottom));
    public static readonly CandidateRow AlignCentre = Row<CandidatePayload.AlignCase>(key: 4,
        mint: static c => SnappingAction.CreateCentreAlignAction(c.Source, c.Target, c.Target.Center.X - c.Source.Center.X));
    public static readonly CandidateRow GapRightward = Row<CandidatePayload.GapCase>(key: 5,
        mint: static c => SnappingAction.CreateVerticalGapActionOnRight(c.Source, c.Target, c.GapSize, c.Delta));
    public static readonly CandidateRow GapLeftward = Row<CandidatePayload.GapCase>(key: 6,
        mint: static c => SnappingAction.CreateVerticalGapActionOnLeft(c.Source, c.Target, c.GapSize, c.Delta));
    public static readonly CandidateRow GapAbove = Row<CandidatePayload.GapCase>(key: 7,
        mint: static c => SnappingAction.CreateHorizontalGapActionAbove(c.Source, c.Target, c.GapSize, c.Delta));
    public static readonly CandidateRow GapBelow = Row<CandidatePayload.GapCase>(key: 8,
        mint: static c => SnappingAction.CreateHorizontalGapActionBelow(c.Source, c.Target, c.GapSize, c.Delta));
    public static readonly CandidateRow OrthoVertical = Row<CandidatePayload.OrthoCase>(key: 9,
        mint: static c => SnappingAction.VerticalOrthoAction(c.Origin, c.Frame));
    public static readonly CandidateRow OrthoHorizontal = Row<CandidatePayload.OrthoCase>(key: 10,
        mint: static c => SnappingAction.HorizontalOrthoAction(c.Origin, c.Frame));
    public static readonly CandidateRow StraightenWire = Row<CandidatePayload.WireCase>(key: 11,
        mint: static c => SnappingAction.CreateStraightenWireAction(c.Source, c.Target));

    [UseDelegateFromConstructor]
    internal partial Fin<SnappingAction> MintOn(CandidatePayload payload, Op key);

    public Fin<SnappingAction> Mint(CandidatePayload payload, Op? key = null) => MintOn(payload: payload, key: key.OrDefault());

    private static CandidateRow Row<TCase>(int key, Func<TCase, SnappingAction> mint) where TCase : CandidatePayload =>
        new(key: key, mintOn: (payload, op) => payload is TCase matched
            ? op.Catch(() => Fin.Succ(mint(matched)))
            : Fin.Fail<SnappingAction>(new KernelFault.InvalidValue(
                Label: typeof(TCase).Name, Requirement: "the payload case this row mints from", Key: Some(op))));
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NudgeVector(float Dx, float Dy) : IValidityEvidence {
    public float Magnitude => float.Hypot(Dx, Dy);
    public bool IsValid => ValidityClaim.All(ValidityClaim.Finite(value: Dx), ValidityClaim.Finite(value: Dy));

    public static Option<SnappingAction> Winner(params ReadOnlySpan<SnappingAction> candidates) =>
        Iterable<SnappingAction>.FromSpan(candidates)
            .Fold(Option<SnappingAction>.None, static (held, next) => held.Match(
                Some: best => Some(SnappingAction.SmallerMagnitude(best, next)),
                None: () => Some(next)));

    public static Seq<Mark> Guides(SnappingAction action, PerceptualColor ink, StrokeSpec stroke, TypeFace face) =>
        toSeq(action.Lines).Map(line => (Mark)new Mark.StrokeCase(
            Path: new PathSpec.LineCase(From: line.From, To: line.To), Stroke: stroke))
        + Seq1((Mark)new Mark.TextCase(
            Face: face, Block: None, Ink: ink, At: action.LabelPoint, Text: action.LabelText));
}
```

## [03]-[SOLVERS]

- Owner: `SelectionSide` `[SmartEnum<string>]` realizing `ICapability` — the selection-scope vocabulary whose corner law FORBIDS the empty set (a snap field over neither selected nor unselected objects snaps against nothing and was a guard every caller re-derived); `SnapScope` `[Union]` — `ExcludingCase(Seq<Guid>)`, `SelectionCase(CapabilitySet<SelectionSide>, Option<HashSet<Guid>>)`, `BoxesCase(Seq<RectangleF>)` — the raw array becomes `Seq`, so structural equality holds by carrier.
- Owner: `SnapField` sealed `[BoundaryAdapter]` — the document snap capsule over `SnappingConstraints`: one polymorphic `Of` over the scope union and `Solve` lifting the `SnapRectangle` out-pair onto `SnapPair` (`Option<SnappingAction>` per axis — a null out is typed absence). NAMED LOSS: the unread `SolveObject`, `SolveWires`, and `DrawGuides` members DELETE — each re-lands as one member when a consumer arrives; the host draws its own drag-time guides.
- Owner: `Lattice` — the numeric snap lattice over `SnapSpace`: `Orthogonal` (the two host arities on the presence of the second cell size) and `Fix` lifting the out-triple onto `SnapVerdict(X, Y)`. NAMED LOSS: the unread `Merge`/`Empty`/`Of` element and numeric arms DELETE with the same re-landing clause; the unread `Rule` string column dies with its refuted no-parse claim.
- Owner: `RoundingPosture` `[SmartEnum<int>]` — `Exact` and `Pixel`, the row carrying whether the solver's `Round()` pass runs; a policy is a row, not a `bool round` parameter. `StretchPlan.Solve` admits every row through `Validation` — N bad rows report N labeled faults, not one.
- Law: capsule state is gesture-scoped — a `SnapField` is built per drag or per arrangement against the CURRENT graph and never cached across mutations, because constraint boxes are position snapshots.
- Law: settings policy is host-direct — `SnappingSettings.Default`/`Current` are the two reads and the fourteen axis getters ARE the evidence row; a local mirror or read wrapper is the deleted form.
- Packages: Grasshopper2 (`SnappingConstraints`, `SnappingSettings`, `SnapSpace`, `StretchLayoutSolver`), LanguageExt.Core (`Validation`), `Rasm.Domain` (`CapabilitySet`, `CapabilityLaw`, `Op`).
- Growth: a new snap source is one `SnapScope` case; a rounding policy is one row; the verdict shapes never fork.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SelectionSide : ICapability<SelectionSide> {
    public static readonly SelectionSide Selected = new(key: "selected");
    public static readonly SelectionSide Unselected = new(key: "unselected");
    public static CapabilityLaw<SelectionSide> Law => Corners.Value;
    private static readonly Lazy<CapabilityLaw<SelectionSide>> Corners =
        new(static () => CapabilityLaw<SelectionSide>.Forbidden(Seq(CapabilitySet<SelectionSide>.None)));
}

[Union]
public abstract partial record SnapScope {
    private SnapScope() { }
    public sealed record ExcludingCase(Seq<Guid> Dragged) : SnapScope;
    public sealed record SelectionCase(CapabilitySet<SelectionSide> Sides, Option<HashSet<Guid>> Filter) : SnapScope;
    public sealed record BoxesCase(Seq<RectangleF> Frames) : SnapScope;
}

[SmartEnum<int>]
public sealed partial class RoundingPosture {
    public static readonly RoundingPosture Exact = new(key: 0, rounds: false);
    public static readonly RoundingPosture Pixel = new(key: 1, rounds: true);
    internal bool Rounds { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SnapPair(Option<SnappingAction> X, Option<SnappingAction> Y);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SnapVerdict(double X, double Y) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Finite(value: X), ValidityClaim.Finite(value: Y));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StretchRow(float Min, float Max, float Ideal) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Ordered(lower: Min, upper: Ideal),
        ValidityClaim.Ordered(lower: Ideal, upper: Max));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StretchVerdict([property: Generator.Equals.OrderedEquality] Seq<float> Lengths) : IValidityEvidence {
    public bool IsValid => Lengths.ForAll(static length => float.IsFinite(length) && length >= 0f);
}

// --- [SERVICES] ------------------------------------------------------------------------
[BoundaryAdapter]
public sealed class SnapField {
    private readonly SnappingConstraints constraints;

    public static Fin<SnapField> Of(Document graph, SnapScope scope, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(value: scope).Bind(valid => valid.Switch(
            state: (Graph: graph, Key: op),
            excludingCase: static (s, c) => s.Key.Catch(() => Fin.Succ(
                new SnapField(constraints: SnappingConstraints.CreateFromDocument(s.Graph, c.Dragged.ToArray())))),
            selectionCase: static (s, c) => SelectionSide.Law.Admit(held: c.Sides).Bind(sides => s.Key.Catch(() => Fin.Succ(
                new SnapField(constraints: SnappingConstraints.CreateFromDocument(
                    s.Graph,
                    sides.Admits(SelectionSide.Selected),
                    sides.Admits(SelectionSide.Unselected),
                    Op.ToHostSlot(c.Filter)))))),
            boxesCase: static (s, c) => s.Key.Catch(() => Fin.Succ(
                new SnapField(constraints: new SnappingConstraints(c.Frames.ToArray()))))));
    }

    public Fin<SnapPair> Solve(RectangleF target, RectangleF visibleLimit, SnappingSettings settings, Op key) {
        SnappingConstraints held = constraints;
        return key.Catch(() => {
            held.SnapRectangle(target, settings, visibleLimit, out SnappingAction snapX, out SnappingAction snapY);
            return Fin.Succ(new SnapPair(X: Optional(snapX), Y: Optional(snapY)));
        });
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct Lattice {
    internal SnapSpace Space { get; }

    public static Fin<Lattice> Orthogonal(double originX, double originY, double sizeX, Option<double> sizeY = default, Op? key = null);

    public Fin<SnapVerdict> Fix(double x, double y, Option<double> cutoff, Op key);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static class StretchPlan {
    public static Fin<StretchVerdict> Solve(Seq<StretchRow> rows, float target, RoundingPosture rounding, Op key) =>
        from admitted in rows.Zip(toSeq(Range(0, rows.Count))).Traverse(pair => pair.Item1.IsValid
                ? Validation<Error, StretchRow>.Success(pair.Item1)
                : Validation<Error, StretchRow>.Fail(new KernelFault.InvalidValue(
                    Label: $"row[{pair.Item2}]", Requirement: "Min <= Ideal <= Max", Key: Some(key))))
            .As().ToFin()
        from verdict in key.Catch(() => {
            StretchLayoutSolver solver = new();
            admitted.Iter(row => solver.Add(row.Min, row.Max, row.Ideal));
            _ = solver.Solve(target);
            Op.SideWhen(condition: rounding.Rounds, action: () => solver.Round());
            return Fin.Succ(new StretchVerdict(
                Lengths: toSeq(Enumerable.Range(0, solver.Count).Select(index => solver[index])).Strict()));
        })
        select verdict;
}
```

## [04]-[ARRANGE]

- Owner: `Axis` `[SmartEnum<int>]` — the distribution axis whose columns answer every read the fold makes (`Pivot`, `Lead`, `Trail`, `Extent`, and the delta composer), so the five `Vertical ? … : …` ternaries inside one fold are five row-column reads. `CanvasArrangement` `[Union]` `[GenerateUnionOps]` — RENAMED from `Arrangement`: the kernel `Meshing/arrangement.md` owns that simple name and the seating brings it into scope (the same rule that renamed `TransformSpec`); the kernel keeps the name. `CanvasLayout` — the one sealed-mutation gate settling `CanvasReceipt<ArrangeFacts>` on `CanvasLane.Arrange` — the local stamp-pair receipt and its stringly, unread `Verb` column are deleted onto the fan's one gauged receipt.
- Entry: `CanvasLayout.Arrange(VerbNoun label, CanvasArrangement plan, MonotonicTimeline clock, Context context, Op? key = null)` → `Fin<CanvasReceipt<ArrangeFacts>>` — the clock is REQUIRED (no mint, no option: two receipts from one gesture under two clocks are unorderable) and the context supplies the device tolerance the zero-move filter reads.
- Law: mutation and undo are one act — per object, a `PivotAction` undo row is ADDED BEFORE `IAttributes.Move`, the host action captures the pre-move pivot, and the filled `ActionList` seals through `HistoryLedger.Seal` inside the same marshal window; a move without its undo record is unconstructible from this gate. Below-tolerance delta contributes no undo row — the filter reads `context.For(ToleranceLane.Hit)`, never a bare `!= 0f` float gate — and an arrangement whose every delta is under it seals nothing and reports a zero-count receipt.
- Law: `VerbNoun` arrives minted — `Document/history.md` owns the mint and this gate never constructs one.
- Boundary: snapped interactive movement during a drag is the host's own; whole-graph selection sweeps and structural verbs are `Document/document.md`'s transaction; the live snap-axis nudge state is a `Canvas/canvas.md` lens read surfaced as `NudgeVector` evidence.
- Packages: Grasshopper2 (`IAttributes`, `Document.Undo`, `ActionList`, `PivotAction`, `VerbNoun`, `WireEnds`), `Document/history.md` (`HistoryLedger.Seal`), `Shell/session.md` (`GhSession`, `ScopeTarget`), `Rasm.Parametric` (`MonotonicTimeline`, `GaugedSpan`), `Rasm.Domain` (`Context`, `ToleranceLane`), LanguageExt.Core.
- Growth: a new arrangement is one case whose delta fold breaks the gate loudly — `DocumentMethods.MakeRoom` is the next such case (`RoomCase`, folding the host's own displacement into the same sealed gate); a new undo posture is `Document/history.md`'s row, never a fork here.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Grasshopper.Document;
using Rasm.Grasshopper.Shell;
using Rasm.Parametric;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Axis {
    public static readonly Axis Horizontal = new(key: 0,
        pivot: static row => row.Pivot.X, lead: static frame => frame.Left,
        trail: static frame => frame.Right, extent: static frame => frame.Width,
        delta: static (dx, _) => (dx, 0f));
    public static readonly Axis Vertical = new(key: 1,
        pivot: static row => row.Pivot.Y, lead: static frame => frame.Top,
        trail: static frame => frame.Bottom, extent: static frame => frame.Height,
        delta: static (_, dy) => (0f, dy));

    [UseDelegateFromConstructor] internal partial float Pivot(IAttributes row);
    [UseDelegateFromConstructor] internal partial float Lead(RectangleF frame);
    [UseDelegateFromConstructor] internal partial float Trail(RectangleF frame);
    [UseDelegateFromConstructor] internal partial float Extent(RectangleF frame);
    [UseDelegateFromConstructor] internal partial (float Dx, float Dy) Delta(float dx, float dy);
}

[Union]
[GenerateUnionOps]
public abstract partial record CanvasArrangement {
    private CanvasArrangement() { }
    public sealed record AlignCase(CandidateRow Edge, Seq<Guid> Objects) : CanvasArrangement;
    public sealed record DistributeCase(Axis Along, int Gap, Seq<Guid> Objects) : CanvasArrangement;
    public sealed record GridCase(double CellWidth, double CellHeight, PointF Origin, Seq<Guid> Objects) : CanvasArrangement;
    public sealed record NudgeCase(SizeF Delta, Seq<Guid> Objects) : CanvasArrangement;
    public sealed record StraightenCase(WireEnds Wire) : CanvasArrangement;
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ArrangeFacts(int Moved, double Displacement) : IValidityEvidence {
    public bool IsValid => Moved >= 0 && ValidityClaim.Nonnegative(value: Displacement).Holds;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static class CanvasLayout {
    public static Fin<CanvasReceipt<ArrangeFacts>> Arrange(
        VerbNoun label, CanvasArrangement plan, MonotonicTimeline clock, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in op.Need(value: plan)
               from gauged in clock.Gauged<ArrangeFacts, CanvasLane>(
                   lane: CanvasLane.Arrange,
                   work: op,
                   body: () => GhSession.Run(ScopeTarget.DocumentHost, scope =>
                       scope.Document.ToFin(op.MissingContext()).Bind(graph =>
                           Deltas(graph: graph, plan: valid, key: op).Bind(moves =>
                               Commit(graph: graph, label: label, moves: moves, step: context.For(lane: ToleranceLane.Hit), key: op))), key: op),
                   key: op)
               from facts in gauged.Value
               select new CanvasReceipt<ArrangeFacts>(Span: gauged.Span, Facts: facts);
    }

    private static Fin<Seq<(IAttributes Target, float Dx, float Dy)>> Deltas(Document graph, CanvasArrangement plan, Op key) =>
        plan.Switch(
            state: (Graph: graph, Key: key),
            alignCase: static (s, c) =>
                from rows in Resolve(graph: s.Graph, objects: c.Objects, key: s.Key)
                from anchor in rows.Head.ToFin(s.Key.InvalidInput())
                from moves in rows.Tail.Map(row => c.Edge.Mint(
                        payload: new CandidatePayload.AlignCase(Source: row.Bounds, Target: anchor.Bounds), key: s.Key)
                        .Map(action => (Target: row, Dx: action.ΔX, Dy: action.ΔY)))
                    .TraverseM(identity).As()
                select moves.Strict(),
            distributeCase: static (s, c) =>
                Resolve(graph: s.Graph, objects: c.Objects, key: s.Key).Map(rows => {
                    Seq<IAttributes> ordered = toSeq(rows.OrderBy(row => c.Along.Pivot(row: row))).Strict();
                    return ordered.Head.Match(
                        Some: head => ordered.Tail.Fold(
                            (Cursor: c.Along.Trail(frame: head.Bounds), Moves: Seq<(IAttributes, float, float)>()),
                            (held, row) => {
                                (float dx, float dy) = c.Along.Delta(
                                    dx: held.Cursor + c.Gap - c.Along.Lead(frame: row.Bounds),
                                    dy: held.Cursor + c.Gap - c.Along.Lead(frame: row.Bounds));
                                return (held.Cursor + c.Along.Extent(frame: row.Bounds) + c.Gap, held.Moves.Add((row, dx, dy)));
                            }).Moves.Strict(),
                        None: static () => Seq<(IAttributes, float, float)>());
                }),
            gridCase: static (s, c) =>
                from rows in Resolve(graph: s.Graph, objects: c.Objects, key: s.Key)
                from lattice in Lattice.Orthogonal(originX: c.Origin.X, originY: c.Origin.Y, sizeX: c.CellWidth, sizeY: Some(c.CellHeight), key: s.Key)
                from moves in rows.Map(row => lattice.Fix(x: row.Pivot.X, y: row.Pivot.Y, cutoff: Option<double>.None, key: s.Key)
                        .Map(verdict => (Target: row, Dx: (float)(verdict.X - row.Pivot.X), Dy: (float)(verdict.Y - row.Pivot.Y))))
                    .TraverseM(identity).As()
                select moves.Strict(),
            nudgeCase: static (s, c) =>
                Resolve(graph: s.Graph, objects: c.Objects, key: s.Key)
                    .Map(rows => rows.Map(row => (Target: row, Dx: c.Delta.Width, Dy: c.Delta.Height)).Strict()),
            straightenCase: static (s, c) =>
                from source in Optional(s.Graph.Objects.FindParameter(c.Wire.Source)).ToFin(s.Key.InvalidInput())
                from target in Optional(s.Graph.Objects.FindParameter(c.Wire.Target)).ToFin(s.Key.InvalidInput())
                from owner in Optional(source.Attributes as IParameterAttributes).ToFin(s.Key.InvalidResult())
                from into in Optional(target.Attributes as IParameterAttributes).ToFin(s.Key.InvalidResult())
                from action in CandidateRow.StraightenWire.Mint(
                    payload: new CandidatePayload.WireCase(Source: owner.Outlet, Target: into.Inlet), key: s.Key)
                select Seq((Target: (IAttributes)owner, Dx: action.ΔX, Dy: action.ΔY)));

    private static Fin<Seq<IAttributes>> Resolve(Document graph, Seq<Guid> objects, Op key) =>
        objects.Map(id => Optional(graph.Objects.Find(id)).Bind(static obj => Optional(obj.Attributes)).ToFin(key.InvalidInput()))
            .TraverseM(identity).As().Map(static rows => rows.Strict());

    private static Fin<ArrangeFacts> Commit(
        Document graph, VerbNoun label, Seq<(IAttributes Target, float Dx, float Dy)> moves, Tolerance step, Op key) {
        Seq<(IAttributes Target, float Dx, float Dy)> real = moves
            .Filter(move => Math.Abs(move.Dx) > step.Value || Math.Abs(move.Dy) > step.Value).Strict();
        if (real.IsEmpty) { return Fin.Succ(new ArrangeFacts(Moved: 0, Displacement: 0d)); }
        return key.Catch(() => {
            ActionList actions = new();
            real.Iter(move => {
                actions.Add(new PivotAction(move.Target.Owner));
                move.Target.Move(move.Dx, move.Dy);
            });
            return Fin.Succ(actions);
        }).Bind(actions => HistoryLedger.Seal(graph.Undo, actions, label, key).Map(_ => new ArrangeFacts(
            Moved: real.Count,
            Displacement: real.Fold(0d, static (sum, move) => sum + Math.Abs(move.Dx) + Math.Abs(move.Dy)))));
    }
}
```

## [05]-[DENSITY_BAR]

| [INDEX] | [CONCERN]            | [OWNER]                              | [RAIL]                                       | [CASES] |
| :-----: | :------------------- | :----------------------------------- | :------------------------------------------- | :-----: |
|  [01]   | snap candidates      | `CandidateRow` + `CandidatePayload`  | one generic `Row<TCase>` factory, typed miss |   12    |
|  [02]   | nudge evidence       | `NudgeVector`                        | value-only; guides project as kernel marks   |    1    |
|  [03]   | document snapping    | `SnapField` + `SnapScope`            | one `Of`, corner-law selection scope         |    3    |
|  [04]   | numeric lattice      | `Lattice` + `SnapVerdict`            | out-params lifted, unread arms deleted       |    1    |
|  [05]   | stretch distribution | `StretchPlan` + `RoundingPosture`    | per-row `Validation`, policy as a row        |    1    |
|  [06]   | sealed arrangement   | `CanvasArrangement` + `CanvasLayout` | one gauged gate, `Axis` row-column folds     |    5    |

Four row-factories with null miss arms, the twelve-row gauge column, the paint-intent evidence record, the minted fallback clock, the five axis ternaries, the three unread verdict columns, the exact-float move gate, and the seven unread solver members are all deleted; the undo-before-move commit law survives as this page's strongest passage.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
