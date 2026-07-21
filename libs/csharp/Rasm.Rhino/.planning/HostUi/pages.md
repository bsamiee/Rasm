# [RASM_RHINO_HOSTUI_PAGES]

`HostPage` realizes every Rhino settings or properties page from one `PagePlan` family, keeps host base classes behind internal leaves, and returns a kind-safe handle for navigation, selection, modification, reveal, and registration. `PagePlan` carries stacked and object-properties payloads as distinct cases, so every field is valid for its host seat and every callback enters one `PageSignal` algebra.

## [01]-[INDEX]

- [02]-[PLAN]: `PagePlan`, `PageSeat`, and the identity owners close page kind, content, chrome, selection, and callback policy.
- [03]-[SIGNAL]: `PageSignal` carries lifecycle, script, parent, and detached selection evidence through one answering rail.
- [04]-[REALIZATION]: `HostPage` and the internal leaves realize the host base and expose kind-safe post-realization operations.
- [05]-[NAVIGATION]: `PageNav` folds stacked activation, reveal, removal, dirty state, title, child adoption, and navigation style.
- [06]-[MOUNT]: `PageBasket` and `PageMount.Land` register realized pages against the matching host collection.

## [02]-[PLAN]

- Owner: `PagePlan` is the closed page declaration with `Stacked` and `Properties` cases.
- Cases: `Stacked` carries `PageSeat` and `StackedIdentity`; `Properties` carries `ObjectIdentity`, `ObjectScope`, and its visibility predicate.
- Owner: `PageSeat` is the host registration and reveal vocabulary for options, document-properties, preferences, and child pages.
- Law: `PageButton` is a frozen capability set, so button combinations are data and no boolean pair reaches a leaf.
- Boundary: `PagePlan.Admit` revalidates identity, scope, content, and delegates before realization; `ObjectKind` remains the sole foreign `ObjectType` admission table.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Eto.Forms;
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
[SmartEnum]
public sealed partial class PageButton {
    public static readonly PageButton Apply = new();
    public static readonly PageButton Defaults = new();
}

[SmartEnum<string>]
public sealed partial class PageSeat {
    public static readonly PageSeat Options = new("options", reveal: Denied(nameof(Options)));
    public static readonly PageSeat Document = new("document", reveal: Windowed(RhinoEtoApp.DocumentPropertiesWindowForPage));
    public static readonly PageSeat Preferences = new("preferences", reveal: Windowed(RhinoEtoApp.ApplicationPreferencesWindowForPage));
    public static readonly PageSeat Child = new("child", reveal: Denied(nameof(Child)));

    [UseDelegateFromConstructor]
    internal partial Fin<Window> Reveal(OptionsDialogPage page, Op op);

    private static Func<OptionsDialogPage, Op, Fin<Window>> Denied(string capability) =>
        (_, op) => Fin.Fail<Window>(error: new UiFault.Unavailable(Key: op, Capability: capability));

    private static Func<OptionsDialogPage, Op, Fin<Window>> Windowed(Func<OptionsDialogPage, Window?> resolve) =>
        (page, op) => op.Catch(() => Optional(resolve(page)).ToFin(Fail: op.MissingContext()));
}

[ComplexValueObject]
public sealed partial class StackedIdentity {
    public HostText Caption { get; }
    public DrawingImage Image { get; }
    public FrozenSet<PageButton> Buttons { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText caption,
        ref DrawingImage image,
        ref FrozenSet<PageButton> buttons) =>
        validationError = caption is null || image is null || buttons is null || buttons.Any(static button => button is null)
            ? new ValidationError(message: "Stacked page identity is invalid.")
            : null;
}

[ComplexValueObject]
public sealed partial class ObjectIdentity {
    public HostText Caption { get; }
    public string IconResource { get; }
    public int Index { get; }
    public ObjectPageSeat Seat { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText caption,
        ref string iconResource,
        ref int index,
        ref ObjectPageSeat seat) =>
        validationError = caption is null || string.IsNullOrWhiteSpace(iconResource) || index < 0 || seat is null
            ? new ValidationError(message: "Object page identity is invalid.")
            : null;
}

[SmartEnum<PropertyPageType>]
public sealed partial class ObjectPageSeat {
    public static readonly ObjectPageSeat Material = new(key: PropertyPageType.Material);
    public static readonly ObjectPageSeat Light = new(key: PropertyPageType.Light);
    public static readonly ObjectPageSeat Custom = new(key: PropertyPageType.Custom);
    public static readonly ObjectPageSeat Dimension = new(key: PropertyPageType.Dimension);
    public static readonly ObjectPageSeat Leader = new(key: PropertyPageType.Leader);
    public static readonly ObjectPageSeat Text = new(key: PropertyPageType.Text);
    public static readonly ObjectPageSeat Hatch = new(key: PropertyPageType.Hatch);
    public static readonly ObjectPageSeat Dot = new(key: PropertyPageType.Dot);
    public static readonly ObjectPageSeat TextureMapping = new(key: PropertyPageType.TextureMapping);
    public static readonly ObjectPageSeat Detail = new(key: PropertyPageType.Detail);
    public static readonly ObjectPageSeat ClippingPlane = new(key: PropertyPageType.ClippingPlane);
    public static readonly ObjectPageSeat NamedView = new(key: PropertyPageType.NamedView);
    public static readonly ObjectPageSeat Decal = new(key: PropertyPageType.Decal);
    public static readonly ObjectPageSeat View = new(key: PropertyPageType.View);
}

