using KnowledgeSpace.ViewModels.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KnowledgeSpace.ViewModels.UnitTest.Systems
{
    public class FunctionCreateRequestValidatorTest
    {
        private FunctionCreateRequestValidator validator;
        private FunctionCreateRequest request;

        public FunctionCreateRequestValidatorTest()
        {
            request = new FunctionCreateRequest()
            {
                Id = "test6",
                Name = "test6",
                ParentId = null,
                SortOrder = 5,
                Url = "/test6"
            };
            validator = new FunctionCreateRequestValidator();
        }
        [Fact]
        public void Should_Valid_Result_When_Valid_Request()
        {
            var result = validator.Validate(request);
            Assert.True(result.IsValid);
        }
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Valid_Result_When_Request_Miss_Id(string data)
        {
            request.Id = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Valid_Result_When_Request_Miss_Name(string data)
        {
            request.Name = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Valid_Result_When_Request_Miss_Url(string data)
        {
            request.Url = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
    }
}
