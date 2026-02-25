import { ListBox, Label, Description, Dropdown, Button } from '@heroui/react';
import { Agent } from '../../types';
import { getLastHeartbeat } from '../../utils';
import HardwareInfo from './HardwareInfo';

interface AgentItemProps {
  agent: Agent;
}

function AgentItem({ agent }: AgentItemProps) {
  return (
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
          <HardwareInfo agent={agent} />
        </div>
        <div>
          <Description>{getLastHeartbeat(agent.lastHeartbeat)}</Description>
        </div>
      </div>
    </ListBox.Item>
  );
}

export default AgentItem;
