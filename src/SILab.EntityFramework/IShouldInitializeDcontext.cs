namespace SILab.EntityFramework
{
    public interface IShouldInitializeDcontext
    {
        void Initialize(SILabEfDbContextInitializationContext initializationContext);
    }
}
