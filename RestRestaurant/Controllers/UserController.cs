using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RestRestaurant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }
       
        /// <summary>
        /// Returns user by token
        /// </summary>
        /// <param name="token">Tocken</param>
        /// <returns>User</returns>
        [HttpGet(Name = "GetUsers")]
        public IResult GetUser(String token)
        {
            User? user = DatabaseManager.GetInstance().GetUserByToken(token);
            if (user == null)
            {
                return Results.NotFound(); 
            }
            return Results.Json(user);
        }

        /// <summary>
        /// Creates new user
        /// </summary>
        /// <param name="name">Username</param>
        /// <param name="email">Email</param>
        /// <param name="password">Password</param>
        /// <returns>Result</returns>
        [HttpPut(Name = "CreateUser")]
        public IResult CreateUser(String name, String email, String password)
        {
            if (DatabaseManager.GetInstance().CheckUserEmail(name, email))
            {
                return Results.BadRequest("User exists");
            }
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            {
                return Results.BadRequest("Empty fields");
            }
            DatabaseManager.GetInstance().CreateUser(name, email, password);
            return Results.Ok();
        }

        /// <summary>
        /// Logs in user
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="password">Password</param>
        /// <returns>Token if success</returns>
        [HttpPost(Name = "LoginUser")]
        public IResult LoginUser(String email, String password)
        {
            int uid = DatabaseManager.GetInstance().CheckUser(email, password);
            if (uid == 0)
            {
                return Results.BadRequest("Wrong credentials");
            }
            var expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(2));
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, email) };
            var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new
            {
                access_token = encodedJwt,
                email = email
            };
            DatabaseManager.GetInstance().UpdateToken(encodedJwt, uid, expires);
            return Results.Json(response, statusCode: 200);
        }
    }
}