using KorpArhivs.Areas.Identity;
using KorpArhivs.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KorpArhivs
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsInRole", policy =>
                    policy.RequireRole("User", "Editor", "Administrator"));
                options.AddPolicy("IsUser", policy =>
                    policy.RequireRole("User"));
                options.AddPolicy("IsEditor", policy =>
                    policy.RequireRole("Editor"));
                options.AddPolicy("IsAdministrator", policy =>
                    policy.RequireRole("Administrator"));
            });

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/", "IsInRole");

                options.Conventions.AuthorizePage("/Users", "IsAdministrator");

                options.Conventions.AuthorizePage("/EditEvent", "IsAdministrator");
                //options.Conventions.AuthorizePage("/EditEvent", "IsEditor");

                options.Conventions.AuthorizePage("/DeleteEventConfirmation", "IsAdministrator");
                //options.Conventions.AuthorizePage("/DeleteEventConfirmation", "IsEditor");

                options.Conventions.AuthorizeFolder("/Identity/Account/Manage");

                options.Conventions.AllowAnonymousToPage("/index");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
