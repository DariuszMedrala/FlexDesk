using FlexDesk.Application.Account.Dtos;

namespace FlexDesk.Application.Account;

public interface IAccountService
{
    Task<ProfileDto> GetProfileAsync(CancellationToken cancellationToken = default);

    Task<ProfileDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
