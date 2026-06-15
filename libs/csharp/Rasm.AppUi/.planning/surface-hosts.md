# [APPUI_SURFACE_HOSTS]

Rasm.AppUi mounts one shell into every admitted host substrate through a single seven-case `SurfaceHost` axis: one seam record carries every host-side delegate column, one mount transaction produces the surface receipt and teardown session, one embed capsule owns the foreign-view boundary, one scheduler boundary completes the UI marshal port, and per-RID native asset rows prove load identity. The page owns the host axis, the embedding capsule, the scheduler boundary, the native asset table, and the host fact stream over Avalonia, ReactiveUI.Avalonia, the SkiaSharp and HarfBuzzSharp native families, LanguageExt rails, and the Rasm.Rhino and Rasm.Grasshopper mount seams.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                           |
| :-----: | :----------------- | :--------------------------------------------------------------- |
|   [1]   | HOST_AXIS          | Seven-case host axis, seam columns, one mount transaction        |
|   [2]   | EMBED_CAPSULE      | Foreign-view embedding capsule, lifecycle order, platform policy |
|   [3]   | SCHEDULER_BOUNDARY | One UI-thread boundary completing the scheduler port marshal     |
|   [4]   | NATIVE_ASSETS      | Per-RID Skia and HarfBuzz rows with load-identity receipts       |
|   [5]   | SCALE_FOCUS        | Closed host fact union for scale, visibility, focus, appearance  |

## [2]-[HOST_AXIS]

- Owner: `SurfaceHost` — one `[Union]` host axis; `SurfaceSeam` — the host-delegate column record; `SurfaceRow` — the resolved policy row; `Surfaces` — the total dispatch and mount surface; `SurfaceFault` — the fault family; `SurfaceReceipt` and `SurfaceSession` — mount evidence.
- Cases: AvaloniaDesktopWindow, RhinoPanel, RhinoModal, Gh2CompanionWindow, SidecarShell, WebBrowser, Headless; `SurfaceFault` = Text | HostAbsent | MountRejected | HandleUnavailable | ThreadAffinity in the 4100 code band.
- Entry: `Fin<SurfaceSession> Mount(SurfaceHost host, SurfaceSeam seam, Control content, ClockPolicy clocks, CorrelationId correlation)` — `Fin` aborts on absent host, rejected mount, missing handle, and thread-affinity violation.
- Auto: one mount transaction replaces seven boot programs — boot-edge guard, builder shaping, parent-handle capture, scale capture, disposal registration, and receipt emission land in one fold; raw case keys serialize through the suite wire law as locked kind literals.
- Receipt: `SurfaceReceipt` — host case, native handle identity as descriptor and value, scale, `Instant`, `CorrelationId`; `TelemetryRow` contributes the mount-outcome and scale-flip instruments inward through the AppHost `TelemetryContributorPort`.
- Packages: Avalonia, Avalonia.Desktop, Avalonia.Headless, Avalonia.Skia, ReactiveUI.Avalonia, System.Reactive, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Rhino (project), Rasm.Grasshopper (project)
- Growth: one case row — payload fields, seam column, capability set — absorbs a new host substrate with zero new surface, and one host instrument is one `InstrumentRow` on `Surfaces.TelemetryRow`; `WebBrowser` stays the designed growth case whose future activation is one seam column plus one dispatch arm swap.
- Boundary: `Surfaces` is the named boundary capsule for the statement carve-out on its boot-edge guard; host-agnostic sourcing law — every probe, marshal, mount, and fact delegate is a `SurfaceSeam` column and no dispatch arm names a host API: rhino rows cross only the Rasm.Rhino panel, semi-modal, and UI-thread ports (the catching marshal that wraps the swallowing host invoke ships with Rasm.Rhino), the gh2 row crosses only the Rasm.Grasshopper mount seam, and the empty-host shell crosses nothing; boot is one `SetupWithoutStarting` admission behind the `Interlocked` edge guard and a second `AppBuilder` or lifetime anywhere is the rejected form; the Skia backend GPU cache is tuned once at boot through one `SkiaOptions` value carrying `MaxGpuResourceSizeBytes` from the `GpuResourceBudget` anchor with `UseOpacitySaveLayer` true so the render-hash lanes share one deterministic GPU budget and a per-shell GPU knob is the rejected form; `WebBrowser` carries an empty capability set and zero payload, so its wire key is its only live surface; the headless row holds host-document capability structurally false, draws through Skia for the render-hash lanes, and is the mount surface of the command-journal replay lane.

