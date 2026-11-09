using System.Drawing;
using System.Globalization;
using Rasm.UI.Layout;
using Rasm.UI.Media;
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

internal sealed record ArtKey(GlyphSource Source, bool Shaded, Option<SKColor> Current, HashMap<string, SKColor> Classes, HashMap<Token, SKColorF> Chrome);

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

internal sealed class Decoding {

    // --- [STATE]

    private readonly SKImageInfo target;
    private readonly Shaping shaping;
    private readonly Func<SKImageInfo, Option<SKSurface>> offscreen;
    private readonly SkiaModel model;
    private readonly SkiaSvgAssetLoader loader;

    internal Decoding(SKImageInfo target, Shaping shaping, Func<SKImageInfo, Option<SKSurface>> offscreen) {
        (this.target, this.shaping, this.offscreen) = (target, shaping, offscreen);
        model = new SkiaModel(Configured(new SKSvgSettings()));
        loader = new SkiaSvgAssetLoader(model);
    }

    // --- [KEYS]

    internal static ArtKey Keyed(LayoutNode layout, GlyphSource source) {
        SKColorF under = layout.Inner.Background;
        Theme theme = layout.Cascade.Theme;
        return source.Animated
            ? new ArtKey(source, source.Shaded(under), None, [], [])
            : new ArtKey(source, source.Shaded(under),
                source.Tinted ? None : theme.Color(layout.Style.Text, layout.Style).Map(resting => (SKColor)theme.Legible(layout.Style, under, resting, text: false)),
                toHashMap(toSeq(source.Classes.AsIterable()).Choose(pair => theme.Color(pair.Value, layout.Style).Map(resolved => (pair.Key, (SKColor)resolved)))), theme.Chrome);
    }

    internal static Seq<GlyphSource> Sources(LayoutNode layout) =>
        (layout.View is View.Glyph glyph ? Seq(glyph.Art) : [])
        + layout.Style.Fills.Bind(static fill => fill.Arts)
        + layout.Style.Filters.Bind(static filter => filter.Arts)
        + layout.View.Traced.ToSeq().Bind(static figure => figure.Art).Map(static marker => marker.Art);

    internal static (ImageSource.Encoded Source, SKSizeI Pixels) Requested(LayoutNode layout, View.Image image, ImageSource.Encoded encoded, float scale) =>
        (encoded, Capture.Pixels(image.Fit.Fitted(layout.ContentBox, new SKSize(encoded.Info.Width, encoded.Info.Height)).Size, scale));

    // --- [LOADS]

    internal Fin<LoadedMedia> Load(ArtKey key) => key.Source.Animated ? Motion(key) : Vector(key);

    internal Fin<Decoded> Decode((ImageSource.Encoded Source, SKSizeI Pixels) key) {
        using SKCodec? codec = SKCodec.Create(key.Source.Data);
        return Optional(codec).ToFin(new UIFault(key.Source.Data.Size, UIReason.Unreadable)).Bind(read => {
            SKSizeI wanted = ImageSource.Encoded.Oriented(read.EncodedOrigin, key.Pixels).Shown;
            SKImageInfo raw = read.Info.WithSize(read.GetScaledDimensions(float.Min(1f, float.Max((float)wanted.Width / read.Info.Width, (float)wanted.Height / read.Info.Height)))).WithAlphaType(SKAlphaType.Premul);
            (SKSizeI shown, SKMatrix orientation) = ImageSource.Encoded.Oriented(read.EncodedOrigin, raw.Size);
            return offscreen(raw.WithSize(shown)).ToFin(new UIFault(shown, UIReason.Refused)).Bind(surface => Frames(read, raw, surface, orientation));
        });
    }

    private static Fin<LoadedMedia> Motion(ArtKey key) {
        using AnimationBuilder builder = Animation.CreateBuilder();
        using DataUriResourceProvider embedded = new(preDecode: true);
        _ = builder.SetFontManager(SKFontManager.Default).SetResourceProvider(embedded);
        return Optional(builder.Build(key.Source.Shown(key.Shaded))).ToFin(new UIFault(key.Source.Key, UIReason.Unreadable)).Map(static motion => (LoadedMedia)new LoadedMedia.Lottie(motion));
    }

