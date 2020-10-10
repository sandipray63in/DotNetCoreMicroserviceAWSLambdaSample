using Domain.Base.Aggregates;
using Domain.Base.Entities;
using DomainServices.Base.CommandDomainServices;
using DomainServices.Base.QueryableDomainServices;
using Infrastructure.ExceptionHandling;
using Infrastructure.ExceptionHandling.PollyBasedExceptionHandling;
using Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Repository;
using Repository.Base;
using Repository.Command;
using Repository.Queryable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestfulWebAPINetCore
{
    public abstract partial class BaseStartup<TDBContext, TEntity, TId>
        where TDBContext : DbContext
        where TEntity : BaseEntity<TId>, ICommandAggregateRoot, IQueryableAggregateRoot
        where TId : struct
    {
        public BaseStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        protected abstract string ConnectionStringName { get; }

        protected abstract string SwaggerDocTitle { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            ConfigureLoggingService(services);

            services.AddSingleton<IPolicy, RetryNTimesPolicy>(x => new RetryNTimesPolicy(x.GetRequiredService<ILogger>(), 3));
            services.AddSingleton<IExceptionHandler, BasicPollyExceptionHandler>(x => new BasicPollyExceptionHandler(new IPolicy[] { x.GetRequiredService<IPolicy>() }, x.GetRequiredService<ILogger>(), true));

            services.AddDbContext<TDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString(ConnectionStringName)));
            services.AddScoped<DbContext, TDBContext>();
            services.AddScoped<ICommand<TEntity>, EntityFrameworkCodeFirstCommand<TEntity, TId>>();
            services.AddScoped<IQuery<TEntity>, EntityFrameworkCodeFirstQueryable<TEntity>>();
            services.AddScoped<ICommandRepository<TEntity>, CommandRepository<TEntity>>();
            services.AddScoped<IQueryableRepository<TEntity>, QueryableRepository<TEntity>>();
            services.AddScoped<ICommandDomainServiceAsync<TEntity>, CommandDomainServiceAsync<TEntity>>();
            services.AddScoped<IQueryableDomainService<TEntity, TId>, QueryableDomainService<TEntity, TId>>();
            services.AddScoped<ITokenManager, TokenManager>();
            services.AddHttpContextAccessor();

            ConfigureJwtAuthService(services);
            ConfigureDistributedCachingService(services);
            ConfigureSwaggerService(services);
            ConfigureOtherServices(services);
        }

        public void Configure(IApplicationBuilder app, ILogger logger, TDBContext context)
        {
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature.Error;
                    logger.LogError(exception, exception.Message);
                    var exceptionMessage = JsonSerializer.Serialize(new { Error = exception.Message });
                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusExceptionMappingUtility.GetHttpStatusCode(exception);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(exceptionMessage);
                });
            });
            app.UseMiddleware<AuditMiddleware>();

            context.Database.EnsureCreated();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        private void ConfigureDistributedCachingService(IServiceCollection services)
        {
            var redisConfig = Configuration.GetSection("Redis");
            var configurationEndpoint = redisConfig["ConfigurationEndpoint"];
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configurationEndpoint;
                options.InstanceName = "UserLoans";
            });
        }

        private void ConfigureSwaggerService(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = SwaggerDocTitle,
                    Description = "A simple example of ASP.NET Core Web API",
                    Contact = new OpenApiContact
                    {
                        Name = "Sandip Ray",
                        Email = string.Empty
                    }
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

        protected abstract void ConfigureLoggingService(IServiceCollection services);

        protected virtual void ConfigureOtherServices(IServiceCollection services) { }
    }
}
