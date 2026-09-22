namespace Arches
{
    public interface IOgeeArchRepository
    {
        ArchBase Create_Ogee();
        ArchBase Create_ReverseOgee();
        ArchBase Create_Tented();
        ArchBase Create_ThreeCenteredOgee();
        ArchBase Create_FourCenteredOgee();
    }
}
