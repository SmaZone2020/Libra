import { ReactNode, useEffect, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { authApi } from '../services/api';

interface RequireAuthProps {
  children: ReactNode;
}

function RequireAuth({ children }: RequireAuthProps) {
  const location = useLocation();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(true);
  const [isValid, setIsValid] = useState(false);

  // 从cookie中获取值
  const getCookieValue = (name: string): string | null => {
    const cookieValue = document.cookie
      .split('; ')
      .find(row => row.startsWith(`${name}=`))
      ?.split('=')[1];
    return cookieValue ? decodeURIComponent(cookieValue) : null;
  };

  // 删除cookie值
  const removeCookieValue = (name: string): void => {
    document.cookie = `${name}=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`;
  };

  // 从cookie中获取token
  const getTokenFromCookie = (): string | null => {
    return getCookieValue('token');
  };

  // 从cookie中获取baseUrl
  const getBaseUrlFromCookie = (): string | null => {
    return getCookieValue('baseUrl');
  };

  // 从cookie中删除token
  const removeTokenFromCookie = (): void => {
    removeCookieValue('token');
  };

  // 从cookie中删除baseUrl
  const removeBaseUrlFromCookie = (): void => {
    removeCookieValue('baseUrl');
  };

  useEffect(() => {
    const validateToken = async () => {
      const savedBaseUrl = getBaseUrlFromCookie();
      const savedToken = getTokenFromCookie();
      
      if (savedBaseUrl && savedToken) {
        try {
          const response = await authApi.validateToken(savedBaseUrl, savedToken);
          
          if (response.code === 0 && response.data.valid) {
            setIsValid(true);
          } else {
            // Token无效，清除存储并跳转到登录页
            removeBaseUrlFromCookie();
            removeTokenFromCookie();
            localStorage.removeItem('isLoggedIn');
            navigate('/login', { replace: true });
          }
        } catch (err) {
          // 验证失败，清除存储并跳转到登录页
          removeBaseUrlFromCookie();
          removeTokenFromCookie();
          localStorage.removeItem('isLoggedIn');
          navigate('/login', { replace: true });
        } finally {
          setIsLoading(false);
        }
      } else {
        // 没有存储的token，跳转到登录页
        navigate('/login', { replace: true });
        setIsLoading(false);
      }
    };

    validateToken();
  }, [navigate]);

  if (isLoading) {
    // 加载中，可以添加一个加载指示器
    return null;
  }

  if (!isValid) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}

export default RequireAuth;