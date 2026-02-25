import { useState, useCallback, useEffect } from 'react';
import { Card, Button, Alert, Select, ListBox, Label } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi, monitorApi } from '../services/api';

function Monitor() {
  const token = localStorage.getItem("libra-token");
  const [agents, setAgents] = useState<string[]>([]);
  const [selectedAgent, setSelectedAgent] = useState<string>('');
  const [imageData, setImageData] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [isMonitoring, setIsMonitoring] = useState(false);
  const [intervalId, setIntervalId] = useState<ReturnType<typeof setInterval> | null>(null);

  // 轮询获取代理列表
  const fetchOnlineAgents = useCallback(async () => {
    if (!token) return;
    
    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      // 使用新的 API 方法获取带 type=1 参数的代理列表
      const response = await agentApi.getOnlineAgentsWithType(baseUrl, token);
      if (response.code === 200) {
        // 处理返回的代理 ID 数组
        const agentIds = response.data.agents;
        setAgents(agentIds);
      } else {
        console.error('API返回错误码:', response.code, '消息:', response.message);
      }
    } catch (error) {
      console.error('获取在线代理失败:', error);
    }
  }, [token]);

  // 获取屏幕帧
  const fetchFrame = useCallback(async () => {
    if (!token) {
      setError('未登录，请先登录');
      return;
    }

    if (!selectedAgent) {
      setError('请选择代理');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      // 使用新的 API 方法获取屏幕帧
      const response = await monitorApi.getScreenFrame(baseUrl, token, selectedAgent);
      if (response.code === 200) {
        // 处理返回的 base64 图片数据
        setImageData(`data:image/jpeg;base64,${response.data}`);
      } else {
        setError(`获取帧失败: ${response.message}`);
      }
    } catch (error) {
      setError(`获取帧时出错: ${error instanceof Error ? error.message : '未知错误'}`);
    } finally {
      setLoading(false);
    }
  }, [token, selectedAgent]);

  const startMonitoring = () => {
    if (!isMonitoring) {
      // 立即获取一帧
      fetchFrame();
      // 然后每 1 秒获取一次
      const id = setInterval(fetchFrame, 1000);
      setIntervalId(id);
      setIsMonitoring(true);
    }
  };

  const stopMonitoring = () => {
    if (intervalId) {
      clearInterval(intervalId);
      setIntervalId(null);
      setIsMonitoring(false);
    }
  };

  // 初始化和轮询代理列表
  useEffect(() => {
    fetchOnlineAgents();
    // 每 2 秒刷新一次代理列表
    const agentInterval = setInterval(fetchOnlineAgents, 2000);
    
    return () => {
      clearInterval(agentInterval);
      if (intervalId) {
        clearInterval(intervalId);
      }
    };
  }, [fetchOnlineAgents, intervalId]);

  return (
    <DefaultLayout>
      <div className="flex-1 p-6 h-screen overflow-y-auto">
        <h1 className="text-2xl font-bold mb-6">监控</h1>

        <Card className="p-6 mb-6">
          <div className="space-y-4">
            {/* 代理选择 */}
            <div>
              <Label className="block text-sm font-medium mb-1">选择代理</Label>
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
                      <ListBox.Item key={agent} id={agent} >
                        {agent}
                      </ListBox.Item>
                    ))}
                  </ListBox>
                </Select.Popover>
              </Select>
            </div>

            {/* 控制按钮 */}
            <div className="grid grid-cols-3 gap-4">
              <Button 
                variant="primary" 
                onClick={fetchFrame}
                isDisabled={loading || !selectedAgent}
              >
                {loading ? '获取中...' : '获取一帧'}
              </Button>
              <Button 
                variant="primary" 
                onClick={startMonitoring}
                isDisabled={isMonitoring || loading || !selectedAgent}
              >
                开始监控
              </Button>
              <Button 
                variant="secondary" 
                onClick={stopMonitoring}
                isDisabled={!isMonitoring}
              >
                结束监控
              </Button>
            </div>

            {/* 错误提示 */}
            {error && (
              <Alert color="danger">
                {error}
              </Alert>
            )}

            {/* 屏幕显示 */}
            <div className="mt-6">
              <h2 className="text-lg font-semibold mb-4">屏幕显示</h2>
              <div className="border border-border rounded-lg p-4 flex items-center justify-center" style={{ minHeight: '400px' }}>
                {imageData ? (
                  <img src={imageData} alt="屏幕监控" className="max-w-full max-h-full" />
                ) : (
                  <p className="text-muted">选择代理后点击"获取一帧"或"开始监控"查看屏幕内容</p>
                )}
              </div>
            </div>
          </div>
        </Card>
      </div>
    </DefaultLayout>
  );
}
 
export { Monitor as default };
