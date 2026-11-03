using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Xml.Linq;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Runtime;
using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Diagnostics;
using Rhino.Runtime.Code.Execution;
using Rhino.Runtime.Code.Languages;
using Rhino.Runtime.Code.Logging;
using Rhino.Runtime.Code.Text;
using Rhino.UI;

namespace Rasm.Harnesses.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ICanvases {
    public object Editor { get; }

    public Grasshopper Start();

    public void Stop();

    public (object Document, Option<object> Prior) Open(Option<string> source);

    public void Release(object document, Option<object> prior);

    public int Logged();

    public ImmutableArray<Diagnosed> Diagnostics(int since);

    public JsonObject Solve(object canvas);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class Special {
    public static readonly Special NaN = new("NaN", double.NaN);
    public static readonly Special Infinity = new("Infinity", double.PositiveInfinity);
    public static readonly Special NegativeInfinity = new("-Infinity", double.NegativeInfinity);
    public static readonly Special NegativeZero = new("-0", -0.0);

    public double Value { get; }

    public bool Holds(double number) => double.IsNaN(Value) ? double.IsNaN(number) : BitConverter.DoubleToInt64Bits(Value) == BitConverter.DoubleToInt64Bits(number);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Tolerances(double Absolute, double? Angle = null, double? Relative = null);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_tag")]
[JsonDerivedType(typeof(Template), nameof(Template))]
[JsonDerivedType(typeof(Open), nameof(Open))]
[JsonDerivedType(typeof(Application), nameof(Application))]
[Union]
public abstract partial record DocumentSource {
    private DocumentSource() { }

    public sealed record Template(string Name, UnitSystem Units, Tolerances Tolerance) : DocumentSource;
    public sealed record Open(string Path, UnitSystem? Units = null, Tolerances? Tolerance = null) : DocumentSource;
    public sealed record Application : DocumentSource;
}

public sealed record View(DefinedViewportProjection? Projection = null, string? Display = null, ImmutableArray<double>? Location = null, ImmutableArray<double>? Target = null, double? Lens = null);
public sealed record Page(string Name, double Width, double Height, UnitSystem? Units = null);
public sealed record LayerRow(string Path, string? Color = null, bool? Locked = null);
public sealed record Added(string As, string? Layer = null);
public sealed record Canvas(string? Open = null);

public sealed record Placed(DocumentSource Source) {
    public View? View { get; init; }
    public Page? Layout { get; init; }
    public ImmutableArray<LayerRow> Layers { get; init; } = [];
    public ImmutableArray<Added> Add { get; init; } = [];
    public ImmutableArray<string> Select { get; init; } = [];
    public Canvas? Canvas { get; init; }
    public bool Batch { get; init; }
}

public sealed record Case(string Name, string Folder, Placed Precondition) {
    public ImmutableArray<string> Reads { get; init; } = [];
    public ImmutableArray<string> Macros { get; init; } = [];
    public ImmutableArray<JsonElement> Args { get; init; } = [];
    public string? Script { get; init; }
}

[Union]
public abstract partial record Expressions {
    private Expressions() { }

    public sealed record Compiled(Action<Scope> Steps, Func<Scope, object?>? Body, ImmutableArray<Func<Scope, object?>> Reads, ImmutableArray<Func<Scope, GeometryBase>> Geometry) : Expressions;
    public sealed record Uncompiled(string Reason) : Expressions;
}

public sealed record Batch(ImmutableArray<JsonElement> Cases, string? Plugin = null);

public sealed record Opened(ICanvases Canvases, object Document, Option<object> Prior);

public sealed record Scope(Option<RhinoDoc> Document, Option<Opened> Canvas, ImmutableArray<JsonElement> Args, string Folder) {
    public Map<string, object?> Made { get; init; } = Map<string, object?>();

    public RhinoDoc Doc => Document.IfNone(static () => throw new InvalidOperationException("application case opens no document"));

    public Opened Shown => Canvas.IfNone(static () => throw new InvalidOperationException("precondition opens no canvas"));

    public Fin<JsonObject> Capture(int width, int height) =>
        from view in Optional(Doc.Views.ActiveView).ToFin(Error.New("document holds no active view"))
        from bitmap in Optional(new ViewCapture { Width = width, Height = height, ScaleScreenItems = false }.CaptureToBitmap(view)).ToFin(Error.New($"view {view.ActiveViewport.Name} captured no bitmap"))
        select Saved(bitmap);

    public JsonObject Solve() => Shown.Canvases.Solve(Shown.Document);

    private JsonObject Saved(Bitmap bitmap) {
        using Bitmap held = bitmap;
        string file = Path.Combine(Folder, string.Create(CultureInfo.InvariantCulture, $"{Directory.EnumerateFiles(Folder, "*.png").Count() + 1}.png"));
        held.Save(file, ImageFormat.Png);
        return new JsonObject { ["file"] = file, ["sha256"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(file))) };
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_tag")]
[JsonDerivedType(typeof(Ran), nameof(Ran))]
[JsonDerivedType(typeof(Thrown), nameof(Thrown))]
[JsonDerivedType(typeof(Failed), nameof(Failed))]
internal abstract record Completion {
    private Completion() { }

    internal sealed record Ran : Completion;
    internal sealed record Thrown(string Message, int? Line = null, string? File = null) : Completion;
    internal sealed record Failed(string Reason) : Completion;
}

internal sealed record Macro(string Text, bool Completed);
internal sealed record Executed(string Name, string? Plugin, global::Rhino.Commands.Result Result, uint Document);
internal sealed record Prompt(string Text, string? Default, ImmutableArray<string> Options);
public sealed record Diagnosed(string Source, string Kind, string Message);
internal sealed record Units(UnitSystem Model, UnitSystem Page);
internal sealed record Saved(string Before, string After);
internal sealed record Viewed(string Name, string Display, bool Parallel);
internal sealed record DocumentSnapshot(uint Serial, string? Path, string? Template, Units Units, Tolerances Tolerance, bool Active, bool Modified, int Selected, Viewed? View, ImmutableArray<string> Undo);

internal sealed record Outcome(string Name, Completion Completion) {
    public JsonNode? Value { get; init; }
    public JsonObject? Before { get; init; }
    public JsonObject? BeforeFailed { get; init; }
    public JsonObject? Read { get; init; }
    public JsonObject? ReadFailed { get; init; }
    public ImmutableArray<string> Leaked { get; init; } = [];
    public ImmutableArray<string>? Console { get; init; }
    public DocumentSnapshot? Document { get; init; }
    public Saved? Saved { get; init; }
    public ImmutableArray<Macro>? Macros { get; init; }
    public ImmutableArray<Executed>? Commands { get; init; }
    public ImmutableArray<Prompt>? Prompts { get; init; }
    public ImmutableArray<Diagnosed>? Diagnostics { get; init; }
    public string? Unhandled { get; init; }
}

internal sealed record PlugInLoad(string File, Guid Id, LoadPlugInResult Result, bool Current);
internal sealed record Loaded(string Name, string Version);
public sealed record Grasshopper(string Version, ImmutableArray<string> Loaded, ImmutableArray<string> Failed, string? Incomplete);
internal sealed record Reply(ImmutableArray<Loaded> Plugins, PlugInLoad? PlugIn = null, Grasshopper? Grasshopper = null, string? Failed = null);
internal sealed record Batched(Option<PlugInLoad> PlugIn, Option<Grasshopper> Grasshopper);
internal sealed record Progress(Option<Grasshopper> Grasshopper, ImmutableArray<uint> Previous);
internal sealed record Recording(Option<string> Running, Seq<Executed> Commands, Seq<Prompt> Prompts, Seq<Diagnosed> Diagnostics) {
    public static readonly Recording Empty = new(None, Seq<Executed>(), Seq<Prompt>(), Seq<Diagnosed>());
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_tag")]
[JsonDerivedType(typeof(Started), nameof(Started))]
[JsonDerivedType(typeof(Finished), nameof(Finished))]
internal abstract record JournalEntry;
internal sealed record Started(int Index) : JournalEntry;
internal sealed record Finished(Outcome Outcome) : JournalEntry;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, UseStringEnumConverter = true, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(JournalEntry))]
[JsonSerializable(typeof(Reply))]
internal sealed partial class Wire : JsonSerializerContext;

internal sealed class Tagged<T> : JsonConverter<T> where T : struct, IBinaryFloatingPointIeee754<T> {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        T.CreateChecked(reader.TokenType is JsonTokenType.StartObject ? Tag(JsonElement.ParseValue(ref reader)) : reader.GetDouble());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        double number = double.CreateChecked(value);
        _ = toSeq(Special.Items).Find(item => item.Holds(number)).Match(
            Some: item => {
                writer.WriteStartObject();
                writer.WriteString("$", item.Key);
                writer.WriteEndObject();
            },
            None: () => writer.WriteNumberValue(number));
    }

    private static double Tag(JsonElement element) =>
        Optional(element.GetProperty("$").GetString())
            .Bind(static key => Special.TryGet(key, out Special? item) ? Some(item.Value) : Option<double>.None)
            .IfNone(() => throw new JsonException($"{element.GetProperty("$")} tags no IEEE 754 special value"));
}

public sealed class Archive<T> : JsonConverter<T> where T : CommonObject {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        CommonObject.FromJSON(JsonElement.ParseValue(ref reader).GetRawText()) as T ?? throw new JsonException($"archive revives no {typeToConvert}");

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => writer.WriteRawValue(value.ToJSON(new SerializationOptions()));
}

internal sealed class Native : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) => typeof(CommonObject).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        Activator.CreateInstance(typeof(Archive<>).MakeGenericType(typeToConvert)) as JsonConverter ?? throw new InvalidOperationException($"no archive converter for {typeToConvert}");
}

