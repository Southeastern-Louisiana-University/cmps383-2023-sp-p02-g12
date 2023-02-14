using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP23.P02.Web.Data;
using SP23.P02.Web.Features;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SP23.P02.Web.Controllers
{
    [Route("/")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly DataContext dataContext;
        public AuthenticationController(DataContext dataContext) {
            this.dataContext = dataContext;

        }

        [HttpPost]
        public ActionResult<LoginDto>





    }
}