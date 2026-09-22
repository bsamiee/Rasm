using Arches;
using Rhino.Arches.Core.HorseShoe;
using Rhino.Arches.Interaction.Constants;
using Rhino.Arches.Interaction.Faactory;
using Rhino.Arches.Interaction.Visualization;
using Rhino.Display;
using Rhino.Input.Custom;


namespace Rhino.Arches.Interaction.Repository
{
    public class HorseShoeArchRepository : InteractionBase, IHorseShoeArchReporsitory
    {
        public ArchBase Create_HorseShoe()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.Rounded);
                    gp.AddOption(Options.Pointed);
                    gp.AddOption(Options.Lobed);
                }

                string startMessage = "Select Horse Shoe Arch Type:";


                var functions = new List<Func<ArchBase>>
            {
                this.Create_Rounded,
                this.Create_Pointed,
                this.Create_Lobed
            };

                var arch = this.StartCommand(startMessage, AddOptions, functions);

                return arch;
            }
            catch
            {
                return null;
            }
        }


        public ArchBase Create_Rounded()
        {
            return this.DrawByStartEndApex((span) =>
            {
                return RoundedArch.Build(span.Start, span.End, span.Apex, span.Normal);
            });
        }



        public ArchBase Create_Pointed()
        {
            return this.DrawByStartEndApex((span) =>
            {
                return PointedArch.Build(span.Start, span.End, span.Apex, span.Normal);
            });

        }


        public ArchBase Create_Lobed()
        {
            return this.DrawByStartAndEnd((span) =>
            {
                var arch = LobedArch.Build(span.Start, span.End, span.Normal);
                return arch;
            });
        }



    }
}
