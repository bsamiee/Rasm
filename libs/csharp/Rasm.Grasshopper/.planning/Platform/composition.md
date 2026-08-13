# [RASM_GRASSHOPPER_PLATFORM_COMPOSITION]

`Compose` owns the macOS composition boundary — CoreAnimation graph custody, transaction-fenced mutation, display-link motion, host animation attachment, and AppKit/CoreImage effects. Kernel owners supply monotonic beat evidence, easing, cycle, spring, perceptual mixing, and profile-aware colour projection; this boundary only binds those values to native resources. `MacGate` admits the platform, `EtoDispatch` owns UI affinity, and every retained native object crosses through `Lease<T>` with one exact inverse.

## [01]-[INDEX]

- [02]-[GRAPH]: `LayerNode` + `LayerMount` + `LayerPaint` + `Compose` — explicit native custody, the one style mint, indexed graph materialization, transaction mutation, and leased teardown.
- [03]-[MOTION]: `PaceWindow` + `DriveSpec` + `MotionDrive` + `MotionAttachment` — shared kernel drive sampling, display-link pacing over `MonotonicTimeline`, transaction application, and completion.
- [04]-[GLIDES]: `GlidePlan` + `TimingCurve` + `Glides` + `Curves` — host-owned animation attachment and the standard timing-function vocabulary.
- [05]-[EFFECTS]: `FilterKind` + `HapticCue` + `VibrancyPane` + `Effects` — CoreImage filter minting, haptic feedback, and visual-effect views.
- [06]-[WIDE_COLOR]: `WideColor` — profile-aware kernel projection crossing into AppKit and CoreAnimation, and the appearance-live inverse.
- [07]-[TELEMETRY_ROOT]: `PlatformIdentity` + `PlatformTelemetry.Resolve` — the plugin-side identity mint (typed `HookScope` discriminator, ALC, version, content root) the `apps/grasshopper/<Plugin>/` composition root binds when it opens the per-ALC AppHost telemetry capsule.

## [02]-[GRAPH]

- Owner: `LayerNode` `[Union]` is the recursive layer graph. `PlainCase` and `ShapeCase` mint boundary-owned layers; `HostedCase(Lease<CALayer>.Owned, ...)` consumes a detached caller-configured layer under sole custody. `LayerStyle` and `StrokeStyle` carry every disposable native payload as `Lease<T>`; an `Owned` row transfers its sole disposal obligation into the resulting mount, a `Borrowed` row remains caller-held for the mount lifetime, and reference-identity deduplication closes repeated occurrences exactly once.
- Owner: `LayerMount` retains the active `Root`, prior `NSView` backing state, mounted `Top`, preorder `Lookup`, derived `Count`, every graph attachment edge, and every transferred native resource. `Find(int ordinal, Op?)` resolves a mounted node without leaking a second index. `Dispose` marshals one inverse through `EtoDispatch`, removes every edge in reverse order inside one disabled-action transaction, restores `Layer` and `WantsLayer`, and then releases owned resources in reverse acquisition order; an existing borrowed root remains untouched while a root minted by this mount dies after restoration.
- Entry: `Compose.Mount(MacAnchor anchor, LayerNode node, Op? key = null)` → `Fin<Lease<LayerMount>>`. One recursive admission fold rejects malformed styles, dead lease payloads, and duplicate hosted or mask identities; the mount scope rejects any graph payload identical to the anchor root before custody can double. Its anchor view sets layer backing, reuses its live `Layer`, or installs `MakeBackingLayer()` as an owned root. Materialization remains detached until the complete preorder graph and lookup exist; attachment is the final mutation. Native custody transfers only as each graph value materializes. Any failure reverses edges and view backing, releases every acquired native, and accumulates inverse faults with the originating refusal.
- Entry: `Compose.Mutate(Action body, bool animated, Op? key = null)` → `Fin<Unit>` is the public mutation fence. `CATransaction.Commit` always closes the begun transaction, and disabling actions is the default posture for sampled motion and teardown.
- Law: every scope refusal rides the rail with its own cause — a graph payload aliasing the anchor root is `InvalidInput`, a host backing-layer mint answering null is `InvalidResult` naming the member, a transfer with no captured backing is `MissingContext`, and `[03]`'s second bind against a live callback target is `InvalidContext`. A raw `throw` inside the enclosing `Op.Catch` converts, but flattens all four to one untyped result token exactly where the custody argument needs the cause addressable, so it is the deleted form; the native writes stay downstream of admission, which is what keeps a refused lease from moving any layer state before it refuses.
- Owner: `LayerPaint` is the one style mint — `Plain` builds a `LayerStyle` and `Stroked` a `StrokeStyle`, every colour crossing `[06]`'s `WideColor.ToLayer` and every path crossing `Eto.Mac.CGConversions.ToCG(this IGraphicsPath)` into an owned `Lease<CGPath>`. Its ingress is admitted domain material — kernel `PerceptualColor` and an already-built Eto `IGraphicsPath` the caller's own geometry owner supplies — so no canvas type crosses down into this stratum and no style field enters a graph unminted. Minting is prefix-safe: a refused colour or path releases every lease already taken in the same call and appends the release fault to the refusal.
- Boundary: graph ordinals are stable only for one mount lease. Every layer `Lookup` or `Find` hands back is borrowed and remains live only while that lease remains live. `Mount` retains only the leases `LayerPaint` produced and converts nothing itself. `Reframe(int, CGRect, Op?)` is the mount's own resize path — anchor bounds snapshot at `MacAnchor.Of`, so a resized canvas re-frames mounted ordinals through the fenced mutation, never a raw `Frame` write off a borrowed handle.
- Law: compositing is live-proven host behavior — the canvas view's backing `CALayer` is live even where `WantsLayer` reads false (the layer-backed window hierarchy backs every view), a mounted sublayer survives the host's own `Drawable` paint, and a compositor-run animation advances its presentation layer with ZERO canvas paint events across the whole run, so mounted decoration costs no paint pass by measurement, not assertion. The canvas backing layer's `ContentsFormat` is 8-bit RGBA, so wide colour rides the mounted overlay layer's own contents and colour values, never the host layer's format.
- Packages: Microsoft.macOS (`NSView`, `CALayer`, `CAShapeLayer`, `CATransaction`, `CGPath`, `CGColor`, `CGRect`), Eto.Drawing (`IGraphicsPath`), Eto.macOS (`CGConversions.ToCG`), `Rasm.Numerics` (`PerceptualColor`), `Rasm.Domain` (`Op`, `Lease<T>`), `Platform/native.md` (`MacGate`, `MacAnchor`), `Eto/runtime.md` (`EtoDispatch`).
- Growth: a new layer family is one `LayerNode` case whose native payload enters through the same scope; graph lookup, transaction fencing, failure cleanup, and teardown do not widen.

## [03]-[MOTION]

- Owner: `DriveSpec` `[Union]` carries the three kernel-sampled drive algebras: eased cycles, fixed-target springs, and perceptual colour blends. `MotionDrive.Step(DriveSpec, MonotonicBeat, AccessibilityPosture, Op?)` is the shared sampling fold consumed unchanged by this display-link attachment and `Canvas/motion.md`'s `UiClock` pacer; it returns a read-only `DriveFrame` whose non-null `Apply` action and `Continues` verdict are minted only inside the fold. `MotionAttachment` owns the display link, callback target, `MonotonicTimeline` sequence, drive, workspace observation, fault cell, transaction application, and terminal completion as one disposable resource.
- Entry: `MotionAttachment.Attach(MacAnchor anchor, PaceWindow window, DriveSpec drive, Option<Action> completed, Op? key = null)` → `Fin<Lease<MotionAttachment>>`. This mint admits each nested drive value, acquires one `WorkspaceWatch`, captures a kernel timeline origin, creates an inert callback target, pauses and tunes the view display link, binds the completed attachment onto the rail so a second claim against that target refuses before any run-loop edge exists, attaches it to `NSRunLoop.Main` under `NSRunLoopMode.Common`, and only then resumes callbacks. Any mint or dispatch fault removes the run-loop attachment, invalidates and disposes every native, releases the watch, and accumulates cleanup faults.
- Law: each callback advances `MonotonicTimeline.Beat` from the origin or prior `MonotonicBeat`; the attachment stores that successful receipt before posture, sampling, or native application can fail because the kernel sequence has already advanced. No host timestamp arithmetic, parallel beat identity, or wall-clock read survives. `MotionDrive.Step` returns the sampled write and continuation decision, and the attachment applies that frame inside `Compose.Fence(animated: false)`; the canvas pacer applies the same frame before its repaint edge.
- Law: completion belongs to the terminal frame. `SpringSettlement` carries dimensionally distinct positive position and velocity bands; a spring inside both snaps to target at zero velocity. Every bounded cycle preserves its yoyo terminal side, and reduce motion selects that same bounded terminal or the far stop of an unbounded cycle. That frame installs an atomic once-gated caller continuation on `CATransaction.CompletionBlock`, pauses the link, and suppresses that continuation after lease teardown begins. Any beat, sampling, write, or deferred-completion fault records in `LastFault` and pauses the link.
- Law: `PerceptualColor.Mix(other, amount, path)` is the sole colour interpolation call. It returns `PerceptualColor` directly; `BlendPath` supplies the interpolation space with whatever traversal that space admits, publishes no interpolation entry to this stratum, and carries no fallible rail.
- Law: one `WorkspaceFact` cell supplies accessibility and pace as a coherent snapshot. Each callback retunes the frame-rate range only when that snapshot's `PaceBounds` changes, then samples against its paired posture; a display migration never combines a stale ceiling with a new accessibility state.
- Law: disposal marshals through `EtoDispatch`, unbinds the callback before removing the link from its run loop, attempts pause, removal, invalidation, and both native disposals independently, then releases the workspace watch even when UI dispatch refuses. Every inverse fault accumulates, and the link never outlives its anchor view, run-loop attachment, callback object, or workspace observation.
- Packages: Microsoft.macOS (`CADisplayLink`, `CAFrameRateRange`, `NSRunLoop`, `NSRunLoopMode`, `NSObject`, `Selector`, `ExportAttribute`), `Rasm.Parametric` (`MonotonicTimeline`, `MonotonicStamp`, `MonotonicBeat`, `BeatSeed`, `Easing`, `CyclePlan`, `SpringShape`, `SpringState`), `Rasm.Numerics` (`PerceptualColor`, `BlendPath`, `UnitInterval`), `Rasm.Domain` (`Op`, `Lease<T>`), `Platform/native.md` (`AccessibilityAxis`, `AccessibilityPosture`, `PaceBounds`, `WorkspaceFact`, `WorkspaceWatch`, `NativeSeam.Watch`).
- Growth: a new sampled drive is one `DriveSpec` case in `MotionDrive.Step`; both pacers inherit the same beat, posture, terminal, and write semantics without gaining a parallel sampling arm.

