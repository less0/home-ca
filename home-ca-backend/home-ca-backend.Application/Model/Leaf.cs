﻿namespace home_ca_backend.Application.Model;

public record Leaf(Guid Id, string Name, string? CertificateChain, string? PrivateKey);