using AutoMapper;
using BW.Website.Application.Features.SampleWorkOrders.Models;
using BW.Website.Domain.Entities;

namespace BW.Website.Application.Mapping;

public sealed class ApplicationMappingProfile : Profile
{
	public ApplicationMappingProfile()
	{
		// SampleWorkOrder <-> SampleWorkOrderDto
		CreateMap<SampleWorkOrder, SampleWorkOrderDto>()
			.ReverseMap();
	}
}
