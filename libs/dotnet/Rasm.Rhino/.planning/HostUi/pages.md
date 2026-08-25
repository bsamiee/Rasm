# [RASM_RHINO_HOSTUI_PAGES]

`HostPage` realizes every Rhino settings or properties page from one `PagePlan` family, keeps the host base classes behind internal leaves, and answers a kind-safe handle for navigation, selection, modification, reveal, and registration. It answers the kernel `IMount` floor, so a page joins any mount's adopted forest as itself; its own re-entrancy machine mirrors the kernel custody shape over that same floor and its `MountPhase` rows, and the registration TOKEN custody stays here because the object-properties collection publishes no removal member and a landed registration must transfer to the host rather than be released.

## [01]-[INDEX]

- [02]-[PLAN]: `PagePlan`, `PageSeat`, `PageReveal`, and the identity owners close page kind, content, chrome, selection reach, and callback policy.
- [03]-[SIGNAL]: `PageSignal` carries lifecycle, script, parent, and detached selection evidence through one answering rail.
- [04]-[REALIZATION]: `HostPage`, the internal leaves, and the custody-plus-claim gate realize the host base and expose kind-safe post-realization operations.
- [05]-[NAVIGATION]: `PageNav` folds stacked activation, reveal, removal, dirty state, title, child adoption, and navigation style.
- [06]-[MOUNT]: `PageBasket` and `PageMount.Land` register realized pages against the matching host collection and answer what a caller's own release will reach.

## [02]-[PLAN]

