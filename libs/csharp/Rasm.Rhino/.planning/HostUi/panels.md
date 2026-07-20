# [RASM_RHINO_HOSTUI_PANELS]

`PanelHost` owns Rhino panel registration, placement, per-document instances, visibility, icon, lifecycle evidence, and dock-bar facts through one typed request family. `Rui` owns toolbar-file mutation and census through one command fold, while `Sections` realizes collapsible host sections — lifecycle hooks, refresh, and full-height rows included — from capability sets, and `HostControl` closes the consumable Rhino control library as exact-payload cases entering the Eto realize fold; all enter the Rhino command thread through `HostThread.Run` and return detached receipts.

## [01]-[INDEX]

- [02]-[PANEL_MODEL]: `PanelKey`, `PanelChange`, `PanelFact`, and `HostPanel` close identity, lifecycle, content, and callback delivery.
- [03]-[PANEL_HOST]: `PanelOp<TPanel>` and `PanelReceipt<TPanel>` own registration, placement, query, close, instance, icon, and dock-bar modalities.
- [04]-[PANEL_OBSERVATION]: `PanelObserve` folds owned and registry-wide lifecycle observation into one subscription entry.
- [05]-[RUI]: `RuiCommand` folds toolbar-file state changes, while `RuiReceipt` carries the full snapshot and any applied-prefix fault.
- [06]-[MENU_LINKS]: `MenuDelta` carries menu update state as cases over one registered host callback.
- [07]-[SECTIONS]: `SectionSpec`, `SectionSignal`, capability sets, and `SectionMount` realize ordered collapsible sections with lifecycle routing and complete content lifetime.
- [08]-[HOST_CONTROLS]: `HostControl`, the `RhinoPad`/`RhinoSpace` layout vocabularies, `ThemePalette`, and `UiServices` close the Rhino control library, theme read, and platform-service seams.

## [02]-[PANEL_MODEL]

- Owner: `PanelKey` admits the panel type's `Guid` once; every registry call derives `Type` and identity from `TPanel`.
- Owner: `PanelChange` closes shown, hidden, unclassified, panel-closing, and document-closing evidence without a boolean payload.
- Receipt: `PanelFact` carries panel, optional document, change, and monotonic ordinal.
- Owner: `HostPanel` is the abstract implement seam over the foreign `Panel` and `IPanel` bases; it realizes Eto content once and routes every host callback through `PanelHost.Stamp`.
- Law: an identity refusal, an `OnLife` throw, and an `OnLife` failure all land in `HostPanel.Faults` — durable typed evidence that never re-enters the host callback.
- Boundary: `Construction` retains `Fin<ElementReceipt>` so realization failure and control-tree lifetime remain typed even when the host requires a constructed panel instance.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Eto;
using Rhino;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Runtime;
using Rhino.UI.Theme;
using DrawingIcon = System.Drawing.Icon;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct PanelKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Panel identity is empty.") : null;

    public static Fin<PanelKey> Of(Type panelType, Op? key = null) {
        ArgumentNullException.ThrowIfNull(panelType);
        return Of(value: panelType.GUID, key: key);
    }

    public static Fin<PanelKey> Of(Guid value, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Validate(value: value, provider: null, out PanelKey? panel) is null && panel is { } admitted
            ? Fin.Succ(value: admitted)
            : Fin.Fail<PanelKey>(error: op.InvalidInput()));
    }

    internal Fin<Unit> Admit(Op op) {
        ValidationError? fault = Validate(value: this, provider: null, out PanelKey? admitted);
        return op.AcceptValidated<PanelKey>(fault: fault, admitted: admitted).Map(static _ => unit);
    }
}

