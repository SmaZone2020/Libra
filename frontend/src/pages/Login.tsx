import { useState, useEffect } from 'react';
import { Button, Card, Input, Label, Form, Fieldset, ErrorMessage, InputOTP, Modal } from '@heroui/react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../services/api';

function Login() {
  const [baseUrl, setBaseUrl] = useState('');
  const [token, setToken] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showTotp, setShowTotp] = useState(false);
  const [showQrModal, setShowQrModal] = useState(false);
  const [qrCodeUrl, setQrCodeUrl] = useState('');
  const navigate = useNavigate();

  // 从存储中获取token
  const getTokenFromStorage = (): string | null => {
    return localStorage.getItem('user-info');
  };

  // 将token存储到存储中
  const setTokenToStorage = (token: string): void => {
    localStorage.setItem('user-info', token);
  };

  // 从存储中删除token
  const removeTokenFromStorage = (): void => {
    localStorage.removeItem('user-info');
  };

  // 从存储中获取baseUrl
  const getBaseUrlFromStorage = (): string | null => {
    return localStorage.getItem('baseUrl');
  };

  // 将baseUrl存储到存储中
  const setBaseUrlToStorage = (baseUrl: string): void => {
    localStorage.setItem('baseUrl', baseUrl);
  };

  // 从存储中删除baseUrl
  const removeBaseUrlFromStorage = (): void => {
    localStorage.removeItem('baseUrl');
  };

  useEffect(() => {
    const validateToken = async () => {
      const savedBaseUrl = getBaseUrlFromStorage();
      const savedToken = getTokenFromStorage();
      console.log('Stored values:', savedBaseUrl, savedToken);
      
      // 如果都为空则保持在Login页面
      if (!savedBaseUrl || !savedToken) {
        return;
      }
      
      // 若不为空则先尝试通过baseUrl请求status接口，检验token是否有效
      try {
        const response = await authApi.validateToken(savedBaseUrl, savedToken);
        
        if (response.code === 200 && response.data.valid) {
          navigate('/main');
        } else {
          // 无效则保持在Login页面
          removeBaseUrlFromStorage();
          removeTokenFromStorage();
          localStorage.removeItem('isLoggedIn');
        }
      } catch (err) {
        // 错误则保持在Login页面
        removeBaseUrlFromStorage();
        removeTokenFromStorage();
        localStorage.removeItem('isLoggedIn');
      }
    };

    validateToken();
  }, [navigate]);

  const testConnectivity = async () => {
    setError('');
    setIsLoading(true);

    try {
      if (!baseUrl) {
        throw new Error('请输入BaseURL');
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
      setError(err instanceof Error ? err.message : '连接失败，请重试');
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
        throw new Error('请填写完整的登录信息');
      }

      const response = await authApi.login(baseUrl, token);
      
      if (response.code !== 200) {
        throw new Error(response.message || '登录失败，请重试');
      }

      setBaseUrlToStorage(baseUrl);
      setTokenToStorage(response.data);
      localStorage.setItem('isLoggedIn', 'true');

      navigate('/main');
    } catch (err) {
      setError(err instanceof Error ? err.message : '登录失败，请重试');
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
              onChange={(e) => setBaseUrl(e.target.value)}
              placeholder="https://example.com"
              required
            />
          </Fieldset>

          {!showTotp ? (
            <Button 
              type="button" 
              className="w-full mb-4" 
              isDisabled={isLoading}
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
                isDisabled={isLoading}
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