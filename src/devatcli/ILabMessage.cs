using System;

namespace devatcli
{
    public interface ILabMessage
    {
        string Information { get; set; }
        DateTime DateTime { get; set; }
    }
}