namespace ServiceNewOrder
{
    public interface IOrdersDatabase
    {
        Task<bool> SaveNewAsync(Order order);
    }
}