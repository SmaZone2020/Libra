import { useState, useEffect, useCallback } from 'react';
import { Card, Button, ListBox, Label, Description, Separator, Popover, Dropdown } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi } from '../services/api';

function Agents() {
  const token = localStorage.getItem("libra-token");
  const [agents, setAgents] = useState<any[]>([]);
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

  const getLastHeartbeat = (lastHeartbeat: string) => {
    const now = Math.floor(Date.now() / 1000);
    const heartbeatTime = Math.floor(new Date(lastHeartbeat).getTime() / 1000);
    const diff = now - heartbeatTime;
    
    if (diff < 60) {
      return `${diff}秒前`;
    } else if (diff < 3600) {
      return `${Math.floor(diff / 60)}分钟前`;
    } else {
      return `${Math.floor(diff / 3600)}小时前`;
    }
  };

  return (
    <DefaultLayout>
      <div className="flex-1 p-6 h-screen overflow-y-auto">
        <h1 className="text-2xl font-bold mb-6">代理列表</h1>
        
        <Card className="p-4">
          <ListBox aria-label="代理列表" selectionMode="none">
            {agents.map((agent) => (
              <ListBox.Item key={agent.agentId} textValue={agent.network.hostname}>
                  <div className="grid grid-cols-7 gap-4 flex justify-center items-center">
                    <div>
                      <Label className="font-medium">{agent.network.hostname}</Label>
                    </div>
                    <div>
                      <Description>{agent.network.username}</Description>
                    </div>
                    <div>
                      <Description>{agent.location || 'N/A'}</Description>
                    </div>
                    <div>
                      <Dropdown>
                        <Button variant="outline" size="sm">
                          {agent.qqAccounts?.length || 0} 个
                        </Button>
                        <Dropdown.Popover>
                          <Dropdown.Menu onAction={(key) => console.log(`Selected: ${key}`)}>
                            {agent.qqAccounts?.map((qqAccount: string, index: number) => (
                              <Dropdown.Item key={index} id={qqAccount} textValue={qqAccount}>
                                <Label>{qqAccount}</Label>
                              </Dropdown.Item>
                            ))}
                          </Dropdown.Menu>
                        </Dropdown.Popover>
                      </Dropdown>
                    </div>
                    <div>
                      <Description>{agent.osVersion}</Description>
                    </div>
                    <div>
                      <Popover>
                        <Popover.Trigger>
                          <Button variant="outline" size="sm">
                            <Label>硬件信息</Label>
                          </Button>
                        </Popover.Trigger>
                        <Popover.Content>
                          <Popover.Arrow />
                          <Popover.Dialog>
                            <Popover.Heading>硬件信息 - {agent.network.hostname}</Popover.Heading>
                            <div className="space-y-4 mt-2">
                              <div>
                                <Label>CPU</Label>
                                <Description><p>{agent.hardware.cpu.name} ({agent.hardware.cpu.cores} 核)</p></Description>
                              </div>
                              <div>
                                <Label>GPU</Label>
                                <Description>
                                  {agent.hardware.gpus.map((gpu: string, index: number) => (
                                    <p key={index}>{gpu}</p>
                                  ))}
                                </Description>
                              </div>
                              <div>
                                <Label>内存</Label>
                                <Description><p>{agent.hardware.memory.toFixed(2)} GB</p></Description>
                              </div>
                              <div>
                                <Label>磁盘</Label>
                                {agent.hardware.disks.map((disk: any, index: number) => {
                                  const usedSize = disk.totalSize - disk.availableSizes;
                                  const usagePercent = (usedSize / disk.totalSize) * 100;
                                  return (
                                    <div key={index} className="mt-2">
                                      <div className="flex justify-between">
                                        <Description>{disk.name} ({disk.label || '无标签'})</Description>
                                        <Description>{usedSize.toFixed(0)}GB / {disk.totalSize.toFixed(0)}GB ({usagePercent.toFixed(0)}%)</Description>
                                      </div>
                                      { /* 进度条，等下写 */}
                                    </div>
                                  );
                                })}
                              </div>
                              <div>
                                <Label>虚拟机</Label>
                                <Description><p>{agent.hardware.isVirtualMachine ? '是' : '否'}</p></Description>
                              </div>
                            </div>
                          </Popover.Dialog>
                        </Popover.Content>
                      </Popover>
                    </div>
                    <div>
                      <Description>{getLastHeartbeat(agent.lastHeartbeat)}</Description>
                    </div>
                  </div>
                </ListBox.Item>
            ))}
          </ListBox>
        </Card>
      </div>
    </DefaultLayout>
  );
}

export default Agents;