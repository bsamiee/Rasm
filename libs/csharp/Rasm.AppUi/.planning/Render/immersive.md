# [APPUI_RENDER_IMMERSIVE]

One immersive owner binds OpenXR stereo design review plus `XR_FB_passthrough` onto the same `Wgpu` device the viewport leases, with `ImmersiveMode` carrying immersive-versus-flat as a value so a host without an OpenXR loader renders the flat viewport through the same receipt family. `ImmersiveSession` runs the `Instance -> SystemId -> Session -> Swapchain` lifecycle against the shared graphics binding, `XrFrame` runs the predicted-display-time `WaitFrame`/`BeginFrame`/`LocateViews`/`EndFrame` loop submitting one `CompositionLayerProjection` per frame, `XrInput` is the action-set controller model, and `Passthrough` chains the `XR_FB_passthrough` environment-blend layer under the rendered scene. The page owns the session lifecycle, stereo frame loop, action-set input, and passthrough composition while sharing the viewport's one `Wgpu` device. `Silk.NET.OpenXR`, its FB extensions, `GpuBinding.Wgpu`, Thinktecture, and LanguageExt supply the substrate; the flat fold remains a complete successful mode when no XR runtime is available.

## [01]-[INDEX]

- [01]-[XR_SESSION]: Instance/system/session lifecycle against the shared `Wgpu` graphics binding; flat-fold fallback.
- [02]-[STEREO_FRAME]: The predicted-display-time frame loop submitting one stereo projection layer per frame.
- [03]-[XR_INPUT_PASSTHROUGH]: The action-set controller model and the `XR_FB_passthrough` env-blend composition.

## [02]-[XR_SESSION]

- Owner: `ImmersiveMode` `[Union]` the availability algebra — `Immersive(ImmersiveSession)` or `Flat(FlatCause)`; `FlatCause` `[SmartEnum<string>]` the flat-state vocabulary; `ImmersiveSession` the OpenXR session lifecycle; `XrHandle`/`XrHandleLedger` the typed handle-to-destroy ledger; `XrRuntime` the impossible-state-free availability union; `ImmersiveFault` the typed fault family on the `AppUiFaultBand.Immersive` registry row (6120).
- Cases: `ImmersiveFault` = Text | SystemUnavailable | SessionRejected | SwapchainFailed | ReleaseFailed — codes derive through `AppUiFaultBand.Immersive`; `FlatCause` = LoaderAbsent | PlatformUnsupported | SystemAbsent — capability states, not faults: an absent loader, a loaderless platform, or a runtime with no attached HMD lands as `Flat` with its cause, and only a present-but-refusing runtime faults.
- Entry: `public static Fin<ImmersiveMode> Create(WgpuDevice device, XrRuntime runtime, Func<WgpuDevice, XrRuntime.Ready, Fin<ImmersiveSession>> bind)` dispatches the complete `XrRuntime.Ready(PassthroughAdvertised, ViewConfig, BlendModes)` payload to the native-open continuation — `Ready.Blend` selects the strongest advertised `EnvironmentBlendMode`, so opaque VR, additive AR, and admitted passthrough are runtime-capability outcomes, never a constant — and preserves `XrRuntime.Unavailable(FlatCause)` as the successful desktop floor. `Ready` alone can carry advertised extension and view-configuration facts, so an absent loader or system can never coexist with usable runtime data. The continuation creates the OpenXR instance, system, session, eye swapchains, and reference space against the shared `WgpuDevice`; `ImmersiveFrame.Frame` then returns the same `FrameReceipt` family for stereo and flat modes.
- Auto: the session creates against the graphics-binding `next` chain sharing the same physical device, queue family, and queue index the wgpu instance negotiated (`KHR_vulkan_enable2`, `GraphicsBindingVulkanKHR`) so the meshlet/path-trace/splat passes render into the OpenXR swapchain images with the one device — a second GPU device for the immersive path is the cross-adapter copy penalty the shared binding avoids; the session probes for `XR_FB_passthrough` through `EnumerateInstanceExtensionProperties` and lists it in `InstanceCreateInfo.EnabledExtensionNames` when advertised; the absence of an installed loader (`libopenxr_loader`) is the `Flat(LoaderAbsent)` capability value that renders through the flat `Render/pipeline` viewport, so the immersive session is an optional surface the desktop path degrades from with the cause preserved and no XR session constructed; every acquired native handle records as its typed `XrHandle` case on the session `XrHandleLedger`, and release is the ledger fold in reverse-acquisition order through the matching `DestroyXxx`/`DestroyXxxFB` entrypoint with each `Result` checked.
- Receipt: the session creation emits a session-resolved evidence row — system id, view config, swapchain format, passthrough-available flag; `TelemetryRow` contributes the session-resolved and session-absent instruments inward through the AppHost `TelemetryContributorPort`.
- Packages: Silk.NET.OpenXR, Silk.NET.OpenXR.Extensions.FB, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new XR extension is one enabled-extension-name row; one immersive instrument is one `InstrumentRow` on `Immersive.TelemetryRow`; zero new surface.
- Boundary: the session shares the one `Wgpu` device the `Render/pipeline` viewport leases through the branch `ONE_WGPU_DEVICE` `EMBED_CAPSULE` law — a second GPU context for the immersive path is the `[05]-[PROHIBITIONS]` rejected form, so the OpenXR session created with the Vulkan binding shares the wgpu device's physical device, queue family, and queue index; `Silk.NET.OpenXR` carries no bundled native runtime so it P/Invokes the host-installed loader (`.api/api-silk-openxr.md` local admission) and the loader-absent case is `Flat(LoaderAbsent)` — macOS ships no Apple OpenXR loader (visionOS uses ARKit/RealityKit), so the immersive session activates on Windows/Linux desktop hosts where the loader is present and lands `Flat(PlatformUnsupported)` on macOS, the session create being a capability probe not a launch precondition, and a rejected XR session (`SessionRejected`/`SwapchainFailed`) stays a distinguishable fault, never conflated with the normal no-loader state; all native handles (`Instance`, `Session`, `Swapchain`, `Space`, `ActionSet`, and the FB passthrough/foveation set) release through the `XrHandleLedger` reverse-order fold naming each matching `DestroyXxx`/`DestroyXxxFB` entrypoint with its `Result` checked — an opaque `IDisposable` teardown erasing the handle-to-destroy correspondence is the deleted form; the runtime arm is SPIKE-gated exactly as the viewport; the `Silk.NET.OpenXR.Extensions.FB` passthrough rides the same `2.23.0` line as the core (Silk.NET publishes its whole core-plus-extension set from one monorepo release) so no version split.

