using System.Text.Json.Serialization;
using SkiaSharp;
using Thinktecture.Text.Json.Serialization;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class Role {
    public static readonly Role Window = new("window", Family.Chrome);
    public static readonly Role Panel = new("panel", Family.Chrome);
    public static readonly Role Toolbar = new("toolbar", Family.Chrome);
    public static readonly Role Rail = new("rail", Family.Chrome);
    public static readonly Role Popover = new("popover", Family.Chrome);
    public static readonly Role Toast = new("toast", Family.Chrome);
    public static readonly Role Tab = new("tab", Family.Chrome);
    public static readonly Role Button = new("button", Family.Chrome);
    public static readonly Role Field = new("field", Family.Chrome);
    public static readonly Role ListRow = new("list-row", Family.Chrome);
    public static readonly Role SectionHeader = new("section-header", Family.Chrome);
    public static readonly Role Well = new("well", Family.Well);
    public static readonly Role Readout = new("readout", Family.Chrome);
    public static readonly Role HudCapsule = new("hud-capsule", Family.Chrome);

    public Family Family { get; }
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class InteractionState {
    public static readonly InteractionState Rest = new("rest", static look => look);
    public static readonly InteractionState Hover = new("hover", static look => Washed(look, 0.08f));
    public static readonly InteractionState Pressed = new("pressed", static look => Washed(look, 0.1f));
    public static readonly InteractionState Selected = new("selected", static look => look with { StateLayer = new Ink.Themed(ColorRole.Selection, 1f) });
    public static readonly InteractionState Disabled = new("disabled", static look => look with {
        Fill = look.Fill.Faded(0.4f),
        Border = look.Border.Map(static border => border with { Ink = border.Ink.Faded(0.4f) }),
        Ink = new Ink.Themed(ColorRole.Disabled, 1f),
    });
    public static readonly InteractionState Focused = new("focused", static look => Washed(look, 0.1f) with { Border = new Border(new Ink.Themed(ColorRole.Accent, 1f), 2f) });
    public static readonly InteractionState Dragging = new("dragging", static look => Washed(look, 0.16f));

    [UseDelegateFromConstructor]
    public partial Look Derive(Look rest);

    private static Look Washed(Look look, float alpha) => look with { StateLayer = look.Ink.Faded(alpha) };
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class Glass {
    public static readonly Glass Regular = new("regular");
    public static readonly Glass Clear = new("clear");
}

public sealed record Border(Ink Ink, float Width);

public sealed record Stop(Ink Ink, float At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Material {
    public abstract Material Faded(float alpha);

    public sealed record Solid(Ink Ink) : Material {
        public override Material Faded(float alpha) => new Solid(Ink.Faded(alpha));
    }

    public sealed record Gradient(Seq<Stop> Stops, float Angle) : Material {
        public override Material Faded(float alpha) => this with { Stops = Stops.Map(stop => stop with { Ink = stop.Ink.Faded(alpha) }) };
    }

    public sealed record Drawn : Material {
        private Drawn(string shader, HashMap<string, Ink> inks) => (Shader, Inks) = (shader, inks);

        public string Shader { get; }
        public HashMap<string, Ink> Inks { get; private init; }

        public static Fin<Material> Of(string shader, HashMap<string, Ink> inks) {
            using SKRuntimeEffect? effect = SKRuntimeEffect.CreateShader(shader, out string errors);
            return Optional(effect).ToFin(Error.New($"drawn shader fails to compile: {errors}"))
                .Bind(compiled => Seq("size").Concat(inks.Keys).Find(name => !compiled.Uniforms.Contains(name, StringComparer.Ordinal)).Match<Fin<Material>>(
                    Some: static name => Error.New($"drawn shader declares no uniform {name}"),
                    None: () => new Drawn(shader, inks)));
        }

        public override Material Faded(float alpha) => this with { Inks = Inks.Map(ink => ink.Faded(alpha)) };
    }

    public sealed record Backdrop(Glass Glass, Ink Fallback) : Material {
        public override Material Faded(float alpha) => this with { Fallback = Fallback.Faded(alpha) };
    }
}

public sealed record Look(Material Fill, Option<Border> Border, Radius Radius, Elevation Elevation, Ink Ink, TypeRole Type, Duration Motion, Option<Ink> StateLayer);

public sealed record Appearance(string Name, HashMap<(Role Role, InteractionState State), Look> Looks);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Appearances {
    // --- [SHIPPED]
    private static readonly Shipped File = Shipped.Read().ThrowIfFail();

    public static Appearance Native { get; } = File.Native;
    public static Appearance Glass { get; } = File.Named["glass"];
    public static Appearance Drawn { get; } = File.Named["drawn"];
    public static Appearance Capsule { get; } = File.Named["capsule"];

    public static Look Rest(Role role) => File.Rests[role];

    public static Palette Palette(Family family) => File.Palettes[family];

    // --- [RESOLUTION]
    public static Look Resolve(Seq<Appearance> chain, Role role, InteractionState state) =>
        chain.Add(Native).Choose(appearance => appearance.Looks.Find((role, state))).Head
            .IfNone(() => state.Derive(chain.Choose(appearance => appearance.Looks.Find((role, InteractionState.Rest))).Head.IfNone(() => Rest(role))));
}
