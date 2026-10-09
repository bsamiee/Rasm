#!/usr/bin/env dotnet
#:package Grasshopper
#:package Grasshopper2
#:package LanguageExt.Core
#:package ModelContextProtocol.Core
#:package RhinoCommon
#:property PublishAot=false
#:property TargetFramework=$(RepoTargetFramework)

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Grasshopper;
using Grasshopper2.UI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Skills.UseRhino;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface ICommand<TSelf> where TSelf : ICommand<TSelf> {
    public static abstract TSelf Query(IReadOnlyDictionary<int, string> slots);
}

internal enum Codes {
    Refused = 1,
}

// --- [MODELS] --------------------------------------------------------------------------
[AttributeUsage(AttributeTargets.Property)]
internal sealed class ShownAttribute(string present, string absent) : Attribute {
    public ShownAttribute(string present) : this(present, present) { }

    public string Present => present;
    public string Absent => absent;
    public bool Omitted { get; init; }

    public Option<string> Text(object? value) => (value is ICollection items ? items.Count : value) switch {
        null or false or 0 when Omitted => None,
        var shown => string.Format(CultureInfo.InvariantCulture, shown is null or false or 0 ? Absent : Present, shown),
    };
}

internal sealed record Server(Uri Url);
internal sealed record Servers([property: JsonPropertyName("rhino-mcp-platform")] Server Router);
internal sealed record Config(Servers McpServers);
internal sealed record Slot(string SlotId, int Port, int Pid, Uri Endpoint);
internal sealed record Routed(Slot[]? Payload = null);
internal sealed record Fault(string Message);
internal sealed record Faulted(Fault Error);
internal sealed record Output(string Stdout);

internal sealed record Entry([property: Shown("{0}", "untitled")] string? Name, [property: Shown("modified", "unmodified")] bool Modified);
internal sealed record Document(Entry Entry, [property: Shown("slot={0}", Omitted = true)] string? Slot, [property: Shown("active", Omitted = true)] bool Active);

internal sealed record Reply<T>(
    [property: Shown("Rhino {0}")] int Major,
    [property: Shown("pre-release", Omitted = true)] bool PreRelease,
    [property: Shown("pid {0}")] int Pid,
    T Command) where T : ICommand<T>;

internal sealed record Listing(
    [property: Shown("Grasshopper 2 editor open", Omitted = true)] bool Gh2Editor,
    [property: Shown("documents: {0}")] IReadOnlyList<Document> Documents,
    [property: Shown("Grasshopper 2 definitions: {0}")] IReadOnlyList<Entry> Gh2,
    [property: Shown("Grasshopper 1 definitions: {0}", Omitted = true)] IReadOnlyList<Entry> Gh1) : ICommand<Listing> {
    public static Listing Query(IReadOnlyDictionary<int, string> slots) => (Loaded("Grasshopper2") ? ReadGh2() : (false, [])) switch {
        var (open, gh2) => new(
            open,
            [.. RhinoDoc.OpenDocuments().Select(document => new Document(
                new Entry(document.Name, document.Modified),
                TryGetPortFor(host: null, document, out int port) ? slots.GetValueOrDefault(port) : null,
                document.RuntimeSerialNumber == RhinoDoc.ActiveDoc?.RuntimeSerialNumber))],
            gh2,
            Loaded("Grasshopper") ? ReadGh1() : []),
    };

    private static bool Loaded(string plugIn) => PlugIn.GetPlugInInfo(PlugIn.IdFromName(plugIn)) is { IsLoaded: true };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (bool Open, IReadOnlyList<Entry> Definitions) ReadGh2() => Editor.Instance is { } editor
        ? (editor.Visible, [.. editor.Documents.All.Select(static stack => new Entry(Path.GetFileName(stack.Root.File.Path), stack.Root.Modified))])
        : (false, []);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IReadOnlyList<Entry> ReadGh1() => [.. Instances.DocumentServer.Select(static definition => new Entry(Path.GetFileName(definition.FilePath), definition.IsModified))];

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(TryGetPortFor))]
    private static extern bool TryGetPortFor([UnsafeAccessorType("Rhino.AI.RhinoAIHost, RhinoAI")] object? host, RhinoDoc document, out int port);
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(Routed))]
[JsonSerializable(typeof(Faulted))]
[JsonSerializable(typeof(Output))]
[JsonSerializable(typeof(Entry))]
[JsonSerializable(typeof(Document))]
[JsonSerializable(typeof(Reply<Listing>))]
internal sealed partial class Wire : JsonSerializerContext {
    public static T Read<T>(string json) => (T)JsonSerializer.Deserialize(json, typeof(T), Default)!;
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, typeof(T), Default);
}