```csharp signature
[Union]
public abstract partial record ImmersiveFault : Expected, IValidationError<ImmersiveFault> {
    private ImmersiveFault(string detail, int code) : base(detail, code, None) { }

    public static ImmersiveFault Create(string message) => new Text(message);

    public sealed record Text : ImmersiveFault { public Text(string detail) : base(detail, AppUiFaultBand.Immersive.Code(0)) { } }
    public sealed record SystemUnavailable : ImmersiveFault { public SystemUnavailable(string detail) : base(detail, AppUiFaultBand.Immersive.Code(1)) { } }
    public sealed record SessionRejected : ImmersiveFault { public SessionRejected(string detail) : base(detail, AppUiFaultBand.Immersive.Code(2)) { } }
    public sealed record SwapchainFailed : ImmersiveFault { public SwapchainFailed(string detail) : base(detail, AppUiFaultBand.Immersive.Code(3)) { } }
    public sealed record ReleaseFailed : ImmersiveFault {
        public ReleaseFailed(Seq<(string Handle, Result Outcome)> failures)
            : base($"xr/release: {string.Join(", ", failures.Map(static row => $"{row.Handle}={row.Outcome}"))}", AppUiFaultBand.Immersive.Code(4)) { }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlatCause {
    public static readonly FlatCause LoaderAbsent = new("loader-absent");
    public static readonly FlatCause PlatformUnsupported = new("platform-unsupported");
    public static readonly FlatCause SystemAbsent = new("system-absent");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record XrRuntime {
    private XrRuntime() { }
    public sealed record Ready(bool PassthroughAdvertised, ViewConfigurationType ViewConfig, Seq<EnvironmentBlendMode> BlendModes) : XrRuntime {
        // Composition selects the strongest ADVERTISED blend: FB passthrough rides AlphaBlend, an AR
        // runtime's AdditiveBlend composits over the see-through display, and Opaque is the VR floor —
        // an unadvertised mode is unrepresentable as the selected composite.
        public EnvironmentBlendMode Blend =>
            PassthroughAdvertised && BlendModes.Contains(EnvironmentBlendMode.AlphaBlend) ? EnvironmentBlendMode.AlphaBlend
            : BlendModes.Contains(EnvironmentBlendMode.AdditiveBlend) ? EnvironmentBlendMode.AdditiveBlend
            : EnvironmentBlendMode.Opaque;
    }

    public sealed record Unavailable(FlatCause Cause) : XrRuntime;

    public static readonly XrRuntime Absent = new Unavailable(FlatCause.LoaderAbsent);
}

// The availability algebra: capability absence is the NORMAL Flat state carrying its cause, and only a
// present-but-refusing runtime faults — both arms render through the one RenderGraph, so the desktop
// floor preserves the FrameReceipt family with zero XR session constructed.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImmersiveMode {
    private ImmersiveMode() { }
    public sealed record Immersive(ImmersiveSession Session) : ImmersiveMode;
    public sealed record Flat(FlatCause Cause) : ImmersiveMode;

    public static Fin<ImmersiveMode> Create(
        WgpuDevice device,
        XrRuntime runtime,
        Func<WgpuDevice, XrRuntime.Ready, Fin<ImmersiveSession>> bind) =>
        runtime.Switch(
            state: (Device: device, Bind: bind),
            ready: static (state, ready) => state.Bind(state.Device, ready).Map(static ImmersiveMode (session) => new Immersive(session)),
            unavailable: static (_, absent) => Fin.Succ<ImmersiveMode>(new Flat(absent.Cause)));
}

// Native lifetime is the HANDLE-TO-DESTROY correspondence, never an opaque IDisposable: every acquired
// core and FB handle records as its typed case, so each handle's matching release entrypoint is
// recoverable from the ledger alone and an unreleased handle is a visible ledger row.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record XrHandle {
    private XrHandle() { }
    public sealed record InstanceHandle(Instance Handle) : XrHandle;
    public sealed record SessionHandle(Session Handle) : XrHandle;
    public sealed record SwapchainHandle(Swapchain Handle) : XrHandle;
    public sealed record SpaceHandle(Space Handle) : XrHandle;
    public sealed record ActionSetHandle(ActionSet Handle) : XrHandle;
    public sealed record PassthroughHandle(PassthroughFB Handle) : XrHandle;
    public sealed record PassthroughLayerHandle(PassthroughLayerFB Handle) : XrHandle;
    public sealed record FoveationHandle(FoveationProfileFB Handle) : XrHandle;

    public string Key => Switch(
        instanceHandle: static _ => "instance",
        sessionHandle: static _ => "session",
        swapchainHandle: static _ => "swapchain",
        spaceHandle: static _ => "space",
        actionSetHandle: static _ => "action-set",
        passthroughHandle: static _ => "passthrough",
        passthroughLayerHandle: static _ => "passthrough-layer",
        foveationHandle: static _ => "foveation-profile");
}

// Reverse-acquisition release through the matching native entrypoints — DestroySwapchain/DestroySpace/
// DestroyActionSet/DestroySession/DestroyInstance on the core, DestroyPassthroughLayerFB/
// DestroyPassthroughFB/DestroyFoveationProfileFB on the FB tables — each Result checked: a failing destroy
// is a counted ImmersiveFault on the telemetry spine, never a swallowed native error.
public sealed record XrHandleLedger(Seq<XrHandle> Acquired) {
    public static readonly XrHandleLedger Empty = new(Seq<XrHandle>());

    public XrHandleLedger Push(XrHandle handle) => this with { Acquired = Acquired.Add(handle) };

    public Fin<Unit> Release(XR xr, Option<FBPassthrough> fb, Option<FBFoveation> foveation) =>
        Acquired.Rev()
            .Choose(handle => Failed(handle.Key, handle.Switch(
                state: (Xr: xr, Fb: fb, Foveation: foveation),
                instanceHandle: static (s, h) => s.Xr.DestroyInstance(h.Handle),
                sessionHandle: static (s, h) => s.Xr.DestroySession(h.Handle),
                swapchainHandle: static (s, h) => s.Xr.DestroySwapchain(h.Handle),
                spaceHandle: static (s, h) => s.Xr.DestroySpace(h.Handle),
                actionSetHandle: static (s, h) => s.Xr.DestroyActionSet(h.Handle),
                passthroughHandle: static (s, h) => s.Fb.Map(api => api.DestroyPassthroughFB(h.Handle)).IfNone(Result.ErrorHandleInvalid),
                passthroughLayerHandle: static (s, h) => s.Fb.Map(api => api.DestroyPassthroughLayerFB(h.Handle)).IfNone(Result.ErrorHandleInvalid),
                foveationHandle: static (s, h) => s.Foveation.Map(api => api.DestroyFoveationProfileFB(h.Handle)).IfNone(Result.ErrorHandleInvalid))))
            .ToSeq() switch {
                { IsEmpty: true } => FinSucc(unit),
                Seq<(string Handle, Result Outcome)> failures => Fin.Fail<Unit>(new ImmersiveFault.ReleaseFailed(failures)),
            };

    private static Option<(string Handle, Result Outcome)> Failed(string handle, Result outcome) =>
        outcome == Result.Success ? None : Some((handle, outcome));
}

public sealed record ImmersiveSession(
    Instance Instance,
    SystemId System,
    Session Session,
    Seq<Swapchain> EyeSwapchains,
    Space ReferenceSpace,
    Option<Passthrough> Passthrough,
    XrHandleLedger Ledger) {
    public Fin<Unit> Release(XR xr, Option<FBPassthrough> fb, Option<FBFoveation> foveation) =>
        Ledger.Release(xr, fb, foveation);

    public const string ResolvedInstrument = "rasm.appui.immersive.session.resolved";
    public const string AbsentInstrument = "rasm.appui.immersive.session.absent";

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version,
            new(ResolvedInstrument, InstrumentKind.Count, "{session}", "XR sessions resolved by system id"),
            new(AbsentInstrument, InstrumentKind.Count, "{session}", "XR session creation absences"));
}
```

