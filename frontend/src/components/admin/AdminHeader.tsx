"use client";

import React from "react";
import { useAuthStore } from "@/stores/useAuthStore";
import { ShieldCheck, User as UserIcon } from "lucide-react";

export function AdminHeader() {
  const { user } = useAuthStore();

  return (
    <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-zinc-800/80 bg-zinc-950/80 px-6 backdrop-blur">
      <div className="flex items-center gap-3">
        <div className="flex items-center gap-2 rounded-full bg-amber-500/10 px-3 py-1 text-xs font-semibold text-amber-400 border border-amber-500/20">
          <ShieldCheck className="h-3.5 w-3.5" />
          HỆ THỐNG QUẢN TRỊ NỘI DUNG & THUẬT TOÁN
        </div>
      </div>

      <div className="flex items-center gap-4">
        <div className="flex items-center gap-3 rounded-full bg-zinc-900 px-3.5 py-1.5 border border-zinc-800">
          <div className="flex h-7 w-7 items-center justify-center rounded-full bg-amber-500/20 text-amber-400">
            <UserIcon className="h-4 w-4" />
          </div>
          <div className="flex flex-col text-left">
            <span className="text-xs font-bold text-zinc-200">{user?.displayName || "Admin"}</span>
            <span className="text-[10px] text-zinc-400">{user?.email}</span>
          </div>
        </div>
      </div>
    </header>
  );
}