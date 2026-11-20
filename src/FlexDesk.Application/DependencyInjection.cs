using System.Globalization;
using System.Reflection;
using FlexDesk.Application.Account;
using FlexDesk.Application.Authentication;
using FlexDesk.Application.Incidents;
using FlexDesk.Application.Reservations;
using FlexDesk.Application.Users;
using FlexDesk.Application.Workspaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FlexDesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
