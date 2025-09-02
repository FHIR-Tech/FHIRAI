using System.Reflection;
using FHIRAI.Application.Common.Exceptions;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Application.Common.Security;
using Microsoft.Extensions.Logging;

namespace FHIRAI.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behavior for FHIR scope validation
/// </summary>
public class FhirScopeBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IPatientAccessService _patientAccessService;
    private readonly ILogger<FhirScopeBehaviour<TRequest, TResponse>> _logger;

    public FhirScopeBehaviour(
        IUser user,
        IPatientAccessService patientAccessService,
        ILogger<FhirScopeBehaviour<TRequest, TResponse>> logger)
    {
        _user = user;
        _patientAccessService = patientAccessService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var fhirScopeAttributes = request.GetType().GetCustomAttributes<FhirScopeAttribute>();

        if (fhirScopeAttributes.Any())
        {
            var userId = _user.Id;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt: User not authenticated");
                throw new UnauthorizedAccessException("User not authenticated");
            }

            foreach (var scopeAttribute in fhirScopeAttributes)
            {
                if (!await ValidateFhirScopeAsync(scopeAttribute, request, userId))
                {
                    _logger.LogWarning("Forbidden access attempt: User {UserId} lacks required FHIR scope {Scope}", 
                        userId, scopeAttribute.Scope);
                    throw new ForbiddenAccessException($"User lacks required FHIR scope: {scopeAttribute.Scope}");
                }
            }
        }

        return await next();
    }

    private async Task<bool> ValidateFhirScopeAsync(FhirScopeAttribute scopeAttribute, TRequest request, string userId)
    {
        var requiredScope = scopeAttribute.Scope;

        // Check if user has the required scope
        if (!_user.HasScope(requiredScope))
        {
            _logger.LogDebug("User {UserId} does not have required scope {Scope}", userId, requiredScope);
            return false;
        }

        // If scope requires patient access validation
        if (scopeAttribute.RequiresPatientAccess)
        {
            var patientId = ExtractPatientId(request, scopeAttribute.PatientIdParameter);
            if (!string.IsNullOrEmpty(patientId))
            {
                var canAccess = await _patientAccessService.CanAccessPatientAsync(userId, patientId, requiredScope);
                if (!canAccess)
                {
                    _logger.LogDebug("User {UserId} cannot access patient {PatientId} with scope {Scope}", 
                        userId, patientId, requiredScope);
                    return false;
                }
            }
        }

        return true;
    }

    private static string? ExtractPatientId(TRequest request, string? patientIdParameter)
    {
        if (string.IsNullOrEmpty(patientIdParameter))
            return null;

        var property = typeof(TRequest).GetProperty(patientIdParameter);
        if (property != null)
        {
            var value = property.GetValue(request);
            return value?.ToString();
        }

        // Try to find patient ID in common FHIR parameters
        var searchParamsProperty = typeof(TRequest).GetProperty("SearchParameters");
        if (searchParamsProperty?.GetValue(request) is Dictionary<string, string> searchParams)
        {
            if (searchParams.TryGetValue("patient", out var patientId))
                return patientId;
            if (searchParams.TryGetValue("subject", out var subject))
                return ExtractPatientIdFromReference(subject);
        }

        return null;
    }

    private static string? ExtractPatientIdFromReference(string? reference)
    {
        if (string.IsNullOrEmpty(reference))
            return null;

        // Handle FHIR references like "Patient/123" or "Patient/123/_history/1"
        if (reference.StartsWith("Patient/"))
        {
            var parts = reference.Split('/');
            if (parts.Length >= 2)
                return parts[1];
        }

        return null;
    }
}