```csharp signature
[Union]
public abstract partial record SurfaceHost {
    private SurfaceHost() { }
    public sealed record AvaloniaDesktopWindow : SurfaceHost;
    public sealed record RhinoPanel(Guid PanelId) : SurfaceHost;
    public sealed record RhinoModal : SurfaceHost;
    public sealed record Gh2CompanionWindow : SurfaceHost;
    public sealed record SidecarShell : SurfaceHost;
    public sealed record WebBrowser : SurfaceHost;
    public sealed record Headless : SurfaceHost;
}

[Union]
public abstract partial record SurfaceFault : Expected, IValidationError<SurfaceFault> {
    private SurfaceFault(string detail, int code) : base(detail, code, None) { }

    public static SurfaceFault Create(string message) => new Text(message);

    public sealed record Text : SurfaceFault { public Text(string detail) : base(detail, 4100) { } }
    public sealed record HostAbsent : SurfaceFault { public HostAbsent(string detail) : base(detail, 4101) { } }
    public sealed record MountRejected : SurfaceFault { public MountRejected(string detail) : base(detail, 4102) { } }
    public sealed record HandleUnavailable : SurfaceFault { public HandleUnavailable(string detail) : base(detail, 4103) { } }
    public sealed record ThreadAffinity : SurfaceFault { public ThreadAffinity(string detail) : base(detail, 4104) { } }
}

public sealed record SurfaceSeam(
    Func<Guid, Func<EmbedCapsule, Fin<IDisposable>>> PanelMount,
    Func<EmbedCapsule, Fin<IDisposable>> ModalMount,
    Func<EmbedCapsule, Fin<IDisposable>> CompanionMount,
    Func<Action, IO<Unit>> HostMarshal,
    Func<bool> OnUiThread,
    Func<AppBuilder, Fin<Unit>> RunLoop,
    Func<double> Scale,
    Func<Action<SurfaceFact>, IDisposable> HostFacts);

public sealed record SurfaceRow(
    Func<AppBuilder, AppBuilder> Build,
    Func<AppBuilder, Fin<Unit>> Start,
    Func<Action, IO<Unit>> Marshal,
    Func<double> Scale,
    Func<bool> OnUiThread,
    Func<Control, Fin<(long Handle, string Descriptor, IDisposable Teardown)>> Attach,
    Func<Action<SurfaceFact>, IDisposable> Facts,
    FrozenSet<Capability> Capabilities,
    bool Interactive);

public sealed record SurfaceReceipt(SurfaceHost Host, string Descriptor, long Handle, double Scale, Instant At, CorrelationId Correlation);

public sealed record SurfaceSession(SurfaceReceipt Receipt, Func<Action<SurfaceFact>, IDisposable> Facts, IDisposable Teardown) : IDisposable {
    public void Dispose() => Teardown.Dispose();
}
```

