namespace Arches
{
    public interface ICatenaryArchRepository
    {
        ArchBase Create_Catenary();
        ArchBase Create_Elliptical();
        ArchBase Create_Parabolic();
    }
}
