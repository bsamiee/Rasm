using System.Runtime.InteropServices;
using Eto.Forms;
using Rasm.Rhino.Display;
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
public sealed class AgXToneMapping() : ParameterEffect<Exposure, float>(Exposure.Neutral, PixelPasses.Formation, static effect => new Stepper(effect)) {
    private sealed class Stepper : ParameterSection<Exposure, float> {
        public Stepper(ParameterEffect<Exposure, float> effect) : base(effect) {
            NumericStepper stepper = new() { MinValue = Exposure.Lower, MaxValue = Exposure.Upper, Increment = Exposure.Step, DecimalPlaces = Exposure.Precision };
            _ = stepper.ValueBinding.Bind(() => (float)effect.Value, value => Write((float)value));
            Content = stepper;
        }
    }
}

[Guid("f2d92349-c684-4276-86dd-587c71634aff")]
[CustomPostEffect(PostEffectType.Late, "Dither", RenderPostEffects.Listed)]
public sealed class OutputDither() : ParameterEffect<Dither, string>(Dither.Triangular, static (dither, _) => IO.pure(dither.Pass), static effect => new Menu(effect)) {
    private sealed class Menu : ParameterSection<Dither, string> {
        public Menu(ParameterEffect<Dither, string> effect) : base(effect) {
            DropDown methods = new();
            foreach (Dither method in Dither.Items)
                methods.Items.Add(method.Key, method.Key);
            _ = methods.SelectedKeyBinding.Bind(() => effect.Value.Key, Write);
            Content = methods;
        }
    }
}
