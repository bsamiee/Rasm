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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Xml.Linq;
using AppKit;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
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
using Result = Rhino.Commands.Result;

namespace Rasm.Harnesses.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ICanvases {
    public object Editor { get; }

    public Grasshopper Start();

    public void Stop();

    public (object Document, object? Prior) Open(string? source);

    public void Release(object document, object? prior);

    public int Logged();

    public ImmutableArray<Diagnosed> Diagnostics(int since);

    public JsonObject Solve(object canvas);
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

public sealed record Case(string Name, DocumentSource Source) {
    public ImmutableArray<string> Reads { get; init; } = [];
    public ImmutableArray<string> Macros { get; init; } = [];
    public ImmutableArray<JsonElement> Args { get; init; } = [];
    public string? Language { get; init; }
    public View? View { get; init; }
    public Page? Layout { get; init; }
    public ImmutableArray<LayerRow> Layers { get; init; } = [];
    public ImmutableArray<Added> Add { get; init; } = [];
    public ImmutableArray<string> Select { get; init; } = [];
    public Canvas? Canvas { get; init; }
    public string? Script { get; init; }
    public bool Batch { get; init; }
}

public sealed record Expressions(Action<Scope> Steps, Func<Scope, object?>? Body, ImmutableArray<Func<Scope, object?>> Reads, ImmutableArray<Func<Scope, GeometryBase>> Geometry);

public sealed record Rows(ImmutableArray<JsonElement> Cases, string? Plugin = null);

public sealed class Scope(Option<RhinoDoc> doc, Option<(ICanvases Canvases, object Document)> canvas, ImmutableArray<JsonElement> args, string captures, int index) {
    private int shots;

    public RhinoDoc Doc => doc.IfNone(static () => throw new InvalidOperationException("application case opens no document"));
    public RhinoView? View => Doc.Views.ActiveView;
    public object Canvas => Shown.Document;
    public object Editor => Shown.Canvases.Editor;
    public ImmutableArray<JsonElement> Args => args;
    public IDictionary<string, object?> Made { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    public JsonObject Capture(int width, int height) {
        RhinoView view = View ?? throw new InvalidOperationException("document holds no active view");
        string file = Path.Combine(captures, string.Create(CultureInfo.InvariantCulture, $"{index}-{++shots}.png"));
        using Bitmap bitmap = new ViewCapture { Width = width, Height = height, ScaleScreenItems = false }.CaptureToBitmap(view)
            ?? throw new InvalidOperationException($"view {view.ActiveViewport.Name} captured no bitmap");
        bitmap.Save(file, ImageFormat.Png);
        return new JsonObject { ["file"] = file, ["sha256"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(file))) };
    }

    public JsonObject Solve() => Shown.Canvases.Solve(Shown.Document);

    private (ICanvases Canvases, object Document) Shown => canvas.IfNone(static () => throw new InvalidOperationException("precondition opens no canvas"));
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
internal sealed record Executed(string Name, string? Plugin, Result Result, uint Document);
internal sealed record Prompt(string Text, string Default, ImmutableArray<string> Options);
internal sealed record Streams(string Stdout, string Stderr);
public sealed record Diagnosed(DateTime Time, string Kind, string Message);
internal sealed record Write(string Kind, string Subject, string? Made = null, JsonNode? Archive = null);
internal sealed record Units(UnitSystem Model, UnitSystem Page);
internal sealed record Saved(string Before, string After);
internal sealed record Viewed(string Name, string Display, bool Parallel);
internal sealed record DocumentState(uint Serial, string? Path, string? Template, Units Units, Tolerances Tolerance, bool Active, bool Modified, int Selected, Viewed? View, ImmutableArray<string> Undo);

internal sealed record Outcome(string Name, Completion Completion) {
    public JsonNode? Value { get; init; }
    public JsonObject? Before { get; init; }
    public JsonObject? BeforeFailed { get; init; }
    public JsonObject? Read { get; init; }
    public JsonObject? ReadFailed { get; init; }
    public ImmutableArray<string> Leaked { get; init; } = [];
    public ImmutableArray<string>? Restored { get; init; }
    public DocumentState? Document { get; init; }
    public Saved? Saved { get; init; }
    public ImmutableArray<Macro>? Macros { get; init; }
    public ImmutableArray<Executed>? Commands { get; init; }
    public ImmutableArray<Prompt>? Prompts { get; init; }
    public ImmutableArray<string>? Printed { get; init; }
    public Streams? Console { get; init; }
    public ImmutableArray<string>? Reported { get; init; }
    public ImmutableArray<string>? Logged { get; init; }
    public ImmutableArray<Write>? Writes { get; init; }
    public ImmutableArray<Diagnosed>? Diagnostics { get; init; }
}

internal sealed record PlugInLoad(string File, Guid Id, LoadPlugInResult Result, bool Current);
internal sealed record Loaded(string Name, string Version);
public sealed record Grasshopper(string Version, ImmutableArray<string> Loaded, ImmutableArray<string> Failed, string? Incomplete);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_tag")]
[JsonDerivedType(typeof(Session), nameof(Session))]
[JsonDerivedType(typeof(Started), nameof(Started))]
[JsonDerivedType(typeof(Began), nameof(Began))]
[JsonDerivedType(typeof(Ended), nameof(Ended))]
[JsonDerivedType(typeof(Prompted), nameof(Prompted))]
[JsonDerivedType(typeof(Reported), nameof(Reported))]
[JsonDerivedType(typeof(Unhandled), nameof(Unhandled))]
[JsonDerivedType(typeof(Finished), nameof(Finished))]
[JsonDerivedType(typeof(Closed), nameof(Closed))]
internal abstract record JournalEntry;
internal sealed record Session(int Language, string Culture, string Version, string Runtime, string System, string Logs, PlugInLoad? PlugIn) : JournalEntry;
internal sealed record Started(string Name) : JournalEntry;
internal sealed record Began(string Name) : JournalEntry;
internal sealed record Ended(string Name) : JournalEntry;
internal sealed record Prompted(string Text, string Default, ImmutableArray<string> Options) : JournalEntry;
internal sealed record Reported(string Source, string Exception) : JournalEntry;
internal sealed record Unhandled(string Exception) : JournalEntry;
internal sealed record Finished(Outcome Outcome, Grasshopper? Grasshopper) : JournalEntry;
internal sealed record Closed(ImmutableArray<Loaded> Plugins) : JournalEntry;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, UseStringEnumConverter = true, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(JournalEntry))]
internal sealed partial class Wire : JsonSerializerContext;

