using FluentValidation;
using ShoppingBasket.Application.DTOs;

namespace ShoppingBasket.Application.Validation;

public class AddToBasketRequestValidator : AbstractValidator<AddToBasketRequest>
{
    public AddToBasketRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
