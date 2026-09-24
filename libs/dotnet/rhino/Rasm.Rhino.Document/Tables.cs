using LanguageExt.UnsafeValueAccess;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class TableKind {
    public static readonly TableKind Materials = new(nameof(Materials));

    public static readonly TableKind Linetypes = new(nameof(Linetypes));

    public static readonly TableKind Layers = new(nameof(Layers));

    public static readonly TableKind Groups = new(nameof(Groups));

    public static readonly TableKind DimStyles = new(nameof(DimStyles));

    public static readonly TableKind Lights = new(nameof(Lights));

    public static readonly TableKind HatchPatterns = new(nameof(HatchPatterns));

    public static readonly TableKind InstanceDefinitions = new(nameof(InstanceDefinitions));

    public static readonly TableKind SectionStyles = new(nameof(SectionStyles));

    public static readonly TableKind Markups = new(nameof(Markups));

    public static readonly TableKind PageViewGroups = new(nameof(PageViewGroups));

    public static readonly TableKind RenderMaterials = new(nameof(RenderMaterials));

    public static readonly TableKind RenderEnvironments = new(nameof(RenderEnvironments));

    public static readonly TableKind RenderTextures = new(nameof(RenderTextures));
}

[SmartEnum<string>]
public sealed partial class PurgeKind {
    public static readonly PurgeKind Linetypes = new(nameof(Linetypes), static doc => doc.Linetypes.PurgeUnused());

    public static readonly PurgeKind Layers = new(nameof(Layers), static doc => doc.Layers.PurgeUnused());

    public static readonly PurgeKind Groups = new(nameof(Groups), static doc => doc.Groups.PurgeUnused());

    public static readonly PurgeKind DimStyles = new(nameof(DimStyles), static doc => doc.DimStyles.PurgeUnused());

    public static readonly PurgeKind HatchPatterns = new(nameof(HatchPatterns), static doc => doc.HatchPatterns.PurgeUnused());

    public static readonly PurgeKind InstanceDefinitions = new(nameof(InstanceDefinitions), static doc => doc.InstanceDefinitions.PurgeUnused());

