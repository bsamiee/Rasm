using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;

namespace Arches;

// --- [COMPOSITION] ---------------------------------------------------------------------

internal abstract class ArchesPlugIn : IDefined<PlugInDefinition> {
    public static PlugInDefinition Definition { get; } = new() {
        Commands = toSeq(ArchFamily.Items).Map(static family =>
            (PlugInCommand)new CommandDefinition<CommandInput.Run>(family.Command, new CommandInput.Run((doc, _, _) => ArchPrompts.Run(doc, family)))),
        Toolbars = new CommandFace(nameof(Arches), nameof(Arches)) switch {
            var face => new RuiFile {
                Groups = [new ToolbarGroupRow(face, IterableNE.create(new ToolbarTab(new ToolbarRow(
                    face,
                    from family in toSeq(ArchFamily.Items)
                    select (from choice in toSeq(family.Choices)
                            let label = choice.Face
                            let button = new CommandFace(label.Option, label.Caption) { Help = label.Help, Icon = new IconResource(typeof(ArchesPlugIn).Assembly, $"{label.Option}.svg") }
                            let macro = new Macro(family.Command, [new ScriptStep.Assign(ArchPrompts.Type, new ScriptValue.Named(label.Option))])
                            select (Command<Macro>)new Command<Macro>.Run(button, macro) {
                                Alternate = (
                                    button with { Help = $"{label.Help}, both sides from the middle" },
                                    macro with { Steps = macro.Steps.Add(new ScriptStep.Assign(ArchPrompts.BothSides, new ScriptValue.Named(LabelPair.NoYes.On))) }),
                            })))))],
            },
        },
    };
}
