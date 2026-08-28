# [RASM_CHROME]

`Rasm.Interaction` owns the one interactive-verb vocabulary and every chrome that vocabulary projects into. A verb is a ROW — its identity, its text, its gesture, its availability probe, its behaviour, and every surface occurrence it claims — and a menu bar, a toolbar, a context menu, a window, a modal, a native prompt, a printed document, and an OS presence are projections of that one row set rather than eight parallel registries. Two projection shapes survive on a named discriminant: PLACEMENT is declarative and rank-ordered, NODE is authored and order-preserving, and neither can express the other without capping a depth or forcing an ordering nothing declared.

Both host boundaries carried the whole spine and each held half of it. Rhino held the placement algebra, the behaviour-carrying command family, the three chrome projections with their conflict admission, the window realize with its reverse-order chrome teardown, the injected-presenter modal, and the entire print flow; Grasshopper held the command mint, the recursive menu tree with its lease-owned recursive detach, the latency-and-fault journal echo, the window posture record with its live mutation verbs, the styling function, the mount custody triple, and the eight-case native prompt family. This owner is their union at every axis. Host residue stays where its host types live: Rhino's `.rui` toolbar-file catalog and `MenuLinks` live menu mutation are that host's OWN menu system and are not this owner, its `Rhino.UI` panel and page registries stay, its multi-value document-scoped `Inquiry` dialogs stay, and Grasshopper2 canvas chrome, editor panes, and the session styling target stay — the kernel carries an optional styling function supplied by each host.

Composition is downward and sideways inside the sub-domain: `Lease<T>`, `Atom`, `Cell`/`Transition`, `Validation`, `ValidityClaim`, `CapabilitySet<TCapability>`, `ICapability<TSelf>`, and `CapabilityLaw<TCapability>` from `Domain`; `ModelUnit` from `Domain/context`; `Dimension`, `UnitInterval`, and `PerceptualColor` from `Numerics/atoms`; `MonotonicTimeline` and `GaugedSpan<TLane>` from `Parametric/projections`; `SheetSize`, `SheetMargin`, and `SheetOrientation` from `Drawing/sheet`; `FaultCell` from `Domain/hooks`; `UiFault`, `RejectReason`, `UiDispatch<T>`, `DispatchLane`, and `UiThread` from `Interaction/dispatch`; `ControlSpec`, `ElementRuntime`, `ElementMount`, `FieldTag`, and `NumberPolicy` from `Interaction/control`; `IUiFact`, `IUiSource<TFact>`, `EventAnchor`, and `EvidenceDrain<TFact>` from `Interaction/input`; `PlatformRequirement` and `HostPlatform` from `Interaction/platform`; `PaintProgram`, `ScenePolicy`, and `TypeFace` from `Interaction/paint`; `IconRender`, `AssetExtent`, and `FileLocation` from `Interaction/asset`.

## [01]-[INDEX]

- [02]-[INTENT]: `IntentKey`, `PlacementKey`, `GroupKey`, `SubMenuKey`, `JobName`, `CommandKind`, `PlacementSlot`, `IntentRow`, `Invocation`, `IntentTable` — the verb row set, its behaviour owner, and the three declarative chrome projections.
- [03]-[MENU]: `MenuNode`, `MenuMode`, `MenuSlot`, `MenuBranch`, `OwnedContextMenu`, `MenuTree` — the authored node tree, its limits, its flat projection, and its recursively lease-owned realization.
- [04]-[WINDOW]: `ShellCapability`, `WindowRole`, `MountPhase`, `WindowChrome`, `WindowSpec`, `WindowVerb`, `IMount`, `MountCustody`, `WindowMount` — the window posture, its styling function, its live verbs, the custody machine every mount on this page composes, and the mount that owns every host object the realize minted.
- [05]-[PROMPT]: `PromptChoice<TResult>`, `Prompt<TResult>`, `FilterPlan`, `PickerSpec`, `PickerResult`, `PickerDemand<TResult>`, `AskVerdict`, `AskTrait`, `AskDelivery`, `AskModality`, `AskPolicy` — the result-typed modal over an injected presenter and the native prompt family as cases of one gate, each case answering its own typed result.
- [06]-[PRINT]: `PrintRoute`, `PageFrame`, `PageSpan`, `PrintScope`, `CollatePosture`, `PageOrder`, `PrintSpec`, `PrintPage`, `PrintPageFact`, `PrintOutcome`, `PrintPlan` — the deferred document run, its sheet-composed page geometry, and its per-page evidence.
- [07]-[PRESENCE]: `ToastKey`, `Toast`, `PulseState`, `PresenceOp`, `PresenceHold`, `PresenceMount`, `Presence` — the OS presence family as cases of one applied-and-restored gate.

## [02]-[INTENT]

