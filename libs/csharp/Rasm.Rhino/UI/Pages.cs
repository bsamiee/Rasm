using Eto.Forms;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PagePhase {
    public static readonly PagePhase Apply = new(0), Cancel = new(1), Activate = new(2), Script = new(3), Display = new(4), Update = new(5), Modify = new(6), Defaults = new(7), Help = new(8), CreateParent = new(9), SizeParent = new(10);
}
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
    Option<global::Rhino.UI.PropertyPageType> PageType = default) {
    // A properties page that REPLACES a built-in category (Material/Light/Dimension/NamedView/View/...): the host swaps it in.
    public static PageMetadata Replacing(global::Rhino.UI.PropertyPageType pageType, Option<string> localTitle = default, Option<string> iconResource = default, int index = -1) =>
        new(LocalTitle: localTitle, IconResource: iconResource, Index: index, PageType: Some(pageType));
    public static PageMetadata Material(Option<string> title = default, Option<string> icon = default) => Replacing(pageType: global::Rhino.UI.PropertyPageType.Material, localTitle: title, iconResource: icon);
    public static PageMetadata Light(Option<string> title = default, Option<string> icon = default) => Replacing(pageType: global::Rhino.UI.PropertyPageType.Light, localTitle: title, iconResource: icon);
    public static PageMetadata Dimension(Option<string> title = default, Option<string> icon = default) => Replacing(pageType: global::Rhino.UI.PropertyPageType.Dimension, localTitle: title, iconResource: icon);
    public static PageMetadata View(Option<string> title = default, Option<string> icon = default) => Replacing(pageType: global::Rhino.UI.PropertyPageType.View, localTitle: title, iconResource: icon);
}

// Stacked-dialog navigation/dirty algebra (options pages only — ObjectPropertiesPage is NOT a StackedDialogPage).
[Union(SwitchMapStateParameterName = "page")]
public abstract partial record PageNav {
    private PageNav() { }
    public sealed record Activate : PageNav;                                         // MakeActivePage()
    public sealed record Reveal(string PageName, bool DocumentProperties = false) : PageNav;   // SetActivePageTo
    public sealed record Remove : PageNav;                                           // RemovePage() (removes self)
    public sealed record Dirty(bool Modified = true) : PageNav;                      // Modified flag enables Apply
    public sealed record Child(global::Rhino.UI.StackedDialogPage Page) : PageNav;   // AddChildPage (Windows-only)
    public sealed record Appearance(bool Bold, DrawingColor Color) : PageNav;        // NavigationText* (Windows-only)
    public sealed record Sequence(Seq<PageNav> Steps) : PageNav;                     // chained sub-navs folded onto one page

    internal Fin<Unit> Apply(global::Rhino.UI.StackedDialogPage page) =>
        RhinoUi.Protect(valid: () => Switch(
            page,
            activate: static (p, _) => Fin.Succ(value: Op.Side(p.MakeActivePage)),
            reveal: static (p, g) => Fin.Succ(value: Op.Side(() => p.SetActivePageTo(pageName: g.PageName, documentPropertiesPage: g.DocumentProperties))),
            remove: static (p, _) => Fin.Succ(value: Op.Side(p.RemovePage)),
            dirty: static (p, d) => Fin.Succ(value: Op.Side(() => p.Modified = d.Modified)),
            child: static (p, c) => WindowsOnly(op: Op.Of(name: nameof(Child)), run: () => p.AddChildPage(pageToAdd: c.Page)),
            appearance: static (p, a) => WindowsOnly(op: Op.Of(name: nameof(Appearance)), run: () => { p.NavigationTextIsBold = a.Bold; p.NavigationTextColor = a.Color; }),
            sequence: static (p, g) => g.Steps.TraverseM(step => step.Apply(page: p)).As().Map(static _ => unit)));

    // BOUNDARY ADAPTER — AddChildPage/NavigationText* exist only on the Windows StackedDialogPage; the macOS host has no
    // equivalent, so Child/Appearance navs fail with a Caution rather than no-op silently (platform asymmetry is explicit).
    private static Fin<Unit> WindowsOnly(Op op, Action run) =>
        OperatingSystem.IsWindows()
            ? Fin.Succ(value: Op.Side(run))
            : Fin.Fail<Unit>(error: op.Caution(concern: "stacked-dialog navigation member is Windows-only"));
}

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

    // Stacked-dialog navigation/dirty driven through the PageNav algebra (this page IS a StackedDialogPage).
    public Fin<Unit> Navigate(PageNav nav) => Op.Of(name: nameof(Navigate)).Need(nav).Bind(valid => valid.Apply(page: this));

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
    public sealed override bool ShouldDisplay(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        ResultOf(context: new PageContext(Phase: PagePhase.Display, Args: e)) == Result.Success;
    public sealed override void UpdatePage(global::Rhino.UI.ObjectPropertiesPageEventArgs e) =>
        _ = Op.SideWhen(ShouldDisplay(e: e), () => _ = ResultOf(context: new PageContext(Phase: PagePhase.Update, Args: e)));
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
        context.Phase == PagePhase.Display
            ? Op.Of(name: nameof(Change)).Need(context.Args)
                .Map(valid => valid.IncludesObjectsType(objectTypes: SupportedTypes, allMustMatch: AllObjectsMustBeSupported) switch {
                    true => Result.Success,
                    false => Result.Cancel,
                })
            : Fin.Succ(value: Result.Success);

    private Result ResultOf(PageContext context) => RhinoUi.Protect(valid: () => Change(context: context)).Match(Succ: static result => result, Fail: static _ => Result.Failure);
}
