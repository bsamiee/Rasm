using System.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PageSelection(uint DocSerial, uint EventSerial, int ObjectCount, Seq<Guid> ObjectIds, Option<uint> ViewSerial, Option<Guid> ViewportId);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPages {
    // --- [PROPERTIES]
    public static PageSelection Read(ObjectPropertiesPageEventArgs e) =>
        new(
            e.DocRuntimeSerialNumber,
            e.EventRuntimeSerialNumber,
            e.ObjectCount,
            toSeq(e.Objects).Map(static row => row.Id).Strict(),
            Optional(e.View).Map(static view => view.RuntimeSerialNumber),
            Optional(e.Viewport).Map(static viewport => viewport.Id));

    public static IO<Seq<Guid>> Selection(ObjectPropertiesPage page, ObjectType filter) =>
        IO.lift(() => toSeq(page.GetSelectedObjects(filter)).Map(static row => row.Id).Strict());

    public static IO<Unit> Modify(ObjectPropertiesPage page, Func<PageSelection, Fin<Unit>> change) =>
        IO.lift(() => Answers.Captured<Unit>(capture => page.ModifyPage(e => capture(change(Read(e)))), nameof(ObjectPropertiesPage.ModifyPage)));

    // --- [NAVIGATION]
    public static IO<Unit> SetActivePageTo(StackedDialogPage page, string name, bool documentProperties) =>
        IO.lift(() => Refused.Unless(page.SetActivePageTo(name, documentProperties), nameof(StackedDialogPage.SetActivePageTo)));

    public static IO<Unit> NavigationText(StackedDialogPage page, bool bold, Option<Color> color) =>
        from windows in IO.lift(static () => WindowsOnly.Unless(HostUtils.RunningOnWindows, nameof(StackedDialogPage.NavigationTextIsBold)))
        from styled in IO.lift(() => {
            page.NavigationTextIsBold = bold;
            page.NavigationTextColor = color.IfNone(Color.Empty);
        })
        select styled;

    public static IO<Option<Window>> WindowFor(OptionsDialogPage page, bool preferences) =>
        IO.lift(() => Optional(preferences ? RhinoEtoApp.ApplicationPreferencesWindowForPage(page) : RhinoEtoApp.DocumentPropertiesWindowForPage(page)));
}