- Owner: `IntentKey` the verb identity, `PlacementKey` the surface identity, `GroupKey` the radio-group identity, `SubMenuKey` the submenu identity, `JobName` the print job's; `CommandKind` the closed behaviour and command-mint family; `PlacementSlot` one surface occurrence; `IntentRow` one verb; `Invocation` one invocation fact; `IntentTable` the materialized deck every projection borrows from.
- Cases: `Act` runs an effect, `Toggle` reads and writes a two-state, `Pick` reads and chooses inside a named radio group. Behaviour is the CASE and mint posture is the ROW, and the row DERIVES from the case — the two boundaries carried these as two independent vocabularies, so a Grasshopper spec could name a radio mint with no group read and a Rhino kind could carry behaviour no mint supported.
- Entry: `Materialize` is the one construction and answers a leased table; `Verb` resolves the host command, `Invoke` the programmatic raise, `Attach` seats the relay every raise publishes through, `RefreshAvailability` the state sweep, and `MenuOf`/`BarOf`/`PopupOf` the three placement projections.
- Auto: identity, gesture, and placement-rank conflicts are THREE independent admissions accumulated through `Validation` before a single command mints, so an author with three defects reads all three rather than being sent back three times.
- Auto: every raise publishes ONE fact through the sub-domain's evidence drain, which is the single minter of the ordinal and the stamp under one compare-and-swap — a second counter here would hand two events one order, and the drain is bounded, so a verb storm sheds against a declared capacity and reports the shed rather than growing a journal for process life.
- Law: latency rides `MonotonicTimeline.Gauged` on `DispatchLane.Interactive` — an invocation IS an interactive crossing and the kernel timeline is the one gauge, so no stopwatch pair exists here. The span is `Option`-shaped because a refused crossing produces none, and a zero-elapsed sentinel is indistinguishable from an instantaneous verb.
- Law: `Invocation` carries `Fin<Unit>`, so consumers match the outcome directly rather than reading parallel settled and fault columns.
- Law: the icon is an `IconRender`, so a verb names WHAT to draw and the asset owner resolves it into the toolkit stack this chrome takes; the resolved bitmap lease rides the bound entry and releases in mint-reverse order with its command. Both boundaries passed a live host image into the row, which left the image with no owner the moment a table rebuilt.
- Law: the table owns every command each projection borrows, so it releases LAST — a window releasing its table while a realized menu still carries those commands releases in the wrong order. Detach precedes dispose at every entry, because a detach-only sever strands one host command widget per verb on every rebuild.
- Output: `Invocation` per raise, UI-driven and programmatic alike, published as an `IUiFact` into the caller's drain — a palette ranking, a usage attribution, and a failure surface all fold `drain.Reader`, and the drain's `Shed` and `Refused` are where each loss reads as a number. `IntentTable` is itself an `IUiSource<IUiFact>`, so a consumer subscribing it and a consumer reading the drain see one order.
- Packages: Eto.Forms for `Command`/`CheckCommand`/`RadioCommand`, `ContextMenu`, `MenuBar`, and `ToolBar` — the menu-bar and toolbar chrome and the two `Command` projection members are seated at `libs/dotnet/.api/api-eto-forms.md`; LanguageExt.Core for `Fin`/`Validation`/`Seq`/`Atom`/`Lease`; Thinktecture.Runtime.Extensions for the union and the keyed rows; `Interaction/input` for the bounded `EvidenceDrain` every raise publishes through.
- Growth: a verb is one `IntentRow`; a surface occurrence is one `PlacementSlot`; a behaviour modality is one `CommandKind` case carrying its mint and execution behavior, and every projection is untouched; a new identity space is one value object beside the four already here.
- Boundary: Rhino's `.rui` toolbar files, `RhinoApp.ToolbarFiles`, and `MenuLinks` live menu mutation are that host's OWN menu catalog over its own persistence format — conflating them with this table is the sharpest trap in the boundary corpus, because the two answer different questions about different files.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct IntentKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "IntentKey requires a non-blank identity.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct PlacementKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "PlacementKey requires a non-blank identity.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct GroupKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "GroupKey requires a non-blank identity.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct SubMenuKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "SubMenuKey requires a non-blank identity.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct JobName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "JobName requires a non-blank job name.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandKind {
    private CommandKind() { }

    public sealed record Act(Func<Fin<Unit>> Effect) : CommandKind;
    public sealed record Toggle(Func<bool> Read, Action<bool> Write) : CommandKind;
    public sealed record Pick(GroupKey Group, Func<bool> Read, Action Choose) : CommandKind;

    internal (Command Host, Option<GroupKey> Group) Mint(Option<RadioCommand> controller) => Switch(
        state: controller,
        act: static (_, _) => ((Command)new Command(), Option<GroupKey>.None),
        toggle: static (_, kind) => (
            (Command)new CheckCommand { Checked = kind.Read() }, Option<GroupKey>.None),
        pick: static (head, kind) => ((Command)head.Match(
            Some: command => new RadioCommand { Controller = command, Checked = kind.Read() },
            None: () => new RadioCommand { Checked = kind.Read() }), Some(kind.Group)));

    internal Fin<Unit> Execute(Command host) => Switch(
        state: host,
        act: static (held, kind) => Try.lift(kind.Effect).Run().Bind(static inner => inner),
        toggle: static (held, kind) => Try.lift(() => HostEdge.Side(() =>
            kind.Write(held is CheckCommand check && check.Checked))).Run(),
        pick: static (held, kind) => Try.lift(() => HostEdge.Side(kind.Choose)).Run());

    internal Unit Refresh(Command host) => Switch(
        state: host,
        act: static (_, _) => unit,
        toggle: static (command, kind) => command is CheckCommand check ? HostEdge.Side(() => check.Checked = kind.Read()) : unit,
        pick: static (command, kind) => command is RadioCommand radio ? HostEdge.Side(() => radio.Checked = kind.Read()) : unit);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlacementSlot(PlacementKey Place, Dimension Rank, GroupKey Group, Option<SubMenuKey> SubMenu);

public sealed record IntentRow(
    IntentKey Key,
    string MenuText,
    Option<string> ToolText,
    Option<string> Hint,
    Option<IconRender> Icon,
    Option<Keys> Gesture,
    Func<bool> Available,
    CommandKind Kind,
    Seq<PlacementSlot> Slots);

[StructLayout(LayoutKind.Auto)]
public readonly record struct Invocation(
    IntentKey Key,
    Fin<Unit> Outcome,
    Option<GaugedSpan<DispatchLane>> Span) : IUiFact, IValidityEvidence {
    public bool IsValid => Outcome.IsSucc;
}