- Owner: `PagePlan` is the closed page declaration with `Stacked` and `Properties` cases; `PageSeat` names the host REGISTRATION target and `PageReveal` the host WINDOW a seated page opens into; `SelectionReach` is the object-properties selection-width vocabulary; `StackedIdentity` and `ObjectIdentity` carry the two chrome shapes their host bases publish.
- Cases: `Stacked` carries a seat, a reveal, and a `StackedIdentity`; `Properties` carries an `ObjectIdentity`, an `ObjectScope`, and its visibility predicate. `PageSeat` is options, document-properties, or child — exactly the three registration callbacks the host publishes — and `PageReveal` is denied, the document-properties window, or the application-preferences window.
- Entry: `PagePlan.Admit` accumulates the columns a caller can leave absent and answers the plan unchanged; a generated value object cannot exist unadmitted, so nothing here re-admits what its own construction already refused.
- Auto: `PageButton` is a capability set, so a button combination is data and no boolean pair reaches a leaf.
- Law: registration and reveal are TWO axes and were one roster. Options, document-properties, and child are the three callbacks `api-rhinocommon-plugins.md` names; preferences is a macOS reveal of an options-seated page and never a fourth callback, so a preferences page was unreachable through the only registration path that existed. The split makes the pairing data — an options-seated page reveals into the preferences window on the platform that has one — and the mount gate compares seats rather than re-deriving one by literal.
- Law: selection width is a capability SET, not a corner roster. Every corner of every-object and every-component is legal and both columns are read by the host base, so the set is the value and the four-row product vocabulary that collided with the table owner's own `CapabilitySet<SelectionAxis>` is the deleted form.
- Law: admission ACCUMULATES. A page declaration carries five independent columns a caller supplies, so a missing content tree and a missing answer delegate are reported together rather than sending the caller back twice.
- Law: the object-page kind is an ADMITTED row off the host enum, so an identity cannot be built around a page type that came from nowhere.
- Boundary: the object-type vocabulary is `Document`'s `ObjectKind`/`ObjectKinds`, composed here and never re-declared, so one set serves this page scope and the modal object asks that share the concept.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`OptionsDialogPage`, `ObjectPropertiesPage`, `PropertyPageType`, `RhinoEtoApp.DocumentPropertiesWindowForPage`/`ApplicationPreferencesWindowForPage`); `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-plugins.md` (the three page-collection callbacks); `libs/dotnet/.api/api-system-drawing-common.md` (the page image the stacked base publishes); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Validation`, `Apply`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[ComplexValueObject]`, `[UseDelegateFromConstructor]`); `Rasm/Interaction` (`ControlSpec`, `UiFault`); `Rasm/Domain` (`Op`, `ICapability`, `CapabilitySet`, `CapabilityLaw`); `Rasm/Numerics` (`Dimension`); `Rasm.Rhino/Document` (`ObjectKinds`, `DocKey`).
- Growth: a new registration target is one `PageSeat` row plus its host callback at the load root; a new reveal window is one `PageReveal` row carrying its own resolver; a new selection axis is one `SelectionReach` row and no consumer edit.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using DrawingImage = System.Drawing.Image;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PageButton : ICapability<PageButton> {
    public static readonly PageButton Apply = new(key: "apply");
    public static readonly PageButton Defaults = new(key: "defaults");

    public static CapabilityLaw<PageButton> Law => CapabilityLaw<PageButton>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PageSeat {
    public static readonly PageSeat Options = new(key: "options");
    public static readonly PageSeat Document = new(key: "document");
    public static readonly PageSeat Child = new(key: "child");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PageReveal {
    public static readonly PageReveal Denied = new(key: "denied", window: Refused);
    public static readonly PageReveal DocumentWindow = new(
        key: "document-window", window: Resolved(RhinoEtoApp.DocumentPropertiesWindowForPage));
    public static readonly PageReveal PreferencesWindow = new(
        key: "preferences-window", window: Resolved(RhinoEtoApp.ApplicationPreferencesWindowForPage));

    [UseDelegateFromConstructor]
    internal partial Fin<Window> Window(OptionsDialogPage page, Op op);

    private static Func<OptionsDialogPage, Op, Fin<Window>> Refused =>
        static (_, op) => Fin.Fail<Window>(error: new UiFault.HostRejected(
            Key: op, Detail: $"{nameof(RhinoEtoApp)} publishes no reveal window for this seat"));

    private static Func<OptionsDialogPage, Op, Fin<Window>> Resolved(Func<OptionsDialogPage, Window?> resolve) =>
        (page, op) => op.Catch(() => Optional(resolve(page)).ToFin(Fail: op.MissingContext()));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SelectionReach : ICapability<SelectionReach> {
    public static readonly SelectionReach Every = new(key: "every");
    public static readonly SelectionReach Component = new(key: "component");

    public static CapabilityLaw<SelectionReach> Law => CapabilityLaw<SelectionReach>.Open;
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

    public static Fin<ObjectPageSeat> OfHost(PropertyPageType candidate, Op? key = null) =>
        key.OrDefault().Row<PropertyPageType, ObjectPageSeat>(candidate: candidate);
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class StackedIdentity {
    public HostText Caption { get; }
    public DrawingImage Image { get; }
    public CapabilitySet<PageButton> Buttons { get; }
}

[ComplexValueObject]
public sealed partial class ObjectIdentity {
    public HostText Caption { get; }
    public string IconResource { get; }
    public Rasm.Numerics.Dimension Index { get; }
    public ObjectPageSeat Seat { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText caption,
        ref string iconResource,
        ref Rasm.Numerics.Dimension index,
        ref ObjectPageSeat seat) {
        iconResource = iconResource?.Trim() ?? string.Empty;
        validationError = iconResource.Length > 0
            ? null
            : new ValidationError(message: "Object page identity requires an embedded icon resource name.");
    }
}

[ComplexValueObject]
public sealed partial class ObjectScope {
    public ObjectKinds Kinds { get; }
    public CapabilitySet<SelectionReach> Reach { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PagePlan {
    private PagePlan() { }

    public sealed record Stacked(
        PageSeat Seat,
        PageReveal Reveal,
        StackedIdentity Identity,
        ControlSpec Content,
        Func<PageSignal, Fin<Unit>> Answer) : PagePlan;

    public sealed record Properties(
        ObjectIdentity Identity,
        ObjectScope Scope,
        ControlSpec Content,
        Func<SelectionEvidence, Fin<bool>> Display,
        Func<PageSignal, Fin<Unit>> Answer) : PagePlan;

    internal Fin<PagePlan> Admit(Op op) => Switch(
        op,
        stacked: static (held, page) => (
                held.Need(page.Seat).ToValidation(),
                held.Need(page.Reveal).ToValidation(),
                held.Need(page.Identity).ToValidation(),
                held.Need(page.Content).ToValidation(),
                held.Need(page.Answer).ToValidation())
            .Apply(static (seat, reveal, identity, content, answer) => (PagePlan)new Stacked(
                Seat: seat, Reveal: reveal, Identity: identity, Content: content, Answer: answer))
            .As()
            .ToFin(),
        properties: static (held, page) => (
                held.Need(page.Identity).ToValidation(),
                held.Need(page.Scope).ToValidation(),
                held.Need(page.Content).ToValidation(),
                held.Need(page.Display).ToValidation(),
                held.Need(page.Answer).ToValidation())
            .Apply(static (identity, scope, content, display, answer) => (PagePlan)new Properties(
                Identity: identity, Scope: scope, Content: content, Display: display, Answer: answer))
            .As()
            .ToFin());
}
```

## [03]-[SIGNAL]

- Owner: `PageSignal` closes every callback the host page bases expose; `PageActivation` names the two lifecycle edges; `SelectionEvidence` is the detached selection fact.
- Cases: activation, apply, cancel, script, defaults, help, native-parent lifecycle, selection visibility, selection refresh, and a refusal the visibility gate could not otherwise report.
- Entry: `PageSignal.Sized` is the one parent-extent admission both leaves reach, so the host's two raw pixel counts are admitted once rather than at each override.
- Auto: `SelectionEvidence` compares by its object roster in ORDER, so two evidence values naming the same objects in different sequence stay two facts and an evidence diff reads them apart.
- Law: `Scripted` carries an admitted `SessionMode`; a foreign `RunMode` never crosses a leaf.
- Law: evidence detaches IDENTITIES, never handles — a retained host object outlives the callback that produced it, and the identity is what a later read re-resolves against a live document.
- Law: the native parent handle rides a CASE and never a retained field. `OnCreateParent` hands a raw window handle the host owns for exactly the duration of the callback, so the boundary reports it and keeps nothing.
- Output: `SelectionEvidence` — document, event ordinal, count, object identities, view, and viewport, all read before callback exit.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`ObjectPropertiesPageEventArgs`); LanguageExt.Core (`Fin`, `Option`, `Seq`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`); Generator.Equals (`[Equatable]`, `[OrderedEquality]`); `Rasm/Numerics` (`Dimension`); `Rasm.Rhino/Document` (`DocKey`, `SessionMode`).
- Growth: a new callback is one `PageSignal` case breaking every answering consumer loudly; a new evidence column is one field on the record.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class PageActivation {
    public static readonly PageActivation Left = new(false);
    public static readonly PageActivation Entered = new(true);
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
    public sealed record ParentSized(Rasm.Numerics.Dimension Width, Rasm.Numerics.Dimension Height) : PageSignal;
    public sealed record SelectionShown(SelectionEvidence Evidence) : PageSignal;
    public sealed record SelectionUpdated(SelectionEvidence Evidence) : PageSignal;
    public sealed record Refused(Error Fault) : PageSignal;

    internal static Fin<PageSignal> Sized(int width, int height, Op op) =>
        from measured in op.AcceptValidated<Rasm.Numerics.Dimension>(width)
        from tall in op.AcceptValidated<Rasm.Numerics.Dimension>(height)
        select (PageSignal)new ParentSized(Width: measured, Height: tall);
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record SelectionEvidence(
    Option<DocKey> Document,
    uint EventOrdinal,
    Rasm.Numerics.Dimension Count,
    [property: OrderedEquality] Seq<Guid> Objects,
    Option<uint> View,
    Option<Guid> Viewport);
```

## [04]-[REALIZATION]

- Owner: `HostPage` is the realized handle and the estate's page `IMount`; `PageLeaf` closes the internal host-base alternatives; `PageOwner` names who claimed a registration; `PageCustody` is the re-entrancy and adoption machine over the kernel mount floor.
- Entry: `Realize` is the sole page mint and runs only inside an existing command-thread frame; `Navigate`, `Reveal`, `Selection`, and `Modify` expose the distinct result regimes of the handle; `Release` is the `IMount` teardown every owner reaches through.
- Auto: realization brackets its own unwind through the leased `ElementMount`, so a refused style hop releases the control tree it had already grown, the cleanup fault aggregates into the primary, and the hand dispose-then-return block has no site.
- Law: this page answers the kernel `IMount` floor, so it is adoptable by every mount in the estate as ITSELF, its child forest is `Seq<IMount>`, and its phase rows are the kernel `MountPhase` with the `Closes` consequence each row carries. NAMED LOSS: none — the local two-row phase vocabulary deletes.
- Law: the re-entrancy machine MIRRORS the kernel custody shape rather than composing it, and the discriminant is an access modifier, not a design: `MountCustody` publishes its enter, leave, close, and adopt transitions `internal` to the kernel assembly, so a boundary mount can answer the floor and hold the phase rows but cannot step the machine. Every transition here takes and answers the kernel's own public types, so the local machine deletes with no call-site change the moment those transitions publish.
- Law: the claim cell is the other half and stays host-specific by CONCEPT: the kernel custody owner band is an `IMount`, while a registration is claimed by a MOUNT TOKEN or transferred to the HOST COLLECTION, neither of which is a mount this page could hand it. Enter, leave, close, and adopt are custody; claim, unclaim, and transfer are the token's.
- Law: a claim is answered by the CLAIMANT, so a close arriving from an owner that never claimed this page refuses rather than silently no-opping — the gap where one owner's release tore down a page another had claimed.
- Law: a leave with no matching enter REFUSES. The visit count is absent rather than zero, so it can no longer run negative and reach a release nothing entered, and the caller reads which page answered instead of inferring it from a later double-dispose.
- Law: each host override calls the plan's `Answer` once and collapses the rail only at the host return type the base fixed.
- Law: visibility conjoins the host type filter, the plan predicate, and the `SelectionShown` answer; refresh emits `SelectionUpdated` alone.
- Law: `Modify` captures the callback's own verdict through a seated cell and refuses a host call that returned without invoking it, because the host publishes no return for the change it ran.
- Law: `Release` and operation exits carry teardown faults on their typed rails; only the host-required `IDisposable` adapter parks a discarded verdict on the bounded ring.
- Exemption: the custody seat is `lock`-held rather than compare-and-swap, because the release it hands back disposes host controls and a replayable transition body would run that disposal on every contended retry. It is contained in this class and no consumer writes one.
- Boundary: `Realize` and every entry below are the published surface a command body in the `apps/<app>/` plugin shell composes; the app root is the sole producer and no in-package fence mints a page.
- Output: `HostPage` retains its leased `ElementMount` and publishes its release faults; the handle IS the evidence and no second value exists.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`OptionsDialogPage`, `ObjectPropertiesPage`, `ObjectPropertiesPageEventArgs`, `StackedDialogPage`, `EtoExtensions.UseRhinoStyle`); `libs/dotnet/Rasm.Rhino/.api/api-eto-forms.md` (`Control`, `Window`); LanguageExt.Core (`Fin`, `Option`, `Atom`, `Seq`); Thinktecture.Runtime.Extensions (`[Union]`); `Rasm/Interaction` (`ControlForge.Realize`, `ElementMount`, `ElementRuntime`, `IMount`, `MountPhase`, `UiFault`); `Rasm/Domain` (`Op`, `Cell`, `Transition`, `Ring<Error>`, `Lease<T>.Use`); `Rasm/Numerics` (`Dimension`); `Rasm.Rhino/Document` (`ObjectKinds`, `DocKey`, `SessionMode`).
- Growth: a new post-realization regime is one entry over the same custody window; a new host base is one `PageLeaf` case with its own leaf class; a new lifecycle phase is one kernel `MountPhase` row and no edit here.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PageLeaf {
    private PageLeaf() { }
    internal sealed record Stacked(OptionsDialogPage Value) : PageLeaf;
    internal sealed record Properties(ObjectPropertiesPage Value) : PageLeaf;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PageOwner {
    private PageOwner() { }
    internal sealed record Parent(HostPage Value) : PageOwner;
    internal sealed record Mount(Guid Token) : PageOwner;
    internal sealed record Host : PageOwner;

    internal bool Owns(HostPage page) => Switch(
        page,
        parent: static (held, row) => ReferenceEquals(held, row.Value),
        mount: static (_, _) => false,
        host: static (_, _) => false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PageCustody {
    private PageCustody() { }
    internal sealed record Live(
        Option<Rasm.Numerics.Dimension> Active, Seq<IMount> Children, MountPhase Phase) : PageCustody;
    internal sealed record Released : PageCustody;

    internal Option<PageCustody> Entered() => Switch(
        live: static row => row.Phase.Closes
            ? Option<PageCustody>.None
            : Some<PageCustody>(row with {
                Active = Some(Rasm.Numerics.Dimension.Create(value: row.Active.Map(static held => held.Value).IfNone(0) + 1)),
            }),
        released: static _ => Option<PageCustody>.None);

    internal Fin<(PageCustody Next, Option<Seq<IMount>> Release)> Left(Op key) => Switch(
        state: key,
        live: static (op, row) => row.Active.Match(
            Some: held => held.Value is 1
                ? row.Phase.Closes
                    ? Fin.Succ(((PageCustody)new Released(), Some(row.Children)))
                    : Fin.Succ(((PageCustody)(row with { Active = None }), Option<Seq<IMount>>.None))
                : Fin.Succ((
                    (PageCustody)(row with { Active = Some(Rasm.Numerics.Dimension.Create(value: held.Value - 1)) }),
                    Option<Seq<IMount>>.None)),
            None: () => Fin.Fail<(PageCustody, Option<Seq<IMount>>)>(new UiFault.Released(Key: op))),
        released: static (op, _) => Fin.Fail<(PageCustody, Option<Seq<IMount>>)>(new UiFault.Released(Key: op)));

    internal (PageCustody Next, Option<Seq<IMount>> Release) Closed() => Switch(
        live: static row => row.Phase.Closes
            ? ((PageCustody)row, Option<Seq<IMount>>.None)
            : row.Active.IsSome
                ? ((PageCustody)(row with { Phase = MountPhase.Closing }), Option<Seq<IMount>>.None)
                : ((PageCustody)new Released(), Some(row.Children)),
        released: static row => ((PageCustody)row, Option<Seq<IMount>>.None));

    internal PageCustody Adopted(IMount child) => Switch(
        state: child,
        live: static (held, row) => (PageCustody)(row with { Children = row.Children.Add(held) }),
        released: static (_, row) => row);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class HostPage : IMount, IDisposable {
    private static readonly Rasm.Numerics.Dimension TeardownCap = Rasm.Numerics.Dimension.Create(value: 32);

    private readonly PagePlan plan;
    private readonly PageLeaf leaf;
    private readonly Lease<ElementMount> content;
    private readonly object sync = new();
    private readonly Atom<Option<PageOwner>> claim = Atom(Option<PageOwner>.None);
    private readonly Ring<Error> teardown = new(cap: TeardownCap);
    private PageCustody custody = new PageCustody.Live(
        Active: None, Children: Seq<IMount>(), Phase: MountPhase.Open);

    private HostPage(PagePlan plan, PageLeaf leaf, Lease<ElementMount> content, Op key) =>
        (this.plan, this.leaf, this.content, Key) = (plan, leaf, content, key);

    public Op Key { get; }

    public PagePlan Plan => plan;

    public Seq<Error> ReleaseFaults => teardown.Parked;

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
        Op op = key.OrDefault();
        return op.Accept<object>(plan, runtime).Bind(_ => HostThread.Run(
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
            key: op));
    }

    public Fin<Unit> Release() => Release(owner: None);

    public void Dispose() => _ = Release(owner: None)
        .IfFail(fault => ignore(teardown.Park(item: fault)));

    public Fin<Unit> Navigate(PageNav nav, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(nav).Bind(_ => HostThread.Run(
            work: new HostWork<Unit>.Execute(Body: () => StackedLeaf
                .ToFin(Fail: Absent(nameof(StackedDialogPage), op))
                .Bind(page => Within(body: () => nav.Apply(owner: this, page: page, op: op), op: op))),
            key: op));
    }

    public Fin<Window> Reveal(Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Window>.Execute(Body: () => Within(
                body: () =>
                    from stacked in StackedPlan.ToFin(Fail: op.InvalidInput())
                    from page in StackedLeaf.ToFin(Fail: op.InvalidResult())
                    from window in stacked.Reveal.Window(page: page, op: op)
                    select window,
                op: op)),
            key: op);
    }

    public Fin<Seq<Guid>> Selection(ObjectKinds filter, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(filter).Bind(_ => HostThread.Run(
            work: new HostWork<Seq<Guid>>.Execute(Body: () => PropertiesLeaf
                .ToFin(Fail: Absent(nameof(ObjectPropertiesPage.GetSelectedObjects), op))
                .Bind(page => Within(
                    body: () => op.Catch(() => Fin.Succ(value: toSeq(page.GetSelectedObjects(filter.Mask))
                        .Map(static item => item.Id)
                        .Strict())),
                    op: op))),
            key: op));
    }

    public Fin<Unit> Modify(Func<Fin<Unit>> change, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(change).Bind(_ => HostThread.Run(
            work: new HostWork<Unit>.Execute(Body: () => PropertiesLeaf
                .ToFin(Fail: Absent(nameof(ObjectPropertiesPage.ModifyPage), op))
                .Bind(page => Within(body: () => Modified(page: page, change: change, op: op), op: op))),
            key: op));
    }

    private static Fin<Unit> Modified(ObjectPropertiesPage page, Func<Fin<Unit>> change, Op op) {
        Atom<Option<Fin<Unit>>> captured = Atom(Option<Fin<Unit>>.None);
        return op.Catch(() => {
            page.ModifyPage(callbackAction: _ => ignore(Cell.Seat(captured, () => op.Catch(change))));
            return Cell.Take(captured).Current.ToFin(Fail: op.InvalidResult()).Bind(static held => held);
        });
    }

    private static Fin<HostPage> Realized<TPlan>(
        TPlan plan,
        ControlSpec tree,
        ElementRuntime runtime,
        Op op,
        Func<TPlan, Control, Op, PageLeaf> seat)
        where TPlan : PagePlan =>
        ControlForge.Realize(spec: tree, runtime: runtime, key: op).Bind(outcome => op
            .Catch(() => {
                EtoExtensions.UseRhinoStyle(outcome.Resource.Host);
                return Fin.Succ(value: new HostPage(
                    plan: plan, leaf: seat(plan, outcome.Resource.Host, op), content: outcome, key: op));
            })
            .Match(
                Succ: page => Fin.Succ(value: page),
                Fail: fault => outcome.Use(_ => Fin.Fail<HostPage>(error: fault), op)));

    internal Fin<Unit> Retain(HostPage child, Action land, Action rollback, Op op) => Within(
        body: () => {
            PageOwner owner = new PageOwner.Parent(Value: this);
            return child.Claim(owner: owner, op: op).Bind(_ => op
                .Catch(() => Fin.Succ(value: Op.Side(land)))
                .Match(
                    Succ: _ => Fin.Succ(value: Track(child)),
                    Fail: primary => op
                        .Catch(() => Fin.Succ(value: (Op.Side(rollback), child.Unclaim(owner)).Item2))
                        .Match(
                            Succ: _ => Fin.Fail<Unit>(error: primary),
                            Fail: cleanup => (Track(child), Fin.Fail<Unit>(error: primary + cleanup)).Item2)));
        },
        op: op);

    private Unit Track(HostPage child) {
        lock (sync) custody = custody.Adopted(child);
        return unit;
    }

    private Fin<T> Within<T>(Func<Fin<T>> body, Op op) {
        lock (sync) {
            if (custody.Entered().Case is not PageCustody entered) return Fin.Fail<T>(error: new UiFault.Released(Key: op));
            custody = entered;
        }
        Fin<T> primary = op.Catch(body);
        Fin<Option<Seq<IMount>>> exited;
        lock (sync) {
            exited = custody.Left(key: op).Map(step => {
                custody = step.Next;
                return step.Release;
            });
        }
        return primary.Settled(
            release: () => exited.Bind(release => release.Match(
                Some: children => ReleaseTree(children: children, key: op),
                None: static () => Fin.Succ(unit))),
            key: op);
    }

    internal Fin<Unit> ClaimMount(Guid token, Op op) => Claim(owner: new PageOwner.Mount(Token: token), op: op);

    internal Unit UnclaimMount(Guid token) => Unclaim(owner: new PageOwner.Mount(Token: token));

    internal Unit TransferMount(Guid token) => ignore(Cell.Step(
        cell: claim,
        step: held => held == Some<PageOwner>(new PageOwner.Mount(Token: token))
            ? Some(Some<PageOwner>(new PageOwner.Host()))
            : Option<Option<PageOwner>>.None,
        declined: Contested(Key)));

    internal Fin<Unit> ReleaseMount(Guid token) => Release(owner: Some<PageOwner>(new PageOwner.Mount(Token: token)));

    private Fin<Unit> Claim(PageOwner owner, Op op) => Cell.Step(
            cell: claim,
            step: held => held.IsNone && !owner.Owns(this) ? Some(Some(owner)) : Option<Option<PageOwner>>.None,
            declined: Contested(op))
        is Transition<Option<PageOwner>>.Committed
        ? Fin.Succ(value: unit)
        : Fin.Fail<Unit>(error: Contested(op));

    private Unit Unclaim(PageOwner owner) => ignore(Cell.Step(
        cell: claim,
        step: held => held == Some(owner) ? Some(Option<PageOwner>.None) : Option<Option<PageOwner>>.None,
        declined: Contested(Key)));

    private Fin<Unit> Release(Option<PageOwner> owner) {
        if (owner != claim.Value) return Fin.Fail<Unit>(error: Contested(Key));
        Option<Seq<IMount>> release;
        lock (sync) { (custody, release) = custody.Closed(); }
        return release.Match(
            Some: children => ReleaseTree(children: children, key: Key),
            None: static () => Fin.Succ(unit));
    }

    private Fin<Unit> ReleaseTree(Seq<IMount> children, Op key) =>
        Custody.Release(held: children, release: static child => child.Release(), key: key)
            .Settled(release: () => content.Use(outcome => outcome.Release(), key), key: key);

    private static Error Absent(string member, Op op) =>
        new UiFault.HostRejected(Key: op, Detail: $"this page publishes no {member}");

    private static Error Contested(Op op) =>
        new UiFault.HostRejected(Key: op, Detail: $"a {nameof(PageOwner)} other than the claimant answered");
}

internal sealed class OptionsLeaf : OptionsDialogPage {
    private readonly PagePlan.Stacked plan;
    private readonly Control content;
    private readonly Op op;

    internal OptionsLeaf(PagePlan.Stacked plan, Control content, Op op) : base(plan.Identity.Caption.English) =>
        (this.plan, this.content, this.op) = (plan, content, op);

    public override object PageControl => content;
    public override string LocalPageTitle => plan.Identity.Caption.Resolve();
    public override DrawingImage PageImage => plan.Identity.Image;
    public override bool ShowApplyButton => plan.Identity.Buttons.Admits(PageButton.Apply);
    public override bool ShowDefaultsButton => plan.Identity.Buttons.Admits(PageButton.Defaults);
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
    public override void OnSizeParent(int width, int height) =>
        ignore(PageSignal.Sized(width: width, height: height, op: op).Bind(Answer));

    private Fin<Unit> Answer(PageSignal signal) => op.Catch(() => plan.Answer(signal));
}

internal sealed class PropertiesLeaf : ObjectPropertiesPage {
    private readonly PagePlan.Properties plan;
    private readonly Control content;
    private readonly Op op;

    internal PropertiesLeaf(PagePlan.Properties plan, Control content, Op op) =>
        (this.plan, this.content, this.op) = (plan, content, op);

    public override object PageControl => content;
    public override string EnglishPageTitle => plan.Identity.Caption.English;
    public override string LocalPageTitle => plan.Identity.Caption.Resolve();
    public override int Index => plan.Identity.Index.Value;
    public override PropertyPageType PageType => plan.Identity.Seat.Key;
    public override string PageIconEmbeddedResourceString => plan.Identity.IconResource;
    public override ObjectType SupportedTypes => plan.Scope.Kinds.Mask;
    public override bool AllObjectsMustBeSupported => plan.Scope.Reach.Admits(SelectionReach.Every);
    public override bool SupportsSubObjects => plan.Scope.Reach.Admits(SelectionReach.Component);
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
    public override void OnSizeParent(int width, int height) =>
        ignore(PageSignal.Sized(width: width, height: height, op: op).Bind(Answer));

    private Fin<Unit> Answer(PageSignal signal) => op.Catch(() => plan.Answer(signal));

    private Fin<bool> Display(ObjectPropertiesPageEventArgs e) => WithEvidence(e, evidence =>
        from included in op.Catch(() => Fin.Succ(value: e.IncludesObjectsType(
            objectTypes: plan.Scope.Kinds.Mask,
            allMustMatch: plan.Scope.Reach.Admits(SelectionReach.Every))))
        from visible in included ? op.Catch(() => plan.Display(evidence)) : Fin.Succ(value: false)
        from shown in visible
            ? Answer(new PageSignal.SelectionShown(Evidence: evidence)).Map(static _ => true)
            : Fin.Succ(value: false)
        select shown);

    private Fin<T> WithEvidence<T>(ObjectPropertiesPageEventArgs e, Func<SelectionEvidence, Fin<T>> body) =>
        op.Catch(() => Evidence(e).Bind(body));

    private Fin<SelectionEvidence> Evidence(ObjectPropertiesPageEventArgs e) =>
        from count in op.AcceptValidated<Rasm.Numerics.Dimension>(e.ObjectCount)
        select new SelectionEvidence(
            Document: DocumentOf(e),
            EventOrdinal: e.EventRuntimeSerialNumber,
            Count: count,
            Objects: toSeq(e.GetObjects(objectTypes: plan.Scope.Kinds.Mask)).Map(static item => item.Id).Strict(),
            View: Optional(e.View).Map(static view => view.RuntimeSerialNumber),
            Viewport: Optional(e.Viewport).Map(static viewport => viewport.Id));

    private static Option<DocKey> DocumentOf(ObjectPropertiesPageEventArgs e) =>
        e.DocRuntimeSerialNumber is 0u ? None : Some(DocKey.Create(value: e.DocRuntimeSerialNumber));
}
```

## [05]-[NAVIGATION]

- Owner: `PageNav` is the stacked-page operation algebra; `PageStyle` carries the navigation emphasis and ink; `PageDirty`, `PageEmphasis`, and `PageDestination` key the three host flag slots.
- Cases: activation, named or document-page reveal, removal, dirty state, retitle, child adoption, style, and a sequence that folds the same algebra.
- Entry: `HostPage.Navigate` applies one case or traverses a sequence through the same fold.
- Auto: every `void` host member rides `Op.Side`, so the fold is total over eight cases with no catch-all and a new verb breaks it loudly.
- Law: `Adopt` claims child custody BEFORE host registration, records the child after landing, and both removes and unclaims it when landing fails — the three-step inverse a partial adoption would otherwise leave half-run.
- Law: the navigation ink is a `PerceptualColor` and quantizes at the host slot alone, so no host colour crosses this owner's public signature.
- Law: the navigation-style members exist on the platforms whose toolkit backend the host resolved, and that roster is DECLARED rather than probed. An ambient operating-system test answers which system is running, not which backend published the member, and the platform owner already holds the admitted answer.
- Boundary: the child page's own seat is checked before adoption, so a page seated for options or document properties cannot be added as a child of a stacked page.
- Output: `Fin<Unit>` per verb — the host publishes no navigation evidence and a fabricated outcome would assert one.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`StackedDialogPage.MakeActivePage`/`SetActivePageTo`/`RemovePage`/`AddChildPage`/`Modified`/`SetEnglishPageTitle`/`NavigationTextIsBold`/`NavigationTextColor`); LanguageExt.Core (`Fin`, `Seq`, `TraverseM`); `Rasm/Interaction` (`HostPlatform.Snapshot`, `PlatformRow`, `UiFault`); `Rasm/Numerics` (`PerceptualColor`).
- Growth: a new stacked verb is one `PageNav` case with one arm; a new backend publishing the style members is one row in the declared set.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class PageDestination {
    public static readonly PageDestination Named = new(false);
    public static readonly PageDestination Document = new(true);
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PageStyle(PageEmphasis Emphasis, PerceptualColor Color);

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

    private static readonly Seq<PlatformRow> Styling = Seq(PlatformRow.WinForms, PlatformRow.Wpf);

    internal Fin<Unit> Apply(HostPage owner, StackedDialogPage page, Op op) =>
        Switch(
            (Owner: owner, Page: page, Op: op),
            activate: static (held, _) => Fin.Succ(value: Op.Side(held.Page.MakeActivePage)),
            reveal: static (held, nav) => held.Op.AcceptText(value: nav.Title.Resolve()).Bind(title =>
                held.Page.SetActivePageTo(pageName: title, documentPropertiesPage: nav.Destination.Key)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: held.Op.InvalidResult(detail: title))),
            remove: static (held, _) => Fin.Succ(value: Op.Side(held.Page.RemovePage)),
            dirty: static (held, nav) => Fin.Succ(value: Op.Side(() => held.Page.Modified = nav.State.Key)),
            retitle: static (held, nav) => held.Op.AcceptText(value: nav.Title.English)
                .Map(title => Op.Side(() => held.Page.SetEnglishPageTitle(title))),
            adopt: static (held, nav) =>
                from child in held.Op.Need(nav.Child)
                from _ in child.StackedPlan
                    .Filter(static seated => seated.Seat == PageSeat.Child)
                    .ToFin(Fail: held.Op.InvalidInput())
                from leaf in child.StackedLeaf.ToFin(Fail: held.Op.InvalidResult())
                from added in held.Owner.Retain(
                    child: child,
                    land: () => held.Page.AddChildPage(pageToAdd: leaf),
                    rollback: leaf.RemovePage,
                    op: held.Op)
                select added,
            styled: static (held, nav) =>
                from platform in HostPlatform.Snapshot(key: held.Op)
                from _ in platform.Row.Filter(Styling.Contains).ToFin(Fail: new UiFault.HostRejected(
                    Key: held.Op,
                    Detail: $"{nameof(StackedDialogPage.NavigationTextColor)} is published by "
                        + string.Join(", ", Styling.Map(static row => row.Key))))
                from ink in nav.Style.Color.ToDrawing(key: held.Op)
                select Op.Side(() => {
                    held.Page.NavigationTextIsBold = nav.Style.Emphasis.Key;
                    held.Page.NavigationTextColor = ink;
                }),
            sequence: static (held, nav) => nav.Steps
                .TraverseM(step => step.Apply(owner: held.Owner, page: held.Page, op: held.Op))
                .As()
                .Map(static _ => unit));
}
```

## [06]-[MOUNT]

- Owner: `PageBasket` closes the host registration collection shapes; `MountedPages` is the live registration custody a caller releases through; `PageRegistration` is one removable registration under its own state machine; `PageMountLease` is the registration bundle; `PageMount.Land` is the entry.
- Entry: `Land` pre-admits the whole batch applicatively, claims every page under one token, then commits by a halting fold and answers `MountedPages` naming what a release will actually reach.
- Auto: the claim fold's rollback is the kernel mount fold's own shape — a refusal unclaims every seat already taken, in reverse, with every step running — so the hand fold-state record that carried a fault sentinel has no site.
- Auto: the pending set on a refusal is what the fold has NOT landed, read off the landed counts rather than mirrored on the state; a stored remaining column and the counts beside it were two authorities over one position.
- Law: the basket carries its own SEAT, so the registration target is data the gate compares rather than a literal one side re-derives. The document-properties callback and the options callback hand the same collection type, and a gate comparing against one literal refused every page seated for the other — a live refusal on the only registration path a document page had.
- Law: `MountedPages` states what a release REACHES. The object-properties collection publishes no removal member, so its registrations are permanent and their custody transfers to the host; splitting the applied count into releasable and permanent is how a caller reads its own reach instead of watching `Release` no-op.
- Law: `MountedPages` is a CLASS, never a record — it holds a live lease, and two values naming one applied count are not one registration, so the completed and partial regimes are the fault slot's presence on one carrier rather than structurally equal cases over live custody.
- Law: a registration's lifecycle is a closed STATE, never a shutdown flag — live, unclaimed, or released — and the transition is a guarded step whose declined arm is READ, because a boolean latch reports success to a second releaser that never won the transition.
- Boundary: options rollback accepts only a true removal result; a false removal leaves a live mount-owned registration and joins the partial fault.
- Boundary: `Land` is reached through the load root's page-callback program, which is an `apps/<app>/` plugin-shell delegate — the app root is the sole producer and no in-package fence calls it.
- Output: `MountedPages` — releasable count, permanent count, and the fault of a partial landing, over a live lease it alone releases.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-plugins.md` (the three page-collection callbacks and the absent removal overload on `ObjectPropertiesPageCollection`); `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`ObjectPropertiesPageCollection.Add`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Atom`, `Traverse`, `foldWhile`); Thinktecture.Runtime.Extensions (`[Union]`); `Rasm/Domain` (`Op`, `Cell`, `Transition`); `Rasm/Numerics` (`Dimension`); `Rasm/Interaction` (`UiFault`).
- Growth: a new host collection is one `PageBasket` case with its own landing pair; a new registration state is one `RegistrationState` case breaking every transition loudly.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageBasket {
    private PageBasket() { }
    public sealed record Stacked(ICollection<OptionsDialogPage> Pages, PageSeat Seat) : PageBasket;
    public sealed record Properties(ObjectPropertiesPageCollection Pages) : PageBasket;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record RegistrationState {
    private RegistrationState() { }
    internal sealed record Live : RegistrationState;
    internal sealed record Unclaimed : RegistrationState;
    internal sealed record Released : RegistrationState;
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record PageLanding(HostPage Page, Action Add, Option<Func<Fin<Unit>>> Remove);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class MountedPages : IDisposable {
    private readonly PageMountLease lease;

    internal MountedPages(Rasm.Numerics.Dimension releasable, Rasm.Numerics.Dimension permanent, Option<Error> fault, PageMountLease lease) =>
        (this.lease, Releasable, Permanent, Fault) = (lease, releasable, permanent, fault);

    public Rasm.Numerics.Dimension Releasable { get; }
    public Rasm.Numerics.Dimension Permanent { get; }
    public Rasm.Numerics.Dimension Applied => Rasm.Numerics.Dimension.Create(value: Releasable.Value + Permanent.Value);

    public Option<Error> Fault { get; }

    public Fin<Unit> Release(Op? key = null) => lease.Release(key.OrDefault());

    public void Dispose() => ignore(Release());
}

internal sealed class PageRegistration {
    private readonly Atom<RegistrationState> state = Atom<RegistrationState>(new RegistrationState.Live());
    private readonly HostPage page;
    private readonly Guid token;
    private readonly Func<Fin<Unit>> remove;

    internal PageRegistration(HostPage page, Guid token, Func<Fin<Unit>> remove) =>
        (this.page, this.token, this.remove) = (page, token, remove);

    internal bool IsLive => state.Value is RegistrationState.Live;

    internal Fin<Unit> Unclaim(Op key) => Close(next: new RegistrationState.Unclaimed(), key: key);

    internal Fin<Unit> Release(Op key) => Close(next: new RegistrationState.Released(), key: key);

    private Fin<Unit> Close(RegistrationState next, Op key) => Cell.Step(
            cell: state,
            step: held => held is RegistrationState.Live ? Some(next) : Option<RegistrationState>.None,
            declined: new UiFault.Released(Key: key))
        is Transition<RegistrationState>.Committed
        ? remove().Bind(_ => next is RegistrationState.Released
            ? page.ReleaseMount(token)
            : Fin.Succ(value: page.UnclaimMount(token)))
        : Fin.Succ(value: unit);
}

internal sealed class PageMountLease {
    private readonly Seq<PageRegistration> registrations;

    internal PageMountLease(Seq<PageRegistration> registrations) => this.registrations = registrations;

    internal Rasm.Numerics.Dimension LiveCount => Rasm.Numerics.Dimension.Create(value: registrations.Count(static row => row.IsLive));

    internal Fin<Unit> Unclaim(Op op) => Drain(close: registration => registration.Unclaim(op), op: op);

    internal Fin<Unit> Release(Op op) => Drain(close: registration => registration.Release(op), op: op);

    private Fin<Unit> Drain(Func<PageRegistration, Fin<Unit>> close, Op op) => HostThread.Run(
        work: new HostWork<Unit>.Execute(Body: () => HostThread.Release(
            releases: registrations.Map(registration => (Func<Fin<Unit>>)(() => close(registration))),
            key: op)),
        key: op);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PageMount {
    public static Fin<MountedPages> Land(PageBasket basket, Seq<HostPage> pages, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(basket).Bind(_ => HostThread.Run(
            work: new HostWork<MountedPages>.Required(Body: () => pages
                .Traverse(page => Prepared(page: page, basket: basket, op: op).ToValidation())
                .As()
                .ToFin()
                .Bind(landings => {
                    Seq<PageLanding> admitted = landings.Strict();
                    Guid token = Guid.NewGuid();
                    return Claim(landings: admitted, token: token, op: op)
                        .Bind(_ => Commit(landings: admitted, token: token, op: op));
                })),
            key: op));
    }

    private static Fin<PageLanding> Prepared(HostPage page, PageBasket basket, Op op) =>
        op.Need(page).Bind(admitted => basket.Switch(
            (Page: admitted, Op: op),
            stacked: static (held, target) =>
                from pages in held.Op.Need(target.Pages)
                from plan in held.Page.StackedPlan.ToFin(Fail: held.Op.InvalidInput())
                from _ in guard(flag: plan.Seat == target.Seat, False: held.Op.InvalidInput()).ToFin()
                from leaf in held.Page.StackedLeaf.ToFin(Fail: held.Op.InvalidResult())
                select new PageLanding(
                    Page: held.Page,
                    Add: () => pages.Add(item: leaf),
                    Remove: Some<Func<Fin<Unit>>>(() => held.Op.Catch(() => held.Op.Confirm(
                        success: pages.Remove(item: leaf))))),
            properties: static (held, target) =>
                from pages in held.Op.Need(target.Pages)
                from leaf in held.Page.PropertiesLeaf.ToFin(Fail: held.Op.InvalidInput())
                select new PageLanding(
                    Page: held.Page,
                    Add: () => pages.Add(page: leaf),
                    Remove: None)));

    private static Fin<Unit> Claim(Seq<PageLanding> landings, Guid token, Op op) =>
        landings.Fold(Fin.Succ(Seq<PageLanding>()), (held, landing) => held.Bind(taken => landing.Page
            .ClaimMount(token: token, op: op)
            .Match(
                Succ: _ => Fin.Succ(taken.Add(landing)),
                Fail: fault => (
                    taken.Rev().Iter(seated => ignore(seated.Page.UnclaimMount(token))),
                    Fin.Fail<Seq<PageLanding>>(error: fault)).Item2)))
            .Map(static _ => unit);

    private static Fin<MountedPages> Commit(Seq<PageLanding> landings, Guid token, Op op) {
        (Rasm.Numerics.Dimension Releasable, Rasm.Numerics.Dimension Permanent, Seq<PageRegistration> Registrations, Option<Error> Fault) seed = (
            Releasable: Rasm.Numerics.Dimension.Create(value: 0),
            Permanent: Rasm.Numerics.Dimension.Create(value: 0),
            Registrations: Seq<PageRegistration>(),
            Fault: None);
        var state = foldWhile(
            (held, landing) => op.Catch(() => Fin.Succ(value: Op.Side(landing.Add))).Match(
                Succ: _ => landing.Remove.Match(
                    Some: remove => held with {
                        Releasable = Rasm.Numerics.Dimension.Create(value: held.Releasable.Value + 1),
                        Registrations = held.Registrations.Add(new PageRegistration(landing.Page, token, remove)),
                    },
                    None: () => (landing.Page.TransferMount(token), held with {
                        Permanent = Rasm.Numerics.Dimension.Create(value: held.Permanent.Value + 1),
                    }).Item2),
                Fail: fault => held with { Fault = Some(fault) }),
            static step => step.State.Fault.IsNone,
            seed,
            landings);
        PageMountLease lease = new(registrations: state.Registrations);
        return state.Fault.Match(
            Some: primary => {
                _ = landings
                    .Skip(state.Releasable.Value + state.Permanent.Value)
                    .Iter(landing => ignore(landing.Page.UnclaimMount(token)));
                Error fault = lease.Unclaim(op).Match(Succ: _ => primary, Fail: unclaim => primary + unclaim);
                return Fin.Succ(value: new MountedPages(
                    releasable: lease.LiveCount, permanent: state.Permanent, fault: Some(fault), lease: lease));
            },
            None: () => Fin.Succ(value: new MountedPages(
                releasable: state.Releasable, permanent: state.Permanent, fault: None, lease: lease)));
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
