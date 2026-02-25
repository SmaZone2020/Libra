import { useState, useEffect, useCallback } from 'react';
import { Card, Button, Select, Alert, TextArea, ListBox, Surface } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi, commandApi } from '../services/api';

function Shell() {
  const token = localStorage.getItem("libra-token");
  const [agents, setAgents] = useState<string[]>([]);
  const [selectedAgent, setSelectedAgent] = useState<string>('');
  const [command, setCommand] = useState<string>('');
  const [output, setOutput] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');

  const fetchOnlineAgents = useCallback(async () => {
    if (!token) return;
    
    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      // 使用新的 API 方法获取带 type=1 参数的代理列表
      const response = await agentApi.getOnlineAgentsWithType(baseUrl, token);
      if (response.code === 200) {
        const agentIds = response.data.agents;
        setAgents(agentIds);
      } else {
        console.error('API返回错误码:', response.code, '消息:', response.message);
      }
    } catch (error) {
      console.error('获取在线代理失败:', error);
    }
  }, [token]);

  useEffect(() => {
    fetchOnlineAgents();
    // 添加轮询，每2秒刷新一次
    const interval = setInterval(fetchOnlineAgents, 2000);
    return () => clearInterval(interval);
  }, [fetchOnlineAgents]);

  const executeCommand = async () => {
    if (!token || !selectedAgent || !command) {
      setError('请选择代理并输入命令');
      return;
    }

    setLoading(true);
    setError('');
    setOutput('');

    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      // 使用新的 API 方法执行命令
      const response = await commandApi.runShellCommand(baseUrl, token, selectedAgent, command);
      if (response.code === 200) {
        setOutput(response.data.result);
      } else {
        setError(`命令执行失败: ${response.message}`);
      }
    } catch (error) {
      setError(`执行命令时出错: ${error instanceof Error ? error.message : '未知错误'}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <DefaultLayout>
      <div className="flex-1 p-6 h-screen overflow-y-auto">
        <h1 className="text-2xl font-bold mb-6">Shell</h1>

        <Card className="p-6 mb-6">
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">选择代理</label>
              <Select
                value={selectedAgent}
                onChange={(value) => setSelectedAgent(value as string)}
                className="w-full"
              >
                <Select.Trigger>
                  <Select.Value />
                  <Select.Indicator />
                </Select.Trigger>
                <Select.Popover>
                  <ListBox>
                    {agents.map((agent) => (
                      <ListBox.Item key={agent} id={agent} textValue={agent}>
                        {agent}
                      </ListBox.Item>
                    ))}
                  </ListBox>
                </Select.Popover>
              </Select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">命令</label>
              <TextArea
                value={command}
                onChange={(e) => setCommand(e.target.value)}
                placeholder="输入要执行的命令"
                rows={4}
                className="w-full"
              />
            </div>

            <Button 
              variant="primary" 
              className="w-full"
              onClick={executeCommand}
              isDisabled={loading || !selectedAgent || !command}
            >
              {loading ? '执行中...' : '执行命令'}
            </Button>
          </div>
        </Card>

        {error && (
          <Alert color="danger" className="mb-4">
            {error}
          </Alert>
        )}

        {output && (
          <Surface className="flex min-w-[320px] flex-col gap-3 rounded-3xl p-6" variant="default">
            <h3 className="text-base font-semibold text-foreground">执行结果</h3>
            <pre className="whitespace-pre-wrap break-all text-sm text-muted">{atob(output)}</pre>
          </Surface>
        )}
      </div>
    </DefaultLayout>
  );
}

export default Shell;