using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst.QueryObj.Helper
{
    public class QFConfigModel : IQFConfigModel
    {

        public string DefaultConnection { get; set; }
        public string Provider { get; set; }
        public string HelperAssembly { get; set; }
        public bool MakeSelfTest { get; set; }
        public string QueryFirstInterfaceType { get; set; }
    }
}
