using System;
using System.Collections.Generic;

namespace Libra.Virgo.Models.MessageType
{
    /// <summary>
    /// 差异屏幕帧，IsFull=true 时包含完整 JPEG，否则只含变化区块
    /// </summary>
    public class ScreenFrame
    {
        public Guid StreamId { get; set; }
        public bool IsFull { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        /// <summary>完整帧 base64 JPEG，IsFull=true 时有值</summary>
        public string? Data { get; set; }

        /// <summary>变化区块列表，IsFull=false 时有值；为 null 表示本帧无变化</summary>
        public List<DiffBlock>? Blocks { get; set; }
    }

    public class DiffBlock
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        /// <summary>区块 base64 JPEG</summary>
        public string Data { get; set; } = "";
    }
}
