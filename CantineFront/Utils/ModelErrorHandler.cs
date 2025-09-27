using DPWorldMobile.ServiceFactory;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CantineFront.Utils
{
    public class ModelErrorHandler<T> where T : class
    {
        public static ApiResponse<T> ModelStateError(ModelStateDictionary modelState)
        {
            var errorList = modelState.ToDictionary(
                  kvp => kvp.Key.Split('.').Last(),
                  kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
              );

            return new ApiResponse<T>() { Success = false, Errors = errorList };
        }

    }
}
