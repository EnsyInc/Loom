using EnsyInc.Loom.Api.Models;

using FluentValidation;

namespace EnsyInc.Loom.Api.Validators;

public sealed class CreateStatusTransitionRequestValidator : AbstractValidator<CreateStatusTransitionRequest>
{
    public CreateStatusTransitionRequestValidator()
    {
        RuleFor(x => x.FromStatusId).NotEqual(Guid.Empty);
        RuleFor(x => x.ToStatusId).NotEqual(Guid.Empty);
    }
}
