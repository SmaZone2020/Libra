import { AnimatePresence, motion } from "framer-motion";
import { useLocation } from "react-router-dom";
import NavBar from "../components/Navbar";

interface DefaultLayoutProps {
  children: React.ReactNode;
  showHeaderFooter?: boolean;
  className?: string;
}

export default function DefaultLayout({
  children,
  showHeaderFooter = false,
  className = "",
}: DefaultLayoutProps) {
  const location = useLocation();

  return (
    <div
      className={`relative flex flex-col bg-[url('/bg.jpg')] bg-cover bg-center h-screen overflow-y-auto ${className}`}
      id="app-container"
    >
      <div className="bg-surface/60 backdrop-blur-md dark:bg-surface/40 bg-blur-10 flex flex-row w-full h-full overflow-hidden">
        <NavBar />
        <main
          className={`mt-[10px] flex-grow relative overflow-hidden ${
            showHeaderFooter ? "pt-16" : ""
          }`}
          id="main-content"
        >
          <AnimatePresence mode="wait">
            <motion.div
              key={location.pathname}
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              transition={{ duration: 0.25 }}
              className="h-full"
            >
              {children}
            </motion.div>
          </AnimatePresence>
        </main>
      </div>
    </div>
  );
}