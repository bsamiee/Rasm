using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Swatch(SKColorF Color, Func<SKRect, IO<Unit>> Pressed) {
    public Node Drawn => new Part.Press(Pressed).Drawn(Face(Color));

    public static Node Face(SKColorF color) =>
        new Node.Styled(new Node.Box { Role = Role.Field, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) {
            Sheet = new StyleSheet { Looks = HashMap((Role.Field, new Look { Fills = Some(Seq<Material>(new Material.Solid(new ColorValue.Fixed(color.ToUnicolour())))) })) },
            Accessible = new Accessible { Value = new AccessValue.Text(color.ToUnicolour().Hex) },
        };
}

public sealed record SwatchList(Seq<SKColorF> Colors, Selection Picked, Func<Edit<Selection>, IO<Unit>> Pick) {
    private Seq<(Key Key, SKColorF Color)> Keyed => Colors.Map(static (color, index) => ((Key)new Key.Ordinal((uint)index), color));

    public Node Drawn =>
        toHashMap(Keyed) switch {
            var faces => new Part.Choice(Keyed.Map(static held => held.Key), Picked, Pick)
                .Drawn(key => Swatch.Face(faces[key]), static items => new Node.Flex(PhysicalAxis.Horizontal, items) { Width = Sizing.Grow.Unit }),
        };
}

public sealed record GradientEditor(ColorRamp Value, Func<Edit<ColorRamp>, IO<Unit>> Edited) {
    private static readonly PlotPlane.Track Track = PlotPlane.Track.Unit(PhysicalAxis.Horizontal);

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
                Fraction.TryCreate(float.Clamp(at.X, 0f, 1f), out Fraction? position)
                    ? ramp.At(position).Map(color => ramp.Stops.Count(stop => (float)stop.At <= (float)position) switch {
                        var index => (ramp.WithStops(ramp.Stops.Take(index).Add(new Stop<SKColorF>(color, position)) + ramp.Stops.Skip(index)), (Key)Keyed(index)),
                    })
                    : None),
            Removed = Some<Func<ColorRamp, Seq<Key>, Option<ColorRamp>>>(static (ramp, keys) => Kept(ramp, index => !keys.Exists(key => key == Keyed(index)))),
            Torn = Some<Func<ColorRamp, Key, Option<ColorRamp>>>(static (ramp, key) => Kept(ramp, index => Keyed(index) != key)),
        };

    private static Key.Ordinal Keyed(int index) => new((uint)index);

    private static (ColorRamp Value, Seq<Key> Keys) Moved(ColorRamp ramp, Seq<(Key Key, Vector2 To)> targets) {
        Seq<(Stop<SKColorF> Stop, int Index)> moved = toSeq(ramp.Stops
            .Map((stop, index) => (Stop: targets.Find(target => target.Key == Keyed(index))
                .Bind(target => Fraction.TryCreate(float.Clamp(target.To.X, 0f, 1f), out Fraction? at) ? Some(stop with { At = at }) : None).IfNone(stop), Index: index))
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
