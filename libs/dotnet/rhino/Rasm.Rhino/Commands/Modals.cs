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
    public static IO<T> Ask<T>(Func<(Result Result, T Value)> ask, string member) =>
        IO.lift(ask).Bind(answer => IO.lift(Conversions.FromResult(answer.Result, member).Map(_ => answer.Value)));

    public static IO<T> Ask<TSeed, T>(TSeed seed, Func<TSeed, (Result Result, T Value)> ask, string member) =>
        Ask(() => ask(seed), member);

    public static IO<T> Get<TGetter, T>(Func<TGetter> create, Func<TGetter, (Result Result, T Value)> get, string member) where TGetter : IDisposable =>
        use(create).Bind(getter => Ask(() => get(getter), member)).Bracket();

    // --- [VIEWS]
    public static IO<ViewportTarget> GetView(string prompt) =>
        Ask(() => (RhinoGet.GetView(prompt, out RhinoView view), view), nameof(RhinoGet.GetView))
            .Bind(static view => IO.lift(Target(view, nameof(RhinoGet.GetView))));

    public static IO<Seq<ViewportTarget>> GetViewports(string prompt) =>
        Ask(() => (RhinoGet.GetViewports(prompt, out RhinoViewport[] viewports), Conversions.Rows(viewports)), nameof(RhinoGet.GetViewports))
            .Bracket(
                Use: static viewports => IO.lift(() => viewports.Map(static viewport => (ViewportTarget)new ViewportTarget.Id(viewport.Id)).Strict()),
                Fin: DisposalOps.Release);

    public static IO<(Rectangle Client, ViewportTarget Viewport)> Get2dRectangle(bool solidPen) =>
        Ask(() => (RhinoGet.Get2dRectangle(solidPen, out Rectangle rectangle, out RhinoView view), (Client: rectangle, View: view)), nameof(RhinoGet.Get2dRectangle))
            .Bind(static answer => IO.lift(Target(answer.View, nameof(RhinoGet.Get2dRectangle)).Map(target => (answer.Client, target))));

    private static Fin<ViewportTarget> Target(RhinoView? view, string member) =>
        Missing.Unless(view, member).Map(static found => (ViewportTarget)new ViewportTarget.Id(found.ActiveViewportID));

    // --- [VALUES]
    public static IO<string> GetFileName(RunMode runMode, GetFileNameMode mode, string defaultName, string title, Option<RhinoGet.BitmapFileTypes> fileTypes) =>
        IO.lift(() => Conversions.Present(runMode switch {
                RunMode.Interactive => fileTypes.Match(
                    Some: types => RhinoGet.GetFileName(mode, defaultName, title, parent: null, types),
                    None: () => RhinoGet.GetFileName(mode, defaultName, title, parent: null)),
                RunMode.Scripted => RhinoGet.GetFileNameScripted(mode, defaultName),
            })
            .ToFin(Errors.Cancelled));

    public static IO<bool> GetBool(RhinoDoc doc, CallbackSite site, string prompt, bool acceptNothing, LabelPair labels, bool defaultValue) =>
        Getters.Choice(doc, new GetterRequest<GetOption, bool>(prompt, site) {
            Accept = new() { Nothing = acceptNothing ? Some((Shown: Some(defaultValue ? labels.On : labels.Off), Then: IO.pure(defaultValue))) : None },
            Options = Seq(OptionSpec.Plain(labels.On, None, hidden: false, IO.pure(value: true)), OptionSpec.Plain(labels.Off, None, hidden: false, IO.pure(value: false))),
        });
}
