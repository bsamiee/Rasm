using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using LanguageExt.ClassInstances;
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
internal sealed record Row(string Id, NuGetVersion Version, bool Exact, Group Attribute);
internal sealed record Move(Row Row, NuGetVersion Version);
internal sealed record Feed(string Source, PackageMetadataResource Metadata, FindPackageByIdResource Downloads);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Packages {
    // --- [ROWS]
    public static Seq<Row> Rows(string text) {
        int[] lineStarts = [0, .. LineBreak.Matches(text).Select(static line => line.Index + line.Length)];
        return toSeq(XDocument.Parse(text, LoadOptions.SetLineInfo).Descendants())
            .Filter(static element => element.Name.LocalName is "PackageVersion" or "GlobalPackageReference")
            .Choose(element =>
                from include in Optional(element.Attribute("Include") ?? element.Attribute("Update"))
                from version in Optional(element.Attribute("Version"))
                from current in Requirement(version.Value)
                let at = (IXmlLineInfo)version
                let attribute = VersionAttribute.Match(text, lineStarts[at.LineNumber - 1] + at.LinePosition - 1).Groups["value"]
                select new Row(include.Value, current.Version, current.Exact, attribute));
    }

    private static Option<(NuGetVersion Version, bool Exact)> Requirement(string text) =>
        NuGetVersion.TryParse(text, out NuGetVersion? version) ? Some((version, false))
        : VersionRange.TryParse(text, out VersionRange? range) && range is { HasLowerAndUpperBounds: true, IsMinInclusive: true, IsMaxInclusive: true } && VersionComparer.Default.Equals(range.MinVersion, range.MaxVersion) ? Some((range.MinVersion, true))
        : None;

    [GeneratedRegex("\\r\\n|[\\r\\n]")]
    private static partial Regex LineBreak { get; }

    [GeneratedRegex("""\GVersion\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)')""")]
    private static partial Regex VersionAttribute { get; }

    // --- [RELEASES]
    public static IO<Seq<Feed>> Feeds(Seq<SourceRepository> sources) {
        static FinT<IO, T> Resource<T>(SourceRepository source) where T : class, INuGetResource =>
            (from resource in IO.liftAsync(env => source.GetResourceAsync<T>(env.Token))
             from required in IO.lift(Optional(resource).ToFin(Error.New($"Package source {source.PackageSource.Name} serves no {typeof(T).Name} resource, update NuGet.config")))
             select required).Try();

        return Collect(sources.Map(static source =>
            (Resource<PackageMetadataResource>(source), Resource<FindPackageByIdResource>(source))
                .Apply((metadata, downloads) => new Feed(source.PackageSource.Name, metadata, downloads))
                .Run()
                .As()
                .Bind(IO.lift)));
    }

    public static IO<Seq<T>> Collect<T>(Seq<IO<T>> effects) =>
        effects
            .Traverse(static effect => effect.Try())
            .Run()
            .As()
            .Bind(IO.lift);

