# 1. Use canonical dash case names

From:
`[02]-[MARK] — Dash cases, lines 72–77`
```csharp
public sealed record SolidCase : Dash;
public sealed record DashedCase : Dash;
public sealed record DottedCase : Dash;
public sealed record DashDotCase : Dash;
public sealed record DashDotDotCase : Dash;
public sealed record PatternedCase(float Offset, Seq<float> Intervals) : Dash;
```

To:
```csharp
public sealed record Solid : Dash;
public sealed record Dash : Dash;
public sealed record Dot : Dash;
public sealed record DashDot : Dash;
public sealed record DashDotDot : Dash;
public sealed record Pattern(float Offset, Seq<float> Intervals) : Dash;
```

Why: `Case` is a structural suffix; Eto already supplies the canonical dash names.

Change: Rename the generated cases without changing their payloads or dispatch.

Delta: 0 LOC; 0 types; 0 members.

Ripples: Rename `Dash.PatternedCase` and its member anchor in `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`.

# 2. Use canonical path case names

From:
`[02]-[MARK] — PathSpec cases, lines 86–95`
```csharp
public sealed record LineCase(EtoPointF From, EtoPointF To) : PathSpec;
public sealed record PolylineCase(Seq<EtoPointF> Points) : PathSpec;
public sealed record PolygonCase(Seq<EtoPointF> Points) : PathSpec;
public sealed record RectCase(EtoRectangleF Frame) : PathSpec;
public sealed record RoundRectCase(EtoRectangleF Frame, float NW, float NE, float SE, float SW) : PathSpec;
public sealed record EllipseCase(EtoRectangleF Frame) : PathSpec;
public sealed record ArcCase(EtoRectangleF Frame, VectorAngle Start, VectorAngle Sweep) : PathSpec;
public sealed record BezierCase(EtoPointF From, EtoPointF ControlA, EtoPointF ControlB, EtoPointF To) : PathSpec;
public sealed record CurveCase(Seq<EtoPointF> Points, UnitInterval Tension) : PathSpec;
public sealed record CompositeCase(Seq<PathSpec> Figures, bool Connect) : PathSpec;
```

To:
```csharp
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
```

Why: Geometry names should identify the represented figure; the suffix and abbreviations add no domain meaning.

Change: Rename the cases to established geometry terms.

Delta: 0 LOC; 0 types; 0 members.

Ripples: Update construction and dispatch in `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/layout.md`, and `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/motion.md`.

# 3. Derive bounds from the built path

From:
`[02]-[MARK] — PathSpec geometry members, line 99`
```csharp
internal Option<EtoRectangleF> Extent { get; }
```

To:
```csharp
// PathSpec.Extent DELETED
```

Why: `IGraphicsPath.Bounds` already owns the admitted figure's bounds and cannot diverge from drawing and hit testing.

Change: Bracket `PathSpec.Build` in the mark-bounds fold and read `Bounds`.

Delta: -1 LOC; 0 types; -1 member.

Ripples: Use built-path bounds in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/paint.md`.

# 4. Model affine transforms with one mint operation

From:
`[02]-[MARK] — PosePlan declaration, lines 116–130`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PosePlan {
    private PosePlan() { }
    public sealed record ShiftCase(float Dx, float Dy) : PosePlan;
    public sealed record SpinCase(VectorAngle Angle) : PosePlan;
    public sealed record StretchCase(float Sx, float Sy, EtoPointF About) : PosePlan;
    public sealed record AffineCase(float XX, float YX, float XY, float YY, float X0, float Y0) : PosePlan;
    [Equatable]
    public sealed partial record MatrixCase([property: ReferenceEquality] EtoMatrix Matrix) : PosePlan;
    public sealed record StackedCase(Seq<PosePlan> Poses) : PosePlan;
    public sealed record InvertedCase(PosePlan Body) : PosePlan;

    internal Fin<Lease<EtoMatrix>> Mint();
    internal Fin<Lease<EtoMatrix>> Inverse();
}
```

To:
```csharp
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
```

