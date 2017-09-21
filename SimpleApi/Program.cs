// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SimpleApi
{
    using System;
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        private static IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                BuildWebHost(args).Run();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigConfiguration)
                .ConfigureLogging(ConfigLogging)
                .UseKestrel(kestrelServerOptions =>
                {
                    // run callback on the transport thread
                    kestrelServerOptions.ApplicationSchedulingMode = SchedulingMode.Inline;

                    kestrelServerOptions.Listen(
                        IPAddress.Loopback,
                        int.Parse(_configuration["hosting:port"]),
                        listenOptions =>
                        {
                            // Uncomment the following to enable Nagle's algorithm for this endpoint
                            // listenOptions.NoDelay = false;
                            listenOptions.UseConnectionLogging();
                        });

                    // Uncomment the following to enable SSL
                    //kestrelServerOptions.Listen(IPAddress.Loopback, 5001, listenOptions =>
                    //{
                    //    listenOptions.UseHttps("testCert.pfx", "testPassword");
                    //    listenOptions.UseConnectionLogging();
                    //});

                    kestrelServerOptions.UseSystemd();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
        }

        private static void ConfigConfiguration(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("azurekeyvault.json", optional: true, reloadOnChange: true) 
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{webHostBuilderContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = configurationBuilder.Build();

            // Uncomment the following to enable key vault
            //configurationBuilder.AddAzureKeyVault(
            //    $"https://{_configuration["azureKeyVault:vault"]}.vault.azure.net/",
            //    _configuration["azureKeyVault:clientId"],
            //    _configuration["azureKeyVault:clientSecret"]);
        }

        private static void ConfigLogging(WebHostBuilderContext webHostBuilderContext, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConfiguration(webHostBuilderContext.Configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        }
    }
}