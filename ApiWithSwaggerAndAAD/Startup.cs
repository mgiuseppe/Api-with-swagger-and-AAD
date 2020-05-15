using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag.Generation.Processors.Security;

namespace ApiWithSwaggerAndAAD
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // API
            // Questo è generico e funziona anche se non hai AAD
            // services.AddAuthentication(opts => opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
            //    {
            //        opts.Authority = $"https://login.microsoftonline.com/acfefe3d-0f49-415f-a7d8-57050a01e985"; // authority to use for oidc calls
            //        opts.Audience = "api://b1e43d8e-5c99-47e1-8adb-1967abfac058/test-api"; // application ID of api
            //    });
            // Questo è specifico per AAD e ha un pò più opzioni di quello di sopra
            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                    .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));

            // SWAGGER UI
            services.AddOpenApiDocument(document =>
            {
                // Con questo usa l'implicit flow (ovvero prende id_token e access_token direttamente dall'authorization endpoint senza avere un client secret) per usare questo in AAD devi abilitare l'implicit flow nell'app registration
                document.AddSecurity("bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
                {
                    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
                    Flows = new NSwag.OpenApiOAuthFlows()
                    {
                        Implicit = new NSwag.OpenApiOAuthFlow()
                        {
                            Scopes = new Dictionary<string, string>
                            {
                                //{ "api://b1e43d8e-5c99-47e1-8adb-1967abfac058/my-api/user_impersonation", "Access the api as the signed-in user" },
                                { "api://b1e43d8e-5c99-47e1-8adb-1967abfac058/test-api/Invoice.Read", "Read access to the API"},
                                { "api://b1e43d8e-5c99-47e1-8adb-1967abfac058/test-api/Products.Read", "Let's find out together!"}
                            },
                            AuthorizationUrl = "https://login.microsoftonline.com/acfefe3d-0f49-415f-a7d8-57050a01e985/oauth2/v2.0/authorize",
                            TokenUrl = "https://login.microsoftonline.com/acfefe3d-0f49-415f-a7d8-57050a01e985/oauth2/v2.0/token"
                        },
                    }
                });

                // Questo usa l'authorization code ma mi dà problemi col CORS (Per usare questo devi creare un client_secret nell'app registration su AAD e aggiungerlo sotto nella configurazione del middleware di swagger ui)
                // document.AddSecurity("bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
                // {
                //     Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
                //     Flows = new NSwag.OpenApiOAuthFlows()
                //     {
                //         AuthorizationCode = new NSwag.OpenApiOAuthFlow()
                //         {
                //             Scopes = new Dictionary<string, string>
                //             {
                //                  { "api://b1e43d8e-5c99-47e1-8adb-1967abfac058/test-api/Invoice.Read", "Read access to the API"},
                //                  { "api://b1e43d8e-5c99-47e1-8adb-1967abfac058/test-api/Products.Read", "Let's find out together!"}
                //             },
                //             AuthorizationUrl = "https://login.microsoftonline.com/acfefe3d-0f49-415f-a7d8-57050a01e985/oauth2/v2.0/authorize",
                //             TokenUrl = "https://login.microsoftonline.com/acfefe3d-0f49-415f-a7d8-57050a01e985/oauth2/v2.0/token"
                //         },
                //     }
                // });

                // con questo ti fa specificare un api key che poi aggiunge agli header
                // document.AddSecurity("bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
                // {
                //     Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                //     Name = "Authorization",
                //     Description = "Copy 'Bearer ' + valid JWT token into field",
                //     In = NSwag.OpenApiSecurityApiKeyLocation.Header
                // }

                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // additions for OpenAPI and SwaggerUI
            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.OAuth2Client = new NSwag.AspNetCore.OAuth2ClientSettings
                {
                    ClientId = "3ab70da9-4439-48e2-9b4f-fd0fe41a563d",
                    // Non serve per implicit flow ma serve per authorization code
                    // ClientSecret = "-O.DJix4j3DwPVrmW9~8b8-Yh-0~EG2K2Y",
                    AppName = "swagger-ui-client"
                };
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
