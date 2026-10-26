using Rasm.Drafting;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ComponentRef<T> {
    public sealed record ById(Guid Id) : ComponentRef<T>;

    public sealed record ByIndex(int Index) : ComponentRef<T>;

    public sealed record ByName(string Name) : ComponentRef<T>;
}

public readonly record struct ComponentIdentity(Guid Id, ModelComponentType Type);

public sealed record TableKind<T>(
    Func<RhinoDoc, RhinoDocCommonTable<T>> Table,
    Func<T, T> Copy,
    Func<RhinoDoc, T, int> Add,
    Func<RhinoDoc, T, int, bool> Modify,
    Func<RhinoDoc, string> FreeName) where T : ModelComponent, new();

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotWeight {
    public sealed record Default() : PlotWeight;

    public sealed record NoPlot() : PlotWeight;

    public sealed record Width : PlotWeight {
        internal Width(Length value) => Value = value;

        public Length Value { get; }
    }

    public static PlotWeight Of(LineWidth width) => new Width(width.Width);

    public static PlotWeight FromHost(double millimeters) =>
        millimeters switch {
            > 0.0 => new Width(Length.FromMillimeters(millimeters)),
            < 0.0 => new NoPlot(),
            _ => new Default(),
        };

    public static Option<PlotWeight> FromInheritable(double millimeters) =>
        millimeters < -1.0 ? None : Some(FromHost(millimeters));

    public static double ToInheritable(Option<PlotWeight> weight) =>
        weight.Match(Some: static some => some.ToHost(), None: static () => -10.0);

    public double ToHost() =>
        Switch(@default: static _ => 0.0, noPlot: static _ => -1.0, width: static width => width.Value.Millimeters.ToDouble());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeRef {
    public sealed record Continuous() : LinetypeRef;

    public sealed record Row(ComponentRef<Linetype> Address) : LinetypeRef;

    public static Option<LinetypeRef> FromHost(int index) =>
        index switch {
            -1 => Some<LinetypeRef>(new Continuous()),
            >= 0 => Some<LinetypeRef>(new Row(new ComponentRef<Linetype>.ByIndex(index))),
            _ => Option<LinetypeRef>.None,
        };

    public IO<int> Resolve(RhinoDoc doc) =>
        Switch(
            doc,
            continuous: static (_, _) => IO.pure(-1),
            row: static (document, row) => TableOps.Find(document.Linetypes, row.Address, includeDeleted: false).Map(static found => found.Index));
}

public readonly record struct SelectionOptions(bool SyncHighlight, bool Persistent, bool IgnoreGrips, bool IgnoreLayerLocks, bool IgnoreLayerVisibility);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionEdit {
    public sealed record Set(bool Select) : SelectionEdit;

    public sealed record Replace() : SelectionEdit;

    public int Apply(ObjectTable table, Seq<Guid> ids, SelectionOptions options) =>
        Switch(
            (Table: table, Ids: ids, Options: options),
            set: static (state, set) => state.Table.Select(
                state.Ids, set.Select, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility),
            replace: static (state, _) => state.Table.SetSelectedObjects(
                state.Ids, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisibilityState {
    public sealed record Hidden(Option<string> Group) : VisibilityState;

    public sealed record Shown() : VisibilityState;

    public sealed record Locked() : VisibilityState;

    public sealed record Unlocked() : VisibilityState;

    public Fin<Unit> Apply(ObjectTable table, RhinoObject target, bool ignoreLayerMode, int index) =>
        Switch(
            (Table: table, Target: target, IgnoreLayerMode: ignoreLayerMode, Index: index),
            hidden: static (state, hidden) => Refused.Unless(
                hidden.Group.Match(Some: group => state.Table.Hide(state.Target, state.IgnoreLayerMode, group), None: () => state.Table.Hide(state.Target, state.IgnoreLayerMode)),
                nameof(ObjectTable.Hide), state.Index),
            shown: static (state, _) => Refused.Unless(state.Table.Show(state.Target, state.IgnoreLayerMode), nameof(ObjectTable.Show), state.Index),
            locked: static (state, _) => Refused.Unless(state.Table.Lock(state.Target, state.IgnoreLayerMode), nameof(ObjectTable.Lock), state.Index),
            unlocked: static (state, _) => Refused.Unless(state.Table.Unlock(state.Target, state.IgnoreLayerMode), nameof(ObjectTable.Unlock), state.Index));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransformMode {
    public sealed record Move(bool DeleteOriginal) : TransformMode;

    public sealed record History() : TransformMode;

    public Fin<Guid> Apply(ObjectTable table, RhinoObject target, Transform xform, int index) =>
        Switch(
            (Table: table, Target: target, Xform: xform, Index: index),
            move: static (state, move) => Conversions.Present(state.Table.Transform(state.Target, state.Xform, move.DeleteOriginal))
                .ToFin(new Refused(nameof(ObjectTable.Transform), Refused.Declined, state.Index)),
            history: static (state, _) => Conversions.Present(state.Table.TransformWithHistory(state.Target, state.Xform))
                .ToFin(new Refused(nameof(ObjectTable.TransformWithHistory), Refused.Declined, state.Index)));
}

public sealed record GeometryPair(GeometryBase Geometry, Option<ObjectAttributes> Attributes);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp {
    public sealed record Add(Seq<(GeometryPair Row, Option<HistoryRecord> History)> Rows, bool Reference) : TableOp;

    public sealed record Replace(Guid Id, GeometryBase Geometry, bool IgnoreModes) : TableOp;

    public sealed record Delete(ObjectTarget Target, bool Quiet, bool IgnoreModes) : TableOp;

    public sealed record Move(ObjectTarget Target, Transform Xform, TransformMode Mode) : TableOp;

    public sealed record ModifyAttributes(ObjectTarget Target, Func<ObjectAttributes, IO<Unit>> Edit, bool Quiet) : TableOp;

    public sealed record State(ObjectTarget Target, VisibilityState Next, bool IgnoreLayerMode) : TableOp;

    public sealed record Undelete(ObjectTarget Target) : TableOp;

    public sealed record PointCloud(int X, int Y, int Z, Box Box, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, bool Reference) : TableOp;

    public sealed record ImportPage(string Path, Guid MainViewportId, string PageName) : TableOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImmediateOp {
    public sealed record Selection(ObjectTarget Target, SelectionEdit Edit, SelectionOptions Options) : ImmediateOp;

    public sealed record ClearSelection(bool IgnorePersistent) : ImmediateOp;

    public sealed record Flash(ObjectTarget Target, bool UseSelectionColor) : ImmediateOp;

    public sealed record Purge(ObjectTarget Target) : ImmediateOp;
}

[Union]
public abstract partial record UndoOp {
    public sealed record Undo() : UndoOp;

    public sealed record Redo() : UndoOp;

    public sealed record ClearUndo(bool PurgeDeleted, Option<uint> Serial) : UndoOp;

    public sealed record ClearRedo() : UndoOp;
}

public sealed record CustomUndo(string Description, IO<Unit> Undo, IO<Unit> Redo, CallbackSite Site);

public sealed record TableSpec<TSpec>(Seq<(string Name, TSpec Spec)> Rows, bool Exclusive);

public sealed record TableUpsert<T, TSpec>(string Name, Option<T> Existing, TSpec Spec);

public sealed record TablePlan<T, TSpec>(Seq<TableUpsert<T, TSpec>> Upserts, Seq<T> Prunes);

// --- [TABLES] --------------------------------------------------------------------------
public static class TableKinds {
    public static TableKind<Layer> Layers { get; } = new(
        static doc => doc.Layers,
        static live => {
            Layer staged = new();
            staged.CopyAttributesFrom(live);
            return staged;
        },
        static (doc, row) => doc.Layers.Add(row),
        static (doc, row, index) => doc.Layers.Modify(row, index, quiet: true),
        static doc => doc.Layers.GetUnusedLayerName());

    public static TableKind<Linetype> Linetypes { get; } = new(
        static doc => doc.Linetypes,
        static live => new Linetype(live),
        static (doc, row) => doc.Linetypes.Add(row),
        static (doc, row, index) => doc.Linetypes.Modify(row, index, quiet: true),
        static doc => doc.Linetypes.GetUnusedLinetypeName());

    public static TableKind<DimensionStyle> DimensionStyles { get; } = new(
        static doc => doc.DimStyles,
        static live => live.Duplicate(),
        static (doc, row) => doc.DimStyles.Add(row, reference: false),
        static (doc, row, index) => doc.DimStyles.Modify(row, index, quiet: true),
        static doc => doc.DimStyles.GetUnusedStyleName());

    public static TableKind<HatchPattern> HatchPatterns { get; } = new(
        static doc => doc.HatchPatterns,
        static live => new HatchPattern(live),
        static (doc, row) => doc.HatchPatterns.Add(row),
        static (doc, row, index) => doc.HatchPatterns.Modify(row, index, quiet: true),
        static doc => doc.HatchPatterns.GetUnusedHatchPatternName());

    public static TableKind<SectionStyle> SectionStyles { get; } = new(
        static doc => doc.SectionStyles,
        static live => new SectionStyle(live),
        static (doc, row) => doc.SectionStyles.Add(row),
        static (doc, row, index) => doc.SectionStyles.Modify(row, index, quiet: true),
        static doc => doc.SectionStyles.GetUnusedSectionStyleName());

    public static TableKind<Markup> Markups { get; } = new(
        static doc => doc.Markups,
        static live => new Markup(live),
        static (doc, row) => doc.Markups.Add(row),
        static (doc, row, index) => doc.Markups.Modify(row, index, quiet: true),
        static doc => doc.Markups.GetUnusedMarkupName());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TableOps {
    // --- [ADDRESSES]
    public static IO<T> Find<T>(RhinoDocCommonTable<T> table, ComponentRef<T> address, bool includeDeleted) where T : ModelComponent =>
        IO.lift(() => address.Switch(
                table,
                byId: static (rows, byId) => Optional(rows.Document.Manifest.FindId<T>(byId.Id)),
                byIndex: static (rows, byIndex) => Optional(rows.Document.Manifest.FindIndex<T>(byIndex.Index)),
                byName: static Fin<Option<T>> (rows, byName) =>
                    ModelComponent.ModelComponentTypeRequiresUniqueName(rows.ComponentType) && !ModelComponent.ModelComponentTypeIncludesParent(rows.ComponentType)
                        ? Optional(rows.Document.Manifest.FindName<T>(byName.Name, Guid.Empty))
                        : Error.New($"{rows.ComponentType} names do not identify one row"))
            .Bind(found => found.Filter(row => includeDeleted || !row.IsDeleted).ToFin(new NotFound($"{table.ComponentType}", $"{address}"))));

    public static IO<int> Index<T>(RhinoDocCommonTable<T> table, Option<ComponentRef<T>> address) where T : ModelComponent =>
        address.Traverse(row => Find(table, row, includeDeleted: false)).As().Map(static found => Conversions.Unset(found.Map(static row => row.Index)));

    public static IEqualityComparer<string> Names<T>() where T : ModelComponent =>
        EqualityComparer<string>.Create(static (left, right) => NameKey<T>(left) == NameKey<T>(right), static name => NameKey<T>(name).GetHashCode());

    public static IO<Unit> Named<T>(T staged, Option<string> name) where T : ModelComponent =>
        IO.lift(() => name.Iter(value => staged.Name = value));

    private static NameHash NameKey<T>(string? name) where T : ModelComponent => new(name, Guid.Empty, ManifestTable.GetModelComponentTypeFromGenericType<T>());

    // --- [ROWS]
    public static IO<Seq<T>> Rows<T>(RhinoDocCommonTable<T> table) where T : ModelComponent =>
        IO.lift(() => Conversions.Rows(table).Filter(static row => !row.IsReference && Conversions.Present(row.Name).IsSome).Strict());

    public static IO<int> AddRow<T>(RhinoDoc doc, TableKind<T> kind, Func<T, IO<Unit>> edit) where T : ModelComponent, new() =>
        DisposalOps.Within(static () => new T(), staged =>
            from edited in edit(staged)
            from index in IO.lift(() => Conversions.Required(kind.Add(doc, staged), nameof(TableKind<>.Add)))
            select index);

    public static IO<Unit> ModifyRow<T>(RhinoDoc doc, TableKind<T> kind, ComponentRef<T> address, Func<T, IO<Unit>> edit) where T : ModelComponent, new() =>
        from live in Find(kind.Table(doc), address, includeDeleted: false)
        from modified in DisposalOps.Within(() => kind.Copy(live), staged => edit(staged).Bind(_ => Modified(doc, kind, staged, live.Index)))
        select modified;

    public static Fin<TablePlan<T, TSpec>> Plan<T, TSpec>(Seq<T> rows, TableSpec<TSpec> spec) where T : ModelComponent =>
        Plan(rows, spec, static row => row.Name, Names<T>());

    public static Fin<TablePlan<T, TSpec>> Plan<T, TSpec>(Seq<T> rows, TableSpec<TSpec> spec, Func<T, string> name, IEqualityComparer<string> names) =>
        from named in Callbacks.Unique(spec.Rows, static row => row.Name, nameof(TableUpsert<,>.Name), names)
        let held = rows.ToLookup(name, names)
        let wanted = named.ToLookup(static row => row.Name, names)
        select new TablePlan<T, TSpec>(
            named.Map(row => new TableUpsert<T, TSpec>(row.Name, toSeq(held[row.Name]).Head, row.Spec)).Strict(),
            spec.Exclusive ? rows.Filter(row => !wanted.Contains(name(row))).Strict() : Seq<T>());

    public static IO<Seq<(string Name, int Index)>> Upsert<T, TSpec>(RhinoDoc doc, TableKind<T> kind, Seq<TableUpsert<T, TSpec>> upserts, Func<T, TSpec, IO<Unit>> write, Func<T, T, IO<bool>> unchanged)
        where T : ModelComponent, new() =>
        upserts.TraverseM(upsert => upsert.Existing.Match(
                Some: row =>
                    DisposalOps.Within(() => kind.Copy(row), staged =>
                     from named in Named(staged, Some(upsert.Name))
                     from written in write(staged, upsert.Spec)
                     from same in unchanged(row, staged)
                     from modified in unless(same && string.Equals(row.Name, upsert.Name, StringComparison.Ordinal), Modified(doc, kind, staged, row.Index)).As()
                     select (upsert.Name, row.Index)),
                None: () => AddRow(doc, kind, staged => Named(staged, Some(upsert.Name)).Bind(_ => write(staged, upsert.Spec))).Map(index => (upsert.Name, index))))
            .As();

    private static IO<Unit> Modified<T>(RhinoDoc doc, TableKind<T> kind, T staged, int index) where T : ModelComponent, new() =>
        IO.lift(() => Refused.Unless(kind.Modify(doc, staged, index), nameof(TableKind<>.Modify)));

    // --- [ATTRIBUTES]
    public static IO<T> WithAttributes<T>(Func<ObjectAttributes> defaults, Option<ObjectAttributes> attributes, Func<ObjectAttributes, IO<T>> body) =>
        attributes.Match(Some: body, None: () => DisposalOps.Within(defaults, body));

    public static IO<T> WithAttributes<T>(Func<ObjectAttributes> defaults, Seq<GeometryPair> members, Func<Seq<GeometryBase>, Seq<ObjectAttributes>, IO<T>> body) =>
        members.FoldBack(
            (Seq<ObjectAttributes> rows) => body(members.Map(static member => member.Geometry), rows),
            (next, member) => rows => WithAttributes(defaults, member.Attributes, value => next(rows.Add(value))))(Seq<ObjectAttributes>());

    // --- [RECORDED]
    public static IO<Seq<Guid>> Apply(RhinoDoc doc, TableOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => Callbacks.Each(add.Rows.Map(row => Add(document, row.Row, row.History, add.Reference))),
            replace: static (document, replace) => IO.lift(() =>
                Refused.Unless(document.Objects.Replace(replace.Id, replace.Geometry, replace.IgnoreModes), Seq(replace.Id), nameof(ObjectTable.Replace))),
            delete: static (document, delete) => Each(document, delete.Target, found => document.Objects.Delete(found, delete.Quiet, delete.IgnoreModes), nameof(ObjectTable.Delete)),
            move: static (document, move) => Each(document, move.Target, (found, _, index) => IO.lift(() => move.Mode.Apply(document.Objects, found, move.Xform, index))),
            modifyAttributes: static (document, modify) => Each(document, modify.Target, (found, id, index) =>
                DisposalOps.Within(() => found.Attributes.Duplicate(), copy =>
                 from edited in modify.Edit(copy)
                 from landed in IO.lift(() => Refused.Unless(document.Objects.ModifyAttributes(found, copy, modify.Quiet), id, nameof(ObjectTable.ModifyAttributes), index))
                 select landed)),
            state: static (document, state) => Each(document, state.Target, (found, id, index) => IO.lift(() => state.Next.Apply(document.Objects, found, state.IgnoreLayerMode, index).Map(_ => id))),
            undelete: static (document, undelete) => Each(document, undelete.Target, document.Objects.Undelete, nameof(ObjectTable.Undelete)),
            pointCloud: static (document, cloud) => WithAttributes(document.CreateDefaultAttributes, cloud.Attributes, attributes => IO.lift(() => Conversions.Required(
                    document.Objects.AddOrderedPointCloud(cloud.X, cloud.Y, cloud.Z, cloud.Box.GetCorners(), attributes, cloud.History.ValueUnsafe(), cloud.Reference),
                    nameof(ObjectTable.AddOrderedPointCloud))
                .Map(static id => Seq(id)))),
            importPage: static (document, page) => IO.lift(() =>
                Refused.Unless(document.Views.ImportPageView(page.Path, page.MainViewportId, page.PageName), Seq<Guid>(), nameof(ViewTable.ImportPageView))));

    public static IO<Guid> Add(RhinoDoc doc, GeometryPair row, Option<HistoryRecord> history, bool reference) =>
        WithAttributes(doc.CreateDefaultAttributes, row.Attributes, attributes =>
            IO.lift(() => Conversions.Required(doc.Objects.Add(row.Geometry, attributes, history.ValueUnsafe(), reference), nameof(ObjectTable.Add))));

    public static IO<Seq<Guid>> Place(RhinoDoc doc, Func<Action<IDisposable>, IO<Seq<(GeometryPair Row, Option<HistoryRecord> History)>>> created, bool reference) =>
        DisposalOps.Within((Action<IDisposable> retain) =>
            from products in created(retain)
            from ids in Apply(doc, new TableOp.Add(products, reference)).Uninterruptible()
            select ids, DisposalOps.Release);

    public static IO<Committed<Seq<Guid>>> Recorded(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<CustomUndo> custom, Seq<TableOp> steps) =>
        Commits.Commit(doc, name, redraw,
            from registered in custom.TraverseM(undo => Register(doc, undo)).As()
            from touched in steps.TraverseM(step => Apply(doc, step)).As()
            select touched.Flatten());

    public static IO<Unit> Register(RhinoDoc doc, CustomUndo undo) =>
        IO.lift(() => Refused.Unless(
            doc.AddCustomUndoEvent(undo.Description, Callbacks.Handler<CustomUndoEventArgs>(args => undo.Undo.Bind(_ => Register(args.Document, undo with { Undo = undo.Redo, Redo = undo.Undo })), undo.Site)),
            nameof(RhinoDoc.AddCustomUndoEvent)));

    public static IO<Unit> Undoable<T>(RhinoDoc doc, string name, IO<T> prior, Func<T, IO<Unit>> assign, T next, CallbackSite site) =>
        from held in prior
        from registered in Register(doc, new CustomUndo(name, assign(held), assign(next), site))
        from written in assign(next)
        select written;

    private static IO<Seq<Guid>> Each(RhinoDoc doc, ObjectTarget target, Func<RhinoObject, Guid, int, IO<Guid>> element) =>
        target.Objects(doc).Bind(objects => Callbacks.Each(objects.Map((found, index) => element(found, found.Id, index)).Strict()));

    private static IO<Seq<Guid>> Each(RhinoDoc doc, ObjectTarget target, Func<RhinoObject, bool> call, string member) =>
        Each(doc, target, (found, id, index) => IO.lift(() => Refused.Unless(call(found), id, member, index)));

    // --- [IMMEDIATE]
    public static IO<int> Immediate(RhinoDoc doc, RedrawPolicy redraw, ImmediateOp op) =>
        Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            selection: static (document, selection) =>
                from objects in selection.Target.Objects(document)
                from count in IO.lift(() => selection.Edit.Apply(document.Objects, objects.Map(static found => found.Id).Strict(), selection.Options))
                select count,
            clearSelection: static (document, clear) => IO.lift(() => document.Objects.UnselectAll(clear.IgnorePersistent)),
            flash: static (document, flash) =>
                from objects in flash.Target.Objects(document)
                from flashed in IO.lift(() => document.Views.FlashObjects(objects, flash.UseSelectionColor))
                select objects.Count,
            purge: static (document, purge) => Each(document, purge.Target, document.Objects.Purge, nameof(ObjectTable.Purge)).Map(static ids => ids.Count)));

    // --- [UNDO]
    public static IO<Unit> Undo(RhinoDoc doc, RedrawPolicy redraw, UndoOp op) =>
        from closed in IO.lift(() => Conversions.Present(doc.CurrentUndoRecordSerialNumber).Case is uint serial ? Error.New($"undo record {serial} is open") : Fin.Succ(unit))
        from stepped in Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            undo: static (document, _) => Commits.Step(document, document.Undo, nameof(RhinoDoc.Undo)),
            redo: static (document, _) => Commits.Step(document, document.Redo, nameof(RhinoDoc.Redo)),
            clearUndo: static (document, clear) => IO.lift(() => clear.Serial.Match(
                Some: serial => document.ClearUndoRecords(serial, clear.PurgeDeleted),
                None: () => document.ClearUndoRecords(clear.PurgeDeleted))),
            clearRedo: static (document, _) => IO.lift(document.ClearRedoRecords)))
        select stepped;
}

internal static class Markups {
    // --- [ROWS]
    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        TableOps.Rows(doc.Markups).Map(static rows => rows.Map(Entry));

    internal static IO<Seq<Guid>> Members(RhinoDoc doc, NamedEntry entry) =>
        Found(doc, entry.Name).Map(index => Conversions.Rows(doc.Markups.GetObjectIds(index)));

    internal static IO<Option<NamedEntry>> Current(RhinoDoc doc, ViewportTarget target) =>
        from shown in Viewports.With(doc, target, row => IO.lift(() => Optional(doc.Views.ActiveView).Exists(view => view.ActiveViewportID == row.Viewport.Id)))
        from active in IO.lift(() => Conversions.Present(doc.Markups.ActiveIndex).Bind(index => Optional(doc.Markups.FindIndex(index))).Map(Entry))
        select active.Filter(_ => shown);

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name) =>
        from index in Found(doc, entry.Name)
        from renamed in Named(doc, index, name)
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry) =>
        Found(doc, entry.Name).Bind(index => Deleted(doc, index)).Map(static _ => (NamedWrite)new NamedWrite.Recorded());

    // --- [STATES]
    internal static IO<NamedWrite> Save(RhinoDoc doc, NamedState.Markup state) =>
        from ended in Ended(doc)
        from prior in IO.lift(() => Conversions.Present(doc.Markups.Find(state.Name)))
        from cleared in prior.Traverse(index => Deleted(doc, index)).As()
        from index in Viewports.With(doc, state.Source, row => IO.lift(() => Conversions.Required(
            doc.Markups.Create(row.View, state.Geometry.Map(static pair => pair.Geometry), state.Geometry.Map(static pair => pair.Attributes.ValueUnsafe())),
            nameof(MarkupTable.Create))))
        from named in Named(doc, index, state.Name)
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<NamedWrite> Restore(RhinoDoc doc, NamedRestore.Markup target) =>
        from index in Found(doc, target.Name)
        from held in Hold(doc)
        from viewed in DisposalOps.OnFailure(IO.lift(() => Refused.Unless(doc.Markups.View(index), nameof(MarkupTable.View))), NamedWrite.Settled([held]))
        select held;

    // --- [HOLD]
    internal static IO<NamedWrite> Hold(RhinoDoc doc) =>
        NamedViews.Hold(doc, None).Map(held => (NamedWrite)new NamedWrite.Compensated(Ended(doc).Bind(_ => NamedWrite.Reverted([held])), NamedWrite.Released([held])));

    // --- [HOST]
    private static IO<int> Found(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.Markups.Find(name)).ToFin(new NotFound(nameof(NamedTable.Markups), name)));

    private static IO<Unit> Named(RhinoDoc doc, int index, StateName name) =>
        TableOps.ModifyRow(doc, TableKinds.Markups, new ComponentRef<Markup>.ByIndex(index), staged => TableOps.Named(staged, Some((string)name)));

    private static IO<Unit> Deleted(RhinoDoc doc, int index) =>
        IO.lift(() => Refused.Unless(doc.Markups.Delete(index, quiet: true), nameof(MarkupTable.Delete)));

    private static IO<Unit> Ended(RhinoDoc doc) =>
        IO.lift(() => doc.MarkupMode is MarkupMode.None ? unit : Refused.Unless(doc.Markups.Cancel(), nameof(MarkupTable.Cancel)));

    private static NamedEntry Entry(Markup row) =>
        new(NamedTable.Markups, row.Name, Conversions.Present(row.Id));
}
