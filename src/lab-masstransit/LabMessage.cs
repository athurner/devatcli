using System;
namespace lab_masstransit
{
    public class LabMessage
    {
        public LabMessage()
        {
            this.DateTime = DateTime.Now;
        }

        public string Information { get; set; }
        public DateTime DateTime { get; set; }
    }
}
