"use client";

import { useState, useEffect, useCallback, Suspense } from "react";
import Link from "next/link";
import { useAuthStore } from "@/stores/useAuthStore";
import { UserActivityItem } from "@/types/savedSlip";
import { apiGetUserHistory } from "@/lib/api";
import {
  History,
  Sparkles,
  Ticket,
  Calendar,
  Layers,
  ChevronLeft,
  ChevronRight,
  Clock,
  Dices,
  HelpCircle
} from "lucide-react";

function UserHistoryContent() {
  const { isAuthenticated, isInitialized } = useAuthStore();

  const [activities, setActivities] = useState<UserActivityItem[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);

  const loadHistory = useCallback(async (p: number) => {
    if (!isAuthenticated) return;
    try {
      setLoading(true);
      const res = await apiGetUserHistory(p, 20);
      setActivities(res.items);
      setPage(res.page);
      setTotalPages(res.totalPages || 1);
    } catch (err) {
      console.error("Lỗi tải lịch sử hoạt động:", err);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      loadHistory(page);
    }
  }, [isAuthenticated, page, loadHistory]);

  if (!isInitialized) {
    return (
      <div className="flex items-center justify-center py-20 text-muted-foreground text-sm">
        Đang kiểm tra thông tin tài khoản...
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="max-w-md mx-auto py-16 px-4 text-center space-y-4">
        <div className="w-16 h-16 rounded-3xl bg-rose-500/10 border border-rose-500/20 text-rose-500 flex items-center justify-center mx-auto">
          <History className="w-8 h-8" />
        </div>
        <h2 className="text-xl font-bold text-foreground">Bạn chưa đăng nhập</h2>
        <p className="text-xs text-muted-foreground">
          Đăng nhập để xem dòng thời gian các hoạt động tạo bộ số của bạn!
        </p>
        <Link
          href="/login?redirect=/my/history"
          className="inline-flex items-center gap-2 px-6 py-2.5 rounded-2xl bg-orange-600 hover:bg-orange-500 text-white text-xs font-bold shadow-lg shadow-orange-600/20 transition-all"
        >
          <span>Đăng nhập ngay</span>
        </Link>
      </div>
    );
  }

  const getActivityIcon = (type: string) => {
    switch (type) {
      case "SavedSlip":
        return <Ticket className="w-4 h-4 text-orange-500" />;
      case "CompletedLuckyJourney":
        return <Sparkles className="w-4 h-4 text-emerald-500" />;
      case "GeneratedRandomLine":
        return <Dices className="w-4 h-4 text-amber-500" />;
      case "CompletedMixedLine":
        return <Layers className="w-4 h-4 text-purple-500" />;
      default:
        return <Clock className="w-4 h-4 text-zinc-400" />;
    }
  };

  return (
    <div className="w-full max-w-3xl mx-auto space-y-6 py-4 animate-in fade-in duration-200">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-black tracking-tight text-foreground flex items-center gap-2.5">
            <History className="w-7 h-7 text-rose-500" />
            <span>Lịch sử hoạt động</span>
          </h1>
          <p className="text-xs text-muted-foreground">
            Dòng thời gian các lần tạo số và lưu phiếu của bạn trên Bịp lót.
          </p>
        </div>

        <Link
          href="/play/POWER_655"
          className="inline-flex items-center gap-1.5 px-4 py-2 rounded-2xl bg-gradient-to-r from-orange-600 to-amber-500 hover:from-orange-500 hover:to-amber-400 text-white text-xs font-bold shadow-md shadow-orange-500/20 active:scale-95 transition-all"
        >
          <Sparkles className="w-3.5 h-3.5" />
          <span>Tạo phiếu mới</span>
        </Link>
      </div>

      {/* Activity Timeline */}
      {loading ? (
        <div className="text-center py-16 text-xs text-muted-foreground">
          Đang tải dòng thời gian...
        </div>
      ) : activities.length === 0 ? (
        <div className="p-12 text-center rounded-3xl bg-card border border-border/80 space-y-4">
          <div className="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center mx-auto text-muted-foreground">
            <HelpCircle className="w-7 h-7" />
          </div>
          <div className="space-y-1">
            <h3 className="text-base font-bold text-foreground">
              Chưa có hoạt động nào được ghi nhận.
            </h3>
            <p className="text-xs text-muted-foreground max-w-sm mx-auto">
              Khi bạn tạo dòng số hoặc lưu vé, các mốc đáng nhớ sẽ xuất hiện tại đây.
            </p>
          </div>
          <Link
            href="/play/POWER_655"
            className="inline-flex items-center gap-1.5 px-5 py-2.5 rounded-2xl bg-orange-600 hover:bg-orange-500 text-white text-xs font-bold shadow-lg shadow-orange-600/20 transition-all"
          >
            <span>Bắt đầu chơi ngay</span>
          </Link>
        </div>
      ) : (
        <div className="relative pl-6 border-l-2 border-border space-y-6">
          {activities.map((act) => (
            <div key={act.id} className="relative space-y-1.5 group">
              {/* Dot Icon on Timeline */}
              <div className="absolute -left-[33px] top-1 w-6 h-6 rounded-full bg-card border-2 border-border flex items-center justify-center shadow-sm group-hover:scale-110 group-hover:border-orange-500 transition-all">
                {getActivityIcon(act.activityType)}
              </div>

              {/* Activity Card */}
              <div className="p-4 rounded-2xl bg-card border border-border shadow-sm group-hover:border-orange-500/30 transition-all space-y-1.5">
                <div className="flex items-center justify-between flex-wrap gap-2">
                  <div className="flex items-center gap-2">
                    <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-orange-500/10 text-orange-600 dark:text-orange-400 border border-orange-500/20">
                      {act.gameName}
                    </span>
                    <h4 className="text-xs font-bold text-foreground">
                      {act.title}
                    </h4>
                  </div>

                  <span className="text-[11px] text-muted-foreground flex items-center gap-1">
                    <Calendar className="w-3 h-3" />
                    {new Date(act.createdAt).toLocaleDateString("vi-VN", {
                      day: "2-digit",
                      month: "2-digit",
                      year: "numeric",
                      hour: "2-digit",
                      minute: "2-digit"
                    })}
                  </span>
                </div>

                <p className="text-xs text-muted-foreground">
                  {act.summary}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 pt-4">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            className="flex items-center gap-1 px-3 py-1.5 rounded-xl border border-border text-xs font-semibold disabled:opacity-40"
          >
            <ChevronLeft className="w-3.5 h-3.5" />
            <span>Trang trước</span>
          </button>
          <span className="text-xs text-muted-foreground px-2">
            Trang {page} / {totalPages}
          </span>
          <button
            type="button"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            className="flex items-center gap-1 px-3 py-1.5 rounded-xl border border-border text-xs font-semibold disabled:opacity-40"
          >
            <span>Trang sau</span>
            <ChevronRight className="w-3.5 h-3.5" />
          </button>
        </div>
      )}
    </div>
  );
}

export default function UserHistoryPage() {
  return (
    <Suspense fallback={<div className="text-center py-12 text-sm text-zinc-400">Đang tải...</div>}>
      <UserHistoryContent />
    </Suspense>
  );
}
