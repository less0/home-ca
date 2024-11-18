using home_ca_backend.Api.Model;
using home_ca_backend.Application.AddRootCertificateAuthority;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace home_ca_backend.Api.Controllers
{
    [Controller]
    public class AddRootCertificateAuthoritiesController(IMediator mediator) : ControllerBase
    {
        [HttpPost("/cas")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Produces(MediaTypeNames.Text.Plain, MediaTypeNames.Application.Json)]
        public async Task<ActionResult> PostRootCertificateAuthority([FromBody] PostRootCertificateAuthority certificateAuthority)
        {
            var response = await mediator.Send(new AddRootCertificateAuthorityCommand
            {
                CertificateAuthority = new()
                {
                    Id = null,
                    Name = certificateAuthority.Name
                },
                Password = certificateAuthority.Password
            });

            return response switch
            {
                ValidResponse validResponse => Ok(validResponse.Id.ToString()),
                ValidationFailedResponse validationFailedResponse => BadRequest(validationFailedResponse.Errors),
                _ => StatusCode(500)
            };
        }
    }
}
