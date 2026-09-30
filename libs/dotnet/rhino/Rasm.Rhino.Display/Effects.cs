using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI;
using Rhino.UI.Controls;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral")]
[ValidationError<ValidationFailure>]
public readonly partial struct Exposure {
    public const float Lower = -32f;

    public const float Upper = 32f;

    public const double Step = 0.01;

    public const int Precision = 3;

    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref float value) =>
        validationError = Limits.AtLeast(Lower).AtMost(Upper).Violated(value, nameof(Exposure));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PixelPass {
    public sealed record Pointwise(Func<Vector4, int, int, Vector4> Pixel) : PixelPass;

    public sealed record Frame(Action<Span<Vector4>, int> Whole) : PixelPass;
}

[SmartEnum<string>]
[ValidationError<ValidationFailure>]
public sealed partial class Dither {
    public static readonly Dither Off = new(nameof(Off), PixelPasses.Noised(static (_, _) => 0.5f));

    public static readonly Dither Triangular = new(nameof(Triangular), PixelPasses.Noised(PixelPasses.Hashed));

    public static readonly Dither BlueNoise = new(nameof(BlueNoise), PixelPasses.Noised(PixelPasses.BlueNoised));

    public static readonly Dither FloydSteinberg = new(nameof(FloydSteinberg), new PixelPass.Frame(PixelPasses.Diffused));

    public PixelPass Pass { get; }
}

public sealed record EffectRow(Guid Id, PostEffectType Type, Option<string> Name, bool On, bool Shown, uint Crc);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class ParameterEffect<TValue, TRaw> : PostEffect
    where TValue : IObjectFactory<TValue, TRaw, ValidationFailure>, IConvertible<TRaw>
    where TRaw : notnull {
    // --- [STATE]
    internal static readonly string Key = typeof(TValue).Name;

    private readonly TValue factory;

    private readonly Func<TValue, PostEffectPipeline, IO<PixelPass>> pass;

    private readonly Func<ParameterEffect<TValue, TRaw>, ParameterSection<TValue, TRaw>> section;

    private readonly Atom<TValue> held;

    protected ParameterEffect(TValue factory, Func<TValue, PostEffectPipeline, IO<PixelPass>> pass, Func<ParameterEffect<TValue, TRaw>, ParameterSection<TValue, TRaw>> section) {
        (this.factory, this.pass, this.section) = (factory, pass, section);
        held = Atom(factory);
    }

    public TValue Value => held.Value;

    // --- [CALLBACKS]
    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Answers.Succeeded(pass(held.Value, pipeline).Bind(formed => RenderPostEffects.Rewrite(pipeline, rect, formed)), ErrorOps.Report);

    public sealed override bool GetParam(string param, ref object v) {
        if (!string.Equals(param, Key, StringComparison.Ordinal))
            return false;
        v = held.Value.ToValue();
        return true;
    }

    public sealed override bool SetParam(string param, object v) =>
        string.Equals(param, Key, StringComparison.Ordinal) && Written(IO.lift(() => Converted(v)));

    public sealed override bool ReadState(PostEffectState state) =>
        Answers.Succeeded(Swapped(IO.lift(() => state.TryGetValue(Key, out TRaw stored) ? Answers.Validated<TValue, TRaw>(stored) : factory)), ErrorOps.Report);

    public sealed override bool WriteState(ref PostEffectState state) =>
        state.SetValue(Key, held.Value.ToValue());

    public sealed override void ResetToFactoryDefaults() => _ = Written(IO.pure(factory));

    private bool Written(IO<TValue> next) =>
        Answers.Succeeded(
            IO.lift(() => BeginChange(RenderContent.ChangeContexts.Program))
                .Bracket(Use: _ => Swapped(next), Fin: _ => IO.lift(() => Refused.Unless(EndChange(), nameof(EndChange))))
                .Bind(_ => IO.lift(Changed)),
            ErrorOps.Report);

    private IO<Unit> Swapped(IO<TValue> next) =>
        next.Map(value => ignore(held.Swap(_ => value)));

    private static Fin<TValue> Converted(object raw) =>
        Answers.Validated<TValue, TRaw>((TRaw)Convert.ChangeType(raw, typeof(TRaw), CultureInfo.InvariantCulture));

    // --- [UI]
    public sealed override void AddUISections(PostEffectUI ui) => ui.AddSection(section(this));

    public sealed override bool DisplayHelp() => false;
}

