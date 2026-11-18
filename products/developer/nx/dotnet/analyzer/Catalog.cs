using LanguageExt.ClassInstances;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Extensions.DependencyModel;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.ProjectModel;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace MsbuildAnalyzer;

// --- [MODELS] ---------------------------------------------------------------------------

internal sealed record Consumer(NuGetFramework Framework, HashMap<EqStringOrdinalIgnoreCase, string, ProjectElement> Pinned, HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion> Supplied);

internal sealed record Pin(ProjectElement Element, string Written, VersionRange Range, Seq<string> Ids, Seq<Consumer> Consumers) {
    public bool Exact => Range is { IsMinInclusive: true, IsMaxInclusive: true, MinVersion: { } min, MaxVersion: { } max } && min == max;

    public string Spelled(NuGetVersion version) => Exact ? $"[{version.ToNormalizedString()}]" : version.ToNormalizedString();

    public (string Old, string New) Tokens(string spelled) => Element is ProjectPropertyElement ? ($">{Written}<", $">{spelled}<") : ($"\"{Written}\"", $"\"{spelled}\"");
}

// --- [OPERATIONS] -----------------------------------------------------------------------

internal static class Catalog {
    private static readonly Seq<string> Pinning = Seq("PackageVersion", "GlobalPackageReference");

    public static IO<int> Upgrade(string root, string solution, bool dry) =>
        from pins in IO.lift(() => Pinned(solution))
        from released in Released(root, pins.Bind(static pin => pin.Ids).Distinct())
        let ranked = pins.Map(pin => (Pin: pin, Candidates: Ranked(pin, released))).Strict()
        let settled = Settled(ranked, released, toHashMap(ranked.Choose(static held => held.Candidates.Head.Map(newest => (held.Pin.Element, newest)))))
        from moved in IO.lift(() => Written(ranked.Choose(held => settled.Find(held.Pin.Element).Filter(version => held.Candidates.Last.Exists(current => version > current)).Map(version => (held.Pin, Spelled: held.Pin.Spelled(version)))), dry))
        select moved;