[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct DockBarKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Dock-bar identity is empty.") : null;

    public static Fin<DockBarKey> Of(Guid value, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Validate(value: value, provider: null, out DockBarKey? dockBar) is null && dockBar is { } admitted
            ? Fin.Succ(value: admitted)
            : Fin.Fail<DockBarKey>(error: op.InvalidInput()));
    }

    internal Fin<Unit> Admit(Op op) {
        ValidationError? fault = Validate(value: this, provider: null, out DockBarKey? admitted);
        return op.AcceptValidated<DockBarKey>(fault: fault, admitted: admitted).Map(static _ => unit);
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

public sealed record PanelFact(PanelKey Panel, Option<DocKey> Document, PanelChange Change, long Ordinal);

// --- [SERVICES] -----------------------------------------------------------------------------
public abstract class HostPanel : Panel, IPanel {
    private readonly Fin<PanelKey> identity;
    private readonly Op op;
    private readonly Option<Control> fallback;
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private int released;

    protected HostPanel(Element content, ElementRuntime runtime, Op? key = null) {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(runtime);
        op = key.OrDefault();
        identity = PanelKey.Of(panelType: GetType(), key: op);
        Construction = content.Realize(runtime: runtime, key: op);
        Control? rejected = null;
        Content = Construction.Match<Control>(
            Succ: receipt => {
                EtoExtensions.UseRhinoStyle(receipt.Host);
                return receipt.Host;
            },
            Fail: fault => rejected = new Label { Text = fault.Message });
        fallback = Optional(rejected);
    }

    public Fin<ElementReceipt> Construction { get; }

    public Seq<Error> Faults => faults.Value;

    protected virtual Fin<Unit> OnLife(PanelFact fact) => Fin.Succ(value: unit);

    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) =>
        Route(serial: documentSerialNumber, change: PanelChange.Admit(reason));

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) =>
        Route(serial: documentSerialNumber, change: PanelChange.Admit(reason));

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) {
        Route(
            serial: documentSerialNumber,
            change: onCloseDocument ? new PanelChange.ClosingDocument() : new PanelChange.ClosingPanel());
        Release();
    }

    private void Release() {
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) return;
        Seq<Func<Fin<Unit>>> releases = Construction.Match(
                Succ: static receipt => Seq<Func<Fin<Unit>>>(() => {
                    receipt.Dispose();
                    return Fin.Succ(value: unit);
                }),
                Fail: static _ => Seq<Func<Fin<Unit>>>())
            + fallback.Match(
                Some: static control => Seq<Func<Fin<Unit>>>(() => {
                    control.Dispose();
                    return Fin.Succ(value: unit);
                }),
                None: static () => Seq<Func<Fin<Unit>>>());
        _ = HostThread.Release(releases: releases, key: op).IfFail(failure => {
            _ = faults.Swap(rows => rows.Add(failure));
            return unit;
        });
    }

    private void Route(uint serial, PanelChange change) => ignore(op.Catch(() => identity.Bind(panel => {
        PanelFact fact = new(
            Panel: panel,
            Document: serial is 0u ? None : Some(DocKey.Create(value: serial)),
            Change: change,
            Ordinal: PanelHost.NextOrdinal());
        _ = PanelHost.Stamp(fact: fact, op: op);
        return OnLife(fact);
    })).IfFail(failure => { _ = faults.Swap(rows => rows.Add(failure)); return unit; }));
}
```

## [03]-[PANEL_HOST]

- Owner: `PanelOp<TPanel>` is the one registry operation family for a panel type.
- Cases: registration, placement, presence, document close, document/serial instances, one document instance, icon replacement, and dock-bar usage.
- Entry: `PanelHost.Run<TPanel>` dispatches one request under one command-thread crossing and returns `PanelReceipt<TPanel>`.
- Law: `PanelOp<TPanel>.Admit` validates every nested identity, scope, placement, and icon payload before dispatch.
- Receipt: `PanelPresence` carries visibility as a closed state plus dock bars and the registry-wide open-panel set.
- Law: `PanelPlacement` carries selected-tab policy beside its placement evidence; call sites never pass a second placement knob.
- Boundary: resource- and path-backed icons minted by this owner are disposed after synchronous host calls, while borrowed native icons remain caller-owned.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelIcon {
    private PanelIcon() { }
    public sealed record Native(DrawingIcon Value) : PanelIcon;
    public sealed record Resource(string Name, Assembly Assembly) : PanelIcon;
    public sealed record Path(string Value) : PanelIcon;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        native: static (held, row) => guard(row.Value is not null, held.InvalidInput()).ToFin(),
        resource: static (held, row) =>
            from _ in held.AcceptText(value: row.Name)
            from __ in guard(row.Assembly is not null, held.InvalidInput()).ToFin()
            select unit,
        path: static (held, row) => held.AcceptText(value: row.Value).Map(static _ => unit));

    internal Fin<TResult> Use<TResult>(Func<DrawingIcon, Fin<TResult>> use, Op op) => Switch(
        (Use: use, Op: op),
        native: static (held, row) => held.Op.Catch(() => held.Use(row.Value)),
        resource: static (held, row) => held.Op.Catch(() =>
            Optional(DrawingUtilities.IconFromResource(row.Name, Panels.IconSizeInPixels, row.Assembly))
                .ToFin(Fail: held.Op.InvalidResult())
                .Bind(owned => {
                    using (owned) return held.Use(owned);
                })),
        path: static (held, row) => held.Op.Catch(() => {
            using DrawingIcon owned = new(fileName: row.Value);
            return held.Use(owned);
        }));
}

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelInstanceScope {
    private PanelInstanceScope() { }
    public sealed record Document(DocumentSession Session) : PanelInstanceScope;
    public sealed record Serial(DocKey Document) : PanelInstanceScope;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        document: static (held, row) => guard(row.Session is not null, held.InvalidInput()).ToFin(),
        serial: static (held, row) => AdmitDocument(document: row.Document, op: held));

    private static Fin<Unit> AdmitDocument(DocKey document, Op op) {
        ValidationError? fault = DocKey.Validate(value: document, provider: null, out DocKey? admitted);
        return op.AcceptValidated<DocKey>(fault: fault, admitted: admitted).Map(static _ => unit);
    }
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
        docked: static (held, row) => guard(row.Focus is not null, held.InvalidInput()).ToFin(),
        atBar: static (held, row) =>
            from _ in row.DockBar.Admit(held)
            from __ in guard(row.Focus is not null, held.InvalidInput()).ToFin()
            select unit,
        beside: static (held, row) =>
            from _ in row.Sibling.Admit(held)
            from __ in guard(row.Focus is not null, held.InvalidInput()).ToFin()
            select unit,
        floating: static (held, row) => guard(row.Mode is not null, held.InvalidInput()).ToFin());
}

[SmartEnum]
public sealed partial class PanelVisibility {
    public static readonly PanelVisibility Hidden = new();
    public static readonly PanelVisibility Visible = new();
    public static readonly PanelVisibility Selected = new();
}

public sealed record PanelPresence(
    PanelKey Panel,
    PanelVisibility Visibility,
    Seq<DockBarKey> DockBars,
    Seq<PanelKey> OpenPanels);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelOp<TPanel> where TPanel : HostPanel {
    private PanelOp() { }
    public sealed record Register(PlugIn Owner, HostText Caption, PanelIcon Icon, PanelSite Site) : PanelOp<TPanel>;
    public sealed record Open(PanelPlacement Placement) : PanelOp<TPanel>;
    public sealed record Presence : PanelOp<TPanel>;
    public sealed record Close(DocumentSession Session) : PanelOp<TPanel>;
    public sealed record Instances(PanelInstanceScope Scope) : PanelOp<TPanel>;
    public sealed record Resolve(DocumentSession Session) : PanelOp<TPanel>;
    public sealed record Rebadge(PanelIcon Icon) : PanelOp<TPanel>;
    public sealed record DockBarUsage(DockBarKey DockBar) : PanelOp<TPanel>;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        register: static (held, row) =>
            from _ in guard(row.Owner is not null && row.Caption is not null && row.Icon is not null && row.Site is not null, held.InvalidInput()).ToFin()
            from __ in row.Icon.Admit(held)
            select unit,
        open: static (held, row) => Optional(row.Placement).ToFin(Fail: held.InvalidInput()).Bind(place => place.Admit(held)),
        presence: static (_, _) => Fin.Succ(value: unit),
        close: static (held, row) => guard(row.Session is not null, held.InvalidInput()).ToFin(),
        instances: static (held, row) => Optional(row.Scope).ToFin(Fail: held.InvalidInput()).Bind(scope => scope.Admit(held)),
        resolve: static (held, row) => guard(row.Session is not null, held.InvalidInput()).ToFin(),
        rebadge: static (held, row) => Optional(row.Icon).ToFin(Fail: held.InvalidInput()).Bind(icon => icon.Admit(held)),
        dockBarUsage: static (held, row) => row.DockBar.Admit(held));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelReceipt<TPanel> where TPanel : HostPanel {
    private PanelReceipt() { }
    public sealed record Registered(PanelKey Panel) : PanelReceipt<TPanel>;
    public sealed record Opened(PanelPresence Presence) : PanelReceipt<TPanel>;
    public sealed record Probed(PanelPresence Presence) : PanelReceipt<TPanel>;
    public sealed record Closed(PanelKey Panel) : PanelReceipt<TPanel>;
    public sealed record Found(Seq<TPanel> Panels) : PanelReceipt<TPanel>;
    public sealed record Resolved(Option<TPanel> Panel) : PanelReceipt<TPanel>;
    public sealed record Rebadged(PanelKey Panel) : PanelReceipt<TPanel>;
    public sealed record DockBar(DockBarKey Id, bool InUse) : PanelReceipt<TPanel>;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PanelHost {
    private static readonly Atom<HashMap<PanelKey, PanelFact>> Ledger = Atom(HashMap<PanelKey, PanelFact>());
    private static readonly Atom<Seq<(long Id, CallbackObserver<PanelFact> Observer)>> Watchers =
        Atom(Seq<(long, CallbackObserver<PanelFact>)>());
    private static long ordinal;
    private static long observerId;

    public static HashMap<PanelKey, PanelFact> Facts => Ledger.Value;

    internal static long NextOrdinal() => Interlocked.Increment(location: ref ordinal);

    public static Fin<PanelReceipt<TPanel>> Run<TPanel>(PanelOp<TPanel> request, Op? key = null) where TPanel : HostPanel {
        ArgumentNullException.ThrowIfNull(request);
        Op op = key.OrDefault();
        return from _ in request.Admit(op)
               from panel in PanelKey.Of(panelType: typeof(TPanel), key: op)
               from receipt in request.Switch(
                   (Panel: panel, Op: op),
                   register: static (held, work) => Registered<TPanel>(panel: held.Panel, work: work, op: held.Op),
                   open: static (held, work) => HostThread.Run(
                       work: new HostWork<PanelReceipt<TPanel>>.Execute(Body: () => Opened<TPanel>(held.Panel, work.Placement, held.Op)),
                       key: held.Op),
                   presence: static (held, _) => HostThread.Run(
                       work: new HostWork<PanelReceipt<TPanel>>.Execute(Body: () => Probe<TPanel>(held.Panel, held.Op)
                           .Map<PanelReceipt<TPanel>>(presence => new PanelReceipt<TPanel>.Probed(Presence: presence))),
                       key: held.Op),
                   close: static (held, work) => HostThread.Run(
                       work: new HostWork<PanelReceipt<TPanel>>.Session(
                           Document: work.Session,
                           Needs: [SessionNeed.Redraw],
                           Body: document => held.Op.Catch(() => {
                               Panels.ClosePanel(typeof(TPanel), document);
                               return Fin.Succ<PanelReceipt<TPanel>>(value: new PanelReceipt<TPanel>.Closed(Panel: held.Panel));
                           })),
                       key: held.Op),
                   instances: static (held, work) => work.Scope.Switch(
                       held.Op,
                       document: static (op, scope) => HostThread.Run(
                           work: new HostWork<PanelReceipt<TPanel>>.Session(
                               Document: scope.Session,
                               Needs: [SessionNeed.Read],
                               Body: document => Fin.Succ<PanelReceipt<TPanel>>(value: new PanelReceipt<TPanel>.Found(
                                   Panels: toSeq(Panels.GetPanels<TPanel>(document)).Strict()))),
                           key: op),
                       serial: static (op, scope) => HostThread.Run(
                           work: new HostWork<PanelReceipt<TPanel>>.Execute(
                               Body: () => Fin.Succ<PanelReceipt<TPanel>>(value: new PanelReceipt<TPanel>.Found(
                                   Panels: toSeq(Panels.GetPanels<TPanel>(scope.Document)).Strict()))),
                           key: op)),
                   resolve: static (held, work) => HostThread.Run(
                       work: new HostWork<PanelReceipt<TPanel>>.Session(
                           Document: work.Session,
                           Needs: [SessionNeed.Read],
                           Body: document => Fin.Succ<PanelReceipt<TPanel>>(value: new PanelReceipt<TPanel>.Resolved(
                               Panel: Optional(Panels.GetPanel(held.Panel, document) as TPanel)))),
                       key: held.Op),
                   rebadge: static (held, work) => Rebadged<TPanel>(panel: held.Panel, work: work, op: held.Op),
                   dockBarUsage: static (held, work) => HostThread.Run(
                       work: new HostWork<PanelReceipt<TPanel>>.Execute(Body: () => Fin.Succ<PanelReceipt<TPanel>>(
                           value: new PanelReceipt<TPanel>.DockBar(Id: work.DockBar, InUse: Panels.DockBarIdInUse(work.DockBar)))),
                       key: held.Op))
               select receipt;
    }

    internal static Unit Stamp(PanelFact fact, Op op) {
        _ = Ledger.Swap(held => held.AddOrUpdate(fact.Panel, fact));
        return ignore(Watchers.Value.Iter(row => row.Observer.Guard(
            project: () => Fin.Succ(value: fact),
            op: op)));
    }

    internal static Subscription Watch(CallbackObserver<PanelFact> observer) {
        long id = Interlocked.Increment(location: ref observerId);
        _ = Watchers.Swap(held => held.Add((Id: id, Observer: observer)));
        return Subscription.Of(detach: () => ignore(Watchers.Swap(held => held.Filter(row => row.Id != id))));
    }

    private static Fin<PanelReceipt<TPanel>> Registered<TPanel>(PanelKey panel, PanelOp<TPanel>.Register work, Op op) where TPanel : HostPanel =>
        HostThread.Run(
            work: new HostWork<PanelReceipt<TPanel>>.Execute(Body: () =>
                from owner in Optional(work.Owner).ToFin(Fail: op.MissingContext())
                from caption in op.AcceptText(value: work.Caption.Resolve())
                from registered in work.Icon.Use(icon => op.Catch(() => {
                    Panels.RegisterPanel(owner, typeof(TPanel), caption, icon, work.Site.Key);
                    return Fin.Succ<PanelReceipt<TPanel>>(value: new PanelReceipt<TPanel>.Registered(Panel: panel));
                }), op)
                select registered),
            key: op);

    private static Fin<PanelReceipt<TPanel>> Opened<TPanel>(PanelKey panel, PanelPlacement placement, Op op) where TPanel : HostPanel =>
        placement.Switch(
            (Panel: panel, Op: op),
            docked: static (held, place) => Fin.Succ(value: Op.Side(() => Panels.OpenPanel(typeof(TPanel), place.Focus.Key))),
            atBar: static (held, place) => held.Op.Confirm(success: Panels.OpenPanel(place.DockBar, typeof(TPanel), place.Focus.Key)),
            beside: static (held, place) => held.Op.Confirm(success: Panels.OpenPanelAsSibling(held.Panel, place.Sibling, place.Focus.Key)),
            floating: static (held, place) => held.Op.Confirm(success: Panels.FloatPanel(typeof(TPanel), place.Mode.Key)))
        .Bind(_ => Probe<TPanel>(panel: panel, op: op))
        .Map<PanelReceipt<TPanel>>(presence => new PanelReceipt<TPanel>.Opened(Presence: presence));

    private static Fin<PanelReceipt<TPanel>> Rebadged<TPanel>(PanelKey panel, PanelOp<TPanel>.Rebadge work, Op op) where TPanel : HostPanel =>
        HostThread.Run(
            work: new HostWork<PanelReceipt<TPanel>>.Execute(Body: () => work.Icon.Use(icon => op.Catch(() => {
                Panels.ChangePanelIcon(typeof(TPanel), icon);
                return Fin.Succ<PanelReceipt<TPanel>>(value: new PanelReceipt<TPanel>.Rebadged(Panel: panel));
            }), op)),
            key: op);

    private static Fin<PanelPresence> Probe<TPanel>(PanelKey panel, Op op) where TPanel : HostPanel => op.Catch(() => {
        bool selected = Panels.IsPanelVisible(typeof(TPanel), selectedTabIsVisible: true);
        bool visible = selected || Panels.IsPanelVisible(typeof(TPanel), selectedTabIsVisible: false);
        return from dockBars in toSeq(Panels.PanelDockBars(panel)).TraverseM(id => DockBarKey.Of(value: id, key: op)).As()
               from openPanels in toSeq(Panels.GetOpenPanelIds()).TraverseM(id => PanelKey.Of(value: id, key: op)).As()
               select new PanelPresence(
                   Panel: panel,
                   Visibility: (selected, visible) switch {
                       (true, _) => PanelVisibility.Selected,
                       (false, true) => PanelVisibility.Visible,
                       (false, false) => PanelVisibility.Hidden,
                   },
                   DockBars: dockBars.Strict(),
                   OpenPanels: openPanels.Strict());
    });
}
```

