using FluentResults;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningPoker.Application
{
    public static class ErrorExtensions
    {
        public const string ErrorCodeMetadata = "ErrorCode";
        public const int NotFoundErrorCode = 404;

        public static Error CreateErrorFromValidationResult(this ValidationFailure validationResult)
        {
            return new Error(validationResult.ErrorMessage)
                .WithMetadata(new Dictionary<string, object>(){
                { ErrorCodeMetadata, validationResult.ErrorCode },
                { nameof(validationResult.AttemptedValue), validationResult.AttemptedValue },
                { nameof(validationResult.CustomState), validationResult.CustomState },
                { nameof(validationResult.PropertyName), validationResult.PropertyName },
                { nameof(validationResult.Severity), validationResult.Severity }
               })
                ;
        }

        public static Error WithNotFoundErrorMetadata(this Error error)
        {
            return error.WithMetadata(ErrorCodeMetadata, NotFoundErrorCode);
        }

        public static bool HasNotFoundErrorMetadata<T>(this Result<T> result)
        {
            return result.HasError(e => e.HasMetadata(ErrorCodeMetadata, v => v.Equals(NotFoundErrorCode)));
        }

    }
}
