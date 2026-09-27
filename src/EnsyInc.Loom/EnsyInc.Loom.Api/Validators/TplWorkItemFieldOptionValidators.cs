using EnsyInc.Loom.Api.Models;

using FluentValidation;

namespace EnsyInc.Loom.Api.Validators;

public sealed class CreateFieldOptionRequestValidator : AbstractValidator<CreateFieldOptionRequest>
{
    public CreateFieldOptionRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(256);
    }
}

public sealed class UpdateFieldOptionRequestValidator : AbstractValidator<UpdateFieldOptionRequest>
{
    public UpdateFieldOptionRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(256);
    }
}
