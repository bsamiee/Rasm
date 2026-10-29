using Rasm.Rhino.Objects;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EngineLight(LightState Light, Option<uint> ObjectSerial);

public sealed record LightSolo(Func<RhinoDoc, Guid, bool, IO<Unit>> Set, Func<RhinoDoc, Guid, IO<bool>> Read, Func<RhinoDoc, IO<int>> Count);

public sealed record LightManagerDefinition(Func<RhinoDoc, IO<Seq<EngineLight>>> Read, Func<RhinoDoc, LightState, IO<Unit>> Modify, Func<LightState, string> Describe) {
    public Option<Func<RhinoDoc, bool, Guid, IO<Unit>>> Delete { get; init; }
    public Option<Func<RhinoDoc, bool, Seq<Guid>, IO<Unit>>> Group { get; init; }
    public Option<Func<RhinoDoc, Seq<Guid>, IO<Unit>>> Edit { get; init; }
    public Option<LightSolo> Solo { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedLightManager<TSelf> : LightManagerSupport where TSelf : DefinedLightManager<TSelf>, IDefined<LightManagerDefinition> {
    // --- [SLOT]
    private static readonly Atom<Option<DefinedLightManager<TSelf>>> Constructed = Atom(Option<DefinedLightManager<TSelf>>.None);

    protected DefinedLightManager() => _ = Constructed.Swap(_ => Some(this));

    public static IO<DefinedLightManager<TSelf>> Present =>
        IO.lift(static () => Constructed.Value.ToFin(new RhinoFault(typeof(TSelf).Name, RhinoReason.Missing)));

    // --- [IDENTITY]
    public sealed override Guid PluginId() => Session.PlugIn.Id;

    public sealed override Guid RenderEngineId() => Session.PlugIn.Id;

    // --- [READS]
    public sealed override void GetLights(RhinoDoc doc, ref LightArray light_array) {
        LightArray lights = light_array;
        _ = Session.Run(
            from gamma in Lights.PreProcessGamma(doc)
            from held in TSelf.Definition.Read(doc)
            from appended in Products.Each(held.Map(engine =>
                (from staged in use(static () => new Light())
                 from written in Staged(staged, engine.Light, gamma)
                 from added in IO.lift(() => lights.Append(written))
                 select added).Bracket()))
            select unit);
    }

    public sealed override bool LightFromId(RhinoDoc doc, Guid uuid, ref Light light) {
        Light target = light;
        return Session.Run(
            Held(doc, uuid).Bind(held => held.Match(
                Some: engine => Lights.PreProcessGamma(doc).Bind(gamma => Staged(target, engine.Light, gamma)).Map(static _ => true),
                None: static () => IO.pure(value: false))),
            failed: false);
    }

    public sealed override int ObjectSerialNumberFromLight(RhinoDoc doc, ref Light light) {
        Guid id = light.Id;
        return Session.Run(Held(doc, id).Map(static held => unchecked((int)Conversions.Unset(held.Bind(static engine => engine.ObjectSerial)))), failed: 0);
    }

    public sealed override string LightDescription(RhinoDoc doc, ref Light light) {
        Guid id = light.Id;
        return Session.Run(Held(doc, id).Map(static held => Conversions.Unset(held.Map(static engine => LocalText.Of(TSelf.Definition.Describe(engine.Light)).Local))), failed: "");
    }

    private static IO<Option<EngineLight>> Held(RhinoDoc doc, Guid id) => TSelf.Definition.Read(doc).Map(held => held.Find(engine => engine.Light.Id == id));

    private static IO<Light> Staged(Light target, LightState state, float gamma) =>
        Lights.Written(state.Spec, gamma).Run(target).Map(written => {
            written.Id = state.Id;
            return written;
        });

    // --- [EDITS]
    public sealed override void ModifyLight(RhinoDoc doc, Light light) =>
        _ = Session.Run(
            from state in IO.lift(() => Lights.State(light))
            from modified in TSelf.Definition.Modify(doc, state)
            from notified in Changed(doc, LightMangerSupportCustomEvent.light_modified)
            select unit);

    public sealed override bool DeleteLight(RhinoDoc doc, Light light, bool bUndelete) =>
        Session.Run(
            TSelf.Definition.Delete.Map(delete =>
                from deleted in delete(doc, bUndelete, light.Id)
                from notified in Changed(doc, bUndelete ? LightMangerSupportCustomEvent.light_undeleted : LightMangerSupportCustomEvent.light_deleted)
                select true).IfNone(IO.pure(value: false)),
            failed: false);

    public sealed override bool OnEditLight(RhinoDoc doc, ref LightArray light_array) =>
        Session.Run(Requested(light_array, TSelf.Definition.Edit.Map(edit => par(edit, doc))).Map(static edited => edited.Map(static _ => true)).IfNone(IO.pure(value: false)), failed: false);

    public sealed override void GroupLights(RhinoDoc doc, ref LightArray light_array) =>
        _ = Session.Run(Grouping(doc, light_array, grouped: true));

    public sealed override void UnGroup(RhinoDoc doc, ref LightArray light_array) =>
        _ = Session.Run(Grouping(doc, light_array, grouped: false));

    private static IO<Unit> Grouping(RhinoDoc doc, LightArray lights, bool grouped) =>
        Requested(lights, TSelf.Definition.Group.Map(group => par(group, doc, grouped))).IfNone(IO.pure(unit));

    private static Option<IO<Unit>> Requested(LightArray lights, Option<Func<Seq<Guid>, IO<Unit>>> request) =>
        request.Map(apply => IO.lift(() => toSeq(Range(0, lights.Count())).Map(index => lights.ElementAt(index).Id).Strict()).Bind(apply));

    // --- [SOLO]
    public sealed override bool SetLightSolo(RhinoDoc doc, Guid uuid_light, bool bSolo) =>
        Session.Run(TSelf.Definition.Solo.Map(solo => solo.Set(doc, uuid_light, bSolo).Map(static _ => true)).IfNone(IO.lift(() => base.SetLightSolo(doc, uuid_light, bSolo))), failed: false);

    public sealed override bool GetLightSolo(RhinoDoc doc, Guid uuid_light) =>
        Session.Run(TSelf.Definition.Solo.Map(solo => solo.Read(doc, uuid_light)).IfNone(IO.lift(() => base.GetLightSolo(doc, uuid_light))), failed: false);

    public sealed override int LightsInSoloStorage(RhinoDoc doc) =>
        Session.Run(TSelf.Definition.Solo.Map(solo => solo.Count(doc)).IfNone(IO.lift(() => base.LightsInSoloStorage(doc))), failed: 0);

    // --- [NOTICES]
    public IO<Unit> Changed(RhinoDoc doc, LightMangerSupportCustomEvent change) =>
        IO.lift(() => {
            Light? none = null;
            OnCustomLightEvent(doc, change, ref none);
        });
}
