using Eto.Drawing;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI.Canvas;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Policies {
    // --- [CANVASES]
    public static IO<Unit> MakeReadOnly(Canvas canvas) =>
        IO.lift(() => {
            CanvasActions actions = canvas.AllowedActions;
            (actions.AllowDrag, actions.AlloWireSelect, actions.AllowObjectSelect, actions.AllowMakeWire, actions.AllowDeleteWire, actions.AllowModifyWire) = (false, false, false, false, false, false);
            (actions.AllowMakeObject, actions.AllowDeleteObject, actions.AllowObjectResponse, actions.AllowDropFile) = (false, false, false, false);
            (actions.AllowWireMenu, actions.AllowObjectMenu, actions.AllowCanvasMenu) = (false, false, false);
            (canvas.WindowSelectObjects, canvas.WindowSelectWires, canvas.WindowSelectGroups) = (false, false, false);
        });

    public static IO<Unit> RestrictWires(Canvas canvas, Func<IParameter, IParameter, bool> permits, IGrasshopperPlugin owner) =>
        IO.lift(() => {
            canvas.AllowedActions.MakeWireFilter = wire => owner.Session.Answer(IO.lift(() => permits(wire.source, wire.target)), failed: false);
            canvas.AllowedActions.AllowModifyWire = false;
        });

    // --- [EDITING]
    public static IO<Unit> ShowInlineEditor<T, TKey>(
        Canvas canvas, RectangleF frame, T current, ITypeAssistant<TKey> assistant, Func<TKey, Fin<T>> create, Func<T, IO<Unit>> assign, IGrasshopperPlugin owner)
        where T : IConvertible<TKey>
        where TKey : notnull =>
        IO.lift(() => {
            Pear<TKey> existing = Garden.Pear(current.ToValue());
            canvas.ShowInlineEditor(
                frame,
                assistant.Format(existing.Item),
                text => owner.Session.Answer(Entered(text), Result<TKey>.Fail("", text)),
                () => owner.Session.Run(assign(current)));

            IO<IResult> Entered(string text) =>
                from parsed in IO.lift(() => assistant.Parse(constructor: null, text, existing))
                from answer in parsed.Success
                    ? create(parsed.Value).Match(
                        Succ: value => assign(value).Map((IResult)parsed),
                        Fail: error => IO.pure<IResult>(Result<TKey>.Fail(error.Message, text)))
                    : IO.pure<IResult>(parsed)
                select answer;
        });
}
