using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;

namespace Rhino.Arches.Plugin.Commands
{
    public class FlatArch_Command : ArchCommandBase
    {
        public FlatArch_Command() : base("flatArch")
        {
            Instance = this;
        }

        public static FlatArch_Command Instance { get; private set; }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var arch = archRepository.FlatArches.Create_Flat();
            if (arch is null)
            {
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}
