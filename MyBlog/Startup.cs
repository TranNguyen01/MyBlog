using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoBlog.Services;
using MyBlog.Models;
using MyBlog.Service;
using MyBlog.Services;
using MyBlog.Utilities;
using StackExchange.Redis;
using System;
using System.Text.Json.Serialization;
using static MyBlog.Utilities.SendMailService;

namespace MyBlog
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
            services.AddOptions();
            var mailsetting = Configuration.GetSection("MailSettings");
            services.Configure<MailSettings>(mailsetting);
            services.AddSingleton<IEmailSender, SendMailService>();


            services.AddControllersWithViews()
                 .AddNewtonsoftJson(options =>
                 {
                     options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                 })
                .AddCookieTempDataProvider();


            services.AddSession();


            services.AddDbContext<AppDbContext>(options =>
            {
                string SqlConnectionString = Configuration.GetConnectionString("MsSqlConnectionString");
                options.UseSqlServer(SqlConnectionString);
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Thiết lập về Password
                options.Password.RequireDigit = false; // Không bắt phải có số
                options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 6; // Số ký tự tối thiểu của password
                options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
                options.Lockout.AllowedForNewUsers = true;

                //// Cấu hình về User.
                //options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                //    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất


                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                options.SignIn.RequireConfirmedAccount = true;

            });



            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login/";
                options.LogoutPath = "/logout/";
                options.AccessDeniedPath = "/Notfound";
            });

            //Redis cache
            services.AddStackExchangeRedisCache(option =>
                option.Configuration = Configuration.GetConnectionString("RedisConnectionString")
            );
            services.AddSingleton<IResponseCacheService, ResponseCacheService>();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(Configuration.GetConnectionString("RedisConnectionString"))
            );
            services.AddSingleton<IElasticsearch, ElasticSearch>();

            services.AddSingleton<IMinIOService, MinIOService>();

            services.AddSingleton(s =>
            {
                var cloud = Configuration.GetSection("Cloudinary").GetValue(typeof(string), "cloud").ToString();
                var apiKey = Configuration.GetSection("Cloudinary").GetValue(typeof(string), "apiKey").ToString();
                var apiSecret = Configuration.GetSection("Cloudinary").GetValue(typeof(string), "apiSecret").ToString();
                return new Cloudinary(new Account(cloud, apiKey, apiSecret));
            });
            //services.AddSingleton(new Cloudinary(new Account("hehohe", "835266539265652", "T_HlU-UeEDX1OxnzhMLnGvlrunA")));
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
