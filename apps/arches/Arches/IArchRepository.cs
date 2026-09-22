namespace Arches
{
    public interface IArchRepository
    {

        ICircularArchRepository CircularArches { get; }
        IHorseShoeArchReporsitory HorseShoeArches { get; }

        ITwoCenterArchRepository TwoCenterArches { get; }

        IThreeCenteredArchRepository ThreeCenteredArches { get; }

        IFourCenteredArchRepository FourCenteredArches { get; }

        IOgeeArchRepository OgeeArches { get; }

        IMultifoilArchRepository MultifoilArches { get; }

        ICatenaryArchRepository CatenaryArches { get; }

        IFlatArchRepository FlatArches { get; }
    }
}