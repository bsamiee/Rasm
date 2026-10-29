using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record EntryForm {
    public sealed record SingleLine : EntryForm;

    public sealed record MultiLine(int Visible, bool Rich) : EntryForm;

    public sealed record Secret : EntryForm;
}

public sealed record EntryRow(Option<string> Value, EntryForm Form, Func<Edit<string>, IO<Unit>> Edited) : IRow<EntryRow> {
    public Option<FileBrowse> Browse { get; init; }
    public Seq<FileTypeRow> Types { get; init; }

    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<EntryRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from entry in first.Form.Switch(
            singleLine: static _ => Entries.Line(),
            multiLine: static multi => multi.Rich ? Entries.Rich(multi.Visible) : Entries.Area(multi.Visible),
            secret: static _ => Entries.Secret())
        from show in Values.Bound(entry.Field,
            new BindableBinding<TextControl, Option<string>>(entry.Field,
                _ => Some(entry.Read()),
                (_, value) => entry.Write(value.IfNone("")),
                static (held, handler) => held.TextChanged += handler,
                static (held, handler) => held.TextChanged -= handler),
            static _ => true,
            Some(Subscriptions.Merged(
                Subscriptions.Inline<EventArgs>(h => entry.Field.LostFocus += h, h => entry.Field.LostFocus -= h).Choose(static _ => Some(unit)),
                Subscriptions.Inline<KeyEventArgs>(h => entry.Field.KeyDown += h, h => entry.Field.KeyDown -= h).Choose(args => Conversions.Present(args.KeyData == entry.Commit, args).Map(static held => {
                    held.Handled = true;
                    return unit;
                })))),
            first.Value, edit => latest.Bind(row => row.Edited(edit)))
        let shown = latest.Bind(row => show(row.Value))
        from escaped in Subscriptions.Inline<KeyEventArgs>(h => entry.Field.KeyDown += h, h => entry.Field.KeyDown -= h).Attach(args =>
            when(args.KeyData is Keys.Escape, IO.lift(() => { args.Handled = true; }) >> latest.Bind(static row => row.Edited(new Edit<string>.Cancel())) >> shown).As())
        from picked in first.Browse.Traverse(browse => Entries.Browsed(label, browse, first.Types, entry.Field, typed => latest.Bind(row => row.Edited(EditPhase.Commit.Of(typed))), scope)).As()
        select new RowCells(entry.Shown, shown) { Pick = picked };
}

internal sealed record Entry(Control Shown, TextControl Field, Keys Commit, Func<string> Read, Action<string> Write);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Entries {
    // --- [FORMS]
    internal static IO<Entry> Line() =>
        from box in use(static () => new TextBox())
        select new Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

    internal static IO<Entry> Area(int visible) =>
        from area in use(static () => new TextArea { Wrap = true, AcceptsReturn = true, AcceptsTab = false, SpellCheck = false })
        from fitted in IO.lift(() => { area.Height = (int)MathF.Ceiling(area.Font.LineHeight * visible); })
        select new Entry(area, area, Application.Instance.CommonModifier | Keys.Enter, () => area.Text, text => area.Text = text);

    internal static IO<Entry> Rich(int visible) =>
        from rich in use(static () => new RichTextAreaWithAlternateText { HideAlternateTextWhenValueChanges = true, SelectAllOnGotFocus = false })
        from fitted in IO.lift(() => { rich.Height = (int)MathF.Ceiling(rich.RichTextArea.Font.LineHeight * visible); })
        select new Entry(rich, rich.RichTextArea, Application.Instance.CommonModifier | Keys.Enter, () => rich.RichTextArea.Rtf, rtf => rich.RichTextArea.Rtf = rtf);

    internal static IO<Entry> Secret() =>
        from box in use(static () => new PasswordBox())
        select new Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

    // --- [BROWSING]
    internal static IO<Control> Browsed(LocalizeStringPair title, FileBrowse browse, Seq<FileTypeRow> types, TextControl box, Func<string, IO<Unit>> commit, RowScope scope) =>
        from picked in CommandRows.Button(
            new Command<Invoke>.Run(new CommandFace("text-browse", "Browse") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.OpenSvg)) }, None,
                new Invoke.Effect(held =>
                    from current in IO.lift(() => box.Text)
                    from path in browse.Asked(title, current, types, held).Catch(Session.Cancellation, static _ => IO.pure(Option<string>.None))
                    from committed in path.Match(Some: commit, None: static () => IO.pure(unit))
                    select committed)),
            IconSlot.Small, scope)
        from dropped in DataTransfer.Drops(box, types, commit)
        select picked.Control;
}