[SmartEnum<ObjectType>]
public sealed partial class ObjectKind {
    public static readonly ObjectKind Point = new(key: ObjectType.Point);
    public static readonly ObjectKind PointSet = new(key: ObjectType.PointSet);
    public static readonly ObjectKind Curve = new(key: ObjectType.Curve);
    public static readonly ObjectKind Surface = new(key: ObjectType.Surface);
    public static readonly ObjectKind Brep = new(key: ObjectType.Brep);
    public static readonly ObjectKind Mesh = new(key: ObjectType.Mesh);
    public static readonly ObjectKind Light = new(key: ObjectType.Light);
    public static readonly ObjectKind Annotation = new(key: ObjectType.Annotation);
    public static readonly ObjectKind InstanceDefinition = new(key: ObjectType.InstanceDefinition);
    public static readonly ObjectKind InstanceReference = new(key: ObjectType.InstanceReference);
    public static readonly ObjectKind TextDot = new(key: ObjectType.TextDot);
    public static readonly ObjectKind Grip = new(key: ObjectType.Grip);
    public static readonly ObjectKind Detail = new(key: ObjectType.Detail);
    public static readonly ObjectKind Hatch = new(key: ObjectType.Hatch);
    public static readonly ObjectKind MorphControl = new(key: ObjectType.MorphControl);
    public static readonly ObjectKind SubD = new(key: ObjectType.SubD);
    public static readonly ObjectKind BrepLoop = new(key: ObjectType.BrepLoop);
    public static readonly ObjectKind BrepVertex = new(key: ObjectType.BrepVertex);
    public static readonly ObjectKind Polysurface = new(key: ObjectType.PolysrfFilter);
    public static readonly ObjectKind Edge = new(key: ObjectType.EdgeFilter);
    public static readonly ObjectKind Polyedge = new(key: ObjectType.PolyedgeFilter);
    public static readonly ObjectKind MeshVertex = new(key: ObjectType.MeshVertex);
    public static readonly ObjectKind MeshEdge = new(key: ObjectType.MeshEdge);
    public static readonly ObjectKind MeshFace = new(key: ObjectType.MeshFace);
    public static readonly ObjectKind Cage = new(key: ObjectType.Cage);
    public static readonly ObjectKind Phantom = new(key: ObjectType.Phantom);
    public static readonly ObjectKind ClipPlane = new(key: ObjectType.ClipPlane);
    public static readonly ObjectKind Extrusion = new(key: ObjectType.Extrusion);
}

[ComplexValueObject]
public sealed partial class ObjectKinds {
    public FrozenSet<ObjectKind> Values { get; }

    internal ObjectType Mask => toSeq(Values).Fold(ObjectType.None, static (mask, kind) => mask | kind.Key);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<ObjectKind> values) =>
        validationError = values is null || values.Count is 0 || values.Any(static kind => kind is null)
            ? new ValidationError(message: "Object selection is empty.")
            : null;
}

[SmartEnum]
public sealed partial class SelectionPolicy {
    public static readonly SelectionPolicy AnyObject = new(all: false, subobjects: false);
    public static readonly SelectionPolicy EveryObject = new(all: true, subobjects: false);
    public static readonly SelectionPolicy AnyComponent = new(all: false, subobjects: true);
    public static readonly SelectionPolicy EveryComponent = new(all: true, subobjects: true);

    internal bool All { get; }
    internal bool Subobjects { get; }
}

