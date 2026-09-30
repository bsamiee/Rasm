using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
public enum HudControl { Play = 0, Pause = 1, Lock = 2, Unlock = 3, ProductName = 4, StatusText = 5, Time = 6, PostEffectsOn = 7, PostEffectsOff = 8 }

public enum HudGesture { LeftClicked = 0, RightClicked = 1, DoubleClicked = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HudEvent(HudControl Control, HudGesture Gesture);

public sealed record RealtimeState(int MaximumPasses, int LastRenderedPass, bool Paused, bool Locked, DateTimeOffset StartTime);

public sealed record RealtimeCallbacks(
    Func<IO<(int Width, int Height)>> RenderSize,
    Func<int, int, RhinoDoc, ViewInfo, ViewportInfo, bool, RenderWindow, IO<Unit>> Start,
    Func<IO<Unit>> Shutdown,
    Func<IO<bool>> Started,
    Func<ViewInfo, IO<bool>> FrameBufferAvailable,
    Func<IO<bool>> Completed,
    Func<IO<RealtimeState>> State,
    Option<Func<DisplayPipelineAttributes, IO<Unit>>> AttributesChanged,
    Option<Func<DisplayPipeline, IO<Unit>>> InitFramebuffer,
    Option<Func<DisplayPipeline, IO<Unit>>> Middleground,
    Option<Func<int, IO<Unit>>> MaxPassesChanged,
    Option<Func<HudEvent, IO<Unit>>> HudGesture,
    Action<Error> Reject);

public sealed record LightManagerCallbacks(
    Func<RhinoDoc, Light, IO<Unit>> Modify,
    Func<RhinoDoc, Light, bool, IO<Unit>> Delete,
    Func<RhinoDoc, IO<Seq<Light>>> Lights,
    Func<RhinoDoc, Guid, Light, IO<Unit>> FromId,
    Func<RhinoDoc, Light, IO<int>> SerialOf,
    Func<RhinoDoc, Seq<Light>, IO<Unit>> Edit,
    Func<RhinoDoc, Seq<Light>, IO<Unit>> Group,
    Func<RhinoDoc, Seq<Light>, IO<Unit>> Ungroup,
    Func<RhinoDoc, Light, IO<string>> Describe,
    Action<Error> Reject);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class RealtimeEngine : RealtimeDisplayMode {
    private readonly RealtimeCallbacks callbacks;

    protected RealtimeEngine(RealtimeCallbacks callbacks) {
        this.callbacks = callbacks;
        _ = callbacks.AttributesChanged.Iter(hook => OnDisplayPipelineSettingsChanged += (_, args) => Deliver(hook(args.Attributes)));
        _ = callbacks.InitFramebuffer.Iter(hook => OnInitFramebuffer += (_, args) => Deliver(hook(args.Pipeline)));
        _ = callbacks.Middleground.Iter(hook => OnDrawMiddleground += (_, args) => Deliver(hook(args.Pipeline)));
        _ = callbacks.MaxPassesChanged.Iter(hook => MaxPassesChanged += (_, args) => Deliver(hook(args.MaxPasses)));
        _ = callbacks.HudGesture.Iter(hook => {
            HudPlayButtonLeftClicked += Hud(hook, HudControl.Play, HudGesture.LeftClicked);
            HudPlayButtonRightClicked += Hud(hook, HudControl.Play, HudGesture.RightClicked);
            HudPlayButtonDoubleClicked += Hud(hook, HudControl.Play, HudGesture.DoubleClicked);
            HudPauseButtonLeftClicked += Hud(hook, HudControl.Pause, HudGesture.LeftClicked);
            HudPauseButtonRightClicked += Hud(hook, HudControl.Pause, HudGesture.RightClicked);
            HudPauseButtonDoubleClicked += Hud(hook, HudControl.Pause, HudGesture.DoubleClicked);
            HudLockButtonLeftClicked += Hud(hook, HudControl.Lock, HudGesture.LeftClicked);
            HudLockButtonRightClicked += Hud(hook, HudControl.Lock, HudGesture.RightClicked);
            HudLockButtonDoubleClicked += Hud(hook, HudControl.Lock, HudGesture.DoubleClicked);
            HudUnlockButtonLeftClicked += Hud(hook, HudControl.Unlock, HudGesture.LeftClicked);
            HudUnlockButtonRightClicked += Hud(hook, HudControl.Unlock, HudGesture.RightClicked);
            HudUnlockButtonDoubleClicked += Hud(hook, HudControl.Unlock, HudGesture.DoubleClicked);
            HudProductNameLeftClicked += Hud(hook, HudControl.ProductName, HudGesture.LeftClicked);
            HudProductNameRightClicked += Hud(hook, HudControl.ProductName, HudGesture.RightClicked);
            HudProductNameDoubleClicked += Hud(hook, HudControl.ProductName, HudGesture.DoubleClicked);
            HudStatusTextLeftClicked += Hud(hook, HudControl.StatusText, HudGesture.LeftClicked);
            HudStatusTextRightClicked += Hud(hook, HudControl.StatusText, HudGesture.RightClicked);
            HudStatusTextDoubleClicked += Hud(hook, HudControl.StatusText, HudGesture.DoubleClicked);
            HudTimeLeftClicked += Hud(hook, HudControl.Time, HudGesture.LeftClicked);
            HudTimeRightClicked += Hud(hook, HudControl.Time, HudGesture.RightClicked);
            HudTimeDoubleClicked += Hud(hook, HudControl.Time, HudGesture.DoubleClicked);
            HudPostEffectsOnButtonLeftClicked += Hud(hook, HudControl.PostEffectsOn, HudGesture.LeftClicked);
            HudPostEffectsOnButtonRightClicked += Hud(hook, HudControl.PostEffectsOn, HudGesture.RightClicked);
            HudPostEffectsOnButtonDoubleClicked += Hud(hook, HudControl.PostEffectsOn, HudGesture.DoubleClicked);
            HudPostEffectsOffButtonLeftClicked += Hud(hook, HudControl.PostEffectsOff, HudGesture.LeftClicked);
            HudPostEffectsOffButtonRightClicked += Hud(hook, HudControl.PostEffectsOff, HudGesture.RightClicked);
            HudPostEffectsOffButtonDoubleClicked += Hud(hook, HudControl.PostEffectsOff, HudGesture.DoubleClicked);
        });
    }

    public sealed override void GetRenderSize(out int width, out int height) =>
        (width, height) = Answers.Answer(callbacks.RenderSize(), callbacks.Reject, (0, 0));

    public sealed override bool StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) =>
        Answers.Succeeded(callbacks.Start(w, h, doc, view, viewportInfo, forCapture, renderWindow), callbacks.Reject);

