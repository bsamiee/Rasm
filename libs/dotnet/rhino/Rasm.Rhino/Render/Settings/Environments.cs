using System.Globalization;
using Rasm.Rhino.Render.Content;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Settings;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Environments {
    // --- [CURRENT]
    public static IO<Option<RenderEnvironment>> Current(RhinoDoc doc, RenderSettings.EnvironmentUsage usage) =>
        RenderSettingsSources.Read(doc, scope => IO.lift(() => Optional(scope.Settings.RenderEnvironment(usage, RenderSettings.EnvironmentPurpose.ForRendering))));

    // --- [INTENSITY]
    private static readonly Seq<string> GainParameters = ["intensity", "multiplier", "rdk-texture-adjust-multiplier"];

    public static IO<Option<double>> Intensity(RenderEnvironment lighting) =>
        IO.lift(() => Optional(lighting.FirstChild)).Bind(static child => child.Match(Some: Gain, None: static () => IO.pure(Option<(string Parameter, double Gain)>.None)))
            .Map(static held => held.Map(static found => found.Gain));

    public static IO<Unit> SetIntensity(RenderEnvironment lighting, double intensity) =>
        from texture in IO.lift(() => Optional(lighting.FirstChild).ToFin(new RhinoFault($"child of {lighting.Id}", RhinoReason.Missing)))
        from edited in Contents.Edit(texture, RenderContent.ChangeContexts.Program, [Gained(intensity)])
        select unit;

    private static IO<Option<(string Parameter, double Gain)>> Gain(RenderContent texture) =>
        GainParameters.TraverseM(parameter => ContentFields.GetParameter<double>(texture, parameter).Map(gain => gain.Map(held => (parameter, held)))).As()
            .Map(static found => found.Somes().Head);

    private static Step<RenderContent> Gained(double intensity) =>
        new(texture =>
            from held in Gain(texture).Bind(found => IO.lift(found.ToFin(new RhinoFault($"gain parameter of {texture.Id}", RhinoReason.Missing))))
            from written in ContentFields.SetParameter(texture, held.Parameter, intensity)
            select texture, string.Create(CultureInfo.InvariantCulture, $"gain {intensity}"));

    // --- [LIGHTING]
    public static IO<ContentKey> LightingKey(RhinoDoc doc) =>
        from lights in IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Map(static row => (row.Id, Crc: row.LightGeometry.DataCRC(0))).Strict())
        from sky in RenderSettingsSources.Read(doc, static scope =>
            (from sun in use(() => scope.Settings.Sun)
             from skylight in use(() => scope.Settings.Skylight)
             select (Sun: sun.Hash, Skylight: skylight.Enabled, Shadow: skylight.ShadowIntensity)).Bracket())
        from environment in Current(doc, RenderSettings.EnvironmentUsage.Skylighting).Map(static held => held.Map(static lit => lit.RenderHash))
        select ContentKey.Of<RenderEnvironment>(stream => stream
            .Rows(lights, static (row, light) => row.Id(light.Id).Integer(light.Crc))
            .Integer(sky.Sun)
            .Integer(sky.Skylight ? 1 : 0)
            .Number(sky.Shadow)
            .Integer(Conversions.Unset(environment)));

    // --- [PHYSICAL_SKY]
    public static IO<Guid> AddPhysicalSky(RhinoDoc doc, string name, bool useDocumentSun, double intensity) =>
        Products.Transfer(() => RenderContentType.NewContentFromTypeId(ContentUuids.BasicEnvironmentType, doc) as RenderEnvironment, created =>
            from slot in IO.lift(() => created.TextureChildSlotName)
            from named in Contents.Edit(created, RenderContent.ChangeContexts.Program, [
                Steps.Then<RenderContent>(content => {
                    content.SetName(name, renameEvents: false, ensureNameUnique: false);
                    return content;
                }),
                Contents.Child(() => RenderContentType.NewContentFromTypeId(ContentUuids.PhysicalSkyTextureType, doc), slot)])
            from texture in IO.lift(() => Optional(created.FindChild(slot)).ToFin(new RhinoFault($"slot {slot} of {created.Id}", RhinoReason.Missing)))
            from configured in Contents.Edit(texture, RenderContent.ChangeContexts.Program, [
                new Step<RenderContent>(sky => ContentFields.SetParameter(sky, "use-document-sun", useDocumentSun).Map(_ => sky), "use-document-sun"),
                Gained(intensity)])
            from attached in Contents.Attach(doc, created)
            select created.Id);
}
