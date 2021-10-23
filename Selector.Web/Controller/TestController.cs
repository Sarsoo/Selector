using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selector.Model;

namespace Selector.Web.Controller {
    
    [ApiController]
    [Route("api/[controller]")]
    public class TestController {

        private readonly SelectorContext db;

        public TestController(SelectorContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Watcher>>> Get()
        {
            // var watchers = ;
            return await db.Watcher.ToListAsync();
        }
    }
}