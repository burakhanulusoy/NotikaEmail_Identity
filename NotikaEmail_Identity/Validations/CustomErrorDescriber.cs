using Microsoft.AspNetCore.Identity;

namespace NotikaEmail_Identity.Validations
{
    public class CustomErrorDescriber : IdentityErrorDescriber
    {

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = nameof(PasswordTooShort),
                Description = $"Şifreniz en az {length} karakter içermelidir."
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "Şifreniz en az bir adet özel karakter (!, ?, *, vb.) içermelidir."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "Şifreniz en az bir adet rakam (0-9) içermelidir."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = "Şifreniz en az bir adet küçük harf (a-z) içermelidir."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "Şifreniz en az bir adet büyük harf (A-Z) içermelidir."
            };
        }


        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"'{userName}' kullanıcı adı zaten kullanılıyor. Lütfen başka bir tane seçiniz."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = $"'{email}' e-posta adresi zaten sistemde kayıtlı."
            };
        }

        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = $"'{userName}' geçersiz bir kullanıcı adıdır. Lütfen sadece harf ve rakam kullanınız."
            };
        }

        public override IdentityError InvalidEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = $"Girdiğiniz e-posta adresi geçersiz bir formattadır."
            };
        }

        // --- VERİTABANI ÇAKIŞMA (CONCURRENCY) HATASI ---
        // (Aynı anda iki kişi aynı hesabı güncellemeye çalışırsa veritabanının patlamasını önler)
        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError
            {
                Code = nameof(ConcurrencyFailure),
                Description = "Veri başka bir işlem tarafından değiştirilmiş olabilir. Lütfen sayfayı yenileyip tekrar deneyin."
            };
        }

    }
}