import { ListBox, Label, Description } from '@heroui/react';
import { DiskItem as DiskItemType } from '../../types';

interface DiskItemProps {
  disk: DiskItemType;
  onAction: () => void;
}

function DiskItem({ disk, onAction }: DiskItemProps) {
  return (
    <ListBox.Item
      key={disk.name}
      id={disk.name}
      textValue={disk.name}
      onAction={onAction}
    >
      <div className="flex flex-col">
        <Label>
          {disk.name} ({disk.label || '无标签'})
        </Label>
        <Description>
          {disk.driveFormat} · {disk.totalSize - disk.availableSizes}GB / {disk.totalSize}GB
        </Description>
      </div>
    </ListBox.Item>
  );
}

export default DiskItem;
