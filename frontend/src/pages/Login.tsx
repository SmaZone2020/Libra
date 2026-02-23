import { useState } from 'react';
import { Button, Card, Input, Label, Form, Fieldset, ErrorMessage } from '@heroui/react';
import { useNavigate } from 'react-router-dom';

function Login() {
  const [baseUrl, setBaseUrl] = useState('');
  const [token, setToken] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      // 简单验证输入
      if (!baseUrl || !token) {
        throw new Error('请填写完整的登录信息');
      }

      // 模拟登录验证
      // 实际项目中应该调用后端API进行验证
      await new Promise(resolve => setTimeout(resolve, 1000));

      // 保存登录信息到localStorage
      localStorage.setItem('baseUrl', baseUrl);
      localStorage.setItem('token', token);
      localStorage.setItem('isLoggedIn', 'true');

      // 跳转到主页面
      navigate('/main');
    } catch (err) {
      setError(err instanceof Error ? err.message : '登录失败，请重试');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <Card className="w-full max-w-md p-8 shadow-lg">
        <h1 className="text-2xl font-bold mb-6 text-center">Libra 登录</h1>
        
        {error && (
          <ErrorMessage className="mb-4">{error}</ErrorMessage>
        )}

        <Form onSubmit={handleSubmit}>
          <Fieldset className="mb-4">
            <Label htmlFor="baseUrl">服务端地址 (BaseURL)</Label>
            <Input
              id="baseUrl"
              type="url"
              value={baseUrl}
              onChange={(e) => setBaseUrl(e.target.value)}
              placeholder="https://example.com"
              required
            />
          </Fieldset>

          <Fieldset className="mb-6">
            <Label htmlFor="token">登录令牌</Label>
            <Input
              id="token"
              type="password"
              value={token}
              onChange={(e) => setToken(e.target.value)}
              placeholder="请输入登录令牌"
              required
            />
          </Fieldset>

          <Button 
            type="submit" 
            className="w-full" 
            disabled={isLoading}
          >
            {isLoading ? '验证中...' : '登录'}
          </Button>
        </Form>
      </Card>
    </div>
  );
}

export default Login;