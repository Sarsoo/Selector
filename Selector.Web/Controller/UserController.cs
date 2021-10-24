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
    public class UsersController {

        private readonly ApplicationDbContext db;

        public UsersController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> Get(string username)
        {
            // TODO: Authorise
            return await db.Users.ToListAsync();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserController {

        private readonly ApplicationDbContext db;

        public UserController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<ApplicationUser>> Get(string username)
        {
            // TODO: Implement
            return await db.Users.SingleAsync();
        }
    }
}