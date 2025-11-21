using visual_softech_ganesh_backend.InterFace;
using visual_softech_ganesh_backend.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace visual_softech_ganesh_backend.Services
{
    
        public class AuthService(StudentDbContext _context, IConfiguration configuration) :IAuthInterface
        {
            public async Task<User?> RegisterAsync(UserDto request)
            {
                if (await _context.User.AnyAsync(u => u.Username == request.Username))
                    return null;

                var user = new User();
                var hashedpassword = new PasswordHasher<User>()
                    .HashPassword(user, request.Password);

                user.Username = request.Username;
                user.PasswordHash = hashedpassword;
            user.Role = request.Role;

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }

            public async Task<TokenResponseDto?> LoginAsync(UserDto request)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Username == request.Username);
                if (user == null)
                    return null;

                if (user.Username != request.Username)
                    return null;

                if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                    == PasswordVerificationResult.Failed)
                    return null;

                return await CreateTokenResponse(user);
            }

            private async Task<TokenResponseDto> CreateTokenResponse(User user)
            {
                return new TokenResponseDto
                {
                    AccessToken = CreateToken(user),
                    RefreshToken = await GenrateAndSaveRefreshTokenAsync(user)
                };
            }

            public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
            {
                var user = await _context.User.FindAsync(request.UserId);
                if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                    return null;

                return await CreateTokenResponse(user);

            }

            private async Task<string> GenrateAndSaveRefreshTokenAsync(User user)
            {
                var randomBytes = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);

                var refreshToken = Convert.ToBase64String(randomBytes);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                _context.User.Update(user);                     
                await _context.SaveChangesAsync();

                return refreshToken;
            }
            private string CreateToken(User user)               // JWT token Created //
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new JwtSecurityToken(
                    issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                    audience: configuration.GetValue<string>("AppSettings:Audience"),

                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: creds
                 );

                return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            }


            


            

        
    }
}
