using Rasm.RhinoCommon.Objects;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Render;

namespace Rasm.RhinoCommon.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EngineLight(LightState Light, Option<uint> ObjectSerial);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedLightManager<TManager> : LightManagerSupport
    where TManager : DefinedLightManager<TManager>, new() {
    // --- [OWNER]
    internal static readonly PlugInSlot<DefinedLightManager<TManager>> Slot = new();

    private readonly IPlugInViews owner;

    protected DefinedLightManager() {
        owner = PlugInOwners.Owner(typeof(TManager));
        _ = owner.Run(Slot.Publish(owner, this));
    }

    // --- [ROWS]
    protected abstract IO<Seq<EngineLight>> Read(RhinoDoc doc);

    protected abstract IO<Unit> Modify(RhinoDoc doc, LightState light);

    protected abstract string Describe(LightShape shape);

    protected virtual Option<Func<RhinoDoc, bool, Guid, IO<Unit>>> Delete => None;

    protected virtual Option<Func<RhinoDoc, bool, Seq<Guid>, IO<Unit>>> Group => None;

    protected virtual Option<Func<RhinoDoc, Seq<Guid>, IO<Unit>>> Edit => None;

    // --- [IDENTITY]
    public sealed override Guid PluginId() => owner.PlugIn.Id;

    public sealed override Guid RenderEngineId() => PluginId();

    // --- [READS]
    public sealed override void GetLights(RhinoDoc doc, ref LightArray light_array) => _ = owner.Run(Appended(doc, light_array));

    public sealed override bool LightFromId(RhinoDoc doc, Guid uuid, ref Light light) => owner.Answer(Filled(doc, uuid, light), failed: false);

    public sealed override int ObjectSerialNumberFromLight(RhinoDoc doc, ref Light light) =>
        owner.Answer(Held(doc, light).Map(static held => unchecked((int)Conversions.Unset(held.Bind(static engine => engine.ObjectSerial)))), failed: 0);

    public sealed override string LightDescription(RhinoDoc doc, ref Light light) =>
        owner.Answer(Held(doc, light).Map(held => Conversions.Unset(held.Map(engine => RowText.Localize(Describe(engine.Light.Spec.Shape), owner).Local))), failed: "");

    private IO<Unit> Appended(RhinoDoc doc, LightArray lights) =>
        from gamma in Lights.PreProcessGamma(doc)
        from held in Read(doc)
        from appended in Callbacks.Each(held.Map(engine =>
            (from staged in use(static () => new Light())
             from written in Staged(staged, engine.Light, gamma)
             from added in IO.lift(() => lights.Append(staged))
             select added).Bracket()))
        select unit;

    private IO<bool> Filled(RhinoDoc doc, Guid id, Light target) =>
        Held(doc, id).Bind(held => held.Match(
            Some: engine => Lights.PreProcessGamma(doc).Bind(gamma => Staged(target, engine.Light, gamma)).Map(value: true),
            None: static () => IO.pure(value: false)));

    private IO<Option<EngineLight>> Held(RhinoDoc doc, Guid id) =>
        Read(doc).Map(held => held.Find(engine => engine.Light.Id == id));

    private IO<Option<EngineLight>> Held(RhinoDoc doc, Light light) =>
        IO.lift(() => light.Id).Bind(id => Held(doc, id));

    private static IO<Unit> Staged(Light target, LightState state, float gamma) =>
        from written in Lights.Written(target, state.Spec, gamma)
        from identified in IO.lift(() => target.Id = state.Id)
        select unit;

    // --- [EDITS]
    public sealed override void ModifyLight(RhinoDoc doc, Light light) => _ = owner.Run(Modified(doc, light));

    public sealed override bool DeleteLight(RhinoDoc doc, Light light, bool bUndelete) =>
        owner.Answer(Delete.Map(delete => Deleted(doc, light, bUndelete, delete).Map(value: true)).IfNone(IO.pure(value: false)), failed: false);

    public sealed override bool OnEditLight(RhinoDoc doc, ref LightArray light_array) =>
        owner.Answer(Requested(light_array, Edit.Map(edit => par(edit, doc))).Map(static edited => edited.Map(value: true)).IfNone(IO.pure(value: false)), failed: false);

    public sealed override void GroupLights(RhinoDoc doc, ref LightArray light_array) =>
        _ = owner.Run(Requested(light_array, Group.Map(group => par(group, doc, b: true))).IfNone(IO.pure(unit)));

    public sealed override void UnGroup(RhinoDoc doc, ref LightArray light_array) =>
        _ = owner.Run(Requested(light_array, Group.Map(group => par(group, doc, b: false))).IfNone(IO.pure(unit)));

    private IO<Unit> Modified(RhinoDoc doc, Light light) =>
        from state in IO.lift(() => Lights.State(light))
        from modified in Modify(doc, state)
        from notified in Notified(doc, LightMangerSupportCustomEvent.light_modified)
        select unit;

    private IO<Unit> Deleted(RhinoDoc doc, Light light, bool undelete, Func<RhinoDoc, bool, Guid, IO<Unit>> delete) =>
        from id in IO.lift(() => light.Id)
        from deleted in delete(doc, undelete, id)
        from notified in Notified(doc, undelete ? LightMangerSupportCustomEvent.light_undeleted : LightMangerSupportCustomEvent.light_deleted)
        select unit;

    private static Option<IO<Unit>> Requested(LightArray lights, Option<Func<Seq<Guid>, IO<Unit>>> request) =>
        request.Map(apply => IO.lift(() => toSeq(Range(0, lights.Count())).Map(index => lights.ElementAt(index).Id).Strict()).Bind(apply));

    // --- [NOTICES]
    internal IO<Unit> Notified(RhinoDoc doc, LightMangerSupportCustomEvent change) =>
        IO.lift(() => {
            Light? none = null;
            OnCustomLightEvent(doc, change, ref none);
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LightManagers {
    public static IO<Unit> Changed<TManager>(IPlugInLifetime owner, RhinoDoc doc, LightMangerSupportCustomEvent change)
        where TManager : DefinedLightManager<TManager>, new() =>
        DefinedLightManager<TManager>.Slot.Of(owner).Bind(manager => manager.Notified(doc, change));
}