## [04]-[PANEL_OBSERVATION]

- Owner: `PanelObserve` chooses the owned callback ledger or the host-wide `DocumentStream` projection.
- Entry: `PanelObservation.Observe` returns one symmetric `Subscription` for either row and delivers projection failures through the sink rail.
- Law: owned callbacks update `PanelHost.Facts`; host-wide projection never re-stamps the owned ledger.
- Law: `PanelHooks.Mount` registers the `rasm.rhino.hostui.panel` point on the `HookRegistry` row grammar — ask `CallbackObserver<PanelFact>`, grant `Subscription` over the owned watcher fan — and the point's replay modality is the `PanelHost.Facts` latest-per-panel ledger a binder reads before its first delivery.
- Boundary: each delivery crosses `CallbackObserver<PanelFact>`; delivery and rejection faults accumulate without starving sibling observers.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PanelObserve {
    public static readonly PanelObserve Owned = new();
    public static readonly PanelObserve All = new();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PanelObservation {
    public static Fin<Subscription> Observe(
        PanelObserve scope,
        CallbackObserver<PanelFact> observer,
        ReceiptPolicy receipts,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(observer);
        ArgumentNullException.ThrowIfNull(receipts);
        Op op = key.OrDefault();
        return scope.Switch(
            (Observer: observer, Receipts: receipts, Op: op),
            owned: static held => Fin.Succ(value: PanelHost.Watch(held.Observer)),
            all: static held => DocumentStream.Observe(new Observation.Host(
                    Scope: new EventScope.AnyDocument(),
                    Families: Seq(EventFamily.PanelVisibility, EventFamily.PanelClosed),
                    Delivery: new Delivery.Inline(Sink: fact => Fin.Succ(value: held.Observer.Guard(
                        project: () => fact.Payload is EventPayload.Panel panel
                            ? PanelKey.Of(value: panel.PanelId, key: held.Op).Map(id => new PanelFact(
                                Panel: id,
                                Document: fact.Key,
                                Change: panel.State.Switch(
                                    shown: static _ => (PanelChange)new PanelChange.Shown(),
                                    hidden: static _ => new PanelChange.Hidden(),
                                    closed: static _ => new PanelChange.ClosingPanel()),
                                Ordinal: PanelHost.NextOrdinal()))
                            : Fin.Fail<PanelFact>(error: held.Op.InvalidResult()),
                        op: held.Op))),
                    Receipts: held.Receipts))
                .Map(watch => Subscription.Of(detach: watch.Dispose)));
    }
}

public static class PanelHooks {
    public static Fin<IDisposable> Mount(PluginKey plugin, Op? key = null) =>
        HookRegistry.Mount(
            mount: new HookMount(
                Point: HookPoint.HostUiPanel,
                Plugin: plugin,
                Ask: typeof(CallbackObserver<PanelFact>),
                Grant: typeof(Subscription),
                Bind: static ask => Fin.Succ<object>(value: PanelHost.Watch((CallbackObserver<PanelFact>)ask))),
            key: key.OrDefault());
}
```

## [05]-[RUI]

- Owner: `RuiCommand` closes file, group, sidebar, and sizing modalities; `Rui.Run` absorbs snapshot and batch arity.
- Entry: `Rui.Run` returns the full post-operation snapshot plus applied-prefix evidence when a command fails.
- Owner: `RuiFileRef` closes identifier, path, and named lookup with `NameMatch` carrying comparison policy.
- Law: `Rui.Run` admits and normalizes the complete command batch before the host crossing; lookup and mutation consume only admitted references and paths.
- Law: `RuiBarSize` carries a nonempty sizing operation; no optional pair reaches the fold.
- Receipt: file, group, toolbar, sidebar, and global-size facts leave as one detached snapshot.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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

[SmartEnum]
public sealed partial class RuiSidebar {
    public static readonly RuiSidebar Primary = new(apply: static visible => Op.Side(() => ToolbarFileCollection.SidebarIsVisible = visible));
    public static readonly RuiSidebar Recent = new(apply: static visible => Op.Side(() => ToolbarFileCollection.MruSidebarIsVisible = visible));

    [UseDelegateFromConstructor]
    internal partial Unit Apply(bool visible);
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
        byName: static (held, address) =>
            from name in held.AcceptText(value: address.Name)
            from _ in guard(address.Match is not null, held.InvalidInput()).ToFin()
            select (RuiFileRef)(address with { Name = name }));

    internal Fin<ToolbarFile> ResolveAdmitted(Op op) => Switch(
        op,
        byId: static (held, address) =>
            from file in toSeq(RhinoApp.ToolbarFiles).Choose(Optional).Find(candidate => candidate.Id == address.Id)
                .ToFin(Fail: held.MissingContext())
            select file,
        byPath: static (held, address) => Optional(RhinoApp.ToolbarFiles.FindByPath(path: address.Path))
            .ToFin(Fail: held.MissingContext()),
        byName: static (held, address) => Optional(RhinoApp.ToolbarFiles.FindByName(name: address.Name, ignoreCase: address.Match.Key))
            .ToFin(Fail: held.MissingContext()));

    internal static Fin<string> PathOf(string candidate, Op op) =>
        from text in op.AcceptText(value: candidate)
        from path in op.Catch(() => Fin.Succ(value: System.IO.Path.GetFullPath(text)))
        from _ in guard(System.IO.Path.IsPathFullyQualified(path), op.InvalidInput()).ToFin()
        select path;
}

[SmartEnum]
public sealed partial class RuiBar {
    public static readonly RuiBar Bitmap = new(apply: static size => Op.Side(() => Toolbar.BitmapSize = size));
    public static readonly RuiBar Tab = new(apply: static size => Op.Side(() => Toolbar.TabSize = size));

    [UseDelegateFromConstructor]
    internal partial Unit Apply(DrawingSize size);
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
        openFile: static (held, row) =>
            from path in RuiFileRef.PathOf(candidate: row.Path, op: held)
            from _ in guard(row.Save is not null, held.InvalidInput()).ToFin()
            select (RuiCommand)(row with { Path = path }),
        closeFile: static (held, row) =>
            from file in Optional(row.File).ToFin(Fail: held.InvalidInput()).Bind(value => value.Admit(held))
            from _ in guard(row.Close is not null, held.InvalidInput()).ToFin()
            select (RuiCommand)(row with { File = file }),
        saveFile: static (held, row) => Optional(row.File).ToFin(Fail: held.InvalidInput())
            .Bind(value => value.Admit(held))
            .Map<RuiCommand>(file => row with { File = file }),
        saveFileAs: static (held, row) =>
            from file in Optional(row.File).ToFin(Fail: held.InvalidInput()).Bind(value => value.Admit(held))
            from target in RuiFileRef.PathOf(candidate: row.Target, op: held)
            select (RuiCommand)(row with { File = file, Target = target }),
        group: static (held, row) =>
            from file in Optional(row.File).ToFin(Fail: held.InvalidInput()).Bind(value => value.Admit(held))
            from _ in guard(row.GroupId != Guid.Empty && row.Visibility is not null, held.InvalidInput()).ToFin()
            select (RuiCommand)(row with { File = file }),
        sidebar: static (held, row) => guard(row.Target is not null && row.Visibility is not null, held.InvalidInput()).ToFin()
            .Map<RuiCommand>(_ => row),
        barSize: static (held, row) => guard(row.Size is not null, held.InvalidInput()).ToFin()
            .Map<RuiCommand>(_ => row));
}

public sealed record RuiFileFact(Guid Id, string Name, string Path, int Groups, int Toolbars);

public sealed record RuiGroupFact(Guid File, Guid Group, string Name, bool Visible, bool Docked);

public sealed record RuiToolbarFact(Guid File, Guid Toolbar, string Name);

public sealed record RuiSnapshot(
    Seq<RuiFileFact> Files,
    Seq<RuiGroupFact> Groups,
    Seq<RuiToolbarFact> Toolbars,
    bool Sidebar,
    bool RecentSidebar,
    DrawingSize Bitmap,
    DrawingSize Tab);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuiReceipt {
    private RuiReceipt() { }
    public sealed record Completed(RuiSnapshot Snapshot, int Applied) : RuiReceipt;
    public sealed record Partial(RuiSnapshot Snapshot, int Applied, Error Fault) : RuiReceipt;
}

internal sealed record RuiBatchState(int Applied, Option<Error> Fault);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Rui {
    public static Fin<RuiReceipt> Run(Op? key = null, params ReadOnlySpan<RuiCommand> commands) {
        Op op = key.OrDefault();
        Seq<RuiCommand> batch = toSeq(commands.ToArray()).Strict();
        return from admitted in batch.TraverseM(command => Optional(command)
                   .ToFin(Fail: op.InvalidInput())
                   .Bind(value => value.Admit(op)))
                   .As()
               from receipt in HostThread.Run(
                   work: new HostWork<RuiReceipt>.Execute(Body: () => Applied(commands: admitted.Strict(), op: op)),
                   key: op)
               select receipt;
    }

    private static Fin<RuiReceipt> Applied(Seq<RuiCommand> commands, Op op) {
        RuiBatchState state = commands.Fold(
            new RuiBatchState(Applied: 0, Fault: None),
            (held, command) => held.Fault.IsSome
                ? held
                : Apply(command: command, op: op).Match(
                    Succ: _ => held with { Applied = held.Applied + 1 },
                    Fail: fault => held with { Fault = Some(fault) }));
        return Census(op: op).Map(snapshot => state.Fault.Match<RuiReceipt>(
            Some: fault => new RuiReceipt.Partial(Snapshot: snapshot, Applied: state.Applied, Fault: fault),
            None: () => new RuiReceipt.Completed(Snapshot: snapshot, Applied: state.Applied)));
    }

    private static Fin<Unit> Apply(RuiCommand command, Op op) => command.Switch(
        op,
        openFile: static (held, work) =>
            from path in held.AcceptText(value: work.Path)
            from file in held.Catch(() => Optional(RhinoApp.ToolbarFiles.Open(path: path)).ToFin(Fail: held.InvalidResult(detail: path)))
            from _ in work.Save.Key ? held.Confirm(success: file.Save()) : Fin.Succ(value: unit)
            select unit,
        closeFile: static (held, work) => work.File.ResolveAdmitted(op: held).Bind(file => held.Confirm(success: file.Close(prompt: work.Close.Key))),
        saveFile: static (held, work) => work.File.ResolveAdmitted(op: held).Bind(file => held.Confirm(success: file.Save())),
        saveFileAs: static (held, work) =>
            from file in work.File.ResolveAdmitted(op: held)
            from _ in held.Confirm(success: file.SaveAs(path: work.Target))
            select unit,
        group: static (held, work) =>
            from file in work.File.ResolveAdmitted(op: held)
            from groups in Indexed(count: file.GroupCount, read: file.GetGroup, op: held)
            from group in groups.Find(candidate => candidate.Id == work.GroupId)
                .ToFin(Fail: held.MissingContext())
            select Op.Side(() => group.Visible = work.Visibility.Key),
        sidebar: static (_, work) => Fin.Succ(value: work.Target.Apply(visible: work.Visibility.Key)),
        barSize: static (held, work) => toSeq(work.Size.Values)
            .TraverseM(size => held.Catch(() => Fin.Succ(value: size.Key.Apply(size.Value))))
            .As()
            .Map(static _ => unit));

    private static Fin<RuiSnapshot> Census(Op op) => op.Catch(() =>
        from files in toSeq(RhinoApp.ToolbarFiles)
            .TraverseM(file => Optional(file).ToFin(Fail: op.InvalidResult(detail: nameof(RhinoApp.ToolbarFiles))))
            .As()
            .Map(static rows => rows.Strict())
        from groups in files.TraverseM(file => Indexed(count: file.GroupCount, read: file.GetGroup, op: op)
                .Map(rows => rows.Map(group => new RuiGroupFact(file.Id, group.Id, group.Name, group.Visible, group.IsDocked)).Strict()))
            .As()
            .Map(static batches => batches.Bind(static batch => batch).Strict())
        from toolbars in files.TraverseM(file => Indexed(count: file.ToolbarCount, read: file.GetToolbar, op: op)
                .Map(rows => rows.Map(toolbar => new RuiToolbarFact(file.Id, toolbar.Id, toolbar.Name)).Strict()))
            .As()
            .Map(static batches => batches.Bind(static batch => batch).Strict())
        select new RuiSnapshot(
            Files: files.Map(static file => new RuiFileFact(file.Id, file.Name, file.Path, file.GroupCount, file.ToolbarCount)).Strict(),
            Groups: groups,
            Toolbars: toolbars,
            Sidebar: ToolbarFileCollection.SidebarIsVisible,
            RecentSidebar: ToolbarFileCollection.MruSidebarIsVisible,
            Bitmap: Toolbar.BitmapSize,
            Tab: Toolbar.TabSize));

    private static Fin<Seq<T>> Indexed<T>(int count, Func<int, T?> read, Op op) where T : class =>
        from _ in guard(flag: count >= 0, False: op.InvalidResult()).ToFin()
        from rows in toSeq(Enumerable.Range(start: 0, count: count))
            .TraverseM(index => Optional(read(index)).ToFin(Fail: op.InvalidResult(detail: $"{typeof(T).Name}[{index}]")))
            .As()
        select rows.Strict();
}
```

## [06]-[MENU_LINKS]

- Owner: `MenuDelta` is the update algebra over enabled, checked, radio, and caption axes.
- Entry: `MenuLinks.Register` seats one host callback, folds every emitted delta onto the live `RuiUpdateUi`, and retains observer faults.
- Law: callback state is recomputed from `RuiAddress`; no mutable menu state escapes the host invocation.
- Boundary: a rejected `RegisterMenuItem` return is the operation's typed failure; callback delivery uses the shared guarded observer owner.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MenuLinks {
    public static Fin<Unit> Register(
        RuiAddress address,
        Func<RuiAddress, Seq<MenuDelta>> sync,
        CallbackObserver<Unit> observer,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(sync);
        ArgumentNullException.ThrowIfNull(observer);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Unit>.Required(Body: () => op.Confirm(success: RuiUpdateUi.RegisterMenuItem(
                address.File,
                address.Menu,
                address.Item,
                (_, live) => {
                    _ = observer.Guard(
                        project: () => op.Catch(() => sync(address)
                            .TraverseM(delta => Apply(live, delta, op))
                            .As()
                            .Map(static _ => unit)),
                        op: op);
                }))),
            key: op);
    }

    private static Fin<Unit> Apply(RuiUpdateUi live, MenuDelta delta, Op op) => delta.Switch(
        (Live: live, Op: op),
        enabled: static (held, value) => Fin.Succ(value: Op.Side(() => held.Live.Enabled = value.State.Key)),
        @checked: static (held, value) => Fin.Succ(value: Op.Side(() => held.Live.Checked = value.State.Key)),
        radio: static (held, value) => Fin.Succ(value: Op.Side(() => held.Live.RadioChecked = value.State.Key)),
        caption: static (held, value) => held.Op.AcceptText(value: value.Value.Resolve()).Map(text => Op.Side(() => held.Live.Text = text)));
}
```

## [07]-[SECTIONS]

- Owner: `SectionSpec` carries caption, body, height, command-option caption, a frozen feature set, and one optional `SectionSignal` lifecycle hook.
- Owner: `SectionFeature` and `SectionHolderFeature` close per-section and holder capability; `SectionSignal` closes attach, detach, holder-visibility, and refresh evidence.
- Entry: `Sections.Mount` realizes every body, preserves declaration order, and returns a `SectionMount` owning the holder, every `ElementReceipt`, and the accumulated hook faults.
- Law: every leaf lifecycle override chains the host base member first, then routes its `SectionSignal` case; a hook fault lands in `SectionMount.Faults` and never re-enters the holder.
- Law: `SectionSpec` admits positive height before realization, an empty section sequence rejects before any host leaf is minted, and at most one section carries the `FullHeight` row.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SectionFeature {
    public static readonly SectionFeature Expanded = new();
    public static readonly SectionFeature Collapsible = new();
    public static readonly SectionFeature Hidden = new();
    public static readonly SectionFeature FullHeight = new();
}

[SmartEnum]
public sealed partial class SectionHolderFeature {
    public static readonly SectionHolderFeature Scrollbars = new();
    public static readonly SectionHolderFeature Checkboxes = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionSignal {
    private SectionSignal() { }
    public sealed record Attaching : SectionSignal;
    public sealed record Attached : SectionSignal;
    public sealed record Detaching : SectionSignal;
    public sealed record Detached : SectionSignal;
    public sealed record HolderShown(bool Visible) : SectionSignal;
    public sealed record Refreshed(uint Flags) : SectionSignal;
}

[ComplexValueObject]
public sealed partial class SectionSpec {
    public HostText Caption { get; }
    public Element Body { get; }
    public int Height { get; }
    public FrozenSet<SectionFeature> Features { get; }
    public Option<HostText> CommandOption { get; }
    public Option<Func<SectionSignal, Fin<Unit>>> Life { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText caption,
        ref Element body,
        ref int height,
        ref FrozenSet<SectionFeature> features,
        ref Option<HostText> commandOption,
        ref Option<Func<SectionSignal, Fin<Unit>>> life) =>
        validationError = caption is null
            || body is null
            || features is null
            || features.Any(static feature => feature is null)
            || commandOption.Exists(static value => value is null)
            || life.Exists(static hook => hook is null)
            || height <= 0
            ? new ValidationError(message: "Section specification is invalid.")
            : null;
}

// --- [SERVICES] -----------------------------------------------------------------------------
internal sealed class SectionLeaf : EtoCollapsibleSection3 {
    private readonly SectionSpec spec;
    private readonly Action<Error> report;
    private readonly Op op;

    internal SectionLeaf(SectionSpec spec, Control content, Action<Error> report, Op op) {
        this.spec = spec;
        this.report = report;
        this.op = op;
        Content = content;
    }

    public override LocalizeStringPair Caption => new(
        spec.Caption.English,
        spec.Caption.Resolve());
    public override int SectionHeight => spec.Height;
    public override bool Collapsible => spec.Features.Contains(SectionFeature.Collapsible);
    public override bool Hidden => spec.Features.Contains(SectionFeature.Hidden);
    public override bool InitiallyExpanded => spec.Features.Contains(SectionFeature.Expanded);
    public override LocalizeStringPair CommandOptionName => spec.CommandOption.Match(
        Some: static caption => caption.OptionName(),
        None: static () => new LocalizeStringPair(string.Empty, string.Empty));

    public override void OnAttachingToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachingToHolder(holder);
        Route(signal: new SectionSignal.Attaching());
    }

    public override void OnAttachedToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachedToHolder(holder);
        Route(signal: new SectionSignal.Attached());
    }

    public override void OnDetachingFromHolder(ICollapsibleSectionHolder2 holder) {
        base.OnDetachingFromHolder(holder);
        Route(signal: new SectionSignal.Detaching());
    }

    public override void OnDetachedFromHolder(ICollapsibleSectionHolder2 holder) {
        base.OnDetachedFromHolder(holder);
        Route(signal: new SectionSignal.Detached());
    }

    public override void HolderVisible(bool visible) {
        base.HolderVisible(visible);
        Route(signal: new SectionSignal.HolderShown(Visible: visible));
    }

    public override void UpdateView(uint flags) {
        base.UpdateView(flags);
        Route(signal: new SectionSignal.Refreshed(Flags: flags));
    }

    private void Route(SectionSignal signal) => ignore(spec.Life.Iter(hook =>
        ignore(op.Catch(() => hook(signal)).IfFail(failure => { report(failure); return unit; }))));
}

public sealed class SectionMount : IDisposable {
    private readonly Seq<ElementReceipt> contents;
    private readonly Atom<Seq<Error>> faults;
    private readonly Op op;
    private int released;

    internal SectionMount(Control host, Seq<ElementReceipt> contents, Atom<Seq<Error>> faults, Op op) {
        Host = host;
        this.contents = contents;
        this.faults = faults;
        this.op = op;
    }

    public Control Host { get; }

    public Seq<Error> Faults => faults.Value;

    public void Dispose() {
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) return;
        Seq<Func<Fin<Unit>>> releases = contents.Rev()
            .Map(static receipt => (Func<Fin<Unit>>)(() => {
                receipt.Dispose();
                return Fin.Succ(value: unit);
            }))
            .Add(() => {
                Host.Dispose();
                return Fin.Succ(value: unit);
            });
        _ = HostThread.Release(releases: releases, key: op).IfFail(failure => {
            _ = faults.Swap(rows => rows.Add(failure));
            return unit;
        });
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Sections {
    public static Fin<SectionMount> Mount(
        Seq<SectionSpec> sections,
        FrozenSet<SectionHolderFeature> features,
        ElementRuntime runtime,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(features);
        ArgumentNullException.ThrowIfNull(runtime);
        Op op = key.OrDefault();
        return from admitted in sections
                   .TraverseM(section => Optional(section).ToFin(Fail: op.InvalidInput()))
                   .As()
               from _ in guard(
                   flag: !admitted.IsEmpty
                       && admitted.Filter(static section => section.Features.Contains(SectionFeature.FullHeight)).Count <= 1,
                   False: op.InvalidInput()).ToFin()
               from contents in ElementReceipt.Gather(admitted.Map(static section => section.Body), runtime, op)
               from mounted in Seat(sections: admitted, contents: contents, features: features, op: op)
               select mounted;
    }

    private static Fin<SectionMount> Seat(
        Seq<SectionSpec> sections,
        Seq<ElementReceipt> contents,
        FrozenSet<SectionHolderFeature> features,
        Op op) {
        EtoCollapsibleSectionHolder2? holder = null;
        Atom<Seq<Error>> faults = Atom(Seq<Error>());
        return op.Catch(() => {
            EtoCollapsibleSectionHolder2 owned = holder = new() {
                UseScrollbars = features.Contains(SectionHolderFeature.Scrollbars),
                UseCheckBoxes = features.Contains(SectionHolderFeature.Checkboxes),
            };
            _ = sections.Zip(contents).Iter(pair => {
                SectionLeaf leaf = new(
                    spec: pair.First,
                    content: pair.Second.Host,
                    report: failure => ignore(faults.Swap(rows => rows.Add(failure))),
                    op: op);
                owned.Add(section: leaf);
                _ = Op.SideWhen(
                    pair.First.Features.Contains(SectionFeature.FullHeight),
                    () => owned.SetFullHeightSection(sec: leaf));
            });
            return Fin.Succ(value: new SectionMount(host: owned, contents: contents, faults: faults, op: op));
        }).MapFail(fault => HostThread.Release(
            releases: contents.Rev()
                .Map(static receipt => (Func<Fin<Unit>>)(() => {
                    receipt.Dispose();
                    return Fin.Succ(value: unit);
                }))
                .Add(() => {
                    holder?.Dispose();
                    return Fin.Succ(value: unit);
                }),
            key: op).Match(
                Succ: _ => fault,
                Fail: cleanup => fault + cleanup));
    }
}
```

