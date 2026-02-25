import { useState, useEffect, useCallback } from 'react';
import { Card } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi } from '../services/api';
import { Agent } from '../types';
import AgentList from '../components/agents/AgentList';

function Agents() {
  const token = localStorage.getItem("libra-token");
  const [agents, setAgents] = useState<Agent[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchOnlineAgents = useCallback(async () => {
    if (!token) return;
    
    try {
      setLoading(true);
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      const response = await agentApi.getOnlineAgents(baseUrl, token);
      if (response.code === 200) {
        setAgents(response.data.agents);
      } else {
        console.error('API返回错误码:', response.code, '消息:', response.message);
      }
    } catch (error) {
      console.error('获取在线代理失败:', error);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    fetchOnlineAgents();
    const interval = setInterval(fetchOnlineAgents, 2000); // 每2秒刷新一次
    return () => clearInterval(interval);
  }, [fetchOnlineAgents]);

  return (
    <DefaultLayout>
      <div className="flex-1 p-6 h-screen overflow-y-auto">
        <h1 className="text-2xl font-bold mb-6">代理列表</h1>
        
        <Card className="p-4">
          <AgentList agents={agents} />
        </Card>
      </div>
    </DefaultLayout>
  );
}

export default Agents;