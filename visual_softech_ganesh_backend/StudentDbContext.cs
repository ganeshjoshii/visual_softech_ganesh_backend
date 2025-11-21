using Microsoft.EntityFrameworkCore;
using visual_softech_ganesh_backend.Models;

namespace visual_softech_ganesh_backend
{
    public class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
    {
        
            public DbSet<User> User { get; set; }
        
    }
}
