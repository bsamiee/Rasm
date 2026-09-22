
using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;


namespace Rhino.Arches.Plugin.Commands
{
    public class TestArchCommand : ArchCommandBase
    {
        public TestArchCommand() : base("testArch")
        {
        }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
                
            return Result.Success;
        }
    }
}
