using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DefectivePackageMemberTests : CSharpCodeFixVerifier<PlatformMemberAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsEachDefectivePackageMemberByItsSymbolAndIgnoresItsSiblingsAndSameNamedMembersElsewhere() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Drawing;
        using AppKit;
        using Foundation;
        using Rhino.Display;
        using Rhino.Render.PostEffects;
        using SkiaSharp;

        internal static class Defective {
            internal static IntPtr Handle(Bitmap bitmap) => {|RASM3113:bitmap.GetHicon()|};
            internal static void Presented(SKCanvas canvas) => {|RASM3113:canvas.Flush()|};
            internal static Func<Guid, RhinoViewport> Viewport => {|RASM3113:RhinoViewport.FromId|};
            internal static float Scale(DisplayPipeline pipeline) => {|RASM3113:pipeline.DpiScale|};
            internal static void Refreshed(DisplayBitmap shown, Bitmap bitmap) => {|RASM3113:shown.Update(bitmap)|};
            internal static PostEffectData Effect(PostEffectCollection effects, Guid id) => {|RASM3113:effects.PostEffectDataFromId(id)|};
            internal static NSObject Animated(NSWindow window) => {|RASM3113:window.Animator|};
            internal static NSObject Observed(EventHandler<NSNotificationEventArgs> handler) => {|RASM3113:NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(handler)|};
            internal static NSObject Scoped(NSObject sender, EventHandler<NSNotificationEventArgs> handler) => {|RASM3113:NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(sender, handler)|};
            internal static Color Sampled(Bitmap bitmap) => bitmap.GetPixel(0, 0);
            internal static IntPtr Local(Images.Bitmap bitmap) => bitmap.GetHicon();
            internal static string Named => nameof(Bitmap.GetHicon);
        }

        namespace Images {
            internal sealed class Bitmap {
                internal IntPtr GetHicon() => IntPtr.Zero;
            }
        }
        """);
}
