import { Routes, Route, Navigate } from 'react-router-dom';
import Login from '../pages/Login';
import Main from '../pages/Main';
import Agents from '../pages/Agents';
import Shell from '../pages/Shell';
import Monitor from '../pages/Monitor';
import Explorer from '../pages/Explorer';
import RequireAuth from '../components/RequireAuth';
import Camera from '../pages/Camera';

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      
      <Route 
        path="/" 
        element={
          <RequireAuth>
            <Main />
          </RequireAuth>
        } 
      />

      <Route 
        path="/agents" 
        element={
          <RequireAuth>
            <Agents />
          </RequireAuth>
        } 
      />

      <Route 
        path="/shell" 
        element={
          <RequireAuth>
            <Shell />
          </RequireAuth>
        } 
      />

      <Route 
        path="/monitor" 
        element={
          <RequireAuth>
            <Monitor />
          </RequireAuth>
        } 
      />

      <Route 
        path="/camera" 
        element={
          <RequireAuth>
            <Camera />
          </RequireAuth>
        } 
      />

      <Route 
        path="/explorer" 
        element={
          <RequireAuth>
            <Explorer />
          </RequireAuth>
        } 
      />

      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}

export default AppRoutes;
