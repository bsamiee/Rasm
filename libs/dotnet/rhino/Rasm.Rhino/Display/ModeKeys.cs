using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IniParser;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using Rasm.Rhino.Document.Files;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
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

    public sealed record Colour(Color Value) : ModeValue {
        public Color Value { get; } = Value.IsEmpty ? Color.Empty : Color.FromArgb(Value.R, Value.G, Value.B);
    }

    public sealed record Id(Guid Value) : ModeValue;

    public sealed record Vector(Vector3d Value) : ModeValue;

    public sealed record Text(string Value) : ModeValue;

    public sealed record Packaged(string Path) : ModeValue;

    // --- [CODEC]
    private const string Yes = "y";
    private const string No = "n";

    internal Fin<string> Formatted(Option<FolderPath> package) =>
        Switch(
            package,
            flag: static (_, flag) => flag.Value ? Yes : No,
            whole: static (_, whole) => whole.Value.ToString(CultureInfo.InvariantCulture),
            real: static (_, real) => real.Value.ToString(CultureInfo.InvariantCulture),
            colour: static (_, colour) => colour.Value.IsEmpty ? "-1" : $"{colour.Value.R},{colour.Value.G},{colour.Value.B}",
            id: static (_, id) => id.Value.ToString("D"),
            vector: static (_, vector) => string.Create(CultureInfo.InvariantCulture, $"{vector.Value.X},{vector.Value.Y},{vector.Value.Z}"),
            text: static (_, text) => text.Value,
            packaged: static (folder, packaged) => InputPath.Of(folder.Match(Some: held => Path.GetFullPath(packaged.Path, held), None: () => packaged.Path)).Map(static path => (string)path));

    internal bool Holds(string host, Option<FolderPath> package) =>
        this is Packaged packaged
            ? InputPath.Of(package.Match(Some: folder => Path.GetFullPath(packaged.Path, folder), None: () => packaged.Path))
                .Exists(path => string.Equals(host, path, StringComparison.Ordinal))
            : Parsed(host).Exists(value => value == this);

    private Option<ModeValue> Parsed(string host) =>
        Switch(
            host,
            flag: static (text, _) => string.Equals(text, Yes, StringComparison.OrdinalIgnoreCase) ? new Flag(Value: true)
                : string.Equals(text, No, StringComparison.OrdinalIgnoreCase) ? new Flag(Value: false) : None,
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
internal static partial class Ini {
    // --- [TEXT]
    private static readonly IniParserConfiguration Configuration = new() {
        AllowDuplicateKeys = true,
        AllowDuplicateSections = true,
        OverrideDuplicateKeys = true,
        AssigmentSpacer = "",
        NewLineStr = "\r\n",
        SectionRegex = Sections,
        CommentRegex = EmptyTextures,
    };

    [GeneratedRegex(@"^\[.*\]$")]
    private static partial Regex Sections { get; }

    [GeneratedRegex("^=$")]
    private static partial Regex EmptyTextures { get; }

    internal static HashMap<(string Section, string Key), string> Read(OutputPath file) =>
        Table(new FileIniDataParser(new IniDataParser(Configuration)).ReadFile(file, Encoding.UTF8));

    internal static Unit Write(OutputPath file, Guid id, HashMap<(string Section, string Key), string> table) {
        new FileIniDataParser(new IniDataParser(Configuration)).WriteFile(file, Data(id, table), Encoding.UTF8);
        return unit;
    }

    internal static HashMap<(string Section, string Key), string> Table(string text) => Table(new IniDataParser(Configuration).Parse(text));

    internal static string Rendered(Guid id, HashMap<(string Section, string Key), string> table) => Data(id, table).ToString();

    internal static Seq<ModeKey> Merged(Seq<ModeKey> keys) =>
        toSeq(HashMap<(string Section, string Key), ModeKey>().AddOrUpdateRange(keys.Map(static key => ((key.Section, key.Key), key))).Values).Strict();

    private static HashMap<(string Section, string Key), string> Table(IniData data) =>
        HashMap<(string Section, string Key), string>().AddOrUpdateRange(
            data.Global.Select(static key => (("", key.KeyName), key.Value))
                .Concat(from section in data.Sections
                        let relative = section.SectionName.Split('\\', 3) is [_, _, var name] ? name : ""
                        from key in section.Keys
                        select ((relative, key.KeyName), key.Value)));

    private static IniData Data(Guid id, HashMap<(string Section, string Key), string> table) {
        IniData data = new() { Configuration = Configuration };
        _ = toSeq(Enumerable.OrderBy(table.AsIterable(), static row => row.Key.Section, StringComparer.Ordinal)).Iter(row =>
            data[row.Key.Section.Length is 0 ? $@"DisplayMode\{id:D}" : $@"DisplayMode\{id:D}\{row.Key.Section}"][row.Key.Key] = row.Value);
        return data;
    }

    // --- [NUMBERS]
    internal static Option<int> Integral(string text) =>
        Number(text).Filter(static value => double.IsInteger(value) && value is >= int.MinValue and <= int.MaxValue).Map(static value => (int)value);

    internal static Option<double> Number(string text) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : None;

    internal static Option<Seq<double>> Numbers(string text) =>
        toSeq(text.Split(',')).Traverse(static part => Number(part)).As();
}
