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
public sealed class Canvases : ICanvases {
    private Option<Density> density;

    // --- [HOST]
    public object Editor => Shown().Canvas;

    public Grasshopper Start() {
        if (Grasshopper2.UI.Editor.Instance is null) {
            _ = PluginServer.LoadAllScopedPlugins();
            _ = Grasshopper2.UI.Editor.ShowEditor(createVisible: false);
        }
        density = density || Some(Logger.Density);
        Logger.Density = Density.Verbose;
        Plugins state = PluginServer.State;
        return new Grasshopper(typeof(Editor).Assembly.GetName().Version?.ToString() ?? "",
            [.. state.LoadedPlugins.Select(static plugin => $"{plugin.Name} {plugin.Version}")],
            [.. state.Failures.Select(static failure => $"{failure.location}: {failure.info.Kind} {failure.info.Reason}")],
            PluginServer.IsFullyLoaded(out string incomplete) ? null : incomplete);
    }

    public void Stop() => _ = density.Iter(static held => Logger.Density = held);

    public int Logged() => Logger.Count;

    public ImmutableArray<Diagnosed> Diagnostics(int since) =>
        [.. Logger.Diagnostics().Skip(since).Select(static entry => new Diagnosed("Grasshopper2", entry.Kind.ToString(), entry.Message))];

    // --- [DOCUMENTS]
    public (object Document, Option<object> Prior) Open(Option<string> source) {
        Editor editor = Shown();
        Option<object> prior = Optional<object>(editor.Canvas.Document);
        Document document = source.Match(
            Some: path => editor.Documents.TryOpenDocument(path, Grasshopper2.UI.Canvas.OpenDocumentOptions.Activate) ? editor.Documents.Current : throw new FileLoadException("editor refused the definition", path),
            None: () => Pushed(editor, Document.NewActiveDocument()));
        editor.Canvas.Document = document;
        return ReferenceEquals(editor.Canvas.Document, document) ? (document, prior) : throw new InvalidOperationException("editor canvas kept another document");
    }

    public void Release(object document, Option<object> prior) =>
        _ = Optional(Grasshopper2.UI.Editor.Instance).Iter(editor => {
            _ = editor.Documents.Pop((Document)document, accept: false);
            editor.Canvas.Document = (Document?)prior.ValueUnsafe();
        });

    // --- [SOLUTIONS]
    public JsonObject Solve(object canvas) {
        Document document = (Document)canvas;
        Task<Solution>? running = document.Solution.Start();
        while (running is { IsCompleted: false }) RhinoApp.Wait();
        return new JsonObject {
            ["phase"] = running?.Status.ToString(),
            ["error"] = running?.Exception is { } failure ? Engine.Text(failure) : null,
            ["objects"] = new JsonArray([.. document.Objects.Forwards.Select(Read)]),
        };
    }

    private static Editor Shown() => Optional(Grasshopper2.UI.Editor.Instance).IfNone(static () => throw new InvalidOperationException("Grasshopper 2 holds no editor"));

    private static Document Pushed(Editor editor, Document document) {
        _ = editor.Documents.Push(document);
        return document;
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
