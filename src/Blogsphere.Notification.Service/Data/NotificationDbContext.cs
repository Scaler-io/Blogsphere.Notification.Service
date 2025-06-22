using System.Reflection;
using Blogsphere.Notification.Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blogsphere.Notification.Service.Data
{
    public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
    {
        public DbSet<NotificationHistory> NotificationHistories { get; set; }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            foreach(var entry in ChangeTracker.Entries<EntityBase>())
            {
                switch(entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("blogshpere");
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetEntryAssembly());
            base.OnModelCreating(modelBuilder);
        }

    }
}