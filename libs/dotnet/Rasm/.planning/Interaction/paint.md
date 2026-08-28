# [RASM_PAINT]

`Rasm.Interaction` owns the one immediate-mode paint vocabulary, the leased resource stock that realizes it, the owner-drawn surface it replays onto, and the single correspondence between the kernel colour owner and the host colour struct. A paint is a VALUE — an ordered mark run whose specs carry their own identity — so the same run bounds, hit-tests, replays onto a live surface, and replays again onto a printed page without any of the four re-deriving geometry the others already resolved. Nothing here retains a scene: the host re-issues the whole run on invalidation, and the swap that changes what is drawn is a value swap under a redraw policy.

Both host boundaries carried the whole algebra and each held half. Rhino held the two-band mark split with its kernel-lowered path run, the dual-projection stroke rows, the `SystemFonts` role roster, the memoized glyph block, the `Drawable` mount with its quality bracket, its invalidation family, its off-event degrade, its IME verbs, and the locked pixel window; Grasshopper held the ten-case path vocabulary, the gradient and texture fill family with its wrap and transform columns, the transform algebra, the spec-to-resource stock with reverse-creation release, the cull tally, and the paint tally. This owner is their union at every axis. Host residue stays where its host types live: Rhino keeps `WorldMark` and its twenty `DisplayPipeline` cases, the `.Rhino()` stroke projection column, `StrokePattern`, `ShadedMaterial`, `IsoBanding`, the `BlendUse` pair, and the `DisplayBitmap` half of its sprite cache; Grasshopper keeps `PaintPhase`, `PaintScene`, `PaintHook`, `PaintAnchor`, the four Grasshopper2-typed mark cases, and the CoreAnimation projection of its overlay.

Composition is downward and sideways inside the sub-domain: `Lease<T>`, `Atom`, `Cell`/`Transition`, `Validation`, `ValidityClaim`, `Deterministic`, and `ContentHash`/`CanonicalWriter` from `Domain`; `FaultCell` from `Domain/hooks`; `TelemetrySource` from `Domain/frame`; `PerceptualColor`, `GamutPolicy`, `BlendPath`, `UnitInterval`, `PositiveMagnitude`, `VectorAngle`, and `Dimension` from `Numerics/atoms`; `MonotonicTimeline`, `MonotonicStamp`, and `GaugedSpan<TLane>` from `Parametric/projections`; `TextHeight` from `Drawing/sheet`; `UiFault`, `RejectReason`, `DispatchLane`, `UiDispatch<T>`, and `UiThread` from `Interaction/dispatch`; `FieldTag` from `Interaction/control`. `Eto.Drawing` never enters as a manifest using — its `Matrix` collides with `Rasm.Numerics.Matrix` and its `Point` with the globally imported `Rhino.Geometry.Point` — so every fence here aliases the host types it names.

## [01]-[INDEX]

- [02]-[MARK]: `Dash`, `PathSpec`, `FillSource`, `TransformSpec`, `TypeRole`, `TypeSource`, `Mark`, `StrokeSpec`, `TypeFace`, `BlockSpec`, `GlyphBlock`, `PaintTally`, `PaintProgram`, `PaintStock` — the spec vocabulary, the mark run that folds it, and the leased stock that realizes it.
- [03]-[SURFACE]: `ScenePolicy`, `Redraw`, `AlphaLayout`, `SurfaceSpec`, `Surface`, `PixelLease` — the owner-drawn mount, its quality bracket, its invalidation family, the coverage carriage a lock publishes, and the pixel window over both imaging stacks.
- [04]-[COLOR]: `ChromeRole`, `PaintColor` — the OS palette read and the one `PerceptualColor` ↔ host-colour correspondence.
- [05]-[THEME]: `ThemeVariant`, `PaletteRole`, `SpaceRole`, `TypeSlot`, `ContrastFloor`, `ContrastRule`, `ThemeProgram`, `ThemeSnapshot`, `ThemeShift`, `ThemeChange`, `ThemeGrid` — the generated colour-by-variant grid beside its spacing and typography scales.

## [02]-[MARK]

