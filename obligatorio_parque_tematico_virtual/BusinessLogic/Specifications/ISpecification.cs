namespace BusinessLogic.Specifications
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T candidate);
    }
}
