using Arches;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.PlugIns;

[assembly: RuiFile<ArchesPlugIn>]

namespace Arches;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class ArchesPlugIn : DefinedPlugIn<ArchesPlugIn>, IDefined<PlugInDefinition>, IRuiSource {
    public static PlugInDefinition Definition { get; } = new() {
        LoadTime = PlugInLoadTime.AtStartup,
        Load = [AppSettings.NudgeFollowsActiveDocument(static (resolution, snap, grid) => (snap, 2 * resolution, grid))],
        Commands = toSeq(ArchFamily.Items).Map(static family =>
            (PlugInCommand)new CommandDefinition<CommandInput.Run>(family.Command, new CommandInput.Run((doc, _, _) => ArchPrompts.Run(doc, family)))),
    };

    public static RuiFile Rui { get; } =
        typeof(ArchesPlugIn).Assembly switch {
            var assembly => new RuiFile(
                [new ToolbarRow(
                    new CommandFace(nameof(Arches), nameof(Arches)) { Icon = IconResource.PlugIn(assembly) },
                    from family in toSeq(ArchFamily.Items)
                    select (from choice in toSeq(family.Choices)
                            let option = choice.Option
                            let face = new CommandFace(option, choice.Caption) { Help = choice.Help, Icon = new IconResource(assembly, $"{family.Command}_{option}.svg") }
                            let macro = new Macro(family.Command, [new ScriptStep.Assign(ArchPrompts.Type, option)])
                            select (Command<Macro>)new Command<Macro>.Run(face, macro) {
                                Alternate = (
                                    face with { Help = $"{choice.Help}, both sides from the middle" },
                                    macro with { Steps = macro.Steps.Add(new ScriptStep.Assign(ArchPrompts.BothSides, LabelPair.NoYes.On)) }),
                            }))],
                []),
        };
}
