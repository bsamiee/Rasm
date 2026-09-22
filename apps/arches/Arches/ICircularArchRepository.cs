
namespace Arches
{
    public interface ICircularArchRepository
    {
        ArchBase Create_SemiCircular();
        ArchBase Create_Segmental();
        ArchBase Create_Circular();
    }
}