internal sealed class Special<T> : JsonConverter<T> where T : struct, IBinaryFloatingPointIeee754<T> {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        T.CreateChecked(reader.TokenType is JsonTokenType.StartObject
            ? JsonElement.ParseValue(ref reader).GetProperty("$").GetString() switch {
                "NaN" => double.NaN,
                "Infinity" => double.PositiveInfinity,
                "-Infinity" => double.NegativeInfinity,
                "-0" => -0.0,
                var tag => throw new JsonException($"{tag} tags no IEEE 754 special value"),
            }
            : reader.GetDouble());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        double number = double.CreateChecked(value);
        string? tag = number switch {
            double.NaN => "NaN",
            double.PositiveInfinity => "Infinity",
            double.NegativeInfinity => "-Infinity",
            0 when double.IsNegative(number) => "-0",
            _ => null,
        };
        if (tag is null) {
            writer.WriteNumberValue(number);
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("$", tag);
        writer.WriteEndObject();
    }
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

internal sealed class Recorder(Journal journal) {
    private readonly List<Executed> commands = [];
    private readonly List<Prompt> prompts = [];
    private readonly List<string> reports = [];
    private readonly List<string> logged = [];
    private readonly List<Write> writes = [];
    private Option<Scope> observed;

    // --- [LIFECYCLE]
    public void Attach() {
        AppDomain.CurrentDomain.UnhandledException += Unhandled;
        HostUtils.OnExceptionReport += Reporting;
        Command.BeginCommand += Beginning;
        Command.EndCommand += Ending;
        RhinoApp.CommandPromptChanged += Prompting;
        RhinoCode.Logger.MessageLogged += Logging;
        RhinoDoc.AddRhinoObject += Adding;
        RhinoDoc.DeleteRhinoObject += Deleting;
        RhinoDoc.ReplaceRhinoObject += Replacing;
        RhinoDoc.ModifyObjectAttributes += Modifying;
        RhinoDoc.LayerTableEvent += Layering;
        RhinoDoc.MaterialTableEvent += Materials;
    }

    public void Detach() {
        RhinoDoc.MaterialTableEvent -= Materials;
        RhinoDoc.LayerTableEvent -= Layering;
        RhinoDoc.ModifyObjectAttributes -= Modifying;
        RhinoDoc.ReplaceRhinoObject -= Replacing;
        RhinoDoc.DeleteRhinoObject -= Deleting;
        RhinoDoc.AddRhinoObject -= Adding;
        RhinoCode.Logger.MessageLogged -= Logging;
        RhinoApp.CommandPromptChanged -= Prompting;
        Command.EndCommand -= Ending;
        Command.BeginCommand -= Beginning;
        HostUtils.OnExceptionReport -= Reporting;
        AppDomain.CurrentDomain.UnhandledException -= Unhandled;
    }

    public void Begin() {
        _ = RhinoApp.CapturedCommandWindowStrings(clearBuffer: true);
        commands.Clear();
        prompts.Clear();
        reports.Clear();
        logged.Clear();
        writes.Clear();
        RhinoApp.CommandWindowCaptureEnabled = true;
    }

    public A Observing<A>(Scope scope, Func<A> work) {
        observed = Some(scope);
        try {
            return work();
        } finally {
            observed = None;
        }
    }

