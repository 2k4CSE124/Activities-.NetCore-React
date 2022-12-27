using Application.Activities;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Persistence;
using AutoMapper;
using API.Extensions;

namespace API
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });


            // Call setting method from Extensions/ApplicationServiceExtensions.cs
            services.AddApplicationServices(Configuration);

            // // Add Connection String for SQlite
            // services.AddDbContext<DataContext>(opt =>
            // {
            //     opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            // });

            // // Cross-origin resource sharing (CORS) is a standard mechanism that allows 
            // // JavaScript XMLHttpRequest (XHR) calls executed in a web page to interact with 
            // // resources from non-origin domains.
            // services.AddCors(opt =>
            // {
            //     opt.AddPolicy("CorsPolicy", policy =>
            //     {
            //         policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000");
            //     });
            // });

            // services.AddMediatR(typeof(List.Handler));
            // services.AddAutoMapper(typeof(MappingProfiles).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Dispose all at end of execution
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<DataContext>();
                    await context.Database.MigrateAsync();
                    await Seed.SeedData(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occured during migration");
                }
            }
        }
    }
}
