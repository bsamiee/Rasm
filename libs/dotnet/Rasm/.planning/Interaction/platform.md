# [RASM_PLATFORM]

`Rasm.Interaction` owns the ambient host backend boundary every interaction surface stands on: which platform is live and what it admits, how a widget's backend handler is registered and resolved with its custody intact, how a raw native view crosses into the managed tree, and how a keyed style claims its identity and rebroadcasts a theme. Rows answer every platform question — never a string comparison or a scattered `IsMac` predicate — and every registration this page performs returns its own inverse because the host registries it writes into are append-only and publish no removal.

Grasshopper held the platform boundary and Rhino the theme catalog. Grasshopper carried the typed snapshot, the polymorphic capability demand behind one gate, the two style registration arms — widget facade and concrete handler — and the injective ledger; Rhino carried the three-case handler demand, the eager-and-deferred native mount with its attachment lease, and the whole theme grid with its plugin-scoped style claim, its tracked weak control set, and its rebroadcast change. This owner is their union at every axis.

Composition is downward and sideways inside the sub-domain: `Lease<T>`, `Atom`, `Cell`/`Transition`, `Validation`, `ValidityClaim`, `CapabilitySet<TCapability>`, and `ICapability<TSelf>` from `Domain`; `FaultCell` from `Domain/hooks`; `TelemetrySource` from `Domain/frame`; `PerceptualColor` from `Numerics/atoms`; `UiFault`, `RejectReason`, `FaultGate`, `UiDispatch<T>`, `DispatchLane`, and `UiThread` from `Interaction/dispatch`; `ThemeGrid`, `ThemeSnapshot`, `ThemeShift`, and `ThemeChange` from `Interaction/paint`. Two settled host facts every registration here composes: `Platform.Add` registers under both the supplied type and its declared contract and then clears the resolved-handler cache, so a prior factory is recoverable through `Platform.Find` and a release RESTORES it; `Style.Add` appends into the active provider's per-key handler list whose only removal is a whole-registry clear, so a style registration's inverse is a dispatch cell its lease empties and never a second `Add`. `Eto.Drawing` never enters as a manifest using, so every fence aliases the host types it names.

## [01]-[INDEX]

- [02]-[HANDLER]: `HandlerDemand`, `Handlers` — restoring registration and custody-preserving resolution.
- [03]-[MOUNT]: `NativeMount` — the two native-supply timings with realized and leased attachment forms.
- [04]-[PLATFORM]: `PlatformCapability`, `Accessibility`, `PlatformRow`, `PlatformRequirement`, `PlatformId`, `PlatformFact`, `HostPlatform`, `StyleKey`, `StyleRow`, `StyleContext`, `ThemePort` — the platform roster with its capability gate, the accessibility-display vocabulary, and the keyed style port that injects a theme grid into the host registry.

## [02]-[HANDLER]

- Owner: `HandlerDemand` states ownership on the way in and names the host member it composes; `Handlers` owns registration and resolution.
- Cases: `HandlerDemand` is `Create` — mint a handler the caller now owns — `Shared` — the platform-cached singleton whose disposal poisons every other consumer — or `Registered`, a factory lookup whose invoke mints as `Create` does.
- Entry: `Handlers.Register` registers a host-shaped batch and leases the restore; `Resolve` answers a custody-carrying option.
- Law: custody survives the crossing. One undifferentiated handler return erases exactly the fact a caller has to act on, so resolution returns the handler with its already-bound release function — separating an owned handler from the platform's shared singleton remains one demand-row decision without exposing an ownership flag to consumers.
- Law: a resolve gates on `Supports<THandler>` before it creates, so a missing capability is a DISCOVERY result answering absence rather than a construction failure raising through the boundary.
- Law: `Platform.Add` publishes no removal, so a seat CAPTURES the prior factory through `Platform.Find` and the release re-registers it. Where the platform registered none, the release seats a factory that refuses typed: the contract was unresolvable before the seat and is unresolvable after, and the refusal is the recoverable spelling of the raise the host otherwise throws. Restoring runs in reverse-seat order so a row layered over another unwinds to the state it found.
- Law: each registration arrives in the host's `(Type Contract, Func<object> Factory)` shape, so nothing past admission carries a second registration model.
- Law: independent registration admission faults accumulate before mutation; ordered host writes then short-circuit and restore their successful prefix on failure.
- Packages: Eto for `Platform`; LanguageExt.Core for the types and the `Lease`; Thinktecture.Runtime.Extensions for the demand roster.
- Growth: a new demand modality is one `HandlerDemand` row carrying its host member and ownership column.
- Boundary: HOST-SPECIFIC-STAYS — the AppKit bridge contracts each boundary registers against these rows (`IMacViewHandler`, `IMacWindow`, and the `MacConversions`/`CGConversions` projection owners) stay at the Grasshopper boundary, because they name `Microsoft.macOS` types the kernel does not reference.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class HandlerDemand {
    public static readonly HandlerDemand Create = new(owned: true,
        resolve: static (platform, contract) => FaultGate.Capture(
            () => Fin.Succ(Some(platform.Create(type: contract)))));
    public static readonly HandlerDemand Shared = new(owned: false,
        resolve: static (platform, contract) => FaultGate.Capture(
            () => Fin.Succ(Some(platform.CreateShared(type: contract)))));
    public static readonly HandlerDemand Registered = new(owned: true,
        resolve: static (platform, contract) => FaultGate.Capture(
            () => Fin.Succ(Optional(platform.Find(type: contract)).Map(static factory => factory()))));

    public bool Owned { get; }

    [UseDelegateFromConstructor] internal partial Fin<Option<object>> Resolve(Platform platform, Type contract);
}

