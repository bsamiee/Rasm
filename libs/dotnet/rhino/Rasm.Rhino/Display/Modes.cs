using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Objects;
using Rasm.Rhino.UI;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using UUIDNext;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ModeEdit {
    private static readonly MethodInfo Answered = ((Func<bool, string, Fin<Unit>>)Refused.Unless).Method;

    private readonly Lazy<Func<DisplayModeDescription, Fin<Unit>>> stored;
    private readonly Option<Lazy<Func<DisplayPipelineAttributes, Fin<Unit>>>> attributes;

    private ModeEdit(Seq<PropertyInfo> path, MethodInfo setter, Seq<object?> arguments) {
        (Path, Setter, Arguments) = (path, setter, arguments);
        stored = new(() => Compiled<DisplayModeDescription>(path, setter, arguments));
        attributes = path.Head.Filter(static step => step.PropertyType == typeof(DisplayPipelineAttributes))
            .Map(_ => new Lazy<Func<DisplayPipelineAttributes, Fin<Unit>>>(() => Compiled<DisplayPipelineAttributes>(path.Tail, setter, arguments)));
    }

    public Seq<PropertyInfo> Path { get; }
    public MethodInfo Setter { get; }
    public Seq<object?> Arguments { get; }

    // --- [FACTORY]
    public static ModeEdit Set<T>(Expression<Func<DisplayModeDescription, T>> member, T value) =>
        member.Body is MemberExpression { Member: PropertyInfo { SetMethod: { IsPublic: true } setter }, Expression: { } owner }
            ? new(Chain(owner), setter, Seq<object?>(value))
            : throw new ArgumentException($"{member.Body} is no settable property on a {nameof(DisplayModeDescription)} chain", nameof(member));

    public static ModeEdit Call(Expression<Action<DisplayModeDescription>> call) =>
        call.Body switch {
            MethodCallExpression { Object: { } instance } body => new(Chain(instance), body.Method, toSeq(body.Arguments).Map(Evaluated).Strict()),
            MethodCallExpression { Object: null, Arguments: [var target, ..] } body => new(Chain(target), body.Method, toSeq(body.Arguments).Tail.Map(Evaluated).Strict()),
            _ => throw new ArgumentException($"{call.Body} is no method call on a {nameof(DisplayModeDescription)} chain", nameof(call)),
        };

    // --- [WRITES]
    public Fin<Unit> Write(DisplayModeDescription staged) => stored.Value(staged);

    public Fin<Unit> Write(DisplayPipelineAttributes live) =>
        attributes.Match(Some: compiled => compiled.Value(live), None: static () => new Refused(nameof(DisplayModeDescription), Refused.Declined));

    // --- [EQUALITY]
    public bool Equals([NotNullWhen(true)] ModeEdit? other) => other is { } edit && Path.Equals(edit.Path) && Setter.Equals(edit.Setter) && Arguments.Equals(edit.Arguments);

    public override int GetHashCode() => HashCode.Combine(Path, Setter, Arguments);

    // --- [PARSE]
    private static Seq<PropertyInfo> Chain(Expression node) =>
        node switch {
            ParameterExpression => Seq<PropertyInfo>(),
            MemberExpression { Member: PropertyInfo { PropertyType.IsValueType: false } step, Expression: { } owner } => Chain(owner).Add(step),
            _ => throw new ArgumentException($"{node} is no reference property chain rooted at the {nameof(DisplayModeDescription)}", nameof(node)),
        };

    private static object? Evaluated(Expression argument) =>
        Expression.Lambda<Func<object?>>(Expression.Convert(argument, typeof(object))).Compile(preferInterpretation: true)();

    // --- [COMPILE]
    private static Func<TRoot, Fin<Unit>> Compiled<TRoot>(Seq<PropertyInfo> path, MethodInfo setter, Seq<object?> arguments) {
        ParameterExpression root = Expression.Parameter(typeof(TRoot));
        Expression target = path.Fold((Expression)root, static (owner, step) => Expression.Property(owner, step));
        Seq<Expression> values = toSeq(setter.GetParameters()).Skip(setter.IsStatic ? 1 : 0).Zip(arguments)
            .Map(static pair => (Expression)Expression.Constant(pair.Second, pair.First.ParameterType)).Strict();
        MethodCallExpression call = setter.IsStatic ? Expression.Call(setter, [target, .. values]) : Expression.Call(target, setter, values);
        Expression answer = setter.ReturnType switch {
            Type returned when returned == typeof(Fin<Unit>) => call,
            Type returned when returned == typeof(bool) => Expression.Call(Answered, call, Expression.Constant(setter.Name)),
            _ => Expression.Block(call, Expression.Constant((Fin<Unit>)unit, typeof(Fin<Unit>))),
        };
        return Expression.Lambda<Func<TRoot, Fin<Unit>>>(answer, root).Compile();
    }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class ModeFlag {
    public static readonly ModeFlag Yes = new("y", value: true);
    public static readonly ModeFlag No = new("n", value: false);

    public bool Value { get; }
}

[Union]
public abstract partial record ModeValue {
    public sealed record Flag(bool Value) : ModeValue;

    public sealed record Whole(int Value) : ModeValue;

    public sealed record Real(double Value) : ModeValue;

    public sealed record Colour(Color Value) : ModeValue;

    public sealed record Id(Guid Value) : ModeValue;

    public sealed record Vector(Vector3d Value) : ModeValue;

    public sealed record Text(string Value) : ModeValue;

    public sealed record Packaged(string Path) : ModeValue;

    // --- [CODEC]
    internal Fin<string> Formatted(Option<DirectoryInfo> package) =>
        Switch(
            package,
            flag: static (_, flag) => (flag.Value ? ModeFlag.Yes : ModeFlag.No).Key,
            whole: static (_, whole) => whole.Value.ToString(CultureInfo.InvariantCulture),
            real: static (_, real) => real.Value.ToString(CultureInfo.InvariantCulture),
            colour: static (_, colour) => colour.Value.IsEmpty ? "-1" : $"{colour.Value.R},{colour.Value.G},{colour.Value.B}",
            id: static (_, id) => id.Value.ToString("D"),
            vector: static (_, vector) => string.Create(CultureInfo.InvariantCulture, $"{vector.Value.X},{vector.Value.Y},{vector.Value.Z}"),
            text: static (_, text) => text.Value,
            packaged: static (folder, packaged) => Exchange.ExistingPath(folder.Match(Some: held => Path.GetFullPath(packaged.Path, held.FullName), None: () => packaged.Path)));

    internal Option<ModeValue> Parsed(string host) =>
        Switch(
            host,
            flag: static (text, _) => Callbacks.Found(ModeFlag.TryGet(text, out ModeFlag? flag), flag!).Map(static held => (ModeValue)new Flag(held.Value)),
            whole: static (text, _) => Integral(text).Map(static value => (ModeValue)new Whole(value)),
            real: static (text, _) => Number(text).Map(static value => (ModeValue)new Real(value)),
            colour: static (text, _) => text is "-1"
                ? new Colour(Color.Empty)
                : Numbers(text).Filter(static parts => parts.Count is 3).Map(static parts => (ModeValue)new Colour(Color.FromArgb((int)parts[0], (int)parts[1], (int)parts[2]))),
            id: static (text, _) => parseGuid(text).Map(static id => (ModeValue)new Id(id)),
            vector: static (text, _) => Numbers(text).Filter(static parts => parts.Count is 3).Map(static parts => (ModeValue)new Vector(new Vector3d(parts[0], parts[1], parts[2]))),
            text: static (text, _) => new Text(text),
            packaged: static (text, _) => new Packaged(text));

    internal bool Holds(string host, Option<DirectoryInfo> package) =>
        Formatted(package).ToOption().Bind(Parsed) is { IsSome: true } declared && declared == Parsed(host);

    internal static Option<int> Integral(string text) =>
        Number(text).Filter(static value => double.IsInteger(value) && value is >= int.MinValue and <= int.MaxValue).Map(static value => (int)value);

    private static Option<double> Number(string text) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : None;

    private static Option<Seq<double>> Numbers(string text) =>
        toSeq(text.Split(',')).Traverse(static part => Number(part)).As();
}

public sealed record ModeKey(string Section, string Key, ModeValue Value);

public sealed record DisplayModeDefinition(string Name, Guid Base) {
    public Seq<ModeEdit> Edits { get; init; }
    public Seq<ModeKey> Keys { get; init; }
}

public sealed record DisplayModeSpec(IO<Seq<DisplayModeDefinition>> Modes) {
    public HashMap<string, DisplayModeDefinition> Successors { get; init; }
    public Seq<HostEvent<Unit>> Refreshed { get; init; }
}

public sealed record ReconciledModes(Seq<Guid> Written, Seq<Guid> Retired);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TechnicalLines {
    // --- [USAGE]
    private const uint Creases = 8;
    private const uint Seams = 16;

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetTechnicalUsageBool")]
    private static extern bool GetTechnicalUsageBool(DisplayPipelineAttributes target, uint tech);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SetTechnicalUsageBool")]
    private static extern void SetTechnicalUsageBool(DisplayPipelineAttributes target, uint which, bool b);

    // --- [HIDDEN]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_HiddenLineColor")]
    public static extern Color HiddenLineColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_HiddenLineColor")]
    public static extern void HiddenLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_HiddenLineUseSingleColor")]
    public static extern bool HiddenLineUseSingleColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_HiddenLineUseSingleColor")]
    public static extern void HiddenLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_HiddenLineThickness")]
    public static extern int HiddenLineThickness(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_HiddenLineThickness")]
    public static extern void HiddenLineThickness(DisplayPipelineAttributes target, int value);

    // --- [EDGES]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_EdgeLineColor")]
    public static extern Color EdgeLineColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_EdgeLineColor")]
    public static extern void EdgeLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_EdgeLineUseSingleColor")]
    public static extern bool EdgeLineUseSingleColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_EdgeLineUseSingleColor")]
    public static extern void EdgeLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_EdgeThickness")]
    public static extern int EdgeThickness(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_EdgeThickness")]
    public static extern void EdgeThickness(DisplayPipelineAttributes target, int value);

    // --- [SILHOUETTES]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SilhouetteLineColor")]
    public static extern Color SilhouetteLineColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_SilhouetteLineColor")]
    public static extern void SilhouetteLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SilhouetteLineUseSingleColor")]
    public static extern bool SilhouetteLineUseSingleColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_SilhouetteLineUseSingleColor")]
    public static extern void SilhouetteLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SilhoutteThickness")]
    public static extern int SilhoutteThickness(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_SilhoutteThickness")]
    public static extern void SilhoutteThickness(DisplayPipelineAttributes target, int value);

    // --- [CREASES]
    public static bool CreaseLineUseSingleColor(DisplayPipelineAttributes target) => GetTechnicalUsageBool(target, Creases);

    public static void CreaseLineUseSingleColor(DisplayPipelineAttributes target, bool value) => SetTechnicalUsageBool(target, Creases, value);

    // --- [SEAMS]
    public static bool SeamLineUseSingleColor(DisplayPipelineAttributes target) => GetTechnicalUsageBool(target, Seams);

    public static void SeamLineUseSingleColor(DisplayPipelineAttributes target, bool value) => SetTechnicalUsageBool(target, Seams, value);

    // --- [INTERSECTIONS]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IntersectionLineColor")]
    public static extern Color IntersectionLineColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IntersectionLineColor")]
    public static extern void IntersectionLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IntersectionLineUseSingleColor")]
    public static extern bool IntersectionLineUseSingleColor(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IntersectionLineUseSingleColor")]
    public static extern void IntersectionLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IntersectionThickness")]
    public static extern int IntersectionThickness(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IntersectionThickness")]
    public static extern void IntersectionThickness(DisplayPipelineAttributes target, int value);

    // --- [SHADING_EFFECTS]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ShadingEffects")]
    public static extern int ShadingEffects(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ShadingEffects")]
    public static extern void ShadingEffects(DisplayPipelineAttributes target, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ShadingEffectsWidth")]
    public static extern int ShadingEffectsWidth(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ShadingEffectsWidth")]
    public static extern void ShadingEffectsWidth(DisplayPipelineAttributes target, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ShadingEffectsSeparation")]
    public static extern int ShadingEffectsSeparation(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ShadingEffectsSeparation")]
    public static extern void ShadingEffectsSeparation(DisplayPipelineAttributes target, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ShadingEffectsRotation")]
    public static extern int ShadingEffectsRotation(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ShadingEffectsRotation")]
    public static extern void ShadingEffectsRotation(DisplayPipelineAttributes target, int value);
}

public static class DisplayModes {
    // --- [IDENTITY]
    public static Guid Id(Guid owner, DisplayModeDefinition mode) => Uuid.NewNameBased(owner, mode.Name);

    // --- [ACQUISITION]
    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Declared(DisplayModeSpec spec) =>
        (owner, retain) => Declared(owner.PlugIn.Id, owner.PlugIn.Assembly, spec, owner, retain);

    public static IO<Unit> Declared(Guid owner, Assembly package, DisplayModeSpec spec, IPlugInSink sink, Action<IDisposable> retain) =>
        from folder in IO.lift(() => Exchange.Installed(package))
        let reconciled = Reconcile(owner, folder, spec).Map(unit)
        from loaded in reconciled.Catch(error => IO.lift(() => sink.Report(error, typeof(DisplayModes), nameof(Declared)))).As()
        from opened in when(!spec.Successors.IsEmpty, EventKind.EndOpenDocument.Inline(
            args => IO.lift(() => Refused.UnlessFound(args.Document, nameof(DocumentOpenEventArgs.Document))).Bind(doc => Reassigned(doc, Successors(owner, spec))),
            sink, retain)).As()
        from refreshed in spec.Refreshed.TraverseM(signal => signal.Inline(_ => reconciled, sink, retain)).As()
        select unit;

    private static HashMap<Guid, Guid> Successors(Guid owner, DisplayModeSpec spec) =>
        toHashMap(spec.Successors.AsIterable().Map(pair => (Uuid.NewNameBased(owner, pair.Key), Id(owner, pair.Value))));

    // --- [RECONCILE]
    private const string Order = "Order";
    private const string DerivedFrom = "DerivedFrom";

    public static IO<ReconciledModes> Reconcile(Guid owner, DirectoryInfo package, DisplayModeSpec spec) =>
        from declared in spec.Modes
        let rows = declared.Map(mode => (Id: Id(owner, mode), Mode: mode)).Strict()
        from valid in IO.lift(() => Checked(owner, rows, spec))
        from present in GetDisplayModes(static mode => (mode.Id, mode.EnglishName))
        let ids = toHashSet(rows.Map(static row => row.Id))
        let retired = present.Filter(mode => mode.Id == Uuid.NewNameBased(owner, mode.EnglishName) && !ids.Contains(mode.Id))
            .Map(mode => (mode.Id, Successor: spec.Successors.Find(mode.EnglishName).Map(successor => Id(owner, successor)))).Strict()
        from bases in Callbacks.Each(rows.Map(static row => row.Mode.Base).Distinct().Map(static id => Exported(id).Map(text => (Id: id, Text: text))))
        let texts = toHashMap(bases)
        from patches in IO.lift(() => Callbacks.Each(rows, (row, _) => Patch(row.Id, Merged(Root(row.Mode, Table(texts[row.Mode.Base])) + row.Mode.Keys), Some(package))))
        let clone = string.Join(Lines, rows.Map(row => texts[row.Mode.Base].Replace(Section(row.Mode.Base), Section(row.Id), StringComparison.Ordinal)))
        from written in Written(
            rows.IsEmpty ? [] : Seq(clone, string.Join(Lines, patches)),
            rows.Map(static row => (row.Id, row.Mode.Edits, Merged(row.Mode.Keys))),
            retired,
            Some(package))
        select written;

    public static IO<Unit> Update(Seq<Guid> ids, Seq<ModeEdit> edits, Seq<ModeKey> keys) =>
        from present in Callbacks.Each(ids.Map(static id => With(id, static _ => IO.pure(unit))))
        let merged = Merged(keys)
        from patches in IO.lift(() => Callbacks.Each(ids, (id, _) => Patch(id, merged, None)))
        from written in Written(merged.IsEmpty || ids.IsEmpty ? [] : Seq(string.Join(Lines, patches)), ids.Map(id => (id, edits, merged)), [], None)
        select unit;

    private static Fin<Unit> Checked(Guid owner, Seq<(Guid Id, DisplayModeDefinition Mode)> rows, DisplayModeSpec spec) =>
        toHashSet(rows.Map(static row => row.Id)) switch {
            var ids => (
                    Callbacks.Unique(rows.Map(static row => row.Mode.Name) + toSeq(spec.Successors.Keys), identity, nameof(DisplayModeDefinition.Name), StringComparer.OrdinalIgnoreCase),
                    Callbacks.Each(rows, (row, index) => Refused.Unless(!ids.Contains(row.Mode.Base), nameof(DisplayModeDefinition.Base), index)),
                    Callbacks.Each(toSeq(spec.Successors.Values), (successor, index) => Refused.Unless(ids.Contains(Id(owner, successor)), nameof(DisplayModeSpec.Successors), index)))
                .Apply(static (_, _, _) => unit)
                .As(),
        };

    private static Seq<ModeKey> Root(DisplayModeDefinition mode, HashMap<(string Section, string Key), string> source) =>
        Seq(new ModeKey("", "Name", new ModeValue.Text(mode.Name)), new ModeKey("", Order, 0), new ModeKey("", "AddToMenu", Value: true))
        + (source.Find(("", Order)).Bind(ModeValue.Integral).Exists(static order => order < 0)
            ? Seq(new ModeKey("", DerivedFrom, mode.Base))
            : Seq<ModeKey>());

    // --- [WRITE]
    private const string Lines = "\r\n";

    private static readonly Guid SubDEdgesOff = new(0xe1eb7363, 0x87f2, 0x4a2b, 0xa8, 0x61, 0x25, 0x6e, 0x77, 0x83, 0x53, 0x69);

    private static readonly Seq<(string Member, Func<DisplayModeDescription, bool> Read)> Technical = [
        (nameof(DisplayModeDescription.SupportsShadeCommand), static mode => mode.SupportsShadeCommand),
        (nameof(DisplayModeDescription.SupportsShading), static mode => mode.SupportsShading),
        (nameof(DisplayModeDescription.AllowObjectAssignment), static mode => mode.AllowObjectAssignment),
        (nameof(DisplayModeDescription.ShadedPipelineRequired), static mode => mode.ShadedPipelineRequired),
        (nameof(DisplayModeDescription.WireframePipelineRequired), static mode => mode.WireframePipelineRequired),
        (nameof(DisplayModeDescription.PipelineLocked), static mode => mode.PipelineLocked),
    ];

    private static readonly Seq<(string Member, Func<DisplayModeDescription, bool> Read)> SubDEdges = [
        (nameof(DisplayPipelineAttributes.ShowSubDEdges), static mode => mode.DisplayAttributes.ShowSubDEdges),
        (nameof(DisplayPipelineAttributes.ShowSubDCreases), static mode => mode.DisplayAttributes.ShowSubDCreases),
        (nameof(DisplayPipelineAttributes.ShowSubDBoundary), static mode => mode.DisplayAttributes.ShowSubDBoundary),
        (nameof(DisplayPipelineAttributes.ShowSubDNonmanifoldEdges), static mode => mode.DisplayAttributes.ShowSubDNonmanifoldEdges),
    ];

    private static IO<ReconciledModes> Written(Seq<string> files, Seq<(Guid Id, Seq<ModeEdit> Edits, Seq<ModeKey> Keys)> rows, Seq<(Guid Id, Option<Guid> Successor)> retired, Option<DirectoryInfo> package) =>
        DisposalOps.Within(
            static () => unit,
            _ => from imported in files.TraverseM(static file => Imported(file)).As()
                 from settled in Callbacks.Each(Seq(
                     Callbacks.Each(rows.Map(static row => Staged(row.Id, row.Edits))).Map(unit),
                     Callbacks.Each(rows.Filter(static row => !row.Keys.IsEmpty).Map(row => ReadBack(row.Id, row.Keys, package))).Map(unit),
                     Retired(rows.Map(static row => row.Id), retired)))
                 select new ReconciledModes(rows.Map(static row => row.Id), retired.Map(static mode => mode.Id)),
            _ => from saved in IO.lift(DisplayModeDescription.SaveDisplayModes)
                 from raised in IO.lift(() => Raise(rows.Map(static row => row.Id) + retired.Map(static mode => mode.Id)))
                 select unit);

    private static IO<Unit> Staged(Guid id, Seq<ModeEdit> edits) =>
        With(id, staged =>
            from technical in With(DisplayModeDescription.TechId, static mode => IO.lift(() => PipelineId(mode.DisplayAttributes)))
            from imported in IO.lift(() => PipelineId(staged.DisplayAttributes) switch {
                var pipeline => ((pipeline == technical ? Technical : []) + (pipeline == SubDEdgesOff ? SubDEdges : []))
                    .Map(member => (member.Member, member.Read, Value: member.Read(staged))).Strict(),
            })
            from edited in IO.lift(() => edits.Traverse(edit => edit.Write(staged)).As())
            from held in IO.lift(() => Accumulated(imported.Filter(member => member.Read(staged) != member.Value).Map(static member => (Error)new Refused(member.Member, Refused.Declined))))
            from updated in IO.lift(() => Refused.Unless(DisplayModeDescription.UpdateDisplayMode(staged, bSave: false), nameof(DisplayModeDescription.UpdateDisplayMode)))
            select updated);

    private static IO<Unit> ReadBack(Guid id, Seq<ModeKey> keys, Option<DirectoryInfo> package) =>
        Keys(id).Bind(table => IO.lift(() => Accumulated(keys
            .Filter(key => !table.Find((key.Section, key.Key)).Exists(text => key.Value.Holds(text, package)))
            .Map(static key => (Error)new Refused(key.Section.Length is 0 ? key.Key : $@"{key.Section}\{key.Key}", Refused.Invalid)))));

    private static IO<Unit> Retired(Seq<Guid> written, Seq<(Guid Id, Option<Guid> Successor)> retired) =>
        from moves in Callbacks.Each(retired.Map(static mode => mode.Successor.Match(
            Some: successor => IO.pure((mode.Id, successor)),
            None: () => Keys(mode.Id).Bind(table => IO.lift(() => table.Find(("", DerivedFrom)).Bind(parseGuid)
                .ToFin(new Refused(DerivedFrom, Refused.Absent))
                .Map(source => (mode.Id, source)))))))
        from moved in Reassigned(toHashMap(written.Map(static id => (id, id))).AddOrUpdateRange(moves))
        from deleted in Callbacks.Each(retired.Map(static mode => IO.lift(() => Refused.Unless(DisplayModeDescription.DeleteDisplayMode(mode.Id), nameof(DisplayModeDescription.DeleteDisplayMode)))))
        select unit;

    private static Fin<Unit> Accumulated(Seq<Error> errors) => errors.IsEmpty ? unit : Error.Many(errors);

    private static Guid PipelineId(DisplayPipelineAttributes attributes) => PipelineId(natives: null, ConstPointer(attributes));

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ConstPointer")]
    private static extern nint ConstPointer(DisplayPipelineAttributes target);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "DisplayPipelineAttributes_GetPipelineId")]
    private static extern Guid PipelineId([UnsafeAccessorType("UnsafeNativeMethods, RhinoCommon")] object? natives, nint attributes);

    // --- [PATCH]
    private static Seq<ModeKey> Merged(Seq<ModeKey> keys) =>
        toSeq(HashMap<(string Section, string Key), ModeKey>().AddOrUpdateRange(keys.Map(static key => ((key.Section, key.Key), key))).Values).Strict();

    private static Fin<string> Patch(Guid id, Seq<ModeKey> keys, Option<DirectoryInfo> package) =>
        keys.Traverse(key => key.Value.Formatted(package).Map(value => (key.Section, Line: $"{key.Key}={value}"))).As()
            .Map(lines => string.Join(Lines, (Seq("") + lines.Map(static line => line.Section).Distinct().Filter(static section => section.Length > 0))
                .Bind(section => Seq(Header(id, section)) + lines.Filter(line => string.Equals(line.Section, section, StringComparison.Ordinal)).Map(static line => line.Line))));

    private static string Section(Guid id) => $@"[DisplayMode\{id:D}";

    private static string Header(Guid id, string section) => $"{Section(id)}{(section.Length is 0 ? "" : $@"\{section}")}]";

    // --- [KEYS]
    public static IO<HashMap<(string Section, string Key), string>> Keys(Guid id) => Exported(id).Map(Table);

    private static HashMap<(string Section, string Key), string> Table(string text) =>
        toSeq(text.ReplaceLineEndings("\n").Split('\n')).Fold(
            (Section: "", Rows: HashMap<(string Section, string Key), string>()),
            static (state, line) => line switch {
                ['[', ..] and [_, .. var header, ']'] => (header.Split('\\', 3) is [_, _, var relative] ? relative : "", state.Rows),
                _ when line.IndexOf('=', StringComparison.Ordinal) is > 0 and var at => (state.Section, state.Rows.AddOrUpdate((state.Section, line[..at]), line[(at + 1)..])),
                _ => state,
            }).Rows;

    private static IO<string> Exported(Guid id) =>
        Scoped(file =>
            from exported in ExportToFile(id, file)
            from text in IO.lift(() => File.ReadAllText(file))
            select text);

    private static IO<Guid> Imported(string text) =>
        Scoped(file =>
            from written in IO.lift(() => File.WriteAllText(file, text))
            from imported in IO.lift(() => Conversions.Required(DisplayModeDescription.ImportFromFile(file, interactive: false), nameof(DisplayModeDescription.ImportFromFile)))
            select imported);

    private static IO<T> Scoped<T>(Func<string, IO<T>> body) =>
        DisposalOps.Within(
            static () => Directory.CreateTempSubdirectory(nameof(DisplayModes)),
            folder => body(Path.Join(folder.FullName, $"{nameof(DisplayModes)}.ini")),
            static folder => IO.lift(() => folder.Delete(recursive: true)));

    // --- [HOLDERS]
    private static IO<Unit> Reassigned(HashMap<Guid, Guid> moves) =>
        IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments())).Bind(documents => Callbacks.Each(documents.Map(doc => Reassigned(doc, moves)))).Map(unit);

    private static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        moves.Filter(static (source, target) => source != target) switch {
            var moved => Callbacks.Each(Seq(
                Viewports.Edit(
                    doc,
                    new ViewportSet.Every(ViewTypeFilter.All, Details: true),
                    viewport => IO.lift(() => Shown(viewport)).Bind(shown => shown.Bind(id => moves.Find(id)).Match(
                        Some: target => With(target, mode => IO.lift(() => { viewport.DisplayMode = mode; })),
                        None: static () => IO.pure(unit))),
                    new RedrawPolicy.AllViews(Deferred: true)).Map(unit),
                when(!moved.IsEmpty, Callbacks.Each(Seq(NamedViews.Reassigned(doc, moved), Overrides(doc, moved), Animations.Reassigned(doc, moved))).Map(unit)).As())).Map(unit),
        };

    private static IO<Unit> Overrides(RhinoDoc doc, HashMap<Guid, Guid> moved) =>
        from slots in DisposalOps.Within(
            (Action<IDisposable> retain) => Viewports.Resolve(doc, new ViewportSet.Every(ViewTypeFilter.All, Details: true), retain)
                .Map(static rows => (Seq(Guid.Empty) + rows.Map(static row => row.Viewport.Id)).Strict()),
            DisposalOps.Release)
        from rewritten in TableOps.Apply(doc, AttributeOps.Modify(
            doc,
            new ObjectTarget.Lookup(table => table.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = true, LockedObjects = true, IncludeLights = true })
                .Where(found => !Overridden(found.Attributes, slots, moved).IsEmpty)),
            Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>>((_, staged) =>
                Callbacks.Each(Overridden(staged, slots, moved).Map(held => AttributeEdits.DisplayModeOverride(staged, held.Slot, Some(held.Target)))).Map(unit)),
            quiet: true))
        select unit;

    private static Seq<(Guid Slot, Guid Target)> Overridden(ObjectAttributes attributes, Seq<Guid> slots, HashMap<Guid, Guid> moved) =>
        slots.Choose(slot => AttributeOps.DisplayMaterialRef(attributes, slot).Bind(held => moved.Find(held)).Map(target => (slot, target))).Strict();

    private static Option<Guid> Shown(RhinoViewport viewport) {
        using DisplayModeDescription? mode = viewport.DisplayMode;
        return Optional(mode).Map(static held => held.Id);
    }

    // --- [SIGNALS]
    private static readonly Atom<Guid> Raised = Atom(Guid.Empty);

    public static HostEvent<Guid> Changed { get; } = Subscriptions.Host(typeof(DisplayModes), static () => Seq(
        Subscriptions.Changed(Raised, nameof(Raised)),
        EventKind.DisplayModeChanged.Choose(static args => Callbacks.Found(Switched(args).IsNone, args.ChangedDisplayModeId))));

    internal static Option<Guid> Switched(DisplayModeChangedEventArgs args) =>
        Optional(args.Viewport)
            .Filter(viewport => args.OldDisplayModeId != args.ChangedDisplayModeId && Shown(viewport) == Some(args.ChangedDisplayModeId))
            .Map(static viewport => viewport.Id);

    private static Unit Raise(Seq<Guid> ids) => ids.Iter(static id => ignore(Raised.Swap(_ => id)));

    // --- [TABLE]
    public static IO<TValue> With<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> body) =>
        DisposalOps.Found(() => DisplayModeDescription.GetDisplayMode(id), nameof(DisplayModeDescription.GetDisplayMode), body);

    public static IO<Seq<T>> GetDisplayModes<T>(Func<DisplayModeDescription, T> project) =>
        DisposalOps.Within(static () => Conversions.Rows(DisplayModeDescription.GetDisplayModes()), modes => IO.lift(() => modes.Map(project).Strict()), DisposalOps.Release);

    public static IO<Guid> FindByName(string englishName) =>
        DisposalOps.Found(() => DisplayModeDescription.FindByName(englishName), nameof(DisplayModeDescription.FindByName), static mode => IO.pure(mode.Id));

    public static IO<Guid> ImportFromFile(string path) =>
        from imported in IO.lift(() => Exchange.ExistingPath(path)
            .Bind(static existing => Conversions.Required(DisplayModeDescription.ImportFromFile(existing, interactive: false), nameof(DisplayModeDescription.ImportFromFile))))
        from saved in IO.lift(DisplayModeDescription.SaveDisplayModes)
        from refreshed in Reassigned(HashMap((imported, imported)))
        from raised in IO.lift(() => Raise(Seq(imported)))
        select imported;

    public static IO<Unit> ExportToFile(Guid id, string path) =>
        from target in IO.lift(Exchange.QualifiedPath(path))
        from exported in With(id, mode => IO.lift(() => Refused.Unless(DisplayModeDescription.ExportToFile(mode, target), nameof(DisplayModeDescription.ExportToFile))))
        select exported;

    // --- [VIEWPORT]
    public static IO<Guid> DisplayMode(RhinoDoc doc, ViewportTarget target) =>
        Viewports.With(doc, target, static row => DisposalOps.Found(() => row.Viewport.DisplayMode, nameof(RhinoViewport.DisplayMode), static mode => IO.pure(mode.Id)));

    public static IO<Seq<bool>> SetDisplayMode(RhinoDoc doc, ViewportSet viewports, Guid id, RedrawPolicy redraw) =>
        With(id, mode => Viewports.Edit(doc, viewports, viewport => IO.lift(() => { viewport.DisplayMode = mode; }), redraw));

    // --- [VARIANTS]
    public static IO<Bitmap> CaptureToBitmap(RhinoView view, Guid id, Option<Size> pixels, Seq<ModeEdit> edits, Action<IDisposable> retain) =>
        With(id, mode => IO.lift(() => edits.Traverse(edit => edit.Write(mode)).As()).Bind(_ => Copies.Owned(
            () => pixels.Match(Some: size => view.CaptureToBitmap(size, mode.DisplayAttributes), None: () => view.CaptureToBitmap(mode.DisplayAttributes)),
            static bitmap => Refused.UnlessFound(bitmap, nameof(RhinoView.CaptureToBitmap)),
            static bitmap => DisposalOps.Release(bitmap), retain)));

    internal static IO<TValue> Overlaid<TValue>(DisplayPipelineAttributes live, Seq<ModeEdit> edits, IO<TValue> draw) =>
        DisposalOps.Within(
            () => Snapshot(live),
            snapshot => DisposalOps.Within(
                IO.pure(snapshot),
                _ => IO.lift(() => edits.Traverse(edit => edit.Write(live)).As()).Bind(_ => draw),
                held => IO.lift(() => CopyContents(live, held))));

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern DisplayPipelineAttributes Snapshot(DisplayPipelineAttributes other);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "CopyContents")]
    private static extern void CopyContents(DisplayPipelineAttributes target, DisplayPipelineAttributes other);
}
