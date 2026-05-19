using Eto.Forms;

namespace Rasm.Rhino.UI;

public sealed record UiIntent<T> {
    private readonly Func<RhinoDoc, Fin<T>> show;

    internal UiIntent(Func<RhinoDoc, Fin<T>> show) => this.show = show;

    internal Fin<T> Show(RhinoDoc document) =>
        Optional(show)
            .ToFin(Fail: Op.Of(name: nameof(UiIntent<T>)).InvalidInput())
            .Bind(run => run(arg: document));
}

public static class UiIntent {
    public static UiIntent<T> Of<T>(Func<RhinoDoc, Fin<T>> show) =>
        new(show: show);

    public static UiIntent<T> Eto<T>(Dialog<T> dialog, bool semiModal = false) =>
        Of(document => Optional(dialog)
            .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
            .Map(valid => semiModal switch {
                true => global::Rhino.UI.EtoExtensions.ShowSemiModal(valid, document, parent: RhinoUi.Parent(document: document)),
                false => valid.ShowModal(owner: RhinoUi.Parent(document: document)),
            }));

    public static UiIntent<Unit> Modeless(Form form) =>
        Of(document => Optional(form)
            .ToFin(Fail: Op.Of(name: nameof(Modeless)).InvalidInput())
            .Map(valid => {
                global::Rhino.UI.EtoExtensions.UseRhinoStyle(valid);
                global::Rhino.UI.EtoExtensions.Show(valid, document);
                return unit;
            }));
}
