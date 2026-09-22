using Arches;
using Rhino.Arches.Core.Catenary;
using Rhino.Arches.Interaction.Constants;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Repository
{
    public class CatenaryArchRepository : InteractionBase, ICatenaryArchRepository
    {
        public ArchBase Create_Catenary()
        {
            try
            {
                static void AddOptions(GetPoint gp)
                {
                    gp.AddOption(Options.Parabolic);
                    gp.AddOption(Options.Elliptical);

                }

                string startMessage = "Select Catenary Arch Type:";

                var functions = new List<Func<ArchBase>>
                {
                                        this.Create_Parabolic,
                    this.Create_Elliptical

                };

                var arch = this.StartCommand(startMessage, AddOptions, functions);
                return arch;
            }
            catch
            {
                return null;
            }
        }

        public ArchBase Create_Elliptical()
        {
            return this.DrawByStartEndApex((span) =>
            {
                var arch = EllipticalArch.Build(span.Start, span.End, span.Apex, span.Normal);
                return arch;
            });
        }

        public ArchBase Create_Parabolic()
        {
            return this.DrawByStartEndApex((span) =>
            {
                var arch = ParabolicArch.Build(span.Start, span.End, span.Apex, span.Normal);
                return arch;
            });
        }
    }
}
