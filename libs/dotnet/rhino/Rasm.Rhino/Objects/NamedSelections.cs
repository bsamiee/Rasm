using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SelectionChange {
    public static readonly SelectionChange Added = new();
    public static readonly SelectionChange Removed = new();
    public static readonly SelectionChange Replaced = new();

    public bool Selects => this != Removed;

    public bool Clears => this == Replaced;
}

internal enum SelectionOperation { Save, Select, Delete, Rename }

// --- [MODELS] --------------------------------------------------------------------------

public sealed record NamedSelection(Guid Id, string Name, Seq<Guid> Objects, Seq<(Guid ObjectId, ComponentIndex Component)> SubObjects) {
    public static IO<Seq<NamedSelection>> Read(RhinoDoc doc) => NamedSelections.SelectionModel(doc).Map(NamedSelections.Selections);
}

public sealed record NamedSelectionState(StateName Name, ObjectTarget Objects) : NamedState(NamedSelections.Table, Name) {
    internal override IO<NamedWrite> Save(RhinoDoc doc) =>
        from objects in Objects.Objects(doc)
        from prior in NamedSelections.Named(doc, Name)
        from saved in NamedSelections.Saved(doc, Name, objects.Map(static found => found.Id).Distinct().Strict(), [])
        select new NamedWrite(
            prior.Head.Match(Some: row => NamedSelections.Saved(doc, Name, row.Objects, row.SubObjects), None: () => NamedSelections.Scripted(doc, SelectionOperation.Delete, Name)),
            IO.pure(unit));
}

public sealed record NamedSelectionRestore(StateName Name, SelectionChange Change) : NamedRestore(NamedSelections.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) =>
        from found in NamedSelections.Found(doc, Name)
        from held in ObjectTarget.Selection(doc)
        from restored in (from cleared in when(Change.Clears, IO.lift(doc.Objects.UnselectAll).Map(unit)).As()
                          from changed in Change.Selects
                              ? NamedSelections.Scripted(doc, SelectionOperation.Select, Name)
                              : IO.lift(() => doc.Objects.Select(found.Bind(static row => row.Objects), @select: false, syncHighlight: true, persistentSelect: true, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true)).Map(unit)
                          select unit).Catch(error => NamedSelections.Reverted(doc, held) >> IO.fail<Unit>(error))
        select new NamedWrite(NamedSelections.Reverted(doc, held), IO.pure(unit));
}

// --- [TABLES] --------------------------------------------------------------------------

public static class NamedSelections {

    // --- [MARKS]

    private const char Quote = '"';

