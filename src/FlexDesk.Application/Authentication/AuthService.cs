using FlexDesk.Application.Authentication.Dtos;
using FlexDesk.Application.Common.Exceptions;
using FlexDesk.Application.Common.Persistence;
using FlexDesk.Application.Common.Security;
using FlexDesk.Domain.Entities;
using FlexDesk.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Application.Authentication;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            throw new ConflictException($"Adres e-mail '{normalizedEmail}' jest już zarejestrowany.");
        }

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Nieprawidłowy e-mail lub hasło.");
        }

        return CreateResponse(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var token = _tokenGenerator.Generate(user);
        return new AuthResponse(
            token.Value,
            token.ExpiresAtUtc,
            user.Email,
            user.Role.ToString(),
            user.FirstName,
            user.LastName);
    }
}
