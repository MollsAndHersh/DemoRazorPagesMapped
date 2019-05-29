using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DemoRazorPages
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IServiceProvider Tenant1ServiceProvider { get; set; }
        public IServiceProvider Tenant2ServiceProvider { get; set; }

        public bool RunInMappedPipeline { get; set; } = true;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterApplicationServices(services);
            Tenant1ServiceProvider = RegisterTenantServices(new ServiceCollection(), "/T1").BuildServiceProvider();
            Tenant2ServiceProvider = RegisterTenantServices(new ServiceCollection(), "/T2").BuildServiceProvider();
        }

        public void RegisterApplicationServices(IServiceCollection services)
        {

            if (!RunInMappedPipeline) // We are going to run razor pages in normal application request pipeline.
            {
                services.AddLogging();
                services.AddRouting();
                services.AddRazorPages().AddNewtonsoftJson();
            }
        }

        public IServiceCollection RegisterTenantServices(IServiceCollection services, string razorPath)
        {
            if(RunInMappedPipeline)
            {
                services.AddLogging();
                services.AddRouting();
                services.AddRazorPages((options) => { options.RootDirectory = razorPath; })
              .AddNewtonsoftJson();

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }

            return services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //  app.UseHttpsRedirection();
            app.UseStaticFiles();

            if (!RunInMappedPipeline)
            {
                app.UseRouting();
                app.Use((con, next) =>
                {
                    var sp = con.RequestServices;
                    // Note part manager has 4 parts.
                    var partManager = sp.GetRequiredService<ApplicationPartManager>();
                    return next.Invoke();
                });
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });
                return;
            }


            app.MapWhen((c) =>
            {
                return c.Request.Path == "/t1";
            }, (branched) =>
            {
                branched.ApplicationServices = Tenant1ServiceProvider;
                branched.Use((con, next) =>
                {
                    var scope = Tenant1ServiceProvider.CreateScope();
                    con.RequestServices = scope.ServiceProvider;
                    // con.
                    con.Response.RegisterForDispose(scope);
                    return next.Invoke();
                });
                branched.Use((con, next) =>
                {
                    Console.WriteLine("Tenant 1 Request Services set..");
                    return next.Invoke();
                });
                branched.UseRouting();
                branched.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    //    var sp = endpoints.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    //    var httpContextServices = sp.HttpContext?.RequestServices;
                    // // endpoints.ServiceProvider = httpContextServices;

                    //    foreach (var dataSource in endpoints.DataSources)
                    //    {
                    //        foreach (var endpoint in dataSource.Endpoints)
                    //        {
                    //            var name = endpoint.DisplayName;
                    //            foreach (var meta in endpoint.Metadata)
                    //            {
                    //                var objString = meta.ToString();
                    //            }
                    //        }
                    //    }
                });

                branched.Use((con, next) =>
                {
                    var sp = con.RequestServices;
                    var partManager = sp.GetRequiredService<ApplicationPartManager>();
                    return next.Invoke();
                });
            });

            app.MapWhen((c) =>
            {
                return c.Request.Path == "/t2";
            }, (branched) =>
            {
                branched.ApplicationServices = Tenant2ServiceProvider;
                branched.Use((con, next) =>
                {
                    var scope = Tenant2ServiceProvider.CreateScope();
                    // con.
                    con.Response.RegisterForDispose(scope);
                    con.RequestServices = scope.ServiceProvider;
                    return next.Invoke();
                });
                branched.Use((con, next) =>
                {
                    Console.WriteLine("Tenant 2 Request Services set..");
                    return next.Invoke();
                });
                branched.UseRouting();
                branched.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    //var sp = endpoints.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    //var httpContextServices = sp.HttpContext?.RequestServices;
                    //// endpoints.ServiceProvider = httpContextServices;

                    //foreach (var dataSource in endpoints.DataSources)
                    //{
                    //    foreach (var endpoint in dataSource.Endpoints)
                    //    {
                    //        var name = endpoint.DisplayName;
                    //        foreach (var meta in endpoint.Metadata)
                    //        {
                    //            var objString = meta.ToString();
                    //        }
                    //    }
                    //}
                });


                branched.Use((con, next) =>
                {
                    var sp = con.RequestServices;
                    var partManager = sp.GetRequiredService<ApplicationPartManager>();
                    return next.Invoke();
                });
            });






        }
    }
}
