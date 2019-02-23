﻿
using IdentityOAuthSpaExtensions.Example.IdentityServer;
using IdentityOAuthSpaExtensions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDbContext<IdentityContext>(options => options
                .UseInMemoryDatabase("OAuthTest"));
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddAspNetIdentity<IdentityUser>()
                .AddExtensionGrantValidator<ExternalAuthenticationGrantValidator<IdentityUser, string>>();
            ;

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "497377001909-v63kflfb7gf26mmug97iinaqr80vr427.apps.googleusercontent.com";
                    options.ClientSecret = "XfnbY7kdOqbAUdDrZoE4juwM";
                })
                .AddFacebook(options =>
                {
                    options.ClientId = "2076005142436006";
                    options.ClientSecret = "0fd775ac8e566f0a113f096ce42cf63a";
                })
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = "ab2ce88f-efef-49c5-b89f-87a87b7dfc2c";
                    options.ClientSecret = "wfileLMHY~_~hcONF32735{";
                })
                .AddGitHub(options =>
                {
                    options.ClientId = "eaf89b3779c82f5a0d65";
                    options.ClientSecret = "baf6e9860dce3dab94a356acd0ce8024d2ee1fd2";
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = "SOxtwARctjMn5ZYouNTcBopMs";
                    options.ConsumerSecret = "f8ZyXWxa7VVdU79cS3KM7hUlx0Z15pmTkCxispLU4JrKkA8B4E";
                })
                
                ;
            services.ConfigureExternalAuth(Configuration);

            // if you want to secure some controllers/actions within the same project with JWT
            // you need to configure something like the following
            services.AddAuthentication(o =>
                {
                    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(
                    options =>
                    {
                        options.Authority = "http://localhost:5000"; // this is a public host
                        options.RequireHttpsMetadata = false;
                        options.Audience = "api1";
                    })
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseIdentityServer();
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}