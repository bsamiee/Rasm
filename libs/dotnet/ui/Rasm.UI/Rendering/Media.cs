using System.Drawing;
using System.Globalization;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SkiaSharp.Resources;
using SkiaSharp.Skottie;
using Svg;
using Svg.Model;
using Svg.Model.Services;
using Svg.Skia;
using Svg.Skia.TypefaceProviders;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record ArtKey(GlyphSource Source, string Resource, Option<SKColor> Current, HashMap<string, SKColor> Classes, HashMap<Token, SKColorF> Chrome);

// --- [SERVICES] ------------------------------------------------------------------------
internal abstract class LoadedMedia : IDisposable {
    private LoadedMedia() { }

    internal virtual SKRect Bounds => Still.Map(static picture => picture.CullRect).IfNone(SKRect.Empty);

    internal virtual bool Animated => false;

    internal virtual Option<SKPicture> Still => None;

    internal virtual void Seek(Duration now) { }

    internal SKRect Fitted(SKRect box) => ImageFit.Contain.Fitted(box, Bounds.Size);

    internal virtual void Draw(SKCanvas canvas, SKRect into, Option<SKPaint> tint) =>
        _ = Still.Iter(picture => {
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.Concat(ImageFit.Mapping(picture.CullRect, into));
            if (tint.Case is SKPaint washed) canvas.DrawRect(picture.CullRect, washed);
            else canvas.DrawPicture(picture);
        });

    public abstract void Dispose();

    internal sealed class Picture(SKPicture art) : LoadedMedia {
        internal override Option<SKPicture> Still => art;

        public override void Dispose() => art.Dispose();
    }

    internal sealed class Smil : LoadedMedia {
        private readonly SKSvg document;

        private Smil(SKSvg document) => this.document = document;

        internal static Fin<LoadedMedia> Of(SvgDocument source, Action<SKSvgSettings> configure, string resource) {
            SKSvg loaded = new();
            configure(loaded.Settings);
            if (loaded.FromSvgDocument(source) is not null) return new Smil(loaded);
            loaded.Dispose();
            return new UIFault(resource, UIReason.Unreadable);
        }

        internal override bool Animated => document.HasAnimations;

        internal override Option<SKPicture> Still => Optional(document.Picture);

        internal override void Seek(Duration now) => document.SetAnimationTime(now.ToTimeSpan());

        public override void Dispose() => document.Dispose();
    }

    internal sealed class Lottie(Animation motion) : LoadedMedia {
        internal override SKRect Bounds => SKRect.Create(motion.Size);

        internal override bool Animated => true;

        internal override void Seek(Duration now) => motion.Seek(now / Duration.FromTimeSpan(motion.Duration) % 1d);

        internal override void Draw(SKCanvas canvas, SKRect into, Option<SKPaint> tint) {
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.Concat(ImageFit.Mapping(Bounds, into));
            if (tint.Case is SKPaint washed) _ = canvas.SaveLayer(Bounds, washed);
            motion.Render(canvas, Bounds);
        }

        public override void Dispose() => motion.Dispose();
    }
}

internal sealed class Decoded(Seq<(SKImage Image, Duration Shown)> frames) : IDisposable {
    private readonly Duration period = frames.Fold(Duration.Zero, static (total, frame) => total + frame.Shown);

    internal bool Animated => frames.Count > 1;

    internal SKImage At(Duration now) {
        Duration phase = period > Duration.Zero ? period * (now / period % 1d) : Duration.Zero;
        return frames.Fold((frames[0].Image, Start: Duration.Zero), (held, frame) => (held.Start <= phase ? frame.Image : held.Image, held.Start + frame.Shown)).Image;
    }

    public void Dispose() => _ = frames.Iter(static frame => frame.Image.Dispose());
}

internal sealed class Media {
    private static readonly HashMap<string, Token> SystemColors = HashMap<string, Token>(
        ("Canvas", new(Role.Window, Channel.Fill)), ("CanvasText", new(Role.Window, Channel.Text)), ("Window", new(Role.Window, Channel.Fill)), ("WindowText", new(Role.Window, Channel.Text)),
        ("GrayText", new(Role.Window, Channel.Text) { Condition = Condition.Disabled }), ("AccentColor", new(Role.Accent, Channel.Fill)), ("AccentColorText", new(Role.Accent, Channel.Text)),
        ("LinkText", new(Role.Accent, Channel.Fill)), ("Highlight", new(Role.Selection, Channel.Fill)), ("HighlightText", new(Role.Selection, Channel.Text)),
        ("SelectedItem", new(Role.Selection, Channel.Fill)), ("SelectedItemText", new(Role.Selection, Channel.Text)), ("ButtonFace", new(Role.Button, Channel.Fill)),
        ("ButtonText", new(Role.Button, Channel.Text)), ("ButtonBorder", new(Role.Button, Channel.Stroke)), ("Field", new(Role.Field, Channel.Fill)),
        ("FieldText", new(Role.Field, Channel.Text)), ("Mark", new(Role.Mark, Channel.Fill)), ("MarkText", new(Role.Mark, Channel.Text)));

