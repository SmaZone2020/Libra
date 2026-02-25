import { Popover, Button, Label, Description } from '@heroui/react';
import { Agent } from '../../types';

interface HardwareInfoProps {
  agent: Agent;
}

function HardwareInfo({ agent }: HardwareInfoProps) {
  return (
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
                    {/* 进度条，等下写 */}
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
  );
}

export default HardwareInfo;