[ComplexValueObject]
public sealed partial class ObjectScope {
    public ObjectKinds Kinds { get; }
    public SelectionPolicy Policy { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ObjectKinds kinds,
        ref SelectionPolicy policy) =>
        validationError = kinds is null || policy is null
            ? new ValidationError(message: "Object page scope is invalid.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PagePlan {
    private PagePlan() { }

    public sealed record Stacked(
        PageSeat Seat,
        StackedIdentity Identity,
        Element Content,
        Func<PageSignal, Fin<Unit>> Answer) : PagePlan;

    public sealed record Properties(
        ObjectIdentity Identity,
        ObjectScope Scope,
        Element Content,
        Func<SelectionEvidence, Fin<bool>> Display,
        Func<PageSignal, Fin<Unit>> Answer) : PagePlan;

    internal Fin<PagePlan> Admit(Op op) => Switch(
        op,
        stacked: static (held, page) =>
            from seat in Optional(page.Seat).ToFin(Fail: held.InvalidInput())
            from candidate in Optional(page.Identity).ToFin(Fail: held.InvalidInput())
            from identity in StackedIdentity.TryCreate(
                caption: candidate.Caption,
                image: candidate.Image,
                buttons: candidate.Buttons,
                out StackedIdentity? admitted) && admitted is { } value
                    ? Fin.Succ(value: value)
                    : Fin.Fail<StackedIdentity>(error: held.InvalidInput())
            from content in Optional(page.Content).ToFin(Fail: held.InvalidInput())
            from answer in Optional(page.Answer).ToFin(Fail: held.InvalidInput())
            select (PagePlan)new Stacked(Seat: seat, Identity: identity, Content: content, Answer: answer),
        properties: static (held, page) =>
            from candidateIdentity in Optional(page.Identity).ToFin(Fail: held.InvalidInput())
            from identity in ObjectIdentity.TryCreate(
                caption: candidateIdentity.Caption,
                iconResource: candidateIdentity.IconResource,
                index: candidateIdentity.Index,
                seat: candidateIdentity.Seat,
                out ObjectIdentity? admittedIdentity) && admittedIdentity is { } identity
                    ? Fin.Succ(value: identity)
                    : Fin.Fail<ObjectIdentity>(error: held.InvalidInput())
            from candidateScope in Optional(page.Scope).ToFin(Fail: held.InvalidInput())
            from scope in ObjectScope.TryCreate(
                kinds: candidateScope.Kinds,
                policy: candidateScope.Policy,
                out ObjectScope? admittedScope) && admittedScope is { } value
                    ? Fin.Succ(value: value)
                    : Fin.Fail<ObjectScope>(error: held.InvalidInput())
            from content in Optional(page.Content).ToFin(Fail: held.InvalidInput())
            from display in Optional(page.Display).ToFin(Fail: held.InvalidInput())
            from answer in Optional(page.Answer).ToFin(Fail: held.InvalidInput())
            select (PagePlan)new Properties(
                Identity: identity,
                Scope: scope,
                Content: content,
                Display: display,
                Answer: answer));
}
```

## [03]-[SIGNAL]

- Owner: `PageSignal` closes every callback the host page bases expose.
- Cases: activation, apply, cancel, script, defaults, help, native-parent lifecycle, selection visibility, and selection refresh.
- Receipt: `SelectionEvidence` detaches document, object, view, and viewport identity plus event ordinal and selection count before callback exit.
- Law: `Scripted` carries admitted `SessionMode`; a foreign `RunMode` never crosses the leaf.
- Boundary: callback evidence retains object identities only; typed or filtered native handles remain pull-based through host leaves.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PageActivation {
    public static readonly PageActivation Entered = new();
    public static readonly PageActivation Left = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageSignal {
    private PageSignal() { }
    public sealed record Activated(PageActivation State) : PageSignal;
    public sealed record Applied : PageSignal;
    public sealed record Cancelled : PageSignal;
    public sealed record Scripted(Option<DocKey> Document, SessionMode Mode) : PageSignal;
    public sealed record Reset : PageSignal;
    public sealed record Helped : PageSignal;
    public sealed record ParentCreated(nint Handle) : PageSignal;
    public sealed record ParentSized(int Width, int Height) : PageSignal;
    public sealed record SelectionShown(SelectionEvidence Evidence) : PageSignal;
    public sealed record SelectionUpdated(SelectionEvidence Evidence) : PageSignal;
    public sealed record Refused(Error Fault) : PageSignal;
}

public sealed record SelectionEvidence(
    Option<DocKey> Document,
    uint EventOrdinal,
    int Count,
    Seq<Guid> Objects,
    Option<uint> View,
    Option<Guid> Viewport);
```

## [04]-[REALIZATION]

- Owner: `HostPage.Realize` is the sole page mint and runs only inside an existing command-thread frame.
- Owner: `PageLeaf` closes the internal host-base alternatives; no host base enters a consumer signature.
- Entry: `Navigate`, `Reveal`, `Selection`, and `Modify` expose the distinct result regimes of the realized handle.
- Receipt: `HostPage` retains its `ElementReceipt`; one open-closing-released custody gate excludes operations from teardown, distinguishes parent, mount-receipt, and host-collection owners, and releases every control tree once.
- Law: each host override calls the plan's `Answer` once and collapses `Fin<Unit>` only at the required host return type.
- Law: visibility combines `IncludesObjectsType`, the plan predicate, and the `SelectionShown` answer; refresh emits only `SelectionUpdated`.
- Boundary: `Modify` captures the callback result and rejects a host call that returns without invoking the callback.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PageLeaf {
    private PageLeaf() { }
    internal sealed record Stacked(OptionsDialogPage Value) : PageLeaf;
    internal sealed record Properties(ObjectPropertiesPage Value) : PageLeaf;
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class HostPage : IDisposable {
    private abstract record PageOwner {
        private PageOwner() { }
        internal sealed record Parent(HostPage Value) : PageOwner;
        internal sealed record Mount(Guid Token) : PageOwner;
        internal sealed record Host : PageOwner;
    }

    private abstract record PageCustody {
        private PageCustody() { }
        internal sealed record Open(PageOwner? Owner, int Active, Seq<HostPage> Children) : PageCustody;
        internal sealed record Closing(PageOwner? Owner, int Active, Seq<HostPage> Children) : PageCustody;
        internal sealed record Released : PageCustody;
    }

    private readonly PagePlan plan;
    private readonly PageLeaf leaf;
    private readonly ElementReceipt content;
    private readonly object sync = new();
    private PageCustody custody = new PageCustody.Open(Owner: null, Active: 0, Children: Seq<HostPage>());

    private HostPage(PagePlan plan, PageLeaf leaf, ElementReceipt content) {
        this.plan = plan;
        this.leaf = leaf;
        this.content = content;
    }

    public PagePlan Plan => plan;

    internal Option<PagePlan.Stacked> StackedPlan => plan.Switch(
        stacked: static page => Some(page),
        properties: static _ => None);

    internal Option<OptionsDialogPage> StackedLeaf => leaf.Switch(
        stacked: static page => Some(page.Value),
        properties: static _ => None);

    internal Option<ObjectPropertiesPage> PropertiesLeaf => leaf.Switch(
        stacked: static _ => None,
        properties: static page => Some(page.Value));

    public static Fin<HostPage> Realize(PagePlan plan, ElementRuntime runtime, Op? key = null) {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(runtime);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<HostPage>.Required(Body: () => plan.Admit(op).Bind(admitted => admitted.Switch(
                (Runtime: runtime, Op: op),
                stacked: static (held, page) => Realized(
                    plan: page,
                    tree: page.Content,
                    runtime: held.Runtime,
                    op: held.Op,
                    seat: static (plan, control, at) => new PageLeaf.Stacked(
                        Value: new OptionsLeaf(plan: plan, content: control, op: at))),
                properties: static (held, page) => Realized(
                    plan: page,
                    tree: page.Content,
                    runtime: held.Runtime,
                    op: held.Op,
                    seat: static (plan, control, at) => new PageLeaf.Properties(
                        Value: new PropertiesLeaf(plan: plan, content: control, op: at)))))),
            key: op);
    }

    public void Dispose() => Release(owner: null);

    public Fin<Unit> Navigate(PageNav nav, Op? key = null) {
        ArgumentNullException.ThrowIfNull(nav);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Unit>.Execute(Body: () => StackedLeaf
                .ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(StackedDialogPage)))
                .Bind(page => Within(body: () => nav.Apply(owner: this, page: page, op: op), op: op))),
            key: op);
    }

    public Fin<Window> Reveal(Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Window>.Execute(Body: () => Within(
                body: () =>
                    from stacked in StackedPlan.ToFin(Fail: op.InvalidInput())
                    from page in StackedLeaf.ToFin(Fail: op.InvalidResult())
                    from window in stacked.Seat.Reveal(page: page, op: op)
                    select window,
                op: op)),
            key: op);
    }

    public Fin<Seq<Guid>> Selection(ObjectKinds filter, Op? key = null) {
        ArgumentNullException.ThrowIfNull(filter);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Seq<Guid>>.Execute(Body: () => PropertiesLeaf
                .ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(ObjectPropertiesPage.GetSelectedObjects)))
                .Bind(page => Within(body: () => op.Catch(() => Fin.Succ(value: toSeq(page.GetSelectedObjects(filter.Mask))
                    .Map(static item => item.Id)
                    .Strict())), op: op))),
            key: op);
    }

    public Fin<Unit> Modify(Func<Fin<Unit>> change, Op? key = null) {
        ArgumentNullException.ThrowIfNull(change);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Unit>.Execute(Body: () => PropertiesLeaf
                .ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(ObjectPropertiesPage.ModifyPage)))
                .Bind(page => Within(body: () => op.Catch(() => {
                    Fin<Unit>? captured = null;
                    page.ModifyPage(callbackAction: _ => captured = op.Catch(change));
                    return captured is { } result ? result : Fin.Fail<Unit>(error: op.InvalidResult());
                }), op: op))),
            key: op);
    }

    private static Fin<HostPage> Realized<TPlan>(
        TPlan plan,
        Element tree,
        ElementRuntime runtime,
        Op op,
        Func<TPlan, Control, Op, PageLeaf> seat)
        where TPlan : PagePlan =>
        tree.Realize(runtime: runtime, key: op).Bind(receipt => op.Catch(() => {
            EtoExtensions.UseRhinoStyle(receipt.Host);
            return Fin.Succ(value: new HostPage(plan: plan, leaf: seat(plan, receipt.Host, op), content: receipt));
        }).MapFail(fault => {
            receipt.Dispose();
            return fault;
        }));

    internal Fin<Unit> Retain(HostPage child, Action land, Action rollback, Op op) => Within(body: () => {
        PageOwner owner = new PageOwner.Parent(Value: this);
        Fin<Unit> claimed = child.Claim(owner: owner, op: op);
        if (claimed.IsFail) return claimed;
        Fin<Unit> landed = op.Catch(() => {
            land();
            return Fin.Succ(value: unit);
        });
        if (landed.IsFail) {
            Error primary = landed.Match(Succ: _ => op.InvalidResult(), Fail: static fault => fault);
            Fin<Unit> restored = op.Catch(() => {
                rollback();
                child.Unclaim(owner: owner);
                return Fin.Succ(value: unit);
            });
            return restored.Match(
                Succ: _ => Fin.Fail<Unit>(error: primary),
                Fail: fault => (Track(child), Fin.Fail<Unit>(error: primary + fault)).Item2);
        }
        _ = Track(child);
        return Fin.Succ(value: unit);
    }, op: op);

    private Unit Track(HostPage child) {
        lock (sync) {
            custody = custody switch {
                PageCustody.Open open => open with { Children = open.Children.Add(child) },
                PageCustody.Closing closing => closing with { Children = closing.Children.Add(child) },
                _ => custody,
            };
        }
        return unit;
    }

    private Fin<T> Within<T>(Func<Fin<T>> body, Op op) {
        lock (sync) {
            if (custody is not PageCustody.Open open)
                return Fin.Fail<T>(error: op.MissingContext());
            custody = open with { Active = open.Active + 1 };
        }
        try { return op.Catch(body); }
        finally {
            Seq<HostPage> children = Seq<HostPage>();
            bool release = false;
            lock (sync) {
                custody = custody switch {
                    PageCustody.Open open => open with { Active = open.Active - 1 },
                    PageCustody.Closing closing => closing with { Active = closing.Active - 1 },
                    _ => custody,
                };
                if (custody is PageCustody.Closing { Active: 0 } closing) {
                    children = closing.Children;
                    custody = new PageCustody.Released();
                    release = true;
                }
            }
            if (release) ReleaseTree(children);
        }
    }

    internal Fin<Unit> ClaimMount(Guid token, Op op) =>
        Claim(owner: new PageOwner.Mount(Token: token), op: op);

    internal void UnclaimMount(Guid token) =>
        Unclaim(owner: new PageOwner.Mount(Token: token));

    internal void TransferMount(Guid token) {
        lock (sync) {
            if (custody is PageCustody.Open open && Equals(open.Owner, new PageOwner.Mount(Token: token)))
                custody = open with { Owner = new PageOwner.Host() };
        }
    }

    internal void ReleaseMount(Guid token) =>
        Release(owner: new PageOwner.Mount(Token: token));

    private Fin<Unit> Claim(PageOwner owner, Op op) {
        lock (sync) {
            if (owner is PageOwner.Parent { Value: var parent } && ReferenceEquals(this, parent)
                || custody is not PageCustody.Open { Owner: null, Active: 0 } open)
                return Fin.Fail<Unit>(error: op.MissingContext());
            custody = open with { Owner = owner };
            return Fin.Succ(value: unit);
        }
    }

    private void Unclaim(PageOwner owner) {
        lock (sync) {
            if (custody is PageCustody.Open open && Equals(open.Owner, owner))
                custody = open with { Owner = null };
        }
    }

    private void Release(PageOwner? owner) {
        Seq<HostPage> children = Seq<HostPage>();
        bool release = false;
        lock (sync) {
            if (custody is not PageCustody.Open open || !Equals(open.Owner, owner)) return;
            if (open.Active > 0) {
                custody = new PageCustody.Closing(Owner: open.Owner, Active: open.Active, Children: open.Children);
            } else {
                children = open.Children;
                custody = new PageCustody.Released();
                release = true;
            }
        }
        if (release) ReleaseTree(children);
    }

    private void ReleaseTree(Seq<HostPage> children) {
        PageOwner owner = new PageOwner.Parent(Value: this);
        _ = children.Rev().Iter(child => Op.Side(() => child.Release(owner: owner)));
        content.Dispose();
    }
}

