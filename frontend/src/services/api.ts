// API服务层，处理与后端的通信

// API响应类型
export interface ApiResponse<T> {
  code: number;
  message: string;
  data: T;
  timestamp: number;
}

// 基础API请求函数
async function apiRequest<T>(
  baseUrl: string,
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const cleanBaseUrl = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
  const url = `${cleanBaseUrl}${endpoint}`;
  
  const defaultOptions: RequestInit = {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  };

  try {
    const response = await fetch(url, defaultOptions);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('API request failed:', error);
    throw error;
  }
}

// 认证相关API
export const authApi = {
  // 登录 - 提交TOTP验证码获取token
  login: async (baseUrl: string, code: string): Promise<ApiResponse<string>> => {
    const cleanBaseUrl = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
    return apiRequest<string>(
      cleanBaseUrl,
      '/api/v1/login',
      {
        method: 'POST',
        body: JSON.stringify(code),
      }
    );
  },

  // 获取初始密钥或二维码
  ping: async (baseUrl: string, type: string = '1'): Promise<ApiResponse<string>> => {
    return apiRequest<string>(
      baseUrl,
      `/api/v1/ping?type=${type}`
    );
  },

  // 验证token是否有效
  validateToken: async (baseUrl: string, token: string): Promise<ApiResponse<{ valid: boolean; expiration: number }>> => {
    return apiRequest<{ valid: boolean; expiration: number }>(
      baseUrl,
      '/api/v1/status',
      {
        method: 'POST',
        body: JSON.stringify(token),
      }
    );
  },
};

export const agentApi = {
  // 在线列表
  getOnlineAgents: async (baseUrl: string, token: string): Promise<ApiResponse<{ count: number; agents: any[] }>> => {
    return apiRequest<{ count: number; agents: any[] }>(
      baseUrl,
      '/api/v1/agents/online',
      {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      }
    );
  },

  // 在线列表（带 type=1 参数）
  getOnlineAgentsWithType: async (baseUrl: string, token: string): Promise<ApiResponse<{ count: number; agents: string[] }>> => {
    return apiRequest<{ count: number; agents: string[] }>(
      baseUrl,
      '/api/v1/agents/online?type=1',
      {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      }
    );
  },

  // 统计信息
  getAgentStats: async (baseUrl: string, token: string): Promise<ApiResponse<{ onlineCount: number; idleCount: number; startTime: number; ping: number }>> => {
    const t = Date.now();
    return apiRequest<{ onlineCount: number; idleCount: number; startTime: number; ping: number }>(
      baseUrl,
      `/api/v1/agents/stats?t=${t}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      }
    );
  },
};

// 命令执行相关API
export const commandApi = {
  // 执行shell命令
  runShellCommand: async (baseUrl: string, token: string, agentId: string, command: string): Promise<ApiResponse<{ result: string }>> => {
    return apiRequest<{ result: string }>(
      baseUrl,
      `/api/v1/command/shell/${agentId}`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(command),
      }
    );
  },
};

// 监控相关API
export const monitorApi = {
  // 获取屏幕帧
  getScreenFrame: async (baseUrl: string, token: string, agentId: string): Promise<ApiResponse<string>> => {
    return apiRequest<string>(
      baseUrl,
      `/api/v1/monitor/frame/${agentId}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      }
    );
  },
};

// 文件资源管理器相关API
export const explorerApi = {
  // 获取文件列表
  getFiles: async (baseUrl: string, token: string, agentId: string, path: string): Promise<ApiResponse<string>> => {
    return apiRequest<string>(
      baseUrl,
      `/api/v1/explorer/getfiles/${agentId}`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(path),
      }
    );
  },
  
  // 获取磁盘信息
  getDisks: async (baseUrl: string, token: string, agentId: string): Promise<ApiResponse<string>> => {
    return apiRequest<string>(
      baseUrl,
      `/api/v1/explorer/disks/${agentId}`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      }
    );
  },
};

export default {
  auth: authApi,
  agent: agentApi,
  command: commandApi,
  monitor: monitorApi,
  explorer: explorerApi,
};