    [UseDelegateFromConstructor]
    public partial int PurgeUnused(RhinoDoc doc);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransformMode {
    public sealed record Move(bool DeleteOriginal) : TransformMode;

    public sealed record History() : TransformMode;

    public Fin<Guid> Apply(ObjectTable table, Guid id, Transform xform) =>
        Switch(
            (Table: table, Id: id, Xform: xform),
            move: static (state, move) => Answers.NonEmpty(state.Table.Transform(state.Id, state.Xform, move.DeleteOriginal), nameof(ObjectTable.Transform)),
            history: static (state, _) => Answers.NonEmpty(state.Table.TransformWithHistory(state.Id, state.Xform), nameof(ObjectTable.TransformWithHistory)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ComponentRef {
    public sealed record ById : ComponentRef {
        private ById(Guid id) => Id = id;

        public Guid Id { get; }

        public static Fin<ComponentRef> Create(Guid id) => Invalid.Unless<ComponentRef>(id != Guid.Empty, new ById(id), nameof(ById));
    }

    public sealed record ByIndex : ComponentRef {
        private ByIndex(int index) => Index = index;

        public int Index { get; }

        public static Fin<ComponentRef> Create(int index) => Limits.AtLeast(0).Check(index, nameof(ByIndex)).Map<ComponentRef>(static valid => new ByIndex(valid));
    }

    public sealed record ByName : ComponentRef {
        private ByName(string name) => Name = name;

        public string Name { get; }

        public static Fin<ComponentRef> Create(string name) => Invalid.Unless<ComponentRef>(name.Length > 0, new ByName(name), nameof(ByName));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotWeight {
    public sealed record Default() : PlotWeight;

    public sealed record NoPlot() : PlotWeight;

    public sealed record Millimeters : PlotWeight {
        internal Millimeters(double value) => Value = value;

        public double Value { get; }

        public static Fin<PlotWeight> Create(double value) => Limits.Above(0.0).Check(value, nameof(Millimeters)).Map<PlotWeight>(static positive => new Millimeters(positive));
    }

    public static PlotWeight FromHost(double value) =>
        value switch {
            0.0 => new Default(),
            < 0.0 => new NoPlot(),
            _ => new Millimeters(value),
        };

    public static Option<PlotWeight> FromHatchBoundary(double value) =>
        value < -1.0 ? Option<PlotWeight>.None : Some(FromHost(value));

    public static double ToHatchBoundary(Option<PlotWeight> weight) =>
        weight.Match(Some: static some => some.ToHost(), None: static () => -10.0);

    public double ToHost() =>
        Switch(@default: static _ => 0.0, noPlot: static _ => -1.0, millimeters: static millimeters => millimeters.Value);
}

public readonly record struct SelectionOptions(bool SyncHighlight, bool Persistent, bool IgnoreGrips, bool IgnoreLayerLocks, bool IgnoreLayerVisibility) {
    public static SelectionOptions Default { get; } = new(SyncHighlight: true, Persistent: true, IgnoreGrips: true, IgnoreLayerLocks: false, IgnoreLayerVisibility: false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionEdit {
    public sealed record Set(bool Select) : SelectionEdit;

    public sealed record Replace() : SelectionEdit;

    public int Apply(ObjectTable table, IEnumerable<Guid> ids, SelectionOptions options) =>
        Switch(
            (Table: table, Ids: ids, Options: options),
            set: static (state, set) => state.Table.Select(
                state.Ids, set.Select, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility),
            replace: static (state, _) => state.Table.SetSelectedObjects(
                state.Ids, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisibilityState {
    public sealed record Hidden(Option<string> HideGroup) : VisibilityState;

    public sealed record Shown() : VisibilityState;

    public sealed record Locked() : VisibilityState;

    public sealed record Unlocked() : VisibilityState;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UndoOp {
    public sealed record Undo() : UndoOp;

    public sealed record Redo() : UndoOp;

    public sealed record ClearUndo(bool PurgeDeleted, Option<uint> Serial) : UndoOp;

    public sealed record ClearRedo() : UndoOp;
}

public sealed record CustomUndo(string Description, EventHandler<CustomUndoEventArgs> Handler, Option<object> Tag);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp {
    public sealed record Add(Seq<GeometryPair> Rows, Option<HistoryRecord> History, bool Reference) : TableOp;

    public sealed record Replace(Guid Id, GeometryBase Geometry, bool IgnoreModes) : TableOp;

    public sealed record Delete(ObjectTarget Target, bool Quiet, bool IgnoreModes) : TableOp;

    public sealed record Move(ObjectTarget Target, Transform Xform, TransformMode Mode) : TableOp;

    public sealed record ModifyAttributes(ObjectTarget Target, Func<ObjectAttributes, IO<Unit>> Edit, bool Quiet) : TableOp;

    public sealed record State(ObjectTarget Target, VisibilityState Next, bool IgnoreLayerMode) : TableOp;

    public sealed record Undelete(ObjectTarget Target) : TableOp;

    public sealed record PointCloud(int X, int Y, int Z, Box Box, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, bool Reference) : TableOp;

    public sealed record ReplaceInstance(ObjectTarget Target, int DefinitionIndex) : TableOp;

    public sealed record ImportPage(string Path, Guid MainViewportId, string PageName) : TableOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImmediateOp {
    public sealed record Selection(ObjectTarget Target, SelectionEdit Edit, SelectionOptions Options) : ImmediateOp;

    public sealed record ClearSelection(bool IgnorePersistent) : ImmediateOp;

    public sealed record Flash(ObjectTarget Target, bool UseSelectionColor) : ImmediateOp;

    public sealed record Purge(ObjectTarget Target) : ImmediateOp;

    public sealed record PurgeUnused(PurgeKind Kind) : ImmediateOp;
}

public sealed record TableAccessors<T>(CommonComponentTable<T> Table, Func<T, int> Add, Func<T, int, bool, bool> Modify, Func<string, T?> FindName, Func<int, T?> FindIndex) where T : ModelComponent;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TableOps {
    // --- [RECORDED]
    public static IO<Seq<Guid>> Apply(RhinoDoc doc, TableOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => add.Rows
                .TraverseM(row =>
                    from proven in Clipped(document, row.Geometry)
                    from id in Added(document, row, add)
                    select id)
                .As(),
            replace: static (document, replace) => IO.lift(() =>
                Refused.Unless(document.Objects.Replace(replace.Id, replace.Geometry, replace.IgnoreModes), Seq(replace.Id), nameof(ObjectTable.Replace))),
            delete: static (document, delete) => Each(document, delete.Target, target =>
                IO.lift(() => Touched(Refused.Unless(document.Objects.Delete(target, delete.Quiet, delete.IgnoreModes), nameof(ObjectTable.Delete)), target))),
            move: static (document, move) => Each(document, move.Target, target => IO.lift(() => move.Mode.Apply(document.Objects, target.Id, move.Xform))),
            modifyAttributes: static (document, modify) => Each(document, modify.Target, target => AttributesModified(document, target, modify)),
            state: static (document, state) => Each(document, state.Target, target =>
                IO.lift(() => Touched(Visibility(document.Objects, target, state.Next, state.IgnoreLayerMode), target))),
            undelete: static (document, undelete) => Each(document, undelete.Target, target =>
                IO.lift(() => Touched(Refused.Unless(document.Objects.Undelete(target.RuntimeSerialNumber), nameof(ObjectTable.Undelete)), target))),
            pointCloud: static (document, cloud) =>
                from valid in IO.lift(() => Invalid.Unless(cloud.Box.IsValid, nameof(Box.IsValid)))
                from id in WithAttributes(document, cloud.Attributes, attributes => IO.lift(() => Answers.NonEmpty(
                    document.Objects.AddOrderedPointCloud(cloud.X, cloud.Y, cloud.Z, cloud.Box.GetCorners(), attributes, cloud.History.ValueUnsafe(), cloud.Reference),
                    nameof(ObjectTable.AddOrderedPointCloud))))
                select Seq(id),
            replaceInstance: static (document, replace) =>
                from inside in IO.lift(() => IndexOutOfRange.Unless(replace.DefinitionIndex, document.InstanceDefinitions.Count, nameof(TableOp.ReplaceInstance.DefinitionIndex)))
                from ids in Each(document, replace.Target, target =>
                    IO.lift(() => Touched(Refused.Unless(document.Objects.ReplaceInstanceObject(target.Id, replace.DefinitionIndex), nameof(ObjectTable.ReplaceInstanceObject)), target)))
                select ids,
            importPage: static (document, page) =>
                from path in Answers.ExistingPath(page.Path)
                from imported in IO.lift(() => Refused.Unless(document.Views.ImportPageView(path, page.MainViewportId, page.PageName), nameof(ViewTable.ImportPageView)))
                select Seq<Guid>());

    public static IO<Seq<Guid>> Recorded(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<CustomUndo> customUndo, Seq<TableOp> ops) =>
        Commits.Commit(
            doc,
            name,
            redraw,
            from registered in customUndo.TraverseM(undo => IO.lift(() => Refused.Unless(doc.AddCustomUndoEvent(undo.Description, undo.Handler, undo.Tag.ValueUnsafe()), nameof(RhinoDoc.AddCustomUndoEvent)))).As()
            from applied in ops.TraverseM(op => Apply(doc, op)).As()
            select applied.Flatten());

    private static IO<Guid> Added(RhinoDoc doc, GeometryPair row, TableOp.Add add) =>
        WithAttributes(doc, row.Attributes, attributes =>
            IO.lift(() => Answers.NonEmpty(doc.Objects.Add(row.Geometry, attributes, add.History.ValueUnsafe(), add.Reference), nameof(ObjectTable.Add))));

    private static IO<Unit> Clipped(RhinoDoc doc, GeometryBase geometry) =>
        Optional(geometry as ClippingPlaneSurface)
            .Map(static surface => toSeq(surface.ViewportIds()))
            .ToSeq()
            .Flatten()
            .TraverseM(id => Viewports.ResolveViewport(doc, new ViewportTarget.Id(id)))
            .As()
            .Map(static _ => unit);

    private static IO<Seq<Guid>> Each(RhinoDoc doc, ObjectTarget target, Func<RhinoObject, IO<Guid>> step) =>
        Queries.Evaluate(doc, target).Bind(objects => objects.TraverseM(step).As());

    private static IO<Guid> AttributesModified(RhinoDoc doc, RhinoObject target, TableOp.ModifyAttributes modify) =>
        Disposal.Using(() => target.Attributes.Duplicate(), copy =>
            from edited in modify.Edit(copy)
            from accepted in IO.lift(() => Refused.Unless(doc.Objects.ModifyAttributes(target.Id, copy, modify.Quiet), nameof(ObjectTable.ModifyAttributes)))
            select target.Id);

    private static Fin<Guid> Touched(Fin<Unit> answer, RhinoObject target) =>
        answer.Map(_ => target.Id);

    private static Fin<Unit> Visibility(ObjectTable table, RhinoObject target, VisibilityState next, bool ignoreLayerMode) =>
        next.Switch(
            (Table: table, Target: target, IgnoreLayerMode: ignoreLayerMode),
            hidden: static (state, hidden) =>
                state.Target.IsHidden ? unit : Refused.Unless(state.Table.Hide(state.Target.Id, state.IgnoreLayerMode, hidden.HideGroup.ValueUnsafe()), nameof(ObjectTable.Hide)),
            shown: static (state, _) =>
                state.Target.IsHidden ? Refused.Unless(state.Table.Show(state.Target.Id, state.IgnoreLayerMode), nameof(ObjectTable.Show)) : unit,
            locked: static (state, _) =>
                state.Target.IsLocked ? unit : Refused.Unless(state.Table.Lock(state.Target.Id, state.IgnoreLayerMode), nameof(ObjectTable.Lock)),
            unlocked: static (state, _) =>
                state.Target.IsLocked ? Refused.Unless(state.Table.Unlock(state.Target.Id, state.IgnoreLayerMode), nameof(ObjectTable.Unlock)) : unit);

    // --- [IMMEDIATE]
    public static IO<int> Immediate(RhinoDoc doc, RedrawPolicy redraw, ImmediateOp op) =>
        Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            selection: static (document, selection) =>
                from ids in Queries.Evaluate(document, selection.Target).Map(static objects => objects.Map(static target => target.Id).Strict())
                from count in IO.lift(() => selection.Edit.Apply(document.Objects, ids, selection.Options))
                select count,
            clearSelection: static (document, clear) => IO.lift(() => document.Objects.UnselectAll(clear.IgnorePersistent)),
            flash: static (document, flash) =>
                from objects in Queries.Evaluate(document, flash.Target)
                from flashed in IO.lift(() => document.Views.FlashObjects(objects, flash.UseSelectionColor))
                select objects.Count,
            purge: static (document, purge) => Each(document, purge.Target, target =>
                    IO.lift(() => Touched(Refused.Unless(document.Objects.Purge(target.RuntimeSerialNumber), nameof(ObjectTable.Purge)), target)))
                .Map(static ids => ids.Count),
            purgeUnused: static (document, unused) => IO.lift(() => unused.Kind.PurgeUnused(document))));

    // --- [UNDO]
    public static IO<Unit> Undo(RhinoDoc doc, RedrawPolicy redraw, UndoOp op) =>
        from closed in IO.lift<Unit>(() => doc.UndoRecordingIsActive ? new UndoRecordOpen(doc.CurrentUndoRecordSerialNumber) : unit)
        from applied in Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            undo: static (document, _) => IO.lift(() => Refused.Unless(document.Undo(), nameof(RhinoDoc.Undo))),
            redo: static (document, _) => IO.lift(() => Refused.Unless(document.Redo(), nameof(RhinoDoc.Redo))),
            clearUndo: static (document, clear) => IO.lift(() => clear.Serial.Match(
                Some: serial => document.ClearUndoRecords(serial, clear.PurgeDeleted),
                None: () => document.ClearUndoRecords(clear.PurgeDeleted))),
            clearRedo: static (document, _) => IO.lift(document.ClearRedoRecords)))
        select applied;

    // --- [ROWS]
    public static IO<Option<T>> Find<T>(ComponentRef address, Func<Guid, Fin<Option<T>>> findId, Func<int, Fin<Option<T>>> findIndex, Func<string, Fin<Option<T>>> findName) =>
        IO.lift(() => address.Switch(
            (FindId: findId, FindIndex: findIndex, FindName: findName),
            byId: static (finders, byId) => finders.FindId(byId.Id),
            byIndex: static (finders, byIndex) => finders.FindIndex(byIndex.Index),
            byName: static (finders, byName) => finders.FindName(byName.Name)));

    public static IO<Option<T>> Find<T>(TableAccessors<T> accessors, ComponentRef address) where T : ModelComponent =>
        Find<T>(address, id => Optional(accessors.Table.FindId(id)), index => Optional(accessors.FindIndex(index)), name => Optional(accessors.FindName(name)));

    public static IO<int> AddRow<T>(TableAccessors<T> accessors, IO<T> create, Func<T, IO<Unit>> edit) where T : ModelComponent =>
        Disposal.Using(create, row =>
            from edited in edit(row)
            from index in IO.lift(() => Answers.NonNegative(accessors.Add(row), nameof(TableAccessors<>.Add)))
            select index);

    public static IO<Unit> ModifyRow<T>(TableAccessors<T> accessors, int index, Func<T, T> copy, Func<T, IO<Unit>> edit, bool quiet) where T : ModelComponent =>
        from live in IO.lift(() => Missing.Unless(accessors.FindIndex(index), nameof(TableAccessors<>.FindIndex)))
        from modified in Disposal.Using(() => copy(live), staged =>
            from edited in edit(staged)
            from accepted in IO.lift(() => Refused.Unless(accessors.Modify(staged, index, quiet), nameof(TableAccessors<>.Modify)))
            select accepted)
        select modified;

    public static IO<Unit> DeleteRow<T>(TableAccessors<T> accessors, T row) where T : ModelComponent =>
        IO.lift(() => Refused.Unless(accessors.Table.Delete(row), nameof(CommonComponentTable<>.Delete)));

    public static IO<string> UnusedName(Func<string?> unusedName) =>
        IO.lift(() => Answers.Present(unusedName()).ToFin(new Invalid(nameof(unusedName))));

    public static IO<Seq<TRow>> ReadRows<T, TRow>(Func<T?[]?> read, Func<T, IO<TRow>> project) where T : class, IDisposable =>
        Disposal.Using(
            IO.lift(() => Missing.Unless(read(), nameof(read)).Map(static rows => Answers.Present(rows))),
            rows => rows.TraverseM(project).As());

    // --- [ATTRIBUTES]
    public static IO<TValue> WithAttributes<TValue>(RhinoDoc doc, Option<ObjectAttributes> attributes, Func<ObjectAttributes, IO<TValue>> body) =>
        attributes.Match(Some: body, None: () => Disposal.Using(doc.CreateDefaultAttributes, body));

    public static IO<TValue> WithAttributes<TValue>(RhinoDoc doc, Seq<Option<ObjectAttributes>> attributes, Func<Seq<ObjectAttributes>, IO<TValue>> body) =>
        attributes.Match(
            Empty: () => body(Seq<ObjectAttributes>()),
            Tail: (head, rest) => WithAttributes(doc, head, first => WithAttributes(doc, rest, others => body(first.Cons(others)))));

    // --- [ANSWERS]
    public static IO<TValue> Locked<TValue>(IO<TValue> write, string member) =>
        write.IfFail(error => IO.fail<TValue>(error.HasException<InvalidOperationException>() ? new Refused(member) : error));
}
