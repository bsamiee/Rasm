
using Rhino.Arches.Interaction.Repository;
using Rhino.Arches.Plugin.Constants;
using Rhino.Commands;

namespace Rhino.Arches.Plugin
{
    public abstract class ArchCommandBase : Command
    {
     

        private readonly string _commandName;
        protected readonly ArchesRepository _archRepository = ArchesRepository.Instance;

        protected ArchCommandBase(string commandName)
        {
            _commandName = $"{CommandConstants.CommandPrefix}{commandName}";

        }

        public override string EnglishName => _commandName;

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var result = this.ExecuteCommand(_archRepository, doc, mode);
            return result;
        }

        public abstract Result ExecuteCommand(ArchesRepository archRepository, RhinoDoc doc, RunMode mode);
    }
}
