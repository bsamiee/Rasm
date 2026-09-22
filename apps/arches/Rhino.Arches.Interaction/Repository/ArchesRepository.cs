using Arches;


namespace Rhino.Arches.Interaction.Repository
{
    public class ArchesRepository : IArchRepository
    {
        private static readonly Lazy<ArchesRepository> _instance = new(() => new ArchesRepository());
        public static ArchesRepository Instance => _instance.Value;

        public ICircularArchRepository CircularArches => new CircularArchRepository();

        public IHorseShoeArchReporsitory HorseShoeArches => new HorseShoeArchRepository();

        public ITwoCenterArchRepository TwoCenterArches => new TwoCenteredArchRepository();

        public IThreeCenteredArchRepository ThreeCenteredArches => new ThreeCenteredArchRepository();

        public IFourCenteredArchRepository FourCenteredArches => new FourCenteredArchRepository();

        public IOgeeArchRepository OgeeArches => new OgeeArchRepository();

        public IMultifoilArchRepository MultifoilArches => new MultifoilArchRepository();

        public ICatenaryArchRepository CatenaryArches => new CatenaryArchRepository();

        public IFlatArchRepository FlatArches => new FlatArchRepository();

        private ArchesRepository() { }


    }
}