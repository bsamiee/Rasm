# [RASM_RHINO_HOSTUI_PAGES]

The polymorphic host-page adapter of `Rasm.Rhino.HostUi` — one `PageKind` discriminant carrying the five host page modalities (options, document properties, application preferences, object properties, stacked child) as rows, one `PagePlan` value declaring a page completely (identity, Eto `Element` content, chrome flags, object-selection scope, one signal handler, one display predicate), and one realization dispatch minting the host base behind the plan. Every Rhino override — `OnActivate`/`OnApply`/`OnCancel`/`RunScript`/`OnDefaults`/`OnHelp`/`OnCreateParent`/`OnSizeParent` on the options base, `ShouldDisplay`/`UpdatePage`/`ModifyPage`/`GetSelectedObjects` on the object-properties base — is owned exactly once by two internal sealed leaves that route into one `PageSignal` family and one answering spine, so a consumer never subclasses a host page: the census `RasmOptionsPage`/`RasmPropertiesPage` public wrapper pair and the `PageRegistration` overload split are dead, registration collection shape rides a `PageBasket` case selected by the kind row, stacked navigation and Windows-only navigation styling ride one `PageNav` fold, and the document-properties/preferences windows resolve as kind-row columns over `RhinoEtoApp`. Content is an `Element` realized through the Eto sub-domain and styled native through `EtoExtensions.UseRhinoStyle` at the one mint site; realization assumes a marshalled frame, and host callbacks arrive on the command thread by host contract.

## [01]-[INDEX]

- [02]-[PAGE_KIND]: `PageKind` + `PageIdentity` + `ObjectScope` — the five-row page discriminant with its stacked and window-resolver columns, page identity, and the object-properties selection scope.
- [03]-[PAGE_SIGNAL]: `PageSignal` — the one lifecycle family every host override routes into, with the scripted lane carried as `SessionMode`.
- [04]-[PLAN_AND_LEAVES]: `PagePlan` + `HostPage` + the internal `OptionsLeaf`/`PropertiesLeaf` — the one page declaration, its realization, the answering spine, and the typed selection and modify surfaces.
- [05]-[NAVIGATION]: `PageNav` — the stacked-dialog navigation fold: activation, reveal, removal, dirty state, child adoption, Windows-gated styling, sequences.
- [06]-[MOUNT_AND_WINDOWS]: `PageBasket` + `PageMount` — kind-agreeing registration onto the host collections, and the kind-column window reveal.

## [02]-[PAGE_KIND]

