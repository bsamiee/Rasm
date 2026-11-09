using System.Text.Json;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace MsbuildAnalyzer;

// --- [COMPOSITION] ----------------------------------------------------------------------

internal static class Program {
    private static readonly FrozenSet<string> Pinned = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, "PackageVersion", "GlobalPackageReference");
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };

    public static int Main(string[] args) =>
        args switch {
            ["version", var version] => Registered(Environment.CurrentDirectory, () => Versioned(version)),
            ["upgrade", var central] => Registered(Path.GetDirectoryName(Path.GetFullPath(central)) ?? Environment.CurrentDirectory, () => Upgraded(Path.GetFullPath(central))),
            [var root] => Registered(Path.GetFullPath(root), () => Write(Path.GetFullPath(root))),
            _ => Refuse("Usage: MsbuildAnalyzer <workspace-root>, with workspace-relative files on stdin, one per line; MsbuildAnalyzer version <version>, with a project file on stdin; or MsbuildAnalyzer upgrade <central-package-file>"),
        };

    private static int Registered(string root, Func<int> run) =>
        toSeq(MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = root }))
            .Head
            .Match(
                Some: sdk => {
                    MSBuildLocator.RegisterInstance(sdk);
                    return run();
                },
                None: () => Refuse($"No .NET SDK resolves from {root}"));

    private static int Write(string root) {
        Console.Out.Write(JsonSerializer.Serialize(Workspace.Analyze(root, toSeq(Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))), Json));
        return 0;
    }

    private static int Versioned(string version) {
        using ProjectCollection collection = new();
        using XmlReader reader = XmlReader.Create(Console.In);
        ProjectRootElement project = ProjectRootElement.Create(reader, collection, preserveFormatting: true);
        _ = project.AddProperty("Version", version);
        project.Save(Console.Out);
        return 0;
    }

    private static int Upgraded(string central) {
        using ProjectCollection collection = new();
        using SourceCacheContext cache = new();
        ISettings settings = Settings.LoadDefaultSettings(Path.GetDirectoryName(central));
        PackageSourceMapping mapping = PackageSourceMapping.GetPackageSourceMapping(settings);
        Seq<SourceRepository> repositories = toSeq(new PackageSourceProvider(settings).LoadPackageSources()).Filter(static source => source.IsEnabled).Map(static source => Repository.Factory.GetCoreV3(source));
        IO<Option<NuGetVersion>> Newest(string id) =>
            IO.liftAsync(async env => Optional(Enumerable.Max(toSeq(await Task.WhenAll(repositories
                .Filter(repository => !mapping.IsEnabled || mapping.GetConfiguredPackageSources(id).Contains(repository.PackageSource.Name, StringComparer.OrdinalIgnoreCase))
                .Map(async repository => await repository.GetResourceAsync<PackageMetadataResource>(env.Token).ConfigureAwait(false) is { } resource
                    ? toSeq(await resource.GetMetadataAsync(id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token).ConfigureAwait(false)).Map(static found => found.Identity.Version)
                    : Seq<NuGetVersion>())).ConfigureAwait(false)).Bind(static found => found))));
        Map<int, (string Attribute, string Id, string Text, NuGetVersion Current, NuGetVersion Found)> moved = toMap(toSeq(ProjectRootElement.Open(central, collection, preserveFormatting: true).Items)
            .Filter(static item => Pinned.Contains(item.ItemType) && item.Include.Length > 0)
            .Choose(static item => toSeq(item.Metadata).Find(static metadata => metadata.ExpressedAsAttribute && metadata.Name is "Version").Bind(metadata => NuGetVersion.TryParse(metadata.Value, out NuGetVersion? current) ? Some((Line: item.Location.Line - 1, Attribute: metadata.Name, Id: item.Include, Text: metadata.Value, Current: current)) : None))
            .Traverse(row => Newest(row.Id).Map(newest => newest.Filter(found => found > row.Current).Map(found => (row.Line, (row.Attribute, row.Id, row.Text, row.Current, Found: found)))))
            .As()
            .Run()
            .Somes());
        string Rewritten(string line, (string Attribute, string Id, string Text, NuGetVersion Current, NuGetVersion Found) row) {
            int start = line.IndexOf(row.Text, line.IndexOf($"{row.Attribute}=", StringComparison.Ordinal), StringComparison.Ordinal);
            int end = start + row.Text.Length;
            string found = row.Found.ToNormalizedString();
            int close = line.IndexOf("/>", end, StringComparison.Ordinal) + 2;
            int gap = line.Length - close - line[close..].TrimStart().Length;
            return string.Concat(line[..start], found, line[end..close], gap > 0 && line.Length > close + gap ? new string(' ', int.Max(1, gap - found.Length + end - start)) : line[close..(close + gap)], line[(close + gap)..]);
        }
        File.WriteAllText(central, string.Join('\n', File.ReadAllText(central).Split('\n').Select((line, index) => moved.Find(index).Match(row => Rewritten(line, row), () => line))));
        Console.Out.Write(string.Concat(moved.Values.Select(static row => $"{row.Id} {row.Current} -> {row.Found}\n")));
        return 0;
    }

    private static int Refuse(string message) {
        Console.Error.WriteLine(message);
        return 2;
    }
}
