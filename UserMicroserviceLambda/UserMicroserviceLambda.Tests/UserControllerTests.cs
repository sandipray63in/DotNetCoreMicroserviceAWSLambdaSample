using DomainServices.Base.CommandDomainServices;
using DomainServices.Base.QueryableDomainServices;
using FluentAssertions;
using Infrastructure.ExceptionHandling.PollyBasedExceptionHandling;
using Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Repository;
using Repository.Command;
using Repository.Queryable;
using RestfulWebAPINetCore;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserMicroserviceLambda.Controllers;
using UserMicroserviceLambda.EFContextsAndMaps;
using UserMicroserviceLambda.Models;

namespace UserMicroserviceLambda.Tests
{
    public class UserControllerTests
    {
        private UserController _adminUserController;
        private UserController _customerUserController;

        [SetUp]
        public void Setup()
        {
            var fakeAdminUser = UserFakes.GetFakeAdminUser();
            var adminClaims = new List<Claim>();
            adminClaims.Add(new Claim(ClaimTypes.NameIdentifier, fakeAdminUser.Id.ToString()));
            adminClaims.Add(new Claim(ClaimTypes.Role, fakeAdminUser.Role.ToString()));
            var adminHttpContext = GetHttpContext(adminClaims);
            var adminHttpContextAccessorMock = new Mock<IHttpContextAccessor>();
            adminHttpContextAccessorMock.Setup(x => x.HttpContext).Returns(adminHttpContext);

            var fakeCustomerUser = UserFakes.GetFakeCustomerUser();
            var customerClaims = new List<Claim>();
            customerClaims.Add(new Claim(ClaimTypes.NameIdentifier, fakeCustomerUser.Id.ToString()));
            customerClaims.Add(new Claim(ClaimTypes.Role, fakeCustomerUser.Role.ToString()));
            var customerHttpContext = GetHttpContext(customerClaims);
            var customerHttpContextAccessorMock = new Mock<IHttpContextAccessor>();
            customerHttpContextAccessorMock.Setup(x => x.HttpContext).Returns(customerHttpContext);

            var nullLoggerFactory = new NullLoggerFactory();
            var nullLogger = nullLoggerFactory.CreateLogger("NullLogger");

            var builder = new DbContextOptionsBuilder<UserContext>();
            builder.UseInMemoryDatabase("CustomerLoans");
            var options = builder.Options;
            var userContext = new UserContext(options);
            var inMemoryEFUserQueryable = new EntityFrameworkCodeFirstQueryable<User>(userContext);
            var inMemoryUserQueryableRepository = new QueryableRepository<User>(inMemoryEFUserQueryable);
            var inMemoryUserQueryableDomainService = new QueryableDomainService<User, int>(inMemoryUserQueryableRepository);
            var inMemoryEFUserCommand = new EntityFrameworkCodeFirstCommand<User, int>(userContext);
            var inMemoryUserCommandRepository = new CommandRepository<User>(inMemoryEFUserCommand);
            var inMemoryUserCommandServiceAsync = new CommandDomainServiceAsync<User>(inMemoryUserCommandRepository, nullLogger);

            var retryPolicy = new RetryNTimesPolicy(nullLogger, 3);
            var exceptionHandler = new BasicPollyExceptionHandler(new IPolicy[] { retryPolicy }, nullLogger, true);

            var myConfiguration = new Dictionary<string, string>
                                    {
                                        {"Audience:Secret", "Y2F0Y2hlciUyMHdvbmclMjBsb3ZlJTIwLm5ldA=="},
                                        {"Audience:Iss", "Sandip Ray"},
                                        {"Audience:Aud", "All"},
                                    };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var configOptions = new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions());
            var distributedCache = new MemoryDistributedCache(configOptions);
            var tokenManagerMock = new Mock<TokenManager>(distributedCache);

            _adminUserController = new UserController(adminHttpContextAccessorMock.Object,
                                      inMemoryUserQueryableDomainService, inMemoryUserCommandServiceAsync,
                                      exceptionHandler, configuration,
                                      tokenManagerMock.Object);
            _customerUserController = new UserController(customerHttpContextAccessorMock.Object,
                                      inMemoryUserQueryableDomainService, inMemoryUserCommandServiceAsync,
                                      exceptionHandler, configuration,
                                      tokenManagerMock.Object);
        }