    private Fin<LoadedMedia> Vector(ArtKey key) {
        using Stream held = key.Source.Shown(key.Shaded).AsStream();
        SvgDictionarySystemColorProvider colors = new(toSeq(Token.SystemColors.AsIterable()).Choose(pair => key.Chrome.Find(pair.Value).Map(color => (Name: pair.Key, Color: (SKColor)color)))
            .ToDictionary(static pair => pair.Name, static pair => Drawn(pair.Color), StringComparer.OrdinalIgnoreCase));
        using IDisposable scope = SvgSystemColorResolver.PushProvider(colors);
        string css = string.Concat(key.Classes.AsIterable().Select(static pair => Rule(pair.Key, pair.Value)));
        SvgParameters parameters = new(Entities: null, Css: css.Length > 0 ? css : null, CurrentColor: key.Current.Map(Drawn).ToNullable());
        return Optional(SvgService.Open(held, parameters)).ToFin(new UIFault(key.Source.Key, UIReason.Unreadable)).Bind(document =>
            document.Descendants().OfType<SvgAnimationElement>().Any()
                ? LoadedMedia.Smil.Of(document, settings => Configured(settings).SystemColorProvider = colors, key.Source.Key)
                : Optional(SKSvg.ToPicture(document, model, loader)).ToFin(new UIFault(key.Source.Key, UIReason.Unreadable)).Map(static art => (LoadedMedia)new LoadedMedia.Picture(art)));
    }

    private SKSvgSettings Configured(SKSvgSettings settings) {
        settings.ColorType = target.ColorType;
        settings.AlphaType = target.AlphaType;
        settings.TypefaceProviders = [new EmbeddedFaces(shaping), .. settings.TypefaceProviders ?? []];
        return settings;
    }

    // --- [DECODING]

    private static Fin<Decoded> Frames(SKCodec codec, SKImageInfo raw, SKSurface surface, SKMatrix orientation) {
        using SKSurface owned = surface;
        using SKBitmap canvas = new(raw);
        owned.Canvas.SetMatrix(orientation);
        Seq<(SKImage Image, Duration Shown)> frames = [];
        int prior = SKCodecOptions.Default.PriorFrame;
        for (int index = 0, count = int.Max(1, codec.FrameCount); index < count; index++) {
            bool animated = codec.GetFrameInfo(index, out SKCodecFrameInfo timing);
            using SKBitmap? temporary = animated && timing.DisposalMethod is SKCodecAnimationDisposalMethod.RestorePrevious ? new(raw) : null;
            SKBitmap destination = temporary ?? canvas;
            int based = animated && timing.RequiredFrame >= 0 ? prior : SKCodecOptions.Default.PriorFrame;
            if (temporary is not null && based >= 0 && !canvas.CopyTo(temporary)) return Failed(new UIFault(raw.Size, UIReason.Refused));
            SKCodecResult result = codec.GetPixels(raw, destination.GetPixels(), new SKCodecOptions(index, based));
            if (result is not (SKCodecResult.Success or SKCodecResult.IncompleteInput)) return Failed(new UIFault(result, UIReason.Unreadable));
            if (animated && codec.EncodedFormat is SKEncodedImageFormat.Heif && !codec.GetFrameInfo(index, out timing)) return Failed(new UIFault(index, UIReason.Unreadable));
            using SKImage pixels = SKImage.FromBitmap(destination);
            owned.Canvas.Clear(SKColors.Transparent);
            owned.Canvas.DrawImage(pixels, 0f, 0f, SKSamplingOptions.Default, paint: null);
            frames = frames.Add((owned.Snapshot(), animated ? Duration.FromMilliseconds(timing.Duration) : Duration.Zero));
            if (temporary is null) prior = index;
        }
        return new Decoded(frames);

        Fin<Decoded> Failed(UIFault error) {
            _ = frames.Iter(static frame => frame.Image.Dispose());
            return error;
        }
    }

    private static Color Drawn(SKColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

    private static string Rule(string name, SKColor color) => string.Create(CultureInfo.InvariantCulture, $".{name}{{fill:#{color.Red:x2}{color.Green:x2}{color.Blue:x2};fill-opacity:{color.Alpha / 255f}}}");

    private sealed class EmbeddedFaces(Shaping shaping) : ITypefaceProvider {
        public SKTypeface? FromFamilyName(string fontFamily, SKFontStyleWeight fontWeight, SKFontStyleWidth fontWidth, SKFontStyleSlant fontStyle) =>
            toSeq(fontFamily.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)).Choose(family => shaping.Matching(family.Trim('\'', '"'), fontWeight, fontWidth, fontStyle)).Head.ValueUnsafe();
    }
}
