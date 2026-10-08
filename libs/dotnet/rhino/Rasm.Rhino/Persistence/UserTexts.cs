using Rhino;
using Rhino.Display;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserTexts {
    public static ValueStore<TValue> Store<TValue, TRaw, TError>(
        RhinoDoc doc, DocumentKey address, ValueKey<TValue, TRaw, TError> key, Option<ViewportTarget> target = default)
        where TValue : notnull
        where TRaw : notnull =>
        ValueStore.Of(
            from text in target.Case is ViewportTarget readTarget
                ? (from row in use(Viewports.ResolveViewport(doc, readTarget))
                   select Conversions.Present(row.Viewport.GetUserString(address.Text))).Bracket()
                : UserStrings.Value(doc, address)
            from value in IO.lift(text.Traverse(key.Read).As())
            select value,
            value => value.Map(key.Text) switch {
                var text => target.Case is ViewportTarget writeTarget
                    ? (from row in use(Viewports.ResolveViewport(doc, writeTarget))
                       from written in IO.lift(() => Refused.Unless(
                           row.Viewport.SetUserStrings([KeyValuePair.Create(address.Text, Conversions.Unset(text))], replace: false),
                           nameof(RhinoViewport.SetUserStrings)))
                       from committed in IO.lift(row.CommitViewportChanges)
                       from marked in IO.lift(() => { doc.Modified = true; })
                       select marked).Bracket()
                    : IO.lift(() => { _ = doc.Strings.SetString(address.Text, Conversions.Unset(text)); }),
            },
            Applied.Live,
            target.IsNone ? Some(EventKind.UserStringChanged.In(doc.RuntimeSerialNumber).Choose(static _ => Some(unit))) : None);
}
