using Mediator;
using Microsoft.AspNetCore.Mvc;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Commands;
using BW.Website.Application.Features.SampleWorkOrders.Queries;
using BW.Website.WebUI.Models.SampleWorkOrders;

namespace BW.Website.WebUI.Controllers;

public sealed class SampleWorkOrdersController : Controller
{
	private readonly IMediator _mediator;

	public SampleWorkOrdersController(IMediator mediator)
	{
		_mediator = mediator;
	}

	// MVC: list page (hosts Vue island)
	[HttpGet]
	public IActionResult Index()
	{
		return View();
	}

	// Vue: JSON endpoint used by island
	[HttpGet]
	public async Task<IActionResult> ListJson(CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(new GetSampleWorkOrdersQuery(), cancellationToken);

		if (result.IsFailure)
		{
			return StatusCode(500, new { error = result.Error.Message });
		}

		return Json(result.Value);
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View(new SampleWorkOrderCreateVm());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(SampleWorkOrderCreateVm vm, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid)
			return View(vm);

		var result = await _mediator.Send(new CreateSampleWorkOrderCommand
		{
			WorkOrderNumber = vm.WorkOrderNumber,
			CustomerName = vm.CustomerName,
			DueDateUtc = vm.DueDateUtc,
			Description = vm.Description
		}, cancellationToken);

		if (result.IsFailure)
		{
			AddErrorsToModelState(result.Error);
			return View(vm);
		}

		return RedirectToAction(nameof(Details), new { id = result.Value.Id });
	}

	[HttpGet]
	public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(new GetSampleWorkOrderByIdQuery { Id = id }, cancellationToken);

		if (result.IsFailure)
		{
			if (result.Error.Code.EndsWith(".NotFound"))
				return NotFound();

			return StatusCode(500);
		}

		return View(result.Value);
	}

	[HttpGet]
	public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(new GetSampleWorkOrderByIdQuery { Id = id }, cancellationToken);

		if (result.IsFailure)
		{
			if (result.Error.Code.EndsWith(".NotFound"))
				return NotFound();

			return StatusCode(500);
		}

		var item = result.Value;
		var vm = new SampleWorkOrderEditVm
		{
			Id = item.Id,
			WorkOrderNumber = item.WorkOrderNumber,
			CustomerName = item.CustomerName,
			DueDateUtc = item.DueDateUtc,
			Status = item.Status,
			Description = item.Description
		};

		return View(vm);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(SampleWorkOrderEditVm vm, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid)
			return View(vm);

		var result = await _mediator.Send(new UpdateSampleWorkOrderCommand
		{
			Id = vm.Id,
			WorkOrderNumber = vm.WorkOrderNumber,
			CustomerName = vm.CustomerName,
			DueDateUtc = vm.DueDateUtc,
			Status = vm.Status,
			Description = vm.Description
		}, cancellationToken);

		if (result.IsFailure)
		{
			if (result.Error.Code.EndsWith(".NotFound"))
				return NotFound();

			AddErrorsToModelState(result.Error);
			return View(vm);
		}

		return RedirectToAction(nameof(Details), new { id = result.Value.Id });
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(new DeleteSampleWorkOrderCommand { Id = id }, cancellationToken);

		if (result.IsFailure)
		{
			if (result.Error.Code.EndsWith(".NotFound"))
				return NotFound();

			TempData["Error"] = result.Error.Message;
		}

		return RedirectToAction(nameof(Index));
	}

	/// <summary>
	/// Adds validation errors from a Result.Error to ModelState.
	/// </summary>
	private void AddErrorsToModelState(Error error)
	{
		if (error.ValidationErrors is not null)
		{
			foreach (var (propertyName, messages) in error.ValidationErrors)
			{
				foreach (var message in messages)
				{
					ModelState.AddModelError(propertyName, message);
				}
			}
		}
		else
		{
			// Add as a general error if no field-specific errors
			ModelState.AddModelError(string.Empty, error.Message);
		}
	}
}
