using EnsyInc.Loom.Api.Models;

using FluentValidation;

namespace EnsyInc.Loom.Api.Validators;

public sealed class CreateStatusRequestValidator : AbstractValidator<CreateStatusRequest>
{
    public CreateStatusRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Category).IsInEnum();
    }
}

public sealed class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
{
    public UpdateStatusRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Category).IsInEnum();
    }
}
