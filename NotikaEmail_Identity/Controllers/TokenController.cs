using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotikaEmail_Identity.Models.JwtViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize(Roles = "Admin, User")]

    public class TokenController : Controller
    {

        private readonly JwtSettingsModel _jwtSettingsModel;

        public TokenController(IOptions<JwtSettingsModel> jwtSettingsModel)
        {
            _jwtSettingsModel = jwtSettingsModel.Value;
        }

        public IActionResult Generate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Generate(SimpleUserViewModel model)
        {
            var claim = new[]
            {
                new Claim("name",model.Name),
                new Claim("surname",model.Surname),
                new Claim("city",model.City),
                new Claim("username",model.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),//amac toke üret


            };

            var key =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsModel.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(

                issuer: _jwtSettingsModel.Issuer,
                audience: _jwtSettingsModel.Audience,
                claims: claim,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettingsModel.ExpireMinutes),
                signingCredentials: creds);



            model.Token=new JwtSecurityTokenHandler().WriteToken(token);

            return View(model);
        }

    }
}
