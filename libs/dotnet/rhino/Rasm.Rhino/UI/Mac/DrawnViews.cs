using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using ObjCRuntime;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.RhinoCommon.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DrawnViews {
    // --- [LAYERS]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint HitTest(nint view, nint selector, CGPoint point);

    private static readonly HitTest PassThrough = static (_, _, _) => 0;
    private static readonly Lazy<Fin<nint>> HostClass = new(static () => NativeControls.Subclass(Class.GetHandle(typeof(NSView)),
        Seq((Selector.GetHandle("hitTest:"), (Delegate)PassThrough)), []));

    public static IO<Unit> Layers(Control control, CALayer layer, IPlugInLifetime owner) =>
        NativeControls.Lifetime(control,
            from parent in IO.lift(() => Conversions.Found(control.GetContainerView()))
            from nativeClass in IO.lift(static () => HostClass.Value)
            from view in use(IO.lift(() => new NSView(parent.Bounds)), static held =>
                Callbacks.Each(Seq(IO.lift(held.RemoveFromSuperview), IO.lift(() => { held.Layer = null; }), IO.lift(held.Dispose))).Map(unit))
            from placed in IO.lift(() => {
                _ = SafeNativeMethods.object_setClass(view.Handle, nativeClass);
                (view.Layer, view.WantsLayer, view.AutoresizingMask) = (layer, true, NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable);
                parent.AddSubview(view);
            })
            let fit = Fit(parent, view, layer)
            from fitted in fit
            from bounds in Observed(handler => NSView.Notifications.ObserveBoundsChanged(parent, handler)).Inline(_ => fit, owner)
            from backing in Observed(NSWindow.Notifications.ObserveDidChangeBackingProperties).Inline(args => when(Equals(args.Notification.Object, view.Window), fit).As(), owner)
            from size in Subscriptions.Host<EventArgs>(h => control.SizeChanged += h, h => control.SizeChanged -= h).Inline(_ => fit, owner)
            from load in Subscriptions.Host<EventArgs>(h => control.LoadComplete += h, h => control.LoadComplete -= h).Inline(_ => fit, owner)
            select unit,
            owner);

    private static IO<Unit> Fit(NSView parent, NSView view, CALayer layer) =>
        IO.lift(CATransaction.Begin) >> IO.lift(() => {
            CATransaction.DisableActions = true;
            view.Frame = parent.Bounds;
            layer.Frame = view.Bounds;
            layer.GeometryFlipped = true;
            _ = Scaled(layer, view.ConvertSizeToBacking(new CGSize(1, 1)).Width);
        }).Finally(IO.lift(CATransaction.Commit));

    // --- [NOTIFICATIONS]
    internal static HostEvent<NSNotificationEventArgs> Observed(Func<EventHandler<NSNotificationEventArgs>, NSObject> observe, [CallerArgumentExpression(nameof(observe))] string label = "") =>
        new(label, (deliver, owner) => use(() => observe((_, args) => owner.Start(deliver(args), label))).Map(unit));

    private static Unit Scaled(CALayer layer, NFloat scale) {
        layer.ContentsScale = scale;
        _ = Optional(layer.Sublayers).Iter(children => toSeq(children).Iter(child => Scaled(child, scale)));
        _ = Optional(layer.Mask).Iter(mask => Scaled(mask, scale));
        return unit;
    }

    // --- [SCENE]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Signal(nint view, nint selector);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Argued(nint view, nint selector, nint argument);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint Tipping(nint owner, nint selector, nint view, nint tag, CGPoint point, nint data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint Answering(nint view, nint selector);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool Acting(nint element, nint selector);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool Allowing(nint element, nint selector, nint query);

    private sealed class Native(SceneView scene, NSView view, nint parent) {
        public SceneView Scene { get; } = scene;
        public NSView View { get; } = view;
        public nint Parent { get; } = parent;
        public HashMap<Seq<Key>, (Element Element, NSAccessibilityElement Shown)> Shown { get; set; }
        public Option<Seq<Element>> Tree { get; set; }
        public Ref<Option<Lease<Unit>>> Keyed { get; } = Ref(Option<Lease<Unit>>.None);
    }

    private sealed record Owner(Native Native, Seq<Key> Id);

    private static readonly ConditionalWeakTable<NSView, Native> Views = [];
    private static readonly ConditionalWeakTable<SceneView, Native> Scenes = [];
    private static readonly ConditionalWeakTable<NSAccessibilityElement, Owner> Owners = [];
    private static readonly AtomHashMap<nint, (Fin<nint> Class, Seq<(nint Selector, Delegate Implementation)> Methods)> Classes =
        AtomHashMap<nint, (Fin<nint> Class, Seq<(nint Selector, Delegate Implementation)> Methods)>();
    private static readonly nint ElementParent = Class.GetHandle(typeof(NSAccessibilityElement));
    private static readonly Func<NSObject, NSString, NSDictionary?, Unit> Posted = fun<NSObject, NSString, NSDictionary?>(NSAccessibility.PostNotification);
    private static readonly nint TipOwner = ObjCExtensions.GetProtocolHandle(typeof(INSViewToolTipOwner).GetCustomAttribute<ProtocolAttribute>()?.Name
                                                                           ?? throw new InvalidOperationException($"{nameof(INSViewToolTipOwner)} binds no protocol"));

    private static readonly HashMap<nint, (Act Act, KeyModifiers Modifiers)> Commands = toHashMap(Seq<(string Member, Act Act, bool Extends)>(
            (nameof(INSStandardKeyBindingResponding.MoveLeft), new Act.Navigate(NavigationDirection.Left), false),
            (nameof(INSStandardKeyBindingResponding.MoveBackward), new Act.Navigate(NavigationDirection.Left), false),
            (nameof(INSStandardKeyBindingResponding.MoveRight), new Act.Navigate(NavigationDirection.Right), false),
            (nameof(INSStandardKeyBindingResponding.MoveForward), new Act.Navigate(NavigationDirection.Right), false),
            (nameof(INSStandardKeyBindingResponding.MoveUp), new Act.Navigate(NavigationDirection.Up), false),
            (nameof(INSStandardKeyBindingResponding.MoveDown), new Act.Navigate(NavigationDirection.Down), false),
            (nameof(INSStandardKeyBindingResponding.MoveToBeginningOfLine), new Act.Navigate(NavigationDirection.Home), false),
            (nameof(INSStandardKeyBindingResponding.MoveToLeftEndOfLine), new Act.Navigate(NavigationDirection.Home), false),
            (nameof(INSStandardKeyBindingResponding.MoveToEndOfLine), new Act.Navigate(NavigationDirection.End), false),
            (nameof(INSStandardKeyBindingResponding.MoveToRightEndOfLine), new Act.Navigate(NavigationDirection.End), false),
            (nameof(INSStandardKeyBindingResponding.MoveToBeginningOfDocument), new Act.Navigate(NavigationDirection.Top), false),
            (nameof(INSStandardKeyBindingResponding.MoveToEndOfDocument), new Act.Navigate(NavigationDirection.Bottom), false),
            (nameof(INSStandardKeyBindingResponding.PageUp), new Act.Navigate(NavigationDirection.PageUp), false),
            (nameof(INSStandardKeyBindingResponding.PageDown), new Act.Navigate(NavigationDirection.PageDown), false),
            (nameof(INSStandardKeyBindingResponding.ScrollPageUp), new Act.Navigate(NavigationDirection.PageUp), false),
            (nameof(INSStandardKeyBindingResponding.ScrollPageDown), new Act.Navigate(NavigationDirection.PageDown), false),
            (nameof(INSStandardKeyBindingResponding.ScrollLineUp), new Act.Navigate(NavigationDirection.Up), false),
            (nameof(INSStandardKeyBindingResponding.ScrollLineDown), new Act.Navigate(NavigationDirection.Down), false),
            (nameof(INSStandardKeyBindingResponding.ScrollToBeginningOfDocument), new Act.Navigate(NavigationDirection.Top), false),
            (nameof(INSStandardKeyBindingResponding.ScrollToEndOfDocument), new Act.Navigate(NavigationDirection.Bottom), false),
            (nameof(INSStandardKeyBindingResponding.MoveLeftAndModifySelection), new Act.Navigate(NavigationDirection.Left), true),
            (nameof(INSStandardKeyBindingResponding.MoveBackwardAndModifySelection), new Act.Navigate(NavigationDirection.Left), true),
            (nameof(INSStandardKeyBindingResponding.MoveRightAndModifySelection), new Act.Navigate(NavigationDirection.Right), true),
            (nameof(INSStandardKeyBindingResponding.MoveForwardAndModifySelection), new Act.Navigate(NavigationDirection.Right), true),
            (nameof(INSStandardKeyBindingResponding.MoveUpAndModifySelection), new Act.Navigate(NavigationDirection.Up), true),
            (nameof(INSStandardKeyBindingResponding.MoveDownAndModifySelection), new Act.Navigate(NavigationDirection.Down), true),
            (nameof(INSStandardKeyBindingResponding.MoveToBeginningOfLineAndModifySelection), new Act.Navigate(NavigationDirection.Home), true),
            (nameof(INSStandardKeyBindingResponding.MoveToLeftEndOfLineAndModifySelection), new Act.Navigate(NavigationDirection.Home), true),
            (nameof(INSStandardKeyBindingResponding.MoveToEndOfLineAndModifySelection), new Act.Navigate(NavigationDirection.End), true),
            (nameof(INSStandardKeyBindingResponding.MoveToRightEndOfLineAndModifySelection), new Act.Navigate(NavigationDirection.End), true),
            (nameof(INSStandardKeyBindingResponding.MoveToBeginningOfDocumentAndModifySelection), new Act.Navigate(NavigationDirection.Top), true),
            (nameof(INSStandardKeyBindingResponding.MoveToEndOfDocumentAndModifySelection), new Act.Navigate(NavigationDirection.Bottom), true),
            (nameof(INSStandardKeyBindingResponding.PageUpAndModifySelection), new Act.Navigate(NavigationDirection.PageUp), true),
            (nameof(INSStandardKeyBindingResponding.PageDownAndModifySelection), new Act.Navigate(NavigationDirection.PageDown), true),
            (nameof(INSStandardKeyBindingResponding.DeleteBackward), new Act.Delete(), false),
            (nameof(INSStandardKeyBindingResponding.DeleteForward), new Act.Delete(), false),
            (nameof(INSStandardKeyBindingResponding.InsertTab), new Act.Next(), false),
            (nameof(INSStandardKeyBindingResponding.InsertBacktab), new Act.Previous(), false),
            (nameof(INSStandardKeyBindingResponding.InsertNewline), new Act.Confirm(), false),
            (nameof(INSStandardKeyBindingResponding.CancelOperation), new Act.Cancel(), false),
            (nameof(INSStandardKeyBindingResponding.SelectAll), new Act.SelectAll(), false),
            (nameof(INSStandardKeyBindingResponding.ShowContextMenuForSelection), new Act.ShowMenu(), false))
        .Map(static row => (Exported(typeof(INSStandardKeyBindingResponding), row.Member), (row.Act, KeyModifiers.Plain with { Shift = row.Extends }))));

    private static readonly HashMap<nint, Act> Performs = toHashMap(Seq<(string Member, Act Act)>(
            (nameof(NSAccessibilityElement.AccessibilityPerformPress), new Act.Press()),
            (nameof(NSAccessibilityElement.AccessibilityPerformConfirm), new Act.Confirm()),
            (nameof(NSAccessibilityElement.AccessibilityPerformCancel), new Act.Cancel()),
            (nameof(NSAccessibilityElement.AccessibilityPerformIncrement), new Act.Increment()),
            (nameof(NSAccessibilityElement.AccessibilityPerformDecrement), new Act.Decrement()),
            (nameof(NSAccessibilityElement.AccessibilityPerformDelete), new Act.Delete()),
            (nameof(NSAccessibilityElement.AccessibilityPerformShowMenu), new Act.ShowMenu()))
        .Map(static row => (Exported(typeof(NSAccessibilityElement), row.Member), row.Act)));

    private static Seq<(nint Selector, Delegate Implementation)> ElementMethods(IPlugInLifetime owner) =>
        Performs.Keys.ToSeq().Map(selector => (selector, (Delegate)new Acting((element, called) => owner.Answer(IO.lift(() => Perform(element, called)), failed: false))))
            .Add((Exported(typeof(NSAccessibilityElement), nameof(NSAccessibilityElement.IsAccessibilitySelectorAllowed)),
                new Allowing((element, called, query) => owner.Answer(IO.lift(() => Allow(element, called, query)), failed: false))));

    private static Seq<(nint Selector, Delegate Implementation)> SceneMethods(IPlugInLifetime owner) => [
        (Exported(typeof(INSStandardKeyBindingResponding), nameof(INSStandardKeyBindingResponding.DoCommandBySelector)),
            new Argued((view, selector, command) => owner.Run(IO.lift(() => Interpreted(view, selector, command))))),
        (Exported(typeof(NSView), nameof(NSView.ViewDidHide)), new Signal((view, selector) => owner.Run(IO.lift(() => Hide(view, selector))))),
        (Exported(typeof(NSView), nameof(NSView.ViewDidUnhide)), new Signal((view, selector) => owner.Run(IO.lift(() => Hide(view, selector))))),
        (Exported(typeof(NSView), nameof(NSView.ViewDidMoveToWindow)), new Signal((view, selector) => owner.Run(IO.lift(() => Moved(view, selector))))),
        (Exported(typeof(NSResponder), nameof(NSResponder.ScrollWheel)), new Argued((view, selector, current) => owner.Run(IO.lift(() => Scroll(view, selector, current))))),
        (Exported(typeof(NSResponder), nameof(NSResponder.MagnifyWithEvent)), new Argued((view, selector, current) => owner.Run(IO.lift(() => Magnify(view, selector, current))))),
        (Exported(typeof(NSResponder), nameof(NSResponder.RotateWithEvent)), new Argued((view, selector, current) => owner.Run(IO.lift(() => Rotate(view, selector, current))))),
        (Exported(typeof(NSResponder), nameof(NSResponder.PressureChange)), new Argued((view, selector, current) => owner.Run(IO.lift(() => Pressure(view, selector, current))))),
        (Exported(typeof(INSViewToolTipOwner), nameof(INSViewToolTipOwner.GetStringForToolTip)),
            new Tipping((tipped, _, _, _, _, _) => owner.Answer(IO.lift(() => Tip(tipped)), failed: 0))),
        (Exported(typeof(NSView), nameof(NSView.GetAccessibilityFocusedUIElement)), new Answering((view, selector) => owner.Answer(IO.lift(() => Focused(view, selector)), failed: 0))),
    ];

    private static Fin<nint> Classed(nint parent, Func<Seq<(nint Selector, Delegate Implementation)>> methods, Seq<nint> protocols) =>
        Classes.FindOrAdd(parent, () => methods() switch {
            var made => (NativeControls.Subclass(parent, made, protocols), made),
        }).Class;

    internal static void Scene(DrawableHandler handler) {
        SceneView scene = (SceneView)handler.Widget;
        NSView view = handler.Control;
        nint parent = SafeNativeMethods.object_getClass(view.Handle);
        _ = scene.Owner.Run(
            from nativeClass in IO.lift(() => Classed(parent, () => SceneMethods(scene.Owner), [TipOwner]))
            from attached in IO.lift(() => {
                Native native = new(scene, view, parent);
                Views.Add(view, native);
                Scenes.Add(scene, native);
                _ = SafeNativeMethods.object_setClass(view.Handle, nativeClass);
                return Revealed(native);
            })
            select unit);
    }

    private static nint Exported(Type owner, string member) =>
        toSeq(owner.GetMember(member)).Choose(static found => Optional(found.GetCustomAttribute<ExportAttribute>())
                | Optional(found as PropertyInfo).Bind(static property => Optional(property.GetMethod?.GetCustomAttribute<ExportAttribute>())))
            .Choose(static export => Optional(export.Selector)).Head
            .Map(Selector.GetHandle)
            .IfNone(() => throw new InvalidOperationException($"{owner.Name}.{member} binds no selector"));

    private static Native Held(nint view) =>
        Optional(Runtime.GetNSObject<NSView>(view)).Bind(static shown => Views.TryGetValue(shown, out Native? native) ? Some(native) : None)
            .IfNone(static () => throw new InvalidOperationException($"{nameof(SceneView)} class on a view no scene attached"));

    private static TDelegate Super<TDelegate>(nint parent, nint selector) where TDelegate : Delegate =>
        Marshal.GetDelegateForFunctionPointer<TDelegate>(SafeNativeMethods.method_getImplementation(ObjCExtensions.GetInstanceMethod(parent, selector)));

    // --- [KEYS]
    private static void Interpreted(nint view, nint selector, nint command) {
        Native native = Held(view);
        bool answered = Commands.Find(command).Exists(row => HostInput.Now.Match(
            Succ: now => native.Scene.Receive(new Input.Perform(row.Act, new Target.Focused()) { Modifiers = row.Modifiers }, now),
            Fail: static _ => false));
        if (answered)
            _ = Optional(native.Scene.Handler as IMacViewHandler).Iter(static responder => responder.TextInputCancelled = true);
        else
            Super<Argued>(native.Parent, selector)(view, selector, command);
    }

    // --- [VISIBILITY]
    private static void Hide(nint view, nint selector) {
        Native native = Held(view);
        Super<Signal>(native.Parent, selector)(view, selector);
        _ = Revealed(native);
    }

    private static void Moved(nint view, nint selector) {
        Native native = Held(view);
        Super<Signal>(native.Parent, selector)(view, selector);
        _ = native.Scene.Owner.Run(Optional(native.View.Window).Match(
                Some: window => Products.Swapped(native.Keyed, Subscriptions.Merged(
                        Observed(handler => NSWindow.Notifications.ObserveDidBecomeKey(window, handler)),
                        Observed(handler => NSWindow.Notifications.ObserveDidResignKey(window, handler)))
                    .Inline(_ => IO.lift(() => Revealed(native)), native.Scene.Owner)),
                None: () => Products.Cleared(native.Keyed))
            .Under(native.Scene.Owned));
        _ = Revealed(native);
    }

    private static Unit Revealed(Native native) {
        _ = native.Scene.Owner.Run(native.Scene.Natively(native.View.Window is not null && !native.View.IsHiddenOrHasHiddenAncestor, native.View.Window is { IsKeyWindow: true }));
        return unit;
    }

    // --- [POINTER]
    private static void Scroll(nint view, nint selector, nint current) =>
        Evented(view, selector, current, static (e, at) =>
            new Vector2((float)e.ScrollingDeltaX, (float)e.ScrollingDeltaY) switch {
                var delta when e.HasPreciseScrollingDeltas => Phased(e.Phase is not NSEventPhase.None ? e.Phase : e.MomentumPhase, delta).Map(phase => (Input)new Input.Scroll(at, phase)),
                var delta => Some((Input)new Input.Wheel(at, delta)),
            });

    private static void Magnify(nint view, nint selector, nint current) =>
        Evented(view, selector, current, static (e, at) => Phased(e.Phase, (float)e.Magnification).Map(phase => (Input)new Input.Magnify(at, phase)));

    private static void Rotate(nint view, nint selector, nint current) =>
        Evented(view, selector, current, static (e, at) => Phased(e.Phase, float.DegreesToRadians(-e.Rotation)).Map(phase => (Input)new Input.Rotate(at, phase)));

    private static void Pressure(nint view, nint selector, nint current) =>
        Evented(view, selector, current, static (e, at) => NSEvent.CurrentPressedMouseButtons is 0
            ? None
            : Some((Input)new Input.Pointer(Edit.Preview(new PointerSample(at, e.Pressure)), 1)));

    private static void Evented(nint view, nint selector, nint current, Func<NSEvent, SKPoint, Option<Input>> read) {
        Native native = Held(view);
        bool answered = Optional(Runtime.GetNSObject<NSEvent>(current)).Exists(e => e.LocationInWindow.ToEto(native.View) switch {
            var at => read(e, new SKPoint(at.X, at.Y)).Map(input => input with { Modifiers = HostInput.Modifiers(e.ModifierFlags.ToEto()) })
                .Exists(input => native.Scene.Receive(input, Duration.FromSeconds(e.Timestamp))),
        });
        if (!answered) Super<Argued>(native.Parent, selector)(view, selector, current);
    }

    private static Option<Edit<T>> Phased<T>(NSEventPhase phase, T value) where T : notnull =>
        phase switch {
            NSEventPhase.Began => Edit.Begin(value),
            NSEventPhase.Changed or NSEventPhase.Stationary => Edit.Preview(value),
            NSEventPhase.Ended => Edit.Commit(value),
            NSEventPhase.Cancelled => Edit.Cancel<T>(),
            _ => None,
        };

    // --- [TOOLTIPS]
    private static nint Tip(nint owner) =>
        Held(owner).Scene.Current.Bind(static mount => mount.Help).Match(Some: static text => (nint)Runtime.RetainAndAutoreleaseNSObject(new NSString(text)), None: static () => 0);

    private static Unit Tooltips(Native native, Seq<Element> tree) {
        native.View.RemoveAllToolTips();
        _ = Access.Flattened(tree).Iter(element =>
            _ = native.View.AddToolTip(Placed(element.Frame, SKRect.Create((float)native.View.Bounds.Width, (float)native.View.Bounds.Height), native.View.IsFlipped), native.View, 0));
        return unit;
    }

    // --- [ACCESSIBILITY]
    private static nint Focused(nint view, nint selector) {
        Native native = Held(view);
        return native.Scene.Current.Bind(static mount => mount.Focused).Bind(native.Shown.Find)
            .Match(Some: static held => (nint)held.Shown.Handle, None: () => Super<Answering>(native.Parent, selector)(view, selector));
    }

    private static bool Perform(nint element, nint selector) =>
        Owned(element).Exists(owner => Performs.Find(selector).Exists(act => owner.Native.Scene.Timed(new Input.Perform(act, new Target.Element(owner.Id)))));

    private static bool Allow(nint element, nint selector, nint query) =>
        Performs.Find(query).Match(
            Some: act => Owned(element).Exists(owner => owner.Native.Shown.Find(owner.Id).Exists(held => held.Element.Actions.Exists(offered => offered == act))),
            None: () => Super<Allowing>(ElementParent, selector)(element, selector, query));

    private static Option<Owner> Owned(nint element) =>
        Optional(Runtime.GetNSObject<NSAccessibilityElement>(element)).Bind(static shown => Owners.TryGetValue(shown, out Owner? owner) ? Some(owner) : None);

    internal static IO<Func<Mount, Output, IO<Unit>>> Projection(SceneView scene) =>
        from native in IO.lift(() => Conversions.Found(Scenes.TryGetValue(scene, out Native? held) ? held : null))
        from elementClass in IO.lift(() => Classed(ElementParent, () => ElementMethods(scene.Owner), []))
        from held in use(IO.pure(native), Withdrawn)
        select fun((Mount mount, Output output) => IO.lift(() => Reconciled(held, elementClass, mount, output)));

    private static Unit Reconciled(Native native, nint elementClass, Mount mount, Output output) {
        bool first = native.Tree.IsNone;
        if (native.Tree != Some(mount.Tree)) _ = Projected(native, elementClass, mount.Tree);
        _ = output.Notices.Iter(notice => Noticed(native, notice));
        if (first || output.Notices.Exists(static notice => notice.Map(announced: false, changed: false, focusChanged: false, layoutChanged: true, detent: false)))
            _ = Tooltips(native, mount.Tree);
        return unit;
    }

    private static IO<Unit> Withdrawn(Native native) =>
        IO.lift(() => {
            native.View.RemoveAllToolTips();
            native.View.AccessibilityChildren = [.. native.View.Subviews];
            _ = native.Shown.Values.Iter(static held => {
                _ = Owners.Remove(held.Shown);
                held.Shown.Dispose();
            });
            (native.Shown, native.Tree) = ([], None);
        });

    private static Unit Projected(Native native, nint elementClass, Seq<Element> tree) {
        HashMap<Seq<Key>, (Element Element, NSAccessibilityElement Shown)> prior = native.Shown;
        native.Shown = [];
        NSAccessibilityElement[] roots = [.. tree.Map(root => Elemented(native, elementClass, prior, root, native.View, SKRect.Create((float)native.View.Bounds.Width, (float)native.View.Bounds.Height), native.View.IsFlipped))];
        _ = prior.Filter((id, _) => !native.Shown.ContainsKey(id)).Values.Iter(static held => {
            _ = Owners.Remove(held.Shown);
            held.Shown.Dispose();
        });
        native.View.AccessibilityChildren = [.. roots, .. native.View.Subviews];
        native.Tree = tree;
        return unit;
    }

    private static NSAccessibilityElement Elemented(Native native, nint elementClass, HashMap<Seq<Key>, (Element Element, NSAccessibilityElement Shown)> prior, Element element, NSObject parent,
                                                    SKRect within, bool flipped) {
        Option<(Element Element, NSAccessibilityElement Shown)> was = prior.Find(element.Id);
        NSAccessibilityElement shown = was.Map(static held => held.Shown).IfNone(() => Made(native, elementClass, element.Id));
        if (!was.Exists(held => held.Element with { Children = [] } == element with { Children = [] })) _ = Written(shown, element, parent);
        shown.AccessibilityFrameInParentSpace = Placed(element.Frame, within, flipped);
        native.Shown = native.Shown.AddOrUpdate(element.Id, (element, shown));
        shown.AccessibilityChildren = [.. element.Children.Map(child => Elemented(native, elementClass, prior, child, shown, element.Frame, flipped: false))];
        return shown;
    }

    private static NSAccessibilityElement Made(Native native, nint elementClass, Seq<Key> id) {
        NSAccessibilityElement made = new();
        _ = SafeNativeMethods.object_setClass(made.Handle, elementClass);
        Owners.Add(made, new Owner(native, id));
        return made;
    }

    private static Unit Written(NSAccessibilityElement shown, Element element, NSObject parent) {
        (NSString role, Option<NSString> subrole) = Roled(element.Role);
        shown.AccessibilityRole = role;
        shown.AccessibilitySubrole = subrole.ValueUnsafe();
        shown.AccessibilityParent = parent;
        shown.AccessibilityIdentifier = string.Join('/', element.Id.Map(static key => key.Identifier));
        shown.AccessibilityLabel = element.Accessible.Label.ValueUnsafe();
        shown.AccessibilityHelp = element.Accessible.Help.ValueUnsafe();
        shown.AccessibilityEnabled = !element.Conditions.Contains(Condition.Disabled);
        shown.AccessibilitySelected = element.Conditions.Contains(Condition.Selected);
        _ = element.Accessible.Expanded.Iter(open => shown.AccessibilityExpanded = open);
        _ = element.Accessible.Orientation.Iter(axis => shown.AccessibilityOrientation = axis.Map(horizontal: NSAccessibilityOrientation.Horizontal, vertical: NSAccessibilityOrientation.Vertical));
        _ = element.Accessible.Value.Iter(value => value.Switch(
            shown,
            text: static (target, text) => target.AccessibilityValue = new NSString(text.Value),
            @checked: static (target, held) => target.AccessibilityValue = NSNumber.FromInt32(held.Mark.Map(off: 0, on: 1, mixed: 2)),
            ranged: static (target, ranged) => {
                target.AccessibilityValue = NSNumber.FromDouble(ranged.Value);
                target.AccessibilityMinValue = NSNumber.FromDouble(ranged.Min);
                target.AccessibilityMaxValue = NSNumber.FromDouble(ranged.Max);
                target.AccessibilityValueDescription = ranged.Description.ValueUnsafe();
                return target.AccessibilityValue;
            },
            point: static (target, point) => target.AccessibilityValue = point.Description.Map(static text => (NSObject)new NSString(text)).ValueUnsafe()));
        return unit;
    }

    private static CGRect Placed(SKRect frame, SKRect within, bool flipped) =>
        new(frame.Left - within.Left, flipped ? frame.Top - within.Top : within.Bottom - frame.Bottom, frame.Width, frame.Height);

    private static (NSString Role, Option<NSString> Subrole) Roled(AccessRole role) =>
        role.Map<(NSString Role, Option<NSString> Subrole)>(
            text: (NSAccessibilityRoles.StaticTextRole, None),
            image: (NSAccessibilityRoles.ImageRole, None),
            heading: (OperatingSystem.IsMacOSVersionAtLeast(26) ? NSAccessibilityRoles.HeadingRole : NSAccessibilityRoles.GroupRole, None),
            button: (NSAccessibilityRoles.ButtonRole, None),
            menuButton: (NSAccessibilityRoles.MenuButtonRole, None),
            popUpButton: (NSAccessibilityRoles.PopUpButtonRole, None),
            disclosure: (NSAccessibilityRoles.DisclosureTriangleRole, None),
            checkBox: (NSAccessibilityRoles.CheckBoxRole, None),
            @switch: (NSAccessibilityRoles.CheckBoxRole, NSAccessibilitySubroles.SwitchSubrole),
            radio: (NSAccessibilityRoles.RadioButtonRole, None),
            tab: (NSAccessibilityRoles.RadioButtonRole, NSAccessibilitySubroles.TabButtonSubrole),
            listItem: (NSAccessibilityRoles.GroupRole, None),
            treeItem: (NSAccessibilityRoles.RowRole, NSAccessibilitySubroles.OutlineRowSubrole),
            gridCell: (NSAccessibilityRoles.CellRole, None),
            menuItem: (NSAccessibilityRoles.MenuItemRole, None),
            slider: (NSAccessibilityRoles.SliderRole, None),
            handle: (NSAccessibilityRoles.HandleRole, None),
            separator: (NSAccessibilityRoles.SplitterRole, None),
            textBox: (NSAccessibilityRoles.TextFieldRole, None),
            progressBar: (NSAccessibilityRoles.ProgressIndicatorRole, None),
            group: (NSAccessibilityRoles.GroupRole, None),
            scrollArea: (NSAccessibilityRoles.ScrollAreaRole, None),
            status: (NSAccessibilityRoles.GroupRole, None),
            dialog: (NSAccessibilityRoles.GroupRole, None),
            toolbar: (NSAccessibilityRoles.ToolbarRole, None),
            radioGroup: (NSAccessibilityRoles.RadioGroupRole, None),
            tabList: (NSAccessibilityRoles.TabGroupRole, None),
            list: (NSAccessibilityRoles.ListRole, NSAccessibilitySubroles.ContentListSubrole),
            tree: (NSAccessibilityRoles.OutlineRole, None),
            grid: (NSAccessibilityRoles.TableRole, None),
            menu: (NSAccessibilityRoles.MenuRole, None));

    private static Unit Noticed(Native native, AccessibilityNotice notice) =>
        notice.Switch(
            native,
            announced: static (held, announced) => Posted(held.View, NSView.AnnouncementRequestedNotification, NSDictionary.FromObjectsAndKeys(
                [new NSString(announced.Text), NSNumber.FromInt64((long)announced.Live.Map(polite: NSAccessibilityPriorityLevel.Medium, assertive: NSAccessibilityPriorityLevel.High))],
                [NSAccessibilityNotificationUserInfoKeys.AnnouncementKey, NSAccessibilityNotificationUserInfoKeys.PriorityKey])),
            changed: static (held, changed) => held.Shown.Find(changed.Element).Map(shown => changed.Change.Switch(
                shown.Shown,
                value: static element => Posted(element, NSView.ValueChangedNotification, null),
                selection: static element => Posted(element, NSView.SelectedChildrenChangedNotification, null))).IfNone(unit),
            focusChanged: static (held, changed) => held.Shown.Find(changed.Element).Map(static shown => {
                shown.Shown.AccessibilityFocused = true;
                return unit;
            }).IfNone(unit),
            layoutChanged: static (held, _) => Posted(held.View, NSView.LayoutChangedNotification, null),
            detent: static (_, _) => {
                NSHapticFeedbackManager.DefaultPerformer.PerformFeedback(NSHapticFeedbackPattern.Alignment, NSHapticFeedbackPerformanceTime.DrawCompleted);
                return unit;
            });
}
