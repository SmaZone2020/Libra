import { Routes, Route, Navigate } from 'react-router-dom';
import Login from '../pages/Login';
import Main from '../pages/Main';
import RequireAuth from '../components/RequireAuth';

function AppRoutes() {
  return (
    <Routes>
      {/* 登录页面 - 无需认证 */}
      <Route path="/login" element={<Login />} />
      
      {/* 主页面 - 需要认证 */}
      <Route 
        path="/main" 
        element={
          <RequireAuth>
            <Main />
          </RequireAuth>
        } 
      />
      
      {/* 默认重定向到登录页面 */}
      <Route path="/" element={<Navigate to="/login" replace />} />
      
      {/* 捕获所有其他路径，重定向到登录页面 */}
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}

export default AppRoutes;
