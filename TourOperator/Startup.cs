using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TourOperator.Models;
using TourOperator.Repositories;
using TourOperator.Repositories.Interfaces;
using TourOperator.Services;
using TourOperator.Services.Interfaces;

namespace TourOperator
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

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            });

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddControllersWithViews();
            services.AddRazorPages();

            //Register Services
            services.AddTransient<IHotelService, HotelService>();
            services.AddTransient<IHotelTypeService, HotelTypeService>();
            services.AddTransient<IBookingRepository, BookingRepository>();
            //Register Repo
            services.AddTransient<IHotelRepository, HotelRepository>();
            services.AddTransient<IHotelTypeRepository, HotelTypeRepository>();
            services.AddTransient<IBookingService, BookingService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            CreateRoles(serviceProvider).Wait();
            Initialize(serviceProvider).Wait();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Hotel}/{action=Overview}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            //initializing custom roles 
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roleNames = { "Admin", "CustomerSupport", "ContentCreator" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    //create the roles and seed them to the database: Question 1
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

        }

        public async Task Initialize(IServiceProvider services)
        {

            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {

                var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var user = new ApplicationUser { UserName = "admin@gmail.com" ,Email = "admin@gmail.com", NormalizedEmail="ADMIN@GMAIL.COM" ,  };

                var _user = await manager.FindByEmailAsync("admin@gmail.com");
                if (_user == null)
                {
                    var result = await manager.CreateAsync(user, "Admin_123");
                    await manager.AddToRoleAsync(user, "Admin");

                }

              
            }
        }
    }
}
