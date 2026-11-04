using System.Collections.Generic;
using System.Text.Json.Nodes;
using Grasshopper2.Components;
using Grasshopper2.Diagnostics;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Rhino;

namespace Rasm.Harnesses.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Canvases : IDisposable {
    // --- [LIFECYCLE]
    private readonly object editor;
    private readonly Action restore;

    public Canvases() {
        if (Grasshopper2.UI.Editor.Instance is null) {
            _ = PluginServer.LoadAllScopedPlugins();
            _ = Grasshopper2.UI.Editor.ShowEditor(createVisible: false);
        }
        editor = Grasshopper2.UI.Editor.Instance!;
        Density density = Logger.Density;
        restore = () => Logger.Density = density;
        Logger.Density = Density.Verbose;
    }

    public void Dispose() => restore();

    // --- [HOST]
    public object Editor => ((Editor)editor).Canvas;

    public static Grasshopper State {
        get {
            Plugins state = PluginServer.State;
            return new Grasshopper(typeof(Editor).Assembly.GetName().Version!.ToString(),
                [.. state.LoadedPlugins.Select(static plugin => $"{plugin.Name} {plugin.Version}")],
                [.. state.Failures.Select(static failure => $"{failure.location}: {failure.info.Kind} {failure.info.Reason}")],
                PluginServer.IsFullyLoaded(out string incomplete) ? null : incomplete);
        }
    }

    public static int Logged() => Logger.Count;

    public static ImmutableArray<Diagnosed> Diagnostics(int since) =>
        [.. Logger.Diagnostics().Skip(since).Select(static entry => new Diagnosed("Grasshopper2", entry.Kind.ToString(), entry.Message))];

    // --- [DOCUMENTS]
    public Opened Open(Option<string> source) {
        Editor shown = (Editor)editor;
        object prior = shown.Canvas.Document;
        Document document = source.Match(
            Some: static path => {
                DocumentIO file = new(trackFiles: false, reportErrors: false);
                return file.Open(path) ? file.Document : throw new FileLoadException("definition did not open", path);
            },
            None: Document.NewActiveDocument);
        _ = shown.Documents.Push(document);
        return new Opened(this, document, prior);
    }

    public void Release(object document, object prior) {
        Editor shown = (Editor)editor;
        _ = shown.Documents.Pop((Document)document, accept: false);
        _ = shown.Documents.Push((Document)prior);
    }

    // --- [SOLUTIONS]
    public static JsonObject Solve(object canvas) {
        Document document = (Document)canvas;
        Task<Solution> running = document.Solution.Start();
        while (!running.IsCompleted) RhinoApp.Wait();
        return new JsonObject {
            ["phase"] = running.Status.ToString(),
            ["error"] = running.Exception?.ToString(),
            ["objects"] = new JsonArray([.. document.Objects.Forwards.Select(Read)]),
        };
    }

    private static JsonObject Read(IDocumentObject member) => new() {
        ["id"] = member.InstanceId.ToString(),
        ["name"] = member.DisplayName,
        ["user"] = member.UserName,
        ["phase"] = member.State.Phase.ToString(),
        ["activity"] = member.Activity.ToString(),
        ["display"] = member.Display.ToString(),
        ["messages"] = new JsonArray([.. member.IterateOverAllMessages().Select(static message => (JsonNode)new JsonObject { ["level"] = message.Level.ToString(), ["text"] = message.Text })]),
        ["persistent"] = member is IParameter own ? Tree(own.PersistentDataWeak) : null,
        ["outputs"] = new JsonArray([.. Outputs(member).Select(static parameter => (JsonNode)new JsonObject { ["name"] = parameter.DisplayName, ["tree"] = Tree(parameter.State.Data?.Tree()) })]),
    };

    private static IEnumerable<IParameter> Outputs(IDocumentObject member) => member switch {
        Component component => component.Parameters.Outputs,
        IParameter parameter => [parameter],
        _ => [],
    };

    private static JsonArray? Tree(Grasshopper2.Data.ITree? tree) =>
        tree is null ? null : new JsonArray([
            .. tree.Paths.Zip(tree.AllTwigs, static (path, twig) => (JsonNode)new JsonObject {
                ["path"] = path.ToString(),
                ["items"] = new JsonArray([.. twig.Items.Select(static item => Engine.Encode(item).IfFail(error => (JsonNode?)JsonValue.Create($"{item?.GetType().FullName} {Engine.Message(error)}")))]),
            }),
        ]);
}