internal sealed class Described : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) => typeof(MemberInfo).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        Activator.CreateInstance(typeof(Member<>).MakeGenericType(typeToConvert)) as JsonConverter ?? throw new InvalidOperationException($"no member converter for {typeToConvert}");
}

public sealed class Member<T> : JsonConverter<T> where T : MemberInfo {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException($"{typeToConvert} is read from the host, not written into it");

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => Documents.Describe(value).WriteTo(writer, options);
}

internal sealed class Answers : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) => Answered(typeToConvert).IsSome;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        Answered(typeToConvert).Bind(static fin => Optional(Activator.CreateInstance(typeof(Answer<>).MakeGenericType(fin.GetGenericArguments())) as JsonConverter))
            .IfNone(() => throw new InvalidOperationException($"no answer converter for {typeToConvert}"));

    private static Option<Type> Answered(Type type) =>
        toSeq(LanguageExt.List.unfold(Optional(type), static held => held.Map(static current => (current, Optional(current.BaseType)))))
            .Find(static held => held.IsGenericType && held.GetGenericTypeDefinition() == typeof(Fin<>));
}

public sealed class Answer<T> : JsonConverter<Fin<T>> {
    public override Fin<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException($"{typeToConvert} is answered by the host, not written into it");

    public override void Write(Utf8JsonWriter writer, Fin<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.IfFail(static error => throw new JsonException(Engine.Message(error))), options);
}

internal sealed class Refused : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert is { IsClass: true } && typeToConvert != typeof(string) && typeToConvert != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(typeToConvert)
        && !typeof(JsonNode).IsAssignableFrom(typeToConvert) && !typeof(CommonObject).IsAssignableFrom(typeToConvert) && !typeof(MemberInfo).IsAssignableFrom(typeToConvert)
        && !typeToConvert.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"{typeToConvert.FullName} is a reference type outside CommonObject, read its members by name");
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Journal(string path) : IDisposable {
    private readonly StreamWriter writer = new(path, append: true) { AutoFlush = true };
    private readonly Lock gate = new();

    public void Write(JournalEntry entry) {
        lock (gate) writer.WriteLine(JsonSerializer.Serialize(entry, Wire.Default.JournalEntry));
    }

    public void Dispose() => writer.Dispose();
}