## [08]-[HOST_CONTROLS]

- Owner: `HostControl` closes the consumable `Rhino.UI.Controls` widget library as exact-payload cases; `ToElement` admits every nested payload before bridging into the settled `Element.Realize` fold, so realization, receipts, styling, and teardown stay the Eto owner's.
- Owner: `RhinoPad` and `RhinoSpace` key the host padding and spacing vocabularies; `UnitPulse` rows fold to the unit-entry update-mode flags — a pixel literal or raw host flag never reaches a call site.
- Owner: `ThemePalette.Detach` folds a received `ThemeZone` into detached `ThemeSwatch` rows under `PerceptualColor`, and `ThemePalette.Feed` joins them to the Eto theme catalog through `ThemeShift.Hosted` under a declared path-to-role map — the map is the positive allow-list, every declared role must resolve to a zone swatch, and an unresolved role fails the feed with the missing paths as typed evidence; `UiServices.Resolve<TService>` is the one platform-service seam over the locator with the provider fallback.
- Law: a new Rhino widget is one `HostControl` case and one `Mint` arm; command-bearing cases resolve through the runtime `IntentTable`, and the runtime captured at projection is the panel's own realize runtime.
- Law: `GridWrap` is the family's one nested case — children are `HostControl` rows admitted and minted through the same dispatch, so the wrapping grid composes the family it belongs to, never a parallel container surface.
- Law: unit-aware numeric entry carries an admitted `UnitSpan` plus one `UnitFormat` case; model-unit and explicit-length-unit formatting share the same mint arm, and the control parses its own text.
- Boundary: a partially minted `RhinoButtonRow` or `ControlGridLayout` is disposed with its orphaned children when any addition or child mint fails; successful rows transfer lifetime to the enclosing element receipt.
- Law: colour payloads enter as `PerceptualColor` and quantize once through `Pigment.ToColor` at the mint arm; the host theme tree is read-only — a consumer detaches swatches and never authors a zone.
- Boundary: the parent-coupled host slider and the document-bound linetype grid stay behind their own document-scoped owners; `Rhino.UI.Forms` dialog bases ride `ShellWindows.Present`, and native `CppPointer` handles never cross this family.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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
}

