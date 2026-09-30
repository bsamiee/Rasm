using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.UI;

namespace Rasm.Rhino.Ui;

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
    public static IO<Unit> Show(Form form, RhinoDoc doc, Option<bool> restore) =>
        from prepared in IO.lift(() => restore.Iter(useRhinoStyle => {
            if (useRhinoStyle)
                form.UseRhinoStyle();
            form.LocalizeAndRestore();
        }))
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
}
