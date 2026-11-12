using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.EtoRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class NativeCallbackTests : CSharpCodeFixVerifier<NativeCallbackAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task RoutedCallbacks() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Runtime.CompilerServices;
        using System.Runtime.InteropServices;
        using AppKit;
        using Foundation;
        using LanguageExt;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Foundation {
            [AttributeUsage(AttributeTargets.Class)]
            public sealed class RegisterAttribute(string name, bool isWrapper) : Attribute {
                public string Name { get; } = name;
                public bool IsWrapper { get; } = isWrapper;
            }

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ExportAttribute(string selector) : Attribute {
                public string Selector { get; } = selector;
            }

            [Register("NSObject", true)]
            public class NSObject {
                public virtual void AwakeFromNib() { }
            }

            [Register("NSNotification", true)]
            public class NSNotification : NSObject {
                public NSObject? Object => null;
            }

            [Register("NSNotificationCenter", true)]
            public class NSNotificationCenter : NSObject {
                public NSObject AddObserver(string name, Action<NSNotification> notify) => this;
            }
        }

        namespace AppKit {
            [Register("NSAnimationContext", true)]
            public class NSAnimationContext : NSObject {
                public double Duration { get; set; }

                public static void RunAnimation(Action<NSAnimationContext> changes, Action completion) { }
            }
        }

        namespace Eto.Mac {
            public static class ObjCExtensions {
                public static bool AddMethod(nint type, nint selector, Delegate implementation, string signature) => true;
            }
        }

        namespace Rasm.Rhino {
            public static class Session {
                public static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") => effect.RunSafe().IfFail(failed);
                public static Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => effect.RunSafe().Map(static _ => unit).IfFail(unit);
                public static Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "") => unit;
            }
        }

        internal sealed class NotificationArgs(NSNotification notification) : EventArgs {
            public NSNotification Notification { get; } = notification;
        }

        internal sealed class RoutedCallbacks {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate nint HitTest(nint view, nint selector, nint point);

            private static readonly HitTest PassThrough = static (_, _, _) => 0;

            internal static NSObject Observed(NSNotificationCenter center, Func<NotificationArgs, IO<Unit>> deliver) =>
                center.AddObserver("changed", notification => Session.Run(deliver(new NotificationArgs(notification))));

            internal static void Animated(Func<IO<Unit>> changes) =>
                NSAnimationContext.RunAnimation(context => Session.Run(changes()), () => Session.Run(IO.pure(unit)));
        }
        """);

    [Fact]
    public Task DirectCallbacks() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Runtime.CompilerServices;
        using System.Runtime.InteropServices;
        using AppKit;
        using Foundation;
        using LanguageExt;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Foundation {
            [AttributeUsage(AttributeTargets.Class)]
            public sealed class RegisterAttribute(string name, bool isWrapper) : Attribute {
                public string Name { get; } = name;
                public bool IsWrapper { get; } = isWrapper;
            }

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ExportAttribute(string selector) : Attribute {
                public string Selector { get; } = selector;
            }

            [Register("NSObject", true)]
            public class NSObject {
                public virtual void AwakeFromNib() { }
            }

            [Register("NSNotification", true)]
            public class NSNotification : NSObject {
                public NSObject? Object => null;
            }

            [Register("NSNotificationCenter", true)]
            public class NSNotificationCenter : NSObject {
                public NSObject AddObserver(string name, Action<NSNotification> notify) => this;
            }
        }

        namespace AppKit {
            [Register("NSAnimationContext", true)]
            public class NSAnimationContext : NSObject {
                public double Duration { get; set; }

                public static void RunAnimation(Action<NSAnimationContext> changes, Action completion) { }
            }
        }

        namespace Eto.Mac {
            public static class ObjCExtensions {
                public static bool AddMethod(nint type, nint selector, Delegate implementation, string signature) => true;
            }
        }

        namespace Rasm.Rhino {
            public static class Session {
                public static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") => effect.RunSafe().IfFail(failed);
                public static Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => effect.RunSafe().Map(static _ => unit).IfFail(unit);
                public static Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "") => unit;
            }
        }

        internal sealed class DirectCallbacks {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void Frame(nint view, nint selector, nint link);

            private static readonly Frame Deliver = static (_, _, link) => {|#0:Ticked(link).Iter(static held => Session.Run(IO.pure(held)))|};

            internal static void Animated(NSAnimationContext context, Func<double> duration) =>
                NSAnimationContext.RunAnimation(changes => { {|#1:changes.Duration = duration()|}; }, () => Session.Run(IO.pure(unit)));

            internal static NSObject Observed(NSNotificationCenter center, Action<NSObject?> changed) =>
                center.AddObserver("changed", notification => {|#2:changed(notification.Object)|});

            internal static bool Added(nint type, nint selector) => Eto.Mac.ObjCExtensions.AddMethod(type, selector, Deliver, "v@:@");

            internal static bool Reassigned(nint type, nint selector) {
                Frame callback = static (_, _, _) => { };
                callback = static (_, _, link) => {|#3:Ticked(link)|};
                return Eto.Mac.ObjCExtensions.AddMethod(type, selector, callback, "v@:@");
            }

            private static Option<int> Ticked(nint link) => link == 0 ? None : Some(1);

            [Export("frame:")]
            private static void Exported(nint link) => {|#4:Ticked(link)|};

            [UnmanagedCallersOnly]
            private static nint Monitored(nint block) => {|#5:Ticked(block).IfNone(0)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Frame", "Option.Iter"),
        Diagnostic().WithLocation(1).WithArguments("NSAnimationContext.RunAnimation", "changes.Duration = duration()"),
        Diagnostic().WithLocation(2).WithArguments("NSNotificationCenter.AddObserver", "changed"),
        Diagnostic().WithLocation(3).WithArguments("Frame", "DirectCallbacks.Ticked"),
        Diagnostic().WithLocation(4).WithArguments("Exported", "DirectCallbacks.Ticked"),
        Diagnostic().WithLocation(5).WithArguments("Monitored", "Option.IfNone"));

    [Fact]
    public Task EagerControl() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Runtime.CompilerServices;
        using Eto.Drawing;
        using Eto.Forms;
        using Foundation;
        using LanguageExt;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Foundation {
            [AttributeUsage(AttributeTargets.Class)]
            public sealed class RegisterAttribute(string name, bool isWrapper) : Attribute {
                public string Name { get; } = name;
                public bool IsWrapper { get; } = isWrapper;
            }

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ExportAttribute(string selector) : Attribute {
                public string Selector { get; } = selector;
            }

            [Register("NSObject", true)]
            public class NSObject {
                public virtual void AwakeFromNib() { }
            }

            [Register("NSNotification", true)]
            public class NSNotification : NSObject {
                public NSObject? Object => null;
            }

            [Register("NSNotificationCenter", true)]
            public class NSNotificationCenter : NSObject {
                public NSObject AddObserver(string name, Action<NSNotification> notify) => this;
            }
        }

        namespace AppKit {
            [Register("NSAnimationContext", true)]
            public class NSAnimationContext : NSObject {
                public double Duration { get; set; }

                public static void RunAnimation(Action<NSAnimationContext> changes, Action completion) { }
            }
        }

        namespace Eto.Mac {
            public static class ObjCExtensions {
                public static bool AddMethod(nint type, nint selector, Delegate implementation, string signature) => true;
            }
        }

        namespace Rasm.Rhino {
            public static class Session {
                public static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") => effect.RunSafe().IfFail(failed);
                public static Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => effect.RunSafe().Map(static _ => unit).IfFail(unit);
                public static Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "") => unit;
            }
        }

        internal sealed class EagerControl : Drawable {
            private static IO<Unit> Move(SizeF size, float scale, MouseEventArgs args) => IO.pure(unit);

            protected override void OnMouseMove(MouseEventArgs e) {
                {|#0:Session.Run(Move(Size, ParentWindow.LogicalPixelSize, e))|};
                base.OnMouseMove(e);
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("OnMouseMove", "Size`, `ParentWindow"));

    [Fact]
    public Task WrappedControl() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Runtime.CompilerServices;
        using Eto.Drawing;
        using Eto.Forms;
        using Foundation;
        using LanguageExt;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Foundation {
            [AttributeUsage(AttributeTargets.Class)]
            public sealed class RegisterAttribute(string name, bool isWrapper) : Attribute {
                public string Name { get; } = name;
                public bool IsWrapper { get; } = isWrapper;
            }

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ExportAttribute(string selector) : Attribute {
                public string Selector { get; } = selector;
            }

            [Register("NSObject", true)]
            public class NSObject {
                public virtual void AwakeFromNib() { }
            }

            [Register("NSNotification", true)]
            public class NSNotification : NSObject {
                public NSObject? Object => null;
            }

            [Register("NSNotificationCenter", true)]
            public class NSNotificationCenter : NSObject {
                public NSObject AddObserver(string name, Action<NSNotification> notify) => this;
            }
        }

        namespace AppKit {
            [Register("NSAnimationContext", true)]
            public class NSAnimationContext : NSObject {
                public double Duration { get; set; }

                public static void RunAnimation(Action<NSAnimationContext> changes, Action completion) { }
            }
        }

        namespace Eto.Mac {
            public static class ObjCExtensions {
                public static bool AddMethod(nint type, nint selector, Delegate implementation, string signature) => true;
            }
        }

        namespace Rasm.Rhino {
            public static class Session {
                public static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") => effect.RunSafe().IfFail(failed);
                public static Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => effect.RunSafe().Map(static _ => unit).IfFail(unit);
                public static Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "") => unit;
            }
        }

        internal sealed class WrappedControl : Drawable {
            private static IO<Unit> Move(SizeF size, MouseEventArgs args) => IO.pure(unit);

            private static void Wrap(SizeF size, MouseEventArgs args) => Session.Run(Move(size, args));

            protected override void OnMouseMove(MouseEventArgs e) {
                {|#0:Wrap(Size, e)|};
                base.OnMouseMove(e);
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("OnMouseMove", "Size"));

    [Fact]
    public Task RoutedControl() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Runtime.CompilerServices;
        using Eto.Drawing;
        using Eto.Forms;
        using Foundation;
        using LanguageExt;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Foundation {
            [AttributeUsage(AttributeTargets.Class)]
            public sealed class RegisterAttribute(string name, bool isWrapper) : Attribute {
                public string Name { get; } = name;
                public bool IsWrapper { get; } = isWrapper;
            }

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ExportAttribute(string selector) : Attribute {
                public string Selector { get; } = selector;
            }

            [Register("NSObject", true)]
            public class NSObject {
                public virtual void AwakeFromNib() { }
            }

            [Register("NSNotification", true)]
            public class NSNotification : NSObject {
                public NSObject? Object => null;
            }

            [Register("NSNotificationCenter", true)]
            public class NSNotificationCenter : NSObject {
                public NSObject AddObserver(string name, Action<NSNotification> notify) => this;
            }
        }

        namespace AppKit {
            [Register("NSAnimationContext", true)]
            public class NSAnimationContext : NSObject {
                public double Duration { get; set; }

                public static void RunAnimation(Action<NSAnimationContext> changes, Action completion) { }
            }
        }

        namespace Eto.Mac {
            public static class ObjCExtensions {
                public static bool AddMethod(nint type, nint selector, Delegate implementation, string signature) => true;
            }
        }

        namespace Rasm.Rhino {
            public static class Session {
                public static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") => effect.RunSafe().IfFail(failed);
                public static Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => effect.RunSafe().Map(static _ => unit).IfFail(unit);
                public static Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "") => unit;
            }
        }

        internal sealed class RoutedControl : Drawable {
            private static IO<Unit> Move(SizeF size, float scale, MouseEventArgs args) => IO.pure(unit);

            protected override void OnMouseMove(MouseEventArgs e) {
                Session.Run(IO.lift(() => Move(Size, ParentWindow.LogicalPixelSize, e)).Bind(static moved => moved));
                base.OnMouseMove(e);
            }

            protected override void OnLostFocus(EventArgs e) => Session.Run(IO.lift(Invalidate));
        }
        """);
}
