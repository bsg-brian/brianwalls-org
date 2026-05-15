namespace BW.Website.Application.Features.SampleWorkOrders.Models
{
	public sealed class SampleWorkOrderDto
	{
		public int Id { get; set; }

		public string WorkOrderNumber { get; set; } = string.Empty;

		public string CustomerName { get; set; } = string.Empty;

		public DateTime CreatedOnUtc { get; set; }

		public DateTime? DueDateUtc { get; set; }

		public string Status { get; set; } = "New";

		public string? Description { get; set; }
	}
}
