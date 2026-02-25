import { useLocation, useNavigate } from "react-router-dom";
import { siteConfig } from "../config/site";
import { Avatar, Button, Tooltip } from "@heroui/react";
import { DynamicIcon } from 'lucide-react/dynamic';
import clsx from "clsx";
import { useEffect, useState } from "react";

export default function NavBar() {
  const location = useLocation();
  const navigate = useNavigate();
  let isActive:boolean = false;
  const [isDark, setIsDark] = useState<boolean>(false);

  useEffect(() => {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
      setIsDark(savedTheme === 'dark');
      document.documentElement.className = savedTheme;
      document.documentElement.setAttribute('data-theme', savedTheme);
    } else {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      setIsDark(prefersDark);
      document.documentElement.className = prefersDark ? 'dark' : 'light';
      document.documentElement.setAttribute('data-theme', prefersDark ? 'dark' : 'light');
    }
  }, []);

  const toggleTheme = () => {
    const newTheme = isDark ? 'light' : 'dark';
    setIsDark(!isDark);
    document.documentElement.className = newTheme;
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
  };

  return (
    <aside
      className={clsx(
        `w-[70px] bg-surface border-r border-border shadow-md transition-all duration-300`
      )}
    >
      {/* LOGO */}
      <div className="flex items-center justify-center h-20 border-b border-border">
        <img src={siteConfig.logo} alt={siteConfig.name} className="h-10 w-10"/>
      </div>

      {/* 导航 */}
      <nav className={clsx(`flex w-[70px] flex-col ml-[4px] p-2 space-y-1`)}>
        {siteConfig.navItems.map((item, idx) => {
          isActive = location.pathname === item.href;
          return (
            <Tooltip delay={0} key={idx}>
              {item.href != "" ? (
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
