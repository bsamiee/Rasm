using System.Drawing;
using System.Globalization;
using Rasm.Rhino.Document.Files;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class ModeFlag {
    public static readonly ModeFlag Yes = new("y", value: true);
    public static readonly ModeFlag No = new("n", value: false);

    public bool Value { get; }
}

[SmartEnum]
public sealed partial class TechnicalLine {
    public static readonly TechnicalLine Hidden = new("TH", 1);
    public static readonly TechnicalLine Edges = new("TE", 2);
    public static readonly TechnicalLine Silhouettes = new("TSi", 4);
    public static readonly TechnicalLine Creases = new("TC", 8);
    public static readonly TechnicalLine Seams = new("TS", 16);
    public static readonly TechnicalLine Intersections = new("TI", 32);

    internal string Prefix { get; }

    internal int Bit { get; }
}

[Union]
public abstract partial record ModeValue {
    public sealed record Flag(bool Value) : ModeValue;

    public sealed record Whole(int Value) : ModeValue;

    public sealed record Real(float Value) : ModeValue;

    public sealed record Colour(Color Value) : ModeValue;

    public sealed record Id(Guid Value) : ModeValue;

    public sealed record Vector(Vector3d Value) : ModeValue;

    public sealed record Text(string Value) : ModeValue;

    public sealed record Packaged(string Path) : ModeValue;

    // --- [CODEC]
    internal Fin<string> Formatted(Option<FolderPath> package) =>
        Switch(
            package,
            flag: static (_, flag) => (flag.Value ? ModeFlag.Yes : ModeFlag.No).Key,
            whole: static (_, whole) => whole.Value.ToString(CultureInfo.InvariantCulture),
            real: static (_, real) => real.Value.ToString(CultureInfo.InvariantCulture),
            colour: static (_, colour) => colour.Value.IsEmpty ? "-1" : $"{colour.Value.R},{colour.Value.G},{colour.Value.B}",
            id: static (_, id) => id.Value.ToString("D"),
            vector: static (_, vector) => string.Create(CultureInfo.InvariantCulture, $"{vector.Value.X},{vector.Value.Y},{vector.Value.Z}"),
            text: static (_, text) => text.Value,
            packaged: static (folder, packaged) => InputPath.Of(folder.Match(Some: held => Path.GetFullPath(packaged.Path, held), None: () => packaged.Path)).Map(static path => (string)path));

    internal bool Holds(string host, Option<FolderPath> package) =>
        Formatted(package).ToOption().Bind(Parsed) is { IsSome: true } declared && declared == Parsed(host);

    private Option<ModeValue> Parsed(string host) =>
        Switch(
            host,
            flag: static (text, _) => Conversions.Present(ModeFlag.TryGet(text, out ModeFlag? flag), flag!).Map(static held => (ModeValue)new Flag(held.Value)),
            whole: static (text, _) => Ini.Integral(text).Map(static value => (ModeValue)new Whole(value)),
            real: static (text, _) => Ini.Number(text).Map(static value => (ModeValue)new Real((float)value)),
            colour: static (text, _) => text is "-1"
                ? new Colour(Color.Empty)
                : Ini.Numbers(text).Filter(static parts => parts.Count is 3).Map(static parts => (ModeValue)new Colour(Color.FromArgb((int)parts[0], (int)parts[1], (int)parts[2]))),
            id: static (text, _) => parseGuid(text).Map(static id => (ModeValue)new Id(id)),
            vector: static (text, _) => Ini.Numbers(text).Filter(static parts => parts.Count is 3).Map(static parts => (ModeValue)new Vector(new Vector3d(parts[0], parts[1], parts[2]))),
            text: static (text, _) => new Text(text),
            packaged: static (text, _) => new Packaged(text));
}

public sealed record ModeKey(string Section, string Key, ModeValue Value) {
    // --- [TECHNICAL]
    private const string TechnicalSection = @"Objects\Technical";

    public static Seq<ModeKey> Technical(Seq<(TechnicalLine Line, Option<Color> Fixed, int Thickness)> lines) =>
        Seq(
            new ModeKey(TechnicalSection, "TechnicalMask", new ModeValue.Whole(lines.Fold(0, static (mask, line) => mask | line.Line.Bit))),
            new ModeKey(TechnicalSection, "TechnicalUsageMask", new ModeValue.Whole(lines.Filter(static line => line.Fixed.IsSome).Fold(0, static (mask, line) => mask | line.Line.Bit))))
        + lines.Bind(static line => line.Fixed.Map(color => new ModeKey(TechnicalSection, $"{line.Line.Prefix}Color", new ModeValue.Colour(color))).ToSeq()
            .Add(new ModeKey(TechnicalSection, $"{line.Line.Prefix}Thickness", new ModeValue.Whole(line.Thickness))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Ini {
    // --- [TEXT]
    internal const string Lines = "\r\n";

    internal static string Header(Guid id, string section) => section.Length is 0 ? $@"[DisplayMode\{id:D}]" : $@"[DisplayMode\{id:D}\{section}]";

    internal static HashMap<(string Section, string Key), string> Table(string text) =>
        toSeq(text.ReplaceLineEndings("\n").Split('\n')).Fold(
            (Section: "", Rows: HashMap<(string Section, string Key), string>()),
            static (state, line) => line switch {
                ['[', ..] and [_, .. var header, ']'] => (header.Split('\\', 3) is [_, _, var relative] ? relative : "", state.Rows),
                _ when line.IndexOf('=', StringComparison.Ordinal) is > 0 and var at => (state.Section, state.Rows.AddOrUpdate((state.Section, line[..at]), line[(at + 1)..])),
                _ => state,
            }).Rows;

    internal static Seq<ModeKey> Merged(Seq<ModeKey> keys) =>
        toSeq(HashMap<(string Section, string Key), ModeKey>().AddOrUpdateRange(keys.Map(static key => ((key.Section, key.Key), key))).Values).Strict();

    internal static string Rendered(Guid id, HashMap<(string Section, string Key), string> table) =>
        string.Join(Lines, toSeq(table.AsIterable()).GroupBy(static row => row.Key.Section, StringComparer.Ordinal).OrderBy(static section => section.Key, StringComparer.Ordinal)
            .SelectMany(section => Seq(Header(id, section.Key)) + toSeq(section).Map(static row => $"{row.Key.Key}={row.Value}")));

    // --- [NUMBERS]
    internal static Option<int> Integral(string text) =>
        Number(text).Filter(static value => double.IsInteger(value) && value is >= int.MinValue and <= int.MaxValue).Map(static value => (int)value);

    internal static Option<double> Number(string text) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : None;

    internal static Option<Seq<double>> Numbers(string text) =>
        toSeq(text.Split(',')).Traverse(static part => Number(part)).As();
}
