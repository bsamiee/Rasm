using System.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInViews : IPlugInSink {
    public PlugIn PlugIn { get; }

    public ViewCatalog Views { get; }

    public TimeProvider Clock { get; }

    public Option<HelpTopic> Help { get; }

    public AtomHashMap<Guid, ValueHistory> Histories { get; }

    public IO<HistoryLimits> Limits { get; }

    public SettingsNode Settings { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class HelpTopic {
    public IO<Unit> Show => IO.lift(() => Refused.Unless(RhinoHelp.Show(_value), nameof(RhinoHelp.Show)));

    public static Option<HelpTopic> Of(Option<HelpTopic> row, IPlugInViews owner) => row || owner.Help;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        (value, validationError) = value.TrimOrNullify() is { } trimmed
            ? (trimmed, default(ValidationError))
            : (value, new ValidationError("help topic is blank"));
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

    public sealed record Viewport(LanguageExt.HashSet<ViewportType> ViewportTypes) : PageSubject;
}

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
    public Option<Func<RhinoDoc, Guid, ValueStore<LanguageExt.HashSet<EntryKey>>>> Locks { get; init; }
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

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnheldView(Type View, Option<Error> Cause = default) : Expected($"Host cannot hold {View.Name}", 0, Cause);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ShownSubscription(Func<Action<IDisposable>, IO<Unit>> listen) : IDisposable {
    private Tuple<IDisposable>? held;

    public IO<Unit> Shown(bool shown) => shown
        ? IO.lift(() => {
            Option<Tuple<IDisposable>> accepted = None;
            return DisposalOps.OnFailure(
                DisposalOps.Acquire(retain => accepted.Match(Some: _ => listen(retain), None: static () => IO.pure(unit)), release => {
                    accepted = Some(Tuple.Create(release)).Filter(owner => Interlocked.CompareExchange(ref held, owner, comparand: null) is null);
                    _ = accepted.IfNone(release.Dispose);
                }),
                IO.lift(() => accepted.Iter(owner => Interlocked.CompareExchange(ref held, value: null, owner))));
        }).Flatten()
        : IO.lift(Dispose);

    public void Dispose() => Optional(Interlocked.Exchange(ref held, value: null)).Iter(static listener => listener.Item1.Dispose());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewOps {
    // --- [STORES]
    public static IO<BindingGroup> Bindings(IPlugInViews owner, View view, RowScope scope) =>
        view.Store.Bind(static store => store.Bindings).Match(
            Some: stored => IO.lift(scope.Document.ToFin(new UnheldView(view.Identity))).Bind(doc => stored(owner, doc)),
            None: () => IO.pure(new BindingGroup(view.Children.Bind(static child => child.Controls).Choose(static rows => rows.Bound).Map(bound => bound(scope)))));

    public static Option<ValueStore<LanguageExt.HashSet<EntryKey>>> Locks(View view, Option<RhinoDoc> document) =>
        from doc in document
        from locks in view.Store.Bind(static store => store.Locks)
        select locks(doc, view.Identity.GUID);

    public static SettingsNode Settings(IPlugInViews owner, View view) => owner.Settings.Child(EntryKey.TypeOwner(view.Identity.GUID));

    public static Fin<HistoryBinding> History(IPlugInViews owner, View view, ViewStore store, BindingGroup bound, Option<RhinoDoc> document) =>
        store.Switch(
                (Owner: owner, View: view, Document: document),
                applicationStores: static (state, _) => new HistoryScope.ApplicationScope(state.Owner.Histories),
                documentStores: static (state, held) => state.Document
                    .Map(doc => (HistoryScope)new HistoryScope.DocumentScope(doc, held.Redraw, new CallbackSite(state.Owner, state.View.Identity, nameof(History))))
                    .ToFin(new UnheldView(state.View.Identity)))
            .Map(scope => new HistoryBinding(view.Identity.GUID, RowText.Localize(view.Caption, owner).Local, bound, scope, owner.Limits, owner.Clock));

    // --- [REALIZATION]
    public static IO<Control> Realize(IPlugInViews owner, Seq<Child> children, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from stack in Containers.Frame(static () => new RhinoNestedStackLayout(Orientation.Vertical, RhinoLayout.SpacingType.Panel), owned)
            from parts in Runs(children).TraverseM(run => Part(owner, run, scope, owned)).As()
            from configured in IO.lift(() => {
                stack.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                _ = parts.Iter(part => stack.Items.Add(new StackLayoutItem(part.Body, part.Fill)));
            })
            select (Control)stack, retain);

    private static Seq<(Child Lead, Seq<Child> Run)> Runs(Seq<Child> children) =>
        children.FoldBack(Seq<(Child Lead, Seq<Child> Run)>(), static (runs, child) =>
            runs.Match(
                Empty: () => Seq((Lead: child, Run: Seq(child))),
                Tail: (next, rest) => child.IsControl && next.Lead.IsControl
                    ? (Lead: child, Run: child.Cons(next.Run)).Cons(rest)
                    : (Lead: child, Run: Seq(child)).Cons(runs)));

    private static IO<(Control Body, bool Fill)> Part(IPlugInViews owner, (Child Lead, Seq<Child> Run) run, RowScope scope, Action<IDisposable> retain) =>
        run.Lead.Switch(
                (Owner: owner, run.Run, Scope: scope, Retain: retain),
                control: static (state, _) => RowGrid.Realize(Joined(state.Run.Map(static child => child.AsControl)), state.Scope, state.Retain).Map(static grid => grid.View),
                holder: static (state, holder) => SectionHolder.Hold(state.Owner, holder, state.Scope, state.Retain).Map(static held => (Control)held),
                container: static (state, container) => Containers.Realize(state.Owner, container, state.Scope, state.Retain))
            .Map(body => (Body: body, Fill: run.Run.Exists(static child => child.Fill)));

    private static ControlRows Joined(Seq<ControlRows> runs) =>
        new(scope => runs.TraverseM(rows => rows.Read(scope)).As().Map(static rows => rows.Flatten()), scope => runs.TraverseM(rows => rows.Signals(scope)).As().Map(static signals => signals.Flatten())) {
            Fill = runs.Exists(static rows => rows.Fill),
        };

    // --- [LISTENERS]
    public static IO<Unit> WhileLoaded(Control control, Func<Action<IDisposable>, IO<Unit>> listen, CallbackSite site, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from subscription in DisposalOps.Acquire(IO.lift(() => new ShownSubscription(listen)), owned)
            from loaded in Subscriptions.Attach(
                handler => control.LoadComplete += handler, handler => control.LoadComplete -= handler, Callbacks.Handler<EventArgs>(_ => subscription.Shown(shown: true), site), owned)
            from unloaded in Subscriptions.Attach(
                handler => control.UnLoad += handler, handler => control.UnLoad -= handler, Callbacks.Handler<EventArgs>(_ => subscription.Shown(shown: false), site), owned)
            from current in subscription.Shown(control.Loaded)
            select unit, retain);
}
