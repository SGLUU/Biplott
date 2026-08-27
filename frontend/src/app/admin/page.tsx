"use client";

import React, { useEffect, useState } from "react";
import { AdminDashboardMetrics } from "@/types/admin";
import { getAdminDashboard } from "@/lib/adminApi";
import { StatusBadge } from "@/components/admin/StatusBadge";
import Link from "next/link";
import {
  Users,
  Bookmark,
  HelpCircle,
  Sparkles,
  ArrowUpRight,
  Loader2,
  RefreshCw,
  Plus
} from "lucide-react";

export default function AdminDashboardPage() {
  const [metrics, setMetrics] = useState<AdminDashboardMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAdminDashboard();
      setMetrics(data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Không thể tải dữ liệu Dashboard");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  if (loading && !metrics) {
    return (
      <div className="flex min-h-[50vh] flex-col items-center justify-center gap-3">
        <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
        <p className="text-sm text-zinc-400">Đang tải chỉ số hệ thống...</p>
      </div>
    );
  }

  if (error && !metrics) {
    return (
      <div className="rounded-2xl border border-red-500/20 bg-red-500/10 p-6 text-center">
        <p className="text-sm text-red-400 font-medium">{error}</p>
        <button
          onClick={loadData}
          className="mt-4 inline-flex items-center gap-2 rounded-lg bg-zinc-800 px-4 py-2 text-xs font-bold text-zinc-200 hover:bg-zinc-700"
        >
          <RefreshCw className="h-3.5 w-3.5" /> Thử lại
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 className="text-2xl md:text-3xl font-extrabold text-zinc-100 tracking-tight">
            Tổng quan Hệ thống
          </h1>
          <p className="mt-1 text-sm text-zinc-400">
            Giám sát kho nội dung, số lượng vé đã lưu và các chỉ số hoạt động.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <button
            onClick={loadData}
            disabled={loading}
            className="inline-flex items-center gap-2 rounded-xl border border-zinc-800 bg-zinc-900 px-3.5 py-2 text-xs font-medium text-zinc-300 hover:bg-zinc-800 transition disabled:opacity-50"
          >
            <RefreshCw className={`h-3.5 w-3.5 ${loading ? "animate-spin" : ""}`} />
            Làm mới
          </button>
          <Link
            href="/admin/questions"
            className="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-4 py-2 text-xs font-bold text-zinc-950 hover:bg-amber-400 transition shadow-lg shadow-amber-500/10"
          >
            <Plus className="h-3.5 w-3.5" /> Thêm câu hỏi
          </Link>
        </div>
      </div>

      {/* Metric Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Total Users */}
        <div className="rounded-2xl border border-zinc-800/80 bg-zinc-900/50 p-5 backdrop-blur">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-zinc-400">Người dùng đăng ký</span>
            <div className="rounded-xl bg-blue-500/10 p-2 text-blue-400">
              <Users className="h-5 w-5" />
            </div>
          </div>
          <div className="mt-4 text-3xl font-extrabold text-zinc-100">{metrics?.totalUsers || 0}</div>
          <div className="mt-2 flex items-center text-xs text-zinc-400">
            <Link href="/admin/users" className="text-blue-400 hover:underline flex items-center gap-0.5">
              Xem quản lý <ArrowUpRight className="h-3 w-3" />
            </Link>
          </div>
        </div>

        {/* Total Saved Slips */}
        <div className="rounded-2xl border border-zinc-800/80 bg-zinc-900/50 p-5 backdrop-blur">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-zinc-400">Vé đã lưu trong DB</span>
            <div className="rounded-xl bg-emerald-500/10 p-2 text-emerald-400">
              <Bookmark className="h-5 w-5" />
            </div>
          </div>
          <div className="mt-4 text-3xl font-extrabold text-zinc-100">{metrics?.totalSavedSlips || 0}</div>
          <div className="mt-2 text-xs text-zinc-400">Vé số người dùng lưu</div>
        </div>

        {/* Questions Total */}
        <div className="rounded-2xl border border-zinc-800/80 bg-zinc-900/50 p-5 backdrop-blur">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-zinc-400">Ngân hàng Câu hỏi</span>
            <div className="rounded-xl bg-amber-500/10 p-2 text-amber-400">
              <HelpCircle className="h-5 w-5" />
            </div>
          </div>
          <div className="mt-4 text-3xl font-extrabold text-zinc-100">{metrics?.totalQuestions || 0}</div>
          <div className="mt-2 flex items-center gap-2 text-xs">
            <span className="text-emerald-400">{metrics?.activeQuestions || 0} hoạt động</span>
            <span className="text-zinc-600">•</span>
            <span className="text-zinc-400">{metrics?.inactiveQuestions || 0} tạm dừng</span>
          </div>
        </div>

        {/* Total Themes & Traits */}
        <div className="rounded-2xl border border-zinc-800/80 bg-zinc-900/50 p-5 backdrop-blur">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-zinc-400">Chủ đề & Thuộc tính</span>
            <div className="rounded-xl bg-purple-500/10 p-2 text-purple-400">
              <Sparkles className="h-5 w-5" />
            </div>
          </div>
          <div className="mt-4 flex items-baseline gap-2">
            <span className="text-3xl font-extrabold text-zinc-100">{metrics?.totalThemes || 0}</span>
            <span className="text-xs text-zinc-400">chủ đề</span>
            <span className="text-zinc-600">/</span>
            <span className="text-xl font-bold text-zinc-300">{metrics?.totalTraits || 0}</span>
            <span className="text-xs text-zinc-400">traits</span>
          </div>
          <div className="mt-2 text-xs text-zinc-400">{metrics?.totalChoices || 0} lựa chọn câu trả lời</div>
        </div>
      </div>

      {/* Two Columns: Recent Questions & Recent Users */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Questions */}
        <div className="rounded-2xl border border-zinc-800/80 bg-zinc-900/40 p-6 backdrop-blur">
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-bold text-zinc-100 flex items-center gap-2 text-base">
              <HelpCircle className="h-4 w-4 text-amber-400" /> Câu hỏi cập nhật gần đây
            </h3>
            <Link href="/admin/questions" className="text-xs font-semibold text-amber-400 hover:underline">
              Xem tất cả
            </Link>
          </div>

          <div className="space-y-3">
            {metrics?.recentQuestions && metrics.recentQuestions.length > 0 ? (
              metrics.recentQuestions.map((q) => (
                <div
                  key={q.id}
                  className="rounded-xl border border-zinc-800 bg-zinc-950/50 p-3.5 hover:border-zinc-700 transition"
                >
                  <div className="flex items-center justify-between gap-2 mb-1.5">
                    <span className="rounded-full bg-amber-500/10 px-2 py-0.5 text-[10px] font-semibold text-amber-400 border border-amber-500/20">
                      {q.themeName}
                    </span>
                    <StatusBadge isActive={q.isActive} size="sm" />
                  </div>
                  <p className="text-xs font-semibold text-zinc-200 line-clamp-1">{q.content}</p>
                  <div className="mt-2 flex items-center justify-between text-[11px] text-zinc-500">
                    <span>{q.choicesCount} lựa chọn ({q.activeChoicesCount} hoạt động)</span>
                    <span>Lượt xem: {q.viewCount}</span>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-xs text-zinc-500 py-4 text-center">Chưa có câu hỏi nào trong hệ thống.</p>
            )}
          </div>
        </div>

        {/* Recent Users */}
        <div className="rounded-2xl border border-zinc-800/80 bg-zinc-900/40 p-6 backdrop-blur">
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-bold text-zinc-100 flex items-center gap-2 text-base">
              <Users className="h-4 w-4 text-blue-400" /> Người dùng mới nhất
            </h3>
            <Link href="/admin/users" className="text-xs font-semibold text-blue-400 hover:underline">
              Xem tất cả
            </Link>
          </div>

          <div className="space-y-3">
            {metrics?.recentUsers && metrics.recentUsers.length > 0 ? (
              metrics.recentUsers.map((u) => (
                <div
                  key={u.id}
                  className="flex items-center justify-between rounded-xl border border-zinc-800 bg-zinc-950/50 p-3.5 hover:border-zinc-700 transition"
                >
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-bold text-xs text-zinc-200">{u.displayName}</span>
                      {u.roles.includes("Admin") && (
                        <span className="rounded bg-amber-500/20 px-1.5 py-0.2 text-[9px] font-extrabold text-amber-300">
                          ADMIN
                        </span>
                      )}
                    </div>
                    <p className="text-[11px] text-zinc-400">{u.email}</p>
                  </div>
                  <div className="text-right">
                    <StatusBadge isActive={u.isActive} size="sm" />
                    <p className="mt-1 text-[10px] text-zinc-500">{u.savedSlipsCount} vé đã lưu</p>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-xs text-zinc-500 py-4 text-center">Chưa có người dùng đăng ký.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}