internal sealed record BoundIntent(
    IntentRow Row, Command Command, EventHandler<EventArgs> Executed, Seq<Lease<IDisposable>> Resources);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class IntentTable : IMount, IUiSource<IUiFact>, IDisposable {
    private readonly Seq<BoundIntent> bound;
    private readonly EvidenceDrain<IUiFact> drain;
    private readonly MonotonicTimeline clock;
    private readonly Atom<MountCustody> custody = Atom<MountCustody>(
        new MountCustody.Live(Active: None, Children: Seq<IMount>(), Phase: MountPhase.Open));
    private readonly Atom<Seq<Error>> teardown = Atom(Seq<Error>());

    public Seq<Error> ReleaseFaults => teardown.Value;

    string IUiSource<IUiFact>.Key => "intent.table";

    public static Fin<Lease<IntentTable>> Materialize(
        Seq<IntentRow> rows, MonotonicTimeline clock, EvidenceDrain<IUiFact> drain);

    public Fin<IDisposable> Attach(EventAnchor anchor, Action<Func<Fin<IUiFact>>> emit);

    public Fin<Command> Verb(IntentKey key);

    public Fin<Unit> Invoke(IntentKey key);
    public Fin<Unit> RefreshAvailability();

    public Fin<MenuBar> MenuOf(PlacementKey place);
    public Fin<ToolBar> BarOf(PlacementKey place);
    public Fin<Lease<ContextMenu>> PopupOf(PlacementKey place);

    public Fin<Unit> Release();
    public void Dispose() => _ = Release();

    private static Fin<Unit> Distinct(Seq<IntentRow> rows);
    private static Fin<(BoundIntent Entry, HashMap<GroupKey, RadioCommand> Heads)> Bind(
        IntentRow row, HashMap<GroupKey, RadioCommand> heads);
    private Seq<(PlacementSlot Slot, BoundIntent Entry)> Placed(PlacementKey place);
    private static Fin<Unit> Severed(Seq<BoundIntent> entries);
}
```

## [03]-[MENU]

- Owner: `MenuNode` the recursive authored vocabulary; `MenuMode` the per-entry presentation row a flat host consumes; `MenuSlot` one entry of the flat projection; `MenuBranch` one realized item with the children it owns; `OwnedContextMenu` the popup that releases its whole forest; `MenuTree` the construction, flattening, and ordinal-resolution gates.
- Cases: `Verb` resolves an intent through the table onto its host menu item, `Stub` folds children into a submenu, `Rule` is the divider. One tree value describes any authored menu, and the parallel per-surface menu builders both boundaries carried collapse onto this node algebra plus the table. `MenuMode` is `Active`, `Muted`, or `Divider` — the three presentations a flat host renders, keyed by ordinal because that is the array a flat host reads.
- Entry: `Context` acquires the full forest inside one marshal and answers a leased popup; `Flatten` projects the same tree for a host that consumes a flat roster and answers an INDEX, and `Choose` resolves that index back to the verb it named. Assignment and display stay at the control boundary.
- Law: this projection and the table's placement projection BOTH survive on a named discriminant. PLACEMENT is declarative and rank-ordered, so a plugin contributing a verb never sees the tree it lands in; NODE is authored and order-preserving, so a canvas popup states its own sequence. Collapsing to one would cap the authored tree at the placement algebra's single submenu level or force rank ordering onto a hand-built tree. Witness: Rhino's placement fold orders by group then rank; the Grasshopper node fold preserves author order.
- Law: menu items are PROJECTIONS of table rows — checked state, enablement, gesture display, and icon all ride the host command the item was created from, so a menu carries no state beside its command and a toggle flip needs no menu code at all.
- Law: a FLAT host is a projection of the same tree, never a second authoring vocabulary. A host consuming a string roster beside a mode array and answering an ordinal — the shape `Dialogs.ShowContextMenu` publishes — reads `Flatten`, so an author writes one `MenuNode` tree and every surface renders what it can. Nesting is what the flat host cannot hold, so a `Stub` emits its own text as a `Muted` header row bracketed by dividers and then its children in order: the grouping survives as presentation where it cannot survive as structure, and NAMED LOSS is the submenu's collapse-and-expand affordance alone.
- Law: availability is READ at flatten, so a verb whose `Available` probe answers false projects `Muted` rather than being dropped — a dropped row shifts every later ordinal and the host's answer would then name a different verb than the one the user pressed.
- Law: the index answer resolves through `Choose` and nowhere else. A negative ordinal is DISMISSAL and answers absence, an out-of-range ordinal and an ordinal naming a header or divider are typed refusals, and only a `Verb` slot answers an `IntentKey` — so no caller re-derives the mapping from its own copy of the roster it passed in.
- Law: the fold recurses on the runtime stack, so `MenuTree` carries private depth and item limits and returns a typed exhaustion refusal at either bound. A generated or hostile tree is the one failure no result below catches, and a success-shaped fall-through past a limit certifies a truncated menu as complete.
- Law: release is recursive and mint-reverse — every submenu detaches before its parent disposes, and the whole teardown runs once behind the `MountCustody` machine's own transition. That machine is the ONE latch on this sub-domain: an interlocked int beside it answers the same question a second time, and re-arming after a faulted release re-runs the whole teardown over natives already detached, which is the double-dispose the sibling lease arrows foreclose.
- Exemption: the recursive native detach is the one statement-shaped region on this page — a submenu's items clear, its branch detaches, and the latch writes its terminal state inside one frame, and no fold, schedule, or expression spine expresses a sequence whose every step must run after an earlier one refused. It is contained here and no consumer writes one.
- Law: the lease IS the evidence; a release fault is retained on the popup rather than raised into the host event pump.
- Packages: Eto.Forms for `ContextMenu`, `MenuItem`, `ButtonMenuItem`, `SubMenuItem`, and the separator item, all seated at `libs/dotnet/.api/api-eto-forms.md`; LanguageExt.Core for `Fin`/`Seq`/`Lease`; Thinktecture.Runtime.Extensions for the union and the budget rows.
- Growth: a new entry kind is one case with one build arm and one flatten arm; a new flat presentation is one `MenuMode` row; the three gates never widen.
- Boundary: menu lifecycle observation — opening, closing, closed — is the input owner's fact algebra over the live popup inside the lease window, never a column on a node. The flat host's own screen-point argument and its call stay at that boundary: `Flatten` answers the roster and `Choose` reads the ordinal, and neither knows where the menu was shown.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MenuNode {
    private MenuNode() { }

    public sealed record Verb(IntentKey Key) : MenuNode;
    public sealed record Stub(string Text, Seq<MenuNode> Items) : MenuNode;
    public sealed record Rule : MenuNode;
}

[SmartEnum<int>]
public sealed partial class MenuMode {
    public static readonly MenuMode Active = new(key: 0);
    public static readonly MenuMode Muted = new(key: 1);
    public static readonly MenuMode Divider = new(key: 2);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct MenuSlot(string Text, MenuMode Mode, Option<IntentKey> Verb);

internal sealed record MenuBranch(MenuItem Root, Seq<Lease<MenuItem>> Owned);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class OwnedContextMenu : ContextMenu, IMount {
    private readonly Seq<MenuBranch> branches;
    private readonly Atom<MountCustody> custody = Atom<MountCustody>(
        new MountCustody.Live(Active: None, Children: Seq<IMount>(), Phase: MountPhase.Open));
    private readonly Atom<Seq<Error>> teardown = Atom(Seq<Error>());

    public Seq<Error> ReleaseFaults => teardown.Value;

    internal OwnedContextMenu(Seq<MenuBranch> branches);

    public Fin<Unit> Release();

    protected override void Dispose(bool disposing);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MenuTree {
    private static readonly Dimension MaximumDepth = Dimension.Create(value: 16);
    private static readonly Dimension MaximumItems = Dimension.Create(value: 512);

    public static Fin<Lease<ContextMenu>> Context(Seq<MenuNode> nodes, IntentTable table);
    public static Fin<Seq<MenuSlot>> Flatten(Seq<MenuNode> nodes, IntentTable table);
    public static Fin<Option<IntentKey>> Choose(Seq<MenuSlot> slots, int index);
}
```

## [04]-[WINDOW]

- Owner: `ShellCapability` the window-capability vocabulary; `WindowRole` the modality row; `MountPhase` the lifecycle row every mount and the handler capsule read; `WindowChrome` the full posture; `WindowSpec` one modeless surface with an optional styling function; `WindowVerb` the live mutation vocabulary; `IMount` the custody floor an adoptable mount answers; `MountCustody` the re-entrancy and adoption state machine; `WindowMount` the owned realization.
- Cases: `Modeless` is the ordinary form and `Floating` the palette — the two roles both boundaries spelled, under two names. `WindowVerb` is `Front`, `Retitle`, and `Redress`, each behind one marshalled gate over the live mount. `MountCustody` is `Live` with its visit count, adopted children, and phase, or `Released`. `MountPhase` is `Open`, `Closing`, or `Released`, each carrying the `Closes` consequence every arm reads.
- Entry: `Realize` builds and owns; showing stays at the application boundary; `Steer` is the ONE live-mutation gate; `Adopt` transfers a child mount's release into its owner's.
- Auto: the six capability bits — a flags enum at one boundary and five loose bool columns at the other — ride ONE `CapabilitySet<ShellCapability>` read by set algebra. Every corner is legal — a fixed always-on-top palette with no close box and a resizable taskbar shell are both real windows — so the law is `CapabilityLaw.Open` and states it.
- Auto: opacity is a `UnitInterval`, so the unbounded double both boundaries accepted can no longer name a window ninety percent past opaque.
- Law: styling is ONE optional function whose presence is the declaration. Realization traverses `Style`, and the host function itself stays uncompared because no value equality compares it.
- Law: ownership transfers only after the complete realize settles. Menu and toolbar mint BEFORE the window exists, so a refusal anywhere after them would leave two host objects with no owner; the drain releases both on the no-window path and the mount owns them once the window has taken them.
- Law: teardown is the exact inverse of construction — content detaches, the element mount releases, the window disposes, then the chrome the window carried releases in mint-reverse order — and every step runs even when an earlier one refuses, each refusal accumulating on the mount's own ledger. Teardown faults never ride an unwinding stack, because a raise from a `finally` REPLACES the primary error.
- Law: custody is a state machine, not a flag pair. A steer ENTERS and LEAVES, a release CLOSES, and a close arriving mid-steer defers to the last leave rather than disposing a form a verb is still writing. The visit count is ABSENT rather than zero — the count carrier starts at one, and absence is exactly what a zero stood for — so a leave with no matching enter is a typed refusal instead of a negative count that then reaches a release nothing entered. Adopted children release with their owner, which is what closes the gap where a chrome owner pinned z-order and nothing released the pinned window.
- Law: this machine is the ONE release latch on the page, and every mount here composes it — the deck, the popup, the modal, and the presence each hold one atom of custody and one atom of retained faults, where four interlocked int latches sat beside it answering the same question four times and each discarded its release verdict through `ignore`. A cleanup refusal never rides a discard: `Dispose` parks the verdict on the mount's own ledger, which `ReleaseFaults` publishes.
- Law: the child forest is `IMount`, not `WindowMount`. A page mount, a panel mount, and a window mount are the same custody problem — something that was minted, can be adopted, and must be released exactly once — so the floor carries a release and nothing else, and a host mount that is not a window composes this machine rather than re-spelling it.
- Law: custody lives on the parent's child roster. Adoption after closing or release returns the child for immediate release instead of dropping it or writing an unavailable child backlink.
- Output: `WindowMount` exposes the surface, the realized plant, and the accumulated release faults; the leased mount IS the transfer of ownership.
- Packages: Eto.Forms for `Form`/`FloatingForm`/`Window`/`WindowState`/`WindowStyle`; Eto.Drawing for the point, size, and icon carriers, entering as prelude aliases alone; LanguageExt.Core for `Fin`/`Seq`/`Atom`/`Lease`; Thinktecture.Runtime.Extensions for the unions and keyed rows; `Domain/hooks` for the `FaultCell` a prompt parks its reporter faults on.
- Growth: a new modality is one `WindowRole` row; a new posture fact is one `WindowChrome` field; a new capability is one `ShellCapability` row nothing else edits; a new live verb is one `WindowVerb` case breaking `Steer` loudly; a new lifecycle phase is one `MountPhase` row carrying its `Closes` consequence; a new adoptable mount kind is one `IMount` implementation and no edit to this machine at all.
- Boundary: window lifecycle facts — closing, closed, state changed, pixel size changed — are the input owner's source rows over the realized form; per-display placement math reads the platform owner's display facts; Grasshopper2's editor panes and slots, and Rhino's dockable panel registry, stay at their boundaries and hand this owner an anchor rather than a case.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoIcon = Eto.Drawing.Icon;
using EtoPoint = Eto.Drawing.Point;
using EtoSize = Eto.Drawing.Size;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShellCapability : ICapability<ShellCapability> {
    public static readonly ShellCapability Resize = new(key: "resize", rank: 0);
    public static readonly ShellCapability Maximize = new(key: "maximize", rank: 1);
    public static readonly ShellCapability Minimize = new(key: "minimize", rank: 2);
    public static readonly ShellCapability Close = new(key: "close", rank: 3);
    public static readonly ShellCapability Topmost = new(key: "topmost", rank: 4);
    public static readonly ShellCapability Taskbar = new(key: "taskbar", rank: 5);

    public int Rank { get; }
}

