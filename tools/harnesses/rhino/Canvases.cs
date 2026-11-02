using System.Collections.Generic;
using System.Text.Json.Nodes;
using Grasshopper2.Components;
using Grasshopper2.Diagnostics;
using Grasshopper2.Extensions;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Rhino;
using GhDocument = Grasshopper2.Doc.Document;

namespace Rasm.Harnesses.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Canvases : ICanvases {
    private Option<Density> density;

    // --- [HOST]
    public object Editor => (Grasshopper2.UI.Editor.Instance ?? throw new InvalidOperationException("Grasshopper 2 holds no editor")).Canvas;

    public Grasshopper Start() {
        _ = Grasshopper2.UI.Editor.Instance is null ? PluginServer.LoadAllScopedPlugins() : default;
        _ = Grasshopper2.UI.Editor.Instance ?? Grasshopper2.UI.Editor.ShowEditor(createVisible: false);
        density = density.IsSome ? density : Some(Grasshopper2.Diagnostics.Logger.Density);
        Grasshopper2.Diagnostics.Logger.Density = Density.Verbose;
        Plugins state = PluginServer.State;
        return new Grasshopper(typeof(Grasshopper2.UI.Editor).Assembly.GetName().Version?.ToString() ?? "",
            [.. state.LoadedPlugins.Select(static plugin => $"{plugin.Name} {plugin.Version}")],
            [.. state.Failures.Select(static failure => $"{failure.location}: {failure.info.Kind} {failure.info.Reason}")],
            PluginServer.IsFullyLoaded(out string incomplete) ? null : incomplete);
    }

    public void Stop() => _ = density.Iter(static held => Grasshopper2.Diagnostics.Logger.Density = held);

    public int Logged() => Grasshopper2.Diagnostics.Logger.Count;

    public ImmutableArray<Diagnosed> Diagnostics(int since) =>
        [.. Grasshopper2.Diagnostics.Logger.Diagnostics().Skip(since).Select(static entry => new Diagnosed(entry.TimeStamp, entry.Kind.ToString(), entry.Message))];

    // --- [DOCUMENTS]
    public (object Document, object? Prior) Open(string? source) {
        Editor editor = Grasshopper2.UI.Editor.Instance ?? throw new InvalidOperationException("Grasshopper 2 holds no editor");
        GhDocument? prior = editor.Canvas.Document;
        GhDocument document = source is null
            ? Pushed(editor, GhDocument.NewActiveDocument())
            : editor.Documents.TryOpenDocument(source, Grasshopper2.UI.Canvas.OpenDocumentOptions.Activate) ? editor.Documents.Current : throw new FileLoadException("editor refused the definition", source);
        editor.Canvas.Document = document;
        return ReferenceEquals(editor.Canvas.Document, document) ? (document, prior) : throw new InvalidOperationException("editor canvas kept another document");
    }

    public void Release(object document, object? prior) {
        Editor? editor = Grasshopper2.UI.Editor.Instance;
        _ = editor?.Documents.Pop((GhDocument)document, accept: false);
        editor?.Canvas.Document = (GhDocument?)prior;
    }

    // --- [SOLUTIONS]
    public JsonObject Solve(object canvas) {
        GhDocument document = (GhDocument)canvas;
        Task<Grasshopper2.Doc.Solution>? running = document.Solution.Start();
        while (running is { IsCompleted: false }) RhinoApp.Wait();
        return new JsonObject {
            ["phase"] = running?.Status.ToString(),
            ["error"] = running?.Exception is { } failure ? Engine.Text(failure) : null,
            ["objects"] = new JsonArray([.. document.Objects.Forwards.Select(Read)]),
        };
    }

    private static GhDocument Pushed(Editor editor, GhDocument document) {
        _ = editor.Documents.Push(document);
        return document;
    }

    private static JsonObject Read(Grasshopper2.Doc.IDocumentObject member) => new() {
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

    private static IEnumerable<IParameter> Outputs(Grasshopper2.Doc.IDocumentObject member) => member switch {
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
