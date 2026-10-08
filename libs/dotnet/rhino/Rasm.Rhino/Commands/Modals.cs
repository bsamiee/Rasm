using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Modals {
    // --- [ASKS]
    public static IO<T> Ask<T>(IO<(Result Result, T Value)> ask, string member) =>
        from answer in ask
        from accepted in IO.lift(Conversions.FromResult(answer.Result, member))
        select answer.Value;

    public static IO<T> Get<TGetter, T>(Func<TGetter> create, Func<TGetter, (Result Result, T Value)> get, string member) where TGetter : IDisposable =>
        Ask(use(create).Map(get), member).Bracket();

    // --- [VIEWS]
    public static IO<ViewportTarget> GetView(string prompt) =>
        Ask(IO.lift(() => (RhinoGet.GetView(prompt, out RhinoView view), view)), nameof(RhinoGet.GetView))
            .Bind(static view => IO.lift(Target(view, nameof(RhinoGet.GetView))));

    public static IO<Seq<ViewportTarget>> GetViewports(string prompt) =>
        Ask(IO.lift(() => (RhinoGet.GetViewports(prompt, out RhinoViewport[] viewports), Conversions.Rows(viewports))), nameof(RhinoGet.GetViewports))
            .Bracket(
                Use: static viewports => IO.lift(() => viewports.Map(static viewport => (ViewportTarget)new ViewportTarget.Id(viewport.Id)).Strict()),
                Fin: DisposalOps.Release);

    public static IO<(Rectangle Client, ViewportTarget Viewport)> Get2dRectangle(bool solidPen) =>
        from answer in Ask(IO.lift(() => (RhinoGet.Get2dRectangle(solidPen, out Rectangle rectangle, out RhinoView view), (Client: rectangle, View: view))), nameof(RhinoGet.Get2dRectangle))
        from target in IO.lift(Target(answer.View, nameof(RhinoGet.Get2dRectangle)))
        select (answer.Client, target);

    private static Fin<ViewportTarget> Target(RhinoView? view, string member) =>
        Missing.Unless(view, member).Map(static found => (ViewportTarget)new ViewportTarget.Id(found.ActiveViewportID));

    // --- [VALUES]
    public static IO<string> GetFileName(RunMode runMode, GetFileNameMode mode, string defaultName, string title, Option<RhinoGet.BitmapFileTypes> fileTypes, Option<object> parent = default) =>
        IO.lift(() => Conversions.Present(runMode switch {
            RunMode.Interactive => RhinoGet.GetFileName(mode, defaultName, title, parent.ValueUnsafe(), fileTypes.IfNone(static () => RhinoGet.AllBitmapFileTypes)),
            RunMode.Scripted => RhinoGet.GetFileNameScripted(mode, defaultName),
        }).ToFin(Errors.Cancelled));

    public static IO<bool> GetBool(RhinoDoc doc, CallbackSite site, string prompt, LabelPair labels, Option<bool> defaultValue) =>
        Getters.Choice(doc, new GetterRequest<GetOption, bool>(prompt, site) {
            Accept = new() { Nothing = defaultValue.Map(value => (Shown: Some(value ? labels.On : labels.Off), Then: IO.pure(value))) },
            Options = Seq(a: true, b: false).Map(value => OptionSpec.Plain(value ? labels.On : labels.Off, None, hidden: false, _ => IO.pure(value))),
        });
}
