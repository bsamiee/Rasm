using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlatformMemberTests : CSharpCodeFixVerifier<PlatformMemberAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3111]

    [Fact(DisplayName = "Path.GetInvalidFileNameChars and GetInvalidPathChars are reported; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs WarnOnThreadSleepInAsyncMethod")]
    public Task RunningPlatformSets() => VerifyAnalyzerAsync(
        """
        using System;
        using System.IO;
        using static System.IO.Path;

        internal static class RunningPlatformSets {
            internal static char[] Names => {|#0:Path.GetInvalidFileNameChars()|};

            internal static char[] Paths => {|#1:System.IO.Path.GetInvalidPathChars()|};

            internal static char[] Imported => {|#2:GetInvalidFileNameChars()|};

            internal static Func<char[]> Grouped => {|#3:Path.GetInvalidPathChars|};
        }
        """,
        Diagnostic("RASM3111").WithLocation(0).WithArguments("Path.GetInvalidFileNameChars()"),
        Diagnostic("RASM3111").WithLocation(1).WithArguments("System.IO.Path.GetInvalidPathChars()"),
        Diagnostic("RASM3111").WithLocation(2).WithArguments("GetInvalidFileNameChars()"),
        Diagnostic("RASM3111").WithLocation(3).WithArguments("Path.GetInvalidPathChars"));

    [Fact(DisplayName = "A declared SearchValues set, other Path members, nameof, and a same-named type of another namespace are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnOtherMethodsWithSleepName")]
    public Task PortableSet() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Buffers;
        using System.IO;

        internal static class PortableSet {
            private static readonly SearchValues<char> Reserved = SearchValues.Create("<>:\"/\\|?*");

            internal static bool Portable(string name) => !name.AsSpan().ContainsAny(Reserved);

            internal static string Leaf(string path) => Path.GetFileName(path);

            internal static char Separator => Path.DirectorySeparatorChar;

            internal static string Named => nameof(Path.GetInvalidFileNameChars);

            internal static char[] Local => Names.Path.GetInvalidFileNameChars();
        }

        namespace Names {
            internal static class Path {
                internal static char[] GetInvalidFileNameChars() => [];
            }
        }
        """);

    // --- [RASM3112]

    [Fact(DisplayName = "Vector2 and Vector3 All and Any comparisons are reported; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs WarnOnThreadSleepInAsyncMethod")]
    public Task WidenedComparisons() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Numerics;

        internal static class WidenedComparisons {
            internal static bool Above(Vector3 left, Vector3 right) => {|#0:Vector3.GreaterThanAll(left, right)|};

            internal static bool Touching(Vector2 left, Vector2 right) => {|#1:Vector2.EqualsAny(left, right)|};

            internal static bool Within(Vector3 left, Vector3 right) => {|#2:Vector3.LessThanOrEqualAll(left, right)|};

            internal static Func<Vector2, Vector2, bool> Grouped => {|#3:Vector2.GreaterThanAny|};
        }
        """,
        Diagnostic("RASM3112").WithLocation(0).WithArguments("Vector3.GreaterThanAll(left, right)"),
        Diagnostic("RASM3112").WithLocation(1).WithArguments("Vector2.EqualsAny(left, right)"),
        Diagnostic("RASM3112").WithLocation(2).WithArguments("Vector3.LessThanOrEqualAll(left, right)"),
        Diagnostic("RASM3112").WithLocation(3).WithArguments("Vector2.GreaterThanAny"));

    [Fact(DisplayName = "Lane-mask forms, ==, zero-extended scalar All and Any, Vector4 comparisons, and nameof are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnTaskDelayInAsyncMethod")]
    public Task ComponentComparisons() => VerifyAnalyzerAsync(
        """
        using System.Numerics;

        internal static class ComponentComparisons {
            internal static bool Above(Vector3 left, Vector3 right) => Vector3.AllWhereAllBitsSet(Vector3.GreaterThan(left, right));

            internal static bool Touching(Vector2 left, Vector2 right) => Vector2.AnyWhereAllBitsSet(Vector2.GreaterThan(left, right));

            internal static bool Same(Vector3 left, Vector3 right) => left == right;

            internal static bool Uniform(Vector3 vector) => Vector3.All(vector, 1f);

            internal static bool Zeroed(Vector2 vector) => Vector2.Any(vector, 0f);

            internal static bool Full(Vector4 left, Vector4 right) => Vector4.GreaterThanAll(left, right);

            internal static string Named => nameof(Vector3.GreaterThanAll);
        }
        """);

    // --- [RASM3113]

    [Fact(DisplayName = "Package members with a known defect are reported with their fact and working form; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs WarnOnThreadSleepInAsyncMethod")]
    public Task DefectiveMembers() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Drawing;
        using Rhino.Display;
        using Rhino.Render.PostEffects;
        using SkiaSharp;

        internal static class DefectiveMembers {
            internal static IntPtr Handle(Bitmap bitmap) => {|#0:bitmap.GetHicon()|};

            internal static void Presented(SKCanvas canvas) => {|#1:canvas.Flush()|};

            internal static RhinoViewport Viewport(Guid id) => {|#2:RhinoViewport.FromId(id)|};

            internal static float Scale(DisplayPipeline pipeline) => {|#3:pipeline.DpiScale|};

            internal static void Refreshed(DisplayBitmap shown, Bitmap bitmap) => {|#4:shown.Update(bitmap)|};

            internal static PostEffectData Effect(PostEffectCollection effects, Guid id) => {|#5:effects.PostEffectDataFromId(id)|};

            internal static Func<Guid, RhinoViewport> Grouped => {|#6:RhinoViewport.FromId|};
        }
        """,
        Diagnostic("RASM3113").WithLocation(0).WithArguments("bitmap.GetHicon()", "leaks a handle no `Icon` releases", "call host overloads taking an assembly and resource name, or `new Icon(Stream)` on `.ico` resources"),
        Diagnostic("RASM3113").WithLocation(1).WithArguments("canvas.Flush()", "does nothing on a raster canvas", "present GPU surfaces through `SKSurface.Flush(submit: true)`"),
        Diagnostic("RASM3113").WithLocation(2).WithArguments("RhinoViewport.FromId(id)", "returns null for detail viewports and another document's viewport for repeated ids",
            "match `doc.Objects.FindId(id) is DetailViewObject`, then `doc.Views.Find(id)`"),
        Diagnostic("RASM3113").WithLocation(3).WithArguments("pipeline.DpiScale", "reads 1 for a view not on screen", "read `RhinoEtoApp.MainWindowForDocument(view.Document).LogicalPixelSize`"),
        Diagnostic("RASM3113").WithLocation(4).WithArguments("shown.Update(bitmap)", "flips images vertically while its constructor does not", "create `new DisplayBitmap(bitmap)` per change and dispose the old one"),
        Diagnostic("RASM3113").WithLocation(5).WithArguments("effects.PostEffectDataFromId(id)", "answers a missing id with data that throws on first use", "find the id among enumerated `PostEffectData` rows"),
        Diagnostic("RASM3113").WithLocation(6).WithArguments("RhinoViewport.FromId", "returns null for detail viewports and another document's viewport for repeated ids",
            "match `doc.Objects.FindId(id) is DetailViewObject`, then `doc.Views.Find(id)`"));

    [Fact(DisplayName = "AppKit members with a known defect are reported over stand-in declarations; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs WarnOnThreadSleepInAsyncMethod")]
    public Task DefectiveAppKitMembers() => VerifyAnalyzerAsync(
        """
        using System;
        using AppKit;
        using Foundation;

        internal static class DefectiveAppKitMembers {
            internal static NSObject Animated(NSWindow window) => {|#0:window.Animator|};

            internal static NSObject Observed(EventHandler<NSNotificationEventArgs> handler) => {|#1:NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(handler)|};

            internal static NSObject Scoped(NSObject sender, EventHandler<NSNotificationEventArgs> handler) => {|#2:NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(sender, handler)|};
        }

        namespace Foundation {
            public class NSObject;

            public sealed class NSNotificationEventArgs : EventArgs;
        }

        namespace AppKit {
            public class NSWindow : NSObject {
                public NSObject Animator => this;
            }

            public class NSWorkspace : NSObject {
                public static class Notifications {
                    public static NSObject ObserveDisplayOptionsDidChange(EventHandler<NSNotificationEventArgs> handler) => new();

                    public static NSObject ObserveDisplayOptionsDidChange(NSObject sender, EventHandler<NSNotificationEventArgs> handler) => sender;
                }
            }
        }
        """,
        Diagnostic("RASM3113").WithLocation(0).WithArguments("window.Animator", "throws `RuntimeException` on an Eto `FloatingForm`", "write Eto `Window.Bounds` from an `Animated` sampled on `FrameLinks.Linked` ticks"),
        Diagnostic("RASM3113").WithLocation(1).WithArguments("NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(handler)", "registers on `NSNotificationCenter.DefaultCenter` while AppKit posts to the workspace center",
            "call `NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DisplayOptionsDidChangeNotification, handler)`"),
        Diagnostic("RASM3113").WithLocation(2).WithArguments("NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(sender, handler)", "registers on `NSNotificationCenter.DefaultCenter` while AppKit posts to the workspace center",
            "call `NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DisplayOptionsDidChangeNotification, handler)`"));

    [Fact(DisplayName = "The working forms, nameof, and a same-named type of another namespace are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnOtherMethodsWithSleepName")]
    public Task WorkingForms() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Drawing;
        using System.IO;
        using System.Linq;
        using Rhino;
        using Rhino.Display;
        using Rhino.DocObjects;
        using Rhino.Render.PostEffects;
        using SkiaSharp;

        internal static class WorkingForms {
            internal static Icon Loaded(Stream resource) => new Icon(resource);

            internal static void Presented(SKSurface surface) => surface.Flush(submit: true);

            internal static bool Detail(RhinoDoc doc, Guid id) => doc.Objects.FindId(id) is DetailViewObject;

            internal static RhinoView Found(RhinoDoc doc, Guid id) => doc.Views.Find(id);

            internal static float Scale(RhinoDoc doc) => Rhino.UI.RhinoEtoApp.MainWindowForDocument(doc).LogicalPixelSize;

            internal static DisplayBitmap Shown(Bitmap bitmap) => new DisplayBitmap(bitmap);

            internal static PostEffectData? Effect(PostEffectCollection effects, Guid id) => effects.FirstOrDefault(effect => effect.Id == id);

            internal static string Named => nameof(Bitmap.GetHicon);

            internal static IntPtr Local(Images.Bitmap bitmap) => bitmap.GetHicon();
        }

        namespace Images {
            internal sealed class Bitmap {
                internal IntPtr GetHicon() => IntPtr.Zero;
            }
        }
        """);
}
