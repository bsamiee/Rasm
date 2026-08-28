# 1. Delete the duplicate handler-custody vocabulary

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, `HandlerCustody`
```csharp
[SmartEnum<int>]
public sealed partial class HandlerCustody {
    public static readonly HandlerCustody Owned = new(key: 0, release: static (handler, key) =>
        FaultGate.Host(() => Fin.Succ(HostEdge.Side(() => (handler as IDisposable)?.Dispose()))));
    public static readonly HandlerCustody Borrowed = new(key: 1, release: static (_, _) => Fin.Succ(unit));

    [UseDelegateFromConstructor] internal partial Fin<Unit> Release(object handler);
}
```

To:

```csharp
// HandlerCustody DELETED
```

Why: `HandlerDemand` already selects handler creation and therefore owns whether the result is released. The keyed two-row type duplicates that decision, and its constructor delegates incorrectly include a generated key argument that the partial method does not accept.

Change: Move the ownership column onto `HandlerDemand` and delete the second vocabulary.

Delta: -8 LOC, -1 module-level type, and -3 declared members.

# 2. Make handler demands keyless behavior rows

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, `HandlerDemand` declaration and rows
```csharp
[SmartEnum<int>]
public sealed partial class HandlerDemand {
    public static readonly HandlerDemand Create = new(key: 0, custody: HandlerCustody.Owned,
        mint: static (platform, contract, key) => FaultGate.Host(() => Fin.Succ(Some(platform.Create(type: contract)))));
    public static readonly HandlerDemand Shared = new(key: 1, custody: HandlerCustody.Borrowed,
        mint: static (platform, contract, key) => FaultGate.Host(() => Fin.Succ(Some(platform.CreateShared(type: contract)))));
    public static readonly HandlerDemand Registered = new(key: 2, custody: HandlerCustody.Owned,
        mint: static (platform, contract, key) => FaultGate.Host(
            () => Fin.Succ(Optional(platform.Find(type: contract)).Map(static factory => factory())), key));
```

To:

```csharp
[SmartEnum]
public sealed partial class HandlerDemand {
    public static readonly HandlerDemand Create = new(owned: true,
        resolve: static (platform, contract) => FaultGate.Host(
            () => Fin.Succ(Some(platform.Create(type: contract)))));
    public static readonly HandlerDemand Shared = new(owned: false,
        resolve: static (platform, contract) => FaultGate.Host(
            () => Fin.Succ(Some(platform.CreateShared(type: contract)))));
    public static readonly HandlerDemand Registered = new(owned: true,
        resolve: static (platform, contract) => FaultGate.Host(
            () => Fin.Succ(Optional(platform.Find(type: contract)).Map(static factory => factory()))));
```

Why: The rows are process-local behavior, not wire identity. Keyless generation removes meaningless integer keys, and constructor delegates must match the generated partial-method parameters exactly.

Change: Remove the keys, retain ownership as a boolean column, and correct the resolution delegate arity.

Delta: +3 LOC; no module-level type or declared-member change.

# 3. Return handler custody without a forwarding record

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, tail of `HandlerDemand` through `HandlerHold<THandler>`
```csharp
    public HandlerCustody Custody { get; }

    [UseDelegateFromConstructor] internal partial Fin<Option<object>> Mint(Platform platform, Type contract);
}

public sealed record HandlerHold<THandler>(HandlerCustody Custody, THandler Handler) where THandler : class {
    public Fin<Unit> Release() => Custody.Release(handler: Handler);
}
```

To:

```csharp
    public bool Owned { get; }

    [UseDelegateFromConstructor] internal partial Fin<Option<object>> Resolve(Platform platform, Type contract);
}
```

Why: `HandlerHold<THandler>` stores one handler and forwards one release operation. A named result tuple can carry the handler and its already-bound release function without exposing another owner or an ownership flag to consumers.

Change: Keep ownership internal to the demand row, rename the generated operation, and delete the wrapper record.

Delta: -4 LOC, -1 module-level type, and -3 declared members.