## [03]-[STEREO_FRAME]

- Owner: `XrFrame` the predicted-display-time frame loop; `EyeView` the per-eye pose-and-fov; `ProjectionLayer` the stereo composition layer.
- Entry: `public IO<FrameReceipt> Frame(RenderGraph graph, ViewportClock clock, FrameBudget budget, int tierRank, Func<RenderPass, bool> passMask)` on `ImmersiveSession` — runs `WaitFrame` (predicted display time) -> `BeginFrame` -> `LocateViews` (per-eye pose/fov) -> render each eye into its swapchain image through the shared `Render/pipeline` `RenderGraph` under the governor tier -> `EndFrame` submitting one `CompositionLayerProjection` plus the optional passthrough layer; `ImmersiveFrame.Frame` on `ImmersiveMode` is the one dispatch over the availability algebra.
- Auto: the frame loop is driven by the runtime-predicted display time from `WaitFrame`'s `FrameState`, never a wall clock, so the render anticipates the display deadline; `LocateViews` resolves the two `View` structs (per-eye `Posef`+`Fovf`) and the render walks the one `Render/pipeline` `RenderGraph` once per eye into the eye's acquired swapchain image, so the meshlet/path-trace passes render stereo with no second scene model; `EndFrame` submits one `CompositionLayerProjection` carrying two `CompositionLayerProjectionView` sub-images (left/right eye) plus the passthrough layer beneath when present; the frame seals the same `Render/pipeline` `FrameReceipt` so the immersive frame rides the one evidence family.
- Receipt: the `Render/pipeline` `FrameReceipt` per submitted frame, carrying the stereo backend and the per-eye pass elapsed.
- Packages: Silk.NET.OpenXR, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new view config (quad views) is one `ViewConfigurationType` row; zero new surface.
- Boundary: the frame loop runs the runtime-predicted display time so a wall-clock frame pace ignoring the predicted display time is the rejected form (`.api/api-silk-openxr.md` reject); each eye renders through the one `Render/pipeline` `RenderGraph` so the immersive path re-models no geometry and re-uses the meshlet/path-trace/residency owners; `EndFrame` submits one `CompositionLayerProjection` with two sub-images so a per-eye separate layer is the deleted form; the passthrough layer chains into the same `EndFrame` layer array beneath the projection layer so the rendered BIM scene composites over the camera feed in one frame submit; the frame seals the `Render/pipeline` `FrameReceipt` so the immersive frame mints no second receipt vocabulary; the swapchain images are the shared `Wgpu` device's textures so the eye render and the desktop render share one device lifetime.

