using Arch.EntityFrameworkCore.UnitOfWork;
using aspnetauthentication.Extensions;
using aspnetauthentication.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Storage;
using System;
using System.Linq;
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

            services.AddCors();
            services.AddControllers();
            services.AllowAuthentication(false, false, true, c => _config.GetSection(nameof(JwtBearerConfig)).Bind(c));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            RunPendingMigrations(app.ApplicationServices).GetAwaiter().GetResult();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
