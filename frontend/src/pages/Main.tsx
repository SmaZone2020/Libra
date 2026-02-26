import { useState, useEffect, useCallback } from 'react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi } from '../services/api';
import { getRunningTime } from '../utils';
import { AgentStats } from '../types';
import StatCard from '../components/common/StatCard';
import SystemOverview from '../components/common/SystemOverview';
import LineChart from '../components/common/LineChart';

function Main() {
  const token = localStorage.getItem("libra-token");
  const [stats, setStats] = useState<AgentStats>({
    onlineCount: 0,
    idleCount: 0,
    startTime: 0,
    ping: 0,
    streamHour: {},
    streamHourOutput: {}
  });

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
      <div className="flex-1 flex flex-col h-screen overflow-y-auto">
        <div className="flex-1 p-6">
          <SystemOverview />

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 my-6">
            {
              [
                { title: '在线设备', value: stats.onlineCount.toString(), icon: '🟢' },
                { title: '空闲设备', value: stats.idleCount.toString(), icon: '🟡' },
                { title: '运行时长', value: getRunningTime(stats.startTime), icon: '📋' },
                { title: '网络延迟', value: `${stats.ping}ms`, icon: '⚡' }
              ].map((stat, index) => (
                <StatCard key={index} title={stat.title} value={stat.value} icon={stat.icon} />
              ))
            }
          </div>

          <LineChart
            datasets={[
              {
                name: '上行流量',
                data: Object.entries(stats.streamHour || {}).slice(-20).map(([hour, value]) => ({ hour, value: Number(value) })),
                color: '#8884d8'
              },
              {
                name: '下行流量',
                data: Object.entries(stats.streamHourOutput || {}).slice(-20).map(([hour, value]) => ({ hour, value: Number(value) })),
                color: '#82ca9d'
              }
            ]}
          />

        </div>
      </div>
    </DefaultLayout>
  );
}

export default Main;