using System.Globalization;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Types;
using Grasshopper2.UI;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.Doc;
using Rasm.Rhino;
using Rasm.Rhino.Persistence;

namespace Rasm.Grasshopper;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class GrasshopperPreferences {

    // --- [HOSTS]

    public static Preference<T> Setting<T>(Settings.Setting<T> setting) =>
        Preferences.Of(
            IO.lift(() => setting.Value),
            value => IO.lift(() => { setting.Value = value; }),
            Some(Subscriptions.Inline<EventArgs>(h => setting.Changed += h, h => setting.Changed -= h).Choose(static _ => Some(unit))));

    // --- [OBJECTS]

    private const string Element = "Value";

    public static Preference<T> Object<T>(IDocumentObject owner, GrasshopperIO.Name key, T fallback, Nomen face) where T : notnull =>
        key.ToString() switch {
            var name => Preferences.Of(
                IO.lift(() => owner.CustomValues.Get(name, NTuple.Empty) switch {
                    var tuple when typeof(T).IsEnum => tuple.Get(Element, StringComparison.Ordinal, out int number) ? (T)Enum.ToObject(typeof(T), number) : fallback,
                    var tuple => tuple.Get(Element, StringComparison.Ordinal, out T stored) ? stored : fallback,
                }),
                value => IO.lift(() => owner.CustomValues.Set(name, new NTuple((Element, typeof(T).IsEnum ? Garden.IPear(Convert.ToInt32(value, CultureInfo.InvariantCulture)) : Garden.Pear(value))))) switch {
                    var set => IO.lift(() => Optional(owner.Document)).Bind(document => document.Match(
                        Some: placed => DocumentEdits.Commit(placed, ("Set", face.Name), actions => IO.lift(() => actions.Add(new CustomValueAction(owner, name))) >> set),
                        None: () => set)),
                },
                Some(Subscriptions.Inline<KeyValueEventArgs>(h => owner.CustomValues.ValueChanged += h, h => owner.CustomValues.ValueChanged -= h)
                    .Choose(args => string.Equals(args.Key, name, StringComparison.OrdinalIgnoreCase) ? Some(unit) : None))),
        };
}
