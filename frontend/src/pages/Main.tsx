import { useState, useEffect, useCallback } from 'react';
import { Button, Card } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi } from '../services/api';

function Main() {
  const token = localStorage.getItem("libra-token");
  const [stats, setStats] = useState({
    onlineCount: 0,
    idleCount: 0,
    startTime: 0,
    ping: 0
  });

  // 计算运行时长
  const getRunningTime = (startTime: number) => {
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
  };

  const fetchAgentStats = useCallback(async () => {
    if (!token) return;
    
    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      const response = await agentApi.getAgentStats(baseUrl, token);
      if (response.code === 200) {
        setStats(response.data);
      } else {
        console.error('API返回错误码:', response.code, '消息:', response.message);
      }
    } catch (error) {
      console.error('获取Agent统计信息失败:', error);
    }
  }, [token]);

  useEffect(() => {
    fetchAgentStats();
    const interval = setInterval(fetchAgentStats, 1000);
    return () => {
      console.log('清理计时器');
      clearInterval(interval);
    };
  }, [token, fetchAgentStats]);

  return (
    <DefaultLayout>
      <div className="flex-1 flex flex-col">
        <div className="flex-1 p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
            {
              [
                { title: '在线设备', value: stats.onlineCount.toString(), icon: '🟢' },
                { title: '空闲设备', value: stats.idleCount.toString(), icon: '🟡' },
                { title: '运行时长', value: getRunningTime(stats.startTime), icon: '📋' },
                { title: '网络延迟', value: `${stats.ping}ms`, icon: '⚡' }
              ].map((stat, index) => (
                <Card key={index} className="p-4">
                  <div className="flex justify-between items-center">
                    <div>
                      <p className="text-sm text-gray-500">{stat.title}</p>
                      <h3 className="text-2xl font-bold mt-1">{stat.value}</h3>
                    </div>
                    <div className="text-2xl">{stat.icon}</div>
                  </div>
                </Card>
              ))
            }
          </div>

          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">系统概览</h3>
            <p className="text-gray-600">
              欢迎使用 Libra 天秤座 C2 框架。这里是控制面板的主页面，您可以通过左侧
              菜单栏访问各个功能模块。
            </p>
            <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
              <Button className="w-full">查看所有代理</Button>
              <Button variant="secondary" className="w-full">创建新任务</Button>
            </div>
          </Card>
        </div>
      </div>
    </DefaultLayout>
  );
}

export default Main;