using System.Text;
using FlexDesk.Application.Common.Security;
using FlexDesk.Domain.Entities;
using FlexDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FlexDesk.Infrastructure.Persistence.Seed;

public class DatabaseSeeder
{
    private static readonly string[] FirstNames =
    [
        "Anna", "Piotr", "Katarzyna", "Tomasz", "Magdalena", "Paweł", "Agnieszka", "Michał", "Joanna", "Marcin",
        "Aleksandra", "Łukasz", "Natalia", "Krzysztof", "Zofia", "Jakub", "Karolina", "Grzegorz", "Weronika", "Rafał",
        "Monika", "Adam", "Ewa", "Bartosz", "Julia",
    ];

    private static readonly string[] LastNames =
    [
        "Nowak", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński", "Lewandowski", "Zieliński", "Szymański", "Woźniak",
        "Dąbrowski", "Kozłowski", "Jankowski", "Mazur", "Krawczyk", "Piotrowski", "Grabowski", "Pawłowski", "Michalski",
        "Nowakowski", "Wieczorek", "Zając", "Król", "Majewski", "Olszewski", "Jaworski",
    ];

    private static readonly string[] WorkspaceNames =
    [
        "Orion", "Kaskada", "Bursztyn", "Panorama", "Zenit", "Laguna", "Horyzont", "Cytadela", "Mozaika", "Feniks",
        "Atrium", "Nebula", "Przystań", "Kometa", "Lazur", "Szafir", "Aurora", "Nimbus",
    ];

    private static readonly string[] Cities = ["Kraków", "Warszawa", "Wrocław", "Gdańsk", "Poznań"];

    private static readonly string[] Streets =
        ["Długa", "Floriańska", "Grodzka", "Marszałkowska", "Świdnicka", "Piotrkowska", "Półwiejska", "Mariacka"];

    private static readonly string[] IncidentDescriptions =
    [
        "Nie działa rzutnik w sali.", "Zepsute krzesło biurowe.", "Brak dostępu do internetu.",
        "Klimatyzacja pracuje zbyt głośno.", "Przepalona żarówka w lampie.", "Niedziałające gniazdko elektryczne.",
        "Zacinające się drzwi.", "Brak papieru w drukarce.", "Uszkodzony kabel HDMI.", "Plama na wykładzinie.",
        "Nie domyka się okno.", "Głośny wentylator w komputerze.",
    ];

    private readonly FlexDeskDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHostEnvironment _environment;

