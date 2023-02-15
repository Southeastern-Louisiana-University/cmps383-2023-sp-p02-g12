using Microsoft.AspNetCore.Identity;

namespace SP23.P02.Web.Features
{
    public class User : IdentityUser <int>
    {
       
        public ICollection<UserRole> Roles { get; set; }

    }
}