[SmartEnum<uint>]
public sealed partial class UnitPulse {
    public static readonly UnitPulse OnValueChange = new(1u);
    public static readonly UnitPulse OnEnterOrLoseFocus = new(2u);
    public static readonly UnitPulse WhenDoneChanging = new(8u);

    internal static NumericUpDownWithUnitParsingUpdateMode Fold(FrozenSet<UnitPulse> pulses) =>
        toSeq(pulses).Fold(
            default(NumericUpDownWithUnitParsingUpdateMode),
            static (mask, pulse) => mask | (NumericUpDownWithUnitParsingUpdateMode)pulse.Key);
}

[ComplexValueObject]
public sealed partial class UnitSpan {
    public double Value { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public double Increment { get; }
    public int Decimals { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double value,
        ref double minimum,
        ref double maximum,
        ref double increment,
        ref int decimals) =>
        validationError = !double.IsFinite(value) || !double.IsFinite(minimum) || !double.IsFinite(maximum)
            || !double.IsFinite(increment) || minimum > maximum || value < minimum || value > maximum
            || increment <= 0d || decimals < 0
            ? new ValidationError(message: "Unit entry span is invalid.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnitFormat {
    private UnitFormat() { }
    public sealed record Model(UnitSystem Units, DistanceDisplayMode Display) : UnitFormat;
    public sealed record Length(LengthUnit Units, DistanceDisplayMode Display) : UnitFormat;

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        model: static (held, row) => guard(Enum.IsDefined(row.Units) && Enum.IsDefined(row.Display), held.InvalidInput()).ToFin(),
        length: static (held, row) => guard(Enum.IsDefined(row.Units) && Enum.IsDefined(row.Display), held.InvalidInput()).ToFin());

    internal Unit Apply(NumericUpDownWithUnitParsing control) => Switch(
        control,
        model: static (held, row) => Op.Side(() => held.SetFormatUnitSystem(row.Units, row.Display)),
        length: static (held, row) => Op.Side(() => held.SetFormatLengthUnits(row.Units, row.Display)));
}

public sealed record HostAction(Image Face, HostText Tip, IntentKey Intent) {
    internal Fin<Unit> Admit(Op op) =>
        from _ in guard(Face is not null && Tip is not null, op.InvalidInput()).ToFin()
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
        FrozenSet<UnitPulse> Pulses,
        Option<HostText> Prefix,
        Option<HostText> Suffix) : HostControl;
    public sealed record RichAlternate(TextAccess Access, Option<HostText> Alternate) : HostControl;
    public sealed record ImageCommand(Image Face, Option<Image> Disabled, IntentKey Intent) : HostControl;
    public sealed record AddRemove(IntentKey Add, IntentKey Remove) : HostControl;
    public sealed record ActionRow(Seq<HostAction> Actions) : HostControl;
    public sealed record GridWrap(
        Seq<HostControl> Items,
        GridWrapMode Direction,
        Size ItemSize,
        bool Stretch) : HostControl;
    public sealed record DividerLine(Option<PerceptualColor> Colour) : HostControl;
    public sealed record CaptionRule(HostText Caption) : HostControl;
    public sealed record PinnedLabel(HostText Text, TextAlignment Alignment) : HostControl;
    public sealed record OutputColour(
        DisplayAndPrintColorPickerMode Mode,
        PerceptualColor Display,
        PerceptualColor Print,
        bool Linked) : HostControl;
    public sealed record ViewportView(Option<HostText> Title) : HostControl;

