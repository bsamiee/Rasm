using System.Drawing;
using System.Globalization;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SkiaSharp.SceneGraph;
using SkiaSharp.Skottie;
using Svg;
using Svg.Model;
using Svg.Model.Services;
using Svg.Skia;
using Svg.Skia.TypefaceProviders;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record ArtKey(Art Art, string Css, HashMap<string, SKColor> Chrome);

internal sealed record DecodeKey(ImageSource.Encoded Source, SKSizeI Pixels);

// --- [SERVICES] ------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial class LoadedMedia : IDisposable {
    internal abstract SKRect Bounds { get; }

    internal virtual bool Animated => false;

    internal virtual Option<SKPicture> Still => None;

    internal virtual bool Seek(Duration now) => false;

    internal abstract void Draw(SKCanvas canvas, SKMatrix place, Option<SKPaint> tint);

    public abstract void Dispose();

    internal sealed class Picture(SKPicture art) : LoadedMedia {
        internal override SKRect Bounds => art.CullRect;

        internal override Option<SKPicture> Still => art;

        internal override void Draw(SKCanvas canvas, SKMatrix place, Option<SKPaint> tint) => canvas.DrawPicture(art, place, tint.ValueUnsafe());

        public override void Dispose() => art.Dispose();
    }

    internal sealed class Smil : LoadedMedia {
        private readonly SKSvg document = new();

        internal Smil(SvgDocument source, Action<SKSvgSettings> configure) {
            configure(document.Settings);
            Loaded = document.FromSvgDocument(source) is not null;
        }

        internal bool Loaded { get; }

        internal override SKRect Bounds => document.Picture?.CullRect ?? SKRect.Empty;

        internal override bool Animated => document.HasAnimations;

        internal override Option<SKPicture> Still => Optional(document.Picture);

        internal override bool Seek(Duration now) {
            document.SetAnimationTime(now.ToTimeSpan());
            return document.HasAnimations;
        }

        internal override void Draw(SKCanvas canvas, SKMatrix place, Option<SKPaint> tint) {
            if (document.Picture is { } frame) canvas.DrawPicture(frame, place, tint.ValueUnsafe());
        }

        public override void Dispose() => document.Dispose();
    }

    internal sealed class Lottie(Animation motion) : LoadedMedia {
        private readonly InvalidationController damage = new();

        internal override SKRect Bounds => SKRect.Create(motion.Size);

        internal override bool Animated => true;

        internal override bool Seek(Duration now) {
            damage.Reset();
            motion.Seek(now / Duration.FromTimeSpan(motion.Duration) % 1d, damage);
            return !damage.Bounds.IsEmpty;
        }

        internal override void Draw(SKCanvas canvas, SKMatrix place, Option<SKPaint> tint) {
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.Concat(place);
            if (tint.Case is SKPaint layerPaint) _ = canvas.SaveLayer(Bounds, layerPaint);
            motion.Render(canvas, Bounds);
        }

        public override void Dispose() {
            damage.Dispose();
            motion.Dispose();
        }
    }
}

internal sealed class Decoded(Seq<Decoded.Frame> frames) : IDisposable {
    private readonly Duration period = frames.Fold(Duration.Zero, static (total, frame) => total + frame.Shown);

    internal bool Animated => frames.Count > 1;

    internal SKImage At(Duration now) {
        Duration phase = period > Duration.Zero ? period * (now / period % 1d) : Duration.Zero;
        return frames.Fold((frames[0].Image, Start: Duration.Zero), (held, frame) => (held.Start <= phase ? frame.Image : held.Image, held.Start + frame.Shown)).Image;
    }

    public void Dispose() => _ = frames.Iter(static frame => frame.Image.Dispose());

    internal sealed record Frame(SKImage Image, Duration Shown);
}

