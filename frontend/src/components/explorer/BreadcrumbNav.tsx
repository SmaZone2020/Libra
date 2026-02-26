import { Surface, Breadcrumbs } from '@heroui/react';

interface BreadcrumbNavProps {
  currentPath: string;
  showDisks: boolean;
  onNavigateToRoot: () => void;
  onNavigateToPath: (path: string) => void;
}

function BreadcrumbNav({ currentPath, showDisks, onNavigateToRoot, onNavigateToPath }: BreadcrumbNavProps) {
  return (
    <Surface className="p-2 rounded-lg">
      <Breadcrumbs>
        <Breadcrumbs.Item onClick={onNavigateToRoot}>
          磁盘列表
        </Breadcrumbs.Item>
        {!showDisks && currentPath.split('\\').filter(Boolean).map((part, index, parts) => {
          const path = `/${parts.slice(0, index + 1).join('\\')}`;
          return (
            <Breadcrumbs.Item key={index} onClick={() => onNavigateToPath(path)}>
              {part}
            </Breadcrumbs.Item>
          );
        })}
      </Breadcrumbs>
    </Surface>
  );
}

export default BreadcrumbNav;
