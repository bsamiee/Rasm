using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NamedSelection(Guid Id, string Name, Seq<Guid> Objects, Seq<(Guid ObjectId, ComponentIndex Component)> SubObjects) {
    public static IO<Seq<NamedSelection>> Read(RhinoDoc doc) =>
        NamedSelections.Model(doc).Bind(static model => IO.lift(() => NamedSelections.Rows(model)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NamedSelections {
    // --- [ROWS]
    private const string Separator = "::";
    private static readonly SelectionOptions Unrestricted = new(SyncHighlight: true, Persistent: true, IgnoreGrips: true, IgnoreLayerLocks: true, IgnoreLayerVisibility: true);

    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        NamedSelection.Read(doc).Map(static rows => rows.Map(static row => new NamedEntry(NamedTable.Selections, row.Name, Some(row.Id))).Strict());

    internal static Fin<Unit> Accepts(StateName name) =>
        ((string)name).Contains(Separator, StringComparison.Ordinal)
            ? Error.New(ErrorCodes.ValidationFailed, $"{nameof(SelectNamedSelections)} reads {name} as a {Separator} child path")
            : unit;

    internal static IO<Seq<Guid>> Members(RhinoDoc doc, NamedEntry entry) =>
        Found(doc, entry.Name).Map(static rows => rows.Bind(static row => row.Objects).Distinct().Strict());

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name) =>
        from model in Model(doc)
        from found in Found(doc, entry.Name)
        from renamed in Renamed(model, entry.Name, name)
        select (NamedWrite)new NamedWrite.Compensated(Renamed(model, name, entry.Name), IO.pure(unit));

    internal static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry) =>
        from model in Model(doc)
        from held in Found(doc, entry.Name)
        from deleted in Deleted(model, entry.Name)
        select (NamedWrite)new NamedWrite.Compensated(
            Saved(doc, model, entry.Name, held.Bind(static row => row.Objects).Distinct().Strict(), held.Bind(static row => row.SubObjects).Distinct().Strict()),
            IO.pure(unit));

    internal static Seq<NamedSelection> Rows(object model) =>
        toSeq(TreeGridCollection(model)).Map(static item => NamedSelectionOf(item)).Map(static selection => new NamedSelection(
            SelectionId(selection),
            SelectionName(selection),
            Items(StandardObjectTypes(selection)).Bind(static standard => Live(Items(StandardObjects(standard)))).Map(ChildId).Distinct().Strict(),
            Items(SubObjects(selection)).Bind(static part => Live(Items(SubObjectChildren(part))))
                .Map(static child => (ObjectId: ChildId(child), Component: ChildComponent(child))).Strict())).Strict();

    private static IO<Seq<NamedSelection>> Found(RhinoDoc doc, string name) =>
        Named(doc, name).Bind(rows => IO.lift(() => rows.IsEmpty ? new NotFound(nameof(NamedTable.Selections), name) : Fin.Succ(rows)));

    private static IO<Seq<NamedSelection>> Named(RhinoDoc doc, string name) =>
        NamedSelection.Read(doc).Map(rows => rows.Filter(row => NamedTable.Selections.Comparer.Equals(row.Name, name)).Strict());

    private static Seq<object> Items(object list) => toSeq((IEnumerable<object>)list);

    private static Seq<object> Live(Seq<object> children) => children.Filter(static child => !ChildDeleted(child));

    // --- [STATES]
    internal static IO<NamedWrite> Save(RhinoDoc doc, NamedState.Selection state) =>
        from model in Model(doc)
        from objects in state.Objects.Objects(doc)
        from prior in Named(doc, state.Name)
        from saved in Saved(doc, model, state.Name, objects.Map(static found => found.Id).Distinct().Strict(), [])
        select (NamedWrite)new NamedWrite.Compensated(
            prior.Head.Match(
                Some: row => Saved(doc, model, state.Name, row.Objects, row.SubObjects),
                None: () => Deleted(model, state.Name)),
            IO.pure(unit));

    internal static IO<NamedWrite> Restore(RhinoDoc doc, NamedRestore.Selection target) =>
        from model in Model(doc)
        from found in Found(doc, target.Name)
        from held in ObjectTarget.Selection(doc)
        from restored in DisposalOps.OnFailure(
            target.Edit.Switch(
                (Document: doc, Model: model, Target: target, Held: held),
                set: static (at, set) => set.Select
                    ? IO.lift(() => SelectNamedSelections(at.Model, at.Target.Name)).Bind(_ => ObjectTarget.Reselected(at.Held))
                    : TableOps.Immediate(at.Document, new RedrawPolicy.Silent(),
                        new ImmediateOp.Selection(new ObjectTarget.Named(new NamedEntry(NamedTable.Selections, at.Target.Name, None)), set, Unrestricted)).Map(static _ => unit),
                replace: static (at, _) => IO.lift(() => SelectNamedSelections(at.Model, at.Target.Name))),
            Reverted(doc, held))
        select (NamedWrite)new NamedWrite.Compensated(Reverted(doc, held), IO.pure(unit));

    private static IO<Unit> Saved(RhinoDoc doc, object model, string name, Seq<Guid> objects, Seq<(Guid ObjectId, ComponentIndex Component)> parts) =>
        from created in DisposalOps.Within(
            ObjectTarget.Selection(doc),
            _ =>
                from picked in TableOps.Immediate(doc, new RedrawPolicy.Silent(), new ImmediateOp.Selection(new ObjectTarget.Ids(objects), new SelectionEdit.Replace(), Unrestricted))
                from parted in toSeq(parts.GroupBy(static part => part.ObjectId)).TraverseM(owned =>
                    from owner in IO.lift(() => Refused.UnlessFound(doc.Objects.FindId(owned.Key), nameof(ObjectTable.FindId)))
                    from selected in RhinoObjects.SelectSubObjects(owner, toSeq(owned).Map(static part => part.Component), ObjectSelection.Persistent, syncHighlight: true)
                    select selected).As()
                from made in IO.lift(() => CreateNamedSectionButtonClick(model, name, RunMode.Scripted, replaceExisting: true, parent: null))
                select unit,
            held => Reverted(doc, held))
        from landed in Named(doc, name)
        from confirmed in IO.lift(() => Refused.Unless(
            landed.Head.Exists(row => toHashSet(row.Objects) == toHashSet(objects) && toHashSet(row.SubObjects) == toHashSet(parts)),
            nameof(CreateNamedSectionButtonClick)))
        select unit;

    private static IO<Unit> Renamed(object model, string name, string newName) =>
        IO.lift(() => {
            RenamedNamedSelection(model, name, newName);
            SaveSelections(model);
        });

    private static IO<Unit> Deleted(object model, string name) =>
        IO.lift(() => {
            BuildSelectionLookup(model);
            DeleteNamedSelection(model, name);
            SaveSelections(model);
        });

    // --- [HOLD]
    internal static IO<NamedWrite> Hold(RhinoDoc doc) =>
        ObjectTarget.Selection(doc).Map(held => (NamedWrite)new NamedWrite.Compensated(Reverted(doc, held), IO.pure(unit)));

    private static IO<Unit> Reverted(RhinoDoc doc, Seq<(RhinoObject Object, bool Persistent)> held) =>
        IO.lift(doc.Objects.UnselectAll).Bind(_ => ObjectTarget.Reselected(held));

    // --- [MARKS]
    internal static HostEvent<Option<uint>> Refreshed { get; } = new(
        typeof(INotifyPropertyChanged),
        nameof(INotifyPropertyChanged.PropertyChanged),
        static (deliver, site, retain) =>
            from watched in IO.lift(static () => Ref(LanguageExt.HashSet<uint>.Empty))
            from attached in EventKind.DocumentArrived.Inline(serial =>
                    from fresh in atomicIO(() => {
                        LanguageExt.HashSet<uint> seen = watched.Value;
                        watched.Value = seen.TryAdd(serial);
                        return !seen.Contains(serial);
                    })
                    from doc in IO.lift(() => Refused.UnlessFound(RhinoDoc.FromRuntimeSerialNumber(serial), nameof(RhinoDoc.FromRuntimeSerialNumber)))
                    from model in Model(doc)
                    from notifier in IO.lift(() => Conversions.Cast<INotifyPropertyChanged>(model))
                    from observed in when(fresh, Subscriptions.Host<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                            typeof(INotifyPropertyChanged), handler => notifier.PropertyChanged += handler, handler => notifier.PropertyChanged -= handler,
                            static handler => handler.Invoke, nameof(INotifyPropertyChanged.PropertyChanged))
                        .Choose(args => Callbacks.Found(string.Equals(args.PropertyName, Collection, StringComparison.Ordinal), Some(serial)))
                        .Attach(deliver, site, retain)).As()
                    select unit,
                site.Sink, retain)
            select attached);

    // --- [HOST]
    private const string ModelType = "Commands.ViewModels.NamedSelectionsViewModel, Commands";
    private const string ItemType = "Commands.ViewModels.NsGridItem, Commands";
    private const string SelectionType = "Commands.ViewModels.NamedSelection, Commands";
    private const string StandardType = "Commands.ViewModels.NsStandardObject, Commands";
    private const string PartType = "Commands.ViewModels.NsSubObject, Commands";
    private const string ChildType = "Commands.ViewModels.NsGridChildObject, Commands";
    private const string StandardList = "System.Collections.Generic.List`1[[" + StandardType + "]]";
    private const string PartList = "System.Collections.Generic.List`1[[" + PartType + "]]";
    private const string ChildList = "System.Collections.Generic.List`1[[" + ChildType + "]]";
    private const string Collection = "TreeGridCollection";

    internal static IO<object> Model(RhinoDoc doc) =>
        from type in IO.lift(static () => Refused.UnlessFound(Type.GetType(ModelType), nameof(Type.GetType)))
        from model in IO.lift(() => doc.RuntimeData.GetValue(type, static owner => Created(owner.RuntimeSerialNumber)))
        from watching in IO.lift(() => IsDocumentEventWatchingEnabled(model))
        from read in when(!watching, IO.lift(() => ReadSavedNamedSelectionArray(model))).As()
        select model;

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(ModelType)]
    private static extern object Created(uint documentRuntimeSerial);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsDocumentEventWatchingEnabled")]
    private static extern bool IsDocumentEventWatchingEnabled([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ReadSavedNamedSelectionArray))]
    private static extern void ReadSavedNamedSelectionArray([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_" + Collection)]
    private static extern TreeGridItemCollection TreeGridCollection([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(CreateNamedSectionButtonClick))]
    private static extern void CreateNamedSectionButtonClick([UnsafeAccessorType(ModelType)] object model, string selectionName, RunMode mode, bool replaceExisting, Control? parent);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(SelectNamedSelections))]
    private static extern void SelectNamedSelections([UnsafeAccessorType(ModelType)] object model, string selectionName);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(RenamedNamedSelection))]
    private static extern void RenamedNamedSelection([UnsafeAccessorType(ModelType)] object model, string existingName, string newName);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(BuildSelectionLookup))]
    private static extern void BuildSelectionLookup([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(DeleteNamedSelection))]
    private static extern void DeleteNamedSelection([UnsafeAccessorType(ModelType)] object model, string selectionName);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(SaveSelections))]
    private static extern void SaveSelections([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_NamedSelection")]
    [return: UnsafeAccessorType(SelectionType)]
    private static extern object NamedSelectionOf([UnsafeAccessorType(ItemType)] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Id")]
    private static extern Guid SelectionId([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Name")]
    private static extern string SelectionName([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_StandardObjectTypes")]
    [return: UnsafeAccessorType(StandardList)]
    private static extern object StandardObjectTypes([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SubObjects")]
    [return: UnsafeAccessorType(PartList)]
    private static extern object SubObjects([UnsafeAccessorType(SelectionType)] object selection);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Objects")]
    [return: UnsafeAccessorType(ChildList)]
    private static extern object StandardObjects([UnsafeAccessorType(StandardType)] object standard);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SubObjectChildren")]
    [return: UnsafeAccessorType(ChildList)]
    private static extern object SubObjectChildren([UnsafeAccessorType(PartType)] object part);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Id")]
    private static extern Guid ChildId([UnsafeAccessorType(ChildType)] object child);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsDeleted")]
    private static extern bool ChildDeleted([UnsafeAccessorType(ChildType)] object child);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ComponentIndex")]
    private static extern ComponentIndex ChildComponent([UnsafeAccessorType(ChildType)] object child);
}
