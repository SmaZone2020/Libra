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
  cameras?: string[];
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

// 差异屏幕流类型
export interface DiffBlock {
  x: number;
  y: number;
  w: number;
  h: number;
  data: string; // base64 JPEG
}

export interface ScreenFrame {
  streamId: string;
  isFull: boolean;
  screenWidth: number;
  screenHeight: number;
  data?: string;        // base64 JPEG，isFull=true 时有值
  blocks?: DiffBlock[]; // 变化区块，isFull=false 时有值
}

