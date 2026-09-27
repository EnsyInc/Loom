using EnsyInc.Loom.Api.Models;

using FluentValidation;

namespace EnsyInc.Loom.Api.Validators;

public sealed class CreateWorkItemFieldRequestValidator : AbstractValidator<CreateWorkItemFieldRequest>
{
    public CreateWorkItemFieldRequestValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(256);
        RuleFor(x => x.DataType).IsInEnum();
    }
}

public sealed class UpdateWorkItemFieldRequestValidator : AbstractValidator<UpdateWorkItemFieldRequest>
{
    public UpdateWorkItemFieldRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(256);
    }
}
