using CB.Blazor.CMS;
using CB.Blazor.CMS.Contracts;
using CB.Blazor.CMS.Mappers;
using CB.Blazor.CMS.Mappers.Contracts;
using CB.Blazor.Infrastructure.Cache;
using CB.Blazor.Infrastructure.Configuration;
using CB.Blazor.Infrastructure.Repositories.SquidexRepo;
using CB.Blazor.Infrastructure.Repositories.SquidexRepo.Contracts;
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Mime;

namespace CB.Blazor.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            services.AddMvc();

            // Adds the Server-Side Blazor services, and those registered by the app project's startup.
            // This doesn't hurt client side
            services.AddServerSideBlazor<App.Startup>();

            Configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            services.Configure<SquidexConfig>(options => Configuration.GetSection("Squidex").Bind(options));
            services.Configure<CacheConfig>(options => Configuration.GetSection("Cache").Bind(options));
            services.Configure<EmailConfig>(options => Configuration.GetSection("Email").Bind(options));
            services.Configure<LoggingConfig>(options => Configuration.GetSection("Logging").Bind(options));

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });

            //cache
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

            services.AddSingleton<ICMSMapper, CMSMapper>();
            services.AddSingleton<ISquidexRepo, SquidexRepo>();
            services.AddSingleton<ICMSService, CMSService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();
            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                // For GetTypedHeaders, add: using Microsoft.AspNetCore.Http;
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromHours(8)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };

                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action}/{id?}");
            });

            // This will also serve a client side app, it just registers SignalR in addition
            app.UseServerSideBlazor<App.Startup>();
        }
    }
}
