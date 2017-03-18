namespace DataAccess.Repositories
{  
    public interface IBasicRepository<in T1, out T2>
    {
        T2 Add(T1 newT);
    }
}
