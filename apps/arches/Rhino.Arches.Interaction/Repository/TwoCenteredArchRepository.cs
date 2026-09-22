
using Arches;
using Rhino.Arches.Core.TwoCentered;
using Rhino.Arches.Interaction.Constants;
using Rhino.Arches.Interaction.Faactory;
using Rhino.Arches.Interaction.Visualization;
using Rhino.Display;
using Rhino.Input.Custom;


namespace Rhino.Arches.Interaction.Repository
{
    public class TwoCenteredArchRepository : InteractionBase, ITwoCenterArchRepository
    {
        public ArchBase Create_TwoCentered()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.Equilateral);
                    gp.AddOption(Options.Lancet);
                    gp.AddOption(Options.Depressed_Two);
                }

                string startMessage = "Select Gothic Arch Type:";


                var functions = new List<Func<ArchBase>>
            {
                // add functions to draw each arch type
                this.CreateEquilateral,
                this.CreateLancet,
                this.CreateDepressed
            };
                var arch = this.StartCommand(startMessage, AddOptions, functions);

                return arch;
            }
            catch
            {

                return null;
            }
        }


        public ArchBase CreateDepressed()
        {

            return this.DrawByStartEndApex((span) =>
            {
                return DepressedArch.Build(span.Start, span.End, span.Apex, span.Normal);
            });

        }

        public ArchBase CreateEquilateral()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = EquilateralArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });

        }

        public ArchBase CreateLancet()
        {

            return this.DrawByStartEndApex((span) =>
            {
                return LancetArch.Build(span.Start, span.End, span.Apex, span.Normal);
            });
        }


    }
}
