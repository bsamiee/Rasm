using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;

namespace Arches;

// --- [COMPOSITION] ---------------------------------------------------------------------

internal abstract class ArchesPlugIn : IDefined<PlugInDefinition>, IRuiSource {
    public static PlugInDefinition Definition { get; } = new() {
        Commands = toSeq(ArchFamily.Items).Map(static family =>
            (PlugInCommand)new CommandDefinition<CommandInput.Run>(family.Command, new CommandInput.Run((doc, _, _) => ArchPrompts.Run(doc, family)))),
    };

    public static RuiFile Rui { get; } = new() {
        Toolbars = [new ToolbarRow(
            new CommandFace(nameof(Arches), nameof(Arches)),
            from family in toSeq(ArchFamily.Items)
            select (from choice in toSeq(family.Choices)
                    let option = choice.Option
                    let face = new CommandFace(option, choice.Switch(plain: static held => held.Arch.Caption, foiled: static held => held.Arch.Caption)) { Help = choice.Help, Icon = new IconResource(typeof(ArchesPlugIn).Assembly, $"{family.Command}_{option}.svg") }
                    let macro = new Macro(family.Command, [new ScriptStep.Assign(ArchPrompts.Type, new ScriptValue.Word(ScriptWord.Named, option))])
                    select (Command<Macro>)new Command<Macro>.Run(face, macro) {
                        Alternate = (
                            face with { Help = $"{choice.Help}, both sides from the middle" },
                            macro with { Steps = macro.Steps.Add(new ScriptStep.Assign(ArchPrompts.BothSides, new ScriptValue.Word(ScriptWord.Named, LabelPair.NoYes.On))) }),
                    }))],
    };
}
