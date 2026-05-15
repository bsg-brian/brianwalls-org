using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BW.Website.Domain.Entities;

namespace BW.Website.Infrastructure.Configurations;

public sealed class SampleWorkOrderConfiguration : IEntityTypeConfiguration<SampleWorkOrder>
{
	public void Configure(EntityTypeBuilder<SampleWorkOrder> builder)
	{
		builder.ToTable("SampleWorkOrders");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.WorkOrderNumber)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(x => x.CustomerName)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(x => x.Status)
			.IsRequired()
			.HasMaxLength(50)
			.HasDefaultValue("New");

		builder.Property(x => x.Description)
			.HasMaxLength(2000);

		builder.Property(x => x.CreatedOnUtc)
			.IsRequired();

		builder.Property(x => x.DueDateUtc);
	}
}
