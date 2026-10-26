using LanguageExt.ClassInstances;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rhino;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserTexts {
    public static ValueStore<TValue> Store<TValue>(RhinoDoc doc, DocumentKey address, ValueKey<TValue> key, Option<ViewportTarget> target = default) where TValue : notnull =>
        ValueStore.Of(
            key.Codec.Stored(address.Text, path =>
                from slot in IO.lift(Addressed(address, path))
                from text in target.Case is ViewportTarget readTarget
                    ? Viewports.With(doc, readTarget, row => IO.lift(() => Conversions.Present(row.Viewport.GetUserString(slot.Text))))
                    : UserStrings.Value(doc, slot)
                select text),
            value =>
                from slots in IO.lift(key.Codec.Slots(value).Traverse(slot => Addressed(address, slot.Path).Map(held => (Key: held, slot.Text))).As())
                from written in target.Case is ViewportTarget writeTarget
                    ? Viewports.Edit(doc, new ViewportSet.One(writeTarget),
                            port => UserStrings.Write(port, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(slots.Map(static slot => (slot.Key.Text, slot.Text)))), new RedrawPolicy.Silent())
                        .Bind(_ => IO.lift(() => { doc.Modified = true; }))
                    : Callbacks.Each(slots.Map(slot => UserStrings.Write(doc, slot.Key, slot.Text))).Map(static _ => unit)
                select written,
            Applied.Live,
            Some(target.Match(Some: held => Viewports.Changed(doc, held), None: () => EventKind.UserStringChanged.In(doc.RuntimeSerialNumber).Choose(static _ => Some(unit)))));

    private static Fin<DocumentKey> Addressed(DocumentKey address, Seq<string> path) =>
        path.Tail.IsEmpty
            ? address
            : address.Entry.Match(
                Some: entry => KeyName.Validated(EntryKey.Name(((string)entry).Cons(path.Tail))).Map(named => address with { Entry = Some(named) }),
                None: () => KeyName.Validated(EntryKey.Name(((string)address.Name).Cons(path.Tail))).Map(named => address with { Name = named }));
}