- Owner: `PageKind` — the closed page-modality vocabulary. `Stacked` marks the rows minting the options base (`Options`, `DocumentProperties`, `Preferences`, `Child`) against the object-properties row; the `Reveal` column resolves the Eto window that presents the page — `DocumentPropertiesWindowForPage` on the document-properties row, `ApplicationPreferencesWindowForPage` on the preferences row — and every other row answers `UiFault.Unavailable` because the host options dialog and the properties panel own their own presentation. `PageIdentity` carries the caption pair (English title, optional localized title), the embedded icon resource for the properties leaf, the optional navigation `Image` for the stacked leaf, the properties-panel `Index`, and the `PropertyPageType` seat. `ObjectScope` carries the object-properties display gate — supported `ObjectType` flags, the all-must-match policy, sub-object participation — with `Any` the canonical row.
- Law: modality is the row, never a wrapper class — the census per-base public pair collapses because the row selects the internal leaf, the registration basket, and the reveal window together, so a new host page modality is one row whose missing arms break realization and mount at compile time.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Eto sub-domain (`UiFault`), Rhino.UI (`RhinoEtoApp.DocumentPropertiesWindowForPage`/`ApplicationPreferencesWindowForPage`, `PropertyPageType`), RhinoCommon (`ObjectType`).
- Growth (HOST): a host page-seat column (a Rhino 9 page placement fact, a per-row help topic) is one `PageKind` or `PageIdentity` column consumed at the one mint site.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Eto;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;
using DrawingColor = System.Drawing.Color;
using DrawingImage = System.Drawing.Image;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PageKind {
    public static readonly PageKind Options = new(key: 0, stacked: true, reveal: Denied(capability: nameof(Options)));
    public static readonly PageKind DocumentProperties = new(key: 1, stacked: true, reveal: Windowed(RhinoEtoApp.DocumentPropertiesWindowForPage));
    public static readonly PageKind Preferences = new(key: 2, stacked: true, reveal: Windowed(RhinoEtoApp.ApplicationPreferencesWindowForPage));
    public static readonly PageKind ObjectProperties = new(key: 3, stacked: false, reveal: Denied(capability: nameof(ObjectProperties)));
    public static readonly PageKind Child = new(key: 4, stacked: true, reveal: Denied(capability: nameof(Child)));

    public bool Stacked { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Window> Reveal(OptionsDialogPage page, Op op);

    private static Func<OptionsDialogPage, Op, Fin<Window>> Denied(string capability) =>
        (_, op) => Fin.Fail<Window>(error: new UiFault.Unavailable(Key: op, Capability: capability));

    private static Func<OptionsDialogPage, Op, Fin<Window>> Windowed(Func<OptionsDialogPage, Window?> resolve) =>
        (page, op) => op.Catch(() => Optional(resolve(page)).ToFin(Fail: op.MissingContext()));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PageIdentity(
    string English,
    Option<string> Local = default,
    Option<string> IconResource = default,
    Option<DrawingImage> Image = default,
    int Index = -1,
    PropertyPageType Seat = PropertyPageType.Custom);

public sealed record ObjectScope(ObjectType Types = ObjectType.AnyObject, bool AllMustMatch = false, bool SubObjects = false) {
    public static readonly ObjectScope Any = new();
}
```

## [03]-[PAGE_SIGNAL]

- Owner: `PageSignal` — the one closed lifecycle family every host override routes into: activation with its entering/leaving polarity, apply, cancel, the scripted lane carrying the document key and `SessionMode`, defaults, help, the native parent handle and size edges, and the object-selection display/update edges carrying detached `SelectionEvidence` (document key from `DocRuntimeSerialNumber`, the host `EventRuntimeSerialNumber` ordinal, the `ObjectCount`). The census `PagePhase` enum beside a `PageEvent` union — two vocabularies for one lifecycle — collapses to this single family, and the answer contract is uniform: the plan's handler returns `Fin<Unit>`, the spine projects success onto the host's `bool` and `Result` returns, and a fault is a refused host callback, never a thrown one.
- Law: the scripted case carries `SessionMode`, not the foreign `RunMode` — the leaf admits the host ordinal through `SessionMode.OfRunMode` at the seam, so an unknown lane refuses before the handler runs; the properties-leaf scripted lane derives its document key from the event args' `DocRuntimeSerialNumber`.
- Law: selection payloads stay on the handle — `SelectionEvidence` carries counts and serials, never an object list, because the typed read is `HostPage.Selection`, a pull over the verified `GetSelectedObjects` at the moment the handler wants it; an eagerly copied object list per callback is churn the host never asked for.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Document sub-domain (`DocKey`, `SessionMode`), Rhino.UI (`ObjectPropertiesPageEventArgs.DocRuntimeSerialNumber`/`EventRuntimeSerialNumber`/`ObjectCount`).
- Growth (DOMAIN): a lifecycle edge the concept demands (a host help-context payload, a page-resize policy fact) is one case breaking the two leaves loudly at compile time.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageSignal {
    private PageSignal() { }
    public sealed record Activated(bool Entering) : PageSignal;
    public sealed record Applied : PageSignal;
    public sealed record Cancelled : PageSignal;
    public sealed record Scripted(Option<DocKey> Document, SessionMode Mode) : PageSignal;
    public sealed record Reset : PageSignal;
    public sealed record Helped : PageSignal;
    public sealed record ParentCreated(nint Handle) : PageSignal;
    public sealed record ParentSized(int Width, int Height) : PageSignal;
    public sealed record SelectionShown(SelectionEvidence Evidence) : PageSignal;
    public sealed record SelectionUpdated(SelectionEvidence Evidence) : PageSignal;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct SelectionEvidence(Option<DocKey> Document, uint EventOrdinal, int Count);
```

## [04]-[PLAN_AND_LEAVES]

- Owner: `PagePlan` — the complete page declaration: kind row, identity, Eto `Element` content, chrome flags (`ShowApply`, `ShowDefaults`), object scope, the one `Answer` handler over `PageSignal`, and the one `Display` predicate — and `HostPage`, the realized handle carrying the typed post-realization surface: `Navigate` folds `PageNav` over the stacked base, `Reveal` resolves the kind-row window, `Selection` projects detached object ids through `GetSelectedObjects`, and `Modify` runs a change inside the host `ModifyPage` wrap so the host owns the refresh cycle around it. `Realize` is the one mint: content realizes through the Eto element algebra, styles native through `UseRhinoStyle`, and the kind row selects the internal leaf.
- Law: the two leaves are `internal sealed` — the host base classes never reach a consumer signature, every override exists exactly once per base, and every override body is one spine call; a second override site or a consumer subclass is the census form this owner deletes.
- Law: the spine is the one host-return projection — affirmative overrides read `IsSucc`, `Result` overrides fold success/failure, void overrides discard — and every handler invocation crosses `Op.Catch`, so a throwing consumer handler refuses the callback instead of detonating inside the host dialog.
- Law: `ShouldDisplay` is the three-gate conjunction — the host `IncludesObjectsType` test over the plan's scope, the plan's `Display` predicate, then the handler's `SelectionShown` affirmation — and `UpdatePage` never re-runs the display gate, because the host has already gated visibility when it asks for an update.
- Packages: LanguageExt.Core, Rasm.Domain (`Op`), Document sub-domain (`DocKey`, `SessionMode`), Eto sub-domain (`Element`, `UiFault`), Rhino.UI (`OptionsDialogPage` full override roster, `ObjectPropertiesPage` full override and metadata roster, `ObjectPropertiesPageEventArgs.IncludesObjectsType`/`DocRuntimeSerialNumber`/`EventRuntimeSerialNumber`/`ObjectCount`, `EtoExtensions.UseRhinoStyle`), Rhino.Commands (`Result`, `RunMode`).
- Growth (CONSUMER): a per-page capability future plugins require (an availability stream for apply, a dirty-state receipt) is one plan slot consumed inside the leaves; a third leaf appears only when the host ships a third page base.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PagePlan(
    PageKind Kind,
    PageIdentity Identity,
    Element Content,
    Func<PageSignal, Fin<Unit>> Answer,
    Option<ObjectScope> Scope = default,
    bool ShowApply = true,
    bool ShowDefaults = false,
    Option<Func<bool>> Display = default) {

    internal ObjectScope Gate => Scope.IfNone(ObjectScope.Any);

    internal bool Displayable => Display.Map(static probe => probe()).IfNone(true);

    public Fin<HostPage> Realize(Op? key = null) {
        Op op = key.OrDefault();
        return Content.Realize(key: op).Bind(control => op.Catch(() => {
            _ = Op.Side(() => EtoExtensions.UseRhinoStyle(control));
            return Fin.Succ(value: Kind.Stacked
                ? HostPage.OfStacked(plan: this, page: new OptionsLeaf(plan: this, content: control, op: op))
                : HostPage.OfProperties(plan: this, page: new PropertiesLeaf(plan: this, content: control, op: op)));
        }));
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class HostPage {
    private readonly PagePlan plan;
    private readonly Either<OptionsDialogPage, ObjectPropertiesPage> leaf;

    private HostPage(PagePlan plan, Either<OptionsDialogPage, ObjectPropertiesPage> leaf) {
        this.plan = plan;
        this.leaf = leaf;
    }

    public PagePlan Plan => plan;

    internal Option<OptionsDialogPage> StackedLeaf => leaf.Swap().ToOption();

    internal Option<ObjectPropertiesPage> PropertiesLeaf => leaf.ToOption();

    internal static HostPage OfStacked(PagePlan plan, OptionsDialogPage page) => new(plan: plan, leaf: Left(page));

    internal static HostPage OfProperties(PagePlan plan, ObjectPropertiesPage page) => new(plan: plan, leaf: Right(page));

    public Fin<Unit> Navigate(PageNav nav, Op? key = null) {
        Op op = key.OrDefault();
        return StackedLeaf.ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(StackedDialogPage)))
            .Bind(page => nav.Apply(page: page, op: op));
    }

    public Fin<Window> Reveal(Op? key = null) {
        Op op = key.OrDefault();
        return StackedLeaf.ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(RhinoEtoApp)))
            .Bind(page => plan.Kind.Reveal(page: page, op: op));
    }

    public Fin<Seq<Guid>> Selection(ObjectType filter, Op? key = null) {
        Op op = key.OrDefault();
        return PropertiesLeaf.ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(ObjectPropertiesPage.GetSelectedObjects)))
            .Bind(page => op.Catch(() => Fin.Succ(value: toSeq(page.GetSelectedObjects(filter)).Map(static held => held.Id).Strict())));
    }

    public Fin<Unit> Modify(Func<Fin<Unit>> change, Op? key = null) {
        Op op = key.OrDefault();
        return PropertiesLeaf.ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(ObjectPropertiesPage.ModifyPage)))
            .Bind(page => op.Catch(() => {
                Fin<Unit> outcome = Fin.Fail<Unit>(error: op.InvalidResult());
                page.ModifyPage(callbackAction: _ => outcome = op.Catch(change));
                return outcome;
            }));
    }
}

