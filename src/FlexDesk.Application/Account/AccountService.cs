using FlexDesk.Application.Account.Dtos;
using FlexDesk.Application.Common.Exceptions;
using FlexDesk.Application.Common.Persistence;
using FlexDesk.Application.Common.Security;
using FlexDesk.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Application.Account;

public class AccountService : IAccountService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public AccountService(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _context = context;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _updateProfileValidator = updateProfileValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<ProfileDto> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var user = await LoadCurrentUserAsync(cancellationToken);
        return ToDto(user);
    }

    public async Task<ProfileDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _updateProfileValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await LoadCurrentUserAsync(cancellationToken);
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (normalizedEmail != user.Email)
        {
            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail && u.Id != user.Id, cancellationToken);

            if (emailTaken)
            {
                throw new ConflictException($"Email '{normalizedEmail}' is already registered.");
            }
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = normalizedEmail;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await LoadCurrentUserAsync(cancellationToken);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new BusinessRuleException("The current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> LoadCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");

        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw NotFoundException.For(nameof(User), userId);
    }

    private static ProfileDto ToDto(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role);
}
