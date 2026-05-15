using Mediator;
using Microsoft.AspNetCore.Mvc;
using BW.Website.Application.Features.SampleWorkOrders.Commands;
using BW.Website.WebUI.Models;
using System.Diagnostics;

namespace BW.Website.WebUI.Controllers;

public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;
	private readonly IMediator _mediator;

	public HomeController(ILogger<HomeController> logger, IMediator mediator)
	{
		_logger = logger;
		_mediator = mediator;
	}

	public IActionResult Index()
	{
		return View();
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}

	#region Error Handling Test Endpoints

	public IActionResult TestException()
	{
		throw new Exception("This is a test exception from HomeController.");
	}

	public IActionResult TestValidationException()
	{
		var failures = new List<FluentValidation.Results.ValidationFailure>
		{
			new("Name", "Name is required."),
			new("Age", "Age must be over 18.")
		};

		throw new FluentValidation.ValidationException(failures);
	}

	public async Task<IActionResult> TestMediatRValidation()
	{
		// Assuming CreateSampleWorkOrderCommand requires:
		// WorkOrderNumber and CustomerName to be non-empty.
		var cmd = new CreateSampleWorkOrderCommand
		{
			WorkOrderNumber = "",  // invalid on purpose
			CustomerName = "",     // invalid on purpose
			Description = "This will not pass validation."
		};

		var result = await _mediator.Send(cmd);

		// You won't reach this line if validation fails
		return Ok(result);
	}

	#endregion
}
