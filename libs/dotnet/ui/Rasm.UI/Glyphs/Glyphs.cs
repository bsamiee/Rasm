using System.Reflection;
using SkiaSharp;
using Svg.Model;
using Svg.Skia;

namespace Rasm.UI.Glyphs;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GlyphRole {
    public static readonly GlyphRole Reset = new("reset");
    public static readonly GlyphRole Load = new("load");
    public static readonly GlyphRole Save = new("save");
    public static readonly GlyphRole Pick = new("pick");
    public static readonly GlyphRole LockOpen = new("lock-open");
    public static readonly GlyphRole LockClosed = new("lock-closed");
    public static readonly GlyphRole Presets = new("presets");
    public static readonly GlyphRole Options = new("options");
    public static readonly GlyphRole Search = new("search");
    public static readonly GlyphRole Grid = new("grid");
    public static readonly GlyphRole List = new("list");
    public static readonly GlyphRole FavoriteOn = new("favorite-on");
    public static readonly GlyphRole FavoriteOff = new("favorite-off");
    public static readonly GlyphRole VerticalWipe = new("vertical-wipe");
    public static readonly GlyphRole HorizontalWipe = new("horizontal-wipe");
    public static readonly GlyphRole SideBySide = new("side-by-side");
    public static readonly GlyphRole Swap = new("swap");
    public static readonly GlyphRole Fit = new("fit");
    public static readonly GlyphRole ActualSize = new("actual-size");
    public static readonly GlyphRole Flip = new("flip");
    public static readonly GlyphRole Region = new("region");
    public static readonly GlyphRole Queue = new("queue");
    public static readonly GlyphRole Synced = new("synced");
    public static readonly GlyphRole Excluded = new("excluded");
}

[SmartEnum]
public sealed partial class GlyphVariant {
    public static readonly GlyphVariant Light = new(None);
    public static readonly GlyphVariant Dark = new(Some("_darkmode"));

    public Option<string> Suffix { get; }

    public static GlyphVariant On(SKColorF ground) =>
        (0.2126f * ground.Red) + (0.7152f * ground.Green) + (0.0722f * ground.Blue) < 0.5f ? Dark : Light;
}

[ComplexValueObject(DefaultStringComparison = StringComparison.Ordinal)]
public sealed partial class GlyphSource {
    public Assembly Assembly { get; }
    public string Key { get; }

    public string Resolved(GlyphVariant variant) =>
        variant.Suffix.Map(suffix => $"{Path.GetFileNameWithoutExtension(Key)}{suffix}{Path.GetExtension(Key)}")
            .Filter(sibling => Assembly.GetManifestResourceInfo(sibling) is not null)
            .IfNone(Key);

    public SKSvg Load(GlyphVariant variant, SKColorF tint) {
        using Stream stream = Assembly.GetManifestResourceStream(Resolved(variant))!;
        SKColor color = (SKColor)tint;
        return SKSvg.CreateFromStream(stream, new SvgParameters(Entities: null, Css: null, CurrentColor: System.Drawing.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue)));
    }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Assembly assembly, ref string key) =>
        validationError = assembly.GetManifestResourceInfo(key) is null ? new ValidationError($"{assembly.GetName().Name} embeds no resource {key}") : null;
}

public sealed record GlyphSet(HashMap<GlyphRole, GlyphSource> Sources) {
    public static GlyphSet Empty { get; } = new([]);

    public GlyphSet Over(GlyphSet below) => new(below.Sources.AddOrUpdateRange(Sources.AsIterable()));

    public Fin<GlyphSource> Find(GlyphRole role) => Sources.Find(role).ToFin(Error.New($"glyph set supplies no source for {role}"));
}
