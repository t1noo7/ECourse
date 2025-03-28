using AspNetCore.Identity.Mongo.Model;

namespace Demo.Core.Permission
{
    public class Role : MongoRole
    {
        public Role() : base()
        {
        }

        public Role(string name) : base(name)
        {
        }
    }
}