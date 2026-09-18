using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PersonalFinanceManager.Data;

namespace PersonalFinanceManager
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            var authConnection = new SqlConnectionStringBuilder(
                builder.Configuration.GetConnectionString("DefaultConnection"))
            {
                InitialCatalog = "PersonalFinanceManagerAuthDb"
            }.ConnectionString;
            builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(authConnection));
            builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();
            builder.Services.AddIdentityCore<IdentityUser>(options =>
                options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Login");

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DatabaseInitializer.Initialize(db);
                var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                authDb.Database.EnsureCreated();
                authDb.Database.ExecuteSqlRaw("IF OBJECT_ID(N'[AspNetRoles]',N'U') IS NULL CREATE TABLE AspNetRoles (Id nvarchar(450) NOT NULL PRIMARY KEY,Name nvarchar(256) NULL,NormalizedName nvarchar(256) NULL,ConcurrencyStamp nvarchar(max) NULL); IF OBJECT_ID(N'[AspNetUserRoles]',N'U') IS NULL CREATE TABLE AspNetUserRoles (UserId nvarchar(450) NOT NULL,RoleId nvarchar(450) NOT NULL,CONSTRAINT PK_AspNetUserRoles PRIMARY KEY(UserId,RoleId));");
                var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                if (!await roles.RoleExistsAsync("Admin")) await roles.CreateAsync(new IdentityRole("Admin"));
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
