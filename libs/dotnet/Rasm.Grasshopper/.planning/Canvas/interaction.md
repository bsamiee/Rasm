# [RASM_GRASSHOPPER_CANVAS_INTERACTION]

Canvas interaction is the kernel input estate projected onto the GH2 responder contract: the kernel `ResponderSpec` (phase-keyed slot maps), `PointerFact`, and `InputVerdict` are the vocabulary, and this page owns only what the host forces — the `Responses`/`IResponsive` adapter with its override relays, the GH2 coordinate frame and rotation-gesture residue, the focus stack, the drag and resize gesture capsules, and the synchronous context-menu population seam. Mount capsule is `Canvas/paint.md`'s `Mounted<TFacts>`; the attach-verify-detach shape is one `Attachment` fold; and the grip vocabulary is the kernel's case family, not a four-bool mask.

Every host-bound acquisition runs inside `GhSession.Run(ScopeTarget.CanvasHost, …)` and returns an owned lease; every cleanup refusal AGGREGATES into its primary through `Error.Many` — the discarding posture this page once held is the ruled-out form.

## [01]-[INDEX]

- [02]-[VERDICT]: `VerdictSeam` + `InputMap` — the host `Response` correspondence and the generated boundary projections.
- [03]-[DISPATCH]: `GhResponder` + `SpecResponder` + `Attachment` + `Dispatch` — the kernel spec beside its GH2 residue columns, the one host adapter, and the lease-owned registration and focus gates.
- [04]-[SESSIONS]: `DragFacts` + `DragSession` + `EdgeResize` — the object-drag capsule and the full-depth resize capsule over the kernel grip family.
- [05]-[MOMENTS]: `MenuMoment` + `MenuMount` — the synchronous context-menu population seam over the kernel menu vocabulary.

## [02]-[VERDICT]

