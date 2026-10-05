using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Rasm.Upgrade;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Row(string Id, string Version, int Line, Match Attribute);

internal sealed record Move(Row Row, NuGetVersion Version);

// --- [ERRORS] --------------------------------------------------------------------------
internal sealed record Usage() : Expected("Pass one argument, the Directory.Packages.props file to upgrade", 2);

internal sealed record UnknownFramework() : Expected("Entry assembly names no target framework to match releases against", 3);

internal sealed record MissingMetadata(string Source) : Expected($"Package source {Source} serves no package metadata resource. Correct the source URL in NuGet.config", 4);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Catalog {
    // --- [ROWS]
    public static Seq<Row> Rows(Lst<string> lines, XDocument document) =>
        toSeq(document.Descendants("PackageVersion")).Choose(element =>
            from include in Optional(element.Attribute("Include"))
            from version in Optional(element.Attribute("Version"))
            let at = (IXmlLineInfo)version
            from line in lines.At(at.LineNumber - 1)
            let attribute = VersionAttribute.Match(line, at.LinePosition - 1)
            where attribute.Success
            select new Row(include.Value, version.Value, at.LineNumber - 1, attribute));

    // --- [RELEASES]
    public static IO<Seq<PackageMetadataResource>> Feeds(ISettings settings) =>
        toSeq(SettingsUtility.GetEnabledSources(settings))
            .Traverse(static source =>
                from resource in IO.liftAsync(env => Repository.Factory.GetCoreV3(source).GetResourceAsync<PackageMetadataResource>(env.Token))
                from metadata in IO.lift(Optional(resource).ToFin(new MissingMetadata(source.Name)))
                select metadata)
            .As();

    public static IO<Option<Move>> Newest(NuGetFramework framework, SourceCacheContext cache, Seq<PackageMetadataResource> feeds, Row row, NuGetVersion current) =>
        feeds
            .Traverse(feed => IO.liftAsync(env => feed.GetMetadataAsync(row.Id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token)).Map(static published => toSeq(published)))
            .As()
            .Map(published =>
                Optional(published.Flatten().Filter(metadata => Compatible(framework, metadata)).MaxBy(static metadata => (metadata.Identity.Version.Version, !metadata.Identity.Version.IsPrerelease, metadata.Published, metadata.Identity.Version)))
                    .Map(static metadata => metadata.Identity.Version)
                    .Filter(version => !VersionComparer.Default.Equals(version, current))
                    .Map(version => new Move(row, version)));

    private static bool Compatible(NuGetFramework framework, IPackageSearchMetadata metadata) =>
        toSeq(metadata.DependencySets).Map(static set => set.TargetFramework) switch {
            { IsEmpty: true } => true,
            var frameworks => new FrameworkReducer().GetNearest(framework, frameworks) is not null,
        };

    // --- [REWRITE]
    public static string Rewrite(Lst<string> lines, Seq<Row> rows, Seq<Move> moves) {
        Option<int> column = toSeq(rows.Map(static row => row.Attribute.Groups["comment"]).Filter(static comment => comment.Success).CountBy(static comment => comment.Index).OrderBy(static count => (count.Value, count.Key)))
            .Last
            .Map(static count => count.Key);
        return string.Join('\n', moves.Fold(lines, (current, move) => current.SetItem(move.Row.Line, Spliced(lines[move.Row.Line], move.Row.Attribute, move.Version, column))));
    }

    private static string Spliced(string line, Match attribute, NuGetVersion version, Option<int> column) =>
        string.Concat(line.AsSpan(0, attribute.Groups["value"].Index), version.ToNormalizedString(), attribute.Groups["quote"].Value, attribute.Groups["close"].Value) switch {
            var head when attribute.Groups["comment"].Success => string.Concat(head.PadRight(column.Filter(at => at > head.Length).IfNone(head.Length + 1)), attribute.Groups["comment"].Value),
            var head => head,
        };

    [GeneratedRegex("""\GVersion\s*=\s*(?<quote>["'])(?<value>[^"']*)\k<quote>(?<close>[^<]*?)\s*(?<comment><!--.*)?$""", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex VersionAttribute { get; }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    public static int Main(string[] args) =>
        Upgrade(args).RunSafe().Match(
            Succ: Report,
            Fail: Report);

    private static IO<Seq<Move>> Upgrade(string[] args) =>
        from path in IO.lift(Fin<string> () => args is [var file] ? Path.GetFullPath(file) : new Usage())
        from framework in IO.lift(static () => Optional(AppContext.TargetFrameworkName).Map(NuGetFramework.Parse).ToFin(new UnknownFramework()))
        from text in IO.lift(() => File.ReadAllText(path))
        let lines = toList(text.Split('\n'))
        let rows = Catalog.Rows(lines, XDocument.Parse(text, LoadOptions.SetLineInfo))
        from cache in use(static () => new SourceCacheContext { NoCache = true })
        from feeds in Catalog.Feeds(Settings.LoadDefaultSettings(Path.GetDirectoryName(path)))
        from moves in rows
            .Choose(static row => NuGetVersion.TryParse(row.Version, out NuGetVersion? current) ? Some((Row: row, Current: current)) : None)
            .Traverse(pending => Catalog.Newest(framework, cache, feeds, pending.Row, pending.Current))
            .As()
            .Map(static moves => moves.Somes())
        from written in when(!moves.IsEmpty, IO.lift(() => File.WriteAllText(path, Catalog.Rewrite(lines, rows, moves)))).As()
        select moves;

    private static int Report(Seq<Move> moves) {
        _ = moves.Iter(static move => Console.WriteLine($"{move.Row.Id} {move.Row.Version} -> {move.Version.ToNormalizedString()}"));
        return 0;
    }

    private static int Report(Error error) {
        Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
        return error.Filter<Expected>().Head is Expected expected ? expected.Code : 1;
    }
}
