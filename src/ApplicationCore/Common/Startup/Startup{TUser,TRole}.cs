namespace GamaEdtech.Backend.Common.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text.Encodings.Web;
    using System.Text.Unicode;

    using GamaEdtech.Backend.Common.Converter;
    using GamaEdtech.Backend.Common.Core;
    using GamaEdtech.Backend.Common.Core.Extensions.Collections;
    using GamaEdtech.Backend.Common.Data;
    using GamaEdtech.Backend.Common.DataAccess.Context;
    using GamaEdtech.Backend.Common.DataAnnotation;
    using GamaEdtech.Backend.Common.Identity;
    using GamaEdtech.Backend.Common.Identity.DataProtection;
    using GamaEdtech.Backend.Common.Localization;
    using GamaEdtech.Backend.Common.Logging;
    using GamaEdtech.Backend.Common.Mapping;
    using GamaEdtech.Backend.Common.ModelBinding;
    using GamaEdtech.Backend.Common.Mvc.Routing;
    using GamaEdtech.Backend.Common.Resources;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.WebEncoders;

    public abstract class Startup<TUser, TRole>
        where TUser : class
        where TRole : class
    {
        private readonly StartupOption startupOption;

        protected Startup([NotNull] StartupOption startupOption)
        {
            Constants.ErrorCodePrefix = startupOption.ErrorCodePrefix;
            this.startupOption = startupOption;
        }

        public IConfiguration Configuration => startupOption.Configuration;

        public void ConfigureServices([NotNull] IServiceCollection services)
        {
            var frameworkAssembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(frameworkAssembly.Location);
            if (string.IsNullOrEmpty(dir))
            {
                return;
            }

            var files = Directory.GetFiles(dir, $"{startupOption.DefaultNamespace}.*.dll").Where(t => !t.Contains(frameworkAssembly.ManifestModule.Name!, StringComparison.OrdinalIgnoreCase));

            var mvcBuilder = ConfigureServicesInternal(services);

            AddScopedDynamic(services, frameworkAssembly, files);
            ConfigureServicesCore(services, mvcBuilder);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (startupOption.Localization)
            {
                // app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);
                _ = app.UseRequestLocalization(LocalizationExtensions.RequestLocalizationOptions);
            }

            _ = app.UseExceptionHandler(_ => { });

            if (!env.IsDevelopment())
            {
                if (startupOption.Https)
                {
                    _ = app.UseHsts();
                    _ = app.UseHttpsRedirection();
                }
            }

            _ = app.UseRouting();

            if (startupOption.Authentication)
            {
                _ = app.UseAuthentication().UseAuthorization();
            }

            ConfigureCore(app, env);

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllerRoute(
                    name: "areaRoute",
                    pattern: $"{(startupOption.Localization ? "{culture=fa}/" : string.Empty)}{{area:slugify:exists}}/{{controller:slugify=Home}}/{{action:slugify=Index}}/{{id?}}");

                _ = endpoints.MapControllerRoute(
                    name: "default",
                    pattern: $"{(startupOption.Localization ? "{culture=fa}/" : string.Empty)}{{controller:slugify=Home}}/{{action:slugify=Index}}/{{id?}}");
            });
        }

        protected abstract void ConfigureServicesCore(IServiceCollection services, IMvcBuilder mvcBuilder);

        protected abstract void ConfigureCore(IApplicationBuilder app, IWebHostEnvironment env);

        private static void AddStores(IServiceCollection services, Type userType, Type roleType, Type contextType)
        {
            var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>)) ?? throw new InvalidOperationException("NotIdentityUser");
            var keyType = identityUserType.GenericTypeArguments[0];

            if (roleType is not null)
            {
                Type userStoreType;
                Type roleStoreType;
                var identityContext = FindGenericBaseType(contextType, typeof(IdentityDbContext<,,,,,,,>));
                if (identityContext is null)
                {
                    // If its a custom DbContext, we can only add the default POCOs
                    userStoreType = typeof(UserStore<,,,>).MakeGenericType(userType, roleType, contextType, keyType);
                    roleStoreType = typeof(RoleStore<,,>).MakeGenericType(roleType, contextType, keyType);
                }
                else
                {
                    userStoreType = typeof(UserStore<,,,,,,,,>).MakeGenericType(userType, roleType, contextType,
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[3],
                        identityContext.GenericTypeArguments[4],
                        identityContext.GenericTypeArguments[5],
                        identityContext.GenericTypeArguments[7],
                        identityContext.GenericTypeArguments[6]);
                    roleStoreType = typeof(RoleStore<,,,,>).MakeGenericType(roleType, contextType,
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[4],
                        identityContext.GenericTypeArguments[6]);
                }

                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
                services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
            }
            else
            { // No Roles
                Type userStoreType;
                var identityContext = FindGenericBaseType(contextType, typeof(IdentityUserContext<,,,,>));
                if (identityContext is null)
                {
                    // If its a custom DbContext, we can only add the default POCOs
                    userStoreType = typeof(UserOnlyStore<,,>).MakeGenericType(userType, contextType, keyType);
                }
                else
                {
                    userStoreType = typeof(UserOnlyStore<,,,,,>).MakeGenericType(userType, contextType,
                        identityContext.GenericTypeArguments[1],
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[3],
                        identityContext.GenericTypeArguments[4]);
                }

                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            }
        }

        private static TypeInfo? FindGenericBaseType([NotNull] Type currentType, [NotNull] Type genericBaseType)
        {
            var type = currentType;
            while (type is not null)
            {
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType is not null && genericType == genericBaseType)
                {
                    return type.GetTypeInfo();
                }

                type = type.BaseType;
            }

            return null;
        }

        private IMvcBuilder ConfigureServicesInternal(IServiceCollection services)
        {
            _ = services.AddTransient(typeof(Lazy<>));

            _ = services.AddExceptionHandler<GlobalExceptionHandler>();

            if (startupOption.Localization)
            {
                services.TryAddSingleton<Microsoft.Extensions.Localization.IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();

                _ = services.AddLocalization();
                services.ConfigureRequestLocalization();
            }

            _ = services.AddRouting(options =>
            {
                options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            var mvcBuilder = services.AddControllers(ConfigureMvc);

            _ = mvcBuilder.AddJsonOptions(options =>
            {
                // options.JsonSerializerOptions.Converters.Add(new DictionaryEnumerationConverter<int>());
                // options.JsonSerializerOptions.Converters.Add(new DictionaryEnumerationConverter<byte>());
                options.JsonSerializerOptions.Converters.Add(new EnumerationConverterFactory());
                options.JsonSerializerOptions.Converters.Add(new FlagsEnumerationConverterFactory());
                options.JsonSerializerOptions.Converters.Add(new BitArrayConverter());
                options.JsonSerializerOptions.Converters.Add(new UlidJsonConverter());

                // options.JsonSerializerOptions.Converters.Add(new DateTimeConverterFactory());
            });

            _ = mvcBuilder.AddDataAnnotationsLocalization(options => options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(GlobalResource)));

            _ = mvcBuilder.ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = actionContext => new OkObjectResult(new ApiResponse<object>(actionContext.ModelState)));

            if (startupOption.Authentication)
            {
                var lifetime = Configuration.GetValue<TimeSpan>("IdentityOptions:DataProtection:Lifetime");
                _ = services.AddDataProtection()
                    .SetApplicationName(startupOption.DefaultNamespace)
                    .PersistKeysToDbContext(lifetime)
                    .SetDefaultKeyLifetime(lifetime);
            }

            var httpClientBuilder = services.AddHttpClient(Constants.HttpClientIgnoreSslAndAutoRedirect).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true,
                AllowAutoRedirect = false,
                UseProxy = false,
            });
            if (startupOption.HttpClientMessageHandler is not null)
            {
                _ = httpClientBuilder.AddHttpMessageHandler(startupOption.HttpClientMessageHandler);
            }

