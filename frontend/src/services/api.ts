import { ApiResponse, Agent, AgentStats } from '../types';

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

async function authenticatedApiRequest<T>(
  baseUrl: string,
  endpoint: string,
  token: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  return apiRequest<T>(baseUrl, endpoint, {
    ...options,
    headers: {
      'Authorization': `Bearer ${token}`,
      ...options.headers,
    },
  });
}

// 认证相关API
export const authApi = {
  // 登录 - 提交TOTP验证码获取token
  login: async (baseUrl: string, code: string): Promise<ApiResponse<string>> => {
    return apiRequest<string>(
      baseUrl,
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
  getOnlineAgents: async (baseUrl: string, token: string): Promise<ApiResponse<{ count: number; agents: Agent[] }>> => {
    return authenticatedApiRequest<{ count: number; agents: Agent[] }>(
      baseUrl,
      '/api/v1/agents/online',
      token
    );
  },

  // 在线列表
  getOnlineAgentsWithType: async (baseUrl: string, token: string): Promise<ApiResponse<{ count: number; agents: string[] }>> => {
    return authenticatedApiRequest<{ count: number; agents: string[] }>(
      baseUrl,
      '/api/v1/agents/online?type=1',
      token
    );
  },

  // 统计信息
  getAgentStats: async (baseUrl: string, token: string): Promise<ApiResponse<AgentStats>> => {
    const t = Date.now();
    return authenticatedApiRequest<AgentStats>(
      baseUrl,
      `/api/v1/agents/stats?t=${t}`,
      token
    );
  },
};

// 命令执行API
export const commandApi = {
  // 执行shell命令
  runShellCommand: async (baseUrl: string, token: string, agentId: string, command: string): Promise<ApiResponse<{ result: string }>> => {
    return authenticatedApiRequest<{ result: string }>(
      baseUrl,
      `/api/v1/command/shell/${agentId}`,
      token,
      {
        method: 'POST',
        body: JSON.stringify(command),
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
  },
};

// 监控API
export const monitorApi = {
  // 获取屏幕帧
  getScreenFrame: async (baseUrl: string, token: string, agentId: string): Promise<ApiResponse<string>> => {
    return authenticatedApiRequest<string>(
      baseUrl,
      `/api/v1/monitor/frame/${agentId}`,
      token
    );
  },
};
export const cameraApi = {
  // 获取屏幕帧
  getScreenFrame: async (baseUrl: string, token: string, agentId: string): Promise<ApiResponse<string>> => {
    return authenticatedApiRequest<string>(
      baseUrl,
      `/api/v1/monitor/camera/${agentId}`,
      token
    );
  },
};
// 文件资源管理器相关API
export const explorerApi = {
  // 获取文件列表
  getFiles: async (baseUrl: string, token: string, agentId: string, path: string): Promise<ApiResponse<string>> => {
    return authenticatedApiRequest<string>(
      baseUrl,
      `/api/v1/explorer/getfiles/${agentId}`,
      token,
      {
        method: 'POST',
        body: JSON.stringify(path),
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
  },
  
  // 获取磁盘信息
  getDisks: async (baseUrl: string, token: string, agentId: string): Promise<ApiResponse<string>> => {
    return authenticatedApiRequest<string>(
      baseUrl,
      `/api/v1/explorer/disks/${agentId}`,
      token,
      {
        method: 'POST',
      }
    );
  },
  
  // 下载文件
  getFile: async (baseUrl: string, token: string, agentId: string, filepath: string): Promise<ApiResponse<{ fileName: string; content: string }>> => {
    return authenticatedApiRequest<{ fileName: string; content: string }>(
      baseUrl,
      `/api/v1/explorer/getfile/${agentId}`,
      token,
      {
        method: 'POST',
        body: JSON.stringify(filepath),
        headers: {
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
