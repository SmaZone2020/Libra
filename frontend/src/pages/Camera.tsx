import { useState, useCallback, useEffect, useRef } from 'react';
import { Card, Button, Alert, Select, ListBox, Label } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi, cameraStreamApi } from '../services/api';
import { ScreenFrame } from '../types';

function loadImage(src: string): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = () => reject(new Error('图片加载失败'));
    img.src = src;
  });
}

type StreamStatus = 'idle' | 'connecting' | 'streaming' | 'error';

const FPS_OPTIONS = [5, 10, 15, 20, 30] as const;

function Camera() {
  const token   = localStorage.getItem('libra-token')    ?? '';
  const baseUrl = localStorage.getItem('libra-base-url') ?? 'http://localhost:5114';

  const [agents,        setAgents]        = useState<string[]>([]);
  const [selectedAgent, setSelectedAgent] = useState('');
  const [cameras,       setCameras]       = useState<string[]>([]);
  const [cameraIndex,   setCameraIndex]   = useState(0);
  const [fps,           setFps]           = useState<number>(10);
  const [streaming,     setStreaming]      = useState(false);
  const [status,        setStatus]        = useState<StreamStatus>('idle');
  const [error,         setError]         = useState('');
  const [realFps,       setRealFps]       = useState(0);
  const [frameCount,    setFrameCount]    = useState(0);

  const canvasRef   = useRef<HTMLCanvasElement>(null);
  const stopRef     = useRef<(() => void) | null>(null);
  const fpsCountRef = useRef(0);
  const fpsTimerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // ── 获取代理列表 ──────────────────────────────────────────────
  const fetchAgents = useCallback(async () => {
    try {
      const res = await agentApi.getOnlineAgentsWithType(baseUrl, token);
      if (res.code === 200) setAgents(res.data.agents);
    } catch { /* 静默忽略 */ }
  }, [baseUrl, token]);

  useEffect(() => {
    fetchAgents();
    const id = setInterval(fetchAgents, 2000);
    return () => clearInterval(id);
  }, [fetchAgents]);

  // ── 切换代理时拉取摄像头列表 ──────────────────────────────────
  useEffect(() => {
    if (!selectedAgent) { setCameras([]); setCameraIndex(0); return; }
    agentApi.getOnlineAgents(baseUrl, token).then(res => {
      if (res.code === 200) {
        const agent = res.data.agents.find(a => a.agentId === selectedAgent);
        const list = agent?.hardware?.cameras ?? [];
        setCameras(list);
        setCameraIndex(0);
      }
    }).catch(() => {});
  }, [selectedAgent, baseUrl, token]);

  // ── 绘制帧到 canvas ───────────────────────────────────────────
  const applyFrame = useCallback(async (frame: ScreenFrame) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    try {
      if (frame.isFull && frame.data) {
        const img = await loadImage(`data:image/jpeg;base64,${frame.data}`);
        if (canvas.width !== img.width || canvas.height !== img.height) {
          canvas.width  = img.width;
          canvas.height = img.height;
        }
        ctx.drawImage(img, 0, 0);
      }
    } catch { /* 单帧渲染失败不影响后续帧 */ }
    fpsCountRef.current++;
    setFrameCount(c => c + 1);
  }, []);

  // ── 启动流 ────────────────────────────────────────────────────
  const startStreamInternal = useCallback((agentId: string, camIdx: number, targetFps: number) => {
    setError('');
    setStatus('connecting');
    setFrameCount(0);
    fpsCountRef.current = 0;

    fpsTimerRef.current = setInterval(() => {
      setRealFps(fpsCountRef.current);
      fpsCountRef.current = 0;
    }, 1000);

    const stop = cameraStreamApi.createStream(
      baseUrl, token, agentId, camIdx, targetFps,
      (frame) => { setStatus('streaming'); applyFrame(frame); },
      (msg)   => {
        setError(msg);
        setStatus('error');
        setStreaming(false);
        if (fpsTimerRef.current) clearInterval(fpsTimerRef.current);
      }
    );

    stopRef.current = stop;
    setStreaming(true);
  }, [baseUrl, token, applyFrame]);

  const startStream = useCallback(() => {
    if (!selectedAgent || streaming) return;
    startStreamInternal(selectedAgent, cameraIndex, fps);
  }, [selectedAgent, cameraIndex, fps, streaming, startStreamInternal]);

  const stopStream = useCallback(() => {
    stopRef.current?.();
    stopRef.current = null;
    if (fpsTimerRef.current) clearInterval(fpsTimerRef.current);
    setStreaming(false);
    setStatus('idle');
    setRealFps(0);
  }, []);

  // 页面卸载时自动停流
  useEffect(() => () => {
    stopRef.current?.();
    if (fpsTimerRef.current) clearInterval(fpsTimerRef.current);
  }, []);

  // ── 状态指示 ──────────────────────────────────────────────────
  const dotClass =
    status === 'streaming'  ? 'bg-green-500 animate-pulse' :
    status === 'connecting' ? 'bg-yellow-500 animate-pulse' :
    status === 'error'      ? 'bg-red-500' : 'bg-gray-400';

  const statusText =
    status === 'idle'       ? '未连接' :
    status === 'connecting' ? '连接中…' :
    status === 'error'      ? '连接断开' :
    `流式传输中 · ${realFps} FPS · 已接收 ${frameCount} 帧`;

  return (
    <DefaultLayout>
      <div className="flex-1 p-6 h-screen overflow-y-auto">
        <h1 className="text-2xl font-bold mb-6">摄像头监控</h1>

        <Card className="p-6 mb-6">
          <div className="space-y-4">

            {/* 代理选择 */}
            <div>
              <Label className="block text-sm font-medium mb-1">选择代理</Label>
              <Select
                value={selectedAgent}
                onChange={(v) => { stopStream(); setSelectedAgent(v as string); }}
                className="w-full"
              >
                <Select.Trigger><Select.Value /><Select.Indicator /></Select.Trigger>
                <Select.Popover>
                  <ListBox>
                    {agents.map(a => <ListBox.Item key={a} id={a}>{a}</ListBox.Item>)}
                  </ListBox>
                </Select.Popover>
              </Select>
            </div>

            {/* 摄像头选择 */}
            {cameras.length > 0 && (
              <div>
                <Label className="block text-sm font-medium mb-1">选择摄像头</Label>
                <div className="flex gap-2 flex-wrap">
                  {cameras.map((name, idx) => (
                    <Button
                      key={idx}
                      size="sm"
                      variant={cameraIndex === idx ? 'primary' : 'outline'}
                      onClick={() => { if (streaming) { stopStream(); setTimeout(() => startStreamInternal(selectedAgent, idx, fps), 0); } setCameraIndex(idx); }}
                    >
                      {name || `摄像头 ${idx}`}
                    </Button>
                  ))}
                </div>
              </div>
            )}

            {/* FPS 选择 */}
            <div>
              <Label className="block text-sm font-medium mb-1">帧率</Label>
              <div className="flex gap-2 flex-wrap">
                {FPS_OPTIONS.map(f => (
                  <Button
                    key={f}
                    size="sm"
                    variant={fps === f ? 'primary' : 'outline'}
                    onClick={() => { setFps(f); if (streaming) { stopStream(); setTimeout(() => startStreamInternal(selectedAgent, cameraIndex, f), 0); } }}
                  >
                    {f} FPS
                  </Button>
                ))}
              </div>
            </div>

            {/* 状态栏 */}
            <div className="flex items-center gap-2 text-sm text-muted">
              <span className={`inline-block h-2 w-2 rounded-full flex-shrink-0 ${dotClass}`} />
              <span>{statusText}</span>
            </div>

            {/* 控制按钮 */}
            <div className="grid grid-cols-2 gap-4">
              <Button variant="primary" onClick={startStream} isDisabled={streaming || !selectedAgent}>
                开始
              </Button>
              <Button variant="secondary" onClick={stopStream} isDisabled={!streaming}>
                结束
              </Button>
            </div>

            {error && <Alert color="danger">{error}</Alert>}

            {/* 画面 */}
            <div className="mt-4">
              <div
                className="border border-border rounded-lg overflow-hidden flex items-center justify-center"
                style={{ minHeight: '400px', background: '#000' }}
              >
                <canvas
                  ref={canvasRef}
                  className="max-w-full max-h-full"
                  style={{ display: frameCount > 0 ? 'block' : 'none' }}
                />
                {frameCount === 0 && (
                  <p className="text-muted text-sm">选择代理后点击「开始」启动摄像头流</p>
                )}
              </div>
            </div>

          </div>
        </Card>
      </div>
    </DefaultLayout>
  );
}

export { Camera as default };
