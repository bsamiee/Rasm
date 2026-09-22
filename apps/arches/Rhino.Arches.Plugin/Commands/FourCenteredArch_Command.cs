
using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;

namespace Rhino.Arches.Plugin.Commands
{
    public class FourCenteredArch_Command : ArchCommandBase
    {
        public FourCenteredArch_Command() : base("fourCentered")
        {
            Instance = this;
        }

        public static FourCenteredArch_Command Instance { get; private set; }
        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var archBase = archRepository.FourCenteredArches.Create_FourCentered();
            if (archBase is null)
            {
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}
