using System;
using System.Collections.Generic;
using System.Text;

namespace Libra.Virgo.Enum
{
    public enum CommandType
    {
        Shell,
        GetFrame,
        GetCameraFrame,

        GetFiles,
        GetDisks,
        ReadFile,

        GetProcesses,

        StartScreenStream,
        StopScreenStream,

        StartCameraStream,
        StopCameraStream,

        ReadFileStream,
    }
}