    public DatabaseSeeder(FlexDeskDbContext context, IPasswordHasher passwordHasher, IHostEnvironment environment)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _environment = environment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var admin = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "admin@flexdesk.local",
            PasswordHash = _passwordHasher.Hash("Admin123!"),
            FirstName = "Dariusz",
            LastName = "Mędrala",
            Role = UserRole.Admin,
            CreatedAt = now,
        };

        var demoUser = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "user@flexdesk.local",
            PasswordHash = _passwordHasher.Hash("User123!"),
            FirstName = "Jan",
            LastName = "Kowalski",
            Role = UserRole.User,
            CreatedAt = now,
        };

        _context.Users.AddRange(admin, demoUser);

        if (!_environment.IsEnvironment("Testing"))
        {
            await SeedSampleDataAsync(demoUser, now, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSampleDataAsync(User demoUser, DateTime now, CancellationToken cancellationToken)
    {
        var random = new Random(20261119);
        var sharedPassword = _passwordHasher.Hash("User123!");

        var users = new List<User> { demoUser };
        for (var i = 0; i < 25; i++)
        {
            var firstName = FirstNames[i % FirstNames.Length];
            var lastName = LastNames[i % LastNames.Length];
            users.Add(new User
            {
                Id = Guid.CreateVersion7(),
                Email = $"{Transliterate(firstName)}.{Transliterate(lastName)}{i + 1}@flexdesk.local",
                PasswordHash = sharedPassword,
                FirstName = firstName,
                LastName = lastName,
                Role = UserRole.User,
                CreatedAt = now.AddDays(-random.Next(1, 200)),
            });
        }

        _context.Users.AddRange(users.Skip(1));

        var workspaces = await _context.Workspaces.ToListAsync(cancellationToken);
        for (var i = 0; i < WorkspaceNames.Length; i++)
        {
            var type = (WorkspaceType)random.Next(0, 3);
            var workspace = new Workspace
            {
                Id = Guid.CreateVersion7(),
                Name = $"{WorkspaceNames[i]} ({TypeLabel(type)})",
                Type = type,
                Capacity = type switch
                {
                    WorkspaceType.HotDesk => 1,
                    WorkspaceType.MeetingRoom => random.Next(4, 13),
                    _ => random.Next(2, 7),
                },
                PricePerHour = type switch
                {
                    WorkspaceType.HotDesk => random.Next(12, 26),
                    WorkspaceType.MeetingRoom => random.Next(40, 91),
                    _ => random.Next(80, 161),
                },
                IsActive = random.Next(0, 10) > 0,
                City = Cities[random.Next(Cities.Length)],
                Street = Streets[random.Next(Streets.Length)],
                BuildingNumber = random.Next(1, 120).ToString(),
                Floor = random.Next(0, 8),
            };
            workspaces.Add(workspace);
            _context.Workspaces.Add(workspace);
        }

        var reservations = new List<Reservation>();
        var payments = new List<Payment>();
        var statuses = new[]
        {
            ReservationStatus.Confirmed, ReservationStatus.Confirmed, ReservationStatus.Confirmed,
            ReservationStatus.Pending, ReservationStatus.Pending, ReservationStatus.Cancelled,
        };
        var methods = new[] { PaymentMethod.Card, PaymentMethod.BankTransfer, PaymentMethod.Cash };

        for (var i = 0; i < 90; i++)
        {
            var user = users[random.Next(users.Count)];
            var workspace = workspaces[random.Next(workspaces.Count)];
            var start = now.Date.AddDays(random.Next(-30, 31)).AddHours(random.Next(7, 18));
            var durationHours = workspace.Type switch
            {
                WorkspaceType.HotDesk => random.Next(1, 5),
                WorkspaceType.MeetingRoom => random.Next(2, 5),
                _ => random.Next(4, 9),
            };
            var end = start.AddHours(durationHours);
            var status = statuses[random.Next(statuses.Length)];

            var reservation = new Reservation
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                WorkspaceId = workspace.Id,
                StartTime = start,
                EndTime = end,
                TotalPrice = CalculatePrice(workspace.PricePerHour, start, end),
                Status = status,
                CreatedAt = start.AddDays(-random.Next(1, 10)),
            };
            reservations.Add(reservation);

            if (status == ReservationStatus.Confirmed)
            {
                payments.Add(new Payment
                {
                    Id = Guid.CreateVersion7(),
                    ReservationId = reservation.Id,
                    Amount = reservation.TotalPrice,
                    PaymentDate = reservation.CreatedAt,
                    Method = methods[random.Next(methods.Length)],
                    IsSuccessful = true,
                });
            }
        }

        var demoWorkspace = workspaces[^1];
        var demoReservation = new Reservation
        {
            Id = Guid.CreateVersion7(),
            UserId = demoUser.Id,
            WorkspaceId = demoWorkspace.Id,
            StartTime = now.AddHours(-2),
            EndTime = now.AddHours(2),
            TotalPrice = CalculatePrice(demoWorkspace.PricePerHour, now.AddHours(-2), now.AddHours(2)),
            Status = ReservationStatus.Confirmed,
            CreatedAt = now.AddDays(-1),
        };
        reservations.Add(demoReservation);
        payments.Add(new Payment
        {
            Id = Guid.CreateVersion7(),
            ReservationId = demoReservation.Id,
            Amount = demoReservation.TotalPrice,
            PaymentDate = demoReservation.CreatedAt,
            Method = PaymentMethod.Card,
            IsSuccessful = true,
        });

        _context.Reservations.AddRange(reservations);
        _context.Payments.AddRange(payments);

        var incidentStatuses = new[]
        {
            IncidentStatus.Reported, IncidentStatus.Reported, IncidentStatus.InProgress,
            IncidentStatus.Resolved, IncidentStatus.Resolved,
        };

        var incidents = new List<Incident>();
        for (var i = 0; i < 45; i++)
        {
            var workspace = workspaces[random.Next(workspaces.Count)];
            var user = users[random.Next(users.Count)];
            var status = incidentStatuses[random.Next(incidentStatuses.Length)];
            var createdAt = now.AddDays(-random.Next(0, 30)).AddHours(-random.Next(0, 12));

            incidents.Add(new Incident
            {
                Id = Guid.CreateVersion7(),
                WorkspaceId = workspace.Id,
                UserId = user.Id,
                Description = IncidentDescriptions[random.Next(IncidentDescriptions.Length)],
                Status = status,
                CreatedAt = createdAt,
                ResolvedAt = status == IncidentStatus.Resolved ? createdAt.AddDays(random.Next(1, 5)) : null,
            });
        }

        _context.Incidents.AddRange(incidents);
    }

    private static string TypeLabel(WorkspaceType type) => type switch
    {
        WorkspaceType.HotDesk => "Biurko",
        WorkspaceType.MeetingRoom => "Sala",
        _ => "Biuro",
    };

    private static decimal CalculatePrice(decimal pricePerHour, DateTime start, DateTime end)
    {
        var hours = (decimal)(end - start).TotalHours;
        var total = pricePerHour * hours;
        if (hours >= 8)
        {
            total *= 0.9m;
        }
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    private static string Transliterate(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(character switch
            {
                'ą' or 'Ą' => "a",
                'ć' or 'Ć' => "c",
                'ę' or 'Ę' => "e",
                'ł' or 'Ł' => "l",
                'ń' or 'Ń' => "n",
                'ó' or 'Ó' => "o",
                'ś' or 'Ś' => "s",
                'ź' or 'Ź' => "z",
                'ż' or 'Ż' => "z",
                _ => character.ToString().ToLowerInvariant(),
            });
        }
        return builder.ToString();
    }
}