## [04]-[GLIDES]

- Owner: `GlidePlan` `[Union]` — `TimedCase` pairs an explicitly owned or borrowed `CAAnimation` with the managed `string` key `CALayer.AddAnimation` requires, and `SprungCase` carries the kernel `SpringShape` with its key path and endpoints, projected onto `CASpringAnimation` at the attach (unit mass, `k = ω²`, `c = 2ζω`, duration from the kernel `Settle` projection). `Glides.Animate` consumes a timed plan's lease inside one transaction and REFUSES a hand-authored `CASpringAnimation` there — the spring door is `SprungCase`, so locally-authored spring constants cannot fork motion feel past the kernel mint. CoreAnimation copies the attached animation, so an owned plan releases immediately after the call while a borrowed plan remains caller-held. `Glides.Halt` admits the same managed key for `CALayer.RemoveAnimation`; `NSString` remains confined to `CAMediaTimingFunction.FromName`, whose catalog member requires it.
- Owner: `TimingCurve` `[SmartEnum<int>]` closes the standard CoreAnimation names: `Default`, `EaseIn`, `EaseOut`, `EaseInEaseOut`, and `Linear`. `Curves.Named(TimingCurve, Op?)` mints an owned `CAMediaTimingFunction`; no raw timing-name string crosses the public surface.
- Entry: `Glides.Animate(CALayer layer, GlidePlan plan, Op? key = null)` and `Glides.Halt(CALayer layer, string glideKey, Op? key = null)` → `Fin<Unit>`; `Curves.Named` → `Fin<Lease<CAMediaTimingFunction>>`.
- Law: sampled drives and host glides remain distinct by state ownership. Each sampled drive exposes kernel state and retained completion through `MotionAttachment`; a glide delegates interpolation to CoreAnimation and owns only attachment and removal. Deferred completion requires a retained callback owner and therefore stays on `MotionAttachment`.
- Packages: Microsoft.macOS (`CAAnimation`, `CAMediaTimingFunction`, `CALayer`, `NSString`), `Rasm.Domain` (`Op`, `Lease<T>`), `Platform/native.md` (`MacGate`).
- Growth: a new standard timing name is one `TimingCurve` row; a new host animation is one `GlidePlan` case on the one attachment lifecycle, and a new physical modality composes its kernel shape exactly as `SprungCase` does.

## [05]-[EFFECTS]

- Owner: `FilterKind` `[SmartEnum<string>]` carries the admitted CoreImage registry keys. `Effects.Filter` disposes the registry template after copying and returns the copy as `Lease<CIFilter>.Owned`; the caller holds that lease across any `CALayer.Filters` or `BackgroundFilters` attachment window.
- Owner: `HapticCue` pairs `NSHapticFeedbackPattern` with `NSHapticFeedbackPerformanceTime`. `Effects.Pulse` performs the selected cue through `NSHapticFeedbackManager.DefaultPerformer` on the UI thread and retains no native object.
- Owner: `VibrancyPane` carries material and blending policy. `Effects.Vibrancy` returns `Lease<NSVisualEffectView>.Owned`; embedding code retains the lease for the hosted-view lifetime and disposes it after detachment.
- Entry: `Effects.Filter(FilterKind, Op?)` → `Fin<Lease<CIFilter>>`; `Effects.Pulse(HapticCue, Op?)` → `Fin<Unit>`; `Effects.Vibrancy(VibrancyPane, Op?)` → `Fin<Lease<NSVisualEffectView>>`.
- Packages: Microsoft.macOS (`CIFilter`, `NSHapticFeedbackManager`, `NSHapticFeedbackPattern`, `NSHapticFeedbackPerformanceTime`, `NSVisualEffectView`, `NSVisualEffectMaterial`, `NSVisualEffectBlendingMode`), `Rasm.Domain` (`Op`, `Lease<T>`), `Platform/native.md` (`MacGate`).
- Growth: a new filter is one `FilterKind` row, a new haptic is one `HapticCue` value, and a new vibrancy posture is data on `VibrancyPane`.

## [06]-[WIDE_COLOR]

- Owner: `WideColor.Project(PerceptualColor colour, GamutPolicy? gamut = null, Op? key = null)` → `Fin<Lease<NSColor>>` composes `PerceptualColor.ToRgb(profile: RgbProfile.DisplayP3, gamut:)` and passes the returned unit channels directly to `NSColor.FromDisplayP3`. This host boundary neither constructs `Unicolour` nor reads its default `.Rgb` accessor, because that accessor is sRGB and cannot be relabelled as Display-P3.
- Owner: `WideColor.ToLayer(PerceptualColor colour, Op? key = null)` → `Fin<Lease<CGColor>>` is the layer-graph crossing `[02]`'s `LayerPaint` calls for every one of the four `Option<Lease<CGColor>>` style fields: it mints the `CGColor` DIRECTLY on the `CGColorSpaceNames.DisplayP3` space from the same admitted unit channels `Project` passes to AppKit, so the two faces of one projection share an admission gate and neither derives its custody from the other.
- Owner: `WideColor.OfSystem(NSColor native, Op? key = null)` → `Fin<PerceptualColor>` is the INBOUND arm — `MacConversions.ToEtoWithAppearance` resolves a dynamic or catalog `NSColor` against the CURRENT appearance and the resulting sRGB channels enter `PerceptualColor.OfRgb`, so a chrome swatch read from `SystemColors` or an `NSAppearance`-dynamic colour crosses live rather than archived; the three arms make the crossing bidirectional on one owner.
- Law: `RgbProfile` carries the kernel's working-space roster and the corpus' one `Configuration` mint, `DisplayP3` the row this boundary names. Its two `ToRgb` overloads split by RESULT, never by policy — `ToRgb(RgbProfile profile, GamutPolicy? gamut = null, RgbTransfer? transfer = null)` returns the profile's `(double Red, double Green, double Blue, double Alpha)` under whichever transfer the trailing row names, and `ToRgb(GamutPolicy? gamut = null)` returns the sRGB byte quadruple — with the reproducibility domain an argument on both, defaulting to the kernel's own perceptual row. This page passes no transfer, taking the `RgbTransfer.Encoded` default: `NSColor.FromDisplayP3` and the `CGColorSpaceNames.DisplayP3` mint both consume COMPANDED Display-P3 components, so a `RgbTransfer.Linear` read here would hand scene-linear light to an encoded-channel constructor and darken every swatch. Profile conversion and gamut bounding stay `PerceptualColor`'s, never a host-local colour pipeline.
- Law: the `NSColor` round trip is REFUSED as the layer crossing — `CGConversions.ToCG(this NSColor)` returns the source-space colour on its primary arm but re-spaces to sRGB and then floors at opaque black without signalling, and its result borrows the receiver's lifetime, so routing the layer face through it both clamps the gamut silently and entangles two custodies on one lease; the direct `CGColorSpaceNames.DisplayP3` mint has one lifetime and no clamp arm.
- Boundary: `NSColor` and its projected `CGColor` are the only native colour objects minted here, each returned as owned custody. All profile selection, chromatic adaptation, transfer encoding, and gamut mapping remain kernel operations.
- Packages: Microsoft.macOS (`NSColor`, `CGColor`, `CGColorSpace`, `CGColorSpaceNames`), Eto.macOS (`MacConversions.ToEtoWithAppearance`), `Rasm.Numerics` (`PerceptualColor`, `RgbProfile`, `GamutPolicy`), `Rasm.Domain` (`Op`, `Lease<T>`), `Platform/native.md` (`MacGate`).
- Growth: a new display profile is one kernel `RgbProfile` row and a new reproducibility domain one kernel `GamutPolicy` row; the AppKit and CoreAnimation projections remain unchanged while the selected rows vary.

## [07]-[TELEMETRY_ROOT]