    private readonly SKImageInfo target;
    private readonly Shaping shaping;
    private readonly Func<SKImageInfo, Option<SKSurface>> offscreen;
    private readonly SkiaModel model;
    private readonly SkiaSvgAssetLoader loader;

    internal Media(SKImageInfo target, Shaping shaping, Func<SKImageInfo, Option<SKSurface>> offscreen) {
        (this.target, this.shaping, this.offscreen) = (target, shaping, offscreen);
        model = new SkiaModel(Configured(new SKSvgSettings()));
        loader = new SkiaSvgAssetLoader(model);
    }

    // --- [KEYS]
    internal static ArtKey Keyed(LayoutNode layout, GlyphSource source) {
        SKColorF under = Styles.Background(layout.Cascade, layout.Style, layout.Node);
        Theme theme = layout.Cascade.Theme;
        return source.Animated
            ? new ArtKey(source, source.Shown(under), None, [], [])
            : new ArtKey(source, source.Shown(under),
                source.Tinted ? None : theme.Color(layout.Style.Text, layout.Style).Map(resting => (SKColor)theme.Legible(layout.Style, under, resting, text: false)),
                toHashMap(toSeq(source.Classes.AsIterable()).Choose(pair => theme.Color(pair.Value, layout.Style).Map(resolved => (pair.Key, (SKColor)resolved)))), theme.Chrome);
    }

    // --- [LOADING]
    internal Fin<LoadedMedia> Load(ArtKey key) {
        using Stream? stream = key.Source.Assembly.GetManifestResourceStream(key.Resource);
        return Optional(stream).ToFin(new UIFault(key.Resource, UIReason.Missing)).Bind(held => key.Source.Animated ? Motion(key, held) : Vector(key, held));
    }

    internal Fin<Decoded> Decode((ImageSource.Encoded Source, SKSizeI Pixels) key) {
        using SKCodec? codec = SKCodec.Create(key.Source.Data);
        return Optional(codec).ToFin(new UIFault(key.Source.Data.Size.ToString(CultureInfo.InvariantCulture), UIReason.Unreadable)).Bind(read => {
            bool transposed = ImageSource.Encoded.Transposed(read.EncodedOrigin);
            SKSizeI wanted = transposed ? new SKSizeI(key.Pixels.Height, key.Pixels.Width) : key.Pixels;
            SKImageInfo raw = read.Info.WithSize(read.GetScaledDimensions(float.Min(1f, float.Max((float)wanted.Width / read.Info.Width, (float)wanted.Height / read.Info.Height)))).WithAlphaType(SKAlphaType.Premul);
            SKImageInfo oriented = transposed ? raw.WithSize(raw.Height, raw.Width) : raw;
            SKCodecFrameInfo[] timing = read.FrameInfo;
            return Range(0, int.Max(1, read.FrameCount))
                .Fold(Fin.Succ<Seq<(SKImage Image, Duration Shown)>>([]), (decoded, index) =>
                    from held in decoded
                    from frame in Frame(read, raw, oriented, held, index, timing)
                    select held.Add(frame))
                .Map(static decoded => new Decoded(decoded));
        });
    }

    private static Fin<LoadedMedia> Motion(ArtKey key, Stream held) {
        using AnimationBuilder builder = Animation.CreateBuilder();
        using DataUriResourceProvider embedded = new(preDecode: true);
        _ = builder.SetFontManager(SKFontManager.Default).SetResourceProvider(embedded);
        return Optional(builder.Build(held)).ToFin(new UIFault(key.Resource, UIReason.Unreadable)).Map(static motion => (LoadedMedia)new LoadedMedia.Lottie(motion));
    }