- Owner: `VerdictSeam` `[SmartEnum<int>]` keyed `(int)Response.<row>` — the closed correspondence between the kernel `InputVerdict` and the host `Response`, both directions TOTAL on one row set: `ToHost` proves every kernel verdict has a row at LOAD (a kernel widening breaks at type init, never inside the Eto pump), and `Of` REFUSES an unrostered host answer typed — a fifth host row is a surfaced boundary fact, never a silent `Ignored`.
- Owner: `InputMap` — the boundary feeder seam: `Moment(PopulateContextMenuEventArgs)` and `Facts(ObjectDragInteraction)` are generated `[Mapper]` rows, and `Fact(ResponseMouseArgs, Control, Op)` is the ONE hand arm the folder Mapperly ruling licenses — it COMPOSES the kernel's one admission `PointerFact.Of` on the wrapped `UnderlyingEtoEventArgs` (buttons, modifiers, delta, pressure, finiteness all the kernel's), then replaces `Content` with the host's OWN canvas projection, the named host demand no scroll-origin derivation reproduces.
- Law: the precedence fold is the kernel's — `InputVerdict.Fold` takes the higher rank; this page adds no second fold and no local verdict type. NAMED LOSS (kernel-ruled): a boundary reads no host column off the kernel value — it projects through `VerdictSeam` at the one host edge.
- Packages: Grasshopper2 (`Response`, `ResponseMouseArgs`), Riok.Mapperly, `Rasm.Interaction` (`PointerFact`, `InputVerdict`), Thinktecture, `Rasm.Domain`.
- Growth: a new host precedence tier is one `VerdictSeam` row; a new boundary projection is one mapper row.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Riok.Mapperly.Abstractions;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VerdictSeam {
    public static readonly VerdictSeam Ignored = new(key: (int)Response.Ignored, verdict: InputVerdict.Ignored);
    public static readonly VerdictSeam Release = new(key: (int)Response.Release, verdict: InputVerdict.Release);
    public static readonly VerdictSeam Handled = new(key: (int)Response.Handled, verdict: InputVerdict.Handled);
    public static readonly VerdictSeam Capture = new(key: (int)Response.Capture, verdict: InputVerdict.Capture);

    public InputVerdict Verdict { get; }

    public Response Host => (Response)Key;

    public static Response ToHost(InputVerdict verdict) => Rows.Value[verdict].Host;

    public static Fin<InputVerdict> Of(Response response, Op key) =>
        TryGet((int)response, out VerdictSeam? row)
            ? Fin.Succ(row.Verdict)
            : Fin.Fail<InputVerdict>(key.InvalidInput(axis: nameof(Response)));

    private static readonly Lazy<System.Collections.Frozen.FrozenDictionary<InputVerdict, VerdictSeam>> Rows =
        new(static () => {
            var rows = Items.ToFrozenDictionary(static row => row.Verdict, static row => row);
            return InputVerdict.Items.All(rows.ContainsKey)
                ? rows
                : throw new InvalidOperationException("VerdictSeam rows drifted from the kernel InputVerdict roster.");
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
internal static partial class InputMap {
    internal static Fin<PointerFact> Fact(ResponseMouseArgs args, Control source, Op key) =>
        PointerFact.Of(args: args.UnderlyingEtoEventArgs, source: source, key: key)
            .Map(fact => fact with { Content = args.ContentLocation });

    [MapProperty(nameof(PopulateContextMenuEventArgs.Control), nameof(MenuMoment.Surface))]
    [MapProperty(nameof(PopulateContextMenuEventArgs.MouseEvent), nameof(MenuMoment.Cause))]
    [MapperIgnoreSource(nameof(PopulateContextMenuEventArgs.Menu))]
    [MapperIgnoreSource(nameof(PopulateContextMenuEventArgs.IsMenu))]
    internal static partial MenuMoment Moment(PopulateContextMenuEventArgs args);

    [MapProperty(nameof(ObjectDragInteraction.Count), nameof(DragFacts.Count))]
    [MapProperty(nameof(ObjectDragInteraction.FirstPoint), nameof(DragFacts.Anchor))]
    internal static partial DragFacts Facts(ObjectDragInteraction interaction);
}
```

## [03]-[DISPATCH]

- Owner: `GhResponder` — the kernel `ResponderSpec` beside the columns only GH2 names: the `CoordinateSystem` frame the host adapter constructs under, the optional rectangular `Boundary` (the host's own `RegionBoundary` fast path beside the kernel's predicate region), the rotation-gesture slot (`ResponseRotationArgs` is a GH2 surface no kernel phase carries), and the `HadEffect` probe (the host's transient-gesture pull contract, distinct from the kernel's post-dispatch `Effected` observer). Composition, never a forward: the kernel value IS the slot authority and the residue rides beside it.
- Owner: `SpecResponder` internal sealed — the ONE host adapter over `Responses`/`IResponsive`. Ten pointer overrides are ten one-line relays into one `Answer` fold keyed by the kernel `PointerPhase` row — the slot map replaces the ten near-identical override bodies — and every arm ends in the base member, so a plug-in subscriber on the same responder still gets its first-non-`Ignored` turn wherever a slot key is absent or answers `Ignored`. Every callback runs inside the raising `Op.Catch`; faults park on the mount's cell and settle `Release` while focused, `Ignored` while unfocused, so an extension raise never escapes into the Eto pump or strands capture.
- Owner: `Attachment` — the ONE attach-verify-detach fold: attach, verify the host table took it, answer the marshalled release that detaches and verifies removal; a failed verify rolls the attach back with the rollback refusal AGGREGATED. Three hand spellings (register/verify/unregister, push/verify/pop, subscribe/unsubscribe) and their five `ReferenceEquals` probes are this one fold's arms.
- Entry: `Dispatch.Mount(FlexControl surface, GhResponder responder, Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default, Op? key = null)` → `Fin<Lease<Mounted<Unit>>>`; `Dispatch.Hold(FlexControl surface, IResponsive target, Op? key = null)` → `Fin<Lease<Mounted<Unit>>>`; `Dispatch.Roster(IFlexControl surface, Op? key = null)` → `Fin<Seq<IResponsive>>`.
- Law: the hover pair is unconditional by host contract — a supplied `Over` or `Leave` key runs the slot AND the base relay, never instead of it; `HadEffect` returning false downgrades this responder's `Release` to `Ignored` so a no-drag right click still reaches context-menu population, and the absent probe inherits the host's `true`.
- Law: a supplied rail makes the responder the `interaction.verdict` fire site — a slot-answered verdict fires the veto point before the host edge, so a governance subscriber can refuse a capture; an absent rail dispatches ungoverned, which is the default a bare mount wants.
- Law: registration order is dispatch order (`ResponsivesForwards`) and focus preempts it; `Hold` refuses to claim an already-focused target, and the host focus stack pops only the NAMED target — the mount records no prior-focus column, because the host restores its own head and a stored copy was written and never read.
- Boundary: window-selection lifecycle verbs are `Canvas/canvas.md` marquee cases; `WindowSelection` and `MouseDwell` facts are `Shell/events.md` rows; `ObjectDragInteraction` neither registers nor focuses itself, so `[04]` acquires `Dispatch.Hold`.
- Packages: Grasshopper2 (`FlexControl` register/focus surface, `IResponsive`, `Responses` and its virtual family, `ResponseRotationArgs`, `CoordinateSystem`), Eto.Forms (`KeyEventArgs`, `TextInputEventArgs`), `Rasm.Interaction` (`ResponderSpec`, `PointerPhase`, `KeyPhase`, `PointerFact`, `InputVerdict`, `UiThread`, `UiDispatch<T>`, `UiClaim`), `Rasm.Domain` (`Op`, `Lease<T>`, `FaultCell`, `HookRail`), `Shell/hooks.md`, `Shell/session.md`.
- Growth: a new host handler virtual is one adapter relay reading one kernel phase key; a new attachment modality reuses `Attachment` and `Mounted<TFacts>`.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;

namespace Rasm.Grasshopper.Canvas;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GhResponder(
    ResponderSpec Spec,
    CoordinateSystem Frame,
    Option<RectangleF> Boundary,
    Option<Func<ResponseRotationArgs, InputVerdict>> Rotation,
    Option<Func<bool>> HadEffect) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Spec.IsValid || Boundary.IsSome || Rotation.IsSome,
        Boundary.ForAll(static frame => ValidityClaim.Finite(frame: frame).Holds));
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class SpecResponder : Responses, IResponsive {
    private readonly GhResponder responder;
    private readonly FlexControl surface;
    private readonly Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail;
    private readonly FaultCell faults;
    private readonly Op operation;

    internal SpecResponder(
        GhResponder responder,
        FlexControl surface,
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail,
        FaultCell faults,
        Op operation) : base(responder.Frame) {
        (this.responder, this.surface, this.rail, this.faults, this.operation) = (responder, surface, rail, faults, operation);
        responder.Boundary.Iter(region => RegionBoundary = region);
        responder.Spec.Filter.Iter(filter => RegionFilter = point => Guarded(filter: filter, point: point));
    }

    public override bool HadEffect => responder.HadEffect.Match(
        Some: probe => operation.Catch(() => Fin.Succ(probe())).IfFail(cause => (Park(cause), true).Item2),
        None: () => base.HadEffect);

    public override void MouseOver(ResponseMouseArgs e) => Beside(PointerPhase.Over, e, () => base.MouseOver(e));
    public override void MouseLeave() => Beside(PointerPhase.Leave, args: null, () => base.MouseLeave());
    public override Response MouseDown(ResponseMouseArgs e) => Answer(PointerPhase.Down, e, () => base.MouseDown(e));
    public override Response MouseDrag(ResponseMouseArgs e) => Answer(PointerPhase.Drag, e, () => base.MouseDrag(e));
    public override Response MouseUp(ResponseMouseArgs e) => Answer(PointerPhase.Up, e, () => base.MouseUp(e));
    public override Response MouseWheel(ResponseMouseArgs e) => Answer(PointerPhase.Wheel, e, () => base.MouseWheel(e));
    public override Response MouseSingleClick(ResponseMouseArgs e) => Answer(PointerPhase.SingleClick, e, () => base.MouseSingleClick(e));
    public override Response MouseDoubleClick(ResponseMouseArgs e) => Answer(PointerPhase.DoubleClick, e, () => base.MouseDoubleClick(e));
    public override Response KeyDown(KeyEventArgs e) => Keyed(KeyPhase.KeyDown, e, () => base.KeyDown(e));
    public override Response KeyUp(KeyEventArgs e) => Keyed(KeyPhase.KeyUp, e, () => base.KeyUp(e));
    public override Response TextInput(TextInputEventArgs e) => Slotted(responder.Spec.Text, e, () => base.TextInput(e));
    public override Response Rotation(ResponseRotationArgs e) => Slotted(responder.Rotation, e, () => base.Rotation(e));

    private Response Answer(PointerPhase phase, ResponseMouseArgs e, Func<Response> inherited) => Settle(
        operation.Catch(() => responder.Spec.Pointer.Find(phase).Match(
            Some: handle => InputMap.Fact(args: e, source: surface, key: operation)
                .Map(handle)
                .Bind(verdict => Governed(verdict: verdict)),
            None: () => VerdictSeam.Of(response: inherited(), key: operation))));

    private Response Keyed(KeyPhase phase, KeyEventArgs e, Func<Response> inherited);
    private Response Slotted<TEvent>(Option<Func<TEvent, InputVerdict>> slot, TEvent e, Func<Response> inherited) where TEvent : class;
    private void Beside(PointerPhase phase, ResponseMouseArgs? args, Action relay);

    private Fin<InputVerdict> Governed(InputVerdict verdict) => rail.Match(
        Some: live => live.Fire(
                at: GrasshopperPoint.InteractionVerdict,
                fact: new HookSignal.IntentCase(Operation: operation, DocumentId: None),
                key: operation)
            .Match(Succ: _ => Fin.Succ(verdict), Fail: _ => Fin.Succ(InputVerdict.Ignored)),
        None: () => Fin.Succ(verdict));

    private bool Guarded(Func<PointF, bool> filter, PointF point);

    private Unit Park(Error cause) {
        InteractionLog.ResponderFault(GhLog.For(category: nameof(SpecResponder)), cause.Message);
        return ignore(faults.Park(point: Rail, cause: cause));
    }

    private Response Settle(Fin<InputVerdict> outcome) => outcome.Match(
        Succ: static verdict => VerdictSeam.ToHost(verdict: verdict),
        Fail: cause => (Park(cause), HasFocus ? Response.Release : Response.Ignored).Item2);

    private static readonly HookId Rail = HookId.Create(value: "rasm.grasshopper.canvas.interaction");
}

internal static partial class InteractionLog {
    internal const int ResponderFaulted = 4714;
    static InteractionLog() => Op.SideWhen(
        condition: ResponderFaulted != FaultBand.GrasshopperLog.Code(offset: 14),
        action: static () => throw new InvalidOperationException("InteractionLog ids drifted from FaultBand.GrasshopperLog."));

    [LoggerMessage(EventId = ResponderFaulted, Level = LogLevel.Error, Message = "Responder callback faulted: {Detail}")]
    internal static partial void ResponderFault(ILogger logger, [UserContent] string detail);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Attachment {
    internal static Fin<Func<Fin<Unit>>> Of(Action attach, Func<bool> verify, Action detach, Op key);
}

[BoundaryAdapter]
public static class Dispatch {
    public static Fin<Lease<Mounted<Unit>>> Mount(
        FlexControl surface, GhResponder responder,
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default, Op? key = null);

    public static Fin<Lease<Mounted<Unit>>> Hold(FlexControl surface, IResponsive target, Op? key = null);

    public static Fin<Seq<IResponsive>> Roster(IFlexControl surface, Op? key = null);
}
```

## [04]-[SESSIONS]

- Owner: `DragFacts` — the drag-poll evidence (renamed from `DragEvidence`: the kernel input page owns that name for its threshold fact, and the two share zero columns). `DragSession` sealed `[BoundaryAdapter]` — the owned object-drag capsule over `ObjectDragInteraction`: `Begin` admits a finite anchor, constructs the host interaction inside the canvas scope, rejects an empty dragged set, and acquires its responder through `Dispatch.Hold` before the marshal returns; `Poll` answers `DragFacts` through the mapper seam; disposal is the cancellation path and idempotently pops any surviving frame.
- Owner: `EdgeResize` sealed `[BoundaryAdapter]` — the interactive resize capsule over `ResizingFrame`. Gesture state is one `Atom<Option<EdgeGrip>>` stepped through `Cell.Step` — `Begin` seats the engaged grip, `Track` requires one, `End` clears it — so the raw `int` + `Volatile` ladder and the bool-returning `Begin` are both gone: `Begin` answers `Fin<Option<EdgeGrip>>`, the kernel case family the caller wanted.
- Law: the grip is the KERNEL's case family — `EdgeGrip.Edge(GripEdge)`, `Corner(GripCorner)`, `Whole` — projected from the host's four edge bools at the one read: a single engaged edge is an `Edge`, an adjacent pair its `Corner`, all four `Whole`, and none is absence; the six meaningless corners of the mask (two opposite pairs, four triples) have no projection and no spelling.
- Law: admission ACCUMULATES — `EdgeResize.Of`'s eight bound clauses land as labeled `Validation` rows, so a caller with an inverted min/max AND a non-finite frame reads both refusals, not `InvalidInput` once.
- Law: both capsules are gesture-scoped and never cached across gestures; snap actions are per-drag host feedback, so `End` clears both snap axes — null is their rest state between drags. Pass-through `Original`/`Resized` mirrors are deleted: `Track` answers the resized frame and the caller holds what it passed to `Of`.
- Boundary: the component-attribute resize policy capsule (`Components/attributes.md`) composes the same host `ResizingFrame` under its own snap-restoration window; this owner is the canvas-general capsule and carries no component policy.
- Packages: Grasshopper2 (`ObjectDragInteraction`, `ResizingFrame`, `Canvas.SnapXAction`/`SnapYAction`, `SnappingConstraints`, `SnappingSettings`), `Rasm.Interaction` (`EdgeGrip`, `GripEdge`, `GripCorner`, `UiClaim`, `Op.ToHostSlot`), LanguageExt.Core, `Rasm.Domain`, `Shell/session.md`.
- Growth: a new gesture capsule is one sealed owner over its host interaction class; evidence rows widen by field, never by sibling record.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;

namespace Rasm.Grasshopper.Canvas;

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DragFacts(int Count, PointF Anchor) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Count > 0, ValidityClaim.Finite(point: Anchor));
}

// --- [SERVICES] ------------------------------------------------------------------------
[BoundaryAdapter]
public sealed class DragSession : IDisposable {
    private readonly Lease<Mounted<Unit>> focus;

    internal ObjectDragInteraction Interaction { get; }

    public static Fin<Lease<DragSession>> Begin(Document graph, PointF anchor, Op? key = null);

    public Fin<DragFacts> Poll(Op key) =>
        from live in guard(!focus.Resource.IsReleased, key.InvalidInput(axis: nameof(focus))).ToFin()
        from facts in key.AcceptValue(value: InputMap.Facts(interaction: Interaction))
        select facts;

    public void Dispose() => ignore(focus.Dispose());
}

[BoundaryAdapter]
public sealed class EdgeResize {
    private readonly ResizingFrame frame;
    private readonly Atom<Option<EdgeGrip>> engaged = Atom(Option<EdgeGrip>.None);

    public static Fin<EdgeResize> Of(
        RectangleF original, SizeF min, SizeF max,
        Option<SnappingConstraints> constraints = default, Option<SnappingSettings> settings = default, Op? key = null);

    public Fin<Option<EdgeGrip>> Begin(PointF mouse, Padding edges, Op key);
    public Fin<RectangleF> Track(PointF mouse, Op key);
    public Fin<Cursor> CursorAt(PointF mouse, Padding edges, Op key);

    public Fin<Unit> End(Op? key = null) =>
        Cell.Step(cell: engaged, step: static held => held.Map(static _ => Option<EdgeGrip>.None),
            declined: key.OrDefault().InvalidContext()) switch {
            Transition<Option<EdgeGrip>>.Refused row => Fin.Fail<Unit>(row.Cause),
            _ => Fin.Succ(unit),
        };

    public Option<EdgeGrip> Grip => engaged.Value;

    private static Option<EdgeGrip> Projected(ResizingFrame frame) =>
        (frame.ResizeTopEdge, frame.ResizeLeftEdge, frame.ResizeRightEdge, frame.ResizeBottomEdge) switch {
            (true, true, true, true) => Some<EdgeGrip>(new EdgeGrip.Whole()),
            (true, true, false, false) => Some<EdgeGrip>(new EdgeGrip.Corner(At: GripCorner.TopLeft)),
            (true, false, true, false) => Some<EdgeGrip>(new EdgeGrip.Corner(At: GripCorner.TopRight)),
            (false, true, false, true) => Some<EdgeGrip>(new EdgeGrip.Corner(At: GripCorner.BottomLeft)),
            (false, false, true, true) => Some<EdgeGrip>(new EdgeGrip.Corner(At: GripCorner.BottomRight)),
            (true, false, false, false) => Some<EdgeGrip>(new EdgeGrip.Edge(Side: GripEdge.Top)),
            (false, true, false, false) => Some<EdgeGrip>(new EdgeGrip.Edge(Side: GripEdge.Left)),
            (false, false, true, false) => Some<EdgeGrip>(new EdgeGrip.Edge(Side: GripEdge.Right)),
            (false, false, false, true) => Some<EdgeGrip>(new EdgeGrip.Edge(Side: GripEdge.Bottom)),
            _ => Option<EdgeGrip>.None,
        };
}
```

## [05]-[MOMENTS]

- Owner: `MenuMoment` — the populate-raise evidence; `MenuMount` — the synchronous population seam over `PopulateContextMenu`. Filler returns `Seq<MenuNode>` — the KERNEL menu vocabulary — and the seam resolves each node through the runtime's `IntentTable` into the host-supplied live menu inside the raise, so authoring rides the one kernel tree and this page keeps only the raise-and-project residue the host forces (the menu must fill before the handler returns).
- Law: whether ANY context menu opens is `Canvas/canvas.md`'s `ActionGate` policy; a filler that opens its own menu beside the host's is the double-menu defect.
- Law: the filler runs through `Op.Catch` and its faults park on the mount's cell; a refused node projection muted-drops that node and the rest of the menu fills — the per-row producer posture.
- Boundary: dwell facts remain `Shell/events.md`; tooltip content remains `Shell/chrome.md`; dwell timing writes ride `CanvasOperator.Apply(new CanvasOp.DwellCase(delay), clock, key)`.
- Packages: Grasshopper2 (`FlexControl.PopulateContextMenu`, `PopulateContextMenuEventArgs`), `Rasm.Interaction` (`MenuNode`, `IntentTable`), Eto.Forms (`ContextMenu`, `MouseEventArgs`), `Rasm.Domain`, `Shell/session.md`.
- Growth: a new synchronous host moment is one moment record with one mount; observation-shaped events stay `Shell/events.md` rows.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;

namespace Rasm.Grasshopper.Canvas;

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MenuMoment(IFlexControl Surface, MouseEventArgs Cause) : IValidityEvidence {
    public bool IsValid => Surface is not null && Cause is not null;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static class MenuMount {
    public static Fin<Lease<Mounted<Unit>>> Mount(
        Func<MenuMoment, Seq<MenuNode>> fill, IntentTable table, FaultCell faults, Op? key = null);
}
```

## [06]-[DENSITY_BAR]

| [INDEX] | [CONCERN]       | [OWNER]                         | [RAIL]                                              | [CASES] |
| :-----: | :-------------- | :------------------------------ | :-------------------------------------------------- | :-----: |
|  [01]   | verdict seam    | `VerdictSeam`                   | one row set, both directions                        |    4    |
|  [02]   | boundary feeds  | `InputMap`                      | two generated projections + one composed admission  |    3    |
|  [03]   | responders      | `GhResponder` + `SpecResponder` | kernel spec + GH2 residue, one phase-keyed fold     |    1    |
|  [04]   | attach custody  | `Attachment` + `Mounted<Unit>`  | one attach-verify-detach fold, aggregating rollback |    1    |
|  [05]   | gestures        | `DragSession` + `EdgeResize`    | kernel `EdgeGrip` family, `Cell.Step` state         |    2    |
|  [06]   | context moments | `MenuMount`                     | kernel `MenuNode` authoring, host fill residue      |    1    |

`ResponderSpec`, `PointerFact`, `InputVerdict`, `PointerPhase`/`KeyPhase`, `EdgeGrip`/`GripEdge`/`GripCorner`, and `UiClaim` are the kernel input estate's; the sixteen-slot local spec, its ten override bodies, `InteractionMount`, the local `Verdict`, the four-bool grip mask, the two `Finite` duplicates, and the `Target`/`PriorFocus` dead columns deleted onto it.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
