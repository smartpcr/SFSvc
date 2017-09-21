
namespace FullApi
{
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.PlatformAbstractions;
    using Swashbuckle.AspNetCore.Swagger;

    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// duplicated configuration code, figure out a way to merge them
        /// </summary>
        /// <param name="env"></param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            builder.AddAzureKeyVault(
                $"https://{Configuration["azureKeyVault:vault"]}.vault.azure.net/",
                Configuration["azureKeyVault:clientId"],
                Configuration["azureKeyVault:clientSecret"]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    Configuration["swagger:version"],
                    new Info
                    {
                        Title = Configuration["swagger:title"],
                        Version = Configuration["swagger:version"],
                        Description = Configuration["swagger:description"],
                        TermsOfService = Configuration["swagger:termsOfService"],
                        Contact = new Contact()
                        {
                            Email = Configuration["swagger:contact:email"],
                            Name = Configuration["swagger:contact:name"],
                            Url = Configuration["swagger:contact:url"]
                        },
                        License = new License()
                        {
                            Name = Configuration["swagger:license:name"],
                            Url = Configuration["swagger:license:url"]
                        }
                    });

                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlDocumentFile = Path.Combine(basePath, Configuration["swagger:xmlDocumentFile"]);
                c.IncludeXmlComments(xmlDocumentFile);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(Configuration["swagger:swaggerEndpoint"], Configuration["swagger:title"]);
            });

            app.UseMvc();
        }
    }
}
