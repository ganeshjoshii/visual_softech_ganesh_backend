using visual_softech_ganesh_backend.Models;

namespace visual_softech_ganesh_backend.InterFace
{
    public interface IAuthInterface
    {
        Task<User?> RegisterAsync(UserDto request);

        //Task<string?> LoginAsync(UserDto request);

        Task<TokenResponseDto?> LoginAsync(UserDto request);

        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}