# 4. Delete the platform-event mirror union

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, `MintFact`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MintFact {
    private MintFact() { }
    public sealed record HandlerCase(object Instance) : MintFact;
    public sealed record WidgetCase(Widget Instance) : MintFact;
}
```

To:

```csharp
// MintFact DELETED
```

Why: Eto already publishes distinct typed `HandlerCreated` and `WidgetCreated` events. Mirroring those payloads into another union adds a second event vocabulary without changing dispatch or lifetime.

Change: Subscribe to the Eto events directly at the observing boundary.

Delta: -6 LOC, -1 module-level type, and -2 nested case types.

# 5. Pass handler registrations in the host shape

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, `HandlerRow`
```csharp
public sealed record HandlerRow(Type Contract, Func<Platform, Fin<Unit>> Seat, Func<Platform, Fin<Unit>> Restore) {
    public static HandlerRow Of<THandler>(Func<THandler> factory) where THandler : class;
}
```

To:

```csharp
// HandlerRow DELETED
```

Why: `Platform.Add(Type, Func<object>)` is already the complete registration input. Prior-factory capture and reverse restoration are operation state, not fields a caller should construct.

Change: Accept `(Type Contract, Func<object> Factory)` tuples and capture restoration inside registration.

Delta: -3 LOC, -1 module-level type, and -3 declared members.

# 6. Delete the unused handler snapshot

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, `HandlerIdentity`
```csharp
public sealed record HandlerIdentity(
    Type Widget,
    Option<string> Id,
    Option<StyleKey> Worn,
    Option<object> Handler,
    Option<nint> Native,
    Option<object> Control,
    MountPhase Phase);
```

To:

```csharp
// HandlerIdentity DELETED
```

Why: The record combines unrelated point-in-time projections and has no consumer. Eto already exposes the widget, handler, style, and `IControlObjectSource.ControlObject`; host-specific native extraction remains at its boundary.

Change: Read the required host member at the consuming boundary.

Delta: -8 LOC, -1 module-level type, and -7 positional members.

# 7. Delete the partial-success handler result

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, `HandlerSeat`
```csharp
public sealed class HandlerSeat : IDisposable {
    public Seq<HandlerRow> Seated { get; }
    public Seq<(HandlerRow Row, Error Cause)> Refused { get; }
    public void Dispose();
}
```

To:

```csharp
// HandlerSeat DELETED
```

Why: A success value containing refused registrations makes failure ornamental. Registration inputs can accumulate independent admission faults before mutation; ordered host writes then short-circuit and restore the successful prefix on failure.

Change: Return only the restoring lease after complete registration succeeds.

Delta: -5 LOC, -1 module-level type, and -3 declared members.

# 8. Narrow handler operations to registration and resolution

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[02]-[HANDLER]` code fence, `Handlers`
```csharp
public static class Handlers {
    public static Fin<Lease<HandlerSeat>> Seat(params ReadOnlySpan<HandlerRow> rows);

    public static Fin<Option<HandlerHold<THandler>>> Resolve<THandler>(HandlerDemand demand)
        where THandler : class;

    public static Fin<HandlerIdentity> Identity(Widget widget);

    public static Fin<Lease<IDisposable>> Census(Action<MintFact> observe, FaultCell faults);
}
```

To:

```csharp
public static class Handlers {
    public static Fin<Lease<IDisposable>> Register(
        params ReadOnlySpan<(Type Contract, Func<object> Factory)> registrations);

    public static Fin<Option<(THandler Handler, Func<Fin<Unit>> Release)>> Resolve<THandler>(HandlerDemand demand)
        where THandler : class;
}
```

Why: Registration mutates an ordered registry and therefore remains `Fin`; only its independent pre-admission belongs in `Validation` before conversion. Resolution is dependent and short-circuits, while identity and event census are direct Eto reads.

Change: Use host-shaped inputs, return the restoring lease only on complete success, return tuple-shaped handler custody, and delete two forwarding entries.

Delta: -3 LOC and -2 declared methods; module-level type count is unchanged.

# 9. Make native supply failure explicit

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[03]-[MOUNT]` code fence, `NativeMount`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NativeMount {
    private NativeMount() { }
    public sealed record Eager(object Native) : NativeMount;
    public sealed record Deferred(Func<object> Supply, FaultCell Faults) : NativeMount;

    public Fin<Control> Realize();
}
```

To:

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NativeMount {
    private NativeMount() { }
    public sealed record Eager(object Native) : NativeMount;
    public sealed record Deferred(Func<Fin<object>> Supply, FaultCell Faults) : NativeMount;

    public Fin<Control> Realize();
    public Fin<Lease<Control>> Attach();
}
```

Why: `Func<object>` can report an expected refusal only by throwing. The supply function should carry its result, while the existing mount owner can also return the standalone control with its disposal custody.

Change: Type the deferred supplier with `Fin` and absorb standalone attachment onto `NativeMount`.

Delta: +1 LOC and +1 declared member; module-level type count is unchanged.

# 10. Delete the second native-mount owner

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[03]-[MOUNT]` code fence, `PlatformMount`
```csharp
public sealed class PlatformMount : IDisposable {
    public static Fin<Lease<PlatformMount>> Attach(NativeMount mount);

    public Control Subject { get; }
    public Seq<Error> Failures { get; }

    public void Dispose();
}
```

