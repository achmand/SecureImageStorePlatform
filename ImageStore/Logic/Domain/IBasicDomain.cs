using Logic.DomainObjects;

namespace Logic.Domain
{
    public interface IBasicDomain<T, in T2>
    {
        DomainResult<T> Add(T2 add);
    }
}
