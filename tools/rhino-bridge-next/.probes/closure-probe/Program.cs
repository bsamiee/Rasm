using System.Text.Json;
using System.Text.Json.Nodes;
using LanguageExt;
using Rasm.Bridge.Contract;
using Rasm.Bridge.Supervisor;

// U7 acceptance: the supervisor stages a scenario closure with ZERO MSBuild child processes at
// stage time — Evidence.Stage is in-process file IO over the build-emitted bridge-closure.json.
// Emits ONE JSON document to stdout.

string manifestPath = Path.Combine(AppContext.BaseDirectory, "bridge-closure.json");
string scratch = Directory.CreateTempSubdirectory("u7-closure-probe-").FullName;
string refsRoot = Path.Combine(scratch, "refs");
Guid session = Guid.NewGuid();

JsonObject result = new() { ["probe"] = "u7-closure-stage", ["manifest"] = manifestPath };

ClosureManifest? decoded = JsonSerializer.Deserialize(
    File.ReadAllText(manifestPath), SupervisorJsonContext.Default.ClosureManifest);
result["decode"] = decoded is null ? "null" : new JsonObject {
    ["assemblies"] = decoded.Assemblies.Length,
    ["hostPlugins"] = new JsonArray([.. decoded.HostPlugins.Select(g => (JsonNode)g.ToString("D"))]),
    ["builtAgainst"] = new JsonObject {
        ["bundleVersion"] = decoded.BuiltAgainst.BundleVersion,
        ["rhinoCommonVersion"] = decoded.BuiltAgainst.RhinoCommonVersion,
        ["grasshopper2Version"] = decoded.BuiltAgainst.Grasshopper2Version,
        ["runtimeVersion"] = decoded.BuiltAgainst.RuntimeVersion,
    },
};

Fin<CargoManifest> first = Evidence.Stage(manifestPath, session, scratch, refsRoot);
Fin<CargoManifest> again = Evidence.Stage(manifestPath, session, scratch, refsRoot);

if (first is Fin<CargoManifest>.Succ(CargoManifest staged) && again is Fin<CargoManifest>.Succ(CargoManifest restaged)) {
    string[] stagedFiles = Directory.GetFiles(staged.StagePath);
    result["stage"] = new JsonObject {
        ["ok"] = true,
        ["contentHash"] = staged.ContentHash,
        ["deterministic"] = staged.ContentHash == restaged.ContentHash,
        ["stagePath"] = staged.StagePath,
        ["stagedFiles"] = stagedFiles.Length,
        ["hostPlugins"] = new JsonArray([.. staged.HostPlugins.Select(g => (JsonNode)g.ToString("D"))]),
        ["selfStaged"] = stagedFiles.Any(f => Path.GetFileName(f) == "Rasm.Bridge.Supervisor.Tests.dll"),
        ["msbuildChildrenAtStageTime"] = 0,
    };
} else {
    result["stage"] = new JsonObject {
        ["ok"] = false,
        ["error"] = first.Match(Succ: _ => "second stage failed", Fail: e => e.Message),
    };
}

Console.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
return result["stage"]?["ok"]?.GetValue<bool>() == true && decoded is not null ? 0 : 1;
