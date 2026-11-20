using FlexDesk.Application.Common.Email;
using FlexDesk.Application.Common.Messaging;
using FlexDesk.Application.Common.Persistence;
using FlexDesk.Application.Common.Security;
using FlexDesk.Infrastructure.Email;
using FlexDesk.Infrastructure.Messaging;
using FlexDesk.Infrastructure.Persistence;
using FlexDesk.Infrastructure.Persistence.Seed;
using FlexDesk.Infrastructure.Scheduling;
using FlexDesk.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlexDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<FlexDeskDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<FlexDeskDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<DatabaseSeeder>();

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddHostedService<ReservationConfirmedConsumer>();
        services.AddHostedService<ReservationExpiryService>();

        return services;
    }
}
