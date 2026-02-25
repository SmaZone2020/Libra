// 时间处理工具函数

/**
 * 计算最后心跳时间的相对时间
 * @param lastHeartbeat 最后心跳时间字符串
 * @returns 相对时间字符串，如"5秒前"、"2分钟前"等
 */
export function getLastHeartbeat(lastHeartbeat: string): string {
  const now = Math.floor(Date.now() / 1000);
  const heartbeatTime = Math.floor(new Date(lastHeartbeat).getTime() / 1000);
  const diff = now - heartbeatTime;
  
  if (diff < 60) {
    return `${diff}秒前`;
  } else if (diff < 3600) {
    return `${Math.floor(diff / 60)}分钟前`;
  } else {
    return `${Math.floor(diff / 3600)}小时前`;
  }
}

/**
 * 计算运行时长
 * @param startTime 开始时间戳（秒）
 * @returns 运行时长字符串，如"1h 30m"、"45s"等
 */
export function getRunningTime(startTime: number): string {
  if (!startTime) return '0s';
  const now = Math.floor(Date.now() / 1000);
  const diff = now - startTime;
  
  const hours = Math.floor(diff / 3600);
  const minutes = Math.floor((diff % 3600) / 60);
  const seconds = diff % 60;
  
  if (hours > 0) {
    return `${hours}h ${minutes}m`;
  } else if (minutes > 0) {
    return `${minutes}m ${seconds}s`;
  } else {
    return `${seconds}s`;
  }
}
