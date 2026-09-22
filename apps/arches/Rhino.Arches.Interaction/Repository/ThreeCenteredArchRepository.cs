using Arches;
using Rhino.Arches.Core.ThreeCentered;
using Rhino.Arches.Interaction.Constants;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Repository
{
    public class ThreeCenteredArchRepository : InteractionBase, IThreeCenteredArchRepository
    {

        public ArchBase Create_ThreeCentered()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.Basket_Handle);
                    gp.AddOption(Options.Depressed_Two);
                    gp.AddOption(Options.Pseudo_Elliptical);
                }

                string startMessage = "Select Three Centered Arch Type:";


                var functions = new List<Func<ArchBase>>
                                {
                                    Create_BasketHandleBridge,
                                    Create_Depressed,
                                    Create_Pseudo_Elliptical
                                };

                var arch = this.StartCommand(startMessage, AddOptions, functions);

                return arch;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ArchBase Create_Depressed()
        {

            return this.DrawByStartEndApex((span) =>
            {
                return DepreesedArch.Build(span.Start, span.End, span.Apex, span.Normal);
            });

        }

        public ArchBase Create_BasketHandleBridge()
        {

            return this.DrawByStartAndEnd((span) =>
            {
                var arch = BasketHandleArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_Pseudo_Elliptical()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = PseudoEllipticalArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }
    }
}