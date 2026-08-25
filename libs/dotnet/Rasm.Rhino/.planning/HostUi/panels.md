# [RASM_RHINO_HOSTUI_PANELS]

`PanelHost` owns Rhino panel registration, placement, per-document instances, visibility, icon, lifecycle evidence, and dock-bar facts through one typed request family. `Rui` owns toolbar-file mutation and census through one command fold, `PanelSections` realizes collapsible host sections from capability sets, and `HostControl` closes the consumable `Rhino.UI.Controls` widget library as instances of the kernel control family. Every control tree is grown by `ControlForge` and owned by an `ElementReceipt`, every icon origin is the kernel asset family, every colour crosses as `PerceptualColor`, and every entry enters the Rhino command thread through `HostThread.Run` and answers a detached receipt.

## [01]-[INDEX]

- [02]-[PANEL_MODEL]: `PanelKey`, `PanelChange`, `PanelSeat`, `PanelFact`, `PanelAudience`, `MountState`, and `HostPanel` close identity, lifecycle, content, one-shot release, and scoped callback delivery.
- [03]-[PANEL_HOST]: `PanelIntent<TPanel>`, `PanelMount<TPanel>`, and `PanelBadge` own registration, placement, query, close, instance, icon, and dock-bar modalities over one registry state.
- [04]-[PANEL_OBSERVATION]: `PanelObserve` folds audience-scoped owned observation and the host-wide projection into one subscription entry mounted through the typed hook binding.
- [05]-[RUI]: `RuiCommand` folds toolbar-file state changes and `RuiReceipt` carries the census snapshot with any applied-prefix fault.
- [06]-[MENU_LINKS]: `MenuDelta` carries menu update state as cases over one registered host callback.
- [07]-[SECTIONS]: `PanelSectionSpec`, `PanelSectionSignal`, the two capability rosters, and `PanelSectionMount` realize ordered collapsible sections with lifecycle routing and complete content lifetime.
- [08]-[HOST_CONTROLS]: `HostControl`, the three `RhinoLayout` vocabularies, `ThemePalette`, and `UiServices` close the Rhino widget library, theme read, and platform-service seams.

## [02]-[PANEL_MODEL]

- Owner: `PanelKey` admits the panel type's declared identity; `PanelChange` closes the lifecycle evidence; `PanelSeat` is the instance identity; `PanelAudience` scopes fact delivery; `PanelFact` is the stamped evidence row; `MountState` is the one-shot release vocabulary both mounting owners on this page step; `HostPanel` is the abstract implement seam over the foreign panel bases.
- Cases: `PanelChange` is shown, hidden, unclassified, panel-closing, or document-closing — five states with no boolean payload. `MountState` is live or released, and the transition between them is the latch.
- Entry: `HostPanel` realizes its control tree once in its constructor, retains the leased receipt, and routes every host callback through `PanelHost.Stamp`.
- Auto: identity is read from the DECLARED attribute and never the runtime type identity, because the runtime synthesizes a fallback for an unattributed type — so an empty-identity gate cannot tell a declared panel key from a build-derived one and the attribute read is the whole admission.
- Law: the instance identity is the triple, not the panel key. A per-document panel holds one live instance per open document and two plug-ins seat their own panels inside one process, so every fact, ledger row, and receipt keys on plug-in, panel, and optional document with a system panel seated under the absent document.
- Law: the host-wide projection names NO plug-in, because the host reports visibility for panels this boundary never registered; an unowned fact seats nowhere and reaches a registry-scoped observer alone.
- Law: the one-shot release is a guarded TRANSITION over a closed state, not an interlocked exchange. A raw latch beside this folder's own cell custody is a second mechanism answering the question the transition already answers, and its verdict is what tells a second releaser it never won. NAMED LOSS: none — the exchange and the state answer the same fact, and only the state can be read.
- Law: an identity refusal, a lifecycle-hook throw, and a lifecycle-hook failure all PARK on the panel's bounded ring — durable typed evidence that never re-enters the host callback and never grows without a ceiling.
- Law: `Admits` is a PREDICATE and answers a bool. It asks whether an audience covers an owner, which every sibling `Admit` on this page does not: those admit a value and can refuse, this one filters a fan and has nothing to recover from. A rail here would be a failure no caller could act on.
- Receipt: `PanelFact` carries owning plug-in, panel, optional document, change, and the monotonic ordinal the registry stamped, and projects its own seat.
- Boundary: `Construction` retains the leased receipt so realization failure and control-tree lifetime stay typed even where the host requires a constructed panel instance.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`Panel`, `IPanel`, `ShowPanelReason`, `Panels.IsShowing`/`IsHiding`, `EtoExtensions.UseRhinoStyle`); `libs/dotnet/Rasm.Rhino/.api/api-eto-forms.md` (`Control`, `Label`); LanguageExt.Core (`Fin`, `Option`, `Atom`, `Seq`); Thinktecture.Runtime.Extensions (`[Union]`, `[ValueObject]`); `Rasm/Interaction` (`ControlSpec`, `ControlForge.Realize`, `ElementReceipt`, `ElementRuntime`, `UiFault`); `Rasm/Domain` (`Op`, `Cell`, `Transition`, `Ring<Error>`, `Lease<T>`); `Rasm/Numerics` (`Dimension`); `Rasm.Rhino/Document` (`DocKey`, `PluginKey`).
- Growth: a new lifecycle evidence is one `PanelChange` case; a new identity axis is one column on the seat, breaking every ledger read loudly.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.InteropServices;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino.UI.Controls;
using Rhino.UI.Runtime;
using Rhino.UI.Theme;
using DrawingIcon = System.Drawing.Icon;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct PanelKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Panel identity is empty.") : null;

    public static Fin<PanelKey> Of(Type panelType, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(panelType).Bind(declared => op
            .Catch(() => Optional(declared.GetCustomAttribute<GuidAttribute>())
                .ToFin(Fail: op.InvalidResult(detail: declared.FullName ?? declared.Name))
                .Bind(marked => op.Catch(() => Fin.Succ(value: new Guid(marked.Value)))))
            .Bind(value => Of(value: value, key: op)));
    }

    public static Fin<PanelKey> Of(Guid value, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PanelKey>(fault: Validate(value: value, provider: null, out PanelKey? admitted), admitted: admitted);
    }
}