```csharp signature
public readonly record struct EyeView(Posef Pose, Fovf Fov, uint SwapchainImageIndex);

public readonly record struct ProjectionLayer(Seq<EyeView> Eyes, Space ReferenceSpace);

public static class XrFrame {
    extension(ImmersiveSession session) {
        public IO<FrameReceipt> Frame(RenderGraph graph, ViewportClock clock, FrameBudget budget, int tierRank, Func<RenderPass, bool> passMask) =>
            from predicted in WaitFrame(session)
            from begin in BeginFrame(session)
            from views in LocateViews(session, predicted)
            from rendered in views.TraverseM(eye => RenderEye(session, graph, eye, clock, budget, tierRank, passMask)).As()
            from receipt in EndFrame(session, predicted, views)
            select receipt;
    }
}

// The one frame entry over the availability algebra: the Immersive arm runs the stereo loop (RenderEye
// projects each located Posef/Fovf onto the per-eye ViewCamera the graph consumes), the Flat arm runs
// the desktop RenderGraph.Frame — one receipt family, two composition arms.
public static class ImmersiveFrame {
    extension(ImmersiveMode mode) {
        public IO<FrameReceipt> Frame(RenderGraph graph, ViewportClock clock, FrameBudget budget, int tierRank, Func<RenderPass, bool> passMask, ViewCamera camera) =>
            mode.Switch(
                immersive: s => s.Session.Frame(graph, clock, budget, tierRank, passMask),
                flat: _ => graph.Frame(clock, budget, tierRank, passMask, camera));
    }
}
```

