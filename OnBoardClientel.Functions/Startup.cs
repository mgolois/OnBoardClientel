using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using OnBoardClientel.Functions.Services;
using Microsoft.EntityFrameworkCore;

[assembly: FunctionsStartup(typeof(OnBoardClientel.Functions.Startup))]

namespace OnBoardClientel.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddDbContext<OnBoardClientelContext>(option =>
                                option.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString")));
        }
    }
}
