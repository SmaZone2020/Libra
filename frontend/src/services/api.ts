// API服务层，处理与后端的通信

// API响应类型
export interface ApiResponse<T> {
  code: number;
  message: string;
  requestId: string;
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

// Agent相关API
export const agentApi = {
  // 获取在线Agent列表
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

  // 获取Agent统计信息
  getAgentStats: async (baseUrl: string, token: string): Promise<ApiResponse<{ onlineCount: number; totalCount: number; timestamp: number }>> => {
    return apiRequest<{ onlineCount: number; totalCount: number; timestamp: number }>(
      baseUrl,
      '/api/v1/agents/stats',
      {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      }
    );
  },
};

// 导出默认API对象
export default {
  auth: authApi,
  agent: agentApi,
};