// --- [MODELS] --------------------------------------------------------------------------
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Handlers {
    public static Fin<Lease<IDisposable>> Register(
        params ReadOnlySpan<(Type Contract, Func<object> Factory)> registrations);

    public static Fin<Option<(THandler Handler, Func<Fin<Unit>> Release)>> Resolve<THandler>(HandlerDemand demand)
        where THandler : class;
}
```

## [03]-[MOUNT]

- Owner: `NativeMount` owns the two supply timings a native view can arrive under and the two teardown shapes consumers require.
- Cases: `Eager` carries a live platform view the host constructor admits directly; `Deferred` carries the supplier and the typed error sink the host's own creation hook invokes, so an expensive native view materializes only when the managed tree demands it.
- Entry: `NativeMount.Realize` answers the bare control for a caller whose mount already owns teardown — the spec-tree `Embedded` node is that caller; `Attach` is the standalone crossing that realizes, attaches, and leases its own detach.
- Law: the two entries are NOT an arity pair, and the discriminant is teardown ownership named here: a realized control inside a realized subtree is released by that subtree's reverse-order mount, and a control crossing on its own has no such owner, so it takes the lease. Callers holding a mount that also takes a lease release twice.
- Law: the mount carries native INTO the managed tree; managed-to-native extraction is a direct `IControlObjectSource.ControlObject` read at its consuming boundary, so an absent view refuses on the result and no second identity snapshot survives beside the host member.
- Law: a deferred supply that refuses RETAINS its fault on the injected `FaultCell` and never on a raw `Action<Error>`, because the host creation hook has no return the refusal rides and a `void` sink licenses a silent discard; the mount's failure set is that retention made readable.
- Law: the `object` payload is the host's own contract — the native host constructor admits an untyped platform view — so the erasure is admitted ONCE here and never widens: nothing downstream re-types it and the boundary that supplied the view is the only party that knows what it is.
- Auto: release detaches before disposing, so a control whose native side was already torn down never double-detaches.
- Packages: Eto.Forms for the native host and its creation hook; LanguageExt.Core for the types, `Atom`, and the `Lease`; `Domain/hooks` for the `FaultCell` a deferred supply's refusal parks on.
- Growth: another supply timing is one `NativeMount` case; native focus, keyboard routing, and event delivery stay on the mounted controls and this port owns custody alone.
- Boundary: HOST-SPECIFIC-STAYS — the AppKit view anchors, the vibrancy panes, the CoreAnimation compositor, and the screen-capture module at both boundaries construct the platform view and hand this owner a value; the kernel becomes Eto-aware, never AppKit-aware.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NativeMount {
    private NativeMount() { }
    public sealed record Eager(object Native) : NativeMount;
    public sealed record Deferred(Func<Fin<object>> Supply, FaultCell Faults) : NativeMount;

    public Fin<Control> Realize();
    public Fin<Lease<Control>> Attach();
}

```

## [04]-[PLATFORM]

