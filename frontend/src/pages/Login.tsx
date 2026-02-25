import { useState, useEffect } from 'react';
import { Button, Card, Input, Label, Form, Fieldset, ErrorMessage, InputOTP, Modal } from '@heroui/react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../services/api';

// 定义存储键名常量，避免硬编码错误
const STORAGE_KEYS = {
  BASE_URL: 'libra-base-url'
};

function Login() {
  const [baseUrl, setBaseUrl] = useState('');
  const [token, setToken] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showTotp, setShowTotp] = useState(false);
  const [showQrModal, setShowQrModal] = useState(false);
  const [qrCodeUrl, setQrCodeUrl] = useState('');
  const navigate = useNavigate();

  // ========== 存储操作函数 ==========
  // 从localStorage获取baseUrl
  const getBaseUrlFromStorage = (): string | null => {
    return localStorage.getItem(STORAGE_KEYS.BASE_URL);
  };

  // 将baseUrl存储到localStorage
  const setBaseUrlToStorage = (baseUrl: string): void => {
    localStorage.setItem(STORAGE_KEYS.BASE_URL, baseUrl);
  };

  // ========== 初始化逻辑 ==========
  useEffect(() => {
    // 只从存储加载baseUrl到页面状态
    const savedBaseUrl = getBaseUrlFromStorage();
    if (savedBaseUrl) setBaseUrl(savedBaseUrl); // 回显BaseURL
  }, []);

  // ========== 测试连接逻辑（优化错误处理） ==========
  const testConnectivity = async () => {
    setError('');
    setIsLoading(true);

    try {
      if (!baseUrl) {
        throw new Error('请输入BaseURL');
      }
      // 校验BaseURL格式
      if (!/^https?:\/\/.+/.test(baseUrl)) {
        throw new Error('BaseURL格式错误，请以http/https开头');
      }
      
      const response = await authApi.ping(baseUrl);
      
      if (response.code !== 200) {
        throw new Error(response.message || '连接失败，请重试');
      }
      
      setBaseUrlToStorage(baseUrl);
      if (response.data) {
        setQrCodeUrl(response.data);
        setShowQrModal(true);
      } else {
        setShowTotp(true);
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : '连接失败，请重试';
      setError(errorMsg);
      console.error('测试连接失败:', errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      if (!baseUrl || !token || token.length !== 6) {
        throw new Error('请填写完整的登录信息（TOTP令牌必须为6位）');
      }

      const response = await authApi.login(baseUrl, token);
      
      if (response.code !== 200) {
        throw new Error(response.message || '登录失败，请重试');
      }
      localStorage.setItem("libra-token",response.data);
      setBaseUrlToStorage(baseUrl);

      navigate('/');
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : '登录失败，请重试';
      setError(errorMsg);
      console.error('登录失败:', errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <Card className="w-[400px] p-8 shadow-lg">
        <h1 className="text-2xl font-bold mb-6 text-center">Libra 登录</h1>
        
        {error && (
          <ErrorMessage className="mb-4">{error}</ErrorMessage>
        )}

        <Form onSubmit={handleSubmit}>
          <Fieldset className="mb-4">
            <Label htmlFor="baseUrl" className="text-base">BaseURL</Label>
            <Input
              id="baseUrl"
              type="url"
              value={baseUrl}
              onChange={(e) => setBaseUrl(e.target.value.trim())} // 去除首尾空格
              placeholder="https://example.com"
              required
            />
          </Fieldset>

          {!showTotp ? (
            <Button 
              type="button" 
              className="w-full mb-4" 
              isDisabled={isLoading || !baseUrl} // 空BaseURL禁用按钮
              onClick={testConnectivity}
            >
              {isLoading ? '测试连接中...' : '测试连通性'}
            </Button>
          ) : (
            <>
              <Fieldset className="mb-6">
                <Label htmlFor="token" className="text-base">TOTP 令牌</Label>
                <InputOTP 
                  maxLength={6} 
                  className='mx-auto'
                  value={token} 
                  onChange={setToken}
                  required >
                  <InputOTP.Group>
                    <InputOTP.Slot index={0} />
                    <InputOTP.Slot index={1} />
                    <InputOTP.Slot index={2} />
                  </InputOTP.Group>
                  <InputOTP.Separator />
                  <InputOTP.Group>
                    <InputOTP.Slot index={3} />
                    <InputOTP.Slot index={4} />
                    <InputOTP.Slot index={5} />
                  </InputOTP.Group>
                </InputOTP>
              </Fieldset>

              <Button 
                type="submit" 
                className="w-full" 
                isDisabled={isLoading || token.length !== 6} // 6位令牌才启用
              >
                {isLoading ? '验证中...' : '校验'}
              </Button>
            </>
          )}
        </Form>

        {/* 二维码模态框 */}
        {showQrModal && (
          <Modal isOpen={showQrModal}>
            <Modal.Backdrop isDismissable={false}>
              <Modal.Container>
                <Modal.Dialog className="sm:max-w-[360px]">
                  <Modal.CloseTrigger />
                  <Modal.Header>
                    <Modal.Heading>
                      请将通行令牌添加到你的TOTP验证应用中
                      请注意! 这只显示一次。请妥善保存。
                    </Modal.Heading>
                  </Modal.Header>
                  <Modal.Body>
                    {qrCodeUrl && (
                      <div className="flex justify-center my-3">
                        <img 
                          src={qrCodeUrl} 
                          alt="TOTP QR Code" 
                          className="w-64 h-64"
                          onError={() => setError('二维码加载失败，请手动添加令牌')} // 二维码加载失败提示
                        />
                      </div>
                    )}
                  </Modal.Body>
                  <Modal.Footer>
                    <Button className="w-full" slot="close" onClick={() => {
                      setShowQrModal(false);
                      setShowTotp(true);
                    }}>
                      我已添加通行令牌到我信任的应用中
                    </Button>
                  </Modal.Footer>
                </Modal.Dialog>
              </Modal.Container>
            </Modal.Backdrop>
          </Modal>
        )}
      </Card>
    </div>
  );
}

export default Login;