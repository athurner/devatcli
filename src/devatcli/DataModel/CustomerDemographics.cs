using System;
using System.Collections.Generic;

namespace devatcli.DataModel
{
    public partial class CustomerDemographics
    {
        public CustomerDemographics()
        {
            CustomerCustomerDemo = new HashSet<CustomerCustomerDemo>();
        }

        public string CustomerTypeId { get; set; }
        public string CustomerDesc { get; set; }
        public Guid Rowguid { get; set; }

        public virtual ICollection<CustomerCustomerDemo> CustomerCustomerDemo { get; set; }
    }
}