## [04]-[XR_INPUT_PASSTHROUGH]

- Owner: `XrInput` the action-set controller model; `Passthrough` the `XR_FB_passthrough` env-blend layer; `PassthroughStyle` the edge-color-and-opacity policy; `XrComfort` the refresh-rate and foveation negotiation the governor tier steps.
- Entry: `public Fin<XrInput> Bind(ImmersiveSession session, Seq<XrAction> actions)` — creates the action set, binds actions to interaction-profile paths, and suggests the bindings; `public Fin<Passthrough> Start(ImmersiveSession session)` — creates the passthrough feature and layer against the session and starts the camera feed.
- Auto: input is the action-set model — an `ActionSet` holds `Action`s bound to interaction-profile paths (`/user/hand/left/input/select/click`, `/user/hand/right/input/aim/pose`), `SyncActions` polls them per frame, and `GetActionStatePose`+`LocateSpace` resolves the controller pose the navigation and measurement tools read, so the controller drives the shell through the OpenXR device abstraction; passthrough creates through `CreatePassthroughFB` (the `IsRunningAtCreationBitFB` flag auto-starting the feed) -> `CreatePassthroughLayerFB` (`ReconstructionFB` for full-screen passthrough) -> the per-frame `CompositionLayerPassthroughFB` chained into the `EndFrame` layer array beneath the projection layer so the rendered BIM scene composites over the camera feed; the `EnvironmentBlendMode` selects opaque VR, additive AR, or `XR_FB_passthrough` mixed-reality compositing, folding to opaque when the runtime lacks the extension; `PassthroughLayerSetStyleFB` carries the edge-color and texture-opacity so an on-site review tints or fades the real-world feed as a per-frame style fold.
- Packages: Silk.NET.OpenXR, Silk.NET.OpenXR.Extensions.FB, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new controller action is one `XrAction` bound to its interaction-profile path; a new passthrough style is one `PassthroughStyle` value; a new comfort lever is one `XrComfort` column; zero new surface.
- Boundary: input rides the action-set model so a raw HID controller read bypassing the action-set is the rejected form (`.api/api-silk-openxr.md` reject — OpenXR owns the device abstraction), and the controller pose resolves through `GetActionStatePose`+`LocateSpace`; the action verbs map onto the `CommandIntent` vocabulary so a controller button raises an intent exactly as the input fabric folds (`Shell/input#INPUT_FABRIC`), never a controller-local command; passthrough is created against the one session the core owns (`.api/api-silk-openxr-fb.md` reject — a second OpenXR session or instance for passthrough is rejected), the FB layer chained into the same `EndFrame` layer array; a passthrough toggle rides `PassthroughLayerPauseFB`/`PassthroughLayerResumeFB` on the live layer so the feed flips without feature teardown and a per-toggle feature re-create is the deleted form; the env-blend folds to the opaque flat composite when the runtime lacks `XR_FB_passthrough` so the page ships without a passthrough-capable runtime; the passthrough handles record as `PassthroughHandle`/`PassthroughLayerHandle` rows on the session `XrHandleLedger` and release through `DestroyPassthroughFB`/`DestroyPassthroughLayerFB` in its reverse-order fold; the style update is a per-frame fold, never a re-created layer; `XrComfort` is the XR arm of the ONE quality authority — `EnumerateDisplayRefreshRatesFB`/`GetDisplayRefreshRateFB`/`RequestDisplayRefreshRateFB` negotiate the display rate and `CreateFoveationProfileFB`/`DestroyFoveationProfileFB` swap the foveation profile, both stepped by the same `Diagnostics/governor.md` `QualityTier` rank that steps the resolve ladder and residency watermark, so a second XR-local quality knob path is the rejected form.

