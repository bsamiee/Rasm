using System.Reflection;
using SkiaSharp;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Media;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ImageFit {
    public static readonly ImageFit Stretch = new(static (content, _) => content);
    public static readonly ImageFit Contain = new(static (content, natural) => content.AspectFit(natural));
    public static readonly ImageFit Cover = new(static (content, natural) => content.AspectFill(natural));

    [UseDelegateFromConstructor]
    public partial SKRect Fitted(SKRect content, SKSize natural);

    internal static SKMatrix Mapping(SKRect from, SKRect to) =>
        (to.Width / from.Width, to.Height / from.Height) switch {
            var (x, y) => SKMatrix.CreateScaleTranslation(x, y, to.Left - (from.Left * x), to.Top - (from.Top * y)),
        };
}

[SmartEnum]
public sealed partial class ImageRendering {
    private static readonly SKSamplingOptions Smooth = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private static readonly SKSamplingOptions Nearest = new(SKFilterMode.Nearest, SKMipmapMode.None);
    private static readonly SKSamplingOptions Anisotropy = new(maxAniso: 16);

    public static readonly ImageRendering Auto = new(Smooth, Smooth);
    public static readonly ImageRendering HighQuality = new(new SKSamplingOptions(SKCubicResampler.Mitchell), Smooth);
    public static readonly ImageRendering CrispEdges = new(Nearest, Nearest);
    public static readonly ImageRendering Pixelated = new(Nearest, Smooth);
    public static readonly ImageRendering Sharp = new(Smooth, new SKSamplingOptions(SKCubicResampler.CatmullRom));
    public static readonly ImageRendering Anisotropic = new(Anisotropy, Anisotropy);

    private readonly SKSamplingOptions magnified;
    private readonly SKSamplingOptions minified;

    internal SKSamplingOptions Sampling(float density) => density > 1f ? magnified : minified;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImageSource {
    public abstract SKImageInfo Info { get; }

    public sealed record Still(SKImage Image) : ImageSource {
        public override SKImageInfo Info => Image.Info;
    }

    public sealed record Encoded : ImageSource {
        private Encoded(SKData data, SKCodec codec) =>
            (Data, FrameCount, Info) = (data, codec.FrameCount, codec.Info.WithSize(Oriented(codec.EncodedOrigin, codec.Info.Size).Shown));

        public override SKImageInfo Info { get; }
        public SKData Data { get; }
        public int FrameCount { get; }

        public static Fin<Encoded> Validated(SKData data) {
            using SKCodec? codec = SKCodec.Create(data);
            return codec is { } read
                ? new Encoded(data, read)
                : new UIFault(data.Size, UIReason.Unreadable);
        }

        internal static (SKSizeI Shown, SKMatrix Matrix) Oriented(SKEncodedOrigin origin, SKSizeI stored) =>
            (new SKSizeI(stored.Height, stored.Width), (float)stored.Width, (float)stored.Height) switch {
                var (transposed, width, height) => origin switch {
                    SKEncodedOrigin.TopRight => (stored, new(-1f, 0f, width, 0f, 1f, 0f, 0f, 0f, 1f)),
                    SKEncodedOrigin.BottomRight => (stored, new(-1f, 0f, width, 0f, -1f, height, 0f, 0f, 1f)),
                    SKEncodedOrigin.BottomLeft => (stored, new(1f, 0f, 0f, 0f, -1f, height, 0f, 0f, 1f)),
                    SKEncodedOrigin.LeftTop => (transposed, new(0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 1f)),
                    SKEncodedOrigin.RightTop => (transposed, new(0f, -1f, height, 1f, 0f, 0f, 0f, 0f, 1f)),
                    SKEncodedOrigin.RightBottom => (transposed, new(0f, -1f, height, -1f, 0f, width, 0f, 0f, 1f)),
                    SKEncodedOrigin.LeftBottom => (transposed, new(0f, 1f, 0f, -1f, 0f, width, 0f, 0f, 1f)),
                    _ => (stored, SKMatrix.Identity),
                },
            };
    }
}

public sealed record GlyphSource {
    public GlyphSource(Assembly assembly, string resource) =>
        (Assembly, Resource, Dark) = (assembly, resource, $"{Path.ChangeExtension(resource, extension: null)}_darkmode{Path.GetExtension(resource)}" switch {
            var dark => Optional(assembly.GetManifestResourceInfo(dark)).Map(_ => dark),
        });

    public Assembly Assembly { get; }
    public string Resource { get; }
    public bool Tinted { get; init; }
    public HashMap<string, ColorValue> Classes { get; init; }

    internal Option<string> Dark { get; }

    internal bool Animated => Path.GetExtension(Resource).Equals(".json", StringComparison.OrdinalIgnoreCase);

    internal string Shown(SKColorF under) =>
        under.ToUnicolour() switch {
            var held => held.Contrast(Css.White) > held.Contrast(Css.Black) ? Dark.IfNone(Resource) : Resource,
        };
}
