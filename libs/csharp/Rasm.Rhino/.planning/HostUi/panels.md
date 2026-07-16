# [RASM_RHINO_HOSTUI_PANELS]

Panel-and-chrome owner of `Rasm.Rhino.HostUi` — one `HostPanel` base owning the `IPanel` lifecycle directly, one registration/placement/visibility surface over `Rhino.UI.Panels` with selected-tab and dock-bar evidence, one icon family driving both `RegisterPanel` overloads and `ChangePanelIcon`, one `RuiOp` fold over the whole `.rui` toolbar-file mutation surface (open/close/save/save-as, group visibility, sidebar state, global toolbar sizing) with a full typed snapshot, and the `RuiUpdateUi` menu-item registration seam. Census `Panel.cs` subscribed panel lifecycle through the Events sibling's watch bus and grew parallel placement, chrome, menu-sync, and RUI APIs; both defects die here: `HostPanel` receives `PanelShown`/`PanelHidden`/`PanelClosing` from the host and stamps one `PanelFact` stream this owner alone carries — no sibling bus, no `Rasm.Rhino.Events` reference — and every RUI mutation is a case of one union folded by one entry. Panel content is an Eto `Element` realized once and styled native; panel-adjacent Eto menus, toolbars, and popups are projections of the Eto chrome `IntentTable` (`MenuOf`/`BarOf`/`PopupOf`), so the census `UiChromeOp.EtoMenu`/`EtoToolbar`/`UiAction` parallel chrome builders have no successor on this page. Everything document-scoped enters as a `DocumentSession` capability demand; every host crossing rides `Op.Catch` and fails as a `UiFault` case.

## [01]-[INDEX]

- [02]-[PANEL_IDENTITY]: `PanelKey` + `PanelLife` + `PanelFact` — panel identity from the type's `GuidAttribute`, the lifecycle vocabulary with its host emit columns, and the one panel fact.
- [03]-[PANEL_SURFACE]: `HostPanel` — the Eto-content panel base implementing `IPanel` directly, routing every host callback into the fact stream and a per-instance override.
- [04]-[REGISTRATION_AND_PLACEMENT]: `PanelIcon` + `PanelPlacement` + `PanelPresence` + `PanelHost` — both registration overloads, open/sibling/float/close, visibility with selected-tab evidence, dock-bar state, icon change, document instances, the owned fact-stream watch, and the registry-wide host-event watch.
- [05]-[RUI_STATE]: `RuiFileRef` + `RuiOp` + `RuiSnapshot` + `Rui` + `RuiAddress`/`MenuLinks` — the toolbar-file mutation fold, the typed RUI census, and menu-item registration.
- [06]-[SECTIONS]: `SectionSpec` + `SectionLeaf` + `Sections.Mount` — the collapsible-section family over `Rhino.UI.Controls`, one spec realized to the internal host leaf and stacked into one holder-mounted control.

## [02]-[PANEL_IDENTITY]

- Owner: `PanelKey` — the `[ValueObject<Guid>]` panel identity, admitted from the panel type's `GuidAttribute` through one polymorphic `Of` (a `Type` or a `TPanel` type argument), rejecting the empty Guid — `PanelLife`, the lifecycle vocabulary whose rows carry the host emit column (`Panels.OnShowPanel` with the show polarity, `Panels.OnClosePanel`), and `PanelFact`, the one lifecycle evidence record: panel, life row, optional document key (a zero host serial is app scope, never a ghost key), the host `ShowPanelReason` where the host supplies one, and the closing-with-document polarity.
- Law: the emit column makes announcement and observation one vocabulary — `PanelHost.Announce` drives the same rows the host callbacks stamp, so a programmatic show/close is indistinguishable in evidence from a host-driven one, and the census kind-to-phase correspondence table has no successor because the row IS the correspondence.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Domain (`Op`), Document sub-domain (`DocKey`), Rhino.UI (`Panels.OnShowPanel`/`OnClosePanel`, `ShowPanelReason`).
- Growth (DOMAIN): a lifecycle axis the concept demands (a dock-transition life, an activation ordinal) is one `PanelLife` row plus one `PanelFact` slot; every observer gains it through the one stream.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Reflection;
using System.Runtime.InteropServices;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Eto;
using Rhino;
using Rhino.PlugIns;
using Rhino.UI;
using DrawingIcon = System.Drawing.Icon;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct PanelKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Panel identity is the empty Guid.") : null;

    public static Fin<PanelKey> Of<TPanel>(Op? key = null) where TPanel : HostPanel => Of(panelType: typeof(TPanel), key: key);

    public static Fin<PanelKey> Of(Type panelType, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Optional(panelType.GetCustomAttribute<GuidAttribute>())
            .ToFin(Fail: op.MissingContext())
            .Map(attribute => Create(value: Guid.Parse(input: attribute.Value))));
    }
}

