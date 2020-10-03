using UserMicroserviceLambda.Models;
using DomainServices.Base.CommandDomainServices;
using DomainServices.Base.QueryableDomainServices;
using Infrastructure.ExceptionHandling;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestfulWebAPINetCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Net;

namespace UserMicroserviceLambda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ClaimsPrincipal _user;
        protected readonly IQueryableDomainService<User, int> _userQueryableDomainService;
        protected readonly ICommandDomainServiceAsync<User> _userCommandDomainServiceAsync;
        protected readonly IExceptionHandler _exceptionHandler;
        private readonly IConfiguration _configuration;
        private readonly ITokenManager _tokenManager;

        public UserController(IHttpContextAccessor httpContextAccessor,
            IQueryableDomainService<User, int> userQueryableDomainService,
            ICommandDomainServiceAsync<User> userCommandDomainServiceAsync
            , IExceptionHandler exceptionHandler
            , IConfiguration configuration
            , ITokenManager tokenManager)
        {
            _user = httpContextAccessor.HttpContext.User;
            _userQueryableDomainService = userQueryableDomainService;
            _userCommandDomainServiceAsync = userCommandDomainServiceAsync;
            _exceptionHandler = exceptionHandler;
            _configuration = configuration;
            _tokenManager = tokenManager;
        }


        [HttpPost("Register")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<string>> Register(User user, CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync<ActionResult<string>>(async c =>
            {
                User userInDB = await _userQueryableDomainService.Get().SingleOrDefaultAsync(x => x.LoginData.UserName.ToLower().Trim() == user.LoginData.UserName.ToLower().Trim(), c).ConfigureAwait(false);
                if (userInDB.IsNotNull())
                {
                    return BadRequest("A user with userName : " + userInDB.LoginData.UserName + ", already exists.Please use a different user name.");
                }
                byte[] passwordHash = null;
                byte[] passwordSalt = null;
                PasswordUtility.CreatePasswordHash(user.LoginData.Password, out passwordHash, out passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                await _userCommandDomainServiceAsync.InsertAsync(user, c).ConfigureAwait(false);
                return Ok("User created successfully");
            }, cancellationToken);
        }


        [HttpPost("Login")]
        [ProducesResponseType(typeof(LogInStatus), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LogInStatus), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(LogInStatus), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<LogInStatus>> Login(LoginData loginData, CancellationToken cancellationToken = default)

        {
            return await _exceptionHandler.HandleExceptionAsync<ActionResult<LogInStatus>>(async c =>
            {
                var userName = loginData.UserName.Trim();
                var password = loginData.Password.Trim();
                User user = await _userQueryableDomainService.Get().SingleOrDefaultAsync(x => x.LoginData.UserName == userName, c).ConfigureAwait(false);
                if (user.IsNull())
                {
                    return NotFound(new LogInStatus { ErrorMessage = "User not present in the system.Please register." });
                }
                else if (await _tokenManager.ValidateTokenAsync(user.Id.ToString(), null, c))
                {
                    var loggedInTokenCacheData = await _tokenManager.GetTokenCacheDataAsync(user.Id.ToString(), c);
                    return BadRequest(new LogInStatus { ErrorMessage = "User already logged-in.", Token = loggedInTokenCacheData.Token });
                }
                else if (!PasswordUtility.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    return NotFound(new LogInStatus { ErrorMessage = "Password incorrect for " + userName + "." });
                }
                else
                {
                    var userIdString = user.Id.ToString();
                    var audienceConfig = _configuration.GetSection("Audience");
                    var jwtToken = await _tokenManager.CreateTokenAsync(new TokenInputData
                    {
                        Issuer = audienceConfig["Iss"],
                        Audience = audienceConfig["Aud"],
                        Secret = audienceConfig["Secret"],
                        Key = userIdString,
                        Expiration = TimeSpan.FromDays(1),//TODO - should be taken from config or "config DB"
                        TokenIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, userIdString),
                        ClaimsToConsiderAfterTokenValidation = new List<Claim>
                                 {
                                     new Claim(ClaimTypes.Role,user.Role.ToString())
                                     ///Add more claims here, if needed, to suit application needs
                                 }
                    });
                    return Ok(new LogInStatus { Token = jwtToken });
                }
            }, cancellationToken);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("All")]
        [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<User>>> Index(CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync(async c =>
            {
                var response = await _userQueryableDomainService.Include(x => x.T1Data).Include(x => x.LoginData).ToListAsync(c).ConfigureAwait(false);
                return Ok(response);
            }, cancellationToken);
        }


        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("Get")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string userName = null, CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync<IActionResult>(async c =>
            {
                User user = null;
                if (_user.IsInRole(Role.Customer.ToString()))
                {
                    var nameIdentifierClaim = _user.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                    var userId = Convert.ToInt32(nameIdentifierClaim.Value);
                    user = await _userQueryableDomainService.Include(x => x.LoginData).SingleOrDefaultAsync(x => x.Id == userId, c).ConfigureAwait(false);
                }
                else
                {
                    if (userName.IsNullOrEmpty())
                    {
                        return BadRequest("userName in querystring cannot be null or empty for Admin role.");
                    }
                    user = await _userQueryableDomainService.Include(x => x.LoginData).SingleOrDefaultAsync(x => x.LoginData.UserName == userName, c).ConfigureAwait(false);
                }
                return Ok(user);
            }, cancellationToken);
        }


        [Authorize(Roles = "Admin,Customer")]
        [HttpPut("Update")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<string>> Update(UserDataForUpdate userDataForUpdate, CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync<ActionResult<string>>(async c =>
            {
                var nameIdentifierClaim = _user.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                var userId = Convert.ToInt32(nameIdentifierClaim.Value);
                if (userDataForUpdate.Id != userId)
                {
                    return BadRequest("User can update only his or her data and not anyone else's data");
                }
                var user = await _userQueryableDomainService.Get().FirstOrDefaultAsync(x => x.Id == userId);

                if (userDataForUpdate.Password.IsNotNullOrEmpty())
                {
                    byte[] passwordHash = null;
                    byte[] passwordSalt = null;
                    PasswordUtility.CreatePasswordHash(userDataForUpdate.Password, out passwordHash, out passwordSalt);
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }
                user.Name = userDataForUpdate.Name;
                user.Address = userDataForUpdate.Address;
                user.State = userDataForUpdate.State;
                user.Country = userDataForUpdate.Country;
                user.Email = userDataForUpdate.Email;
                user.PAN = userDataForUpdate.PAN;
                user.ContactNumber = userDataForUpdate.ContactNumber;

                await _userCommandDomainServiceAsync.UpdateAsync(user, c).ConfigureAwait(false);
                return Ok("User data updated successfully");
            }, cancellationToken);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("AssignRole")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<string>> AssignRole(string userName, Role role, CancellationToken cancellationToken = default)
        {
            ContractUtility.Requires<ArgumentNullException>(userName.IsNotNullOrEmpty(), "userName cannot be null or empty");
            ContractUtility.Requires<ArgumentNullException>(role.IsEnum(), "role must be valid");
            return await _exceptionHandler.HandleExceptionAsync<ActionResult<string>>(async c =>
            {
                var user = await _userQueryableDomainService.Get().SingleOrDefaultAsync(x => x.LoginData.UserName.ToLower().Trim() == userName.ToLower().Trim()).ConfigureAwait(false);
                if (user.IsNull())
                {
                    return NotFound("User not present in the system.");
                }
                if (user.Role == role)
                {
                    return BadRequest("Trying to assign the existing role for the user.");
                }
                await _userCommandDomainServiceAsync.UpdateAsync(user, c).ConfigureAwait(false);
                await _tokenManager.RemoveTokenAsync(user.Id.ToString());
                return Ok(userName + "has been assigned with role : " + role + ", successfully");
            }, cancellationToken);
        }


        [Authorize(Roles = "Admin,Customer")]
        [HttpPost("Logout")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<string>> Logout(CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync(async c =>
            {
                var nameIdentifierClaim = _user.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                var response = await _tokenManager.RemoveTokenAsync(nameIdentifierClaim.Value, c);
                if (response)
                {
                    return Ok("User successfully logged out");
                }
                else
                {
                    return Problem("User could not be logged out due to some internal error.Please try after sometime");
                }
            }, cancellationToken);
        }
    }
}
