using CantineBack.Identity;
using CantineBack.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CantineBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        public TokenController(ApplicationDbContext context,ITokenService tokenService)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(ApplicationDbContext));
            this._tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }
        [HttpPost]
        [Route("refresh")]
        public IActionResult Refresh(RefreshTokenRequest refreshTokenRequest)
        {
            if (refreshTokenRequest is null)
                return BadRequest("Invalid client request");
            string accessToken = refreshTokenRequest.AccessToken;
            string refreshToken = refreshTokenRequest.RefreshToken;
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var username = principal!.Identity!.Name; //this is mapped to the Name claim by default
            var userRefreshToken = _context.RefreshTokens!.SingleOrDefault(u => u.UserName == username);
            if (userRefreshToken is null || userRefreshToken.Refresh_Token != refreshToken || userRefreshToken.IsExpired)
                return BadRequest("Invalid client request");
            var res = _tokenService.GenerateAccessToken(principal.Claims);
            string newAccessToken = res.Item1;
            DateTime expire = res.Item2;

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            userRefreshToken.Refresh_Token = newRefreshToken;
            _context.SaveChanges();
            return Ok(new Token()
            {
                ExpireAt=expire,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [HttpPost, Authorize]
        [Route("revoke")]
        public IActionResult Revoke()
        {
            var username = User.Identity?.Name;

            if (_context.RefreshTokens == null|| String.IsNullOrWhiteSpace(username))
            {
                return BadRequest();
            }
            var userRefresh = _context.RefreshTokens.SingleOrDefault(u => u.UserName == username);
            if (userRefresh == null) return BadRequest();
            userRefresh.Refresh_Token = null;
            userRefresh.Revoked = DateTime.Now;
            _context.SaveChanges();
            return NoContent();
        }
    }
}
