using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.UI;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
public enum HudControl { Play = 0, Pause = 1, Lock = 2, Unlock = 3, ProductName = 4, StatusText = 5, Time = 6, PostEffectsOn = 7, PostEffectsOff = 8 }

public enum HudGesture { LeftClicked = 0, RightClicked = 1, DoubleClicked = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HudEvent(HudControl Control, HudGesture Gesture);

public sealed record RealtimeHud(LocalizeStringPair ProductName, Option<Func<IO<string>>> Status, bool AllowEditMaxPasses, bool ShowMaxPasses, bool ShowPasses, bool ShowControls);

public sealed record RealtimeState(int MaximumPasses, int LastRenderedPass, bool Paused, bool Locked, Instant StartTime);

public sealed record RealtimeSettings(Option<int> MaxPasses, Option<bool> PostEffectsOn, bool DrawOpenGl, bool FastDraw, bool DontRegisterAttributesOnStart, DisplayTechnology RequiredTechnology);

public sealed record RealtimeCallbacks(
    Func<IO<(int Width, int Height)>> RenderSize,
    Func<int, int, RhinoDoc, ViewInfo, ViewportInfo, bool, RenderWindow, IO<Unit>> Start,
    Func<IO<Unit>> Shutdown,
    Func<IO<bool>> Started,
    Func<ViewInfo, IO<bool>> FrameBufferAvailable,
    Func<IO<bool>> Completed,
    Func<IO<RealtimeState>> State,
    Option<Func<RhinoDoc, ViewInfo, DisplayPipelineAttributes, IO<Unit>>> CreateWorld,
    Option<Func<IO<double>>> CaptureProgress,
    Option<Func<int, int, IO<bool>>> SizeChanged,
    Option<Func<DisplayPipelineAttributes, IO<Unit>>> AttributesChanged,
    Option<Func<DisplayPipeline, IO<Unit>>> InitFramebuffer,
    Option<Func<DisplayPipeline, IO<Unit>>> Middleground,
    Option<Func<int, IO<Unit>>> MaxPassesChanged,
    Option<Func<HudEvent, IO<Unit>>> HudGesture,
    Option<RealtimeHud> Hud,
    RealtimeSettings Settings,
    Action<Error> Reject);

public sealed record LightManagerCallbacks(
    Guid PluginId,
    Guid RenderEngineId,
    Func<RhinoDoc, Light, IO<Unit>> Modify,
    Func<RhinoDoc, Light, bool, IO<Unit>> Delete,
    Func<RhinoDoc, IO<Seq<Light>>> Lights,
    Func<RhinoDoc, Guid, IO<Option<Light>>> FromId,
    Func<RhinoDoc, Light, IO<int>> SerialOf,
    Func<RhinoDoc, Seq<Light>, IO<Unit>> Edit,
    Func<RhinoDoc, Seq<Light>, IO<Unit>> Group,
    Func<RhinoDoc, Seq<Light>, IO<Unit>> Ungroup,
    Func<RhinoDoc, Light, IO<string>> Describe,
    Option<Func<RhinoDoc, Guid, bool, IO<Unit>>> SetSolo,
    Option<Func<RhinoDoc, Guid, IO<bool>>> GetSolo,
    Option<Func<RhinoDoc, IO<int>>> SoloCount,
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

    public sealed override void PostConstruct() {
        base.PostConstruct();
        _ = callbacks.Settings.MaxPasses.Iter(passes => MaxPasses = passes);
        _ = callbacks.Settings.PostEffectsOn.Iter(enabled => PostEffectsOn = enabled);
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

    public sealed override void CreateWorld(RhinoDoc doc, ViewInfo viewInfo, DisplayPipelineAttributes displayPipelineAttributes) {
        base.CreateWorld(doc, viewInfo, displayPipelineAttributes);
        _ = Answers.Answer(callbacks.CreateWorld.Map(create => create(doc, viewInfo, displayPipelineAttributes)), callbacks.Reject, static () => unit);
    }

    public sealed override int LastRenderedPass() =>
        Read(static state => state.LastRenderedPass, base.LastRenderedPass());

    public sealed override bool ShowCaptureProgress() =>
        callbacks.CaptureProgress.IsSome;

    public sealed override double CaptureProgress() =>
        Answers.Answer(callbacks.CaptureProgress.Map(static progress => progress()), callbacks.Reject, base.CaptureProgress);

    public sealed override bool OnRenderSizeChanged(int width, int height) =>
        Answers.Answer(callbacks.SizeChanged.Map(changed => changed(width, height)), callbacks.Reject, () => base.OnRenderSizeChanged(width, height));

    public sealed override bool DrawOpenGl() =>
        callbacks.Settings.DrawOpenGl;

    public sealed override bool UseFastDraw() =>
        callbacks.Settings.FastDraw;

    public sealed override string HudProductName() =>
        callbacks.Hud.Match(Some: static hud => hud.ProductName.Local, None: base.HudProductName);

    public sealed override string HudCustomStatusText() =>
        Answers.Answer(callbacks.Hud.Bind(static hud => hud.Status).Map(static status => status()), callbacks.Reject, base.HudCustomStatusText);

    public sealed override int HudMaximumPasses() =>
        Read(static state => state.MaximumPasses, base.HudMaximumPasses());

    public sealed override int HudLastRenderedPass() =>
        Read(static state => state.LastRenderedPass, base.HudLastRenderedPass());

    public sealed override bool HudRendererPaused() =>
        Read(static state => state.Paused, base.HudRendererPaused());

    public sealed override bool HudRendererLocked() =>
        Read(static state => state.Locked, base.HudRendererLocked());

    public sealed override DateTime HudStartTime() =>
        Read(static state => state.StartTime.ToDateTimeUtc(), base.HudStartTime());

    public sealed override bool HudAllowEditMaxPasses() =>
        callbacks.Hud.Match(Some: static hud => hud.AllowEditMaxPasses, None: base.HudAllowEditMaxPasses);

    public sealed override bool HudShowMaxPasses() =>
        callbacks.Hud.Match(Some: static hud => hud.ShowMaxPasses, None: base.HudShowMaxPasses);

    public sealed override bool HudShowPasses() =>
        callbacks.Hud.Match(Some: static hud => hud.ShowPasses, None: base.HudShowPasses);

    public sealed override bool HudShowCustomStatusText() =>
        callbacks.Hud.Exists(static hud => hud.Status.IsSome);

    public sealed override bool HudShowControls() =>
        callbacks.Hud.Match(Some: static hud => hud.ShowControls, None: base.HudShowControls);

    public sealed override bool HudShow() =>
        callbacks.Hud.IsSome;

    private TValue Read<TValue>(Func<RealtimeState, TValue> field, TValue fallback) =>
        Answers.Answer(callbacks.State().Map(field), callbacks.Reject, fallback);

    private void Deliver(IO<Unit> effect) =>
        _ = Answers.Answer(effect, callbacks.Reject, unit);

    private EventHandler Hud(Func<HudEvent, IO<Unit>> hook, HudControl control, HudGesture gesture) =>
        (_, _) => Deliver(hook(new HudEvent(control, gesture)));
}

public abstract class RealtimeEngineInfo(LocalizeStringPair name, Guid id, Type engine, RealtimeSettings settings) : RealtimeDisplayModeClassInfo {
    public sealed override string Name => name.Local;

    public sealed override Guid GUID => id;

    public sealed override Type RealtimeDisplayModeType => engine;

    public sealed override bool DrawOpenGl => settings.DrawOpenGl;

    public sealed override bool DontRegisterAttributesOnStart => settings.DontRegisterAttributesOnStart;

    public sealed override DisplayTechnology RequiredDisplayTechnology => settings.RequiredTechnology;
}

public abstract class LightManager(LightManagerCallbacks callbacks) : LightManagerSupport {
    public sealed override Guid PluginId() =>
        callbacks.PluginId;

    public sealed override Guid RenderEngineId() =>
        callbacks.RenderEngineId;

    public sealed override void ModifyLight(RhinoDoc doc, Light light) =>
        _ = Answers.Answer(callbacks.Modify(doc, light), callbacks.Reject, unit);

    public sealed override bool DeleteLight(RhinoDoc doc, Light light, bool bUndelete) =>
        Answers.Succeeded(callbacks.Delete(doc, light, bUndelete), callbacks.Reject);

    public sealed override void GetLights(RhinoDoc doc, ref LightArray light_array) {
        LightArray target = light_array;
        _ = Answers.Answer(callbacks.Lights(doc), callbacks.Reject, Seq<Light>()).Iter(target.Append);
    }

    public sealed override bool LightFromId(RhinoDoc doc, Guid uuid, ref Light light) {
        Option<Light> found = Answers.Answer(callbacks.FromId(doc, uuid), callbacks.Reject, Option<Light>.None);
        light = found.IfNone(light);
        return found.IsSome;
    }

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

    public sealed override bool SetLightSolo(RhinoDoc doc, Guid uuid_light, bool bSolo) =>
        Answers.Succeeded(callbacks.SetSolo.Map(set => set(doc, uuid_light, bSolo)), callbacks.Reject, () => base.SetLightSolo(doc, uuid_light, bSolo));

    public sealed override bool GetLightSolo(RhinoDoc doc, Guid uuid_light) =>
        Answers.Answer(callbacks.GetSolo.Map(get => get(doc, uuid_light)), callbacks.Reject, refused: false, () => base.GetLightSolo(doc, uuid_light));

    public sealed override int LightsInSoloStorage(RhinoDoc doc) =>
        Answers.Answer(callbacks.SoloCount.Map(count => count(doc)), callbacks.Reject, refused: 0, () => base.LightsInSoloStorage(doc));

    private static Seq<Light> Rows(LightArray lights) =>
        toSeq(Enumerable.Range(0, lights.Count()).Select(lights.ElementAt));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Realtime {
    public static IO<Seq<RealtimeDisplayModeClassInfo>> Register(System.Reflection.Assembly assembly, Guid plugInId) =>
        Answers.Registered(RealtimeDisplayMode.RegisterDisplayModes, assembly, plugInId, nameof(RealtimeDisplayMode.RegisterDisplayModes));

    public static IO<Unit> Notify(LightManager manager, RhinoDoc doc, LightMangerSupportCustomEvent change) =>
        IO.lift(() => {
            using Light light = new();
            Light handed = light;
            manager.OnCustomLightEvent(doc, change, ref handed);
        });
}
