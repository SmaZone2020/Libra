import { useState, useCallback, useEffect } from 'react';
import { Card, Select, ListBox, Label, Alert, Surface, Breadcrumbs, Description } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi, explorerApi } from '../services/api';

interface FileItem {
  fileName: string;
  changeDate: string;
  size: string;
  type: string;
  isFolder: boolean;
}

interface DiskItem {
  label: string;
  name: string;
  driveFormat: string;
  totalSize: number;
  availableSizes: number;
}


function base64ToUtf8(base64: string) {
  try {
    const binary = atob(base64);
    const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));
    return new TextDecoder().decode(bytes);
  } catch {
    return '';
  }
}


function Explorer() {
  const token = localStorage.getItem("libra-token");

  const [agents, setAgents] = useState<string[]>([]);
  const [selectedAgent, setSelectedAgent] = useState<string>('');
  const [currentPath, setCurrentPath] = useState<string>('');
  const [files, setFiles] = useState<FileItem[]>([]);
  const [disks, setDisks] = useState<DiskItem[]>([]);
  const [showDisks, setShowDisks] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');


  const fetchOnlineAgents = useCallback(async () => {
    if (!token) return;

    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      const response = await agentApi.getOnlineAgentsWithType(baseUrl, token);

      if (response.code === 200) {
        setAgents(response.data.agents || []);
      }
    } catch (err) {
      console.error("获取代理失败", err);
    }
  }, [token]);


  const fetchDisks = useCallback(async () => {
    if (!token || !selectedAgent) return;

    setLoading(true);
    setError('');

    setDisks([]);

    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      const response = await explorerApi.getDisks(baseUrl, token, selectedAgent);

      if (response.code === 200 && response.data) {
        const decoded = base64ToUtf8(response.data);
        const rawList = JSON.parse(decoded) || [];

        const normalized: DiskItem[] = rawList.map((d: any) => ({
          label: d.label ?? d.Label ?? '',
          name: d.name ?? d.Name ?? '',
          driveFormat: d.driveFormat ?? d.DriveFormat ?? '',
          totalSize: d.totalSize ?? d.TotalSize ?? 0,
          availableSizes: d.availableSizes ?? d.AvailableSizes ?? 0
        }));

        setDisks(normalized);
      } else {
        setError(response.message || "获取磁盘失败");
      }
    } catch (err: any) {
      setError(err.message || "磁盘解析错误");
    } finally {
      setLoading(false);
    }
  }, [token, selectedAgent]);


  const fetchFiles = useCallback(async () => {
    if (!token || !selectedAgent || !currentPath) return;

    setLoading(true);
    setError('');
    setFiles([]);
    try {
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      const response = await explorerApi.getFiles(baseUrl, token, selectedAgent, currentPath);

      if (response.code === 200 && response.data) {
        const decoded = base64ToUtf8(response.data);
        console.log("文件原始JSON:", decoded);

        const rawList = JSON.parse(decoded) || [];

        const normalized: FileItem[] = rawList.map((f: any) => ({
          fileName: f.fileName ?? f.FileName ?? '',
          changeDate: f.changeDate ?? f.ChangeDate ?? '',
          size: f.size ?? f.Size ?? '',
          type: f.type ?? f.Type ?? '',
          isFolder: f.isFolder ?? f.IsFolder ?? false
        }));

        setFiles(normalized);
      } else {
        setError(response.message || "获取文件失败");
      }
    } catch (err: any) {
      setError(err.message || "文件解析错误");
    } finally {
      setLoading(false);
    }
  }, [token, selectedAgent, currentPath]);


const navigateToFolder = (folderName: string) => {
  if (folderName === '..') {
    const lastSlash = currentPath.lastIndexOf('\\');
    if (lastSlash > 2) {
      setCurrentPath(currentPath.substring(0, lastSlash));
    } else {
      setShowDisks(true);
      setCurrentPath('');
    }
    return;
  }
  if (showDisks) {
    const rootPath = folderName.endsWith('\\')
      ? folderName
      : folderName + '\\';

    setShowDisks(false);
    setCurrentPath(rootPath);
    return;
  }

  setCurrentPath(`${currentPath}${folderName}\\`);
};


  useEffect(() => {
    fetchOnlineAgents();
    const timer = setInterval(fetchOnlineAgents, 2000);
    return () => clearInterval(timer);
  }, [fetchOnlineAgents]);

  useEffect(() => {
    if (selectedAgent) {
      setShowDisks(true);
      setCurrentPath('');
      fetchDisks();
    }
  }, [selectedAgent, fetchDisks]);

  useEffect(() => {
    if (!showDisks && currentPath) {
      fetchFiles();
    }
  }, [showDisks, currentPath, fetchFiles]);

  return (
    <DefaultLayout>
      <div className="flex-1 p-6 h-screen overflow-y-auto">
        <h1 className="text-2xl font-bold mb-6">文件资源管理器</h1>

        <Card className="p-6">
          <Select
            value={selectedAgent}
            onChange={(v) => setSelectedAgent(v as string)}
            className="mb-4"
          >
            <Select.Trigger>
              <Select.Value />
            </Select.Trigger>
            <Select.Popover>
              <ListBox>
                {agents.map(agent => (
                  <ListBox.Item key={agent} id={agent} textValue={agent}>
                    {agent}
                  </ListBox.Item>
                ))}
              </ListBox>
            </Select.Popover>
          </Select>

          {error && <Alert color="danger">{error}</Alert>}

          {selectedAgent && (
            <div className="mb-4">
              <Label className="block text-sm font-medium mb-1">当前路径</Label>
              <Surface className="p-2 rounded-lg">
                <Breadcrumbs>
                  <Breadcrumbs.Item onClick={() => setShowDisks(true)}>
                    磁盘列表
                  </Breadcrumbs.Item>
                  {!showDisks && currentPath.split('\\').filter(Boolean).map((part, index, parts) => {
                    const path = `/${parts.slice(0, index + 1).join('\\')}`;
                    return (
                      <Breadcrumbs.Item key={index} onClick={() => setCurrentPath(path)}>
                        {part}
                      </Breadcrumbs.Item>
                    );
                  })}
                </Breadcrumbs>
              </Surface>
            </div>
          )}

          {/* 磁盘或文件列表 */}
          <ListBox aria-label={showDisks ? "磁盘列表" : "文件列表"} className="w-full" selectionMode="none">
            {!showDisks && currentPath && (
              <ListBox.Item 
                id="parent" 
                textValue=".." 
                onAction={() => navigateToFolder('..')}>
                <div className="flex items-center">
                  <Label>..</Label>
                </div>
              </ListBox.Item>
            )}
            {showDisks
                ? disks.map((disk) => (
                  <ListBox.Item
                    key={disk.name}
                    id={disk.name}
                    textValue={disk.name}
                    onAction={() => navigateToFolder(disk.name)}
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
                ))
                : files.map((file) => (
                  <ListBox.Item
                    key={file.fileName}
                    id={file.fileName}
                    textValue={file.fileName}
                    onAction={() =>
                      file.isFolder && navigateToFolder(file.fileName)
                    }
                  >
                    <div className="flex flex-col">
                      <Label>{file.fileName}</Label>
                      <Description>
                        {file.type} · {file.size} · {file.changeDate}
                      </Description>
                    </div>
                  </ListBox.Item>
                ))
              }
          </ListBox>

          {loading && <div className="mt-4 text-sm">加载中...</div>}
        </Card>
      </div>
    </DefaultLayout>
  );
}

export default Explorer;