    public Outcome End(Outcome outcome) {
        string[] printed = RhinoApp.CapturedCommandWindowStrings(clearBuffer: true);
        RhinoApp.CommandWindowCaptureEnabled = false;
        return outcome with {
            Commands = commands is [] ? null : [.. commands],
            Prompts = prompts is [] ? null : [.. prompts],
            Printed = printed is [] ? null : [.. printed],
            Reported = reports is [] ? null : [.. reports],
            Logged = logged is [] ? null : [.. logged],
            Writes = writes is [] ? null : [.. writes],
        };
    }

    // --- [HANDLERS]
    private void Beginning(object? sender, CommandEventArgs args) => journal.Write(new Began(args.CommandEnglishName));

    private void Ending(object? sender, CommandEventArgs args) {
        commands.Add(new Executed(args.CommandEnglishName, args.CommandPluginName, args.CommandResult, args.DocumentRuntimeSerialNumber));
        journal.Write(new Ended(args.CommandEnglishName));
    }

    private void Prompting(object? sender, CommandPromptChangedEventArgs args) {
        if (string.IsNullOrEmpty(args.Prompt)) return;
        Prompt prompt = new(args.Prompt, args.PromptDefault ?? "", args.Options is { } options ? [.. options.Select(static option => option.EnglishName)] : []);
        prompts.Add(prompt);
        journal.Write(new Prompted(prompt.Text, prompt.Default, prompt.Options));
    }

    private void Reporting(string source, Exception exception) {
        reports.Add($"{source}: {Engine.Text(exception)}");
        journal.Write(new Reported(source, Engine.Text(exception)));
    }

    private void Unhandled(object? sender, UnhandledExceptionEventArgs args) =>
        journal.Write(new Unhandled(args.ExceptionObject is Exception exception ? exception.ToString() : args.ExceptionObject.GetType().FullName ?? ""));

    private void Logging(ILogger logger, LoggerWriteEventArgs args) {
        if (args.Level >= LogLevel.Warn) logged.Add($"{args.Level}: {args.Message}");
    }

    private void Adding(object? sender, RhinoObjectEventArgs args) => Wrote("add", args.ObjectId, args.TheObject.Geometry);

    private void Deleting(object? sender, RhinoObjectEventArgs args) => Wrote("delete", args.ObjectId, geometry: null);

    private void Replacing(object? sender, RhinoReplaceObjectEventArgs args) => Wrote("replace", args.ObjectId, args.NewRhinoObject.Geometry);

    private void Modifying(object? sender, RhinoModifyObjectAttributesEventArgs args) => Wrote("attributes", args.RhinoObject.Id, geometry: null);

    private void Layering(object? sender, LayerTableEventArgs args) =>
        observed.Iter(_ => writes.Add(new Write($"layer {args.EventType}", args.NewState?.FullPath ?? args.OldState?.FullPath ?? args.LayerIndex.ToString(CultureInfo.InvariantCulture))));

    private void Materials(object? sender, MaterialTableEventArgs args) =>
        observed.Iter(_ => writes.Add(new Write($"material {args.EventType}", args.Index.ToString(CultureInfo.InvariantCulture))));

    private void Wrote(string kind, Guid id, GeometryBase? geometry) =>
        observed.Iter(scope => writes.Add(new Write(kind, id.ToString(), scope.Made.FirstOrDefault(pair => pair.Value is Guid made && made == id).Key,
            geometry is null ? null : JsonNode.Parse(geometry.ToJSON(new SerializationOptions())))));
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
    private static readonly ImmutableArray<(string Name, MethodInfo Current, MethodInfo Update)> Pairs = [
        .. ImmutableArray.Create(
                typeof(AppearanceSettings), typeof(ChooseOneObjectSettings), typeof(CursorTooltipSettings), typeof(FileSettings), typeof(GeneralSettings), typeof(GumballSettings),
                typeof(ModelAidSettings), typeof(OpenGLSettings), typeof(SelectionFilterSettings), typeof(SmartTrackSettings), typeof(SoftTransformSettings), typeof(ViewSettings))
            .Select(static type => type.GetMethod(nameof(ModelAidSettings.GetCurrentState), BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes))
            .OfType<MethodInfo>()
            .Select(static current => (current, update: current.DeclaringType?.GetMethod(nameof(ModelAidSettings.UpdateFromState), BindingFlags.Public | BindingFlags.Static, [current.ReturnType])))
            .Where(static pair => pair.update is not null)
            .Select(static pair => (pair.current.DeclaringType?.Name ?? pair.current.ReturnType.Name, pair.current, pair.update!)),
    ];

    public static ImmutableArray<(int Index, object State, string Text)> Snapshot() =>
        [.. Pairs.Select(static (pair, index) => pair.Current.Invoke(obj: null, parameters: null) is { } state ? (index, state, JsonSerializer.Serialize(state, state.GetType(), Engine.Data)) : throw new InvalidOperationException($"{pair.Name} answered no state"))];

