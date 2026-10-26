using LanguageExt.ClassInstances;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rhino;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserTexts {
    public static ValueStore<TValue> Store<TValue, TRaw>(
        RhinoDoc doc, DocumentKey address, ValueKey<TValue, TRaw> key, Option<ViewportTarget> target = default)
        where TValue : notnull
        where TRaw : notnull =>
        ValueStore.Of(
            from text in target.Case is ViewportTarget readTarget
                ? Viewports.With(doc, readTarget, row => IO.lift(() => Conversions.Present(row.Viewport.GetUserString(address.Text))))
                : UserStrings.Value(doc, address)
            from value in IO.lift(text.Traverse(key.Read).As())
            select value,
            value => value.Map(key.Text) switch {
                var text => target.Case is ViewportTarget writeTarget
                    ? Viewports.Edit(doc, new ViewportSet.One(writeTarget),
                            port => UserStrings.Write(port, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((address.Text, text)))), new RedrawPolicy.Silent())
                        .Bind(_ => IO.lift(() => { doc.Modified = true; }))
                    : UserStrings.Write(doc, address, text).Map(static _ => unit),
            },
            Applied.Live,
            Some(target.Match(Some: held => Viewports.Changed(doc, held), None: () => EventKind.UserStringChanged.In(doc.RuntimeSerialNumber).Choose(static _ => Some(unit)))));
}
