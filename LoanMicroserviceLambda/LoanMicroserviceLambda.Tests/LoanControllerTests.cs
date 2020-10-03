using DomainServices.Base.QueryableDomainServices;
using FluentAssertions;
using Infrastructure.ExceptionHandling.PollyBasedExceptionHandling;
using Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies;
using LoanMicroserviceLambda.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Repository;
using Repository.Queryable;
using RestfulWebAPINetCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using UserMicroserviceLambda.Controllers;
using LoanMicroserviceLambda.EFContextsAndMaps;
using Repository.Command;
using DomainServices.Base.CommandDomainServices;

namespace LoanMicroserviceLambda.Tests
{
    [TestFixture]
    public class LoanControllerTests
    {
        private LoanController _adminLoanController;
        private LoanController _customerLoanController;

        [SetUp]
        public void Setup()
        {
            var adminClaims = new List<Claim>();
            adminClaims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));
            adminClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
            var adminHttpContext = GetHttpContext(adminClaims);
            var adminHttpContextAccessorMock = new Mock<IHttpContextAccessor>();
            adminHttpContextAccessorMock.Setup(x => x.HttpContext).Returns(adminHttpContext);

            var customerClaims = new List<Claim>();
            customerClaims.Add(new Claim(ClaimTypes.NameIdentifier, "2"));
            customerClaims.Add(new Claim(ClaimTypes.Role, "Customer"));
            var customerHttpContext = GetHttpContext(customerClaims);
            var customerHttpContextAccessorMock = new Mock<IHttpContextAccessor>();
            customerHttpContextAccessorMock.Setup(x => x.HttpContext).Returns(customerHttpContext);

            var nullLoggerFactory = new NullLoggerFactory();
            var nullLogger = nullLoggerFactory.CreateLogger("NullLogger");

            var builder = new DbContextOptionsBuilder<LoanContext>();
            builder.UseInMemoryDatabase("CustomerLoans");
            var options = builder.Options;
            var loanContext = new LoanContext(options);
            var inMemoryEFLoanQueryable = new EntityFrameworkCodeFirstQueryable<Loan>(loanContext);
            var inMemoryLoanQueryableRepository = new QueryableRepository<Loan>(inMemoryEFLoanQueryable);
            var inMemoryLoanQueryableDomainService = new QueryableDomainService<Loan,int>(inMemoryLoanQueryableRepository);
            var inMemoryEFLoanCommand = new EntityFrameworkCodeFirstCommand<Loan,int>(loanContext);
            var inMemoryLoanCommandRepository = new CommandRepository<Loan>(inMemoryEFLoanCommand);
            var inMemoryLoanCommandServiceAsync = new CommandDomainServiceAsync<Loan>(inMemoryLoanCommandRepository,nullLogger);

            var retryPolicy = new RetryNTimesPolicy(nullLogger, 3);
            var exceptionHandler = new BasicPollyExceptionHandler(new IPolicy[] { retryPolicy }, nullLogger, true);

            var myConfiguration = new Dictionary<string, string>
                                    {
                                        {"CustomerUrl", "CustomerUrl"}
                                    };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            //var configOptions = new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions());
            //var distributedCache = new MemoryDistributedCache(configOptions);
            var tokenManagerMock = new Mock<ITokenManager>();
            tokenManagerMock.Setup( x => x.GetTokenCacheDataAsync(It.IsAny<string>(), default)).Returns(Task.Run(()=>new TokenCacheData { Token = "TestToken"}));

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

            _adminLoanController = new LoanController(adminHttpContextAccessorMock.Object,
                                      inMemoryLoanQueryableDomainService, inMemoryLoanCommandServiceAsync,
                                      exceptionHandler, configuration,
                                      tokenManagerMock.Object, httpClientFactoryMock.Object);
            _customerLoanController = new LoanController(customerHttpContextAccessorMock.Object,
                                      inMemoryLoanQueryableDomainService, inMemoryLoanCommandServiceAsync,
                                      exceptionHandler, configuration,
                                      tokenManagerMock.Object, httpClientFactoryMock.Object);
        }

        [Test]
        public async Task Test_Apply_Loan()
        {
            //Arrange
            var fakeLoan = LoanFakes.GetFakeLoan();

            //Act
            await _customerLoanController.Apply(fakeLoan);
            var loans = await _customerLoanController.Index();
            var loansValue = (loans.Result as OkObjectResult).Value as IEnumerable<Loan>;

            //Assert
            loansValue.Should().NotBeNull();
            loansValue.Count().Should().Be(1);
            loansValue.ToArray()[0].CustomerID.Should().Be(2);
        }

        [Test]
        public async Task Test_Get_All_Loans()
        {
            //Act
            var loans = await _customerLoanController.Index();
            var loansValue = (loans.Result as OkObjectResult).Value as IEnumerable<Loan>;

            //Assert
            loansValue.Should().NotBeNull();
            loansValue.Count().Should().Be(1);
            loansValue.ToArray()[0].CustomerID.Should().Be(2);
        }

        [Test]
        public async Task Test_Get_Loan_For_Customer()
        {
            //Act
            var loans = await _customerLoanController.Get();
            var loansValue = (loans as OkObjectResult).Value as IEnumerable<Loan>;

            //Assert
            loansValue.Should().NotBeNull();
            loansValue.Count().Should().Be(1);
            loansValue.ToArray()[0].CustomerID.Should().Be(2);
        }

        [Test]
        public async Task Test_Get_Loan_For_Admin_UserName_Null()
        {
            //Act
            var loans = await _adminLoanController.Get();

            //Assert
            loans.Should().BeOfType<BadRequestObjectResult>();
        }

        //[Test]
        public async Task Test_Get_Loan_For_Admin_UserName_Not_Null()
        {
            //Act
            var loans = await _adminLoanController.Get("x");

            //Assert
            //loans.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Test_Update_Loan()
        {
            //Arrange
            var fakeLoan = LoanFakes.GetFakeLoan();
            fakeLoan.Amount = 0;

            //Act
            await _customerLoanController.Update(fakeLoan);
            var loans = await _customerLoanController.Index();
            var loansValue = (loans.Result as OkObjectResult).Value as IEnumerable<Loan>;

            //Assert
            loansValue.Should().NotBeNull();
            loansValue.Count().Should().Be(1);
            loansValue.ToList()[0].Amount.Should().Be(0);
        }

        private HttpContext GetHttpContext(IEnumerable<Claim> claims)
        {
            var defaultHttpContext = new DefaultHttpContext();
            var claimsPrincipal = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity(claims);
            claimsPrincipal.AddIdentity(claimsIdentity);
            defaultHttpContext.User = claimsPrincipal;
            return defaultHttpContext;
        }
    }
}