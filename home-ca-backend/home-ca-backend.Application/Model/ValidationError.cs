namespace home_ca_backend.Application.Model;

public record ValidationError(string PropertyName, string Reason);