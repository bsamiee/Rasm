using System.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Views;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.UI;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError<RhinoFault>]
public sealed partial class HelpTopic {
    public IO<Unit> Show => Products.Accepted(() => RhinoHelp.Show(_value));

    public static Option<HelpTopic> Of(Option<HelpTopic> row) => row || ViewCatalog.Current.Definition.Help;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        (value, validationError) = value.TrimOrNullify() is { } trimmed
            ? (trimmed, default(RhinoFault))
            : (value, new RhinoFault($"value = '{value}'", RhinoReason.Invalid));
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class WindowKind {
    public static readonly WindowKind Standard = new(static () => Eto.Platform.Instance.Create<Form.IHandler>());
    public static readonly WindowKind Floating = new(static () => Eto.Platform.Instance.Create<FloatingForm.IHandler>());

    [UseDelegateFromConstructor]
    public partial Form.IHandler Handler();
}

[SmartEnum<OptionsDialogPage.PageType>(SkipIComparable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class PageDialog {
    public static readonly PageDialog Options = new(OptionsDialogPage.PageType.Options, "OptionsPage", RhinoEtoApp.ApplicationPreferencesWindowForPage);
    public static readonly PageDialog DocumentProperties = new(OptionsDialogPage.PageType.DocumentProperties, "DocumentPropertiesPage", RhinoEtoApp.DocumentPropertiesWindowForPage);

    public string Command { get; }

    [UseDelegateFromConstructor]
    public partial Window? WindowForPage(OptionsDialogPage page);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageSubject {
    public sealed record Objects : PageSubject {
        public Option<ObjectType> SupportedTypes { get; init; }
        public Option<bool> AllObjectsMustBeSupported { get; init; }
        public Option<bool> SupportsSubObjects { get; init; }
        public Option<PropertyPageType> PageType { get; init; }
    }

    public sealed record Viewport(HashSet<ViewportType> ViewportTypes) : PageSubject;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Placement {
    public sealed record Last : Placement;

    public sealed record Into(Guid DockBar) : Placement;

    public sealed record Beside(Guid Panel) : Placement;

    public sealed record Floating : Placement;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record View {
    public required Type Identity { get; init; }
    public required string Caption { get; init; }
    public Option<IconResource> Icon { get; init; }
    public Option<HelpTopic> Help { get; init; }
    public Option<RowRule> Enabled { get; init; }
    public Option<RowRule> Visible { get; init; }
    public Option<ViewStore> Store { get; init; }
    public Seq<Child> Children { get; init; }

    public sealed record Window : View {
        public required WindowKind Kind { get; init; }
        public Option<Eto.Drawing.Size> ClientSize { get; init; }
    }

    public sealed record Dialog : View {
        public required bool Cancelable { get; init; }
        public Option<DialogDisplayMode> DisplayMode { get; init; }
        public Option<Preference<bool>> Suppression { get; init; }
    }

    public sealed record Panel : View {
        public Option<PanelType> Kind { get; init; }
        public Placement Placement { get; init; } = new Placement.Last();
        public Option<Eto.Drawing.Size> MinimumSize { get; init; }
    }

    public sealed record Section : View {
        public Seq<Command<Invoke>> Actions { get; init; }
    }

    public sealed record Page : View {
        public required PageDialog Kind { get; init; }
        public required CommitMode Commit { get; init; }
        public bool NavigationBold { get; init; }
        public Option<Color> NavigationColor { get; init; }
        public Seq<Page> SubPages { get; init; }
    }

    public sealed record PropertiesPage : View {
        public required PageSubject Subject { get; init; }
        public Option<int> Index { get; init; }
    }

    public sealed record Tab : View {
        public required EngineScope Scope { get; init; }
    }

    public sealed record Pane : View {
        public required EngineScope Scope { get; init; }
        public required bool InitialShow { get; init; }
        public RenderPanels.ExtraSidePanePosition Position { get; init; } = RenderPanels.ExtraSidePanePosition.Left;
    }

    public sealed record ContentSection : View {
        public required ContentScope Scope { get; init; }
        public Seq<Command<Invoke>> Actions { get; init; }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewStore {
    public Option<Func<RhinoDoc, Guid, Preference<HashSet<EntryKey>>>> Locks { get; init; }
    public Option<Func<RhinoDoc, IO<BindingGroup>>> Bindings { get; init; }

    public sealed record ApplicationStores : ViewStore;

    public sealed record DocumentStores(RedrawPolicy Redraw) : ViewStore;
}

[Union<ControlRows, HolderRow, ContainerRow>(T1Name = "Control", T2Name = "Holder", T3Name = "Container")]
public sealed partial class Child {
    public bool Fill =>
        Switch(control: static row => row.Fill, holder: static _ => true, container: static row => row.Fill);

    public Seq<ControlRows> Controls =>
        Switch(
            control: static rows => Seq(rows),
            holder: static _ => Seq<ControlRows>(),
            container: static row => row.Children.Bind(static child => child.Controls));

    public Seq<View.Section> Sections =>
        Switch(
            control: static _ => Seq<View.Section>(),
            holder: static row => row.Sections,
            container: static row => row.Children.Bind(static child => child.Sections));
}

public sealed record ViewCatalog(PlugInDefinition Definition, Seq<View> Rows) {
    private static readonly Atom<Option<ViewCatalog>> Loaded = Atom(Option<ViewCatalog>.None);

    public static ViewCatalog Current =>
        Loaded.Value.IfNone(static () => throw new InvalidOperationException($"{typeof(ViewCatalog).Assembly.GetName().Name} holds no loaded view catalog"));

    public static SettingsNode Settings => new(new SettingsRoot.PlugInNode(Session.PlugIn.Id));

    internal static IO<Unit> Load(ViewCatalog catalog) =>
        from published in IO.lift(() => Loaded.Swap(_ => Some(catalog)))
        from registered in Seq(ViewRegistration.RegisterPanels(), ContentViews.RegisterSections(), Notices.Revealing(), Toolbars.Registered()).TraverseM(static step => step).As()
        select unit;

    public Option<TCase> Find<TCase>(Guid id) where TCase : View =>
        Rows.Bind(Tree).Find(view => view.Identity.GUID == id).Bind(static view => Optional(view as TCase));

    public Seq<TCase> Listed<TCase>() where TCase : View =>
        Rows.Choose(static view => Optional(view as TCase));

    public static Seq<View> Tree(View view) =>
        view.Cons(view.Children.Bind(static child => child.Sections).Bind(static section => Tree(section)));
}
