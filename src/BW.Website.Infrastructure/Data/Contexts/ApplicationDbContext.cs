using Microsoft.EntityFrameworkCore;
using BW.Website.Domain.Entities;

namespace BW.Website.Infrastructure.Data.Contexts;

public sealed class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<SampleWorkOrder> SampleWorkOrders => Set<SampleWorkOrder>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Applies IEntityTypeConfiguration<> implementations from this assembly
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
