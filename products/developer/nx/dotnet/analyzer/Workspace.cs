using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.Execution;
using NuGet.Configuration;
using NuGet.ProjectModel;
using System.Text.RegularExpressions;

namespace MsbuildAnalyzer;

// --- [MODELS] ---------------------------------------------------------------------------

internal sealed record Reference(string Root, string Declared);

internal sealed record Package(string Id, string Version, string Hash, bool Direct, ImmutableArray<string> Dependencies);

internal sealed record Tracking(string Shipped, string Unshipped);

internal sealed record Mode(string Configuration, ImmutableArray<string> Outputs, ImmutableArray<string> Published, ImmutableArray<string> Reports, ImmutableArray<string> Documents, string Packed);

internal sealed record Node(
    string File,
    string Name,
    string Language,
    string Platform,
    bool Test,
    bool Packable,
    bool Publishable,
    bool Pushed,
    bool Versioned,
    Mode Evaluated,
    ImmutableArray<Mode> Declared,
    ImmutableArray<Reference> References,
    ImmutableArray<string> Inputs,
    ImmutableArray<string> Bases,
    ImmutableArray<Package> Packages,
    ImmutableArray<string> Referenced,
    ImmutableArray<string> Locks,
    ImmutableArray<string> Restored,
    ImmutableArray<string> Results,
    ImmutableArray<string> Platforms,
    ImmutableArray<string> Implicit,
    ImmutableArray<string> Tags,
    ImmutableArray<string> Environment,
    ImmutableArray<string> External,
    Tracking? Releases);

internal sealed record Failure(string File, string Message);

internal sealed record Report(ImmutableArray<Node> Nodes, ImmutableArray<Failure> Failures, ImmutableArray<string> Evaluated);

// --- [OPERATIONS] -----------------------------------------------------------------------