internal sealed class OptionsLeaf : OptionsDialogPage {
    private readonly PagePlan.Stacked plan;
    private readonly Control content;
    private readonly Op op;

    internal OptionsLeaf(PagePlan.Stacked plan, Control content, Op op) : base(plan.Identity.Caption.English) {
        this.plan = plan;
        this.content = content;
        this.op = op;
    }

    public override object PageControl => content;
    public override string LocalPageTitle => plan.Identity.Caption.Resolve();
    public override DrawingImage PageImage => plan.Identity.Image;
    public override bool ShowApplyButton => plan.Identity.Buttons.Contains(PageButton.Apply);
    public override bool ShowDefaultsButton => plan.Identity.Buttons.Contains(PageButton.Defaults);
    public override bool OnApply() => Answer(new PageSignal.Applied()).IsSucc;
    public override void OnCancel() => ignore(Answer(new PageSignal.Cancelled()));
    public override bool OnActivate(bool active) => Answer(new PageSignal.Activated(
        State: active ? PageActivation.Entered : PageActivation.Left)).IsSucc;
    public override Result RunScript(RhinoDoc doc, RunMode mode) =>
        SessionMode.OfRunMode(mode: mode, key: op).Bind(lane => DocKey.Of(document: doc, key: op)
            .Bind(document => Answer(new PageSignal.Scripted(Document: Some(document), Mode: lane))))
            .Match(Succ: static _ => Result.Success, Fail: static _ => Result.Failure);
    public override void OnDefaults() => ignore(Answer(new PageSignal.Reset()));
    public override void OnHelp() => ignore(Answer(new PageSignal.Helped()));
    public override void OnCreateParent(nint hwndParent) => ignore(Answer(new PageSignal.ParentCreated(Handle: hwndParent)));
    public override void OnSizeParent(int width, int height) => ignore(Answer(new PageSignal.ParentSized(Width: width, Height: height)));