internal sealed class Captured : IDisposable {
    private readonly (TextWriter Output, TextWriter Error) prior = (Console.Out, Console.Error);
    private readonly StringWriter stdout = new(CultureInfo.InvariantCulture);
    private readonly StringWriter stderr = new(CultureInfo.InvariantCulture);

    public Captured() {
        Console.SetOut(stdout);
        Console.SetError(stderr);
    }

    public string Output => stdout.ToString();

    public string Errors => stderr.ToString();

    public void Dispose() {
        Console.SetOut(prior.Output);
        Console.SetError(prior.Error);
        stdout.Dispose();
        stderr.Dispose();
    }
}

internal sealed class Recorder(Journal journal) {
    private readonly Atom<Recording> state = Atom(Recording.Empty);

    // --- [LIFECYCLE]
    public IO<Unit> Attached() =>
        from unhandled in Attached<UnhandledExceptionEventHandler>(static handler => AppDomain.CurrentDomain.UnhandledException += handler, static handler => AppDomain.CurrentDomain.UnhandledException -= handler, Unhandled)
        from reporting in Attached<HostUtils.ExceptionReportDelegate>(static handler => HostUtils.OnExceptionReport += handler, static handler => HostUtils.OnExceptionReport -= handler, Reporting)
        from ending in Attached<EventHandler<CommandEventArgs>>(static handler => Command.EndCommand += handler, static handler => Command.EndCommand -= handler, Ending)
        from prompting in Attached<EventHandler<CommandPromptChangedEventArgs>>(static handler => RhinoApp.CommandPromptChanged += handler, static handler => RhinoApp.CommandPromptChanged -= handler, Prompting)
        from logging in Attached<LoggerEventHandler<LoggerWriteEventArgs>>(static handler => RhinoCode.Logger.MessageLogged += handler, static handler => RhinoCode.Logger.MessageLogged -= handler, Logging)
        select unit;

    public IO<Unit> Start(int index, string name) =>
        IO.lift(() => {
            journal.Write(new Started(index));
            _ = state.Swap(_ => Recording.Empty with { Running = Some(name) });
        });

    public IO<Unit> Finish(Outcome outcome) =>
        IO.lift(() => {
            _ = state.Swap(static held => held with { Running = None });
            journal.Write(new Finished(outcome));
        });

    public IO<Unit> Begin() =>
        IO.lift(() => {
            _ = RhinoApp.CapturedCommandWindowStrings(clearBuffer: true);
            _ = state.Swap(static held => Recording.Empty with { Running = held.Running });
            RhinoApp.CommandWindowCaptureEnabled = true;
        });

    public IO<Outcome> End(Outcome outcome, string stdout, string stderr, ImmutableArray<Diagnosed> canvas) =>
        IO.lift(() => {
            string[] printed = RhinoApp.CapturedCommandWindowStrings(clearBuffer: true);
            RhinoApp.CommandWindowCaptureEnabled = false;
            Recording held = state.Value;
            ImmutableArray<string> console = [.. printed, .. Lines(stdout), .. Lines(stderr)];
            ImmutableArray<Diagnosed> logged = [.. held.Diagnostics, .. canvas];
            return outcome with {
                Console = console.IsEmpty ? null : console,
                Commands = held.Commands.IsEmpty ? null : [.. held.Commands],
                Prompts = held.Prompts.IsEmpty ? null : [.. held.Prompts],
                Diagnostics = logged.IsEmpty ? null : logged,
            };
        });

    private static IO<Unit> Attached<THandler>(Action<THandler> add, Action<THandler> remove, THandler handler) where THandler : Delegate =>
        use(() => {
            add(handler);
            return handler;
        }, remove).Map(static _ => unit);

    // --- [HANDLERS]
    private static string[] Lines(string text) => text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private void Ending(object? sender, CommandEventArgs args) =>
        _ = state.Swap(held => held with { Commands = held.Commands.Add(new Executed(args.CommandEnglishName, args.CommandPluginName, args.CommandResult, args.DocumentRuntimeSerialNumber)) });

    private void Prompting(object? sender, CommandPromptChangedEventArgs args) {
        if (string.IsNullOrEmpty(args.Prompt)) return;
        _ = state.Swap(held => held with { Prompts = held.Prompts.Add(new Prompt(args.Prompt, args.PromptDefault, args.Options is { } options ? [.. options.Select(static option => option.EnglishName)] : [])) });
    }

    private void Reporting(string source, Exception exception) => _ = state.Swap(held => held with { Diagnostics = held.Diagnostics.Add(new Diagnosed("HostUtils", source, Engine.Text(exception))) });

    private void Logging(ILogger logger, LoggerWriteEventArgs args) {
        if (args.Level >= LogLevel.Warn) _ = state.Swap(held => held with { Diagnostics = held.Diagnostics.Add(new Diagnosed("RhinoCode", args.Level.ToString(), args.Message)) });
    }