// --- [ERRORS] --------------------------------------------------------------------------
internal sealed record Refused(string Text) : Expected(Text, (int)Codes.Refused);

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    public static int Main(string[] args) => args switch {
        [] or [nameof(Listing)] => Run<Listing>(),
        _ => Usage(args),
    };

    private static int Run<T>([CallerFilePath] string source = "") where T : ICommand<T> {
        Console.WriteLine(IO.lift(static () => new HttpClient())
            .Bracket(Use: http => List<T>(http, Router(new FileInfo(source).Directory)), Fin: static http => IO.lift(http.Dispose))
            .Catch(Unanswered, static error => IO.pure($"Rhino: unknown, {error.Message}"))
            .Run());
        return 0;
    }

    private static int Usage(string[] args) {
        Console.Error.WriteLine($"No command named '{string.Join(' ', args)}'");
        return 2;
    }

    private static void Answer<T>(IReadOnlyDictionary<int, string> slots) where T : ICommand<T> =>
        Console.Write(Wire.Write(new Reply<T>(RhinoApp.ExeVersion, RhinoApp.IsPreRelease, Environment.ProcessId, T.Query(slots))));

    private static Uri Router(DirectoryInfo? directory) => directory?.GetFiles(".mcp.json") switch {
        [FileInfo config] => Wire.Read<Config>(File.ReadAllText(config.FullName)).McpServers.Router.Url,
        [] => Router(directory.Parent),
        _ => throw new FileNotFoundException("No .mcp.json in a folder above the script", ".mcp.json"),
    };

    private static IO<string> List<T>(HttpClient http, Uri router) where T : ICommand<T> =>
        from reply in Call(http, router, "list_slots", ReadOnlyDictionary<string, object?>.Empty).Map(static result => result.Content.OfType<TextContentBlock>().Single().Text)
        from slots in Wire.Read<Routed>(reply).Payload is { } payload ? IO.pure(payload) : IO.fail<Slot[]>(new Refused(Wire.Read<Faulted>(reply).Error.Message))
        from applications in toSeq(slots.GroupBy(static slot => slot.Pid)).Traverse(application => Ask<T>(http, application)).As()
        select applications.IsEmpty ? "Rhino: not running" : string.Join('\n', applications.Bind(identity));

    private static IO<Seq<string>> Ask<T>(HttpClient http, IGrouping<int, Slot> application) where T : ICommand<T> =>
        Call(http, application.First().Endpoint, "run_csharp", new Dictionary<string, object?>(StringComparer.Ordinal) { ["script"] = Loader<T>(application) })
            .Bind(static result => result.IsError is true
                ? IO.fail<Seq<string>>(new Refused(Wire.Read<Fault>(result.Content.OfType<TextContentBlock>().First().Text).Message))
                : IO.pure(Lines(Wire.Read<Reply<T>>(Wire.Read<Output>(result.Content.OfType<TextContentBlock>().Last().Text).Stdout))))
            .Catch(Unanswered, error => IO.pure(Seq(string.Create(CultureInfo.InvariantCulture, $"pid {application.Key}, unanswered: {error.Message}"))));

    private static IO<CallToolResult> Call(HttpClient http, Uri endpoint, string tool, IReadOnlyDictionary<string, object?> arguments) =>
        IO.liftAsync(env => McpClient.CreateAsync(new HttpClientTransport(new() { Endpoint = endpoint }, http), cancellationToken: env.Token)).Bracket(
            Use: client => IO.liftVAsync(env => client.CallToolAsync(tool, arguments, cancellationToken: env.Token)),
            Fin: static client => IO.liftVAsync(() => client.DisposeAsync().ToUnit()));

    private static bool Unanswered(Error error) => error.IsType<Refused>() || error.HasException<HttpRequestException>();

    private static string Loader<T>(IEnumerable<Slot> slots) where T : ICommand<T> => string.Join('\n', [
        "var context = new System.Runtime.Loader.AssemblyLoadContext(null, isCollectible: true);",
        "try {",
        $"    var module = context.LoadFromStream(new System.IO.MemoryStream(System.IO.File.ReadAllBytes({Wire.Write(typeof(T).Assembly.Location)}))).ManifestModule;",
        string.Create(CultureInfo.InvariantCulture, $"    ((System.Reflection.MethodInfo)module.ResolveMethod({((Action<IReadOnlyDictionary<int, string>>)Answer<T>).Method.MetadataToken})).MakeGenericMethod(module.ResolveType({typeof(T).MetadataToken}))"),
        $"        .Invoke(null, System.Reflection.BindingFlags.DoNotWrapExceptions, null, new object[] {{ new Dictionary<int, string> {{ {string.Join(", ", slots.Select(static slot => string.Create(CultureInfo.InvariantCulture, $"[{slot.Port}] = {Wire.Write(slot.SlotId)}")))} }} }}, null);",
        "} finally {",
        "    context.Unload();",
        "}",
    ]);

    private static Seq<(ShownAttribute Shown, object? Value)> Fields(object node) =>
        toSeq(Wire.Default.Options.GetTypeInfo(node.GetType()).Properties).Bind(property => property.AttributeProvider?.GetCustomAttributes(typeof(ShownAttribute), inherit: false) is [ShownAttribute shown]
            ? Seq((shown, property.Get!(node)))
            : Fields(property.Get!(node)!));

    private static Seq<string> Lines(object node) => Fields(node) switch {
        var fields => [
            string.Join(", ", fields.Filter(static field => field.Value is not ICollection).Bind(static field => field.Shown.Text(field.Value).ToSeq())),
            .. fields.Bind(static field => field.Value is ICollection items ? Section(field.Shown.Text(items), toSeq(items.Cast<object>()).Bind(Lines)) : []).Map(static line => $"  {line}"),
        ],
    };

    private static Seq<string> Section(Option<string> header, Seq<string> lines) => header.ToSeq().Bind(text => text.Cons(lines.Map(static line => $"  {line}")));
}