    private Fin<Unit> Answer(PageSignal signal) => op.Catch(() => plan.Answer(signal));
}

internal sealed class PropertiesLeaf : ObjectPropertiesPage {
    private readonly PagePlan.Properties plan;
    private readonly Control content;
    private readonly Op op;

    internal PropertiesLeaf(PagePlan.Properties plan, Control content, Op op) {
        this.plan = plan;
        this.content = content;
        this.op = op;
    }

    public override object PageControl => content;
    public override string EnglishPageTitle => plan.Identity.Caption.English;
    public override string LocalPageTitle => plan.Identity.Caption.Resolve();
    public override int Index => plan.Identity.Index;
    public override PropertyPageType PageType => plan.Identity.Seat.Key;
    public override string PageIconEmbeddedResourceString => plan.Identity.IconResource;
    public override ObjectType SupportedTypes => plan.Scope.Kinds.Mask;
    public override bool AllObjectsMustBeSupported => plan.Scope.Policy.All;
    public override bool SupportsSubObjects => plan.Scope.Policy.Subobjects;
    public override bool OnActivate(bool active) => Answer(new PageSignal.Activated(
        State: active ? PageActivation.Entered : PageActivation.Left)).IsSucc;
    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        SessionMode.OfRunMode(mode: RunMode.Scripted, key: op)
            .Bind(mode => Answer(new PageSignal.Scripted(Document: DocumentOf(e), Mode: mode)))
            .Match(Succ: static _ => Result.Success, Fail: static _ => Result.Failure);
    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        Display(e).Match(
            Succ: static visible => visible,
            Fail: fault => (ignore(Answer(new PageSignal.Refused(Fault: fault))), false).Item2);
    public override void UpdatePage(ObjectPropertiesPageEventArgs e) =>
        ignore(WithEvidence(e, evidence => Answer(new PageSignal.SelectionUpdated(Evidence: evidence))));
    public override void OnHelp() => ignore(Answer(new PageSignal.Helped()));
    public override void OnCreateParent(nint hwndParent) => ignore(Answer(new PageSignal.ParentCreated(Handle: hwndParent)));
    public override void OnSizeParent(int width, int height) => ignore(Answer(new PageSignal.ParentSized(Width: width, Height: height)));

