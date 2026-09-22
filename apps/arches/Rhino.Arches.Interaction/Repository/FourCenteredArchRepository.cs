

using Arches;
using Rhino.Arches.Core.FourCentered;
using Rhino.Arches.Interaction.Constants;
using Rhino.Arches.Interaction.Faactory;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Repository
{
    public class FourCenteredArchRepository : InteractionBase, IFourCenteredArchRepository
    {
        public ArchBase Create_FourCentered()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.Persian);
                    gp.AddOption(Options.Tudor);
                    gp.AddOption(Options.Keel);
                }

                string startMessage = "Select Four Centered Arch Type:";

                var functions = new List<Func<ArchBase>>
            {
                this.Create_Persian,
                this.Create_Tudor,
                this.Create_Keel,
            };


                var arch = this.StartCommand(startMessage, AddOptions, functions);
                return arch;
            }
            catch
            {
                return null;
            }
        }

        public ArchBase Create_Keel()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = KeelArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_Persian()
        {

            return this.DrawByStartAndEnd((span) =>
            {
                var arch = PersianArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_Tudor()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = TudorArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }
    }
}
