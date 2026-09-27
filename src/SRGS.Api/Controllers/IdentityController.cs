using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SRGS.Api.Controllers;


[ApiController]
[ApiVersionNeutral]
public sealed class IdentityController(ISender sender) : ApiController
{



}