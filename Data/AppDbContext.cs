using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Past2Future.Models; 

namespace Past2Future.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        
        public DbSet<Event> Events { get; set; }
        public DbSet<User>Users {get;set;}
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Country> Countries { get; set; }

    }
}