    public Fin<Element> ToElement(ElementSpec spec, ElementRuntime runtime, Op? key = null) {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(runtime);
        Op op = key.OrDefault();
        HostControl control = this;
        return Admit(op).Map<Element>(_ => new Element.Custom(Spec: spec, Mint: () => control.Mint(runtime: runtime, op: op)));
    }

    internal Fin<Unit> Admit(Op op) => Switch(
        op,
        unitEntry: static (held, row) =>
            from _ in guard(row.Span is not null
                && row.Format is not null
                && row.Pulses is not null
                && row.Pulses.All(static pulse => pulse is not null)
                && row.Prefix.ForAll(static text => text is not null)
                && row.Suffix.ForAll(static text => text is not null), held.InvalidInput()).ToFin()
            from __ in row.Format.Admit(held)
            select unit,
        richAlternate: static (held, row) => guard(row.Access is not null
            && row.Alternate.ForAll(static text => text is not null), held.InvalidInput()).ToFin(),
        imageCommand: static (held, row) =>
            from _ in guard(row.Face is not null && row.Disabled.ForAll(static image => image is not null), held.InvalidInput()).ToFin()
            from __ in held.AcceptValidated<IntentKey>(candidate: row.Intent.Value)
            select unit,
        addRemove: static (held, row) =>
            from add in held.AcceptValidated<IntentKey>(candidate: row.Add.Value)
            from remove in held.AcceptValidated<IntentKey>(candidate: row.Remove.Value)
            from _ in guard(add != remove, held.InvalidInput()).ToFin()
            select unit,
        actionRow: static (held, row) =>
            from _ in guard(!row.Actions.IsEmpty, held.InvalidInput()).ToFin()
            from __ in row.Actions
                .TraverseM(action => Optional(action).ToFin(Fail: held.InvalidInput()).Bind(value => value.Admit(held)))
                .As()
            select unit,
        gridWrap: static (held, row) =>
            from _ in guard(!row.Items.IsEmpty
                && Enum.IsDefined(row.Direction)
                && row.ItemSize.Width > 0
                && row.ItemSize.Height > 0, held.InvalidInput()).ToFin()
            from __ in row.Items
                .TraverseM(item => Optional(item).ToFin(Fail: held.InvalidInput()).Bind(value => value.Admit(held)))
                .As()
            select unit,
        dividerLine: static (held, row) => guard(row.Colour.ForAll(static colour => colour is not null), held.InvalidInput()).ToFin(),
        captionRule: static (held, row) => guard(row.Caption is not null, held.InvalidInput()).ToFin(),
        pinnedLabel: static (held, row) => guard(row.Text is not null && Enum.IsDefined(row.Alignment), held.InvalidInput()).ToFin(),
        outputColour: static (held, row) => guard(Enum.IsDefined(row.Mode)
            && row.Display is not null
            && row.Print is not null, held.InvalidInput()).ToFin(),
        viewportView: static (held, row) => guard(row.Title.ForAll(static title => title is not null), held.InvalidInput()).ToFin());

