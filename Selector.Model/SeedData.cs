using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Selector.Model.Authorisation;

namespace Selector.Model
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                GetRole(Constants.AdminRole, "00c64c0a-3387-4933-9575-83443fa9092b")
            );
        }

        public static IdentityRole GetRole(string name, string id)
        {
            return new IdentityRole
            {
                Name = name,
                NormalizedName = name.ToUpperInvariant(),
                Id = id
            };
        }
    }
}
