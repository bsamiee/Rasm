using Rhino.Arches.Interaction.Repository;
using Rhino.Commands;
using System;

namespace Rhino.Arches.Plugin.Commands
{
    public class HorseShoeArch_Command : ArchCommandBase
    {
        public HorseShoeArch_Command() : base("horseShoeArch")
        {
            Instance = this;
        }
        public static HorseShoeArch_Command Instance { get; private set; }

        public override Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode)
        {
            var arch = archRepository.HorseShoeArches.Create_HorseShoe();
            if(arch is null)
            {
                return Result.Failure;
            }
            return Result.Success;
        } 
    }
}
