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
        [Produces(MediaTypeNames.Text.Plain)]
        public async Task<ActionResult> PostRootCertificateAuthority([FromBody] CertificateAuthority certificateAuthority, [FromQuery] string password)
        {
            var response = await mediator.Send(new AddRootCertificateAuthorityCommand
            {
                CertificateAuthority = new()
                {
                    Id = null,
                    Name = certificateAuthority.Name
                },
                Password = password
            });

            return response switch
            {
                ValidResponse validResponse => Ok(validResponse.Id.ToString()),
                ValidationFailedResponse validationFailedResponse => BadRequest(),
                _ => StatusCode(500)
            };
        }
    }
}
