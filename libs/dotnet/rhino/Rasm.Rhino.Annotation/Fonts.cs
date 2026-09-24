using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FontQuery {
    public sealed record Quartet(string QuartetName, bool Bold, bool Italic) : FontQuery;

    public sealed record Properties(string FamilyName, Font.FontWeight Weight, Font.FontStyle Style, Font.FontStretch Stretch, bool Underlined, bool Strikethrough) : FontQuery;

    public sealed record RichText(string RichTextFontName, bool Bold, bool Italic, bool Underlined, bool Strikethrough) : FontQuery;
}

public sealed record FontState(
    string FaceName,
    string FamilyName,
    string FamilyPlusFaceName,
    string QuartetName,
    string PostScriptName,
    string LogfontName,
    string RichTextFontName,
    string Description,
    string EnglishFaceName,
    string EnglishFamilyName,
    string EnglishQuartetName,
    Font.FontWeight Weight,
    Font.FontStyle Style,
    Font.FontStretch Stretch,
    bool Bold,
    bool Italic,
    bool Underlined,
    bool Strikeout,
    double PointSize,
    bool IsInstalled,
    bool IsSimulated,
    bool IsEngravingFont,
    bool IsSymbolFont,
    bool IsSingleStrokeFont,
    bool IsGeometricToleranceFont);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Fonts {
    public static IO<Font> Resolve(FontQuery query) =>
        IO.lift(() => query.Switch(
            quartet: static quartet => Missing.Unless(Font.FromQuartetProperties(quartet.QuartetName, quartet.Bold, quartet.Italic), nameof(Font.FromQuartetProperties)),
            properties: static properties => Fin.Succ(new Font(properties.FamilyName, properties.Weight, properties.Style, properties.Stretch, properties.Underlined, properties.Strikethrough)),
            richText: static rich =>
                Missing.Unless(Font.FromRichTextProperties(rich.RichTextFontName, rich.Bold, rich.Italic, rich.Underlined, rich.Strikethrough), nameof(Font.FromRichTextProperties))));

    [MapProperty(nameof(Font.FaceName), nameof(FontState.FaceName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.FamilyName), nameof(FontState.FamilyName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.FamilyPlusFaceName), nameof(FontState.FamilyPlusFaceName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.QuartetName), nameof(FontState.QuartetName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.PostScriptName), nameof(FontState.PostScriptName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.LogfontName), nameof(FontState.LogfontName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.RichTextFontName), nameof(FontState.RichTextFontName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.Description), nameof(FontState.Description), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.EnglishFaceName), nameof(FontState.EnglishFaceName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.EnglishFamilyName), nameof(FontState.EnglishFamilyName), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Font.EnglishQuartetName), nameof(FontState.EnglishQuartetName), SuppressNullMismatchDiagnostic = true)]
    public static partial FontState State(Font font);

    public static IO<Seq<FontState>> Installed(Option<string> familyName) =>
        IO.lift(() => Answers.Present(Font.InstalledFonts(familyName.ValueUnsafe())).Map(State).Strict());
}