    public static (ImmutableArray<string> Restored, ImmutableArray<string> Leaked) Restore(ImmutableArray<(int Index, object State, string Text)> before) {
        ImmutableArray<(string Name, bool Held)> changed = [
            .. before.Zip(Snapshot()).Where(static pair => !string.Equals(pair.First.Text, pair.Second.Text, StringComparison.Ordinal)).Select(static pair => {
                (string name, _, MethodInfo update) = Pairs[pair.First.Index];
                _ = update.Invoke(null, [pair.First.State]);
                return (name, string.Equals(Snapshot()[pair.First.Index].Text, pair.First.Text, StringComparison.Ordinal));
            }),
        ];
        return ([.. changed.Where(static pair => pair.Held).Select(static pair => pair.Name)], [.. changed.Where(static pair => !pair.Held).Select(static pair => pair.Name)]);
    }
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
    private static readonly JsonSerializerOptions Codec = new(JsonSerializerDefaults.Web) {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new Special<double>(), new Special<float>(), new JsonStringEnumConverter(allowIntegerValues: false), new Native(), new Described(), new Refused() },
    };

    internal static readonly JsonSerializerOptions Data = new(JsonSerializerDefaults.Web) {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new Special<double>(), new Special<float>(), new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    // --- [BATCH]
    public static void Run(string[] args, ImmutableArray<Expressions> codes, Func<ICanvases>? canvases = null) {
        (string stream, string captures, Rows batch) = (args[0], Directory.CreateDirectory(args[1]).FullName, JsonSerializer.Deserialize<Rows>(File.ReadAllText(args[2]), Data) ?? throw new JsonException($"{args[2]} holds no batch"));
        using Journal journal = new(stream);
        using FileSystemWatcher cancels = new(Path.GetDirectoryName(Path.GetFullPath(stream)) ?? Directory.GetCurrentDirectory(), $"{Path.GetFileName(stream)}.cancel") { EnableRaisingEvents = true };
        cancels.Created += Cancelled;
        cancels.Changed += Cancelled;
        NSApplication.SharedApplication.Hide(NSApplication.SharedApplication);
        Recorder recorder = new(journal);
        Lazy<Fin<ICanvases>> surface = new(() => Surface(canvases));
        recorder.Attach();
        try {
            journal.Write(new Session(Localization.CurrentLanguageId, CultureInfo.GetCultureInfo(Localization.CurrentLanguageId).Name, RhinoApp.Version.ToString(), RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription, RhinoCode.LogsDirectory, batch.Plugin is null ? null : Load(batch.Plugin)));
            _ = toSeq(args[3..]).Map(static index => int.Parse(index, CultureInfo.InvariantCulture)).Fold((Editing: false, Previous: ImmutableArray<uint>.Empty), (held, index) => {
                Fin<Case> decoded = Lift(() => batch.Cases[index].Deserialize<Case>(Data) ?? throw new JsonException(string.Create(CultureInfo.InvariantCulture, $"case {index} is null")));
                journal.Write(new Started(decoded.Match(Succ: static row => row.Name, Fail: _ => index.ToString(CultureInfo.InvariantCulture))));
                Fin<Grasshopper?> started = decoded.Bind(row => row.Canvas is not null && !held.Editing ? surface.Value.Bind(static made => Lift<Grasshopper?>(made.Start)) : Fin.Succ<Grasshopper?>(value: null));
                (Outcome outcome, ImmutableArray<uint> opened) = decoded.Bind(row => started.Map(_ => row)).Match(
                    Succ: row => Execute(row, codes[index], index, captures, recorder, held.Previous, row.Canvas is null ? None : surface.Value.ToOption()),
                    Fail: error => (new Outcome(decoded.Match(Succ: static row => row.Name, Fail: _ => index.ToString(CultureInfo.InvariantCulture)), new Completion.Failed(Message(error))), held.Previous));
                journal.Write(new Finished(outcome, started.IfFail(static _ => null)));
                return (held.Editing || started.Exists(static loaded => loaded is not null), opened);
            });
        } finally {
            _ = surface.IsValueCreated ? surface.Value.Iter(static held => held.Stop()) : unit;
            recorder.Detach();
            journal.Write(new Closed([
                .. PlugIn.GetInstalledPlugIns().Keys.Select(PlugIn.GetPlugInInfo).Where(static info => info is { IsLoaded: true }).Select(static info => new Loaded(info.Name, info.Version)).OrderBy(static held => held.Name, StringComparer.Ordinal),
            ]));
        }
    }

    public static object? Call(Delegate function, ImmutableArray<JsonElement> args) =>
        function.Method.Invoke(
            function.Target,
            BindingFlags.DoNotWrapExceptions,
            binder: null,
            [.. function.Method.GetParameters().Select((parameter, index) => args[index].Deserialize(parameter.ParameterType, Codec))],
            CultureInfo.InvariantCulture);

    public static Fin<JsonNode?> Encode(object? value) =>
        Lift(() => value is null ? null : JsonSerializer.SerializeToNode(value, value.GetType(), Codec))
            .MapFail(error => Error.New($"value of {value?.GetType().FullName} did not encode: {Message(error)}"));

    public static string Text(Exception exception) => string.Join(' ', [
        $"{exception.GetType().FullName}: {exception.Message}",
        .. (exception.StackTrace ?? "").Split(System.Environment.NewLine, 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(1),
        .. Optional(exception.InnerException).ToSeq().Map(static inner => $"> {Text(inner)}"),
    ]);

    public static string Message(Error error) => error.Exception.Match(Some: Text, None: () => error.Message);

    private static void Cancelled(object sender, FileSystemEventArgs args) {
        (double time, nint window, short subtype, nint data, NSEventModifierMask flags) = (0, 0, 0, 0, default);
        RhinoApp.PostCancelEvent(Foremost());
        NSApplication.SharedApplication.PostEvent(NSEvent.OtherEvent(NSEventType.ApplicationDefined, CoreGraphics.CGPoint.Empty, flags, time, window, context: null, subtype, data, data), atStart: true);
    }

    private static Fin<ICanvases> Surface(Func<ICanvases>? canvases) =>
        canvases is null ? Error.New("batch built without a canvas case references no Grasshopper 2")
        : PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2")) ? Lift(canvases) : Error.New("Grasshopper2 plug-in did not load");

    // --- [CASES]
    private static (Outcome Outcome, ImmutableArray<uint> Opened) Execute(Case @case, Expressions code, int index, string captures, Recorder recorder, ImmutableArray<uint> previous, Option<ICanvases> canvases) {
        uint[] open = [.. RhinoDoc.OpenDocuments().Select(static doc => doc.RuntimeSerialNumber)];
        ImmutableArray<(int Index, object State, string Text)> settings = Settings.Snapshot();
        Option<RhinoDoc> held = None;
        Option<(ICanvases Canvases, object Document, object? Prior)> canvas = None;
        TextWriter[] streams = [Console.Out, Console.Error];
        using StringWriter stdout = new(CultureInfo.InvariantCulture);
        using StringWriter stderr = new(CultureInfo.InvariantCulture);
        Console.SetOut(stdout);
        Console.SetError(stderr);
        bool batch = @case.Batch && !RhinoApp.IsRunningBatchMode && RhinoApp.RunScript("_BatchMode _On", echo: false);
        Outcome recorded = new(@case.Name, new Completion.Failed("case did not finish"));
        try {
            Fin<Scope> prepared =
                from language in Spoken(@case.Language)
                from doc in Document(@case, Path.Combine(captures, string.Create(CultureInfo.InvariantCulture, $"{index}.3dm")), previous, made => held = made)
                from active in Active(doc, @case)
                from opened in Lift(() => Optional(@case.Canvas).Bind(source => canvases.Map(held => Shown(held, source.Open))))
                from scope in Lift(() => {
                    canvas = opened;
                    Scope made = new(doc, opened.Map(static held => (held.Canvases, held.Document)), @case.Args, captures, index);
                    _ = toSeq(@case.Add.Zip(code.Geometry)).Iter(pair => made.Made[pair.First.As] = Add(made, pair.First, pair.Second));
                    return made;
                })
                from selected in Selected(scope, @case.Select)
                from stepped in Recorded(doc, $"{@case.Name} steps", () => {
                    code.Steps(scope);
                    return Fin.Succ(unit);
                })
                select scope;
            recorder.Begin();
            Option<int> logged = canvas.Map(static held => held.Canvases.Logged());
            Outcome outcome = prepared.Match(Succ: scope => Observe(@case, code, scope, recorder), Fail: error => new Outcome(@case.Name, new Completion.Failed(Message(error))));
            recorded = recorder.End(outcome) with {
                Console = stdout.ToString() is "" && stderr.ToString() is "" ? null : new Streams(stdout.ToString(), stderr.ToString()),
                Diagnostics = canvas.Bind(held => logged.Map(held.Canvases.Diagnostics)).Filter(static entries => !entries.IsEmpty).Map<ImmutableArray<Diagnosed>?>(static entries => entries).ValueUnsafe(),
            };
            recorded = held.Match(Some: doc => recorded with { Document = State(doc), Saved = Save(doc, Path.Combine(captures, string.Create(CultureInfo.InvariantCulture, $"{index}.after.3dm"))) }, None: () => recorded);
        } finally {
            Console.SetOut(streams[0]);
            Console.SetError(streams[1]);
            _ = batch && RhinoApp.RunScript("_BatchMode _Off", echo: false);
            _ = canvas.Iter(static held => held.Canvases.Release(held.Document, held.Prior));
            (ImmutableArray<string> restored, ImmutableArray<string> leaked) = Settings.Restore(settings);
            recorded = recorded with { Restored = restored.IsEmpty ? null : restored, Leaked = leaked };
        }
        ImmutableArray<uint> current = [.. RhinoDoc.OpenDocuments().Select(static doc => doc.RuntimeSerialNumber).Where(serial => !open.Contains(serial))];
        return (recorded, held.IsSome ? current : [.. previous, .. current]);
    }

    private static Outcome Observe(Case @case, Expressions code, Scope scope, Recorder recorder) {
        Option<RhinoDoc> doc = Optional(@case.Source is DocumentSource.Application ? null : scope.Doc);
        (JsonObject before, JsonObject beforeFailed) = Reads(@case.Reads, code.Reads, scope);
        (ImmutableArray<Macro> macros, Fin<JsonNode?> result) = recorder.Observing<(ImmutableArray<Macro>, Fin<JsonNode?>)>(scope, () => (
            [.. @case.Macros.Select(macro => new Macro(macro, doc.Match(Some: held => RhinoApp.RunScript(held.RuntimeSerialNumber, macro, echo: true), None: () => RhinoApp.RunScript(macro, echo: true))))],
            Recorded(doc, @case.Name, () =>
                from ran in Optional(@case.Script).Match(Some: script => Run(script, doc), None: static () => unit)
                from value in Optional(code.Body).Match(Some: body => Lift(() => body(scope)).Bind(Encode), None: static () => Fin.Succ<JsonNode?>(value: null))
                select value)));
        (JsonObject read, JsonObject readFailed) = Reads(@case.Reads, code.Reads, scope);
        return new Outcome(@case.Name, result.Match<Completion>(Succ: static _ => new Completion.Ran(), Fail: Thrown)) {
            Value = result.IfFail(static _ => null),
            Before = Empty(before),
            BeforeFailed = Empty(beforeFailed),
            Read = Empty(read),
            ReadFailed = Empty(readFailed),
            Macros = macros.IsEmpty ? null : macros,
        };
    }

    private static Fin<A> Recorded<A>(Option<RhinoDoc> doc, string label, Func<Fin<A>> work) {
        Option<(RhinoDoc Doc, uint Serial)> record = doc.Map(held => (held, held.BeginUndoRecord(label)));
        try {
            return Lift(work).Bind(static inner => inner);
        } finally {
            _ = record.Iter(static pair => pair.Doc.EndUndoRecord(pair.Serial));
        }
    }

    private static Fin<Unit> Run(string path, Option<RhinoDoc> doc) =>
        from source in Lift(() => new SourceCode(new Uri(path)))
        from spec in Spec(path, source.LanguageSpec)
        from started in Lift(() => Languages.Start(spec))
        from code in Lift(source.CreateCode)
        from ran in Lift(() => {
            using MemoryStream output = new();
            using MemoryStream error = new();
            RunContext context = new() { AutoApplyParams = true, OutputStream = output, ErrorStream = error };
            _ = doc.Iter(held => ((ParamValueSet)context.Inputs)["__rhino_doc__"] = held);
            try {
                code.Run(context);
            } finally {
                Console.Out.Write(Encoding.UTF8.GetString(output.ToArray()));
                Console.Error.Write(Encoding.UTF8.GetString(error.ToArray()));
            }
            return unit;
        })
        select ran;

    private static Fin<LanguageSpec> Spec(string path, LanguageSpec spec) =>
        spec == LanguageSpec.Any ? Error.New($"{path} names no language RhinoCode registers, by extension or #! header") : spec;

    private static Completion Thrown(Error error) => error.Exception.Match<Completion>(
        Some: static exception => exception switch {
            CompileException compile => new Completion.Thrown(
                $"{compile.GetType().FullName}: {string.Join("; ", compile.Diagnosis.Select(static diagnostic => $"{diagnostic.Id} {diagnostic.Severity} {diagnostic.Reference}: {diagnostic.Message}"))}",
                compile.Diagnosis.FirstOrDefault(static diagnostic => diagnostic is { Severity: DiagnosticSeverity.Error, HasReference: true })?.Reference.Position.LineNumber,
                compile.Diagnosis.FirstOrDefault(static diagnostic => diagnostic is { Severity: DiagnosticSeverity.Error, HasReference: true })?.Reference.Uri?.LocalPath),
            ExecuteException execute when execute.Position != TextPosition.None => new Completion.Thrown(Text(execute), execute.Position.LineNumber, execute.Trace.Reference.Uri?.LocalPath),
            _ => new Completion.Thrown(Text(exception)),
        },
        None: () => new Completion.Thrown(error.Message));

    private static (JsonObject Values, JsonObject Failures) Reads(ImmutableArray<string> keys, ImmutableArray<Func<Scope, object?>> reads, Scope scope) =>
        toSeq(keys.Zip(reads)).Fold((Values: new JsonObject(), Failures: new JsonObject()), (held, read) => {
            _ = Lift(() => read.Second(scope)).Bind(Encode).Match(
                Succ: value => held.Values[read.First] = value,
                Fail: error => held.Failures[read.First] = Message(error));
            return held;
        });

    // --- [PRECONDITIONS]
    private static Fin<Unit> Spoken(string? language) =>
        Optional(language).Filter(static code => !Ancestry(CultureInfo.GetCultureInfo(Localization.CurrentLanguageId)).Exists(held => string.Equals(held.Name, code, StringComparison.OrdinalIgnoreCase))).Match<Fin<Unit>>(
            Some: static code => Error.New(string.Create(CultureInfo.InvariantCulture, $"instance runs language {Localization.CurrentLanguageId} ({CultureInfo.GetCultureInfo(Localization.CurrentLanguageId).Name}), not {code}, set Options/Appearance/LanguageIdentifier and relaunch")),
            None: static () => unit);

    private static Seq<CultureInfo> Ancestry(CultureInfo culture) =>
        culture.Equals(CultureInfo.InvariantCulture) ? [] : culture.Cons(Ancestry(culture.Parent));

    private static Fin<Option<RhinoDoc>> Document(Case @case, string file, ImmutableArray<uint> previous, Action<Option<RhinoDoc>> hold) =>
        @case.Source.Switch(
            (@case, file, previous, hold),
            template: static (state, template) => Path.Combine(FileSettings.TemplateFolder, template.Name) switch {
                var path when File.Exists(path) => Prepared(state.@case, path, template.Units, template.Tolerance, state.file, state.previous, state.hold),
                _ => Error.New($"template {template.Name} is absent from {FileSettings.TemplateFolder}, which holds {string.Join(", ", Directory.EnumerateFiles(FileSettings.TemplateFolder, "*.3dm").Select(Path.GetFileName))}"),
            },
            open: static (state, source) => File.Exists(source.Path) ? Prepared(state.@case, source.Path, source.Units, source.Tolerance, state.file, state.previous, state.hold) : Error.New($"document {source.Path} does not exist"),
            application: static (_, _) => Option<RhinoDoc>.None);

    private static Fin<Option<RhinoDoc>> Prepared(Case @case, string source, UnitSystem? units, Tolerances? tolerance, string file, ImmutableArray<uint> previous, Action<Option<RhinoDoc>> hold) =>
        from doc in Lift(() => New(source))
        from held in Lift(() => {
            hold(Some(doc));
            _ = toSeq(previous).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Iter(Close);
            return doc;
        })
        from written in Lift(() => Prepare(held, @case, units, tolerance, file))
        select Some(written);

    private static RhinoDoc New(string source) {
        uint[] open = [.. RhinoDoc.OpenDocuments().Select(static doc => doc.RuntimeSerialNumber)];
        bool created = RhinoApp.RunScript($"_-New \"{source}\"", echo: false);
        return RhinoDoc.OpenDocuments().SingleOrDefault(doc => !open.Contains(doc.RuntimeSerialNumber))
            ?? throw new InvalidOperationException($"_-New {source} opened no document, RunScript answered {created}");
    }

    private static RhinoDoc Prepare(RhinoDoc doc, Case @case, UnitSystem? units, Tolerances? tolerance, string file) {
        _ = Optional(units).Iter(unit => doc.AdjustModelUnitSystem(unit, scale: false));
        _ = Optional(tolerance).Iter(held => {
            doc.ModelAbsoluteTolerance = held.Absolute;
            _ = Optional(held.Angle).Iter(angle => doc.ModelAngleToleranceDegrees = angle);
            _ = Optional(held.Relative).Iter(relative => doc.ModelRelativeTolerance = relative);
        });
        _ = Optional(@case.View).Iter(view => Show(doc, view));
        _ = Optional(@case.Layout).Iter(page => AddPage(doc, page));
        _ = toSeq(@case.Layers).Iter(row => Layered(doc, row));
        using FileWriteOptions options = new() { UpdateDocumentPath = true, SuppressDialogBoxes = true, IncludePreviewImage = false };
        _ = doc.WriteFile(file, options) ? unit : throw new IOException($"document {doc.RuntimeSerialNumber} did not write {file}");
        doc.Modified = false;
        return doc;
    }

    private static void Show(RhinoDoc doc, View view) {
        RhinoView active = doc.Views.ActiveView ?? throw new InvalidOperationException($"document {doc.RuntimeSerialNumber} holds no active view");
        RhinoViewport viewport = active.ActiveViewport;
        _ = Optional(view.Projection).Iter(projection => viewport.SetProjection(projection, viewName: null, updateConstructionPlane: true));
        _ = Optional(view.Display).Iter(name => {
            using DisplayModeDescription mode = DisplayModeDescription.FindByName(name)
                ?? throw new InvalidOperationException($"display mode {name} is not one of {string.Join(", ", DisplayModeDescription.GetDisplayModes().Select(static held => held.EnglishName))}");
            viewport.DisplayMode = mode;
        });
        _ = Optional(view.Location).Iter(location => viewport.SetCameraLocation(new Point3d(location[0], location[1], location[2]), updateTargetLocation: false));
        _ = Optional(view.Target).Iter(target => viewport.SetCameraTarget(new Point3d(target[0], target[1], target[2]), updateCameraLocation: false));
        _ = Optional(view.Lens).Iter(lens => viewport.Camera35mmLensLength = lens);
        active.Redraw();
    }

    private static void AddPage(RhinoDoc doc, Page page) {
        _ = Optional(page.Units).Iter(units => doc.AdjustPageUnitSystem(units, scale: false));
        _ = doc.Views.AddPageView(page.Name, page.Width, page.Height)
            ?? throw new InvalidOperationException(string.Create(CultureInfo.InvariantCulture, $"layout {page.Name} {page.Width} x {page.Height} was not added"));
    }

    private static int Layered(RhinoDoc doc, string path) =>
        (doc.Layers.FindByFullPath(path, notFoundReturnValue: -1), path.LastIndexOf(ModelComponent.NamePathSeparator, StringComparison.Ordinal)) switch {
            ( >= 0 and var found, _) => found,
            (_, var cut) => doc.Layers.Add(new Layer {
                Name = cut < 0 ? path : path[(cut + ModelComponent.NamePathSeparator.Length)..],
                ParentLayerId = cut < 0 ? Guid.Empty : doc.Layers[Layered(doc, path[..cut])].Id,
            }) switch {
                >= 0 and var made => made,
                _ => throw new InvalidOperationException($"layer {path} was not added"),
            },
        };

    private static void Layered(RhinoDoc doc, LayerRow row) {
        Layer layer = doc.Layers[Layered(doc, row.Path)];
        _ = Optional(row.Color).Iter(color => layer.Color = ColorTranslator.FromHtml(color));
        _ = Optional(row.Locked).Iter(locked => layer.IsLocked = locked);
    }

    private static Guid Add(Scope scope, Added row, Func<Scope, GeometryBase> geometry) {
        RhinoDoc doc = scope.Doc;
        ObjectAttributes attributes = doc.CreateDefaultAttributes();
        _ = Optional(row.Layer).Iter(path => attributes.LayerIndex = doc.Layers.FindByFullPath(path, notFoundReturnValue: -1) is >= 0 and var found ? found : throw new InvalidOperationException($"layer {path} is absent"));
        Guid id = doc.Objects.Add(geometry(scope), attributes);
        return id == Guid.Empty ? throw new InvalidOperationException($"add {row.As} added no object") : id;
    }

    private static Fin<Unit> Selected(Scope scope, ImmutableArray<string> names) =>
        toSeq(names).Filter(name => !(scope.Made.TryGetValue(name, out object? id) && id is Guid guid && scope.Doc.Objects.Select(guid))) is { IsEmpty: false } refused
            ? Error.New($"select left {string.Join(", ", refused)} unselected, locked, hidden, or absent from made")
            : unit;

    private static Fin<Unit> Active(Option<RhinoDoc> doc, Case @case) =>
        doc.Filter(_ => !@case.Macros.IsEmpty || @case.Script is not null).Map(static held => held.RuntimeSerialNumber).Filter(static serial => serial != Foremost()).Match<Fin<Unit>>(
            Some: static serial => Error.New($"document {serial} is not active ({Foremost()}), so its commands cannot pick points"),
            None: static () => unit);

    private static (ICanvases Canvases, object Document, object? Prior) Shown(ICanvases canvases, string? source) {
        (object document, object? prior) = canvases.Open(source);
        return (canvases, document, prior);
    }

    // --- [DOCUMENTS]
    private static uint Foremost() => RhinoDoc.ActiveDoc?.RuntimeSerialNumber ?? 0;

    private static DocumentState State(RhinoDoc doc) =>
        new(doc.RuntimeSerialNumber, doc.Path, doc.TemplateFileUsed, new Units(doc.ModelUnitSystem, doc.PageUnitSystem),
            new Tolerances(doc.ModelAbsoluteTolerance, doc.ModelAngleToleranceDegrees, doc.ModelRelativeTolerance),
            Foremost() == doc.RuntimeSerialNumber, doc.Modified, doc.Objects.GetSelectedObjects(includeLights: true, includeGrips: false).Count(),
            doc.Views.ActiveView?.ActiveViewport is { } viewport ? Viewing(viewport) : null,
            [.. doc.GetUndoRecords().Select(static record => record.Description)]);

    private static Viewed Viewing(RhinoViewport viewport) {
        using DisplayModeDescription mode = viewport.DisplayMode;
        return new Viewed(viewport.Name, mode.EnglishName, viewport.IsParallelProjection);
    }

    private static Saved? Save(RhinoDoc doc, string after) {
        using FileWriteOptions options = new() { UpdateDocumentPath = false, SuppressDialogBoxes = true, IncludePreviewImage = false };
        return doc.WriteFile(after, options) ? new Saved(doc.Path, after) : null;
    }

    private static void Close(RhinoDoc doc) {
        doc.Modified = false;
        _ = RhinoApp.RunScript(doc.RuntimeSerialNumber, string.IsNullOrEmpty(doc.Path) ? "_-Close" : $"_-Close \"{doc.Path}\"", echo: false);
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

    private static Fin<A> Lift<A>(Func<A> thunk) => Try.lift(thunk).Run();
}
