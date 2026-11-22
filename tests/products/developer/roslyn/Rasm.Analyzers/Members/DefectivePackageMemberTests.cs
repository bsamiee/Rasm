using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DefectivePackageMemberTests : CSharpCodeFixVerifier<PlatformMemberAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsDefectivePackageMemberWithItsWorkingForm() => VerifyAnalyzerAsync(
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

    [Fact]
    public Task ReportsDefectiveAppKitMemberWithItsWorkingForm() => VerifyAnalyzerAsync(
        """
        using System;
        using AppKit;
        using Foundation;

        internal static class DefectiveAppKitMembers {
            internal static NSObject Animated(NSWindow window) => {|#0:window.Animator|};

            internal static NSObject Observed(EventHandler<NSNotificationEventArgs> handler) => {|#1:NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(handler)|};

            internal static NSObject Scoped(NSObject sender, EventHandler<NSNotificationEventArgs> handler) => {|#2:NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(sender, handler)|};
        }
        """,
        Diagnostic("RASM3113").WithLocation(0).WithArguments("window.Animator", "throws `RuntimeException` on an Eto `FloatingForm`", "write Eto `Window.Bounds` from an `Animated` sampled on `FrameLinks.Linked` ticks"),
        Diagnostic("RASM3113").WithLocation(1).WithArguments("NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(handler)", "registers on `NSNotificationCenter.DefaultCenter` while AppKit posts to the workspace center",
            "call `NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DisplayOptionsDidChangeNotification, handler)`"),
        Diagnostic("RASM3113").WithLocation(2).WithArguments("NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(sender, handler)", "registers on `NSNotificationCenter.DefaultCenter` while AppKit posts to the workspace center",
            "call `NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DisplayOptionsDidChangeNotification, handler)`"));

    [Fact]
    public Task IgnoresWorkingFormsAndSameNamedTypeElsewhere() => VerifyAnalyzerAsync(
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
