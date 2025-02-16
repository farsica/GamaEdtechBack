namespace GamaEdtech.Backend.Common.Localization
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading.Tasks;

    using GamaEdtech.Backend.Common.Core;
    using GamaEdtech.Backend.Common.Core.Extensions.Collections;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;

    public class RouteValueRequestCultureProvider(CultureInfo[] cultures) : IRequestCultureProvider
    {
        private readonly CultureInfo[] cultures = cultures;

        public Task<ProviderCultureResult?> DetermineProviderCultureResult([NotNull] HttpContext httpContext)
        {
            var path = httpContext.Request.Path;

            if (string.IsNullOrEmpty(path))
            {
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(Constants.DefaultLanguageCode));
            }

            var routeValues = httpContext.Request.Path.Value.Split('/');
            return routeValues.Length <= 1
                ? Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(Constants.DefaultLanguageCode))
                : !cultures.Exists(t => t.Name.Equals(routeValues[1], StringComparison.OrdinalIgnoreCase))
                ? Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(Constants.DefaultLanguageCode))
                : Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(routeValues[1]));
        }
    }
}
