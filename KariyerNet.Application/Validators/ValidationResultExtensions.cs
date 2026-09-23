using FluentValidation.Results;

namespace KariyerNet.Application.Validators
{
    public static class ValidationResultExtensions
    {
        public static Dictionary<string, string[]> ToErrorDictionary(this ValidationResult result)
        {
            return result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }
    }
}
