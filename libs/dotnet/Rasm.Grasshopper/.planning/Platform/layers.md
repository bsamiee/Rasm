# [RASM_GRASSHOPPER_PLATFORM_LAYERS]

`Compose` owns the macOS composition estate: CoreAnimation graph custody, the transaction fence every layer write rides, display-link motion over the kernel sampler, host-delegated glides, and the Display-P3 layer-colour crossing. Kernel owners supply the whole algebra — `MotionScript`/`MotionSample`/`MotionDrive.Step` sample motion, `MonotonicTimeline.Beat` mints temporal identity, `PaceBand` paces, `SettleBand` stops, and `PerceptualColor` projects — so this boundary binds values to native resources and computes nothing. `MacGate` admits the platform, `UiThread` owns UI affinity, and every retained native crosses through `Lease<T>` with one exact inverse.

## [01]-[INDEX]

- [02]-[LAYER_GRAPH]: `LayerTrait` + `LayerStyle` + `StrokePlan` + `LayerNode` + `LayerPaint` + `LayerMount` + `Compose.Mount` — explicit native custody, the one style mint, indexed graph materialization, and leased teardown.
- [03]-[TRANSACTION]: `TransactionPosture` + `Compose.Mutate`/`Fence` — the one `CATransaction` fence and its animated/instant posture row.
- [04]-[DISPLAY_LINK]: `MotionAttachment` — the `CADisplayLink` lease sampling kernel `MotionDrive.Step` and applying each frame at the host.
- [05]-[GLIDES]: `GlideKey` + `GlidePlan` + `TimingCurve` + `Glides` + `Curves` — host-delegated animation attachment and the standard timing-function vocabulary.
- [06]-[WIDE_COLOR]: `WideColor.ToLayer` — the one Display-P3 `CGColor` mint off the kernel colour owner.

## [02]-[LAYER_GRAPH]