    private Fin<Unit> Answer(PageSignal signal) => op.Catch(() => plan.Answer(signal));

    private Fin<bool> Display(ObjectPropertiesPageEventArgs e) => WithEvidence(e, evidence =>
        from included in op.Catch(() => Fin.Succ(value: e.IncludesObjectsType(
            objectTypes: plan.Scope.Kinds.Mask,
            allMustMatch: plan.Scope.Policy.All)))
        from visible in included ? op.Catch(() => plan.Display(evidence)) : Fin.Succ(value: false)
        from shown in visible
            ? Answer(new PageSignal.SelectionShown(Evidence: evidence)).Map(static _ => true)
            : Fin.Succ(value: false)
        select shown);

    private Fin<T> WithEvidence<T>(ObjectPropertiesPageEventArgs e, Func<SelectionEvidence, Fin<T>> body) =>
        op.Catch(() => body(Evidence(e)));

    private SelectionEvidence Evidence(ObjectPropertiesPageEventArgs e) => new(
        Document: DocumentOf(e),
        EventOrdinal: e.EventRuntimeSerialNumber,
        Count: e.ObjectCount,
        Objects: toSeq(e.GetObjects(objectTypes: plan.Scope.Kinds.Mask)).Map(static item => item.Id).Strict(),
        View: Optional(e.View).Map(static view => view.RuntimeSerialNumber),
        Viewport: Optional(e.Viewport).Map(static viewport => viewport.Id));

    private static Option<DocKey> DocumentOf(ObjectPropertiesPageEventArgs e) =>
        e.DocRuntimeSerialNumber is 0u ? None : Some(DocKey.Create(value: e.DocRuntimeSerialNumber));
}
```

## [05]-[NAVIGATION]

- Owner: `PageNav` is the stacked-page operation algebra.
- Cases: activation, named or document-page reveal, removal, dirty state, retitle, child adoption, style, and sequence.
- Entry: `HostPage.Navigate` applies one case or traverses a sequence through the same fold.
- Law: `Adopt` claims child custody before host registration, records the child after landing, and removes plus unclaims it when landing fails.
- Boundary: `Styled` rejects outside Windows because the host exposes the navigation-style members only there.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PageDestination {
    public static readonly PageDestination Named = new(documentProperties: false);
    public static readonly PageDestination Document = new(documentProperties: true);

    internal bool DocumentProperties { get; }
}

[SmartEnum<bool>]
public sealed partial class PageDirty {
    public static readonly PageDirty Clean = new(false);
    public static readonly PageDirty Modified = new(true);
}

[SmartEnum<bool>]
public sealed partial class PageEmphasis {
    public static readonly PageEmphasis Regular = new(false);
    public static readonly PageEmphasis Bold = new(true);
}

public sealed record PageStyle(PageEmphasis Emphasis, DrawingColor Color);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageNav {
    private PageNav() { }
    public sealed record Activate : PageNav;
    public sealed record Reveal(HostText Title, PageDestination Destination) : PageNav;
    public sealed record Remove : PageNav;
    public sealed record Dirty(PageDirty State) : PageNav;
    public sealed record Retitle(HostText Title) : PageNav;
    public sealed record Adopt(HostPage Child) : PageNav;
    public sealed record Styled(PageStyle Style) : PageNav;
    public sealed record Sequence(Seq<PageNav> Steps) : PageNav;

    internal Fin<Unit> Apply(HostPage owner, StackedDialogPage page, Op op) =>
        Switch(
            (Owner: owner, Page: page, Op: op),
            activate: static (held, _) => Fin.Succ(value: Op.Side(held.Page.MakeActivePage)),
            reveal: static (held, nav) => held.Op.AcceptText(value: nav.Title.Resolve()).Bind(title =>
                held.Page.SetActivePageTo(pageName: title, documentPropertiesPage: nav.Destination.DocumentProperties)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: held.Op.InvalidResult(detail: title))),
            remove: static (held, _) => Fin.Succ(value: Op.Side(held.Page.RemovePage)),
            dirty: static (held, nav) => Fin.Succ(value: Op.Side(() => held.Page.Modified = nav.State.Key)),
            retitle: static (held, nav) => held.Op.AcceptText(value: nav.Title.English)
                .Map(title => Op.Side(() => held.Page.SetEnglishPageTitle(title))),
            adopt: static (held, nav) =>
                from child in Optional(nav.Child).ToFin(Fail: held.Op.InvalidInput())
                from _ in child.StackedPlan
                    .Filter(static child => child.Seat == PageSeat.Child)
                    .ToFin(Fail: held.Op.InvalidInput())
                from leaf in child.StackedLeaf.ToFin(Fail: held.Op.InvalidResult())
                from added in held.Owner.Retain(
                    child: child,
                    land: () => held.Page.AddChildPage(pageToAdd: leaf),
                    rollback: leaf.RemovePage,
                    op: held.Op)
                select added,
            styled: static (held, nav) => OperatingSystem.IsWindows()
                ? Fin.Succ(value: Op.Side(() => {
                    held.Page.NavigationTextIsBold = nav.Style.Emphasis.Key;
                    held.Page.NavigationTextColor = nav.Style.Color;
                }))
                : Fin.Fail<Unit>(error: new UiFault.Unavailable(Key: held.Op, Capability: nameof(StackedDialogPage.NavigationTextColor))),
            sequence: static (held, nav) => nav.Steps
                .TraverseM(step => step.Apply(owner: held.Owner, page: held.Page, op: held.Op))
                .As()
                .Map(static _ => unit));
}
```

