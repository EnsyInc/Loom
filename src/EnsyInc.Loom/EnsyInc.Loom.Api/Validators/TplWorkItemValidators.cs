using EnsyInc.Loom.Api.Models;

using FluentValidation;

namespace EnsyInc.Loom.Api.Validators;

public sealed class CreateWorkItemTypeRequestValidator : AbstractValidator<CreateWorkItemTypeRequest>
{
    public CreateWorkItemTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.IconUrl).MaximumLength(2048);
    }
}

public sealed class UpdateWorkItemTypeRequestValidator : AbstractValidator<UpdateWorkItemTypeRequest>
{
    public UpdateWorkItemTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.IconUrl).MaximumLength(2048);
    }
}

public sealed class SetInitialStatusRequestValidator : AbstractValidator<SetInitialStatusRequest>
{
    public SetInitialStatusRequestValidator()
    {
        RuleFor(x => x.StatusId).NotEqual(Guid.Empty);
    }
}
