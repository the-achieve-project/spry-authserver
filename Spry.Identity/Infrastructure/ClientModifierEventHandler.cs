using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using System.Collections.Immutable;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;

namespace Spry.Identity.Infrastructure
{

    public static class IddictServerEventHandlers 
    {
        public static OpenIddictServerBuilder ServerEventHandlers(this OpenIddictServerBuilder builder)
        {
            builder.AddEventHandler<ValidateAuthorizationRequestContext>(handler =>
            {
                handler.UseInlineHandler(context =>
                {
                    ArgumentNullException.ThrowIfNull(context);

                    Console.WriteLine("ValidateAuthorizationRequestContext inline handler");
                    //context.Request!.RedirectUri = "https://jwt.ms";

                    return default;
                });

                //Console.WriteLine("ValidateAuthorizationRequestContext EventHandler");
            })
            .AddEventHandler<ValidateTokenRequestContext>(handler =>
            {
                handler.SetType(OpenIddictServerHandlerType.Custom);
                handler.UseInlineHandler(context =>
                {
                    ArgumentNullException.ThrowIfNull(context);

                    Console.WriteLine("ValidateTokenRequestContext inline handler");
                    //context.Request!.RedirectUri = "https://oauth.pstmn.io/v1/callback2";

                    return default;
                });

                //Console.WriteLine("ValidateTokenRequestContext EventHandler");
            })
            .AddEventHandler<HandleAuthorizationRequestContext>(handler =>
            {
                handler.UseInlineHandler(context =>
                {
                    ArgumentNullException.ThrowIfNull(context);

                    Console.WriteLine("HandleAuthorizationRequestContext inline handler");
                    //context.Request!.RedirectUri = "https://oauth.pstmn.io/v1/callback2";

                    return default;
                });

                //Console.WriteLine("HandleAuthorizationRequestContext EventHandler");
            })           
            .AddEventHandler<ApplyAuthorizationResponseContext>(handler =>
            {
                handler.UseInlineHandler(context =>
                {
                    ArgumentNullException.ThrowIfNull(context);

                    Console.WriteLine("ApplyAuthorizationResponseContext inline handler");
                    //context.Request!.RedirectUri = "https://oauth.pstmn.io/v1/callback2";

                    return default;
                });

                //Console.WriteLine("ApplyAuthorizationResponseContext EventHandler");
            })
            .AddEventHandler<ApplyTokenResponseContext>(handler =>
            {
                handler.UseInlineHandler(context =>
                {
                    ArgumentNullException.ThrowIfNull(context);

                    Console.WriteLine("ApplyTokenResponseContext inline handler");
                    //context.Request!.RedirectUri = "https://oauth.pstmn.io/v1/callback2";

                    return default;
                });

                //Console.WriteLine("ApplyTokenResponseContext EventHandler");
            });

            return builder;
        }
    }

    public sealed class CustomValidateClientRedirectUri : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>
    {
        private readonly IOpenIddictApplicationManager _applicationManager;

        //
        // Summary:
        //     Gets the default descriptor definition assigned to this handler.
        public static OpenIddictServerHandlerDescriptor Descriptor { get; } =
            OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.ValidateAuthorizationRequestContext>()
            .UseScopedHandler<CustomValidateClientRedirectUri>()
            .SetOrder(ValidateResponseType.Descriptor.Order + 1000)
            .SetType(OpenIddictServerHandlerType.Custom)
            .Build();

        public CustomValidateClientRedirectUri()
        {
            throw new InvalidOperationException(OpenIddictResources.GetResourceString("ID0016"));
        }

        public CustomValidateClientRedirectUri(IOpenIddictApplicationManager applicationManager)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException("applicationManager");
        }

        public async ValueTask HandleAsync(OpenIddictServerEvents.ValidateAuthorizationRequestContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
        }
    }

    /// <summary>
    /// Contains the logic responsible of rejecting authorization requests that don't specify a valid prompt parameter.
    /// </summary>
    public class ValidatePromptParameter : IOpenIddictServerHandler<ValidateAuthorizationRequestContext>
    {
        /// <summary>
        /// Gets the default descriptor definition assigned to this handler.
        /// </summary>
        public static OpenIddictServerHandlerDescriptor Descriptor { get; }
            = OpenIddictServerHandlerDescriptor.CreateBuilder<ValidateAuthorizationRequestContext>()
                .UseSingletonHandler<ValidatePromptParameter>()
                .SetOrder(ValidateNonceParameter.Descriptor.Order + 1_000)
                .SetType(OpenIddictServerHandlerType.BuiltIn)
                .Build();

        /// <inheritdoc/>
        public ValueTask HandleAsync(ValidateAuthorizationRequestContext context)
        {
            context.Logger.LogInformation("working");

            ArgumentNullException.ThrowIfNull(context);

            // Reject requests specifying prompt=none with consent/login or select_account.
            if (context.Request.HasPrompt(Prompts.None) && (context.Request.HasPrompt(Prompts.Consent) ||
                                                            context.Request.HasPrompt(Prompts.Login) ||
                                                            context.Request.HasPrompt(Prompts.SelectAccount)))
            {
                context.Logger.LogInformation("working");

                return default;
            }

            return default;
        }
    }

    #region
    //public class ClientModifierEventHandler
    //{        
    //    public static void ModifyClientValues2(OpenIddictServerHandlerDescriptor.Builder<ValidateAuthorizationRequestContext> configuration)
    //    {
    //        configuration.UseScopedHandler<CustomEventHandler>();
    //        configuration.Build();
    //    }
        
    //    public static void ModifyClientValues3(OpenIddictServerHandlerDescriptor.Builder<ApplyAuthorizationResponseContext> configuration)
    //    {
    //        configuration.UseScopedHandler<CustomAuthorizationResponseEventHandler>();
    //        configuration.Build();
    //    }
    //}

    //public class CustomEventHandler : IOpenIddictServerHandler<ValidateAuthorizationRequestContext>
    //{
    //    //readonly ILogger<CustomEventHandler> _logger;

    //    //public CustomEventHandler(ILogger<CustomEventHandler> logger)
    //    //{
    //    //    _logger = logger;
    //    //}

    //    public async ValueTask HandleAsync(ValidateAuthorizationRequestContext context)
    //    {
    //        //_logger.LogInformation(JsonSerializer.Serialize(context.Request));
    //        Console.WriteLine("handle ValidateAuthorizationRequestContext");


    //       //return new ValueTask();
    //        //throw new NotImplementedException();
    //    }
    //} 
    
    //public class CustomAuthorizationResponseEventHandler : IOpenIddictServerHandler<ApplyAuthorizationResponseContext>
    //{
    //    //readonly ILogger<CustomEventHandler> _logger;

    //    //public CustomEventHandler(ILogger<CustomEventHandler> logger)
    //    //{
    //    //    _logger = logger;
    //    //}

    //    public async ValueTask HandleAsync(ApplyAuthorizationResponseContext context)
    //    {
    //        //_logger.LogInformation(JsonSerializer.Serialize(context.Request));
    //        Console.WriteLine("handle ApplyAuthorizationResponseContext");


    //       //return new ValueTask();
    //        //throw new NotImplementedException();
    //    }
    //}

#endregion
}