internal static class PageSpine {
    internal static bool Affirm(PagePlan plan, PageSignal signal, Op op) => op.Catch(() => plan.Answer(signal)).IsSucc;

    internal static Unit Notify(PagePlan plan, PageSignal signal, Op op) => ignore(op.Catch(() => plan.Answer(signal)));

    internal static Result Verdict(PagePlan plan, PageSignal signal, Op op) =>
        op.Catch(() => plan.Answer(signal)).Match(Succ: static _ => Result.Success, Fail: static _ => Result.Failure);

    internal static Result ScriptVerdict(PagePlan plan, Option<DocKey> document, RunMode mode, Op op) =>
        SessionMode.OfRunMode(mode: mode, key: op).Match(
            Succ: lane => Verdict(plan: plan, signal: new PageSignal.Scripted(Document: document, Mode: lane), op: op),
            Fail: static _ => Result.Failure);
}

internal sealed class OptionsLeaf : OptionsDialogPage {
    private readonly PagePlan plan;
    private readonly Control content;
    private readonly Op op;

    internal OptionsLeaf(PagePlan plan, Control content, Op op) : base(plan.Identity.English) {
        this.plan = plan;
        this.content = content;
        this.op = op;
    }

    public override object PageControl => content;
    public override string LocalPageTitle => plan.Identity.Local.IfNone(plan.Identity.English);
    public override DrawingImage PageImage => plan.Identity.Image.IfNoneUnsafe((DrawingImage?)null)!;
    public override bool ShowApplyButton => plan.ShowApply;
    public override bool ShowDefaultsButton => plan.ShowDefaults;
    public override bool OnApply() => PageSpine.Affirm(plan: plan, signal: new PageSignal.Applied(), op: op);
    public override void OnCancel() => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.Cancelled(), op: op));
    public override bool OnActivate(bool active) => PageSpine.Affirm(plan: plan, signal: new PageSignal.Activated(Entering: active), op: op);
    public override Result RunScript(RhinoDoc doc, RunMode mode) =>
        PageSpine.ScriptVerdict(plan: plan, document: DocKey.Of(document: doc, key: op).ToOption(), mode: mode, op: op);
    public override void OnDefaults() => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.Reset(), op: op));
    public override void OnHelp() => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.Helped(), op: op));
    public override void OnCreateParent(nint hwndParent) => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.ParentCreated(Handle: hwndParent), op: op));
    public override void OnSizeParent(int width, int height) => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.ParentSized(Width: width, Height: height), op: op));
}

