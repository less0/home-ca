using home_ca_backend.Application.Model;
using MediatR;

namespace home_ca_backend.Application;

public class GetCertificateAuthorities : IRequest<CertificateAuthority[]>;