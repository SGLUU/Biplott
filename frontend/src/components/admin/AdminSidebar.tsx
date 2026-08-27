"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  Palette,
  HelpCircle,
  Sparkles,
  UploadCloud,
  Sliders,
  Users,
  ChevronLeft,
  ChevronRight,
  Flame,
  ArrowLeft
} from "lucide-react";

const navItems = [
  { name: "Tổng quan", href: "/admin", icon: LayoutDashboard },
  { name: "Chủ đề câu hỏi", href: "/admin/themes", icon: Palette },
  { name: "Ngân hàng Câu hỏi", href: "/admin/questions", icon: HelpCircle },
  { name: "Thuộc tính (Traits)", href: "/admin/traits", icon: Sparkles },
  { name: "Nhập dữ liệu (Bulk)", href: "/admin/import", icon: UploadCloud },
  { name: "Cấu hình Thuật toán", href: "/admin/settings", icon: Sliders },
  { name: "Người dùng", href: "/admin/users", icon: Users }
];

export function AdminSidebar() {
  const pathname = usePathname();
  const [collapsed, setCollapsed] = React.useState(false);

  return (
    <aside
      className={`sticky top-0 h-screen border-r border-zinc-800 bg-zinc-950/90 backdrop-blur transition-all duration-300 flex flex-col justify-between z-30 ${
        collapsed ? "w-18" : "w-64"
      }`}
    >
      <div>
        {/* Brand */}
        <div className="flex h-16 items-center justify-between px-4 border-b border-zinc-800/80">
          <Link href="/admin" className="flex items-center gap-2.5 overflow-hidden">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-amber-500 to-red-600 shadow-md shadow-amber-500/20">
              <Flame className="h-5 w-5 text-zinc-950" />
            </div>
            {!collapsed && (
              <div className="flex flex-col">
                <span className="font-extrabold tracking-tight text-zinc-100 text-sm">BỊP LÓT</span>
                <span className="text-[10px] font-bold text-amber-400 uppercase tracking-wider">Admin Portal</span>
              </div>
            )}
          </Link>

          <button
            onClick={() => setCollapsed(!collapsed)}
            className="rounded-lg p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200 transition"
            title={collapsed ? "Mở rộng" : "Thu gọn"}
          >
            {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
          </button>
        </div>

        {/* Navigation */}
        <nav className="p-3 space-y-1.5">
          {navItems.map((item) => {
            const isActive =
              item.href === "/admin"
                ? pathname === "/admin"
                : pathname.startsWith(item.href);

            const Icon = item.icon;

            return (
              <Link
                key={item.href}
                href={item.href}
                className={`flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all ${
                  isActive
                    ? "bg-amber-500/15 text-amber-400 shadow-sm border border-amber-500/20"
                    : "text-zinc-400 hover:bg-zinc-900 hover:text-zinc-100"
                }`}
                title={collapsed ? item.name : undefined}
              >
                <Icon className={`h-5 w-5 shrink-0 ${isActive ? "text-amber-400" : "text-zinc-400"}`} />
                {!collapsed && <span>{item.name}</span>}
              </Link>
            );
          })}
        </nav>
      </div>

      {/* Bottom return link */}
      <div className="p-3 border-t border-zinc-800/80">
        <Link
          href="/"
          className="flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-zinc-400 hover:bg-zinc-900 hover:text-zinc-100 transition"
          title={collapsed ? "Về trang chơi" : undefined}
        >
          <ArrowLeft className="h-5 w-5 shrink-0 text-zinc-400" />
          {!collapsed && <span>Về Bịp lót Portal</span>}
        </Link>
      </div>
    </aside>
  );
}