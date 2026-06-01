using System.Runtime.InteropServices;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using EtoColor = Eto.Drawing.Color;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public interface IUiInput<TState> {
    public TState State { get; }
    public bool Shift { get; }
    public bool Control { get; }
    public bool Alt { get; }
}

public abstract partial record UiInputEvent<TState> : IUiInput<TState> {
    private UiInputEvent() { }
    public abstract TState State { get; }
    public abstract bool Shift { get; }
    public abstract bool Control { get; }
    public abstract bool Alt { get; }

    public sealed record CanvasPointer(UiCanvasContext<TState> Value) : UiInputEvent<TState> {
        public override TState State => Value.State;
        public override bool Shift => Value.Shift;
        public override bool Control => Value.Control;
        public override bool Alt => Value.Alt;
    }

    public sealed record CanvasKey(UiCanvasKey<TState> Value) : UiInputEvent<TState> {
        public override TState State => Value.State;
        public override bool Shift => Value.Shift;
        public override bool Control => Value.Control;
        public override bool Alt => Value.Alt;
    }

    public sealed record ViewportMouse(MouseContext<TState> Value) : UiInputEvent<TState> {
        public override TState State => Value.State;
        public override bool Shift => Value.Shift;
        public override bool Control => Value.Control;
        public override bool Alt => false;
    }
}

public enum KeyPhase { Down, Up }

[Union]
public abstract partial record SpriteSource {
    private SpriteSource() { }
    public sealed record Memory(DrawingBitmap Bitmap) : SpriteSource;
    public sealed record Path(string Value) : SpriteSource;
    internal DisplayBitmap Load() => Switch(
        memory: static m => new DisplayBitmap(bitmap: m.Bitmap),
        path: static p => DisplayBitmap.Load(path: p.Value));
    internal DrawingColor AveragePixel() => Switch(
        memory: static m => Average(m.Bitmap),
        path: static p => { using DrawingBitmap loaded = new(p.Value); return Average(loaded); });
    private static DrawingColor Average(DrawingBitmap source) {
        using DrawingBitmap one = new(1, 1);
        using System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(one);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(source, new System.Drawing.Rectangle(0, 0, 1, 1));
        return one.GetPixel(0, 0);
    }
    internal Eto.Drawing.Bitmap ToEtoBitmap() => Switch(
        memory: static m => {
            using MemoryStream stream = new();
            m.Bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            return new Eto.Drawing.Bitmap(stream);
        },
        path: static p => new Eto.Drawing.Bitmap(p.Value));
}

public enum SystemFontKind { Default, Bold, Label, Menu, MenuBar, Message, Palette, Status, TitleBar, ToolTip, User }

