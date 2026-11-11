using System.Globalization;
using System.Reflection;
using FlexDesk.Application.Authentication;
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

        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