internal sealed class Media {
    private static readonly HashMap<string, ColorValue> SystemColors = HashMap<string, ColorValue>(
        ("Canvas", Hosted(Role.Window, Channel.Fill)), ("CanvasText", Hosted(Role.Window, Channel.Text)), ("Window", Hosted(Role.Window, Channel.Fill)), ("WindowText", Hosted(Role.Window, Channel.Text)),
        ("GrayText", Hosted(Role.Window, Channel.Text) with { Condition = Condition.Disabled }), ("AccentColor", Hosted(Role.Accent, Channel.Fill)), ("AccentColorText", Hosted(Role.Accent, Channel.Text)),
        ("LinkText", Hosted(Role.Accent, Channel.Fill)), ("Highlight", Hosted(Role.Selection, Channel.Fill)), ("HighlightText", Hosted(Role.Selection, Channel.Text)),
        ("SelectedItem", Hosted(Role.Selection, Channel.Fill)), ("SelectedItemText", Hosted(Role.Selection, Channel.Text)), ("ButtonFace", Hosted(Role.Button, Channel.Fill)),
        ("ButtonText", Hosted(Role.Button, Channel.Text)), ("ButtonBorder", Hosted(Role.Button, Channel.Stroke)), ("Field", Hosted(Role.Field, Channel.Fill)),
        ("FieldText", Hosted(Role.Field, Channel.Text)), ("Mark", Hosted(Role.Mark, Channel.Fill)), ("MarkText", Hosted(Role.Mark, Channel.Text)));

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
    internal static ArtKey Keyed(LayoutNode layout, GlyphSource source, SKColorF under, ColorCache colorCache) {
        Art shown = source.Resolved(under);
        ColorScheme theme = layout.Cascade.Theme;
        return new ArtKey(
            shown.Tinted ? shown : shown with { Current = theme.Color(layout.Style.Text, layout.Style).Map(resting => (SKColor)Styles.Legible(colorCache, layout.Style, under, resting, theme.Contrast.Graphic)) },
            string.Concat(toSeq(Enumerable.OrderBy(shown.Classes.AsIterable(), static pair => pair.Key, StringComparer.Ordinal)).Choose(pair => theme.Color(pair.Value, layout.Style).Map(resolved => Rule(pair.Key, (SKColor)resolved)))),
            toHashMap(toSeq(SystemColors.AsIterable()).Choose(pair => theme.Color(pair.Value, layout.Style).Map(resolved => (pair.Key, (SKColor)resolved)))));
    }

    // --- [LOADING]
    internal Fin<LoadedMedia> Load(ArtKey key) {
        using Stream? stream = key.Art.Assembly.GetManifestResourceStream(key.Art.Resource);
        return Optional(stream).ToFin(new UIFault(key.Art.Resource, UIReason.Missing)).Bind(held => key.Art.Animated ? Motion(key, held) : Vector(key, held));
    }

    internal Fin<Decoded> Decode(DecodeKey key) {
        using SKCodec? codec = SKCodec.Create(key.Source.Data);
        return Optional(codec).ToFin(new UIFault(key.Source.Data.Size.ToString(CultureInfo.InvariantCulture), UIReason.Unreadable)).Bind(read => {
            SKEncodedOrigin origin = read.EncodedOrigin;
            SKSizeI wanted = Transposed(origin) ? new SKSizeI(key.Pixels.Height, key.Pixels.Width) : key.Pixels;
            SKImageInfo raw = read.Info.WithSize(read.GetScaledDimensions(float.Min(1f, float.Max((float)wanted.Width / read.Info.Width, (float)wanted.Height / read.Info.Height)))).WithAlphaType(SKAlphaType.Premul);
            SKCodecFrameInfo[] timing = read.FrameInfo;
            return toSeq(Enumerable.Range(0, int.Max(1, read.FrameCount)))
                .Fold(Fin.Succ<Seq<Decoded.Frame>>([]), (decoded, index) =>
                    from held in decoded
                    from frame in Frame(read, raw, origin, held, index, timing)
                    select held.Add(frame))
                .Map(static decoded => new Decoded(decoded));
        });
    }

    private static Fin<LoadedMedia> Motion(ArtKey key, Stream held) {
        using AnimationBuilder builder = Animation.CreateBuilder();
        _ = builder.SetFontManager(SKFontManager.Default);
        return Optional(builder.Build(held)).ToFin(new UIFault(key.Art.Resource, UIReason.Unreadable)).Map(static motion => (LoadedMedia)new LoadedMedia.Lottie(motion));
    }

