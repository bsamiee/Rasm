using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;

namespace Rhino.Arches.Plugin.Commands
{
    public class OgeeArch_Command : ArchCommandBase
    {
        public OgeeArch_Command() : base("ogeeArch")
        {
            Instance = this;
        }

        public static OgeeArch_Command Instance { get; private set; }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var arch = archRepository.OgeeArches.Create_Ogee();
            if (arch is null)
            {
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}
