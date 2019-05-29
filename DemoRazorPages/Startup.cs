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
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //});
            // services.AddRouting();

            if (!RunInMappedPipeline)
            {
                services.AddRazorPages()
                .AddNewtonsoftJson();
                return;
            }


            var tenant1Services = new ServiceCollection();
            tenant1Services.AddLogging();
            tenant1Services.AddRouting();
            tenant1Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var tenant2Services = new ServiceCollection();
            tenant2Services.AddRouting();
            tenant2Services.AddLogging();
            tenant2Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            tenant1Services.AddRazorPages((options) => { options.RootDirectory = "/T1"; })
                .AddNewtonsoftJson();

            //tenant1Services.AddRazorPages()
            //   .AddNewtonsoftJson();

            Tenant1ServiceProvider = tenant1Services.BuildServiceProvider();

            tenant2Services.AddRazorPages((options) => { options.RootDirectory = "/T2"; })
                .AddNewtonsoftJson();

            Tenant2ServiceProvider = tenant2Services.BuildServiceProvider();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    // app.UseHsts();
            //}

            //  app.UseHttpsRedirection();
            app.UseStaticFiles();

            //app.UseCookiePolicy();

            if (!RunInMappedPipeline)
            {
                app.UseRouting();
                app.UseAuthorization();
                app.Use((con, next) =>
                {
                    var sp = con.RequestServices;
                    var partManager = sp.GetRequiredService<ApplicationPartManager>();
                    return next.Invoke();
                });
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    var sp = endpoints.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var httpContextServices = sp.HttpContext?.RequestServices;
                    // endpoints.ServiceProvider = httpContextServices;

                    foreach (var dataSource in endpoints.DataSources)
                    {
                        foreach (var endpoint in dataSource.Endpoints)
                        {
                            var name = endpoint.DisplayName;
                            foreach (var meta in endpoint.Metadata)
                            {
                                var objString = meta.ToString();
                            }
                        }
                    }
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
                branched.UseAuthorization();
                branched.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    var sp = endpoints.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var httpContextServices = sp.HttpContext?.RequestServices;
                    // endpoints.ServiceProvider = httpContextServices;

                    foreach (var dataSource in endpoints.DataSources)
                    {
                        foreach (var endpoint in dataSource.Endpoints)
                        {
                            var name = endpoint.DisplayName;
                            foreach (var meta in endpoint.Metadata)
                            {
                                var objString = meta.ToString();
                            }
                        }
                    }
                });


                branched.Use((con, next) =>
                {
                    var sp = con.RequestServices;
                    var partManager = sp.GetRequiredService<ApplicationPartManager>();
                    return next.Invoke();
                });
                //branched.UseEndpointExecutor();
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
                branched.UseAuthorization();

                branched.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    var sp = endpoints.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var httpContextServices = sp.HttpContext?.RequestServices;
                    // endpoints.ServiceProvider = httpContextServices;

                    foreach (var dataSource in endpoints.DataSources)
                    {
                        foreach (var endpoint in dataSource.Endpoints)
                        {
                            var name = endpoint.DisplayName;
                            foreach (var meta in endpoint.Metadata)
                            {
                                var objString = meta.ToString();
                            }
                        }
                    }
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
