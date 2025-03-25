using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Core.Enums
{
    public enum StatusEnum
    {
        [Description("Hoạt động")]
        Active = 1,

        [Description("Không hoạt động")]
        Inactive = 0
    }
}