```csharp signature
public static class Surfaces {
    static int booted;

    public const long GpuResourceBudget = 268_435_456;

    static readonly SkiaOptions SkiaBudget = new() { MaxGpuResourceSizeBytes = GpuResourceBudget, UseOpacitySaveLayer = true };

    public static Fin<SurfaceRow> Row(SurfaceHost host, SurfaceSeam seam) => host.Switch(
        state: seam,
        avaloniaDesktopWindow: static (s, own) => Fin.Succ(Shell(s, static b => b.UsePlatformDetect().UseSkia().With(SkiaBudget).UseReactiveUI(), s.RunLoop, interactive: true)),
        rhinoPanel: static (s, own) => Fin.Succ(Embedded(s, s.PanelMount(own.PanelId))),
        rhinoModal: static (s, own) => Fin.Succ(Embedded(s, s.ModalMount)),
        gh2CompanionWindow: static (s, own) => Fin.Succ(Embedded(s, s.CompanionMount)),
        sidecarShell: static (s, own) => Fin.Succ(Shell(s, static b => b.UsePlatformDetect().UseSkia().With(SkiaBudget).UseReactiveUI(), s.RunLoop, interactive: true)),
        webBrowser: static (s, own) => Fin.Fail<SurfaceRow>(new SurfaceFault.HostAbsent(nameof(SurfaceHost.WebBrowser))),
        headless: static (s, own) => Fin.Succ(Shell(s, static b => b.UseSkia().With(SkiaBudget).UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).UseReactiveUI(), Setup, interactive: false)));

    public static Fin<Unit> Boot(SurfaceHost host, SurfaceSeam seam, Func<AppBuilder> entry) =>
        from row in Row(host, seam)
        from started in FirstBoot() ? row.Start(row.Build(entry())) : Fin.Succ(unit)
        select started;

    public static Fin<SurfaceSession> Mount(SurfaceHost host, SurfaceSeam seam, Control content, ClockPolicy clocks, CorrelationId correlation) =>
        from row in Row(host, seam)
        from gate in SurfaceScheduler.For(host, row).Affinity(nameof(Mount))
        from attached in row.Attach(content)
        select new SurfaceSession(
            new SurfaceReceipt(host, attached.Descriptor, attached.Handle, row.Scale(), clocks.Now, correlation),
            row.Facts,
            attached.Teardown);

    static SurfaceRow Embedded(SurfaceSeam seam, Func<EmbedCapsule, Fin<IDisposable>> mount) => new(
        Build: static builder => EmbedOptions.Embedded.Admit(builder),
        Start: Setup,
        Marshal: seam.HostMarshal,
        Scale: seam.Scale,
        OnUiThread: seam.OnUiThread,
        Attach: content => new EmbedCapsule(content, EmbedOptions.Embedded).Mounted(mount),
        Facts: seam.HostFacts,
        Capabilities: Capability.Set(Capability.HostDocument),
        Interactive: true);

    static SurfaceRow Shell(SurfaceSeam seam, Func<AppBuilder, AppBuilder> build, Func<AppBuilder, Fin<Unit>> start, bool interactive) => new(
        Build: build,
        Start: start,
        Marshal: SurfaceScheduler.Post,
        Scale: seam.Scale,
        OnUiThread: seam.OnUiThread,
        Attach: Windowed,
        Facts: seam.HostFacts,
        Capabilities: Capability.Set(),
        Interactive: interactive);

    static Fin<Unit> Setup(AppBuilder builder) => Fin.Succ(ignore(builder.SetupWithoutStarting()));

    static bool FirstBoot() => Interlocked.Exchange(ref booted, 1) == 0;

    static Fin<(long Handle, string Descriptor, IDisposable Teardown)> Windowed(Control content) =>
        Fin.Succ(new Window { Content = content })
            .Map(static window => (fun(window.Show)(), window).Item2)
            .Map(static window => (
                window.TryGetPlatformHandle() is { } handle ? handle.Handle.ToInt64() : 0L,
                window.TryGetPlatformHandle()?.HandleDescriptor ?? nameof(SurfaceHost.Headless),
                (IDisposable)Disposable.Create(window.Close)));

    public const string MountInstrument = "rasm.appui.surface.mounted";
    public const string ScaleInstrument = "rasm.appui.surface.scaled";

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, MountInstrument, ScaleInstrument);
}
```

## [3]-[EMBED_CAPSULE]

- Owner: `EmbedCapsule` — the foreign-view boundary capsule deriving the embeddable top-level; `EmbedOptions` — the embedded platform policy row.
- Entry: `Fin<(long Handle, string Descriptor, IDisposable Teardown)> Mounted(Func<EmbedCapsule, Fin<IDisposable>> mount)` — `Fin` aborts on handle absence and seam rejection with defensive capsule disposal.
- Auto: construction runs the load-bearing order in one body — `EnforceClientSize` value, `Content`, `Prepare` — and `Mounted` appends retained-view capture, seam attach, and `StartRendering`; teardown composes `StopRendering`, seam detach, `Dispose` in declared order.
- Packages: Avalonia, System.Reactive, LanguageExt.Core, Rasm.Rhino (project)
- Growth: one `EmbedOptions` policy value per new platform knob; zero new surface.
- Boundary: `EmbedCapsule` is the named boundary capsule for the statement carve-out — the constructor carries the ordered statements; `GetNSViewRetained` hands a retained pointer whose release belongs to the host seam after detach, and the accessor carries Avalonia's unstable-API obsolete marker, so the capsule's `RetainedView` body is the single acknowledged suppression site; `EnforceClientSize` is a protected setter reachable only inside the derived capsule, and the host seam pushes frame sync on every panel-resize fact while it holds true; `MacOSPlatformOptions` and `AvaloniaNativePlatformOptions` values enter only through `EmbedOptions.Admit` and a hardcoded platform knob in boot code is the rejected form — `ShowInDock` false keeps embedded rows out of the macOS Dock, `DisableDefaultApplicationMenuItems` strips the default app menu under the host menu bar, and `RenderingMode` over `AvaloniaNativeRenderingMode` is the backend policy column over Metal, OpenGl, and Software whose embedded value the render research row decides; Avalonia owns GPU backend selection through `RenderingMode`, so a direct `GRContext.CreateMetal`/`CreateVulkan`/`CreateDirect3D`/`CreateGl` call inside a dispatch arm is the rejected form (PROHIBITION host-API-in-arm) — a shared-context requirement against the host pipeline rides one `SurfaceSeam` delegate column bound at composition, never a per-host GPU call site, and the exact shared-context spelling is the EMBED_SPIKE render research row; the win32 reparent column and the dispatcher-pump regime stay research-gated.

