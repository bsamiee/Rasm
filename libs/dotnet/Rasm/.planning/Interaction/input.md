# [RASM_INPUT]

`Rasm.Interaction` owns the admitted input evidence and the one leased event module over it. A host callback hands raw device state exactly once, at the boundary, and what crosses into the interior is a finiteness-admitted fact carrying both coordinate frames it was measured in; a responder answers with a precedence verdict rather than a bare bool; and every host event source is a row whose attach returns its own detacher.

Both boundaries mounted their own event tables, their own pointer records, and their own verdict vocabularies. The evidence, the responder algebra, the source roster, and the bounded drain are one body here; what stays at each boundary is the genuinely host-shaped residue — the Rhino viewport callback with its veto semantics, and the Grasshopper canvas hit plane.

The module is GENERIC over the fact band it carries. `IUiFact` is the floor, `UiFact` is the Eto band this page instantiates, and each boundary declares its own closed union with a wrapping case over the kernel band — a canvas fact, a document fact, and a conduit fact ride the same subscription, drain, and total order the Eto facts do, because a union with a private root constructor cannot be extended and a host that cannot publish its own facts has to build the module twice.

## [01]-[INDEX]

- [02]-[POINTER]: `DisplayFacts`, `DisplayQuery`, `Displays`, `PointerSnapshot`, `InputState`, `PointerFact` — ambient host facts and the admitted pointer evidence.
- [03]-[GESTURE]: `EventTable`, `InputVerdict`, `PointerPhase`, `KeyPhase`, `DragPhase`, `ResponderSpec`, `DragEvidence`, `GestureFact`, `IUiFact`, `UiFact`, `LifecycleStage`, `UiEvent` — the host-event carrier, the precedence algebra, the phase vocabularies, the gesture evidence, and the extensible fact band.
- [04]-[PICK]: `PickAxis`, `PickGates`, `GripEdge`, `GripCorner`, `EdgeGrip`, `EventAnchor`, `IUiSource`, `UiSource`, `Atomicity`, `DrainPolicy`, `UiSubscription`, `UiEvents`, `EvidenceDrain` — the pick capability set, the source roster, and the bounded evidence drain that mints the total order.

## [02]-[POINTER]

