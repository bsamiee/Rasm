using Rasm.Rhino.Commands;
using Rasm.TestKit;
using Rhino.Input.Custom;
using Color = System.Drawing.Color;

namespace Rasm.Rhino.Tests.Commands;

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class CommandOptionScriptLaws {
    [Fact]
    public void ScriptedToggleTextColorAndChoiceProjectTypedValues() {
        Spec.Some(CommandOption.Of(name: "Enabled", value: true).Script(token: "Enabled=No"), value => {
            Assert.Equal(expected: CommandLineOptionType.Toggle, actual: value.OptionType);
            Spec.Some(value.As<bool>(), actual => Assert.False(condition: actual));
        });
        Spec.Some(CommandOption.Of(name: "Name", value: "", policy: new CommandOptionPolicy(AllowEmpty: true)).Script(token: "Name=panel"), value =>
            Spec.Some(value.As<string>(), actual => Assert.Equal(expected: "panel", actual: actual)));
        Spec.Some(CommandOption.Of(name: "Tint", value: Color.Black).Script(token: "Tint=#ff00aa"), value =>
            Spec.Some(value.As<Color>(), actual => Assert.Equal(expected: Color.FromArgb(red: 255, green: 0, blue: 170), actual: actual)));
        Spec.Some(CommandOption.Choice(name: "Mode", values: ["Add", "Move", "Delete"], label: static value => value).Script(token: "Mode=Move"), value => {
            Spec.Some(value.As<string>(), actual => Assert.Equal(expected: "Move", actual: actual));
            Spec.Some(value.ListIndex, actual => Assert.Equal(expected: 1, actual: actual));
        });
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
    public void ScriptChoosesTheFirstMatchingOptionAcrossAValidatedSet() {
        Seq<CommandOption> options = Seq(
            CommandOption.Of(name: "Enabled", value: true),
            CommandOption.Choice(name: "Mode", values: ["Add", "Move"], label: static value => value));

        Spec.Some(CommandOption.Script(options: options, token: "Mode=Move"), value => {
            Assert.Equal(expected: "Mode", actual: value.Key);
            Spec.Some(value.ListIndex, actual => Assert.Equal(expected: 1, actual: actual));
        });
    }

    [Fact]
    public void ScriptedChoicesCannotBypassInvalidDeclarationState() =>
        Spec.None(CommandOption.Choice(name: "Mode", values: ["Add", "Move"], label: static value => value, policy: new CommandOptionPolicy(Current: 7)).Script(token: "Mode=Move"));
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
