using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KnowledgeSpace.ViewModels.Systems
{
    public class PermissionVm
    {
        public string FunctionId { get; set; }

        public string RoleId { get; set; }

        public string CommandId { get; set; }
    }
}
