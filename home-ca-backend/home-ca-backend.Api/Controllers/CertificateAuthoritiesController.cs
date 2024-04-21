﻿using home_ca_backend.Api.Model;
using home_ca_backend.Application;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_ca_backend.Api.Controllers;

[Controller]
public class CertificateAuthoritiesController(IMediator mediator) : Controller
{
    private IMediator _mediator = mediator;

    [HttpGet("/cas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> GetCertificateAuthorities([FromQuery] bool root = false)
    {
        var response = await _mediator.Send(new GetCertificateAuthorities());
        return Ok(response.Select(x => new CertificateAuthority
        {
            Id = x.Id,
            Name = x.Name,
            IsRoot = x.IsRoot,
            HasChildren = false
        }));
    }

    [HttpGet("/cas/{id}/children")]
    public async Task<ActionResult> GetCertificateAuthoritiesChildren(string id)
    {
        var response = await _mediator.Send(new GetChildrenCertificateAuthorities()
        {
            Id = Guid.Parse(id)
        });
        return Ok(response.Select(x => new CertificateAuthority
        {
            Id = x.Id,
            Name = x.Name,
            IsRoot = false,
            HasChildren = false
        }));
    }
}