[SmartEnum<int>]
public sealed partial class PanelLife {
    public static readonly PanelLife Shown = new(key: 0, emit: Announced(show: Some(true)));
    public static readonly PanelLife Hidden = new(key: 1, emit: Announced(show: Some(false)));
    public static readonly PanelLife Closing = new(key: 2, emit: Announced(show: None));

    public static PanelLife OfReason(ShowPanelReason reason) => Panels.IsShowing(reason: reason) ? Shown : Hidden;

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Emit(PanelKey panel, Option<DocKey> document, Op op);

    private static Func<PanelKey, Option<DocKey>, Op, Fin<Unit>> Announced(Option<bool> show) =>
        (panel, document, op) => op.Catch(() => {
            uint serial = document.Match(Some: static held => (uint)held, None: static () => 0u);
            _ = show.Match(
                Some: polarity => Op.Side(() => Panels.OnShowPanel(panelId: panel, documentSerialNumber: serial, show: polarity)),
                None: () => Op.Side(() => Panels.OnClosePanel(panelId: panel, documentSerialNumber: serial)));
            return Fin.Succ(value: unit);
        });
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct PanelFact(
    PanelKey Panel,
    PanelLife Life,
    Option<DocKey> Document,
    Option<ShowPanelReason> Reason = default,
    bool ClosingDocument = false);
```

## [03]-[PANEL_SURFACE]

- Owner: `HostPanel` — the one panel base: an Eto `Panel` implementing `IPanel` directly, realizing its `Element` content once at construction, styling itself native through `EtoExtensions.UseRhinoStyle`, and routing every host lifecycle callback into one private funnel that stamps the `PanelHost` fact stream and calls the per-instance `OnLife` override. A realization fault never detonates the host panel factory — the panel presents the fault message as its content and the fault is already on the construction `Op`'s evidence. Census `RasmPanel` phase enum, context struct, and watch-bus subscription have no successors: the fact IS the context, and observation is `PanelHost.Watch`.
- Law: panel chrome is composed, never minted — a panel wanting a context menu or a toolbar region declares Eto chrome `IntentTable` projections inside its `Element` tree, so panel verbs share availability sweeps and receipts with every other chrome placement.
- Law: `OnLife` runs on the host callback thread inside `Op.Catch`, so a throwing subclass reaction is a typed fault, never a crash inside the host's panel notification walk.
- Packages: LanguageExt.Core, Rasm.Domain (`Op`), Eto sub-domain (`Element`, `UiFault`), Eto.Forms (`Panel`, `Label`), Rhino.UI (`IPanel.PanelShown`/`PanelHidden`/`PanelClosing`, `EtoExtensions.UseRhinoStyle`).
- Growth (CONSUMER): a per-panel capability future plugins require (persisted panel state, a busy overlay) is one `HostPanel` member or one `ElementSpec` axis on the content, never a second panel base.

```csharp signature
// --- [SERVICES] -----------------------------------------------------------------------------
public abstract class HostPanel : Panel, IPanel {
    private readonly Fin<PanelKey> identity;
    private readonly Op op;

    protected HostPanel(Element content, Op? key = null) {
        op = key.OrDefault();
        identity = PanelKey.Of(panelType: GetType(), key: op);
        _ = content.Realize(key: op).Match(
            Succ: control => Op.Side(() => { Content = control; EtoExtensions.UseRhinoStyle(this); }),
            Fail: fault => Op.Side(() => Content = new Label { Text = fault.Message }));
    }