public abstract class ParameterSection<TValue, TRaw>(ParameterEffect<TValue, TRaw> effect) : EtoPostEffectCollapsibleSection
    where TValue : IObjectFactory<TValue, TRaw, ValidationFailure>, IConvertible<TRaw>
    where TRaw : notnull {
    public sealed override Guid PostEffectId => effect.Id;

    public sealed override LocalizeStringPair Caption { get; } = new(ParameterEffect<TValue, TRaw>.Key, ParameterEffect<TValue, TRaw>.Key);

    public sealed override int SectionHeight => Content.Height;

    protected void Write(TRaw raw) => _ = effect.SetParam(ParameterEffect<TValue, TRaw>.Key, raw);
}

public sealed class CallbackExecutionControl(Func<Guid, IO<bool>> ready, Action<Error> reject) : PostEffectExecutionControl {
    public override bool ReadyToExecutePostEffect(Guid pep_id) =>
        Answers.Answer(ready(pep_id), reject, fallback: false);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class EffectMapper {
    [MapProperty(nameof(PostEffectData.LocalName), nameof(EffectRow.Name))]
    [MapPropertyFromSource(nameof(EffectRow.Crc), Use = nameof(Crc))]
    internal static partial EffectRow ToRow(PostEffectData data);

    private static uint Crc(PostEffectData data) => data.DataCRC(0u);
}

public static class RenderPostEffects {
    // --- [DOCUMENT]
    public static IO<Seq<EffectRow>> Rows(RhinoDoc doc) =>
        WithEffects(doc, static effects => TableOps.ReadRows(effects.ToArray, static data => IO.lift(() => EffectMapper.ToRow(data))));

    public static IO<Unit> SetState(RhinoDoc doc, Guid id, Option<bool> enabled, Option<bool> shown) =>
        WithEffect(doc, id, data =>
            from turned in IO.lift(() => enabled.Iter(value => data.On = value))
            from revealed in IO.lift(() => shown.Iter(value => data.Shown = value))
            select unit);

    public static IO<Unit> Reorder(RhinoDoc doc, Seq<Guid> order) =>
        WithEffects(doc, effects => order.TraverseM(id => IO.lift(() => Refused.Unless(effects.MovePostEffectBefore(id, Guid.Empty), nameof(PostEffectCollection.MovePostEffectBefore)))).As().Map(static _ => unit));

    public static IO<Option<Guid>> Selected(RhinoDoc doc, PostEffectType type) =>
        WithEffects(doc, effects => IO.lift(() => Answers.Found(effects.GetSelectedPostEffect(SelectionNode(type), out Guid id), id).Bind(Answers.Present)));

    public static IO<Unit> Select(RhinoDoc doc, PostEffectType type, Guid id) =>
        WithEffects(doc, effects => IO.lift(() => effects.SetSelectedPostEffect(SelectionNode(type), id)));

    public static IO<IConvertible> GetParameter(RhinoDoc doc, Guid id, string name) =>
        WithEffect(doc, id, data => IO.lift(() => Missing.Unless(data.GetParameter(name), nameof(PostEffectData.GetParameter))));

    private static IO<TValue> WithEffects<TValue>(RhinoDoc doc, Func<PostEffectCollection, IO<TValue>> body) =>
        IO.lift(() => doc.RenderSettings)
            .Map(static settings => (Settings: settings, Effects: settings.PostEffects))
            .Bracket(Use: held => body(held.Effects), Fin: static held => DisposalOps.Release(Seq<IDisposable>(held.Settings, held.Effects)));

    private static IO<TValue> WithEffect<TValue>(RhinoDoc doc, Guid id, Func<PostEffectData, IO<TValue>> body) =>
        WithEffects(doc, effects => DisposalOps.Using(
            IO.lift(() => Answers.Present(effects)),
            rows => IO.lift(rows.Find(row => row.Id == id).ToFin(new UnknownEffect(id))).Bind(body)));

    private static PostEffectType SelectionNode(PostEffectType type) =>
        (PostEffectType)((int)type + 1);

    // --- [PIPELINE]
    public const PostEffectStyles Listed = PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn;

    private static readonly Guid RgbaChannel = RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA);