    public static IO<Option<Move>> Newest(NuGetFramework framework, SourceCacheContext cache, PackageSourceMapping mapping, Seq<Feed> feeds, Row row) {
        HashSet<EqStringOrdinalIgnoreCase, string> configured = toHashSet<EqStringOrdinalIgnoreCase, string>(mapping.GetConfiguredPackageSources(row.Id));
        return from published in Collect(feeds
                   .Filter(feed => !mapping.IsEnabled || configured.Contains(feed.Source))
                   .Map(feed =>
                       IO.liftAsync(env => feed.Metadata.GetMetadataAsync(row.Id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token))
                           .Map(releases => toSeq(releases).Map(metadata => (Metadata: metadata, feed.Downloads)))))
               from latest in IO.liftAsync(async env =>
                   await published.Flatten()
                       .Where(release => !row.Exact || (release.Metadata.Identity.Version.Major, release.Metadata.Identity.Version.Minor) == (row.Version.Major, row.Version.Minor))
                       .OrderByDescending(static release => release.Metadata, Comparer<IPackageSearchMetadata>.Create(Rank))
                       .ToAsyncEnumerable()
                       .Where(SupportedAsync)
                       .Select(static release => Some(release.Metadata.Identity.Version))
                       .FirstOrDefaultAsync(env.Token)
                       .ConfigureAwait(false))
               select latest
                   .Filter(version => !VersionComparer.Default.Equals(version, row.Version))
                   .Map(version => new Move(row, version));

        static int Rank(IPackageSearchMetadata left, IPackageSearchMetadata right) =>
            (VersionComparer.Version.Compare(left.Identity.Version, right.Identity.Version), left.Identity.Version.ReleaseLabels.Zip(right.Identity.Version.ReleaseLabels).FirstOrDefault(static labels => !string.Equals(labels.First, labels.Second, StringComparison.OrdinalIgnoreCase))) switch {
                (not 0 and var bases, _) => bases,
                (_, (string first, string second)) when !(first.All(char.IsAsciiDigit) && second.All(char.IsAsciiDigit)) && Nullable.Compare(left.Published, right.Published) is not 0 and var published => published,
                _ => VersionComparer.Default.Compare(left.Identity.Version, right.Identity.Version),
            };

        ValueTask<bool> SupportedAsync((IPackageSearchMetadata Metadata, FindPackageByIdResource Downloads) release, CancellationToken token) =>
            toSeq(release.Metadata.DependencySets) is { IsEmpty: false } groups && groups.ForAll(static group => group.TargetFramework.IsSpecificFramework)
                ? ValueTask.FromResult(NuGetFrameworkUtility.GetNearest(groups, framework, static group => group.TargetFramework) is not null)
                : InspectedAsync(release.Metadata.Identity, release.Downloads, token);

        async ValueTask<bool> InspectedAsync(PackageIdentity listed, FindPackageByIdResource downloads, CancellationToken token) {
            FileStream nupkg = File.Create(Path.GetTempFileName(), 4096, FileOptions.DeleteOnClose);
            await using ConfiguredAsyncDisposable disposal = nupkg.ConfigureAwait(false);
            if (!await downloads.CopyNupkgToStreamAsync(listed.Id, listed.Version, nupkg, cache, NullLogger.Instance, token).ConfigureAwait(false))
                return false;
            using PackageArchiveReader package = new(nupkg, leaveStreamOpen: true);
            return Compatible(listed.Id, toSeq(await package.GetFilesAsync(token).ConfigureAwait(false)), package.NuspecReader);
        }

        bool Compatible(string packageId, Seq<string> files, NuspecReader nuspec) {
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
    }

    // --- [REWRITE]
    public static string Rewrite(string text, Seq<Move> moves) {
        StringBuilder rewritten = new(text.Length);
        int end = 0;
        foreach (Move move in moves) {
            Group attribute = move.Row.Attribute;
            _ = rewritten.Append(text.AsSpan(end, attribute.Index - end)).Append(Spelling(move));
            end = attribute.Index + attribute.Length;
        }
        string versioned = rewritten.Append(text.AsSpan(end)).ToString();
        int column = TrailingComment.Matches(versioned).Select(static row => row.Groups["code"].Length).DefaultIfEmpty().Max() + 1;
        return TrailingComment.Replace(versioned, row => row.Groups["code"].Value.PadRight(column));
    }

    public static string Spelling(Move move) =>
        new VersionRange(move.Version, includeMinVersion: true, move.Row.Exact ? move.Version : null, includeMaxVersion: move.Row.Exact).ToString("A", VersionRangeFormatter.Instance);

    [GeneratedRegex("""^(?<code>[^\r\n]*?\S)[ \t]*(?=<!--)""", RegexOptions.Multiline)]
    private static partial Regex TrailingComment { get; }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    private const int ConcurrentRows = 256;

    public static async Task<int> Main(string[] args) =>
        (await Upgrade(args).RunSafeAsync().ConfigureAwait(false)).Match(
            Succ: Report,
            Fail: Report);

    private static IO<Seq<Move>> Upgrade(string[] args) =>
        from path in IO.lift(Fin<string> () => args is [var file] ? Path.GetFullPath(file) : Error.New("Pass one Directory.Packages.props path"))
        from framework in IO.lift(static () => Optional(AppContext.TargetFrameworkName).Map(NuGetFramework.Parse).ToFin(Error.New("Entry assembly names no target framework")))
        from text in IO.liftAsync(env => File.ReadAllTextAsync(path, env.Token))
        let rows = Packages.Rows(text)
        from cache in use(static () => new SourceCacheContext())
        let settings = Settings.LoadDefaultSettings(Path.GetDirectoryName(path))
        let mapping = PackageSourceMapping.GetPackageSourceMapping(settings)
        from feeds in Packages.Feeds(toSeq(SettingsUtility.GetEnabledSources(settings)).Map(static source => Repository.Factory.GetCoreV3(source)))
        from moves in toSeq(rows.Chunk(ConcurrentRows))
            .TraverseM(chunk => Packages.Collect(toSeq(chunk).Map(row => Packages.Newest(framework, cache, mapping, feeds, row))))
            .As()
            .Map(static chunks => chunks.Flatten().Somes())
        from written in when(!moves.IsEmpty, IO.liftAsync(env => File.WriteAllTextAsync(path, Packages.Rewrite(text, moves), env.Token).ToUnit())).As()
        select moves;

    private static int Report(Seq<Move> moves) {
        _ = moves.Iter(static move => Console.WriteLine($"{move.Row.Id} {move.Row.Attribute.Value} -> {Packages.Spelling(move)}"));
        return 0;
    }

    private static int Report(Error error) {
        Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
        return 1;
    }
}