    internal Fin<Control> Mint(ElementRuntime runtime, Op op) => Switch(
        (Runtime: runtime, Op: op),
        unitEntry: static (held, row) => held.Op.Catch(() => {
            NumericUpDownWithUnitParsing stepper = new(showStepper: true) {
                MinValue = row.Span.Minimum,
                MaxValue = row.Span.Maximum,
                Increment = row.Span.Increment,
                DecimalPlaces = row.Span.Decimals,
                Value = row.Span.Value,
                ValueUpdateMode = UnitPulse.Fold(pulses: row.Pulses),
            };
            _ = row.Format.Apply(stepper);
            _ = row.Prefix.Iter(text => stepper.Prefix = text.Resolve());
            _ = row.Suffix.Iter(text => stepper.Suffix = text.Resolve());
            return Fin.Succ<Control>(value: stepper);
        }),
        richAlternate: static (held, row) => held.Op.Catch(() => {
            RichTextAreaWithAlternateText rich = new() { ReadOnly = row.Access.HostReadOnly };
            _ = row.Alternate.Iter(text => Op.Side(() => {
                rich.AlternateText = text.Resolve();
                rich.ShowAlternateText = true;
            }));
            return Fin.Succ<Control>(value: rich);
        }),
        imageCommand: static (held, row) => held.Runtime.Intents.Command(row.Intent).Bind(command => held.Op.Catch(() => {
            ImageButton button = new() { Image = row.Face, Command = command };
            _ = row.Disabled.Iter(image => button.DisabledImage = image);
            return Fin.Succ<Control>(value: button);
        })),
        addRemove: static (held, row) =>
            from add in held.Runtime.Intents.Command(row.Add)
            from remove in held.Runtime.Intents.Command(row.Remove)
            from control in held.Op.Catch(() =>
                Fin.Succ<Control>(value: new AddRemoveButton { AddCommand = add, RemoveCommand = remove }))
            select control,
        actionRow: static (held, row) => row.Actions
            .TraverseM(action => held.Runtime.Intents.Command(action.Intent).Map(command => (Action: action, Command: command)))
            .As()
            .Bind(pairs => {
                RhinoButtonRow? bar = null;
                return held.Op.Catch(() => {
                    bar = new RhinoButtonRow();
                    RhinoButtonRow owned = bar;
                    _ = pairs.Iter(pair => Op.Side(() =>
                        owned.AddButton(pair.Action.Face, false, pair.Action.Tip.Resolve()).Command = pair.Command));
                    return Fin.Succ<Control>(value: owned);
                }).MapFail(fault => {
                    return held.Op.Catch(() => {
                        bar?.Dispose();
                        return Fin.Succ(value: unit);
                    }).Match(
                        Succ: _ => fault,
                        Fail: cleanup => fault + cleanup);
                });
            }),
        gridWrap: static (held, row) => row.Items
            .TraverseM(item => item.Mint(runtime: held.Runtime, op: held.Op))
            .As()
            .Bind(children => {
                ControlGridLayout? grid = null;
                return held.Op.Catch(() => {
                    grid = new ControlGridLayout {
                        GridWrapMode = row.Direction,
                        ItemSize = row.ItemSize,
                        StretchItemsToWidth = row.Stretch,
                    };
                    ControlGridLayout owned = grid;
                    _ = children.Iter(child => Op.Side(() => owned.Items.Add(child)));
                    return Fin.Succ<Control>(value: owned);
                }).MapFail(fault => held.Op.Catch(() => {
                    grid?.Dispose();
                    _ = children.Rev().Iter(child => Op.Side(child.Dispose));
                    return Fin.Succ(value: unit);
                }).Match(
                    Succ: _ => fault,
                    Fail: cleanup => fault + cleanup));
            }),
        dividerLine: static (held, row) => held.Op.Catch(() => {
            Divider line = new();
            _ = row.Colour.Iter(colour => line.Color = Pigment.ToColor(colour: colour));
            return Fin.Succ<Control>(value: line);
        }),
        captionRule: static (held, row) => held.Op.Catch(() =>
            Fin.Succ<Control>(value: new LabelSeparator { Text = row.Caption.Resolve() })),
        pinnedLabel: static (held, row) => held.Op.Catch(() =>
            Fin.Succ<Control>(value: new StaticAlignedLabel(row.Alignment) { Text = row.Text.Resolve() })),
        outputColour: static (held, row) => held.Op.Catch(() => Fin.Succ<Control>(value: new DisplayAndPrintColorPicker {
            PickerMode = row.Mode,
            LinkPrintToDisplay = row.Linked,
            DisplayColor = Pigment.ToColor(colour: row.Display),
            PrintColor = Pigment.ToColor(colour: row.Print),
        })),
        viewportView: static (held, row) => held.Op.Catch(() => Fin.Succ<Control>(value: row.Title.Match(
            Some: static title => new ViewportControl(viewportTitle: title.Resolve()),
            None: static () => new ViewportControl()))));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ThemePalette {
    public static Fin<Seq<ThemeSwatch>> Detach(ThemeZone zone, Op? key = null) {
        ArgumentNullException.ThrowIfNull(zone);
        Op op = key.OrDefault();
        return toSeq(zone.Enumerate())
            .Choose(static entry => entry.Value is Color colour ? Some((Entry: entry, Colour: colour)) : None)
            .TraverseM(row => PerceptualColor.OfRgb(
                    red: (byte)row.Colour.Rb,
                    green: (byte)row.Colour.Gb,
                    blue: (byte)row.Colour.Bb,
                    alpha: row.Colour.A,
                    key: op)
                .Map(colour => new ThemeSwatch(Path: $"{zone.Id}/{row.Entry.Id}", Value: colour)))
            .As()
            .Map(static swatches => swatches.Strict());
    }

    public static Fin<ThemeChange> Feed(
        ThemeZone zone,
        ThemeSeam seam,
        ThemeVariant variant,
        HashMap<string, PaletteRole> roles,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(seam);
        ArgumentNullException.ThrowIfNull(variant);
        Op op = key.OrDefault();
        return Detach(zone, op).Bind(swatches => {
            HashMap<string, PerceptualColor> found = toHashMap(swatches.Map(static swatch => (swatch.Path, swatch.Value)));
            Seq<string> missing = toSeq(roles).Filter(row => found.Find(row.Key).IsNone).Map(static row => row.Key).Strict();
            return missing.IsEmpty
                ? seam.Change(
                    shift: new ThemeShift.Hosted(
                        Variant: variant,
                        Cells: toHashMap(toSeq(roles).Choose(row => found.Find(row.Key).Map(value => (row.Value, value))))),
                    key: op)
                : Fin.Fail<ThemeChange>(error: op.InvalidResult(detail: string.Join(",", missing)));
        });
    }
}

public static class UiServices {
    public static Fin<TService> Resolve<TService>(Op? key = null) where TService : class {
        Op op = key.OrDefault();
        return op.Catch(() =>
            (Optional(RhinoUiServiceLocator.GetService<TService>()) | Optional(PlatformServiceProvider.Service as TService))
                .ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: typeof(TService).Name)));
    }
}
```
