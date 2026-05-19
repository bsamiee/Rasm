using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;
using Rhino;
using InputPanelControl = Grasshopper2.UI.InputPanel.InputPanel;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record UiIntent<T> {
    private readonly Func<GrasshopperUi.Scope, Fin<T>> run;

    internal UiIntent(Func<GrasshopperUi.Scope, Fin<T>> run) => this.run = run;

    internal Fin<T> Run(GrasshopperUi.Scope scope) => run(arg: scope);
}

// --- [SERVICES] -------------------------------------------------------------------------
[BoundaryAdapter]
public static class UiIntent {
    public static UiIntent<T> Of<T>(Func<Option<Window>, Fin<T>> run) =>
        new(run: scope => Optional(run)
            .ToFin(Fail: Op.Of(name: nameof(Of)).InvalidInput())
            .Bind(valid => valid(arg: scope.Parent)));

    public static UiIntent<Control> Eto(Control control) =>
        Of(run: _ => GrasshopperUi.Protect(
            valid: () => Optional(control)
                .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
                .Map(GrasshopperUi.Style),
            name: nameof(Eto)));

    public static UiIntent<T> Eto<T>(Dialog<T> dialog) =>
        Of(run: parent => GrasshopperUi.Protect(
            valid: () => Optional(dialog)
                .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
                .Map(dialogValue => {
                    Dialog<T> styled = GrasshopperUi.Style(control: dialogValue);
                    return parent
                        .Map(owner => styled.ShowModal(owner: owner))
                        .IfNone(() => styled.ShowModal());
                }),
            name: nameof(Eto)));

    public static UiIntent<Unit> Eto(Form form) =>
        Of(run: _ => GrasshopperUi.Protect(
            valid: () => Optional(form)
                .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
                .Map(valid => {
                    Form styled = GrasshopperUi.Style(control: valid);
                    styled.Show();
                    return unit;
                }),
            name: nameof(Eto)));

    public static UiIntent<Seq<string>> Open(OpenFileDialog dialog) =>
        Of(run: parent => Optional(dialog)
            .ToFin(Fail: Op.Of(name: nameof(Open)).InvalidInput())
            .Bind(dialogValue => GrasshopperUi.Protect(valid: () => parent
                .Map(owner => dialogValue.Show(parent: owner))
                .IfNone(() => dialogValue.Show(parent: null!)) switch {
                    DialogResult.Ok => Optional(dialogValue.Filenames)
                        .Map(static paths => toSeq(paths).Filter(static path => !string.IsNullOrWhiteSpace(value: path)))
                        .Filter(static paths => !paths.IsEmpty)
                        .IfNone(() => Optional(dialogValue.FileName)
                            .Filter(static path => !string.IsNullOrWhiteSpace(value: path))
                            .Map(static path => Seq(path))
                            .IfNone(Seq<string>())) switch {
                                Seq<string> paths when !paths.IsEmpty => Fin.Succ(value: paths),
                                _ => Fin.Fail<Seq<string>>(error: Op.Of(name: nameof(Open)).InvalidResult()),
                            },
                    _ => Fin.Fail<Seq<string>>(error: new Fault.Cancelled()),
                }, name: nameof(Open))));

    public static UiIntent<string> Save(SaveFileDialog dialog) =>
        Of(run: parent => Optional(dialog)
            .ToFin(Fail: Op.Of(name: nameof(Save)).InvalidInput())
            .Bind(dialogValue => GrasshopperUi.Protect(valid: () => parent
                .Map(owner => dialogValue.Show(parent: owner))
                .IfNone(() => dialogValue.Show(parent: null!)) switch {
                    DialogResult.Ok => Optional(dialogValue.FileName)
                        .Filter(static path => !string.IsNullOrWhiteSpace(value: path))
                        .ToFin(Fail: Op.Of(name: nameof(Save)).InvalidResult()),
                    _ => Fin.Fail<string>(error: new Fault.Cancelled()),
                }, name: nameof(Save))));

    public static UiIntent<Control> InputPanel(Func<InputPanelControl, Fin<Unit>> build) =>
        Of(run: _ => RenderInputPanel(
            build: build,
            render: static panel => {
                // BOUNDARY ADAPTER - GH2 emits an Eto Control; Rhino style starts at that Control boundary.
                Control control = panel.ToEtoControl();
                return Fin.Succ(value: GrasshopperUi.Style(control: control));
            },
            name: nameof(InputPanel)));

    public static UiIntent<Form> InputPanelPopup(Control owner, PointF point, RectangleF bounds, Func<InputPanelControl, Fin<Unit>> build) =>
        Of(run: _ =>
            from validOwner in Optional(owner).ToFin(Fail: Op.Of(name: nameof(InputPanelPopup)).InvalidInput())
            from form in RenderInputPanel(
                build: build,
                render: panel => {
                    // BOUNDARY ADAPTER - GH2 owns popup placement; returned Eto Form receives Rhino styling.
                    Form form = panel.ShowAsForm(owner: validOwner, point: point, bounds: bounds);
                    return Fin.Succ(value: GrasshopperUi.Style(control: form));
                },
                name: nameof(InputPanelPopup))
            select form);

    private static Fin<T> RenderInputPanel<T>(Func<InputPanelControl, Fin<Unit>> build, Func<InputPanelControl, Fin<T>> render, string name) =>
        from validBuild in Optional(build).ToFin(Fail: Op.Of(name: name).InvalidInput())
        from validRender in Optional(render).ToFin(Fail: Op.Of(name: name).InvalidInput())
        from result in GrasshopperUi.Protect(valid: () => {
            InputPanelControl panel = new();
            return validBuild(arg: panel).Bind(_ => validRender(arg: panel));
        }, name: name)
        select result;
}

[BoundaryAdapter]
public sealed record GrasshopperUi {
    private readonly Option<Window> parent;

    public GrasshopperUi(Window? parent = null) => this.parent = Optional(parent);

    internal readonly record struct Scope(Option<Window> Parent);

    public Fin<T> Use<T>(UiIntent<T> intent) =>
        Optional(intent)
            .ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
            .Bind(valid => OnUiThread(run: () => valid.Run(scope: new Scope(Parent: parent))));

    internal static TControl Style<TControl>(TControl control) where TControl : Control {
        // BOUNDARY ADAPTER - Rhino host styling starts where an Eto Control enters the GH UI rail.
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control: control);
        return control;
    }

    private static Fin<T> OnUiThread<T>(Func<Fin<T>> run) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: nameof(OnUiThread)).InvalidInput())
            .Bind(valid => RhinoApp.IsOnMainThread switch {
                true => Protect(valid: valid),
                false => Try.lift<Fin<T>>(f: () => {
                    // BOUNDARY ADAPTER - Rhino owns the UI thread and InvokeAndWait exposes no return channel.
                    Fin<T> result = Fin.Fail<T>(error: Op.Of(name: nameof(OnUiThread)).InvalidResult());
                    RhinoApp.InvokeAndWait(action: () => result = Protect(valid: valid));
                    return result;
                })
                    .Run()
                    .MapFail(static _ => Op.Of(name: nameof(OnUiThread)).InvalidResult())
                    .Bind(static result => result),
            });

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Optional(valid)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
            .Bind(callback => Try.lift<Fin<T>>(f: callback)
                .Run()
                .MapFail(_ => Op.Of(name: name).InvalidResult())
                .Bind(static result => result));
}
