using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;

namespace Rhino.Arches.Plugin.Commands
{
    public class CatenaryArch_Command : ArchCommandBase
    {
        public CatenaryArch_Command() : base("catenaryArch")
        {
            Instance = this;
        }

        public static CatenaryArch_Command Instance { get; private set; }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var arch = archRepository.CatenaryArches.Create_Catenary();
            if (arch is null)
            {
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}
