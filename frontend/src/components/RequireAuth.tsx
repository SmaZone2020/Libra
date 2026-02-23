import { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';

interface RequireAuthProps {
  children: ReactNode;
}

function RequireAuth({ children }: RequireAuthProps) {
  const location = useLocation();
  const isLoggedIn = localStorage.getItem('isLoggedIn') === 'true';

  if (!isLoggedIn) {
    // 保存当前位置，登录后可以重定向回来
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}

export default RequireAuth;