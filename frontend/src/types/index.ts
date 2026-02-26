// 类型定义文件

// Agent 相关类型
export interface AgentNetwork {
  hostname: string;
  username: string;
  location?: string;
}

export interface AgentHardware {
  cpu: {
    name: string;
    cores: number;
  };
  gpus: string[];
  memory: number;
  disks: {
    name: string;
    label?: string;
    totalSize: number;
    availableSizes: number;
  }[];
  isVirtualMachine: boolean;
}

export interface Agent {
  agentId: string;
  network: AgentNetwork;
  qqAccounts?: string[];
  osVersion: string;
  hardware: AgentHardware;
  lastHeartbeat: string;
  location?: string;
}

// 文件资源管理器相关类型
export interface FileItem {
  fileName: string;
  changeDate: string;
  size: string;
  type: string;
  isFolder: boolean;
}

export interface DiskItem {
  label: string;
  name: string;
  driveFormat: string;
  totalSize: number;
  availableSizes: number;
}

// API 响应类型
export interface ApiResponse<T = any> {
  code: number;
  message: string;
  data: T;
}

export interface AgentStats {
  onlineCount: number;
  idleCount: number;
  startTime: number;
  ping: number;
  streamHour: Record<string, number> ;
  streamHourOutput: Record<string, number> ;
}
