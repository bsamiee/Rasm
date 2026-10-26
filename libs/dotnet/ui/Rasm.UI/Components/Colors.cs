using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ColorField(PlotPlane Plane, Func<Vector2, SKColorF> Color, SKColorSpace Space) {
    public Node Drawn {
        get {
            (int columns, int rows) = Plane.Switch(
                this,
                cartesian: static (held, _) => held.Lattice,
                track: static (held, _) => held.Lattice,
                polar: static (held, _) => held.Lattice switch { var (across, up) => int.Max(across, up) switch { var side => (side, side) } });
            SKColorF[] texels = [.. from row in Enumerable.Range(0, rows) from column in Enumerable.Range(0, columns) select Texel(new SKPoint((column + 0.5f) / columns, 1f - ((row + 0.5f) / rows)))];
            using SKImage table = SKImage.FromPixelCopy(new SKImageInfo(columns, rows, SKColorType.RgbaF32, SKAlphaType.Unpremul, Space), MemoryMarshal.AsBytes(texels.AsSpan()));
            using SKBitmap bitmap = new(new SKImageInfo(columns, rows, SKColorType.RgbaF16Clamped, SKAlphaType.Premul, Space));
            using SKPixmap pixels = bitmap.PeekPixels();
            _ = table.ReadPixels(pixels);
            bitmap.SetImmutable();
            return new Node.Image(SKImage.FromBitmap(bitmap)) {
                Fit = Plane.Aspect.IsSome ? ImageFit.Contain : ImageFit.Stretch,
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            };
        }
    }

    private (int Columns, int Rows) Lattice => (Needed(Orientation.Horizontal), Needed(Orientation.Vertical));

    private Vector2 Middle => Plane.Dimensions.Fold(new Vector2(0.5f), static (value, held) => held.Along.With(value, held.Axis.Value(0.5f)));

    private int Needed(Orientation along) =>
        Plane.Dimension(along).Map(axis => (Seq(0f) + ColorRamp.Divided(at => Color(along.With(Middle, axis.Value(at))), 0f, 1f) + Seq(1f)) switch {
            var cuts => (int)MathF.Ceiling(1f / cuts.Zip(cuts.Tail).Fold(1f, static (least, cut) => float.Min(least, cut.Second - cut.First)) / Plane.Length(along)) + 1,
        }).IfNone(1);

    private SKColorF Texel(SKPoint square) =>
        Plane.Value(square) switch {
            var value => Plane.Dimensions.ForAll(held => held.Axis.Contains(held.Axis.Range, held.Along.Of(value))) ? Color(value) : SKColorF.Empty,
        };
}

public sealed record ColorWheel(PlotPlane.Polar Plane, Vector2 Puck, PlotPlane.Track Track, float Side, Func<Edit<(Vector2 Puck, float Side)>, IO<Unit>> Edited) {
    public Vector2 PuckRest { get; init; } = new(Plane.Angle.Range.From, Plane.Radius.Map(static radius => radius.Range.From).IfNone(0f));
    public float SideRest { get; init; } = Track.Axis.Value(0.5f);
    public Seq<Node> Field { get; init; }

    public Node Drawn =>
        new Node.Flex(Orientation.Horizontal, [
            new Handles<Vector2>(Puck, Points<Vector2>.Fixed([Lens<Vector2, Vector2>.New(static held => held, static at => _ => at)], Plane.Spoken), edit => Edited(edit.Map(puck => (puck, Side)).As())) {
                Rest = Some(PuckRest),
            }.Drawn(Plane, Field.Add(new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = Role.GridFine }),
                Some<Func<Key, Vector2, Node>>(static (_, at) => new Node.Pin(at, new Node.Box { Role = Role.Puck }) { Align = UnitPoint.Center })) with {
                Width = Sizing.Grow.Unit,
                Aspect = Plane.Aspect,
            },
            new Slider(Track, Side, edit => Edited(edit.Map(side => (Puck, side)).As())) { Origin = None, Rest = Some(SideRest) }.Drawn,
        ]) {
            Role = Role.Well,
            Width = Sizing.Grow.Unit,
        };
}

public sealed record Swatch(SKColorF Color, Func<SKRect, IO<Unit>> Pressed) {
    public Node Drawn => new Part.Press(Pressed).Drawn(Face(Color));