    public static IO<Unit> Rewrite(PostEffectPipeline pipeline, Rectangle rect, PixelPass pass) =>
        from size in IO.lift(pipeline.Dimensions)
        from rewritten in pass.Switch(
            (Pipeline: pipeline, Rect: rect, Size: size),
            pointwise: static (state, pointwise) => Rewritten(state.Pipeline, state.Rect, values => Mapped(values, state.Rect, state.Size.Height, pointwise.Pixel)),
            frame: static (state, frame) => when(
                state.Rect == new Rectangle(default, state.Size),
                Rewritten(state.Pipeline, state.Rect, values => frame.Whole(MemoryMarshal.Cast<float, Vector4>(values.AsSpan()), state.Rect.Width))).As())
        select rewritten;

    public static IO<Option<float>> OutputGamma(PostEffectPipeline pipeline) =>
        IO.lift(() => Optional(((IPostEffects)pipeline).PostEffectFromId(PostEffectUuids.Gamma))
            .Filter(gamma => gamma.CanExecute(pipeline))
            .Traverse(static gamma => {
                object? value = null;
                return Refused.Unless(gamma.GetParam("gamma", ref value), nameof(PostEffect.GetParam)).Map(_ => (float)value);
            })
            .As());

    private static void Mapped(float[] values, Rectangle rect, int height, Func<Vector4, int, int, Vector4> pixel) =>
        _ = Parallel.For(0, rect.Height, line => {
            Span<Vector4> row = MemoryMarshal.Cast<float, Vector4>(values.AsSpan()).Slice(line * rect.Width, rect.Width);
            for (int column = 0; column < row.Length; column++)
                row[column] = pixel(row[column], rect.X + column, height - 1 - rect.Y - line);
        });

    private static IO<Unit> Rewritten(PostEffectPipeline pipeline, Rectangle rect, Action<float[]> pass) =>
        from values in Opened(pipeline.GetChannelForRead, nameof(PostEffectPipeline.GetChannelForRead), (_, channel) => IO.lift(() => Batches.Values(channel, rect, ComponentOrders.RGBA)))
        from passed in IO.lift(() => pass(values))
        from written in Opened(pipeline.GetChannelForWrite, nameof(PostEffectPipeline.GetChannelForWrite), (opened, channel) =>
            from set in IO.lift(() => channel.SetValues(rect, rect.Size, new PixelBuffer(Marshal.UnsafeAddrOfPinnedArrayElement(values, 0))))
            from committed in IO.lift(opened.Commit)
            select committed)
        select written;

    private static IO<TValue> Opened<TValue>(Func<Guid, PostEffectChannel?> open, string member, Func<PostEffectChannel, RenderWindow.Channel, IO<TValue>> body) =>
        DisposalOps.Using(
            IO.lift(() => Missing.Unless(open(RgbaChannel), member)),
            opened =>
                from channel in IO.lift(() => Missing.Unless(opened.CPU(), nameof(PostEffectChannel.CPU)))
                from value in body(opened, channel)
                select value);
}

public static class PixelPasses {
    // --- [FORMATION]
    private const float SrgbGamma = 2.4f;
    private const float SrgbOffset = 0.055f;
    private static readonly float SrgbBreak = MathF.Pow(SrgbOffset * SrgbGamma / ((SrgbGamma - 1f) * (1f + SrgbOffset)), SrgbGamma);
    private static readonly float SrgbSlope = SrgbOffset / (SrgbGamma - 1f) / SrgbBreak;
    private static readonly (int Size, Vector3[] Cube) Lut = Parsed();

    public static IO<PixelPass> Formation(Exposure exposure, PostEffectPipeline pipeline) =>
        RenderPostEffects.OutputGamma(pipeline).Map<PixelPass>(gamma => {
            float scale = float.Exp2((float)exposure);
            Func<float, float> output = gamma.Match<Func<float, float>>(Some: static power => value => MathF.Pow(value, power), None: static () => static value => value);
            return new PixelPass.Pointwise((pixel, _, _) => Vector4.Create(Formed(scale * pixel.AsVector3(), output), pixel.W));
        });

