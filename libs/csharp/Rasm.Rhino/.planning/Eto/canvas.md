# [RASM_RHINO_ETO_CANVAS]

`Surface` mounts the host `Drawable` and replays the mounted `PaintProgram` against its `Graphics` stream. `Pigment` owns `PerceptualColor` quantization, `GlyphBlock` owns text metrics, and `PixelLease` brackets bitmap access; the retained mark, path, stroke, and fill vocabulary is `Rasm.Rhino.Display` law and never re-minted here.

## [01]-[INDEX]

- [02]-[PIGMENT_EDGE]: `Pigment` owns quantization; paint specs are Display vocabulary.
- [03]-[GLYPHS]: `TypeRole` rows over `SystemFonts` + `GlyphBlock` — decorated, shaped, wrapped, aligned, trimmed text with one metric authority.
- [04]-[PAINT_SEAM]: `PaintProgram` — the delegate pair carrying Display's mark vocabulary onto this surface.
- [05]-[SURFACE]: `ScenePolicy` + `SurfaceSpec` + `Surface` + `PixelLease` — `Drawable` mount and lifecycle, quality policy, invalidation rows, off-event `Graphics` leases, IME verbs, and `Bitmap.Lock` pixel windows under `Lease<T>`.

## [02]-[PIGMENT_EDGE]

