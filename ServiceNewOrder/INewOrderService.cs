namespace ServiceNewOrder
{
    public interface INewOrderService
    {
        Task<bool> New(string email, decimal amount);
    }
}