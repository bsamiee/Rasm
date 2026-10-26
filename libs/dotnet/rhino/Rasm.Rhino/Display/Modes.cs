using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using UUIDNext;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ModeEdit {
    private ModeEdit(Seq<PropertyInfo> path, MethodInfo setter, Seq<object?> arguments) => (Path, Setter, Arguments) = (path, setter, arguments);

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
    public Fin<Unit> Write(DisplayModeDescription staged) => Written(staged, Path);

    public Fin<Unit> Write(DisplayPipelineAttributes live) =>
        Path.Head.Filter(static step => step.PropertyType == typeof(DisplayPipelineAttributes))
            .ToFin(new Refused(nameof(DisplayModeDescription), Refused.Declined))
            .Bind(_ => Written(live, Path.Tail));

    private Fin<Unit> Written(object root, Seq<PropertyInfo> path) {
        object target = path.Fold(root, static (owner, step) => step.GetValue(owner)!);
        _ = Setter.Invoke(Setter.IsStatic ? null : target, Setter.IsStatic ? [target, .. Arguments] : [.. Arguments]);
        return unit;
    }

    // --- [PARSE]
    private static Seq<PropertyInfo> Chain(Expression node) =>
        node switch {
            ParameterExpression => Seq<PropertyInfo>(),
            MemberExpression { Member: PropertyInfo { PropertyType.IsValueType: false } step, Expression: { } owner } => Chain(owner).Add(step),
            _ => throw new ArgumentException($"{node} is no reference property chain rooted at the {nameof(DisplayModeDescription)}", nameof(node)),
        };

    private static object? Evaluated(Expression argument) =>
        Expression.Lambda<Func<object?>>(Expression.Convert(argument, typeof(object))).Compile(preferInterpretation: true)();
}

public sealed record DisplayModeDefinition(Guid Base, Seq<ModeEdit> Edits);

public sealed record DisplayModeSpec(Seq<(string Name, DisplayModeDefinition Definition)> Modes, Seq<string> Retired);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TechnicalLines {
    // --- [HIDDEN]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_HiddenLineColor")]
    public static extern void HiddenLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_HiddenLineUseSingleColor")]
    public static extern void HiddenLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_HiddenLineThickness")]
    public static extern void HiddenLineThickness(DisplayPipelineAttributes target, int value);

    // --- [EDGES]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_EdgeLineColor")]
    public static extern void EdgeLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_EdgeLineUseSingleColor")]
    public static extern void EdgeLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_EdgeThickness")]
    public static extern void EdgeThickness(DisplayPipelineAttributes target, int value);

    // --- [SILHOUETTES]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_SilhouetteLineColor")]
    public static extern void SilhouetteLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_SilhouetteLineUseSingleColor")]
    public static extern void SilhouetteLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_SilhoutteThickness")]
    public static extern void SilhoutteThickness(DisplayPipelineAttributes target, int value);

    // --- [INTERSECTIONS]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IntersectionLineColor")]
    public static extern void IntersectionLineColor(DisplayPipelineAttributes target, Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IntersectionLineUseSingleColor")]
    public static extern void IntersectionLineUseSingleColor(DisplayPipelineAttributes target, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IntersectionThickness")]
    public static extern void IntersectionThickness(DisplayPipelineAttributes target, int value);
}

public static class DisplayModes {
    // --- [RECONCILE]
    public static IO<Seq<Guid>> Reconcile(Guid owner, DisplayModeSpec spec, Func<Seq<Guid>, IO<Unit>> reassign) =>
        from named in IO.lift(() => Callbacks.Unique(spec.Modes.Map(static row => row.Name) + spec.Retired, identity, nameof(DisplayModeDescription.EnglishName), StringComparer.OrdinalIgnoreCase))
        let rows = spec.Modes.Map(row => (Id: Uuid.NewNameBased(owner, row.Name), row.Name, row.Definition)).Strict()
        from imported in unless(rows.IsEmpty, Imported(rows)).As()
        from written in Callbacks.Each(rows.Map(static row => With(row.Id, staged =>
            from renamed in IO.lift(() => { staged.EnglishName = row.Name; })
            from edited in IO.lift(() => row.Definition.Edits.Traverse(edit => edit.Write(staged)).As())
            from updated in IO.lift(() => Refused.Unless(DisplayModeDescription.UpdateDisplayMode(staged, bSave: false), nameof(DisplayModeDescription.UpdateDisplayMode)))
            select updated)))
        from present in GetDisplayModes(static mode => mode.Id)
        let retired = spec.Retired.Map(name => Uuid.NewNameBased(owner, name)).Filter(toHashSet(present).Contains).Strict()
        from moved in reassign(retired)
        from deleted in Callbacks.Each(retired.Map(static id => IO.lift(() => Refused.Unless(DisplayModeDescription.DeleteDisplayMode(id), nameof(DisplayModeDescription.DeleteDisplayMode)))))
        from saved in IO.lift(DisplayModeDescription.SaveDisplayModes)
        select rows.Map(static row => row.Id);

    private static IO<Unit> Imported(Seq<(Guid Id, string Name, DisplayModeDefinition Definition)> rows) =>
        DisposalOps.Within(static () => Directory.CreateTempSubdirectory(nameof(DisplayModes)), folder =>
            from bases in Callbacks.Each(rows.Map(static row => row.Definition.Base).Distinct().Map(id => Exported(id, Path.Join(folder.FullName, $"{id:D}.ini"))))
            let texts = toHashMap(bases)
            from imported in Callbacks.Each(rows.Map(row => Declared(Path.Join(folder.FullName, $"{row.Id:D}.ini"), texts[row.Definition.Base].Replace(Section(row.Definition.Base), Section(row.Id), StringComparison.Ordinal))))
            select unit,
            static folder => IO.lift(() => folder.Delete(recursive: true)));

    private static IO<(Guid Base, string Text)> Exported(Guid id, string file) =>
        from exported in ExportToFile(id, file)
        from text in IO.lift(() => File.ReadAllText(file))
        select (id, text);

    private static IO<Guid> Declared(string file, string text) =>
        from written in IO.lift(() => File.WriteAllText(file, text))
        from imported in IO.lift(() => Conversions.Required(DisplayModeDescription.ImportFromFile(file, interactive: false), nameof(DisplayModeDescription.ImportFromFile)))
        select imported;

    private static string Section(Guid id) => $@"[DisplayMode\{id:D}";

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
}