    private static Vector3 Formed(Vector3 scene, Func<float, float> output) {
        const float lower = -12.47393f;
        const float upper = 12.5260688117f;
        Vector3 gamut = new(
            Vector3.Dot(new(0.5593711333100773f, 0.30478334662300893f, 0.1358455604804771f), scene),
            Vector3.Dot(new(0.07622070548350622f, 0.7879717701028465f, 0.1358074661747215f), scene),
            Vector3.Dot(new(0.06552671177877017f, 0.16454675460372412f, 0.7699265016503745f), scene));
        Vector3 formed = Tetrahedral(Vector3.Clamp((Vector3.Log2(Vector3.Max(gamut, Vector3.Zero)) - Vector3.Create(lower)) / (upper - lower), Vector3.Zero, Vector3.One));
        return new(output(Encoded(formed.X)), output(Encoded(formed.Y)), output(Encoded(formed.Z)));
    }

    private static Vector3 Tetrahedral(Vector3 coordinate) {
        (int size, Vector3[] cube) = Lut;
        Vector3 position = (size - 1) * coordinate;
        Vector3 cell = Vector3.Min(Vector3.Truncate(position), Vector3.Create(size - 2));
        Vector3 fraction = position - cell;
        int origin = (int)cell.X + (size * ((int)cell.Y + (size * (int)cell.Z)));
        ((float Weight, int Step) first, (float Weight, int Step) second, (float Weight, int Step) third) = Descending((fraction.X, 1), (fraction.Y, size), (fraction.Z, size * size));
        return ((1f - first.Weight) * cube[origin])
            + ((first.Weight - second.Weight) * cube[origin + first.Step])
            + ((second.Weight - third.Weight) * cube[origin + first.Step + second.Step])
            + (third.Weight * cube[origin + first.Step + second.Step + third.Step]);
    }

    private static ((float Weight, int Step) First, (float Weight, int Step) Second, (float Weight, int Step) Third) Descending((float Weight, int Step) r, (float Weight, int Step) g, (float Weight, int Step) b) =>
        r.Weight > g.Weight
            ? g.Weight > b.Weight ? (r, g, b) : r.Weight > b.Weight ? (r, b, g) : (b, r, g)
            : b.Weight > g.Weight ? (b, g, r) : b.Weight > r.Weight ? (g, b, r) : (g, r, b);

    private static float Encoded(float formed) {
        const float rec1886 = 2.4f;
        float linear = MathF.Pow(formed, rec1886);
        return linear <= SrgbBreak ? linear * SrgbSlope : ((1f + SrgbOffset) * MathF.Pow(linear, 1f / SrgbGamma)) - SrgbOffset;
    }

