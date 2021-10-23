using System;
using Microsoft.AspNetCore.Mvc;

namespace Selector.Web.Controller {
    
    [ApiController]
    [Route("api/[controller]")]
    public class TestController {

        [HttpGet]
        public ActionResult<string> Get()
        {
            return "Hello World!";
        }
    }
}