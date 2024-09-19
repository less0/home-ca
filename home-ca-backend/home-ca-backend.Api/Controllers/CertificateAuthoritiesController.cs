using System.Net.Mime;
using home_ca_backend.Api.Model;
using home_ca_backend.Application.AddIntermediateCertificateAuthority;
using home_ca_backend.Application.AddRootCertificateAuthority;
using home_ca_backend.Application.GetCertificateAuthorities;
using home_ca_backend.Application.GetChildrenCertificateAuthorities;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_ca_backend.Api.Controllers;

[Controller]
public class CertificateAuthoritiesController(IMediator mediator) : Controller
{
    [HttpGet("/cas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> GetCertificateAuthorities()
    {
        var response = await mediator.Send(new GetCertificateAuthorities());
        return Ok(response.Select(x => new CertificateAuthority(x.Id, x.Name, x.IsRoot, false)));
    }

    [HttpGet("/cas/{id}/children")]
    public async Task<ActionResult> GetCertificateAuthoritiesChildren(string id)
    {
        var response = await mediator.Send(new GetChildrenCertificateAuthorities()
        {
            Id = Guid.Parse(id)
        });

        return response switch
        {
            GetChildrenCertificateAuthoritiesValidResponse validResponse => Ok(GetCertificateAuthorities(validResponse)),
            GetChildrenCertificateAuthoritiesParentIdNotFoundResponse => NotFound(),
            _ => StatusCode(500)
        };
    }

    [HttpPost("/cas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces(MediaTypeNames.Text.Plain)]
    public async Task<ActionResult> PostRootCertificateAuthority([FromBody] CertificateAuthority certificateAuthority, [FromQuery] string password)
    {
        var response = await mediator.Send(new AddRootCertificateAuthority
        {
            CertificateAuthority = new()
            {
                Id = null,
                Name = certificateAuthority.Name
            },
            Password = password
        });
        return Ok(response.Guid.ToString());
    }

    [HttpPost("/cas/{id}/children")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces(MediaTypeNames.Text.Plain)]
    public async Task<ActionResult> PostIntermediateCertificateAuthority([FromBody] CertificateAuthority certificateAuthority, string id, [FromQuery] string password, [FromQuery] string parentPassword)
    {
        var response = await mediator.Send(new AddIntermediateCertificateAuthority
        {
            CertificateAuthority = new()
            {
                Id = null,
                Name = certificateAuthority.Name
            },
            ParentId = id,
            Password = password,
            ParentPassword = parentPassword
        });


        return response switch
        {
            ValidResponse validResponse => Ok(validResponse.CreatedCertificateAuthorityId.Guid
                .ToString()),
            ParentNotFoundResponse => NotFound(),
            InvalidPasswordResponse => Forbid(),
            _ => StatusCode(500)
        };
    }
    
    private static IEnumerable<CertificateAuthority> GetCertificateAuthorities(GetChildrenCertificateAuthoritiesValidResponse validResponse) => 
        validResponse.CertificateAuthorities.Select(x => new CertificateAuthority(x.Id, x.Name, false, false));
}