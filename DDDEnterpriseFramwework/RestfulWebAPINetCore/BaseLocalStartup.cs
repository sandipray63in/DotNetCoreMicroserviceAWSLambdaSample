using Domain.Base.Aggregates;
using Domain.Base.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace RestfulWebAPINetCore
{
    public abstract class BaseLocalStartup<TDBContext, TEntity, TId> : BaseStartup<TDBContext, TEntity, TId>
        where TDBContext : DbContext
        where TEntity : BaseEntity<TId>, ICommandAggregateRoot, IQueryableAggregateRoot
        where TId : struct
    {
        public BaseLocalStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureDistributedCachingService(IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "CustomerLoans";
            });
        }

        protected override void ConfigureSwaggerService(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = SwaggerDocTitle,
                    Description = "A simple example of ASP.NET Core Web API",
                    //TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Sandip Ray",
                        Email = string.Empty,
                        //Url = new Uri("https://twitter.com/spboyer"),
                    }
                    //License = new OpenApiLicense
                    //{
                    //   // Name = "Use under LICX",
                    //    //Url = new Uri("https://example.com/license"),
                    //}
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.OperationFilter<SwaggerAuthorizeCheckOperationFilter>();
            });
        }

        protected override void ConfigureSwaggerUse(IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
