namespace ServiceReports
{
    public interface IGenerateAllUsersReportsService
    {
        Task<bool> Run();
    }
}