- Owner: `PlatformCapability` the admitted-feature vocabulary; `Accessibility` the host accessibility-display vocabulary every motion, theme, and translucency consumer reads as one `CapabilitySet<Accessibility>`; `PlatformRow` the backend roster; `PlatformRequirement` the one capability demand; `PlatformFact` the ambient snapshot; `HostPlatform` the snapshot and demand entries; `StyleKey`, `StyleRow`, and `StyleContext` the registration family; `ThemePort` the injection port binding a `ThemeGrid` to the host style registry.
- Cases: `PlatformRow` carries six rows keyed on the host's own platform identifiers — macOS, WinForms, WPF, GTK, iOS, Android — and `PlatformRequirement` is `Features`, `Handler`, or `Backend`, three demand shapes behind one gate. `Accessibility` carries five rows — `ReduceMotion`, `IncreaseContrast`, `DifferentiateWithoutColor`, `ReduceTransparency`, `InvertColors` — the closed macOS accessibility-display axis set; `CapabilitySet` carries the combination, so the five bool columns three folders declared independently are one canonical key-ordered membership, and only ONE row is a motion fact — the other four are display settings, which is why the roster seats here and not with the motion fold.
- Entry: `HostPlatform.Snapshot` reads the ambient fact; `Demand` is the ONE capability gate; `ThemePort.Register` claims and seats a style batch, `Apply` assigns a claimed key to a widget, and `Change` lands a theme shift.
- Law: form factor is the host's `IsMobile` fact. Desktop is its negation where required, so a second vocabulary and a paired boolean cannot drift from the admitted snapshot.
- Law: bundle validity is an ADMISSION, not a column. Platforms reporting themselves invalid produce no usable fact, so `Snapshot` refuses typed and the `Valid` bool disappears rather than riding out for every reader to re-check. The identity is a `PlatformId`, so the fact carries no evidence fold at all — a one-conjunct claim over an admitted value object measures nothing its own construction did not refuse.
- Law: capability is a `CapabilitySet<PlatformCapability>` over the host's own feature vocabulary — the upstream is the platform feature flag set and each row names its flag — so a demand is set algebra with an ordinal-key wire rather than a bitwise test, and a claim carrying two required features admits or refuses as one.
- Law: an unrecognised platform answers ABSENCE on the row, never a synthetic fallback. Spelling that absence as an `Other` row — the negation of the other four — hides what it stands for, so the option carries it readable instead. NAMED LOSS: a consumer switching total over the roster answers the absent case explicitly. Witness: `Rasm.Rhino/.planning/Eto/platform.md:37 Other` rebuilt as `PlatformFact.Row : Option<PlatformRow>`.
- Law: a `StyleKey` is process-global CONTENDED state, because the host registry appends per key and publishes no per-key removal. Keys are therefore CLAIMED on the owning `TelemetrySource` first-claim-wins: a foreign claimant refuses typed instead of stacking an unarbitrated second handler under one identity, the owning package re-registers only itself, and the lease drops the claim so a reloaded package re-claims its own keys.
- Law: the registration's inverse is INDIRECTION, never a second `Add`. Seated handlers dispatch through a per-registration cell the lease empties, so a released registration stays resident and INERT and a package reloaded into a fresh load context leaves its predecessors doing nothing.
- Law: a batch with a duplicate key refuses BEFORE it seats anything, so the host registry never holds half of a rejected ledger.
- Law: this port REGISTERS and never generates. Cell grid, contrast floors, and transition admission are `Interaction/paint#[05]-[THEME]`'s frozen value; here a shift is forwarded to that owner and only its ACCEPTED change drives the rebroadcast, so a refused shift never reaches a tracked control.
- Auto: tracked controls are held WEAKLY and compacted on every rebroadcast, so the port never keeps a retired control alive and a rebroadcast walks only what the host still holds.
- Auto: independent rebroadcast failures combine through `Error.operator +` on `WriterT<Error, Fin, ThemeChange>` rather than entering the accepted change — a single control refusing its restyle does not un-accept a theme every other control already took.
- Output: `ThemeChange` from the grid carries generation, variant, and changed roles; the writer output carries rebroadcast failures.
- Packages: Eto for `Platform`, its identifiers, its feature flags, and `Style` (`libs/dotnet/.api/api-eto-platform.md`); Eto.Forms for `Widget` and `Control`; LanguageExt.Core for `Atom`, `HashMap`, `WriterT`, the types, and the `Lease`; Thinktecture.Runtime.Extensions for the rows, the requirement union, and the two key value objects; `Domain/hooks` for the `FaultCell` a registration's restyle faults park on.
- Growth: a new backend is one `PlatformRow` key; a new host feature is one `PlatformCapability` row naming its flag; a new accessibility accommodation is one `Accessibility` row plus one probe entry at each boundary that reads the host; a new demand shape is one `PlatformRequirement` case.
- Boundary: HOST-SPECIFIC-STAYS — the Rhino theme-zone swatch feeder that produces a hosted shift and its host UI service resolution stay at that boundary; the Grasshopper session styling target stays at that one, and every `NSWorkspace` or preference PROBE filling `CapabilitySet<Accessibility>` stays at the boundary that reads its host. Plugin identity is `Domain/frame`'s `PackageIdentity`, never a second identity here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlatformCapability : ICapability<PlatformCapability> {
    public static readonly PlatformCapability CustomCellControlView = new(key: "cell-view", rank: 0, flag: PlatformFeatures.CustomCellSupportsControlView);
    public static readonly PlatformCapability TransparentDrawableContent = new(key: "transparency", rank: 1, flag: PlatformFeatures.DrawableWithTransparentContent);
    public static readonly PlatformCapability CustomContainerTabIndex = new(key: "tab-order", rank: 2, flag: PlatformFeatures.TabIndexWithCustomContainers);
    public static readonly PlatformCapability MultiThreadedUi = new(key: "multi-thread", rank: 3, flag: PlatformFeatures.MultiThreadedUI);
    public static readonly PlatformCapability Mnemonics = new(key: "mnemonics", rank: 4, flag: PlatformFeatures.Mnemonics);

    public int Rank { get; }
    internal PlatformFeatures Flag { get; }

    internal static CapabilitySet<PlatformCapability> Of(PlatformFeatures admitted);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Accessibility : ICapability<Accessibility> {
    public static readonly Accessibility ReduceMotion = new(key: "reduce-motion", rank: 0);
    public static readonly Accessibility IncreaseContrast = new(key: "increase-contrast", rank: 1);
    public static readonly Accessibility DifferentiateWithoutColor = new(key: "differentiate-without-color", rank: 2);
    public static readonly Accessibility ReduceTransparency = new(key: "reduce-transparency", rank: 3);
    public static readonly Accessibility InvertColors = new(key: "invert-colors", rank: 4);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlatformRow {
    public static readonly PlatformRow Mac = new(key: Platforms.macOS);
    public static readonly PlatformRow WinForms = new(key: Platforms.WinForms);
    public static readonly PlatformRow Wpf = new(key: Platforms.Wpf);
    public static readonly PlatformRow Gtk = new(key: Platforms.Gtk);
    public static readonly PlatformRow Ios = new(key: Platforms.Ios);
    public static readonly PlatformRow Android = new(key: Platforms.Android);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlatformRequirement {
    private PlatformRequirement() { }
    public sealed record Features(CapabilitySet<PlatformCapability> Required) : PlatformRequirement;
    public sealed record Handler(Type Contract) : PlatformRequirement;
    public sealed record Backend(PlatformRow Required) : PlatformRequirement;
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct StyleKey {
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

public readonly record struct PlatformFact(
    PlatformId Id,
    Option<PlatformRow> Row,
    bool IsMobile,
    CapabilitySet<PlatformCapability> Capabilities);

internal sealed record StyleContext(Func<ThemeSnapshot> Snapshot, Action<Control> Track, FaultCell Faults);

public sealed record StyleRow(StyleKey Key, Func<StyleContext, Fin<Unit>> Register) {
    public static StyleRow ForWidget<TWidget>(StyleKey key, Action<TWidget, ThemeSnapshot> apply) where TWidget : Widget;
    public static StyleRow ForHandler<THandler>(StyleKey key, Action<THandler, ThemeSnapshot> apply) where THandler : class, Widget.IHandler;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ThemePort(ThemeGrid grid) {
    public Fin<Lease<IDisposable>> Register(
        TelemetrySource owner, FaultCell faults, params ReadOnlySpan<StyleRow> rows);

    public Fin<Unit> Apply(Widget widget, StyleKey style);
    public WriterT<Error, Fin, ThemeChange> Change(ThemeShift shift);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPlatform {
    public static Fin<PlatformFact> Snapshot();

    public static Fin<Unit> Demand(PlatformRequirement requirement);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
