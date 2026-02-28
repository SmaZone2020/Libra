import { Routes, Route, Navigate } from 'react-router-dom';
import Login from '../pages/Login';
import Main from '../pages/Main';
import Agents from '../pages/Agents';
import Shell from '../pages/Shell';
import Explorer from '../pages/Explorer';
import RequireAuth from '../components/RequireAuth';
import Camera from '../pages/Camera';
import ScreenStream from '../pages/ScreenStream';

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

      <Route
        path="/screen-stream"
        element={
          <RequireAuth>
            <ScreenStream />
          </RequireAuth>
        }
      />

      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}

export default AppRoutes;

