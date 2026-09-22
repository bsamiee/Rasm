using Arches;
using Rhino.Arches.Core.Circular;
using Rhino.Arches.Interaction.Constants;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Repository
{
    public class CircularArchRepository : InteractionBase, ICircularArchRepository
    {
        // Point3d? _startPt = null;
        public ArchBase Create_Circular()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.SemiCircular);
                    gp.AddOption(Options.Segmental);
                }

                string startMessage = "Select Circular Arch Type:";


                var functions = new List<Func<ArchBase>>
            {
                this.Create_SemiCircular,
                this.Create_Segmental
            };

                var arch = this.StartCommand(startMessage, AddOptions, functions);
                return arch;
            }
            catch
            {
                return null;
            }

        }

        public ArchBase Create_Segmental()
        {
            return this.DrawByStartEndApex((span) =>
        {
            return SegmentalArch.Build(span.Start, span.End, span.Apex, span.Normal);
        });

        }

        public ArchBase Create_SemiCircular()
        {

            return this.DrawByStartAndEnd((span) =>
            {
                var arch = SemiCircularArch.BuildByEndPoints(span.Start, span.End, span.Normal);
                return arch;
            });
        }
    }
}
