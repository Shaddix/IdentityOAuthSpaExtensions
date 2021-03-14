using IdentityOAuthSpaExtensions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityOAuthSpaExtensions.Example
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

            services.AddDbContext<IdentityContext>(options => options
                .UseInMemoryDatabase("OAuthTest"));

            services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Lockout.AllowedForNewUsers = false;
                })
                .AddRoles<IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiScopes(Configuration.GetSection("IdentityServer:ApiScopes"))
                .AddInMemoryApiResources(Configuration.GetSection("IdentityServer:ApiResources"))
                .AddInMemoryClients(Configuration.GetSection("IdentityServer:Clients"))
                .AddExtensionGrantValidator<
                    ExternalAuthenticationGrantValidator<IdentityUser, string>>();
            ;

            services.AddAuthentication()
                .AddGoogle(options => { Configuration.GetSection("Google").Bind(options); })
                .AddFacebook(options => { Configuration.GetSection("Facebook").Bind(options); })
                .AddMicrosoftAccount(options =>
                {
                    Configuration.GetSection("Microsoft").Bind(options);
                })
                .AddGitHub(options => { Configuration.GetSection("GitHub").Bind(options); })
                .AddTwitter(options => { Configuration.GetSection("Twitter").Bind(options); })
                .AddOpenIdConnect(options => Configuration.Bind("AzureAd", options));
            ;
            services.ConfigureExternalAuth();

            // if you want to secure some controllers/actions within the same project with JWT
            // you need to configure something like the following
            services.AddAuthentication(o =>
                {
                    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(
                    options =>
                    {
                        options.Authority =
                            Configuration.GetSection("Auth")["PublicHost"]; // this is a public host
                        options.RequireHttpsMetadata = false;
                        options.Audience = "local";
                    })
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            //ToDo: fix that
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHttpsRedirection();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();

            app.UseExternalAuth();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseIdentityServer();


            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}