    protected virtual Unit OnLife(PanelFact fact) => unit;

    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) =>
        Route(life: PanelLife.Shown, serial: documentSerialNumber, reason: Some(reason), closingDocument: false);

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) =>
        Route(life: PanelLife.Hidden, serial: documentSerialNumber, reason: Some(reason), closingDocument: false);

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) =>
        Route(life: PanelLife.Closing, serial: documentSerialNumber, reason: None, closingDocument: onCloseDocument);

    private void Route(PanelLife life, uint serial, Option<ShowPanelReason> reason, bool closingDocument) =>
        ignore(op.Catch(() => identity.Map(panel => {
            PanelFact fact = new(
                Panel: panel,
                Life: life,
                Document: serial is 0u ? None : Some(DocKey.Create(value: serial)),
                Reason: reason,
                ClosingDocument: closingDocument);
            _ = PanelHost.Stamp(fact: fact, op: op);
            return OnLife(fact: fact);
        })));
}
```

## [04]-[REGISTRATION_AND_PLACEMENT]

- Owner: `PanelHost` — the one operations surface over the `Panels` registry, generic on the panel type so identity, host `Type`, and registration stay one declaration. `PanelIcon` closes the three icon addressings (a live `System.Drawing.Icon`, an embedded resource with its assembly, a full disk path) and drives both `RegisterPanel` overloads plus `Rebadge` over both `ChangePanelIcon` overloads, the resource arm loading through the dialogs page's `HostResources` icon row and the raster size defaulting to the host `Panels.IconSizeInPixels`. `PanelPlacement` closes the four open shapes — docked, at a named dock bar, as a sibling tab, floating with its `FloatPanelMode` — and `Open` returns a fresh `PanelPresence` so every open carries selected-tab evidence. `Presence` probes visibility, selected-tab state, the panel's dock bars, and the registry-wide open-panel census; `Close` and `Instances` are document-session demands riding `HostThread.OnSession`; `Announce` drives the `PanelLife` emit rows; `Watch` subscribes the owned fact stream and `WatchAll` the registry-wide panel families through `DocumentStream.Observe`, each returning the Document `Subscription` capsule.
- Law: placement is the value, arity is the type — the panel type parameter is the discriminant every host call derives its `Type` and `Guid` from, so no entry takes a stringy id, a raw `Type`, or a parallel `Guid` the key already owns.
- Law: every registry crossing — registration, placement, probe, rebadge, dock-bar read — rides one `HostThread.On` marshal, and `Open` probes inside the same crossing its placement lands in, so presence evidence is read on the thread that mutated it.
- Law: the fact stream is the one observation surface with two admission edges — `Watch` observers ride every stamp the `IPanel` callbacks and `Announce` produce for panels this package owns, and the `Facts` census holds the latest fact per panel key (current state, never an unbounded log), while `WatchAll` composes `DocumentStream.Observe(Observation.Host(EventScope.AnyDocument, [PanelVisibility, PanelClosed], ...))` for host-wide observation across foreign panels — the Document stream owns the `Panels.Show`/`Panels.Closed` host wires, this owner only projects `EventPayload.Panel` onto the `PanelFact` shape without stamping the owned ledger so an owned panel never double-stamps; each observer call is trapped on its own `Op.Catch`, so one throwing observer never starves its siblings or the host callback. A per-panel event, a second bus, or an events-sibling subscription is the census breach this owner forecloses.
- Law: `PanelLife.OfReason` classifies a host `ShowPanelReason` through the host's own `Panels.IsShowing`/`IsHiding` polarity probes, so reason-to-life correspondence is host truth, never a local table.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Domain (`Op`), Document sub-domain (`DocumentSession`, `SessionNeed`, `DocumentStream`, `Observation`, `EventFamily.PanelVisibility`/`PanelClosed`, `EventScope`, `Delivery`, `ReceiptPolicy`, `Subscription`), Eto sub-domain (`UiFault`), dialogs page (`HostResources`, `ResourceKind`), shell page (`HostThread.On`/`OnSession`), Rhino.UI (`Panels.RegisterPanel` both overloads, `OpenPanel` both arities, `OpenPanelAsSibling`, `FloatPanel`, `ClosePanel`, `GetPanels<T>`, `IsPanelVisible`, `PanelDockBars`, `DockBarIdInUse`, `GetOpenPanelIds`, `ChangePanelIcon` both overloads, `IconSizeInPixels`, `Panels.IsShowing`/`IsHiding`, `Panels.FloatPanelMode`, `PanelType`), Rhino.PlugIns (`PlugIn`).
- Growth (HOST): a new placement shape or registry probe the host ships is one `PanelPlacement` case or one `PanelPresence` column read in `Probe`; a parallel placement API is the census regression.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelIcon {
    private PanelIcon() { }
    public sealed record Native(DrawingIcon Value) : PanelIcon;
    public sealed record Resource(string Name, Assembly Assembly) : PanelIcon;
    public sealed record AtPath(string Path) : PanelIcon;

    public static PanelIcon Embedded<TAnchor>(string name) => new Resource(Name: name, Assembly: typeof(TAnchor).Assembly);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelPlacement {
    private PanelPlacement() { }
    public sealed record Docked : PanelPlacement;
    public sealed record AtBar(Guid DockBar) : PanelPlacement;
    public sealed record Beside(PanelKey Sibling) : PanelPlacement;
    public sealed record Afloat(Panels.FloatPanelMode Mode) : PanelPlacement;

    public static readonly PanelPlacement Default = new Docked();
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PanelPresence(PanelKey Panel, bool Visible, bool SelectedTab, Seq<Guid> DockBars, Seq<Guid> OpenPanels);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PanelHost {
    private static readonly Atom<HashMap<PanelKey, PanelFact>> Ledger = Atom(HashMap<PanelKey, PanelFact>());
    private static readonly Atom<Seq<(long Id, Action<PanelFact> Observer)>> Watchers = Atom(Seq<(long, Action<PanelFact>)>());
    private static long watchSeq;

    public static HashMap<PanelKey, PanelFact> Facts => Ledger.Value;

    public static Fin<Unit> Register<TPanel>(PlugIn owner, string caption, PanelIcon icon, PanelType site, Op? key = null) where TPanel : HostPanel {
        Op op = key.OrDefault();
        return from title in op.AcceptText(value: caption)
               from _ in HostThread.On(body: () => icon.Switch(
                   state: (Owner: owner, Title: title, Site: site, Op: op),
                   native: static (held, badge) => held.Op.Catch(() => {
                       Panels.RegisterPanel(held.Owner, typeof(TPanel), held.Title, badge.Value, held.Site);
                       return Fin.Succ(value: unit);
                   }),
                   resource: static (held, badge) => held.Op.Catch(() => {
                       Panels.RegisterPanel(plugIn: held.Owner, type: typeof(TPanel), caption: held.Title,
                           iconAssembly: badge.Assembly, iconResourceId: badge.Name, panelType: held.Site);
                       return Fin.Succ(value: unit);
                   }),
                   atPath: static (held, badge) => held.Op.AcceptText(value: badge.Path).Bind(path => held.Op.Catch(() => {
                       Panels.RegisterPanel(held.Owner, typeof(TPanel), held.Title, new DrawingIcon(fileName: path), held.Site);
                       return Fin.Succ(value: unit);
                   }))), key: op)
               select unit;
    }

    public static Fin<PanelPresence> Open<TPanel>(Option<PanelPlacement> placement = default, bool selected = true, Op? key = null) where TPanel : HostPanel {
        Op op = key.OrDefault();
        return PanelKey.Of<TPanel>(key: op).Bind(panel => HostThread.On(
            body: () => placement.IfNone(PanelPlacement.Default).Switch(
                    state: (Panel: panel, Selected: selected, Op: op),
                    docked: static (held, _) => Fin.Succ(value: Op.Side(() => Panels.OpenPanel(typeof(TPanel), held.Selected))),
                    atBar: static (held, place) => Fin.Succ(value: Op.Side(() => ignore(Panels.OpenPanel(place.DockBar, typeof(TPanel), held.Selected)))),
                    beside: static (held, place) => held.Op.Confirm(success: Panels.OpenPanelAsSibling(held.Panel, place.Sibling, held.Selected)),
                    afloat: static (held, place) => held.Op.Confirm(success: Panels.FloatPanel(typeof(TPanel), place.Mode)))
                .Bind(_ => Probe<TPanel>(panel: panel, op: op)),
            key: op));
    }

    public static Fin<PanelPresence> Presence<TPanel>(Op? key = null) where TPanel : HostPanel {
        Op op = key.OrDefault();
        return PanelKey.Of<TPanel>(key: op).Bind(panel => HostThread.On(body: () => Probe<TPanel>(panel: panel, op: op), key: op));
    }

    public static Fin<Unit> Close<TPanel>(DocumentSession session, Op? key = null) where TPanel : HostPanel {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: document => Fin.Succ(value: Op.Side(() => Panels.ClosePanel(typeof(TPanel), document))),
            op: op,
            SessionNeed.Redraw);
    }

    public static Fin<Seq<TPanel>> Instances<TPanel>(DocumentSession session, Op? key = null) where TPanel : HostPanel {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: document => Fin.Succ(value: toSeq(Panels.GetPanels<TPanel>(document)).Strict()),
            op: op,
            SessionNeed.Read);
    }

    public static Fin<Unit> Rebadge<TPanel>(PanelIcon icon, Option<DrawingSize> rasterSize = default, Op? key = null) where TPanel : HostPanel {
        Op op = key.OrDefault();
        return HostThread.On(body: () => icon.Switch(
            state: (Size: rasterSize.IfNone(() => Panels.IconSizeInPixels), Op: op),
            native: static (held, badge) => held.Op.Catch(() => { Panels.ChangePanelIcon(typeof(TPanel), badge.Value); return Fin.Succ(value: unit); }),
            resource: static (held, badge) =>
                HostResources.Load<DrawingIcon>(kind: ResourceKind.Icon, resourceName: badge.Name, size: held.Size, assembly: badge.Assembly, key: held.Op)
                    .Bind(loaded => held.Op.Catch(() => { Panels.ChangePanelIcon(typeof(TPanel), loaded); return Fin.Succ(value: unit); })),
            atPath: static (held, badge) => held.Op.AcceptText(value: badge.Path)
                .Bind(path => held.Op.Catch(() => { Panels.ChangePanelIcon(typeof(TPanel), path); return Fin.Succ(value: unit); }))), key: op);
    }

    public static Fin<bool> DockBarInUse(Guid dockBar, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.On(body: () => Fin.Succ(value: Panels.DockBarIdInUse(dockBar)), key: op);
    }

    public static Fin<Unit> Announce(PanelFact fact, Op? key = null) {
        Op op = key.OrDefault();
        return fact.Life.Emit(panel: fact.Panel, document: fact.Document, op: op).Map(_ => Stamp(fact: fact, op: op));
    }

    public static Subscription Watch(Action<PanelFact> observer) {
        long id = Interlocked.Increment(location: ref watchSeq);
        _ = Watchers.Swap(held => held.Add((Id: id, Observer: observer)));
        return Subscription.Of(detach: () => ignore(Watchers.Swap(held => held.Filter(row => row.Id != id))));
    }

    public static Fin<Subscription> WatchAll(Action<PanelFact> observer, Op? key = null) {
        Op op = key.OrDefault();
        return DocumentStream.Observe(new Observation.Host(
                Scope: new EventScope.AnyDocument(),
                Families: Seq(EventFamily.PanelVisibility, EventFamily.PanelClosed),
                Delivery: new Delivery.Inline(Sink: fact => op.Catch(() => {
                    _ = fact.Payload is EventPayload.Panel panel
                        ? Op.Side(() => observer(new PanelFact(
                            Panel: PanelKey.Create(value: panel.PanelId),
                            Life: panel.State.Switch(
                                shown: static () => PanelLife.Shown, hidden: static () => PanelLife.Hidden, closed: static () => PanelLife.Closing),
                            Document: fact.Key)))
                        : unit;
                    return Fin.Succ(value: unit);
                })),
                Receipts: ReceiptPolicy.Operational))
            .Map(watch => Subscription.Of(detach: watch.Dispose));
    }

    internal static Unit Stamp(PanelFact fact, Op op) {
        _ = Ledger.Swap(held => held.AddOrUpdate(fact.Panel, fact));
        return ignore(Watchers.Value.Iter(row => ignore(op.Catch(() => {
            row.Observer(fact);
            return Fin.Succ(value: unit);
        }))));
    }

    private static Fin<PanelPresence> Probe<TPanel>(PanelKey panel, Op op) where TPanel : HostPanel =>
        op.Catch(() => Fin.Succ(value: new PanelPresence(
            Panel: panel,
            Visible: Panels.IsPanelVisible(typeof(TPanel), false),
            SelectedTab: Panels.IsPanelVisible(typeof(TPanel), true),
            DockBars: toSeq(Panels.PanelDockBars(panel)).Strict(),
            OpenPanels: toSeq(Panels.GetOpenPanelIds()).Strict())));
}
```