    public sealed override void ShutdownRenderer() =>
        Deliver(callbacks.Shutdown());

    public sealed override bool IsRendererStarted() =>
        Answers.Answer(callbacks.Started(), callbacks.Reject, fallback: false);

    public sealed override bool IsFrameBufferAvailable(ViewInfo view) =>
        Answers.Answer(callbacks.FrameBufferAvailable(view), callbacks.Reject, fallback: false);

    public sealed override bool IsCompleted() =>
        Answers.Answer(callbacks.Completed(), callbacks.Reject, fallback: false);

    public sealed override int LastRenderedPass() =>
        Read(static state => state.LastRenderedPass, base.LastRenderedPass());

    public sealed override int HudMaximumPasses() =>
        Read(static state => state.MaximumPasses, base.HudMaximumPasses());

    public sealed override int HudLastRenderedPass() =>
        Read(static state => state.LastRenderedPass, base.HudLastRenderedPass());

    public sealed override bool HudRendererPaused() =>
        Read(static state => state.Paused, base.HudRendererPaused());

    public sealed override bool HudRendererLocked() =>
        Read(static state => state.Locked, base.HudRendererLocked());

    public sealed override DateTime HudStartTime() =>
        Read(static state => state.StartTime.UtcDateTime, base.HudStartTime());

    private TValue Read<TValue>(Func<RealtimeState, TValue> field, TValue fallback) =>
        Answers.Answer(callbacks.State().Map(field), callbacks.Reject, fallback);

    private void Deliver(IO<Unit> effect) =>
        _ = Answers.Answer(effect, callbacks.Reject, unit);

    private EventHandler Hud(Func<HudEvent, IO<Unit>> hook, HudControl control, HudGesture gesture) =>
        (_, _) => Deliver(hook(new HudEvent(control, gesture)));
}

public abstract class LightManager(LightManagerCallbacks callbacks) : LightManagerSupport {
    public sealed override void ModifyLight(RhinoDoc doc, Light light) =>
        _ = Answers.Answer(callbacks.Modify(doc, light), callbacks.Reject, unit);

    public sealed override bool DeleteLight(RhinoDoc doc, Light light, bool bUndelete) =>
        Answers.Succeeded(callbacks.Delete(doc, light, bUndelete), callbacks.Reject);

    public sealed override void GetLights(RhinoDoc doc, ref LightArray light_array) {
        LightArray target = light_array;
        _ = Answers.Answer(callbacks.Lights(doc), callbacks.Reject, Seq<Light>()).Iter(target.Append);
    }

    public sealed override bool LightFromId(RhinoDoc doc, Guid uuid, ref Light light) =>
        Answers.Succeeded(callbacks.FromId(doc, uuid, light), callbacks.Reject);

    public sealed override int ObjectSerialNumberFromLight(RhinoDoc doc, ref Light light) =>
        Answers.Answer(callbacks.SerialOf(doc, light), callbacks.Reject, 0);

    public sealed override bool OnEditLight(RhinoDoc doc, ref LightArray light_array) =>
        Answers.Succeeded(callbacks.Edit(doc, Rows(light_array)), callbacks.Reject);

    public sealed override void GroupLights(RhinoDoc doc, ref LightArray light_array) =>
        _ = Answers.Answer(callbacks.Group(doc, Rows(light_array)), callbacks.Reject, unit);

    public sealed override void UnGroup(RhinoDoc doc, ref LightArray light_array) =>
        _ = Answers.Answer(callbacks.Ungroup(doc, Rows(light_array)), callbacks.Reject, unit);

    public sealed override string LightDescription(RhinoDoc doc, ref Light light) =>
        Answers.Answer(callbacks.Describe(doc, light), callbacks.Reject, "");

    protected IO<Unit> Notify(RhinoDoc doc, LightMangerSupportCustomEvent change) =>
        IO.lift(() => {
            Light? unread = null;
            OnCustomLightEvent(doc, change, ref unread);
        });

    private static Seq<Light> Rows(LightArray lights) =>
        toSeq(Enumerable.Range(0, lights.Count()).Select(lights.ElementAt));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Realtime {
    // --- [REGISTRY]
    public static IO<Seq<RealtimeDisplayModeClassInfo>> Register(System.Reflection.Assembly assembly, Guid plugInId) =>
        Answers.Registered(RealtimeDisplayMode.RegisterDisplayModes, assembly, plugInId, nameof(RealtimeDisplayMode.RegisterDisplayModes));

    // --- [CHANGE_QUEUE]
    public static IO<TValue> WithView<TValue>(global::Rhino.Render.ChangeQueue.ChangeQueue queue, Func<ViewInfo, IO<TValue>> body) =>
        DisposalOps.Using(IO.lift(() => Missing.Unless(queue.GetQueueView(), nameof(queue.GetQueueView))), body);
}
