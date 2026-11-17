#!/usr/bin/env dotnet
#:package Grasshopper
#:package Grasshopper2
#:package LanguageExt.Core
#:package ModelContextProtocol.Core
#:package RhinoCommon
#:property PublishAot=false
#:property TargetFramework=$(RepoTargetFramework)

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Grasshopper;
using Grasshopper2.UI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Skills.UseRhino;

// --- [MODELS] --------------------------------------------------------------------------

[AttributeUsage(AttributeTargets.Property)]
internal sealed class ShownAttribute(string present, string absent) : Attribute {
    public ShownAttribute(string present) : this(present, present) { }

    public string Present { get; } = present;
    public string Absent { get; } = absent;
    public bool Omitted { get; init; }

    public Option<string> Text(object? value) => (value is ICollection items ? items.Count : value) switch {
        null or false or 0 when Omitted => None,
        var shown => string.Format(CultureInfo.InvariantCulture, shown is null or false or 0 ? Absent : Present, shown),
    };
}

internal sealed record Row(string Command, string[] Args, Dictionary<string, string?>? Env = null);
internal sealed record Rows(Dictionary<string, JsonElement?> McpServers);
internal sealed record Slot(string SlotId, int Port, int Pid, Uri Endpoint);
internal sealed record Routed(Slot[]? Payload = null);
internal sealed record Fault(string Message);
internal sealed record Faulted(Fault Error);
internal sealed record Output(string Stdout);

internal sealed record Entry([property: Shown("{0}", "untitled")] string? Name, [property: Shown("modified", "unmodified")] bool Modified);
internal sealed record Document(Entry Entry, [property: Shown("slot={0}", Omitted = true)] string? Slot, [property: Shown("active", Omitted = true)] bool Active);

internal sealed record Reply(
    [property: Shown("Rhino {0}")] int Major,
    [property: Shown("pre-release", Omitted = true)] bool PreRelease,
    [property: Shown("Process ID {0}")] int Pid,
    Listing Command);

internal sealed record Listing(
    [property: Shown("Grasshopper 2 editor open", Omitted = true)] bool Gh2Editor,
    [property: Shown("Documents ({0})")] IReadOnlyList<Document> Documents,
    [property: Shown("Grasshopper 2 definitions ({0})")] IReadOnlyList<Entry> Gh2,
    [property: Shown("Grasshopper 1 definitions ({0})", Omitted = true)] IReadOnlyList<Entry> Gh1) {
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
[JsonSerializable(typeof(Rows))]
[JsonSerializable(typeof(Row))]
[JsonSerializable(typeof(Routed))]
[JsonSerializable(typeof(Faulted))]
[JsonSerializable(typeof(Output))]
[JsonSerializable(typeof(Entry))]
[JsonSerializable(typeof(Document))]
[JsonSerializable(typeof(Reply))]
internal sealed partial class Wire : JsonSerializerContext {
    public static IO<T> Read<T>(string json, JsonTypeInfo<T> type) =>
        IO.lift(() => JsonSerializer.Deserialize(json, type)).Bind(value => IO.lift(Optional(value).ToFin(new Refused($"{type.Type.Name} reply is JSON null"))));
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, typeof(T), Default);
}

// --- [ERRORS] --------------------------------------------------------------------------

internal sealed record Refused(string Text) : Expected(Text, 0);

// --- [COMPOSITION] ---------------------------------------------------------------------

internal static class Program {
    private const string Server = "rhino-mcp-platform";
    private static readonly Refused Unrouted = new($"no {Server} row in the .mcp.json of the plugin holding this script");

    public static async Task<int> Main(string[] args) => args switch {
        [] or [nameof(Listing)] => await RunAsync().ConfigureAwait(false),
        _ => Usage(args),
    };

    private static async Task<int> RunAsync() {
        IO<string> listed = Router()
            .Bind(static router => IO.lift(static () => new HttpClient()).Bracket(Use: http => List(http, router), Fin: static http => IO.lift(http.Dispose)));
        Console.WriteLine(await (listed | @catch(Unanswered, static error => IO.pure($"Rhino state unknown. {error.Message}")))
            .RunAsync(CancellationToken.None).ConfigureAwait(false));
        return 0;
    }

    private static int Usage(string[] args) {
        Console.Error.WriteLine($"No command named '{string.Join(' ', args)}', run without arguments");
        return 2;
    }

    private static void Answer(IReadOnlyDictionary<int, string> slots) =>
        Console.Write(Wire.Write(new Reply(RhinoApp.ExeVersion, RhinoApp.IsPreRelease, Environment.ProcessId, Listing.Query(slots))));

