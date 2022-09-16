using KnowledgeSpace.BackendServer.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;

namespace KnowledgeSpace.BackendServer.Autherization
{
    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        private FunctionCode CONTENT_CATEGORY;
        private CommandCode CREATE;

        public ClaimRequirementAttribute(FunctionCode functionId, CommandCode commandId) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { functionId, commandId };
        }
    }
}
