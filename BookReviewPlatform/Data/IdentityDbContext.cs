using Microsoft.EntityFrameworkCore;

namespace BookReviewPlatform.Data
{
    public class IdentityDbContext<T>
    {
        private DbContextOptions<ApplicationDbContext> options;

        public IdentityDbContext(DbContextOptions<ApplicationDbContext> options)
        {
            this.options = options;
        }
    }
}