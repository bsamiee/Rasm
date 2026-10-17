using System.Runtime.CompilerServices;
using System.Text;
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
internal sealed record Row(string Id, NuGetVersion Version, Group Attribute);
internal sealed record Move(Row Row, NuGetVersion Version);

// --- [ERRORS] --------------------------------------------------------------------------
internal sealed record Usage() : Expected("Pass one argument, the Directory.Packages.props file to upgrade", 0);
internal sealed record UnknownFramework() : Expected("Entry assembly names no target framework to match releases against", 0);
internal sealed record MissingResource(string Source, Type Resource) : Expected($"Package source {Source} serves no {Resource.Name} resource, update NuGet.config", 0);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Packages {
    // --- [ROWS]
    public static Seq<Row> Rows(string text) {
        int[] lineStarts = [0, .. LineBreak.Matches(text).Select(static line => line.Index + line.Length)];
        return toSeq(XDocument.Parse(text, LoadOptions.SetLineInfo).Descendants())
            .Filter(static element => element.Name.LocalName.Equals("PackageVersion", StringComparison.Ordinal) || element.Name.LocalName.Equals("GlobalPackageReference", StringComparison.Ordinal))
            .Choose(element =>
                from include in Optional(element.Attribute("Include") ?? element.Attribute("Update"))
                from version in Optional(element.Attribute("Version"))
                from current in NuGetVersion.TryParse(version.Value, out NuGetVersion? parsed) ? Some(parsed) : None
                let at = (IXmlLineInfo)version
                let attribute = VersionAttribute.Match(text, lineStarts[at.LineNumber - 1] + at.LinePosition - 1).Groups["value"]
                select new Row(include.Value, current, attribute));
    }

    [GeneratedRegex("\\r\\n|[\\r\\n]")]
    private static partial Regex LineBreak { get; }

    [GeneratedRegex("""\GVersion\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)')""")]
    private static partial Regex VersionAttribute { get; }

    // --- [RELEASES]
    public static IO<Seq<(PackageMetadataResource Metadata, FindPackageByIdResource Downloads)>> Feeds(PackageSourceMapping mapping, Seq<SourceRepository> sources, string packageId) {
        static FinT<IO, T> Resource<T>(SourceRepository source) where T : class, INuGetResource =>
            (from resource in IO.liftAsync(env => source.GetResourceAsync<T>(env.Token))
             from required in IO.lift(Optional(resource).ToFin(new MissingResource(source.PackageSource.Name, typeof(T))))
             select required).Try();

        System.Collections.Generic.HashSet<string> configured = mapping.GetConfiguredPackageSources(packageId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return Collect(sources
            .Filter(source => !mapping.IsEnabled || configured.Contains(source.PackageSource.Name))
            .Map(static source =>
                (Resource<PackageMetadataResource>(source), Resource<FindPackageByIdResource>(source))
                    .Apply(static (metadata, downloads) => (Metadata: metadata, Downloads: downloads))
                    .Run()
                    .As()
                    .Bind(IO.lift)));
    }

    public static IO<Option<Move>> Newest(NuGetFramework framework, SourceCacheContext cache, PackageSourceMapping mapping, Seq<SourceRepository> sources, Row row) =>
        from feeds in Feeds(mapping, sources, row.Id)
        from published in Collect(feeds
            .Map(feed =>
                IO.liftAsync(env => feed.Metadata.GetMetadataAsync(row.Id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token))
                    .Map(releases => toSeq(releases).Map(metadata => (Metadata: metadata, feed.Downloads)))))
        from latest in IO.liftAsync(async env =>
            await published.Flatten()
                .OrderByDescending(static release => (release.Metadata.Identity.Version.Version, !release.Metadata.Identity.Version.IsPrerelease, release.Metadata.Published, release.Metadata.Identity.Version))
                .ToAsyncEnumerable()
                .Where(async (release, token) => {
                    FileStream nupkg = File.Create(Path.GetTempFileName(), 4096, FileOptions.DeleteOnClose);
                    await using ConfiguredAsyncDisposable disposal = nupkg.ConfigureAwait(false);
                    if (!await release.Downloads.CopyNupkgToStreamAsync(release.Metadata.Identity.Id, release.Metadata.Identity.Version, nupkg, cache, NullLogger.Instance, token).ConfigureAwait(false))
                        return false;
                    using PackageArchiveReader package = new(nupkg, leaveStreamOpen: true);
                    return Compatible(framework, release.Metadata.Identity.Id, toSeq(await package.GetFilesAsync(token).ConfigureAwait(false)), package.NuspecReader);
                })
                .Select(static release => Some(release.Metadata.Identity.Version))
                .FirstOrDefaultAsync(env.Token)
                .ConfigureAwait(false))
        select latest
            .Filter(version => !VersionComparer.Default.Equals(version, row.Version))
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
            || references.ForAll(group => group.Items.Contains(Path.GetFileName(item.Path), StringComparer.OrdinalIgnoreCase));
        PatternSet[][] assemblies = [
            [patterns.CompileRefAssemblies, patterns.CompileLibAssemblies],
            [patterns.RuntimeAssemblies],
        ];
        Seq<PatternSet> builds = Seq(patterns.MSBuildFiles, patterns.MSBuildTransitiveFiles, patterns.MSBuildMultiTargetingFiles);
        List<ContentItemGroup> contentFiles = [];
        content.PopulateItemGroups(patterns.ContentFiles, contentFiles);

        return !nuspec.GetPackageTypes().Any(static type => type == PackageType.DotnetTool || type == PackageType.DotnetPlatform)
            && (!files.Exists(file => file.StartsWith(reference, StringComparison.OrdinalIgnoreCase) || file.StartsWith(lib, StringComparison.OrdinalIgnoreCase))
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
            || Optional(nuspec.GetFrameworkRefGroups().GetNearest(framework)).Exists(static group => group.FrameworkReferences.Any()));
    }

