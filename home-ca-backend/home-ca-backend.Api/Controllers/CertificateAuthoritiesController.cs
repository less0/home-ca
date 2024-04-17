using home_ca_backend.Api.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_ca_backend.Api.Controllers;

[Controller]
public class CertificateAuthoritiesController : Controller
{
    [HttpGet("/cas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult GetCertificateAuthorities([FromQuery] bool root = false)
    {
        return Ok(new List<CertificateAuthority>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                IsRoot = true,
                Name = "Example Root CA",
                HasChildren = (Random.Shared.Next(2) % 2) == 0
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                IsRoot = true,
                Name = "Second Root CA",
                HasChildren = (Random.Shared.Next(2) % 2) == 0
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                IsRoot = true,
                Name = "Third Root CA",
                HasChildren = (Random.Shared.Next(2) % 2) == 0
            }
        });
    }

    [HttpGet("/cas/{id}/children")]
    public ActionResult GetCertificateAuthoritiesChildren(string id)
    {
        return Ok(new List<CertificateAuthority>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                IsRoot = true,
                Name = "Example Child CA",
                HasChildren = (Random.Shared.Next(2) % 2) == 0
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                IsRoot = true,
                Name = "Second Child CA",
                HasChildren = (Random.Shared.Next(2) % 2) == 0
            }
        });
    }
}