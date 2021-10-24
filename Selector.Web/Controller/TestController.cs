using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selector.Model;

namespace Selector.Web.Controller {
    
    [ApiController]
    [Route("api/[controller]")]
    public class TestController {

        private readonly ApplicationDbContext db;

        public TestController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> Get()
        {
            // var watchers = ;
            return await db.Users.ToListAsync();
        }
    }
}