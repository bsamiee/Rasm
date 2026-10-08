using Rhino;
using Rhino.Display;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserTexts {
    // --- [CROSSING]
    private static ValueStore<TValue> TextStore<TValue, TRaw, TError>(
        ValueKey<TValue, TRaw, TError> key, IO<Option<string>> read, Func<Option<string>, IO<Unit>> write, Option<HostEvent<Unit>> changed)
        where TValue : notnull
        where TRaw : notnull =>
        ValueStore.Of(read.Bind(text => IO.lift(text.Traverse(key.Read).As())), value => write(value.Map(key.Text)), Applied.Live, changed);

    // --- [STORES]
    public static ValueStore<TValue> Document<TValue, TRaw, TError>(RhinoDoc doc, DocumentKey address, ValueKey<TValue, TRaw, TError> key)
        where TValue : notnull
        where TRaw : notnull =>
        TextStore(
            key,
            UserStrings.Value(doc, address),
            text => UserStrings.Write(doc, address, text).Map(static _ => unit),
            Some(ValueStore.Signal(EventKind.UserStringChanged.In(doc.RuntimeSerialNumber), static _ => true)));

    public static ValueStore<TValue> View<TValue, TRaw, TError>(RhinoDoc doc, ViewportTarget target, DocumentKey address, ValueKey<TValue, TRaw, TError> key)
        where TValue : notnull
        where TRaw : notnull =>
        TextStore(
            key,
            use(Viewports.ResolveViewport(doc, target)).Bind(row => IO.lift(() => Conversions.Present(row.Viewport.GetUserString(address.Text)))).Bracket(),
            text => (
                from row in use(Viewports.ResolveViewport(doc, target))
                from written in IO.lift(() => Refused.Unless(
                    (text.IsSome || row.Viewport.UserStringCount > 1) && row.Viewport.SetUserString(address.Text, Conversions.Unset(text)),
                    nameof(RhinoViewport.SetUserString)))
                from committed in IO.lift(row.CommitViewportChanges)
                select committed).Bracket(),
            None);
}
