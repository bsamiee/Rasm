using Eto.Forms;

namespace Rasm.Rhino.UI;

public sealed record UiDialogIntent<T> {
    private readonly Func<RhinoDoc, Fin<T>> show;

    internal UiDialogIntent(Func<RhinoDoc, Fin<T>> show) => this.show = show;

    internal Fin<T> Show(RhinoDoc document) =>
        Optional(show)
            .ToFin(Fail: Op.Of(name: nameof(UiDialogIntent<T>)).InvalidInput())
            .Bind(run => run(arg: document));
}

public static class UiDialogIntent {
    public static UiDialogIntent<T> Of<T>(Func<RhinoDoc, Fin<T>> show) =>
        new(show: show);

    public static UiDialogIntent<T> Eto<T>(Dialog<T> dialog, bool semiModal = false) =>
        Of(document => Optional(dialog)
            .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
            .Map(valid => semiModal switch {
                true => global::Rhino.UI.EtoExtensions.ShowSemiModal(valid, document, parent: RhinoUi.Parent(document: document)),
                false => valid.ShowModal(owner: RhinoUi.Parent(document: document)),
            }));

    public static UiDialogIntent<Unit> Modeless(Form form) =>
        Of(document => Optional(form)
            .ToFin(Fail: Op.Of(name: nameof(Modeless)).InvalidInput())
            .Map(valid => {
                global::Rhino.UI.EtoExtensions.UseRhinoStyle(valid);
                global::Rhino.UI.EtoExtensions.Show(valid, document);
                return unit;
            }));
}