        [Test]
        public async Task Test1_Register_Admin()
        {
            //Arrange
            var fakeAdminUser = UserFakes.GetFakeAdminUser();

            //Act
            await _adminUserController.Register(fakeAdminUser, default);
            var user = await _adminUserController.Get(fakeAdminUser.LoginData.UserName);

            //Assert
            user.Should().NotBeNull();
            (user as OkObjectResult).Value.Should().NotBeNull();
            ((user as OkObjectResult).Value as User).Should().NotBeNull();
            ((user as OkObjectResult).Value as User).Id.Should().Be(1);
        }

        [Test]
        public async Task Test2_Register_Customer()
        {
            //Arrange
            var fakeCustomerUser = UserFakes.GetFakeCustomerUser();

            //Act
            await _customerUserController.Register(fakeCustomerUser, default);
            var user = await _customerUserController.Get();

            //Assert
            user.Should().NotBeNull();
            (user as OkObjectResult).Value.Should().NotBeNull();
            ((user as OkObjectResult).Value as User).Should().NotBeNull();
            ((user as OkObjectResult).Value as User).Id.Should().Be(2);
        }

        [Test]
        public async Task Test3_Login_Admin()
        {
            //Arrange
            var fakeAdminUser = UserFakes.GetFakeAdminUser();
            var tokenHandler = new JwtSecurityTokenHandler();

            //Act
            var tokenResult = await _adminUserController.Login(new LoginData { UserName = fakeAdminUser.LoginData.UserName, Password = fakeAdminUser.LoginData.Password });

            //Assert
            tokenResult.Should().NotBeNull();
            ((tokenResult.Result as OkObjectResult).Value as string).Should().NotBeNullOrEmpty();
            tokenHandler.ReadToken((tokenResult.Result as OkObjectResult).Value as string).Should().NotBeNull();
        }

        [Test]
        public async Task Test4_Login_Customer()
        {
            //Arrange
            var fakeCustomerUser = UserFakes.GetFakeCustomerUser();
            var tokenHandler = new JwtSecurityTokenHandler();

            //Act
            var tokenResult = await _customerUserController.Login(new LoginData { UserName = fakeCustomerUser.LoginData.UserName, Password = fakeCustomerUser.LoginData.Password });

            //Assert
            tokenResult.Should().NotBeNull();
            ((tokenResult.Result as OkObjectResult).Value as string).Should().NotBeNullOrEmpty();
            tokenHandler.ReadToken((tokenResult.Result as OkObjectResult).Value as string).Should().NotBeNull();
        }

        [Test]
        public async Task Test5_Index()
        {
            //Arrange

            //Act
            var result = await _adminUserController.Index();

            //Assert
            result.Should().NotBeNull();
            ((result.Result as OkObjectResult).Value as IEnumerable<User>).Count().Should().Be(2);
        }

        [Test]
        public async Task Test6_Update()
        {
            //Arrange
            var updatedName = "Sandip3";
            var fakeCustomerUser = UserFakes.GetFakeCustomerUser();
            var userDataForUpdate = new UserDataForUpdate
            {
                Id = fakeCustomerUser.Id,
                Address = fakeCustomerUser.Address,
                ContactNumber = fakeCustomerUser.ContactNumber,
                Country = fakeCustomerUser.Country,
                Email = fakeCustomerUser.Email,
                PAN = fakeCustomerUser.PAN,
                Password = fakeCustomerUser.LoginData.Password,
                State = fakeCustomerUser.State,
                Name = updatedName
            };

            //Act
            await _customerUserController.Update(userDataForUpdate);
            var userResult = await _customerUserController.Get();

            //Assert
            ((userResult as OkObjectResult).Value as User).Name.Should().Be(updatedName);
        }

        //[Test]
        public async Task Test7_AssignRole()
        {
            //Arrange
            var fakeCustomerUser = UserFakes.GetFakeCustomerUser();
            var userName = fakeCustomerUser.LoginData.UserName;

            //Act
            await _adminUserController.AssignRole(userName, Role.Admin);
            var userResult = await _adminUserController.Get(userName);

            //Assert
            ((userResult as OkObjectResult).Value as User).Role.Should().Be(Role.Admin);
        }

        [Test]
        public async Task Test8_Logout()
        {
            //Arrange

            //Act
            var okResult = await _customerUserController.Logout();

            //Assert
            ((okResult.Result as OkObjectResult).Value as string).Should().NotBeNullOrEmpty();
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