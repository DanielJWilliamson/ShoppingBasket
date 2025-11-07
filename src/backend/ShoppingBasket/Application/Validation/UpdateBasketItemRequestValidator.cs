using FluentValidation;
using ShoppingBasket.Application.DTOs;

namespace ShoppingBasket.Application.Validation;

public class UpdateBasketItemRequestValidator : AbstractValidator<UpdateBasketItemRequest>
{
    public UpdateBasketItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
    }
}
