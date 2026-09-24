using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.UI;
using Rhino.UI.Theme;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FormSetup {
    public sealed record LocalizeAndRestore(bool UseRhinoStyle) : FormSetup;

    public sealed record None() : FormSetup;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostWindows {
    // --- [READS]
    public static IO<Option<Window>> MainWindow(Option<RhinoDoc> doc) =>
        IO.lift(() => Optional(doc.Match(Some: RhinoEtoApp.MainWindowForDocument, None: static () => RhinoEtoApp.MainWindow)));

    public static IO<Option<RhinoDoc>> Document(Form form) =>
        IO.lift(() => Optional(form.GetRhinoDoc()));

    public static IO<Seq<TWindow>> WindowsFromDocument<TWindow>(RhinoDoc doc) where TWindow : Window =>
        IO.lift(() => toSeq(EtoExtensions.WindowsFromDocument<TWindow>(doc)).Strict());

    // --- [PRESENTATION]
    public static IO<Unit> Show(Form form, RhinoDoc doc, FormSetup setup) =>
        from prepared in IO.lift(() => setup.Switch(
            form,
            localizeAndRestore: static (target, restore) => {
                if (restore.UseRhinoStyle)
                    target.UseRhinoStyle();
                target.LocalizeAndRestore();
            },
            none: static (_, _) => { }))
        from shown in IO.lift(() => form.Show(doc))
        select shown;

    public static IO<TValue> ShowModal<TValue>(RhinoDoc doc, Option<Control> parent, Func<Control, TValue> show) =>
        from owner in Parent(doc, parent)
        from answer in IO.lift(() => show(owner))
        select answer;

    internal static IO<Control> Parent(RhinoDoc doc, Option<Control> parent) =>
        parent.Match(
            Some: static control => IO.pure(control),
            None: () => MainWindow(Some(doc)).Bind(static window => IO.lift(window.ToFin(new Missing(nameof(RhinoEtoApp.MainWindowForDocument))).Map(static found => (Control)found))));

    // --- [THEME]
    public static IO<Seq<((string Zone, string Entry) Key, Eto.Drawing.Color Value)>> Swatches(ThemeZone zone) =>
        IO.lift(() => (from entry in toSeq(zone.Enumerate())
                       from color in Optional(entry.Value as Eto.Drawing.Color?).ToSeq()
                       select (Key: (Zone: zone.Id, Entry: entry.Id), Value: color)).Strict());
}