```csharp signature
public sealed record EmbedOptions(
    bool DisableAvaloniaAppDelegate,
    bool DisableSetProcessName,
    bool DisableNativeMenus,
    bool DisableDefaultApplicationMenuItems,
    bool ShowInDock,
    bool EnforceClientSize,
    Option<string> NativeLibraryPath,
    Option<Seq<AvaloniaNativeRenderingMode>> RenderingMode) {
    public static readonly EmbedOptions Embedded = new(
        DisableAvaloniaAppDelegate: true,
        DisableSetProcessName: true,
        DisableNativeMenus: true,
        DisableDefaultApplicationMenuItems: true,
        ShowInDock: false,
        EnforceClientSize: true,
        NativeLibraryPath: None,
        RenderingMode: None);

    public AppBuilder Admit(AppBuilder builder) =>
        builder
            .With(RenderingMode
                .Map(modes => new AvaloniaNativePlatformOptions { AvaloniaNativeLibraryPath = (string?)NativeLibraryPath.Case, RenderingMode = [.. modes] })
                .IfNone(() => new AvaloniaNativePlatformOptions { AvaloniaNativeLibraryPath = (string?)NativeLibraryPath.Case }))
            .UseSkia()
            .UseAvaloniaNative()
            .UseReactiveUI()
            .With(new MacOSPlatformOptions {
                DisableAvaloniaAppDelegate = DisableAvaloniaAppDelegate,
                DisableSetProcessName = DisableSetProcessName,
                DisableNativeMenus = DisableNativeMenus,
                DisableDefaultApplicationMenuItems = DisableDefaultApplicationMenuItems,
                ShowInDock = ShowInDock,
            });
}

public sealed class EmbedCapsule : EmbeddableControlRoot {
    public EmbedCapsule(Control content, EmbedOptions options) {
        EnforceClientSize = options.EnforceClientSize;
        Content = content;
        Prepare();
    }

    public Fin<(long Handle, string Descriptor, IDisposable Teardown)> Mounted(Func<EmbedCapsule, Fin<IDisposable>> mount) =>
        (from view in RetainedView()
         from detach in mount(this)
         from live in Start()
         select (view.Handle, view.Descriptor, (IDisposable)new CompositeDisposable(Disposable.Create(StopRendering), detach, Disposable.Create(Dispose))))
        .MapFail(fault => (fun(Dispose)(), fault).Item2);

    public Fin<(long Handle, string Descriptor)> RetainedView() =>
        TryGetPlatformHandle() switch {
            IMacOSTopLevelPlatformHandle mac => Fin.Succ((mac.GetNSViewRetained().ToInt64(), "NSView")),
            { } handle => Fin.Succ((handle.Handle.ToInt64(), handle.HandleDescriptor ?? string.Empty)),
            null => Fin.Fail<(long, string)>(new SurfaceFault.HandleUnavailable(nameof(EmbedCapsule))),
        };

    public Fin<Unit> Start() => Fin.Succ(fun(StartRendering)());
}
```

```mermaid
stateDiagram-v2
    [*] --> Content
    Content --> Prepare
    Prepare --> StartRendering
    StartRendering --> StopRendering
    StopRendering --> Dispose
    Dispose --> [*]
```

## [4]-[SCHEDULER_BOUNDARY]

