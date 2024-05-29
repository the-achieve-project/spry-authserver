using PhoneNumbers;
using Spry.Identity.Pages.Account;
using System.ComponentModel.DataAnnotations;

namespace Spry.Identity.Application.Attributes
{
#nullable disable
    public class ValidatePhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance as RegisterModel.InputModel;

            try
            {
                var regionCode = "GH";
                var util = PhoneNumberUtil.GetInstance();

                instance.PhoneNumber = util.Format(util.Parse(instance.PhoneNumber, regionCode), PhoneNumberFormat.E164);

                if (!util.IsValidNumberForRegion(util.Parse(instance.PhoneNumber, regionCode), regionCode))
                {
                    return new ValidationResult("invalid number");
                }
            }
            catch (Exception)
            {
                return new ValidationResult("invalid number");
            }

            return ValidationResult.Success;
        }
    }
}
