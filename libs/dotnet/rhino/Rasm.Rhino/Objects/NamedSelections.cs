using System.ComponentModel;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SelectionChange {
    public static readonly SelectionChange Added = new(selects: true, clears: false);
    public static readonly SelectionChange Removed = new(selects: false, clears: false);
    public static readonly SelectionChange Replaced = new(selects: true, clears: true);

    public bool Selects { get; }

    public bool Clears { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NamedSelection(Guid Id, string Name, Seq<Guid> Objects, Seq<(Guid ObjectId, ComponentIndex Component)> SubObjects) {
    public static IO<Seq<NamedSelection>> Read(RhinoDoc doc) => Internals.SelectionModel(doc).Map(Internals.Selections);
}

public sealed record NamedSelectionState(StateName Name, ObjectTarget Objects) : NamedState(NamedSelections.Table, Name) {
    internal override IO<NamedWrite> Save(RhinoDoc doc) =>
        from objects in Objects.Objects(doc)
        from prior in NamedSelections.Named(doc, Name)
        from saved in NamedSelections.Saved(doc, Name, objects.Map(static found => found.Id).Distinct().Strict(), [])
        select new NamedWrite(
            prior.Head.Match(Some: row => NamedSelections.Saved(doc, Name, row.Objects, row.SubObjects), None: () => NamedSelections.Scripted(doc, "Delete", Name)),
            IO.pure(unit));
}

public sealed record NamedSelectionRestore(StateName Name, SelectionChange Change) : NamedRestore(NamedSelections.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) =>
        from found in NamedSelections.Found(doc, Name)
        from held in ObjectTarget.Selection(doc)
        from restored in (from cleared in when(Change.Clears, IO.lift(doc.Objects.UnselectAll).Map(unit)).As()
                          from changed in Change.Selects
                              ? NamedSelections.Scripted(doc, "Select", Name)
                              : IO.lift(() => doc.Objects.Select(found.Bind(static row => row.Objects), @select: false, syncHighlight: true, persistentSelect: true, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true)).Map(unit)
                          select unit).Catch(error => NamedSelections.Reverted(doc, held) >> IO.fail<Unit>(error))
        select new NamedWrite(NamedSelections.Reverted(doc, held), IO.pure(unit));
}

// --- [TABLES] --------------------------------------------------------------------------
public static class NamedSelections {
    // --- [MARKS]
    private const char Quote = '"';

    private static HostEvent<Option<uint>> Refreshed { get; } = new(
        $"{Documents.Arrived.Label} | {Internals.Collection}",
        static deliver =>
            from scope in IO.env
            from watches in IO.lift(static () => Ref(HashMap<uint, Owned<Unit>>()))
            from arrived in Documents.Arrived.Attach(serial => Products.Replace(watches, serial, Watched(serial, deliver)).Scoped(scope))
            from closed in Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.CloseDocument += h, static h => RhinoDoc.CloseDocument -= h)
                .Attach(args => Products.Clear(watches, args.DocumentSerialNumber).Scoped(scope))
            select unit);

    // --- [TABLE]
    public static NamedTable Table { get; } = new(nameof(NamedSelections), StringComparer.OrdinalIgnoreCase) {
        Marks = [Refreshed],
        Entries = static doc => NamedSelection.Read(doc).Map(static rows => rows.Map(Entry).Strict()),
        Rename = static (doc, entry, name) =>
            from found in Found(doc, entry.Name)
            from renamed in Scripted(doc, "Rename", entry.Name, name)
            from landed in Found(doc, name)
            select new NamedWrite(Scripted(doc, "Rename", name, entry.Name), IO.pure(unit)),
        Delete = static (doc, entry) =>
            from held in Found(doc, entry.Name)
            from deleted in Scripted(doc, "Delete", entry.Name)
            select new NamedWrite(Saved(doc, entry.Name, held.Bind(static row => row.Objects).Distinct().Strict(), held.Bind(static row => row.SubObjects).Distinct().Strict()), IO.pure(unit)),
        Hold = static (doc, _) => ObjectTarget.Selection(doc).Map(held => new NamedWrite(Reverted(doc, held), IO.pure(unit))),
        Accepts = Some<Func<StateName, Fin<Unit>>>(static name =>
            RhinoFault.Unless(!((string)name).Contains("::", StringComparison.Ordinal) && !((string)name).Contains(Quote, StringComparison.Ordinal), RhinoReason.Invalid, (string)name)),
        Members = Some<Func<RhinoDoc, NamedEntry, IO<Seq<Guid>>>>(static (doc, entry) => Found(doc, entry.Name).Map(static rows => rows.Bind(static row => row.Objects).Distinct().Strict())),
    };

    private static NamedEntry Entry(NamedSelection row) => new(Table, row.Name, Some(row.Id));

    // --- [SCRIPT]
    internal static IO<Unit> Scripted(RhinoDoc doc, string verb, params Seq<string> names) =>
        Documents.RunScript(doc, $"-_NamedSelections _{verb} {string.Join(' ', names.Map(static name => $"{Quote}{name}{Quote}"))}", echo: false, display: None);

    internal static IO<Unit> Saved(RhinoDoc doc, string name, Seq<Guid> objects, Seq<(Guid ObjectId, ComponentIndex Component)> parts) =>
        from created in ObjectTarget.Selection(doc).Bind(held =>
            (from picked in IO.lift(() => doc.Objects.SetSelectedObjects(objects, syncHighlight: true, persistentSelect: true, ignoreGripsState: true, ignoreLayerLocking: true, ignoreLayerVisibility: true))
             from parted in toSeq(parts.GroupBy(static part => part.ObjectId)).TraverseM(owned =>
                 from found in IO.lift(() => Conversions.Required(doc.Objects.FindId(owned.Key)))
                 from selected in found.SelectSubObjects(toSeq(owned).Map(static part => part.Component), ObjectSelection.Persistent, syncHighlight: true)
                 select selected).As()
             from made in Scripted(doc, "Save", name)
             select unit).Finally(Reverted(doc, held)))
        from landed in Named(doc, name)
        from confirmed in IO.lift(() => RhinoFault.Unless(
            landed.Head.Exists(row => toHashSet(row.Objects) == toHashSet(objects) && toHashSet(row.SubObjects) == toHashSet(parts)), RhinoReason.Declined, name))
        select unit;

    internal static IO<Unit> Reverted(RhinoDoc doc, Seq<(RhinoObject Object, bool Persistent)> held) =>
        IO.lift(doc.Objects.UnselectAll).Bind(_ => ObjectTarget.Reselected(held));

    // --- [ROWS]
    internal static IO<Seq<NamedSelection>> Found(RhinoDoc doc, string name) =>
        Named(doc, name).Bind(rows => IO.lift(() => rows.IsEmpty ? Table.Missing(name) : Fin.Succ(rows)));

    internal static IO<Seq<NamedSelection>> Named(RhinoDoc doc, string name) =>
        NamedSelection.Read(doc).Map(rows => rows.Filter(row => Table.Comparer.Equals(row.Name, name)).Strict());

    private static IO<Unit> Watched(uint serial, Func<Option<uint>, IO<Unit>> deliver) =>
        from doc in IO.lift(() => Conversions.Required(RhinoDoc.FromRuntimeSerialNumber(serial)))
        from model in Internals.SelectionModel(doc)
        from notifier in IO.lift(() => Conversions.Cast<INotifyPropertyChanged>(model))
        from observed in Subscriptions.On<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => notifier.PropertyChanged += handler, handler => notifier.PropertyChanged -= handler, static handler => handler.Invoke)
            .Choose(args => Conversions.Present(string.Equals(args.PropertyName, Internals.Collection, StringComparison.Ordinal), Some(serial)))
            .Attach(deliver)
        select unit;
}
