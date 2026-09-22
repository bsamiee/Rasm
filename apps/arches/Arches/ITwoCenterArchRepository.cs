

namespace Arches
{
    public interface ITwoCenterArchRepository
    {
        ArchBase Create_TwoCentered();

        ArchBase CreateLancet();

        ArchBase CreateEquilateral();

        ArchBase CreateDepressed();
    }
}
