import { useState, useCallback, useEffect } from 'react';
import { Card, Select, ListBox, Label, Alert, Tabs } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi, explorerApi } from '../services/api';
import { FileItem as FileItemType, DiskItem as DiskItemType } from '../types';
import { base64ToUtf8 } from '../utils';
import DiskItem from '../components/explorer/DiskItem';
import FileItem from '../components/explorer/FileItem';
import BreadcrumbNav from '../components/explorer/BreadcrumbNav';
import { LayoutGrid, List } from 'lucide-react';

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
  const [viewMode, setViewMode] = useState<'list' | 'grid'>('list');


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
      const parent = getParentPath(currentPath);

      if (!parent) {
        setShowDisks(true);
        setCurrentPath('');
      } else {
        setCurrentPath(parent);
        setShowDisks(false);
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

  const getParentPath = (path: string) => {
    if (!path) return '';
    const trimmed = path.endsWith('\\')
      ? path.slice(0, -1)
      : path;

    const index = trimmed.lastIndexOf('\\');
    if (index === -1) return '';

    const parent = trimmed.substring(0, index + 1);
    if (parent.length <= 3) {
      return parent;
    }

    return parent;
  };

  const handleNavigateToPath = (path: string) => {
    setCurrentPath(path);
  };

  const handleDownloadFile = async (fileName: string) => {
    console.log(!token || !selectedAgent)
    if (!token || !selectedAgent) return;
    
    setLoading(true);
    setError('');

    try {
      const fullPath = `${currentPath}${fileName}`;
      const baseUrl = localStorage.getItem("libra-base-url") || "http://localhost:5114";
      const response = await explorerApi.getFile(baseUrl, token, selectedAgent, fullPath);

      if (response.code === 200 && response.data && response.data.content) {
        // 处理Base64内容并下载
        const content = response.data.content;
        const fileName = response.data.fileName;

        // 解码Base64内容
        const binaryString = atob(content);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
          bytes[i] = binaryString.charCodeAt(i);
        }

        // 创建Blob对象
        const blob = new Blob([bytes], { type: 'application/octet-stream' });
        const url = URL.createObjectURL(blob);

        // 创建下载链接并触发下载
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      } else {
        setError('文件不存在或为空');
      }
    } catch (err: any) {
      setError(err.message || '下载文件失败');
    } finally {
      setLoading(false);
    }
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
              <BreadcrumbNav 
                currentPath={currentPath}
                showDisks={showDisks}
                onNavigateToRoot={() => setShowDisks(true)}
                onNavigateToPath={handleNavigateToPath}
              />
              
              {/* 视图切换 */}
              {!showDisks && currentPath && (
                <div className="mt-4 mb-6 max-w-[120px]">
                  <Tabs variant="secondary"
                        selectedKey={viewMode} 
                        onSelectionChange={(key) => setViewMode(key as 'list' | 'grid')}>
                    <Tabs.ListContainer>
                      <Tabs.List aria-label="视图选项">
                        <Tabs.Tab id="list">
                          <List/>
                          <Tabs.Indicator />
                        </Tabs.Tab>
                        <Tabs.Tab id="grid">
                          <LayoutGrid/>
                          <Tabs.Indicator />
                        </Tabs.Tab>
                      </Tabs.List>
                    </Tabs.ListContainer>
                  </Tabs>
                </div>
              )}
            </div>
          )}

          {/* 磁盘或文件列表 */}
          {showDisks ? (
            <ListBox aria-label="磁盘列表" className="w-full" selectionMode="none">
              {disks.map((disk) => (
                <DiskItem 
                  key={disk.name} 
                  disk={disk} 
                  onAction={() => navigateToFolder(disk.name)} 
                />
              ))}
            </ListBox>
          ) : (
            <>
              {viewMode === 'list' ? (
                <ListBox aria-label="文件列表" className="w-full" selectionMode="none">
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
                  {files.map((file) => (
                    <FileItem 
                      key={file.fileName} 
                      file={file} 
                      onAction={file.isFolder ? () => navigateToFolder(file.fileName) : () => handleDownloadFile(file.fileName)} 
                    />
                  ))}
                </ListBox>
              ) : (
                <div className="grid gap-4 grid-cols-[repeat(auto-fill,minmax(160px,1fr))]">
                  {!showDisks && currentPath && (
                    <Card 
                      key="parent" 
                      className="p-4 cursor-pointer hover:bg-gray-50" 
                      onClick={() => navigateToFolder('..')}
                    >
                      <div className="flex flex-col items-center text-center">
                        <Label className="mb-2">..</Label>
                        <div className="text-sm text-gray-500">
                          父目录
                        </div>
                      </div>
                    </Card>
                  )}
                  {files.map((file) => (
                    <Card 
                      key={file.fileName} 
                      className="p-4 cursor-pointer hover:bg-gray-50" 
                      onClick={file.isFolder ? () => navigateToFolder(file.fileName) : () => handleDownloadFile(file.fileName)}
                    >
                      <div className="flex flex-col items-center text-center">
                        <Label className="mb-2">{file.fileName}</Label>
                        <div className="text-sm text-gray-500">
                          {file.type} · {file.size}
                        </div>
                      </div>
                    </Card>
                  ))}
                </div>
              )}
            </>
          )}

          {loading && <div className="mt-4 text-sm">加载中...</div>}
        </Card>
      </div>
    </DefaultLayout>
  );
}

export default Explorer;