    private static HostEvent<Option<uint>> Refreshed { get; } = new(
        $"{Documents.Arrived.Label} | {Collection}",
        static deliver =>
            from scope in IO.env
            from watches in IO.lift(static () => Ref(HashMap<uint, Owned<Unit>>()))
            from arrived in Documents.Arrived.Attach(serial => Products.Replace(watches, serial, Watched(serial, deliver)).Scoped(scope))
            from closed in Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.CloseDocument += h, static h => RhinoDoc.CloseDocument -= h)
                .Attach(args => Products.Clear(watches, args.DocumentSerialNumber).Scoped(scope))
            select unit);

    // --- [ROWS]

    public static NamedTable Table { get; } = new(nameof(NamedSelections), StringComparer.OrdinalIgnoreCase) {
        Marks = [Refreshed],
        Entries = static doc => NamedSelection.Read(doc).Map(static rows => rows.Map(Entry).Strict()),
        Rename = static (doc, entry, name) =>
            from found in Found(doc, entry.Name)
            from renamed in Scripted(doc, SelectionOperation.Rename, entry.Name, name)
            select new NamedWrite(Scripted(doc, SelectionOperation.Rename, name, entry.Name), IO.pure(unit)),
        Delete = static (doc, entry) =>
            from held in Found(doc, entry.Name)
            from deleted in Scripted(doc, SelectionOperation.Delete, entry.Name)
            select new NamedWrite(Saved(doc, entry.Name, held.Bind(static row => row.Objects).Distinct().Strict(), held.Bind(static row => row.SubObjects).Distinct().Strict()), IO.pure(unit)),
        Hold = static (doc, _) => ObjectTarget.Selection(doc).Map(held => new NamedWrite(Reverted(doc, held), IO.pure(unit))),
        Accepts = Some<Func<StateName, Fin<Unit>>>(static name =>
            RhinoFault.Unless(!((string)name).Contains("::", StringComparison.Ordinal) && !((string)name).Contains(Quote, StringComparison.Ordinal), RhinoReason.Invalid, (string)name)),
        Members = Some<Func<RhinoDoc, NamedEntry, IO<Seq<Guid>>>>(static (doc, entry) => Found(doc, entry.Name).Map(static rows => rows.Bind(static row => row.Objects).Distinct().Strict())),
    };

    private static NamedEntry Entry(NamedSelection row) => new(Table, row.Name, Some(row.Id));

    // --- [SCRIPTS]

    internal static IO<Unit> Scripted(RhinoDoc doc, SelectionOperation option, params Seq<string> names) =>
        Documents.RunScript(doc, $"-_NamedSelections _{option} {string.Join(' ', names.Map(static name => $"{Quote}{name}{Quote}"))}", echo: false, display: None);

    internal static IO<Unit> Saved(RhinoDoc doc, string name, Seq<Guid> objects, Seq<(Guid ObjectId, ComponentIndex Component)> parts) =>
        from created in ObjectTarget.Selection(doc).Bind(held =>
            (from picked in IO.lift(() => doc.Objects.SetSelectedObjects(objects, syncHighlight: true, persistentSelect: true, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true))
             from parted in toSeq(parts.GroupBy(static part => part.ObjectId)).TraverseM(owned =>
                 from found in IO.lift(() => Conversions.Required(doc.Objects.FindId(owned.Key)))
                 from selected in found.SelectSubObjects(toSeq(owned).Map(static part => part.Component), ObjectSelection.Persistent, syncHighlight: true)
                 select selected).As()
             from made in Scripted(doc, SelectionOperation.Save, name)
             select unit).Finally(Reverted(doc, held)))
        select created;

    internal static IO<Unit> Reverted(RhinoDoc doc, Seq<(RhinoObject Object, bool Persistent)> held) =>
        IO.lift(doc.Objects.UnselectAll).Bind(_ => ObjectTarget.Reselected(held));

    // --- [SELECTION_MODEL]

    private const string ModelType = "Commands.ViewModels.NamedSelectionsViewModel, Commands";
    private const string ItemType = "Commands.ViewModels.NsGridItem, Commands";
    private const string SelectionType = "Commands.ViewModels.NamedSelection, Commands";
    private const string StandardType = "Commands.ViewModels.NsStandardObject, Commands";
    private const string PartType = "Commands.ViewModels.NsSubObject, Commands";
    private const string ChildType = "Commands.ViewModels.NsGridChildObject, Commands";
    private const string CollectionType = "Eto.Forms.TreeGridItemCollection, Eto";
    private const string StandardList = "System.Collections.Generic.List`1[[" + StandardType + "]]";
    private const string PartList = "System.Collections.Generic.List`1[[" + PartType + "]]";
    private const string ChildList = "System.Collections.Generic.List`1[[" + ChildType + "]]";
    private const string Collection = "TreeGridCollection";

    internal static IO<object> SelectionModel(RhinoDoc doc) =>
        from type in IO.lift(static () => Conversions.Required(Type.GetType(ModelType)))
        from model in IO.lift(() => doc.RuntimeData.GetValue(type, static owner => Created(owner.RuntimeSerialNumber)))
        from read in unless(IsDocumentEventWatchingEnabled(model), IO.lift(() => ReadSavedNamedSelectionArray(model))).As()
        select model;

    internal static Seq<NamedSelection> Selections(object model) =>
        Items(TreeGridCollection(model)).Map(static item => NamedSelectionOf(item)).Map(static selection => new NamedSelection(
            SelectionId(selection),
            SelectionName(selection),
            Items(StandardObjectTypes(selection)).Bind(static standard => Live(Items(StandardObjects(standard)))).Map(ChildId).Distinct().Strict(),
            Items(SubObjects(selection)).Bind(static part => Live(Items(SubObjectChildren(part)))).Map(static child => (ObjectId: ChildId(child), Component: ChildComponent(child))).Strict())).Strict();

    private static Seq<object> Items(object list) => toSeq(((IEnumerable)list).Cast<object>());

    private static Seq<object> Live(Seq<object> children) => children.Filter(static child => !ChildDeleted(child));

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(ModelType)]
    private static extern object Created(uint documentRuntimeSerial);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsDocumentEventWatchingEnabled")]
    private static extern bool IsDocumentEventWatchingEnabled([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void ReadSavedNamedSelectionArray([UnsafeAccessorType(ModelType)] object model);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_" + Collection)]
    [return: UnsafeAccessorType(CollectionType)]
    private static extern object TreeGridCollection([UnsafeAccessorType(ModelType)] object model);

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

    // --- [HOSTS]

    internal static IO<Seq<NamedSelection>> Found(RhinoDoc doc, string name) =>
        Named(doc, name).Bind(rows => IO.lift(rows.IsEmpty ? Table.Missing(name) : Fin.Succ(rows)));

    internal static IO<Seq<NamedSelection>> Named(RhinoDoc doc, string name) =>
        NamedSelection.Read(doc).Map(rows => rows.Filter(row => Table.Comparer.Equals(row.Name, name)).Strict());

    private static IO<Unit> Watched(uint serial, Func<Option<uint>, IO<Unit>> deliver) =>
        from doc in IO.lift(() => Conversions.Required(RhinoDoc.FromRuntimeSerialNumber(serial)))
        from model in SelectionModel(doc)
        from notifier in IO.lift(Conversions.Cast<INotifyPropertyChanged>(model))
        from observed in Subscriptions.On<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => notifier.PropertyChanged += handler, handler => notifier.PropertyChanged -= handler, static handler => handler.Invoke)
            .Choose(args => Conversions.Present(string.Equals(args.PropertyName, Collection, StringComparison.Ordinal), Some(serial)))
            .Attach(deliver)
        select unit;
}