    private static Seq<Pin> Pinned(string solution) {
        EvaluationContext context = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared);
        using ProjectCollection collection = new();
        Project Load(string path, ImmutableDictionary<string, string> properties) =>
            Project.FromFile(path, new ProjectOptions { GlobalProperties = properties, ProjectCollection = collection, EvaluationContext = context });
        Seq<(ProjectElement Element, string Written, string Id, Option<Consumer> Consumer)> read =
            from project in toSeq(SolutionFile.Parse(solution).ProjectsInOrder).Filter(static project => project.ProjectType is SolutionProjectType.KnownToBeMSBuildFormat)
            let outer = Load(project.AbsolutePath, Workspace.Defaults)
            let locked = Locked(outer)
            from evaluated in toSeq(outer.GetPropertyValue("TargetFrameworks").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) switch {
                { IsEmpty: true } => Seq(outer),
                var frameworks => frameworks.Map(framework => Load(project.AbsolutePath, Workspace.Defaults.SetItem("TargetFramework", framework))),
            }
            let framework = NuGetFramework.Parse(evaluated.GetPropertyValue("TargetFramework"))
            let consumed = locked.Find(framework).IfNone([])
            let transitive = string.Equals(evaluated.GetPropertyValue("CentralPackageTransitivePinningEnabled"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
            let direct = evaluated.GetItems("PackageReference").Select(static item => item.EvaluatedInclude).ToFrozenSet(StringComparer.OrdinalIgnoreCase)
            let rows = Pinning.Bind(type => toSeq(evaluated.GetItems(type))).Choose(item => Optional(item.GetMetadata("Version")).Map(version =>
                version.UnevaluatedValue is ['$', '(', .. var name, ')'] && evaluated.GetProperty(name) is { Xml: { } property }
                    ? (Id: item.EvaluatedInclude, Element: (ProjectElement)property, Written: property.Value)
                    : (Id: item.EvaluatedInclude, Element: version.Xml, Written: version.Xml.Value))).Strict()
            let consumer = new Consumer(framework, rows.Filter(row => transitive || direct.Contains(row.Id)).Fold(HashMap<EqStringOrdinalIgnoreCase, string, ProjectElement>(), static (pinned, row) => pinned.AddOrUpdate(row.Id, row.Element)), string.Equals(evaluated.GetPropertyValue("IsRoslynComponent"), bool.TrueString, StringComparison.OrdinalIgnoreCase) ? Compiler(evaluated.GetPropertyValue("RoslynTargetsPath")) : [])
            from row in rows
            select (row.Element, row.Written, row.Id, Some(consumer).Filter(_ => consumed.Contains(row.Id)));
        return toSeq(read.AsEnumerable().GroupBy(static held => held.Element))
            .Choose(static group => VersionRange.TryParse(group.First().Written, out VersionRange? range)
                ? Some(new Pin(group.Key, group.First().Written, range, toSeq(group.Select(static held => held.Id).Distinct(StringComparer.OrdinalIgnoreCase)).Strict(), toSeq(group).Map(static held => held.Consumer).Somes().Distinct().Strict()))
                : None)
            .Strict();
    }

    private static HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion> Compiler(string roslyn) {
        using DependencyContextJsonReader reader = new();
        return toSeq(Directory.EnumerateFiles(Path.Combine(roslyn, "bincore"), "*.deps.json"))
            .Bind(file => {
                using FileStream stream = File.OpenRead(file);
                return toSeq(reader.Read(stream).RuntimeLibraries).Strict();
            })
            .Choose(static library => NuGetVersion.TryParse(library.Version, out NuGetVersion? version) ? Some((library.Name, Version: version)) : None)
            .Fold(HashMap<EqStringOrdinalIgnoreCase, string, NuGetVersion>(), static (supplied, library) => supplied.AddOrUpdate(library.Name, library.Version));
    }

    private static HashMap<NuGetFramework, FrozenSet<string>> Locked(Project project) =>
        Path.Combine(project.DirectoryPath, project.GetPropertyValue("NuGetLockFilePath") is { Length: > 0 } named ? named : PackagesLockFileFormat.LockFileName) switch {
            var locked when File.Exists(locked) => toHashMap(PackagesLockFileFormat.Read(locked).Targets
                .Where(static target => string.IsNullOrEmpty(target.RuntimeIdentifier))
                .Select(static target => (target.TargetFramework, target.Dependencies.Select(static dependency => dependency.Id).ToFrozenSet(StringComparer.OrdinalIgnoreCase)))),
            _ => HashMap<NuGetFramework, FrozenSet<string>>(),
        };

    private static IO<HashMap<string, HashMap<NuGetVersion, IPackageSearchMetadata>>> Released(string root, Seq<string> ids) =>
        IO.liftAsync(async env => {
            ISettings settings = Settings.LoadDefaultSettings(root);
            PackageSourceMapping mapping = PackageSourceMapping.GetPackageSourceMapping(settings);
            using SourceCacheContext cache = new() { NoCache = true };
            Seq<(string Name, PackageMetadataResource Resource)> feeds = toSeq(await Task.WhenAll(new PackageSourceProvider(settings).LoadPackageSources()
                .Where(static source => source.IsEnabled)
                .Select(async source => (source.Name, Resource: Optional(await Repository.Factory.GetCoreV3(source).GetResourceAsync<PackageMetadataResource>(env.Token).ConfigureAwait(false))))).ConfigureAwait(false))
                .Choose(static feed => feed.Resource.Map(resource => (feed.Name, resource)));
            (string Id, Seq<IPackageSearchMetadata> Found)[] found = await Task.WhenAll(ids.Map(async id => (id, toSeq((await Task.WhenAll(feeds
                .Filter(feed => !mapping.IsEnabled || mapping.GetConfiguredPackageSources(id).Contains(feed.Name, StringComparer.OrdinalIgnoreCase))
                .Map(feed => feed.Resource.GetMetadataAsync(id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token))).ConfigureAwait(false)).SelectMany(static releases => releases)).Strict()))).ConfigureAwait(false);
            return toHashMap(found.Select(static held => (held.Id, toHashMap(held.Found.AsEnumerable().DistinctBy(static release => release.Identity.Version).Select(static release => (release.Identity.Version, release))))));
        });

    private static Option<IPackageSearchMetadata> Release(HashMap<string, HashMap<NuGetVersion, IPackageSearchMetadata>> released, string id, NuGetVersion version) =>
        released.Find(id).Bind(versions => versions.Find(version));

    private static Option<Seq<PackageDependency>> Dependencies(IPackageSearchMetadata release, NuGetFramework framework) =>
        release.DependencySets.ToArray() switch {
            [] => Some(Seq<PackageDependency>()),
            var groups => Optional(NuGetFrameworkUtility.GetNearest(
                    System.Array.FindAll(groups, group => StringComparer.OrdinalIgnoreCase.Equals(group.TargetFramework.Framework, framework.Framework)) is { Length: > 0 } kin ? kin : groups,
                    framework,
                    static group => group.TargetFramework))
                .Map(static group => toSeq(group.Packages)),
        };

    private static Seq<NuGetVersion> Ranked(Pin pin, HashMap<string, HashMap<NuGetVersion, IPackageSearchMetadata>> released) {
        bool held = pin.Exact || pin.Element is { Condition.Length: > 0 } or ProjectMetadataElement { Parent: ProjectItemElement { Update.Length: > 0 } } || pin.Element.AllParents.Any(static parent => parent is { Condition.Length: > 0 });
        bool Usable(NuGetVersion current, NuGetVersion version) =>
            (from id in pin.Ids
             let prior = Release(released, id, current).Bind(static found => Optional(found.Published))
             select Release(released, id, version).Exists(found =>
                 (!version.IsPrerelease || prior.ForAll(published => found.Published >= published)) && pin.Consumers.ForAll(consumer => Dependencies(found, consumer.Framework).IsSome && consumer.Supplied.Find(id).ForAll(supplied => (version.Major, version.Minor).CompareTo((supplied.Major, supplied.Minor)) <= 0))))
            .ForAll(identity);
        return Optional(pin.Range.MinVersion).ToSeq().Bind(current => toSeq(pin.Ids.Bind(id => released.Find(id).ToSeq()).Bind(static versions => versions.Keys.ToSeq()).Distinct().OrderDescending())
            .Filter(version => version > current && (!held || version.Major == current.Major) && Usable(current, version))
            .Add(current));
    }

    private static HashMap<ProjectElement, NuGetVersion> Settled(Seq<(Pin Pin, Seq<NuGetVersion> Candidates)> ranked, HashMap<string, HashMap<NuGetVersion, IPackageSearchMetadata>> released, HashMap<ProjectElement, NuGetVersion> table) {
        bool Satisfied(Pin pin, NuGetVersion version) =>
            (from consumer in pin.Consumers
             from id in pin.Ids
             from found in Release(released, id, version).ToSeq()
             from dependencies in Dependencies(found, consumer.Framework).ToSeq()
             from dependency in dependencies
             from pinned in consumer.Pinned.Find(dependency.Id).Bind(table.Find).ToSeq()
             select dependency.VersionRange.Satisfies(pinned))
            .ForAll(identity);
        HashMap<ProjectElement, NuGetVersion> next = ranked.Fold(table, (lowered, held) =>
            (held.Candidates.Find(version => table.Find(held.Pin.Element).ForAll(at => version <= at) && Satisfied(held.Pin, version)) | held.Candidates.Last)
                .Map(version => lowered.SetItem(held.Pin.Element, version))
                .IfNone(lowered));
        return next.Equals(table) ? table : Settled(ranked, released, next);
    }

    private static int Written(Seq<(Pin Pin, string Spelled)> moves, bool dry) {
        toSeq(moves.AsEnumerable().GroupBy(static move => move.Pin.Element.ContainingProject.FullPath, StringComparer.Ordinal)).Filter(_ => !dry).Iter(static file => {
            FrozenDictionary<int, (string Old, string New)> edits = file.ToFrozenDictionary(static move => move.Pin.Element.Location.Line - 1, static move => move.Pin.Tokens(move.Spelled));
            File.WriteAllText(file.Key, string.Join('\n', File.ReadAllText(file.Key).Split('\n').Select((line, index) => edits.TryGetValue(index, out (string Old, string New) edit) ? Padded(line, line.Replace(edit.Old, edit.New, StringComparison.Ordinal)) : line)));
        });
        moves.Iter(static move => Console.Out.WriteLine($"{string.Join(", ", move.Pin.Ids)}: {move.Pin.Written} -> {move.Spelled}"));
        return 0;
    }

    private static string Padded(string before, string after) =>
        (before.IndexOf("<!--", StringComparison.Ordinal), after.IndexOf("<!--", StringComparison.Ordinal), after) switch {
            ( > 0 and var was, > 0 and var now, var line) when line[..now].TrimEnd() is var code => code + new string(' ', Math.Max(1, was - code.Length)) + line[now..],
            _ => after,
        };
}
