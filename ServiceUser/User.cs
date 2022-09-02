using Newtonsoft.Json;

namespace ServiceUser
{
    public class User
    {
        public User(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
