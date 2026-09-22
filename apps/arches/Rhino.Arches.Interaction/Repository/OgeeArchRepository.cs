using Arches;
using Rhino.Arches.Core.Ogee;
using Rhino.Arches.Interaction.Constants;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Repository
{
    public class OgeeArchRepository : InteractionBase, IOgeeArchRepository
    {
        public ArchBase Create_Ogee()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.ThreeCenteredOgee);
                    gp.AddOption(Options.ReverseOgee);
                    gp.AddOption(Options.Tented);
                    gp.AddOption(Options.FourCenteredOgee);
                }

                string startMessage = "Select Ogee Arch Type:";

                var functions = new List<Func<ArchBase>>
                {
                    this.Create_ThreeCenteredOgee,
                    this.Create_ReverseOgee,
                    this.Create_Tented,
                    this.Create_FourCenteredOgee
                };

                var arch = this.StartCommand(startMessage, AddOptions, functions);
                return arch;
            }
            catch
            {
                return null;
            }
        }

        public ArchBase Create_ReverseOgee()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = ReverseOgeeArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_Tented()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = TentedArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_ThreeCenteredOgee()
        {
            return this.DrawByStartEndApex((span) =>
            {
                var arch = ThreeCenteredOgeeArch.Build(span.Start, span.End, span.Apex, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_FourCenteredOgee()
        {
            return this.DrawByStartEndApex((span) =>
            {
                var arch = FourCenteredOgeeArch.Build(span.Start, span.End, span.Apex, span.Normal);
                return arch;
            });
        }
    }
}
