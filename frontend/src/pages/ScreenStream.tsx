import { useState, useCallback, useEffect, useRef } from 'react';
import { Card, Button, Alert, Select, ListBox, Label } from '@heroui/react';
import DefaultLayout from '../layouts/DefaultLayout';
import { agentApi, screenStreamApi } from '../services/api';
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

const QUALITY_OPTIONS = [
  { value: 'native', label: '原画' },
  { value: '1080p',  label: '1080P' },
  { value: '720p',   label: '720P' },
  { value: '540p',   label: '540P' },
  { value: '370p',   label: '370P' },
] as const;

function ScreenStream() {
  const token   = localStorage.getItem('libra-token')    ?? '';
  const baseUrl = localStorage.getItem('libra-base-url') ?? 'http://localhost:5114';

  const [agents,        setAgents]        = useState<string[]>([]);
  const [selectedAgent, setSelectedAgent] = useState('');
  const [quality,       setQuality]       = useState('720p');
  const [streaming,     setStreaming]      = useState(false);
  const [status,        setStatus]        = useState<StreamStatus>('idle');
  const [error,         setError]         = useState('');
  const [fps,           setFps]           = useState(0);
  const [frameCount,    setFrameCount]    = useState(0);

  const canvasRef    = useRef<HTMLCanvasElement>(null);
  const stopRef      = useRef<(() => void) | null>(null);
  const fpsCountRef  = useRef(0);
  const fpsTimerRef  = useRef<ReturnType<typeof setInterval> | null>(null);

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

  // ── 将一帧绘制到 canvas ────────────────────────────────────────
  const applyFrame = useCallback(async (frame: ScreenFrame) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    try {
      if (frame.isFull && frame.data) {
        canvas.width  = frame.screenWidth;
        canvas.height = frame.screenHeight;
        const img = await loadImage(`data:image/jpeg;base64,${frame.data}`);
        ctx.drawImage(img, 0, 0);
      } else if (!frame.isFull && frame.blocks && frame.blocks.length > 0) {
        const images = await Promise.all(
          frame.blocks.map(b => loadImage(`data:image/jpeg;base64,${b.data}`))
        );
        frame.blocks.forEach((b, i) => ctx.drawImage(images[i], b.x, b.y));
      }
    } catch { /* 单帧渲染失败不影响后续帧 */ }

    fpsCountRef.current++;
    setFrameCount(c => c + 1);
  }, []);

  // ── 启动流（内部逻辑，不直接改 streaming 状态，由调用方管理）
  const startStreamInternal = useCallback((agentId: string, q: string) => {
    setError('');
    setStatus('connecting');
    setFrameCount(0);
    fpsCountRef.current = 0;

    fpsTimerRef.current = setInterval(() => {
      setFps(fpsCountRef.current);
      fpsCountRef.current = 0;
    }, 1000);

    const stop = screenStreamApi.createStream(
      baseUrl, token, agentId, q,
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

  // ── 开始 ──────────────────────────────────────────────────────
  const startStream = useCallback(() => {
    if (!selectedAgent || streaming) return;
    startStreamInternal(selectedAgent, quality);
  }, [selectedAgent, quality, streaming, startStreamInternal]);

  // ── 结束 ──────────────────────────────────────────────────────
  const stopStream = useCallback(() => {
    stopRef.current?.();
    stopRef.current = null;
    if (fpsTimerRef.current) clearInterval(fpsTimerRef.current);
    setStreaming(false);
    setStatus('idle');
    setFps(0);
  }, []);

  // ── 切换清晰度时自动重启流 ────────────────────────────────────
  const handleQualityChange = useCallback((newQuality: string) => {
    setQuality(newQuality);
    if (streaming && selectedAgent) {
      stopRef.current?.();
      stopRef.current = null;
      if (fpsTimerRef.current) clearInterval(fpsTimerRef.current);
      setStreaming(false);
      // 下一 tick 重启，让状态先落定
      setTimeout(() => startStreamInternal(selectedAgent, newQuality), 0);
    }
  }, [streaming, selectedAgent, startStreamInternal]);

  // 页面卸载时自动停流
  useEffect(() => () => {
    stopRef.current?.();
    if (fpsTimerRef.current) clearInterval(fpsTimerRef.current);
  }, []);

  // ── 状态指示 ──────────────────────────────────────────────────
  const dotClass =
    status === 'streaming' ? 'bg-green-500 animate-pulse' :
    status === 'connecting' ? 'bg-yellow-500 animate-pulse' :
    status === 'error'      ? 'bg-red-500' : 'bg-gray-400';

  const statusText =
    status === 'idle'       ? '未连接' :
    status === 'connecting' ? '连接中…' :
    status === 'error'      ? '连接断开' :
    `流式传输中 · ${fps} FPS · 已接收 ${frameCount} 帧`;

  const qualityLabel = QUALITY_OPTIONS.find(o => o.value === quality)?.label ?? quality;

  return (
    <DefaultLayout>
      <div className="flex-1 p-6 h-screen overflow-y-auto">
        <h1 className="text-2xl font-bold mb-6">差异屏幕流</h1>

        <Card className="p-6 mb-6">
          <div className="space-y-4">

            {/* 代理选择 */}
            <div>
              <Label className="block text-sm font-medium mb-1">选择代理</Label>
              <Select
                value={selectedAgent}
                onChange={(v) => setSelectedAgent(v as string)}
                className="w-full"
              >
                <Select.Trigger>
                  <Select.Value />
                  <Select.Indicator />
                </Select.Trigger>
                <Select.Popover>
                  <ListBox>
                    {agents.map(a => (
                      <ListBox.Item key={a} id={a}>{a}</ListBox.Item>
                    ))}
                  </ListBox>
                </Select.Popover>
              </Select>
            </div>

            {/* 清晰度选择 */}
            <div>
              <Label className="block text-sm font-medium mb-1">
                清晰度
                {streaming && (
                  <span className="ml-2 text-xs text-muted font-normal">
                    (切换后自动重连)
                  </span>
                )}
              </Label>
              <div className="flex gap-2 flex-wrap">
                {QUALITY_OPTIONS.map(opt => (
                  <Button
                    key={opt.value}
                    size="sm"
                    variant={quality === opt.value ? 'primary' : 'outline'}
                    onClick={() => handleQualityChange(opt.value)}
                  >
                    {opt.label}
                  </Button>
                ))}
              </div>
            </div>

            {/* 状态栏 */}
            <div className="flex items-center gap-2 text-sm text-muted">
              <span className={`inline-block h-2 w-2 rounded-full flex-shrink-0 ${dotClass}`} />
              <span>{statusText}</span>
              {streaming && (
                <span className="ml-2 px-1.5 py-0.5 text-xs rounded bg-primary/10 text-primary font-medium">
                  {qualityLabel}
                </span>
              )}
            </div>

            {/* 控制按钮 */}
            <div className="grid grid-cols-2 gap-4">
              <Button
                variant="primary"
                onClick={startStream}
                isDisabled={streaming || !selectedAgent}
              >
                开始
              </Button>
              <Button
                variant="secondary"
                onClick={stopStream}
                isDisabled={!streaming}
              >
                结束
              </Button>
            </div>

            {/* 错误提示 */}
            {error && <Alert color="danger">{error}</Alert>}

            {/* 屏幕画布 */}
            <div className="mt-4">
              <h2 className="text-lg font-semibold mb-4">屏幕显示</h2>
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
                  <p className="text-muted text-sm">
                    选择代理后点击「开始」启动差异流式传输
                  </p>
                )}
              </div>
            </div>

          </div>
        </Card>
      </div>
    </DefaultLayout>
  );
}

export default ScreenStream;