To:

```csharp
// PlatformMount DELETED
```

Why: `PlatformMount` duplicates `NativeMount` and `Lease<T>`. The attached control is the leased value, and deferred failures already park on the supplied `FaultCell`.

Change: Use `NativeMount.Attach()` and return `Lease<Control>` directly.

Delta: -8 LOC, -1 module-level type, and -4 declared members.

# 11. Delete the two-row form-factor vocabulary

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `FormFactor`
```csharp
[SmartEnum<int>]
public sealed partial class FormFactor {
    public static readonly FormFactor Desktop = new(key: 0);
    public static readonly FormFactor Mobile = new(key: 1);
}
```

To:

```csharp
// FormFactor DELETED
```

Why: The rows carry no distinct behavior and only restate a two-state host fact. A boolean column on `PlatformFact` preserves the capability without a second owner.

Change: Store `IsMobile` on the platform snapshot and derive desktop as its negation where required.

Delta: -5 LOC, -1 module-level type, and -2 declared row fields.

# 12. Resolve platform rows through their generated keys

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `PlatformRow` declarations and members
```csharp
    public static readonly PlatformRow Mac = new(key: Platforms.macOS, factor: FormFactor.Desktop, probe: static platform => platform.IsMac);
    public static readonly PlatformRow WinForms = new(key: Platforms.WinForms, factor: FormFactor.Desktop, probe: static platform => platform.IsWinForms);
    public static readonly PlatformRow Wpf = new(key: Platforms.Wpf, factor: FormFactor.Desktop, probe: static platform => platform.IsWpf);
    public static readonly PlatformRow Gtk = new(key: Platforms.Gtk, factor: FormFactor.Desktop, probe: static platform => platform.IsGtk);
    public static readonly PlatformRow Ios = new(key: Platforms.Ios, factor: FormFactor.Mobile, probe: static platform => platform.IsIos);
    public static readonly PlatformRow Android = new(key: Platforms.Android, factor: FormFactor.Mobile, probe: static platform => platform.IsAndroid);

    public FormFactor Factor { get; }

    [UseDelegateFromConstructor]
    internal partial bool Probe(Platform platform);
```

To:

```csharp
    public static readonly PlatformRow Mac = new(key: Platforms.macOS);
    public static readonly PlatformRow WinForms = new(key: Platforms.WinForms);
    public static readonly PlatformRow Wpf = new(key: Platforms.Wpf);
    public static readonly PlatformRow Gtk = new(key: Platforms.Gtk);
    public static readonly PlatformRow Ios = new(key: Platforms.Ios);
    public static readonly PlatformRow Android = new(key: Platforms.Android);
```

Why: Each row key is already the exact `Platform.ID` value. The generated keyed lookup admits a recognized backend and returns absence for an unknown one; six predicate delegates and a duplicated form-factor column add a second identity path.

Change: Resolve `PlatformRow` with generated `TryGet(platform.ID, out row)` inside `Snapshot` and remove the redundant columns.

Delta: -6 LOC and -2 declared members; module-level type count is unchanged.

# 13. Delete the host-context wrapper vocabulary

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `PlatformScope`
```csharp
[SmartEnum<int>]
public sealed partial class PlatformScope {
    public static readonly PlatformScope Context = new(key: 0);
    public static readonly PlatformScope Worker = new(key: 1);
}
```

To:

```csharp
// PlatformScope DELETED
```

Why: Eto already owns the non-interchangeable operations as `Platform.Invoke` and `Platform.ThreadStart`. A second vocabulary hides which host lifetime a caller selected and has no consumer.

Change: Call the appropriate Eto operation at the platform boundary.

Delta: -5 LOC, -1 module-level type, and -2 declared row fields.

# 14. Use the Eto feature contract names

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `PlatformCapability` rows
```csharp
public static readonly PlatformCapability CellView = new(key: "cell-view", rank: 0, flag: PlatformFeatures.CustomCellSupportsControlView);
public static readonly PlatformCapability Transparency = new(key: "transparency", rank: 1, flag: PlatformFeatures.DrawableWithTransparentContent);
public static readonly PlatformCapability TabOrder = new(key: "tab-order", rank: 2, flag: PlatformFeatures.TabIndexWithCustomContainers);
public static readonly PlatformCapability MultiThread = new(key: "multi-thread", rank: 3, flag: PlatformFeatures.MultiThreadedUI);
public static readonly PlatformCapability Mnemonics = new(key: "mnemonics", rank: 4, flag: PlatformFeatures.Mnemonics);
```

