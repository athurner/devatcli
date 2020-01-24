using System;
using System.Collections.Generic;

namespace devatcli.DataModel
{
    public partial class CustomerCustomerDemo
    {
        public string CustomerId { get; set; }
        public string CustomerTypeId { get; set; }
        public Guid Rowguid { get; set; }

        public virtual Customers Customer { get; set; }
        public virtual CustomerDemographics CustomerType { get; set; }
    }
}
