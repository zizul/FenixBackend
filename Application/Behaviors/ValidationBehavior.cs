﻿using FluentValidation;
using MediatR;

namespace Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;


        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            this.validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(validators.Select(v =>
                v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f
                           != null).ToList();

                if (failures.Any())
                    throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