#pragma warning disable CA5398 // Avoid hardcoded SslProtocols values
            var httpClientBuilder13 = services.AddHttpClient(Constants.HttpClientIgnoreSslAndAutoRedirectTls13).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true,
                AllowAutoRedirect = false,
                UseProxy = false,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls13,
            });
#pragma warning restore CA5398 // Avoid hardcoded SslProtocols values
            if (startupOption.HttpClientMessageHandler is not null)
            {
                _ = httpClientBuilder13.AddHttpMessageHandler(startupOption.HttpClientMessageHandler);
            }

            _ = services.AddHttpContextAccessor();
            _ = services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
            _ = services.ConfigureOptions<ConfigureJsonOptions>();

            if (startupOption.Authentication)
            {
                _ = services.AddAuthorization(options => options.AddPolicy(PermissionConstants.PermissionPolicy, policy => policy.RequireAssertion(context =>
                {
                    if (context.Resource is not HttpContext httpContext)
                    {
                        return false;
                    }

                    var endPoint = httpContext.GetEndpoint();
                    if (endPoint is null)
                    {
                        return false;
                    }

                    var permission = endPoint.Metadata.OfType<PermissionAttribute>().LastOrDefault();
                    if (permission is null)
                    {
                        return false;
                    }

                    if (permission.Roles?.Exists(t => context.User.IsInRole(t)) == true)
                    {
                        return true;
                    }

                    var claims = context.User.Claims.Where(t => t.Type == PermissionConstants.PermissionPolicy);
                    return claims.Any(t => t.Value.Equals(endPoint?.DisplayName, StringComparison.OrdinalIgnoreCase));
                })));

                _ = services.AddAuthentication().AddScheme<TokenAuthenticationSchemeOptions, TokenAuthenticationHandler>(PermissionConstants.TokenAuthenticationScheme, t => { });
                _ = services.Configure<ApiDataProtectorTokenProviderOptions>(Configuration.GetSection("IdentityOptions:Tokens:ApiDataProtectorTokenProviderOptions"));
                _ = services.Configure<IdentityOptions>(options => options.Tokens.ProviderMap[PermissionConstants.ApiDataProtectorTokenProvider] = new TokenProviderDescriptor(typeof(IApiDataProtectorTokenProvider<TUser>)));
            }

            return mvcBuilder;

            static void ConfigureMvc(MvcOptions options)
            {
                options.ModelMetadataDetailsProviders.Add(new DisplayMetadataProvider());

                options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor((v) => string.Format(GlobalResource.Validation_MissingBindRequiredValueAccessor, v));
                options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => GlobalResource.Validation_MissingKeyOrValueAccessor);
                options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() => GlobalResource.Validation_MissingRequestBodyRequiredValueAccessor);
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor((v) => string.Format(GlobalResource.Validation_ValueMustNotBeNullAccessor, v));
                options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((v1, v2) => string.Format(GlobalResource.Validation_AttemptedValueIsInvalidAccessor, v1, v2));
                options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor((v) => string.Format(GlobalResource.Validation_NonPropertyAttemptedValueIsInvalidAccessor, v));
                options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor((v) => string.Format(GlobalResource.Validation_UnknownValueIsInvalidAccessor, v));
                options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => GlobalResource.Validation_NonPropertyUnknownValueIsInvalidAccessor);
                options.ModelBindingMessageProvider.SetValueIsInvalidAccessor((v) => string.Format(GlobalResource.Validation_ValueIsInvalidAccessor, v));
                options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor((v) => string.Format(GlobalResource.Validation_ValueMustBeANumberAccessor, v));
                options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => string.Format(GlobalResource.Validation_NonPropertyValueMustBeANumberAccessor));

                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

                options.ModelBinderProviders.Insert(0, new EnumerationQueryStringModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new FlagsEnumerationQueryStringModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new DateTimeOffsetQueryStringModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new UlidQueryStringModelBinderProvider());
            }
        }

        private void AddScopedDynamic(IServiceCollection services, Assembly frameworkAssembly, IEnumerable<string>? assemblyFiles)
        {
            if (assemblyFiles is null)
            {
                return;
            }

            var assemblies = assemblyFiles.Select(Assembly.LoadFrom)
                .Where(t => t.GetCustomAttribute<InjectableAttribute>() is not null)
                .Union(new[] { frameworkAssembly });
            var allTypes = assemblies.SelectMany(t => t.DefinedTypes);
            var injectableTypes = allTypes.Where(t => t.GetCustomAttribute<InjectableAttribute>() is not null);
            foreach (var serviceType in injectableTypes)
            {
                var implementationTypes = serviceType.GetAllTypesImplementingType(allTypes);
                if (implementationTypes is null || !implementationTypes.Any())
                {
                    continue;
                }

                foreach (var implementationType in implementationTypes)
                {
                    var serviceLifetimeAttribute = implementationType.GetCustomAttribute<ServiceLifetimeAttribute>();
                    serviceLifetimeAttribute ??= new ServiceLifetimeAttribute(ServiceLifetime.Transient);

                    if (serviceLifetimeAttribute.Parameters?.Any() == true)
                    {
                        services.Add(new ServiceDescriptor(serviceType, provider =>
                        {
                            var parameters = serviceLifetimeAttribute.Parameters.Select(t => provider.GetService(t)).ToArray();
                            return Activator.CreateInstance(implementationType, parameters);
                        }, serviceLifetimeAttribute.ServiceLifetime));

                        services.Add(new ServiceDescriptor(implementationType, provider =>
                        {
                            var parameters = serviceLifetimeAttribute.Parameters.Select(t => provider.GetService(t)).ToArray();
                            return Activator.CreateInstance(implementationType, parameters);
                        }, serviceLifetimeAttribute.ServiceLifetime));
                    }
                    else
                    {
                        services.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetimeAttribute.ServiceLifetime));
                    }

                    // switch (serviceLifetimeAttribute.ServiceLifetime)
                    // {
                    //    case ServiceLifetime.Singleton:
                    //        services.AddSingleton(Lazy<serviceType>);
                    //        break;
                    //    case ServiceLifetime.Scoped:
                    //        services.AddScoped(serviceType);
                    //        break;
                    //    case ServiceLifetime.Transient:
                    //        services.AddTransient(serviceType);
                    //        break;
                    // }
                    if (serviceType == typeof(IEntityContext))
                    {
                        var arguments = implementationType?.BaseType?.GetGenericArguments();
                        if (arguments is not null && startupOption.Identity)
                        {
                            AddStores(services, arguments[1], arguments[2], arguments[0]);
                        }
                    }
                }

                if (startupOption.Identity)
                {
                    if (serviceType == typeof(IEntityContext))
                    {
                        _ = services
                            .AddIdentity<TUser, TRole>(options => Configuration.Bind("IdentityOptions", options))
                            .AddDefaultTokenProviders()
                            .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

                        _ = services.Configure<SecurityStampValidatorOptions>(options => Configuration.Bind("IdentityOptions:SecurityStampValidator", options));
                    }
                }
            }

            var config = new TypeAdapterConfig();
            _ = config.Scan(assemblies.ToArray());
            _ = services.AddSingleton(config);
        }
    }
}