Why: Translation, rotation, scale, composition, and inversion are affine transforms; `Inverse()` duplicates the `Inverse` case's mint path.

Change: Rename the owner and cases and mint inversion through generated dispatch.

Delta: -1 LOC; 0 net types; -1 member.

Ripples: Replace `PosePlan` in `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/paint.md`, and `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/layout.md`.

# 5. Keep fill transforms as specifications

From:
`[02]-[MARK] — FillSource cases, lines 106–111`
```csharp
public sealed record SolidCase(PerceptualColor Colour) : FillSource;
public sealed record LinearCase(PerceptualColor From, PerceptualColor To, EtoPointF Start, EtoPointF End, GradientWrapMode Wrap, Option<EtoMatrix> Warp) : FillSource;
public sealed record SheetCase(EtoRectangleF Frame, PerceptualColor From, PerceptualColor To, VectorAngle Angle, GradientWrapMode Wrap, Option<EtoMatrix> Warp) : FillSource;
public sealed record RadialCase(PerceptualColor From, PerceptualColor To, EtoPointF Centre, EtoPointF Origin, EtoSizeF Radius, GradientWrapMode Wrap, Option<EtoMatrix> Warp) : FillSource;
[Equatable]
public sealed partial record TextureCase([property: ReferenceEquality] EtoImage Source, UnitInterval Opacity, Option<EtoMatrix> Warp) : FillSource;
```

To:
```csharp
public sealed record Solid(PerceptualColor Colour) : FillSource;
public sealed record LinearPoints(PerceptualColor From, PerceptualColor To, EtoPointF Start, EtoPointF End, GradientWrapMode Wrap, Option<TransformSpec> Transform) : FillSource;
public sealed record LinearAngle(EtoRectangleF Frame, PerceptualColor From, PerceptualColor To, VectorAngle Angle, GradientWrapMode Wrap, Option<TransformSpec> Transform) : FillSource;
public sealed record Radial(PerceptualColor From, PerceptualColor To, EtoPointF Centre, EtoPointF Origin, EtoSizeF Radius, GradientWrapMode Wrap, Option<TransformSpec> Transform) : FillSource;
[Equatable]
public sealed partial record Texture([property: ReferenceEquality] EtoImage Source, UnitInterval Opacity, Option<TransformSpec> Transform) : FillSource;
```

Why: A paint specification should carry the affine value rather than an already-minted host handle, and the two linear constructors need distinct input names.

Change: Mint the optional host matrix inside `FillSource.Mint` under the brush lease.

Delta: 0 LOC; 0 types; 0 members; three host-handle fields replaced by value specifications.

