using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Configs;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;

namespace Server.Services
{
    public interface IAuthenticationService
    {
        public Task<LoginResponseDto> Authenticate(string username, string password);
        public Task<LoginResponseDto> Refresh();
        public Task<LoginResponseDto> GetLoginResponse(ApplicationUser user);
    }

    public class AuthenticationService : LoggableService<AuthenticationService>, IAuthenticationService
    {
        private readonly IOptions<JwtTokenConfig> _config;

        public AuthenticationService(IServiceProvider provider) : base(provider)
        {
            _config = provider.GetRequiredService<IOptions<JwtTokenConfig>>();
        }

        public async Task<LoginResponseDto> Authenticate(string username, string password)
        {
            ApplicationUser user = await Context.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
            if (user == null || Manager.PasswordHasher
                .VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
            {
                throw new BadHttpRequestException("Invalid username or password");
            }

            return await GetLoginResponse(user);
        }

        public async Task<LoginResponseDto> Refresh()
        {
            ApplicationUser user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (user == null)
            {
                return null;
            }

            return await GetLoginResponse(user);
        }

        public async Task<LoginResponseDto> GetLoginResponse(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", user.Id),
            };
            foreach (var role in await Manager.GetRolesAsync(user))
            {
                claims.Add(new Claim("roles", role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Value.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddSeconds(_config.Value.Expires);
            var token = new JwtSecurityToken(_config.Value.Issuer, _config.Value.Audience, claims,
                expires: expires, signingCredentials: credentials);
            return new LoginResponseDto
            {
                Id = user.Id,
                Username = user.ContestantId,
                FullName = user.ContestantName,
                Roles = (await Manager.GetRolesAsync(user)).ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Issued = DateTime.Now.ToUniversalTime(),
                Expires = expires.ToUniversalTime()
            };
        }
    }
}