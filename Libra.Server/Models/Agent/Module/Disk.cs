using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Libra.Agent.Models.Module
{
    public class Disk
    {
        public string Label { get; set; }
        public string Name { get; set; }
        public string DriveFormat { get; set; }
        public double TotalSize { get; set; }
        public double AvailableSizes { get; set; }
        [JsonIgnore]
        public string LabelS => Label == "" ? "未知" : Label;
        [JsonIgnore]
        public string Size => $"{AvailableSizes}G/{TotalSize}G";
    }
}