- Owner: `PathSpec` the closed geometry vocabulary; `FillSource` the fill family; `StrokeSpec` and `Dash` the stroke pair; `TransformSpec` the affine family; `TypeRole`, `TypeSource`, `TypeFace`, and `BlockSpec` the typography vocabulary; `GlyphBlock` the retained measured run; `Mark` the one drawable case family; `PaintProgram` the ordered run every consumer passes whole; `PaintStock` the leased spec-to-resource registry; `PaintTally` the pass evidence.
- Cases: `Mark` is `Stroke`, `Fill`, `Text`, `Glyph`, `Image`, `ImageRegion`, `Clip`, and `Transform` — eight drawables over one run. `PathSpec` carries ten figures, `FillSource` five sources, `TransformSpec` seven affines, and `Dash` six patterns.
- Entry: `PaintProgram.Of` admits a run and computes its identity once; `Bounds` answers the run's union extent, `Hit` its z-ordered ordinals, and `Replay` its one crossing onto a live target.
- Law: `PaintProgram` is a VALUE, not a delegate pair. The boundaries spelled it as `(Func<Graphics, Fin<Unit>> Paint, Func<PointF, Fin<Seq<int>>> Hit)` because strata forbade the Eto page reaching its own mark vocabulary; here the vocabulary and the surface are one namespace, so the run itself crosses. NAMED LOSS: a consumer can no longer mount an opaque paint closure, and a host that paints through its own pipeline — Rhino's `WorldMark` band — paints there rather than through this owner. Witness: `Rasm.Rhino/.planning/Eto/canvas.md:172` rebuilt as `PaintProgram.Of(marks)`.
- Law: DENSITY is READ off the target, never threaded. `Graphics.PointsPerPixel` carries the device-pixel ratio the replay scales against, so a replay takes no density argument and cannot disagree with the surface it draws on; only the off-graphics probes — `Bounds` and `Hit`, which have no target — take the density their caller measured, and `Surface.HitTest` supplies its own so a consumer never guesses one.
- Law: the run's `Identity` is a `Deterministic`-seeded fold over the canonical writer's rows and REFUSES `ContentHash.Halves` by decision, not by omission: the discriminant is reference-ordinal PROCESS identity, so two runs holding distinct handles over identical bytes must stay two runs where a content key would merge them.
- Law: cache identity is the SPEC's, stated once for the whole page. Every spec is `[Equatable]`, its value columns compare structurally and its host handles — `EtoImage`, `EtoMatrix`, `EtoDash` — carry member-level `[ReferenceEquality]`, because a host handle publishes no content and two handles wrapping identical bytes are two resources with two lifetimes. The stock keys on the spec, so the law is what makes one pen serve every mark that asked for it.
- Law: the stock's release aggregates EVERY disposal fault from `Errors.None` through `Error.operator +`, so `Faults` is the existing zero-one-many carrier and `Release` returns that accumulated error — a stock that refused three handles reports three, where the single-slot column lost every fault but the last.
- Law: the stock seats first-writer-wins through `Cell.Claim`, and a `Ceded` verdict RELEASES this caller's surplus mint. The Grasshopper form cleared its stranded handle before the table insert, so a duplicate key threw past a resource nothing then owned (`Rasm.Grasshopper/.planning/Canvas/paint.md:770`); the transition owner makes the losing mint's disposal the arm rather than the omission.
- Law: a paint-pass resource dies with the pass, so `Platform.Cache<TKey,TValue>` is REFUSED here and named in the fence — its lifetime is the platform's, and a pen cached across a plugin's life outlives every surface that asked for it. The one shared half is a `TypeRole` face, which is already process-cached by `SystemFonts` and therefore leases `Borrowed`.
- Law: the pass is gauged by `MonotonicTimeline.Gauged<T, DispatchLane>` and `PaintTally` carries its `GaugedSpan`. The Grasshopper tally stored an entered stamp, a settled stamp, and a latency span — a mark-and-elapsed pair below the app root, which is the deleted form the kernel timeline closes. NAMED LOSS: a consumer ordering two passes reads the timeline's own `Order` over captured stamps rather than two stamps stored on the tally.
- Law: a shaped run is measured ONCE. `Text` is the immediate run a pass measures through its own stock; `Glyph` carries a `GlyphBlock` whose memo survives frames. The discriminant is payload TIMING and it is named here: a pointer move hit-tests every mark in a frame, so a per-probe re-shape pays host text layout per mouse pixel.
- Law: text shaping is UI-affine — no Eto backend declares its platform text stack safe off the marshal — so `GlyphBlock.Measure` crosses `UiThread` and answers a result; a layout pass computing extents on a compute lane is exactly the caller this forecloses.
- Law: a `Group` carrying both a transform and a clip becomes a `Transform` wrapping a `Clip`. NAMED LOSS: the one-value group; the former optional-transform and optional-clip payloads rebuild as the two-level nest, which is also what makes each case's apply and unapply total.
- Exemption: the composite path arms lower through `Rasm.Parametric` at admission on the Rhino boundary and that lowering STAYS there — the kernel `Composite` carries the authored figures and the host path builder consumes them, because `IGraphicsPath` already owns rounded-rectangle and cardinal-curve construction and a second tessellator beside it is the deleted form (`api-eto-drawing.md:169`, `:172`).
- Output: `PaintTally` accounts every mark as drawn or culled and carries the gauged span; the accountability fold is what makes a silent cull visible.
- Packages: Eto.Drawing for the paint algebra (prelude-aliased); LanguageExt.Core for the types, `Seq`, `Atom`, and `Lease`; Thinktecture.Runtime.Extensions for the unions, rows, and the family-name value object; Generator.Equals for the spec cache identity; `Numerics/atoms` for the bounded tension and the three measured angles.
- Growth: a new drawable is one `Mark` case breaking every replay arm loudly; a new figure is one `PathSpec` case; a new fill is one `FillSource` case; a new dash pattern is one `Dash` case.
- Boundary: HOST-SPECIFIC-STAYS — Rhino's `WorldMark`, its `Stroke.Rhino()` projection column, `StrokePattern`, `ShadedMaterial`, `IsoBanding`, the `BlendUse` source-and-destination pair, and the `DisplayBitmap` sprite cache; Grasshopper's `IconCase`, `CapsuleCase`, `WireGhostCase`, and the `EdgeDescription` stroke column. The `DisplayPen` eight-entry dash cap is the Rhino projection's own admission and never bounds this vocabulary.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoBrush = Eto.Drawing.Brush;
using EtoDash = Eto.Drawing.DashStyle;
using EtoFont = Eto.Drawing.Font;
using EtoImage = Eto.Drawing.Image;
using EtoMatrix = Eto.Drawing.IMatrix;
using EtoPen = Eto.Drawing.Pen;
using EtoPointF = Eto.Drawing.PointF;
using EtoRectangleF = Eto.Drawing.RectangleF;
using EtoSizeF = Eto.Drawing.SizeF;
using FillMode = Eto.Drawing.FillMode;
using FontDecoration = Eto.Drawing.FontDecoration;
using FontStyle = Eto.Drawing.FontStyle;
using FormattedTextAlignment = Eto.Drawing.FormattedTextAlignment;
using FormattedTextTrimming = Eto.Drawing.FormattedTextTrimming;
using FormattedTextWrapMode = Eto.Drawing.FormattedTextWrapMode;
using Generator.Equals;
using GradientWrapMode = Eto.Drawing.GradientWrapMode;
using Graphics = Eto.Drawing.Graphics;
using IGraphicsPath = Eto.Drawing.IGraphicsPath;
using PenLineCap = Eto.Drawing.PenLineCap;
using PenLineJoin = Eto.Drawing.PenLineJoin;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using SystemFonts = Eto.Drawing.SystemFonts;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Dash {
    private Dash() { }
    public sealed record Solid : Dash;
    public sealed record Dashed : Dash;
    public sealed record Dot : Dash;
    public sealed record DashDot : Dash;
    public sealed record DashDotDot : Dash;
    public sealed record Pattern(float Offset, Seq<float> Intervals) : Dash;

    public static Fin<Dash> Of(float offset, Seq<float> intervals);
    internal EtoDash Mint();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathSpec {
    private PathSpec() { }
    public sealed record Line(EtoPointF From, EtoPointF To) : PathSpec;
    public sealed record Polyline(Seq<EtoPointF> Points) : PathSpec;
    public sealed record Polygon(Seq<EtoPointF> Points) : PathSpec;
    public sealed record Rectangle(EtoRectangleF Frame) : PathSpec;
    public sealed record RoundedRectangle(EtoRectangleF Frame, float NW, float NE, float SE, float SW) : PathSpec;
    public sealed record Ellipse(EtoRectangleF Frame) : PathSpec;
    public sealed record Arc(EtoRectangleF Frame, VectorAngle Start, VectorAngle Sweep) : PathSpec;
    public sealed record CubicBezier(EtoPointF From, EtoPointF ControlA, EtoPointF ControlB, EtoPointF To) : PathSpec;
    public sealed record Curve(Seq<EtoPointF> Points, UnitInterval Tension) : PathSpec;
    public sealed record Composite(Seq<PathSpec> Figures, bool Connect) : PathSpec;

    public static Fin<PathSpec> Admit(PathSpec spec);
    internal Fin<Lease<IGraphicsPath>> Build(FillMode rule);
    internal Fin<bool> Hits(EtoPointF at, FillMode rule, Option<StrokeSpec> edge);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FillSource {
    private FillSource() { }
    public sealed record Solid(PerceptualColor Colour) : FillSource;
    public sealed record LinearPoints(PerceptualColor From, PerceptualColor To, EtoPointF Start, EtoPointF End, GradientWrapMode Wrap, Option<TransformSpec> Transform) : FillSource;
    public sealed record LinearAngle(EtoRectangleF Frame, PerceptualColor From, PerceptualColor To, VectorAngle Angle, GradientWrapMode Wrap, Option<TransformSpec> Transform) : FillSource;
    public sealed record Radial(PerceptualColor From, PerceptualColor To, EtoPointF Centre, EtoPointF Origin, EtoSizeF Radius, GradientWrapMode Wrap, Option<TransformSpec> Transform) : FillSource;
    [Equatable]
    public sealed partial record Texture([property: ReferenceEquality] EtoImage Source, UnitInterval Opacity, Option<TransformSpec> Transform) : FillSource;

    internal Fin<Lease<EtoBrush>> Mint();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransformSpec {
    private TransformSpec() { }
    public sealed record Translation(float X, float Y) : TransformSpec;
    public sealed record Rotation(VectorAngle Angle) : TransformSpec;
    public sealed record Scale(float X, float Y, EtoPointF Origin) : TransformSpec;
    public sealed record Affine(float XX, float YX, float XY, float YY, float X0, float Y0) : TransformSpec;
    [Equatable]
    public sealed partial record Matrix([property: ReferenceEquality] EtoMatrix Value) : TransformSpec;
    public sealed record Sequence(Seq<TransformSpec> Transforms) : TransformSpec;
    public sealed record Inverse(TransformSpec Transform) : TransformSpec;

    internal Fin<Lease<EtoMatrix>> Mint();
}

[SmartEnum]
public sealed partial class TypeRole {
    public static readonly TypeRole Body = new(resolve: static (size, decoration) => SystemFonts.Default(size: Host(size), decoration: decoration));
    public static readonly TypeRole Strong = new(resolve: static (size, decoration) => SystemFonts.Bold(size: Host(size), decoration: decoration));
    public static readonly TypeRole Caption = new(resolve: static (size, decoration) => SystemFonts.Label(size: Host(size), decoration: decoration));
    public static readonly TypeRole MenuText = new(resolve: static (size, decoration) => SystemFonts.Menu(size: Host(size), decoration: decoration));
    public static readonly TypeRole BarText = new(resolve: static (size, decoration) => SystemFonts.MenuBar(size: Host(size), decoration: decoration));
    public static readonly TypeRole StatusText = new(resolve: static (size, decoration) => SystemFonts.StatusBar(size: Host(size), decoration: decoration));
    public static readonly TypeRole HintText = new(resolve: static (size, decoration) => SystemFonts.ToolTip(size: Host(size), decoration: decoration));
    public static readonly TypeRole TitleText = new(resolve: static (size, decoration) => SystemFonts.TitleBar(size: Host(size), decoration: decoration));
    public static readonly TypeRole MessageText = new(resolve: static (size, decoration) => SystemFonts.Message(size: Host(size), decoration: decoration));
    public static readonly TypeRole PaletteText = new(resolve: static (size, decoration) => SystemFonts.Palette(size: Host(size), decoration: decoration));
    public static readonly TypeRole UserText = new(resolve: static (size, decoration) => SystemFonts.User(size: Host(size), decoration: decoration));

    private static float? Host(Option<PositiveMagnitude> size) =>
        HostEdge.Nullable(size.Map(static magnitude => (float)magnitude.Value));

    [UseDelegateFromConstructor]
    internal partial EtoFont Resolve(Option<PositiveMagnitude> size, FontDecoration decoration);
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct FontFamilyName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "FontFamilyName requires a non-blank family.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TypeSource {
    private TypeSource() { }
    public sealed record Role(TypeRole Value) : TypeSource;
    public sealed record Family(FontFamilyName Name) : TypeSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Mark {
    private Mark() { }
    public sealed record Stroke(PathSpec Path, StrokeSpec Style) : Mark;
    public sealed record Fill(PathSpec Path, FillSource Source, FillMode Rule) : Mark;
    public sealed record Text(TypeFace Face, Option<BlockSpec> Layout, PerceptualColor Colour, EtoPointF Origin, string Value) : Mark;
    public sealed record Glyph(GlyphBlock Block, EtoPointF Origin) : Mark;
    public sealed record Image(EtoImage Source, EtoPointF Origin) : Mark;
    public sealed record ImageRegion(EtoImage Source, EtoRectangleF SourceBounds, EtoRectangleF DestinationBounds) : Mark;
    public sealed record Clip(PathSpec Region, FillMode Rule, Seq<Mark> Children) : Mark;
    public sealed record Transform(TransformSpec Value, Seq<Mark> Children) : Mark;

    internal Fin<Option<EtoRectangleF>> Extent(PaintStock stock, PositiveMagnitude density);
    internal Fin<bool> Hits(EtoPointF at, PaintStock stock, PositiveMagnitude density);
    internal Fin<Dimension> Draw(Graphics target, PaintStock stock);
    internal static void Write(Mark mark, CanonicalWriter writer);
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record StrokeSpec(
    PerceptualColor Colour,
    PositiveMagnitude Width,
    PenLineCap Cap,
    PenLineJoin Join,
    Dash Dash) {
    public static Fin<StrokeSpec> Hairline(PerceptualColor colour, Graphics target);
    internal Fin<Lease<EtoPen>> Mint();
}

[Equatable]
public sealed partial record TypeFace(TypeSource Source, Option<PositiveMagnitude> Size, FontStyle Style, FontDecoration Decoration) {
    public static TypeFace Of(TypeRole role);
    internal Fin<Lease<EtoFont>> Mint();
}

[Equatable]
public sealed partial record BlockSpec(
    FormattedTextWrapMode Wrap,
    FormattedTextTrimming Trim,
    FormattedTextAlignment Align,
    Option<EtoSizeF> Max) {
    public static readonly BlockSpec Default;
}

public sealed class GlyphBlock {
    public static Fin<GlyphBlock> Of(string text, TypeFace face, BlockSpec block, Option<PerceptualColor> ink = default);
    public string Text { get; }
    public TypeFace Face { get; }
    public BlockSpec Block { get; }
    public Option<PerceptualColor> Ink { get; }
    public Fin<EtoSizeF> Measure();
    internal Fin<Unit> Draw(Graphics target, EtoPointF at, PaintStock stock);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintTally(Dimension Marks,
    Dimension Drawn,
    Dimension Culled,
    GaugedSpan<DispatchLane> Span) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: Drawn.Value + Culled.Value, expected: Marks.Value),
        ValidityClaim.Evidence(evidence: Some(Span)));
}

public sealed record PaintProgram {
    private PaintProgram(Seq<Mark> marks, UInt128 identity) => (Marks, Identity) = (marks, identity);

    public static readonly PaintProgram Blank;

    public Seq<Mark> Marks { get; }

    public UInt128 Identity { get; }

    public static Fin<PaintProgram> Of(Seq<Mark> marks);

    public Fin<Option<EtoRectangleF>> Bounds(PaintStock stock, PositiveMagnitude density);

    public Fin<Seq<int>> Hit(EtoPointF at, PaintStock stock, PositiveMagnitude density);

    public Fin<PaintTally> Replay(
        Graphics target, ScenePolicy policy, PaintStock stock, MonotonicTimeline clock, DispatchLane lane);

}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintStock : IDisposable {
    public static Fin<Lease<PaintStock>> Open();

    public Error Faults { get; }

    internal Fin<EtoBrush> Brush(FillSource source);
    internal Fin<EtoPen> Pen(StrokeSpec stroke);
    internal Fin<EtoFont> Face(TypeFace face);

    private Fin<TResource> Seat<TSpec, TResource>(TSpec spec, Func<TSpec, Fin<Lease<TResource>>> mint)
        where TSpec : notnull
        where TResource : class, IDisposable;

    public Fin<Unit> Release();
    public void Dispose();
}

```

## [03]-[SURFACE]

- Owner: `ScenePolicy` the quality tier bracketed around each replay; `Redraw` the invalidation family; `AlphaLayout` the coverage carriage a lock publishes and every egress normalizes against; `SurfaceSpec` the admitted mount request carrying its independent scroll and focus postures directly; `Surface` the mounted `Drawable` with its program cell; `PixelLease` the locked and unlocked bitmap access over both imaging stacks.
- Cases: `Redraw` is `Whole`, `Region`, or `Immediate` — three invalidation modalities, not one with a rectangle knob whose absence means "everything". `AlphaLayout` carries the three carriages a lock can publish — straight, premultiplied, and coverage-less.
- Entry: `Surface.Mount` applies the record gate directly and leases the mounted surface; `Surface.Of` recovers the owner from its realized `Drawable`, so an `Embedded`-or-`Painted` consumer swaps through the one control realization returned and no parallel surface registry exists.
- Auto: the paint handler replays the CURRENT admitted program; a swap publishes for redraw, commits on redraw success, and restores the prior program on failure, so paint truth and hit truth can never disagree about which run is live.
- Auto: a swap whose next program carries the SAME `Identity` skips the invalidation entirely — the run is content-keyed, so a re-derived but identical program costs no frame.
- Law: quality knobs are a `ScenePolicy` VALUE bracketed once per replay and restored on exit, never per-mark toggles; `Use` brackets the transform and clip stack with the quality tuple, so every mounted or printed replay leaves the caller's `Graphics` stream unchanged.
- Law: `Surface` is HOST-AFFINE end to end, so the mount, every invalidation, the off-event acquisition, the composition verbs, and the release cross `UiThread`. `HitTest` is the ONE entry that does not, because it replays the program's hit projection over admitted geometry and reaches no host object; replay needs no crossing either, running inside a host callback that already holds the thread.
- Law: the mount table is weak-KEYED on its `Drawable`, so a surface's lifetime is its control's and a retired surface never outlives the control it mounted. The residue this leaves is stated rather than hidden: a plugin reloaded into a fresh load context keeps a retired surface reachable exactly as long as host chrome still holds the leaked control, and a claim beside the key would widen every mount with an identity it does not need without freeing that control.
- Law: `Acquire` probes `SupportsCreateGraphics` and DEGRADES where the handler refuses — the caller's `Redraw` invalidates and the mounted program paints on the next pass — so a backend answering `false` loses immediacy, never capability; the off-event handle flushes queued commands before the lease disposes it, because an off-event stream is not committed by the paint loop.
- Law: a `Locked` projection answers a VALUE through a `scoped in` window. A `struct` constraint alone does NOT foreclose escape — a struct tuple carrying the window satisfies it and hands the live lock straight out — so the window crosses as a `scoped in` parameter on a named delegate, which is what the compiler actually proves. RESIDUAL: a projection may still copy a raw pointer out of the window's own address, which no CLR constraint forecloses; past that line the declared layout and the lock's extent are the whole contract.
- Law: the imaging STACK is the argument's own type. Every egress carries a toolkit arity and a GDI arity discriminated by the bitmap it takes — Rhino's capture surface, its z-buffer read, and its render window answer `System.Drawing` bitmaps while every Eto surface answers toolkit ones — so a name-suffixed `OfGdi` family never exists and neither does a conversion between the two. The Eto arity stays PRIMARY: it is the stack the mounted surface, the print page, and the asset resolve all reach, and the GDI arity exists for the host contracts that publish no other shape.
- Law: `Locked` reads and `WriteLocked` writes, so the lock MODE is the member rather than an argument — a caller cannot ask for a read window and mutate through it, and the GDI arity's `ImageLockMode` is decided here rather than threaded.
- Law: the coverage carriage is READ off the lock, never assumed per backend. `BitmapData.PremultipliedAlpha` is set per lock from the live representation — the macOS handler computes it as the representation's alpha presence minus its non-premultiplied format flag — so a raster minted `Format32bppRgba` and a raster decoded from a straight-alpha file answer differently on the SAME backend, and a page pinning one carriage per host would be wrong on half its own bitmaps. `AlphaLayout` is that read, and the GDI arity's `PixelFormat` is the same fact under the other stack's spelling.
- Law: `Bytes` answers ONE declared carriage — `AlphaLayout.Straight`, straight BGRA at four bytes per pixel, rows tightly packed top-down — and normalizing to it is this member's own hop. STRAIGHT is FORCED rather than preferred: `TranslateDataToArgb` is the only channel-order-canonical member the toolkit stack publishes and it already divides coverage back out, so premultiplied rows would cost a divide-then-multiply round trip quantizing every low-coverage texel, and no channel-order column exists to read raw words against instead. The GDI arity pays none of it, because `LockBits` takes the format and GDI+ converts inside the lock. `AssetRaster.Pixels` carries exactly these rows.
- Law: a stride is NOT a width. Both stacks publish a padded row pitch — `ScanWidth` and `Stride` — and both can present rows bottom-up, which `BitmapData.Flipped` names on the toolkit stack and a negative `Stride` on the GDI one; `Bytes` repacks against both, which is the whole distance between the declared layout and the platform's.
- Law: the lock's FORMAT is the host's own asymmetry, named rather than hidden. `Bitmap.Lock()` admits no format, so the toolkit window hands its read carriage beside itself and the caller reads what the representation holds; `LockBits` admits one, so the GDI window is ASKED for a carriage and GDI+ converts on the way in. Single-pixel reads and writes compose `GetPixel` and `SetPixel` with `PaintColor` at their bitmap boundary; `WriteLocked` asks for no format because the handler's translate pair already speaks straight colour.
- Law: `Clone` hands back a leased bitmap, because a clone is a fresh host raster under caller custody and a bare return is the leak the page's own lease law forecloses.
- Law: IME composition rides the host verbs and a text-editing overlay ignoring composition state is the named defect; `CancelComposition` and `CommitComposition` are that pair.
- Output: `Replay` answers the `PaintTally`; the surface callback captures the replay through `Try.lift(...).Run().Match(...)` and parks failures under its own `HookId` on the spec's `FaultCell`. A paint that refuses never throws through the host callback, and the cell bounds what a repainting storm can accumulate where a `void` reporter bounded nothing.
- Packages: Eto.Forms for `Drawable` and the marshal; Eto.Drawing for `Graphics`, `Bitmap`, and the pixel window; `System.Drawing.Common` for the GDI bitmap, its locked window, and its pixel-format roster (all prelude-aliased); LanguageExt.Core for `Atom`, `Lease`, and the packed `Arr<byte>` rows; `Domain/hooks` for the bounded `FaultCell` a surface parks its paint faults on.
- Growth: a new invalidation modality is one `Redraw` case; a new quality tier is one `ScenePolicy` row; a new coverage carriage is one `AlphaLayout` row carrying the GDI format that demands it, breaking every normalize arm loudly; a new pixel egress is one `PixelLease` member per stack, both landing together because a shape one stack answers and the other refuses is a hole a consumer discovers at its own boundary.
- Boundary: frame pacing, display-link cadence, and animation clocks belong to `Parametric/projections` and `Interaction/clock` — this surface exposes swap-and-invalidate and nothing temporal. HOST-SPECIFIC-STAYS: Grasshopper paints into the Grasshopper2 canvas through its own `PaintPhase` hooks and hands a `Graphics` to `PaintProgram.Replay` rather than mounting a surface at all.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoBitmap = Eto.Drawing.Bitmap;
using EtoPixels = Eto.Drawing.BitmapData;
using EtoPointF = Eto.Drawing.PointF;
using EtoRectangle = Eto.Drawing.Rectangle;
using EtoPoint = Eto.Drawing.Point;
using GdiBitmap = System.Drawing.Bitmap;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;
using GdiPixels = System.Drawing.Imaging.BitmapData;
using GdiPoint = System.Drawing.Point;
using GdiRectangle = System.Drawing.Rectangle;
using Graphics = Eto.Drawing.Graphics;
using ImageInterpolation = Eto.Drawing.ImageInterpolation;
using PixelOffsetMode = Eto.Drawing.PixelOffsetMode;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ScenePolicy {
    public static readonly ScenePolicy Crisp = new(antiAlias: false, interpolation: ImageInterpolation.None, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Balanced = new(antiAlias: true, interpolation: ImageInterpolation.Default, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Fidelity = new(antiAlias: true, interpolation: ImageInterpolation.High, offset: PixelOffsetMode.Half);

    internal bool AntiAlias { get; }
    internal ImageInterpolation Interpolation { get; }
    internal PixelOffsetMode Offset { get; }

    internal TResult Use<TResult>(Graphics target, Func<TResult> body);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Redraw {
    private Redraw() { }
    public sealed record Whole : Redraw;
    public sealed record Region(EtoRectangle Bounds) : Redraw;
    public sealed record Immediate(EtoRectangle Bounds) : Redraw;

    internal Unit Apply(Drawable host);
}

[SmartEnum]
public sealed partial class AlphaLayout {
    public static readonly AlphaLayout Straight = new(channels: 4, gdi: GdiPixelFormat.Format32bppArgb);
    public static readonly AlphaLayout Premultiplied = new(channels: 4, gdi: GdiPixelFormat.Format32bppPArgb);
    public static readonly AlphaLayout Opaque = new(channels: 3, gdi: GdiPixelFormat.Format24bppRgb);

    internal int Channels { get; }
    internal GdiPixelFormat Gdi { get; }

    public static AlphaLayout OfHost(EtoPixels window);
    public static AlphaLayout OfHost(GdiPixels window);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SurfaceSpec(
    PaintProgram Initial,
    ScenePolicy Policy,
    bool Scrollable,
    bool Focusable,
    FaultCell Faults);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Surface : IDisposable {
    private static readonly HookId Point = HookId.Create(value: "rasm.kernel.interaction.paint");

    public static Fin<Lease<Surface>> Mount(SurfaceSpec spec);

    public static Option<Surface> Of(Drawable host);

    public Drawable Host { get; }
    public PaintProgram Program { get; }

    public Fin<Unit> Swap(Func<PaintProgram, PaintProgram> next, Redraw redraw);

    public Fin<Seq<int>> HitTest(EtoPointF at);

    public Fin<Option<TResult>> Acquire<TResult>(
        Func<Graphics, Fin<TResult>> draw, Redraw fallback);

    public Fin<Unit> CancelComposition();
    public Fin<Unit> CommitComposition();

    public void Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PixelLease {
    public delegate TResult PixelRead<out TResult>(scoped in EtoPixels window, AlphaLayout layout);
    public delegate TResult GdiRead<out TResult>(scoped in GdiPixels window);

    public static Fin<TResult> Locked<TResult>(EtoBitmap bitmap, PixelRead<TResult> read)
        where TResult : struct;

    public static Fin<TResult> Locked<TResult>(GdiBitmap bitmap, AlphaLayout layout, GdiRead<TResult> read)
        where TResult : struct;

    public static Fin<Unit> WriteLocked(EtoBitmap bitmap, Seq<(EtoPoint At, PerceptualColor Colour)> pixels);
    public static Fin<Unit> WriteLocked(GdiBitmap bitmap, Seq<(GdiPoint At, PerceptualColor Colour)> pixels);

    public static Fin<Arr<byte>> Bytes(EtoBitmap bitmap, Option<EtoRectangle> region = default);
    public static Fin<Arr<byte>> Bytes(GdiBitmap bitmap, Option<GdiRectangle> region = default);

    public static Fin<Lease<EtoBitmap>> Clone(EtoBitmap bitmap, Option<EtoRectangle> region = default);
    public static Fin<Lease<GdiBitmap>> Clone(GdiBitmap bitmap, Option<GdiRectangle> region = default);
}
```

## [04]-[COLOR]

- Owner: `ChromeRole` the OS palette read; `PaintColor` the ONE `PerceptualColor` ↔ `Eto.Drawing.Color` correspondence, carried as an extension block so the numeric floor stays Eto-free.
- Cases: `ChromeRole` carries the ten `SystemColors` rows the host resolves — control, control background, control text, disabled text, highlight, highlight text, selection, selection text, window background, and link text.
- Law: the correspondence is `Numerics/atoms`' own, extended here rather than re-spelled. The host egress REFUSES an out-of-display colour where the federation byte leg clips, because a paint instruction that clipped silently hands a painter a colour no consumer can attribute; the ingress admits through the packed ARGB word the numeric floor already owns.
- Law: an OS swatch is READ, never captured. `SystemColors` re-resolves on an appearance flip, so a stored value stales at the flip and a literal beside a native panel diverges on every accent or contrast change — `ChromeRole.Sample` re-reads on each call and that is the whole reason it is a row rather than a table.
- Law: a host-colour-to-host-colour blend does not exist here. Both boundaries carried one; a pair of host colours interpolates by admitting both into `PerceptualColor`, calling `PerceptualColor.Mix` along a `BlendPath`, and quantizing once, with no second member on this owner.
- Auto: alpha rides the correspondence in both directions — the host struct's unit-ranged alpha admits through `UnitInterval` and the egress quantizes it with the triple, so no consumer multiplies a channel by 255 at a call site.
- Packages: Eto.Drawing for `Color` and `SystemColors` (prelude-aliased); `Numerics/atoms` for the perceptual owner and its gamut policies.
- Growth: a new OS swatch is one `ChromeRole` row; a new reproducibility domain is a `GamutPolicy` row on the numeric floor, never a second egress here.
- Boundary: Grasshopper's DisplayP3 `CGColor` mint STAYS at that boundary, reading the kernel triple; Rhino's `ThemePalette.Detach` swatch feeder STAYS at its boundary and hands a `ThemeShift.Hosted` to the theme grid.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using EtoColor = Eto.Drawing.Color;
using Rasm.Domain;
using Rasm.Numerics;
using SystemColors = Eto.Drawing.SystemColors;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ChromeRole {
    public static readonly ChromeRole Control = new(sample: static () => PerceptualColor.OfHost(SystemColors.Control));
    public static readonly ChromeRole ControlBack = new(sample: static () => PerceptualColor.OfHost(SystemColors.ControlBackground));
    public static readonly ChromeRole ControlText = new(sample: static () => PerceptualColor.OfHost(SystemColors.ControlText));
    public static readonly ChromeRole DisabledText = new(sample: static () => PerceptualColor.OfHost(SystemColors.DisabledText));
    public static readonly ChromeRole Highlight = new(sample: static () => PerceptualColor.OfHost(SystemColors.Highlight));
    public static readonly ChromeRole HighlightText = new(sample: static () => PerceptualColor.OfHost(SystemColors.HighlightText));
    public static readonly ChromeRole Selection = new(sample: static () => PerceptualColor.OfHost(SystemColors.Selection));
    public static readonly ChromeRole SelectionText = new(sample: static () => PerceptualColor.OfHost(SystemColors.SelectionText));
    public static readonly ChromeRole WindowBack = new(sample: static () => PerceptualColor.OfHost(SystemColors.WindowBackground));
    public static readonly ChromeRole LinkText = new(sample: static () => PerceptualColor.OfHost(SystemColors.LinkText));

    [UseDelegateFromConstructor]
    public partial Fin<PerceptualColor> Sample();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PaintColor {
    extension(PerceptualColor colour) {
        public static Fin<PerceptualColor> OfHost(EtoColor host);
        public Fin<EtoColor> ToEto(Option<GamutPolicy> gamut = default);
    }
}
```

## [05]-[THEME]

- Owner: `ThemeVariant`, `PaletteRole`, `SpaceRole`, and `TypeSlot` the four vocabularies, the last three ranked so every frozen axis reads by index; `ContrastFloor` the published readability floors and `ContrastRule` one role pair against one floor; `ThemeProgram` the generator over the whole cross-product; `ThemeSnapshot` the published immutable grid; `ThemeShift` the transition family; `ThemeChange` the settled transition; `ThemeGrid` the frozen state every consumer reads.
- Cases: `ThemeVariant` carries light, dark, and high-contrast; `PaletteRole` twelve semantic paint roles carrying no colour; `ThemeShift` is `Generated` — select a variant off the frozen grid — or `Hosted` — merge live host swatches over that variant's row, which is how a generated palette and an OS theme meet in one owner.
- Entry: `ThemeGrid.Freeze` admits the generator, the initial variant, and every contrast rule through one applicative fold so every absence reports together; `Swap` dispatches the shift directly through `ThemeShift.Switch`; `Current` publishes the snapshot.
- Auto: the generator derives EVERY cell from the cross-product of the generated roster items, so a missing-cell fallback cannot exist and a new role is a compile break at the generator rather than a runtime default.
- Auto: a `Hosted` merge re-enters the same contrast gate `Freeze` runs, so an ingested swatch breaching a floor rejects without touching the grid — the refusal is the caller's answer alone and never becomes state a later reader inherits.
- Law: the grid has THREE axes, not one. Colour, spacing, and typography are the three scales a chrome surface reads together, and a boundary carrying a colour grid beside hand-picked pixel insets and hand-picked font sizes is exactly how the same nominal theme rendered at two spacings in two panels. Each axis is keyed by variant because a contrast variant legitimately widens a hairline and enlarges a caption.
- Law: two of the three axes DERIVE from one root each. A spacing row carries its multiple of the variant's base step and a type slot carries its rung off `Drawing/sheet`'s ISO 3098-1 lettering ladder, so the generator states one inset and one face per variant and the twenty-one spacing cells and twenty-four type cells fall out — a per-cell function is the same hand-picking one layer up, and both rosters now pass the provenance test their two siblings already passed against `SystemColors` and `SystemFonts`.
- Law: a contrast floor is a PUBLISHED clause, never a bare ratio. Each row names the WCAG success criterion it transcribes and carries the `PositiveMagnitude` `PerceptualColor.ToneFor` takes, so a rule feeds the kernel's own tonal solve and a reviewer reads which rule a refusal enforced. NAMED LOSS: a floor no standard publishes has no spelling — an unaudited ratio is a policy scalar wearing a type.
- Law: spacing is `PositiveMagnitude` and typography is `TypeFace`, so a theme cell is the kernel's own owner rather than a raw double or a host font — a consumer reading a spacing step gets a guarded magnitude and a consumer reading a slot gets a face the paint vocabulary already knows how to mint.
- Law: a content-identical shift emits an EMPTY changed set and holds the generation, so a rebroadcast triggered by an unchanged theme costs no relayout — `ThemeChange` is the discriminant, not a caller-side comparison.
- Law: the generation is a `MonotonicStamp` off the grid's own timeline, never a hand-kept counter. A rebroadcast and the paint pass it triggers then order against ONE clock through `MonotonicTimeline.Order`, where two counters below the app root are exactly the pair the kernel timeline exists to delete.
- Law: every frozen axis is `Rank`-INDEXED and its fill is proved once at `Freeze`, so a snapshot read is total by construction. A hash-map indexer raises out of a value this page calls frozen the first time a generator misses a row, which is exception-style control flow inside the one owner that publishes immutability.
- Law: this owner publishes and NEVER registers. Style registration, control tracking, and rebroadcast are the injection point on `Interaction/platform#[04]-[PLATFORM]`, because they are host-registry state and this is a frozen value.
- Output: `ThemeChange` carries only the accepted generation, variant, and changed-role set; host restyle warnings stay outside accepted grid state on the platform writer channel.
- Packages: LanguageExt.Core for `Arr`, `HashMap`, `Validation`, and the types; Thinktecture.Runtime.Extensions for the rows and the shift union; `Numerics/atoms` for the colour and magnitude owners; `Drawing/sheet` for the `TextHeight` ladder the type slots stand on; `Parametric/projections` for the timeline the generation is stamped by.
- Growth: a new role is one row plus the generator arm the compile break demands; a new spacing step is one row carrying its multiple and a new type slot one row carrying its rung, neither touching the generator; a new published floor is one `ContrastFloor` row; another transition modality is one `ThemeShift` case with every consumer's dispatch loudly broken.
- Boundary: the shift arrives INJECTED — variant polarity and any live host swatches are read at the boundary that owns the OS theme, and this owner never reads a host theme global.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ThemeVariant {
    public static readonly ThemeVariant Light = new();
    public static readonly ThemeVariant Dark = new();
    public static readonly ThemeVariant Contrast = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteRole {
    public static readonly PaletteRole Canvas = new(key: "canvas", rank: 0);
    public static readonly PaletteRole Panel = new(key: "panel", rank: 1);
    public static readonly PaletteRole Accent = new(key: "accent", rank: 2);
    public static readonly PaletteRole Stroke = new(key: "stroke", rank: 3);
    public static readonly PaletteRole GlyphPrimary = new(key: "glyph-primary", rank: 4);
    public static readonly PaletteRole GlyphMuted = new(key: "glyph-muted", rank: 5);
    public static readonly PaletteRole Focus = new(key: "focus", rank: 6);
    public static readonly PaletteRole Selection = new(key: "selection", rank: 7);
    public static readonly PaletteRole Hover = new(key: "hover", rank: 8);
    public static readonly PaletteRole Success = new(key: "success", rank: 9);
    public static readonly PaletteRole Warning = new(key: "warning", rank: 10);
    public static readonly PaletteRole Failure = new(key: "failure", rank: 11);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpaceRole {
    public static readonly SpaceRole Hairline = new(key: "hairline", rank: 0, step: 0.125d);
    public static readonly SpaceRole Tight = new(key: "tight", rank: 1, step: 0.25d);
    public static readonly SpaceRole Snug = new(key: "snug", rank: 2, step: 0.5d);
    public static readonly SpaceRole Base = new(key: "base", rank: 3, step: 1.0d);
    public static readonly SpaceRole Wide = new(key: "wide", rank: 4, step: 1.5d);
    public static readonly SpaceRole Loose = new(key: "loose", rank: 5, step: 2.0d);
    public static readonly SpaceRole Section = new(key: "section", rank: 6, step: 4.0d);

    public int Rank { get; }
    public double Step { get; }

    internal Fin<PositiveMagnitude> Of(PositiveMagnitude root) =>
        FactoryBridge.Accept<PositiveMagnitude>(candidate: root.Value * Step);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TypeSlot {
    public static readonly TypeSlot Body = new(key: "body", rank: 0, rung: TextHeight.H35);
    public static readonly TypeSlot Strong = new(key: "strong", rank: 1, rung: TextHeight.H35);
    public static readonly TypeSlot Caption = new(key: "caption", rank: 2, rung: TextHeight.H25);
    public static readonly TypeSlot Title = new(key: "title", rank: 3, rung: TextHeight.H50);
    public static readonly TypeSlot Chrome = new(key: "chrome", rank: 4, rung: TextHeight.H35);
    public static readonly TypeSlot Status = new(key: "status", rank: 5, rung: TextHeight.H25);
    public static readonly TypeSlot Hint = new(key: "hint", rank: 6, rung: TextHeight.H25);
    public static readonly TypeSlot Numeric = new(key: "numeric", rank: 7, rung: TextHeight.H35);

    public int Rank { get; }
    public TextHeight Rung { get; }

    internal Fin<TypeFace> Of(TypeFace root) =>
        from seat in root.Size.ToFin(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(TypeFace.Size)), Reason: RejectReason.RootFaceSize))
        from size in FactoryBridge.Accept<PositiveMagnitude>(candidate: seat.Value * (Rung.Height / Body.Rung.Height))
        select root with { Size = Some(size) };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ThemeShift {
    private ThemeShift() { }
    public sealed record Generated(ThemeVariant Variant) : ThemeShift;
    public sealed record Hosted(ThemeVariant Variant, HashMap<PaletteRole, PerceptualColor> Cells) : ThemeShift;
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContrastFloor {
    public static readonly ContrastFloor AaText = new(key: "aa-text",
        ratio: PositiveMagnitude.Create(value: 4.5), clause: "WCAG 2.2 SC 1.4.3");
    public static readonly ContrastFloor AaLarge = new(key: "aa-large",
        ratio: PositiveMagnitude.Create(value: 3.0), clause: "WCAG 2.2 SC 1.4.3");
    public static readonly ContrastFloor AaaText = new(key: "aaa-text",
        ratio: PositiveMagnitude.Create(value: 7.0), clause: "WCAG 2.2 SC 1.4.6");
    public static readonly ContrastFloor AaaLarge = new(key: "aaa-large",
        ratio: PositiveMagnitude.Create(value: 4.5), clause: "WCAG 2.2 SC 1.4.6");
    public static readonly ContrastFloor NonText = new(key: "non-text",
        ratio: PositiveMagnitude.Create(value: 3.0), clause: "WCAG 2.2 SC 1.4.11");

    public PositiveMagnitude Ratio { get; }
    public string Clause { get; }
}

public sealed record ContrastRule(PaletteRole Foreground, PaletteRole Background, ContrastFloor Floor);

public sealed record ThemeProgram(
    Func<PaletteRole, ThemeVariant, PerceptualColor> Paint,
    Func<ThemeVariant, PositiveMagnitude> Base,
    Func<ThemeVariant, TypeFace> Root) {
    internal Validation<Error, ThemeSnapshot> Cells(ThemeVariant variant, MonotonicStamp generation);
}

public sealed record ThemeSnapshot(
    MonotonicStamp Generation,
    ThemeVariant Variant,
    Arr<PerceptualColor> Cells,
    Arr<PositiveMagnitude> Spacing,
    Arr<TypeFace> Faces) {
    public PerceptualColor this[PaletteRole role] => Cells[role.Rank];
    public PositiveMagnitude this[SpaceRole step] => Spacing[step.Rank];
    public TypeFace this[TypeSlot slot] => Faces[slot.Rank];
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ThemeChange(
    MonotonicStamp Generation,
    ThemeVariant Variant,
    Seq<PaletteRole> Changed) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(Optional(Generation)),
        ValidityClaim.Evidence(Optional(Variant)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ThemeGrid {
    public static Validation<Error, ThemeGrid> Freeze(
        ThemeProgram program, ThemeVariant initial, Seq<ContrastRule> contrast, MonotonicTimeline clock);

    public ThemeSnapshot Current { get; }

    public Fin<ThemeChange> Swap(ThemeShift shift);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
