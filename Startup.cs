using Arch.EntityFrameworkCore.UnitOfWork;
using aspnetauthentication.Extensions;
using aspnetauthentication.Models;
using aspnetauthentication.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Storage;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace aspnetauthentication
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(option =>
            {
                option.UseNpgsql(_config.GetConnectionString("DbConnection"));
            },ServiceLifetime.Transient).AddUnitOfWork<ApplicationDbContext>();
            services.Configure<SwaggerDocConfig>(c => _config.GetSection(nameof(SwaggerDocConfig)).Bind(c));
            services.AddCors();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
                    new BadRequestApiResponse("Model validation errors") 
                    { 
                        Errors = context.GetError()
                    });
            });

            services.AllowAuthentication(false, false, true, c => _config.GetSection(nameof(JwtBearerConfig)).Bind(c));
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",new OpenApiInfo 
                { 
                    Title = ".NET CORE API 3.1 AUTHENTICATION",
                    Description = "Authentication in .NET CORE 3.1",
                    Version= "v1"
                });

                options.EnableAnnotations();

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            RunPendingMigrations(app.ApplicationServices).GetAwaiter().GetResult();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var swaggerDocConfig = app.ApplicationServices.GetService<IOptions<SwaggerDocConfig>>().Value;

            if (swaggerDocConfig.ShowSwaggerUI)
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "3.1 AUTHENTICATION");
                    if (!swaggerDocConfig.EnableSwaggerTryIt)
                    {
                        c.SupportedSubmitMethods(new SubmitMethod[] { });
                    }
                });

            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static async Task RunPendingMigrations(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();

                    var migrationsCount = (await dbContext.Database.GetPendingMigrationsAsync()).Count();

                    if (migrationsCount > 0)
                    {
                        await dbContext.Database.MigrateAsync();
                    }
                }
                catch (Exception)
                {

                    // TODO: add logging
                }
            }
        }
    }
}
