using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Imaging;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Grading {
    // --- [PLANES]
    private const int Sextants = 6;
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));
    private static readonly Key Only = new Key.Ordinal(0);
    private static readonly PlotPlane.Track Unit = new(Orientation.Horizontal, new Axis.Linear(AxisRange.Create(0f, 1f)));
    private static readonly PlotPlane.Polar Disc = new(new Axis.Periodic(AxisRange.Create(0f, MathF.Tau), Sextants, 0), Some<Axis>(new Axis.Linear(AxisRange.Create(0f, 1f))));
    private static readonly Func<CurveAxis, Node.Image> Hues = memo(static (CurveAxis axis) => Ramps.Field(new PlotPlane.Track(Orientation.Horizontal, axis.Plotted), static at => Saturated(at.X), Gamut.StandardRgb));
    private static readonly Func<Gamut, Node.Image> Tints = memo(static (Gamut gamut) => Ramps.Field(Disc, at => new Vector4(Exposure.MiddleGray * WheelOffset.Tint(gamut, 0.5f * at.Y * new Vector2(MathF.Cos(at.X), MathF.Sin(at.X))), 1f), gamut));

    extension(CurveAxis axis) {
        public AxisRange Ranged => AxisRange.Create((float)axis.Span.Low, (float)axis.Span.High);

        public Axis Plotted => axis.Periodic ? new Axis.Periodic(axis.Ranged, Sextants, 0) : new Axis.Linear(axis.Ranged);
    }

    // --- [CURVES]
    public static Node Curve(
        PointCurve value, TracePlane plane, Option<Traces> histogram, Seq<PointCurve> others, Selection picked, Func<Edit<Selection>, IO<Unit>> pick, Func<Edit<PointCurve>, IO<Unit>> edited) =>
        Curved(value, static curve => curve.Points, static points => ValidationResults.ToFin(PointCurve.Validate(points, out PointCurve? curve), curve).ToOption(), static curve => curve.Fit(),
            (Input: CurveAxis.Stops, Output: CurveAxis.Stops), Some(plane), histogram, others.Map(static other => other.Fit()), picked, pick, edited);

    public static Node Curve<TKind>(HueCurve<TKind> value, Option<Traces> histogram, Selection picked, Func<Edit<Selection>, IO<Unit>> pick, Func<Edit<HueCurve<TKind>>, IO<Unit>> edited)
        where TKind : IHueCurveKind =>
        Curved(value, static curve => curve.Points, static points => ValidationResults.ToFin(HueCurve<TKind>.Validate(points, out HueCurve<TKind>? curve), curve).ToOption(), static curve => curve.Fit(),
            (TKind.Kind.Input.Axis, TKind.Kind.Output), TKind.Kind.Input == InputAxis.Stops ? Some(TracePlane.Luma) : None, histogram, [], picked, pick, edited);

    private static Node Curved<TCurve>(
        TCurve value, Func<TCurve, Seq<CurvePoint>> points, Func<Seq<CurvePoint>, Option<TCurve>> rebuilt, Func<TCurve, Func<double, double>> fit,
        (CurveAxis Input, CurveAxis Output) axes, Option<TracePlane> plane, Option<Traces> histogram, Seq<Func<double, double>> others,
        Selection picked, Func<Edit<Selection>, IO<Unit>> pick, Func<Edit<TCurve>, IO<Unit>> edited) where TCurve : notnull {
        PlotPlane.Cartesian cartesian = new(axes.Input.Plotted, axes.Output.Plotted);
        Seq<Node> ground =
            (axes.Input.Periodic ? Seq<Node>(Hues(axes.Input)) : [])
            + (from traced in plane.ToSeq()
               from traces in histogram.ToSeq()
               select (Node)new Node.Mark(new Figure.Area(axes.Input.Ranged, Underlay(traced.Bins(traces).Row(0), axes))) { Role = ImagingRoles.Trace })
            + Seq<Node>(new Node.Mark(new Figure.Grid(Orientation.Horizontal)) { Role = Role.Grid }, new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = Role.Grid })
            + axes.Input.Origin.Map(origin => (Node)new Node.Mark(new Figure.Rule(new Vector2((float)origin, axes.Output.Ranged.From), new Vector2((float)origin, axes.Output.Ranged.To))) { Role = ImagingRoles.Major }).ToSeq()
            + axes.Output.Origin.Map(origin => (Node)new Node.Mark(new Figure.Rule(new Vector2(axes.Input.Ranged.From, (float)origin), new Vector2(axes.Input.Ranged.To, (float)origin))) { Role = ImagingRoles.Major }).ToSeq()
            + (axes.Input == axes.Output ? Seq<Node>(new Node.Mark(new Figure.Rule(new Vector2(axes.Input.Ranged.From), new Vector2(axes.Input.Ranged.To))) { Role = ImagingRoles.GridFine }) : [])
            + others.Map(other => (Node)new Node.Mark(Fitted(other, axes.Input)) { Role = ImagingRoles.Trace })
            + Seq<Node>(plane.Match(
                Some: traced => new Node.Mark(Fitted(fit(value), axes.Input)) { Role = ImagingRoles.Series, Series = Scopes.Seriesed(traced) },
                None: () => new Node.Mark(Fitted(fit(value), axes.Input)) { Role = ImagingRoles.Trace }));
        return new Handles<TCurve>(value, new Points<TCurve>(
                curve => points(curve).Map(static (point, index) => ((Key)new Key.Ordinal((uint)index), new Vector2((float)point.X, (float)point.Y))),
                (curve, targets, _) => (rebuilt(points(curve).Map((point, index) => targets.Find(target => Index(target.Key) == Some(index))
                    .Map(static target => new CurvePoint(target.To.X, target.To.Y)).IfNone(point))).IfNone(curve), targets.Map(static target => target.Key)),
                Spoken) {
            Placed = Some<Func<TCurve, Vector2, Option<Key>, Option<(TCurve Value, Key Key)>>>((curve, at, _) =>
                points(curve).Filter(point => point.X < at.X).Count switch {
                    var index => rebuilt(points(curve).Take(index).Add(new CurvePoint(at.X, double.Clamp(fit(curve)(at.X), axes.Output.Span.Low, axes.Output.Span.High))).Concat(points(curve).Skip(index)))
                        .Map(inserted => (inserted, (Key)new Key.Ordinal((uint)index))),
                }),
            Removed = Some<Func<TCurve, Seq<Key>, Option<TCurve>>>((curve, keys) => rebuilt(Kept(points(curve), index => !keys.Exists(key => Index(key) == Some(index))))),
            Torn = Some<Func<TCurve, Key, Option<TCurve>>>((curve, key) => rebuilt(Kept(points(curve), index => Index(key) != Some(index)))),
        },
            edited) {
            Picked = picked,
            Pick = Some(pick),
        }.Drawn(cartesian, ground) with {
            Role = ImagingRoles.Well,
            Width = Grown,
            Aspect = Factor.Create(1f),
        };
    }

    private static Figure.Curve Fitted(Func<double, double> fit, CurveAxis input) =>
        new(input.Ranged, toSeq(Range(0, RampTable.Texels)).Map(sample => (float)fit(input.Span.Low + (sample * (input.Span.High - input.Span.Low) / (RampTable.Texels - 1)))));

    private static Seq<float> Underlay(ReadOnlySpan<int> counts, (CurveAxis Input, CurveAxis Output) axes) {
        float[] taken = new float[RampTable.Texels];
        for (int sample = 0; sample < taken.Length; sample++)
            taken[sample] = counts[HistogramAxis.Bin(CurveAxis.FromStops(axes.Input.Span.Low + (sample * (axes.Input.Span.High - axes.Input.Span.Low) / (taken.Length - 1))))];
        float peak = float.Max(1f, TensorPrimitives.Max<float>(taken));
        Axis output = axes.Output.Plotted;
        return toSeq(taken).Map(count => output.Value(count / peak)).Strict();
    }

    // --- [WHEEL]
    public static Node Wheel(WheelOffset value, Gamut gamut, Func<Edit<WheelOffset>, IO<Unit>> edited) =>
        new PlotPlane.Track(Orientation.Vertical, new Axis.Linear(AxisRange.Create(WheelLuma.MinValue, WheelLuma.MaxValue))) switch {
            var luma => new Node.Flex(Orientation.Horizontal, Seq(
                new Handles<WheelOffset>(value, new Points<WheelOffset>(
                        static offset => Seq((Only, new Vector2(offset.Hue, offset.Strength))),
                        static (offset, targets, _) => (targets.Last.Bind(target =>
                                (WheelHue.Validated(float.Min(target.To.X, WheelHue.MaxValue)), AxisFraction.Validated(float.Clamp(target.To.Y, 0f, 1f)))
                                    .Apply((hue, strength) => offset with { Hue = hue, Strength = strength }).As().ToOption())
                            .IfNone(offset), targets.Map(static target => target.Key)),
                        static at => Spoken(at with { X = float.RadiansToDegrees(at.X) })),
                    edited) {
                    Rest = Some(value with { Hue = WheelHue.Origin, Strength = AxisFraction.MinValue }),
                }.Drawn(Disc, [Tints(gamut), new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = ImagingRoles.GridFine }],
                    Some<Func<Key, Vector2, Node>>(static (_, at) => new Node.Pin(at, new Node.Box { Role = ImagingRoles.Puck }) { Align = UnitPoint.Center })) with {
                    Width = Grown,
                    Aspect = Factor.Create(1f),
                },
                new Handles<WheelOffset>(value, Points<WheelOffset>.Scalar(
                        luma,
                        Lens<WheelOffset, float>.New(static offset => offset.Luma, static shown => offset => offset with { Luma = WheelLuma.Create(float.Clamp(shown, WheelLuma.MinValue, WheelLuma.MaxValue)) }),
                        static shown => shown.ToString("F2", CultureInfo.InvariantCulture)),
                    edited) {
                    Rest = Some(value with { Luma = WheelLuma.Neutral }),
                }.Drawn(luma, [new Node.Box { Role = Role.Track, Height = Grown }],
                    Some<Func<Key, Vector2, Node>>(static (_, at) => new Node.Pin(at, new Node.Box { Role = ImagingRoles.Knob }) { Align = UnitPoint.Center })) with {
                    Width = Sizing.Fit.Content,
                })) {
                Role = ImagingRoles.Well,
                Width = Grown,
            },
        };

    // --- [RANGE]
    public static Node Qualified(HueRange value, RangeAxis axis, Option<Func<HueRange, Vector4, Vector4>> grade, Func<Edit<HueRange>, IO<Unit>> edited) {
        Axis along = axis.Input.Axis.Plotted;
        PlotPlane.Track track = new(Orientation.Horizontal, along);
        RangeBand band = axis.Band.Get(value);
        Hsy reference = toSeq(RangeAxis.Items).Fold(new Hsy(0d, 0d, 0d), (hsy, item) => item.Replaced(hsy, item.Band.Get(value).Center));
        Seq<RangeHandle> handles = toSeq(RangeHandle.Items);
        Func<Vector2, Vector4> input = at =>
            axis.Map(hue: Saturated(at.X), saturation: axis.Replaced(reference, along.Fraction(at.X)).ToRgb(1f), luma: axis.Replaced(reference, along.Fraction(at.X)).ToRgb(1f));
        Node bar = new Handles<HueRange>(value, new Points<HueRange>(
                range => handles.Map((handle, index) => ((Key)new Key.Ordinal((uint)index), new Vector2(along.Value(handle.Position(axis.Band.Get(range))), 0.5f))),
                (range, targets, modifiers) => (targets.Fold(range, (held, target) =>
                    (from index in Index(target.Key)
                     from handle in handles.At(index)
                     let start = axis.Band.Get(range)
                     let picked = modifiers.Contains(Modifier.Alternate) ? handle.Alternate.IfNone(handle) : handle
                     let travel = along.Delta(along.Value(handle.Position(start)), target.To.X) / along.Range.Span
                     from moved in axis.Moved(held, picked, picked.Position(start) + travel).ToOption()
                     select moved).IfNone(held)), targets.Map(static target => target.Key)),
                at => Spoken(at with { X = along.Fraction(at.X) })),
            edited) {
            Rest = Some(axis.Band.Set(axis.Band.Get(HueRange.Default), value)),
        }.Drawn(track, [
                Ramps.Field(track, input, Gamut.StandardRgb),
                new Node.Mark(new Figure.Area(along.Range, toSeq(Range(0, RampTable.Texels)).Map(sample =>
                    band.Membership(axis.Distance((float)sample / (RampTable.Texels - 1), band.Center))))) { Role = Role.Selection },
            ],
            Some<Func<Key, Vector2, Node>>((key, at) => new Node.Pin(at, Index(key).Bind(index => handles.At(index)).Map(static handle => handle.Map(
                    band: new Node.Box { Role = Role.Handle },
                    lowerSoftness: new Node.Box { Role = ImagingRoles.Stop },
                    upperSoftness: new Node.Box { Role = ImagingRoles.Stop },
                    lowerEdge: new Node.Box { Role = ImagingRoles.Needle, Height = Grown },
                    upperEdge: new Node.Box { Role = ImagingRoles.Needle, Height = Grown }))
                .IfNone(static () => new Node.Box { Role = Role.Handle })) { Align = UnitPoint.Center })) with {
            Role = ImagingRoles.Well,
            Width = Grown,
        };
        return grade.Match(
            Some: graded => new Node.Flex(Orientation.Vertical, Seq(bar, Ramps.Field(track, at => graded(value, input(at)), Gamut.StandardRgb) with { Role = ImagingRoles.Well, Width = Grown })) {
                Width = Grown,
            },
            None: () => bar);
    }

    // --- [GRADIENT]
    public static Node Gradient(Ramp value, Gamut gamut, Selection picked, Func<Edit<Selection>, IO<Unit>> pick, Func<Edit<Ramp>, IO<Unit>> edited) =>
        new Handles<Ramp>(value, new Points<Ramp>(
                static ramp => ramp.Stops.Map(static (stop, index) => ((Key)new Key.Ordinal((uint)index), new Vector2((float)(double)stop.Position, 0.5f))),
                static (ramp, targets, _) => Shifted(ramp, targets),
                static at => at.X.ToString("F3", CultureInfo.InvariantCulture)) {
            Placed = Some<Func<Ramp, Vector2, Option<Key>, Option<(Ramp Value, Key Key)>>>((ramp, at, _) =>
                from position in RampPosition.Validated(float.Clamp(at.X, 0f, 1f)).ToOption()
                let index = ramp.Stops.Filter(stop => stop.Position <= position).Count
                from stop in ValidationResults.ToFin(RampStop.Validate(position, ramp.Tabulate(gamut).Sample(position), out RampStop placed), placed).ToOption()
                from added in ValidationResults.ToFin(Ramp.Validate(ramp.Stops.Take(index).Add(stop).Concat(ramp.Stops.Skip(index)), ramp.Interpolation, out Ramp? added), added).ToOption()
                select (added, (Key)new Key.Ordinal((uint)index))),
            Removed = Some<Func<Ramp, Seq<Key>, Option<Ramp>>>(static (ramp, keys) => Rebuilt(ramp, Kept(ramp.Stops, index => !keys.Exists(key => Index(key) == Some(index))))),
            Torn = Some<Func<Ramp, Key, Option<Ramp>>>(static (ramp, key) => Rebuilt(ramp, Kept(ramp.Stops, index => Index(key) != Some(index)))),
        },
            edited) {
            Picked = picked,
            Pick = Some(pick),
        }.Drawn(Unit, [Ramps.Key(value, gamut, []) with { Width = Grown, Height = Grown }],
            Some<Func<Key, Vector2, Node>>(static (_, at) => new Node.Pin(at, new Node.Box { Role = ImagingRoles.Stop }) { Align = UnitPoint.Center })) with {
            Width = Grown,
        };

    private static (Ramp Value, Seq<Key> Keys) Shifted(Ramp ramp, Seq<(Key Key, Vector2 To)> targets) {
        (Ramp moved, HashMap<int, int> at) = targets.Fold((Ramp: ramp, At: toHashMap(targets.Choose(static target => Index(target.Key)).Map(static index => (index, index)))), static (held, target) =>
            (from original in Index(target.Key)
             from current in held.At.Find(original)
             from position in RampPosition.Validated(float.Clamp(target.To.X, 0f, 1f)).ToOption()
             let shifted = held.Ramp.Moved(current, position)
             select (shifted.Ramp, held.At.Map(index => current < shifted.Index
                 ? index > current && index <= shifted.Index ? index - 1 : index
                 : index >= shifted.Index && index < current ? index + 1 : index).SetItem(original, shifted.Index))).IfNone(held));
        return (moved, targets.Map(target => Index(target.Key).Bind(at.Find).Map(static index => (Key)new Key.Ordinal((uint)index)).IfNone(target.Key)));
    }

    private static Option<Ramp> Rebuilt(Ramp ramp, Seq<RampStop> stops) =>
        ValidationResults.ToFin(Ramp.Validate(stops, ramp.Interpolation, out Ramp? kept), kept).ToOption();

    private static Seq<T> Kept<T>(Seq<T> items, Func<int, bool> keep) => items.Map(static (item, index) => (Item: item, Index: index)).Filter(held => keep(held.Index)).Map(static held => held.Item);

    // --- [SWATCH]
    public static Node Swatch(Vector4 color, Gamut gamut, Func<SKRect, IO<Unit>> pressed) =>
        new Part.Press(pressed).Drawn(new Node.Overlay(new Node.Box { Role = ImagingRoles.Checker, Width = Grown, Height = Grown }, [Ramps.Tabulated(1, 1, None, gamut, (_, _, _, _) => color)]) {
            Role = Role.Field,
        });

    private static Vector4 Saturated(float hue) => new Hsy(hue, 1d, Exposure.MiddleGray).ToRgb(1f);

    private static Option<int> Index(Key key) => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None;

    private static string Spoken(Vector2 at) => string.Create(CultureInfo.InvariantCulture, $"{at.X:F3}, {at.Y:F3}");
}
