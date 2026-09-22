using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;

namespace Rhino.Arches.Plugin.Commands
{
    public class MultifoilArch_Command : ArchCommandBase
    {
        public MultifoilArch_Command() : base("multifoilArch")
        {
            Instance = this;
        }

        public static MultifoilArch_Command Instance { get; private set; }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var arch = archRepository.MultifoilArches.Create_FoilArches();
            if (arch is null)
            {
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}
