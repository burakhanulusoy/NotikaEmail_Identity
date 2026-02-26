using FluentValidation;
using NotikaEmail_Identity.Models;

namespace NotikaEmail_Identity.Validations
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordViewModel>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Eski şifre alanı boş bırakılamaz.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifre alanı boş bırakılamaz.")
                .MinimumLength(6).WithMessage("Şifreniz en az 6 karakter olmalıdır.")
                .Matches(".*[A-Z].*").WithMessage("Şifreniz en az 1 büyük harf içermelidir.")
                .Matches(".*[a-z].*").WithMessage("Şifreniz en az 1 küçük harf içermelidir.")
                .Matches(".*[0-9].*").WithMessage("Şifreniz en az 1 rakam içermelidir.")
                .Matches(".*[^a-zA-Z0-9].*").WithMessage("Şifreniz en az 1 özel karakter (örneğin: !, *, ?, vb.) içermelidir.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Şifre tekrar alanı boş bırakılamaz.")
                .Equal(x => x.NewPassword).WithMessage("Girdiğiniz şifreler birbiriyle uyuşmuyor.");
        }
    }
}