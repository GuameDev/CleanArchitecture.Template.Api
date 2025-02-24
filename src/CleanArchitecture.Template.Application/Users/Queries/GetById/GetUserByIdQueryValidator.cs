﻿using FluentValidation;

namespace CleanArchitecture.Template.Application.Users.Queries.GetById
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id cannot be empty")
                .NotNull().WithMessage("Id cannot be null");
        }
    }
}