## [06]-[MOUNT]

- Owner: `PageBasket` closes the host registration collection shapes.
- Entry: `PageMount.Land` applicatively pre-admits the complete batch, claims every page before mutation, and returns a completed or partial applied-prefix receipt.
- Law: stacked children land only through `PageNav.Adopt`; root stacked and object-properties pages must agree with the basket case.
- Boundary: `PageMountReceipt` owns removable options registrations and releases their page trees; object-properties registration exposes no public removal member, so custody transfers to the host collection and its applied prefix remains explicit.
- Boundary: options rollback accepts only a true `ICollection.Remove` result; false removal remains a live receipt-owned registration and joins the partial fault.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageBasket {
    private PageBasket() { }
    public sealed record Options(ICollection<OptionsDialogPage> Pages) : PageBasket;
    public sealed record Properties(ObjectPropertiesPageCollection Pages) : PageBasket;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageMountReceipt : IDisposable {
    private readonly PageMountLease lease;

    private protected PageMountReceipt(PageMountLease lease) => this.lease = lease;

    public Fin<Unit> Release(Op? key = null) => lease.Release(key.OrDefault());

    public void Dispose() => ignore(Release());

    public sealed record Completed : PageMountReceipt {
        internal Completed(int landed, PageMountLease lease) : base(lease) => Landed = landed;
        public int Landed { get; }
    }

    public sealed record Partial : PageMountReceipt {
        internal Partial(int applied, Error fault, PageMountLease lease) : base(lease) {
            Applied = applied;
            Fault = fault;
        }

        public int Applied { get; }
        public Error Fault { get; }
    }
}

internal sealed record PageLanding(HostPage Page, Action Add, Option<Func<Fin<Unit>>> Remove);

internal sealed record PageClaimState(Seq<PageLanding> Claimed, Option<Error> Fault);

internal sealed record PageLandingState(
    int Applied,
    Seq<PageRegistration> Registrations,
    Seq<PageLanding> Remaining,
    Option<Error> Fault);

internal sealed class PageRegistration {
    private readonly object sync = new();
    private readonly HostPage page;
    private readonly Guid token;
    private readonly Func<Fin<Unit>> remove;
    private bool live = true;

    internal PageRegistration(HostPage page, Guid token, Func<Fin<Unit>> remove) {
        this.page = page;
        this.token = token;
        this.remove = remove;
    }

    internal bool IsLive {
        get { lock (sync) return live; }
    }

    internal Fin<Unit> Rollback() => Close(release: false);

    internal Fin<Unit> Release() => Close(release: true);

    private Fin<Unit> Close(bool release) {
        lock (sync) {
            if (!live) return Fin.Succ(value: unit);
        }
        return remove().Map(_ => {
            lock (sync) {
                if (!live) return unit;
                if (release) page.ReleaseMount(token);
                else page.UnclaimMount(token);
                live = false;
                return unit;
            }
        });
    }
}

internal sealed class PageMountLease {
    private readonly Seq<PageRegistration> registrations;

    internal PageMountLease(Seq<PageRegistration> registrations) => this.registrations = registrations;

    internal int LiveCount => registrations.Count(static registration => registration.IsLive);

    internal Fin<Unit> Rollback(Op op) => HostThread.Run(
        work: new HostWork<Unit>.Execute(Body: () => HostThread.Release(
            releases: registrations.Map(registration => (Func<Fin<Unit>>)(registration.Rollback)),
            key: op)),
        key: op);

    internal Fin<Unit> Release(Op op) => HostThread.Run(
        work: new HostWork<Unit>.Execute(Body: () => HostThread.Release(
            releases: registrations.Map(registration => (Func<Fin<Unit>>)(registration.Release)),
            key: op)),
        key: op);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PageMount {
    public static Fin<PageMountReceipt> Land(PageBasket basket, Op? key = null, params ReadOnlySpan<HostPage> pages) {
        ArgumentNullException.ThrowIfNull(basket);
        Op op = key.OrDefault();
        Seq<HostPage> batch = toSeq(pages.ToArray()).Strict();
        return HostThread.Run(
            work: new HostWork<PageMountReceipt>.Required(Body: () => batch
                .Traverse(page => Prepared(page: page, basket: basket, op: op).ToValidation())
                .As()
                .ToFin()
                .Bind(landings => {
                    Seq<PageLanding> admitted = landings.Strict();
                    Guid token = Guid.NewGuid();
                    return Claim(landings: admitted, token: token, op: op)
                        .Bind(_ => Commit(landings: admitted, token: token, op: op));
                })),
            key: op);
    }

    private static Fin<PageLanding> Prepared(HostPage page, PageBasket basket, Op op) =>
        Optional(page).ToFin(Fail: op.InvalidInput()).Bind(admitted => basket.Switch(
            (Page: admitted, Op: op),
            options: static (held, target) =>
                from pages in Optional(target.Pages).ToFin(Fail: held.Op.InvalidInput())
                from plan in held.Page.StackedPlan.ToFin(Fail: held.Op.InvalidInput())
                from _ in guard(flag: plan.Seat == PageSeat.Options, False: held.Op.InvalidInput()).ToFin()
                from leaf in held.Page.StackedLeaf.ToFin(Fail: held.Op.InvalidResult())
                select new PageLanding(
                    Page: held.Page,
                    Add: () => pages.Add(item: leaf),
                    Remove: Some<Func<Fin<Unit>>>(() => held.Op.Catch(() => pages.Remove(item: leaf)
                        ? Fin.Succ(value: unit)
                        : Fin.Fail<Unit>(error: held.Op.InvalidResult(detail: nameof(ICollection<OptionsDialogPage>.Remove)))))),
            properties: static (held, target) =>
                from pages in Optional(target.Pages).ToFin(Fail: held.Op.InvalidInput())
                from leaf in held.Page.PropertiesLeaf.ToFin(Fail: held.Op.InvalidInput())
                select new PageLanding(
                    Page: held.Page,
                    Add: () => pages.Add(page: leaf),
                    Remove: None)));

    private static Fin<Unit> Claim(Seq<PageLanding> landings, Guid token, Op op) {
        PageClaimState state = landings.Fold(
            new PageClaimState(Claimed: Seq<PageLanding>(), Fault: None),
            (held, landing) => held.Fault.IsSome
                ? held
                : landing.Page.ClaimMount(token: token, op: op).Match(
                    Succ: _ => held with { Claimed = held.Claimed.Add(landing) },
                    Fail: fault => held with { Fault = Some(fault) }));
        return state.Fault.Match(
            Some: fault => {
                _ = state.Claimed.Iter(landing => Op.Side(() => landing.Page.UnclaimMount(token)));
                return Fin.Fail<Unit>(error: fault);
            },
            None: () => Fin.Succ(value: unit));
    }

    private static Fin<PageMountReceipt> Commit(Seq<PageLanding> landings, Guid token, Op op) {
        PageLandingState state = landings.Fold(
            new PageLandingState(
                Applied: 0,
                Registrations: Seq<PageRegistration>(),
                Remaining: landings,
                Fault: None),
            (held, landing) => held.Fault.IsSome
                ? held
                : op.Catch(() => {
                    landing.Add();
                    return Fin.Succ(value: unit);
                }).Match(
                    Succ: _ => landing.Remove.Match(
                        Some: remove => held with {
                            Applied = held.Applied + 1,
                            Registrations = held.Registrations.Add(new PageRegistration(landing.Page, token, remove)),
                            Remaining = held.Remaining.Tail,
                        },
                        None: () => {
                            landing.Page.TransferMount(token);
                            return held with { Applied = held.Applied + 1, Remaining = held.Remaining.Tail };
                        }),
                    Fail: fault => held with { Fault = Some(fault) }));
        PageMountLease lease = new(registrations: state.Registrations);
        return state.Fault.Match(
            Some: primary => {
                _ = state.Remaining.Iter(landing => Op.Side(() => landing.Page.UnclaimMount(token)));
                return lease.Rollback(op).Match<Fin<PageMountReceipt>>(
                    Succ: _ => Fin.Succ<PageMountReceipt>(value: new PageMountReceipt.Partial(
                        applied: state.Applied - state.Registrations.Count + lease.LiveCount,
                        fault: primary,
                        lease: lease)),
                    Fail: rollback => Fin.Succ<PageMountReceipt>(value: new PageMountReceipt.Partial(
                        applied: state.Applied - state.Registrations.Count + lease.LiveCount,
                        fault: primary + rollback,
                        lease: lease)));
            },
            None: () => Fin.Succ<PageMountReceipt>(value: new PageMountReceipt.Completed(
                landed: state.Applied,
                lease: lease)));
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
