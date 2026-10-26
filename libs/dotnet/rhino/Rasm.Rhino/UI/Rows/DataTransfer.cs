using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Rhino.UI.Rows;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DataTransfer {
    // --- [CLIPBOARD]
    public static IO<Unit> Copy(ValueSet values) =>
        IO.lift(() => Clipboard.Instance.Text = values.Text()).Map(unit);

    public static IO<Unit> Copy(BindingGroup bound, Option<IterableNE<EntryKey>> keys, RowScope scope) =>
        scope.Shown(bound).Bind(values => Copy(values.Scoped(keys)));

    public static IO<ValueDiff> Paste(BindingGroup bound, Option<IterableNE<EntryKey>> keys, RowScope scope) =>
        from text in IO.lift(static () => Refused.UnlessFound(Clipboard.Instance.Text, nameof(Clipboard.Text)))
        from read in IO.lift(ValueSet.Read(text))
        let scoped = read.Scoped(keys)
        from matched in IO.lift(guard(bound.Keys().Exists(scoped.Entries.ContainsKey), Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{read.Entries.Count} pasted values match no row"))).ToFin())
        from diff in scope.Commit(bound, scoped)
        select diff;

    // --- [DRAGS]
    public static IO<Unit> Drag(Control source, Seq<string> files, Option<Image> image) =>
        from paths in Callbacks.Each(files.Map(static file => IO.lift(() => InputPath.Of(file))))
        from dragged in DisposalOps.Within(static () => new DataObject(), data => Dragged(source, paths, image, data))
        select dragged;

    private static IO<Unit> Dragged(Control source, Seq<InputPath> paths, Option<Image> image, DataObject data) =>
        IO.lift(() => { data.Uris = [.. paths.Map(static path => new Uri(path))]; }).Bind(_ => IO.lift(() => image.Match(
            Some: held => source.DoDragDrop(data, DragEffects.Copy, held, new PointF(held.Width / 2f, held.Height / 2f)),
            None: () => source.DoDragDrop(data, DragEffects.Copy))));

    // --- [DROPS]
    public static IO<Unit> Drops(Control target, Seq<FileTypeRow> accepted, Func<string, IO<Unit>> commit, IPlugInSink sink, Action<IDisposable> retain) =>
        fun((DragEventArgs args) => IO.lift(() => { args.Effects = Accepted(args.Data, accepted).Match(Some: static _ => DragEffects.Copy, None: static () => DragEffects.None); })) switch {
            var gate =>
                from changed in DisposalOps.Change(() => target.AllowDrop, held => target.AllowDrop = held, value: true, retain)
                from attached in Seq(
                        Subscriptions.During<DragEventArgs>(target.GetType(), h => target.DragEnter += h, h => target.DragEnter -= h, nameof(Control.DragEnter)).Inline(gate, sink, retain),
                        Subscriptions.During<DragEventArgs>(target.GetType(), h => target.DragOver += h, h => target.DragOver -= h, nameof(Control.DragOver)).Inline(gate, sink, retain),
                        Subscriptions.Host<DragEventArgs>(target.GetType(), h => target.DragDrop += h, h => target.DragDrop -= h, nameof(Control.DragDrop))
                            .Inline(args => IO.lift(() => Accepted(args.Data, accepted)).Bind(found => found.Match(Some: commit, None: static () => IO.pure(unit))), sink, retain))
                    .TraverseM(static attached => attached).As()
                select unit,
        };

    private static Option<string> Accepted(DataObject data, Seq<FileTypeRow> accepted) =>
        Optional(data.Uris)
            .Bind(static uris => uris is [{ IsFile: true } only] ? Some(only.LocalPath) : Option<string>.None)
            .Filter(path => accepted.Exists(row => row.Accepts(path)));
}