- Owner: `PlatformIdentity` — the plugin-side identity record the telemetry capsule binds: typed `HookScope` plugin discriminator, plugin `AssemblyLoadContext`, assembly version, and resolved content root; `PlatformTelemetry.Resolve` is its sole mint.
- Entry: `PlatformTelemetry.Resolve(Assembly pluginRoot, HookScope plugin, Op? key = null)` → `Fin<PlatformIdentity>`.
- Law: the app root alone references `Rasm.AppHost` beside `Rasm.Grasshopper` — no package source names an AppHost or OpenTelemetry type, and this section's fence compiles against `Rasm` and this folder's `HookScope` alone.
- Law: the plugin discriminator admits through the typed `Shell/hooks.md` `HookScope` — the same key space the hook registry and the `gh.plugin` meter tag share — so the telemetry resource attribute and every other per-plugin surface spell one identity by construction, and the raw-string parameter this seam once carried is the deleted fork.
- Law: content root resolves at the mint because it is plugin knowledge — plugins load from their own install directory, and a host reporting no location for a collectible or single-file assembly falls to the process base, the one path that then resolves.
- Boundary: the AppHost lacing is composition-root work, homed whole at the `apps/grasshopper/<Plugin>/` shell per the branch composition-root ruling — over one resolved `PlatformIdentity` the root gates `ProfileSurface.Resolve` on the `HostRows.Gh2` row (`Tenancy.None`, `DeploymentTopology.InHost`, `LifecycleOwner.CallerOwned`, `Isolation.InProc`, no providers — Rhino owns the canvas process and the plugin binds no provider port, so the row samples whole and projects its logs locally) under `TelemetryDomain.Grasshopper.Key`, `Environments.Production`, and the identity's content root and version, then opens `PluginTelemetryHost.Open` on the identity's `Alc` with the one self-minting `TelemetryContributorPort` `Shell/telemetry.md`'s port boundary spells — `TelemetrySource.Grasshopper.Key` scope, empty `Instruments` (handles live on the meter `GhInstruments` mints through the capsule factory, so the root binds nothing), `GhInstruments.Rows` published, `GhInstruments.Board` on the pack column — and the plugin discriminator read off the identity as `TelemetryDomain.Host.Measure("plugin")`; `Resolve` gates the axis values BEFORE the capsule opens, so an unservable row refuses while no provider exists to dispose.
- Law: capsule cardinality is one per plugin `AssemblyLoadContext`, opened once at plugin load, never per canvas or component; a second plugin is a second identity mint and a second open with its own discriminator.
- Law: `ProfileIdentity.ResourceAttributes` owns resource identity; this package supplies the identity record and its discriminator alone.
- Law: `TelemetryDomain.Qualify` renders `service.name` off the `TelemetryDomain.Grasshopper` row, never a literal; the plugin discriminator spells `TelemetryDomain.Host.Measure("plugin")`, and `SignalGovernance.Rostered` refuses a bare `rasm.plugin`.
- Law: the port carries a scope coordinate and no `InstrumentSpec` row, so `Views` reads GH streams on its foreign arm.
- Law: `Environments.Production` floors the environment row; `OTEL_RESOURCE_ATTRIBUTES` detection outranks it at deploy.
- Boundary: lifetime is the capsule's own `AssemblyLoadContext.Unloading` hook — `ForceFlush` then `Dispose` per the AppHost provider-lifetime law.
- Boundary: `MacGate` admission stays unneeded, because the identity mint touches no AppKit surface.
- Packages: `Rasm` and BCL inbox alone — `Rasm.AppHost`, `Microsoft.Extensions.Hosting`, and `NodaTime` are `apps/grasshopper/<Plugin>/` references, never this package's; `Shell/hooks.md` (`HookScope`) is the one composed folder owner.
- Growth: a new plugin-side resource dimension is one `PlatformIdentity` column; a new machine dimension is one detector row inside `ResourceIdentity.Compose` at the root.
- Growth: a first GH-declared `InstrumentSpec` row is one entry on the port's `Instruments` seq at the root, which then takes the rostered view arm.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// [BOUNDARY]: the AppHost lacing over this record — the ProfileSurface.Resolve gate on the HostRows.Gh2
// row, PluginTelemetryHost.Open with the self-minting GhInstruments contributor port, and the
// TelemetryDomain discriminator spelling — is the apps/grasshopper/<Plugin>/ composition root's alone:
// the one assembly referencing Rasm.AppHost beside Rasm.Grasshopper.
public sealed record PlatformIdentity(
    HookScope Plugin,
    Version Version,
    string ContentRoot,
    AssemblyLoadContext Alc);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PlatformTelemetry {
    public static Fin<PlatformIdentity> Resolve(Assembly pluginRoot, HookScope plugin, Op? key = null) {
        ArgumentNullException.ThrowIfNull(pluginRoot);
        Op op = key.OrDefault();
        // HookScope IS the admission — a default-constructed struct throws on its key read and lands the
        // rail here, so the record carries only a scope the factory has proved.
        return from _scope in op.Catch(body: () => Fin.Succ((string)plugin))
               from alc in Optional(AssemblyLoadContext.GetLoadContext(pluginRoot)).ToFin(Fail: op.MissingContext())
               from version in Optional(pluginRoot.GetName().Version).ToFin(Fail: op.MissingContext())
               select new PlatformIdentity(
                   Plugin: plugin,
                   Version: version,
                   ContentRoot: ContentRoot(pluginRoot),
                   Alc: alc);
    }

    // Plugins load from their own install directory; a host reporting no location for a collectible or
    // single-file assembly falls to the process base, the one path that then resolves.
    private static string ContentRoot(Assembly root) =>
        Path.GetDirectoryName(root.Location) is { Length: > 0 } held ? held : AppContext.BaseDirectory;
}
```

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
using Eto.Mac;
using Foundation;
using ObjCRuntime;
using Rasm.Domain;
using Rasm.Grasshopper.Eto;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Grasshopper.Platform;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record LayerNode {
    private LayerNode() { }
    public sealed record PlainCase(LayerStyle Style, Seq<LayerNode> Children) : LayerNode;
    public sealed record ShapeCase(LayerStyle Style, StrokeStyle Stroke, Seq<LayerNode> Children) : LayerNode;
    public sealed record HostedCase(Lease<CALayer>.Owned Layer, Seq<LayerNode> Children) : LayerNode;
}

[Union]
public abstract partial record DriveSpec {
    private DriveSpec() { }
    public sealed record EasedCase(Easing Curve, TimeSpan Period, CyclePlan Cycle, Action<double> Write) : DriveSpec;
    public sealed record SprungCase(
        SpringShape Shape, SpringState From, double Target, SpringSettlement Settlement, Action<SpringState> Write) : DriveSpec;
    public sealed record BlendCase(
        BlendPath Path, PerceptualColor From, PerceptualColor To, Easing Curve, TimeSpan Period, CyclePlan Cycle,
        Action<PerceptualColor> Write) : DriveSpec;
    // Inertial release rides the kernel DecayShape — a fling or pan release projects position under decaying
    // velocity from the one owner; a host-local velocity-decay expression is the deleted form the branch
    // spring-parity ruling names.
    public sealed record DecayCase(
        DecayShape Shape, double Origin, double Velocity, double Epsilon, Action<double> Write) : DriveSpec;
}

[SmartEnum<string>]
public sealed partial class FilterKind {
    public static readonly FilterKind ColorControls = new(key: "CIColorControls");
    public static readonly FilterKind ColorMatrix = new(key: "CIColorMatrix");
    public static readonly FilterKind ExposureAdjust = new(key: "CIExposureAdjust");
    public static readonly FilterKind GaussianBlur = new(key: "CIGaussianBlur");
    public static readonly FilterKind Bloom = new(key: "CIBloom");
}

[SmartEnum<int>]
public sealed partial class TimingCurve {
    public static readonly TimingCurve Default = new(key: 0, name: CAMediaTimingFunction.Default);
    public static readonly TimingCurve EaseIn = new(key: 1, name: CAMediaTimingFunction.EaseIn);
    public static readonly TimingCurve EaseOut = new(key: 2, name: CAMediaTimingFunction.EaseOut);
    public static readonly TimingCurve EaseInEaseOut = new(key: 3, name: CAMediaTimingFunction.EaseInEaseOut);
    public static readonly TimingCurve Linear = new(key: 4, name: CAMediaTimingFunction.Linear);
    internal NSString Name { get; }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct HapticCue(
    NSHapticFeedbackPattern Pattern, NSHapticFeedbackPerformanceTime Timing) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: Enum.IsDefined(value: Pattern)),
        ValidityClaim.Of(holds: Enum.IsDefined(value: Timing)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record LayerStyle(
    CGRect Frame, Option<Lease<CGColor>> Fill, Option<Lease<CGColor>> Border, NFloat BorderWidth, NFloat CornerRadius,
    bool Clip, Option<Lease<CALayer>> Mask);

public sealed record StrokeStyle(
    Lease<CGPath> Path, Option<Lease<CGColor>> Fill, Option<Lease<CGColor>> Stroke, NFloat Width, bool Rounded);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PaceWindow(float Minimum, float Maximum, float Preferred) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: Minimum),
        ValidityClaim.Ordered(lower: Minimum, upper: Maximum),
        ValidityClaim.Ordered(lower: Minimum, upper: Preferred),
        ValidityClaim.Ordered(lower: Preferred, upper: Maximum));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpringSettlement(double Position, double Velocity) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: Position),
        ValidityClaim.Positive(value: Velocity));
}

// Spring motion has ONE source — the kernel SpringShape mint — so a compositor-run spring is the SprungCase
// projection of that shape onto CASpringAnimation, and TimedCase refuses a hand-authored CASpringAnimation at
// admission: locally-authored mass/stiffness/damping constants are the silent motion-feel fork the branch
// spring-parity ruling forecloses.
[Union]
public abstract partial record GlidePlan {
    private GlidePlan() { }
    public sealed record TimedCase(Lease<CAAnimation> Animation, string Key) : GlidePlan;
    public sealed record SprungCase(SpringShape Shape, string KeyPath, double From, double To, string Key) : GlidePlan;
}

public sealed record VibrancyPane(
    NSVisualEffectMaterial Material, NSVisualEffectBlendingMode Blending) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: Enum.IsDefined(value: Material)),
        ValidityClaim.Of(holds: Enum.IsDefined(value: Blending)));
}

public sealed record DriveFrame {
    internal DriveFrame(bool Continues, Action Apply) { this.Continues = Continues; this.Apply = Apply; }
    public bool Continues { get; }
    public Action Apply { get; }
}

internal sealed record WorkspaceObservation(
    Atom<Option<WorkspaceFact>> Latest, Lease<WorkspaceWatch> Watch, WorkspaceFact Initial);

internal readonly record struct LayerEdge(CALayer Parent, CALayer Child);

internal readonly record struct ViewBacking(NSView View, bool WantsLayer, CALayer? Layer);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class LayerMount : IDisposable {
    private readonly ViewBacking backing;
    private readonly LayerEdge[] edges;
    private readonly IDisposable[] owned;
    private readonly Atom<Option<Error>> lastFault = Atom(Option<Error>.None);
    private int releaseState;

    internal LayerMount(
        CALayer root, CALayer top, IReadOnlyDictionary<int, CALayer> lookup,
        ViewBacking backing, LayerEdge[] edges, IDisposable[] owned) {
        Root = root;
        Top = top;
        Lookup = lookup;
        this.backing = backing;
        this.edges = edges;
        this.owned = owned;
    }

    public CALayer Root { get; }
    public CALayer Top { get; }
    public IReadOnlyDictionary<int, CALayer> Lookup { get; }
    public int Count => Lookup.Count;
    public Option<Error> LastFault => lastFault.Value;

    public Fin<CALayer> Find(int ordinal, Op? key = null) {
        Op op = key.OrDefault();
        return Lookup.TryGetValue(key: ordinal, value: out CALayer? layer) && layer is not null
            ? Fin.Succ(layer)
            : Fin.Fail<CALayer>(op.InvalidInput());
    }

    // The mount's own re-frame path: anchor bounds are a snapshot, so a resized canvas re-frames mounted
    // ordinals through the same fenced mutation every other layer write rides — a caller-owned raw Frame
    // write off a Find handle is the deleted form this member exists to absorb.
    public Fin<Unit> Reframe(int ordinal, CGRect frame, Op? key = null) {
        Op op = key.OrDefault();
        return from layer in Find(ordinal: ordinal, key: op)
               from settled in Compose.Mutate(body: () => layer.Frame = frame, animated: false, key: op)
               select settled;
    }

    public void Dispose() => ignore(Release(key: Op.Of(name: nameof(Dispose))));

    private Fin<Unit> Release(Op key) {
        if (Interlocked.CompareExchange(location1: ref releaseState, value: 1, comparand: 0) != 0) return Fin.Succ(unit);
        Fin<Unit> outcome = EtoDispatch.Run(body: () => {
            Fin<Unit> detached = NativeScope.Detach(edges: edges, key: key);
            Fin<Unit> restored = NativeScope.Restore(backing: backing, key: key);
            Fin<Unit> released = NativeScope.Release(resources: owned, key: key);
            return NativeScope.Join(
                left: detached,
                right: NativeScope.Join(left: restored, right: released));
        }, key: key);
        outcome.IfFail(error => ignore(lastFault.Swap(_ => Some(error))));
        Volatile.Write(location: ref releaseState, value: 2);
        return outcome;
    }
}

public sealed class MotionAttachment : IDisposable {
    private readonly CADisplayLink link;
    private readonly LinkTarget target;
    private readonly NSRunLoop runLoop;
    private readonly NSRunLoopMode runLoopMode;
    private readonly MonotonicTimeline timeline;
    private readonly MonotonicStamp origin;
    private readonly DriveSpec drive;
    private readonly PaceWindow window;
    private readonly Atom<Option<WorkspaceFact>> workspace;
    private readonly Lease<WorkspaceWatch> workspaceWatch;
    private readonly Option<Action> completed;
    private readonly Atom<Option<Error>> lastFault = Atom(Option<Error>.None);
    private PaceBounds appliedPace;
    private Option<MonotonicBeat> previous = Option<MonotonicBeat>.None;
    private int completionState;
    private int releaseState;

    private MotionAttachment(
        CADisplayLink link, LinkTarget target, NSRunLoop runLoop, NSRunLoopMode runLoopMode,
        MonotonicTimeline timeline, MonotonicStamp origin, DriveSpec drive, PaceWindow window,
        Atom<Option<WorkspaceFact>> workspace, Lease<WorkspaceWatch> workspaceWatch,
        WorkspaceFact initial, Option<Action> completed) {
        this.link = link;
        this.target = target;
        this.runLoop = runLoop;
        this.runLoopMode = runLoopMode;
        this.timeline = timeline;
        this.origin = origin;
        this.drive = drive;
        this.window = window;
        this.workspace = workspace;
        this.workspaceWatch = workspaceWatch;
        appliedPace = initial.Pace;
        this.completed = completed;
    }

    public Option<Error> LastFault => lastFault.Value;

    public static Fin<Lease<MotionAttachment>> Attach(
        MacAnchor anchor, PaceWindow window, DriveSpec drive, Option<Action> completed, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from activeAnchor in op.Need(anchor)
               // NSView.GetDisplayLink carries a macos14.0 availability floor the platform gate cannot see,
               // so the attachment names its own version gate and refuses below it instead of throwing native.
               from _floor in guard(OperatingSystem.IsMacOSVersionAtLeast(major: 14), (Error)op.InvalidContext()).ToFin()
               let view = activeAnchor.View
               from validDrive in MotionDrive.Admit(spec: drive, key: op)
               from admitted in guard(window.IsValid, op.InvalidInput()).ToFin().Map(_ => window)
               from timeline in MonotonicTimeline.Of(provider: TimeProvider.System, key: op)
               from origin in timeline.Capture(key: op)
               from observation in Observe(anchor: activeAnchor, key: op)
               let mounted = EtoDispatch.Run(body: () => {
                   CADisplayLink? native = null;
                   LinkTarget? bridge = null;
                   NSRunLoop? loop = null;
                   NSRunLoopMode mode = NSRunLoopMode.Common;
                   Fin<Lease<MotionAttachment>> outcome = op.Catch(body: () => {
                       bridge = new LinkTarget();
                       native = view.GetDisplayLink(target: bridge, selector: LinkTarget.TickSelector);
                       if (native is not { } link) return Fin.Fail<Lease<MotionAttachment>>(op.InvalidResult());
                       link.Paused = true;
                       WorkspaceFact snapshot = observation.Latest.Value.IfNone(observation.Initial);
                       Tune(link: link, window: admitted, pace: snapshot.Pace);
                       loop = NSRunLoop.Main;
                       if (loop is not { } main) return Fin.Fail<Lease<MotionAttachment>>(op.MissingContext());
                       MotionAttachment attachment = new(
                           link: link, target: bridge, runLoop: main, runLoopMode: mode,
                           timeline: timeline, origin: origin, drive: validDrive, window: admitted,
                           workspace: observation.Latest, workspaceWatch: observation.Watch,
                           initial: snapshot, completed: completed);
                       // The bind refuses a second callback claim on the rail, so the run-loop attachment and
                       // the resume both stay behind a target this attachment provably owns.
                       return bridge.Bind(callback: () => attachment.OnTick(key: op), key: op).Map(_ => {
                           link.AddToRunLoop(runloop: main, mode: mode);
                           link.Paused = false;
                           return (Lease<MotionAttachment>)new Lease<MotionAttachment>.Owned(Value: attachment);
                       });
                   });
                   return outcome.Match(
                       Succ: static value => Fin.Succ(value),
                       Fail: error => NativeScope.Join<Lease<MotionAttachment>>(
                           primary: error,
                           cleanup: Cleanup(link: native, target: bridge, runLoop: loop, runLoopMode: mode, key: op)));
               }, key: op)
               from lease in mounted.Match(
                   Succ: static value => Fin.Succ(value),
                   Fail: error => NativeScope.Join<Lease<MotionAttachment>>(
                       primary: error,
                       cleanup: Dispose(lease: observation.Watch, key: op)))
               select lease;
    }

    public void Dispose() => ignore(Release(key: Op.Of(name: nameof(Dispose))));

    private void OnTick(Op key) => key.Catch(body: () => Tick(key: key)).IfFail(error => Record(error: error));

    private Fin<Unit> Tick(Op key) {
        BeatSeed seed = previous.Match(Some: static prior => (BeatSeed)prior, None: () => (BeatSeed)origin);
        Fin<(MonotonicBeat Beat, bool Continues)> outcome =
            from beat in timeline.Beat(seed: seed, key: key).Map(Advance)
            from fact in workspace.Value.ToFin(key.InvalidResult())
            from _tuned in Retune(pace: fact.Pace, key: key)
            from frame in MotionDrive.Step(spec: drive, beat: beat, posture: fact.Posture, key: key)
            let terminal = frame.Continues
                ? Option<Action>.None
                : completed.Map<Action>(finish => Completion(finish: finish, key: key))
            from applied in Compose.Fence(body: frame.Apply, animated: false, completed: terminal, key: key)
            select (Beat: beat, Continues: frame.Continues);
        return outcome.Match(
            Succ: advanced => advanced.Continues ? Fin.Succ(unit) : Pause(key: key),
            Fail: error => NativeScope.Join<Unit>(primary: error, cleanup: Pause(key: key)));
    }

    private MonotonicBeat Advance(MonotonicBeat beat) { previous = Some(beat); return beat; }
    private void Record(Error error) => ignore(lastFault.Swap(_ => Some(error)));
    private Fin<Unit> Pause(Op key) => key.Catch(body: () => Fin.Succ(Op.Side(action: () => link.Paused = true)));
    private Action Completion(Action finish, Op key) => () => Op.SideWhen(
        condition: Volatile.Read(location: ref releaseState) == 0 &&
            Interlocked.CompareExchange(location1: ref completionState, value: 1, comparand: 0) == 0,
        action: () => key.Catch(body: () => Fin.Succ(Op.Side(action: finish))).IfFail(error => Record(error: error)));

    private Fin<Unit> Retune(PaceBounds pace, Op key) => pace == appliedPace
        ? Fin.Succ(unit)
        : key.Catch(body: () => {
            Tune(link: link, window: window, pace: pace);
            appliedPace = pace;
            return Fin.Succ(unit);
        });

    private Fin<Unit> Release(Op key) {
        if (Interlocked.CompareExchange(location1: ref releaseState, value: 1, comparand: 0) != 0) return Fin.Succ(unit);
        Fin<Unit> native = EtoDispatch.Run(
            body: () => Cleanup(link: link, target: target, runLoop: runLoop, runLoopMode: runLoopMode, key: key),
            key: key);
        Fin<Unit> observed = Dispose(lease: workspaceWatch, key: key);
        Fin<Unit> outcome = NativeScope.Join(left: native, right: observed);
        outcome.IfFail(error => Record(error: error));
        Volatile.Write(location: ref releaseState, value: 2);
        return outcome;
    }

    private static Fin<WorkspaceObservation> Observe(MacAnchor anchor, Op key) {
        Atom<Option<WorkspaceFact>> latest = Atom(Option<WorkspaceFact>.None);
        return NativeSeam.Watch(
                anchor: anchor,
                publish: fact => ignore(latest.Swap(_ => Some(fact))),
                key: key)
            .Bind(watch => latest.Value.ToFin(key.InvalidResult()).Match(
                Succ: initial => Fin.Succ(new WorkspaceObservation(Latest: latest, Watch: watch, Initial: initial)),
                Fail: error => NativeScope.Join<WorkspaceObservation>(primary: error, cleanup: Dispose(lease: watch, key: key))));
    }

    private static Fin<Unit> Cleanup(
        CADisplayLink? link, LinkTarget? target, NSRunLoop? runLoop, NSRunLoopMode runLoopMode, Op key) {
        Fin<Unit> unbound = key.Catch(body: () => Fin.Succ(Op.Side(action: () => target?.Unbind())));
        Fin<Unit> paused = key.Catch(body: () => Fin.Succ(Op.Side(action: () => { if (link is not null) link.Paused = true; })));
        Fin<Unit> removed = key.Catch(body: () => Fin.Succ(Op.Side(action: () => {
            if (link is not null && runLoop is not null) link.RemoveFromRunLoop(runloop: runLoop, mode: runLoopMode);
        })));
        Fin<Unit> invalidated = key.Catch(body: () => Fin.Succ(Op.Side(action: () => link?.Invalidate())));
        IDisposable[] resources = (link, target) switch {
            ({ } native, { } bridge) => [native, bridge],
            ({ } native, null) => [native],
            (null, { } bridge) => [bridge],
            _ => [],
        };
        return NativeScope.Join(
            left: unbound,
            right: NativeScope.Join(
                left: paused,
                right: NativeScope.Join(
                    left: removed,
                    right: NativeScope.Join(
                        left: invalidated,
                        right: NativeScope.Release(resources: resources, key: key)))));
    }

    private static Fin<Unit> Dispose(Lease<WorkspaceWatch> lease, Op key) => key.Catch(body: () => Fin.Succ(lease.Dispose()));

    private static Unit Tune(CADisplayLink link, PaceWindow window, PaceBounds pace) {
        float ceiling = float.CreateChecked(pace.MaximumFramesPerSecond);
        link.PreferredFrameRateRange = CAFrameRateRange.Create(
            minimum: float.Min(window.Minimum, ceiling),
            maximum: float.Min(window.Maximum, ceiling),
            preferred: float.Min(window.Preferred, ceiling));
        return unit;
    }

    private sealed class LinkTarget : NSObject {
        private Action? tick;
        internal static readonly Selector TickSelector = new("pacerTick:");
        internal Fin<Unit> Bind(Action callback, Op key) =>
            Interlocked.CompareExchange(location1: ref tick, value: callback, comparand: null) is null
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(key.InvalidContext());
        internal void Unbind() => Volatile.Write(location: ref tick, value: null);
        [Export("pacerTick:")]
        public void Tick(CADisplayLink _) => Volatile.Read(location: ref tick)?.Invoke();
    }
}

internal sealed class NativeScope {
    private readonly Dictionary<int, CALayer> lookup = [];
    private readonly List<LayerEdge> edges = [];
    private readonly List<IDisposable> owned = [];
    private readonly HashSet<IDisposable> identities = new(ReferenceEqualityComparer.Instance);
    private Option<ViewBacking> backing = Option<ViewBacking>.None;
    private CALayer? root;
    private bool transferred;

    internal Fin<T> Own<T>(T resource, Op key) where T : class, IDisposable =>
        RejectRoot(resource: resource, key: key).Map(admitted =>
            (Op.SideWhen(condition: identities.Add(item: admitted), action: () => owned.Add(item: admitted)), admitted).Item2);

    internal Fin<T> Hold<T>(Lease<T> lease, Op key) where T : class, IDisposable => lease.Switch(
        state: (Scope: this, Key: key),
        owned: static (state, row) => state.Scope.Own(resource: row.Value, key: state.Key),
        borrowed: static (state, row) => state.Scope.RejectRoot(resource: row.Value, key: state.Key));

    internal Fin<Option<T>> Hold<T>(Option<Lease<T>> lease, Op key) where T : class, IDisposable =>
        lease.Traverse(active => Hold(lease: active, key: key)).As();

    internal CALayer Index(CALayer layer) {
        lookup.Add(key: lookup.Count, value: layer);
        return layer;
    }

    internal Unit Attach(CALayer parent, CALayer child) {
        parent.AddSublayer(layer: child);
        edges.Add(item: new LayerEdge(Parent: parent, Child: child));
        return unit;
    }

    internal Fin<CALayer> Bind(NSView view, Op key) {
        backing = Some(new ViewBacking(View: view, WantsLayer: view.WantsLayer, Layer: view.Layer));
        view.WantsLayer = true;
        if (view.Layer is { } active) { root = active; return Fin.Succ(active); }
        return Optional(view.MakeBackingLayer())
            .ToFin(key.InvalidResult(detail: nameof(NSView.MakeBackingLayer)))
            .Bind(candidate => Own(resource: candidate, key: key))
            .Map(minted => { view.Layer = minted; root = minted; return minted; });
    }

    private Fin<T> RejectRoot<T>(T resource, Op key) where T : class, IDisposable =>
        resource is CALayer layer && ReferenceEquals(objA: root, objB: layer)
            ? Fin.Fail<T>(key.InvalidInput())
            : Fin.Succ(resource);

    internal Fin<LayerMount> Transfer(CALayer root, CALayer top, Op key) =>
        backing.ToFin(key.MissingContext()).Map(captured => {
            LayerMount mount = new(
                root: root,
                top: top,
                lookup: new ReadOnlyDictionary<int, CALayer>(dictionary: new Dictionary<int, CALayer>(lookup)),
                backing: captured,
                edges: [.. edges],
                owned: [.. owned]);
            transferred = true;
            return mount;
        });

    internal Fin<Unit> Release(Op key) => transferred ? Fin.Succ(unit) : Release(resources: [.. owned], key: key);

    internal Fin<Unit> Detach(Op key) => transferred ? Fin.Succ(unit) : Detach(edges: [.. edges], key: key);

    internal Fin<Unit> Restore(Op key) => transferred
        ? Fin.Succ(unit)
        : backing.Match(Some: value => Restore(backing: value, key: key), None: static () => Fin.Succ(unit));

    internal static Fin<Unit> Detach(IEnumerable<LayerEdge> edges, Op key) => key.Catch(body: () => {
        Validation<Error, Unit> outcome = Fin.Succ(unit).ToValidation();
        CATransaction.Begin();
        try {
            outcome = (outcome, key.Catch(body: () => Fin.Succ(Op.Side(action: () => CATransaction.DisableActions = true))).ToValidation())
                .Apply(static (_, _) => unit)
                .As();
            foreach (LayerEdge edge in edges.Reverse()) {
                outcome = (outcome, key.Catch(body: () => Fin.Succ(Op.Side(action: edge.Child.RemoveFromSuperLayer))).ToValidation())
                    .Apply(static (_, _) => unit)
                    .As();
            }
        }
        finally {
            outcome = (outcome, key.Catch(body: () => Fin.Succ(Op.Side(action: CATransaction.Commit))).ToValidation())
                .Apply(static (_, _) => unit)
                .As();
        }
        return outcome.ToFin();
    });

    internal static Fin<Unit> Restore(ViewBacking backing, Op key) => key.Catch(body: () => {
        Validation<Error, Unit> outcome = Fin.Succ(unit).ToValidation();
        CATransaction.Begin();
        try {
            Fin<Unit>[] inverses = [
                key.Catch(body: () => Fin.Succ(Op.Side(action: () => CATransaction.DisableActions = true))),
                key.Catch(body: () => Fin.Succ(Op.Side(action: () => backing.View.Layer = backing.Layer))),
                key.Catch(body: () => Fin.Succ(Op.Side(action: () => backing.View.WantsLayer = backing.WantsLayer))),
            ];
            foreach (Fin<Unit> inverse in inverses) {
                outcome = (outcome, inverse.ToValidation()).Apply(static (_, _) => unit).As();
            }
        }
        finally {
            outcome = (outcome, key.Catch(body: () => Fin.Succ(Op.Side(action: CATransaction.Commit))).ToValidation())
                .Apply(static (_, _) => unit)
                .As();
        }
        return outcome.ToFin();
    });

    internal static Fin<Unit> Release(IEnumerable<IDisposable> resources, Op key) {
        Validation<Error, Unit> outcome = Fin.Succ(unit).ToValidation();
        foreach (IDisposable resource in resources.Distinct(ReferenceEqualityComparer.Instance).Reverse()) {
            outcome = (outcome, key.Catch(body: () => Fin.Succ(Op.Side(action: resource.Dispose))).ToValidation())
                .Apply(static (_, _) => unit)
                .As();
        }
        return outcome.ToFin();
    }

    internal static Fin<Unit> Join(Fin<Unit> left, Fin<Unit> right) =>
        (left.ToValidation(), right.ToValidation()).Apply(static (_, _) => unit).As().ToFin();

    internal static Fin<T> Join<T>(Error primary, Fin<Unit> cleanup) =>
        (Fin.Fail<T>(error: primary).ToValidation(), cleanup.ToValidation()).Apply(static (value, _) => value).As().ToFin();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static class MotionDrive {
    public static Fin<DriveFrame> Step(
        DriveSpec spec, MonotonicBeat beat, AccessibilityPosture posture, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in Admit(spec: spec, key: op)
               from evidence in op.Need(beat)
               from _valid in guard(evidence.IsValid, op.InvalidInput()).ToFin()
               from frame in admitted.Switch(
                   state: (Beat: evidence, Posture: posture, Key: op),
                   easedCase: static (state, row) => state.Posture.Holds(axis: AccessibilityAxis.ReduceMotion)
                       ? Fin.Succ(new DriveFrame(
                           Continues: false,
                           Apply: () => row.Write(obj: row.Curve.Evaluate(t: Terminal(plan: row.Cycle)))))
                       : row.Cycle.Phase(
                               elapsed: state.Beat.Elapsed.TotalSeconds,
                               period: row.Period.TotalSeconds,
                               key: state.Key)
                           .Map(phase => new DriveFrame(
                               Continues: !phase.Completed,
                               Apply: () => row.Write(obj: row.Curve.Evaluate(t: phase.Local)))),
                   sprungCase: static (state, row) => state.Posture.Holds(axis: AccessibilityAxis.ReduceMotion)
                       ? Fin.Succ(new DriveFrame(
                           Continues: false,
                           Apply: () => row.Write(obj: new SpringState(Position: row.Target, Velocity: 0.0))))
                       : row.Shape.Evaluate(
                               origin: row.From,
                               target: row.Target,
                               elapsed: state.Beat.Elapsed.TotalSeconds,
                               key: state.Key)
                           .Map(value => {
                               bool continues = Math.Abs(value.Position - row.Target) > row.Settlement.Position ||
                                   Math.Abs(value.Velocity) > row.Settlement.Velocity;
                               SpringState terminal = continues
                                   ? value
                                   : new SpringState(Position: row.Target, Velocity: 0.0);
                               return new DriveFrame(
                                   Continues: continues,
                                   Apply: () => row.Write(obj: terminal));
                           }),
                   blendCase: static (state, row) => state.Posture.Holds(axis: AccessibilityAxis.ReduceMotion)
                       ? Fin.Succ(new DriveFrame(
                           Continues: false,
                           Apply: () => row.Write(obj: row.From.Mix(
                               other: row.To,
                               amount: Terminal(plan: row.Cycle),
                               path: row.Path))))
                       : row.Cycle.Phase(
                               elapsed: state.Beat.Elapsed.TotalSeconds,
                               period: row.Period.TotalSeconds,
                               key: state.Key)
                           .Bind(phase => state.Key.AcceptValidated<UnitInterval>(
                                   candidate: double.Clamp(row.Curve.Evaluate(t: phase.Local), 0.0, 1.0))
                               .Map(amount => new DriveFrame(
                                   Continues: !phase.Completed,
                                   Apply: () => row.Write(obj: row.From.Mix(
                                       other: row.To,
                                       amount: amount,
                                       path: row.Path))))),
                   decayCase: static (state, row) => state.Posture.Holds(axis: AccessibilityAxis.ReduceMotion)
                       ? row.Shape.Project(velocity: row.Velocity, key: state.Key)
                           .Map(rest => new DriveFrame(
                               Continues: false,
                               Apply: () => row.Write(obj: row.Origin + rest)))
                       : from position in row.Shape.Advance(
                             origin: row.Origin,
                             velocity: row.Velocity,
                             elapsed: state.Beat.Elapsed.TotalSeconds,
                             key: state.Key)
                         from horizon in row.Shape.Settle(
                             velocity: row.Velocity,
                             epsilon: row.Epsilon,
                             key: state.Key)
                         select new DriveFrame(
                             Continues: state.Beat.Elapsed.TotalSeconds < horizon,
                             Apply: () => row.Write(obj: position)))
               select frame;
    }

    internal static Fin<DriveSpec> Admit(DriveSpec spec, Op key) => key.Need(spec).Bind(valid => valid.Switch(
        state: key,
        easedCase: static (op, row) =>
            from _curve in op.Need(row.Curve)
            from _write in op.Need(row.Write)
            from _period in op.Positive(value: row.Period.TotalSeconds)
            from _cycle in guard(Valid(plan: row.Cycle), op.InvalidInput()).ToFin()
            select (DriveSpec)row,
        sprungCase: static (op, row) =>
            from _shape in guard(row.Shape.IsValid, op.InvalidInput()).ToFin()
            from _from in guard(row.From.IsValid, op.InvalidInput()).ToFin()
            from _target in op.Finite(value: row.Target)
            from _settle in guard(row.Settlement.IsValid, op.InvalidInput()).ToFin()
            from _write in op.Need(row.Write)
            select (DriveSpec)row,
        blendCase: static (op, row) =>
            from _path in op.Need(row.Path)
            from _from in op.Need(row.From)
            from _to in op.Need(row.To)
            from _curve in op.Need(row.Curve)
            from _write in op.Need(row.Write)
            from _period in op.Positive(value: row.Period.TotalSeconds)
            from _cycle in guard(Valid(plan: row.Cycle), op.InvalidInput()).ToFin()
            select (DriveSpec)row,
        decayCase: static (op, row) =>
            from _shape in guard(row.Shape.IsValid, op.InvalidInput()).ToFin()
            from _origin in op.Finite(value: row.Origin)
            from _velocity in op.Finite(value: row.Velocity)
            from _epsilon in op.Positive(value: row.Epsilon)
            from _write in op.Need(row.Write)
            select (DriveSpec)row));

    private static bool Valid(CyclePlan plan) => plan.Count.Match(
        Some: static count => count >= 1,
        None: static () => true);

    private static UnitInterval Terminal(CyclePlan plan) => UnitInterval.Create(
        value: plan.Count.Match(
            Some: count => plan.Yoyo && count % 2 == 0 ? 0.0 : 1.0,
            None: static () => 1.0));
}

[BoundaryAdapter]
public static class LayerPaint {
    public static Fin<LayerStyle> Plain(
        CGRect frame, Option<PerceptualColor> fill, Option<PerceptualColor> border, NFloat borderWidth,
        NFloat cornerRadius, bool clip, Option<Lease<CALayer>> mask, Op? key = null) {
        Op op = key.OrDefault();
        return Tints(colours: [fill, border], key: op).Map(minted => new LayerStyle(
            Frame: frame, Fill: minted[0], Border: minted[1], BorderWidth: borderWidth,
            CornerRadius: cornerRadius, Clip: clip, Mask: mask));
    }

    public static Fin<StrokeStyle> Stroked(
        IGraphicsPath path, Option<PerceptualColor> fill, Option<PerceptualColor> stroke,
        NFloat width, bool rounded, Op? key = null) {
        Op op = key.OrDefault();
        return from managed in op.Need(path)
               from minted in Tints(colours: [fill, stroke], key: op)
               from native in op.Catch(body: () => Fin.Succ(managed.ToCG()))
                   .Bind(value => Optional(value).ToFin(op.InvalidResult()))
                   .MapFail(fault => Discard(minted: minted, key: op).Match(
                       Succ: _ => fault, Fail: cleanup => fault + cleanup))
               select new StrokeStyle(
                   Path: new Lease<CGPath>.Owned(Value: native), Fill: minted[0], Stroke: minted[1],
                   Width: width, Rounded: rounded);
    }

    // One fold mints every colour in declaration order and releases the whole minted prefix when a
    // later mint refuses, so a refused style leaves no orphaned CGColor behind; `ToLayer` returns
    // `Owned` alone, which is what makes the prefix release total.
    private static Fin<Seq<Option<Lease<CGColor>>>> Tints(Seq<Option<PerceptualColor>> colours, Op key) =>
        colours.Fold(Fin.Succ(Seq<Option<Lease<CGColor>>>()), (held, colour) => held.Bind(minted =>
            colour.Traverse(value => WideColor.ToLayer(colour: value, key: key)).As().Match(
                Succ: lease => Fin.Succ(minted.Add(lease)),
                Fail: fault => NativeScope.Join<Seq<Option<Lease<CGColor>>>>(
                    primary: fault, cleanup: Discard(minted: minted, key: key)))));

    private static Fin<Unit> Discard(Seq<Option<Lease<CGColor>>> minted, Op key) =>
        minted.Choose(identity).Fold(
            Fin.Succ(unit),
            (held, lease) => NativeScope.Join(
                left: held, right: key.Catch(body: () => Fin.Succ(lease.Dispose()))));
}

[BoundaryAdapter]
public static class Compose {
    public static Fin<Lease<LayerMount>> Mount(MacAnchor anchor, LayerNode node, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from activeAnchor in op.Need(anchor)
               from validNode in AdmitGraph(node: node, key: op)
               from lease in EtoDispatch.Run(body: () => {
                   NativeScope scope = new();
                   Fin<Lease<LayerMount>> outcome = op.Catch(body: () => {
                       CATransaction.Begin();
                       try {
                           CATransaction.DisableActions = true;
                           return from root in scope.Bind(view: activeAnchor.View, key: op)
                                  from top in Materialize(node: validNode, scope: scope, key: op)
                                  from _attached in Fin.Succ(scope.Attach(parent: root, child: top))
                                  from mount in scope.Transfer(root: root, top: top, key: op)
                                  select (Lease<LayerMount>)new Lease<LayerMount>.Owned(Value: mount);
                       }
                       finally { CATransaction.Commit(); }
                   });
                   return outcome.Match(
                       Succ: static value => Fin.Succ(value),
                       Fail: error => NativeScope.Join<Lease<LayerMount>>(
                           primary: error,
                           cleanup: NativeScope.Join(
                               left: scope.Detach(key: op),
                               right: NativeScope.Join(
                                   left: scope.Restore(key: op),
                                   right: scope.Release(key: op)))));
               }, key: op)
               select lease;
    }

    public static Fin<Unit> Mutate(Action body, bool animated = false, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from valid in op.Need(body)
               from settled in EtoDispatch.Run(
                   body: () => Fence(body: valid, animated: animated, completed: Option<Action>.None, key: op), key: op)
               select settled;
    }

    internal static Fin<Unit> Fence(Action body, bool animated, Option<Action> completed, Op key) => key.Catch(body: () => {
        bool applied = false;
        CATransaction.Begin();
        try {
            CATransaction.DisableActions = !animated;
            completed.Iter(continuation => CATransaction.CompletionBlock = () => Op.SideWhen(condition: applied, action: continuation));
            body();
            applied = true;
        }
        finally { CATransaction.Commit(); }
        return Fin.Succ(unit);
    });

    private static Fin<LayerNode> AdmitGraph(LayerNode node, Op key) =>
        AdmitGraph(node: node, layers: new HashSet<CALayer>(ReferenceEqualityComparer.Instance), key: key);

    private static Fin<LayerNode> AdmitGraph(LayerNode node, HashSet<CALayer> layers, Op key) => key.Need(node).Bind(valid => valid.Switch(
        state: (Layers: layers, Key: key),
        plainCase: static (state, row) =>
            from style in state.Key.Need(row.Style)
            from _frame in guard(Valid(frame: style.Frame), state.Key.InvalidInput()).ToFin()
            from _widths in guard(Nonnegative(value: style.BorderWidth) && Nonnegative(value: style.CornerRadius), state.Key.InvalidInput()).ToFin()
            from _fill in AdmitLease(lease: style.Fill, key: state.Key)
            from _border in AdmitLease(lease: style.Border, key: state.Key)
            from _mask in AdmitLayer(lease: style.Mask, layers: state.Layers, key: state.Key)
            from _children in row.Children.TraverseM(child => AdmitGraph(node: child, layers: state.Layers, key: state.Key)).As()
            select (LayerNode)row,
        shapeCase: static (state, row) =>
            from style in state.Key.Need(row.Style)
            from stroke in state.Key.Need(row.Stroke)
            from _frame in guard(Valid(frame: style.Frame), state.Key.InvalidInput()).ToFin()
            from _widths in guard(
                Nonnegative(value: style.BorderWidth) && Nonnegative(value: style.CornerRadius) && Nonnegative(value: stroke.Width),
                state.Key.InvalidInput()).ToFin()
            from _fill in AdmitLease(lease: style.Fill, key: state.Key)
            from _border in AdmitLease(lease: style.Border, key: state.Key)
            from _mask in AdmitLayer(lease: style.Mask, layers: state.Layers, key: state.Key)
            from _path in AdmitLease(lease: stroke.Path, key: state.Key)
            from _strokeFill in AdmitLease(lease: stroke.Fill, key: state.Key)
            from _stroke in AdmitLease(lease: stroke.Stroke, key: state.Key)
            from _children in row.Children.TraverseM(child => AdmitGraph(node: child, layers: state.Layers, key: state.Key)).As()
            select (LayerNode)row,
        hostedCase: static (state, row) =>
            from lease in state.Key.Need(row.Layer)
            from layer in state.Key.Need(lease.Value)
            from _unique in guard(state.Layers.Add(item: layer), state.Key.InvalidInput()).ToFin()
            from _children in row.Children.TraverseM(child => AdmitGraph(node: child, layers: state.Layers, key: state.Key)).As()
            select (LayerNode)row));

    private static Fin<Unit> AdmitLease<T>(Lease<T> lease, Op key) where T : class, IDisposable =>
        from active in key.Need(lease)
        from _ in key.Need(active.Resource)
        select unit;

    private static Fin<Unit> AdmitLease<T>(Option<Lease<T>> lease, Op key) where T : class, IDisposable => lease.Match(
        Some: active => AdmitLease(lease: active, key: key),
        None: static () => Fin.Succ(unit));

    private static Fin<Unit> AdmitLayer(Option<Lease<CALayer>> lease, HashSet<CALayer> layers, Op key) => lease.Match(
        Some: active =>
            from _lease in AdmitLease(lease: active, key: key)
            from layer in key.Need(active.Resource)
            from _unique in guard(layers.Add(item: layer), key.InvalidInput()).ToFin()
            select unit,
        None: static () => Fin.Succ(unit));

    private static bool Valid(CGRect frame) =>
        Finite(value: frame.X) && Finite(value: frame.Y) && Nonnegative(value: frame.Width) && Nonnegative(value: frame.Height);

    private static bool Nonnegative(NFloat value) => Finite(value: value) && value >= NFloat.CreateChecked(0.0);

    private static bool Finite(NFloat value) => NFloat.IsFinite(value);

    private static Fin<CALayer> Materialize(LayerNode node, NativeScope scope, Op key) => node.Switch(
        state: (Scope: scope, Key: key),
        plainCase: static (state, row) =>
            from minted in state.Scope.Own(resource: new CALayer(), key: state.Key)
            from styled in Styled(layer: minted, style: row.Style, scope: state.Scope, key: state.Key)
            from settled in Settle(layer: state.Scope.Index(layer: styled), children: row.Children, scope: state.Scope, key: state.Key)
            select settled,
        shapeCase: static (state, row) =>
            from minted in state.Scope.Own(resource: new CAShapeLayer(), key: state.Key)
            from styled in Styled(layer: minted, style: row.Style, scope: state.Scope, key: state.Key)
            from stroked in Stroked(layer: styled, stroke: row.Stroke, scope: state.Scope, key: state.Key)
            from settled in Settle(layer: state.Scope.Index(layer: stroked), children: row.Children, scope: state.Scope, key: state.Key)
            select settled,
        hostedCase: static (state, row) =>
            from held in state.Scope.Hold(lease: row.Layer, key: state.Key)
            from settled in Settle(layer: state.Scope.Index(layer: held), children: row.Children, scope: state.Scope, key: state.Key)
            select settled);

    private static Fin<CALayer> Settle(CALayer layer, Seq<LayerNode> children, NativeScope scope, Op key) =>
        children
            .TraverseM(child => Materialize(node: child, scope: scope, key: key)
                .Map(materialized => scope.Attach(parent: layer, child: materialized)))
            .As()
            .Map(_ => layer);

    // The native property writes are this owner's platform-forced seam; every lease the writes consume is
    // admitted onto the rail FIRST, so a root-aliasing or dead payload refuses before one pixel of state moves.
    private static Fin<TLayer> Styled<TLayer>(TLayer layer, LayerStyle style, NativeScope scope, Op key) where TLayer : CALayer =>
        from fill in scope.Hold(lease: style.Fill, key: key)
        from border in scope.Hold(lease: style.Border, key: key)
        from mask in scope.Hold(lease: style.Mask, key: key)
        select (Op.Side(action: () => {
            layer.Frame = style.Frame;
            fill.Iter(colour => layer.BackgroundColor = colour);
            border.Iter(colour => layer.BorderColor = colour);
            layer.BorderWidth = style.BorderWidth;
            layer.CornerRadius = style.CornerRadius;
            layer.MasksToBounds = style.Clip;
            mask.Iter(masking => layer.Mask = masking);
        }), layer).Item2;

    private static Fin<CAShapeLayer> Stroked(CAShapeLayer layer, StrokeStyle stroke, NativeScope scope, Op key) =>
        from path in scope.Hold(lease: stroke.Path, key: key)
        from fill in scope.Hold(lease: stroke.Fill, key: key)
        from edge in scope.Hold(lease: stroke.Stroke, key: key)
        select (Op.Side(action: () => {
            layer.Path = path;
            fill.Iter(colour => layer.FillColor = colour);
            edge.Iter(colour => layer.StrokeColor = colour);
            layer.LineWidth = stroke.Width;
            Op.SideWhen(condition: stroke.Rounded, action: () => {
                layer.LineCap = CAShapeLayer.CapRound;
                layer.LineJoin = CAShapeLayer.JoinRound;
            });
        }), layer).Item2;
}

[BoundaryAdapter]
public static class Glides {
    public static Fin<Unit> Animate(CALayer layer, GlidePlan plan, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from target in op.Need(layer)
               from valid in op.Need(plan)
               from settled in valid.Switch(
                   state: (Target: target, Key: op),
                   timedCase: static (frame, row) =>
                       from animation in frame.Key.Need(row.Animation)
                       from name in Named(raw: row.Key, op: frame.Key)
                       from settled in EtoDispatch.Run(body: () => animation.Use(native =>
                           native is CASpringAnimation
                               // a hand-authored spring bypasses the kernel algebra; the SprungCase is the one spring door
                               ? Fin.Fail<Unit>(frame.Key.InvalidInput())
                               : Compose.Fence(
                                   body: () => frame.Target.AddAnimation(animation: native, key: name),
                                   animated: true,
                                   completed: Option<Action>.None,
                                   key: frame.Key)), key: frame.Key)
                       select settled,
                   sprungCase: static (frame, row) =>
                       from shape in guard(row.Shape.IsValid, frame.Key.InvalidInput()).ToFin().Map(_ => row.Shape)
                       from path in Named(raw: row.KeyPath, op: frame.Key)
                       from name in Named(raw: row.Key, op: frame.Key)
                       from horizon in shape.Settle(
                           origin: new SpringState(Position: row.From, Velocity: 0.0),
                           target: row.To, epsilon: SettleEpsilon, key: frame.Key)
                       from settled in EtoDispatch.Run(body: () => frame.Key.Catch(body: () => {
                           // ω and ζ project onto the unit-mass CASpringAnimation columns: k = ω², c = 2ζω —
                           // the kernel shape stays the one spring algebra and the host carries only values.
                           using CASpringAnimation native = CASpringAnimation.FromKeyPath(path: path);
                           native.Mass = 1f;
                           native.Stiffness = (float)(shape.AngularFrequency * shape.AngularFrequency);
                           native.Damping = (float)(2.0 * shape.DampingRatio * shape.AngularFrequency);
                           native.From = NSNumber.FromDouble(row.From);
                           native.To = NSNumber.FromDouble(row.To);
                           native.Duration = horizon;
                           return Compose.Fence(
                               body: () => frame.Target.AddAnimation(animation: native, key: name),
                               animated: true,
                               completed: Option<Action>.None,
                               key: frame.Key);
                       }), key: frame.Key)
                       select settled)
               select settled;
    }

    // Settling epsilon for a compositor-run spring: the projection is conservative by kernel law, and the
    // declared value keeps the duration a policy fact rather than a per-call literal.
    private const double SettleEpsilon = 0.001;

    private static Fin<string> Named(string raw, Op op) =>
        guard(!string.IsNullOrWhiteSpace(value: raw), (Error)op.InvalidInput()).ToFin().Map(_ => raw);

    public static Fin<Unit> Halt(CALayer layer, string glideKey, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from target in op.Need(layer)
               from name in op.Need(glideKey)
               from admitted in guard(!string.IsNullOrWhiteSpace(value: name), op.InvalidInput()).ToFin().Map(_ => name)
               from settled in EtoDispatch.Run(body: () => Compose.Fence(
                   body: () => target.RemoveAnimation(key: admitted),
                   animated: false,
                   completed: Option<Action>.None,
                   key: op), key: op)
               select settled;
    }
}

[BoundaryAdapter]
public static class Curves {
    public static Fin<Lease<CAMediaTimingFunction>> Named(TimingCurve curve, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from row in op.Need(curve)
               from native in op.Catch(body: () => Optional(CAMediaTimingFunction.FromName(name: row.Name)).ToFin(op.InvalidResult()))
               select (Lease<CAMediaTimingFunction>)new Lease<CAMediaTimingFunction>.Owned(Value: native);
    }
}

[BoundaryAdapter]
public static class Effects {
    public static Fin<Lease<CIFilter>> Filter(FilterKind kind, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from row in op.Need(kind)
               from template in op.Catch(body: () => Optional(CIFilter.FromName(name: row.Key)).ToFin(op.InvalidResult()))
               from owned in op.Catch(body: () => {
                   using (template) {
                       NSObject? copied = template.Copy(zone: null);
                       if (copied is CIFilter filter && !ReferenceEquals(objA: template, objB: filter))
                           return Fin.Succ((Lease<CIFilter>)new Lease<CIFilter>.Owned(Value: filter));
                       if (copied is not null && !ReferenceEquals(objA: template, objB: copied)) copied.Dispose();
                       return Fin.Fail<Lease<CIFilter>>(op.InvalidResult());
                   }
               })
               select owned;
    }

    public static Fin<Unit> Pulse(HapticCue cue, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from valid in guard(cue.IsValid, op.InvalidInput()).ToFin().Map(_ => cue)
               from settled in EtoDispatch.Run(body: () => op.Catch(body: () => Fin.Succ(Op.Side(action: () =>
                   NSHapticFeedbackManager.DefaultPerformer.PerformFeedback(
                       pattern: valid.Pattern, performanceTime: valid.Timing)))), key: op)
               select settled;
    }

    public static Fin<Lease<NSVisualEffectView>> Vibrancy(VibrancyPane pane, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from valid in op.Need(pane)
               from admitted in guard(valid.IsValid, op.InvalidInput()).ToFin().Map(_ => valid)
               from lease in EtoDispatch.Run(body: () => {
                   NSVisualEffectView? view = null;
                   Fin<Lease<NSVisualEffectView>> outcome = op.Catch(body: () => {
                       view = new NSVisualEffectView();
                       view.Material = admitted.Material;
                       view.BlendingMode = admitted.Blending;
                       return Fin.Succ((Lease<NSVisualEffectView>)new Lease<NSVisualEffectView>.Owned(Value: view));
                   });
                   return outcome.Match(
                       Succ: static value => Fin.Succ(value),
                       Fail: error => NativeScope.Join<Lease<NSVisualEffectView>>(
                           primary: error,
                           cleanup: NativeScope.Release(
                               resources: view is null ? Array.Empty<IDisposable>() : new IDisposable[] { view },
                               key: op)));
               }, key: op)
               select lease;
    }
}

[BoundaryAdapter]
public static class WideColor {
    // Both crossings carry the reproducibility domain because the wide-colour path is exactly where it changes the
    // answer: a chroma the Display-P3 volume holds and sRGB does not renders differently under a clipping row than
    // under a chroma-reducing one, and a boundary that fixed either would spend the headroom the profile bought.
    public static Fin<Lease<NSColor>> Project(PerceptualColor colour, GamutPolicy? gamut = null, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from admitted in op.Need(colour)
               from channels in op.Catch(body: () => Fin.Succ(admitted.ToRgb(profile: RgbProfile.DisplayP3, gamut: gamut)))
               from _channels in guard(
                   Channel(value: channels.Red) && Channel(value: channels.Green) &&
                   Channel(value: channels.Blue) && Channel(value: channels.Alpha),
                   op.InvalidResult()).ToFin()
               from lease in op.Catch(body: () => Fin.Succ((Lease<NSColor>)new Lease<NSColor>.Owned(Value: NSColor.FromDisplayP3(
                   red: NFloat.CreateChecked(channels.Red),
                   green: NFloat.CreateChecked(channels.Green),
                   blue: NFloat.CreateChecked(channels.Blue),
                   alpha: NFloat.CreateChecked(channels.Alpha)))))
               select lease;
    }

    public static Fin<Lease<CGColor>> ToLayer(PerceptualColor colour, GamutPolicy? gamut = null, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from admitted in op.Need(colour)
               from channels in op.Catch(body: () => Fin.Succ(admitted.ToRgb(profile: RgbProfile.DisplayP3, gamut: gamut)))
               from _channels in guard(
                   Channel(value: channels.Red) && Channel(value: channels.Green) &&
                   Channel(value: channels.Blue) && Channel(value: channels.Alpha),
                   op.InvalidResult()).ToFin()
               from space in op.Catch(body: () => Fin.Succ(CGColorSpace.CreateWithName(name: CGColorSpaceNames.DisplayP3)))
                                .Bind(value => Optional(value).ToFin(op.InvalidResult()))
               from lease in op.Catch(body: () => Fin.Succ((Lease<CGColor>)new Lease<CGColor>.Owned(Value: new CGColor(
                   colorspace: space,
                   components: [
                       NFloat.CreateChecked(channels.Red), NFloat.CreateChecked(channels.Green),
                       NFloat.CreateChecked(channels.Blue), NFloat.CreateChecked(channels.Alpha),
                   ]))))
               select lease;
    }

    public static Fin<PerceptualColor> OfSystem(NSColor native, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from admitted in op.Need(native)
               from eto in op.Catch(body: () => Fin.Succ(admitted.ToEtoWithAppearance()))
               from colour in PerceptualColor.OfRgb(
                   red: eto.R, green: eto.G, blue: eto.B, profile: RgbProfile.Srgb, alpha: eto.A, key: op)
               select colour;
    }

    private static bool Channel(double value) => double.IsFinite(value) && value is >= 0.0 and <= 1.0;
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Native composition ownership and motion flow
    accDescr: A leased layer mount owns the indexed native tree and teardown while a workspace-observed motion attachment runs kernel-timed frames from the main run loop through a transaction fence; profile-aware colour projection reaches AppKit only as NSColor.
    Anchor["MacAnchor view"] --> Mount["Compose.Mount"]
    Graph["LayerNode with native leases"] --> Mount
    Mount --> Lease["Lease of LayerMount"]
    Lease --> Tree["indexed CALayer tree"]
    Lease -->|"dispose on UI thread"| Inverse["detach and release owned natives"]
    Timeline["MonotonicTimeline"] --> Motion["leased MotionAttachment"]
    Watch["WorkspaceWatch"] -->|"WorkspaceFact"| Motion
    Kernel["Easing · CyclePlan · SpringShape · PerceptualColor"] --> Drive["MotionDrive.Step"]
    Drive -->|"DriveFrame"| Motion
    Drive -->|"DriveFrame"| Canvas["CanvasPacer"]
    Motion -->|"Common mode"| Loop["NSRunLoop.Main"]
    Motion -->|"DriveFrame"| Fence["CATransaction fence"]
    Fence --> Tree
    Profile["RgbProfile.DisplayP3"] --> Wide["PerceptualColor.ToRgb"]
    Wide --> Native["owned NSColor"]
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
