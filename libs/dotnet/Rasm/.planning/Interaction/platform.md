# [RASM_PLATFORM]

`Rasm.Interaction` owns the ambient host backend seam every interaction surface stands on: which platform is live and what it admits, how a widget's backend handler is registered and resolved with its custody intact, how a raw native view crosses into the managed tree and back out, and how a keyed style claims its identity and rebroadcasts a theme. Rows answer every platform question — never a string comparison or a scattered `IsMac` predicate — and every registration this page performs returns its own inverse because the host registries it writes into are append-only and publish no removal.

Grasshopper held the platform seam and Rhino the theme estate. Grasshopper carried the typed snapshot, the polymorphic capability demand behind one gate, the context window, the handler-identity capsule with its native handle and control object, the leased mint census over both platform raise points, the two style mint arms — widget facade and concrete handler — the injective ledger, and the provider swap; Rhino carried the three-case handler demand with its matching custody triple, the platform-context and worker scopes, the eager-and-deferred native mount with its attachment lease, and the whole theme grid with its plugin-scoped style claim, its tracked weak control set, and its rebroadcast receipt. This owner is their union at every axis.

Composition is downward and sideways inside the sub-domain: `Op`, `Lease<T>`, `Atom`, `Cell`/`Transition`, `Validation`, `ValidityClaim`, `CapabilitySet<TCapability>`, and `ICapability<TSelf>` from `Domain`; `FaultCell` from `Domain/hooks`; `TelemetrySource` from `Domain/frame`; `PerceptualColor` from `Numerics/atoms`; `UiFault`, `RejectReason`, `FaultRail`, `UiDispatch<T>`, `DispatchLane`, and `UiThread` from `Interaction/dispatch`; `MountPhase` from `Interaction/chrome`; `ThemeGrid`, `ThemeSnapshot`, `ThemeShift`, and `ThemeChange` from `Interaction/paint`. Two settled host facts every registration here composes: `Platform.Add` registers under both the supplied type and its declared contract and then clears the resolved-handler cache, so a prior factory is recoverable through `Platform.Find` and a release RESTORES it; `Style.Add` appends into the active provider's per-key handler list whose only removal is a whole-registry clear, so a style registration's inverse is a dispatch cell its lease empties and never a second `Add`. `Eto.Drawing` never enters as a manifest using, so every fence aliases the host types it names.

## [01]-[INDEX]

- [02]-[HANDLER]: `HandlerDemand`, `HandlerCustody`, `HandlerHold<THandler>`, `MintFact`, `HandlerRow`, `HandlerIdentity`, `HandlerSeat`, `Handlers` — the registration rows with their restoring lease, the custody-preserving resolution, the widget identity capsule, and the mint census.
- [03]-[MOUNT]: `NativeMount`, `PlatformMount` — the two native-supply timings and the realize-plus-attach capsule that owns the crossing's inverse.
- [04]-[PLATFORM]: `PlatformCapability`, `FormFactor`, `PlatformScope`, `PlatformRow`, `PlatformClaim`, `PlatformId`, `PlatformFact`, `HostPlatform`, `StyleKey`, `StyleRow`, `StyleContext`, `StyleSeat`, `ThemeSeam` — the platform roster with its capability gate and scope rows, and the keyed style seam that injects a theme grid into the host registry.

## [02]-[HANDLER]

