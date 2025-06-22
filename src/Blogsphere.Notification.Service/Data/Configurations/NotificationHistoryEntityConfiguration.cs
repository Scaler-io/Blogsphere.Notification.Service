using Blogsphere.Notification.Service.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogsphere.Notification.Service.Data.Configurations
{
    public class NotificationHistoryEntityConfiguration : IEntityTypeConfiguration<NotificationHistory>
    {
        public void Configure(EntityTypeBuilder<NotificationHistory> builder)
        {
            builder.ToTable("NotificationHistories");
            builder.HasIndex(x => x.Id).IsUnique();
        }
    }
}