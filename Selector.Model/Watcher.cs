using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Selector.Model
{
    public class Watcher
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [Required]
        public ApplicationUser User { get; set; }

        public WatcherType Type { get; set; }
    }
}