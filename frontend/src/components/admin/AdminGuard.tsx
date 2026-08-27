"use client";

import React, { useEffect, useState } from "react";
import { useAuthStore } from "@/stores/useAuthStore";
import { useRouter, usePathname } from "next/navigation";
import { ShieldAlert, Loader2 } from "lucide-react";
import Link from "next/link";

interface AdminGuardProps {
  children: React.ReactNode;
}

export function AdminGuard({ children }: AdminGuardProps) {
  const { user, isAuthenticated, isLoading } = useAuthStore();
  const router = useRouter();
  const pathname = usePathname();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted || isLoading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
        <p className="text-sm text-zinc-400">Đang xác thực quyền Quản trị viên...</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    if (typeof window !== "undefined") {
      router.replace(`/login?returnUrl=${encodeURIComponent(pathname)}`);
    }
    return null;
  }

  const isAdmin = user?.roles?.includes("Admin");

  if (!isAdmin) {
    return (
      <div className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center p-6 text-center">
        <div className="mb-4 rounded-full bg-red-500/10 p-4 text-red-500 ring-1 ring-red-500/20">
          <ShieldAlert className="h-12 w-12" />
        </div>
        <h1 className="text-2xl font-bold text-zinc-100">403 - Quyền truy cập bị từ chối</h1>
        <p className="mt-2 text-sm text-zinc-400">
          Tài khoản của bạn (<strong className="text-zinc-200">{user?.email}</strong>) không có quyền Quản trị viên để truy cập khu vực này.
        </p>
        <div className="mt-6 flex gap-3">
          <Link
            href="/"
            className="rounded-lg bg-zinc-800 px-4 py-2 text-sm font-medium text-zinc-200 hover:bg-zinc-700 transition"
          >
            Quay về Trang chủ
          </Link>
          <Link
            href="/my/slips"
            className="rounded-lg bg-amber-500/20 px-4 py-2 text-sm font-medium text-amber-300 hover:bg-amber-500/30 transition"
          >
            Vé đã lưu của tôi
          </Link>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}