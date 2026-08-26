"use client";

import { useState, useEffect, use } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuthStore } from "@/stores/useAuthStore";
import { SavedSlipDetail } from "@/types/savedSlip";
import { apiGetSlipDetail, apiToggleFavoriteSlip, apiDeleteSlip } from "@/lib/api";
import { NumberBall } from "@/components/slip/NumberBall";
import {
  Ticket,
  Heart,
  Trash2,
  ChevronLeft,
  Calendar,
  BookOpen,
  AlertCircle
} from "lucide-react";

interface SlipDetailPageProps {
  params: Promise<{ id: string }>;
}

export default function SlipDetailPage({ params }: SlipDetailPageProps) {
  const { id } = use(params);
  const router = useRouter();
  const { isAuthenticated, isInitialized } = useAuthStore();

  const [slip, setSlip] = useState<SavedSlipDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated && isInitialized) {
      router.push(`/login?redirect=/my/slips/${id}`);
      return;
    }

    if (isAuthenticated) {
      setLoading(true);
      apiGetSlipDetail(id)
        .then((data) => {
          setSlip(data);
          setError(null);
        })
        .catch((err: unknown) => {
          const msg = err instanceof Error ? err.message : "Không thể tải chi tiết vé.";
          setError(msg);
        })
        .finally(() => setLoading(false));
    }
  }, [id, isAuthenticated, isInitialized, router]);

  const handleToggleFavorite = async () => {
    if (!slip) return;
    try {
      const res = await apiToggleFavoriteSlip(slip.id);
      setSlip({ ...slip, isFavorite: res.isFavorite });
    } catch (err) {
      console.error("Lỗi cập nhật yêu thích:", err);
    }
  };

  const handleDelete = async () => {
    if (!slip) return;
    if (!confirm("Bạn có chắc chắn muốn xóa phiếu số này?")) return;
    try {
      await apiDeleteSlip(slip.id);
      router.push("/my/slips");
    } catch (err) {
      console.error("Lỗi khi xóa vé:", err);
    }
  };

  if (!isInitialized || loading) {
    return (
      <div className="text-center py-20 text-xs text-muted-foreground">
        Đang tải thông tin chi tiết vé...
      </div>
    );
  }

  if (error || !slip) {
    return (
      <div className="max-w-md mx-auto py-16 px-4 text-center space-y-4">
        <div className="w-14 h-14 rounded-2xl bg-red-500/10 text-red-500 flex items-center justify-center mx-auto">
          <AlertCircle className="w-7 h-7" />
        </div>
        <h2 className="text-lg font-bold text-foreground">Không tìm thấy vé số</h2>
        <p className="text-xs text-muted-foreground">{error || "Vé số không tồn tại hoặc bạn không có quyền xem."}</p>
        <Link
          href="/my/slips"
          className="inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-muted hover:bg-muted/80 text-foreground text-xs font-semibold"
        >
          <ChevronLeft className="w-4 h-4" />
          <span>Quay lại danh sách</span>
        </Link>
      </div>
    );
  }

  return (
    <div className="w-full max-w-3xl mx-auto space-y-6 py-4 animate-in fade-in duration-200">
      {/* Top Navigation */}
      <div className="flex items-center justify-between">
        <Link
          href="/my/slips"
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors"
        >
          <ChevronLeft className="w-4 h-4" />
          <span>Quay lại danh sách phiếu</span>
        </Link>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={handleToggleFavorite}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-xs font-bold transition-colors ${
              slip.isFavorite
                ? "bg-rose-50 dark:bg-rose-950/40 border-rose-200 dark:border-rose-800 text-rose-600 dark:text-rose-400"
                : "border-border text-muted-foreground hover:text-rose-500 hover:bg-muted"
            }`}
          >
            <Heart className={`w-3.5 h-3.5 ${slip.isFavorite ? "fill-current" : ""}`} />
            <span>{slip.isFavorite ? "Đã yêu thích" : "Yêu thích"}</span>
          </button>

          <button
            type="button"
            onClick={handleDelete}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-border text-xs font-semibold text-muted-foreground hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/40 transition-colors"
          >
            <Trash2 className="w-3.5 h-3.5" />
            <span>Xóa</span>
          </button>
        </div>
      </div>

      {/* Ticket Board Card */}
      <div className="relative rounded-3xl bg-card border border-border/80 shadow-xl overflow-hidden">
        <div className="h-3 bg-gradient-to-r from-rose-600 via-orange-500 to-amber-500"></div>

        {/* Ticket Header */}
        <div className="px-6 py-5 border-b border-dashed border-border/80 bg-muted/20 flex items-center justify-between flex-wrap gap-3">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-10 h-10 rounded-2xl bg-gradient-to-br from-rose-500 to-amber-500 text-white shadow-md shadow-rose-500/20">
              <Ticket className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h2 className="text-xl font-black text-foreground tracking-tight">
                  {slip.gameName}
                </h2>
                <span className="text-[10px] font-extrabold uppercase px-2 py-0.5 rounded-full bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-500/20">
                  {slip.gameCode}
                </span>
              </div>
              <p className="text-xs text-muted-foreground">{slip.title}</p>
            </div>
          </div>

          <div className="text-right flex flex-col items-end gap-0.5">
            <span className="font-mono font-black text-xs sm:text-sm text-foreground bg-muted px-2.5 py-1 rounded-lg border border-border">
              {slip.slipCode}
            </span>
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
          </div>
        </div>

        {/* Saved Lines */}
        <div className="p-6 space-y-3">
          {slip.lines.map((line) => (
            <div
              key={line.lineLabel}
              className="flex items-center justify-between gap-3 p-3 rounded-2xl bg-muted/30 border border-border/60 flex-wrap"
            >
              <div className="flex items-center gap-3 flex-wrap">
                <span className="w-8 h-8 rounded-xl bg-zinc-200 dark:bg-zinc-800 font-extrabold text-sm flex items-center justify-center text-foreground shadow-sm">
                  {line.lineLabel}
                </span>

                <div className="flex items-center gap-2 flex-wrap">
                  {line.numbers.map((num, idx) => (
                    <NumberBall
                      key={`${line.lineLabel}-${num.poolIndex}-${num.value}-${idx}`}
                      value={num.value}
                      poolIndex={num.poolIndex}
                      source={num.source}
                      size="md"
                    />
                  ))}
                </div>
              </div>

              {/* Mode Badge */}
              <span
                className={`text-[11px] font-bold px-2.5 py-1 rounded-lg border ${
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
                  ? "🧩 Mixed (Tự xây)"
                  : line.derivedMode === "Lucky"
                  ? "🍀 Lucky Journey"
                  : line.derivedMode === "Random"
                  ? "🎲 Thần Tài"
                  : "👤 Tự chọn"}
              </span>
            </div>
          ))}
        </div>
      </div>

      {/* Lucky Stories Section */}
      <div className="p-6 rounded-3xl bg-card border border-border/80 shadow-md space-y-4">
        <div className="flex items-center gap-2">
          <BookOpen className="w-5 h-5 text-emerald-500" />
          <h3 className="text-base font-extrabold text-foreground">
            Câu chuyện của các con số (Lucky Story)
          </h3>
        </div>

        {slip.luckyStories.length === 0 ? (
          <div className="p-6 text-center rounded-2xl bg-muted/40 text-xs text-muted-foreground space-y-1">
            <p className="font-semibold text-foreground">
              Chưa có câu chuyện Lucky Journey nào trong phiếu này.
            </p>
            <p>
              Các con số được tạo bằng chế độ Tự chọn hoặc Thần Tài ngẫu nhiên.
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3.5 pt-1">
            {slip.luckyStories.map((story, index) => (
              <div
                key={`${story.lineLabel}-${story.numberValue}-${index}`}
                className="p-4 rounded-2xl bg-gradient-to-br from-emerald-500/5 to-teal-500/5 border border-emerald-500/20 space-y-2.5"
              >
                {/* Header of Story Card */}
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="w-7 h-7 rounded-full bg-emerald-600 text-white font-mono font-bold text-xs flex items-center justify-center shadow-sm">
                      {story.formatted}
                    </span>
                    <span className="text-xs font-bold text-foreground">
                      Dòng {story.lineLabel}
                    </span>
                  </div>

                  <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20">
                    {story.themeName || "Tâm linh"}
                  </span>
                </div>

                {/* Question & Answer */}
                <div className="space-y-1 text-xs">
                  <p className="text-muted-foreground font-medium">
                    ❓ {story.questionText}
                  </p>
                  <p className="text-foreground font-bold">
                    👉 Lựa chọn: &quot;{story.choiceText}&quot;
                  </p>
                </div>

                {/* Explanation */}
                {story.explanation && (
                  <p className="text-[11px] text-emerald-700 dark:text-emerald-300 italic bg-emerald-50 dark:bg-emerald-950/30 p-2 rounded-xl border border-emerald-200 dark:border-emerald-800/40">
                    💬 &quot;{story.explanation}&quot;
                  </p>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