To:

```csharp
public static readonly PlatformCapability CustomCellControlView = new(key: "cell-view", rank: 0, flag: PlatformFeatures.CustomCellSupportsControlView);
public static readonly PlatformCapability TransparentDrawableContent = new(key: "transparency", rank: 1, flag: PlatformFeatures.DrawableWithTransparentContent);
public static readonly PlatformCapability CustomContainerTabIndex = new(key: "tab-order", rank: 2, flag: PlatformFeatures.TabIndexWithCustomContainers);
public static readonly PlatformCapability MultiThreadedUi = new(key: "multi-thread", rank: 3, flag: PlatformFeatures.MultiThreadedUI);
public static readonly PlatformCapability Mnemonics = new(key: "mnemonics", rank: 4, flag: PlatformFeatures.Mnemonics);
```

Why: The current names broaden the external contracts. Capability identifiers should state the precise feature being admitted while their stable keys remain unchanged.

Change: Rename four rows to the Eto feature semantics they carry.

Delta: 0 LOC; no module-level type or declared-member change.

# 15. Use the accessibility setting's canonical name

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `Accessibility` rows
```csharp
public static readonly Accessibility ReduceMotion = new(key: "reduce-motion", rank: 0);
public static readonly Accessibility IncreaseContrast = new(key: "increase-contrast", rank: 1);
public static readonly Accessibility DifferentiateColour = new(key: "differentiate-colour", rank: 2);
public static readonly Accessibility ReduceTransparency = new(key: "reduce-transparency", rank: 3);
public static readonly Accessibility InvertColors = new(key: "invert-colors", rank: 4);
```

To:

```csharp
public static readonly Accessibility ReduceMotion = new(key: "reduce-motion", rank: 0);
public static readonly Accessibility IncreaseContrast = new(key: "increase-contrast", rank: 1);
public static readonly Accessibility DifferentiateWithoutColor = new(key: "differentiate-without-color", rank: 2);
public static readonly Accessibility ReduceTransparency = new(key: "reduce-transparency", rank: 3);
public static readonly Accessibility InvertColors = new(key: "invert-colors", rank: 4);
```

Why: The host setting is “differentiate without color”; the current name drops the operative condition and gives it a different identity string.

Change: Rename the row and key to the platform accessibility concept.

Delta: 0 LOC; no module-level type or declared-member change.

Ripples: Replace `Accessibility.DifferentiateColour` in `libs/dotnet/Rasm.Grasshopper/.planning/Platform/native.md` and `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md`.

# 16. Name platform requirements by their meaning

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `PlatformClaim`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlatformClaim {
    private PlatformClaim() { }
    public sealed record FeatureCase(CapabilitySet<PlatformCapability> Required) : PlatformClaim;
    public sealed record HandlerCase(Type Contract) : PlatformClaim;
    public sealed record RowCase(PlatformRow Row) : PlatformClaim;
}
```

To:

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlatformRequirement {
    private PlatformRequirement() { }
    public sealed record Features(CapabilitySet<PlatformCapability> Required) : PlatformRequirement;
    public sealed record Handler(Type Contract) : PlatformRequirement;
    public sealed record Backend(PlatformRow Required) : PlatformRequirement;
}
```

Why: `Demand` consumes requirements, not claims. `Case` and `Row` describe generated storage rather than the platform condition being required.

Change: Rename the union and cases without compatibility aliases.

Delta: 0 LOC; no module-level type or declared-member change.

Ripples: Replace `new PlatformClaim.HandlerCase(...)` with `new PlatformRequirement.Handler(...)` in `libs/dotnet/Rasm/.planning/Interaction/chrome.md`.

# 17. Store form factor directly on the platform snapshot

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `PlatformFact`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct PlatformFact(
    PlatformId Id,
    Option<PlatformRow> Row,
    CapabilitySet<PlatformCapability> Capabilities) {
    public Option<FormFactor> Factor => Row.Map(static row => row.Factor);
}
```

To:

```csharp
public readonly record struct PlatformFact(
    PlatformId Id,
    Option<PlatformRow> Row,
    bool IsMobile,
    CapabilitySet<PlatformCapability> Capabilities);
```

Why: `StructLayout(Auto)` adds nothing to an ordinary record struct, and recognized-row lookup should not determine a host fact that `Platform.IsMobile` already supplies.

Change: Capture `Platform.IsMobile` during `Snapshot` and remove the forwarding projection and layout attribute.

Delta: -2 LOC; declared-member and module-level type counts are unchanged.

# 18. Keep only the load-bearing platform operations

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `HostPlatform`
```csharp
public static class HostPlatform {
    public static Fin<PlatformFact> Snapshot();

