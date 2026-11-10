using System.Reflection;
using LanguageExt.ClassInstances;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Extensions.DependencyModel;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace MsbuildAnalyzer;

// --- [MODELS] ---------------------------------------------------------------------------

internal sealed record Row(int Line, string Attribute, string Id, string Text, VersionRange Range, NuGetVersion Held);

internal sealed record Host(HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion> Libraries, HashMap<EqStringOrdinalIgnoreCase, string, Version> Declared, Seq<string> Folders) {
    public Option<Version> Loaded(string assembly) =>
        Folders.Map(folder => Path.Combine(folder, assembly)).Find(File.Exists).Bind(found => Declared.Find(assembly) || Optional(AssemblyName.GetAssemblyName(found).Version));
}

internal sealed record Consumer(NuGetFramework Framework, Option<Host> Host);

internal sealed record Installed(string Folder, RepositoryMetadata Repository, Seq<FrameworkSpecificGroup> Assemblies);

internal sealed record Candidate(Row Row, Seq<Consumer> Consumers, Option<RepositoryMetadata> Repository, Either<string, Seq<IPackageSearchMetadata>> Newer);

// --- [SERVICES] -------------------------------------------------------------------------

internal sealed record Feeds(Seq<SourceRepository> Repositories, PackageSourceMapping Mapping, PackageDownloadContext Download, string Folder) {
    public async Task<Seq<IPackageSearchMetadata>> ReleasesAsync(string id, CancellationToken token) =>
        toSeq(await Task.WhenAll(Serving(id).Map(async repository => await repository.GetResourceAsync<PackageMetadataResource>(token).ConfigureAwait(false) is { } resource
            ? toSeq(await resource.GetMetadataAsync(id, includePrerelease: true, includeUnlisted: true, Download.SourceCacheContext, NullLogger.Instance, token).ConfigureAwait(false))
            : Seq<IPackageSearchMetadata>())).ConfigureAwait(false)).Flatten();

    public async Task<Option<Installed>> InstalledAsync(PackageIdentity package, CancellationToken token) =>
        await Serving(package.Id).ToAsyncEnumerable()
            .Select(async (repository, next) => {
                using DownloadResourceResult? result = await repository.GetResourceAsync<DownloadResource>(next).ConfigureAwait(false) is { } resource
                    ? await resource.GetDownloadResourceResultAsync(package, Download, Folder, NullLogger.Instance, next).ConfigureAwait(false)
                    : null;
                return result is { Status: DownloadResourceResultStatus.Available, PackageReader: { } reader }
                    ? Some(new Installed(
                        new VersionFolderPathResolver(Folder).GetInstallPath(package.Id, package.Version),
                        reader.NuspecReader.GetRepositoryMetadata(),
                        Seq(PackagingConstants.Folders.Ref, PackagingConstants.Folders.Lib).Bind(folder => toSeq(reader.GetItems(folder))).Strict()))
                    : None;
            })
            .FirstOrDefaultAsync(static found => found.IsSome, token)
            .ConfigureAwait(false);

    private Seq<SourceRepository> Serving(string id) =>
        Repositories.Filter(repository => !Mapping.IsEnabled || Mapping.GetConfiguredPackageSources(id).Contains(repository.PackageSource.Name, StringComparer.OrdinalIgnoreCase));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

internal static class Packages {
    private static readonly FrozenSet<string> Pinned = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, "PackageVersion", "GlobalPackageReference");

    public static Seq<Row> Rows(ProjectRootElement central) =>
        toSeq(central.Items)
            .Filter(static item => Pinned.Contains(item.ItemType))
            .Choose(static item => toSeq(item.Metadata)
                .Find(static metadata => metadata.ExpressedAsAttribute && metadata.Name is "Version")
                .Bind(metadata => VersionRange.TryParse(metadata.Value, out VersionRange? range) && range.MinVersion is { } held
                    ? Some(new Row(item.Location.Line - 1, metadata.Name, item.Include is { Length: > 0 } included ? included : item.Update, metadata.Value, range, held))
                    : None));

