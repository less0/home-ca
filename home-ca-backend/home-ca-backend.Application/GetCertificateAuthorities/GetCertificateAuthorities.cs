using home_ca_backend.Application.Model;
using MediatR;

namespace home_ca_backend.Application.GetCertificateAuthorities;

public class GetCertificateAuthorities : IRequest<CertificateAuthority[]>;