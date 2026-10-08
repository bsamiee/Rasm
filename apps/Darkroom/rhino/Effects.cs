using System.Runtime.InteropServices;
using Eto.Forms;
using Rhino.PlugIns;
using Rhino.Render.PostEffects;

[assembly: Guid("b5bac151-510d-4dc3-9f2d-14526e39d793")]

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPlugIn : PlugIn {
    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
}

[Guid("72433230-65e7-4c28-b1a9-0afa4e6f32e2")]
[CustomPostEffect(PostEffectType.ToneMapping, "AgX", RenderPostEffects.Listed)]
public sealed class AgXToneMapping() : ParameterEffect<Exposure, float>(Exposure.Neutral, AgX.Formation, static effect => new Stepper(effect)) {
    private sealed class Stepper : ParameterSection<Exposure, float> {
        public Stepper(ParameterEffect<Exposure, float> effect) : base(effect) {
            NumericStepper stepper = new() { MinValue = Exposure.Lower, MaxValue = Exposure.Upper, Increment = Exposure.Step, DecimalPlaces = Exposure.Precision };
            _ = stepper.ValueBinding.Bind(() => (float)effect.Value, static value => Write((float)value));
            Content = stepper;
        }
    }
}
