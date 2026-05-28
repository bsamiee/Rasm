using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum PagePhase { Apply, Cancel, Activate, Script, Display, Update, Modify, Defaults, Help, CreateParent, SizeParent }
public enum PageHost { Options, DocumentProperties, ObjectProperties }

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct PageContext(PagePhase Phase, bool Active = false, RhinoDoc? Document = null, RunMode Mode = RunMode.Interactive, global::Rhino.UI.ObjectPropertiesPageEventArgs? Args = null, IntPtr ParentHandle = default, int Width = 0, int Height = 0) {
    public Option<uint> EventSerialNumber => Optional(Args).Map(static args => args.EventRuntimeSerialNumber);
    public Option<uint> DocumentSerialNumber => Optional(Args).Map(static args => args.DocRuntimeSerialNumber) | Optional(Document).Map(static document => document.RuntimeSerialNumber);
    public Option<global::Rhino.UI.ObjectPropertiesPage> Page => Optional(Args).Bind(static args => Optional(args.Page));
    public Option<RhinoView> View => Optional(Args).Bind(static args => Optional(args.View));
    public Option<RhinoViewport> Viewport => Optional(Args).Bind(static args => Optional(args.Viewport));
    public int ObjectCount => Optional(Args).Map(static args => args.ObjectCount).IfNone(0);
    public ObjectType ObjectTypes => Optional(Args).Map(static args => (ObjectType)args.ObjectTypes).IfNone(ObjectType.None);

    public Fin<RhinoDoc> RequireDocument() =>
        (Document, Args) switch {
            (RhinoDoc document, _) => Fin.Succ(value: document),
            (_, { Document: RhinoDoc document }) => Fin.Succ(value: document),
            _ => Fin.Fail<RhinoDoc>(error: Op.Of(name: nameof(RequireDocument)).InvalidInput()),
        };

    public Fin<Seq<TObject>> Objects<TObject>() where TObject : RhinoObject => Op.Of(name: nameof(Objects)).Need(Args).Map(static args => toSeq(args.GetObjects<TObject>()));
    public Fin<Seq<RhinoObject>> Objects(ObjectType filter) => Op.Of(name: nameof(Objects)).Need(Args).Map(args => toSeq(args.GetObjects(filter: filter)));
    public Fin<Seq<Guid>> ObjectIds() => Objects<RhinoObject>().Map(static objects => objects.Map(static value => value.Id));
    public Fin<Seq<(Guid Id, ObjectType Type)>> Snapshot(ObjectType filter = ObjectType.AnyObject) =>
        Objects(filter: filter).Map(static objects => objects.Map(static native => (native.Id, native.ObjectType)));
    public Fin<RhinoUi> Ui() { RunMode mode = Mode; return RequireDocument().Map(document => new RhinoUi(document: document, mode: mode)); }
}

public readonly record struct PageMetadata(
    Option<string> LocalTitle = default,
    Option<string> IconResource = default,
    int Index = -1,
    Option<global::Rhino.UI.PropertyPageType> PageType = default);

