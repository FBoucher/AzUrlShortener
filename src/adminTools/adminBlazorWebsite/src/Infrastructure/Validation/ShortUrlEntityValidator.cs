using adminBlazorWebsite.Data;
using FluentValidation;

namespace adminBlazorWebsite.Infrastructure.Validation
{
    public class ShortUrlEntityValidator : AbstractValidator<ShortUrlEntity>
    {
        public ShortUrlEntityValidator()
        {
            RuleFor(m => m.Title)
                .NotEmpty();

            RuleFor(m => m.Url)
                .NotEmpty();

            RuleFor(m => m.ShortUrl)
                .NotEmpty();
        }
    }
}