- Owner: `Pigment` is the sole `PerceptualColor` to `Color` projector; every brush, pen, and glyph ink on this surface quantizes through it.
- Law: this page mints no stroke, fill, dash, or path spec — the paint vocabulary is `Rasm.Rhino.Display` `Marks`, and paint reaches this surface only through the mounted `PaintProgram`.
- Packages: Rasm (kernel — `PerceptualColor`), Eto.Drawing (host — `Color`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Drawing;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Rhino.Eto;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Pigment {
    public static Color ToColor(PerceptualColor colour) =>
        colour.ToRgb() switch {
            { } rgb => Color.FromArgb(red: rgb.Red, green: rgb.Green, blue: rgb.Blue, alpha: rgb.Alpha),
        };
}
```

## [03]-[GLYPHS]

- Owner: `TypeRole` resolves host typography policy over point size and decoration, `GlyphBlock` is the one text-shaping and metric owner every folder composes, and `GlyphShape` owns the coupled `FormattedText` and foreground brush for one draw window.
- Law: a role resolves through `SystemFonts`, whose fonts are process-cached per (font, size, decoration) — the resolved `Font` is a shared host instance and is never disposed; a raw `new Font(...)` in paint code is the deleted form, and a custom face enters as a new `TypeRole` row whose resolve column constructs it.
- Law: `Measure` memoizes per retained block and shapes without ink — foreground is `Option`al, so a measure-only block never mints a brush — and repeated bounds and hit probes never re-enter uncached host shaping or key on `Font.Equals`, which omits `FontDecoration`.
- Law: `Draw` is the block's one paint egress — shape, draw at a location, release the shape — so a consumer never touches `FormattedText` or a `GlyphShape` directly.
- Growth: a new role is one row; a new text treatment (max-extent ellipsis, right-aligned numeric, point size) is field values on the block, never a sibling text type.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TypeRole {
    public static readonly TypeRole Body = new(key: 0, resolve: static (size, decoration) => SystemFonts.Default(size: size, decoration: decoration));
    public static readonly TypeRole Strong = new(key: 1, resolve: static (size, decoration) => SystemFonts.Bold(size: size, decoration: decoration));
    public static readonly TypeRole Caption = new(key: 2, resolve: static (size, decoration) => SystemFonts.Label(size: size, decoration: decoration));
    public static readonly TypeRole MenuText = new(key: 3, resolve: static (size, decoration) => SystemFonts.Menu(size: size, decoration: decoration));
    public static readonly TypeRole StatusText = new(key: 4, resolve: static (size, decoration) => SystemFonts.StatusBar(size: size, decoration: decoration));
    public static readonly TypeRole HintText = new(key: 5, resolve: static (size, decoration) => SystemFonts.ToolTip(size: size, decoration: decoration));
    public static readonly TypeRole TitleText = new(key: 6, resolve: static (size, decoration) => SystemFonts.TitleBar(size: size, decoration: decoration));
    public static readonly TypeRole MenuBarText = new(key: 7, resolve: static (size, decoration) => SystemFonts.MenuBar(size: size, decoration: decoration));
    public static readonly TypeRole MessageText = new(key: 8, resolve: static (size, decoration) => SystemFonts.Message(size: size, decoration: decoration));
    public static readonly TypeRole PaletteText = new(key: 9, resolve: static (size, decoration) => SystemFonts.Palette(size: size, decoration: decoration));
    public static readonly TypeRole UserText = new(key: 10, resolve: static (size, decoration) => SystemFonts.User(size: size, decoration: decoration));
    [UseDelegateFromConstructor]
    internal partial Font Resolve(float? size = null, FontDecoration decoration = FontDecoration.None);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed class GlyphBlock(
    string text,
    TypeRole role,
    Option<PerceptualColor> foreground = default,
    Option<float> size = default,
    FontDecoration decoration = FontDecoration.None,
    FormattedTextWrapMode wrap = FormattedTextWrapMode.Word,
    FormattedTextAlignment alignment = FormattedTextAlignment.Left,
    FormattedTextTrimming trimming = FormattedTextTrimming.None,
    Option<SizeF> maxExtent = default) {
    private readonly Lazy<SizeF> measured = new(() => {
        using FormattedText shaped = ShapeText(text, role, size, decoration, wrap, alignment, trimming, maxExtent, None);
        return shaped.Measure();
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    public string Text { get; } = text;
    public TypeRole Role { get; } = role;
    public Option<PerceptualColor> Foreground { get; } = foreground;
    public Option<float> Size { get; } = size;
    public FontDecoration Decoration { get; } = decoration;
    public FormattedTextWrapMode Wrap { get; } = wrap;
    public FormattedTextAlignment Alignment { get; } = alignment;
    public FormattedTextTrimming Trimming { get; } = trimming;
    public Option<SizeF> MaxExtent { get; } = maxExtent;

    public SizeF Measure() => measured.Value;

    public Unit Draw(Graphics graphics, PointF at) => Op.Side(() => {
        using GlyphShape shaped = Shape();
        graphics.DrawText(formattedText: shaped.Text, location: at);
    });

    internal GlyphShape Shape() {
        SolidBrush? ink = null;
        FormattedText? shaped = null;
        try {
            ink = Foreground.Map(static colour => new SolidBrush(color: Pigment.ToColor(colour: colour))).IfNoneUnsafe((SolidBrush?)null);
            shaped = ShapeText(Text, Role, Size, Decoration, Wrap, Alignment, Trimming, MaxExtent, Optional(ink));
            return new GlyphShape(shaped, Optional(ink));
        } catch {
            shaped?.Dispose();
            ink?.Dispose();
            throw;
        }
    }

    private static FormattedText ShapeText(
        string text,
        TypeRole role,
        Option<float> size,
        FontDecoration decoration,
        FormattedTextWrapMode wrap,
        FormattedTextAlignment alignment,
        FormattedTextTrimming trimming,
        Option<SizeF> maxExtent,
        Option<SolidBrush> ink) {
        FormattedText shaped = new() {
            Text = text,
            Font = role.Resolve(size: size.ToNullable(), decoration: decoration),
            Wrap = wrap,
            Alignment = alignment,
            Trimming = trimming,
        };
        _ = ink.Iter(brush => shaped.ForegroundBrush = brush);
        _ = maxExtent.Iter(extent => shaped.MaximumSize = extent);
        return shaped;
    }
}

internal sealed class GlyphShape(FormattedText text, Option<SolidBrush> ink) : IDisposable {
    private int released;

    public FormattedText Text { get; } = text;

    public void Dispose() {
        if (Interlocked.Exchange(ref released, 1) == 0) {
            try { Text.Dispose(); }
            finally { _ = ink.Iter(static brush => brush.Dispose()); }
        }
    }
}
```

## [04]-[PAINT_SEAM]

- Owner: `PaintProgram` admits the mounted paint-and-hit delegate pair; the retained mark, path, stroke, and fill vocabulary is `Rasm.Rhino.Display` — `Marks.Render`/`Marks.Hit` own it and `Marks.Program` mints the program this seam mounts.
- Law: strata forbid an upward reference, so the vocabulary enters as two delegates — this surface owns `Graphics` custody, quality policy, and replay; the program owns geometry, paint order, and hit truth over ONE retained value, and hit ordinals preserve z-order evidence (topmost is the last ordinal).
- Law: print flow hands its page `Graphics` to the same mounted program, so paint truth stays single-owned across surface and print.
- Growth: a new drawable is a Display mark case; this seam gains nothing per drawable.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PaintProgram(Func<Graphics, Fin<Unit>> Paint, Func<PointF, Fin<Seq<int>>> Hit) {
    public static readonly PaintProgram Blank = new(
        Paint: static _ => Fin.Succ(value: unit),
        Hit: static _ => Fin.Succ(value: Seq<int>()));

    internal static Fin<PaintProgram> Admit(PaintProgram? program, Op op) => program switch {
        null => Fin.Fail<PaintProgram>(new UiFault.Rejected(Key: op, Field: nameof(PaintProgram), Reason: "paint program is absent")),
        { Paint: null } => Fin.Fail<PaintProgram>(new UiFault.Rejected(Key: op, Field: nameof(Paint), Reason: "paint projection is absent")),
        { Hit: null } => Fin.Fail<PaintProgram>(new UiFault.Rejected(Key: op, Field: nameof(Hit), Reason: "hit projection is absent")),
        _ => Fin.Succ(program),
    };
}
```

## [05]-[SURFACE]

- Owner: `ScenePolicy` + `SurfaceSpec` + `Surface` own quality state, `Drawable` construction, paint-fault egress, program swaps, invalidation, hit-testing, graphics acquisition, IME composition, and accumulated release; `PixelLease` owns locked and unlocked bitmap access plus whole-or-region `Clone` egress.
- Law: paint handlers replay the current admitted atom program; a swap publishes for redraw, commits on redraw success, and conditionally restores the prior program on failure, so paint and hit truth stay aligned.
- Law: quality knobs (`AntiAlias`, `ImageInterpolation`, `PixelOffsetMode`) are `ScenePolicy` values bracketed once around each replay and restored on exit, never per-mark toggles.
- Law: `ScenePolicy.Use` brackets transform and clip state with the quality tuple, so every mounted or printed program leaves the caller's `Graphics` stream unchanged.
- Law: the realized `Drawable` is its surface handle — `Surface.Of(host)` recovers the mounted owner, so an `Element.Painted` consumer swaps programs and hit-tests through the one control `Realize` returned, and a parallel surface registry is the deleted form.
- Law: IME composition rides the host verbs — `CancelComposition`/`CommitComposition` project `CancelTextComposition`/`CommitTextComposition` — and a text-editing overlay that ignores composition state is the named defect.
- Law: `Acquire` succeeds only where the host advertises it — `SupportsCreateGraphics` gates the lease with a typed `UiFault.Unavailable`, never a downstream null; the off-event handle `Flush`es queued commands before the lease disposes it, since an off-event `CreateGraphics` stream is not committed by the paint loop.
- Growth: a new invalidation modality is one `Redraw` case; frame pacing, display-link cadence, and animation clocks are the Viewport unit's motion owner — this surface exposes swap-and-invalidate and nothing temporal.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ScenePolicy {
    public static readonly ScenePolicy Crisp = new(key: 0, antiAlias: false, interpolation: ImageInterpolation.None, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Balanced = new(key: 1, antiAlias: true, interpolation: ImageInterpolation.Default, offset: PixelOffsetMode.None);
    public static readonly ScenePolicy Fidelity = new(key: 2, antiAlias: true, interpolation: ImageInterpolation.High, offset: PixelOffsetMode.Half);
    internal bool AntiAlias { get; }
    internal ImageInterpolation Interpolation { get; }
    internal PixelOffsetMode Offset { get; }

    internal TResult Use<TResult>(Graphics graphics, Func<TResult> body) {
        using IDisposable transform = graphics.SaveTransformState();
        (bool AntiAlias, ImageInterpolation Interpolation, PixelOffsetMode Offset) prior =
            (graphics.AntiAlias, graphics.ImageInterpolation, graphics.PixelOffsetMode);
        try {
            graphics.AntiAlias = AntiAlias;
            graphics.ImageInterpolation = Interpolation;
            graphics.PixelOffsetMode = Offset;
            return body();
        } finally {
            graphics.AntiAlias = prior.AntiAlias;
            graphics.ImageInterpolation = prior.Interpolation;
            graphics.PixelOffsetMode = prior.Offset;
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Redraw {
    private Redraw() { }
    public sealed record Whole : Redraw;
    public sealed record Region(Rectangle Bounds) : Redraw;
    public sealed record Immediate(Rectangle Bounds) : Redraw;

    internal Unit Apply(Drawable host) => Switch(
        state: host,
        whole: static (surface, _) => Op.Side(surface.Invalidate),
        region: static (surface, bounded) => Op.Side(() => surface.Invalidate(rect: bounded.Bounds)),
        immediate: static (surface, bounded) => Op.Side(() => surface.Update(region: bounded.Bounds)));
}

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

public sealed record SurfaceSpec(
    PaintProgram Initial,
    ScenePolicy Policy,
    CanvasExtent Extent,
    FocusPolicy Focus,
    Action<Error> Report) {
    public Fin<Surface> Mount(Op? key = null) => Surface.Mount(spec: this, key: key);

    internal Fin<SurfaceSpec> Admit(Op op) => (Initial, Policy, Extent, Focus, Report) switch {
        (null, _, _, _, _) => Fin.Fail<SurfaceSpec>(new UiFault.Rejected(Key: op, Field: nameof(Initial), Reason: "initial program is absent")),
        (_, null, _, _, _) => Fin.Fail<SurfaceSpec>(new UiFault.Rejected(Key: op, Field: nameof(Policy), Reason: "scene policy is absent")),
        (_, _, null, _, _) => Fin.Fail<SurfaceSpec>(new UiFault.Rejected(Key: op, Field: nameof(Extent), Reason: "canvas extent is absent")),
        (_, _, _, null, _) => Fin.Fail<SurfaceSpec>(new UiFault.Rejected(Key: op, Field: nameof(Focus), Reason: "focus policy is absent")),
        (_, _, _, _, null) => Fin.Fail<SurfaceSpec>(new UiFault.Rejected(Key: op, Field: nameof(Report), Reason: "fault reporter is absent")),
        _ => PaintProgram.Admit(Initial, op).Map(_ => this),
    };
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class Surface : UiLease {
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Drawable, Surface> Mounted = new();
    private readonly Atom<PaintProgram> program;
    private readonly EventHandler<PaintEventArgs> paint;
    private readonly Op key;
    private Surface(Drawable host, Atom<PaintProgram> program, EventHandler<PaintEventArgs> paint, Op key) {
        Host = host;
        this.program = program;
        this.paint = paint;
        this.key = key;
    }

    public Drawable Host { get; }

    public static Fin<Surface> Mount(SurfaceSpec? spec, Op? key = null) {
        Op op = key.OrDefault();
        Drawable? host = null;
        EventHandler<PaintEventArgs>? paint = null;
        return Optional(spec)
            .ToFin(new UiFault.Rejected(Key: op, Field: nameof(spec), Reason: "surface specification is absent"))
            .Bind(admitted => admitted.Admit(op))
            .Bind(admitted => op.Catch(() => {
            Atom<PaintProgram> held = Atom(admitted.Initial);
            Drawable owned = host = new Drawable(largeCanvas: admitted.Extent.Large) { CanFocus = admitted.Focus.Focusable };
            EventHandler<PaintEventArgs> handler = paint = (_, args) => {
                _ = op.Catch(() => admitted.Policy.Use(args.Graphics, () => held.Value.Paint(args.Graphics))).Match(
                    Succ: static _ => unit,
                    Fail: fault => Op.Side(() => admitted.Report(fault)));
            };
            owned.Paint += handler;
            Surface surface = new(host: owned, program: held, paint: handler, key: op);
            Mounted.Add(owned, surface);
            return Fin.Succ(value: surface);
        }).MapFail(fault => Optional(host).Map(owned => fault.Also(
            (Optional(paint).Map(handler => Seq<Action>(() => owned.Paint -= handler)).IfNone(Seq<Action>())
             + Seq<Action>(owned.Dispose)).Drained(op))).IfNone(fault)));
    }

    public static Option<Surface> Of(Drawable host) =>
        Mounted.TryGetValue(host, out Surface? held) ? Some(held) : None;

    public Fin<Unit> Swap(Func<PaintProgram, PaintProgram> next, Redraw redraw, Op? key = null) {
        Op op = key.OrDefault();
        return Live(op)
            .Bind(_ => Optional(next).ToFin(new UiFault.Rejected(Key: op, Field: nameof(next), Reason: "program transition is absent")))
            .Bind(advance => op.Catch(() => {
                PaintProgram prior = program.Value;
                return PaintProgram.Admit(advance(prior), op).Map(candidate => (Prior: prior, Candidate: candidate));
            }))
            .Bind(held => {
                _ = program.Swap(_ => held.Candidate);
                return op.Catch(() => Fin.Succ(redraw.Apply(Host)))
                    .MapFail(fault => {
                        _ = program.Swap(current => ReferenceEquals(current, held.Candidate) ? held.Prior : current);
                        return fault;
                    });
            });
    }

    public Fin<Seq<int>> HitTest(PointF point, Op? key = null) {
        Op op = key.OrDefault();
        return Live(op).Bind(_ => op.Catch(() => program.Value.Hit(point)));
    }

    public Fin<TResult> Acquire<TResult>(Func<Graphics, TResult> draw, Op? key = null) =>
        Live(key.OrDefault()).Bind(_ => Host.SupportsCreateGraphics
            ? key.OrDefault().Catch(() => Fin.Succ(value: new Lease<Graphics>.Owned(Value: Host.CreateGraphics()).Use(project: graphics => {
                TResult drawn = draw(graphics);
                graphics.Flush();
                return drawn;
            })))
            : Fin.Fail<TResult>(error: new UiFault.Unavailable(Key: key.OrDefault(), Capability: nameof(Host.SupportsCreateGraphics))));

    public Fin<Unit> CancelComposition(Op? key = null) => Composition(Host.CancelTextComposition, key);

    public Fin<Unit> CommitComposition(Op? key = null) => Composition(Host.CommitTextComposition, key);

    protected override Fin<Unit> Free() =>
        Seq<Action>(() => Host.Paint -= paint, () => { _ = Mounted.Remove(Host); }, Host.Dispose).Drained(key);

    private Fin<Unit> Composition(Action verb, Op? key) =>
        Live(key.OrDefault()).Bind(_ => key.OrDefault().Catch(() => Fin.Succ(Op.Side(verb))));

    private Fin<Unit> Live(Op op) => Released
        ? Fin.Fail<Unit>(new UiFault.Released(Key: op, Resource: nameof(Surface)))
        : Fin.Succ(unit);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PixelLease {
    public static Fin<TResult> Locked<TResult>(Bitmap bitmap, Func<BitmapData, TResult> read, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: new Lease<BitmapData>.Owned(Value: bitmap.Lock()).Use(project: read)));

    public static Fin<PerceptualColor> Sample(Bitmap bitmap, Point at, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: bitmap.GetPixel(position: at))).Bind(colour =>
            PerceptualColor.OfRgb(red: (byte)colour.Rb, green: (byte)colour.Gb, blue: (byte)colour.Bb, alpha: colour.A, key: key));

    public static Fin<Unit> Write(Bitmap bitmap, Point at, PerceptualColor colour, Op? key = null) =>
        WriteLocked(bitmap, Seq((At: at, Colour: colour)), key);

    public static Fin<Unit> WriteLocked(Bitmap bitmap, Seq<(Point At, PerceptualColor Colour)> pixels, Op? key = null) {
        Op op = key.OrDefault();
        return pixels
            .Traverse(pixel => op.Catch(() => Fin.Succ(bitmap.GetPixel(position: pixel.At))).ToValidation().Map(_ => pixel))
            .As()
            .ToFin()
            .Bind(admitted => Locked(
                bitmap,
                data => admitted.Iter(pixel => data.SetPixel(position: pixel.At, color: Pigment.ToColor(pixel.Colour))),
                op));
    }

    public static Fin<byte[]> Encode(Bitmap bitmap, ImageFormat format, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: bitmap.ToByteArray(imageFormat: format)));

    public static Fin<Bitmap> Clone(Bitmap bitmap, Option<Rectangle> region, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: bitmap.Clone(rectangle: region.ToNullable())));
}
```