Ripples: Update fill construction and dispatch in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/paint.md` and `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`.

# 6. Remove process-local type-role keys

From:
`[02]-[MARK] — TypeRole declaration head, lines 132–144`
```csharp
[SmartEnum<int>]
public sealed partial class TypeRole {
    public static readonly TypeRole Body = new(key: 0, resolve: static (size, decoration) => SystemFonts.Default(size: Host(size), decoration: decoration));
    public static readonly TypeRole Strong = new(key: 1, resolve: static (size, decoration) => SystemFonts.Bold(size: Host(size), decoration: decoration));
    public static readonly TypeRole Caption = new(key: 2, resolve: static (size, decoration) => SystemFonts.Label(size: Host(size), decoration: decoration));
    public static readonly TypeRole MenuText = new(key: 3, resolve: static (size, decoration) => SystemFonts.Menu(size: Host(size), decoration: decoration));
    public static readonly TypeRole BarText = new(key: 4, resolve: static (size, decoration) => SystemFonts.MenuBar(size: Host(size), decoration: decoration));
    public static readonly TypeRole StatusText = new(key: 5, resolve: static (size, decoration) => SystemFonts.StatusBar(size: Host(size), decoration: decoration));
    public static readonly TypeRole HintText = new(key: 6, resolve: static (size, decoration) => SystemFonts.ToolTip(size: Host(size), decoration: decoration));
    public static readonly TypeRole TitleText = new(key: 7, resolve: static (size, decoration) => SystemFonts.TitleBar(size: Host(size), decoration: decoration));
    public static readonly TypeRole MessageText = new(key: 8, resolve: static (size, decoration) => SystemFonts.Message(size: Host(size), decoration: decoration));
    public static readonly TypeRole PaletteText = new(key: 9, resolve: static (size, decoration) => SystemFonts.Palette(size: Host(size), decoration: decoration));
    public static readonly TypeRole UserText = new(key: 10, resolve: static (size, decoration) => SystemFonts.User(size: Host(size), decoration: decoration));
```

To:
```csharp
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
```

Why: No wire, host, or persisted lookup consumes the hand-numbered key.

Change: Use a keyless Thinktecture smart enum.

Delta: 0 source LOC; 0 declared types or members; generated key and keyed lookup surfaces removed.

# 7. Remove structural suffixes from type-source cases

From:
`[02]-[MARK] — TypeSource cases, lines 166–167`
```csharp
public sealed record RoleCase(TypeRole Role) : TypeSource;
public sealed record FamilyCase(FontFamilyName Family) : TypeSource;
```

To:
```csharp
public sealed record Role(TypeRole Value) : TypeSource;
public sealed record Family(FontFamilyName Name) : TypeSource;
```

Why: The alternatives are a system role and a font family; `Case` adds no distinction.

Change: Rename the cases and their single payloads.

Delta: 0 LOC; 0 types; 0 members.

Ripples: Update `TypeSource.FamilyCase` matching in `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`.

# 8. Name marks by their paint operations

From:
`[02]-[MARK] — Mark cases, lines 173–180`
```csharp
public sealed record StrokeCase(PathSpec Path, StrokeSpec Stroke) : Mark;
public sealed record FillCase(PathSpec Path, FillSource Fill, FillMode Rule) : Mark;
public sealed record TextCase(TypeFace Face, Option<BlockSpec> Block, PerceptualColor Ink, EtoPointF At, string Text) : Mark;
public sealed record GlyphCase(GlyphBlock Block, EtoPointF At) : Mark;
public sealed record ImageCase(EtoImage Source, EtoPointF At) : Mark;
public sealed record PaneCase(EtoImage Source, EtoRectangleF From, EtoRectangleF To) : Mark;
public sealed record ClipCase(PathSpec Region, FillMode Rule, Seq<Mark> Children) : Mark;
public sealed record PoseCase(PosePlan Pose, Seq<Mark> Children) : Mark;
```

To:
```csharp
public sealed record Stroke(PathSpec Path, StrokeSpec Style) : Mark;
public sealed record Fill(PathSpec Path, FillSource Source, FillMode Rule) : Mark;
public sealed record Text(TypeFace Face, Option<BlockSpec> Layout, PerceptualColor Colour, EtoPointF Origin, string Value) : Mark;
public sealed record Glyph(GlyphBlock Block, EtoPointF Origin) : Mark;
public sealed record Image(EtoImage Source, EtoPointF Origin) : Mark;
public sealed record ImageRegion(EtoImage Source, EtoRectangleF SourceBounds, EtoRectangleF DestinationBounds) : Mark;
public sealed record Clip(PathSpec Region, FillMode Rule, Seq<Mark> Children) : Mark;
public sealed record Transform(TransformSpec Value, Seq<Mark> Children) : Mark;
```

Why: `Pane`, `From`, `To`, and `Pose` obscure the concrete draw operation and rectangle direction.

Change: Rename cases and payloads to their paint semantics.

Delta: 0 LOC; 0 types; 0 members.

Ripples: Update construction and dispatch in `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/layout.md`, and `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/paint.md`.

# 9. Read hairline density from a borrowed graphics value

From:
`[02]-[MARK] — StrokeSpec.Hairline, line 196`
```csharp
public static Fin<StrokeSpec> Hairline(PerceptualColor colour, Lease<Graphics> target);
```

To:
```csharp
public static Fin<StrokeSpec> Hairline(PerceptualColor colour, Graphics target);
```

Why: `Hairline` only reads device density; it neither acquires nor releases the live context.

Change: Call it inside the caller's graphics lease bracket.

Delta: 0 LOC; 0 types; 0 members; one wrapper removed from the signature.

Ripples: Update hairline construction in `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`.

# 10. Keep graphics custody at the caller boundary

From:
`[02]-[MARK] — PaintProgram.Replay signature, lines 250–251`
```csharp
public Fin<PaintTally> Replay(
    Lease<Graphics> target, ScenePolicy policy, PaintStock stock, MonotonicTimeline clock, DispatchLane lane);
```

To:
```csharp
public Fin<PaintTally> Replay(
    Graphics target, ScenePolicy policy, PaintStock stock, MonotonicTimeline clock, DispatchLane lane);
```

Why: Replay consumes a borrowed graphics stream and must not receive the caller's ownership wrapper.

Change: Invoke replay inside the owning lease bracket.

Delta: 0 LOC; 0 types; 0 members; one wrapper removed from the public signature.

Ripples: Bracket replay in `libs/dotnet/Rasm.Rhino/.planning/Display/draw.md`; pass event graphics directly in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/paint.md`.

# 11. Inline the program writer

From:
`[02]-[MARK] — PaintProgram.Write, lines 253–254`
```csharp
internal static void Write(PaintProgram program, CanonicalWriter writer) =>
    writer.Rows(rows: program.Marks, field: Mark.Write);
```

To:
```csharp
// PaintProgram.Write DELETED
```

Why: The member forwards one writer call used only by `PaintProgram.Of`.

Change: Call `writer.Rows(marks, Mark.Write)` directly while computing identity.

Delta: -2 LOC; 0 types; -1 member.

# 12. Accumulate stock faults in Error

From:
`[02]-[MARK] — PaintStock.Faults, line 261`
```csharp
public Seq<Error> Faults { get; }
```

To:
```csharp
public Error Faults { get; }
```

Why: `Error` already represents zero, one, or many failures and combines them with `+`; a parallel sequence needs another collapse.

Change: Seed with `Errors.None`, append disposal failures with `+`, and return the error from `Release`.

Delta: 0 LOC; 0 types; 0 members; one redundant collection carrier removed.

# 13. Delete the interpolation wrapper family

From:
`[02]-[MARK] — Tween declaration, lines 275–283`
```csharp
public static class Tween {
    public static float Between(float from, float to, UnitInterval at);
    public static double Between(double from, double to, UnitInterval at);
    public static EtoPointF Between(EtoPointF from, EtoPointF to, UnitInterval at);
    public static EtoSizeF Between(EtoSizeF from, EtoSizeF to, UnitInterval at);
    public static EtoRectangleF Between(EtoRectangleF from, EtoRectangleF to, UnitInterval at);
    public static Fin<PerceptualColor> Between(PerceptualColor from, PerceptualColor to, UnitInterval at, Option<BlendPath> path = default);
}
```

To:
```csharp
// Tween DELETED
```

Why: `float.Lerp`, `double.Lerp`, direct Eto value construction, and `PerceptualColor.Mix` own every operation; the colour wrapper invents a failure carrier for a total value.

Change: Bind those operations directly to host interpolation delegates.

Delta: -9 LOC; -1 module-level type; -6 members.

Ripples: Replace `Tween.Between` and its prose in `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/motion.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/wires.md`, and `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/paint.md`.

# 14. Remove process-local scene-policy keys

From:
`[03]-[SURFACE] — ScenePolicy declaration head, lines 334–338`
```csharp
[SmartEnum<int>]
public sealed partial class ScenePolicy {
    public static readonly ScenePolicy Crisp = new(key: 0, antiAlias: false, interpolation: ImageInterpolation.None, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Balanced = new(key: 1, antiAlias: true, interpolation: ImageInterpolation.Default, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Fidelity = new(key: 2, antiAlias: true, interpolation: ImageInterpolation.High, offset: PixelOffsetMode.Half);
```

To:
```csharp
[SmartEnum]
public sealed partial class ScenePolicy {
    public static readonly ScenePolicy Crisp = new(antiAlias: false, interpolation: ImageInterpolation.None, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Balanced = new(antiAlias: true, interpolation: ImageInterpolation.Default, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Fidelity = new(antiAlias: true, interpolation: ImageInterpolation.High, offset: PixelOffsetMode.Half);
```

Why: The behavior-bearing item has no serialized, persisted, or foreign key consumer.

Change: Use a keyless smart enum.

Delta: 0 source LOC; 0 declared types or members; generated key and lookup surfaces removed.

# 15. Use Option for offscreen absence

From:
`[03]-[SURFACE] — OffscreenDraw declaration, lines 357–362`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffscreenDraw<TResult> {
    private OffscreenDraw() { }
    public sealed record DrawnCase(TResult Value) : OffscreenDraw<TResult>;
    public sealed record InvalidatedCase : OffscreenDraw<TResult>;
}
```

To:
```csharp
// OffscreenDraw<TResult> DELETED
```

Why: The operation has one value case and reason-free absence after applying its supplied fallback; `Option<TResult>` is that carrier.

Change: Return `Some(result)` after drawing and `None` after invalidation.

Delta: -6 LOC; -1 module-level type; -2 nested case types.

# 16. Store independent mount postures directly

From:
`[03]-[SURFACE] — CanvasExtent and FocusPolicy declarations, lines 364–376`
```csharp
[SmartEnum<int>]
public sealed partial class CanvasExtent {
    public static readonly CanvasExtent Viewport = new(key: 0, large: false);
    public static readonly CanvasExtent Scrolling = new(key: 1, large: true);
    internal bool Large { get; }
}

[SmartEnum<int>]
public sealed partial class FocusPolicy {
    public static readonly FocusPolicy Passive = new(key: 0, focusable: false);
    public static readonly FocusPolicy Interactive = new(key: 1, focusable: true);
    internal bool Focusable { get; }
}
```

To:
```csharp
// CanvasExtent DELETED
// FocusPolicy DELETED
```

Why: Each two-row roster only renames one independent boolean and carries no behavior, identity, admission, or payload.

Change: Put `Scrollable` and `Focusable` on `SurfaceSpec` and consume them once during mount.

Delta: -13 LOC; -2 module-level types; -6 declared members; both generated surfaces removed.

# 17. Remove the alpha-layout key and alias

From:
`[03]-[SURFACE] — AlphaLayout rows and alias, lines 378–384`
```csharp
[SmartEnum<int>]
public sealed partial class AlphaLayout {
    public static readonly AlphaLayout Straight = new(key: 0, channels: 4, gdi: GdiPixelFormat.Format32bppArgb);
    public static readonly AlphaLayout Premultiplied = new(key: 1, channels: 4, gdi: GdiPixelFormat.Format32bppPArgb);
    public static readonly AlphaLayout Opaque = new(key: 2, channels: 3, gdi: GdiPixelFormat.Format24bppRgb);

