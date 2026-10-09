using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NuGet.Client;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.ContentModel;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
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
internal sealed record MissingResource<T>(string Source) : Expected($"Package source {Source} serves no {typeof(T).Name} resource, update NuGet.config", 4);

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
    public static IO<Seq<(PackageMetadataResource Metadata, FindPackageByIdResource Downloads)>> Feeds(ISettings settings) {
        static FinT<IO, T> Resource<T>(SourceRepository source) where T : class, INuGetResource =>
            (from resource in IO.liftAsync(env => source.GetResourceAsync<T>(env.Token))
             from required in IO.lift(Optional(resource).ToFin(new MissingResource<T>(source.PackageSource.Name)))
             select required).Try();

        return Collect(toSeq(SettingsUtility.GetEnabledSources(settings))
            .Map(static source => Repository.Factory.GetCoreV3(source))
            .Map(static source =>
                (Resource<PackageMetadataResource>(source), Resource<FindPackageByIdResource>(source))
                    .Apply(static (metadata, downloads) => (Metadata: metadata, Downloads: downloads))
                    .Run()
                    .As()
                    .Bind(IO.lift)));
    }

    public static IO<Option<Move>> Newest(NuGetFramework framework, SourceCacheContext cache, Seq<(PackageMetadataResource Metadata, FindPackageByIdResource Downloads)> feeds, Row row, NuGetVersion current) =>
        from published in Collect(feeds
            .Map(feed =>
                IO.liftAsync(env => feed.Metadata.GetMetadataAsync(row.Id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token))
                    .Map(releases => toSeq(releases).Map(metadata => (Metadata: metadata, feed.Downloads)))))
        from latest in IO.liftAsync(async env =>
            await published.Flatten()
                .OrderByDescending(static release => (release.Metadata.Identity.Version.Version, !release.Metadata.Identity.Version.IsPrerelease, release.Metadata.Published, release.Metadata.Identity.Version))
                .ToAsyncEnumerable()
                .Where(async (release, token) => {
                    using IPackageDownloader downloader = await release.Downloads.GetPackageDownloaderAsync(release.Metadata.Identity, cache, NullLogger.Instance, token).ConfigureAwait(false);
                    Seq<string> files = toSeq(await downloader.CoreReader.GetFilesAsync(token).ConfigureAwait(false));
                    Stream manifest = await downloader.CoreReader.GetNuspecAsync(token).ConfigureAwait(false);
                    await using ConfiguredAsyncDisposable disposal = manifest.ConfigureAwait(false);
                    return Compatible(framework, release.Metadata.Identity.Id, files, new NuspecReader(manifest));
                })
                .Select(static release => Some(release.Metadata.Identity.Version))
                .FirstOrDefaultAsync(env.Token)
                .ConfigureAwait(false))
        select latest
            .Filter(version => !VersionComparer.Default.Equals(version, current))
            .Map(version => new Move(row, version));

    public static IO<Seq<T>> Collect<T>(Seq<IO<T>> effects) =>
        effects
            .Traverse(static effect => effect.Try())
            .Run()
            .As()
            .Bind(static results => IO.lift(results.Map(static values => values.As())));

    private static bool Compatible(NuGetFramework framework, string packageId, Seq<string> files, NuspecReader nuspec) {
        string lib = $"{PackagingConstants.Folders.Lib}/";
        string reference = $"{PackagingConstants.Folders.Ref}/";
        ManagedCodeConventions conventions = new(runtimeGraph: null);
        SelectionCriteria criteria = conventions.Criteria.ForFramework(framework);
        ManagedCodeConventions.ManagedCodePatterns patterns = conventions.Patterns;
        ContentItemCollection content = new();
        content.Load(files);
        Option<FrameworkSpecificGroup> references = Optional(nuspec.GetReferenceGroups().GetNearest(framework));
        Func<ContentItem, bool> included = item =>
            !item.Path.StartsWith(lib, StringComparison.Ordinal)
            || references.Match(Some: group => group.Items.Contains(Path.GetFileName(item.Path), StringComparer.OrdinalIgnoreCase), None: static () => true);
        PatternSet[][] assemblies = [
            [patterns.CompileRefAssemblies, patterns.CompileLibAssemblies],
            [patterns.RuntimeAssemblies],
        ];
        Seq<PatternSet> builds = Seq(patterns.MSBuildFiles, patterns.MSBuildTransitiveFiles, patterns.MSBuildMultiTargetingFiles);
        List<ContentItemGroup> contentFiles = [];
        content.PopulateItemGroups(patterns.ContentFiles, contentFiles);

        return !files.Exists(file => file.StartsWith(reference, StringComparison.OrdinalIgnoreCase) || file.StartsWith(lib, StringComparison.OrdinalIgnoreCase))
            || assemblies.Any(definitions => Optional(content.FindBestItemGroup(criteria, definitions)).Exists(group => group.Items.Any(included)))
            || content.FindBestItemGroup(criteria, patterns.ResourceAssemblies) is { Items.Count: > 0 }
            || builds
                .Map(pattern => Optional(content.FindBestItemGroup(criteria, pattern)))
                .Somes()
                .Bind(static group => toSeq(group.Items))
                .Exists(item => Path.GetFileNameWithoutExtension(item.Path).Equals(packageId, StringComparison.OrdinalIgnoreCase) || Path.GetFileName(item.Path).Equals(PackagingCoreConstants.EmptyFolder, StringComparison.Ordinal))
            || contentFiles
                .GroupBy(static group => (string)group.Properties[ManagedCodeConventions.PropertyNames.CodeLanguage], StringComparer.OrdinalIgnoreCase)
                .Any(language => Optional(NuGetFrameworkUtility.GetNearest(language, framework, static group => (NuGetFramework)group.Properties[ManagedCodeConventions.PropertyNames.TargetFrameworkMoniker])).Exists(static group => group.Items.Count > 0))
            || (!framework.IsPackageBased && Optional(nuspec.GetFrameworkAssemblyGroups().GetNearest(framework)).Exists(static group => group.Items.Any()))
            || Optional(nuspec.GetFrameworkRefGroups().GetNearest(framework)).Exists(static group => group.FrameworkReferences.Any());
    }

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
        from moves in Catalog.Collect(rows
            .Choose(static row => NuGetVersion.TryParse(row.Version, out NuGetVersion? current) ? Some((Row: row, Current: current)) : None)
            .Map(pending => Catalog.Newest(framework, cache, feeds, pending.Row, pending.Current)))
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