    private Fin<LoadedMedia> Vector(ArtKey key, Stream held) {
        SvgDictionarySystemColorProvider colors = new(toSeq(SystemColors.AsIterable()).Choose(pair => key.Chrome.Find(pair.Value).Map(color => (Name: pair.Key, Color: (SKColor)color)))
            .ToDictionary(static pair => pair.Name, static pair => Drawn(pair.Color), StringComparer.OrdinalIgnoreCase));
        using IDisposable scope = SvgSystemColorResolver.PushProvider(colors);
        string css = string.Concat(key.Classes.AsIterable().Select(static pair => Rule(pair.Key, pair.Value)));
        SvgParameters parameters = new(Entities: null, Css: css.Length > 0 ? css : null, CurrentColor: key.Current.Map(Drawn).ToNullable());
        return Optional(SvgService.Open(held, parameters)).ToFin(new UIFault(key.Resource, UIReason.Unreadable)).Bind(document =>
            document.Descendants().OfType<SvgAnimationElement>().Any()
                ? LoadedMedia.Smil.Of(document, settings => Configured(settings).SystemColorProvider = colors, key.Resource)
                : Optional(SKSvg.ToPicture(document, model, loader)).ToFin(new UIFault(key.Resource, UIReason.Unreadable)).Map(static art => (LoadedMedia)new LoadedMedia.Picture(art)));
    }

    private SKSvgSettings Configured(SKSvgSettings settings) {
        settings.ColorType = target.ColorType;
        settings.AlphaType = target.AlphaType;
        settings.TypefaceProviders = [new EmbeddedFaces(shaping), .. settings.TypefaceProviders ?? []];
        return settings;
    }

    // --- [DECODING]
    private Fin<(SKImage Image, Duration Shown)> Frame(SKCodec codec, SKImageInfo raw, SKImageInfo oriented, Seq<(SKImage Image, Duration Shown)> decoded, int index, SKCodecFrameInfo[] timing) {
        int required = timing.Length > index ? timing[index].RequiredFrame : -1;
        using SKBitmap pixels = new(raw);
        if (required >= 0) _ = decoded[required].Image.ReadPixels(pixels.PeekPixels());
        else pixels.Erase(SKColors.Transparent);
        SKCodecResult result = codec.GetPixels(raw, pixels.GetPixels(), new SKCodecOptions(index, required));
        Duration shown = timing.Length > index ? Duration.FromMilliseconds(timing[index].Duration) : Duration.Zero;
        Fin<SKSurface> surface = result is SKCodecResult.Success or SKCodecResult.IncompleteInput
            ? offscreen(oriented).ToFin(new UIFault(oriented.Size.ToString(), UIReason.Refused))
            : new UIFault(result.ToString(), UIReason.Unreadable);
        return surface.Map(held => Oriented(held, pixels, codec.EncodedOrigin, raw, shown));
    }

    private static (SKImage Image, Duration Shown) Oriented(SKSurface surface, SKBitmap pixels, SKEncodedOrigin origin, SKImageInfo raw, Duration shown) {
        using SKSurface owned = surface;
        using SKImage frame = SKImage.FromBitmap(pixels);
        owned.Canvas.SetMatrix(Orientation(origin, raw.Width, raw.Height));
        owned.Canvas.DrawImage(frame, 0f, 0f, SKSamplingOptions.Default, paint: null);
        return (owned.Snapshot(), shown);
    }

    private static SKMatrix Orientation(SKEncodedOrigin origin, float width, float height) =>
        origin switch {
            SKEncodedOrigin.TopRight => new(-1f, 0f, width, 0f, 1f, 0f, 0f, 0f, 1f),
            SKEncodedOrigin.BottomRight => new(-1f, 0f, width, 0f, -1f, height, 0f, 0f, 1f),
            SKEncodedOrigin.BottomLeft => new(1f, 0f, 0f, 0f, -1f, height, 0f, 0f, 1f),
            SKEncodedOrigin.LeftTop => new(0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 1f),
            SKEncodedOrigin.RightTop => new(0f, -1f, height, 1f, 0f, 0f, 0f, 0f, 1f),
            SKEncodedOrigin.RightBottom => new(0f, -1f, height, -1f, 0f, width, 0f, 0f, 1f),
            SKEncodedOrigin.LeftBottom => new(0f, 1f, 0f, -1f, 0f, width, 0f, 0f, 1f),
            _ => SKMatrix.Identity,
        };

    private static Color Drawn(SKColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

    private static string Rule(string name, SKColor color) => string.Create(CultureInfo.InvariantCulture, $".{name}{{fill:#{color.Red:x2}{color.Green:x2}{color.Blue:x2};fill-opacity:{color.Alpha / 255f}}}");

    private sealed class EmbeddedFaces(Shaping shaping) : ITypefaceProvider {
        public SKTypeface? FromFamilyName(string fontFamily, SKFontStyleWeight fontWeight, SKFontStyleWidth fontWidth, SKFontStyleSlant fontStyle) =>
            toSeq(fontFamily.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)).Choose(family => shaping.Matching(family.Trim('\'', '"'), fontWeight, fontWidth, fontStyle)).Head.ValueUnsafe();
    }
}
