using home_ca_backend.Api.Model;
using home_ca_backend.Application.AddIntermediateCertificateAuthority;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace home_ca_backend.Api.Controllers
{
    [Controller]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AddIntermediateCertificateAuthoritiesController(IMediator mediator) : Controller
    {
        [HttpPost("/cas/{id}/children")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Produces(MediaTypeNames.Text.Plain)]
        public async Task<ActionResult> PostIntermediateCertificateAuthority([FromBody] PostIntermediateCertificateAuthority certificateAuthority, string id)
        {
            var response = await mediator.Send(new AddIntermediateCertificateAuthorityCommand
            {
                CertificateAuthority = new()
                {
                    Id = null,
                    Name = certificateAuthority.Name
                },
                ParentId = id,
                Password = certificateAuthority.Password,
                ParentPassword = certificateAuthority.ParentPassword
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
    }
}
