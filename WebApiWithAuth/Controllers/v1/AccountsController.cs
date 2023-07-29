using AuthenticationJwt.Configuration;
using AuthenticationJwt.Models.Dtos.Incoming;
using AuthenticationJwt.Models.Dtos.Outgoing;
using DataService.IConfiguration;
using Entities.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApiWithAuth.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        public AccountsController(IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userRegistrationDto)
        {
            if(ModelState.IsValid)
            {
                var userExists = await _userManager.FindByEmailAsync(userRegistrationDto.Email);
               if (userExists != null){
                    return BadRequest(new UserRegistrationResponseDto(){
                        Success = false,
                        Errors = new List<string>(){
                        "Email already in use"
                        }
                    });
                }

                var newUser = new IdentityUser()
                {
                    Email = userRegistrationDto.Email,
                    UserName = userRegistrationDto.Email,
                    EmailConfirmed = true
                };
                var isCreated = await _userManager.CreateAsync(newUser, userRegistrationDto.Password);
                if (!isCreated.Succeeded)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = isCreated.Succeeded,
                        Errors = isCreated.Errors.Select(x => x.Description).ToList()
                    });
                }

                var _user = new User();
                _user.IndentityId = new Guid(newUser.Id);
                _user.LastName = userRegistrationDto.LastName;
                _user.FirstName = userRegistrationDto.FirstName;
                _user.Email = userRegistrationDto.Email;
                _user.DateOfBirth = DateTime.UtcNow;
                _user.Country ="";
                _user.Phone = "";
                _user.Status = 1;

                await _unitOfWork.Users.Add(_user);
                await _unitOfWork.CompletedAsync();
                var token = GenerateJwtToken(newUser);

                return Ok(new UserRegistrationResponseDto()
                {
                    Success = true,
                    Token = token
                });
            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid payload"
                    }
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequestDto)
        {
            if (ModelState.IsValid) {
                var userExists = await _userManager.FindByEmailAsync(loginRequestDto.Email);
                if (userExists == null)
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success= false,
                        Errors = new List<string>()
                        {
                            "Invalid Authentication request"
                        }
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(userExists, loginRequestDto.Password);
                if(isCorrect)
                {
                    var jwtToken = GenerateJwtToken(userExists);

                    return Ok(new UserLoginResponseDto(){
                        Success =true,
                        Token = jwtToken
                    });
                }
                else
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid Authentication request"
                        }
                    });
                }
            }
            else
            {
                return BadRequest( new UserRegistrationResponseDto
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid payload"
                    }
                });
            }
        }

        private string GenerateJwtToken(IdentityUser user)
        {

            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id",user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                    new Claim(JwtRegisteredClaimNames.Email,user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                    )
            };

            var token  = jwtHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtHandler.WriteToken(token);
            return jwtToken;
        }

    }
}