- Owner: `HandlerDemand` states ownership on the way in and names the host member it composes; `HandlerCustody` states it on the way out and carries the release; `HandlerHold<THandler>` the custody-carrying answer; `HandlerRow` one registration closed over its contract at mint; `HandlerSeat` the seated batch with its restoring release; `HandlerIdentity` the per-widget capsule; `MintFact` the census fact; `Handlers` the four entries.
- Cases: `HandlerDemand` is `Create` — mint a handler the caller now owns — `Shared` — the platform-cached singleton whose disposal poisons every other consumer — or `Registered`, a factory lookup whose invoke mints as `Create` does. `HandlerCustody` is `Owned` or `Borrowed`, the one axis a release reads, and each demand row names which custody its answer travels under.
- Entry: `Handlers.Seat` registers a row batch and leases the restore; `Resolve` answers a custody-carrying option; `Identity` reads one widget's capsule; `Census` leases an observer over both platform mint raises.
- Law: custody survives the crossing. One undifferentiated handler return erases exactly the fact a caller has to act on, so the demand row NAMES the custody its answer carries and the hold carries both — separating an owned handler from the platform's shared singleton is one row read at both ends rather than two parallel families a call site keeps consistent by hand. NAMED LOSS: a third custody spelling for a registry lookup, which named the demand rather than the disposal it licensed.
- Law: a resolve gates on `Supports<THandler>` before it creates, so a missing capability is a DISCOVERY result answering absence rather than a construction failure raising through the seam.
- Law: `Platform.Add` publishes no removal, so a seat CAPTURES the prior factory through `Platform.Find` and the release re-registers it. Where the platform registered none, the release seats a factory that refuses typed: the contract was unresolvable before the seat and is unresolvable after, and the refusal is the recoverable spelling of the raise the host otherwise throws. Restoring runs in reverse-seat order so a row layered over another unwinds to the state it found.
- Law: the row CLOSES its generic at mint, so nothing past construction carries an erased factory a consumer mis-casts; the `Type` column exists because `Platform.Add(Type, Func<object>)` is the host contract this page admits once, and it never leaves the row.
- Law: the seat carries the rows it took. Hosts that refuse one registration leave the others live, and a caller reads WHICH refused rather than inferring a partial seat from a count.
- Law: attach and detach both marshal — the platform's mint raises are UI-thread state, so an off-thread subtraction races the raises it is removing itself from, and the exact delegate identity attach used is what comes back off.
- Law: the identity capsule is a READ, never a key. Its two opaque host slots are handles the boundary casts at its own edge, its native handle rides `Option` because `nint.Zero` is a legal address the moment a platform hands one back, and its measured lifecycle fact reads the sub-domain's own `MountPhase` row rather than a second two-state vocabulary spelled beside it.
- Receipt: `HandlerSeat` publishes `Seated` and `Refused`; `Handlers.Census` publishes `MintFact` through the observer and parks observer faults on the injected `FaultCell` through `FaultRail.Isolate` rather than failing the raise — a `void` reporter licenses a discard and bounds nothing, while the cell's parks, sheds, and declined parks all read as numbers.
- Packages: Eto for `Platform` and its mint raises, Eto.Forms for `Widget` and `Control`; LanguageExt.Core for the rails and the `Lease`; Thinktecture.Runtime.Extensions for the two rosters; `Domain/hooks` for the bounded `FaultCell` every observer raise parks on.
- Growth: a new demand modality is one `HandlerDemand` row carrying its host member and its custody column; a new census raise is one `MintFact` case breaking every observer dispatch loudly.
- Boundary: HOST-SPECIFIC-STAYS — the AppKit bridge contracts each boundary registers against these rows (`IMacViewHandler`, `IMacWindow`, and the `MacConversions`/`CGConversions` projection owners) stay at the Grasshopper boundary, because they name `Microsoft.macOS` types the kernel does not reference.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Eto;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class HandlerCustody {
    public static readonly HandlerCustody Owned = new(key: 0, release: static (handler, key) =>
        FaultRail.Host(() => Fin.Succ(Op.Side(() => (handler as IDisposable)?.Dispose())), key));
    public static readonly HandlerCustody Borrowed = new(key: 1, release: static (_, _) => Fin.Succ(unit));

    [UseDelegateFromConstructor] internal partial Fin<Unit> Release(object handler, Op key);
}

