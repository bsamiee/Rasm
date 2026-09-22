namespace Arches
{
    public interface IMultifoilArchRepository
    {
        ArchBase Create_FoilArches();
        ArchBase Create_Multifoil();
        ArchBase Create_Trefoil();
        ArchBase Create_Cinquefoil();
    }
}