[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct DockBarKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Dock-bar identity is empty.") : null;

    public static Fin<DockBarKey> Of(Guid value, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<DockBarKey>(fault: Validate(value: value, provider: null, out DockBarKey? admitted), admitted: admitted);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelChange {
    private PanelChange() { }
    public sealed record Shown : PanelChange;
    public sealed record Hidden : PanelChange;
    public sealed record Unclassified : PanelChange;
    public sealed record ClosingPanel : PanelChange;
    public sealed record ClosingDocument : PanelChange;

    internal static PanelChange Admit(ShowPanelReason reason) => (Panels.IsShowing(reason), Panels.IsHiding(reason)) switch {
        (true, false) => new Shown(),
        (false, true) => new Hidden(),
        _ => new Unclassified(),
    };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record MountState {
    private MountState() { }
    internal sealed record Live : MountState;
    internal sealed record Released : MountState;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelAudience {
    private PanelAudience() { }
    public sealed record Plugin(PluginKey Key) : PanelAudience;
    public sealed record Registry : PanelAudience;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        plugin: static (held, row) => row.Key.Admit(held),
        registry: static (_, _) => Fin.Succ(value: unit));

    internal bool Admits(Option<PluginKey> owner) => Switch(
        owner,
        plugin: static (held, row) => held.Exists(key => key == row.Key),
        registry: static (_, _) => true);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct PanelSeat(PluginKey Plugin, PanelKey Panel, Option<DocKey> Document);

public sealed record PanelFact(
    Option<PluginKey> Plugin,
    PanelKey Panel,
    Option<DocKey> Document,
    PanelChange Change,
    long Ordinal) {
    public Option<PanelSeat> Seat =>
        Plugin.Map(owner => new PanelSeat(Plugin: owner, Panel: Panel, Document: Document));
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class HostPanel : Panel, IPanel {
    private static readonly Rasm.Numerics.Dimension FaultCap = Rasm.Numerics.Dimension.Create(value: 32);

    private readonly Fin<PluginKey> owner;
    private readonly Fin<PanelKey> identity;
    private readonly Op op;
    private readonly Option<Control> fallback;
    private readonly Ring<Error> faults = new(cap: FaultCap);
    private readonly Atom<MountState> state = Atom<MountState>(new MountState.Live());

    protected HostPanel(PluginKey plugin, ControlSpec content, ElementRuntime runtime, Op? key = null) {
        op = key.OrDefault();
        owner = plugin.Admit(op).Map(_ => plugin);
        identity = PanelKey.Of(panelType: GetType(), key: op);
        Construction = ControlForge.Realize(spec: content, runtime: runtime, key: op);
        Control? rejected = null;
        Content = Construction.Match<Control>(
            Succ: receipt => {
                EtoExtensions.UseRhinoStyle(receipt.Resource.Host);
                return receipt.Resource.Host;
            },
            Fail: fault => rejected = new Label { Text = fault.Message });
        fallback = Optional(rejected);
    }

    public Fin<Lease<ElementReceipt>> Construction { get; }

    public Seq<Error> Faults => faults.Parked;

    protected virtual Fin<Unit> OnLife(PanelFact fact) => Fin.Succ(value: unit);

    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) =>
        Route(serial: documentSerialNumber, change: PanelChange.Admit(reason));

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) =>
        Route(serial: documentSerialNumber, change: PanelChange.Admit(reason));

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) {
        Route(
            serial: documentSerialNumber,
            change: onCloseDocument ? new PanelChange.ClosingDocument() : new PanelChange.ClosingPanel());
        ignore(Release());
    }

    protected override void Dispose(bool disposing) {
        if (disposing) ignore(Release());
        base.Dispose(disposing);
    }

    private Fin<Unit> Release() => Cell.Step(
            cell: state,
            step: static held => held is MountState.Live ? Some<MountState>(new MountState.Released()) : Option<MountState>.None,
            declined: new UiFault.Released(Key: op))
        is Transition<MountState>.Committed
        ? HostThread.Release(
                releases: Construction.Match(
                        Succ: receipt => Seq<Func<Fin<Unit>>>(() => receipt.Use(seated => seated.Release(), op)),
                        Fail: static _ => Seq<Func<Fin<Unit>>>())
                    + fallback.Match(
                        Some: control => Seq<Func<Fin<Unit>>>(() => op.Catch(() => Fin.Succ(value: Op.Side(control.Dispose)))),
                        None: static () => Seq<Func<Fin<Unit>>>()),
                key: op)
            .IfFail(failure => ignore(faults.Park(item: failure)))
        : Fin.Succ(value: unit);

    private void Route(uint serial, PanelChange change) => ignore(op
        .Catch(() =>
            from plugin in owner
            from panel in identity
            from fact in PanelHost.Stamp(
                plugin: Some(plugin),
                panel: panel,
                document: serial is 0u ? None : Some(DocKey.Create(value: serial)),
                change: change,
                op: op)
            from _ in OnLife(fact)
            select unit)
        .IfFail(failure => ignore(faults.Park(item: failure))));
}
```

## [03]-[PANEL_HOST]

- Owner: `PanelIntent<TPanel>` is the one registry operation family for a panel type; `PanelMount<TPanel>` is its result family; `PanelVerb` names which settlement a mount reports; `PanelBadge` is the two-seam icon projection the host publishes; `PanelPresence` carries visibility beside the dock bars and the registry-wide open set; `PanelRegistry` is the one process state the host reads and writes.
- Cases: registration, placement, presence, document close, scoped instances, icon replacement, and dock-bar usage — session-scoped and serial-scoped instance reads are one case because the instance scope already discriminates them.
- Entry: `PanelHost.Run<TPanel>` dispatches one request under one command-thread crossing; `PanelHost.Use<TPanel,T>` is the ONE live-instance surface, running its body inside the session frame that resolved the instances so no panel crosses out of the boundary.
- Auto: the registry is ONE state — the seat-keyed fact ledger, the watcher fan, and the two monotone ordinals move together under one commit, so a stamp mints its ordinal from the state it lands in rather than from a process-wide counter beside it.
- Auto: the three settle-only receipts fold to one case carrying the verb, so a reader takes the verb off a column rather than off a case name and a fourth settlement is one row.
- Law: the icon is a kernel `AssetOrigin` and the host publishes exactly TWO seams for it — a resource named by assembly and path, and a live icon object. `Resource` takes the resource-backed registration with no materialization and no disposal at all; `File` mints an icon under a lease; every other origin case names a byte source the host panel registry has no member for and refuses TYPED by name.
- Law: a rebadge from a resource anchor refuses when the anchor names an assembly other than the panel type's own, because the host's rebadge member takes the resource path ALONE and resolves it against that type — a silently mismatched anchor would replace the icon with nothing.
- Law: `Register` proves the host plug-in identity IS the declared key, so a panel registered under one plug-in never stamps another plug-in's seat.
- Law: a generated row cannot be absent, so the placement admission gates the identities it carries and nothing else — a null check over a smart-enum column is a guard at use where construction already closed the corner.
- Law: visibility is resolved by ONE row read over the two host probes, so the selected-and-visible corner pair is a row lookup rather than a truth table restated at the call site.
- Receipt: `PanelMount<TPanel>` — a settlement with its verb, a presence, a found seat with its live count, or a dock-bar usage row.
- Boundary: an icon minted from a path is disposed after the synchronous host call; a resource anchor mints nothing, and a borrowed native icon stays the caller's.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`Panels.RegisterPanel` both overloads, `OpenPanel`, `OpenPanelAsSibling`, `FloatPanel`, `ClosePanel`, `IsPanelVisible`, `PanelDockBars`, `GetOpenPanelIds`, `DockBarIdInUse`, `ChangePanelIcon` both overloads, `GetPanels<T>`, `PanelType`, `FloatPanelMode`); `libs/dotnet/.api/api-system-drawing-common.md` (the icon the registry takes); LanguageExt.Core (`Fin`, `Option`, `HashMap`, `Seq`, `Atom`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`); `Rasm/Interaction` (`AssetOrigin`, `AssetAnchor`, `FileLocation`, `UiFault`); `Rasm/Domain` (`Op`, `Cell`, `Transition`, `Lease<T>`); `Rasm.Rhino/Document` (`DocumentSession`, `SessionNeed`, `DocKey`, `PluginKey`, `Subscription`).
- Growth: a new registry operation is one `PanelIntent` case, one arm, and one `PanelMount` shape only if no existing shape carries it; a new settlement is one `PanelVerb` row.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class PanelFocus {
    public static readonly PanelFocus Background = new(false);
    public static readonly PanelFocus Selected = new(true);
}

[SmartEnum<Panels.FloatPanelMode>]
public sealed partial class PanelFloat {
    public static readonly PanelFloat Show = new(key: Panels.FloatPanelMode.Show);
    public static readonly PanelFloat Hide = new(key: Panels.FloatPanelMode.Hide);
    public static readonly PanelFloat Toggle = new(key: Panels.FloatPanelMode.Toggle);
}

[SmartEnum<PanelType>]
public sealed partial class PanelSite {
    public static readonly PanelSite Document = new(key: PanelType.PerDoc);
    public static readonly PanelSite System = new(key: PanelType.System);
}

[SmartEnum<bool>]
public sealed partial class DockBarUse {
    public static readonly DockBarUse Free = new(false);
    public static readonly DockBarUse Taken = new(true);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelVerb {
    public static readonly PanelVerb Registered = new(key: "registered");
    public static readonly PanelVerb Closed = new(key: "closed");
    public static readonly PanelVerb Rebadged = new(key: "rebadged");
}

[SmartEnum<int>]
public sealed partial class PanelVisibility {
    public static readonly PanelVisibility Hidden = new(key: 0);
    public static readonly PanelVisibility Visible = new(key: 1);
    public static readonly PanelVisibility Selected = new(key: 2);

    internal static PanelVisibility Of(bool selected, bool visible) =>
        selected ? Selected : visible ? Visible : Hidden;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelInstanceScope {
    private PanelInstanceScope() { }
    public sealed record Document(DocumentSession Session) : PanelInstanceScope;
    public sealed record Serial(DocKey Document) : PanelInstanceScope;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        document: static (held, row) => held.Need(row.Session).Map(static _ => unit),
        serial: static (held, row) => held
            .AcceptValidated<DocKey>(
                fault: DocKey.Validate(value: row.Document.ToValue(), provider: null, out DocKey? admitted),
                admitted: admitted)
            .Map(static _ => unit));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelPlacement {
    private PanelPlacement() { }
    public sealed record Docked(PanelFocus Focus) : PanelPlacement;
    public sealed record AtBar(DockBarKey DockBar, PanelFocus Focus) : PanelPlacement;
    public sealed record Beside(PanelKey Sibling, PanelFocus Focus) : PanelPlacement;
    public sealed record Floating(PanelFloat Mode) : PanelPlacement;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        docked: static (_, _) => Fin.Succ(value: unit),
        atBar: static (held, row) => DockBarKey.Of(value: row.DockBar.ToValue(), key: held).Map(static _ => unit),
        beside: static (held, row) => PanelKey.Of(value: row.Sibling.ToValue(), key: held).Map(static _ => unit),
        floating: static (_, _) => Fin.Succ(value: unit));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PanelBadge {
    private PanelBadge() { }
    internal sealed record Named(AssetAnchor Anchor) : PanelBadge;
    internal sealed record Owned(Lease<DrawingIcon> Icon) : PanelBadge;

    internal static Fin<PanelBadge> Of(AssetOrigin origin, Op op) => origin.Switch(
        state: op,
        resource: static (op, row) => Fin.Succ<PanelBadge>(new Named(Anchor: row.Anchor)),
        file: static (op, row) => Lease<DrawingIcon>
            .Acquire(mint: () => new DrawingIcon(fileName: row.Location.Value), key: op)
            .Map(static icon => (PanelBadge)new Owned(Icon: icon)),
        stream: static (op, _) => Unserved(nameof(AssetOrigin.Stream), op),
        raster: static (op, _) => Unserved(nameof(AssetOrigin.Raster), op),
        vector: static (op, _) => Unserved(nameof(AssetOrigin.Vector), op),
        source: static (op, _) => Unserved(nameof(AssetOrigin.Source), op),
        render: static (op, _) => Unserved(nameof(AssetOrigin.Render), op));

    private static Fin<PanelBadge> Unserved(string origin, Op op) =>
        Fin.Fail<PanelBadge>(error: new UiFault.HostRejected(
            Key: op,
            Detail: $"{nameof(Panels.RegisterPanel)} takes a resource anchor or an icon; {origin} is neither"));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PanelPresence(
    PanelKey Panel,
    PanelVisibility Visibility,
    Seq<DockBarKey> DockBars,
    Seq<PanelKey> OpenPanels);

internal sealed record PanelRegistry(
    HashMap<PanelSeat, PanelFact> Facts,
    Seq<(long Id, PanelAudience Audience, CallbackObserver<PanelFact> Observer)> Watchers,
    long Stamped,
    long Observers) {
    internal static PanelRegistry Empty => new(
        Facts: HashMap<PanelSeat, PanelFact>(),
        Watchers: Seq<(long, PanelAudience, CallbackObserver<PanelFact>)>(),
        Stamped: 0L,
        Observers: 0L);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelIntent<TPanel> where TPanel : HostPanel {
    private PanelIntent() { }
    public sealed record Register(PlugIn Owner, HostText Caption, AssetOrigin Icon, PanelSite Site) : PanelIntent<TPanel>;
    public sealed record Open(PanelPlacement Placement) : PanelIntent<TPanel>;
    public sealed record Presence : PanelIntent<TPanel>;
    public sealed record Close(DocumentSession Session) : PanelIntent<TPanel>;
    public sealed record Instances(PanelInstanceScope Scope) : PanelIntent<TPanel>;
    public sealed record Rebadge(AssetOrigin Icon) : PanelIntent<TPanel>;
    public sealed record DockBarUsage(DockBarKey DockBar) : PanelIntent<TPanel>;

    internal Fin<Unit> Admit(PluginKey plugin, Op op) => Switch(
        (Plugin: plugin, Op: op),
        register: static (held, row) =>
            from owner in held.Op.Need(row.Owner)
            from _ in guard(flag: owner.Id == held.Plugin.ToValue(), False: held.Op.InvalidInput()).ToFin()
            from __ in held.Op.Accept<object>(row.Caption, row.Icon, row.Site)
            select unit,
        open: static (held, row) => held.Op.Need(row.Placement).Bind(place => place.Admit(held.Op)),
        presence: static (_, _) => Fin.Succ(value: unit),
        close: static (held, row) => held.Op.Need(row.Session).Map(static _ => unit),
        instances: static (held, row) => held.Op.Need(row.Scope).Bind(scope => scope.Admit(held.Op)),
        rebadge: static (held, row) => held.Op.Need(row.Icon).Map(static _ => unit),
        dockBarUsage: static (held, row) => DockBarKey.Of(value: row.DockBar.ToValue(), key: held.Op).Map(static _ => unit));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelMount<TPanel> where TPanel : HostPanel {
    private PanelMount() { }
    public sealed record Settled(PanelKey Panel, PanelVerb Verb) : PanelMount<TPanel>;
    public sealed record Opened(PanelPresence Presence) : PanelMount<TPanel>;
    public sealed record Probed(PanelPresence Presence) : PanelMount<TPanel>;
    public sealed record Found(PanelSeat Seat, Rasm.Numerics.Dimension Live) : PanelMount<TPanel>;
    public sealed record DockBar(DockBarKey Id, DockBarUse Use) : PanelMount<TPanel>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PanelHost {
    private static readonly Atom<PanelRegistry> Registry = Atom(PanelRegistry.Empty);

    public static HashMap<PanelSeat, PanelFact> Facts => Registry.Value.Facts;

    public static HashMap<Option<DocKey>, PanelFact> FactsFor(PluginKey plugin, PanelKey panel) =>
        toHashMap(toSeq(Registry.Value.Facts).Choose(row => row.Key.Plugin == plugin && row.Key.Panel == panel
            ? Some((row.Key.Document, row.Value))
            : Option<(Option<DocKey>, PanelFact)>.None));

    public static Fin<PanelMount<TPanel>> Run<TPanel>(PluginKey plugin, PanelIntent<TPanel> request, Op? key = null)
        where TPanel : HostPanel {
        Op op = key.OrDefault();
        return from _ in op.Need(request)
               from __ in plugin.Admit(op)
               from ___ in request.Admit(plugin: plugin, op: op)
               from panel in PanelKey.Of(panelType: typeof(TPanel), key: op)
               from receipt in request.Switch(
                   (Plugin: plugin, Panel: panel, Op: op),
                   register: static (held, work) => Badged<TPanel>(
                       origin: work.Icon,
                       op: held.Op,
                       named: (anchor, caption) => Op.Side(() => Panels.RegisterPanel(
                           work.Owner, typeof(TPanel), caption, anchor.Owner, anchor.ResourcePath, work.Site.Key)),
                       owned: (icon, caption) => Op.Side(() => Panels.RegisterPanel(
                           work.Owner, typeof(TPanel), caption, icon, work.Site.Key)),
                       caption: work.Caption,
                       verb: PanelVerb.Registered,
                       panel: held.Panel),
                   open: static (held, work) => HostThread.Run(
                       work: new HostWork<PanelMount<TPanel>>.Execute(
                           Body: () => Opened<TPanel>(held.Panel, work.Placement, held.Op)),
                       key: held.Op),
                   presence: static (held, _) => HostThread.Run(
                       work: new HostWork<PanelMount<TPanel>>.Execute(Body: () => Probe<TPanel>(held.Panel, held.Op)
                           .Map<PanelMount<TPanel>>(presence => new PanelMount<TPanel>.Probed(Presence: presence))),
                       key: held.Op),
                   close: static (held, work) => HostThread.Run(
                       work: new HostWork<PanelMount<TPanel>>.Session(
                           Document: work.Session,
                           Needs: [SessionNeed.Redraw],
                           Body: document => held.Op.Catch(() => Fin.Succ<PanelMount<TPanel>>(
                               value: (Op.Side(() => Panels.ClosePanel(typeof(TPanel), document)),
                                   new PanelMount<TPanel>.Settled(Panel: held.Panel, Verb: PanelVerb.Closed)).Item2))),
                       key: held.Op),
                   instances: static (held, work) => Use<TPanel, PanelMount<TPanel>>(
                       plugin: held.Plugin,
                       scope: work.Scope,
                       body: (seat, live) => Fin.Succ<PanelMount<TPanel>>(value: new PanelMount<TPanel>.Found(
                           Seat: seat, Live: Rasm.Numerics.Dimension.Create(value: live.Count))),
                       key: held.Op),
                   rebadge: static (held, work) => Badged<TPanel>(
                       origin: work.Icon,
                       op: held.Op,
                       named: (anchor, _) => Op.Side(() => Panels.ChangePanelIcon(typeof(TPanel), anchor.ResourcePath)),
                       owned: (icon, _) => Op.Side(() => Panels.ChangePanelIcon(typeof(TPanel), icon)),
                       caption: None,
                       verb: PanelVerb.Rebadged,
                       panel: held.Panel),
                   dockBarUsage: static (held, work) => HostThread.Run(
                       work: new HostWork<PanelMount<TPanel>>.Execute(Body: () => held.Op.Catch(() =>
                           held.Op.Row<bool, DockBarUse>(candidate: Panels.DockBarIdInUse(work.DockBar))
                               .Map<PanelMount<TPanel>>(use => new PanelMount<TPanel>.DockBar(
                                   Id: work.DockBar, Use: use)))),
                       key: held.Op))
               select receipt;
    }

    public static Fin<T> Use<TPanel, T>(
        PluginKey plugin,
        PanelInstanceScope scope,
        Func<PanelSeat, Seq<TPanel>, Fin<T>> body,
        Op? key = null)
        where TPanel : HostPanel {
        Op op = key.OrDefault();
        return from _ in op.Accept<object>(scope, body)
               from __ in plugin.Admit(op)
               from ___ in scope.Admit(op)
               from panel in PanelKey.Of(panelType: typeof(TPanel), key: op)
               from result in scope.Switch(
                   (Plugin: plugin, Panel: panel, Body: body, Op: op),
                   document: static (held, seat) => HostThread.Run(
                       work: new HostWork<T>.Session(
                           Document: seat.Session,
                           Needs: [SessionNeed.Read],
                           Body: document => DocKey.Of(document: document, key: held.Op).Bind(model => held.Body(
                               new PanelSeat(Plugin: held.Plugin, Panel: held.Panel, Document: Some(model)),
                               toSeq(Panels.GetPanels<TPanel>(document)).Strict()))),
                       key: held.Op),
                   serial: static (held, seat) => HostThread.Run(
                       work: new HostWork<T>.Execute(Body: () => held.Body(
                           new PanelSeat(Plugin: held.Plugin, Panel: held.Panel, Document: Some(seat.Document)),
                           toSeq(Panels.GetPanels<TPanel>(seat.Document)).Strict())),
                       key: held.Op))
               select result;
    }

    internal static Fin<PanelFact> Stamp(
        Option<PluginKey> plugin, PanelKey panel, Option<DocKey> document, PanelChange change, Op op) {
        PanelFact Draft(long ordinal) => new(
            Plugin: plugin, Panel: panel, Document: document, Change: change, Ordinal: ordinal);
        return Cell.Commit(cell: Registry, compute: seen => {
                PanelFact fact = Draft(ordinal: seen.Stamped + 1L);
                return fact.Seat.Match(
                    Some: seat => seen with { Facts = seen.Facts.AddOrUpdate(seat, fact), Stamped = fact.Ordinal },
                    None: () => seen with { Stamped = fact.Ordinal });
            })
            .Switch(
                state: (Draft: (Func<long, PanelFact>)Draft, Op: op),
                committed: static (held, row) => {
                    PanelFact fact = held.Draft(row.State.Stamped);
                    return Fin.Succ(value: (row.State.Watchers
                        .Filter(watcher => watcher.Audience.Admits(fact.Plugin))
                        .Iter(watcher => watcher.Observer.Guard(project: () => Fin.Succ(value: fact), op: held.Op)),
                        fact).Item2);
                },
                ceded: static (held, _) => Fin.Fail<PanelFact>(held.Op.InvalidResult()),
                refused: static (_, row) => Fin.Fail<PanelFact>(row.Cause),
                contended: static (held, _) => Fin.Fail<PanelFact>(held.Op.InvalidResult()));
    }

    internal static Subscription Watch(PanelAudience audience, CallbackObserver<PanelFact> observer) {
        long id = Cell.Commit(cell: Registry, compute: seen => seen with {
            Observers = seen.Observers + 1L,
            Watchers = seen.Watchers.Add((Id: seen.Observers + 1L, Audience: audience, Observer: observer)),
        }).Current.Observers;
        return Subscription.Of(detach: () => ignore(Cell.Commit(
            cell: Registry,
            compute: seen => seen with { Watchers = seen.Watchers.Filter(row => row.Id != id) })));
    }

    private static Fin<PanelMount<TPanel>> Badged<TPanel>(
        AssetOrigin origin,
        Op op,
        Func<AssetAnchor, string, Unit> named,
        Func<DrawingIcon, string, Unit> owned,
        Option<HostText> caption,
        PanelVerb verb,
        PanelKey panel)
        where TPanel : HostPanel =>
        HostThread.Run(
            work: new HostWork<PanelMount<TPanel>>.Execute(Body: () =>
                from text in caption.Match(
                    Some: value => op.AcceptText(value: value.Resolve()),
                    None: () => Fin.Succ(value: string.Empty))
                from badge in PanelBadge.Of(origin: origin, op: op)
                from _ in badge.Switch(
                    (Anchor: named, Icon: owned, Text: text, Op: op),
                    named: (held, row) => guard(
                            flag: row.Anchor.Owner == typeof(TPanel).Assembly || verb != PanelVerb.Rebadged,
                            False: new UiFault.HostRejected(
                                Key: held.Op,
                                Detail: $"{nameof(Panels.ChangePanelIcon)} resolves a resource against {typeof(TPanel).Assembly.GetName().Name}"))
                        .ToFin()
                        .Map(_ => held.Anchor(row.Anchor, held.Text)),
                    owned: (held, row) => row.Icon.Use(icon => Fin.Succ(value: held.Icon(icon, held.Text)), held.Op))
                select (PanelMount<TPanel>)new PanelMount<TPanel>.Settled(Panel: panel, Verb: verb)),
            key: op);

    private static Fin<PanelMount<TPanel>> Opened<TPanel>(PanelKey panel, PanelPlacement placement, Op op)
        where TPanel : HostPanel =>
        placement.Switch(
            (Panel: panel, Op: op),
            docked: static (held, place) => Fin.Succ(value: Op.Side(() => Panels.OpenPanel(typeof(TPanel), place.Focus.Key))),
            atBar: static (held, place) => held.Op.Confirm(success: Panels.OpenPanel(place.DockBar, typeof(TPanel), place.Focus.Key)),
            beside: static (held, place) => held.Op.Confirm(success: Panels.OpenPanelAsSibling(held.Panel, place.Sibling, place.Focus.Key)),
            floating: static (held, place) => held.Op.Confirm(success: Panels.FloatPanel(typeof(TPanel), place.Mode.Key)))
        .Bind(_ => Probe<TPanel>(panel: panel, op: op))
        .Map<PanelMount<TPanel>>(presence => new PanelMount<TPanel>.Opened(Presence: presence));

    private static Fin<PanelPresence> Probe<TPanel>(PanelKey panel, Op op) where TPanel : HostPanel => op.Catch(() => {
        bool selected = Panels.IsPanelVisible(typeof(TPanel), selectedTabIsVisible: true);
        bool visible = selected || Panels.IsPanelVisible(typeof(TPanel), selectedTabIsVisible: false);
        return from dockBars in toSeq(Panels.PanelDockBars(panel)).TraverseM(id => DockBarKey.Of(value: id, key: op)).As()
               from openPanels in toSeq(Panels.GetOpenPanelIds()).TraverseM(id => PanelKey.Of(value: id, key: op)).As()
               select new PanelPresence(
                   Panel: panel,
                   Visibility: PanelVisibility.Of(selected: selected, visible: visible),
                   DockBars: dockBars.Strict(),
                   OpenPanels: openPanels.Strict());
    });
}
```

## [04]-[PANEL_OBSERVATION]

- Owner: `PanelObserve` chooses the owned callback ledger under a declared audience or the host-wide document projection; `PanelObservation.Observe` is the one subscription entry; `PanelHooks.Mount` seats the point.
- Entry: `Observe` answers one symmetric subscription for either case and delivers projection failures through the sink rail.
- Law: owned callbacks update the registry ledger; the host-wide projection never re-stamps it.
- Law: the point binding is TYPED end to end. The ask is the callback observer and the grant is the subscription, both named on the binding, so the registry's typed bind answers by name and the cast that once turned an untyped ask into an observer has no site.
- Law: multi-plug-in coexistence is one law — a point seat is first-mount-wins, every subscriber is keyed by the plug-in its mount declared, and teardown returns the seat, so a second plug-in mounting the same point faults typed instead of forking discovery or crossing fact streams.
- Law: the point's replay modality is the seat-keyed latest fact a binder reads before its first delivery, and the per-plug-in projection reads one panel's rows per document.
- Boundary: each delivery crosses the guarded observer; delivery and rejection faults accumulate without starving sibling observers.
- Receipt: `Subscription` — the symmetric detach both cases answer.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (panel visibility semantics); LanguageExt.Core (`Fin`, `Option`, `Seq`); `Rasm/Domain` (`Op`, `HookBinding`); `Rasm.Rhino/Document` (`MountRegistry`, `RhinoPoint`, `DocumentStream`, `Observation`, `EventScope`, `EventFamily`, `EventPayload`, `Delivery`, `ReceiptPolicy`, `Subscription`, `PluginKey`).
- Growth: a new observation source is one `PanelObserve` case with one arm.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelObserve {
    private PanelObserve() { }
    public sealed record Owned(PanelAudience Audience) : PanelObserve;
    public sealed record Hosted : PanelObserve;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        owned: static (held, row) => held.Need(row.Audience).Bind(audience => audience.Admit(held)),
        hosted: static (_, _) => Fin.Succ(value: unit));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PanelObservation {
    public static Fin<Subscription> Observe(
        PanelObserve scope,
        CallbackObserver<PanelFact> observer,
        ReceiptPolicy receipts,
        Op? key = null) {
        Op op = key.OrDefault();
        return op.Accept<object>(scope, observer, receipts).Bind(_ => scope.Admit(op)).Bind(_ => scope.Switch(
            (Observer: observer, Receipts: receipts, Op: op),
            owned: static (held, row) => Fin.Succ(value: PanelHost.Watch(audience: row.Audience, observer: held.Observer)),
            hosted: static (held, _) => DocumentStream.Observe(new Observation.Host(
                    Scope: new EventScope.AnyDocument(),
                    Families: Seq(EventFamily.PanelVisibility, EventFamily.PanelClosed),
                    Delivery: new Delivery.Inline(Sink: fact => Fin.Succ(value: held.Observer.Guard(
                        project: () => fact.Payload is EventPayload.Panel panel
                            ? PanelKey.Of(value: panel.PanelId, key: held.Op).Bind(id => PanelHost.Stamp(
                                plugin: None,
                                panel: id,
                                document: fact.Key,
                                change: panel.State.Switch(
                                    shown: static _ => (PanelChange)new PanelChange.Shown(),
                                    hidden: static _ => new PanelChange.Hidden(),
                                    closed: static _ => new PanelChange.ClosingPanel()),
                                op: held.Op))
                            : Fin.Fail<PanelFact>(error: held.Op.InvalidResult()),
                        op: held.Op))),
                    Receipts: held.Receipts))
                .Map(watch => Subscription.Of(detach: watch.Dispose))));
    }
}

public static class PanelHooks {
    public static Fin<IDisposable> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return MountRegistry.Mount(
            binding: new HookBinding<RhinoPoint, PluginKey, CallbackObserver<PanelFact>, Subscription>(
                Point: RhinoPoint.HostUiPanel,
                Owner: plugin,
                Bind: observer => Fin.Succ(value: PanelHost.Watch(
                    audience: new PanelAudience.Plugin(Key: plugin),
                    observer: observer))),
            key: op);
    }
}
```

## [05]-[RUI]

- Owner: `RuiCommand` closes file, group, sidebar, and sizing modalities; `RuiFileRef` closes identifier, path, and named lookup; `RuiSnapshot` is the census; `RuiReceipt` carries the snapshot with applied-prefix evidence; `RuiMap` is the host-to-record projection.
- Entry: `Rui.Run` admits the whole batch, then applies it under one command crossing and answers the post-operation snapshot.
- Auto: the census reads each file ONCE and nests its own groups and toolbars, so the file identity every flat row repeated is containment and the two count columns are the rosters' own lengths.
- Auto: the batch fold halts on the first refusal and the applied count IS the fold's state, so no parallel record carries a fault sentinel beside a count the traversal already holds.
- Law: batch ADMISSION accumulates and batch APPLICATION halts. A malformed roster is the caller's own set of mistakes and reporting one of six sends them back six times; a half-applied mutation is a host state the next command reads, so application stops where it broke.
- Law: the snapshot's read side mirrors the write side's ROSTERS. Sidebar visibility keys on the sidebar row and bar sizing on the bar row, because two flat columns per axis are a hand-kept mirror of a roster the write side already owns and they diverge the first time a third row lands.
- Law: group state is a capability SET over a named vocabulary, so a visible-and-docked group is one value rather than two bools nothing relates.
- Law: the file lookup scans the LIVE host collection and memoizes nothing. The batch this lookup serves mutates that collection — opening, closing, and saving files — so a frozen index is stale by construction at exactly the site that would read it.
- Law: a sizing command carries a NONEMPTY map, so the fold cannot be reached with nothing to apply.
- Receipt: `RuiReceipt` — the full post-operation snapshot with the applied count, and the fault beside it when the batch stopped early.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`RhinoApp.ToolbarFiles`, `ToolbarFileCollection.SidebarIsVisible`/`MruSidebarIsVisible`/`FindByPath`/`FindByName`, `ToolbarFile`, `Toolbar.BitmapSize`/`TabSize`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `HashMap`, `Traverse`, `foldWhile`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[ComplexValueObject]`, `[UseDelegateFromConstructor]`); `Rasm/Domain` (`Op`, `ICapability`, `CapabilitySet`, `CapabilityLaw`); `Rasm/Numerics` (`Dimension`).
- Growth: a new toolbar command is one `RuiCommand` case with one apply arm; a new census column is one field on its own fact; a new sidebar or bar is one row both sides read.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class NameMatch {
    public static readonly NameMatch Ordinal = new(false);
    public static readonly NameMatch IgnoreCase = new(true);
}

[SmartEnum<bool>]
public sealed partial class RuiVisibility {
    public static readonly RuiVisibility Hidden = new(false);
    public static readonly RuiVisibility Visible = new(true);
}

[SmartEnum<bool>]
public sealed partial class SavePolicy {
    public static readonly SavePolicy LeaveDirty = new(false);
    public static readonly SavePolicy Save = new(true);
}

[SmartEnum<bool>]
public sealed partial class ClosePolicy {
    public static readonly ClosePolicy Silent = new(false);
    public static readonly ClosePolicy Prompt = new(true);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuiGroupTrait : ICapability<RuiGroupTrait> {
    public static readonly RuiGroupTrait Visible = new(key: "visible");
    public static readonly RuiGroupTrait Docked = new(key: "docked");

    public static CapabilityLaw<RuiGroupTrait> Law => CapabilityLaw<RuiGroupTrait>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuiSidebar {
    public static readonly RuiSidebar Primary = new(
        key: "primary",
        apply: static visible => Op.Side(() => ToolbarFileCollection.SidebarIsVisible = visible.Key),
        read: static () => ToolbarFileCollection.SidebarIsVisible);
    public static readonly RuiSidebar Recent = new(
        key: "recent",
        apply: static visible => Op.Side(() => ToolbarFileCollection.MruSidebarIsVisible = visible.Key),
        read: static () => ToolbarFileCollection.MruSidebarIsVisible);

    [UseDelegateFromConstructor] internal partial Unit Apply(RuiVisibility visible);
    [UseDelegateFromConstructor] internal partial bool Read();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuiBar {
    public static readonly RuiBar Bitmap = new(
        key: "bitmap",
        apply: static size => Op.Side(() => Toolbar.BitmapSize = size),
        read: static () => Toolbar.BitmapSize);
    public static readonly RuiBar Tab = new(
        key: "tab",
        apply: static size => Op.Side(() => Toolbar.TabSize = size),
        read: static () => Toolbar.TabSize);

    [UseDelegateFromConstructor] internal partial Unit Apply(DrawingSize size);
    [UseDelegateFromConstructor] internal partial DrawingSize Read();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuiFileRef {
    private RuiFileRef() { }
    public sealed record ById(Guid Id) : RuiFileRef;
    public sealed record ByPath(string Path) : RuiFileRef;
    public sealed record ByName(string Name, NameMatch Match) : RuiFileRef;

    internal Fin<RuiFileRef> Admit(Op op) => Switch(
        op,
        byId: static (held, address) => address.Id != Guid.Empty
            ? Fin.Succ<RuiFileRef>(value: address)
            : Fin.Fail<RuiFileRef>(error: held.InvalidInput()),
        byPath: static (held, address) => PathOf(candidate: address.Path, op: held)
            .Map<RuiFileRef>(path => address with { Path = path }),
        byName: static (held, address) => held.AcceptText(value: address.Name)
            .Map<RuiFileRef>(name => address with { Name = name }));

    internal Fin<ToolbarFile> ResolveAdmitted(Op op) => Switch(
        op,
        byId: static (held, address) => toSeq(RhinoApp.ToolbarFiles).Choose(Optional)
            .Find(candidate => candidate.Id == address.Id)
            .ToFin(Fail: held.MissingContext()),
        byPath: static (held, address) => Optional(RhinoApp.ToolbarFiles.FindByPath(path: address.Path))
            .ToFin(Fail: held.MissingContext()),
        byName: static (held, address) => Optional(RhinoApp.ToolbarFiles.FindByName(name: address.Name, ignoreCase: address.Match.Key))
            .ToFin(Fail: held.MissingContext()));

    internal static Fin<string> PathOf(string candidate, Op op) =>
        from text in op.AcceptText(value: candidate)
        from path in op.Catch(() => Fin.Succ(value: System.IO.Path.GetFullPath(text)))
        from _ in guard(flag: System.IO.Path.IsPathFullyQualified(path), False: op.InvalidInput()).ToFin()
        select path;
}

[ComplexValueObject]
public sealed partial class RuiBarSize {
    public FrozenDictionary<RuiBar, DrawingSize> Values { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenDictionary<RuiBar, DrawingSize> values) =>
        validationError = values.Count is 0 || values.Values.Any(static size => size.Width <= 0 || size.Height <= 0)
            ? new ValidationError(message: "Toolbar sizing is empty or nonpositive.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuiCommand {
    private RuiCommand() { }
    public sealed record OpenFile(string Path, SavePolicy Save) : RuiCommand;
    public sealed record CloseFile(RuiFileRef File, ClosePolicy Close) : RuiCommand;
    public sealed record SaveFile(RuiFileRef File) : RuiCommand;
    public sealed record SaveFileAs(RuiFileRef File, string Target) : RuiCommand;
    public sealed record Group(RuiFileRef File, Guid GroupId, RuiVisibility Visibility) : RuiCommand;
    public sealed record Sidebar(RuiSidebar Target, RuiVisibility Visibility) : RuiCommand;
    public sealed record BarSize(RuiBarSize Size) : RuiCommand;

    internal Fin<RuiCommand> Admit(Op op) => Switch(
        op,
        openFile: static (held, row) => RuiFileRef.PathOf(candidate: row.Path, op: held)
            .Map<RuiCommand>(path => row with { Path = path }),
        closeFile: static (held, row) => held.Need(row.File)
            .Bind(value => value.Admit(held))
            .Map<RuiCommand>(file => row with { File = file }),
        saveFile: static (held, row) => held.Need(row.File)
            .Bind(value => value.Admit(held))
            .Map<RuiCommand>(file => row with { File = file }),
        saveFileAs: static (held, row) =>
            from file in held.Need(row.File).Bind(value => value.Admit(held))
            from target in RuiFileRef.PathOf(candidate: row.Target, op: held)
            select (RuiCommand)(row with { File = file, Target = target }),
        group: static (held, row) =>
            from file in held.Need(row.File).Bind(value => value.Admit(held))
            from _ in guard(flag: row.GroupId != Guid.Empty, False: held.InvalidInput()).ToFin()
            select (RuiCommand)(row with { File = file }),
        sidebar: static (_, row) => Fin.Succ<RuiCommand>(value: row),
        barSize: static (held, row) => held.Need(row.Size).Map<RuiCommand>(_ => row));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RuiGroupFact(Guid Group, string Name, CapabilitySet<RuiGroupTrait> State);

public sealed record RuiToolbarFact(Guid Toolbar, string Name);

public sealed record RuiFileFact(
    Guid Id, string Name, string Path, Seq<RuiGroupFact> Groups, Seq<RuiToolbarFact> Toolbars);

public sealed record RuiSnapshot(
    Seq<RuiFileFact> Files,
    HashMap<RuiSidebar, RuiVisibility> Sidebars,
    HashMap<RuiBar, DrawingSize> Bars);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuiReceipt {
    private RuiReceipt() { }
    public sealed record Completed(RuiSnapshot Snapshot, Rasm.Numerics.Dimension Applied) : RuiReceipt;
    public sealed record Partial(RuiSnapshot Snapshot, Rasm.Numerics.Dimension Applied, Error Fault) : RuiReceipt;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Rui {
    public static Fin<RuiReceipt> Run(Seq<RuiCommand> commands, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in commands
                   .Traverse(command => op.Need(command).Bind(value => value.Admit(op)).ToValidation())
                   .As()
                   .ToFin()
               from receipt in HostThread.Run(
                   work: new HostWork<RuiReceipt>.Execute(Body: () => Applied(commands: admitted.Strict(), op: op)),
                   key: op)
               select receipt;
    }

    private static Fin<RuiReceipt> Applied(Seq<RuiCommand> commands, Op op) {
        (int Applied, Option<Error> Fault) seed = (Applied: 0, Fault: None);
        var state = foldWhile(
            (held, command) => Apply(command: command, op: op).Match(
                Succ: _ => held with { Applied = held.Applied + 1 },
                Fail: fault => held with { Fault = Some(fault) }),
            static step => step.State.Fault.IsNone,
            seed,
            commands);
        return Census(op: op).Map(snapshot => state.Fault.Match<RuiReceipt>(
            Some: fault => new RuiReceipt.Partial(
                Snapshot: snapshot, Applied: Rasm.Numerics.Dimension.Create(value: state.Applied), Fault: fault),
            None: () => new RuiReceipt.Completed(
                Snapshot: snapshot, Applied: Rasm.Numerics.Dimension.Create(value: state.Applied))));
    }

    private static Fin<Unit> Apply(RuiCommand command, Op op) => command.Switch(
        op,
        openFile: static (held, work) =>
            from file in held.Catch(() => Optional(RhinoApp.ToolbarFiles.Open(path: work.Path))
                .ToFin(Fail: held.InvalidResult(detail: work.Path)))
            from _ in work.Save.Key ? held.Confirm(success: file.Save()) : Fin.Succ(value: unit)
            select unit,
        closeFile: static (held, work) => work.File.ResolveAdmitted(op: held)
            .Bind(file => held.Confirm(success: file.Close(prompt: work.Close.Key))),
        saveFile: static (held, work) => work.File.ResolveAdmitted(op: held).Bind(file => held.Confirm(success: file.Save())),
        saveFileAs: static (held, work) => work.File.ResolveAdmitted(op: held)
            .Bind(file => held.Confirm(success: file.SaveAs(path: work.Target))),
        group: static (held, work) =>
            from file in work.File.ResolveAdmitted(op: held)
            from groups in Indexed(count: file.GroupCount, read: file.GetGroup, op: held)
            from group in groups.Find(candidate => candidate.Id == work.GroupId).ToFin(Fail: held.MissingContext())
            select Op.Side(() => group.Visible = work.Visibility.Key),
        sidebar: static (_, work) => Fin.Succ(value: work.Target.Apply(visible: work.Visibility)),
        barSize: static (held, work) => toSeq(work.Size.Values)
            .TraverseM(size => held.Catch(() => Fin.Succ(value: size.Key.Apply(size.Value))))
            .As()
            .Map(static _ => unit));

    private static Fin<RuiSnapshot> Census(Op op) => op.Catch(() =>
        from files in toSeq(RhinoApp.ToolbarFiles)
            .TraverseM(file => Optional(file)
                .ToFin(Fail: op.InvalidResult(detail: nameof(RhinoApp.ToolbarFiles)))
                .Bind(seated => Filed(file: seated, op: op)))
            .As()
        select new RuiSnapshot(
            Files: files.Strict(),
            Sidebars: toHashMap(toSeq(RuiSidebar.Items).Map(static row => (row, row.Read()
                ? RuiVisibility.Visible
                : RuiVisibility.Hidden))),
            Bars: toHashMap(toSeq(RuiBar.Items).Map(static row => (row, row.Read())))));

    private static Fin<RuiFileFact> Filed(ToolbarFile file, Op op) =>
        from groups in Indexed(count: file.GroupCount, read: file.GetGroup, op: op)
        from toolbars in Indexed(count: file.ToolbarCount, read: file.GetToolbar, op: op)
        select new RuiFileFact(
            Id: file.Id,
            Name: file.Name,
            Path: file.Path,
            Groups: groups.Map(static group => new RuiGroupFact(
                Group: group.Id,
                Name: group.Name,
                State: CapabilitySet<RuiGroupTrait>.Of(
                    [.. Seq((Row: RuiGroupTrait.Visible, Held: group.Visible), (Row: RuiGroupTrait.Docked, Held: group.IsDocked))
                        .Filter(static row => row.Held)
                        .Map(static row => row.Row)]))).Strict(),
            Toolbars: toolbars.Map(static toolbar => new RuiToolbarFact(
                Toolbar: toolbar.Id, Name: toolbar.Name)).Strict());

    private static Fin<Seq<T>> Indexed<T>(int count, Func<int, T?> read, Op op) where T : class =>
        from _ in guard(flag: count >= 0, False: op.InvalidResult()).ToFin()
        from rows in Seq.generate(count, static index => index)
            .TraverseM(index => Optional(read(index)).ToFin(Fail: op.InvalidResult(detail: $"{typeof(T).Name}[{index}]")))
            .As()
        select rows.Strict();
}
```

## [06]-[MENU_LINKS]

- Owner: `RuiAddress` is the menu-item address; `MenuToggle` is the two-state axis every togglable delta carries; `MenuDelta` is the update algebra over enabled, checked, radio, and caption; `MenuLinks.Register` seats the callback.
- Entry: `Register` seats one host callback, folds every emitted delta onto the live update surface, and retains observer faults.
- Law: callback state is RECOMPUTED from the address on every raise; no mutable menu state escapes the host invocation.
- Law: registration mutates a process-wide handler table and mints no host leaf, so it MARSHALS like every sibling entry on this page — the required-frame crossing is reserved for the entries whose host leaves must originate in the caller's own frame.
- Boundary: a rejected registration is the operation's typed failure; delivery uses the shared guarded observer and runs on whatever thread the host raises the update on.
- Receipt: `Fin<Unit>` — the host publishes nothing beyond acceptance, and a fabricated receipt would assert one.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`RuiUpdateUi.RegisterMenuItem`, `RuiUpdateUi.Enabled`/`Checked`/`RadioChecked`/`Text`); LanguageExt.Core (`Fin`, `Seq`, `TraverseM`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[ComplexValueObject]`).
- Growth: a new menu axis is one `MenuDelta` case with one apply arm.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class MenuToggle {
    public static readonly MenuToggle Off = new(false);
    public static readonly MenuToggle On = new(true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MenuDelta {
    private MenuDelta() { }
    public sealed record Enabled(MenuToggle State) : MenuDelta;
    public sealed record Checked(MenuToggle State) : MenuDelta;
    public sealed record Radio(MenuToggle State) : MenuDelta;
    public sealed record Caption(HostText Value) : MenuDelta;
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class RuiAddress {
    public Guid File { get; }
    public Guid Menu { get; }
    public Guid Item { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid file,
        ref Guid menu,
        ref Guid item) =>
        validationError = file == Guid.Empty || menu == Guid.Empty || item == Guid.Empty
            ? new ValidationError(message: "RUI menu address contains an empty identity.")
            : null;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MenuLinks {
    public static Fin<Unit> Register(
        RuiAddress address,
        Func<RuiAddress, Seq<MenuDelta>> sync,
        CallbackObserver<Unit> observer,
        Op? key = null) {
        Op op = key.OrDefault();
        return op.Accept<object>(address, sync, observer).Bind(_ => HostThread.Run(
            work: new HostWork<Unit>.Execute(Body: () => op.Confirm(success: RuiUpdateUi.RegisterMenuItem(
                address.File,
                address.Menu,
                address.Item,
                (_, live) => ignore(observer.Guard(
                    project: () => op.Catch(() => sync(address)
                        .TraverseM(delta => Apply(live, delta, op))
                        .As()
                        .Map(static _ => unit)),
                    op: op))))),
            key: op));
    }

    private static Fin<Unit> Apply(RuiUpdateUi live, MenuDelta delta, Op op) => delta.Switch(
        (Live: live, Op: op),
        enabled: static (held, value) => Fin.Succ(value: Op.Side(() => held.Live.Enabled = value.State.Key)),
        @checked: static (held, value) => Fin.Succ(value: Op.Side(() => held.Live.Checked = value.State.Key)),
        radio: static (held, value) => Fin.Succ(value: Op.Side(() => held.Live.RadioChecked = value.State.Key)),
        caption: static (held, value) => held.Op.AcceptText(value: value.Value.Resolve())
            .Map(text => Op.Side(() => held.Live.Text = text)));
}
```

## [07]-[SECTIONS]

- Owner: `PanelSectionSpec` carries caption, body, height, command-option caption, a capability set, and one optional lifecycle hook; `PanelSectionFeature` and `PanelSectionHolderFeature` close per-section and holder capability; `PanelSectionSignal` closes attach, detach, holder-visibility, and refresh evidence; `PanelSectionLeaf` is the host section; `PanelSectionMount` owns the holder and every grown receipt.
- Entry: `PanelSections.Mount` opens ONE crossing, grows every body inside it, preserves declaration order, and answers a mount owning the holder, every receipt, and the accumulated hook faults.
- Auto: bodies grow through the DISPATCH-FREE core rather than the affinity-gated entry, because this owner already holds the crossing — a per-body gate would re-marshal inside a frame that is already the marshal.
- Auto: a mid-fold refusal releases the receipts it already grew in reverse order, so a partial realize leaks no host control and the hand cleanup tower has no site.
- Law: every leaf lifecycle override chains its host base FIRST, then routes its signal; a hook fault parks on the mount's bounded ring and never re-enters the holder.
- Law: the full-height law is a MOUNT law, not a section law. Every capability corner on one section is legal — a hidden section can still be collapsible and initially expanded — while at most one section in a holder can claim the full height, so the corner gate is open and the roster gate is at the fold that sees all of them.
- Law: an empty section sequence refuses BEFORE any host leaf mints, because a holder with no sections is a control the caller then has to discover is inert.
- Law: the refresh flags stay a host word. The host publishes no named flag vocabulary for its view update, so a roster here would be an authored guess at a set the host owns; the case carries the word and names it as the host's.
- Law: release is a guarded TRANSITION over the same state the panel base steps, so both one-shot owners on this page answer the same vocabulary and neither carries a latch of its own.
- Receipt: `PanelSectionMount` — the holder control, the accumulated hook and teardown faults, and the reverse-order drain.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui-controls.md` (`EtoCollapsibleSection3`, `ICollapsibleSectionHolder2`, `EtoCollapsibleSectionHolder2`, `LocalizeStringPair`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Atom`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[ComplexValueObject]`); `Rasm/Interaction` (`ControlSpec`, `ControlForge.Grow`, `ElementReceipt`, `ElementRuntime`, `UiFault`); `Rasm/Domain` (`Op`, `Cell`, `Transition`, `Ring<Error>`, `Lease<T>`, `ICapability`, `CapabilitySet`, `CapabilityLaw`); `Rasm/Numerics` (`Dimension`).
- Growth: a new lifecycle signal is one `PanelSectionSignal` case with one override; a new section capability is one row on its roster.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelSectionFeature : ICapability<PanelSectionFeature> {
    public static readonly PanelSectionFeature Expanded = new(key: "expanded");
    public static readonly PanelSectionFeature Collapsible = new(key: "collapsible");
    public static readonly PanelSectionFeature Hidden = new(key: "hidden");
    public static readonly PanelSectionFeature FullHeight = new(key: "full-height");

    public static CapabilityLaw<PanelSectionFeature> Law => CapabilityLaw<PanelSectionFeature>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelSectionHolderFeature : ICapability<PanelSectionHolderFeature> {
    public static readonly PanelSectionHolderFeature Scrollbars = new(key: "scrollbars");
    public static readonly PanelSectionHolderFeature Checkboxes = new(key: "checkboxes");

    public static CapabilityLaw<PanelSectionHolderFeature> Law => CapabilityLaw<PanelSectionHolderFeature>.Open;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelSectionSignal {
    private PanelSectionSignal() { }
    public sealed record Attaching : PanelSectionSignal;
    public sealed record Attached : PanelSectionSignal;
    public sealed record Detaching : PanelSectionSignal;
    public sealed record Detached : PanelSectionSignal;
    public sealed record HolderShown : PanelSectionSignal;
    public sealed record HolderHidden : PanelSectionSignal;
    public sealed record Refreshed(uint Flags) : PanelSectionSignal;
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class PanelSectionSpec {
    public HostText Caption { get; }
    public ControlSpec Body { get; }
    public Rasm.Numerics.Dimension Height { get; }
    public CapabilitySet<PanelSectionFeature> Features { get; }
    public Option<HostText> CommandOption { get; }
    public Option<Func<PanelSectionSignal, Fin<Unit>>> Life { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText caption,
        ref ControlSpec body,
        ref Rasm.Numerics.Dimension height,
        ref CapabilitySet<PanelSectionFeature> features,
        ref Option<HostText> commandOption,
        ref Option<Func<PanelSectionSignal, Fin<Unit>>> life) =>
        validationError = height.Value > 0
            ? null
            : new ValidationError(message: "Panel section requires a positive height.");
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class PanelSectionLeaf : EtoCollapsibleSection3 {
    private readonly PanelSectionSpec spec;
    private readonly Action<Error> report;
    private readonly Op op;

    internal PanelSectionLeaf(PanelSectionSpec spec, Control content, Action<Error> report, Op op) =>
        (this.spec, this.report, this.op, Content) = (spec, report, op, content);

    public override LocalizeStringPair Caption => new(spec.Caption.English, spec.Caption.Resolve());
    public override int SectionHeight => spec.Height.Value;
    public override bool Collapsible => spec.Features.Admits(PanelSectionFeature.Collapsible);
    public override bool Hidden => spec.Features.Admits(PanelSectionFeature.Hidden);
    public override bool InitiallyExpanded => spec.Features.Admits(PanelSectionFeature.Expanded);
    public override LocalizeStringPair CommandOptionName => spec.CommandOption.Match(
        Some: static caption => caption.OptionName(),
        None: static () => new LocalizeStringPair(string.Empty, string.Empty));

    public override void OnAttachingToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachingToHolder(holder);
        Route(signal: new PanelSectionSignal.Attaching());
    }

    public override void OnAttachedToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachedToHolder(holder);
        Route(signal: new PanelSectionSignal.Attached());
    }

    public override void OnDetachingFromHolder(ICollapsibleSectionHolder2 holder) {
        base.OnDetachingFromHolder(holder);
        Route(signal: new PanelSectionSignal.Detaching());
    }

    public override void OnDetachedFromHolder(ICollapsibleSectionHolder2 holder) {
        base.OnDetachedFromHolder(holder);
        Route(signal: new PanelSectionSignal.Detached());
    }

    public override void HolderVisible(bool visible) {
        base.HolderVisible(visible);
        Route(signal: visible ? new PanelSectionSignal.HolderShown() : new PanelSectionSignal.HolderHidden());
    }

    public override void UpdateView(uint flags) {
        base.UpdateView(flags);
        Route(signal: new PanelSectionSignal.Refreshed(Flags: flags));
    }

    private void Route(PanelSectionSignal signal) => ignore(spec.Life.Iter(hook =>
        ignore(op.Catch(() => hook(signal)).IfFail(failure => { report(failure); return unit; }))));
}

public sealed class PanelSectionMount : IDisposable {
    private readonly Seq<ElementReceipt> contents;
    private readonly Ring<Error> faults;
    private readonly Atom<MountState> state = Atom<MountState>(new MountState.Live());
    private readonly Op op;

    internal PanelSectionMount(Control host, Seq<ElementReceipt> contents, Ring<Error> faults, Op op) =>
        (Host, this.contents, this.faults, this.op) = (host, contents, faults, op);

    public Control Host { get; }

    public Seq<Error> Faults => faults.Parked;

    public Fin<Unit> Release() => Cell.Step(
            cell: state,
            step: static held => held is MountState.Live ? Some<MountState>(new MountState.Released()) : Option<MountState>.None,
            declined: new UiFault.Released(Key: op))
        is Transition<MountState>.Committed
        ? HostThread.Release(
                releases: contents.Rev()
                    .Map(receipt => (Func<Fin<Unit>>)(() => receipt.Release()))
                    .Add(() => op.Catch(() => Fin.Succ(value: Op.Side(Host.Dispose)))),
                key: op)
            .IfFail(failure => ignore(faults.Park(item: failure)))
        : Fin.Succ(value: unit);

    public void Dispose() => ignore(Release());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PanelSections {
    private static readonly Rasm.Numerics.Dimension FaultCap = Rasm.Numerics.Dimension.Create(value: 64);

    public static Fin<PanelSectionMount> Mount(
        Seq<PanelSectionSpec> sections,
        CapabilitySet<PanelSectionHolderFeature> features,
        ElementRuntime runtime,
        Op? key = null) {
        Op op = key.OrDefault();
        return from _ in op.Accept<object>(runtime)
               from admitted in sections.TraverseM(section => op.Need(section)).As()
               from __ in guard(
                       flag: !admitted.IsEmpty
                           && admitted.Count(static section => section.Features.Admits(PanelSectionFeature.FullHeight)) <= 1,
                       False: op.InvalidInput())
                   .ToFin()
               from mounted in HostThread.Run(
                   work: new HostWork<PanelSectionMount>.Execute(
                       Body: () => Seat(sections: admitted.Strict(), features: features, runtime: runtime, op: op)),
                   key: op)
               select mounted;
    }

    private static Fin<PanelSectionMount> Seat(
        Seq<PanelSectionSpec> sections,
        CapabilitySet<PanelSectionHolderFeature> features,
        ElementRuntime runtime,
        Op op) =>
        sections
            .Fold(Fin.Succ(Seq<ElementReceipt>()), (held, section) => held.Bind(grown => ControlForge
                .Grow(spec: section.Body, runtime: runtime, key: op)
                .Match(
                    Succ: receipt => Fin.Succ(grown.Add(receipt)),
                    Fail: fault => HostThread.Release(
                        releases: grown.Rev().Map(receipt => (Func<Fin<Unit>>)(() => receipt.Release())),
                        key: op).Match(
                            Succ: _ => Fin.Fail<Seq<ElementReceipt>>(error: fault),
                            Fail: cleanup => Fin.Fail<Seq<ElementReceipt>>(error: fault + cleanup)))))
            .Bind(contents => Held(sections: sections, contents: contents.Strict(), features: features, op: op));

    private static Fin<PanelSectionMount> Held(
        Seq<PanelSectionSpec> sections,
        Seq<ElementReceipt> contents,
        CapabilitySet<PanelSectionHolderFeature> features,
        Op op) {
        EtoCollapsibleSectionHolder2? holder = null;
        Ring<Error> faults = new(cap: FaultCap);
        return op.Catch(() => {
            EtoCollapsibleSectionHolder2 owned = holder = new() {
                UseScrollbars = features.Admits(PanelSectionHolderFeature.Scrollbars),
                UseCheckBoxes = features.Admits(PanelSectionHolderFeature.Checkboxes),
            };
            _ = sections.Zip(contents).Iter(pair => {
                PanelSectionLeaf leaf = new(
                    spec: pair.Item1,
                    content: pair.Item2.Host,
                    report: failure => ignore(faults.Park(item: failure)),
                    op: op);
                owned.Add(section: leaf);
                _ = Op.SideWhen(
                    pair.Item1.Features.Admits(PanelSectionFeature.FullHeight),
                    () => owned.SetFullHeightSection(sec: leaf));
            });
            return Fin.Succ(value: new PanelSectionMount(host: owned, contents: contents, faults: faults, op: op));
        }).MapFail(fault => HostThread.Release(
            releases: contents.Rev()
                .Map(receipt => (Func<Fin<Unit>>)(() => receipt.Release()))
                .Add(() => op.Catch(() => Fin.Succ(value: Op.SideWhen(holder is not null, () => holder!.Dispose())))),
            key: op).Match(
                Succ: _ => fault,
                Fail: cleanup => fault + cleanup));
    }
}
```

## [08]-[HOST_CONTROLS]

- Owner: `HostControl` closes the consumable `Rhino.UI.Controls` widget library as exact-payload cases; `HostCommandRow` is the one command-bearing button row; `RhinoPad`, `RhinoSpace`, and `RhinoWidth` key the three host layout vocabularies; `UnitPulse`, `UnitSpan`, and `UnitFormat` carry the unit-aware entry; `ThemePalette` detaches and feeds host theme swatches; `UiServices` is the platform-service seam.
- Entry: `HostControl.ToSpec` admits every nested payload and answers a kernel `ControlSpec.Custom`, so realization, receipts, styling, and teardown stay the control owner's.
- Auto: a mint answers the kernel MINT carrier, so the buttons a row builds inside itself and the children a grid holds are child mints the receipt drains in reverse order — the two hand cleanup towers that once drained them are gone with their own failure paths.
- Auto: `GridWrap` is the family's one nested case and its children are `HostControl` rows minted through the same dispatch, so the wrapping grid composes the family it belongs to rather than a parallel container surface.
- Law: the padding, spacing, and width vocabularies are ROWS the host publishes and every widget that takes one reads its row — a pixel literal or a raw host flag never reaches a call site, and a roster nothing reads would be decorative.
- Law: the command row carries BOTH tooltips and its second one selects the host overload: a row with one tip mints the plain image button and a row with two mints the dual-tooltip one, which is a widget the library publishes and this family had no shape for.
- Law: unit-entry update modes are a capability SET whose ONE illegal corner is the empty one, because the host flag word carries no zero member and an empty set would mint a control that never reports a value. The law is stated on the roster and refused at admission rather than seeded away inside a fold.
- Law: text-area access is the kernel EDIT capability set, so read-only is the absence of the editable row rather than a boundary-local access vocabulary — the same set every other text surface in the estate reads.
- Law: colour payloads enter as `PerceptualColor` and quantize once at the mint arm through the paint correspondence; the host theme tree is read-only, so a consumer detaches swatches and never authors a zone.
- Law: the theme feed's role map is the positive ALLOW-LIST — every declared role must resolve to a zone swatch, and an unresolved role fails the feed with the missing paths as typed evidence rather than seating a partial grid.
- Law: `ThemePalette.Detach` and `UiServices.Resolve` cross the command thread like every other entry on this page, and `Feed` inherits the crossing through `Detach` rather than opening a second one.
- Boundary: the parent-coupled host slider and the document-bound linetype grid stay behind their own document-scoped owners; the host dialog bases ride the shell presenter, and native pointer handles never cross this family.
- Receipt: `ControlMint` per case and `ThemeChange` per feed — both settled values the kernel owners already publish.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui-controls.md` (`NumericUpDownWithUnitParsing`, `RichTextAreaWithAlternateText`, `ImageButton`, `ImageToolTipButton`, `AddRemoveButton`, `RhinoButtonRow`, `ControlGridLayout`, `Divider`, `LabelSeparator`, `StaticAlignedLabel`, `DisplayAndPrintColorPicker`, `ViewportControl`, `RhinoLayout` padding/spacing/width/label factories, `NumericUpDownWithUnitParsingUpdateMode`, `DistanceDisplayMode`, `GridWrapMode`, `DisplayAndPrintColorPickerMode`); `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`Theme.ThemeZone`, `RhinoUiServiceLocator`, `PlatformServiceProvider`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `HashMap`); `Rasm/Interaction` (`ControlSpec`, `ControlMint`, `ElementSpec`, `ElementRuntime`, `EditTrait`, `IntentTable.Verb`, `IntentKey`, `PaintColor`, `ThemeSeam`, `ThemeShift`, `ThemeVariant`, `PaletteRole`, `ThemeChange`, `UiFault`); `Rasm/Domain` (`Op`, `Lease<T>`, `ICapability`, `CapabilitySet`, `CapabilityLaw`); `Rasm/Numerics` (`PerceptualColor`, `Dimension`, `PositiveMagnitude`).
- Growth: a new Rhino widget is one `HostControl` case and one mint arm; a new layout row is one entry on its own vocabulary; a new update mode is one `UnitPulse` row the mask fold already reads.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<RhinoLayout.PaddingType>]
public sealed partial class RhinoPad {
    public static readonly RhinoPad None = new(key: RhinoLayout.PaddingType.None);
    public static readonly RhinoPad Dialog = new(key: RhinoLayout.PaddingType.Dialog);
    public static readonly RhinoPad Indented = new(key: RhinoLayout.PaddingType.Indented);
    public static readonly RhinoPad Panel = new(key: RhinoLayout.PaddingType.RhinoPanel);
    public static readonly RhinoPad PropertiesPage = new(key: RhinoLayout.PaddingType.RhinoPropertiesPage);
    public static readonly RhinoPad ButtonRow = new(key: RhinoLayout.PaddingType.ButtonRow);
    public static readonly RhinoPad Table = new(key: RhinoLayout.PaddingType.Table);

    internal Padding Resolve() => RhinoLayout.Padding(paddingType: Key);
}

[SmartEnum<RhinoLayout.SpacingType>]
public sealed partial class RhinoSpace {
    public static readonly RhinoSpace Dialog = new(key: RhinoLayout.SpacingType.Dialog);
    public static readonly RhinoSpace Panel = new(key: RhinoLayout.SpacingType.Panel);
    public static readonly RhinoSpace PropertiesPage = new(key: RhinoLayout.SpacingType.PropertiesPage);
    public static readonly RhinoSpace ButtonRow = new(key: RhinoLayout.SpacingType.ButtonRow);
    public static readonly RhinoSpace Table = new(key: RhinoLayout.SpacingType.Table);

    internal Size Resolve() => RhinoLayout.Spacing(spacingType: Key);

    internal int Stacked(Orientation axis) => RhinoLayout.StackedSpacing(orientation: axis, spacingType: Key);
}

[SmartEnum<RhinoLayout.WidthControlType>]
public sealed partial class RhinoWidth {
    public static readonly RhinoWidth Numeric = new(key: RhinoLayout.WidthControlType.Numeric);
    public static readonly RhinoWidth Magnitude = new(key: RhinoLayout.WidthControlType.OrderOfMagnitude);
    public static readonly RhinoWidth Text = new(key: RhinoLayout.WidthControlType.Text);
    public static readonly RhinoWidth Automatic = new(key: RhinoLayout.WidthControlType.AutoSize);

    internal int Resolve() => RhinoLayout.FixedWidth(widthControlType: Key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UnitPulse : ICapability<UnitPulse> {
    public static readonly UnitPulse OnValueChange = new(
        key: "on-value-change", flag: NumericUpDownWithUnitParsingUpdateMode.OnValueChange);
    public static readonly UnitPulse OnEnterOrLoseFocus = new(
        key: "on-enter-or-lose-focus", flag: NumericUpDownWithUnitParsingUpdateMode.OnEnterOrLoseFocus);
    public static readonly UnitPulse WhenDoneChanging = new(
        key: "when-done-changing", flag: NumericUpDownWithUnitParsingUpdateMode.WhenDoneChanging);

    internal NumericUpDownWithUnitParsingUpdateMode Flag { get; }

    public static CapabilityLaw<UnitPulse> Law =>
        CapabilityLaw<UnitPulse>.Forbidden(Seq(CapabilitySet<UnitPulse>.None));

    internal static NumericUpDownWithUnitParsingUpdateMode Fold(CapabilitySet<UnitPulse> pulses) =>
        (NumericUpDownWithUnitParsingUpdateMode)pulses.Mask(static row => (int)row.Flag);
}

[SmartEnum<bool>]
public sealed partial class GridStretch {
    public static readonly GridStretch Free = new(false);
    public static readonly GridStretch ToWidth = new(true);
}

[SmartEnum<bool>]
public sealed partial class ColourLink {
    public static readonly ColourLink Independent = new(false);
    public static readonly ColourLink Linked = new(true);
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class UnitSpan {
    public double Value { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public PositiveMagnitude Increment { get; }
    public Rasm.Numerics.Dimension Decimals { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double value,
        ref double minimum,
        ref double maximum,
        ref PositiveMagnitude increment,
        ref Rasm.Numerics.Dimension decimals) =>
        validationError = double.IsFinite(value) && double.IsFinite(minimum) && double.IsFinite(maximum)
            && minimum <= value && value <= maximum
            ? null
            : new ValidationError(message: "Unit entry span is not an ordered finite range around its value.");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnitFormat {
    private UnitFormat() { }
    public sealed record Model(UnitSystem Units, DistanceDisplayMode Display) : UnitFormat;
    public sealed record Length(LengthUnit Units, DistanceDisplayMode Display) : UnitFormat;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        model: static (held, row) => guard(
            flag: Enum.IsDefined(row.Units) && Enum.IsDefined(row.Display), False: held.InvalidInput()).ToFin(),
        length: static (held, row) => guard(
            flag: Enum.IsDefined(row.Units) && Enum.IsDefined(row.Display), False: held.InvalidInput()).ToFin());

    internal Unit Apply(NumericUpDownWithUnitParsing control) => Switch(
        control,
        model: static (held, row) => Op.Side(() => held.SetFormatUnitSystem(row.Units, row.Display)),
        length: static (held, row) => Op.Side(() => held.SetFormatLengthUnits(row.Units, row.Display)));
}

public sealed record HostCommandRow(
    Image Face,
    Option<Image> Disabled,
    Option<HostText> Tip,
    Option<HostText> AltTip,
    IntentKey Intent) {
    internal Fin<Unit> Admit(Op op) =>
        from _ in op.Need(Face)
        from __ in op.AcceptValidated<IntentKey>(candidate: Intent.Value)
        select unit;
}

public sealed record ThemeSwatch(string Path, PerceptualColor Value);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostControl {
    private HostControl() { }
    public sealed record UnitEntry(
        UnitSpan Span,
        UnitFormat Format,
        CapabilitySet<UnitPulse> Pulses,
        RhinoWidth Width,
        Option<HostText> Prefix,
        Option<HostText> Suffix) : HostControl;
    public sealed record RichAlternate(CapabilitySet<EditTrait> Traits, Option<HostText> Alternate) : HostControl;
    public sealed record Command(HostCommandRow Row) : HostControl;
    public sealed record AddRemove(IntentKey Add, IntentKey Remove) : HostControl;
    public sealed record ActionRow(Seq<HostCommandRow> Rows, RhinoSpace Gap) : HostControl;
    public sealed record GridWrap(
        Seq<HostControl> Items,
        GridWrapMode Direction,
        Size ItemSize,
        GridStretch Stretch,
        RhinoPad Pad) : HostControl;
    public sealed record LabelRow(HostText Caption, HostControl Field, RhinoSpace Gap) : HostControl;
    public sealed record DividerLine(Option<PerceptualColor> Colour) : HostControl;
    public sealed record CaptionRule(HostText Caption) : HostControl;
    public sealed record PinnedLabel(HostText Text, TextAlignment Alignment) : HostControl;
    public sealed record OutputColour(
        DisplayAndPrintColorPickerMode Mode,
        PerceptualColor Display,
        PerceptualColor Print,
        ColourLink Link) : HostControl;
    public sealed record ViewportView(Option<HostText> Title) : HostControl;

    public Fin<ControlSpec> ToSpec(ElementSpec spec, ElementRuntime runtime, Op? key = null) {
        Op op = key.OrDefault();
        HostControl control = this;
        return op.Accept<object>(spec, runtime)
            .Bind(_ => control.Admit(op))
            .Map<ControlSpec>(_ => new ControlSpec.Custom(Spec: spec, Mint: () => control.Mint(runtime: runtime, op: op)));
    }

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        unitEntry: static (held, row) =>
            from _ in UnitPulse.Law.Admit(held: row.Pulses)
            from __ in row.Format.Admit(held)
            select unit,
        richAlternate: static (held, row) => EditTrait.Law.Admit(held: row.Traits).Map(static _ => unit),
        command: static (held, row) => row.Row.Admit(held),
        addRemove: static (held, row) =>
            from add in held.AcceptValidated<IntentKey>(candidate: row.Add.Value)
            from remove in held.AcceptValidated<IntentKey>(candidate: row.Remove.Value)
            from _ in guard(flag: add != remove, False: held.InvalidInput()).ToFin()
            select unit,
        actionRow: static (held, row) =>
            from _ in guard(flag: !row.Rows.IsEmpty, False: held.InvalidInput()).ToFin()
            from __ in row.Rows.TraverseM(action => held.Need(action).Bind(value => value.Admit(held))).As()
            select unit,
        gridWrap: static (held, row) =>
            from _ in guard(
                    flag: !row.Items.IsEmpty && Enum.IsDefined(row.Direction)
                        && row.ItemSize.Width > 0 && row.ItemSize.Height > 0,
                    False: held.InvalidInput())
                .ToFin()
            from __ in row.Items.TraverseM(item => held.Need(item).Bind(value => value.Admit(held))).As()
            select unit,
        labelRow: static (held, row) => held.Need(row.Field).Bind(field => field.Admit(held)),
        dividerLine: static (_, _) => Fin.Succ(value: unit),
        captionRule: static (_, _) => Fin.Succ(value: unit),
        pinnedLabel: static (held, row) => guard(flag: Enum.IsDefined(row.Alignment), False: held.InvalidInput()).ToFin(),
        outputColour: static (held, row) => guard(flag: Enum.IsDefined(row.Mode), False: held.InvalidInput()).ToFin(),
        viewportView: static (_, _) => Fin.Succ(value: unit));

    internal Fin<ControlMint> Mint(ElementRuntime runtime, Op op) => Switch(
        (Runtime: runtime, Op: op),
        unitEntry: static (held, row) => held.Op.Catch(() => {
            NumericUpDownWithUnitParsing stepper = new(showStepper: true) {
                MinValue = row.Span.Minimum,
                MaxValue = row.Span.Maximum,
                Increment = row.Span.Increment.Value,
                DecimalPlaces = row.Span.Decimals.Value,
                Value = row.Span.Value,
                ValueUpdateMode = UnitPulse.Fold(pulses: row.Pulses),
                Width = row.Width.Resolve(),
            };
            _ = row.Format.Apply(stepper);
            _ = row.Prefix.Iter(text => stepper.Prefix = text.Resolve());
            _ = row.Suffix.Iter(text => stepper.Suffix = text.Resolve());
            return Fin.Succ(value: ControlMint.Editor(
                host: stepper,
                pick: () => Fin.Succ<FieldValue>(value: new FieldValue.Number(Value: stepper.Value))));
        }),
        richAlternate: static (held, row) => held.Op.Catch(() => {
            RichTextAreaWithAlternateText rich = new() { ReadOnly = !row.Traits.Admits(EditTrait.Editable) };
            _ = row.Alternate.Iter(text => Op.Side(() => {
                rich.AlternateText = text.Resolve();
                rich.ShowAlternateText = true;
            }));
            return Fin.Succ(value: ControlMint.Editor(
                host: rich,
                pick: () => Fin.Succ<FieldValue>(value: new FieldValue.Markup(Rtf: rich.Text))));
        }),
        command: static (held, row) => Button(row: row.Row, runtime: held.Runtime, op: held.Op)
            .Map(ControlMint.Leaf),
        addRemove: static (held, row) =>
            from add in held.Runtime.Intents.Verb(row.Add, held.Op)
            from remove in held.Runtime.Intents.Verb(row.Remove, held.Op)
            from control in held.Op.Catch(() => Fin.Succ(value: ControlMint.Leaf(
                host: new AddRemoveButton { AddCommand = add, RemoveCommand = remove })))
            select control,
        actionRow: static (held, row) => held.Op
            .Catch(() => Fin.Succ(value: new RhinoButtonRow {
                Spacing = row.Gap.Stacked(axis: Orientation.Horizontal),
            }))
            .Bind(bar => row.Rows
                .TraverseM(entry => Button(row: entry, runtime: held.Runtime, op: held.Op)
                    .Bind(button => held.Op.Catch(() => Fin.Succ(
                        value: (Op.Side(() => bar.AddButton(button)), ControlMint.Leaf(host: button)).Item2))))
                .As()
                .Map(children => ControlMint.Leaf(host: bar) with { Children = children.Strict() })),
        gridWrap: static (held, row) => row.Items
            .TraverseM(item => item.Mint(runtime: held.Runtime, op: held.Op))
            .As()
            .Bind(children => held.Op.Catch(() => {
                ControlGridLayout grid = new() {
                    GridWrapMode = row.Direction,
                    ItemSize = row.ItemSize,
                    ItemPadding = row.Pad.Resolve(),
                    StretchItemsToWidth = row.Stretch.Key,
                };
                _ = children.Iter(child => Op.Side(() => grid.Items.Add(child.Host.Resource)));
                return Fin.Succ(value: ControlMint.Leaf(grid) with { Children = children.Strict() });
            })),
        labelRow: static (held, row) => row.Field.Mint(runtime: held.Runtime, op: held.Op).Bind(field => held.Op
            .Catch(() => Fin.Succ(value: ControlMint.Leaf(host: RhinoLayout.LabelTableLayout(
                    row.Caption.Resolve(), field.Host.Resource, true, row.Gap.Key))
                with { Children = Seq(field) }))),
        dividerLine: static (held, row) => row.Colour
            .Map(colour => colour.ToEto())
            .Sequence()
            .Bind(ink => held.Op.Catch(() => {
                Divider line = new();
                _ = ink.Iter(colour => line.Color = colour);
                return Fin.Succ(value: ControlMint.Leaf(host: line));
            })),
        captionRule: static (held, row) => held.Op.Catch(() => Fin.Succ(value: ControlMint.Leaf(
            host: new LabelSeparator { Text = row.Caption.Resolve() }))),
        pinnedLabel: static (held, row) => held.Op.Catch(() => Fin.Succ(value: ControlMint.Leaf(
            host: new StaticAlignedLabel(row.Alignment) { Text = row.Text.Resolve() }))),
        outputColour: static (held, row) =>
            from display in row.Display.ToEto()
            from print in row.Print.ToEto()
            from picker in held.Op.Catch(() => Fin.Succ(value: new DisplayAndPrintColorPicker {
                PickerMode = row.Mode,
                LinkPrintToDisplay = row.Link.Key,
                DisplayColor = display,
                PrintColor = print,
            }))
            select ControlMint.Editor(
                host: picker,
                pick: () => PaintColor.OfHost(host: picker.DisplayColor, key: held.Op)
                    .Map<FieldValue>(static value => new FieldValue.Colour(Value: value))),
        viewportView: static (held, row) => held.Op.Catch(() => Fin.Succ(value: ControlMint.Leaf(host: row.Title.Match(
            Some: static title => new ViewportControl(viewportTitle: title.Resolve()),
            None: static () => new ViewportControl())))));

    private static Fin<ImageButton> Button(HostCommandRow row, ElementRuntime runtime, Op op) =>
        runtime.Intents.Verb(row.Intent, op).Bind(command => op.Catch(() => {
            ImageButton button = row.AltTip.Match(
                Some: alternate => new ImageToolTipButton {
                    ToolTip = row.Tip.Map(static tip => tip.Resolve()).IfNone(string.Empty),
                    RightToolTip = alternate.Resolve(),
                },
                None: () => new ImageButton());
            button.Image = row.Face;
            button.Command = command;
            _ = row.Disabled.Iter(image => button.DisabledImage = image);
            _ = row.AltTip.IsNone ? row.Tip.Iter(tip => button.ToolTip = tip.Resolve()) : unit;
            return Fin.Succ(value: button);
        }));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ThemePalette {
    public static Fin<Seq<ThemeSwatch>> Detach(ThemeZone zone, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(zone).Bind(_ => HostThread.Run(
            work: new HostWork<Seq<ThemeSwatch>>.Execute(Body: () => toSeq(zone.Enumerate())
                .Choose(static entry => entry.Value is Color colour ? Some((Entry: entry, Colour: colour)) : None)
                .TraverseM(row => PaintColor.OfHost(host: row.Colour, key: op)
                    .Map(colour => new ThemeSwatch(Path: $"{zone.Id}/{row.Entry.Id}", Value: colour)))
                .As()
                .Map(static swatches => swatches.Strict())),
            key: op));
    }

    public static Fin<ThemeChange> Feed(
        ThemeZone zone,
        ThemeSeam seam,
        ThemeVariant variant,
        HashMap<string, PaletteRole> roles,
        Op? key = null) {
        Op op = key.OrDefault();
        return op.Accept<object>(seam, variant).Bind(_ => Detach(zone, op)).Bind(swatches => {
            HashMap<string, PerceptualColor> found = toHashMap(swatches.Map(static swatch => (swatch.Path, swatch.Value)));
            Seq<string> missing = toSeq(roles.AsIterable())
                .Filter(row => found.Find(row.Key).IsNone)
                .Map(static row => row.Key)
                .Strict();
            return missing.IsEmpty
                ? seam.Change(
                    shift: new ThemeShift.Hosted(
                        Variant: variant,
                        Cells: toHashMap(toSeq(roles.AsIterable())
                            .Choose(row => found.Find(row.Key).Map(value => (row.Value, value))))),
                    key: op)
                : Fin.Fail<ThemeChange>(error: op.InvalidResult(detail: string.Join(",", missing)));
        });
    }
}

public static class UiServices {
    public static Fin<TService> Resolve<TService>(Op? key = null) where TService : class {
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<TService>.Execute(Body: () => op.Catch(() =>
                (Optional(RhinoUiServiceLocator.GetService<TService>()) | Optional(PlatformServiceProvider.Service as TService))
                    .ToFin(Fail: new UiFault.HostRejected(
                        Key: op, Detail: $"no {typeof(TService).Name} is registered on this host")))),
            key: op);
    }
}
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
