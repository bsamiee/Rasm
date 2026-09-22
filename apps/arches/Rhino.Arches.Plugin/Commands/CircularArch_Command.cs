
using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;

namespace Rhino.Arches.Plugin.Commands
{
    public class CircularArch_Command : ArchCommandBase
    {
        public CircularArch_Command() : base("circularArch")
        {
           Instance = this;
        }

        public static CircularArch_Command Instance { get; private set; }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var archBase = archRepository.CircularArches.Create_Circular();
            if(archBase is null)
            {
                return Result.Failure;
            }


            return Result.Success;
        }
    }
}