public sealed record PageRegistration<TPage>(TPage Page, PageHost Host) where TPage : class {
    public Fin<Unit> Add(ICollection<TPage> pages) =>
        from page in Op.Of(name: nameof(Add)).Need(Page)
        from validPages in Op.Of(name: nameof(Add)).Need(pages)
        from added in Host switch {
            PageHost.Options or PageHost.DocumentProperties => RhinoUi.Protect(valid: () => {
                validPages.Add(item: page);
                return Fin.Succ(value: unit);
            }),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Add)).InvalidInput()),
        }
        select added;

    public Fin<Unit> Add(global::Rhino.UI.ObjectPropertiesPageCollection pages) =>
        (Page, pages, Host) switch {
            (global::Rhino.UI.ObjectPropertiesPage page, global::Rhino.UI.ObjectPropertiesPageCollection collection, PageHost.ObjectProperties) => RhinoUi.Protect(valid: () => {
                collection.Add(page: page);
                return Fin.Succ(value: unit);
            }),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Add)).InvalidInput()),
        };
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmOptionsPage : global::Rhino.UI.OptionsDialogPage {
    private readonly Control control;
    private readonly bool showApplyButton;
    private readonly bool showDefaultsButton;

    protected RasmOptionsPage(string englishTitle, Control control, bool showApplyButton = true, bool showDefaultsButton = false) : base(englishTitle) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        ArgumentNullException.ThrowIfNull(argument: control);
        this.control = control;
        this.showApplyButton = showApplyButton;
        this.showDefaultsButton = showDefaultsButton;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;
    public sealed override bool ShowApplyButton => showApplyButton;
    public sealed override bool ShowDefaultsButton => showDefaultsButton;
    public sealed override bool OnApply() => Apply(phase: PagePhase.Apply);
    public sealed override void OnCancel() => _ = ResultOf(context: new PageContext(Phase: PagePhase.Cancel));
    public sealed override bool OnActivate(bool active) => Apply(phase: PagePhase.Activate, active: active);
    public sealed override Result RunScript(RhinoDoc doc, RunMode mode) => ResultOf(context: new PageContext(Phase: PagePhase.Script, Document: doc, Mode: mode));
    public sealed override void OnDefaults() => _ = ResultOf(context: new PageContext(Phase: PagePhase.Defaults));
    public sealed override void OnHelp() => _ = ResultOf(context: new PageContext(Phase: PagePhase.Help));
    public sealed override void OnCreateParent(IntPtr hwndParent) => _ = ResultOf(context: new PageContext(Phase: PagePhase.CreateParent, ParentHandle: hwndParent));
    public sealed override void OnSizeParent(int width, int height) => _ = ResultOf(context: new PageContext(Phase: PagePhase.SizeParent, Width: width, Height: height));

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
    private readonly PageMetadata metadata;

    protected RasmPropertiesPage(string englishTitle, Control control, ObjectType supportedTypes = ObjectType.AnyObject, bool allObjectsMustBeSupported = false, bool supportsSubObjects = false, PageMetadata metadata = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: englishTitle);
        ArgumentNullException.ThrowIfNull(argument: control);
        this.englishTitle = englishTitle;
        this.control = control;
        this.supportedTypes = supportedTypes;
        this.allObjectsMustBeSupported = allObjectsMustBeSupported;
        this.supportsSubObjects = supportsSubObjects;
        this.metadata = metadata;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(control);
    }

    public sealed override object PageControl => control;
    public sealed override string EnglishPageTitle => englishTitle;
    public sealed override string LocalPageTitle => metadata.LocalTitle.IfNone(englishTitle);
    public sealed override int Index => metadata.Index;
    public sealed override global::Rhino.UI.PropertyPageType PageType => metadata.PageType.IfNone(global::Rhino.UI.PropertyPageType.Custom);
    public sealed override string PageIconEmbeddedResourceString => metadata.IconResource.IfNone(string.Empty);
    public sealed override ObjectType SupportedTypes => supportedTypes;
    public sealed override bool AllObjectsMustBeSupported => allObjectsMustBeSupported;
    public sealed override bool SupportsSubObjects => supportsSubObjects;
    public sealed override bool ShouldDisplay(global::Rhino.UI.ObjectPropertiesPageEventArgs e) => ResultOf(context: new PageContext(Phase: PagePhase.Display, Args: e)) == Result.Success;
    public sealed override void UpdatePage(global::Rhino.UI.ObjectPropertiesPageEventArgs e) => _ = ResultOf(context: new PageContext(Phase: PagePhase.Update, Args: e));
    public sealed override bool OnActivate(bool active) => ResultOf(context: new PageContext(Phase: PagePhase.Activate, Active: active)) == Result.Success;
    public sealed override Result RunScript(global::Rhino.UI.ObjectPropertiesPageEventArgs e) => ResultOf(context: new PageContext(Phase: PagePhase.Script, Mode: RunMode.Scripted, Args: e));
    public sealed override void OnHelp() => _ = ResultOf(context: new PageContext(Phase: PagePhase.Help));
    public sealed override void OnCreateParent(IntPtr hwndParent) => _ = ResultOf(context: new PageContext(Phase: PagePhase.CreateParent, ParentHandle: hwndParent));
    public sealed override void OnSizeParent(int width, int height) => _ = ResultOf(context: new PageContext(Phase: PagePhase.SizeParent, Width: width, Height: height));

    protected Fin<Unit> Modify(Func<PageContext, Fin<Unit>> change) =>
        Op.Of(name: nameof(Modify)).Need(change).Bind(valid => RhinoUi.Protect(valid: () => {
            Fin<Unit> result = Fin.Fail<Unit>(error: Op.Of(name: nameof(Modify)).InvalidResult());
            ModifyPage(callbackAction: args => result = valid(arg: new PageContext(Phase: PagePhase.Modify, Args: args)));
            return result;
        }));

    protected virtual Fin<Result> Change(PageContext context) =>
        context.Phase switch {
            PagePhase.Display => Op.Of(name: nameof(Change)).Need(context.Args)
                .Map(valid => valid.IncludesObjectsType(objectTypes: SupportedTypes, allMustMatch: AllObjectsMustBeSupported) switch {
                    true => Result.Success,
                    false => Result.Cancel,
                }),
            _ => Fin.Succ(value: Result.Success),
        };

    private Result ResultOf(PageContext context) => RhinoUi.Protect(valid: () => Change(context: context)).Match(Succ: static result => result, Fail: static _ => Result.Failure);
}
