using Newtonsoft.Json;

namespace ServiceReports
{
    public class User
    {
        public string Id { get; set; }

        public User(string id)
        {
            Id = id;
        }

        public string GetReportFileName()
        {
            return $"{Id}-report.txt";
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
