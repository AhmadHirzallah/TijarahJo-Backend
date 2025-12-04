using API.Requests;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.PersonServices;
using API.Response;

namespace API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private PersonService _personService;

        public UserController(PersonService service /*, User Service*/)
        {
            _personService = service;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest loginRequest)
        {

            var user = LoginRequestBinder.Bind(loginRequest);
            return Ok();
        }

        [HttpGet("details")]
        public ActionResult Details()
        {
            var user = new User { UserName = "exampleUser", Password = "examplePass" };
            var person = _personService.GetPerson(user);
            return Ok(UserReponseBinder.Bind(new User { Email= "hadshdash.com" }, person));
        }


    }
}
