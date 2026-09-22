
using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;


namespace Rhino.Arches.Plugin.Commands
{
    public class ThreeCenteredArch_Command : ArchCommandBase
    {
        public ThreeCenteredArch_Command() : base("threeCenteredArch")
        {
        }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var arch = archRepository.ThreeCenteredArches.Create_ThreeCentered();
            if (arch is null)
            {
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}
