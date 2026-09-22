using Arches;
using Rhino.Arches.Core.Multifoil;
using Rhino.Arches.Interaction.Constants;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Repository
{
    public class MultifoilArchRepository : InteractionBase, IMultifoilArchRepository
    {
        public ArchBase Create_FoilArches()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.Multifoil);
                    gp.AddOption(Options.Cinquefoil);
                    gp.AddOption(Options.Trefoil);

                }

                string startMessage = "Select Multifoil Arch Type:";

                var functions = new List<Func<ArchBase>>
                {
                    this.Create_Multifoil,
                    this.Create_Cinquefoil,
                    this.Create_Trefoil,

                };

                var arch = this.StartCommand(startMessage, AddOptions, functions);
                return arch;
            }
            catch
            {
                return null;
            }
        }

        public ArchBase Create_Trefoil()
        {
            return this.DrawStaticFoilByStartAndEnd((span) =>
            {
                var arch = TrefoilArch.Build(span.Start, span.End, span.Normal, span.IsPointed);
                return arch;
            });
        }

        public ArchBase Create_Cinquefoil()
        {
            return this.DrawStaticFoilByStartAndEnd((span) =>
            {
                var arch = CinquefoilArch.Build(span.Start, span.End, span.Normal,span.IsPointed);
                return arch;
            });
        }

        public ArchBase Create_Multifoil()
        {
            return this.DrawByStartEndFoilCount((span) =>
            {
                var arch = MultifoilArch.Build(span.Start, span.End, span.Normal, span.IsPointed, span.FoilCount);
                return arch;
            });
        }


    }
}