- Owner: `SurfaceScheduler` — the one record where the UI dispatcher, the Avalonia reactive scheduler, and the host marshal meet.
- Entry: `SurfaceScheduler For(SurfaceHost host, SurfaceRow row, Option<TimeProvider> virtualTime = default)` — pure projection over the resolved row; the UI-thread predicate is sourced once from `row.OnUiThread`, which the row builders carry from `seam.OnUiThread` at resolution, so no parallel `onUiThread` parameter threads beside the row.
- Auto: `Port` completes `UiSchedulerPort.Marshal` from this boundary at the composition root — `Phases` and `Degradation` arrive already bound; `UseReactiveUI` admission wires the reactive main-thread scheduler onto `AvaloniaScheduler`.
- Packages: ReactiveUI.Avalonia, Avalonia, System.Reactive, LanguageExt.Core, BCL inbox
- Growth: one marshal column per new host thread regime; carrier swap on the virtual-time slot; zero new surface.
- Boundary: `Affinity` is the single thread-affinity assertion and a per-call-site access check is the rejected form; the UI-thread predicate originates once at the seam's `OnUiThread` column and flows through `row.OnUiThread` into the scheduler — one source, no parallel parameter — so the access-assertion spelling stays a seam delegate and never a hardcoded dispatcher call inside a dispatch arm; embedded rows marshal through the seam's host column, windowed and headless rows post through the `AvaloniaScheduler` UI scheduler; the headless row receives its virtual `TimeProvider` from the test composition so the command-journal replay lane runs under deterministic time; `ObserveOn` rides `Ui` exactly once inside binding capsules, never at call sites.

```csharp signature
public sealed record SurfaceScheduler(IScheduler Ui, Func<Action, IO<Unit>> Marshal, Func<bool> OnUiThread, Option<TimeProvider> VirtualTime) {
    public static SurfaceScheduler For(SurfaceHost host, SurfaceRow row, Option<TimeProvider> virtualTime = default) => new(
        AvaloniaScheduler.Instance,
        row.Marshal,
        row.OnUiThread,
        host is SurfaceHost.Headless ? virtualTime : None);

    public static IO<Unit> Post(Action action) =>
        IO.lift(() => (AvaloniaScheduler.Instance.Schedule(action), unit).Item2);

    public static UiSchedulerPort Port(UiSchedulerPort spine, SurfaceScheduler boundary) => spine with { Marshal = boundary.Marshal };

    public Fin<Unit> Affinity(string operation) =>
        OnUiThread() ? Fin.Succ(unit) : Fin.Fail<Unit>(new SurfaceFault.ThreadAffinity(operation));
}
```

## [5]-[NATIVE_ASSETS]

- Owner: `NativeAssetRow` — per-RID asset rows; `NativeAssetReceipt` — load-identity evidence; `NativeAssets` — the frozen row table and identity fold.
- Entry: `Fin<Seq<NativeAssetReceipt>> Identity(NativeAssetRow row)` — traverses the row's native libraries into receipts.
- Receipt: `NativeAssetReceipt` — library, version, path, RID.
- Packages: SkiaSharp.NativeAssets.macOS, SkiaSharp.NativeAssets.Win32, SkiaSharp.NativeAssets.Linux.NoDependencies, HarfBuzzSharp.NativeAssets.macOS, HarfBuzzSharp.NativeAssets.Win32, HarfBuzzSharp.NativeAssets.Linux, LanguageExt.Core, BCL inbox
- Growth: one `NativeAssetRow` per new RID; zero new surface.
- Boundary: one shaping family rides every desktop row — each Skia asset row pairs its HarfBuzz row across the full desktop RID matrix (osx universal, win-x64/x86/arm64, linux-x64/arm64, linux-musl-x64) so cross-architecture load identity is one row per RID and a missing-architecture load surfaces as an absent receipt; the fontconfig-dependent Linux Skia variant stays pinned and excluded at the AppUi admission, so NoDependencies is the only Linux Skia asset and the glibc and musl rows share it; the WebAssembly native pins stay dormant transitive floors with no row while `WebBrowser` stays a designed growth case; identity receipts run at mount, so a wrong-RID load surfaces as a receipt, never a draw fault.

