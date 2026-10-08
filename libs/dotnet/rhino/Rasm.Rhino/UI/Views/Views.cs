using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Views;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInViews : IPlugInSink {
    public ViewCollection Views { get; }

    public TimeProvider Clock { get; }

    public AtomHashMap<Guid, ValueHistory> Histories { get; }

    public IO<HistoryLimits> Limits { get; }

    public SettingsNode Settings { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HelpTopic {
    public IO<Unit> Show => IO.lift(() => Refused.Unless(RhinoHelp.Show(_value), nameof(RhinoHelp.Show)));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify();
        if (trimmed is null) {
            validationError = new InvalidRhinoValue();
            return;
        }
        value = trimmed.ToLowerInvariant();
    }
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
    public static readonly EngineScope Own = new(false, static plugIn => plugIn.Id);
    public static readonly EngineScope Every = new(true, static _ => new Guid("99999999-9999-9999-9999-999999999999"));

    public bool AlwaysShow { get; }

    [UseDelegateFromConstructor]
    public partial Guid TabEngineId(PlugIn plugIn);
}

public sealed record NavigationText(bool Bold, Option<System.Drawing.Color> Color);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record View {
    public required Type Identity { get; init; }
    public required string Caption { get; init; }
    public Option<IGlyph> Icon { get; init; }
    public Option<HelpTopic> Help { get; init; }
    public Option<RowRule> Enabled { get; init; }
    public Option<RowRule> Visible { get; init; }
    public Option<ViewStore> Store { get; init; }
    public Seq<Child> Children { get; init; }

    public string HelpUrl => Conversions.Unset(Help.Map(static string (topic) => topic));

    public abstract RhinoLayout.SpacingType Spacing { get; }

    public sealed record Window : View {
        public required WindowKind Kind { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Dialog;
    }

    public sealed record Dialog : View {
        public required bool Cancelable { get; init; }
        public Option<DialogDisplayMode> DisplayMode { get; init; }
        public Option<PlugInSetting<bool, bool, InvalidRhinoValue>> Suppression { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Dialog;
    }

    public sealed record Panel : View {
        public Option<PanelType> Kind { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Panel;
    }

    public sealed record Section : View {
        public Option<Func<RowScope, IO<Seq<MenuPick>>>> Presets { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Panel;
    }

    public sealed record OptionsPage : View {
        public required CommitMode Commit { get; init; }
        public Option<NavigationText> Navigation { get; init; }
        public Seq<OptionsPage> SubPages { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Dialog;
    }

    public sealed record DocumentPage : View {
        public required CommitMode Commit { get; init; }
        public Option<NavigationText> Navigation { get; init; }
        public Seq<DocumentPage> SubPages { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Dialog;
    }

    public sealed record PropertiesPage : View {
        public Option<PropertyPageType> PageType { get; init; }
        public Option<ObjectType> SupportedTypes { get; init; }
        public Option<bool> AllObjectsMustBeSupported { get; init; }
        public Option<bool> SupportsSubObjects { get; init; }
        public Option<int> Index { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.PropertiesPage;
    }

    public sealed record Tab : View {
        public required EngineScope Scope { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Panel;
    }

    public sealed record Pane : View {
        public required EngineScope Scope { get; init; }
        public required bool InitialShow { get; init; }

        public override RhinoLayout.SpacingType Spacing => RhinoLayout.SpacingType.Panel;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewStore {
    public Option<Func<RhinoDoc, Guid, ValueStore<LanguageExt.HashSet<EntryKey>>>> Locks { get; init; }
    public Option<Func<IPlugInViews, RhinoDoc, Fin<BindingGroup>>> Bindings { get; init; }

    public sealed record ApplicationStores : ViewStore;

    public sealed record DocumentStores(RedrawPolicy Redraw) : ViewStore;
}

[Union<ControlRow, HolderRow, ContainerRow>(T1Name = "Control", T2Name = "Holder", T3Name = "Container")]
public sealed partial class Child {
    public bool Fill =>
        Switch(control: static row => row.Fill, holder: static _ => true, container: static row => row.Fill);

    public Seq<ControlRow> Controls =>
        Switch(
            control: static row => Seq(row),
            holder: static _ => Seq<ControlRow>(),
            container: static row => row.Children.Bind(static child => child.Controls));

    public Seq<HolderRow> Holders =>
        Switch(
            control: static _ => Seq<HolderRow>(),
            holder: static row => Seq(row),
            container: static row => row.Children.Bind(static child => child.Holders));

    public Seq<View.Section> Sections => Holders.Bind(static holder => holder.Sections);
}

public sealed record ViewBody(Control Content, IO<Unit> Release);

public sealed record ViewCollection {
    private readonly Seq<View> rows;
    private readonly HashMap<Guid, View> views;

    private ViewCollection(Seq<View> rows, HashMap<Guid, View> views) => (this.rows, this.views) = (rows, views);

    public static ViewCollection Empty { get; } = new(Seq<View>(), HashMap<Guid, View>());

    public static Validation<Error, ViewCollection> Of(Seq<View> rows) => Indexed(rows, rows.Bind(static view => Tree(view)).Strict());

    public Option<TCase> Find<TCase>(Guid id) where TCase : View =>
        views.Find(id).Bind(static view => Optional(view as TCase));

    public Seq<TCase> Listed<TCase>() where TCase : View =>
        rows.Choose<View, TCase>(static view => Optional(view as TCase));

    public static Seq<View> Tree(View view) =>
        view.Cons(view.Children.Bind(static child => child.Sections).Bind(static section => Tree(section)));

    private static Validation<Error, ViewCollection> Indexed(Seq<View> rows, Seq<View> tree) =>
        (Callbacks.Unique(tree, static view => view.Identity.GUID, nameof(ViewCollection)),
         tree.Bind(static view => Hideable(view)).Traverse(static section => Validation.Fail<Error, Unit>(new UnsupportedView(section.Identity, None))).As())
            .Apply((listed, _) => new ViewCollection(rows, toHashMap(listed.Map(static view => (view.Identity.GUID, view)))))
            .As();

    private static Seq<View.Section> Hideable(View view) =>
        view.Children.Bind(static child => child.Holders).Bind(static holder => holder.Switch(
            stacked: static _ => Seq<View.Section>(),
            filled: static filled => filled.Above.Add(filled.Fill).Filter(static section => section.Visible.IsSome)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ShownSubscription(IO<IDisposable> listen, CallbackSite site) : IDisposable {
    private readonly Atom<Option<IDisposable>> held = Atom(Option<IDisposable>.None);

    public Unit Shown(bool shown) =>
        held.Value.Match(
            Some: listener => shown ? unit : Unsubscribed(listener),
            None: () => shown ? Subscribed(Callbacks.Answer(listen.Map(static listener => Some(listener)), static () => Option<IDisposable>.None, site)) : unit);

    public void Dispose() => _ = Shown(false);

    private Unit Subscribed(Option<IDisposable> listener) => ignore(held.Swap(_ => listener));

    private Unit Unsubscribed(IDisposable listener) {
        _ = held.Swap(static _ => Option<IDisposable>.None);
        return Callbacks.Answer(IO.lift(listener.Dispose), static () => unit, site);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewOps {
    // --- [CONSTRUCTION]
    public static IO<TView> Construct<TView>(View row) where TView : class =>
        IO.lift(() => (TView)Activator.CreateInstance(row.Identity)!);

    // --- [STORES]
    public static Validation<Error, BindingGroup> Bindings(IPlugInViews owner, View view, RowScope scope) =>
        view.Store.Bind(static store => store.Bindings).Match(
            Some: stored => scope.Document.ToFin(new UnsupportedView(view.Identity, None)).Bind(doc => stored(owner, doc)).ToValidation(),
            None: () => BindingGroup.Of(Sources(view).Map(source => source.Values(scope))));

    public static Option<ValueStore<LanguageExt.HashSet<EntryKey>>> Locks(View view, Option<RhinoDoc> document) =>
        from doc in document
        from locks in view.Store.Bind(static store => store.Locks)
        select locks(doc, view.Identity.GUID);

    public static SettingsNode Settings(IPlugInViews owner, View view) => owner.Settings.Child(EntryKey.TypeOwner(view.Identity.GUID));

    public static IO<ValueSet> Defaults(View view) =>
        Sources(view).TraverseM(static source => source.Defaults).As()
            .Map(static sets => new ValueSet(toHashMap(sets.Bind(static set => toSeq(set.Entries.AsIterable())))));

    public static Fin<HistoryBinding> History(IPlugInViews owner, View view, ViewStore store, BindingGroup group, Option<RhinoDoc> document) =>
        store.Switch<(IPlugInViews Owner, View View, Option<RhinoDoc> Document), Fin<HistoryScope>>(
                (Owner: owner, View: view, Document: document),
                applicationStores: static (state, _) => new HistoryScope.ApplicationScope(state.Owner.Histories),
                documentStores: static (state, held) => state.Document
                    .Map(doc => (HistoryScope)new HistoryScope.DocumentScope(doc, held.Redraw, new CallbackSite(state.Owner, state.View.Identity, nameof(History))))
                    .ToFin(new UnsupportedView(state.View.Identity, None)))
            .Map(scope => new HistoryBinding(view.Identity.GUID, RowText.Localize(view.Caption, table: Some<object>(owner)).Local, group, scope, owner.Limits, owner.Clock));

    private static Seq<RowSource> Sources(View view) =>
        view.Children.Bind(static child => child.Controls).Choose<ControlRow, RowSource>(static row => row.Bound).Distinct();

    // --- [REALIZATION]
    public static IO<ViewBody> Realize(IPlugInViews owner, Seq<Child> children, RowScope scope) =>
        from parts in DisposalOps.AcquireAll(
            Runs(children).Map(run => Part(owner, children, run, scope)),
            static held => Released(held.Map(static part => part.Body)))
        from body in DisposalOps.OnFailure(IO.lift(() => Stacked(owner, parts)), Released(parts.Map(static part => part.Body)))
        select body;

    internal static IO<Unit> Released(Seq<ViewBody> bodies) =>
        Callbacks.Each(bodies.Rev().Map(static body => body.Release)).Map(static _ => unit);

    private static Seq<(Child Lead, Seq<Child> Run)> Runs(Seq<Child> children) =>
        children.FoldBack(Seq<(Child Lead, Seq<Child> Run)>(), static (runs, child) =>
            runs.Match(
                Empty: () => Seq((Lead: child, Run: Seq(child))),
                Tail: (next, rest) => child.IsControl && next.Lead.IsControl
                    ? (Lead: child, Run: child.Cons(next.Run)).Cons(rest)
                    : (Lead: child, Run: Seq(child)).Cons(runs)));

    private static IO<(ViewBody Body, bool Fill)> Part(IPlugInViews owner, Seq<Child> children, (Child Lead, Seq<Child> Run) run, RowScope scope) =>
        run.Lead.Switch(
                (Owner: owner, Children: children, Run: run.Run, Scope: scope),
                control: static (state, _) => RowGrid.Realize(state.Children.Bind(static child => child.Controls), state.Run.Map(static child => child.AsControl), state.Scope)
                    .Map(static grid => new ViewBody(grid, IO.lift(grid.Dispose))),
                holder: static (state, holder) => SectionStack.Hold(state.Owner, holder, state.Scope),
                container: static (state, container) => Containers.Realize(state.Owner, container, state.Scope))
            .Map(body => (Body: body, Fill: run.Run.Exists(static child => child.Fill)));

    private static ViewBody Stacked(IPlugInViews owner, Seq<(ViewBody Body, bool Fill)> parts) {
        RhinoNestedStackLayout stack = new(Orientation.Vertical, RhinoLayout.SpacingType.Panel) { HorizontalContentAlignment = HorizontalAlignment.Stretch };
        _ = parts.Iter(part => stack.Items.Add(new StackLayoutItem(part.Body.Content, part.Fill)));
        Disposal<Seq<ViewBody>> held = new(
            parts.Map(static part => part.Body),
            bodies => _ = Callbacks.Answer(Released(bodies), static () => unit, new CallbackSite(owner, typeof(ViewOps), nameof(Realize))));
        return new ViewBody(stack, IO.lift(held.Dispose));
    }

    // --- [LISTENERS]
    public static IO<IDisposable> WhileLoaded(Control control, IO<IDisposable> listen, CallbackSite site) =>
        from subscription in IO.lift(() => new ShownSubscription(listen, site))
        from attached in DisposalOps.AcquireAll(
            Seq(
                IO.pure<IDisposable>(subscription),
                Subscriptions.Attach<EventHandler<EventArgs>>(
                    handler => control.Load += handler, handler => control.Load -= handler, Callbacks.Handler<EventArgs>(_ => IO.lift(() => subscription.Shown(true)), site)),
                Subscriptions.Attach<EventHandler<EventArgs>>(
                    handler => control.UnLoad += handler, handler => control.UnLoad -= handler, Callbacks.Handler<EventArgs>(_ => IO.lift(() => subscription.Shown(false)), site))),
            DisposalOps.Release)
        select DisposalOps.Composite(attached, site);
}