    private static (int Size, Vector3[] Cube) Parsed() {
        const string resource = "AgX_Base_sRGB.cube";
        using Stream stream = typeof(PixelPasses).Assembly.GetManifestResourceStream(typeof(PixelPasses), resource) ?? throw new FileNotFoundException(resource);
        using StreamReader reader = new(stream);
        string text = reader.ReadToEnd();
        Span<System.Range> tokens = stackalloc System.Range[3];
        (int size, Vector3[] cube, int filled) = (0, [], 0);
        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines()) {
            Span<System.Range> read = tokens[..line.Split(tokens, ' ')];
            if (read is [_, var edge] && line.StartsWith("LUT_3D_SIZE", StringComparison.Ordinal)) {
                size = int.Parse(line[edge], CultureInfo.InvariantCulture);
                cube = new Vector3[size * size * size];
            } else if (read is [var red, var green, var blue] && line is [char first, ..] && char.IsAsciiDigit(first))
                cube[filled++] = new(float.Parse(line[red], CultureInfo.InvariantCulture), float.Parse(line[green], CultureInfo.InvariantCulture), float.Parse(line[blue], CultureInfo.InvariantCulture));
        }
        return (size, cube);
    }

    // --- [DITHER]
    private const float MaxCode = byte.MaxValue;
    private const int Tile = 64;
    private static readonly float[] Ranks = VoidAndCluster();

    internal static PixelPass.Pointwise Noised(Func<int, int, float> uniform) =>
        new((pixel, x, y) => {
            float signed = (2f * uniform(x, y)) - 1f;
            return Vector4.Create(Level(Quantized((MaxCode * pixel.AsVector3()) + Vector3.Create(MathF.CopySign(1f - MathF.Sqrt(1f - MathF.Abs(signed)), signed)))), pixel.W);
        });

    internal static float Hashed(int x, int y) {
        const uint iq = 1103515245u;
        return unchecked(iq * ((iq * (((uint)x >> 1) ^ (uint)y)) ^ ((iq * (((uint)y >> 1) ^ (uint)x)) >> 3))) * (1f / uint.MaxValue);
    }

    internal static float BlueNoised(int x, int y) =>
        Ranks[(y % Tile * Tile) + (x % Tile)];

    internal static void Diffused(Span<Vector4> pixels, int width) {
        Vector3[] carried = new Vector3[2 * width];
        for (int line = 0; line < pixels.Length / width; line++) {
            int direction = (line & 1) == 0 ? 1 : -1;
            Span<Vector3> current = carried.AsSpan((line & 1) * width, width);
            Span<Vector3> next = carried.AsSpan(((line + 1) & 1) * width, width);
            next.Clear();
            for (int step = 0; step < width; step++) {
                int x = direction > 0 ? step : width - 1 - step;
                ref Vector4 pixel = ref pixels[(line * width) + x];
                Vector3 held = (MaxCode * pixel.AsVector3()) + current[x];
                Vector3 code = Quantized(held);
                Vector3 residual = held - code;
                pixel = Vector4.Create(Level(code), pixel.W);
                Spread(current, x + direction, 7f / 16f * residual);
                Spread(next, x - direction, 3f / 16f * residual);
                Spread(next, x, 5f / 16f * residual);
                Spread(next, x + direction, 1f / 16f * residual);
            }
        }
    }

    private static Vector3 Quantized(Vector3 code) =>
        Vector3.Truncate(Vector3.Clamp(code + Vector3.Create(0.5f), Vector3.Zero, Vector3.Create(MaxCode)));

    private static Vector3 Level(Vector3 quantized) =>
        (quantized + Vector3.Create(0.5f)) / (MaxCode + 0.5f);

    private static void Spread(Span<Vector3> row, int x, Vector3 share) {
        if (x >= 0 && x < row.Length)
            row[x] += share;
    }

    private static float[] VoidAndCluster() {
        const int count = Tile * Tile;
        const double sigma = 1.5;
        const float density = 0.1f;
        static int Wrapped(int cell, int source) =>
            (((cell / Tile) - (source / Tile) + Tile) % Tile * Tile) + (((cell % Tile) - (source % Tile) + Tile) % Tile);
        double[] kernel = [.. Enumerable.Range(0, count).Select(static offset => {
            int dx = Math.Min(offset % Tile, Tile - (offset % Tile));
            int dy = Math.Min(offset / Tile, Tile - (offset / Tile));
            return Math.Exp(-((dx * dx) + (dy * dy)) / (2 * sigma * sigma));
        })];
        bool[] ones = new bool[count];
        double[] energy = new double[count];
        int[] ranks = new int[count];
        void Toggle(int pixel, bool on) {
            ones[pixel] = on;
            for (int cell = 0; cell < count; cell++)
                energy[cell] += (on ? 1 : -1) * kernel[Wrapped(cell, pixel)];
        }
        int Extreme(bool one) =>
            Enumerable.Range(0, count).Where(pixel => ones[pixel] == one).Aggregate((best, pixel) => (one ? energy[pixel] > energy[best] : energy[pixel] < energy[best]) ? pixel : best);
        for (int pixel = 0; pixel < count; pixel++) {
            if (Hashed(pixel % Tile, pixel / Tile) < density)
                Toggle(pixel, on: true);
        }
        int cluster = Extreme(one: true);
        Toggle(cluster, on: false);
        for (int gap = Extreme(one: false); gap != cluster; gap = Extreme(one: false)) {
            Toggle(gap, on: true);
            cluster = Extreme(one: true);
            Toggle(cluster, on: false);
        }
        Toggle(cluster, on: true);
        bool[] prototypeOnes = [.. ones];
        double[] prototypeEnergy = [.. energy];
        int seeded = ones.Count(static one => one);
        for (int rank = seeded - 1; rank >= 0; rank--) {
            int tightest = Extreme(one: true);
            Toggle(tightest, on: false);
            ranks[tightest] = rank;
        }
        prototypeOnes.CopyTo(ones, 0);
        prototypeEnergy.CopyTo(energy, 0);
        for (int rank = seeded; rank < count; rank++) {
            int largest = Extreme(one: false);
            Toggle(largest, on: true);
            ranks[largest] = rank;
        }
        return [.. ranks.Select(static rank => (rank + 0.5f) / count)];
    }
}