[SmartEnum<int>]
public sealed partial class HandlerDemand {
    public static readonly HandlerDemand Create = new(key: 0, custody: HandlerCustody.Owned,
        mint: static (platform, contract, key) => FaultRail.Host(() => Fin.Succ(Some(platform.Create(type: contract))), key));
    public static readonly HandlerDemand Shared = new(key: 1, custody: HandlerCustody.Borrowed,
        mint: static (platform, contract, key) => FaultRail.Host(() => Fin.Succ(Some(platform.CreateShared(type: contract))), key));
    public static readonly HandlerDemand Registered = new(key: 2, custody: HandlerCustody.Owned,
        mint: static (platform, contract, key) => FaultRail.Host(
            () => Fin.Succ(Optional(platform.Find(type: contract)).Map(static factory => factory())), key));

    public HandlerCustody Custody { get; }

    [UseDelegateFromConstructor] internal partial Fin<Option<object>> Mint(Platform platform, Type contract, Op key);
}

public sealed record HandlerHold<THandler>(HandlerCustody Custody, THandler Handler) where THandler : class {
    public Fin<Unit> Release(Op? key = null) => Custody.Release(handler: Handler, key: key.OrDefault());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MintFact {
    private MintFact() { }
    public sealed record HandlerCase(object Instance) : MintFact;
    public sealed record WidgetCase(Widget Instance) : MintFact;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HandlerRow(Type Contract, Func<Platform, Op, Fin<Unit>> Seat, Func<Platform, Op, Fin<Unit>> Restore) {
    public static HandlerRow Of<THandler>(Func<THandler> factory) where THandler : class;
}

[BoundaryAdapter]
public sealed record HandlerIdentity(
    Type Widget,
    Option<string> Id,
    Option<StyleKey> Worn,
    Option<object> Handler,
    Option<nint> Native,
    Option<object> Control,
    MountPhase Phase);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class HandlerSeat : IDisposable {
    public Seq<HandlerRow> Seated { get; }
    public Seq<(HandlerRow Row, Error Cause)> Refused { get; }
    public void Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Handlers {
    [BoundaryAdapter]
    public static Fin<Lease<HandlerSeat>> Seat(Op? key = null, params ReadOnlySpan<HandlerRow> rows);

    [BoundaryAdapter]
    public static Fin<Option<HandlerHold<THandler>>> Resolve<THandler>(HandlerDemand demand, Op? key = null)
        where THandler : class;

    [BoundaryAdapter] public static Fin<HandlerIdentity> Identity(Widget widget, Op? key = null);

    [BoundaryAdapter]
    public static Fin<Lease<IDisposable>> Census(Action<MintFact> observe, FaultCell faults, Op? key = null);
}
```

## [03]-[MOUNT]

- Owner: `NativeMount` the two supply timings a native view can arrive under; `PlatformMount` the crossing capsule owning realize, attach, and the leased detach.
- Cases: `Eager` carries a live platform view the host constructor admits directly; `Deferred` carries the supplier and the typed error sink the host's own creation hook invokes, so an expensive native view materializes only when the managed tree demands it.
- Entry: `NativeMount.Realize` answers the bare control for a caller whose receipt already owns teardown — the spec-tree `Embedded` node is that caller; `PlatformMount.Attach` is the standalone crossing that realizes, attaches, and leases its own detach.
- Law: the two entries are NOT an arity pair, and the discriminant is teardown ownership named here: a realized control inside a realized subtree is released by that subtree's reverse-order receipt, and a control crossing on its own has no such owner, so it takes the lease. Callers holding a receipt that also takes a lease release twice.
- Law: hosting and extraction are two directions of ONE bridge — the mount carries native INTO the managed tree while the identity capsule's native handle and control object carry managed OUT — and both cross typed: an absent view refuses on the rail, an extraction miss lowers to absence, and no direction raises through the seam.
- Law: a deferred supply that refuses RETAINS its fault on the injected `FaultCell` and never on a raw `Action<Error>`, because the host creation hook has no return the refusal rides and a `void` sink licenses a silent discard; the mount's failure set is that retention made readable.
- Law: the `object` payload is the host's own contract — the native host constructor admits an untyped platform view — so the erasure is admitted ONCE here and never widens: nothing downstream re-types it and the boundary that supplied the view is the only party that knows what it is.
- Auto: release detaches before disposing, so a control whose native side was already torn down never double-detaches.
- Packages: Eto.Forms for the native host and its creation hook; LanguageExt.Core for the rails, `Atom`, and the `Lease`; `Domain/hooks` for the `FaultCell` a deferred supply's refusal parks on.
- Growth: another supply timing is one `NativeMount` case; native focus, keyboard routing, and event delivery stay on the mounted controls and this seam owns custody alone.
- Boundary: HOST-SPECIFIC-STAYS — the AppKit view anchors, the vibrancy panes, the CoreAnimation compositor, and the screen-capture estate at both boundaries construct the platform view and hand this owner a value; the kernel becomes Eto-aware, never AppKit-aware.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NativeMount {
    private NativeMount() { }
    public sealed record Eager(object Native) : NativeMount;
    public sealed record Deferred(Func<object> Supply, FaultCell Faults) : NativeMount;

    [BoundaryAdapter] public Fin<Control> Realize(Op? key = null);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlatformMount : IDisposable {
    [BoundaryAdapter] public static Fin<Lease<PlatformMount>> Attach(NativeMount mount, Op? key = null);

    public Control Subject { get; }
    public Seq<Error> Failures { get; }

    public void Dispose();
}
```

## [04]-[PLATFORM]

- Owner: `PlatformCapability` the admitted-feature vocabulary; `FormFactor` the two device postures; `PlatformRow` the backend roster; `PlatformClaim` the one capability demand; `PlatformFact` the ambient snapshot; `HostPlatform` the four ambient entries; `StyleKey` the keyed style identity; `StyleRow`, `StyleContext`, and `StyleSeat` the registration family; `ThemeSeam` the injection seam binding a `ThemeGrid` to the host style registry.
- Cases: `PlatformRow` carries six rows keyed on the host's own platform identifiers — macOS, WinForms, WPF, GTK, iOS, Android — and `PlatformClaim` is `FeatureCase`, `HandlerCase`, or `RowCase`, three demand shapes behind one gate.
- Entry: `HostPlatform.Snapshot` reads the ambient fact; `Demand` is the ONE capability gate; `Scope(scope, body, key)` is the ONE scoped crossing over the `PlatformScope` row; `ThemeSeam.Register` claims and seats a style batch, `Wear` assigns a claimed key to a widget, `Change` rails a theme shift, and `Provide` swaps the provider.
- Law: form factor is a ROW COLUMN, not a probe pair. Reading `IsDesktop` and `IsMobile` off the live platform and carrying both as bools admits a platform answering neither and a platform answering both; the roster declares each row's factor once and the fact projects it, so the two host predicates are the deleted form and the scattered `IsMac` tests they enabled have no spelling left.
- Law: bundle validity is an ADMISSION, not a column. Platforms reporting themselves invalid produce no usable fact, so `Snapshot` refuses typed and the `Valid` bool disappears rather than riding out for every reader to re-check. The identity is a `PlatformId`, so the fact carries no evidence fold at all — a one-conjunct claim over an admitted value object measures nothing its own construction did not refuse.
- Law: capability is a `CapabilitySet<PlatformCapability>` over the host's own feature vocabulary — the upstream is the platform feature flag set and each row names its flag — so a demand is set algebra with a rank-ordered wire rather than a bitwise test, and a claim carrying two required features admits or refuses as one.
- Law: an unrecognised platform answers ABSENCE on the row, never a synthetic fallback. Spelling that absence as an `Other` row — the negation of the other four — hides what it stands for, so the option carries it readable instead. NAMED LOSS: a consumer switching total over the roster answers the absent case explicitly. Witness: `Rasm.Rhino/.planning/Eto/platform.md:37 Other` rebuilt as `PlatformFact.Row : Option<PlatformRow>`.
- Law: a `StyleKey` is process-global CONTENDED state, because the host registry appends per key and publishes no per-key removal. Keys are therefore CLAIMED on the owning `TelemetrySource` first-claim-wins: a foreign claimant refuses typed instead of stacking an unarbitrated second handler under one identity, the owning package re-registers only itself, and the lease drops the claim so a reloaded package re-claims its own keys.
- Law: the registration's inverse is INDIRECTION, never a second `Add`. Seated handlers dispatch through a per-registration cell the lease empties, so a released registration stays resident and INERT and a package reloaded into a fresh load context leaves its predecessors doing nothing.
- Law: a batch with a duplicate key refuses BEFORE it seats anything, so the host registry never holds half of a rejected ledger.
- Law: this seam REGISTERS and never generates. Cell grid, contrast floors, and transition admission are `Interaction/paint#[05]-[THEME]`'s frozen value; here a shift is forwarded to that owner and only its ACCEPTED change drives the rebroadcast, so a refused shift never reaches a tracked control.
- Auto: tracked controls are held WEAKLY and compacted on every rebroadcast, so the seam never keeps a retired control alive and a rebroadcast walks only what the host still holds.
- Auto: a rebroadcast failure lands on the change's failure set rather than on the caller's rail — a single control refusing its restyle does not un-accept a theme every other control already took.
- Receipt: `ThemeChange` from the grid carries generation, variant, changed roles, and rebroadcast failures; `StyleSeat` publishes claimed and refused rows.
- Packages: Eto for `Platform`, its identifiers, its feature flags, `Style`, and the `IStyleProvider` contract a provider swap seats (`libs/dotnet/.api/api-eto-platform.md`); Eto.Forms for `Widget` and `Control`; LanguageExt.Core for `Atom`, `HashMap`, the rails, and the `Lease`; Thinktecture.Runtime.Extensions for the rows, the claim union, and the two key value objects; `Domain/hooks` for the `FaultCell` a seat's restyle faults park on.
- Growth: a new backend is one `PlatformRow` row carrying its probe and its factor; a new host feature is one `PlatformCapability` row naming its flag; a new demand shape is one `PlatformClaim` case; a new ambient scope is one `PlatformScope` row and no second entry.
- Boundary: HOST-SPECIFIC-STAYS — the Rhino theme-zone swatch feeder that produces a hosted shift and its host UI service resolution stay at that boundary; the Grasshopper session styling target and its accessibility axis stay at that one. Plugin identity is `Domain/frame`'s `PackageIdentity`, never a second identity here.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Eto;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlatformCapability : ICapability<PlatformCapability> {
    public static readonly PlatformCapability CellView = new(key: "cell-view", rank: 0, flag: PlatformFeatures.CustomCellSupportsControlView);
    public static readonly PlatformCapability Transparency = new(key: "transparency", rank: 1, flag: PlatformFeatures.DrawableWithTransparentContent);
    public static readonly PlatformCapability TabOrder = new(key: "tab-order", rank: 2, flag: PlatformFeatures.TabIndexWithCustomContainers);
    public static readonly PlatformCapability MultiThread = new(key: "multi-thread", rank: 3, flag: PlatformFeatures.MultiThreadedUI);
    public static readonly PlatformCapability Mnemonics = new(key: "mnemonics", rank: 4, flag: PlatformFeatures.Mnemonics);

    public int Rank { get; }
    internal PlatformFeatures Flag { get; }

    internal static CapabilitySet<PlatformCapability> Of(PlatformFeatures admitted);
}

[SmartEnum<int>]
public sealed partial class FormFactor {
    public static readonly FormFactor Desktop = new(key: 0);
    public static readonly FormFactor Mobile = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class PlatformScope {
    public static readonly PlatformScope Context = new(key: 0);
    public static readonly PlatformScope Worker = new(key: 1);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlatformRow {
    public static readonly PlatformRow Mac = new(key: Platforms.macOS, factor: FormFactor.Desktop, probe: static platform => platform.IsMac);
    public static readonly PlatformRow WinForms = new(key: Platforms.WinForms, factor: FormFactor.Desktop, probe: static platform => platform.IsWinForms);
    public static readonly PlatformRow Wpf = new(key: Platforms.Wpf, factor: FormFactor.Desktop, probe: static platform => platform.IsWpf);
    public static readonly PlatformRow Gtk = new(key: Platforms.Gtk, factor: FormFactor.Desktop, probe: static platform => platform.IsGtk);
    public static readonly PlatformRow Ios = new(key: Platforms.Ios, factor: FormFactor.Mobile, probe: static platform => platform.IsIos);
    public static readonly PlatformRow Android = new(key: Platforms.Android, factor: FormFactor.Mobile, probe: static platform => platform.IsAndroid);

    public FormFactor Factor { get; }

    [UseDelegateFromConstructor]
    internal partial bool Probe(Platform platform);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlatformClaim {
    private PlatformClaim() { }
    public sealed record FeatureCase(CapabilitySet<PlatformCapability> Required) : PlatformClaim;
    public sealed record HandlerCase(Type Contract) : PlatformClaim;
    public sealed record RowCase(PlatformRow Row) : PlatformClaim;
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct StyleKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "StyleKey requires a non-blank identity.");
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct PlatformId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "PlatformId requires a non-blank identity.");
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PlatformFact(
    PlatformId Id,
    Option<PlatformRow> Row,
    CapabilitySet<PlatformCapability> Capabilities) {
    public Option<FormFactor> Factor => Row.Map(static row => row.Factor);
}

internal sealed record StyleContext(Func<ThemeSnapshot> Snapshot, Action<Control> Track, FaultCell Faults);

public sealed record StyleRow(StyleKey Tag, Action<StyleContext> Seat) {
    public static StyleRow OfWidget<TWidget>(StyleKey tag, Action<TWidget, ThemeSnapshot> dress) where TWidget : Widget;
    public static StyleRow OfHandler<THandler>(StyleKey tag, Action<THandler, ThemeSnapshot> dress) where THandler : class, Widget.IHandler;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class StyleSeat : IDisposable {
    public TelemetrySource Owner { get; }
    public Seq<StyleRow> Claimed { get; }
    public Seq<(StyleRow Row, Error Cause)> Refused { get; }
    public void Dispose();
}

public sealed class ThemeSeam {
    public static Fin<ThemeSeam> Of(ThemeGrid grid, Op? key = null);

    public ThemeSnapshot Current { get; }
    public Seq<Error> Failures { get; }

    [BoundaryAdapter]
    public Fin<Lease<StyleSeat>> Register(TelemetrySource owner, FaultCell faults, Op? key = null, params ReadOnlySpan<StyleRow> rows);

    [BoundaryAdapter] public Fin<Unit> Wear(Widget widget, StyleKey style, Op? key = null);

    [BoundaryAdapter] public Fin<Unit> Provide(IStyleProvider provider, Op? key = null);

    public Unit Track(Control control);

    [BoundaryAdapter] public Fin<ThemeChange> Change(ThemeShift shift, Op? key = null);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPlatform {
    [BoundaryAdapter] public static Fin<PlatformFact> Snapshot(Op? key = null);

    [BoundaryAdapter] public static Fin<Unit> Demand(PlatformClaim claim, Op? key = null);

    [BoundaryAdapter] public static Fin<TResult> Scope<TResult>(PlatformScope scope, Func<Fin<TResult>> body, Op? key = null);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
