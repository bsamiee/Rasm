

namespace Arches
{
    public interface IHorseShoeArchReporsitory
    {
        ArchBase Create_HorseShoe();
        ArchBase Create_Rounded();
        ArchBase Create_Pointed();
        ArchBase Create_Lobed();
    }
}
