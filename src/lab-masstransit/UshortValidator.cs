using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace lab_masstransit
{
    internal class UshortValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            if (!option.HasValue())
            {
                return ValidationResult.Success;
            }

            if (ushort.TryParse(option.Value(), out _))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"The value for --{option.LongName} must be a valid ushort");
        }
    }
}