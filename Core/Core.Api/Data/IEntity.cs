namespace Core.Api.Data
{
    public interface IEntity<out TKey>
        where TKey : notnull
    {
        TKey Key { get; }
    }
}