public enum UiAnchor { TopLeft, TopCenter, TopRight, MiddleLeft, Center, MiddleRight, BottomLeft, BottomCenter, BottomRight }

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record UiCurveSeg {    // screen-space (px) path segment
    private UiCurveSeg() { }
    public sealed record Line(System.Drawing.PointF From, System.Drawing.PointF To) : UiCurveSeg;
    public sealed record Arc(System.Drawing.RectangleF Bounds, float StartAngle, float SweepAngle) : UiCurveSeg;
    public sealed record Bezier(System.Drawing.PointF Start, System.Drawing.PointF Control1, System.Drawing.PointF Control2, System.Drawing.PointF End) : UiCurveSeg;
    internal Unit AddTo(Eto.Drawing.GraphicsPath path) => Switch(path,
        line: static (p, l) => Op.Side(() => p.AddLine(l.From.X, l.From.Y, l.To.X, l.To.Y)),
        arc: static (p, a) => Op.Side(() => p.AddArc(a.Bounds.X, a.Bounds.Y, a.Bounds.Width, a.Bounds.Height, a.StartAngle, a.SweepAngle)),
        bezier: static (p, b) => Op.Side(() => p.AddBezier(new Eto.Drawing.PointF(b.Start.X, b.Start.Y), new Eto.Drawing.PointF(b.Control1.X, b.Control1.Y), new Eto.Drawing.PointF(b.Control2.X, b.Control2.Y), new Eto.Drawing.PointF(b.End.X, b.End.Y))));
    internal Option<Curve> ToCurve() => Switch(
        line: static l => Some<Curve>(new LineCurve(new global::Rhino.Geometry.Line(new Point3d(l.From.X, l.From.Y, 0d), new Point3d(l.To.X, l.To.Y, 0d)))),
        arc: static a => (a.Bounds.Width / 2f, a.Bounds.Height / 2f) switch {
            (float rx, float ry) when rx > 0f && Math.Abs(rx - ry) < 1e-3f => Some<Curve>(new ArcCurve(new global::Rhino.Geometry.Arc(
                new Circle(new Plane(new Point3d(a.Bounds.X + rx, a.Bounds.Y + ry, 0d), Vector3d.ZAxis), rx),
                new Interval(a.StartAngle * Math.PI / 180.0, (a.StartAngle + a.SweepAngle) * Math.PI / 180.0)))),
            _ => Option<Curve>.None,
        },
        bezier: static b => Some<Curve>(new BezierCurve([new Point3d(b.Start.X, b.Start.Y, 0d), new Point3d(b.Control1.X, b.Control1.Y, 0d), new Point3d(b.Control2.X, b.Control2.Y, 0d), new Point3d(b.End.X, b.End.Y, 0d)]).ToNurbsCurve()));
    internal Seq<System.Drawing.PointF> Sample(int steps) => Switch(
        line: static l => Seq(l.From, l.To),
        arc: a => toSeq(Enumerable.Range(start: 0, count: steps + 1)).Map(i => {
            double angle = (a.StartAngle + (a.SweepAngle * i / steps)) * Math.PI / 180.0;
            float rx = a.Bounds.Width / 2f;
            float ry = a.Bounds.Height / 2f;
            return new System.Drawing.PointF(a.Bounds.X + rx + (float)(rx * Math.Cos(angle)), a.Bounds.Y + ry + (float)(ry * Math.Sin(angle)));
        }),
        bezier: b => toSeq(Enumerable.Range(start: 0, count: steps + 1)).Map(i => {
            float t = (float)i / steps;
            float u = 1f - t;
            return new System.Drawing.PointF(
                (u * u * u * b.Start.X) + (3f * u * u * t * b.Control1.X) + (3f * u * t * t * b.Control2.X) + (t * t * t * b.End.X),
                (u * u * u * b.Start.Y) + (3f * u * u * t * b.Control1.Y) + (3f * u * t * t * b.Control2.Y) + (t * t * t * b.End.Y));
        }));
}

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record UiFill {
    private UiFill() { }
    public sealed record Solid(DrawingColor Color, Option<float> Opacity = default) : UiFill;
    public sealed record Linear(DrawingColor Start, DrawingColor End, System.Drawing.PointF From, System.Drawing.PointF To, Option<float> Opacity = default) : UiFill;
    public sealed record Radial(DrawingColor Start, DrawingColor End, System.Drawing.PointF Center, System.Drawing.PointF Origin, System.Drawing.SizeF Radius, Option<float> Opacity = default) : UiFill;
    public sealed record Texture(SpriteSource Image, Option<float> Opacity = default) : UiFill;

    internal DrawingColor Primary => Switch(   // screen rail degrades gradient -> first stop, texture -> mean pixel
        solid: static s => s.Color,
        linear: static l => l.Start,
        radial: static r => r.Start,
        texture: static t => t.Image.AveragePixel());
    private static float Alpha(Option<float> opacity) =>
        opacity.Map(static value => Math.Clamp(value: value, min: 0f, max: 1f)).IfNone(1f);

    private static DrawingColor Fade(DrawingColor c, Option<float> o) =>
        o.IsSome ? DrawingColor.FromArgb((int)(c.A * Alpha(opacity: o)), c.R, c.G, c.B) : c;

    internal Unit UseBrush(Action<Eto.Drawing.Brush> paint) => Switch(paint,
        solid: static (draw, s) => Op.Side(() => { using Eto.Drawing.Brush brush = new Eto.Drawing.SolidBrush(Fade(s.Color, s.Opacity).ToEto()); draw(brush); }),
        linear: static (draw, l) => Op.Side(() => { using Eto.Drawing.Brush brush = new Eto.Drawing.LinearGradientBrush(Fade(l.Start, l.Opacity).ToEto(), Fade(l.End, l.Opacity).ToEto(), new Eto.Drawing.PointF(l.From.X, l.From.Y), new Eto.Drawing.PointF(l.To.X, l.To.Y)); draw(brush); }),
        radial: static (draw, r) => Op.Side(() => { using Eto.Drawing.Brush brush = new Eto.Drawing.RadialGradientBrush(Fade(r.Start, r.Opacity).ToEto(), Fade(r.End, r.Opacity).ToEto(), new Eto.Drawing.PointF(r.Center.X, r.Center.Y), new Eto.Drawing.PointF(r.Origin.X, r.Origin.Y), new Eto.Drawing.SizeF(r.Radius.Width, r.Radius.Height)); draw(brush); }),
        texture: static (draw, t) => Op.Side(() => {
            using Eto.Drawing.Bitmap bitmap = t.Image.ToEtoBitmap();
            using Eto.Drawing.Brush brush = new Eto.Drawing.TextureBrush(bitmap) { Opacity = Alpha(opacity: t.Opacity) };
            draw(brush);
        }));
}

[Union]
public abstract partial record UiFont {
    private const float AverageGlyphWidthRatio = 0.6f;
    private const float MinimumGlyphWidthRatio = 0.5f;
    private const float LineBoxHeightRatio = 1.25f;
    private UiFont() { }
    public sealed record FamilyCase(string Name, float Size, Eto.Drawing.FontStyle Style = Eto.Drawing.FontStyle.None, Eto.Drawing.FontDecoration Decoration = Eto.Drawing.FontDecoration.None) : UiFont;
    public sealed record SystemCase(SystemFontKind Kind, Option<float> Size = default, Eto.Drawing.FontDecoration Decoration = Eto.Drawing.FontDecoration.None) : UiFont;

    public static UiFont Of(string family, float size) => new FamilyCase(Name: family, Size: size);
    public static UiFont Family(string family, float size, Eto.Drawing.FontStyle style = Eto.Drawing.FontStyle.None, Eto.Drawing.FontDecoration decoration = Eto.Drawing.FontDecoration.None) => new FamilyCase(Name: family, Size: size, Style: style, Decoration: decoration);
    public static UiFont System(SystemFontKind kind, Option<float> size = default, Eto.Drawing.FontDecoration decoration = Eto.Drawing.FontDecoration.None) => new SystemCase(Kind: kind, Size: size, Decoration: decoration);
    public static UiFont Default { get; } = new FamilyCase(Name: "Arial", Size: 12f);