```csharp signature
public readonly record struct XrAction(string Name, string ProfilePath, ActionType Type);

public sealed record XrInput(ActionSet ActionSet, Seq<(XrAction Action, Action Handle)> Bound, Space ActionSpace) {
    public IO<Unit> Sync(ImmersiveSession session) => IO.lift(() => SyncActions(session, this));
}

public readonly record struct PassthroughStyle(float EdgeR, float EdgeG, float EdgeB, float TextureOpacity) {
    public static readonly PassthroughStyle Clear = new(0f, 0f, 0f, 1f);
}

// Passthrough handles record on the ONE session ledger (PassthroughHandle/PassthroughLayerHandle rows), so
// release rides the session's reverse-order fold and no second lifetime owner exists.
public sealed record Passthrough(PassthroughFB Feature, PassthroughLayerFB Layer, PassthroughStyle Style, EnvironmentBlendMode BlendMode) {
    public Passthrough Restyle(PassthroughStyle style) => this with { Style = style };

    // Per-layer toggle without feature teardown: PassthroughLayerPauseFB/ResumeFB flip the camera feed
    // while the feature, layer, and style survive.
    public IO<Unit> Pause() => IO.lift(() => PauseLayer(Layer));

    public IO<Unit> Resume() => IO.lift(() => ResumeLayer(Layer));
}

// XR-native quality levers on the one governor authority: refresh-rate negotiation through the
// FBDisplayRefreshRate triple, foveation through CreateFoveationProfileFB/DestroyFoveationProfileFB —
// stepped by the same hysteresis-banded tier rank that steps the resolve ladder.
public sealed record XrComfort(Seq<float> AdvertisedRates, float ActiveRate, Option<FoveationProfileFB> Foveation) {
    public IO<XrComfort> Step(ImmersiveSession session, int tierRank) =>
        from rate in RequestRate(session, RateFor(tierRank))
        from profile in ApplyFoveation(session, tierRank)
        select this with { ActiveRate = rate, Foveation = profile };

    // The rate ladder DERIVES from the advertised set — sorted descending, indexed by the distance below
    // the top tier — so five tiers walk the runtime's real rates and comfort follows the governor, never a
    // two-arm max/min knob or a second XR-local quality path.
    private float RateFor(int tierRank) =>
        AdvertisedRates.IsEmpty
            ? ActiveRate
            : AdvertisedRates.OrderByDescending(identity).ToSeq()[Math.Clamp(4 - tierRank, 0, AdvertisedRates.Count - 1)];
}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Immersive and flat rendering flow
    accDescr: Runtime availability selects an OpenXR session or the flat render graph while preserving one frame receipt family.
    XrRuntime --> ImmersiveMode
    WgpuDevice --> ImmersiveMode
    ImmersiveMode -->|Immersive| ImmersiveSession
    ImmersiveMode -->|Flat| RenderGraph
    ImmersiveSession --> XrFrame
    XrFrame -->|per eye| RenderGraph
    XrFrame -->|EndFrame| ProjectionLayer
    ImmersiveSession --> XrInput
    XrInput --> CommandIntent
    ImmersiveSession --> Passthrough
    Passthrough --> ProjectionLayer
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class ImmersiveMode,ImmersiveSession,XrFrame,Passthrough primary
    class XrRuntime,WgpuDevice,XrInput data
    class RenderGraph,ProjectionLayer,CommandIntent boundary
```

## [05]-[XR_BOUNDARY]

- [XR_SESSION_GRAPHICS]: `XrRuntime.Ready` carries the advertised view configurations, blend modes, refresh rates, and extension set consumed by `ImmersiveMode.Bind`. The bound session owns `CreateSession`, swapchain enumeration/acquire/wait, `LocateViews`, and `EndFrame` behind one `WgpuDevice`; `XrHandleLedger` releases every acquired handle in reverse order and accumulates every failed `Result`.
- [FB_PASSTHROUGH]: the passthrough arm admits only when `XR_FB_passthrough` is advertised, then owns `CreatePassthroughFB`, `CreatePassthroughLayerFB`, `PassthroughStartFB`, `PassthroughLayerSetStyleFB`, and `CompositionLayerPassthroughFB` submission as one `Passthrough` case. An unavailable extension folds to the opaque projection path and cannot create a partial handle graph.

## [06]-[RESEARCH]

(none)
