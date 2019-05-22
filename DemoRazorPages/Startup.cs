using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
            });


            services.AddRazorPages((options) => { options.RootDirectory = "/T1"; })
                .AddNewtonsoftJson();

            Tenant1ServiceProvider = services.BuildServiceProvider();

            services.AddRazorPages((options) => { options.RootDirectory = "/T2"; })
                .AddNewtonsoftJson();

            Tenant2ServiceProvider = services.BuildServiceProvider();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.MapWhen((c) =>
            {
                return c.Request.Path == "/t1";
            }, (branched) =>
            {
                branched.ApplicationServices = Tenant1ServiceProvider;
                branched.Use((con, next) =>
                {
                    var scope = Tenant1ServiceProvider.CreateScope();
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
                branched.UseAuthorization();
                branched.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });
            });




        }
    }
}