    // --- [REWRITE]
    public static string Rewrite(string text, Seq<Move> moves) {
        StringBuilder rewritten = new(text.Length);
        int end = 0;
        foreach (Move move in moves) {
            Group attribute = move.Row.Attribute;
            _ = rewritten.Append(text.AsSpan(end, attribute.Index - end)).Append(move.Version.ToNormalizedString());
            end = attribute.Index + attribute.Length;
        }
        string versioned = rewritten.Append(text.AsSpan(end)).ToString();
        int column = TrailingComment.Matches(versioned).Select(static row => row.Groups["code"].Length).DefaultIfEmpty().Max() + 1;
        return TrailingComment.Replace(versioned, row => row.Groups["code"].Value.PadRight(column));
    }

    [GeneratedRegex("""^(?<code>[^\r\n]*?\S)[ \t]*(?=<!--)""", RegexOptions.Multiline)]
    private static partial Regex TrailingComment { get; }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    public static async Task<int> Main(string[] args) =>
        (await Upgrade(args).RunSafeAsync().ConfigureAwait(false)).Match(
            Succ: Report,
            Fail: Report);

    private static IO<Seq<Move>> Upgrade(string[] args) =>
        from path in IO.lift(Fin<string> () => args is [var file] ? Path.GetFullPath(file) : new Usage())
        from framework in IO.lift(static () => Optional(AppContext.TargetFrameworkName).Map(NuGetFramework.Parse).ToFin(new UnknownFramework()))
        from text in IO.liftAsync(env => File.ReadAllTextAsync(path, env.Token))
        let rows = Packages.Rows(text)
        from cache in use(static () => new SourceCacheContext { NoCache = true })
        let settings = Settings.LoadDefaultSettings(Path.GetDirectoryName(path))
        let mapping = PackageSourceMapping.GetPackageSourceMapping(settings)
        let sources = toSeq(SettingsUtility.GetEnabledSources(settings)).Map(static source => Repository.Factory.GetCoreV3(source))
        from moves in Packages.Collect(rows
            .Map(row => Packages.Newest(framework, cache, mapping, sources, row)))
            .Map(static moves => moves.Somes())
        from written in when(!moves.IsEmpty, IO.liftAsync(env => File.WriteAllTextAsync(path, Packages.Rewrite(text, moves), env.Token).ToUnit())).As()
        select moves;

    private static int Report(Seq<Move> moves) {
        _ = moves.Iter(static move => Console.WriteLine($"{move.Row.Id} {move.Row.Version.OriginalVersion} -> {move.Version.ToNormalizedString()}"));
        return 0;
    }

    private static int Report(Error error) {
        Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
        return error.Filter<Expected>().Head switch {
            Usage => 2,
            UnknownFramework => 3,
            MissingResource => 4,
            _ => 1,
        };
    }
}
