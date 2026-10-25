using LanguageExt.ClassInstances;
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
                ? DisposalOps.Within<IDisposable, Option<string>>(retain =>
                    from row in Viewports.ResolveViewport(doc, readTarget, retain)
                    from found in IO.lift(() => Conversions.Present(row.Viewport.GetUserString(address.Text)))
                    select found, DisposalOps.Release)
                : UserStrings.Value(doc, address)
            from value in IO.lift(text.Traverse(key.Read).As())
            select value,
            value => value.Map(key.Text) switch {
                var text => target.Case is ViewportTarget writeTarget
                    ? DisposalOps.Within<IDisposable, Unit>(retain =>
                        from row in Viewports.ResolveViewport(doc, writeTarget, retain)
                        from written in UserStrings.Write(row.Viewport, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((address.Text, text))))
                        from committed in IO.lift(row.CommitViewportChanges)
                        from marked in IO.lift(() => { doc.Modified = true; })
                        select marked, DisposalOps.Release)
                    : UserStrings.Write(doc, address, text).Map(static _ => unit),
            },
            Applied.Live,
            target.IsNone ? Some(EventKind.UserStringChanged.In(doc.RuntimeSerialNumber).Choose(static _ => Some(unit))) : None);
}