- Owner: `DisplayFacts` the per-screen geometry and density snapshot; `Displays` the query surface over the screen set; `PointerSnapshot` the captured pointer frame; `InputState` the live device reads; `PointerFact` the admitted per-event evidence.
- Cases: a display query is `Primary`, `All`, `At(point)`, or `Covering(rect)` — four shapes over one entry, so an absent screen REFUSES on the result rather than dereferencing a null the host returns.
- Entry: `Displays.Resolve(query)` reads the screen set; `Error.New(bounds.Message, bounds)` is the ONE member minting a host image resource, and it returns a `Lease`; `InputState.Snapshot` captures the frame, `Held` reads the live provider predicate, `Locked` answers only for keys the platform's own lock set admits, and `Observe` leases a modifier watch.
- Auto: every density and geometry column is an admitted owner, so a screen's scale, its backing scale, its two dots-per-inch readings, and its colour depth cannot enter as raw primitives and the validity fold states only the claims the owners do not already hold. Both the logical and the REAL scale ride the snapshot, because a backing scale and a logical scale disagree on exactly the displays a hairline is drawn wrong on.
- Auto: `PointerFact` carries BOTH frames it was measured in — the control-local point and the content point — because a consumer that re-derives one from the other must know the scroll offset and the density, and every consumer that guessed produced a hit test off by the scroll. Finiteness is admitted at the boundary, so an interior consumer never re-checks.
- Law: `PointerSnapshot` is a CAPTURED frame and `InputState.Held` is a LIVE read, and the two never substitute — a snapshot's button mask answers what was true when the frame was taken, and a gesture that reads it as "is the button down now" drops every release that happened since.
- Law: `Locked` returns `Option<bool>` gated on the platform's supported-lock set: a platform that does not report caps-lock answers absence, never `false`, because `false` is a measurement and absence is not (`FORGED_ZERO`).
- Law: pressure is the concrete value `MouseEventArgs` supplies, including the host's fallback, and admission preserves that value directly rather than adding an absence carrier the provider cannot substantiate.
- Output: `DisplayFacts` and `PointerFact` are their own evidence and carry `IValidityEvidence` folds.
- Packages: Eto.Forms for `Screen`, `Mouse`, and `Keyboard` (verified in `libs/dotnet/.api/api-eto-runtime.md`); Eto.Drawing for the geometry carriers (prelude-aliased); `Numerics/atoms` for the admitted scalars.
- Growth: a new ambient fact is one column on `DisplayFacts`.
- Boundary: HOST-SPECIFIC-STAYS — the Rhino viewport pointer adapter keeps its whole family, because `MouseCallbackEventArgs` carries a VETO the host reads back and `RhinoView`'s static event tables have no host-neutral form; the Grasshopper canvas keeps its hit plane for the same reason.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using EtoImage = Eto.Drawing.Image;
using EtoPointF = Eto.Drawing.PointF;
using EtoRectangleF = Eto.Drawing.RectangleF;
using EtoSizeF = Eto.Drawing.SizeF;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DisplayQuery {
    private DisplayQuery() { }
    public sealed record Primary : DisplayQuery;
    public sealed record All : DisplayQuery;
    public sealed record At(EtoPointF Point) : DisplayQuery;
    public sealed record Covering(EtoRectangleF Region) : DisplayQuery;
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DisplayFacts(
    EtoRectangleF Bounds,
    EtoRectangleF WorkingArea,
    EtoRectangleF DisplayBounds,
    PositiveMagnitude LogicalPixelSize,
    PositiveMagnitude Dpi,
    PositiveMagnitude RealDpi,
    PositiveMagnitude Scale,
    PositiveMagnitude RealScale,
    Dimension Depth,
    bool Primary) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(frame: Bounds) && Bounds.Width > 0f && Bounds.Height > 0f,
        ValidityClaim.Finite(frame: WorkingArea) && WorkingArea.Width > 0f && WorkingArea.Height > 0f,
        ValidityClaim.Finite(frame: DisplayBounds) && DisplayBounds.Width > 0f && DisplayBounds.Height > 0f);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerSnapshot(EtoPointF Position, MouseButtons Buttons, Keys Modifiers);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerFact(
    EtoPointF Local, EtoPointF Content, MouseButtons Buttons, Keys Modifiers,
    EtoSizeF Delta, UnitInterval Pressure) : IValidityEvidence {
    public static Fin<PointerFact> Of(MouseEventArgs args, Control source);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(point: Local), ValidityClaim.Finite(point: Content),
        ValidityClaim.Finite(value: Delta.Width), ValidityClaim.Finite(value: Delta.Height));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UiClaim {
    extension(ValidityClaim) {
        public static ValidityClaim Finite(EtoPointF point) =>
            ValidityClaim.All(ValidityClaim.Finite(value: point.X), ValidityClaim.Finite(value: point.Y));
        public static ValidityClaim Finite(EtoRectangleF frame) => ValidityClaim.All(
            ValidityClaim.Finite(value: frame.X), ValidityClaim.Finite(value: frame.Y),
            ValidityClaim.Finite(value: frame.Width) && frame.Width >= 0f,
            ValidityClaim.Finite(value: frame.Height) && frame.Height >= 0f);
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class Displays {
    public static Fin<Seq<DisplayFacts>> Resolve(DisplayQuery query);
    public static Fin<Lease<EtoImage>> Capture(EtoRectangleF bounds);
}

public static class InputState {
    public static Fin<PointerSnapshot> Snapshot();
    public static Fin<bool> Held(MouseButtons buttons);
    public static Option<bool> Locked(Keys key);
    public static Fin<Lease<IDisposable>> Observe(Action<Keys> publish);
}
```

## [03]-[GESTURE]

- Owner: `EventTable<TOwner,TArgs>` the add-and-remove pair as one value; `InputVerdict` the precedence algebra a responder answers with; `PointerPhase`, `KeyPhase`, and `DragPhase` the three phase vocabularies, each carrying its own table; `ResponderSpec` the keyed slot set one region binds; `DragEvidence` the drag-threshold fact; `GestureFact` the recognized gesture; `IUiFact` the band floor; `UiFact` the Eto band; `UiEvent<TFact>` the ordered evidence a drain publishes.
- Cases: `InputVerdict` ranks `Ignored`, `Release`, `Handled`, `Capture` — the fold takes the higher rank, so two responders over one point compose without an ordering convention at the call site.
- Cases: `PointerPhase` closes the pointer axis at seven rows, `KeyPhase` at two, and `DragPhase` at five, each carrying the `EventTable` that names its own host event. NAMED LOSS: the fourteen per-slot property names on the old responder. Recovered by key — `spec.Pointer[PointerPhase.Down]` reads what `spec.Down` read, and the absent-slot-inherits law now holds as an ABSENT KEY rather than as fourteen `Option` columns each restating it. Witness: the Grasshopper responder's pointer overrides become one attach fold over `PointerPhase.Items`.
- Entry: `ResponderSpec` is a value one region binds; every phase attach comes from its own row, so no consumer names a host event and no consumer writes a handler per phase.
- Auto: a drag threshold is MEASURED — `DragEvidence` carries the press origin, the live pointer, and the slop as a Device-band `Tolerance`, and `Travel` and `Engaged` derive. Both boundaries stored an engaged flag beside a threshold they then re-compared, which disagrees the first time a platform changes its slop; the lane read is what makes that change one row rather than every mint site.
- Law: a responder answers a VERDICT, never a bool. A bool cannot distinguish "I did nothing", "I am done and release the capture", "I consumed this", and "I want every subsequent event", and the four are the whole reason a nested responder tree resolves deterministically.
- Law: every phase slot answers a verdict, including the two the old shape spelled as void actions. A leave and an over that cannot claim an event force the surface underneath to guess; the widening costs one return value and removes that guess.
- Law: an ABSENT phase key inherits the host's own behaviour, which is why the maps are keyed rather than defaulted — a no-op default silently consumes the event the host would otherwise have handled.
- Law: a phase is a ROW wherever one exists. A key event carries `KeyPhase` and a transfer drag carries `DragPhase`; a `bool Down` beside a two-row vocabulary is the flat form that makes a third phase unrepresentable.
- Law: a drop resolves through `Drop.Resolve` inside the `DragEventArgs` callback; `UiFact.DragCase` carries only the copied location, effect, and phase.
- Law: a host event pair is named ONCE, on the phase row that owns it, and travels as one `EventTable` value. The responder attach and the source roster are two readers of that one column, so a pair cannot be spelled at both and cannot drift between them; add and remove ride one value, so a subscription a row cannot undo is unspellable rather than merely discouraged.
- Law: the slop threshold derives from a `ToleranceLane` row and never from a caller magnitude. `Context.For` is the branch's ONE tolerance read, `Hit` is the Device-band gate a drag slop IS, and both sides of the engagement test are squared, so the predicate stays exact and pays no root.
- Law: the fact band is EXTENSIBLE by type parameter, not by case. `UiFact` stays the closed Eto band with a private root constructor, and a boundary declaring its own `IUiFact` union with a wrapping case over `UiFact` rides every owner on this page — so the kernel's dispatch stays total over what the kernel can construct, and a canvas or document fact is never a case the kernel cannot name.
- Law: `IUiFact` is a marker bound. Each closed fact band carries only its measured payload and dispatches through its own generated cases; no unchecked string roster repeats case identity on the shared floor.
- Law: `Verdict` at the Grasshopper boundary loses its host column here — `InputVerdict` carries the precedence fold alone, and each boundary supplies its own `OfHost`/`Host` projection. NAMED LOSS stated: a boundary can no longer read a host verdict off a kernel value, and it re-derives one from the case it answered.
- Output: `UiEvent<TFact>` carries the source, the fact, the monotonic stamp, and the drain-minted `Ordinal` — a consumer ordering two sources reads the ordinal, never arrival order and never a stamp two sources can tie on.
- Growth: a new Eto fact is one `UiFact` case that breaks every kernel dispatch loudly; a new host fact is one case on that host's own union; a new phase is one row.
- Boundary: `KeyEventArgs` and `TextInputEventArgs` cross as host types on the responder slots ALONE, because their veto members are read back by the host after the handler returns — a projected copy would drop the veto the host is waiting for.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using EtoPointF = Eto.Drawing.PointF;
using EtoRectangleF = Eto.Drawing.RectangleF;
using EtoSizeF = Eto.Drawing.SizeF;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct EventTable<TOwner, TArgs>(
    Action<TOwner, EventHandler<TArgs>> Add, Action<TOwner, EventHandler<TArgs>> Drop) where TArgs : EventArgs;

[SmartEnum<int>]
public sealed partial class InputVerdict {
    public static readonly InputVerdict Ignored = new(key: 0);
    public static readonly InputVerdict Release = new(key: 1);
    public static readonly InputVerdict Handled = new(key: 2);
    public static readonly InputVerdict Capture = new(key: 3);

    public InputVerdict Fold(InputVerdict other) => Key >= other.Key ? this : other;
}

[SmartEnum]
public sealed partial class PointerPhase {
    public static readonly PointerPhase Over = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseEnter += h, Drop: static (c, h) => c.MouseEnter -= h));
    public static readonly PointerPhase Leave = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseLeave += h, Drop: static (c, h) => c.MouseLeave -= h));
    public static readonly PointerPhase Down = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseDown += h, Drop: static (c, h) => c.MouseDown -= h));
    public static readonly PointerPhase Move = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseMove += h, Drop: static (c, h) => c.MouseMove -= h));
    public static readonly PointerPhase Up = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseUp += h, Drop: static (c, h) => c.MouseUp -= h));
    public static readonly PointerPhase Wheel = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseWheel += h, Drop: static (c, h) => c.MouseWheel -= h));
    public static readonly PointerPhase DoubleClick = new(
        table: new EventTable<Control, MouseEventArgs>(
            Add: static (c, h) => c.MouseDoubleClick += h, Drop: static (c, h) => c.MouseDoubleClick -= h));

    internal EventTable<Control, MouseEventArgs> Table { get; }

    internal Fin<IDisposable> Attach(Control control, Func<PointerFact, InputVerdict> respond) =>
        Bind(control: control, respond: respond, table: Table);

    private static Fin<IDisposable> Bind(
        Control control, Func<PointerFact, InputVerdict> respond,
        EventTable<Control, MouseEventArgs> table);
}

[SmartEnum]
public sealed partial class KeyPhase {
    public static readonly KeyPhase Down = new(
        table: new EventTable<Control, KeyEventArgs>(
            Add: static (c, h) => c.KeyDown += h, Drop: static (c, h) => c.KeyDown -= h));
    public static readonly KeyPhase Up = new(
        table: new EventTable<Control, KeyEventArgs>(
            Add: static (c, h) => c.KeyUp += h, Drop: static (c, h) => c.KeyUp -= h));

    internal EventTable<Control, KeyEventArgs> Table { get; }

    internal Fin<IDisposable> Attach(Control control, Func<KeyEventArgs, InputVerdict> respond) =>
        Bind(control: control, respond: respond, table: Table);

    private static Fin<IDisposable> Bind(
        Control control, Func<KeyEventArgs, InputVerdict> respond,
        EventTable<Control, KeyEventArgs> table);
}

[SmartEnum]
public sealed partial class DragPhase {
    public static readonly DragPhase Enter = new(
        table: new EventTable<Control, DragEventArgs>(
            Add: static (c, h) => c.DragEnter += h, Drop: static (c, h) => c.DragEnter -= h));
    public static readonly DragPhase Over = new(
        table: new EventTable<Control, DragEventArgs>(
            Add: static (c, h) => c.DragOver += h, Drop: static (c, h) => c.DragOver -= h));
    public static readonly DragPhase Leave = new(
        table: new EventTable<Control, DragEventArgs>(
            Add: static (c, h) => c.DragLeave += h, Drop: static (c, h) => c.DragLeave -= h));
    public static readonly DragPhase Drop = new(
        table: new EventTable<Control, DragEventArgs>(
            Add: static (c, h) => c.DragDrop += h, Drop: static (c, h) => c.DragDrop -= h));
    public static readonly DragPhase End = new(
        table: new EventTable<Control, DragEventArgs>(
            Add: static (c, h) => c.DragEnd += h, Drop: static (c, h) => c.DragEnd -= h));

    internal EventTable<Control, DragEventArgs> Table { get; }
}

[SmartEnum]
public sealed partial class LifecycleStage {
    public static readonly LifecycleStage Initialized = new();
    public static readonly LifecycleStage Load = new();
    public static readonly LifecycleStage Shown = new();
    public static readonly LifecycleStage Closing = new();
    public static readonly LifecycleStage Closed = new();
    public static readonly LifecycleStage Terminating = new();
}

public interface IUiFact { }

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UiFact : IUiFact {
    private UiFact() { }
    public sealed record GestureCase(GestureFact Fact) : UiFact;
    public sealed record KeyCase(Keys Key, Keys Modifiers, KeyPhase Phase) : UiFact {
        public bool BindsShortcut =>
            (Key & ~Keys.ModifierMask) is not Keys.None and var bare
            && ((Modifiers & (Keys.Application | Keys.Control | Keys.Alt)) != Keys.None
                || bare is >= Keys.F1 and <= Keys.F12 or Keys.Escape or Keys.Delete or Keys.Insert
                    or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown);
    }
    public sealed record TextCase(string Text) : UiFact;
    public sealed record DragCase(EtoPointF At, DragEffects Effect, DragPhase Phase) : UiFact;
    public sealed record FocusCase(bool Gained) : UiFact;
    public sealed record BoundsCase(EtoRectangleF Bounds) : UiFact;
    public sealed record DensityCase(PositiveMagnitude Scale) : UiFact;
    public sealed record StateCase(WindowState State) : UiFact;
    public sealed record LifeCase(LifecycleStage Stage) : UiFact;
    public sealed record ModifierCase(Keys Modifiers) : UiFact;
    public sealed record BeatCase(PulseBeat Beat) : UiFact;
    public sealed record NoticeCase(string Id, Option<string> Data) : UiFact;
    public sealed record FaultCase(Error Cause) : UiFact;
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DragEvidence(PointerFact Origin, PointerFact Current, Tolerance Slop) : IValidityEvidence {
    public static DragEvidence Of(PointerFact origin, PointerFact current, Context context) =>
        new(Origin: origin, Current: current, Slop: context.For(lane: ToleranceLane.Hit));

    public EtoSizeF Travel => new(width: Current.Local.X - Origin.Local.X, height: Current.Local.Y - Origin.Local.Y);

    public bool Engaged => (Travel.Width * Travel.Width) + (Travel.Height * Travel.Height) > Slop.Value * Slop.Value;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(evidence: Optional(Origin)),
        ValidityClaim.Evidence(evidence: Optional(Current)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct GestureFact(PointerPhase Phase, PointerFact Fact, Option<DragEvidence> Drag) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(evidence: Optional(Fact)),
        ValidityClaim.Evidence(evidence: Drag));
}

public sealed record ResponderSpec(
    HashMap<PointerPhase, Func<PointerFact, InputVerdict>> Pointer,
    HashMap<KeyPhase, Func<KeyEventArgs, InputVerdict>> Keys,
    Option<Func<TextInputEventArgs, InputVerdict>> Text,
    Option<Func<EtoPointF, bool>> Region,
    Option<Func<PointerFact, bool>> Filter,
    Option<Func<InputVerdict, Unit>> Effected) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Region.IsSome || Filter.IsSome,
        Pointer.Count + Keys.Count > 0 || Text.IsSome);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiEvent<TFact>(IUiSource<TFact> Source, TFact Fact, MonotonicStamp Stamp, long Ordinal) : IValidityEvidence
    where TFact : IUiFact {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(evidence: Optional(Stamp)),
        Ordinal >= 0L);
}
```

## [04]-[PICK]

- Owner: `PickAxis` the pick capability vocabulary and `PickGates` its three named sets; `GripEdge` and `GripCorner` the two-dimensional grip vocabularies; `EdgeGrip` the grip family; `IUiSource<TFact>` the source floor; `UiSource` the Eto event-source roster; `EventAnchor` what a subscription attaches to; `Atomicity` the attachment posture; `UiEvents` the one observe entry; `EvidenceDrain<TFact>` the bounded evidence channel and the total order's one minter.
- Cases: `EventAnchor` is `OnControl`, `OnWindow`, `Ambient`, or `OnClock` — four attach shapes over one entry, so a subscription's lifetime is recoverable from the value. Five binders serve them, because the ambient case splits by publisher: an application event and a static input table are two host surfaces under one anchor.
- Cases: `Atomicity` is `Partial` — a refused row leaves the others live and the subscription reports which — or `AllOrNothing`, which detaches every attached row and refuses the whole observe. Both are lawful postures a caller NAMES: a diagnostic panel wants whatever it can get and a replayable journal wants all or none, and neither can be inferred from the row set.
- Entry: `UiEvents.Observe(anchor, drain, atomicity, rows)` attaches the named source rows into ONE drain and returns a `Lease<UiSubscription<TFact>>` whose disposal detaches every row in reverse order.
- Auto: the roster carries 30 rows — twenty control, four window, five ambient, one clock — each naming its event table and the fact it projects. That pairing is the whole reason a consumer never writes `+=`: the table carries add and remove together, so a row stating a subscription it cannot undo is unspellable. The fourteen rows with a phase counterpart read that phase row's table rather than restating it.
- Auto: pick capability is a `CapabilitySet<PickAxis>`, so the five-bool gate both boundaries carried becomes one column and the three named gates become three set literals — `Whole` is the full set, `Bodies` is the set difference the record-`with` subtraction was spelling by hand, and `Wiring` is a singleton. Each is accessor-backed, because the generated roster fills from its own static constructor and an eager static field would freeze an EMPTY set.
- Law: the ORDINAL has one minter. `EvidenceDrain.Publish` mints the stamp and the ordinal together under one compare-and-swap and refuses at saturation, so two sources publishing in one frame serialize and a replay reads the order the sink observed. NAMED LOSS: the free `Action<UiEvent>` publication sink deletes — `Observe` writes into a drain and a consumer reads `drain.Reader`. Witness: an inline publish closure becomes a read loop over the reader, and that is precisely what makes the order replayable rather than callback-scheduled.
- Law: `EdgeGrip` is a CASE family over a TWO-DIMENSIONAL vocabulary. A screen grip has four edges and four corners; a three-dimensional signed axis admits a depth row no window frame has, and a corner spelled as two independent edges admits left-with-right. `GripCorner` closes the four legal corners and carries its two edges as columns, so the pair is readable and has one authority.
- Law: `EvidenceDrain` is BOUNDED and accounts BOTH losses — a shed count for evidence the bound dropped and a refused count for a thunk whose admission failed. A UI event storm drops rather than growing a queue for process lifetime, and each count is what makes its own loss observable; a drop-mode channel reports ADMISSION, never delivery, so a write result is not the evidence.
- Law: writer completion is idempotent and total — `Complete` terminates the reader's loop no matter how many detach paths reach it, which is what a single-reader consumer needs to finish rather than block on a channel nothing will write again.
- Law: every attach and detach marshals through `UiThread` — a subscription wired off the marshal races the host's own table.
- Output: `UiSubscription` carries the rows it attached beside the rows it refused; `Shed` and `Refused` are the drain's own accounting. Neither is a return value.
- Packages: Eto.Forms for the event surfaces (rosters verified in `libs/dotnet/.api/api-eto-forms.md` and `api-eto-runtime.md`); `System.Threading.Channels` for the bounded drain; `Parametric/projections` for the timeline that stamps; LanguageExt.Core for the leases and types.
- Growth: a new Eto source is one `UiSource` row carrying its own table; a source over an existing phase costs no new table at all; a new HOST source roster is one `IUiSource<TFact>` implementation at that boundary; a new pick axis is one `PickAxis` row.
- Boundary: a host event table is named on a ROW and nowhere else — the phase rosters own every pointer, key, and drag pair, and this roster owns the rest — so a consumer subscribes by row and never by `+=`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.ComponentModel;
using System.Threading.Channels;
using Eto.Forms;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PickAxis : ICapability<PickAxis> {
    public static readonly PickAxis Grips = new(key: "grips", rank: 0);
    public static readonly PickAxis Foreground = new(key: "foreground", rank: 1);
    public static readonly PickAxis Background = new(key: "background", rank: 2);
    public static readonly PickAxis Wires = new(key: "wires", rank: 3);
    public static readonly PickAxis Recursive = new(key: "recursive", rank: 4);

    public int Rank { get; }
}

[SmartEnum]
public sealed partial class GripEdge {
    public static readonly GripEdge Left = new();
    public static readonly GripEdge Right = new();
    public static readonly GripEdge Top = new();
    public static readonly GripEdge Bottom = new();
}

[SmartEnum]
public sealed partial class GripCorner {
    public static readonly GripCorner TopLeft = new(across: GripEdge.Left, down: GripEdge.Top);
    public static readonly GripCorner TopRight = new(across: GripEdge.Right, down: GripEdge.Top);
    public static readonly GripCorner BottomLeft = new(across: GripEdge.Left, down: GripEdge.Bottom);
    public static readonly GripCorner BottomRight = new(across: GripEdge.Right, down: GripEdge.Bottom);

    public GripEdge Across { get; }
    public GripEdge Down { get; }
}

[Union<GripEdge, GripCorner, Unit>(T1Name = "Edge", T2Name = "Corner", T3Name = "Whole", T3IsStateless = true)]
public readonly partial struct EdgeGrip;

[SmartEnum]
public sealed partial class Atomicity {
    public static readonly Atomicity Partial = new();
    public static readonly Atomicity AllOrNothing = new();
}

[Union<Control, Window, Unit, UiClock>(
    T1Name = "OnControl", T2Name = "OnWindow", T3Name = "Ambient", T4Name = "OnClock", T3IsStateless = true)]
public readonly partial struct EventAnchor;

public interface IUiSource<TFact> where TFact : IUiFact {
    string Key { get; }
    Fin<IDisposable> Attach(EventAnchor anchor, Action<Func<Fin<TFact>>> emit);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UiSource : IUiSource<UiFact> {
    // --- [CONTROL]
    public static readonly UiSource PointerOver = new(key: "pointer.over",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Over.Table, Gesture(PointerPhase.Over)));
    public static readonly UiSource PointerLeave = new(key: "pointer.leave",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Leave.Table, Gesture(PointerPhase.Leave)));
    public static readonly UiSource PointerDown = new(key: "pointer.down",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Down.Table, Gesture(PointerPhase.Down)));
    public static readonly UiSource PointerMove = new(key: "pointer.move",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Move.Table, Gesture(PointerPhase.Move)));
    public static readonly UiSource PointerUp = new(key: "pointer.up",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Up.Table, Gesture(PointerPhase.Up)));
    public static readonly UiSource PointerWheel = new(key: "pointer.wheel",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.Wheel.Table, Gesture(PointerPhase.Wheel)));
    public static readonly UiSource PointerDouble = new(key: "pointer.double",
        attach: static (anchor, emit) => OnControl(anchor, emit, PointerPhase.DoubleClick.Table, Gesture(PointerPhase.DoubleClick)));

    public static readonly UiSource KeyDown = new(key: "key.down",
        attach: static (anchor, emit) => OnControl(anchor, emit, KeyPhase.Down.Table, Keyed(KeyPhase.Down)));
    public static readonly UiSource KeyUp = new(key: "key.up",
        attach: static (anchor, emit) => OnControl(anchor, emit, KeyPhase.Up.Table, Keyed(KeyPhase.Up)));
    public static readonly UiSource TextInput = new(key: "key.text",
        attach: static (anchor, emit) => OnControl(anchor, emit,
            new EventTable<Control, TextInputEventArgs>(
                Add: static (c, h) => c.TextInput += h, Drop: static (c, h) => c.TextInput -= h),
            static (_, args) => Fin.Succ<UiFact>(new UiFact.TextCase(Text: args.Text))));

    public static readonly UiSource DragEnter = new(key: "drag.enter",
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Enter.Table, Dragged(DragPhase.Enter)));
    public static readonly UiSource DragOver = new(key: "drag.over",
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Over.Table, Dragged(DragPhase.Over)));
    public static readonly UiSource DragLeave = new(key: "drag.leave",
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Leave.Table, Dragged(DragPhase.Leave)));
    public static readonly UiSource DragDrop = new(key: "drag.drop",
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.Drop.Table, Dragged(DragPhase.Drop)));
    public static readonly UiSource DragEnd = new(key: "drag.end",
        attach: static (anchor, emit) =>
            OnControl(anchor, emit, DragPhase.End.Table, Dragged(DragPhase.End)));

    public static readonly UiSource FocusGained = new(key: "focus.gained",
        attach: static (anchor, emit) => OnControl(anchor, emit,
            new EventTable<Control, EventArgs>(
                Add: static (c, h) => c.GotFocus += h, Drop: static (c, h) => c.GotFocus -= h),
            Focused(gained: true)));
    public static readonly UiSource FocusLost = new(key: "focus.lost",
        attach: static (anchor, emit) => OnControl(anchor, emit,
            new EventTable<Control, EventArgs>(
                Add: static (c, h) => c.LostFocus += h, Drop: static (c, h) => c.LostFocus -= h),
            Focused(gained: false)));
    public static readonly UiSource Resized = new(key: "control.resized",
        attach: static (anchor, emit) => OnControl(anchor, emit,
            new EventTable<Control, EventArgs>(
                Add: static (c, h) => c.SizeChanged += h, Drop: static (c, h) => c.SizeChanged -= h),
            static (control, _) => Fin.Succ<UiFact>(new UiFact.BoundsCase(Bounds: control.Bounds))));
    public static readonly UiSource Loaded = new(key: "control.loaded",
        attach: static (anchor, emit) => OnControl(anchor, emit,
            new EventTable<Control, EventArgs>(
                Add: static (c, h) => c.Load += h, Drop: static (c, h) => c.Load -= h),
            Staged<Control, EventArgs>(LifecycleStage.Load)));
    public static readonly UiSource Shown = new(key: "control.shown",
        attach: static (anchor, emit) => OnControl(anchor, emit,
            new EventTable<Control, EventArgs>(
                Add: static (c, h) => c.Shown += h, Drop: static (c, h) => c.Shown -= h),
            Staged<Control, EventArgs>(LifecycleStage.Shown)));

    // --- [WINDOW]
    public static readonly UiSource Closing = new(key: "window.closing",
        attach: static (anchor, emit) => OnWindow(anchor, emit,
            new EventTable<Window, CancelEventArgs>(
                Add: static (w, h) => w.Closing += h, Drop: static (w, h) => w.Closing -= h),
            Staged<Window, CancelEventArgs>(LifecycleStage.Closing)));
    public static readonly UiSource Closed = new(key: "window.closed",
        attach: static (anchor, emit) => OnWindow(anchor, emit,
            new EventTable<Window, EventArgs>(
                Add: static (w, h) => w.Closed += h, Drop: static (w, h) => w.Closed -= h),
            Staged<Window, EventArgs>(LifecycleStage.Closed)));
    public static readonly UiSource StateChanged = new(key: "window.state",
        attach: static (anchor, emit) => OnWindow(anchor, emit,
            new EventTable<Window, EventArgs>(
                Add: static (w, h) => w.WindowStateChanged += h, Drop: static (w, h) => w.WindowStateChanged -= h),
            static (window, _) => Fin.Succ<UiFact>(new UiFact.StateCase(State: window.WindowState))));
    public static readonly UiSource DensityChanged = new(key: "window.density",
        attach: static (anchor, emit) => OnWindow(anchor, emit,
            new EventTable<Window, EventArgs>(
                Add: static (w, h) => w.LogicalPixelSizeChanged += h,
                Drop: static (w, h) => w.LogicalPixelSizeChanged -= h),
            static (window, _) => FactoryBridge.Accept<PositiveMagnitude>(
                    PositiveMagnitude.Validate(window.LogicalPixelSize, provider: null, out PositiveMagnitude scale),
                    scale)
                .Map(static admitted => (UiFact)new UiFact.DensityCase(Scale: admitted))));

    // --- [AMBIENT]
    public static readonly UiSource Initialized = new(key: "app.initialized",
        attach: static (anchor, emit) => OnApp(anchor, emit,
            new EventTable<Application, EventArgs>(
                Add: static (a, h) => a.Initialized += h, Drop: static (a, h) => a.Initialized -= h),
            Staged<Application, EventArgs>(LifecycleStage.Initialized)));
    public static readonly UiSource Terminating = new(key: "app.terminating",
        attach: static (anchor, emit) => OnApp(anchor, emit,
            new EventTable<Application, CancelEventArgs>(
                Add: static (a, h) => a.Terminating += h, Drop: static (a, h) => a.Terminating -= h),
            Staged<Application, CancelEventArgs>(LifecycleStage.Terminating)));
    public static readonly UiSource Raised = new(key: "app.raised",
        attach: static (anchor, emit) => OnApp(anchor, emit,
            new EventTable<Application, UnhandledExceptionEventArgs>(
                Add: static (a, h) => a.UnhandledException += h,
                Drop: static (a, h) => a.UnhandledException -= h),
            static (_, args) => Admit.Need(value: args.ExceptionObject as Exception)
                .Map(raised => (UiFact)new UiFact.FaultCase(Cause: Error.New(raised.Message, raised)))));
    public static readonly UiSource Notified = new(key: "app.notified",
        attach: static (anchor, emit) => OnApp(anchor, emit,
            new EventTable<Application, NotificationEventArgs>(
                Add: static (a, h) => a.NotificationActivated += h,
                Drop: static (a, h) => a.NotificationActivated -= h),
            static (_, args) => Fin.Succ<UiFact>(new UiFact.NoticeCase(Id: args.ID, Data: Optional(args.UserData)))));
    public static readonly UiSource ModifiersChanged = new(key: "input.modifiers",
        attach: static (anchor, emit) => OnAmbient(anchor, emit,
            static h => Keyboard.ModifiersChanged += h, static h => Keyboard.ModifiersChanged -= h,
            static () => Fin.Succ<UiFact>(new UiFact.ModifierCase(Modifiers: Keyboard.Modifiers))));

    // --- [CLOCK]
    public static readonly UiSource Beat = new(key: "clock.beat",
        attach: static (anchor, emit) => OnClock(anchor, emit,
            static (clock, observer) => clock.Tap(observer: observer),
            static beat => Fin.Succ<UiFact>(new UiFact.BeatCase(Beat: beat))));

    [UseDelegateFromConstructor]
    public partial Fin<IDisposable> Attach(EventAnchor anchor, Action<Func<Fin<UiFact>>> emit);

    private static Fin<IDisposable> OnControl<TArgs>(
        EventAnchor anchor, Action<Func<Fin<UiFact>>> emit,
        EventTable<Control, TArgs> table,
        Func<Control, TArgs, Fin<UiFact>> project) where TArgs : EventArgs;

    private static Fin<IDisposable> OnWindow<TArgs>(
        EventAnchor anchor, Action<Func<Fin<UiFact>>> emit,
        EventTable<Window, TArgs> table,
        Func<Window, TArgs, Fin<UiFact>> project) where TArgs : EventArgs;

    private static Fin<IDisposable> OnApp<TArgs>(
        EventAnchor anchor, Action<Func<Fin<UiFact>>> emit,
        EventTable<Application, TArgs> table,
        Func<Application, TArgs, Fin<UiFact>> project) where TArgs : EventArgs;

    private static Fin<IDisposable> OnAmbient(
        EventAnchor anchor, Action<Func<Fin<UiFact>>> emit,
        Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> drop,
        Func< Fin<UiFact>> project);

    private static Fin<IDisposable> OnClock(
        EventAnchor anchor, Action<Func<Fin<UiFact>>> emit,
        Func<UiClock, Action<PulseBeat>, Fin<IDisposable>> tap,
        Func<PulseBeat, Fin<UiFact>> project);

    private static Func<Control, MouseEventArgs, Fin<UiFact>> Gesture(PointerPhase phase) =>
        (control, args) => PointerFact.Of(args: args, source: control)
            .Map(fact => (UiFact)new UiFact.GestureCase(
                Fact: new GestureFact(Phase: phase, Fact: fact, Drag: Option<DragEvidence>.None)));

    private static Func<Control, KeyEventArgs, Fin<UiFact>> Keyed(KeyPhase phase) =>
        (_, args) => Fin.Succ<UiFact>(new UiFact.KeyCase(Key: args.Key, Modifiers: args.Modifiers, Phase: phase));

    private static Func<Control, DragEventArgs, Fin<UiFact>> Dragged(DragPhase phase) =>
        (_, args) => (phase == DragPhase.Drop
                ? Drop.Resolve(source: args, effect: args.Effects)
                : Fin.Succ(unit))
            .Map(_ => (UiFact)new UiFact.DragCase(At: args.Location, Effect: args.Effects, Phase: phase));

    private static Func<TOwner, TArgs, Fin<UiFact>> Staged<TOwner, TArgs>(LifecycleStage stage) =>
        (_, _) => Fin.Succ<UiFact>(new UiFact.LifeCase(Stage: stage));

    private static Func<Control, EventArgs, Fin<UiFact>> Focused(bool gained) =>
        (_, _) => Fin.Succ<UiFact>(new UiFact.FocusCase(Gained: gained));
}