    internal int Height => Switch(familyCase: static f => (int)f.Size, systemCase: static s => (int)s.Size.IfNone(12f));
    internal string Face => Switch(familyCase: static f => f.Name, systemCase: static _ => string.Empty);   // pipeline rail face hint (system presets resolve via Eto, not by face name)
    internal System.Drawing.SizeF HitSize(string value) {
        float height = Math.Max(val1: 1f, val2: Height);
        return new(
            width: Math.Max(val1: height * MinimumGlyphWidthRatio, val2: value.Length * height * AverageGlyphWidthRatio),
            height: height * LineBoxHeightRatio);
    }
    internal Eto.Drawing.Font ToEto() => Switch(
        familyCase: static f => new Eto.Drawing.Font(f.Name, f.Size, f.Style, f.Decoration),
        systemCase: static s => s.Kind switch {
            SystemFontKind.Bold => Eto.Drawing.SystemFonts.Bold(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.Label => Eto.Drawing.SystemFonts.Label(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.Menu => Eto.Drawing.SystemFonts.Menu(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.MenuBar => Eto.Drawing.SystemFonts.MenuBar(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.Message => Eto.Drawing.SystemFonts.Message(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.Palette => Eto.Drawing.SystemFonts.Palette(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.Status => Eto.Drawing.SystemFonts.StatusBar(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.TitleBar => Eto.Drawing.SystemFonts.TitleBar(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.ToolTip => Eto.Drawing.SystemFonts.ToolTip(s.Size.ToNullable(), s.Decoration),
            SystemFontKind.User => Eto.Drawing.SystemFonts.User(s.Size.ToNullable(), s.Decoration),
            _ => Eto.Drawing.SystemFonts.Default(s.Size.ToNullable(), s.Decoration),
        });
}

[Union]
public abstract partial record UiMark {    // screen-space (px), backend-neutral
    private UiMark() { }
    public sealed record Text(string Value, System.Drawing.PointF At, DrawingColor Color, UiFont? Font = null, UiAnchor Anchor = UiAnchor.TopLeft) : UiMark;
    public sealed record Stroke(System.Drawing.PointF From, System.Drawing.PointF To, UiStroke Pen) : UiMark;
    public sealed record Path(Seq<System.Drawing.PointF> Points, UiStroke Pen, Option<UiFill> Fill = default, bool Closed = false) : UiMark;
    public sealed record Curve(Seq<UiCurveSeg> Segs, UiStroke Pen, Option<UiFill> Fill = default, bool Closed = false) : UiMark;
    public sealed record Box(System.Drawing.Rectangle Bounds, UiStroke Outline, UiFill Fill, float Corner = 0f) : UiMark;
    public sealed record Sprite(UiSprite Value) : UiMark;
    internal Fin<Unit> Render(UiSurface surface) =>
        RhinoUi.Protect(valid: () => Fin.Succ(value: Switch(surface,
            text: static (UiSurface surf, Text t) => surf.Text(t.Value, t.At, t.Color, t.Font ?? UiFont.Default, t.Anchor),
            stroke: static (UiSurface surf, Stroke k) => surf.Stroke(k.From, k.To, k.Pen),
            path: static (UiSurface surf, Path p) => surf.Path(p.Points, p.Pen, p.Fill, p.Closed),
            curve: static (UiSurface surf, Curve c) => surf.Curve(c.Segs, c.Pen, c.Fill, c.Closed),
            box: static (UiSurface surf, Box b) => surf.Box(b.Bounds, b.Outline, b.Fill, b.Corner),
            sprite: static (UiSurface surf, Sprite s) => surf.Sprite(s.Value))));
    internal bool Hit(System.Drawing.PointF p, float tolerance = 4f) {
        using Eto.Drawing.GraphicsPath path = new();
        Eto.Drawing.PointF q = new(p.X, p.Y);
        using Eto.Drawing.Pen probe = new(Eto.Drawing.Colors.Black, tolerance);
        return Switch(
            state: (Path: path, Point: q, Probe: probe, Tolerance: tolerance),
            text: static (state, t) => {
                UiFont font = t.Font ?? UiFont.Default;
                System.Drawing.SizeF size = font.HitSize(value: t.Value);
                System.Drawing.PointF origin = UiSurface.TextOrigin(anchor: t.Anchor, at: t.At, size: size);
                state.Path.AddRectangle(origin.X, origin.Y, size.Width, size.Height);
                return state.Path.FillContains(state.Point);
            },
            stroke: static (state, k) => { state.Path.AddLine(k.From.X, k.From.Y, k.To.X, k.To.Y); return state.Path.StrokeContains(state.Probe, state.Point); },
            path: static (state, pp) => { state.Path.AddLines(pp.Points.Map(static z => new Eto.Drawing.PointF(z.X, z.Y)).AsEnumerable()); _ = Op.SideWhen(pp.Closed, state.Path.CloseFigure); return pp.Closed ? state.Path.FillContains(state.Point) : state.Path.StrokeContains(state.Probe, state.Point); },
            curve: static (state, cc) => { _ = cc.Segs.Iter(s => s.AddTo(state.Path)); _ = Op.SideWhen(cc.Closed, state.Path.CloseFigure); return cc.Closed ? state.Path.FillContains(state.Point) : state.Path.StrokeContains(state.Probe, state.Point); },
            box: static (state, b) => b.Corner > 0f ? RoundedHit(bounds: b.Bounds, corner: b.Corner, point: state.Point) : RectHit(path: state.Path, bounds: b.Bounds, point: state.Point),
            sprite: static (state, s) => { System.Drawing.PointF a = s.Value.Place.Anchor; return Math.Abs(state.Point.X - a.X) <= state.Tolerance && Math.Abs(state.Point.Y - a.Y) <= state.Tolerance; });

        static bool RectHit(Eto.Drawing.GraphicsPath path, System.Drawing.Rectangle bounds, Eto.Drawing.PointF point) {
            path.AddRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            return path.FillContains(point);
        }

        static bool RoundedHit(System.Drawing.Rectangle bounds, float corner, Eto.Drawing.PointF point) {
            Eto.Drawing.RectangleF rect = new(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            Eto.Drawing.IGraphicsPath rounded = Eto.Drawing.GraphicsPath.GetRoundRect(rect, corner);
            try {
                return rounded.FillContains(point);
            } finally {
                _ = Optional(rounded as IDisposable).Iter(static disposable => disposable.Dispose());
            }
        }
    }
}

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record UiSpritePlace {
    private UiSpritePlace() { }
    public sealed record Screen(System.Drawing.PointF At) : UiSpritePlace;
    public sealed record World(Point3d At) : UiSpritePlace;
    public sealed record Cloud(Seq<Point3d> At, Option<Seq<DrawingColor>> Colors = default) : UiSpritePlace;   // one GPU batch (instanced draw-list)

    internal System.Drawing.PointF Anchor => Switch(   // canvas rail has no projection — world/cloud degrade to (X,Y) of the first point
        screen: static s => s.At,
        world: static w => new System.Drawing.PointF((float)w.At.X, (float)w.At.Y),
        cloud: static cl => cl.At.IsEmpty ? System.Drawing.PointF.Empty : new System.Drawing.PointF((float)cl.At[0].X, (float)cl.At[0].Y));
}

[Union]
public abstract partial record UiSurface {
    private UiSurface() { }
    public sealed record Pipeline(DisplayPipeline Display, UiSpriteAtlas? Atlas = null) : UiSurface;   // viewport screen-space 2D (Atlas optional)
    public sealed record Canvas(Eto.Drawing.Graphics Graphics, UiSpriteAtlas? Atlas = null) : UiSurface;   // Eto Drawable (Atlas caches decoded sprites)

    internal Unit Text(string value, System.Drawing.PointF at, DrawingColor color, UiFont font, UiAnchor anchor) => Switch(
        pipeline: p => Op.Side(() => {
            System.Drawing.Rectangle box = p.Display.Measure2dText(text: value, definitionPoint: Point2d.Origin, middleJustified: false, rotationRadians: 0d, height: font.Height, fontFace: font.Face);
            System.Drawing.PointF origin = TextOrigin(anchor: anchor, at: at, size: new System.Drawing.SizeF(box.Width, box.Height));
            p.Display.Draw2dText(text: value, color: color, screenCoordinate: new Point2d(origin.X, origin.Y), middleJustified: false, height: font.Height, fontface: font.Face);
        }),
        canvas: c => Op.Side(() => {
            using Eto.Drawing.Font face = font.ToEto();
            Eto.Drawing.SizeF measured = c.Graphics.MeasureString(face, value);
            System.Drawing.PointF origin = TextOrigin(anchor: anchor, at: at, size: new System.Drawing.SizeF(measured.Width, measured.Height));
            c.Graphics.DrawText(face, color.ToEto(), origin.X, origin.Y, value);
        }));
    internal Unit Stroke(System.Drawing.PointF from, System.Drawing.PointF to, UiStroke stroke) => Switch(
        pipeline: p => Op.Side(() => p.Display.Draw2dLine(from: from, to: to, color: stroke.Color, thickness: stroke.Width)),
        canvas: c => Op.Side(() => {
            using Eto.Drawing.Pen pen = stroke.ToPen();
            c.Graphics.DrawLine(pen, new Eto.Drawing.PointF(from.X, from.Y), new Eto.Drawing.PointF(to.X, to.Y));
        }));
    internal Unit Path(Seq<System.Drawing.PointF> points, UiStroke stroke, Option<UiFill> fill, bool closed) => Switch(
        pipeline: p => Polyline(display: p.Display, pts: points, stroke: stroke, closed: closed),
        canvas: c => Op.Side(() => {
            Eto.Drawing.PointF[] eto = [.. points.Map(static p => new Eto.Drawing.PointF(p.X, p.Y))];
            _ = Op.SideWhen(closed, () => fill.Iter(f => f.UseBrush(paint: brush => c.Graphics.FillPolygon(brush, eto))));
            using Eto.Drawing.Pen pen = stroke.ToPen();
            _ = closed switch { true => Op.Side(() => c.Graphics.DrawPolygon(pen, eto)), false => Op.Side(() => c.Graphics.DrawLines(pen, eto)) };
        }));
    internal Unit Box(System.Drawing.Rectangle bounds, UiStroke stroke, UiFill fill, float corner) => Switch(
        pipeline: p => Op.Side(() => p.Display.DrawRoundedRectangle(
            center: new System.Drawing.PointF(bounds.X + (bounds.Width / 2f), bounds.Y + (bounds.Height / 2f)),
            pixelWidth: bounds.Width, pixelHeight: bounds.Height, cornerRadius: corner,
            strokeColor: stroke.Color, strokeWidth: stroke.Width, fillColor: fill.Primary)),
        canvas: c => Op.Side(() => {
            using Eto.Drawing.Pen pen = stroke.ToPen();
            Eto.Drawing.RectangleF rect = new(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            _ = fill.UseBrush(paint: brush => _ = corner > 0f
                ? Op.Side(() => {
                    Eto.Drawing.IGraphicsPath path = Eto.Drawing.GraphicsPath.GetRoundRect(rect, corner);
                    try {
                        c.Graphics.FillPath(brush, path);
                        c.Graphics.DrawPath(pen, path);
                    } finally {
                        _ = Optional(path as IDisposable).Iter(static disposable => disposable.Dispose());
                    }
                })
                : Op.Side(() => { c.Graphics.FillRectangle(brush, rect); c.Graphics.DrawRectangle(pen, rect); }));
        }));
    internal Unit Sprite(UiSprite sprite) => Switch(
        pipeline: p => Optional(p.Atlas).Bind(atlas => atlas.Resolve(source: sprite.Source)).Iter(bitmap => Draw(display: p.Display, bitmap: bitmap, sprite: sprite)),
        canvas: c => Optional(c.Atlas).Bind(atlas => atlas.ResolveEto(source: sprite.Source)).Iter(image => Op.Side(() => c.Graphics.DrawImage(image, new Eto.Drawing.PointF(sprite.Place.Anchor.X, sprite.Place.Anchor.Y)))));

    internal Unit Curve(Seq<UiCurveSeg> segs, UiStroke stroke, Option<UiFill> fill, bool closed) => Switch(
        pipeline: p => CurvePipeline(display: p.Display, segs: segs, stroke: stroke, closed: closed),
        canvas: c => Op.Side(() => {
            using Eto.Drawing.GraphicsPath path = new();
            _ = segs.Iter(seg => seg.AddTo(path));
            _ = Op.SideWhen(closed, path.CloseFigure);
            _ = Op.SideWhen(closed, () => fill.Iter(f => f.UseBrush(paint: brush => c.Graphics.FillPath(brush, path))));
            using Eto.Drawing.Pen pen = stroke.ToPen();
            c.Graphics.DrawPath(pen, path);
        }));
    internal Fin<System.Drawing.SizeF> MeasureText(string value, UiFont font) => Switch(
        pipeline: p => {
            System.Drawing.Rectangle box = p.Display.Measure2dText(text: value, definitionPoint: Point2d.Origin, middleJustified: false, rotationRadians: 0d, height: font.Height, fontFace: font.Face);
            return Fin.Succ(value: new System.Drawing.SizeF(box.Width, box.Height));
        },
        canvas: c => {
            using Eto.Drawing.Font face = font.ToEto();
            Eto.Drawing.SizeF measured = c.Graphics.MeasureString(face, value);
            return Fin.Succ(value: new System.Drawing.SizeF(measured.Width, measured.Height));
        });
    internal Fin<Unit> Clip(System.Drawing.RectangleF region, Func<Fin<Unit>> draw) => Switch(
        pipeline: _ => draw(),
        canvas: c => {
            c.Graphics.SetClip(new Eto.Drawing.RectangleF(region.X, region.Y, region.Width, region.Height));
            try { return draw(); } finally { c.Graphics.ResetClip(); }
        });
    internal Fin<Unit> Transform(System.Drawing.PointF translate, float scale, float rotation, Func<Fin<Unit>> draw) => Switch(
        pipeline: _ => draw(),
        canvas: c => {
            using IDisposable state = c.Graphics.SaveTransformState();
            c.Graphics.TranslateTransform(translate.X, translate.Y);
            c.Graphics.ScaleTransform(scale);
            c.Graphics.RotateTransform(rotation);
            return draw();
        });

    internal static System.Drawing.PointF TextOrigin(UiAnchor anchor, System.Drawing.PointF at, System.Drawing.SizeF size) {
        float x = anchor switch {
            UiAnchor.TopLeft or UiAnchor.MiddleLeft or UiAnchor.BottomLeft => at.X,
            UiAnchor.TopCenter or UiAnchor.Center or UiAnchor.BottomCenter => at.X - (size.Width / 2f),
            _ => at.X - size.Width,
        };
        float y = anchor switch {
            UiAnchor.TopLeft or UiAnchor.TopCenter or UiAnchor.TopRight => at.Y,
            UiAnchor.MiddleLeft or UiAnchor.Center or UiAnchor.MiddleRight => at.Y - (size.Height / 2f),
            _ => at.Y - size.Height,
        };
        return new System.Drawing.PointF(x, y);
    }
    private static Unit Draw(DisplayPipeline display, DisplayBitmap bitmap, UiSprite sprite) {
        (BlendMode source, BlendMode destination) = sprite.Blend.IfNone((BlendMode.SourceAlpha, BlendMode.OneMinusSourceAlpha));
        bitmap.SetBlendFunction(source: source, destination: destination);
        DrawingColor tint = sprite.Tint.IfNone(DrawingColor.White);
        return sprite.Place.Switch(
            state: (Display: display, Bitmap: bitmap, Sprite: sprite, Tint: tint),
            screen: static (state, s) => Op.Side(() => state.Display.DrawSprite(bitmap: state.Bitmap, screenLocation: new Point2d(s.At.X, s.At.Y), size: state.Sprite.Size, blendColor: state.Tint)),
            world: static (state, w) => Op.Side(() => state.Display.DrawSprite(bitmap: state.Bitmap, worldLocation: w.At, size: state.Sprite.Size, blendColor: state.Tint, sizeInWorldSpace: true)),
            cloud: static (state, cl) => Op.Side(() => {
                DisplayBitmapDrawList list = new();
                _ = cl.Colors.Case switch {
                    Seq<DrawingColor> cs => Op.Side(() => list.SetPoints(points: cl.At.AsEnumerable(), colors: cs.AsEnumerable())),
                    _ => Op.Side(() => list.SetPoints(points: cl.At.AsEnumerable(), blendColor: state.Tint)),
                };
                _ = list.Sort(cameraDirection: state.Display.Viewport.CameraDirection);
                state.Display.DrawSprites(bitmap: state.Bitmap, items: list, size: state.Sprite.Size, sizeInWorldSpace: true);
            }));
    }
    private static Unit Polyline(DisplayPipeline display, Seq<System.Drawing.PointF> pts, UiStroke stroke, bool closed) =>
        pts.IsEmpty ? unit : Op.Side(() => {
            Seq<Point3d> world = pts.Map(static q => new Point3d(q.X, q.Y, 0d));
            using PolylineCurve curve = new(points: (closed ? world.Add(world[0]) : world).AsEnumerable());
            _ = new UiRenderState(Screen2d: true).Scope(pipeline: display, draw: () => display.DrawCurve(curve: curve, pen: stroke.ToDisplayPen()));
        });
    private static Unit CurvePipeline(DisplayPipeline display, Seq<UiCurveSeg> segs, UiStroke stroke, bool closed) {
        Seq<Curve> parts = toSeq(segs.Choose(static seg => seg.ToCurve()));
        return parts.Count == segs.Count && !parts.IsEmpty
            ? Op.Side(() => {
                try {
                    using PolyCurve poly = new();
                    _ = parts.Iter(part => poly.Append(part));
                    _ = Op.SideWhen(closed, () => poly.Append(new Line(poly.PointAtEnd, poly.PointAtStart)));
                    _ = new UiRenderState(Screen2d: true).Scope(pipeline: display, draw: () => display.DrawCurve(curve: poly, pen: stroke.ToDisplayPen()));
                } finally {
                    _ = parts.Iter(static part => part.Dispose());
                }
            })
            : Op.Side(() => {
                _ = parts.Iter(static part => part.Dispose());
                _ = Polyline(display: display, pts: segs.Bind(static seg => seg.Sample(steps: 24)), stroke: stroke, closed: closed);
            });
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct UiCanvasContext<TState>(MousePhase Phase, TState State, Eto.Forms.MouseEventArgs Args) : IUiInput<TState> {
    public Eto.Drawing.PointF Location => Args.Location;
    public bool Shift => Args.Modifiers.HasFlag(Eto.Forms.Keys.Shift);
    public bool Control => Args.Modifiers.HasFlag(Eto.Forms.Keys.Control);
    public bool Alt => Args.Modifiers.HasFlag(Eto.Forms.Keys.Alt);
    public Eto.Drawing.SizeF Scroll => Args.Delta;
    public Eto.Forms.MouseButtons Buttons => Args.Buttons;
    public bool Primary => Args.Buttons.HasFlag(Eto.Forms.MouseButtons.Primary);
    public bool Secondary => Args.Buttons.HasFlag(Eto.Forms.MouseButtons.Alternate);
    public bool Middle => Args.Buttons.HasFlag(Eto.Forms.MouseButtons.Middle);
    public float Pressure => Args.Pressure;
}

public readonly record struct UiCanvasKey<TState>(KeyPhase Phase, TState State, Eto.Forms.KeyEventArgs Args) : IUiInput<TState> {
    public bool Shift => Args.Shift;
    public bool Control => Args.Control;
    public bool Alt => Args.Alt;
    public Eto.Forms.Keys Key => Args.Key;
}

public readonly record struct UiHud(Seq<UiMark> Marks) {
    public static UiHud Empty => new(Seq<UiMark>());
    public static UiHud operator +(UiHud left, UiHud right) => new(left.Marks + right.Marks);
    public UiHud Add(UiMark mark) => new(Marks.Add(mark));
    internal Fin<Unit> Render(UiSurface surface) => Marks.TraverseM(mark => mark.Render(surface: surface)).As().Map(static _ => unit);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiHudLayout(System.Drawing.RectangleF Bounds, float DpiScale = 1f) {
    public static UiHudLayout Of(RhinoViewport viewport, float dpiScale) {
        ArgumentNullException.ThrowIfNull(argument: viewport);
        System.Drawing.Size size = viewport.Size;
        return new(Bounds: new System.Drawing.RectangleF(0f, 0f, size.Width, size.Height), DpiScale: dpiScale);
    }
    public float Scale(float px) => px * DpiScale;
    public System.Drawing.PointF Place(UiAnchor anchor, System.Drawing.SizeF content, float margin = 8f) {
        float m = margin * DpiScale;
        float x = anchor is UiAnchor.TopLeft or UiAnchor.MiddleLeft or UiAnchor.BottomLeft ? Bounds.Left + m
                : anchor is UiAnchor.TopCenter or UiAnchor.Center or UiAnchor.BottomCenter ? Bounds.Left + ((Bounds.Width - content.Width) / 2f)
                : Bounds.Right - content.Width - m;
        float y = anchor is UiAnchor.TopLeft or UiAnchor.TopCenter or UiAnchor.TopRight ? Bounds.Top + m
                : anchor is UiAnchor.MiddleLeft or UiAnchor.Center or UiAnchor.MiddleRight ? Bounds.Top + ((Bounds.Height - content.Height) / 2f)
                : Bounds.Bottom - content.Height - m;
        return new System.Drawing.PointF(x, y);
    }
    public (System.Drawing.RectangleF Region, UiHudLayout Remaining) Stack(UiAnchor anchor, System.Drawing.SizeF content, float margin = 8f) {
        System.Drawing.PointF at = Place(anchor: anchor, content: content, margin: margin);
        System.Drawing.RectangleF region = new(at.X, at.Y, content.Width, content.Height);
        float m = margin * DpiScale;
        System.Drawing.RectangleF remaining = anchor switch {
            UiAnchor.TopLeft or UiAnchor.TopCenter or UiAnchor.TopRight => new(Bounds.X, region.Bottom + m, Bounds.Width, Bounds.Height - region.Height - (2f * m)),
            UiAnchor.BottomLeft or UiAnchor.BottomCenter or UiAnchor.BottomRight => new(Bounds.X, Bounds.Y, Bounds.Width, region.Top - Bounds.Y - m),
            _ => Bounds,
        };
        return (region, this with { Bounds = remaining });
    }
    public static (Seq<System.Drawing.RectangleF> Regions, UiHudLayout Final) StackMany(UiHudLayout layout, Seq<(UiAnchor Anchor, System.Drawing.SizeF Size)> items) =>
        items.Fold((Seq<System.Drawing.RectangleF>(), layout), static (acc, item) => {
            (System.Drawing.RectangleF region, UiHudLayout remaining) = acc.layout.Stack(anchor: item.Anchor, content: item.Size);
            return (acc.Item1.Add(region), remaining);
        });
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiRenderHint(
    Option<bool> AntiAlias = default,
    Option<Eto.Drawing.ImageInterpolation> Interpolation = default,
    Option<Eto.Drawing.PixelOffsetMode> PixelOffset = default) {
    public static UiRenderHint HighQuality => new(AntiAlias: true, Interpolation: Eto.Drawing.ImageInterpolation.High, PixelOffset: Eto.Drawing.PixelOffsetMode.None);
    public static UiRenderHint operator +(UiRenderHint l, UiRenderHint r) => new(
        AntiAlias: r.AntiAlias.IsSome ? r.AntiAlias : l.AntiAlias,
        Interpolation: r.Interpolation.IsSome ? r.Interpolation : l.Interpolation,
        PixelOffset: r.PixelOffset.IsSome ? r.PixelOffset : l.PixelOffset);
    internal Unit Apply(Eto.Drawing.Graphics g) {
        Option<bool> antiAlias = AntiAlias;   // struct lambdas cannot capture `this`
        Option<Eto.Drawing.ImageInterpolation> interpolation = Interpolation;
        Option<Eto.Drawing.PixelOffsetMode> pixelOffset = PixelOffset;
        return Op.Side(() => {
            _ = antiAlias.Iter(v => g.AntiAlias = v);
            _ = interpolation.Iter(v => g.ImageInterpolation = v);
            _ = pixelOffset.Iter(v => g.PixelOffsetMode = v);
        });
    }
}

public readonly record struct UiSprite(SpriteSource Source, UiSpritePlace Place, float Size = 1f, Option<(BlendMode Source, BlendMode Destination)> Blend = default, Option<DrawingColor> Tint = default);

public readonly record struct UiStroke(DrawingColor Color, float Width = 1f, Seq<float> Dashes = default, float Halo = 0f, DrawingColor HaloColor = default, Eto.Drawing.PenLineJoin Join = Eto.Drawing.PenLineJoin.Miter, Eto.Drawing.PenLineCap Cap = Eto.Drawing.PenLineCap.Square, float Miter = 10f) {
    public static UiStroke Of(DrawingColor color, float width = 1f) => new(Color: color, Width: width);
    internal Eto.Drawing.Pen ToPen() {
        Seq<float> dashes = Dashes;   // struct lambdas cannot capture `this`
        Eto.Drawing.Pen pen = new(Color.ToEto(), Width) { LineJoin = Join, LineCap = Cap, MiterLimit = Miter };
        _ = Op.SideWhen(!dashes.IsEmpty, () => pen.DashStyle = new Eto.Drawing.DashStyle(0f, [.. dashes]));
        return pen;
    }
    internal DisplayPen ToDisplayPen() {
        Seq<float> dashes = Dashes;
        DisplayPen pen = new() {
            Color = Color,
            Thickness = Width,
            HaloThickness = Halo,
            HaloColor = HaloColor.IsEmpty switch { true => Color, false => HaloColor },
            CapStyle = Cap switch { Eto.Drawing.PenLineCap.Butt => LineCapStyle.Flat, Eto.Drawing.PenLineCap.Round => LineCapStyle.Round, _ => LineCapStyle.Square },
            JoinStyle = Join switch { Eto.Drawing.PenLineJoin.Bevel => LineJoinStyle.Bevel, Eto.Drawing.PenLineJoin.Round => LineJoinStyle.Round, _ => LineJoinStyle.Miter },
        };
        _ = Op.SideWhen(!dashes.IsEmpty, () => pen.SetPattern(dashesAndGaps: dashes.AsEnumerable()));
        return pen;
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class UiCanvas<TState> : Eto.Forms.Drawable {
    private readonly UiSpriteAtlas atlas = new();
    private readonly Atom<TState> state;
    private readonly Func<TState, Eto.Drawing.Size, Fin<UiHud>> paint;
    private readonly Func<UiInputEvent<TState>, Fin<MouseDecision<TState>>>? input;
    private readonly Func<Eto.Drawing.Size, Fin<Unit>>? resize;
    private readonly UiRenderHint hint;

    public UiCanvas(
        TState initial,
        Func<TState, Eto.Drawing.Size, Fin<UiHud>> paint,
        Func<UiInputEvent<TState>, Fin<MouseDecision<TState>>>? input = null,
        Func<Eto.Drawing.Size, Fin<Unit>>? resize = null,
        UiRenderHint hint = default) {
        state = Atom(initial);
        this.paint = paint;
        this.input = input;
        this.resize = resize;
        this.hint = hint;
        CanFocus = input is not null;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(this);
    }

    public TState State => state.Value;

    public Fin<Unit> Transition(Func<TState, TState> transition) =>
        Op.Of(name: nameof(Transition)).Need(transition).Map(apply => { _ = state.Swap(apply); Invalidate(); return unit; });

    protected override void OnPaint(Eto.Forms.PaintEventArgs e) =>
        _ = RhinoUi.Protect(valid: () => { _ = hint.Apply(g: e.Graphics); return paint(arg1: state.Value, arg2: Size).Bind(hud => hud.Render(surface: new UiSurface.Canvas(Graphics: e.Graphics, Atlas: atlas))); });

    protected override void OnMouseDown(Eto.Forms.MouseEventArgs e) {
        if (input is not null) Focus();
        _ = Pointer(phase: MousePhase.Down, args: e);
    }
    protected override void OnMouseMove(Eto.Forms.MouseEventArgs e) => _ = Pointer(phase: MousePhase.Move, args: e);
    protected override void OnMouseUp(Eto.Forms.MouseEventArgs e) => _ = Pointer(phase: MousePhase.Up, args: e);
    protected override void OnMouseDoubleClick(Eto.Forms.MouseEventArgs e) => _ = Pointer(phase: MousePhase.DoubleClick, args: e);
    protected override void OnMouseWheel(Eto.Forms.MouseEventArgs e) => _ = Pointer(phase: MousePhase.Wheel, args: e);
    protected override void OnKeyDown(Eto.Forms.KeyEventArgs e) => _ = Optional(input).Iter(handler => Commit(() => handler(new UiInputEvent<TState>.CanvasKey(Value: new UiCanvasKey<TState>(Phase: KeyPhase.Down, State: state.Value, Args: e))), handled: cancel => e.Handled = cancel));
    protected override void OnKeyUp(Eto.Forms.KeyEventArgs e) => _ = Optional(input).Iter(handler => Commit(() => handler(new UiInputEvent<TState>.CanvasKey(Value: new UiCanvasKey<TState>(Phase: KeyPhase.Up, State: state.Value, Args: e))), handled: cancel => e.Handled = cancel));
    protected override void OnSizeChanged(EventArgs e) => _ = Optional(resize).Iter(handler => _ = RhinoUi.Protect(valid: () => handler(Size)));

    private Unit Pointer(MousePhase phase, Eto.Forms.MouseEventArgs args) =>
        Optional(input).Iter(handler => Commit(() => handler(new UiInputEvent<TState>.CanvasPointer(Value: new UiCanvasContext<TState>(Phase: phase, State: state.Value, Args: args))), handled: cancel => args.Handled = cancel));
    private Unit Commit(Func<Fin<MouseDecision<TState>>> run, Action<bool> handled) =>
        RhinoUi.Deliver(
            state: state,
            run: _ => run().Map(decision => (decision.State, decision)),
            apply: decision => { ToolTip = decision.ToolTip.IfNone(string.Empty); handled(decision.Cancel); Invalidate(); },
            failed: error => {
                if (error is not Fault.Cancelled) RhinoApp.WriteLine($"Rasm canvas commit failed: {error.Message}");
            });
    protected override void Dispose(bool disposing) {
        if (disposing) {
            atlas.Dispose();
        }
        base.Dispose(disposing);
    }
}

public sealed class UiSpriteAtlas : IDisposable {
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SpriteSource, DisplayBitmap> cache = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SpriteSource, Eto.Drawing.Bitmap> etoCache = new();
    private int disposed;

    public static UiSpriteAtlas Empty => new();
    internal Option<DisplayBitmap> Resolve(SpriteSource source) =>
        Volatile.Read(ref disposed) == 0
            ? Optional(cache.GetOrAdd(key: source, valueFactory: static key => key.Load()))
            : Option<DisplayBitmap>.None;
    internal Option<Eto.Drawing.Bitmap> ResolveEto(SpriteSource source) =>
        Volatile.Read(ref disposed) == 0
            ? Optional(etoCache.GetOrAdd(key: source, valueFactory: static key => key.ToEtoBitmap()))
            : Option<Eto.Drawing.Bitmap>.None;
    public void Dispose() {
        _ = Interlocked.Exchange(ref disposed, 1) == 1
            ? unit
            : Op.Side(() => {
                _ = toSeq(cache.Values).Iter(static bitmap => bitmap.Dispose());
                _ = toSeq(etoCache.Values).Iter(static bitmap => bitmap.Dispose());
            });
        GC.SuppressFinalize(obj: this);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class UiInput {
    extension<TState>(IUiInput<TState> input) {
        public MouseDecision<TState> Pass => MouseDecision.Pass(input.State);

        public MouseDecision<TState> Next(TState state, bool cancel = false, Option<string> toolTip = default) =>
            MouseDecision.Next(state, cancel, toolTip);

        public MouseDecision<TState> Hint(string value) => MouseDecision.Hint(input.State, value);
    }
}

internal static class DrawingConvert {
    internal static EtoColor ToEto(this DrawingColor color) => EtoColor.FromArgb(color.R, color.G, color.B, color.A);
}
