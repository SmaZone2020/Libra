import { Card, Button } from '@heroui/react';

function SystemOverview() {
  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold mb-4">系统概览</h3>
      <p className="text-gray-600">
        欢迎使用 Libra C2 框架。这里是控制面板的主页面，您可以通过左侧
        菜单栏访问各个功能模块。
      </p>
      <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
        <Button className="w-full">查看所有代理</Button>
        <Button variant="secondary" className="w-full">创建新任务</Button>
      </div>
    </Card>
  );
}

export default SystemOverview;
