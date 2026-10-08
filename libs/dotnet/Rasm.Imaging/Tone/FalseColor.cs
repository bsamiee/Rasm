using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone.Formations;

namespace Rasm.Imaging.Tone;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FalseColor {
    public static readonly Func<Option<AgXLook>, (Ramp Ramp, Seq<Option<float>> Stops)> Key =
        memoUnsafe(static (Option<AgXLook> look) => Derived(AgX.FalseColorBands, look).ThrowIfFail());

    private static Fin<(Ramp Ramp, Seq<Option<float>> Stops)> Derived(Lut1D table, Option<AgXLook> look) {
        Seq<Vector4> codes = toSeq(Range(0, table.Length)).Map(k => new Vector4(table.Channel(0)[k], table.Channel(1)[k], table.Channel(2)[k], 1f)).Strict();
        Seq<(int Entry, Vector4 Code)> bands = codes
            .Zip(Option<Vector4>.None.Cons(codes.Map(static code => Some(code))))
            .Map(static (pair, k) => (Entry: k, Code: pair.First, Before: pair.Second))
            .Filter(static step => step.Before != Some(step.Code))
            .Map(static step => (step.Entry, step.Code));
        Seq<double> openings = bands.Tail.Map(band => (band.Entry - 0.5d) / (table.Length - 1)).Strict();
        Vector4[] colours = [.. bands.Map(static band => band.Code)];
        Vector4[] positions = [.. toSeq(Range(0, table.Length)).Map(i => new Vector4(new Vector3(i / (table.Length - 1f)), 1f))];
        Display.Rec1886.Encoding.Decode(colours);
        new LutTable.Antilog(LogSpace.AgXLog.Allocation).Apply(positions);
        foreach (Action<Span<Vector4>> step in AgX.ToBandPosition(look, Gamut.EGamut))
            step(positions);
        return (
                Lut1D.From(table.Length, positions.SelectMany(static position => (float[])[position.X, position.Y, position.Z]).ToArray(), Vector3.Zero, Vector3.One)
                    .Bind(static response => new LutTable.Sequence([new LutTable.Log(LogSpace.AgXLog.Allocation), new LutTable.ChannelCurve(response)]).Inverse()),
                0d.Cons(openings).Zip(toSeq(colours)).Traverse(static stop =>
                        RampPosition.Validate(stop.First, provider: null, out RampPosition position) is { } error ? error
                        : RampStop.Validate(position, stop.Second, out RampStop item) is { } invalid ? invalid
                        : (Fin<RampStop>)item).As()
                    .Bind(static stops => Ramp.Validate(stops, RampInterpolation.Constant, out Ramp? keyed) is { } refused ? refused : (Fin<Ramp>)keyed!))
            .Apply((scene, ramp) => (ramp, Labels(openings, scene)))
            .As();

        static Seq<Option<float>> Labels(Seq<double> openings, LutTable scene) {
            Vector4[] edges = [.. openings.Map(static v => new Vector4(new Vector3((float)v), 1f))];
            scene.Apply(edges);
            return Option<float>.None.Cons(toSeq(edges).Map(static edge => Some(float.Log2(edge.X / Exposure.MiddleGrey))));
        }
    }
}
