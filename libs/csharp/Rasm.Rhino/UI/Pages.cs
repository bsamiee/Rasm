using Eto.Forms;

namespace Rasm.Rhino.UI;

public enum PagePhase { Apply, Cancel, Activate, Script, Display, Update }

public readonly record struct PageContext(PagePhase Phase, bool Active = false, RhinoDoc? Document = null, RunMode Mode = RunMode.Interactive, global::Rhino.UI.ObjectPropertiesPageEventArgs? Args = null);

public abstract class RasmOptionsPage : global::Rhino.UI.OptionsDialogPage {
    private readonly Control control;
    private readonly bool showApplyButton;

    protected RasmOptionsPage(string englishTitle, Control control, bool showApplyButton = true) : base(englishTitle) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        ArgumentNullException.ThrowIfNull(argument: control);
        this.control = control;
        this.showApplyButton = showApplyButton;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;
    public sealed override bool ShowApplyButton => showApplyButton;
    public sealed override bool OnApply() => Apply(phase: PagePhase.Apply);
    public sealed override void OnCancel() => _ = Change(context: new PageContext(Phase: PagePhase.Cancel));
    public sealed override bool OnActivate(bool active) => Apply(phase: PagePhase.Activate, active: active);
    public sealed override Result RunScript(RhinoDoc doc, RunMode mode) => ResultOf(context: new PageContext(Phase: PagePhase.Script, Document: doc, Mode: mode));

    protected virtual Fin<Result> Change(PageContext context) => Fin.Succ(value: Result.Success);

    private bool Apply(PagePhase phase, bool active = false) => ResultOf(context: new PageContext(Phase: phase, Active: active)) == Result.Success;
    private Result ResultOf(PageContext context) => RhinoUi.Protect(valid: () => Change(context: context)).Match(Succ: static result => result, Fail: static _ => Result.Failure);
}

public abstract class RasmPropertiesPage : global::Rhino.UI.ObjectPropertiesPage {
    private readonly Control control;
    private readonly string englishTitle;
    private readonly ObjectType supportedTypes;
    private readonly bool allObjectsMustBeSupported;
    private readonly bool supportsSubObjects;

    protected RasmPropertiesPage(string englishTitle, Control control, ObjectType supportedTypes = ObjectType.AnyObject, bool allObjectsMustBeSupported = false, bool supportsSubObjects = false) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        ArgumentNullException.ThrowIfNull(argument: control);
        this.englishTitle = englishTitle;
        this.control = control;
        this.supportedTypes = supportedTypes;
        this.allObjectsMustBeSupported = allObjectsMustBeSupported;
        this.supportsSubObjects = supportsSubObjects;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;
    public sealed override string EnglishPageTitle => englishTitle;
    public sealed override ObjectType SupportedTypes => supportedTypes;
    public sealed override bool AllObjectsMustBeSupported => allObjectsMustBeSupported;
    public sealed override bool SupportsSubObjects => supportsSubObjects;
    public sealed override bool ShouldDisplay(global::Rhino.UI.ObjectPropertiesPageEventArgs e) => ResultOf(context: new PageContext(Phase: PagePhase.Display, Args: e)) == Result.Success;
    public sealed override void UpdatePage(global::Rhino.UI.ObjectPropertiesPageEventArgs e) => _ = ResultOf(context: new PageContext(Phase: PagePhase.Update, Args: e));
    public sealed override bool OnActivate(bool active) => ResultOf(context: new PageContext(Phase: PagePhase.Activate, Active: active)) == Result.Success;
    public sealed override Result RunScript(global::Rhino.UI.ObjectPropertiesPageEventArgs e) => ResultOf(context: new PageContext(Phase: PagePhase.Script, Args: e));

    protected Fin<Unit> Modify(Func<global::Rhino.UI.ObjectPropertiesPageEventArgs, Fin<Unit>> change) =>
        Optional(change).ToFin(Fail: Op.Of(name: nameof(Modify)).InvalidInput()).Bind(valid => RhinoUi.Protect(valid: () => {
            Fin<Unit> result = Fin.Fail<Unit>(error: Op.Of(name: nameof(Modify)).InvalidResult());
            ModifyPage(callbackAction: args => result = valid(arg: args));
            return result;
        }));

    protected virtual Fin<Result> Change(PageContext context) =>
        context.Phase == PagePhase.Display
            ? Optional(context.Args).ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput()).Map(valid => valid.IncludesObjectsType(objectTypes: SupportedTypes, allMustMatch: AllObjectsMustBeSupported) ? Result.Success : Result.Cancel)
            : Fin.Succ(value: Result.Success);

    private Result ResultOf(PageContext context) => RhinoUi.Protect(valid: () => Change(context: context)).Match(Succ: static result => result, Fail: static _ => Result.Failure);
}