[SmartEnum]
public sealed partial class WindowRole {
    public static readonly WindowRole Modeless = new(mint: static () => new Form());
    public static readonly WindowRole Floating = new(mint: static () => new FloatingForm());

    [UseDelegateFromConstructor] internal partial Form Mint();
}

[SmartEnum]
public sealed partial class MountPhase {
    public static readonly MountPhase Open = new(closes: false);
    public static readonly MountPhase Closing = new(closes: true);
    public static readonly MountPhase Released = new(closes: true);

    public bool Closes { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowVerb {
    private WindowVerb() { }

    public sealed record Front : WindowVerb;
    public sealed record Retitle(string Title) : WindowVerb;
    public sealed record Redress(WindowChrome Chrome) : WindowVerb;
}

public interface IMount {
    Fin<Unit> Release();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MountCustody {
    private MountCustody() { }

    public sealed record Live(
        Option<Dimension> Active, Seq<IMount> Children, MountPhase Phase) : MountCustody;
    public sealed record Released : MountCustody;

    public Fin<MountCustody> Entered() => Switch(
        live: static row => row.Phase.Closes
            ? Fin.Fail<MountCustody>(new UiFault.Released())
            : Fin.Succ<MountCustody>(row with {
                Active = Some(Dimension.Create(value: row.Active.Map(static held => held.Value).IfNone(0) + 1)),
            }),
        released: static _ => Fin.Fail<MountCustody>(new UiFault.Released()));

    public Fin<(MountCustody Next, Option<Seq<IMount>> Release)> Left() => Switch(
        live: static (row) => row.Active.Match(
            Some: held => held.Value is 1
                ? row.Phase.Closes
                    ? Fin.Succ(((MountCustody)new Released(), Some(row.Children)))
                    : Fin.Succ(((MountCustody)(row with { Active = None }), Option<Seq<IMount>>.None))
                : Fin.Succ(((MountCustody)(row with { Active = Some(Dimension.Create(value: held.Value - 1)) }), Option<Seq<IMount>>.None)),
            None: () => Unmatched()),
        released: static (_) => Unmatched());

    public (MountCustody Next, Option<Seq<IMount>> Release) Closed() => Switch(
        live: static row => row.Phase.Closes
            ? ((MountCustody)row, Option<Seq<IMount>>.None)
            : row.Active.IsSome
                ? ((MountCustody)(row with { Phase = MountPhase.Closing }), Option<Seq<IMount>>.None)
                : ((MountCustody)new Released(), Some(row.Children)),
        released: static row => ((MountCustody)row, Option<Seq<IMount>>.None));

    public (MountCustody Next, Option<IMount> Release) Adopted(IMount child) => Switch(
        state: child,
        live: static (held, row) => row.Phase.Closes
            ? ((MountCustody)row, Some(held))
            : ((MountCustody)(row with { Children = row.Children.Add(held) }), Option<IMount>.None),
        released: static (held, row) => ((MountCustody)row, Some(held)));

    private static Fin<(MountCustody Next, Option<Seq<IMount>> Release)> Unmatched() =>
        Fin.Fail<(MountCustody, Option<Seq<IMount>>)>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(Live.Active)),
            Reason: RejectReason.UnmatchedLeave));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WindowChrome(
    CapabilitySet<ShellCapability> Capabilities,
    Option<EtoPoint> Origin,
    Option<EtoSize> Extent,
    Option<UnitInterval> Opacity,
    WindowState State,
    WindowStyle Style,
    Option<Lease<EtoIcon>> Badge,
    Option<Window> Owner) {
    public static WindowChrome Portable => Seed.Value;

    public static CapabilityLaw<ShellCapability> Law => CapabilityLaw<ShellCapability>.Open;

    private static readonly Lazy<WindowChrome> Seed = new(static () => new(
        Capabilities: CapabilitySet<ShellCapability>.Of(
            ShellCapability.Resize, ShellCapability.Maximize, ShellCapability.Minimize,
            ShellCapability.Close, ShellCapability.Taskbar),
        Origin: None, Extent: None, Opacity: None,
        State: WindowState.Normal, Style: WindowStyle.Default, Badge: None, Owner: None));
}

public sealed record WindowSpec(
    string Title,
    ControlSpec Content,
    WindowRole Role,
    WindowChrome Chrome,
    Option<Func<Control, Fin<Unit>>> Style,
    Option<PlacementKey> Menu,
    Option<PlacementKey> Bar,
    bool ShowActivated) {
    public Fin<Lease<WindowMount>> Realize(ElementRuntime runtime);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WindowMount : IMount, IDisposable {
    private readonly Lease<Form> surface;
    private readonly Lease<Control> content;
    private readonly ElementMount plant;
    private readonly Seq<Lease<IDisposable>> chrome;
    private readonly Atom<MountCustody> custody = Atom<MountCustody>(
        new MountCustody.Live(Active: None, Children: Seq<IMount>(), Phase: MountPhase.Open));
    private readonly Atom<Seq<Error>> teardown = Atom(Seq<Error>());

    public Form Surface => surface.Resource;
    public ElementMount Plant => plant;
    public Seq<Error> ReleaseFaults => teardown.Value;

    public Fin<Unit> Steer(WindowVerb verb);

    public Fin<Unit> Adopt(IMount child);

    public Fin<Unit> Release();
    public void Dispose() => _ = Release();
}
```

## [05]-[PROMPT]

- Owner: `PromptChoice<TResult>` one affirmative outcome; `Prompt<TResult>` the result-typed modal with its refusal policy; `FilterPlan` one file-type filter; `PickerSpec` the native prompt family, `PickerResult` its mirror, and `PickerDemand<TResult>` the typed half that pairs them; `AskVerdict` the message verdict roster; `AskTrait`, `AskDelivery`, and `AskModality` the message vocabularies with `AskPolicy` the knob record carrying them.
- Cases: eight prompts — open, save, folder, shade, glyph, ask, edit, and number — against eight results, so every native prompt settles typed through ONE gate and a per-prompt member family never exists. `AskTrait` carries foreground, topmost, right-aligned, and right-to-left presentation; `AskDelivery` carries the application, desktop, and service targets; `AskModality` carries the application, system, and task scopes.
- Entry: one `Ask` returns `IO<Option<TResult>>` through the injected presenter, `Present` runs on the picker union, and `Typed.Present` on each picker case returns the typed answer. Each is one marshal: construction, styling, the modal loop, result capture, and reverse-order release share the window, and no dialog handle escapes it.
- Auto: a choice projects from the harvested field map, so every choice reads admitted values through `HashMap.Find` and no choice needs a captured control.
- Auto: the two host number-prompt overloads are ONE case whose `NumberPolicy` presence selects them: a policy naming both a floor and a ceiling takes the bounded call, any other takes the unbounded one. The knob set the sibling control owner already declares is the knob set here.
- Law: `KeepOpenOnRefusal` is the prompt's refusal policy. Rhino closes on an admission refusal and Grasshopper renders it and keeps the dialog open; both configure the same modal type.
- Law: dismissal is ordinary absence inside `IO<Option<TResult>>`; admission and host refusal ride the effect's failure channel, and cancellation belongs to the effect.
- Law: presentation IS the presenter value. A host boundary hands its own semi-modal presenter, so a host-parented dialog cannot route around that contract; an owner-taking entry that shows the dialog itself is the deleted form.
- Law: the SPEC case fixes the answer shape, so the pairing is declared once at each case as its `Typed` demand and a consumer never re-probes the result union. `Present` answers the closed `PickerResult` because that family is what a host dialog settles into and what a journal stores; `Typed.Present` answers `Fin<Option<TResult>>` over the same crossing, dismissal riding the absence the page already rules and a host answering the WRONG shape riding a typed refusal naming both cases. A consumer switching over eight results to recover the one it asked for is the defect the demand deletes; folding the two into one generic entry is the unsound form, because nothing but the case ties the request to its answer.
- Law: the picker's shade and glyph cases carry `PerceptualColor` and `TypeFace`, its three file cases carry `FileLocation` on both legs, and its message case carries an `AskVerdict` row — so a captured colour, face, path, or verdict is the kernel's own identity and no consumer of this family converts a host struct. One path type crosses in and out of all three file cases, where a `Uri` on two and a raw string on the third made one concept two types inside one union.
- Law: the message prompt's knobs ride a POLICY, not four more positional columns. Buttons, icon, default button, presentation traits, delivery target, and modality are one knob set both hosts publish, and the set carries the ONE admission a message prompt has: a default naming a button the roster does not present is unpressable, so the host silently defaults to its own first button and the declared default is lost. Every trait corner is legal — a topmost right-to-left foreground box is a real dialog — so the capability law is open and says so.
- Law: delivery is a REQUEST the host answers, and only the application target is platform-universal. A desktop or service target on a host publishing no session routing refuses TYPED by name before the show, the same posture the presence owner holds for a tray anchor — a silent fall back to the in-application box delivers the message somewhere the caller did not ask for. NAMED LOSS: a Rhino-only button roster — abort/retry/ignore and retry/cancel — has no member on the toolkit surface this case presents through and stays at that boundary as its own inquiry.
- Law: the edit and number cases route the host's native single-value fast lane, which the branch catalogue names as the one form — a hand-built one-field dialog beside them is the deleted shape, and it is exactly what a kernel-only spelling would re-mint.
- Output: `IO<Option<TResult>>` for the modal, `PickerResult` for the picker, and `Fin<Option<TResult>>` for its typed demand; none exposes a live host dialog.
- Packages: Eto.Forms for `Dialog<T>`, the four common dialogs, and the message-box vocabulary — `MessageBoxDefaultButton` registering at `libs/dotnet/.api/api-eto-forms.md` beside the window vocabulary it joins; Eto.Drawing for the size carrier, prelude-aliased; RhinoCommon `Rhino.UI` for the two native value prompts; LanguageExt.Core for `Fin`/`Option`/`Seq`/`Lease`; Thinktecture.Runtime.Extensions for the unions, the posture row, and the message vocabularies; `Domain/validation` for `ICapability` and `CapabilitySet`.
- Growth: a new affirmative outcome is one `PromptChoice` row; a new native prompt is one `PickerSpec` case with one `Present` arm, its `PickerResult` mirror, and the `Typed` demand that pairs them; a new message presentation is one `AskTrait` row, a new delivery target one `AskDelivery` row, and a new host verdict one `AskVerdict` row; neither gate widens.
- Boundary: Rhino's multi-value, document-scoped, and resource-scoped dialogs — layer, linetype, print-width, sun, and the property and check rosters — stay at that boundary as its own instances, because each is a `Rhino.UI` document surface with no host-neutral analogue.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoSize = Eto.Drawing.Size;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AskTrait : ICapability<AskTrait> {
    public static readonly AskTrait Foreground = new(key: "foreground", rank: 0);
    public static readonly AskTrait Topmost = new(key: "topmost", rank: 1);
    public static readonly AskTrait RightAligned = new(key: "right-aligned", rank: 2);
    public static readonly AskTrait RightToLeft = new(key: "right-to-left", rank: 3);

    public int Rank { get; }
}

[SmartEnum]
public sealed partial class AskDelivery {
    public static readonly AskDelivery Application = new();
    public static readonly AskDelivery Desktop = new();
    public static readonly AskDelivery Service = new();
}

[SmartEnum]
public sealed partial class AskModality {
    public static readonly AskModality Application = new();
    public static readonly AskModality System = new();
    public static readonly AskModality Task = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AskVerdict {
    public static readonly AskVerdict Ok = new(key: "ok", host: DialogResult.Ok);
    public static readonly AskVerdict Cancel = new(key: "cancel", host: DialogResult.Cancel);
    public static readonly AskVerdict Yes = new(key: "yes", host: DialogResult.Yes);
    public static readonly AskVerdict No = new(key: "no", host: DialogResult.No);

    internal DialogResult Host { get; }

    public static Fin<AskVerdict> OfHost(DialogResult host);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickerSpec {
    private PickerSpec() { }

    public sealed record Open(string Title, Option<FileLocation> Home, bool Multi, Seq<FilterPlan> Filters) : PickerSpec {
        public PickerDemand<Seq<FileLocation>> Typed => Demand<Seq<FileLocation>>(
            static answer => answer is PickerResult.Paths picked ? Some(picked.Values) : None);
    }

    public sealed record Save(string Title, Option<FileLocation> Home, Option<string> Seed, Seq<FilterPlan> Filters) : PickerSpec {
        public PickerDemand<FileLocation> Typed => Demand<FileLocation>(
            static answer => answer is PickerResult.Path picked ? Some(picked.Value) : None);
    }

    public sealed record Folder(string Title, Option<FileLocation> Home) : PickerSpec {
        public PickerDemand<FileLocation> Typed => Demand<FileLocation>(
            static answer => answer is PickerResult.Path picked ? Some(picked.Value) : None);
    }

    public sealed record Shade(
        PerceptualColor Seed, bool AllowAlpha,
        Option<Seq<PerceptualColor>> Palette = default, Option<Func<PerceptualColor, Fin<Unit>>> Preview = default) : PickerSpec {
        public PickerDemand<PerceptualColor> Typed => Demand<PerceptualColor>(
            static answer => answer is PickerResult.Shade picked ? Some(picked.Value) : None);
    }

    public sealed record Glyph(Option<TypeFace> Seed) : PickerSpec {
        public PickerDemand<TypeFace> Typed => Demand<TypeFace>(
            static answer => answer is PickerResult.Glyph picked ? Some(picked.Value) : None);
    }

    public sealed record Ask(string Text, string Caption, AskPolicy Policy) : PickerSpec {
        public PickerDemand<AskVerdict> Typed => Demand<AskVerdict>(
            static answer => answer is PickerResult.Verdict picked ? Some(picked.Value) : None);
    }

    public sealed record Edit(string Title, string Message, string Seed, bool Multiline) : PickerSpec {
        public PickerDemand<string> Typed => Demand<string>(
            static answer => answer is PickerResult.Text picked ? Some(picked.Value) : None);
    }

    public sealed record Number(string Title, string Message, double Seed, NumberPolicy Policy) : PickerSpec {
        public PickerDemand<double> Typed => Demand<double>(
            static answer => answer is PickerResult.Number picked ? Some(picked.Value) : None);
    }

    public Fin<PickerResult> Present(Option<Control> anchor);

    private PickerDemand<TResult> Demand<TResult>(Func<PickerResult, Option<TResult>> shape) =>
        new(Spec: this, Shape: shape);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickerResult {
    private PickerResult() { }

    public sealed record Paths(Seq<FileLocation> Values) : PickerResult;
    public sealed record Path(FileLocation Value) : PickerResult;
    public sealed record Shade(PerceptualColor Value, Seq<Error> PreviewFaults = default) : PickerResult;
    public sealed record Glyph(TypeFace Value) : PickerResult;
    public sealed record Verdict(AskVerdict Value) : PickerResult;
    public sealed record Text(string Value) : PickerResult;
    public sealed record Number(double Value) : PickerResult;
    public sealed record Dismissed : PickerResult;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PickerDemand<TResult>(PickerSpec Spec, Func<PickerResult, Option<TResult>> Shape) {
    public Fin<Option<TResult>> Present(Option<Control> anchor);
}

public sealed record FilterPlan(string Label, Seq<string> Extensions) {
    internal Fin<FileFilter> Resolve();
}

public sealed record AskPolicy(
    MessageBoxButtons Buttons,
    MessageBoxType Kind,
    MessageBoxDefaultButton Default,
    CapabilitySet<AskTrait> Traits,
    AskDelivery Delivery,
    AskModality Modality) {
    public static AskPolicy Plain => Seed.Value;

    public static CapabilityLaw<AskTrait> Law => CapabilityLaw<AskTrait>.Open;

    internal Fin<Unit> Admit();

    private static readonly Lazy<AskPolicy> Seed = new(static () => new(
        Buttons: MessageBoxButtons.OK,
        Kind: MessageBoxType.Information,
        Default: MessageBoxDefaultButton.Default,
        Traits: CapabilitySet<AskTrait>.Of(),
        Delivery: AskDelivery.Application,
        Modality: AskModality.Application));
}

public sealed record PromptChoice<TResult>(string Caption, Func<HashMap<FieldTag, FieldValue>, Fin<TResult>> Project);

public sealed record Prompt<TResult>(
    string Title,
    ControlSpec Content,
    Seq<PromptChoice<TResult>> Choices,
    Option<string> CancelCaption,
    Option<EtoSize> ClientSize,
    DialogDisplayMode DisplayMode,
    Option<Func<Control, Fin<Unit>>> Style,
    bool KeepOpenOnRefusal,
    FaultCell Faults) {
    public IO<Option<TResult>> Ask(
        ElementRuntime runtime,
        Func<Dialog<Option<TResult>>, IO<Option<TResult>>> present);

    private Fin<Unit> Admit();
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class PromptMount<TResult> : IMount, IDisposable {
    private readonly ElementMount body;
    private readonly Atom<MountCustody> custody = Atom<MountCustody>(
        new MountCustody.Live(Active: None, Children: Seq<IMount>(), Phase: MountPhase.Open));
    private readonly Atom<Seq<Error>> teardown = Atom(Seq<Error>());

    public Seq<Error> ReleaseFaults => teardown.Value;

    internal Dialog<Option<TResult>> Dialog { get; }

    public Fin<Unit> Release();
    public void Dispose() => _ = Release();
}
```

## [06]-[PRINT]

- Owner: `PrintRoute` the presentation modality; `PageFrame` the page-geometry source (its `Sheet` arm composes `Drawing/sheet`'s `SheetSize`, `SheetMargin`, and `SheetOrientation`); `PageSpan` the admitted page interval; `PrintScope` the job extent; `CollatePosture` and `PageOrder` the two tri-valued job classifiers; `PrintSpec` the job configuration; `PrintPage` one replayable page; `PrintPageFact` per-page evidence; `PrintOutcome` the run outcome; `PrintPlan` the deferred job.
- Cases: `PrintRoute` is silent, chooser, or preview. `PageFrame` is the host page size, the printer's own printable area, a caller rectangle, or a `SheetSize` inset by margins — the fourth case is what makes a page geometry declarable before a printer is known. `PrintScope` carries the host's own three selection rows: the whole job, an admitted page span, or the host's current selection.
- Entry: `Run` answers `IO<PrintOutcome>`; printer interaction and document lifetime begin only when the caller executes the effect, and that execution crosses the marshal ONCE so job construction, every page callback, both dialog routes, and disposal share one UI-affine scope. ONE carrier: `IO` already carries failure, and a `Fin` inside it makes a caller run the effect and then match a second result for one outcome.
- Auto: the sheet arm performs the ONE admitted scale hop from the sheet roster's millimetre regime into the printer-point regime, so no constant restating a points-per-inch factor or a per-standard extent exists above the sheet owner. The host surface publishes no margin at all — the printable area is a printer read — which is why the inset is the kernel's own value rather than a host property.
- Auto: copies stay `Option`-shaped while collation and page order are closed ROWS carrying their host default as a case — a `bool?` strands that default in `case null` at every reader. Each row's own apply arm writes nothing on the host row, so an omitted choice inherits the host's `[DefaultValue]` and a restated default is never forged.
- Law: nothing inside the run's scope re-marshals. Page and completion callbacks fire from the run the driver is already blocking on, so presence lands through the affinity assertion and a refusal reaches the outcome's fault roster; a blocking crossing from inside that callback waits on the thread that raised it.
- Law: every attempted page normalizes the host's selected-range page number to a zero-based source and scope ordinal, so a page fact's index means the same thing under every scope. A page outside the scope is a typed refusal carrying its own fact rather than a silently dropped render.
- Law: completion is DERIVED and never stored — one in-range fact per expected page, distinct indices, host completion, and no failed fact. A page's render failure is stored because the host page event cannot return it, and the run never converts a stored failure into successful completion.
- Law: the copy count is a `Dimension`, so the non-positive copy count both boundaries had to refuse at admission is unrepresentable. `PageSpan` admits the ordered interval once, and `PrintScope.Admit` compares its last page with the live job ceiling; the toolkit's own `Range<int>` is named at the single `PrintSettings` write, under the one prelude alias that disambiguates it from the carrier library's.
- Output: `PrintOutcome` is this driver's RAW run outcome alone — printer-evidence vocabulary belongs to the publishing surface that folds these facts into its own.
- Packages: Eto.Forms printing for `PrintDocument`, both dialogs, `PageSettings`, `PrintSettings`, and the orientation and selection vocabularies, registered at `libs/dotnet/.api/api-eto-printing.md`; Eto.Drawing for the rectangle carrier, prelude-aliased; `Drawing/sheet` for the extent roster, its orientation row, and its margin quad, and `Domain/context` for the unit projection; LanguageExt.Core for `IO`/`Fin`/`Seq`/`Atom`/`Set`; Thinktecture.Runtime.Extensions for the unions, the two job classifiers, and the page-span value object.
- Growth: a route is one `PrintRoute` case, a geometry source one `PageFrame` case, a job option one `PrintSpec` field, a job posture one row on its own classifier, and a page fact one `PrintPageFact` case.
- Boundary: the page paints through the same paint program the on-screen surface mounts, so a printed page and a drawn frame are one program replayed under two scene policies rather than two render paths.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoRange = Eto.Forms.Range<int>;
using EtoRectangleF = Eto.Drawing.RectangleF;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PrintRoute {
    private PrintRoute() { }

    public sealed record Silent : PrintRoute;
    public sealed record Chooser(Control Parent) : PrintRoute;
    public sealed record Preview(Window Parent) : PrintRoute;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageFrame {
    private PageFrame() { }

    public sealed record Host : PageFrame;
    public sealed record Printer(PageSettings Settings) : PageFrame;
    public sealed record Bounded(EtoRectangleF Bounds) : PageFrame;

    public sealed record Sheet(SheetSize Size, SheetMargin Margin, SheetOrientation Orientation) : PageFrame {
        internal Fin<EtoRectangleF> Inset() =>
            from points in ModelUnit.Of(value: UnitSystem.PrinterPoints)
            let laid = Orientation.Extent(size: Size)
            from surface in SheetSize.Of(
                width: laid.Width, height: laid.Height, standard: Size.Standard)
            from extent in surface.In(unit: points)
            from margin in Margin.In(unit: points)
            from admitted in margin.Left + margin.Right < extent.Width && margin.Top + margin.Bottom < extent.Height
                ? Fin.Succ(new EtoRectangleF(
                    x: (float)margin.Left,
                    y: (float)margin.Top,
                    width: (float)(extent.Width - margin.Left - margin.Right),
                    height: (float)(extent.Height - margin.Top - margin.Bottom)))
                : Fin.Fail<EtoRectangleF>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(SheetMargin)), Reason: RejectReason.SheetInset))
            select admitted;
    }

    internal Fin<EtoRectangleF> Resolve(PrintPageEventArgs args) => Switch(
        state: args,
        host: static (held, _) => Fin.Succ(new EtoRectangleF(
            x: 0f, y: 0f, width: held.PageSize.Width, height: held.PageSize.Height)),
        printer: static (_, frame) => Fin.Succ(frame.Settings.PrintableArea),
        bounded: static (_, frame) => Fin.Succ(frame.Bounds),
        sheet: static (_, frame) => frame.Inset());
}

[ComplexValueObject]
public sealed partial class PageSpan {
    public Dimension First { get; }
    public Dimension Last { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Dimension first, ref Dimension last) =>
        validationError = last.Value >= first.Value
            ? null
            : new ValidationError(message: "PageSpan requires a last page at or after its first.");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PrintScope {
    private PrintScope() { }

    public sealed record All : PrintScope;
    public sealed record Selected(PageSpan Span) : PrintScope;
    public sealed record HostSelection : PrintScope;

    internal Unit Apply(PrintSettings settings) => Switch(
        state: settings,
        all: static (host, _) => HostEdge.Side(() => host.PrintSelection = PrintSelection.AllPages),
        selected: static (host, scope) => HostEdge.Side(() => {
            host.PrintSelection = PrintSelection.SelectedPages;
            host.SelectedPageRange = new EtoRange(start: scope.Span.First.Value, end: scope.Span.Last.Value);
        }),
        hostSelection: static (host, _) => HostEdge.Side(() => host.PrintSelection = PrintSelection.Selection));

    internal Fin<Unit> Admit(Dimension pageCount) => Switch(
        state: pageCount,
        all: static (_, _) => Fin.Succ(unit),
        selected: static (held, scope) => scope.Span.Last.Value <= held.Value
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new UiFault.Rejected(
                Field: FieldTag.Create(value: nameof(PageSpan)), Reason: RejectReason.PageSpan)),
        hostSelection: static (_, _) => Fin.Succ(unit));