    public static Node Face(SKColorF color) =>
        new Node.Overlay(new Node.Box { Role = Role.Checker, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }, [
            new Node.Styled(new Node.Box { Role = Role.Span, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) {
                Sheet = new StyleSheet { Looks = HashMap((Role.Span, new Look { Fills = Some(Seq<Material>(new Material.Solid(new Ink.Fixed(color)))) })) },
            },
        ]) {
            Role = Role.Field,
            Accessible = new Accessible { Value = new AccessValue.Text(new Ink.Fixed(color).ToValue()) },
        };
}

public sealed record SwatchList(Seq<SKColorF> Colors, Selection Picked, Func<Edit<Selection>, IO<Unit>> Pick) {
    private Seq<(Key Key, SKColorF Color)> Keyed => Colors.Map(static (color, index) => ((Key)new Key.Ordinal((uint)index), color));

    public Node Drawn =>
        toHashMap(Keyed) switch {
            var faces => new Part.Choice(Keyed.Map(static held => held.Key), Picked, Pick)
                .Drawn(key => Swatch.Face(faces[key]), static items => new Node.Flex(Orientation.Horizontal, items) { Width = Sizing.Grow.Unit }),
        };
}

public sealed record GradientEditor(ColorRamp Value, Func<Edit<ColorRamp>, IO<Unit>> Edited) {
    private static readonly PlotPlane.Track Track = PlotPlane.Track.Unit(Orientation.Horizontal);

    public Selection Picked { get; init; } = new Selection.Many([], None, None);
    public Option<Func<Edit<Selection>, IO<Unit>>> Pick { get; init; }

    public Node Drawn =>
        new Handles<ColorRamp>(Value, Stops, Edited) { Picked = Picked, Pick = Pick }
            .Drawn(Track, [new GradientKey(Value, []).Drawn], Some<Func<Key, Vector2, Node>>((key, at) => new Node.Pin(at, new Node.Box {
                Role = Role.Stop,
                Conditions = Picked.Holds(key) ? [Condition.Selected] : [],
            }) { Align = UnitPoint.Center })) with {
            Width = Sizing.Grow.Unit,
        };

    private static Points<ColorRamp> Stops { get; } =
        new(static ramp => ramp.Stops.Map(static (stop, index) => ((Key)Keyed(index), new Vector2((float)stop.At, 0.5f))), static (ramp, targets, _) => Moved(ramp, targets), Track.Spoken) {
            Placed = Some<Func<ColorRamp, Vector2, Option<Key>, Option<(ColorRamp Value, Key Key)>>>(static (ramp, at, _) =>
                Fraction.Create(float.Clamp(at.X, 0f, 1f)) switch {
                    var position => ramp.Stops.Count(stop => (float)stop.At <= (float)position) switch {
                        var index => Some((ramp.WithStops(ramp.Stops.Take(index).Add(new Stop<SKColorF>(ramp.At(position), position)) + ramp.Stops.Skip(index)), (Key)Keyed(index))),
                    },
                }),
            Removed = Some<Func<ColorRamp, Seq<Key>, Option<ColorRamp>>>(static (ramp, keys) => Kept(ramp, index => !keys.Exists(key => key == Keyed(index)))),
            Torn = Some<Func<ColorRamp, Key, Option<ColorRamp>>>(static (ramp, key) => Kept(ramp, index => Keyed(index) != key)),
        };

    private static Key.Ordinal Keyed(int index) => new((uint)index);

    private static (ColorRamp Value, Seq<Key> Keys) Moved(ColorRamp ramp, Seq<(Key Key, Vector2 To)> targets) {
        Seq<(Stop<SKColorF> Stop, int Index)> moved = toSeq(ramp.Stops
            .Map((stop, index) => (Stop: targets.Find(target => target.Key == Keyed(index)).Map(target => stop with { At = Fraction.Create(float.Clamp(target.To.X, 0f, 1f)) }).IfNone(stop), Index: index))
            .OrderBy(static held => (float)held.Stop.At));
        HashMap<Key, Key> rekeyed = toHashMap(moved.Map(static (held, index) => ((Key)Keyed(held.Index), (Key)Keyed(index))));
        return (ramp.WithStops(moved.Map(static held => held.Stop)), targets.Map(target => rekeyed[target.Key]));
    }

    private static Option<ColorRamp> Kept(ColorRamp ramp, Func<int, bool> keep) =>
        ramp.Stops.Map(static (stop, index) => (Stop: stop, Index: index)).Filter(held => keep(held.Index)).Map(static held => held.Stop) switch {
            { IsEmpty: true } => None,
            var kept => Some(ramp.WithStops(kept)),
        };
}