    public static Fin<Unit> Demand(PlatformClaim claim);

    public static Fin<TResult> Scope<TResult>(PlatformScope scope, Func<Fin<TResult>> body);
}
```

To:

```csharp
public static class HostPlatform {
    public static Fin<PlatformFact> Snapshot();

    public static Fin<Unit> Demand(PlatformRequirement requirement);
}
```

Why: `Snapshot` performs platform admission and `Demand` is the capability gate. `Scope` only renames distinct Eto context operations and has no consumer.

Change: Delete `Scope` and accept the renamed requirement at the retained gate.

Delta: -2 LOC and -1 declared method; module-level type count is unchanged.

# 19. Make style rows report registration failure

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `StyleRow`
```csharp
public sealed record StyleRow(StyleKey Tag, Action<StyleContext> Seat) {
    public static StyleRow OfWidget<TWidget>(StyleKey tag, Action<TWidget, ThemeSnapshot> dress) where TWidget : Widget;
    public static StyleRow OfHandler<THandler>(StyleKey tag, Action<THandler, ThemeSnapshot> dress) where THandler : class, Widget.IHandler;
}
```

To:

```csharp
public sealed record StyleRow(StyleKey Key, Func<StyleContext, Fin<Unit>> Register) {
    public static StyleRow ForWidget<TWidget>(StyleKey key, Action<TWidget, ThemeSnapshot> apply) where TWidget : Widget;
    public static StyleRow ForHandler<THandler>(StyleKey key, Action<THandler, ThemeSnapshot> apply) where THandler : class, Widget.IHandler;
}
```

Why: `Action<StyleContext>` leaves a host registration refusal outside the result carrier. The generic factories are load-bearing type erasure, but their names should state registration targets and style application.

Change: Return `Fin<Unit>` from row registration and replace `Tag`, `Seat`, `Of*`, and `dress` with registry terms.

Delta: 0 LOC; no module-level type or declared-member change.

# 20. Delete the partial-success style result

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `StyleSeat`
```csharp
public sealed class StyleSeat : IDisposable {
    public TelemetrySource Owner { get; }
    public Seq<StyleRow> Claimed { get; }
    public Seq<(StyleRow Row, Error Cause)> Refused { get; }
    public void Dispose();
}
```

To:

```csharp
// StyleSeat DELETED
```

Why: Duplicate keys and foreign claims are independent admissions that must all refuse before any `Style.Add`. Publishing claimed and refused rows after mutation preserves an invalid partial registry.

Change: Accumulate claims before mutation, register only after admission succeeds, and return the inerting release directly.

Delta: -6 LOC, -1 module-level type, and -4 declared members.

# 21. Collapse the theme port onto registration, application, and change

From:

`libs/dotnet/Rasm/.planning/Interaction/platform.md` — `[04]-[PLATFORM]` code fence, `ThemePort`
```csharp
public sealed class ThemePort {
    public static Fin<ThemePort> Of(ThemeGrid grid);

    public ThemeSnapshot Current { get; }
    public Seq<Error> Failures { get; }

    public Fin<Lease<StyleSeat>> Register(TelemetrySource owner, FaultCell faults, params ReadOnlySpan<StyleRow> rows);

    public Fin<Unit> Wear(Widget widget, StyleKey style);

    public Fin<Unit> Provide(IStyleProvider provider);

    public Unit Track(Control control);

    public Fin<ThemeChange> Change(ThemeShift shift);
}
```

To:

```csharp
public sealed class ThemePort(ThemeGrid grid) {
    public Fin<Lease<IDisposable>> Register(
        TelemetrySource owner, FaultCell faults, params ReadOnlySpan<StyleRow> rows);

    public Fin<Unit> Apply(Widget widget, StyleKey style);
    public Fin<ThemeChange> Change(ThemeShift shift);
}
```

Why: `ThemeGrid.Freeze` already admits construction; `Current` forwards to the grid, `Failures` duplicates `ThemeChange`, `Provide` wraps `Style.Provider`, and public `Track` exposes an internal registration detail. Registry mutation remains `Fin`: independent claim checks accumulate before conversion, then ordered `Style.Add` calls short-circuit and inert the successful prefix on failure.

Change: Construct the port directly, retain claimed-style application and rebroadcasting theme changes, return the inerting lease, and delete five convenience members.

Delta: -9 LOC and -4 declared members; module-level type count is unchanged.

Ripples: In `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md`, replace the `ThemePort.Of(grid)` query bind with `new ThemePort(grid)` after `ThemeGrid.Freeze(...).ToFin()` succeeds.
