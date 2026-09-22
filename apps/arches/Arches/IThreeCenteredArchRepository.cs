//create interface for Three Centered Arch Repository
namespace Arches
{
    public interface IThreeCenteredArchRepository
    {
        ArchBase Create_ThreeCentered();
        ArchBase Create_Depressed();

        ArchBase Create_BasketHandleBridge();

        ArchBase Create_Pseudo_Elliptical();
    }
}