"use client";

import * as React from "react";
import { useTheme } from "next-themes";
import { Moon, Sun, Laptop } from "lucide-react";

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = React.useState(false);

  React.useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) {
    return (
      <div className="w-9 h-9 rounded-lg bg-zinc-100 dark:bg-zinc-800 animate-pulse" />
    );
  }

  const cycleTheme = () => {
    if (theme === "light") setTheme("dark");
    else if (theme === "dark") setTheme("system");
    else setTheme("light");
  };

  return (
    <button
      onClick={cycleTheme}
      className="relative flex items-center justify-center w-9 h-9 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white/80 dark:bg-zinc-900/80 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors shadow-sm"
      title={`Theme hiện tại: ${theme} (Bấm để đổi)`}
      aria-label="Toggle theme"
    >
      {theme === "light" && <Sun className="w-4 h-4 text-amber-500 transition-transform rotate-0 scale-100" />}
      {theme === "dark" && <Moon className="w-4 h-4 text-rose-400 transition-transform rotate-0 scale-100" />}
      {theme === "system" && <Laptop className="w-4 h-4 text-zinc-400 transition-transform rotate-0 scale-100" />}
    </button>
  );
}
