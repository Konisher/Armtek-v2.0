using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PersonLib;
using Server.Model;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private static List<Person> people = LoadPeopleFromJson();

        [HttpPost("/token")]
        public IActionResult Token([FromQuery] string username, [FromQuery] string password)
        {
            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            return Json(response);
        }

        [HttpPost("/api/Account/Register")]
        public IActionResult Register([FromBody] Person personData)
        {
            people.Add(new Person
            {
                Email = personData.Email,
                Password = personData.Password,
                Role = "user"
            });

            SavePeopleToJson(people);

            return Ok("Registration successful");
        }

        [Authorize]
        [HttpGet("UserInfo")]
        public IActionResult UserInfo()
        {
            var username = User.Identity.Name;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)?.Value;

            var userInfo = new
            {
                Username = username,
                Role = role
            };

            return Ok(userInfo);
        }

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            Person person = people.FirstOrDefault(x => x.Email == username && x.Password == password);
            if (person != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, person.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
            };
                ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, "token", ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            return null;
        }
        private static List<Person> LoadPeopleFromJson()
        {
            string jsonFilePath = "users.json";

            if (System.IO.File.Exists(jsonFilePath))
            {
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                return JsonConvert.DeserializeObject<List<Person>>(jsonContent);
            }

            return new List<Person>();
        }

        private static void SavePeopleToJson(List<Person> people)
        {
            string jsonFilePath = "users.json";
            string jsonContent = JsonConvert.SerializeObject(people, Formatting.Indented);
            System.IO.File.WriteAllText(jsonFilePath, jsonContent);
        }
    }

}