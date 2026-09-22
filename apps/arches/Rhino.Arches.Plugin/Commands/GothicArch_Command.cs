using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;

namespace Rhino.Arches.Plugin.Commands
{
    public class GothicArchCommand : ArchCommandBase
    {
        public GothicArchCommand() : base("GothicArch")
        {
        }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
           var arch = archRepository.TwoCenterArches.Create_TwoCentered();

            if (arch is not null)
            {
                return Result.Success;
            }

            return Result.Failure;
        }
    }
}
