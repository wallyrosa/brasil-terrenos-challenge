using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Transfers.Application.Common.Exceptions;
using Transfers.Domain.Common;

namespace Transfers.Api.ErrorHandling;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, code) = Map(exception);

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message,
                Extensions = { ["code"] = code }
            }
        });
    }

    private static (int Status, string Title, string Code) Map(Exception exception) =>
        exception switch
        {
            DomainException domain => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada.", domain.Code),
            NotFoundException notFound => (StatusCodes.Status404NotFound, "Recurso não encontrado.", notFound.Code),
            ConflictException conflict => (StatusCodes.Status409Conflict, "Conflito.", conflict.Code),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requisição inválida.", "bad_request"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno.", "internal_error")
        };
}
