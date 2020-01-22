using System;

namespace lab_masstransit
{
    public interface ILabMessage
    {
        string Information { get; set; }
        DateTime DateTime { get; set; }
    }
}