    internal static Dimension Expected(PrintSettings settings, Dimension pageCount);

    internal static Fin<PrintPageSeat> Seat(PrintSettings settings, int currentPage, Dimension pageCount);
}

[Union(
    ConversionFromValue = ConversionOperatorsGeneration.None,
    MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record PrintPageFact : IValidityEvidence {
    private PrintPageFact() { }

    public sealed record Rendered(int PageIndex, EtoRectangleF Frame) : PrintPageFact;
    public sealed record Failed(int PageIndex, Error Failure) : PrintPageFact;

    public int Index => Switch(
        rendered: static fact => fact.PageIndex,
        failed: static fact => fact.PageIndex);

    public bool IsValid => Switch(
        rendered: static _ => true,
        failed: static _ => false);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class CollatePosture {
    public static readonly CollatePosture Host = new(apply: static _ => unit);
    public static readonly CollatePosture Collated = new(apply: static settings => HostEdge.Side(() => settings.Collate = true));
    public static readonly CollatePosture Uncollated = new(apply: static settings => HostEdge.Side(() => settings.Collate = false));

    [UseDelegateFromConstructor] internal partial Unit Apply(PrintSettings settings);
}

[SmartEnum]
public sealed partial class PageOrder {
    public static readonly PageOrder Host = new(apply: static _ => unit);
    public static readonly PageOrder Forward = new(apply: static settings => HostEdge.Side(() => settings.Reverse = false));
    public static readonly PageOrder Reverse = new(apply: static settings => HostEdge.Side(() => settings.Reverse = true));

    [UseDelegateFromConstructor] internal partial Unit Apply(PrintSettings settings);
}

public sealed record PrintSpec(
    Option<Dimension> Copies,
    CollatePosture Collate,
    PageOrder Order,
    SheetOrientation Orientation,
    PrintScope Scope) {
    internal Fin<PrintSpec> Admit(Dimension pageCount) => Scope.Admit(pageCount: pageCount).Map(_ => this);

    internal PrintSettings Configure(Dimension pageCount);
}

public readonly record struct PrintPageSeat(int Ordinal, int Source, int Expected);

public sealed record PrintPage(PaintProgram Program, PageFrame Frame, ScenePolicy Policy) {
    internal Fin<EtoRectangleF> Render(PrintPageEventArgs args);
}

public sealed record PrintOutcome(
    JobName Name,
    Seq<PrintPageFact> Pages,
    Dimension Expected,
    bool HostCompleted,
    Seq<Error> Faults) : IValidityEvidence {
    public bool Completed =>
        HostCompleted
        && Pages.Count == Expected.Value
        && Pages.ForAll(fact => fact.IsValid && fact.Index >= 0 && fact.Index < Expected.Value)
        && Pages.Map(static fact => fact.Index).Distinct().Count == Pages.Count;

    public bool IsValid => ValidityClaim.All(Completed, Faults.IsEmpty);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed record PrintPlan(JobName Name, Seq<PrintPage> Pages, PrintSpec Spec, PrintRoute Route) {
    public IO<PrintOutcome> Run();

    private Fin<Unit> Admit();
    private Fin<Unit> Present(PrintDocument document);
}
```

## [07]-[PRESENCE]

- Owner: `ToastKey` the notification identity; `Toast` the transient alert; `PulseState` the progress projection; `PresenceOp` the closed presence family and acquisition gate; `PresenceHold` what a mount actually holds per case; `PresenceMount` the applied presence.
- Cases: `Alert` delivers a system toast with its activation route, `Tray` holds persistent tray presence with an icon and an optional menu, `Pulse` projects job progress onto the OS taskbar or dock, and `Badge` sets the application badge label.
- Entry: `PresenceOp.Apply` answers a leased mount; `Steer` is the live re-point on a standing mount — same-case only, the hold's `Prior` never re-captured — so a stepping consumer never releases and re-applies per frame. It crosses as `UiDispatch<T>.Blocking`, which runs in-frame when the caller already holds the marshal and invokes otherwise, so a caller inside a UI-affine callback reaches the same member and never waits on the thread it is holding.
- Auto: release RESTORES rather than merely detaching — a pulse mount lands the idle state, a badge mount clears the label, a tray mount hides and detaches before disposing, and an alert mount detaches its activation route. Both boundaries applied progress and badge state as fire-and-forget writes, so a job that ended without clearing left the taskbar pinned at its last fraction forever.
- Auto: the progress fraction is a `UnitInterval` and the row projects it, so the host member that throws outside its bound is guarded by the type rather than by a caught argument exception.
- Law: an alert whose host demands a tray anchor and receives none refuses TYPED before the show, naming the clause it refused under. The precondition is an INSTANCE read off the minted notification's own handler — a per-platform fact that does not exist until the card is minted — so the gate runs between the mint and the show and the refusal releases the card like any other failed acquisition. A bool on the op could only ever answer for the tray case and answered false for every alert that actually needed an anchor.
- Law: the tray case's precondition is the handler CONTRACT, demanded through the one capability gate before any host object mints: a shell publishing no tray indicator refuses by name rather than raising mid-mount.
- Law: a failed acquisition detaches and disposes each newly minted host widget WITHOUT consuming an input image, tray, or menu lease — a caller's lease survives a refusal, because the gate never owned it.
- Law: a dependent widget cannot release before its host detaches, so an alert holding a tray anchor borrows it and the tray mount alone owns it.
- Output: `PresenceMount` carries the applied operation, the ONE hold its case minted, and the release faults it retained; activation facts stay inside the mount's own window and never cross as a return value. Four independent lease slots left twelve of sixteen corners representable and unreachable, and the release had to re-derive which case it was holding.
- Packages: Eto.Forms runtime for `Notification`, `TrayIndicator`, and the application badge (`libs/dotnet/.api/api-eto-runtime.md`); Eto.Forms printing for the taskbar projection, registered at `libs/dotnet/.api/api-eto-printing.md`; Eto.Drawing for the image carrier, prelude-aliased; LanguageExt.Core for `Fin`/`Option`/`Seq`/`Atom`/`Lease`; Thinktecture.Runtime.Extensions for the unions and the key; `Domain/hooks` for the `FaultCell` a restore refusal parks on.
- Growth: a new presence surface is one `PresenceOp` case with one apply arm, its `PresenceHold` case carrying exactly what that apply minted, one restore arm, and its precondition verdict; a new progress mode is one `PulseState` case carrying exactly the evidence the host projection consumes.
- Boundary: this owner reaches OS notification-center, tray, taskbar, and badge presence ALONE — a Rhino in-viewport toast, a Rhino status-bar meter, and a Grasshopper2 canvas notice are host surfaces over their own chrome, and the two never alias.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoImage = Eto.Drawing.Image;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ToastKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "ToastKey requires a non-blank identity.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PulseState {
    private PulseState() { }

    public sealed record Idle : PulseState;
    public sealed record Working(UnitInterval Progress) : PulseState;
    public sealed record Waiting : PulseState;
    public sealed record Paused(UnitInterval Progress) : PulseState;
    public sealed record Failed(UnitInterval Progress) : PulseState;

    internal (TaskbarProgressState State, float Progress) Project() => Switch(
        idle: static _ => (TaskbarProgressState.None, 0f),
        working: static state => (TaskbarProgressState.Progress, (float)state.Progress.Value),
        waiting: static _ => (TaskbarProgressState.Indeterminate, 0f),
        paused: static state => (TaskbarProgressState.Paused, (float)state.Progress.Value),
        failed: static state => (TaskbarProgressState.Error, (float)state.Progress.Value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresenceOp {
    private PresenceOp() { }

    public sealed record Alert(Toast Card, Action<ToastKey> Activated, Option<PresenceMount> Anchor) : PresenceOp;
    public sealed record Tray(string Title, Lease<EtoImage> Icon, Option<Lease<ContextMenu>> Menu, Action Activated) : PresenceOp;
    public sealed record Pulse(PulseState State) : PresenceOp;
    public sealed record Badge(Option<string> Label) : PresenceOp;

    public Fin<Lease<PresenceMount>> Apply(FaultCell faults);

    internal Fin<Unit> Precondition() => Switch(
        alert: static _ => Fin.Succ(unit),
        tray: static (_) => HostPlatform.Demand(
            requirement: new PlatformRequirement.Handler(Contract: typeof(TrayIndicator.IHandler))),
        pulse: static _ => Fin.Succ(unit),
        badge: static _ => Fin.Succ(unit));

    internal static Fin<Unit> Anchored(Alert alert, Notification card) =>
        !card.RequiresTrayIndicator || alert.Anchor.Bind(static mount => mount.Indicator).IsSome
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(Alert)), Reason: RejectReason.TrayAnchor));
}

[Union(
    ConversionFromValue = ConversionOperatorsGeneration.None,
    MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record PresenceHold {
    private PresenceHold() { }

    internal sealed record AlertHold(Lease<Notification> Card, Option<Lease<EtoImage>> Content) : PresenceHold;
    internal sealed record TrayHold(
        Lease<TrayIndicator> Indicator, Option<Lease<EtoImage>> Icon, Option<Lease<ContextMenu>> Menu) : PresenceHold;
    internal sealed record PulseHold(PulseState Prior) : PresenceHold;
    internal sealed record BadgeHold(Option<string> Prior) : PresenceHold;

    internal Fin<Unit> Restore();
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Toast(ToastKey Key, string Title, string Message, Option<Lease<EtoImage>> Content);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PresenceMount : IMount, IDisposable {
    private readonly PresenceHold hold;
    private readonly Atom<MountCustody> custody = Atom<MountCustody>(
        new MountCustody.Live(Active: None, Children: Seq<IMount>(), Phase: MountPhase.Open));
    private readonly Atom<Seq<Error>> teardown = Atom(Seq<Error>());

    public PresenceOp Applied { get; }
    public Seq<Error> ReleaseFaults => teardown.Value;

    public Fin<Unit> Steer(PresenceOp operation);

    internal Option<TrayIndicator> Indicator => hold.Map(
        @default: static _ => Option<TrayIndicator>.None,
        trayHold: static tray => Some(tray.Indicator.Resource));

    public Fin<Unit> Release();
    public void Dispose() => _ = Release();
}

```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
