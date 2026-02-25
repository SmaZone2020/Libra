import { ListBox, Label, Description } from '@heroui/react';
import { FileItem as FileItemType } from '../../types';

interface FileItemProps {
  file: FileItemType;
  onAction: () => void;
}

function FileItem({ file, onAction }: FileItemProps) {
  return (
    <ListBox.Item
      key={file.fileName}
      id={file.fileName}
      textValue={file.fileName}
      onAction={file.isFolder ? onAction : undefined}
    >
      <div className="flex flex-col">
        <Label>{file.fileName}</Label>
        <Description>
          {file.type} · {file.size} · {file.changeDate}
        </Description>
      </div>
    </ListBox.Item>
  );
}

export default FileItem;
