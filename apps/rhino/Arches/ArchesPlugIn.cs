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
                    new CommandFace(nameof(Arches), nameof(Arches)) { Icon = Icons.PlugInImage(assembly) },
                    toSeq(ArchFamily.Items).Map(family => toSeq(family.Choices).Map(choice =>
                        (Face: new CommandFace(choice.Option, choice.Caption) { Help = choice.Help, Icon = new IconResource(assembly, $"{family.Command}_{choice.Option}.svg") },
                            Macro: new Macro(family.Command, [new ScriptStep.Assign(ArchPrompts.Type, choice.Option)])) switch {
                            var press => (Command<Macro>)new Command<Macro>.Run(press.Face, press.Macro) {
                                Alternate = (
                                    press.Face with { Help = $"{choice.Help}, both sides from the middle" },
                                    press.Macro with { Steps = press.Macro.Steps.Add(new ScriptStep.Assign(ArchPrompts.BothSides, LabelPair.NoYes.On)) }),
                            },
                        })))],
                []),
        };
}
