"use client";

import { useState, useEffect, useCallback, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { useAuthStore } from "@/stores/useAuthStore";
import { SavedSlipSummary } from "@/types/savedSlip";
import { apiGetUserSlips, apiToggleFavoriteSlip, apiDeleteSlip } from "@/lib/api";
import { NumberBall } from "@/components/slip/NumberBall";
import {
  Ticket,
  Heart,
  Trash2,
  ExternalLink,
  Sparkles,
  Calendar,
  Layers,
  ChevronLeft,
  ChevronRight,
  HelpCircle
} from "lucide-react";

function SavedSlipsContent() {
  const searchParams = useSearchParams();
  const initialTab = searchParams.get("tab") === "favorite" ? "favorite" : "all";

  const { isAuthenticated, isInitialized } = useAuthStore();

  const [activeTab, setActiveTab] = useState<"all" | "favorite">(initialTab);
  const [slips, setSlips] = useState<SavedSlipSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const loadSlips = useCallback(async (p: number, favOnly: boolean) => {
    if (!isAuthenticated) return;
    try {
      setLoading(true);
      const res = await apiGetUserSlips(p, 10, favOnly);
      setSlips(res.items);
      setTotalCount(res.totalCount);
      setPage(res.page);
      setTotalPages(res.totalPages || 1);
    } catch (err) {
      console.error("Lỗi khi tải danh sách vé:", err);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      loadSlips(page, activeTab === "favorite");
    }
  }, [isAuthenticated, page, activeTab, loadSlips]);

  const handleToggleFavorite = async (id: string) => {
    try {
      const res = await apiToggleFavoriteSlip(id);
      setSlips((prev) =>
        prev.map((s) => (s.id === id ? { ...s, isFavorite: res.isFavorite } : s))
      );
      if (activeTab === "favorite" && !res.isFavorite) {
        setSlips((prev) => prev.filter((s) => s.id !== id));
        setTotalCount((c) => Math.max(0, c - 1));
      }
    } catch (err) {
      console.error("Lỗi khi cập nhật yêu thích:", err);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Bạn có chắc chắn muốn xóa phiếu số này khỏi danh sách?")) return;
    try {
      setDeletingId(id);
      await apiDeleteSlip(id);
      setSlips((prev) => prev.filter((s) => s.id !== id));
      setTotalCount((c) => Math.max(0, c - 1));
    } catch (err) {
      console.error("Lỗi khi xóa vé:", err);
    } finally {
      setDeletingId(null);
    }
  };

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
        <div className="w-16 h-16 rounded-3xl bg-orange-500/10 border border-orange-500/20 text-orange-500 flex items-center justify-center mx-auto">
          <Ticket className="w-8 h-8" />
        </div>
        <h2 className="text-xl font-bold text-foreground">Bạn chưa đăng nhập</h2>
        <p className="text-xs text-muted-foreground">
          Đăng nhập ngay để xem và quản lý những lần nát (vé số đã lưu) của bạn!
        </p>
        <Link
          href="/login?redirect=/my/slips"
          className="inline-flex items-center gap-2 px-6 py-2.5 rounded-2xl bg-orange-600 hover:bg-orange-500 text-white text-xs font-bold shadow-lg shadow-orange-600/20 transition-all"
        >
          <span>Đăng nhập ngay</span>
        </Link>
      </div>
    );
  }

  return (
    <div className="w-full max-w-4xl mx-auto space-y-6 py-4 animate-in fade-in duration-200">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-black tracking-tight text-foreground flex items-center gap-2.5">
            <Ticket className="w-7 h-7 text-orange-500" />
            <span>Phiếu của tôi</span>
          </h1>
          <p className="text-xs text-muted-foreground">
            Lưu giữ những bộ số tâm linh và các pha đu đỉnh huyền thoại.
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

      {/* Tabs */}
      <div className="flex items-center gap-2 border-b border-border pb-2">
        <button
          type="button"
          onClick={() => {
            setActiveTab("all");
            setPage(1);
          }}
          className={`flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold transition-colors ${
            activeTab === "all"
              ? "bg-orange-500/10 text-orange-600 dark:text-orange-400 border border-orange-500/20"
              : "text-muted-foreground hover:text-foreground hover:bg-muted"
          }`}
        >
          <Layers className="w-3.5 h-3.5" />
          <span>Tất cả {activeTab === "all" ? `(${totalCount})` : ""}</span>
        </button>

        <button
          type="button"
          onClick={() => {
            setActiveTab("favorite");
            setPage(1);
          }}
          className={`flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold transition-colors ${
            activeTab === "favorite"
              ? "bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-500/20"
              : "text-muted-foreground hover:text-foreground hover:bg-muted"
          }`}
        >
          <Heart className="w-3.5 h-3.5 fill-current" />
          <span>Yêu thích {activeTab === "favorite" ? `(${totalCount})` : ""}</span>
        </button>
      </div>

      {/* Slips List */}
      {loading ? (
        <div className="text-center py-16 text-xs text-muted-foreground">
          Đang tải danh sách vé...
        </div>
      ) : slips.length === 0 ? (
        <div className="p-12 text-center rounded-3xl bg-card border border-border/80 space-y-4">
          <div className="w-14 h-14 rounded-2xl bg-muted flex items-center justify-center mx-auto text-muted-foreground">
            <HelpCircle className="w-7 h-7" />
          </div>
          <div className="space-y-1">
            <h3 className="text-base font-bold text-foreground">
              {activeTab === "favorite"
                ? "Chưa có lần nát nào được yêu thích."
                : "Bạn chưa giữ lại lần nát nào."}
            </h3>
            <p className="text-xs text-muted-foreground max-w-sm mx-auto">
              Hãy thử vận may bằng Thần Tài, Lucky Journey hoặc Tự xây bộ số ngay bây giờ!
            </p>
          </div>
          <Link
            href="/play/POWER_655"
            className="inline-flex items-center gap-1.5 px-5 py-2.5 rounded-2xl bg-orange-600 hover:bg-orange-500 text-white text-xs font-bold shadow-lg shadow-orange-600/20 transition-all"
          >
            <span>Tạo bộ số ngay</span>
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {slips.map((slip) => (
            <div
              key={slip.id}
              className="p-5 rounded-3xl bg-card border border-border/80 shadow-md hover:border-orange-500/40 transition-all space-y-4"
            >
              {/* Slip Card Header */}
              <div className="flex items-center justify-between flex-wrap gap-2">
                <div className="flex items-center gap-2.5">
                  <span className="text-xs font-extrabold px-2.5 py-1 rounded-xl bg-orange-500/10 text-orange-600 dark:text-orange-400 border border-orange-500/20">
                    {slip.gameName}
                  </span>
                  <span className="font-mono text-xs font-bold text-foreground">
                    {slip.slipCode}
                  </span>
                </div>

                <div className="flex items-center gap-2">
                  <span className="text-[11px] text-muted-foreground flex items-center gap-1">
                    <Calendar className="w-3 h-3" />
                    {new Date(slip.createdAt).toLocaleDateString("vi-VN", {
                      day: "2-digit",
                      month: "2-digit",
                      year: "numeric",
                      hour: "2-digit",
                      minute: "2-digit"
                    })}
                  </span>

                  {/* Favorite Toggle */}
                  <button
                    type="button"
                    onClick={() => handleToggleFavorite(slip.id)}
                    className={`p-1.5 rounded-xl border transition-colors ${
                      slip.isFavorite
                        ? "bg-rose-50 dark:bg-rose-950/40 border-rose-200 dark:border-rose-800 text-rose-500"
                        : "border-border text-muted-foreground hover:text-rose-500 hover:bg-muted"
                    }`}
                    title={slip.isFavorite ? "Bỏ yêu thích" : "Yêu thích"}
                  >
                    <Heart className={`w-4 h-4 ${slip.isFavorite ? "fill-current" : ""}`} />
                  </button>

                  {/* Delete Button */}
                  <button
                    type="button"
                    disabled={deletingId === slip.id}
                    onClick={() => handleDelete(slip.id)}
                    className="p-1.5 rounded-xl border border-border text-muted-foreground hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/40 transition-colors"
                    title="Xóa vé"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>

              {/* Lines Preview */}
              <div className="space-y-2 pt-1">
                {slip.lines.map((line) => (
                  <div
                    key={line.lineLabel}
                    className="flex items-center gap-2.5 flex-wrap p-2 rounded-2xl bg-muted/40 border border-border/40"
                  >
                    <span className="w-6 h-6 rounded-lg bg-zinc-200 dark:bg-zinc-800 font-bold text-xs flex items-center justify-center text-foreground">
                      {line.lineLabel}
                    </span>

                    {/* Balls */}
                    <div className="flex items-center gap-1.5 flex-wrap flex-1">
                      {line.numbers.map((num, idx) => (
                        <NumberBall
                          key={`${line.lineLabel}-${num.poolIndex}-${num.value}-${idx}`}
                          value={num.value}
                          poolIndex={num.poolIndex}
                          source={num.source}
                          size="sm"
                        />
                      ))}
                    </div>

                    {/* Derived Badge */}
                    <span
                      className={`text-[10px] font-bold px-2 py-0.5 rounded-md border ${
                        line.derivedMode === "Mixed"
                          ? "bg-purple-500/10 text-purple-600 dark:text-purple-400 border-purple-500/20"
                          : line.derivedMode === "Lucky"
                          ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20"
                          : line.derivedMode === "Random"
                          ? "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20"
                          : "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20"
                      }`}
                    >
                      {line.derivedMode === "Mixed"
                        ? "🧩 Mixed"
                        : line.derivedMode === "Lucky"
                        ? "🍀 Lucky"
                        : line.derivedMode === "Random"
                        ? "🎲 Thần Tài"
                        : "👤 Tự chọn"}
                    </span>
                  </div>
                ))}
              </div>

              {/* Card Footer */}
              <div className="pt-2 flex items-center justify-between border-t border-dashed border-border/60">
                <span className="text-[11px] text-muted-foreground font-medium">
                  {slip.completedLineCount} dòng hoàn chỉnh
                </span>

                <Link
                  href={`/my/slips/${slip.id}`}
                  className="inline-flex items-center gap-1 text-xs font-bold text-orange-600 dark:text-orange-400 hover:underline"
                >
                  <span>Xem chi tiết & Lucky Story</span>
                  <ExternalLink className="w-3 h-3" />
                </Link>
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

export default function SavedSlipsPage() {
  return (
    <Suspense fallback={<div className="text-center py-12 text-sm text-zinc-400">Đang tải...</div>}>
      <SavedSlipsContent />
    </Suspense>
  );
}
