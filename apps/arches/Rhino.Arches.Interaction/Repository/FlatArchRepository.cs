using Arches;
using Rhino.Arches.Core.Flat;
using Rhino.Arches.Interaction.Constants;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Repository
{
    public class FlatArchRepository : InteractionBase, IFlatArchRepository
    {
        public ArchBase Create_Flat()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.FlatArch);
                    gp.AddOption(Options.CamberedJack);
                }

                string startMessage = "Select Flat Arch Type:";

                var functions = new List<Func<ArchBase>>
                {
                    this.Create_FlatArch,
                    this.Create_CamberedJack
                };

                var arch = this.StartCommand(startMessage, AddOptions, functions);
                return arch;
            }
            catch
            {
                return null;
            }
        }

        public ArchBase Create_FlatArch()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = FlatArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_CamberedJack()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = CamberedJackArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }
    }
}
