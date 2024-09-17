using home_ca_backend.Application.GetLeafs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leaf = home_ca_backend.Api.Model.Leaf;

namespace home_ca_backend.Api.Controllers;

[Controller]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LeafsController(IMediator mediator) : Controller
{
    [HttpGet("/cas/{id}/leafs")]
    public async Task<IActionResult> GetLeafs(string id)
    {
        var response = await mediator.Send(new GetLeafsQuery(Guid.Parse(id)));
        return response switch
        {
            GetLeafsValidResponse validResponse => Ok(validResponse.Leafs.Select(x => (Leaf)x)),
            GetLeafsUnknownParentIdResponse => NotFound(),
            _ => StatusCode(500)
        };
    }
}