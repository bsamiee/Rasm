using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.Display;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FileNameMethod {
    public sealed record GetFileName(Option<string> Title) : FileNameMethod;

    public sealed record GetFileNameScripted() : FileNameMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Modals {
    // --- [PICKS]
    public static IO<ViewportIdentity> GetView(string prompt) =>
        IO.lift(() => Answers.FromResult(RhinoGet.GetView(prompt, out RhinoView view), nameof(RhinoGet.GetView))
            .Bind(_ => Missing.Unless(view, nameof(RhinoGet.GetView)))
            .Map(static found => Viewports.Identity(found, found.MainViewport)));

    public static IO<Seq<ViewportIdentity>> GetViewports(string prompt) =>
        from viewports in IO.lift(() => Answers.FromResult(RhinoGet.GetViewports(prompt, out RhinoViewport[] found), nameof(RhinoGet.GetViewports))
            .Bind(_ => Missing.Unless(found, nameof(RhinoGet.GetViewports))))
        from identities in IO.lift(toSeq(viewports)
            .TraverseM(static viewport => Missing.Unless(viewport.ParentView, nameof(RhinoViewport.ParentView))
                .Map(parent => Viewports.Identity(parent, viewport)))
            .As())
        select identities;

    // --- [VALUES]
    public static IO<Color> GetColor(string prompt, bool acceptNothing, Color defaultValue) =>
        IO.lift(() => {
            Color value = defaultValue;
            return Answers.FromResult(RhinoGet.GetColor(prompt, acceptNothing, ref value), nameof(RhinoGet.GetColor)).Map(_ => value);
        });

    public static IO<bool> GetBool(string prompt, bool acceptNothing, string offLabel, string onLabel, bool defaultValue) =>
        from named in IO.lift(() => (
                InvalidOptionName.Unless(CommandLineOption.IsValidOptionName(offLabel), offLabel).ToValidation()
                & InvalidOptionName.Unless(CommandLineOption.IsValidOptionName(onLabel), onLabel).ToValidation())
            .ToFin())
        from answer in IO.lift(() => {
            bool value = defaultValue;
            return Answers.FromResult(RhinoGet.GetBool(prompt, acceptNothing, offLabel, onLabel, ref value), nameof(RhinoGet.GetBool)).Map(_ => value);
        })
        select answer;

    public static IO<double> GetAngle(string prompt, Point3d basePoint, Point3d reference, double defaultRadians) =>
        IO.lift(() => Answers.FromResult(RhinoGet.GetAngle(prompt, basePoint, reference, defaultRadians, out double angle), nameof(RhinoGet.GetAngle)).Map(_ => angle));

    public static IO<double> GetDistance(string prompt, double defaultDistance) =>
        IO.lift(() => Answers.FromResult(RhinoGet.GetDistance(prompt, defaultDistance, out double distance), nameof(RhinoGet.GetDistance)).Map(_ => distance));

    public static IO<string> GetFileName(GetFileNameMode mode, string defaultName, FileNameMethod method) =>
        IO.lift(() => Answers.Present(method.Switch(
                (Mode: mode, DefaultName: defaultName),
                getFileName: static (state, dialog) => RhinoGet.GetFileName(state.Mode, state.DefaultName, dialog.Title.ValueUnsafe(), parent: null),
                getFileNameScripted: static (state, _) => RhinoGet.GetFileNameScripted(state.Mode, state.DefaultName)))
            .ToFin(new Canceled()));

    // --- [SHAPES]
    public static IO<Plane> GetPlane() =>
        IO.lift(static () => Answers.FromResult(RhinoGet.GetPlane(out Plane plane), nameof(RhinoGet.GetPlane))
            .Bind(_ => Invalid.Unless(plane.IsValid, nameof(Plane.IsValid)))
            .Map(_ => plane));

    public static IO<Seq<Point3d>> GetRectangle(Option<string> firstPrompt) =>
        from answer in IO.lift(() => firstPrompt.Match(
            Some: static prompt => (Result: RhinoGet.GetRectangle(prompt, out Point3d[] corners), Corners: corners),
            None: static () => (Result: RhinoGet.GetRectangle(out Point3d[] corners), Corners: corners)))
        from corners in IO.lift(() => Answers.FromResult(answer.Result, nameof(RhinoGet.GetRectangle))
            .Bind(_ => Missing.Unless(answer.Corners, nameof(RhinoGet.GetRectangle))))
        select toSeq(corners);

    public static IO<Box> GetBox() =>
        IO.lift(static () => Answers.FromResult(RhinoGet.GetBox(out Box box), nameof(RhinoGet.GetBox)).Map(_ => box));

    public static IO<Line> GetLine() =>
        IO.lift(static () => Answers.FromResult(RhinoGet.GetLine(out Line line), nameof(RhinoGet.GetLine))
            .Bind(_ => Invalid.Unless(line.IsValid, nameof(Line.IsValid)))
            .Map(_ => line));

    public static IO<Polyline> GetPolyline() =>
        IO.lift(static () => Answers.FromResult(RhinoGet.GetPolyline(out Polyline polyline), nameof(RhinoGet.GetPolyline))
            .Bind(_ => Missing.Unless(polyline, nameof(RhinoGet.GetPolyline))));

    public static IO<Arc> GetArc() =>
        IO.lift(static () => Answers.FromResult(RhinoGet.GetArc(out Arc arc), nameof(RhinoGet.GetArc))
            .Bind(_ => Invalid.Unless(arc.IsValid, nameof(Arc.IsValid)))
            .Map(_ => arc));

    public static IO<Circle> GetCircle() =>
        IO.lift(static () => Answers.FromResult(RhinoGet.GetCircle(out Circle circle), nameof(RhinoGet.GetCircle))
            .Bind(_ => Invalid.Unless(circle.IsValid, nameof(Circle.IsValid)))
            .Map(_ => circle));
}
