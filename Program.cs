using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using Notes.Identity;
using Notes.Identity.Data;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Notes.Identity.Models;
using Microsoft.AspNetCore.Identity;

internal class Program
{
    private static void Main(string[] args)
    {
        var cfgbuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        var configuration = cfgbuilder.Build();
        var connectionString = configuration.GetValue<string>("DbConnection");

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer()
            .AddAspNetIdentity<AppUser>()
            .AddInMemoryApiResources(Configuration.ApiResources)
            .AddInMemoryIdentityResources(Configuration.IdentityResources)
            .AddInMemoryApiScopes(Configuration.ApiScopes)
            .AddInMemoryClients(Configuration.Clients)
            .AddDeveloperSigningCredential();

        builder.Services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddIdentity<AppUser, IdentityRole>(cfg =>
        {
            cfg.Password.RequiredLength = 4;
            cfg.Password.RequireDigit = false;
            cfg.Password.RequireUppercase = false;
            cfg.Password.RequireNonAlphanumeric = false;
        })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(cfg =>
        {
            cfg.Cookie.Name = "Notes.Identity.Cookie";
            cfg.LoginPath = "/Auth/Login";
            cfg.LogoutPath = "/Auth/Logout";
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            try
            {
                var context = serviceProvider.GetRequiredService<AuthDbContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, ex.Message);
            }
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseIdentityServer();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}