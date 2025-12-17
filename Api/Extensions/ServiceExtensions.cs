using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddCustomValidationResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .Select(e => new
                        {
                            Field = e.Key,
                            Error = GetCleanErrorMessage(e.Value.Errors.First())
                        })
                        .ToList();

                    var responseObj = new
                    {
                        Success = false,
                        Message = "Input Validation Failed",
                        Errors = errors
                    };

                    return new BadRequestObjectResult(responseObj);
                };
            });
        }

        private static string GetCleanErrorMessage(Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error)
        {
            if (!string.IsNullOrEmpty(error.ErrorMessage))
            {
                return error.ErrorMessage;
            }

            if (error.Exception != null)
            {
                var msg = error.Exception.Message;
                if (msg.Contains("could not be converted") || msg.Contains("invalid start of a value"))
                {
                    return "The input format is invalid for this field.";
                }
            }

            return "Invalid input value.";
        }
    }
}