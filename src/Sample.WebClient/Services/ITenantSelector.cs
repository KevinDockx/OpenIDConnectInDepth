namespace Sample.WebClient.Services
{
    public interface ITenantSelector<T> where T:class
    {
        T Current { get; }
        T Select();
    }
}
