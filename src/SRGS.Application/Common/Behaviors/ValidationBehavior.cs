using FluentValidation;

using MediatR;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Common.Results.Abstractions;

namespace SRGS.Application.Common.Behaviors;

// Runs before every command/query handler. Without this registered in the MediatR
// pipeline (see DependencyInjection.cs), CreateRequestCommandValidator would sit in the
// project unused — FluentValidation validators don't run themselves, something has to
// call ValidateAsync. This is that something, for every request in one place instead of
// each handler doing it manually.
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
{
    private readonly IValidator<TRequest>? _validator = validator;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_validator is null)
        {
            return await next(ct);
        }

        var validationResult = await _validator.ValidateAsync(request, ct);

        if (validationResult.IsValid)
        {
            return await next(ct);
        }

        var errors = validationResult.Errors
            .ConvertAll(error => Error.Validation(
                code: error.PropertyName,
                description: error.ErrorMessage));

        return (dynamic)errors;
    }
}
