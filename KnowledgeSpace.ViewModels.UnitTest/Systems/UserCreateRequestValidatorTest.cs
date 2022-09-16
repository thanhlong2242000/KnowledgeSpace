using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KnowledgeSpace.ViewModels.UnitTest.Systems
{
    public class UserCreateRequestValidatorTest
    {
        public class RoleCreateRequestValidatorTest
        {
            private UserCreateRequestValidator validator;
            private UserCreateRequest request;

            public RoleCreateRequestValidatorTest()
            {
                request = new UserCreateRequest()
                {
                    Dob = DateTime.Now,
                    Email = "Tedu@international@gmail.com",
                    FirstName = "Test",
                    LastName = "test",
                    Password = "Admin@123",
                    PhoneNumber = "123",
                    UserName = "test"
                };
                validator = new UserCreateRequestValidator();
            }
            [Fact]
            public void Should_Valid_Result_When_Valid_Request()
            {
                var result = validator.Validate(request);
                Assert.False(result.IsValid);
            }
            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public void Should_Valid_Result_When_Request_Miss_UserName(string username)
            {
                request.UserName = username;
                var result = validator.Validate(request);
                Assert.False(result.IsValid);
            }
            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public void Should_Valid_Result_When_Request_Miss_FirstName(string data)
            {
                request.FirstName = data;
                var result = validator.Validate(request);
                Assert.False(result.IsValid);
            }
            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public void Should_Valid_Result_When_Request_Miss_LastName(string data)
            {
                request.LastName = data;
                var result = validator.Validate(request);
                Assert.False(result.IsValid);
            }
            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public void Should_Valid_Result_When_Request_Miss_PhoneNumber(string data)
            {
                request.PhoneNumber = data;
                var result = validator.Validate(request);
                Assert.False(result.IsValid);
            }
            [Theory]
            [InlineData("asddas")]
            [InlineData("")]
            [InlineData("1234567")]
            [InlineData("admin123")]
            [InlineData(null)]
            public void Should_Valid_Result_When_Request_Password_Not_Match(string data)
            {
                request.Password = data;
                var result = validator.Validate(request);
                Assert.False(result.IsValid);
            }
        }
    }
}
