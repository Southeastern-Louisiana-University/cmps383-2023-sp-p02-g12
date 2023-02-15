using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP23.P02.Web.Data;
using SP23.P02.Web.Features;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SP23.P02.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbSet<User> user;
        private readonly DataContext dataContext;
        public UserController(DataContext dataContext)
        {
            this.dataContext = dataContext;
            user = dataContext.Set<User>();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route( "/ api / users")]
        public ActionResult<CreateUserDto> CreateUser(CreateUserDto dto)
        {
            if (IsInvalid(dto))
            {
            return BadRequest();
            }

            var user = new CreateUserDto
            {
                UserName = dto.UserName,
                Password = dto.Password,
            };

            var usertoReturn = new UserDto
            {
                UserName = dto.UserName,
            };
            dataContext.SaveChanges();
            return Ok();

        }

        private bool IsInvalid(CreateUserDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
