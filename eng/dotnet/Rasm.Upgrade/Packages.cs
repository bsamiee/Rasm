using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CliWrap;
using CliWrap.Buffered;
using LanguageExt.ClassInstances;
using Microsoft.Extensions.DependencyModel;
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
internal sealed record Host(HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion> Library, HashMap<EqStringOrdinalIgnoreCase, string, string> Assembly) {
    public static Host Read(string resources) {
        using FileStream dependencies = File.OpenRead(Directory.GetFiles(resources, "dotnetstart.*.deps.json").Single());
        using DependencyContextJsonReader reader = new();
        return new(
            toHashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion>(reader.Read(dependencies).RuntimeLibraries.Select(static library => (library.Name, NuGetVersion.Parse(library.Version)))),
            toHashMap<EqStringOrdinalIgnoreCase, string, string>(Directory.EnumerateDirectories(Path.Join(resources, "ManagedPlugIns"), "*.rhp").Prepend(resources).SelectMany(static folder => Directory.EnumerateFiles(folder, "*.dll")).Select(static file => (Path.GetFileNameWithoutExtension(file), file))));
    }

    public Option<Version> Ceiling(string id) =>
        Assembly.Find(id).Bind(static file => Optional(AssemblyName.GetAssemblyName(file).Version));
}
internal sealed record Project(
    NuGetFramework TargetFramework,
    bool CentralPackageTransitivePinningEnabled,
    HashSet<EqStringOrdinalIgnoreCase, string> PackageReference,
    HashMap<EqStringOrdinalIgnoreCase, string, string> PackageVersion,
    Option<Host> Host) {
    public bool Consumes(Row row) =>
        PackageVersion.Find(row.Id).Exists(version => version.Equals(row.Attribute.Value, StringComparison.Ordinal))
        && (CentralPackageTransitivePinningEnabled || PackageReference.Contains(row.Id));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Packages {
    // --- [ROWS]
    public static Seq<Row> Rows(string text) {
        int[] lineStarts = [0, .. LineBreak.Matches(text).Select(static line => line.Index + line.Length)];
        return toSeq(Located(text).Descendants())
            .Filter(static element => element.Name.LocalName is nameof(Project.PackageVersion) or "GlobalPackageReference")
            .Choose(element =>
                from include in Optional(element.Attribute("Include") ?? element.Attribute("Update"))
                from version in Optional(element.Attribute("Version"))
                from current in Requirement(version.Value)
                let at = (IXmlLineInfo)version
                let attribute = VersionAttribute.Match(text, lineStarts[at.LineNumber - 1] + at.LinePosition - 1).Groups["value"]
                select new Row(include.Value, current.Version, current.Exact, attribute));
    }

    private static XDocument Located(string xml) =>
        XDocument.Parse(xml, LoadOptions.SetLineInfo);

    private static Option<(NuGetVersion Version, bool Exact)> Requirement(string text) =>
        VersionRange.TryParse(text, out VersionRange? range) && range is { HasLowerBound: true, IsMinInclusive: true, IsFloating: false } && (!range.HasUpperBound || (range.IsMaxInclusive && VersionComparer.Default.Equals(range.MinVersion, range.MaxVersion)))
            ? Some((range.MinVersion, range.HasUpperBound))
            : None;

    [GeneratedRegex("\\r\\n|[\\r\\n]")]
    private static partial Regex LineBreak { get; }

    [GeneratedRegex("""\GVersion\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)')""")]
    private static partial Regex VersionAttribute { get; }

    // --- [PROJECTS]
    public static IO<Seq<Project>> Projects(string? root) =>
        IO.liftAsync(env => File.ReadAllTextAsync(Path.Join(root, "Workspace.slnx"), env.Token))
            .Bind(solution => Collect(toSeq(Located(solution).Descendants("Project").Attributes("Path")).Map(path => Evaluate(Path.Join(root, path.Value)))))
            .Bind(static evaluated => IO.lift(() => toHashMap<EqStringOrdinal, string, Host>(evaluated.Map(static held => held.Resources).Filter(static resources => resources is { Length: > 0 }).Distinct().Map(static resources => (resources, Host.Read(resources)))) switch {
                var hosts => evaluated.Map(held => held.Project with { Host = hosts.Find(held.Resources) }),
            }));

    private static IO<(Project Project, string Resources)> Evaluate(string path) =>
        IO.liftAsync(env => Cli.Wrap("dotnet")
                .WithArguments([
                    "msbuild",
                    path,
                    "-t:ResolveRhinoInstallation",
                    $"-getProperty:{nameof(Project.TargetFramework)}",
                    $"-getProperty:{nameof(Project.CentralPackageTransitivePinningEnabled)}",
                    "-getProperty:RhinoResourcesDir",
                    $"-getItem:{nameof(Project.PackageReference)}",
                    $"-getItem:{nameof(Project.PackageVersion)}",
                ])
                .ExecuteBufferedAsync(env.Token)
                .Task)
            .Map(static result => {
                JsonElement build = JsonSerializer.Deserialize<JsonElement>(result.StandardOutput);
                JsonElement properties = build.GetProperty("Properties");
                JsonElement items = build.GetProperty("Items");
                Seq<JsonElement> Listed(string item) => toSeq(items.GetProperty(item).EnumerateArray());
                static string Identity(JsonElement row) => row.GetProperty("Identity").ToString();
                static string Version(JsonElement row) => row.GetProperty("Version").ToString();
                return (new Project(
                    NuGetFramework.Parse(properties.GetProperty(nameof(Project.TargetFramework)).ToString()),
                    bool.TryParse(properties.GetProperty(nameof(Project.CentralPackageTransitivePinningEnabled)).ToString(), out bool pinning) && pinning,
                    toHashSet<EqStringOrdinalIgnoreCase, string>(Listed(nameof(Project.PackageReference)).Map(Identity)),
                    toHashMap<EqStringOrdinalIgnoreCase, string, string>(Listed(nameof(Project.PackageVersion)).Map(static row => (Identity(row), Version(row)))),
                    None), properties.GetProperty("RhinoResourcesDir").ToString());
            });

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
        effects.Traverse(static effect => effect.Try()).As().Run().As().Bind(IO.lift);

    public static IO<Option<Move>> Newest(Seq<Project> projects, SourceCacheContext cache, PackageSourceMapping mapping, Seq<Feed> feeds, Row row) {
        Seq<Project> consumers = projects.Filter(project => project.Consumes(row));
        Seq<NuGetFramework> frameworks = consumers.Map(static consumer => consumer.TargetFramework).Distinct();
        HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion> host = consumers.Head
            .Filter(_ => row.Exact && consumers.ForAll(static consumer => consumer.Host.IsSome))
            .Bind(static consumer => consumer.Host)
            .Map(static held => held.Library)
            .IfNone(HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion>());
        Option<Version> ceiling = consumers
            .Filter(consumer => consumer.PackageReference.Contains(row.Id))
            .Choose(static consumer => consumer.Host)
            .Head
            .Bind(held => held.Ceiling(row.Id));
        return host.Find(row.Id).Match(
                Some: static version => IO.pure(Some(version)),
                None: () => Collect(Serving(row.Id).Map(feed =>
                        IO.liftAsync(env => feed.Metadata.GetMetadataAsync(row.Id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token))
                            .Map(releases => toSeq(releases).Map(metadata => (Metadata: metadata, feed.Downloads)))))
                    .Bind(published => IO.liftAsync(async env =>
                        await published.Flatten()
                            .Where(release => !host.IsEmpty || !row.Exact || (release.Metadata.Identity.Version.Major, release.Metadata.Identity.Version.Minor) == (row.Version.Major, row.Version.Minor))
                            .Where(release => ceiling.ForAll(limit => release.Metadata.Identity.Version.Version <= limit))
                            .OrderByDescending(static release => release.Metadata.Identity.Version.Version)
                            .ThenBy(static release => release.Metadata.Identity.Version.IsPrerelease)
                            .ThenByDescending(static release => release.Metadata.Published?.UtcDateTime.Date)
                            .ThenByDescending(static release => release.Metadata.Identity.Version, VersionComparer.VersionRelease)
                            .ToAsyncEnumerable()
                            .Where((release, token) => AdmittedAsync(release.Metadata, token))
                            .Where(SupportedAsync)
                            .Select(static release => Some(release.Metadata.Identity.Version))
                            .FirstOrDefaultAsync(env.Token)
                            .ConfigureAwait(false))))
            .Map(latest => latest
                .Filter(version => !VersionComparer.Default.Equals(version, row.Version))
                .Map(version => new Move(row, version)));

        Seq<Feed> Serving(string id) =>
            feeds.Filter(feed => !mapping.IsEnabled || mapping.GetConfiguredPackageSources(id).Contains(feed.Source, StringComparer.OrdinalIgnoreCase));

        async ValueTask<bool> AdmittedAsync(IPackageSearchMetadata release, CancellationToken token) {
            Seq<PackageDependency> dependencies = frameworks
                .Choose(framework => Optional(release.DependencySets.GetNearest(framework)))
                .Bind(static group => toSeq(group.Packages));
            return host.IsEmpty
                || (dependencies.ForAll(dependency => host.Find(dependency.Id).ForAll(dependency.VersionRange.Satisfies))
                && await dependencies
                    .Filter(dependency => !host.ContainsKey(dependency.Id) && dependency.VersionRange.HasLowerBound)
                    .ToAsyncEnumerable()
                    .AllAsync((dependency, next) => Serving(dependency.Id)
                        .ToAsyncEnumerable()
                        .AnyAsync(async (feed, inner) =>
                            await feed.Metadata.GetMetadataAsync(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), cache, NullLogger.Instance, inner).ConfigureAwait(false) is { } resolved
                            && await AdmittedAsync(resolved, inner).ConfigureAwait(false), next), token)
                    .ConfigureAwait(false));
        }

        async ValueTask<bool> SupportedAsync((IPackageSearchMetadata Metadata, FindPackageByIdResource Downloads) release, CancellationToken token) {
            FileStream nupkg = new(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
                new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Options = FileOptions.Asynchronous | FileOptions.DeleteOnClose });
            await using ConfiguredAsyncDisposable disposal = nupkg.ConfigureAwait(false);
            PackageIdentity listed = release.Metadata.Identity;
            if (!await release.Downloads.CopyNupkgToStreamAsync(listed.Id, listed.Version, nupkg, cache, NullLogger.Instance, token).ConfigureAwait(false))
                return false;
            using PackageArchiveReader package = new(nupkg, leaveStreamOpen: true);
            Seq<string> files = toSeq(await package.GetFilesAsync(token).ConfigureAwait(false));
            return frameworks.ForAll(framework => Compatible(framework, files, package.NuspecReader));
        }

        static bool Compatible(NuGetFramework framework, Seq<string> files, NuspecReader nuspec) {
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
            List<ContentItemGroup> contentFiles = [];
            content.PopulateItemGroups(patterns.ContentFiles, contentFiles);

            return !nuspec.GetPackageTypes().Any(static type => type == PackageType.DotnetTool || type == PackageType.DotnetPlatform)
                && (!files.Exists(file => file.StartsWith(reference, StringComparison.OrdinalIgnoreCase) || file.StartsWith(lib, StringComparison.OrdinalIgnoreCase))
                || assemblies.Any(definitions => Optional(content.FindBestItemGroup(criteria, definitions)).Exists(group => group.Items.Any(included)))
                || content.FindBestItemGroup(criteria, patterns.ResourceAssemblies) is { Items.Count: > 0 }
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
    public static async Task<int> Main(string[] args) =>
        (await Upgrade(args).RunSafeAsync().ConfigureAwait(false)).Match(
            Succ: Report,
            Fail: Report);

    private static IO<Seq<Move>> Upgrade(string[] args) =>
        from path in IO.lift(Fin<string> () => args is [var file] ? Path.GetFullPath(file) : Error.New("Pass one Directory.Packages.props path"))
        from text in IO.liftAsync(env => File.ReadAllTextAsync(path, env.Token))
        let rows = Packages.Rows(text)
        let root = Path.GetDirectoryName(path)
        from projects in Packages.Projects(root)
        from cache in use(static () => new SourceCacheContext())
        let settings = Settings.LoadDefaultSettings(root)
        let mapping = PackageSourceMapping.GetPackageSourceMapping(settings)
        from feeds in Packages.Feeds(toSeq(SettingsUtility.GetEnabledSources(settings)).Map(static source => Repository.Factory.GetCoreV3(source)))
        from moves in Packages.Collect(rows.Filter(row => projects.Exists(project => project.Consumes(row))).Map(row => Packages.Newest(projects, cache, mapping, feeds, row))).Map(static releases => releases.Somes())
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