    private void Unhandled(object? sender, UnhandledExceptionEventArgs args) {
        Recording held = state.Value;
        _ = held.Running.Iter(name => journal.Write(new Finished(new Outcome(name, new Completion.Failed("unhandled exception terminating the host")) {
            Commands = held.Commands.IsEmpty ? null : [.. held.Commands],
            Prompts = held.Prompts.IsEmpty ? null : [.. held.Prompts],
            Diagnostics = held.Diagnostics.IsEmpty ? null : [.. held.Diagnostics],
            Unhandled = string.Create(CultureInfo.InvariantCulture, $"{args.ExceptionObject}"),
        })));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Languages {
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Unit Start(LanguageSpec language) {
        if (!RhinoCodePlatform.Rhino3D.Registrar.IsScriptingLanguageStarted(language)) RhinoCodePlatform.Rhino3D.Registrar.StartScriptingLanguages(language, startServer: false);
        return unit;
    }
}

internal static class Settings {
    public static FrozenDictionary<string, string> Snapshot() => Entries(PersistentSettings.RhinoAppSettings, "").ToFrozenDictionary(StringComparer.Ordinal);

    public static ImmutableArray<string> Changed(FrozenDictionary<string, string> before) {
        FrozenDictionary<string, string> after = Snapshot();
        return [.. before.Keys.Union(after.Keys, StringComparer.Ordinal).Where(path => !string.Equals(before.GetValueOrDefault(path), after.GetValueOrDefault(path), StringComparison.Ordinal)).Order(StringComparer.Ordinal)];
    }

    private static IEnumerable<KeyValuePair<string, string>> Entries(PersistentSettings settings, string parent) =>
        settings.Keys.SelectMany(key => settings.TryGetString(key, out string value) ? [KeyValuePair.Create(parent + key, value)] : Enumerable.Empty<KeyValuePair<string, string>>())
            .Concat(settings.ChildKeys.SelectMany(key => settings.TryGetChild(key, out PersistentSettings child) ? Entries(child, $"{parent}{key}/") : []));
}

internal static class Documents {
    private static readonly ConcurrentDictionary<Assembly, FrozenDictionary<string, string>> Summaries = new();

    public static JsonObject Describe(MemberInfo member) => member switch {
        Type type => new JsonObject {
            ["name"] = type.FullName,
            ["kind"] = type switch { { IsEnum: true } => "enum", { IsInterface: true } => "interface", { IsValueType: true } => "struct", _ => "class" },
            ["base"] = type.BaseType?.FullName,
            ["summary"] = Summary(type, $"T:{type.FullName}"),
            ["values"] = type.IsEnum ? new JsonObject([.. Enum.GetNames(type).Select(name => KeyValuePair.Create(name, (JsonNode?)JsonValue.Create(Convert.ToInt64(Enum.Parse(type, name), CultureInfo.InvariantCulture))))]) : null,
            ["members"] = new JsonArray([.. type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(static held => held is not (Type or MethodBase { IsSpecialName: true })).Select(static held => (JsonNode)Describe(held))]),
        },
        _ => new JsonObject {
            ["kind"] = member.MemberType.ToString(),
            ["name"] = member.Name,
            ["signature"] = member.ToString(),
            ["summary"] = Summary(member.DeclaringType ?? typeof(object), Identifier(member)),
        },
    };

    private static string Identifier(MemberInfo member) => member switch {
        MethodBase method => $"M:{method.DeclaringType?.FullName}.{(method.IsConstructor ? "#ctor" : method.Name)}{(method.GetParameters() is [_, ..] parameters ? $"({string.Join(',', parameters.Select(static parameter => parameter.ParameterType.FullName))})" : "")}",
        PropertyInfo => $"P:{member.DeclaringType?.FullName}.{member.Name}",
        FieldInfo => $"F:{member.DeclaringType?.FullName}.{member.Name}",
        EventInfo => $"E:{member.DeclaringType?.FullName}.{member.Name}",
        _ => member.Name,
    };

    private static string? Summary(Type owner, string identifier) =>
        Summaries.GetOrAdd(owner.Assembly, static assembly => Path.ChangeExtension(assembly.Location, ".xml") is var file && File.Exists(file)
            ? XDocument.Load(file).Descendants("member").Where(static node => node.Attribute("name") is not null)
                .DistinctBy(static node => node.Attribute("name")!.Value, StringComparer.Ordinal)
                .ToFrozenDictionary(static node => node.Attribute("name")!.Value, static node => string.Join(' ', (node.Element("summary")?.Value ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)), StringComparer.Ordinal)
            : FrozenDictionary<string, string>.Empty).GetValueOrDefault(identifier);
}

public static class Engine {
    internal static readonly JsonSerializerOptions Data = new(JsonSerializerDefaults.Web) {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new Tagged<double>(), new Tagged<float>(), new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    private static readonly JsonSerializerOptions Codec = new(Data) { Converters = { new Native(), new Described(), new Answers(), new Refused() } };

    // --- [BATCH]
    public static void Run(string[] args, ImmutableArray<Expressions> codes, Func<ICanvases> canvases) =>
        (from paths in IO.lift(() => (File: args[0], Records: args[1], Reply: args[2]))
         from reply in Batch(paths.File, paths.Records, codes, canvases)
             .Map(static held => new Reply(Plugins(), held.PlugIn.ValueUnsafe(), held.Grasshopper.ValueUnsafe()))
             .Catch(static (Error error) => IO.pure(new Reply(Plugins(), Failed: Message(error)))).As()
         from written in IO.lift(() => Written(paths.Reply, reply))
         select unit).Run();

    public static object? Call(Delegate function, ImmutableArray<JsonElement> args) =>
        function.Method.Invoke(
            function.Target,
            BindingFlags.DoNotWrapExceptions,
            binder: null,
            [.. function.Method.GetParameters().Select((parameter, index) => args[index].Deserialize(parameter.ParameterType, Codec))],
            CultureInfo.InvariantCulture);

    public static Fin<JsonNode?> Encode(object? value) =>
        Try.lift(() => value is null ? null : JsonSerializer.SerializeToNode(value, value.GetType(), Codec)).Run()
            .MapFail(error => Error.New($"value of {value?.GetType().FullName} did not encode: {Message(error)}"));

    public static string Text(Exception exception) => Text(exception, exception.Message);

    public static string Message(Error error) => error.Exception.Match(Some: Text, None: () => error.Message);

    private static string Text(Exception exception, string detail) => string.Join(' ', [
        $"{exception.GetType().FullName}: {detail}",
        .. (exception.StackTrace ?? "").Split(System.Environment.NewLine, 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(1),
        .. Optional(exception.InnerException).ToSeq().Map(static inner => $"> {Text(inner)}"),
    ]);

    private static IO<Batched> Batch(string file, string records, ImmutableArray<Expressions> codes, Func<ICanvases> canvases) =>
        (from journal in use(() => new Journal(records))
         from recorder in IO.pure(new Recorder(journal))
         from attached in recorder.Attached()
         from surface in use(() => new Lazy<Fin<ICanvases>>(() => Surface(canvases)), static held => when(held.IsValueCreated, IO.lift(() => held.Value.Iter(static made => made.Stop()))).As())
         from batch in IO.lift(() => Optional(JsonSerializer.Deserialize<Batch>(File.ReadAllText(file), Data)).ToFin(Error.New($"{file} holds no batch")))
         from plugIn in Optional(batch.Plugin).Traverse(static plugin => IO.lift(() => Load(plugin))).As()
         from progress in toSeq(batch.Cases).Map(static (element, index) => (Element: element, Index: index))
             .FoldM(new Progress(None, []), (held, next) => Cased(next.Element, next.Index, codes, recorder, surface, held)).As()
         select new Batched(plugIn, progress.Grasshopper)).Bracket();

    private static IO<Progress> Cased(JsonElement element, int index, ImmutableArray<Expressions> codes, Recorder recorder, Lazy<Fin<ICanvases>> surface, Progress held) =>
        from started in recorder.Start(index, Named(element, index))
        from outcome in (
            from row in IO.lift(() => Optional(element.Deserialize<Case>(Data)).ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"case {index} is null"))))
            from code in IO.lift(() => Compiled(codes[index]))
            from grasshopper in Started(row, surface, held.Grasshopper)
            from executed in Execute(row, code, recorder, held.Previous, row.Precondition.Canvas is null ? None : surface.Value.ToOption())
            select (executed.Outcome, executed.Opened, Grasshopper: grasshopper))
            .Catch((Error error) => IO.pure((Outcome: new Outcome(Named(element, index), new Completion.Failed(Message(error))), Opened: held.Previous, held.Grasshopper))).As()
        from finished in recorder.Finish(outcome.Outcome)
        select new Progress(outcome.Grasshopper, outcome.Opened);

    private static string Named(JsonElement element, int index) =>
        Optional(element.ValueKind is JsonValueKind.Object && element.TryGetProperty("name", out JsonElement name) && name.ValueKind is JsonValueKind.String ? name.GetString() : null)
            .IfNone(() => index.ToString(CultureInfo.InvariantCulture));

    private static IO<Option<Grasshopper>> Started(Case row, Lazy<Fin<ICanvases>> surface, Option<Grasshopper> current) =>
        row.Precondition.Canvas is not null && current.IsNone
            ? IO.lift(() => surface.Value).Bind(static made => IO.lift(() => made.Start())).Map(static made => Some(made))
            : IO.pure(current);

    private static Unit Written(string file, Reply reply) {
        string draft = Path.ChangeExtension(file, ".tmp");
        File.WriteAllText(draft, JsonSerializer.Serialize(reply, Wire.Default.Reply));
        File.Move(draft, file, overwrite: true);
        return unit;
    }

    private static Fin<ICanvases> Surface(Func<ICanvases> canvases) =>
        PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2")) ? Try.lift(canvases).Run() : Error.New("Grasshopper2 plug-in did not load");

    private static Fin<Expressions.Compiled> Compiled(Expressions code) =>
        code.Switch<Fin<Expressions.Compiled>>(
            compiled: static held => held,
            uncompiled: static held => Error.New(held.Reason));

    private static ImmutableArray<Loaded> Plugins() =>
        [.. PlugIn.GetInstalledPlugIns().Keys.Select(PlugIn.GetPlugInInfo).Where(static info => info is { IsLoaded: true }).Select(static info => new Loaded(info.Name, info.Version)).OrderBy(static held => held.Name, StringComparer.Ordinal)];

    // --- [CASES]
    private static IO<(Outcome Outcome, ImmutableArray<uint> Opened)> Execute(Case @case, Expressions.Compiled code, Recorder recorder, ImmutableArray<uint> previous, Option<ICanvases> canvases) =>
        (from open in IO.lift(static () => toSet(RhinoDoc.OpenDocuments().Select(static doc => doc.RuntimeSerialNumber)))
         from settings in IO.lift(Settings.Snapshot)
         from captured in use(static () => new Captured())
         from batch in use(() => @case.Precondition.Batch && !RhinoApp.IsRunningBatchMode && RhinoApp.RunScript("_BatchMode _On", echo: false), static enabled => when(enabled, IO.lift(static () => RhinoApp.RunScript("_BatchMode _Off", echo: false)).Map(static _ => unit)).As())
         from outcome in Attempt(@case, code, recorder, previous, canvases, captured)
             .Catch((Error error) => IO.pure((Outcome: new Outcome(@case.Name, new Completion.Failed(Message(error))), Doc: Option<RhinoDoc>.None))).As()
         from leaked in IO.lift(() => Settings.Changed(settings))
         from current in IO.lift(() => RhinoDoc.OpenDocuments().Select(static doc => doc.RuntimeSerialNumber).Where(serial => !open.Contains(serial)).ToImmutableArray())
         select (outcome.Outcome with { Leaked = [.. outcome.Outcome.Leaked, .. leaked] }, outcome.Doc.IsSome ? current : [.. previous, .. current])).Bracket();

    private static IO<(Outcome Outcome, Option<RhinoDoc> Doc)> Attempt(Case @case, Expressions.Compiled code, Recorder recorder, ImmutableArray<uint> previous, Option<ICanvases> canvases, Captured captured) =>
        from folder in IO.lift(() => Directory.CreateDirectory(@case.Folder).FullName)
        from opened in Document(@case, previous)
        from outcome in Observed(@case, code, recorder, canvases, captured, opened.Doc, folder)
        from reported in IO.lift(() => opened.Doc.Match(Some: held => Reported(held, outcome, Path.Combine(@case.Folder, "after.3dm")), None: () => outcome))
        select (reported with { Leaked = [.. opened.Unclosed, .. reported.Leaked] }, opened.Doc);

    private static IO<Outcome> Observed(Case @case, Expressions.Compiled code, Recorder recorder, Option<ICanvases> canvases, Captured captured, Option<RhinoDoc> doc, string folder) =>
        (from observed in (
             from scope in Prepared(@case, code, canvases, doc, folder)
             from begun in recorder.Begin()
             from logged in IO.lift(() => scope.Canvas.Map(static shown => shown.Canvases.Logged()))
             from outcome in Observe(@case, code, scope)
             from diagnostics in IO.lift(() => (
                 from shown in scope.Canvas
                 from since in logged
                 select shown.Canvases.Diagnostics(since)).IfNone([]))
             select (Outcome: outcome, Diagnostics: diagnostics))
             .Catch((Error error) => recorder.Begin().Map(_ => (Outcome: new Outcome(@case.Name, new Completion.Failed(Message(error))), Diagnostics: ImmutableArray<Diagnosed>.Empty))).As()
         from ended in recorder.End(observed.Outcome, captured.Output, captured.Errors, observed.Diagnostics)
         select ended).Bracket();

    private static IO<Scope> Prepared(Case @case, Expressions.Compiled code, Option<ICanvases> canvases, Option<RhinoDoc> doc, string folder) =>
        from measures in IO.pure(Measures(@case.Precondition.Source))
        from prepared in doc.Traverse(held => Prepare(held, @case.Precondition, measures.Units, measures.Tolerance, Path.Combine(folder, "before.3dm"))).As()
        from active in IO.lift(() => Active(doc, @case))
        from canvas in (
            from source in Optional(@case.Precondition.Canvas)
            from made in canvases
            select (Source: source, Made: made))
            .Traverse(static pair => use(() => Shown(pair.Made, Optional(pair.Source.Open)), static shown => shown.Canvases.Release(shown.Document, shown.Prior))).As()
        from scope in toSeq(@case.Precondition.Add.Zip(code.Geometry))
            .FoldM(new Scope(doc, canvas, @case.Args, folder), static (held, pair) => Add(held, pair.First, pair.Second).Map(id => held with { Made = held.Made.AddOrUpdate(pair.First.As, id) })).As()
        from selected in IO.lift(() => Selected(scope, @case.Precondition.Select))
        from stepped in Recorded(doc, $"{@case.Name} steps", IO.lift(() => code.Steps(scope)))
        select scope;

    private static IO<Outcome> Observe(Case @case, Expressions.Compiled code, Scope scope) =>
        from before in IO.lift(() => Reads(@case.Reads, code.Reads, scope))
        from macros in IO.lift(() => Macros(@case, scope.Document))
        from result in Recorded(scope.Document, @case.Name,
                from ran in Optional(@case.Script).Traverse(script => Run(script, scope.Document)).As()
                from value in Optional(code.Body).Match(Some: body => IO.lift(() => body(scope)).Bind(static held => IO.lift(() => Encode(held))), None: static () => IO.pure<JsonNode?>(value: null))
                select value)
            .Map(static value => (Completion: (Completion)new Completion.Ran(), Value: (JsonNode?)value))
            .Catch(static (Error error) => IO.pure((Completion: Thrown(error), Value: (JsonNode?)null))).As()
        from read in IO.lift(() => Reads(@case.Reads, code.Reads, scope))
        select new Outcome(@case.Name, result.Completion) {
            Value = result.Value,
            Before = Empty(before.Values),
            BeforeFailed = Empty(before.Failures),
            Read = Empty(read.Values),
            ReadFailed = Empty(read.Failures),
            Macros = macros.IsEmpty ? null : macros,
        };

    private static ImmutableArray<Macro> Macros(Case @case, Option<RhinoDoc> doc) =>
        [.. @case.Macros.Select(macro => new Macro(macro, doc.Match(Some: held => RhinoApp.RunScript(held.RuntimeSerialNumber, macro, echo: true), None: () => RhinoApp.RunScript(macro, echo: true))))];

    private static IO<A> Recorded<A>(Option<RhinoDoc> doc, string label, IO<A> work) =>
        doc.Match(
            Some: held =>
                from serial in IO.lift(() => held.BeginUndoRecord(label))
                from value in work.Finally(IO.lift(() => held.EndUndoRecord(serial)))
                select value,
            None: () => work);

    private static IO<Unit> Run(string path, Option<RhinoDoc> doc) =>
        (from source in IO.lift(() => new SourceCode(new Uri(path)))
         from spec in IO.lift(() => Spec(path, source.LanguageSpec))
         from started in IO.lift(() => Languages.Start(spec))
         from code in IO.lift(source.CreateCode)
         from output in use(static () => new MemoryStream())
         from error in use(static () => new MemoryStream())
         from context in IO.lift(() => Contextual(output, error, doc))
         from ran in IO.lift(() => code.Run(context)).Finally(IO.lift(() => Flushed(output, error)))
         select unit).Bracket();

    private static RunContext Contextual(MemoryStream output, MemoryStream error, Option<RhinoDoc> doc) {
        RunContext context = new() { AutoApplyParams = true, OutputStream = output, ErrorStream = error };
        _ = doc.Iter(held => ((ParamValueSet)context.Inputs)["__rhino_doc__"] = held);
        return context;
    }

    private static void Flushed(MemoryStream output, MemoryStream error) {
        Console.Out.Write(Encoding.UTF8.GetString(output.ToArray()));
        Console.Error.Write(Encoding.UTF8.GetString(error.ToArray()));
    }

    private static Fin<LanguageSpec> Spec(string path, LanguageSpec spec) =>
        spec == LanguageSpec.Any ? Error.New($"{path} names no language RhinoCode registers, by extension or #! header") : spec;

    private static Completion Thrown(Error error) => error.Exception.Match<Completion>(
        Some: static exception => exception switch {
            CompileException compile => Compiled(compile),
            ExecuteException execute when execute.Position != TextPosition.None => new Completion.Thrown(Text(execute), execute.Position.LineNumber, execute.Trace.Reference.Uri?.LocalPath),
            _ => new Completion.Thrown(Text(exception)),
        },
        None: () => new Completion.Thrown(error.Message));

    private static Completion.Thrown Compiled(CompileException compile) {
        Option<Diagnostic> faulted = Optional(compile.Diagnosis.FirstOrDefault(static diagnostic => diagnostic is { Severity: DiagnosticSeverity.Error, HasReference: true }));
        return new Completion.Thrown(
            Text(compile, string.Join("; ", compile.Diagnosis.Select(static diagnostic => $"{diagnostic.Id} {diagnostic.Severity} {diagnostic.Reference}: {diagnostic.Message}"))),
            faulted.Map(static diagnostic => diagnostic.Reference.Position.LineNumber).ToNullable(),
            faulted.Bind(static diagnostic => Optional(diagnostic.Reference.Uri?.LocalPath)).ValueUnsafe());
    }

    private static (JsonObject Values, JsonObject Failures) Reads(ImmutableArray<string> keys, ImmutableArray<Func<Scope, object?>> reads, Scope scope) =>
        toSeq(keys.Zip(reads)).Fold((Values: new JsonObject(), Failures: new JsonObject()), (held, read) => {
            _ = Try.lift(() => read.Second(scope)).Run().Bind(Encode).Match(
                Succ: value => held.Values[read.First] = value,
                Fail: error => held.Failures[read.First] = Message(error));
            return held;
        });

    // --- [PRECONDITIONS]
    private static IO<(Option<RhinoDoc> Doc, ImmutableArray<string> Unclosed)> Document(Case @case, ImmutableArray<uint> previous) =>
        @case.Precondition.Source.Switch<ImmutableArray<uint>, IO<(Option<RhinoDoc> Doc, ImmutableArray<string> Unclosed)>>(
            previous,
            template: static (state, template) => Opened(Path.Combine(FileSettings.TemplateFolder, template.Name), state, () => Error.New($"template {template.Name} is absent from {FileSettings.TemplateFolder}, which holds {string.Join(", ", Directory.EnumerateFiles(FileSettings.TemplateFolder, "*.3dm").Select(Path.GetFileName))}")),
            open: static (state, source) => Opened(source.Path, state, () => Error.New($"document {source.Path} does not exist")),
            application: static (_, _) => IO.pure((Doc: Option<RhinoDoc>.None, Unclosed: ImmutableArray<string>.Empty)));

    private static (Option<UnitSystem> Units, Option<Tolerances> Tolerance) Measures(DocumentSource source) =>
        source.Switch<(Option<UnitSystem> Units, Option<Tolerances> Tolerance)>(
            template: static template => (Some(template.Units), Some(template.Tolerance)),
            open: static open => (Optional(open.Units), Optional(open.Tolerance)),
            application: static _ => (None, None));

    private static IO<(Option<RhinoDoc> Doc, ImmutableArray<string> Unclosed)> Opened(string source, ImmutableArray<uint> previous, Func<Error> absent) =>
        from path in IO.lift(() => Present(source, absent))
        from doc in New(path)
        from unclosed in IO.lift(() => toSeq(previous).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Choose(Unclosed).ToImmutableArray())
        select (Doc: Some(doc), Unclosed: unclosed);

    private static Fin<string> Present(string path, Func<Error> absent) => File.Exists(path) ? path : absent();

    private static IO<RhinoDoc> New(string source) =>
        from open in IO.lift(static () => toSet(RhinoDoc.OpenDocuments().Select(static doc => doc.RuntimeSerialNumber)))
        from created in IO.lift(() => RhinoApp.RunScript($"""
            _-New "{source}"
            """, echo: false))
        from doc in IO.lift(() => Optional(RhinoDoc.OpenDocuments().SingleOrDefault(doc => !open.Contains(doc.RuntimeSerialNumber))).ToFin(Error.New($"_-New {source} opened no document, RunScript answered {created}")))
        select doc;

    private static IO<RhinoDoc> Prepare(RhinoDoc doc, Placed placed, Option<UnitSystem> units, Option<Tolerances> tolerance, string file) =>
        from adjusted in IO.lift(() => Adjusted(doc, units, tolerance))
        from shown in Optional(placed.View).Traverse(view => IO.lift(() => Show(doc, view))).As()
        from paged in Optional(placed.Layout).Traverse(page => IO.lift(() => AddPage(doc, page))).As()
        from layered in toSeq(placed.Layers).TraverseM(row => IO.lift(() => Layered(doc, row))).As()
        from written in IO.lift(() => Written(doc, file, adopted: true))
        select doc;

    private static Unit Adjusted(RhinoDoc doc, Option<UnitSystem> units, Option<Tolerances> tolerance) {
        _ = units.Iter(system => doc.AdjustModelUnitSystem(system, scale: false));
        _ = tolerance.Iter(held => {
            doc.ModelAbsoluteTolerance = held.Absolute;
            _ = Optional(held.Angle).Iter(angle => doc.ModelAngleToleranceDegrees = angle);
            _ = Optional(held.Relative).Iter(relative => doc.ModelRelativeTolerance = relative);
        });
        return unit;
    }

    private static Fin<Unit> Written(RhinoDoc doc, string file, bool adopted) {
        using FileWriteOptions options = new() { UpdateDocumentPath = adopted, SuppressDialogBoxes = true, IncludePreviewImage = false };
        if (!doc.WriteFile(file, options)) return Error.New($"document {doc.RuntimeSerialNumber} did not write {file}");
        doc.Modified = doc.Modified && !adopted;
        return unit;
    }

    private static Fin<Unit> Show(RhinoDoc doc, View view) =>
        Optional(doc.Views.ActiveView).ToFin(Error.New($"document {doc.RuntimeSerialNumber} holds no active view")).Bind(active => Framed(active, view));

    private static Fin<Unit> Framed(RhinoView active, View view) {
        RhinoViewport viewport = active.ActiveViewport;
        _ = Optional(view.Projection).Iter(projection => viewport.SetProjection(projection, viewName: null, updateConstructionPlane: true));
        return Optional(view.Display).Traverse(name => Displayed(viewport, name)).As().Map(_ => Aimed(active, view));
    }

    private static Fin<Unit> Displayed(RhinoViewport viewport, string name) {
        using DisplayModeDescription? mode = DisplayModeDescription.FindByName(name);
        if (mode is null) return Error.New($"display mode {name} is not one of {string.Join(", ", DisplayModeDescription.GetDisplayModes().Select(static held => held.EnglishName))}");
        viewport.DisplayMode = mode;
        return unit;
    }

    private static Unit Aimed(RhinoView active, View view) {
        RhinoViewport viewport = active.ActiveViewport;
        _ = Optional(view.Location).Iter(location => viewport.SetCameraLocation(new Point3d(location[0], location[1], location[2]), updateTargetLocation: false));
        _ = Optional(view.Target).Iter(target => viewport.SetCameraTarget(new Point3d(target[0], target[1], target[2]), updateCameraLocation: false));
        _ = Optional(view.Lens).Iter(lens => viewport.Camera35mmLensLength = lens);
        active.Redraw();
        return unit;
    }

    private static Fin<Unit> AddPage(RhinoDoc doc, Page page) {
        _ = Optional(page.Units).Iter(units => doc.AdjustPageUnitSystem(units, scale: false));
        return Optional(doc.Views.AddPageView(page.Name, page.Width, page.Height))
            .ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"layout {page.Name} {page.Width} x {page.Height} was not added")))
            .Map(static _ => unit);
    }

    private static Fin<Unit> Layered(RhinoDoc doc, LayerRow row) {
        int index = doc.Layers.FindByFullPath(row.Path, notFoundReturnValue: -1) is >= 0 and var found ? found : doc.Layers.AddPath(row.Path);
        if (index < 0) return Error.New($"layer {row.Path} was not added");
        Layer layer = doc.Layers[index];
        _ = Optional(row.Color).Iter(color => layer.Color = ColorTranslator.FromHtml(color));
        _ = Optional(row.Locked).Iter(locked => layer.IsLocked = locked);
        return unit;
    }

    private static IO<Guid> Add(Scope scope, Added row, Func<Scope, GeometryBase> geometry) =>
        (from doc in IO.lift(() => scope.Doc)
         from attributes in use(doc.CreateDefaultAttributes)
         from layer in IO.lift(() => Optional(row.Layer).Traverse(path => Index(doc, path)).As())
         from assigned in IO.lift(() => layer.Iter(index => attributes.LayerIndex = index))
         from id in IO.lift(() => Added(doc, row, geometry(scope), attributes))
         select id).Bracket();

    private static Fin<int> Index(RhinoDoc doc, string path) =>
        doc.Layers.FindByFullPath(path, notFoundReturnValue: -1) switch {
            >= 0 and var found => found,
            _ => Error.New($"layer {path} is absent"),
        };

    private static Fin<Guid> Added(RhinoDoc doc, Added row, GeometryBase geometry, ObjectAttributes attributes) =>
        doc.Objects.Add(geometry, attributes) switch {
            var id when id == Guid.Empty => Error.New($"add {row.As} added no object"),
            var id => id,
        };

    private static Fin<Unit> Selected(Scope scope, ImmutableArray<string> names) =>
        toSeq(names).Filter(name => !scope.Made.Find(name).Exists(id => id is Guid guid && scope.Doc.Objects.Select(guid))) is { IsEmpty: false } refused
            ? Error.New($"select left {string.Join(", ", refused)} unselected, locked, hidden, or absent from made")
            : unit;

    private static Fin<Unit> Active(Option<RhinoDoc> doc, Case @case) =>
        doc.Filter(_ => !@case.Macros.IsEmpty || @case.Script is not null).Map(static held => held.RuntimeSerialNumber).Filter(static serial => serial != Foremost()).Match<Fin<Unit>>(
            Some: static serial => Error.New($"document {serial} is not active ({Foremost()}), so its commands cannot pick points"),
            None: static () => unit);

    private static Opened Shown(ICanvases canvases, Option<string> source) {
        (object document, Option<object> prior) = canvases.Open(source);
        return new Opened(canvases, document, prior);
    }

    // --- [DOCUMENTS]
    private static uint Foremost() => RhinoDoc.ActiveDoc?.RuntimeSerialNumber ?? 0;

    private static DocumentSnapshot State(RhinoDoc doc) =>
        new(doc.RuntimeSerialNumber, doc.Path, doc.TemplateFileUsed, new Units(doc.ModelUnitSystem, doc.PageUnitSystem),
            new Tolerances(doc.ModelAbsoluteTolerance, doc.ModelAngleToleranceDegrees, doc.ModelRelativeTolerance),
            Foremost() == doc.RuntimeSerialNumber, doc.Modified, doc.Objects.GetSelectedObjects(includeLights: true, includeGrips: false).Count(),
            doc.Views.ActiveView?.ActiveViewport is { } viewport ? Viewing(viewport) : null,
            [.. doc.GetUndoRecords().Select(static record => record.Description)]);

    private static Viewed Viewing(RhinoViewport viewport) {
        using DisplayModeDescription mode = viewport.DisplayMode;
        return new Viewed(viewport.Name, mode.EnglishName, viewport.IsParallelProjection);
    }

    private static Outcome Reported(RhinoDoc doc, Outcome outcome, string after) {
        Outcome stated = outcome with { Document = State(doc) };
        return Written(doc, after, adopted: false).Match(
            Succ: _ => stated with { Saved = new Saved(doc.Path, after) },
            Fail: error => stated with { Leaked = [.. stated.Leaked, Message(error)] });
    }

    private static Option<string> Unclosed(RhinoDoc doc) {
        doc.Modified = false;
        return RhinoApp.RunScript(doc.RuntimeSerialNumber, string.IsNullOrEmpty(doc.Path) ? "_-Close" : $"""
            _-Close "{doc.Path}"
            """, echo: false) ? Option<string>.None : Some($"document {doc.RuntimeSerialNumber} stayed open, _-Close answered false");
    }

    // --- [PLUG_INS]
    private static PlugInLoad Load(string file) {
        LoadPlugInResult result = PlugIn.LoadPlugIn(file, out Guid id);
        using PEReader reader = new(File.OpenRead(file));
        MetadataReader metadata = reader.GetMetadataReader();
        return new PlugInLoad(file, id, result, PlugIn.Find(id)?.Assembly.ManifestModule.ModuleVersionId.Equals(metadata.GetGuid(metadata.GetModuleDefinition().Mvid)) ?? false);
    }

    // --- [VALUES]
    private static JsonObject? Empty(JsonObject node) => node.Count is 0 ? null : node;
}