## [05]-[RUI_STATE]

- Owner: `RuiOp` — the one mutation fold over the `.rui` toolbar-file state: file open with optional save-after-open, close with its prompt polarity, save and save-as, group visibility, the two sidebar axes, the global toolbar bitmap/tab sizing, and batches as one traversal — with `RuiFileRef` closing the three file-addressing regimes (id, path, name with case policy) the host collection resolves, `RuiSnapshot` the full typed census (file facts, group facts, toolbar facts, sidebar flags, global sizes) every `Rui.Apply` returns, and `RuiAddress`/`MenuLinks` the `RuiUpdateUi.RegisterMenuItem` seam. Census code scattered this across seven `UiChromeOp` case families on a generic result parameter; one union, one fold, one snapshot replaces them.
- Law: every mutation answers with the whole census — the snapshot after the fold is the selected-tab-equivalent evidence for RUI state, so a consumer never issues a mutation then separately probes what it changed.
- Law: sizing writes gate on positive dimensions and land both axes in one arm — a partial `BarSize` with neither axis is a typed rejection, not a silent no-op.
- Law: menu-state synchronization is a delta, not a callback body — `MenuLinks.Register` seats the host `RuiUpdateUi.UpdateMenuItemEventHandler` once and applies the consumer's `MenuSync` value per invocation, writing only the declared axes (`Enabled`, `Checked`, `RadioChecked`, `Text`) onto the live `RuiUpdateUi`, and the host's `false` registration return is a typed refusal through `op.Confirm`.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Domain (`Op`, `op.Confirm`), shell page (`HostThread`), Rhino.UI (`ToolbarFileCollection.Open`/`FindByPath`/`FindByName`/`SidebarIsVisible`/`MruSidebarIsVisible`, `ToolbarFile.Id`/`Name`/`Path`/`GroupCount`/`ToolbarCount`/`GetGroup`/`GetToolbar`/`Save`/`SaveAs`/`Close`, `Toolbar.BitmapSize`/`TabSize`, `RuiUpdateUi.RegisterMenuItem`/`Enabled`/`Checked`/`RadioChecked`/`Text`, `RuiUpdateUi.UpdateMenuItemEventHandler`), RhinoCommon (`RhinoApp.ToolbarFiles`).
- Growth (HOST): a new RUI mutation the host ships is one `RuiOp` case breaking `Apply` at compile time; a new census axis is one snapshot fact column read in `Census`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuiFileRef {
    private RuiFileRef() { }
    public sealed record ById(Guid Id) : RuiFileRef;
    public sealed record ByPath(string Path) : RuiFileRef;
    public sealed record ByName(string Name, bool IgnoreCase = true) : RuiFileRef;

    internal Fin<ToolbarFile> Resolve(Op op) =>
        Switch(
            state: op,
            byId: static (op, address) => toSeq(RhinoApp.ToolbarFiles).Choose(Optional).Find(file => file.Id == address.Id)
                .ToFin(Fail: op.MissingContext()),
            byPath: static (op, address) => Optional(RhinoApp.ToolbarFiles.FindByPath(path: address.Path))
                .ToFin(Fail: op.MissingContext()),
            byName: static (op, address) => Optional(RhinoApp.ToolbarFiles.FindByName(name: address.Name, ignoreCase: address.IgnoreCase))
                .ToFin(Fail: op.MissingContext()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuiOp {
    private RuiOp() { }
    public sealed record OpenFile(string Path, bool SaveAfterOpen = false) : RuiOp;
    public sealed record CloseFile(RuiFileRef File, bool Prompt = false) : RuiOp;
    public sealed record SaveFile(RuiFileRef File) : RuiOp;
    public sealed record SaveFileAs(RuiFileRef File, string Target) : RuiOp;
    public sealed record GroupVisible(RuiFileRef File, Guid Group, bool Visible) : RuiOp;
    public sealed record Sidebar(bool Visible, bool Mru = false) : RuiOp;
    public sealed record BarSize(Option<DrawingSize> Bitmap = default, Option<DrawingSize> Tab = default) : RuiOp;
    public sealed record Batch(Seq<RuiOp> Ops) : RuiOp;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record RuiFileFact(Guid Id, string Name, string Path, int Groups, int Toolbars);

public sealed record RuiGroupFact(Guid File, Guid Group, string Name, bool Visible, bool Docked);

public sealed record RuiToolbarFact(Guid File, Guid Toolbar, string Name);

public sealed record RuiSnapshot(
    Seq<RuiFileFact> Files,
    Seq<RuiGroupFact> Groups,
    Seq<RuiToolbarFact> Toolbars,
    bool Sidebar,
    bool MruSidebar,
    DrawingSize Bitmap,
    DrawingSize Tab);

public readonly record struct RuiAddress(Guid File, Guid Menu, Guid Item);

public readonly record struct MenuSync(
    Option<bool> Enabled = default,
    Option<bool> Checked = default,
    Option<bool> Radio = default,
    Option<string> Caption = default);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Rui {
    public static Fin<RuiSnapshot> Apply(RuiOp request, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.On(body: () => Land(request: request, op: op).Bind(_ => Census(op: op)), key: op);
    }

    public static Fin<RuiSnapshot> Snapshot(Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.On(body: () => Census(op: op), key: op);
    }

    private static Fin<Unit> Land(RuiOp request, Op op) =>
        request.Switch(
            state: op,
            openFile: static (op, mutation) =>
                from path in op.AcceptText(value: mutation.Path)
                from file in op.Catch(() => Optional(RhinoApp.ToolbarFiles.Open(path: path)).ToFin(Fail: op.InvalidResult(detail: path)))
                from _ in mutation.SaveAfterOpen ? op.Confirm(success: file.Save()) : Fin.Succ(value: unit)
                select unit,
            closeFile: static (op, mutation) =>
                from file in mutation.File.Resolve(op: op)
                from _ in op.Confirm(success: file.Close(prompt: mutation.Prompt))
                select unit,
            saveFile: static (op, mutation) =>
                from file in mutation.File.Resolve(op: op)
                from _ in op.Confirm(success: file.Save())
                select unit,
            saveFileAs: static (op, mutation) =>
                from file in mutation.File.Resolve(op: op)
                from target in op.AcceptText(value: mutation.Target)
                from _ in op.Confirm(success: file.SaveAs(path: target))
                select unit,
            groupVisible: static (op, mutation) =>
                from file in mutation.File.Resolve(op: op)
                from found in Indexed(count: file.GroupCount, read: file.GetGroup)
                    .Find(candidate => candidate.Id == mutation.Group)
                    .ToFin(Fail: op.MissingContext())
                select Op.Side(() => found.Visible = mutation.Visible),
            sidebar: static (op, mutation) => op.Catch(() => Fin.Succ(value: mutation.Mru
                ? Op.Side(() => ToolbarFileCollection.MruSidebarIsVisible = mutation.Visible)
                : Op.Side(() => ToolbarFileCollection.SidebarIsVisible = mutation.Visible))),
            barSize: static (op, mutation) =>
                Seq<(Option<DrawingSize> Size, Action<DrawingSize> Set)>(
                    (Size: mutation.Bitmap, Set: static value => Toolbar.BitmapSize = value),
                    (Size: mutation.Tab, Set: static value => Toolbar.TabSize = value))
                .Choose(static row => row.Size.Filter(static size => size is { Width: > 0, Height: > 0 }).Map(size => (Size: size, row.Set))) switch {
                    Seq<(DrawingSize Size, Action<DrawingSize> Set)> rows when !rows.IsEmpty =>
                        Fin.Succ(value: ignore(rows.Iter(row => Op.Side(() => row.Set(row.Size))))),
                    _ => Fin.Fail<Unit>(error: op.InvalidInput()),
                },
            batch: static (op, mutation) => mutation.Ops.TraverseM(inner => Land(request: inner, op: op)).As().Map(static _ => unit));

    private static Fin<RuiSnapshot> Census(Op op) =>
        op.Catch(() => {
            Seq<ToolbarFile> files = toSeq(RhinoApp.ToolbarFiles).Choose(Optional).Strict();
            return Fin.Succ(value: new RuiSnapshot(
                Files: files.Map(static file => new RuiFileFact(Id: file.Id, Name: file.Name, Path: file.Path, Groups: file.GroupCount, Toolbars: file.ToolbarCount)).Strict(),
                Groups: files.Bind(static file => Indexed(count: file.GroupCount, read: file.GetGroup)
                    .Map(held => new RuiGroupFact(File: file.Id, Group: held.Id, Name: held.Name, Visible: held.Visible, Docked: held.IsDocked))).Strict(),
                Toolbars: files.Bind(static file => Indexed(count: file.ToolbarCount, read: file.GetToolbar)
                    .Map(held => new RuiToolbarFact(File: file.Id, Toolbar: held.Id, Name: held.Name))).Strict(),
                Sidebar: ToolbarFileCollection.SidebarIsVisible,
                MruSidebar: ToolbarFileCollection.MruSidebarIsVisible,
                Bitmap: Toolbar.BitmapSize,
                Tab: Toolbar.TabSize));
        });

    private static Seq<T> Indexed<T>(int count, Func<int, T?> read) where T : class =>
        toSeq(Enumerable.Range(start: 0, count: count)).Choose(index => Optional(read(index))).Strict();
}

public static class MenuLinks {
    public static Fin<Unit> Register(RuiAddress address, Func<RuiAddress, MenuSync> sync, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => op.Confirm(success: RuiUpdateUi.RegisterMenuItem(
            address.File, address.Menu, address.Item,
            (_, live) => ignore(op.Catch(() => {
                MenuSync state = sync(address);
                _ = state.Enabled.Iter(value => live.Enabled = value);
                _ = state.Checked.Iter(value => live.Checked = value);
                _ = state.Radio.Iter(value => live.RadioChecked = value);
                _ = state.Caption.Iter(value => live.Text = value);
                return Fin.Succ(value: unit);
            })))));
    }
}
```

## [06]-[SECTIONS]

- Owner: `SectionSpec` — the collapsible-section family over `Rhino.UI.Controls`: one spec per section (caption with optional localized column projected onto `LocalizeStringPair`, Eto `Element` body, section height, expanded/collapsible/hidden polarity, optional command-option caption pair) realized by the internal `SectionLeaf : EtoCollapsibleSection` owning every host override exactly once, and `Sections.Mount` stacking the leaves into one `EtoCollapsibleSectionHolder` with its scrollbar and checkbox axes. Census `RasmSection` factory carried caption, height, visibility, and command-option wiring; the spec absorbs each as a field consumed at the one mount site.
- Law: the holder is the one stack owner — `Add` lands each leaf in declaration order, and a consumer receives one `Control`, never the holder type, so holder capability grows behind the mount without consumer edits.
- Packages: LanguageExt.Core, Rasm.Domain (`Op`), Eto sub-domain (`Element`), Rhino.UI (`LocalizeStringPair`), Rhino.UI.Controls (`EtoCollapsibleSection.Caption`/`SectionHeight`/`Collapsible`/`Hidden`/`InitiallyExpanded`/`CommandOptionName`, `EtoCollapsibleSectionHolder.Add`/`UseScrollbars`/`UseCheckBoxes`).
- Growth (DOMAIN): a section axis the concept demands (a header accessory verb, per-section availability) is one `SectionSpec` field consumed by `SectionLeaf`; a holder axis is one `Mount` policy value.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record SectionSpec(
    string Caption,
    Element Body,
    int Height,
    bool Expanded = true,
    bool Collapsible = true,
    bool Hidden = false,
    Option<string> LocalCaption = default,
    Option<(string English, string Local)> CommandOption = default);

// --- [SERVICES] -----------------------------------------------------------------------------
internal sealed class SectionLeaf : EtoCollapsibleSection {
    private readonly SectionSpec spec;

    internal SectionLeaf(SectionSpec spec, Control content) {
        this.spec = spec;
        Content = content;
    }

    public override LocalizeStringPair Caption => new(spec.Caption, spec.LocalCaption.IfNone(spec.Caption));
    public override int SectionHeight => spec.Height;
    public override bool Collapsible => spec.Collapsible;
    public override bool Hidden => spec.Hidden;
    public override bool InitiallyExpanded => spec.Expanded;
    public override LocalizeStringPair CommandOptionName =>
        spec.CommandOption.Match(
            Some: static option => new LocalizeStringPair(option.English, option.Local),
            None: static () => new LocalizeStringPair(string.Empty, string.Empty));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Sections {
    public static Fin<Control> Mount(Seq<SectionSpec> sections, bool scrollbars = true, bool checkboxes = false, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(flag: !sections.IsEmpty, False: op.InvalidInput()).ToFin()
               from leaves in sections
                   .TraverseM(spec => spec.Body.Realize(key: op).Map(content => new SectionLeaf(spec: spec, content: content)))
                   .As()
               from mounted in op.Catch(() => {
                   EtoCollapsibleSectionHolder holder = new() { UseScrollbars = scrollbars, UseCheckBoxes = checkboxes };
                   _ = leaves.Iter(leaf => holder.Add(section: leaf));
                   return Fin.Succ(value: (Control)holder);
               })
               select mounted;
    }
}
```
