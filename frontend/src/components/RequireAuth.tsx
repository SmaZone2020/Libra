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

  const getTokenFromLocalStorage = (): string | null => {
    return localStorage.getItem('libra-token');
  };

  const getBaseUrlFromLocalStorage = (): string | null => {
    return localStorage.getItem('libra-base-url');
  };

  const removeTokenFromLocalStorage = (): void => {
    localStorage.removeItem('libra-token');
  };

  const removeBaseUrlFromLocalStorage = (): void => {
    localStorage.removeItem('libra-base-url');
  };

  useEffect(() => {
    const validateToken = async () => {
      const savedBaseUrl = getBaseUrlFromLocalStorage();
      const savedToken = getTokenFromLocalStorage();

      if (savedBaseUrl && savedToken) {
        try {
          const response = await authApi.validateToken(savedBaseUrl, savedToken);
          
          if (response.code === 200 && response.data.valid) {
            setIsValid(true);
          } else {
            removeBaseUrlFromLocalStorage();
            removeTokenFromLocalStorage();
            localStorage.removeItem('isLoggedIn');
            navigate('/login', { replace: true });
          }
        } catch (err) {
          removeBaseUrlFromLocalStorage();
          removeTokenFromLocalStorage();
          localStorage.removeItem('isLoggedIn');
          navigate('/login', { replace: true });
        } finally {
          setIsLoading(false);
        }
      } else {
        navigate('/login', { replace: true });
        setIsLoading(false);
      }
    };

    validateToken();
  }, [navigate]);

  if (isLoading) {
    return null;
  }

  if (!isValid) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}

export default RequireAuth;