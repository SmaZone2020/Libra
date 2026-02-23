import { useState } from 'react';
import { Button, Card, Surface, Separator, Link } from '@heroui/react';
import { useNavigate } from 'react-router-dom';

function Main() {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const navigate = useNavigate();

  const handleLogout = () => {
    // 清除登录信息
    localStorage.removeItem('baseUrl');
    localStorage.removeItem('token');
    localStorage.removeItem('isLoggedIn');
    // 跳转到登录页面
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-100 flex">
      {/* 侧边栏 */}
      <Surface 
        className={`bg-gray-800 text-white transition-all duration-300 ease-in-out ${
          sidebarCollapsed ? 'w-20' : 'w-64'
        }`}
      >
        <div className="p-4 flex items-center justify-between">
          {!sidebarCollapsed && (
            <h1 className="text-xl font-bold">Libra</h1>
          )}
          <Button
            variant="ghost"
            className="text-white hover:bg-gray-700"
            onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
          >
            {sidebarCollapsed ? '>' : '<'}
          </Button>
        </div>
        <Separator className="bg-gray-700" />
        <div className="py-4">
          {[
            { name: '控制面板', icon: '📊' },
            { name: '代理管理', icon: '🖥️' },
            { name: '任务管理', icon: '📋' },
            { name: '文件管理', icon: '📁' },
            { name: '系统设置', icon: '⚙️' }
          ].map((item, index) => (
            <div key={index} className="px-4 py-2">
              <Link 
                className="flex items-center gap-2 hover:bg-gray-700 p-2 rounded-md transition-colors"
              >
                <span>{item.icon}</span>
                {!sidebarCollapsed && <span>{item.name}</span>}
              </Link>
            </div>
          ))}
        </div>
        <div className="absolute bottom-0 w-full p-4">
          <Separator className="bg-gray-700 mb-4" />
          <Button
            variant="ghost"
            className="w-full text-white hover:bg-gray-700 justify-start"
            onClick={handleLogout}
          >
            <span>🚪</span>
            {!sidebarCollapsed && <span className="ml-2">退出登录</span>}
          </Button>
        </div>
      </Surface>

      {/* 主内容区域 */}
      <div className="flex-1 flex flex-col">
        {/* 顶部导航栏 */}
        <Surface className="bg-white shadow-sm p-4 flex justify-between items-center">
          <h2 className="text-lg font-semibold">控制面板</h2>
          <div className="flex items-center gap-4">
            <Button variant="ghost">设置</Button>
            <Button variant="ghost">帮助</Button>
          </div>
        </Surface>

        {/* 内容区域 */}
        <div className="flex-1 p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
            {[
              { title: '在线代理', value: '12', icon: '🟢' },
              { title: '离线代理', value: '3', icon: '🔴' },
              { title: '任务总数', value: '45', icon: '📋' },
              { title: '系统状态', value: '正常', icon: '✅' }
            ].map((stat, index) => (
              <Card key={index} className="p-4">
                <div className="flex justify-between items-center">
                  <div>
                    <p className="text-sm text-gray-500">{stat.title}</p>
                    <h3 className="text-2xl font-bold mt-1">{stat.value}</h3>
                  </div>
                  <div className="text-2xl">{stat.icon}</div>
                </div>
              </Card>
            ))}
          </div>

          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">系统概览</h3>
            <p className="text-gray-600">
              欢迎使用 Libra 天秤座 C2 框架。这里是控制面板的主页面，您可以通过左侧
              菜单栏访问各个功能模块。
            </p>
            <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
              <Button className="w-full">查看所有代理</Button>
              <Button variant="secondary" className="w-full">创建新任务</Button>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}

export default Main;