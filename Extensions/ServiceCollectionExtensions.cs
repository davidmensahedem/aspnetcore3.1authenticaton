using aspnetauthentication.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace aspnetauthentication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AllowAuthentication(this IServiceCollection services,
            bool allowBasicAuth = false,
            bool allowPrivateAuth = false,
            bool allowBearer = false,
            Action<JwtBearerConfig> jwtBearerAction = null
            )
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (allowBearer)
            {
                if(jwtBearerAction == null){
                    throw new ArgumentNullException(nameof(jwtBearerAction));
                }

                var jwtBearerConfig = new JwtBearerConfig();
                jwtBearerAction.Invoke(jwtBearerConfig);

                services.AddAuthentication(s =>{
                    s.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    s.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(jwtOptions => {

                    jwtOptions.Events = new JwtBearerEvents(){
                        OnTokenValidated = async (ctx) => 
                        {
                            var userPhoneNumber = ctx.Principal.FindFirst(c => c.Type == ClaimTypes.MobilePhone)?.Value;
                            var userBearerToken = ctx.HttpContext.Request.Headers["Authorization"][0].Split(new []{' '})[1];

                            await Task.Delay(0);
                            //TODO:
                            // get user by phone number and token
                            // if no user found return 

                            // create claims
                            // create Identity
                            // add Identity to the principal


                        }   
                    };

                    jwtOptions.SaveToken = true;

                    jwtOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtBearerConfig.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtBearerConfig.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearerConfig.SigningKey)),
                        ValidateLifetime = false
                    };


                });
            }

            throw new Exception();

        }
    }
}
