
using FluentValidation;
using NotikaEmail_Identity.Models;

namespace NotikaEmail_Identity.Validators
{
    public class RegisterValidator:AbstractValidator<RegisterUserViewModel>
    {

        public RegisterValidator()
        {

            RuleFor(x => x.Name)              
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir!");

            RuleFor(x => x.Surname)
                .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir!");

          


        }





    }
}
