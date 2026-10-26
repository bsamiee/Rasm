using Rasm.Imaging.Color;
using Rasm.Rhino.Objects;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EngineLight(LightState Light, Option<uint> ObjectSerial);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedLightManager<TManager> : LightManagerSupport
    where TManager : DefinedLightManager<TManager>, new() {
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
    public sealed override void GetLights(RhinoDoc doc, ref LightArray light_array) =>
        Callbacks.Run(Appended(doc, light_array), CallbackSite.Of(owner, this));

    public sealed override bool LightFromId(RhinoDoc doc, Guid uuid, ref Light light) =>
        Callbacks.Answer(Filled(doc, uuid, light), static () => false, CallbackSite.Of(owner, this));

    public sealed override int ObjectSerialNumberFromLight(RhinoDoc doc, ref Light light) =>
        Callbacks.Answer(
            Held(doc, light).Map(static held => unchecked((int)Conversions.Unset(held.Bind(static engine => engine.ObjectSerial)))),
            static () => 0,
            CallbackSite.Of(owner, this));

    public sealed override string LightDescription(RhinoDoc doc, ref Light light) =>
        Callbacks.Answer(
            Held(doc, light).Map(held => Conversions.Unset(held.Map(engine => RowText.Localize(Describe(engine.Light.Spec.Shape), owner).Local))),
            static () => "",
            CallbackSite.Of(owner, this));

    private IO<Unit> Appended(RhinoDoc doc, LightArray lights) =>
        from transfer in Lights.Gamma(doc)
        from held in Read(doc)
        from appended in Callbacks.Each(held.Map(engine => DisposalOps.Within(static () => new Light(), staged =>
            from written in Staged(staged, engine.Light, transfer)
            from added in IO.lift(() => lights.Append(staged))
            select added)))
        select unit;

    private IO<bool> Filled(RhinoDoc doc, Guid id, Light target) =>
        Held(doc, id).Bind(held => held.Match(
            Some: engine => Lights.Gamma(doc).Bind(transfer => Staged(target, engine.Light, transfer)).Map(value: true),
            None: static () => IO.pure(value: false)));

    private IO<Option<EngineLight>> Held(RhinoDoc doc, Guid id) =>
        Read(doc).Map(held => held.Find(engine => engine.Light.Id == id));

    private IO<Option<EngineLight>> Held(RhinoDoc doc, Light light) =>
        IO.lift(() => light.Id).Bind(id => Held(doc, id));

    private static IO<Unit> Staged(Light target, LightState state, Transfer transfer) =>
        from written in Lights.Written(target, state.Spec, transfer)
        from identified in IO.lift(() => target.Id = state.Id)
        select unit;

    // --- [EDITS]
    public sealed override void ModifyLight(RhinoDoc doc, Light light) =>
        Callbacks.Run(Modified(doc, light), CallbackSite.Of(owner, this));

    public sealed override bool DeleteLight(RhinoDoc doc, Light light, bool bUndelete) =>
        Callbacks.Succeeded(Delete.Map(delete => Deleted(doc, light, bUndelete, delete)), static () => false, CallbackSite.Of(owner, this));

    public sealed override bool OnEditLight(RhinoDoc doc, ref LightArray light_array) =>
        Callbacks.Succeeded(Requested(light_array, Edit.Map(edit => par(edit, doc))), static () => false, CallbackSite.Of(owner, this));

    public sealed override void GroupLights(RhinoDoc doc, ref LightArray light_array) =>
        Callbacks.Run(Requested(light_array, Group.Map(group => par(group, doc, b: true))), CallbackSite.Of(owner, this));

    public sealed override void UnGroup(RhinoDoc doc, ref LightArray light_array) =>
        Callbacks.Run(Requested(light_array, Group.Map(group => par(group, doc, b: false))), CallbackSite.Of(owner, this));

    private IO<Unit> Modified(RhinoDoc doc, Light light) =>
        from transfer in Lights.Gamma(doc)
        from state in IO.lift(() => Lights.State(light, transfer))
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
    internal static readonly Atom<Option<DefinedLightManager<TManager>>> Registered = Atom(Option<DefinedLightManager<TManager>>.None);

    private readonly IPlugInViews owner;

    protected DefinedLightManager() {
        owner = HostedTypes.Owner(typeof(TManager));
        _ = Registered.Swap(_ => Some(this));
    }

    internal IO<Unit> Notified(RhinoDoc doc, LightMangerSupportCustomEvent change) =>
        IO.lift(() => {
            Light? none = null;
            OnCustomLightEvent(doc, change, ref none);
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LightManagers {
    public static IO<Unit> Changed<TManager>(RhinoDoc doc, LightMangerSupportCustomEvent change)
        where TManager : DefinedLightManager<TManager>, new() =>
        IO.lift(static () => DefinedLightManager<TManager>.Registered.Value.ToFin(new Refused(nameof(LightManagerSupport.RegisterLightManager), Refused.Absent)))
            .Bind(manager => manager.Notified(doc, change));
}
