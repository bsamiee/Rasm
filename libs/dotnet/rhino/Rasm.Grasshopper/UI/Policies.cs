using Eto.Drawing;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI.Canvas;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.PlugIns;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Policies {
    // --- [CANVASES]
    public static IO<Unit> MakeReadOnly(Canvas canvas) =>
        IO.lift(() => {
            _ = toSeq(typeof(CanvasActions).GetProperties())
                .Filter(static property => property.PropertyType == typeof(bool))
                .Iter(property => property.SetValue(canvas.AllowedActions, value: false));
            (canvas.WindowSelectObjects, canvas.WindowSelectWires, canvas.WindowSelectGroups) = (false, false, false);
        });

    public static IO<Unit> RestrictWires(Canvas canvas, Func<IParameter, IParameter, bool> permits, IPlugInLifetime owner) =>
        IO.lift(() => {
            canvas.AllowedActions.MakeWireFilter = wire => owner.Answer(IO.lift(() => permits(wire.source, wire.target)), failed: false);
            canvas.AllowedActions.AllowModifyWire = false;
        });

    // --- [EDITING]
    public static IO<Unit> ShowInlineEditor<T, TKey>(
        Canvas canvas, RectangleF frame, T current, ITypeAssistant<TKey> assistant, Func<TKey, Fin<T>> create, Func<T, IO<Unit>> assign, IPlugInLifetime owner)
        where T : IConvertible<TKey>
        where TKey : notnull =>
        IO.lift(() => {
            Pear<TKey> existing = Garden.Pear(current.ToValue());
            canvas.ShowInlineEditor(
                frame,
                assistant.Format(existing.Item),
                text => owner.Answer(Entered(text), Result<TKey>.Fail("", text)),
                () => owner.Run(assign(current)));

            IO<IResult> Entered(string text) =>
                from parsed in IO.lift(() => assistant.Parse(constructor: null, text, existing))
                from answer in parsed.Success
                    ? create(parsed.Value).Match(
                        Succ: value => assign(value).Map((IResult)parsed),
                        Fail: error => IO.pure<IResult>(Result<TKey>.Fail(ErrorOps.Messages(error), text)))
                    : IO.pure<IResult>(parsed)
                select answer;
        });
}
