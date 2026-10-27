using System.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Settings;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.Render.Content;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInViews : IPlugInLifetime {
    public PlugIn PlugIn { get; }

    public ViewCatalog Views { get; }

    public TimeProvider Clock { get; }

    public Option<HelpTopic> Help { get; }

    public SettingsNode Settings { get; }

    public Seq<DocumentFrame> Frames { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError<RhinoFault>]
public sealed partial class HelpTopic {
    public IO<Unit> Show => IO.lift(() => RhinoFault.Unless(RhinoHelp.Show(_value)));

    public static Option<HelpTopic> Of(Option<HelpTopic> row, IPlugInViews owner) => row || owner.Help;

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

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class EngineScope {
    public static readonly EngineScope Own = new(alwaysShow: false, static plugIn => plugIn.Id);
    public static readonly EngineScope Every = new(alwaysShow: true, static _ => new Guid(0x99999999, 0x9999, 0x9999, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99));

    public bool AlwaysShow { get; }

    [UseDelegateFromConstructor]
    public partial Guid TabEngineId(PlugIn plugIn);
}

public sealed record NavigationText(bool Bold, Option<Color> Color);

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

[Union]
public abstract partial record ContentScope {
    public bool Covers(RenderContent content) =>
        Switch(
            content,
            kinds: static (held, scope) => scope.Mask.HasFlag(ContentKinds.KindOf(held)),
            types: static (held, scope) => scope.TypeIds.Contains(held.TypeId));

    public sealed record Kinds(RenderContentKind Mask) : ContentScope;

    public sealed record Types(HashSet<Guid> TypeIds) : ContentScope;
}

public sealed record ContentMenuRow(
    string Caption,
    GlyphSource Icon,
    ContentScope Scope,
    HashSet<RenderContentMenu.Context> Contexts,
    RenderContentMenu.SeparatorStyle Separator,
    bool TopLevel,
    Func<Seq<RenderContent>, IO<Unit>> Run);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record View {
    public required Type Identity { get; init; }
    public required string Caption { get; init; }
    public Option<GlyphSource> Icon { get; init; }
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
        public Option<PlugInSetting<bool>> Suppression { get; init; }
    }

    public sealed record Panel : View {
        public Option<PanelType> Kind { get; init; }
    }

    public sealed record Section : View {
        public required Either<Func<Section, IPlugInViews, DefinedSection>, Func<Section, IPlugInViews, DefinedContentSection>> Create { get; init; }
        public Seq<Command<RowRule, Invoke>> Actions { get; init; }
    }

    public sealed record OptionsPage : View {
        public required Func<IPlugInViews, DefinedOptionsPage> Create { get; init; }
        public required CommitMode Commit { get; init; }
        public Option<NavigationText> Navigation { get; init; }
        public Seq<OptionsPage> SubPages { get; init; }
    }

    public sealed record DocumentPage : View {
        public required Func<IPlugInViews, DefinedOptionsPage> Create { get; init; }
        public required CommitMode Commit { get; init; }
        public Option<NavigationText> Navigation { get; init; }
        public Seq<DocumentPage> SubPages { get; init; }
    }

    public sealed record PropertiesPage : View {
        public required Func<IPlugInViews, DefinedPropertiesPage> Create { get; init; }
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
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewStore {
    public Option<Func<RhinoDoc, Guid, ValueStore<HashSet<EntryKey>>>> Locks { get; init; }
    public Option<Func<IPlugInViews, RhinoDoc, IO<BindingGroup>>> Bindings { get; init; }

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

    public Seq<HolderRow> Holders =>
        Switch(
            control: static _ => Seq<HolderRow>(),
            holder: static row => Seq(row),
            container: static row => row.Children.Bind(static child => child.Holders));

    public Seq<View.Section> Sections => Holders.Bind(static holder => holder.Sections);
}

public sealed record ViewCatalog(Seq<View> Rows) {
    public static ViewCatalog Empty { get; } = new(Seq<View>());

    public Option<TCase> Find<TCase>(Guid id) where TCase : View =>
        Rows.Bind(Tree).Find(view => view.Identity.GUID == id).Bind(static view => Optional(view as TCase));

    public Seq<TCase> Listed<TCase>() where TCase : View =>
        Rows.Choose(static view => Optional(view as TCase));

    public static Seq<View> Tree(View view) =>
        view.Cons(view.Children.Bind(static child => child.Sections).Bind(static section => Tree(section)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewOps {
    // --- [STORES]
    public static IO<BindingGroup> Bindings(IPlugInViews owner, View view, RowScope scope) =>
        view.Store.Bind(static store => store.Bindings).Match(
            Some: stored => IO.lift(() => scope.Document.ToFin(new RhinoFault(view.Identity.Name, RhinoReason.Unheld))).Bind(doc => stored(owner, doc)),
            None: () => IO.pure(new BindingGroup(view.Children.Bind(static child => child.Controls).Choose(static rows => rows.Bound).Map(bound => bound(scope)))));

    public static Option<ValueStore<HashSet<EntryKey>>> Locks(View view, Option<RhinoDoc> document) =>
        from doc in document
        from locks in view.Store.Bind(static store => store.Locks)
        select locks(doc, view.Identity.GUID);

    public static SettingsNode Settings(IPlugInViews owner, View view) => owner.Settings.Child(EntryKey.TypeOwner(view.Identity.GUID));

    public static Fin<Option<HistoryBinding>> History(IPlugInViews owner, View view, BindingGroup bound, Option<RhinoDoc> document) =>
        view.Store
            .Bind(store => owner.Frames.Choose(static frame => Optional(frame as HistoryFrame)).Head.Map(frame => store.Switch(
                    (Owner: owner, View: view, Document: document, Frame: frame),
                    applicationStores: static (state, _) => new HistoryScope.ApplicationScope(state.Frame.Registry),
                    documentStores: static (state, held) => state.Document
                        .Map(doc => (HistoryScope)new HistoryScope.DocumentScope(doc, held.Redraw, state.Owner))
                        .ToFin(new RhinoFault(state.View.Identity.Name, RhinoReason.Unheld)))
                .Map(scope => new HistoryBinding(view.Identity.GUID, RowText.Localize(view.Caption, owner).Local, bound, scope, frame.Limits, owner.Clock))))
            .Traverse(static held => held)
            .As();

    // --- [REALIZATION]
    public static IO<Control> Realize(IPlugInViews owner, Seq<Child> children, RowScope scope) =>
        from stack in Containers.Frame(static () => new RhinoNestedStackLayout(Orientation.Vertical, RhinoLayout.SpacingType.Panel))
        from parts in Runs(children).TraverseM(run => Part(owner, run, scope)).As()
        from configured in IO.lift(() => {
            stack.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            _ = parts.Iter(part => stack.Items.Add(new StackLayoutItem(part.Body, part.Fill)));
        })
        select (Control)stack;

    private static Seq<(Child Lead, Seq<Child> Run)> Runs(Seq<Child> children) =>
        children.FoldBack(Seq<(Child Lead, Seq<Child> Run)>(), static (runs, child) =>
            runs.Match(
                Empty: () => Seq((Lead: child, Run: Seq(child))),
                Tail: (next, rest) => child.IsControl && next.Lead.IsControl
                    ? (Lead: child, Run: child.Cons(next.Run)).Cons(rest)
                    : (Lead: child, Run: Seq(child)).Cons(runs)));

    private static IO<(Control Body, bool Fill)> Part(IPlugInViews owner, (Child Lead, Seq<Child> Run) run, RowScope scope) =>
        run.Lead.Switch(
                (Owner: owner, run.Run, Scope: scope),
                control: static (state, _) => RowGrid.Realize(Joined(state.Run.Map(static child => child.AsControl)), state.Scope).Map(static grid => grid.View),
                holder: static (state, holder) => SectionHolder.Hold(state.Owner, holder, state.Scope).Map(static held => (Control)held),
                container: static (state, container) => Containers.Realize(state.Owner, container, state.Scope))
            .Map(body => (Body: body, Fill: run.Run.Exists(static child => child.Fill)));

    private static ControlRows Joined(Seq<ControlRows> runs) =>
        new(scope => runs.TraverseM(rows => rows.Read(scope)).As().Map(static rows => rows.Flatten()), scope => runs.TraverseM(rows => rows.Signals(scope)).As().Map(static signals => signals.Flatten())) {
            Fill = runs.Exists(static rows => rows.Fill),
        };

    // --- [LISTENERS]
    public static IO<Unit> WhileLoaded(Control control, IO<Unit> listen, IPlugInLifetime owner) =>
        from scope in IO.env
        from held in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
        from loaded in Subscriptions.Host<EventArgs>(h => control.LoadComplete += h, h => control.LoadComplete -= h).Inline(_ => Toggled(held, shown: true, listen).Under(scope), owner)
        from unloaded in Subscriptions.Host<EventArgs>(h => control.UnLoad += h, h => control.UnLoad -= h).Inline(_ => Toggled(held, shown: false, listen).Under(scope), owner)
        from current in IO.lift(() => control.Loaded).Bind(shown => Toggled(held, shown, listen))
        select unit;

    public static IO<Unit> Toggled(Ref<Option<Lease<Unit>>> held, bool shown, IO<Unit> acquire) =>
        shown
            ? IO.lift(() => held.Value.IsSome).Bind(present => unless(present, Products.Swapped(held, acquire)).As())
            : Products.Cleared(held);
}
