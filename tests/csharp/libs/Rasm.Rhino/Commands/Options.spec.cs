using Rasm.Rhino.Commands;
using Rasm.TestKit;
using Rhino.Input.Custom;
using Color = System.Drawing.Color;

namespace Rasm.Rhino.Tests.Commands;

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class CommandOptionScriptLaws {
    [Fact]
    public void ScriptedToggleTextColorAndNumberProjectTypedValues() {
        Spec.Some(CommandOption.Of(name: "Enabled", value: true).Script(token: "Enabled=No"), value => {
            Assert.Equal(expected: CommandLineOptionType.Toggle, actual: value.OptionType);
            Spec.Some(value.As<bool>(), actual => Assert.False(condition: actual));
        });
        Spec.Some(CommandOption.Of(name: "Name", value: "", policy: new CommandOptionPolicy(AllowEmpty: true)).Script(token: "Name=panel"), value =>
            Spec.Some(value.As<string>(), actual => Assert.Equal(expected: "panel", actual: actual)));
        Spec.Some(CommandOption.Of(name: "Tint", value: Color.Black).Script(token: "Tint=#ff00aa"), value =>
            Spec.Some(value.As<Color>(), actual => Assert.Equal(expected: Color.FromArgb(red: 255, green: 0, blue: 170), actual: actual)));
        Spec.Some(CommandOption.Of(name: "Count", value: 2).Script(token: "Count=3"), value =>
            Spec.Some(value.As<int>(), actual => Assert.Equal(expected: 3, actual: actual)));
    }

    [Fact]
    public void ScriptedNumbersRespectBoundsAndInvalidTokensFail() {
        CommandOption bounded = CommandOption.Of(name: "Count", value: 2, policy: new CommandOptionPolicy(Lower: Some(1.0), Upper: Some(4.0)));
        Spec.Some(bounded.Script(token: "Count=3"), value => Spec.Some(value.As<int>(), actual => Assert.Equal(expected: 3, actual: actual)));
        Spec.None(bounded.Script(token: "Count=5"));
        Spec.None(CommandOption.Of(name: "Enabled", value: true).Script(token: "Other=Yes"));
    }

    [Fact]
    public void ScriptParserAcceptsBooleanAliasesColorFormsAndColonSeparator() {
        CommandOption toggle = CommandOption.Of(name: "Enabled", value: false);
        Spec.Some(toggle.Script(token: "Enabled:on"), value => Spec.Some(value.As<bool>(), Assert.True));
        Spec.Some(toggle.Script(token: "Enabled=0"), value => Spec.Some(value.As<bool>(), Assert.False));
        Spec.Some(CommandOption.Of(name: "Tint", value: Color.Black).Script(token: "Tint=255,0,170"), value =>
            Spec.Some(value.As<Color>(), actual => Assert.Equal(expected: Color.FromArgb(red: 255, green: 0, blue: 170), actual: actual)));
        Spec.Some(CommandOption.Of(name: "Tint", value: Color.Black).Script(token: "Tint=Red"), value =>
            Spec.Some(value.As<Color>(), actual => Assert.Equal(expected: Color.Red.ToArgb(), actual: actual.ToArgb())));
    }

    [Fact]
    public void ScriptChoosesTheFirstMatchingPureOptionAcrossAValidatedSet() {
        Seq<CommandOption> options = Seq(
            CommandOption.Of(name: "Enabled", value: true),
            CommandOption.Of(name: "Name", value: "", policy: new CommandOptionPolicy(AllowEmpty: true)));

        Spec.Some(CommandOption.Script(options: options, token: "Name=Panel"), value => {
            Assert.Equal(expected: "Name", actual: value.Key);
            Spec.Some(value.As<string>(), actual => Assert.Equal(expected: "Panel", actual: actual));
        });
    }
}

public sealed class CommandOptionProjectionLaws {
    [Fact]
    public void ScriptedValuesPreserveNameAndDisplayTextContracts() =>
        Spec.Cases(
            items: [
                CommandOption.Of(name: "Enabled", value: true).Script(token: "Enabled=No"),
                CommandOption.Of(name: "Name", value: "", policy: new CommandOptionPolicy(AllowEmpty: true)).Script(token: "Name=panel"),
                CommandOption.Of(name: "Tint", value: Color.Black).Script(token: "Tint=#ff00aa"),
            ],
            key: value => value.Match(Some: static option => option.Name, None: static () => ""),
            law: value => Spec.Some(value, option => {
                Assert.NotEmpty(option.Name);
                Spec.Some(option.EnglishName, Assert.NotEmpty);
                Assert.NotEqual(expected: CommandLineOptionType.Hidden, actual: option.OptionType);
            }));

}
