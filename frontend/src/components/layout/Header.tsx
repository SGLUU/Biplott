"use client";

import Link from "next/link";
import { ThemeToggle } from "./ThemeToggle";
import { Sparkles, Dices } from "lucide-react";

export function Header() {
  return (
    <header className="sticky top-0 z-50 w-full border-b border-zinc-200/80 dark:border-zinc-800/80 bg-white/75 dark:bg-zinc-950/75 backdrop-blur-md transition-colors">
      <div className="max-w-6xl mx-auto px-4 h-16 flex items-center justify-between">
        {/* Brand Logo */}
        <Link href="/" className="flex items-center gap-3 group">
          <div className="relative w-10 h-10 rounded-2xl bg-gradient-to-tr from-rose-600 via-orange-500 to-amber-400 p-[2px] shadow-md shadow-orange-500/20 group-hover:scale-105 transition-transform">
            <div className="w-full h-full bg-zinc-950 rounded-[14px] flex items-center justify-center">
              <Dices className="w-5 h-5 text-amber-400 group-hover:rotate-12 transition-transform duration-300" />
            </div>
            <div className="absolute -top-1 -right-1 w-3 h-3 bg-amber-400 rounded-full border-2 border-zinc-950 animate-pulse" />
          </div>
          <div className="flex flex-col">
            <div className="flex items-center gap-1.5">
              <span className="font-extrabold text-xl tracking-tight bg-gradient-to-r from-rose-600 via-orange-500 to-amber-500 bg-clip-text text-transparent">
                Bịp lót
              </span>
              <span className="text-[10px] uppercase font-bold tracking-widest px-1.5 py-0.5 rounded bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-400 border border-rose-200 dark:border-rose-800/50">
                v1.0
              </span>
            </div>
            <span className="text-[11px] font-medium text-zinc-500 dark:text-zinc-400 -mt-0.5">
              Cơ hội để nát hơn
            </span>
          </div>
        </Link>

        {/* Right Actions */}
        <div className="flex items-center gap-2.5">
          <div className="hidden sm:flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300 border border-amber-200/60 dark:border-amber-800/40">
            <Sparkles className="w-3.5 h-3.5 text-amber-500" />
            <span>Tâm linh vui vẻ</span>
          </div>
          <ThemeToggle />
        </div>
      </div>
    </header>
  );
}
