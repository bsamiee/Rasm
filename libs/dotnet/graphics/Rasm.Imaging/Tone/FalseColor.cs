using System.Numerics;
using Rasm.Imaging.Color;

namespace Rasm.Imaging.Tone;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FalseColor {
    private static readonly ColorEncoding Signal = new(Gamut.StandardRgb, Transfer.Gamma.Rec1886);

    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, LogSpace space, Option<Action<Span<Vector4>>> look, LutTable table, Lut1D key) =>
        device.Formed(working, space.Gamut, Positioned(space, look, table).Add(new LutTable.ChannelCurve(key).Apply), Signal);

    public static Fin<(Seq<(double At, Vector4 Color)> Bands, Seq<Option<float>> Stops)> Legend(LogSpace space, Option<Action<Span<Vector4>>> look, LutTable table, Lut1D key) {
        Seq<Vector4> codes = toSeq(Range(0, key.Length)).Map(k => new Vector4(key.Channel(0)[k], key.Channel(1)[k], key.Channel(2)[k], 1f)).Strict();
        Seq<(int Entry, Vector4 Code)> bands = codes
            .Zip(Option<Vector4>.None.Cons(codes.Map(static code => Some(code))))
            .Map(static (pair, k) => (Entry: k, Code: pair.First, Before: pair.Second))
            .Filter(static step => step.Before != Some(step.Code))
            .Map(static step => (step.Entry, step.Code));
        Seq<double> openings = bands.Tail.Map(band => (band.Entry - 0.5d) / (key.Length - 1)).Strict();
        Vector4[] colors = [.. bands.Map(static band => band.Code)];
        Vector4[] positions = [.. toSeq(Range(0, key.Length)).Map(i => new Vector4(new Vector3(i / (key.Length - 1f)), 1f))];
        Signal.Transfer.Decode(colors);
        new LutTable.Antilog(space.Allocation).Apply(positions);
        foreach (Action<Span<Vector4>> step in Positioned(space, look, table))
            step(positions);
        return Lut1D.From(key.Length, positions.SelectMany(static position => (float[])[position.X, position.Y, position.Z]).ToArray(), Vector3.Zero, Vector3.One)
            .Bind(response => new LutTable.Sequence([new LutTable.Log(space.Allocation), new LutTable.ChannelCurve(response)]).Inverse())
            .Map(scene => (0d.Cons(openings).Zip(toSeq(colors)).Map(static stop => (At: stop.First, Color: stop.Second)).Strict(), Labels(openings, scene)));

        static Seq<Option<float>> Labels(Seq<double> openings, LutTable scene) {
            Vector4[] edges = [.. openings.Map(static v => new Vector4(new Vector3((float)v), 1f))];
            scene.Apply(edges);
            return Option<float>.None.Cons(toSeq(edges).Map(static edge => Some(float.Log2(edge.X / Exposure.MiddleGray))));
        }
    }

    private static Seq<Action<Span<Vector4>>> Positioned(LogSpace space, Option<Action<Span<Vector4>>> look, LutTable table) =>
        AgX.Opened(space, look) + Seq<Action<Span<Vector4>>>(
            table.Apply, Transfer.Gamma.Rec1886.Decode, AgX.Grey.Apply, new LutTable.Exponent(ExponentStyle.BasicRev, new Vector3(2.5f), Vector3.Zero).Apply);
}
