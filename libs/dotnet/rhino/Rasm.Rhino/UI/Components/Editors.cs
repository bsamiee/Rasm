using System.Collections.Generic;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.UI;

namespace Rasm.Rhino.UI.Components;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Editors {
    // --- [LISTS]
    public static ControlRows List<TItem, TKey>(string label, Preference<Seq<TItem>> items, Func<TItem, TKey> key, Func<TItem, string> caption, Func<TItem, string, Fin<TItem>> rename, Func<RowScope, IO<TItem>> add)
        where TItem : notnull where TKey : notnull =>
        new(_ => items.Read.Map(current => Seq(Row.Of(label, new ListRow<TItem, TKey>(IterableNE.create<ListColumn<TItem>>(new ListColumn<TItem>.Label(Some(label), caption, None, Some(rename))), key, current, HashSet<TKey>()) {
            Edited = edit => edit.Applied.Traverse(items.Write).As().Map(unit),
            Reorderable = true,
            Commands = chosen => Seq<Command<Invoke>>(
                new Command<Invoke>.Run(new CommandFace("list-add", "Add"), None, new Invoke.Effect(scope =>
                    from item in add(scope)
                    from held in items.Read
                    from written in items.Write(held.Add(item))
                    select unit)),
                new Command<Invoke>.Run(new CommandFace("list-remove", "Remove"), None, new Invoke.Effect(_ =>
                    from picked in chosen
                    from held in items.Read
                    from written in items.Write(held.Filter(item => !picked.Exists(gone => EqualityComparer<TKey>.Default.Equals(key(gone), key(item)))))
                    select unit))),
        }))),
            _ => IO.pure(Seq(PreferenceRows.Signal(items))));

    // --- [PRESETS]
    public static Command<Invoke>.Run Presets(PresetBinding binding) =>
        new(new CommandFace("section-presets", "Presets") { Icon = Some(IconResource.Presets) }, None, new Invoke.Popup(_ =>
            from shelf in binding.Shelf
            from current in binding.Current
            select Seq(
                shelf.Listed.Map(preset => (Command<Invoke>)CommandRows.Picked(new CommandFace($"preset-{preset.Name}", (string)preset.Name), current.Exists(held => held.Name == preset.Name), _ => binding.Apply(preset).Map(unit))),
                Seq<Command<Invoke>>(new Command<Invoke>.Run(new CommandFace("preset-save", "Save preset"), None, new Invoke.Effect(_ =>
                    from text in HostDialogs.Ask(() => (Dialogs.ShowEditBox(LocalText.Of("Save preset").Local, LocalText.Of("Name").Local, Conversions.Unset(current.Map(static held => (string)held.Name)), multiline: false, out string entered), entered))
                    from name in IO.lift(() => PresetName.Validated(text))
                    from saved in binding.Save(name)
                    select unit))))));
}