    private Fin<LoadedMedia> Vector(ArtKey key, Stream held) {
        SvgDictionarySystemColorProvider colors = new(toSeq(key.Chrome.AsIterable()).ToDictionary(static pair => pair.Key, static pair => Color.FromArgb(pair.Value.Alpha, pair.Value.Red, pair.Value.Green, pair.Value.Blue), StringComparer.OrdinalIgnoreCase));
        using IDisposable scope = SvgSystemColorResolver.PushProvider(colors);
        SvgParameters parameters = new(Entities: null, Css: key.Css.Length > 0 ? key.Css : null, CurrentColor: key.Art.Current.Map(static resolved => Color.FromArgb(resolved.Alpha, resolved.Red, resolved.Green, resolved.Blue)).ToNullable());
        return Optional(SvgService.Open(held, parameters)).ToFin(new UIFault(key.Art.Resource, UIReason.Unreadable)).Bind(document =>
            document.Descendants().OfType<SvgAnimationElement>().Any()
                ? Animated(key, document, colors)
                : Optional(SKSvg.ToPicture(document, model, loader)).ToFin(new UIFault(key.Art.Resource, UIReason.Unreadable)).Map(static art => (LoadedMedia)new LoadedMedia.Picture(art)));
    }

    private Fin<LoadedMedia> Animated(ArtKey key, SvgDocument document, ISvgSystemColorProvider colors) {
        LoadedMedia.Smil loaded = new(document, settings => Configured(settings).SystemColorProvider = colors);
        if (loaded.Loaded) return loaded;
        loaded.Dispose();
        return new UIFault(key.Art.Resource, UIReason.Unreadable);
    }

    private SKSvgSettings Configured(SKSvgSettings settings) {
        settings.ColorType = target.ColorType;
        settings.AlphaType = target.AlphaType;
        settings.TypefaceProviders = [new EmbeddedFaces(shaping), .. settings.TypefaceProviders ?? []];
        return settings;
    }

    // --- [DECODING]
    private Fin<Decoded.Frame> Frame(SKCodec codec, SKImageInfo raw, SKEncodedOrigin origin, Seq<Decoded.Frame> decoded, int index, SKCodecFrameInfo[] timing) {
        int required = timing.Length > index ? timing[index].RequiredFrame : -1;
        using SKBitmap pixels = new(raw);
        if (required >= 0) _ = decoded[required].Image.ReadPixels(pixels.PeekPixels());
        else pixels.Erase(SKColors.Transparent);
        SKCodecResult result = codec.GetPixels(raw, pixels.GetPixels(), new SKCodecOptions(index, required));
        SKSizeI oriented = Transposed(origin) ? new SKSizeI(raw.Height, raw.Width) : new SKSizeI(raw.Width, raw.Height);
        Duration shown = timing.Length > index ? Duration.FromMilliseconds(timing[index].Duration) : Duration.Zero;
        Fin<SKSurface> surface = result is SKCodecResult.Success or SKCodecResult.IncompleteInput
            ? offscreen(raw.WithSize(oriented.Width, oriented.Height)).ToFin(new UIFault(oriented.ToString(), UIReason.Refused))
            : new UIFault(result.ToString(), UIReason.Unreadable);
        return surface.Map(held => Oriented(held, pixels, origin, raw, shown));
    }

    private static Decoded.Frame Oriented(SKSurface surface, SKBitmap pixels, SKEncodedOrigin origin, SKImageInfo raw, Duration shown) {
        using SKSurface owned = surface;
        using SKImage frame = SKImage.FromBitmap(pixels);
        owned.Canvas.SetMatrix(Orientation(origin, raw.Width, raw.Height));
        owned.Canvas.DrawImage(frame, 0f, 0f, SKSamplingOptions.Default, paint: null);
        return new Decoded.Frame(owned.Snapshot(), shown);
    }

    private static bool Transposed(SKEncodedOrigin origin) => origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

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

    private static ColorValue.Host Hosted(Role role, Channel channel) => new(channel) { Role = role };

    private static string Rule(string name, SKColor color) => string.Create(CultureInfo.InvariantCulture, $".{name}{{fill:#{color.Red:x2}{color.Green:x2}{color.Blue:x2};fill-opacity:{color.Alpha / 255f}}}");

    private sealed class EmbeddedFaces(Shaping shaping) : ITypefaceProvider {
        public SKTypeface? FromFamilyName(string fontFamily, SKFontStyleWeight fontWeight, SKFontStyleWidth fontWidth, SKFontStyleSlant fontStyle) =>
            shaping.Matching(fontFamily, fontWeight, fontWidth, fontStyle).ValueUnsafe();
    }
}
