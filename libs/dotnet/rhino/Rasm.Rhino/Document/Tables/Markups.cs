using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MarkupState(StateName Name, RhinoView Source, Seq<GeometryPair> Geometry) : NamedState(Markups.Table, Name) {
    internal override IO<NamedWrite> Save(RhinoDoc doc) =>
        from ended in Markups.Ended(doc)
        from prior in IO.lift(() => Conversions.Present(doc.Markups.Find(Name)))
        from cleared in prior.Traverse(index => Products.Accepted(() => doc.Markups.Delete(index, quiet: true))).As()
        from index in IO.lift(() => Conversions.Required(Conversions.Present(
            doc.Markups.Create(Source, Geometry.Map(static pair => pair.Geometry), Geometry.Map(static pair => pair.Attributes.ValueUnsafe())))))
        from named in Markups.Named(doc, index, Name)
        select NamedWrite.Undoable;
}

public sealed record MarkupRestore(StateName Name) : NamedRestore(Markups.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) =>
        from index in Markups.Index(doc, Name)
        from held in Markups.Hold(doc)
        from viewed in Products.Accepted(() => doc.Markups.View(index)).Catch(error => NamedWrite.Undone([held]) >> IO.fail<Unit>(error))
        select held;
}

// --- [TABLES] --------------------------------------------------------------------------
public static class Markups {
    public static NamedTable Table { get; } = new(nameof(Markups), Rows.Names<Markup>()) {
        Marks = [ReadModels.Marks(Subscriptions.On<MarkupTableEventArgs>(static h => RhinoDoc.MarkupTableEvent += h, static h => RhinoDoc.MarkupTableEvent -= h).Documents(static args => args.Document))],
        Entries = static doc => Rows.Listed(doc.Markups).Map(static rows => rows.Map(Entry)),
        Rename = static (doc, entry, name) => Index(doc, entry.Name).Bind(index => Named(doc, index, name)).Map(static _ => NamedWrite.Undoable),
        Delete = static (doc, entry) =>
            Index(doc, entry.Name).Bind(index => Products.Accepted(() => doc.Markups.Delete(index, quiet: true))).Map(static _ => NamedWrite.Undoable),
        Hold = static (doc, _) => Hold(doc),
        Members = Some<Func<RhinoDoc, NamedEntry, IO<Seq<Guid>>>>(static (doc, entry) => Index(doc, entry.Name).Map(index => Conversions.Rows(doc.Markups.GetObjectIds(index)))),
        Current = Some<Func<RhinoDoc, Guid, IO<Option<NamedEntry>>>>(static (doc, viewport) => IO.lift(() =>
            Optional(doc.Views.ActiveView).Filter(view => view.ActiveViewportID == viewport)
                .Bind(_ => Conversions.Present(doc.Markups.ActiveIndex))
                .Bind(index => Optional(doc.Markups.FindIndex(index)))
                .Map(Entry))),
    };

    // --- [HOLD]
    internal static IO<NamedWrite> Hold(RhinoDoc doc) =>
        from view in Products.Of(() => doc.Views.ActiveView)
        from held in IO.lift(() => new ViewInfo(view.ActiveViewport))
        select new NamedWrite(
            Commits.WithinRedraw(doc, new RedrawPolicy.View(view), Ended(doc) >> Products.Accepted(() => view.ActiveViewport.PushViewInfo(held, includeTraceImage: true))),
            IO.lift(held.Dispose));

    // --- [HOST]
    internal static IO<int> Index(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.Markups.Find(name)).ToFin(Table.Missing(name)));

    internal static IO<Markup> Named(RhinoDoc doc, int index, StateName name) =>
        Rows.Edit(doc, TableKinds.Markups, new ComponentRef<Markup>.ByIndex(index), [Rows.Set<Markup, string>(static row => row.Name, static (row, held) => row.Name = held, name)]);

    internal static IO<Unit> Ended(RhinoDoc doc) =>
        IO.lift(() => doc.MarkupMode is not MarkupMode.None).Bind(active => when(active, Products.Accepted(() => doc.Markups.Cancel())).As());

    private static NamedEntry Entry(Markup row) =>
        new(Table, row.Name, Conversions.Present(row.Id));
}
