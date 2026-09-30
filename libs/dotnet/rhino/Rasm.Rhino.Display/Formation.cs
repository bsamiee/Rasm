using System.Globalization;
using System.Numerics;
using Rasm.Rhino.Document;
using Rhino.Render.PostEffects;

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AgX {
    private const float SrgbGamma = 2.4f;
    private const float SrgbOffset = 0.055f;
    private static readonly float SrgbBreak = MathF.Pow(SrgbOffset * SrgbGamma / ((SrgbGamma - 1f) * (1f + SrgbOffset)), SrgbGamma);
    private static readonly float SrgbSlope = SrgbOffset / (SrgbGamma - 1f) / SrgbBreak;
    private static readonly (int Size, Vector3[] Cube) Lut = Parsed();

    public static IO<PixelPass> Formation(Exposure exposure, PostEffectPipeline pipeline) =>
        EffectPipeline.OutputGamma(pipeline).Map<PixelPass>(gamma => {
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
        using Stream stream = typeof(AgX).Assembly.GetManifestResourceStream(typeof(AgX), resource) ?? throw new FileNotFoundException(resource);
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
}
