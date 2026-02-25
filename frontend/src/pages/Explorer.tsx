import { useState, useCallback, useEffect } from 'react';
import { Card, Select, ListBox, Label, Alert } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi, explorerApi } from '../services/api';
import { FileItem as FileItemType, DiskItem as DiskItemType } from '../types';
import { base64ToUtf8 } from '../utils';
import DiskItem from '../components/explorer/DiskItem';
import FileItem from '../components/explorer/FileItem';
import BreadcrumbNav from '../components/explorer/BreadcrumbNav';

function Explorer() {
  const token = localStorage.getItem("libra-token");

  const [agents, setAgents] = useState<string[]>([]);
  const [selectedAgent, setSelectedAgent] = useState<string>('');
  const [currentPath, setCurrentPath] = useState<string>('');
  const [files, setFiles] = useState<FileItemType[]>([]);
  const [disks, setDisks] = useState<DiskItemType[]>([]);
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

        const normalized: DiskItemType[] = rawList.map((d: any) => ({
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

        const normalized: FileItemType[] = rawList.map((f: any) => ({
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

  const handleNavigateToPath = (path: string) => {
    setCurrentPath(path);
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
              <BreadcrumbNav 
                currentPath={currentPath}
                showDisks={showDisks}
                onNavigateToRoot={() => setShowDisks(true)}
                onNavigateToPath={handleNavigateToPath}
              />
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
                  <DiskItem 
                    key={disk.name} 
                    disk={disk} 
                    onAction={() => navigateToFolder(disk.name)} 
                  />
                ))
                : files.map((file) => (
                  <FileItem 
                    key={file.fileName} 
                    file={file} 
                    onAction={() => navigateToFolder(file.fileName)} 
                  />
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