internal sealed class PropertiesLeaf : ObjectPropertiesPage {
    private readonly PagePlan plan;
    private readonly Control content;
    private readonly Op op;

    internal PropertiesLeaf(PagePlan plan, Control content, Op op) {
        this.plan = plan;
        this.content = content;
        this.op = op;
    }

    public override object PageControl => content;
    public override string EnglishPageTitle => plan.Identity.English;
    public override string LocalPageTitle => plan.Identity.Local.IfNone(plan.Identity.English);
    public override int Index => plan.Identity.Index;
    public override PropertyPageType PageType => plan.Identity.Seat;
    public override string PageIconEmbeddedResourceString => plan.Identity.IconResource.IfNone(string.Empty);
    public override ObjectType SupportedTypes => plan.Gate.Types;
    public override bool AllObjectsMustBeSupported => plan.Gate.AllMustMatch;
    public override bool SupportsSubObjects => plan.Gate.SubObjects;
    public override bool OnActivate(bool active) => PageSpine.Affirm(plan: plan, signal: new PageSignal.Activated(Entering: active), op: op);
    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        PageSpine.ScriptVerdict(plan: plan, document: Evidenced(e: e).Document, mode: RunMode.Scripted, op: op);
    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        e.IncludesObjectsType(objectTypes: plan.Gate.Types, allMustMatch: plan.Gate.AllMustMatch)
            && plan.Displayable
            && PageSpine.Affirm(plan: plan, signal: new PageSignal.SelectionShown(Evidence: Evidenced(e: e)), op: op);
    public override void UpdatePage(ObjectPropertiesPageEventArgs e) =>
        ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.SelectionUpdated(Evidence: Evidenced(e: e)), op: op));
    public override void OnHelp() => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.Helped(), op: op));
    public override void OnCreateParent(nint hwndParent) => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.ParentCreated(Handle: hwndParent), op: op));
    public override void OnSizeParent(int width, int height) => ignore(PageSpine.Notify(plan: plan, signal: new PageSignal.ParentSized(Width: width, Height: height), op: op));

    private static SelectionEvidence Evidenced(ObjectPropertiesPageEventArgs e) => new(
        Document: e.DocRuntimeSerialNumber is 0u ? None : Some(DocKey.Create(value: e.DocRuntimeSerialNumber)),
        EventOrdinal: e.EventRuntimeSerialNumber,
        Count: e.ObjectCount);
}
```

## [05]-[NAVIGATION]

- Owner: `PageNav` — the stacked-dialog navigation fold over the realized options base: activation (`MakeActivePage`), named reveal (`SetActivePageTo` with its document-properties polarity, a missing name a typed rejection because the host's `false` return is evidence), removal, dirty marking (`Modified`), retitling (`SetEnglishPageTitle`), child adoption (a `PagePlan` realized to a stacked leaf and landed through `AddChildPage`), navigation styling (`NavigationTextIsBold`/`NavigationTextColor`, Windows-only by host contract and gated as `UiFault.Unavailable` elsewhere), and sequences as one traversal. One fold owns every stacked mutation; per-verb methods on a page wrapper are the census form this union deletes.
- Law: child adoption is recursive page realization — the child plan must be a stacked kind, its content realizes through the same one mint, and the parent-child relation exists only in the host tree, never as a parallel local registry.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Domain (`Op`), Eto sub-domain (`UiFault`), Rhino.UI (`StackedDialogPage.MakeActivePage`/`SetActivePageTo`/`RemovePage`/`Modified`/`SetEnglishPageTitle`/`AddChildPage`/`NavigationTextIsBold`/`NavigationTextColor`).
- Growth (HOST): a stacked navigation member the host ships is one case breaking `Apply` at compile time.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageNav {
    private PageNav() { }
    public sealed record Activate : PageNav;
    public sealed record Reveal(string Page, bool DocumentProperties = false) : PageNav;
    public sealed record Remove : PageNav;
    public sealed record Dirty(bool Modified = true) : PageNav;
    public sealed record Retitle(string Title) : PageNav;
    public sealed record Adopt(PagePlan Child) : PageNav;
    public sealed record Styled(bool Bold, DrawingColor Color) : PageNav;
    public sealed record Sequence(Seq<PageNav> Steps) : PageNav;

    internal Fin<Unit> Apply(StackedDialogPage page, Op op) =>
        Switch(
            state: (Page: page, Op: op),
            activate: static (held, _) => Fin.Succ(value: Op.Side(held.Page.MakeActivePage)),
            reveal: static (held, nav) => held.Page.SetActivePageTo(pageName: nav.Page, documentPropertiesPage: nav.DocumentProperties)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: held.Op.InvalidResult(detail: nav.Page)),
            remove: static (held, _) => Fin.Succ(value: Op.Side(held.Page.RemovePage)),
            dirty: static (held, nav) => Fin.Succ(value: Op.Side(() => held.Page.Modified = nav.Modified)),
            retitle: static (held, nav) => held.Op.AcceptText(value: nav.Title)
                .Map(title => Op.Side(() => held.Page.SetEnglishPageTitle(title))),
            adopt: static (held, nav) => nav.Child.Kind.Stacked
                ? nav.Child.Realize(key: held.Op).Bind(child => child.StackedLeaf.Match(
                    Some: leaf => held.Op.Catch(() => { held.Page.AddChildPage(pageToAdd: leaf); return Fin.Succ(value: unit); }),
                    None: () => Fin.Fail<Unit>(error: held.Op.InvalidResult())))
                : Fin.Fail<Unit>(error: held.Op.InvalidInput()),
            styled: static (held, nav) => OperatingSystem.IsWindows()
                ? Fin.Succ(value: Op.Side(() => { held.Page.NavigationTextIsBold = nav.Bold; held.Page.NavigationTextColor = nav.Color; }))
                : Fin.Fail<Unit>(error: new UiFault.Unavailable(Key: held.Op, Capability: nameof(StackedDialogPage.NavigationTextColor))),
            sequence: static (held, nav) => nav.Steps.TraverseM(step => step.Apply(page: held.Page, op: held.Op)).As().Map(static _ => unit));
}
```