    public static AlphaLayout Declared => Straight;
```

To:
```csharp
[SmartEnum]
public sealed partial class AlphaLayout {
    public static readonly AlphaLayout Straight = new(channels: 4, gdi: GdiPixelFormat.Format32bppArgb);
    public static readonly AlphaLayout Premultiplied = new(channels: 4, gdi: GdiPixelFormat.Format32bppPArgb);
    public static readonly AlphaLayout Opaque = new(channels: 3, gdi: GdiPixelFormat.Format24bppRgb);
```

Why: The process-local behavior roster needs no key, and `Declared` only forwards `Straight`.

Change: Use `AlphaLayout.Straight` directly in byte normalization.

Delta: -1 source LOC; 0 types; -1 declared member; generated key and lookup surfaces removed.

# 18. Keep SurfaceSpec as data only

From:
`[03]-[SURFACE] — SurfaceSpec declaration, lines 394–402`
```csharp
public sealed record SurfaceSpec(
    PaintProgram Initial,
    ScenePolicy Policy,
    CanvasExtent Extent,
    FocusPolicy Focus,
    FaultCell Faults) {
    public static SurfaceSpec Of(PaintProgram initial, FaultCell faults);
    internal Fin<SurfaceSpec> Admit();
}
```

To:
```csharp
public sealed record SurfaceSpec(
    PaintProgram Initial,
    ScenePolicy Policy,
    bool Scrollable,
    bool Focusable,
    FaultCell Faults);
```

Why: The factory hides mount policy defaults and `Admit` has only the `Surface.Mount` consumer.

Change: Require explicit policy and perform the record gate directly in `Mount`.

Delta: -3 LOC; 0 types; -2 members.

Ripples: Update `ControlSpec.Painted` construction in `libs/dotnet/Rasm/.planning/Interaction/control.md` and Rhino surface mounts.

# 19. Bracket offscreen graphics inside Surface

From:
`[03]-[SURFACE] — Surface.Acquire signature, lines 417–418`
```csharp
public Fin<OffscreenDraw<TResult>> Acquire<TResult>(
    Func<Lease<Graphics>, Fin<TResult>> draw, Redraw fallback);
```

To:
```csharp
public Fin<Option<TResult>> Acquire<TResult>(
    Func<Graphics, Fin<TResult>> draw, Redraw fallback);
```

Why: `Surface` acquires, flushes, and releases the context; exposing its lease permits duplicated or escaped custody.

Change: Run the callback inside the owned bracket and return `None` after fallback invalidation.

Delta: 0 LOC; 0 types; 0 members; one wrapper parameter and the custom carrier removed.

# 20. Delete single-pixel forwarding methods

From:
`[03]-[SURFACE] — PixelLease single-pixel members, lines 437–441`
```csharp
public static Fin<PerceptualColor> Sample(EtoBitmap bitmap, EtoPoint at);
public static Fin<PerceptualColor> Sample(GdiBitmap bitmap, GdiPoint at);

public static Fin<Unit> Write(EtoBitmap bitmap, EtoPoint at, PerceptualColor colour);
public static Fin<Unit> Write(GdiBitmap bitmap, GdiPoint at, PerceptualColor colour);
```

To:
```csharp
// PixelLease.Sample DELETED
// PixelLease.Write DELETED
```

Why: These only rename `GetPixel` and `SetPixel` beside existing colour conversions; they add no lock, batching, normalization, or custody.

Change: Compose the host pixel member with `PaintColor` at the bitmap boundary.

Delta: -4 LOC; 0 types; -4 members.

# 21. Delete encoding convenience overloads

From:
`[03]-[SURFACE] — PixelLease.Encode members, lines 449–450`
```csharp
public static Fin<ReadOnlyMemory<byte>> Encode(EtoBitmap bitmap, ImageFormat format);
public static Fin<ReadOnlyMemory<byte>> Encode(GdiBitmap bitmap, GdiFormat format);
```

To:
```csharp
// PixelLease.Encode DELETED
```

Why: Eto already returns encoded bytes through `ToByteArray`, and GDI encodes into the caller-owned stream through `Save`.

Change: Encode at the artifact boundary that owns the destination.

Delta: -2 LOC; 0 types; -2 members.

# 22. Make ChromeRole a keyless behavior roster

From:
`[04]-[COLOR] — ChromeRole declaration and rows, lines 479–491`
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChromeRole {
    public static readonly ChromeRole Control = new(key: "control", read: static () => SystemColors.Control);
    public static readonly ChromeRole ControlBack = new(key: "control-back", read: static () => SystemColors.ControlBackground);
    public static readonly ChromeRole ControlText = new(key: "control-text", read: static () => SystemColors.ControlText);
    public static readonly ChromeRole DisabledText = new(key: "disabled-text", read: static () => SystemColors.DisabledText);
    public static readonly ChromeRole Highlight = new(key: "highlight", read: static () => SystemColors.Highlight);
    public static readonly ChromeRole HighlightText = new(key: "highlight-text", read: static () => SystemColors.HighlightText);
    public static readonly ChromeRole Selection = new(key: "selection", read: static () => SystemColors.Selection);
    public static readonly ChromeRole SelectionText = new(key: "selection-text", read: static () => SystemColors.SelectionText);
    public static readonly ChromeRole WindowBack = new(key: "window-back", read: static () => SystemColors.WindowBackground);
    public static readonly ChromeRole LinkText = new(key: "link-text", read: static () => SystemColors.LinkText);
```

To:
```csharp
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
```

Why: Live OS palette rows have no wire, persistence, host-key, or lookup consumer.

Change: Remove ten key strings and return the admitted colour from each row.

Delta: -1 source LOC; 0 declared types or members; generated key and lookup surfaces removed.

# 23. Generate the final ChromeRole operation directly

From:
`[04]-[COLOR] — ChromeRole behavior members, lines 493–496`
```csharp
[UseDelegateFromConstructor]
internal partial EtoColor Read();

public Fin<PerceptualColor> Sample() => PerceptualColor.OfHost(host: Read());
```

To:
```csharp
[UseDelegateFromConstructor]
public partial Fin<PerceptualColor> Sample();
```

Why: `Read` exists only to feed `Sample`; generated behavior can expose the final carrier directly.

Change: Bind the constructor delegate to `Sample`.

Delta: -2 LOC; 0 types; -1 member.

# 24. Remove process-local theme-variant keys

From:
`[05]-[THEME] — ThemeVariant declaration, lines 538–543`
```csharp
[SmartEnum<int>]
public sealed partial class ThemeVariant {
    public static readonly ThemeVariant Light = new(key: 0);
    public static readonly ThemeVariant Dark = new(key: 1);
    public static readonly ThemeVariant Contrast = new(key: 2);
}
```

To:
```csharp
[SmartEnum]
public sealed partial class ThemeVariant {
    public static readonly ThemeVariant Light = new();
    public static readonly ThemeVariant Dark = new();
    public static readonly ThemeVariant Contrast = new();
}
```

Why: The grid axis is selected by its declared item and crosses no wire or persistence boundary.

Change: Use a keyless smart enum.

Delta: 0 source LOC; 0 declared types or members; generated key and lookup surfaces removed.

# 25. Dispatch theme shifts at their consumer

From:
`[05]-[THEME] — ThemeShift.Merge, line 609`
```csharp
internal (ThemeVariant Variant, HashMap<PaletteRole, PerceptualColor> Overlay) Merge();
```

To:
```csharp
// ThemeShift.Merge DELETED
```

Why: The tuple projection has one consumer and hides generated exhaustive dispatch behind a forwarding member.

Change: Call `ThemeShift.Switch` directly inside `ThemeGrid.Swap`.

Delta: -1 LOC; 0 types; -1 member.

# 26. Keep restyle failures outside accepted theme state

From:
`[05]-[THEME] — ThemeChange declaration, lines 651–660`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct ThemeChange(
    MonotonicStamp Generation,
    ThemeVariant Variant,
    Seq<PaletteRole> Changed,
    Seq<Error> Failures) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Failures.IsEmpty,
        ValidityClaim.Evidence(evidence: Optional(Generation)),
        ValidityClaim.Evidence(evidence: Optional(Variant)));
}
```

To:
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct ThemeChange(
    MonotonicStamp Generation,
    ThemeVariant Variant,
    Seq<PaletteRole> Changed) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(Optional(Generation)),
        ValidityClaim.Evidence(Optional(Variant)));
}
```

Why: Accepted grid state and non-fatal control-restyle failures are different channels; warnings must not make a successful change invalid.

Change: Keep `ThemeGrid.Swap` as `Fin<ThemeChange>` and return `WriterT<Error, Fin, ThemeChange>` from `ThemePort.Change`, combining independent restyle failures with `+`.

Delta: -2 LOC; 0 types; -1 member; one parallel failure collection removed.

Ripples: Update `ThemePort.Change` in `libs/dotnet/Rasm/.planning/Interaction/platform.md`, `ThemePalette.Feed` in `libs/dotnet/Rasm.Rhino/.planning/HostUi/panels.md`, and theme calls in `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md` and `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md` to run and fold the writer transformer.
