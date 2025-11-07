using FluentValidation;
using ShoppingBasket.Application.DTOs;

namespace ShoppingBasket.Application.Validation;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        // BuyerId optional; IncludeVat is a boolean, no rule needed.
        RuleForEach(r => r.VatOverrides!).ChildRules(v =>
        {
            v.RuleFor(x => x.ProductId).GreaterThan(0);
            // IncludeVat is bool; no additional rule.
        }).When(r => r.VatOverrides != null);
    }
}