internal static partial class Workspace {
    public static readonly ImmutableDictionary<string, string> Defaults = Seq("EnableDefaultEmbeddedResourceItems", "EnableDefaultNoneItems", "EnableDefaultContentItems")
        .Fold(ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase), static (held, name) => held.SetItem(name, "false"));

    private static readonly FrozenSet<string> Projects = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, ".csproj", ".fsproj", ".vbproj");

    [GeneratedRegex("""--file-name[=:\s]\s*"?(?<name>[^\s"]+)"?""", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex FileName { get; }

    public static Report Analyze(string root, Seq<string> files) {
        EvaluationContext context = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared);
        using ProjectCollection collection = new();
        Seq<string> projects = files.Filter(static file => Projects.Contains(Path.GetExtension(file)));
        Seq<string> folders = projects.Choose(file => Optional(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(root, file))))).Distinct();
        FrozenDictionary<string, Seq<string>> listed = folders.Bind(folder => Ancestors(root, folder)).Distinct().AsEnumerable().ToFrozenDictionary(static at => at, static at => toSeq(Directory.EnumerateFiles(at)).Strict());
        using SettingsLoadingContext loading = new();
        FrozenDictionary<string, Fin<(Seq<string> Walked, bool Pushed)>> located = folders.AsEnumerable().ToFrozenDictionary(
            static folder => folder,
            folder => Try.lift(() => Settings.LoadDefaultSettings(folder, configFileName: null, new XPlatMachineWideSetting(), loading)).Run().Map(settings => (Walk(root, folder, listed, settings), !string.IsNullOrEmpty(SettingsUtility.GetDefaultPushSource(settings)))));
        Seq<(string File, Fin<(Node Node, Seq<string> Evaluated)> Result)> read = projects.Map(file => (file, Try.lift(() => Evaluate(root, file, located, context, collection)).Run()));
        return new Report(
            [.. read.Choose(static held => held.Result.ToOption().Map(static evaluated => evaluated.Node))],
            [.. read.Choose(static held => held.Result.Match(Succ: static _ => None, Fail: error => Some(new Failure(held.File, error.Message))))],
            [.. read.Bind(static held => held.Result.Match(Succ: static evaluated => evaluated.Evaluated, Fail: static _ => Seq<string>())).Distinct().Order(StringComparer.Ordinal)]);
    }

    private static Fin<(Node Node, Seq<string> Evaluated)> Evaluate(string root, string file, FrozenDictionary<string, Fin<(Seq<string> Walked, bool Pushed)>> located, EvaluationContext context, ProjectCollection collection) {
        string path = Path.GetFullPath(Path.Combine(root, file));
        if (Path.GetDirectoryName(path) is not { } folder) return Error.New($"{path} names no folder");
        ProjectInstance outer = Load(path, Defaults, context, collection);
        Seq<string> frameworks = Listed(outer, "TargetFrameworks");
        Seq<ProjectInstance> Inner(ImmutableDictionary<string, string> properties) =>
            frameworks.IsEmpty ? Seq(Load(path, properties, context, collection)) : frameworks.Map(framework => Load(path, properties.SetItem("TargetFramework", framework), context, collection));
        Seq<ProjectInstance> evaluated = frameworks.IsEmpty ? Seq(outer) : Inner(Defaults);
        Seq<(string Configuration, Seq<ProjectInstance> Instances)> declared = Listed(outer, "Configurations")
            .Filter(configuration => !configuration.Equals(outer.GetPropertyValue("Configuration"), StringComparison.OrdinalIgnoreCase))
            .Map(configuration => (configuration, Inner(Defaults.SetItem("Configuration", configuration))));
        Seq<ProjectInstance> instances = (Seq(outer) + evaluated + declared.Bind(static mode => mode.Instances)).Distinct();
        string Placed(string output) => Relative(root, Path.GetFullPath(Path.Combine(folder, output)));
        Mode Described(string configuration, Seq<ProjectInstance> held) => new(
            configuration,
            [.. held.Bind(static instance => Seq(instance.GetPropertyValue("OutputPath"), instance.GetPropertyValue("IntermediateOutputPath"))).Map(Placed).Distinct().Order(StringComparer.Ordinal)],
            [.. held.Filter(static instance => Flag(instance, "IsPublishable")).Map(static instance => instance.GetPropertyValue("PublishDir")).Map(Placed).Distinct().Order(StringComparer.Ordinal)],
            [.. held.Choose(static instance => toSeq(instance.GetPropertyValue("ErrorLog").Split([';', ','], StringSplitOptions.RemoveEmptyEntries)).Head).Map(Placed).Distinct().Order(StringComparer.Ordinal)],
            [.. held
                .Filter(static instance => instance.GetPropertyValue("OpenApiDocumentsDirectory").Length > 0 && !instance.GetPropertyValue("OpenApiGenerateDocumentsOnBuild").Equals("false", StringComparison.OrdinalIgnoreCase))
                .Map(static instance => (
                    Instance: instance,
                    Directory: instance.ExpandString("$([MSBuild]::NormalizeDirectory($(MSBuildProjectDirectory), $(OpenApiDocumentsDirectory)))"),
                    Stem: FileName.Match(instance.GetPropertyValue("OpenApiGenerateDocumentsOptions")) is { Success: true } named ? named.Groups["name"].Value : instance.GetPropertyValue("MSBuildProjectName")))
                .Filter(documents => !Under(documents.Directory, documents.Instance.ExpandString("$([MSBuild]::NormalizeDirectory($(MSBuildProjectDirectory), $(OutputPath)))")) && !instances.Exists(instance => Under(documents.Directory, Extensions(instance))))
                .Bind(static documents => Seq(Path.Combine(documents.Directory, $"{documents.Stem}.json"), Path.Combine(documents.Directory, $"{documents.Stem}_*.json")))
                .Map(Placed)
                .Filter(static documents => !documents.StartsWith("..", StringComparison.Ordinal))
                .Distinct()
                .Order(StringComparer.Ordinal)],
            Placed(held.Head.IfNone(outer).GetPropertyValue("PackageOutputPath")));
        string Extensions(ProjectInstance instance) => Path.GetFullPath(Path.Combine(folder, instance.GetPropertyValue("MSBuildProjectExtensionsPath")));
        Seq<string> imports =
            from instance in instances
            from import in toSeq(instance.ImportPaths)
            where !Under(import, Extensions(instance))
            select import;
        Seq<string> bases = Seq("BaseOutputPath", "BaseIntermediateOutputPath").Map(name => outer.ExpandString($"$([MSBuild]::NormalizeDirectory($(MSBuildProjectDirectory), $({name})))"));
        Seq<string> inside = imports.Map(import => Relative(root, import)).Filter(static relative => !relative.StartsWith("..", StringComparison.Ordinal));
        Seq<string> locks = instances
            .Map(static instance => Path.GetFullPath(Path.Combine(instance.Directory, instance.GetPropertyValue("NuGetLockFilePath") is { Length: > 0 } named ? named : PackagesLockFileFormat.LockFileName)))
            .Distinct()
            .Filter(File.Exists);
        Seq<string> hosted = instances.Bind(static instance => Seq(instance.GetPropertyValue("NuGetPackageRoot"), instance.GetPropertyValue("NetCoreRoot"))).Filter(static at => at.Length > 0).Distinct();
        return located[folder].Map(held => (
            new Node(
                file.Replace('\\', '/'),
                outer.GetPropertyValue(Flag(outer, "IsPackable") ? "PackageId" : "MSBuildProjectName"),
                evaluated.Head.IfNone(outer).GetPropertyValue("Language"),
                outer.GetPropertyValue("Platform"),
                instances.Exists(static instance =>
                    Flag(instance, "IsTestProject") || Flag(instance, "IsTestingPlatformApplication") || (instance.GetPropertyValue("IsTestProject").Length is 0
                        && toSeq(instance.GetItems("PackageReference")).Exists(static item => item.EvaluatedInclude.Equals("Microsoft.NET.Test.Sdk", StringComparison.OrdinalIgnoreCase) || item.EvaluatedInclude.StartsWith("Microsoft.Testing.", StringComparison.OrdinalIgnoreCase)))),
                Flag(outer, "IsPackable"),
                Flag(outer, "IsPublishable"),
                held.Pushed,
                ProjectRootElement.Open(path, collection).Properties.Any(static property => property.Name.Equals("Version", StringComparison.Ordinal)),
                Described(outer.GetPropertyValue("Configuration"), evaluated),
                [.. declared.Map(mode => Described(mode.Configuration, mode.Instances))],
                [.. instances
                    .Bind(static instance => toSeq(instance.GetItems("ProjectReference")))
                    .Choose(reference => Optional(Path.GetDirectoryName(reference.GetMetadataValue("FullPath"))).Map(referenced => new Reference(Relative(root, referenced), Relative(root, reference.GetMetadataValue("DefiningProjectFullPath")))))
                    .Distinct()],
                [.. imports
                    .Append(from instance in instances from type in Seq("Compile", "AdditionalFiles", "EmbeddedResource", "Content", "GlobalAnalyzerConfigFiles") from item in toSeq(instance.GetItems(type)) select item.GetMetadataValue("FullPath"))
                    .Append(from instance in instances from target in toSeq(instance.Targets.Values) from input in toSeq(instance.ExpandString(target.Inputs).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) select Path.GetFullPath(Path.Combine(instance.Directory, input)))
                    .Filter(input => File.Exists(input) && !Under(input, folder) && !bases.Exists(at => Under(input, at)))
                    .Map(input => Relative(root, input))
                    .Filter(static relative => !relative.StartsWith("..", StringComparison.Ordinal))
                    .Append(held.Walked)
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                [.. bases.Map(at => Relative(root, at)).Distinct()],
                [.. locks
                    .Bind(static locked => toSeq(PackagesLockFileFormat.Read(locked).Targets))
                    .Bind(static target => toSeq(target.Dependencies))
                    .Filter(static dependency => dependency.Type is not PackageDependencyType.Project)
                    .AsEnumerable()
                    .DistinctBy(static dependency => (dependency.Id, dependency.ResolvedVersion))
                    .Select(static dependency => new Package(dependency.Id, dependency.ResolvedVersion.ToNormalizedString(), dependency.ContentHash, dependency.Type is PackageDependencyType.Direct, [.. dependency.Dependencies.Select(static held => held.Id)]))],
                [.. evaluated.Bind(static instance => toSeq(instance.GetItems("PackageReference"))).Map(static item => item.EvaluatedInclude).Distinct().Order(StringComparer.Ordinal)],
                [.. locks.Map(locked => Relative(root, locked))],
                [.. instances
                    .Bind(instance => toSeq(instance.ImportPaths).Filter(import => Under(import, Extensions(instance))).Add(Path.GetFullPath(Path.Combine(instance.Directory, instance.GetPropertyValue("ProjectAssetsFile")))))
                    .Map(restored => Relative(root, restored))
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                [.. (Flag(outer, "UseArtifactsOutput")
                    ? Seq(Path.Combine(outer.GetPropertyValue("ArtifactsPath"), "test", outer.GetPropertyValue("ArtifactsProjectName")))
                    : instances.Exists(static instance => Flag(instance, "IsTestingPlatformApplication"))
                        ? instances.Map(static instance => instance.GetPropertyValue("TargetDir")).Filter(static target => target.Length > 0).Map(static target => Path.Combine(target, "TestResults"))
                        : Seq(Path.Combine(folder, "TestResults")))
                    .Map(results => Relative(root, results))
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                [.. evaluated.Map(static instance => instance.GetPropertyValue("TargetPlatformIdentifier")).Distinct().Order(StringComparer.Ordinal)],
                [.. toSeq(outer.GetItems("NxImplicitDependency")).Map(static item => item.EvaluatedInclude).Distinct().Order(StringComparer.Ordinal)],
                [.. instances.Bind(static instance => toSeq(instance.GetItems("NxTag"))).Map(static item => item.EvaluatedInclude).Distinct().Order(StringComparer.Ordinal)],
                [.. instances.Bind(static instance => toSeq(instance.GetItems("NxEnvironmentInput"))).Map(static item => item.EvaluatedInclude).Distinct().Order(StringComparer.Ordinal)],
                [.. (from instance in instances from type in Seq("UpToDateCheckInput", "Reference") from item in toSeq(instance.GetItems(type)) select item.GetMetadataValue("HintPath") is { Length: > 0 } hinted ? hinted : item.GetMetadataValue("FullPath"))
                    .Filter(input => File.Exists(input) && !Under(input, root) && !hosted.Exists(at => Under(input, at)))
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                Tracked(root, instances)),
            Seq(file.Replace('\\', '/')) + inside + held.Walked));
    }

    public static ProjectInstance Load(string path, ImmutableDictionary<string, string> properties, EvaluationContext context, ProjectCollection collection) =>
        ProjectInstance.FromFile(path, new ProjectOptions { GlobalProperties = properties, ProjectCollection = collection, EvaluationContext = context, LoadSettings = ProjectLoadSettings.DoNotEvaluateElementsWithFalseCondition });

    private static Tracking? Tracked(string root, Seq<ProjectInstance> instances) {
        Seq<string> additional = instances.Bind(static instance => toSeq(instance.GetItems("AdditionalFiles"))).Map(static item => item.GetMetadataValue("FullPath")).Distinct();
        Option<string> Named(string name) => additional.Find(file => Path.GetFileName(file).Equals(name, StringComparison.OrdinalIgnoreCase)).Map(file => Relative(root, file));
        return (from shipped in Named("AnalyzerReleases.Shipped.md") from unshipped in Named("AnalyzerReleases.Unshipped.md") select new Tracking(shipped, unshipped)).ValueUnsafe();
    }

    private static Seq<string> Walk(string root, string folder, FrozenDictionary<string, Seq<string>> listed, ISettings settings) {
        Seq<string> entries = Ancestors(root, folder).Bind(at => listed[at]);
        Seq<string> Named(string name) => entries.Filter(entry => Path.GetFileName(entry).Equals(name, StringComparison.OrdinalIgnoreCase));
        return Seq("global.json", "Directory.Build.rsp").Choose(name => Named(name).Head)
            .Append(Seq(".editorconfig", ".globalconfig").Bind(Named))
            .Append(toSeq(settings.GetConfigFilePaths()).Choose(config => entries.Find(entry => entry.Equals(config, StringComparison.OrdinalIgnoreCase))))
            .Map(found => Relative(root, found));
    }

    private static Seq<string> Ancestors(string root, string folder) =>
        Path.GetRelativePath(root, folder) is "." ? Seq(folder) : Seq(folder) + Optional(Path.GetDirectoryName(folder)).ToSeq().Bind(parent => Ancestors(root, parent));

    private static Seq<string> Listed(ProjectInstance instance, string property) =>
        toSeq(instance.GetPropertyValue(property).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static bool Flag(ProjectInstance instance, string property) =>
        bool.TryParse(instance.GetPropertyValue(property), out bool value) && value;

    private static bool Under(string path, string folder) =>
        Path.GetFullPath(path).StartsWith(Path.TrimEndingDirectorySeparator(folder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/').TrimEnd('/');
}
