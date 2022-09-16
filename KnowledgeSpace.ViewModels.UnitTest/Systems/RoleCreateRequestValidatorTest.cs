using KnowledgeSpace.ViewModels.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KnowledgeSpace.ViewModels.UnitTest.Systems
{
    public class RoleCreateRequestValidatorTest
    {
        private RoleCreateRequestValidator validator;
        private RoleCreateRequest request;

        public RoleCreateRequestValidatorTest()
        {
            request = new RoleCreateRequest()
            {
                Id = "admin",
                Name = "admin"
            };
            validator = new RoleCreateRequestValidator();
        }
        [Fact]
        public void Should_Valid_Result_When_Valid_Request()
        {
            var result = validator.Validate(request);
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Should_Valid_Result_When_Request_Miss_RoleId()
        {
            request.Id = String.Empty;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
        [Fact]
        public void Should_Valid_Result_When_Request_Miss_RoleName()
        {
            request.Name = String.Empty;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
        [Fact]
        public void Should_Valid_Result_When_Request_Role_Empty()
        {
            request.Id = String.Empty;
            request.Name = String.Empty;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
    }
}
