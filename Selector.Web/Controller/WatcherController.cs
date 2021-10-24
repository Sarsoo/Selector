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
    public class WatchersController {

        private readonly ApplicationDbContext db;

        public WatchersController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Watcher>>> Get()
        {
            // TODO: Authorise
            return await db.Watcher.ToListAsync();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class WatcherController {

        private readonly ApplicationDbContext db;

        public WatcherController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Watcher>> Get(int id)
        {
            // TODO: Implement
            return await db.Watcher.FirstAsync();
        }
    }
}