using DomainServices.Base.CommandDomainServices;
using DomainServices.Base.QueryableDomainServices;
using Infrastructure.ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestfulWebAPINetCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using LoanMicroserviceLambda.Models;
using System.Collections.Generic;
using System.Net.Http;
using Infrastructure.Extensions;
using System.Net;

namespace UserMicroserviceLambda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ClaimsPrincipal _user;
        protected readonly IQueryableDomainService<Loan, int> _loanQueryableDomainService;
        protected readonly ICommandDomainServiceAsync<Loan> _loanCommandDomainServiceAsync;
        protected readonly IExceptionHandler _exceptionHandler;
        private readonly IConfiguration _configuration;
        private readonly ITokenManager _tokenManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoanController(IHttpContextAccessor httpContextAccessor
            , IQueryableDomainService<Loan, int> loanQueryableDomainService
            , ICommandDomainServiceAsync<Loan> loanCommandDomainServiceAsync
            , IExceptionHandler exceptionHandler
            , IConfiguration configuration
            , ITokenManager tokenManager
            , IHttpClientFactory httpClientFactory)
        {
            _user = httpContextAccessor.HttpContext.User;
            _loanQueryableDomainService = loanQueryableDomainService;
            _loanCommandDomainServiceAsync = loanCommandDomainServiceAsync;
            _exceptionHandler = exceptionHandler;
            _configuration = configuration;
            _tokenManager = tokenManager;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("Apply")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<string>> Apply(Loan loan, CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync<ActionResult<string>>(async c =>
            {
                var nameIdentifierClaim = _user.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                var customerId = Convert.ToInt32(nameIdentifierClaim.Value);
                if(loan.CustomerID != customerId)
                {
                    return BadRequest("You can apply loan only for yourself and not others");
                }
                await _loanCommandDomainServiceAsync.InsertAsync(loan, c).ConfigureAwait(false);
                return Ok("Loan created successfully");
            }, cancellationToken);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("All")]
        [ProducesResponseType(typeof(IEnumerable<Loan>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Loan>>> Index(CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync(async c =>
            {
                var response = await _loanQueryableDomainService.Include(x => x.T1Data).ToListAsync(c).ConfigureAwait(false);
                return Ok(response);
            }, cancellationToken);
        }


        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("Get")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IEnumerable<Loan>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string userName = null, CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync<IActionResult>(async c =>
            {
                IEnumerable<Loan> loans = null;
                var nameIdentifierClaim = _user.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (_user.IsInRole(Role.Customer.ToString()))
                {
                    var customerId = Convert.ToInt32(nameIdentifierClaim.Value);
                    loans = await _loanQueryableDomainService.Get().Where(x => x.CustomerID == customerId).ToListAsync(c).ConfigureAwait(false);
                }
                else
                {
                    if (userName.IsNullOrEmpty())
                    {
                        return BadRequest("userName in querystring cannot be null or empty for Admin role.");
                    }
                    var customerUrl = _configuration.GetValue<string>("CustomerUrl");
                    var tokenCacheData = await _tokenManager.GetTokenCacheDataAsync(nameIdentifierClaim.Value, c);
                    if(tokenCacheData.IsNull())
                    {
                        return Problem("Please try after sometime");
                    }
                    var jwtToken = tokenCacheData.Token;
                    var customer = await _httpClientFactory.GetDataAsync<Customer, int>(customerUrl + userName.Trim(), jwtToken);
                    loans = await _loanQueryableDomainService.Get().Where(x => x.CustomerID == customer.Id).ToListAsync(c).ConfigureAwait(false);
                }
                return Ok(loans);
            }, cancellationToken);
        }


        [Authorize(Roles = "Customer")]
        [HttpPut("Update")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<string>> Update(Loan loan, CancellationToken cancellationToken = default)
        {
            return await _exceptionHandler.HandleExceptionAsync<ActionResult<string>>(async c =>
            {
                var nameIdentifierClaim = _user.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                var customerId = Convert.ToInt32(nameIdentifierClaim.Value);
                if (loan.CustomerID != customerId)
                {
                    return BadRequest("You can update loans only applied by you");
                }
                await _loanCommandDomainServiceAsync.UpdateAsync(loan, c).ConfigureAwait(false);
                return Ok("Loan data updated successfully");
            }, cancellationToken);
        }
    }
}
