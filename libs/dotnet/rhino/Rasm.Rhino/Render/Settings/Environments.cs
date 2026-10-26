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
    public static IO<Option<double>> Intensity(RenderEnvironment lighting) =>
        IO.lift(() => Optional(lighting.FirstChild))
            .Bind(static child => child.Match(Some: Held, None: static () => IO.pure(Option<(string Parameter, double Gain)>.None)))
            .Map(static held => held.Map(static found => found.Gain));

    public static IO<Unit> SetIntensity(RenderEnvironment lighting, double intensity) =>
        from texture in IO.lift(() => Refused.UnlessFound(lighting.FirstChild, nameof(RenderContent.FirstChild)))
        from written in Contents.WithinContentChange(texture, RenderContent.ChangeContexts.Program, Gain(texture, intensity))
        select written;

    private static IO<Option<(string Parameter, double Gain)>> Held(RenderContent texture) =>
        Seq("intensity", "multiplier", "rdk-texture-adjust-multiplier")
            .TraverseM(parameter => ContentFields.GetParameter(texture, parameter, value => Conversions.Typed<double>(value, parameter))
                .Map(gain => gain.Map(held => (Parameter: parameter, Gain: held))))
            .As()
            .Map(static found => found.Somes().Head);

    private static IO<Unit> Gain(RenderContent texture, double intensity) =>
        from held in Held(texture).Bind(static found => IO.lift(found.ToFin(new Refused(nameof(RenderContent.GetParameter), Refused.Absent))))
        from written in ContentFields.SetParameter(texture, held.Parameter, intensity)
        select written;

    // --- [PHYSICAL_SKY]
    public static IO<Guid> AddPhysicalSky(RhinoDoc doc, string name, bool useDocumentSun, double intensity) =>
        DisposalOps.Within<IDisposable, Guid>(retain =>
            from created in Contents.FreeFloating(doc, ContentUuids.BasicEnvironmentType, retain)
            from environment in IO.lift(Conversions.Cast<RenderEnvironment>(created.Content))
            from slot in IO.lift(() => environment.TextureChildSlotName)
            from applied in Contents.Apply(doc, environment, RenderContent.ChangeContexts.Program, [
                new ContentEdit.Set((content, _) => content.SetName(name, renameEvents: false, ensureNameUnique: false)),
                new ContentEdit.SetChild(ContentUuids.PhysicalSkyTextureType, slot)])
            from texture in IO.lift(() => Optional(environment.FindChild(slot)).ToFin(new NotFound("child content", $"slot {slot} of {environment.Id}")))
            from configured in Contents.WithinContentChange(texture, RenderContent.ChangeContexts.Program,
                ContentFields.SetParameter(texture, "use-document-sun", useDocumentSun).Bind(_ => Gain(texture, intensity)))
            from attached in Contents.Attach(doc, created)
            select environment.Id, DisposalOps.Release);
}
