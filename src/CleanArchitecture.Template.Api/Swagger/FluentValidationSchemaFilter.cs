﻿using FluentValidation;
using FluentValidation.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitecture.Template.Api.Swagger
{
    public class FluentValidationSchemaFilter : ISchemaFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public FluentValidationSchemaFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetService(typeof(IValidator<>).MakeGenericType(context.Type)) as IValidator;

            if (validator == null)
                return;

            var validatorDescriptor = validator.CreateDescriptor();

            foreach (var propertyValidators in validatorDescriptor.GetMembersWithValidators())
            {
                foreach (var propertyValidator in propertyValidators)
                {
                    var key = model.Properties.First(x => x.Key.ToLower() == propertyValidators.Key.ToLower()).Key;

                    if (propertyValidator.Validator is INotNullValidator || propertyValidator.Validator is INotEmptyValidator)
                        model.Required.Add(key);

                    if (propertyValidator.Validator is ILengthValidator lengthValidator)
                    {
                        if (lengthValidator.Max > 0)
                            model.Properties[key].MaxLength = lengthValidator.Max;

                        model.Properties[key].MinLength = lengthValidator.Min;
                    }

                    if (propertyValidator.Validator is IRegularExpressionValidator expressionValidator)
                        model.Properties[key].Pattern = expressionValidator.Expression;
                }
            }
        }
    }
}