```csharp signature
public sealed record NativeAssetReceipt(string Library, string Version, string Path, string Rid);

public sealed record NativeAssetRow(string Rid, string SkiaAsset, string ShapingAsset, string HostAsset, Seq<string> Libraries);

public static class NativeAssets {
    public static readonly Seq<NativeAssetRow> Rows = Seq(
        new NativeAssetRow("osx", "SkiaSharp.NativeAssets.macOS", "HarfBuzzSharp.NativeAssets.macOS", "libAvaloniaNative.dylib", Seq("libSkiaSharp", "libHarfBuzzSharp", "libAvaloniaNative")),
        new NativeAssetRow("win-x64", "SkiaSharp.NativeAssets.Win32", "HarfBuzzSharp.NativeAssets.Win32", "Avalonia.Win32.dll", Seq("libSkiaSharp", "libHarfBuzzSharp")),
        new NativeAssetRow("win-x86", "SkiaSharp.NativeAssets.Win32", "HarfBuzzSharp.NativeAssets.Win32", "Avalonia.Win32.dll", Seq("libSkiaSharp", "libHarfBuzzSharp")),
        new NativeAssetRow("win-arm64", "SkiaSharp.NativeAssets.Win32", "HarfBuzzSharp.NativeAssets.Win32", "Avalonia.Win32.dll", Seq("libSkiaSharp", "libHarfBuzzSharp")),
        new NativeAssetRow("linux-x64", "SkiaSharp.NativeAssets.Linux.NoDependencies", "HarfBuzzSharp.NativeAssets.Linux", "Avalonia.X11.dll", Seq("libSkiaSharp", "libHarfBuzzSharp")),
        new NativeAssetRow("linux-arm64", "SkiaSharp.NativeAssets.Linux.NoDependencies", "HarfBuzzSharp.NativeAssets.Linux", "Avalonia.X11.dll", Seq("libSkiaSharp", "libHarfBuzzSharp")),
        new NativeAssetRow("linux-musl-x64", "SkiaSharp.NativeAssets.Linux.NoDependencies", "HarfBuzzSharp.NativeAssets.Linux", "Avalonia.X11.dll", Seq("libSkiaSharp", "libHarfBuzzSharp")));

    public static Fin<Seq<NativeAssetReceipt>> Identity(NativeAssetRow row) =>
        row.Libraries.TraverseM(library => Probe(row, library)).As();

    static Fin<NativeAssetReceipt> Probe(NativeAssetRow row, string library) =>
        Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
            .Where(module => module.ModuleName.Contains(library, StringComparison.OrdinalIgnoreCase))
            .Select(module => new NativeAssetReceipt(library, module.FileVersionInfo.FileVersion ?? string.Empty, module.FileName, row.Rid))
            .ToSeq()
            .HeadOrNone() is { IsSome: true, Case: NativeAssetReceipt receipt }
                ? Fin.Succ(receipt)
                : Fin.Fail<NativeAssetReceipt>(new SurfaceFault.HostAbsent(library));
}
```

## [6]-[SCALE_FOCUS]

- Owner: `SurfaceFact` — one closed host fact union for scale, visibility, focus, and appearance.
- Cases: ScaleChanged, VisibilityChanged, FocusChanged, AppearanceChanged.
- Packages: Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one fact case per new host signal extends the `SurfaceFact` family; every subscriber is a total fold over the closed family, zero new surface.
- Boundary: facts enter only through the seam's `HostFacts` column — macOS rows feed `NSScreen` `BackingScaleFactor` flips and appearance changes host-side, panel rows feed visibility and focus from panel events through the Rasm.Rhino port; visibility facts feed the activation rail and live-data suspend-resume, appearance facts feed the host-matched variant re-probe, scale facts feed DPI-variant selection; a second host event channel beside this union is the rejected form.

```csharp signature
[Union]
public abstract partial record SurfaceFact {
    private SurfaceFact() { }
    public sealed record ScaleChanged(double Scale) : SurfaceFact;
    public sealed record VisibilityChanged(bool Visible) : SurfaceFact;
    public sealed record FocusChanged(bool Focused) : SurfaceFact;
    public sealed record AppearanceChanged(bool Dark) : SurfaceFact;
}
```

## [7]-[RESEARCH]

- [EMBED_SPIKE]: the seam `OnUiThread` access-assertion spelling and scheduler boundary under the Rhino-owned AppKit run-loop — the UI-thread predicate the seam column binds, input and IME delivery, and the CADisplayLink-paced pump fallback row; `EnforceClientSize` tracking of the foreign host view against seam-pushed frame sync on panel resize; render-backend selection for the embedded surface — `RenderingMode` orderings of Metal against the host pipeline against software raster, compared on frame-elapsed receipts; the shared-`GRContext` spelling against the host GPU pipeline when the embedded surface composites into a host-owned context, bound as one `SurfaceSeam` delegate column.
- [WIN32_ROUTE]: raw HWND reparent against the WinForms interoperability host (a package outside the current restore set) and the Rhino Windows panel host type.
