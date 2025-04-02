using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Core.Enums
{
    public enum DashboardEnum
    {
        [Description("Tuần")]
        Week = 1,
        [Description("Tháng")]
        Month = 2
    }
}
