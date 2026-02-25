import { ListBox } from '@heroui/react';
import { Agent } from '../../types';
import AgentItem from './AgentItem';

interface AgentListProps {
  agents: Agent[];
}

function AgentList({ agents }: AgentListProps) {
  return (
    <ListBox aria-label="代理列表" selectionMode="none">
      {agents.map((agent) => (
        <AgentItem key={agent.agentId} agent={agent} />
      ))}
    </ListBox>
  );
}

export default AgentList;