    public static ILookup<PackageIdentity, Consumer> Consumed(Seq<string> projects) {
        EvaluationContext context = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared);
        using ProjectCollection collection = new();
        Seq<(string Lock, string Host)> read = projects
            .Map(project => Workspace.Load(Path.GetFullPath(project), Workspace.Defaults, context, collection))
            .Map(static instance => (Workspace.Locked(instance), instance.GetPropertyValue("RhinoResourcesDir")))
            .Strict();
        HashMap<string, Host> hosts = toHashMap(read.Map(static held => held.Host).Filter(static resources => resources.Length > 0).Distinct().Map(static resources => {
            using FileStream dependencies = File.OpenRead(Directory.GetFiles(resources, "dotnetstart.*.deps.json").Single());
            using DependencyContextJsonReader reader = new();
            Seq<RuntimeLibrary> libraries = toSeq(reader.Read(dependencies).RuntimeLibraries);
            return (resources, new Host(
                libraries.Filter(static library => library.Hash is { Length: > 0 }).Fold(HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion>(), static (held, library) => held.TryAdd(library.Name, NuGetVersion.Parse(library.Version))),
                libraries.Bind(static library => toSeq(library.RuntimeAssemblyGroups)).Bind(static group => toSeq(group.RuntimeFiles))
                    .Fold(HashMap<EqStringOrdinalIgnoreCase, string, Version>(), static (held, file) => Version.TryParse(file.AssemblyVersion, out Version? version) ? held.TryAdd(Path.GetFileName(file.Path), version) : held),
                Seq(resources) + toSeq(Directory.GetDirectories(Path.Combine(resources, "ManagedPlugIns"), "*.rhp"))));
        }));
        return (
            from held in read.Filter(static held => File.Exists(held.Lock))
            from target in toSeq(PackagesLockFileFormat.Read(held.Lock).Targets)
            from dependency in toSeq(target.Dependencies)
            where dependency.Type is not PackageDependencyType.Project
            select (Identity: new PackageIdentity(dependency.Id, dependency.ResolvedVersion), Consumer: new Consumer(target.TargetFramework, hosts.Find(held.Host))))
            .ToLookup(static pair => pair.Identity, static pair => pair.Consumer);
    }

    public static IO<Seq<(Row Row, Either<string, NuGetVersion> Decision)>> Decided(Feeds feeds, Seq<Row> rows, ILookup<PackageIdentity, Consumer> consumed) =>
        IO.liftAsync(async env => {
            Seq<Candidate> offered = toSeq(await Task.WhenAll(rows.Map(row => OfferedAsync(feeds, consumed, row, env.Token))).ConfigureAwait(false));
            Seq<(Row Row, Either<string, NuGetVersion> Decision)>[] moved = await Task.WhenAll(offered
                .GroupBy(static candidate => candidate.Repository.Map(repository => (repository, candidate.Row.Held)).ToEither(candidate.Row.Line))
                .Select(family => MovedAsync(feeds, toSeq(family), env.Token))).ConfigureAwait(false);
            return toSeq(moved.SelectMany(static family => family).OrderBy(static decided => decided.Row.Line));
        });

    public static string Rewritten(string line, Row row, NuGetVersion moved) {
        int start = line.IndexOf(row.Text, line.IndexOf($"{row.Attribute}=", StringComparison.Ordinal), StringComparison.Ordinal);
        int end = start + row.Text.Length;
        string found = moved.ToNormalizedString();
        int close = line.IndexOf("/>", end, StringComparison.Ordinal) + 2;
        int gap = line.Length - close - line[close..].TrimStart().Length;
        return string.Concat(line[..start], found, line[end..close], gap > 0 && line.Length > close + gap ? new string(' ', int.Max(1, gap - found.Length + end - start)) : line[close..(close + gap)], line[(close + gap)..]);
    }

    private static async Task<Candidate> OfferedAsync(Feeds feeds, ILookup<PackageIdentity, Consumer> consumed, Row row, CancellationToken token) {
        PackageIdentity pinned = new(row.Id, row.Held);
        Seq<IPackageSearchMetadata> releases = await feeds.ReleasesAsync(row.Id, token).ConfigureAwait(false);
        Option<Installed> installed = await feeds.InstalledAsync(pinned, token).ConfigureAwait(false);
        Seq<Consumer> consumers = toSeq(consumed[pinned]);
        Seq<NuGetVersion> limits = consumers.Choose(consumer => consumer.Host.Bind(host => host.Libraries.Find(row.Id)));
        return new Candidate(
            row,
            consumers,
            installed.Map(static held => held.Repository).Filter(static repository => repository.Url.Length + repository.Commit.Length > 0),
            from held in releases.Find(release => release.Identity.Version == row.Held).ToEither($"no source lists {row.Held}")
            from _ in guard(row.Range is { HasUpperBound: false, IsMinInclusive: true, IsFloating: false }, $"range {row.Text} holds")
            from newer in Some(releases.Filter(release => release.IsListed && release.Published >= held.Published && Rank(release).CompareTo(Rank(held)) > 0)).Filter(static found => !found.IsEmpty).ToEither("newest")
            from shipped in Some(newer.Filter(release => limits.ForAll(limit => release.Identity.Version <= limit))).Filter(static found => !found.IsEmpty).ToEither(() => $"host ships {Enumerable.Min(limits)}")
            select toSeq(shipped.OrderByDescending(Rank)));
    }

    private static async Task<Seq<(Row Row, Either<string, NuGetVersion> Decision)>> MovedAsync(Feeds feeds, Seq<Candidate> family, CancellationToken token) {
        Option<(Row Row, string Reason)> held = family.Choose(static member => member.Newer.Match<Option<(Row Row, string Reason)>>(Left: reason => Some((member.Row, reason)), Right: static _ => None)).Head;
        Seq<Seq<NuGetVersion>> offers = family.Map(static member => member.Newer.IfLeft(Seq<IPackageSearchMetadata>()).Map(static release => release.Identity.Version));
        Option<NuGetVersion> found = await offers.Head.IfNone(Seq<NuGetVersion>())
            .Filter(version => offers.ForAll(offer => offer.Exists(offered => offered == version)))
            .ToAsyncEnumerable()
            .Where((version, next) => family.ToAsyncEnumerable()
                .AllAsync(async (member, inner) => member.Consumers.IsEmpty
                    || (await feeds.InstalledAsync(new PackageIdentity(member.Row.Id, version), inner).ConfigureAwait(false)).Exists(installed => member.Consumers.ForAll(consumer => Fits(installed, consumer))), next))
            .Select(static version => Some(version))
            .FirstOrDefaultAsync(token)
            .ConfigureAwait(false);
        string frameworks = string.Join(' ', family.Bind(static member => member.Consumers).Map(static consumer => consumer.Framework.GetShortFolderName()).Distinct());
        return family.Map(member => (member.Row, held.Match(
            Some: first => member.Newer.Bind<NuGetVersion>(_ => $"moves with {first.Row.Id}, {first.Reason}"),
            None: () => found.ToEither($"no shared release supports {frameworks}"))));
    }

    private static bool Fits(Installed installed, Consumer consumer) {
        Seq<NuGetFramework> named = installed.Assemblies.Map(static group => group.TargetFramework);
        bool Family(NuGetFramework framework) => framework.Framework.Equals(consumer.Framework.Framework, StringComparison.OrdinalIgnoreCase);
        return named.IsEmpty || Optional(new FrameworkReducer().GetNearest(consumer.Framework, named)).Exists(nearest =>
            (Family(nearest) || !named.Exists(Family))
            && consumer.Host.ForAll(host => installed.Assemblies
                .Filter(group => group.TargetFramework.Equals(nearest))
                .Bind(static group => toSeq(group.Items))
                .Filter(static item => Path.GetExtension(item).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                .ForAll(item => host.Loaded(Path.GetFileName(item)).ForAll(limit => AssemblyName.GetAssemblyName(Path.Combine(installed.Folder, item)).Version <= limit))));
    }

    private static (Version Numeric, bool Stable, DateTimeOffset? Published) Rank(IPackageSearchMetadata release) =>
        (release.Identity.Version.Version, !release.Identity.Version.IsPrerelease, release.Published);
}
