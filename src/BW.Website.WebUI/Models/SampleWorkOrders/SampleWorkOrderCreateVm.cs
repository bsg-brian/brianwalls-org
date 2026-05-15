using System.ComponentModel.DataAnnotations;

namespace BW.Website.WebUI.Models.SampleWorkOrders;

public sealed class SampleWorkOrderCreateVm
{
	[Required, StringLength(50)]
	public string WorkOrderNumber { get; set; } = string.Empty;

	[Required, StringLength(200)]
	public string CustomerName { get; set; } = string.Empty;

	public DateTime? DueDateUtc { get; set; }

	[StringLength(2000)]
	public string? Description { get; set; }
}