// --- [MODELS] --------------------------------------------------------------------------
public static class PickGates {
    public static CapabilitySet<PickAxis> Whole => whole.Value;
    public static CapabilitySet<PickAxis> Bodies => bodies.Value;
    public static CapabilitySet<PickAxis> Wiring => wiring.Value;

    private static readonly Lazy<CapabilitySet<PickAxis>> whole = new(static () => CapabilitySet<PickAxis>.All);
    private static readonly Lazy<CapabilitySet<PickAxis>> bodies = new(static () =>
        CapabilitySet<PickAxis>.Of(PickAxis.Foreground, PickAxis.Background, PickAxis.Recursive));
    private static readonly Lazy<CapabilitySet<PickAxis>> wiring = new(static () =>
        CapabilitySet<PickAxis>.Of(PickAxis.Wires));
}

public sealed record DrainPolicy(Dimension Capacity, BoundedChannelFullMode Full) {
    public static DrainPolicy Default { get; } = new(
        Capacity: Dimension.Create(value: (int)(TimeSpan.FromSeconds(value: 1d) / DispatchLane.Paced.Bound)),
        Full: BoundedChannelFullMode.DropOldest);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class UiSubscription<TFact> : IDisposable where TFact : IUiFact {
    public Seq<IUiSource<TFact>> Attached { get; }
    public Seq<(IUiSource<TFact> Row, Error Cause)> Refused { get; }
    public void Dispose();
}

public sealed class EvidenceDrain<TFact> : IDisposable where TFact : IUiFact {
    private readonly Atom<long> ordinal = Atom(0L);

    public static Fin<Lease<EvidenceDrain<TFact>>> Open(
        MonotonicTimeline clock,
        Option<DrainPolicy> policy = default);

    public ChannelReader<UiEvent<TFact>> Reader { get; }
    public long Shed { get; }
    public long Refused { get; }

    public Fin<UiEvent<TFact>> Publish(IUiSource<TFact> source, Func<Fin<TFact>> fact);

    public Unit Complete();

    public void Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UiEvents {
    public static Fin<Lease<UiSubscription<TFact>>> Observe<TFact>(
        EventAnchor anchor,
        EvidenceDrain<TFact> drain,
        Atomicity atomicity,
        params ReadOnlySpan<IUiSource<TFact>> rows) where TFact : IUiFact;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