- Owner: `LayerTrait` `[SmartEnum<string>]` realizing `ICapability<LayerTrait>` — the two independent style bits (`Clip` masks to bounds, `Rounded` caps and joins a stroke) as one set; every corner is legal, so the law is `CapabilityLaw.Open` and states it. `LayerStyle` — the ONE style shape both node families read: frame, background, an edge carried as a colour-and-width PAIR (a border colour with no width and a width with no colour are unrepresentable), corner radius, traits, and an optional mask. `StrokePlan` — only what a shape ADDS: the owned path, the interior fill, and the stroke edge pair; the former `StrokeStyle` twin carried the style's columns again under second names and its `Fill` collided with the layer background's. `LayerNode` `[Union]` is the recursive graph — `PlainCase` and `ShapeCase` mint boundary-owned layers, `HostedCase(Lease<CALayer>.Owned, …)` consumes a detached caller-configured layer under sole custody. `LayerPaint` is the one style mint; `LayerMount` the retained graph; `Compose.Mount` the entry.
- Entry: `Compose.Mount(MacAnchor anchor, LayerNode node, Op? key = null)` → `Fin<Lease<LayerMount>>`. One recursive admission fold rejects malformed styles, dead lease payloads, and duplicate hosted or mask identities; the mount scope rejects any graph payload identical to the anchor root before custody can double. Materialization stays detached until the complete preorder graph and lookup exist; attachment is the final mutation; any failure reverses edges and view backing, releases every acquired native, and aggregates inverse faults with the originating refusal through the `Error` monoid.
- Entry: `LayerPaint.Plain` builds a `LayerStyle` and `LayerPaint.Stroked` a `StrokePlan`, every colour crossing `WideColor.ToLayer` and every path crossing `Eto.Mac.CGConversions.ToCG(this IGraphicsPath)` into an owned `Lease<CGPath>`. Minting is prefix-safe: a refused colour or path releases every lease already taken in the same call and aggregates the release fault into the refusal — the kernel's ruled disposal posture, never a discard.
- Law: every scope refusal rides the rail with its own cause — a graph payload aliasing the anchor root is `InvalidInput`, a host backing-layer mint answering null is `InvalidResult` naming the member, a transfer with no captured backing is `MissingContext` — and the native writes stay downstream of admission, which is what keeps a refused lease from moving any layer state before it refuses.
- Law: `LayerNode` and its style shapes carry declared equality — `[Equatable]` with `[OrderedEquality]` on `Children` and member-level `[ReferenceEquality]` on every lease and mask handle — because a host handle publishes no content and two handles wrapping identical bytes are two resources with two lifetimes (the kernel paint page's cache-identity law, held here for the same reason).
- Law: graph ordinals are stable only for one mount lease; every layer `Lookup` or `Find` hands back is borrowed and lives only while that lease lives. `Reframe(int, CGRect, Op?)` is the mount's own resize path — a raw `Frame` write off a borrowed handle is the deleted form the member absorbs.
- Law: compositing is live-proven host behavior — the canvas backing `CALayer` is live even where `WantsLayer` reads false, a mounted sublayer survives the host's own `Drawable` paint, and a compositor-run animation advances its presentation layer with zero canvas paint events across the run. Canvas backing layer's `ContentsFormat` is 8-bit RGBA, so wide colour rides the mounted overlay's own contents, never the host layer's format.
- Law: release is the kernel custody idiom — a one-shot `Atom<bool>` latch stepped through `Cell.Step` (a second release reads its `Refused` verdict rather than racing an interlocked int), the inverse marshalled once through `UiThread.Run`, edges removed in reverse order inside one `TransactionPosture.Instant` fence, view backing restored, and owned resources released in reverse acquisition order with every disposal fault aggregated onto the mount's `FaultCell`.
- Packages: Microsoft.macOS (`NSView`, `CALayer`, `CAShapeLayer`, `CGPath`, `CGColor`, `CGRect`), Eto.Drawing (`IGraphicsPath`), Eto.macOS (`CGConversions.ToCG`), Generator.Equals, `Rasm.Numerics` (`PerceptualColor`), `Rasm.Domain` (`Op`, `Lease<T>`, `FaultCell`, `Cell`, `Custody`), `Rasm.Interaction` (`UiThread`, `UiDispatch<T>`), `Platform/native.md` (`MacGate`, `MacAnchor`).
- Growth: a new layer family is one `LayerNode` case whose native payload enters through the same scope; a new style bit is one `LayerTrait` row; graph lookup, fencing, failure cleanup, and teardown never widen.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Mac;
using Foundation;
using Generator.Equals;
using ObjCRuntime;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;

namespace Rasm.Grasshopper.Platform;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerTrait : ICapability<LayerTrait> {
    public static readonly LayerTrait Clip = new(key: "clip");
    public static readonly LayerTrait Rounded = new(key: "rounded");

    public static CapabilityLaw<LayerTrait> Law => CapabilityLaw<LayerTrait>.Open;
}

[Union]
public abstract partial record LayerNode {
    private LayerNode() { }
    [Equatable]
    public sealed partial record PlainCase(
        [property: ReferenceEquality] LayerStyle Style,
        [property: OrderedEquality] Seq<LayerNode> Children) : LayerNode;
    [Equatable]
    public sealed partial record ShapeCase(
        [property: ReferenceEquality] LayerStyle Style,
        [property: ReferenceEquality] StrokePlan Stroke,
        [property: OrderedEquality] Seq<LayerNode> Children) : LayerNode;
    [Equatable]
    public sealed partial record HostedCase(
        [property: ReferenceEquality] Lease<CALayer>.Owned Layer,
        [property: OrderedEquality] Seq<LayerNode> Children) : LayerNode;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LayerStyle(
    CGRect Frame,
    Option<Lease<CGColor>> Background,
    Option<(Lease<CGColor> Colour, NFloat Width)> Edge,
    NFloat CornerRadius,
    CapabilitySet<LayerTrait> Traits,
    Option<Lease<CALayer>> Mask);

public sealed record StrokePlan(
    Lease<CGPath> Path,
    Option<Lease<CGColor>> Interior,
    Option<(Lease<CGColor> Colour, NFloat Width)> Edge);

internal readonly record struct LayerEdge(CALayer Parent, CALayer Child);

internal readonly record struct ViewBacking(NSView View, bool WantsLayer, CALayer? Layer);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LayerMount : IDisposable {
    private readonly ViewBacking backing;
    private readonly LayerEdge[] edges;
    private readonly IDisposable[] owned;
    private readonly FaultCell faults;
    private readonly Atom<bool> released = Atom(false);

    internal LayerMount(
        CALayer root, CALayer top, IReadOnlyDictionary<int, CALayer> lookup,
        ViewBacking backing, LayerEdge[] edges, IDisposable[] owned, FaultCell faults);

    public CALayer Root { get; }
    public CALayer Top { get; }
    public IReadOnlyDictionary<int, CALayer> Lookup { get; }
    public int Count => Lookup.Count;

    public Fin<CALayer> Find(int ordinal, Op? key = null);

    public Fin<Unit> Reframe(int ordinal, CGRect frame, Op? key = null) {
        Op op = key.OrDefault();
        return from layer in Find(ordinal: ordinal, key: op)
               from settled in Compose.Mutate(
                   body: () => layer.Frame = frame, posture: TransactionPosture.Instant, key: op)
               select settled;
    }

    public Fin<Unit> Release(Op? key = null);
    public void Dispose() => _ = Release();
}

internal sealed class NativeScope {
    [SmartEnum<int>]
    internal sealed partial class ScopeCustody {
        public static readonly ScopeCustody Building = new(key: 0);
        public static readonly ScopeCustody Transferred = new(key: 1);
    }

    internal Fin<T> Own<T>(T resource, Op key) where T : class, IDisposable;
    internal Fin<T> Hold<T>(Lease<T> lease, Op key) where T : class, IDisposable;
    internal Fin<Option<T>> Hold<T>(Option<Lease<T>> lease, Op key) where T : class, IDisposable;
    internal CALayer Index(CALayer layer);
    internal Unit Attach(CALayer parent, CALayer child);
    internal Fin<CALayer> Bind(NSView view, Op key);
    internal Fin<LayerMount> Transfer(CALayer root, CALayer top, FaultCell faults, Op key);
    internal Fin<Unit> Unwind(Op key);

}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static class LayerPaint {
    public static Fin<LayerStyle> Plain(
        CGRect frame, Option<PerceptualColor> background, Option<(PerceptualColor Colour, NFloat Width)> edge,
        NFloat cornerRadius, CapabilitySet<LayerTrait> traits, Option<Lease<CALayer>> mask, Op? key = null);

    public static Fin<StrokePlan> Stroked(
        IGraphicsPath path, Option<PerceptualColor> interior, Option<(PerceptualColor Colour, NFloat Width)> edge,
        Op? key = null);
}

[BoundaryAdapter]
public static class Compose {
    public static Fin<Lease<LayerMount>> Mount(MacAnchor anchor, LayerNode node, Op? key = null);

    public static Fin<Unit> Mutate(Action body, TransactionPosture posture, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from valid in op.Need(body)
               from settled in UiThread.Run(
                   new UiDispatch<Unit>.Blocking(() => Fence(body: valid, posture: posture, completed: None, key: op)),
                   DispatchLane.Immediate, op)
               select settled;
    }

    internal static Fin<Unit> Fence(Action body, TransactionPosture posture, Option<Action> completed, Op key);
}
```

## [03]-[TRANSACTION]

- Owner: `TransactionPosture` `[SmartEnum<int>]` — `Instant` (actions disabled: sampled motion and teardown, the default posture) and `Animated` (actions enabled: host-delegated glides); the row carries `DisableActions`, so no caller re-branches a bool at the fence.
- Law: `CATransaction.Commit` always closes the begun transaction, and the completion block installs only after the body applied — a completion firing over a refused body certifies a mutation that never landed.
- Growth: a third posture is one row; the fence never widens.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
namespace Rasm.Grasshopper.Platform;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TransactionPosture {
    public static readonly TransactionPosture Instant = new(key: 0, disableActions: true);
    public static readonly TransactionPosture Animated = new(key: 1, disableActions: false);

    internal bool DisableActions { get; }
}
```

## [04]-[DISPLAY_LINK]

- Owner: `MotionAttachment` — the `CADisplayLink` lease: callback target, run-loop attachment, workspace observation, and the per-tick fold that advances the injected timeline, samples kernel `MotionDrive.Step`, and applies the sample at the host inside the transaction fence. Kernel owns every number; this owner owns the timer lease and the apply seam alone (the branch motion ruling), and `Canvas/motion.md`'s pacer is its one mount.
- Entry: `Attach(anchor, script, apply, clock, faults, completed = default, key = null)` → `Fin<Lease<MotionAttachment>>` — admits the script through kernel `MotionDrive.Admit`, acquires the workspace watch, seats the callback through `Cell.Seat` so a second claim refuses typed, pauses and tunes the link, attaches to `NSRunLoop.Main` under `Common` mode, and only then resumes. Any mint or dispatch fault removes the run-loop attachment, invalidates and disposes every native, releases the watch, and aggregates cleanup faults through kernel `Custody.Release`.
- Law: each callback advances `clock.Beat(seed, cadence, key)` from the origin or prior beat — the cadence DERIVES from the applied pace band's preferred rate, so the ordinal counts display periods and a coalesced tick reads as a gap. Clock is the session's ONE injected timeline (folder RULINGS `[02]`); this page mints none.
- Law: the posture each sample carries is `MotionPosture(fact.Concessions, pace)` read off one `WorkspaceFact` snapshot, and the pace band scales through `PaceBand.ScaleTo` off the display's live ceiling — a retune fires only when the snapshot's pace moved, so a display migration never combines a stale ceiling with a new accessibility state.
- Law: a colour tween is an `Eased` script whose apply closure samples `PerceptualColor.Mix` at the eased value — the kernel refuses a colour case by design, and the blend path lives in the closure (the kernel's own NAMED LOSS, honoured here).
- Law: completion belongs to the terminal frame — the sample whose `Continues` is false installs a once-gated continuation (`Cell.Step` over the completion latch) on `CATransaction.CompletionBlock`, pauses the link, and suppresses the continuation after release begins. Every beat, sampling, or apply fault parks on the injected `FaultCell` and pauses the link — the cell is bounded evidence, never an `Atom<Option<Error>>` holding only the newest fault.
- Law: release is the kernel one-shot — the `Atom<bool>` latch through `Cell.Step`, unbind before run-loop removal, pause, removal, invalidation, and both native disposals attempted independently, the workspace watch released even when UI dispatch refuses, every inverse fault aggregated.
- Packages: Microsoft.macOS (`CADisplayLink`, `CAFrameRateRange`, `NSRunLoop`, `NSObject`, `Selector`, `ExportAttribute`), `Rasm.Parametric` (`MonotonicTimeline`, `BeatSeed`, `MonotonicBeat`, `MotionScript`, `MotionSample`, `MotionDrive`, `MotionPosture`, `PaceBand`), `Rasm.Domain` (`Op`, `Lease<T>`, `FaultCell`, `Cell`), `Platform/native.md` (`WorkspaceFact`, `WorkspaceWatch`, `NativeSeam.Watch`, `MacGate`).
- Growth: a new sampled modality is one kernel `MotionScript` case; this attachment inherits beat, posture, terminal, and verdict semantics with no arm of its own.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using AppKit;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Parametric;

namespace Rasm.Grasshopper.Platform;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class MotionAttachment : IDisposable {
    public static Fin<Lease<MotionAttachment>> Attach(
        MacAnchor anchor,
        MotionScript script,
        Action<MotionSample> apply,
        MonotonicTimeline clock,
        FaultCell faults,
        Option<Action> completed = default,
        Op? key = null);

    public void Dispose();

    private Fin<Unit> Tick(Op key) =>
        from beat in clock.Beat(seed: seed.Value, cadence: cadence, key: key)
        from fact in workspace.Value.ToFin(key.InvalidResult())
        from pace in PaceBand.Portable.ScaleTo(reference: fact.Ceiling, key: key)
        from _tuned in Retune(pace: pace, key: key)
        from stepped in MotionDrive.Step(
            script: script, beat: beat, posture: new MotionPosture(Concessions: fact.Concessions, Pace: pace), key: key)
        let terminal = stepped.Continues ? Option<Action>.None : Continuation(key: key)
        from applied in Compose.Fence(
            body: () => apply(stepped.Sample), posture: TransactionPosture.Instant, completed: terminal, key: key)
        select stepped.Continues ? unit : ignore(Pause(key: key));

    private sealed class LinkTarget : NSObject {
        private readonly Atom<Option<Action>> tick = Atom(Option<Action>.None);
        internal static readonly Selector TickSelector = new("pacerTick:");
        internal Transition<Option<Action>> Bind(Action callback) => Cell.Seat(cell: tick, mint: () => callback);
        internal Transition<Option<Action>> Unbind() => Cell.Take(cell: tick);
        [Export("pacerTick:")]
        public void Tick(CADisplayLink _) => tick.Value.Iter(static callback => callback());
    }
}
```

## [05]-[GLIDES]

- Owner: `GlideKey` `[ValueObject<string>]` — the admitted managed key `CALayer.AddAnimation`/`RemoveAnimation` require, so the two raw-string guards the pair carried are one admission; `GlidePlan` `[Union]` — `TimedCase` pairs an explicitly owned or borrowed `CAAnimation` with its key, `SprungCase` carries the kernel `SpringShape` with its key path, endpoints, and `SettleBand`, projected onto `CASpringAnimation` at the attach (unit mass, `k = ω²`, `c = 2ζω`, duration from the kernel `Settle` projection); `TimingCurve` `[SmartEnum<int>]` closes the standard CoreAnimation timing names; `Glides` and `Curves` are the entries.
- Law: `Glides.Animate` REFUSES a hand-authored `CASpringAnimation` on the timed arm — the spring door is `SprungCase`, so locally-authored spring constants cannot fork motion feel past the kernel mint (branch spring-parity ruling). CoreAnimation copies the attached animation, so an owned plan releases immediately after the call while a borrowed plan stays caller-held.
- Law: the settle horizon reads `SpringShape.Settle(origin, target, band, key)` off the caller's `SettleBand` — the hand epsilon const is deleted, and `SettleBand.Perceptual` is the row a caller with no tighter band names.
- Law: sampled drives and host glides stay distinct by state ownership — a sampled drive exposes kernel state and retained completion through `MotionAttachment`; a glide delegates interpolation to CoreAnimation and owns only attachment and removal.
- Packages: Microsoft.macOS (`CAAnimation`, `CASpringAnimation`, `CAMediaTimingFunction`, `CALayer`, `NSString`), Thinktecture.Runtime.Extensions, `Rasm.Parametric` (`SpringShape`, `SettleBand`), `Rasm.Domain` (`Op`, `Lease<T>`), `Platform/native.md` (`MacGate`). Consumer: `Canvas/paint.md`'s CoreAnimation overlay projection.
- Growth: a new standard timing name is one `TimingCurve` row; a new host animation is one `GlidePlan` case on the one attachment lifecycle.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using CoreAnimation;
using Foundation;
using Rasm.Domain;
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Grasshopper.Platform;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct GlideKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "GlideKey requires a non-blank animation key.");
    }
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

[Union]
public abstract partial record GlidePlan {
    private GlidePlan() { }
    public sealed record TimedCase(Lease<CAAnimation> Animation, GlideKey Key) : GlidePlan;
    public sealed record SprungCase(SpringShape Shape, string KeyPath, double From, double To, SettleBand Band, GlideKey Key) : GlidePlan;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static class Glides {
    public static Fin<Unit> Animate(CALayer layer, GlidePlan plan, Op? key = null);
    public static Fin<Unit> Halt(CALayer layer, GlideKey glide, Op? key = null);
}

[BoundaryAdapter]
public static class Curves {
    public static Fin<Lease<CAMediaTimingFunction>> Named(TimingCurve curve, Op? key = null);
}
```

## [06]-[WIDE_COLOR]

- Owner: `WideColor.ToLayer(PerceptualColor colour, Option<GamutPolicy> gamut = default, Op? key = null)` → `Fin<Lease<CGColor>>` — the layer-graph crossing `LayerPaint` calls for every style colour: it composes `PerceptualColor.ToRgb(profile: RgbProfile.DisplayP3, gamut:)` and mints the `CGColor` DIRECTLY on the `CGColorSpaceNames.DisplayP3` space from the returned unit channels, so the projection reads the kernel tuple and owns one lifetime.
- Law: this page passes no transfer, taking the `RgbTransfer.Encoded` default — the `CGColorSpaceNames.DisplayP3` mint consumes COMPANDED components, so a `RgbTransfer.Linear` read here hands scene-linear light to an encoded-channel constructor and darkens every swatch. Profile conversion, chromatic adaptation, and gamut mapping stay `PerceptualColor`'s, never a host-local pipeline.
- Law: the `NSColor` round trip is REFUSED as the layer crossing — `CGConversions.ToCG(this NSColor)` re-spaces to sRGB and floors at opaque black without signalling, and its result borrows the receiver's lifetime; the direct mint has one lifetime and no clamp arm. Outbound Eto colour members `Color.ToNSUI()`/`Color.ToCG()` are refused with it — the folder's colour currency is the kernel `PerceptualColor`, never an Eto `Color`, so neither has an admitted ingress. `IMatrix.ToCG()` stays uncomposed while no layer or context transform crosses this boundary; a transform-bearing `LayerStyle` column takes it the moment one does. NAMED LOSS: the deleted `Project` (NSColor egress) and `OfSystem` (appearance-resolved ingress) arms — both had zero consumers; an OS swatch reads through kernel `ChromeRole.Sample`, and an AppKit `NSColor` consumer re-lands as one member on this owner.
- Packages: Microsoft.macOS (`CGColor`, `CGColorSpace`, `CGColorSpaceNames`), `Rasm.Numerics` (`PerceptualColor`, `RgbProfile`, `GamutPolicy`), `Rasm.Domain` (`Op`, `Lease<T>`), `Platform/native.md` (`MacGate`).
- Growth: a new display profile is one kernel `RgbProfile` row and a new reproducibility domain one kernel `GamutPolicy` row; the projection is unchanged while the selected rows vary.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using CoreGraphics;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Grasshopper.Platform;

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static class WideColor {
    public static Fin<Lease<CGColor>> ToLayer(PerceptualColor colour, Option<GamutPolicy> gamut = default, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from channels in op.Catch(body: () => Fin.Succ(colour.ToRgb(profile: RgbProfile.DisplayP3, gamut: Op.ToHostSlot(gamut))))
               from space in op.Catch(body: () => Optional(CGColorSpace.CreateWithName(name: CGColorSpaceNames.DisplayP3)).ToFin(op.InvalidResult()))
               from lease in op.Catch(body: () => Fin.Succ((Lease<CGColor>)new Lease<CGColor>.Owned(Value: new CGColor(
                   colorspace: space,
                   components: [
                       NFloat.CreateChecked(channels.Red), NFloat.CreateChecked(channels.Green),
                       NFloat.CreateChecked(channels.Blue), NFloat.CreateChecked(channels.Alpha),
                   ]))))
               select lease;
    }
}
```

## [07]-[DENSITY_BAR]

| [INDEX] | [CONCERN]    | [OWNER]                      | [RAIL]                                          | [CASES] |
| :-----: | :----------- | :--------------------------- | :---------------------------------------------- | :-----: |
|  [01]   | layer graph  | `LayerNode` + `LayerMount`   | one recursive admission, one leased inverse     |    3    |
|  [02]   | style mint   | `LayerPaint`                 | prefix-safe colour/path minting                 |    2    |
|  [03]   | transaction  | `TransactionPosture` + fence | one fence, posture as a row                     |    2    |
|  [04]   | display link | `MotionAttachment`           | kernel `MotionDrive.Step`, host apply seam      |    1    |
|  [05]   | glides       | `GlidePlan` + `Glides`       | kernel spring projection, host-delegated timing |    2    |
|  [06]   | wide colour  | `WideColor.ToLayer`          | one Display-P3 mint off the kernel tuple        |    1    |

Motion algebra (`DriveSpec`/`MotionDrive`/`DriveFrame`/`PaceWindow`/`SpringSettlement`) deleted onto kernel `MotionScript`/`MotionSample`/`MotionDrive.Step`/`PaceBand`/`SettleBand`; the CoreImage/haptic/vibrancy estate deleted with zero consumers; a new native capability re-lands as one case or row on the owners above.

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