## [06]-[MOUNT_AND_WINDOWS]

- Owner: `PageBasket` — the two host registration collections as one closed family: the options-page collection a plugin's page hook supplies, and the `ObjectPropertiesPageCollection` the properties hook supplies — and `PageMount.Land`, the one registration fold absorbing arity through `params ReadOnlySpan<HostPage>`: the kind row and the basket case must agree per page, a stacked child never lands in a basket (it lands through `PageNav.Adopt`), and the census `PageRegistration.Add` overload pair — modality smuggled into overload resolution — is dead because the basket case is the collection shape.
- Law: registration is marshalled and trapped — landing runs inside the host page hook on the command thread, and a throwing collection add is a typed fault the hook reports instead of a plugin load crash.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Domain (`Op`), Eto sub-domain (`UiFault`), Rhino.UI (`ObjectPropertiesPageCollection.Add`).
- Growth (HOST): a new host page collection is one basket case plus the kind rows that agree with it, both breaking `Land` at compile time.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageBasket {
    private PageBasket() { }
    public sealed record Options(ICollection<OptionsDialogPage> Pages) : PageBasket;
    public sealed record Properties(ObjectPropertiesPageCollection Pages) : PageBasket;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PageMount {
    public static Fin<Unit> Land(PageBasket basket, params ReadOnlySpan<HostPage> pages) {
        Op op = Op.Of();
        return Iterable<HostPage>.FromSpan(pages)
            .TraverseM(page => Landed(page: page, basket: basket, op: op))
            .As()
            .Map(static _ => unit);
    }

    private static Fin<Unit> Landed(HostPage page, PageBasket basket, Op op) =>
        basket.Switch(
            state: (Page: page, Op: op),
            options: static (held, basket) => held.Page.Plan.Kind.Stacked && held.Page.Plan.Kind != PageKind.Child
                ? held.Page.StackedLeaf.Match(
                    Some: leaf => held.Op.Catch(() => { basket.Pages.Add(item: leaf); return Fin.Succ(value: unit); }),
                    None: () => Fin.Fail<Unit>(error: held.Op.InvalidResult()))
                : Fin.Fail<Unit>(error: held.Op.InvalidInput()),
            properties: static (held, basket) => held.Page.PropertiesLeaf.Match(
                Some: leaf => held.Op.Catch(() => { basket.Pages.Add(page: leaf); return Fin.Succ(value: unit); }),
                None: () => Fin.Fail<Unit>(error: held.Op.InvalidInput())));
}
```