    private static IO<Func<IClientTransport>> Router() =>
        from file in IO.lift(static () => Optional(AppContext.GetData("EntryPointFileDirectoryPath") as string)
            .Map(static scripts => Path.GetFullPath(Path.Combine(scripts, "..", "..", "..", ".mcp.json")))
            .Filter(File.Exists)
            .ToFin(Unrouted))
        from text in IO.liftAsync(env => File.ReadAllTextAsync(file, env.Token))
        from rows in Wire.Read(text, Wire.Default.Rows)
        from entry in IO.lift(Optional(rows.McpServers.GetValueOrDefault(Server)).ToFin(Unrouted))
        from row in Wire.Read(entry.GetRawText(), Wire.Default.Row)
        select (Func<IClientTransport>)(() => new StdioClientTransport(new() { Name = Server, Command = row.Command, Arguments = row.Args, EnvironmentVariables = row.Env }));

    private static IO<string> List(HttpClient http, Func<IClientTransport> router) =>
        from reply in Call(router, "list_slots", ReadOnlyDictionary<string, object?>.Empty).Map(static result => result.Content.OfType<TextContentBlock>().Single().Text)
        from routed in Wire.Read(reply, Wire.Default.Routed)
        from slots in routed.Payload is { } payload ? IO.pure(payload) : Wire.Read(reply, Wire.Default.Faulted).Bind(static faulted => IO.fail<Slot[]>(new Refused(faulted.Error.Message)))
        from applications in toSeq(slots.GroupBy(static slot => slot.Pid)).Traverse(application => Ask(http, application)).As()
        select applications.IsEmpty ? "Rhino not running" : string.Join('\n', applications.Bind(identity));

    private static IO<Seq<string>> Ask(HttpClient http, IGrouping<int, Slot> application) =>
        Call(() => new HttpClientTransport(new() { Endpoint = application.First().Endpoint }, http), "run_csharp", new Dictionary<string, object?>(StringComparer.Ordinal) { ["script"] = Loader(application) })
            .Bind(static result => result.IsError is true
                ? Wire.Read(result.Content.OfType<TextContentBlock>().First().Text, Wire.Default.Fault).Bind(static fault => IO.fail<Seq<string>>(new Refused(fault.Message)))
                : from output in Wire.Read(result.Content.OfType<TextContentBlock>().Last().Text, Wire.Default.Output)
                  from reply in Wire.Read(output.Stdout, Wire.Default.Reply)
                  select Lines(reply))
            | @catch(Unanswered, error => IO.pure(Seq(string.Create(CultureInfo.InvariantCulture, $"Process ID {application.Key} listing failed. {error.Message}"))));

    private static IO<CallToolResult> Call(Func<IClientTransport> transport, string tool, IReadOnlyDictionary<string, object?> arguments) =>
        IO.liftAsync(env => McpClient.CreateAsync(transport(), cancellationToken: env.Token)).Bracket(
            Use: client => IO.liftVAsync(env => client.CallToolAsync(tool, arguments, cancellationToken: env.Token)),
            Fin: static client => IO.liftVAsync(() => client.DisposeAsync().ToUnit()));

    private static bool Unanswered(Error error) => error.IsType<Refused>() || error.HasException<HttpRequestException>();

    private static string Loader(IEnumerable<Slot> slots) => string.Join('\n', [
        "var context = new System.Runtime.Loader.AssemblyLoadContext(null, isCollectible: true);",
        "try {",
        $"    var module = context.LoadFromStream(new System.IO.MemoryStream(System.IO.File.ReadAllBytes({Wire.Write(typeof(Program).Assembly.Location)}))).ManifestModule;",
        string.Create(CultureInfo.InvariantCulture, $"    module.ResolveMethod({((Action<IReadOnlyDictionary<int, string>>)Answer).Method.MetadataToken})"),
        $"        .Invoke(null, System.Reflection.BindingFlags.DoNotWrapExceptions, null, new object[] {{ new Dictionary<int, string> {{ {string.Join(", ", slots.Select(static slot => string.Create(CultureInfo.InvariantCulture, $"[{slot.Port}] = {Wire.Write(slot.SlotId)}")))} }} }}, null);",
        "} finally {",
        "    context.Unload();",
        "}",
    ]);

    private static Seq<(ShownAttribute Shown, object? Value)> Fields(object node) =>
        toSeq(Wire.Default.Options.GetTypeInfo(node.GetType()).Properties).Bind(property => property switch {
            { Get: { } get } when property.AttributeProvider?.GetCustomAttributes(typeof(ShownAttribute), inherit: false) is [ShownAttribute shown] => Seq((shown, get(node))),
            { Get: { } get } when get(node) is { } nested => Fields(nested),
            _ => [],
        });

    private static Seq<string> Lines(object node) => Fields(node) switch {
        var fields => [
            string.Join(", ", fields.Filter(static field => field.Value is not ICollection).Bind(static field => field.Shown.Text(field.Value).ToSeq())),
            .. fields.Bind(static field => field.Value is ICollection items
                ? field.Shown.Text(items).ToSeq().Bind(text => text.Cons(toSeq(items.Cast<object>()).Bind(Lines).Map(static line => $"  {line}")))
                : []).Map(static line => $"  {line}"),
        ],
    };
}
