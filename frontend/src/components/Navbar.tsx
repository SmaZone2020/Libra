import { useLocation, useNavigate } from "react-router-dom";
import { siteConfig } from "../config/site";
import { Button, Tooltip } from "@heroui/react";
import { DynamicIcon } from 'lucide-react/dynamic';
import clsx from "clsx";
import { useLocalStorage } from "../hooks";

export default function NavBar() {
  const location = useLocation();
  const navigate = useNavigate();
  
  // 使用useLocalStorage hook管理主题
  const [isDark, setIsDark] = useLocalStorage<boolean>('isDark', window.matchMedia('(prefers-color-scheme: dark)').matches);

  // 初始化主题
  if (typeof window !== 'undefined') {
    const theme = isDark ? 'dark' : 'light';
    document.documentElement.className = theme;
    document.documentElement.setAttribute('data-theme', theme);
  }

  const toggleTheme = () => {
    setIsDark(!isDark);
  };

  return (
    <aside
      className={clsx(
        `w-[70px] sm:w-[60px] bg-surface border-r border-border shadow-md transition-all duration-300`
      )}
    >
      {/* LOGO */}
      <div className="flex items-center justify-center h-20 border-b border-border">
        <img src={siteConfig.logo} alt={siteConfig.name} className="h-10 w-10"/>
      </div>

      {/* 导航 */}
      <nav className={clsx(`flex flex-col items-center p-2 space-y-1`)}>
        {siteConfig.navItems.map((item, idx) => {
          const isActive = location.pathname === item.href;
          return (
            <Tooltip delay={0} key={idx}>
              {item.href !== "" ? (
                  <Button
                    isIconOnly
                    variant={isActive ? "primary" : "outline"}
                    onClick={() => navigate(item.href)}
                    size="lg"
                    className="rounded-2xl w-[45px] h-[45px]"
                  >
                    <DynamicIcon name={item.icon as any} className="scale-125" />
                  </Button>
              ): null}
              <Tooltip.Content showArrow placement="right" className="text-base">
                {item.label}
              </Tooltip.Content>
            </Tooltip>
          );
        })}
      </nav>

      {/* 主题切换按钮 */}
      <div className="mt-auto flex items-center justify-center p-2 border-t border-border">
        <Tooltip delay={0}>
          <Button
            isIconOnly
            variant="outline"
            onClick={toggleTheme}
            size="lg"
            className="rounded-2xl w-[45px] h-[45px]"
          >
            <DynamicIcon name={isDark ? "sun" : "moon"} className="scale-125" />
          </Button>
          <Tooltip.Content showArrow placement="right" className="text-base">
            {isDark ? "亮色模式" : "暗色模式"}
          </Tooltip.Content>
        </Tooltip>
      </div>
    </aside>
  );
}
