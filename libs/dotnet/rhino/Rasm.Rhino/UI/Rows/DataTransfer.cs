using Eto.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.PlugIns;

namespace Rasm.RhinoCommon.UI.Rows;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DataTransfer {
    // --- [CLIPBOARD]
    public static IO<Unit> Copy(ValueSet values) =>
        IO.lift(() => Clipboard.Instance.Text = values.Text()).Map(unit);

    public static IO<Unit> Copy(BindingGroup bound, Option<IterableNE<EntryKey>> keys, RowScope scope) =>
        scope.Shown(bound).Bind(values => Copy(values.Scoped(keys)));

    public static IO<ValueDiff> Paste(BindingGroup bound, Option<IterableNE<EntryKey>> keys, RowScope scope) =>
        from text in IO.lift(static () => Conversions.Found(Clipboard.Instance.Text))
        from read in IO.lift(ValueSet.Read(text))
        let scoped = read.Scoped(keys)
        from matched in IO.lift(() => RhinoFault.Unless(bound.Keys().Exists(scoped.Entries.ContainsKey)))
        from diff in scope.Commit(bound, scoped)
        select diff;

    // --- [DRAGS]
    public static IO<Unit> Drag(Control source, Seq<string> files, Option<Image> image) =>
        from paths in Callbacks.Each(files.Map(static file => IO.lift(() => InputPath.Of(file))))
        from dragged in (from data in use(static () => new DataObject()) from dropped in Dragged(source, paths, image, data) select dropped).Bracket()
        select dragged;

    private static IO<Unit> Dragged(Control source, Seq<InputPath> paths, Option<Image> image, DataObject data) =>
        IO.lift(() => { data.Uris = [.. paths.Map(static path => new Uri(path))]; }).Bind(_ => IO.lift(() => image.Match(
            Some: held => source.DoDragDrop(data, DragEffects.Copy, held, new PointF(held.Width / 2f, held.Height / 2f)),
            None: () => source.DoDragDrop(data, DragEffects.Copy))));

    // --- [DROPS]
    public static IO<Unit> Drops(Control target, Seq<FileTypeRow> accepted, Func<string, IO<Unit>> commit, IPlugInLifetime owner) =>
        fun((DragEventArgs args) => IO.lift(() => { args.Effects = Accepted(args.Data, accepted).Match(Some: static _ => DragEffects.Copy, None: static () => DragEffects.None); })) switch {
            var gate =>
                from changed in Products.Changed(() => target.AllowDrop, held => target.AllowDrop = held, value: true)
                from entered in Subscriptions.During<DragEventArgs>(h => target.DragEnter += h, h => target.DragEnter -= h).Inline(gate, owner)
                from over in Subscriptions.During<DragEventArgs>(h => target.DragOver += h, h => target.DragOver -= h).Inline(gate, owner)
                from dropped in Subscriptions.Host<DragEventArgs>(h => target.DragDrop += h, h => target.DragDrop -= h)
                    .Inline(args => IO.lift(() => Accepted(args.Data, accepted)).Bind(found => found.Match(Some: commit, None: static () => IO.pure(unit))), owner)
                select unit,
        };

    private static Option<string> Accepted(DataObject data, Seq<FileTypeRow> accepted) =>
        Optional(data.Uris)
            .Bind(static uris => uris is [{ IsFile: true } only] ? Some(only.LocalPath) : Option<string>.None)
            .Filter(path => accepted.Exists(row => row.Accepts(path)));
}
