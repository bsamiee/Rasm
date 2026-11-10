using System.Text.Json;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Rasm.Hosting;

namespace MsbuildAnalyzer;

// --- [COMPOSITION] ----------------------------------------------------------------------

internal static class Program {
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };

    public static int Main(string[] args) =>
        ProcessEntry.Run(_ => IO.lift(() => args switch {
            ["version", var version] => Registered(Environment.CurrentDirectory, () => Versioned(version)),
            ["upgrade", var central] => Registered(Path.GetDirectoryName(Path.GetFullPath(central)) ?? Environment.CurrentDirectory, () => Upgraded(Path.GetFullPath(central))),
            [var root] => Registered(Path.GetFullPath(root), () => Write(Path.GetFullPath(root))),
            _ => Error.New("Usage: MsbuildAnalyzer <workspace-root>, with workspace-relative files on stdin, one per line; MsbuildAnalyzer version <version>, with a project file on stdin; or MsbuildAnalyzer upgrade <central-package-file>, with the project files reading it on stdin, one per line"),
        }));

    private static Fin<int> Registered(string root, Func<int> run) =>
        toSeq(MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = root }))
            .Head
            .ToFin(Error.New($"No .NET SDK resolves from {root}"))
            .Map(sdk => {
                MSBuildLocator.RegisterInstance(sdk);
                return run();
            });

    private static int Write(string root) {
        Console.Out.Write(JsonSerializer.Serialize(Workspace.Analyze(root, Listed()), Json));
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
        Feeds feeds = new(
            toSeq(new PackageSourceProvider(settings).LoadPackageSources()).Filter(static source => source.IsEnabled).Map(static source => Repository.Factory.GetCoreV3(source)),
            PackageSourceMapping.GetPackageSourceMapping(settings),
            new PackageDownloadContext(cache),
            SettingsUtility.GetGlobalPackagesFolder(settings));
        Seq<(Row Row, Either<string, NuGetVersion> Decision)> decided = Packages.Decided(feeds, Packages.Rows(ProjectRootElement.Open(central, collection, preserveFormatting: true)), Packages.Consumed(Listed())).Run();
        Map<int, (Row Row, NuGetVersion Found)> moved = toMap(decided.Choose(static held => held.Decision.ToOption().Map(found => (held.Row.Line, (held.Row, found)))));
        File.WriteAllText(central, string.Join('\n', File.ReadAllText(central).Split('\n').Select((line, index) => moved.Find(index).Match(row => Packages.Rewritten(line, row.Row, row.Found), () => line))));
        Console.Out.Write(string.Concat(decided.Map(static held => held.Decision.Match(Left: reason => $"{held.Row.Id} {held.Row.Text} holds: {reason}\n", Right: found => $"{held.Row.Id} {held.Row.Text} -> {found}\n"))));
        return 0;
    }

    private static Seq<string> Listed() =>
        toSeq(Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
