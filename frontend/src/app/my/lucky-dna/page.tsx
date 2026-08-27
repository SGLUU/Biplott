"use client";

import React, { useState, useEffect, useCallback } from "react";
import Link from "next/link";
import { useAuthStore } from "@/stores/useAuthStore";
import { apiGetLuckyDna, apiResetLuckyDna } from "@/lib/api";
import { getOrCreateGuestSessionToken } from "@/lib/utils";
import {
  Dna,
  Sparkles,
  RefreshCw,
  AlertCircle,
  UserPlus,
  ChevronRight,
  HelpCircle,
  Info
} from "lucide-react";

import { LuckyDna } from "@/types/lucky";

export default function LuckyDnaPage() {
  const { isAuthenticated, isInitialized } = useAuthStore();
  const [dna, setDna] = useState<LuckyDna | null>(null);
  const [loading, setLoading] = useState(true);
  const [resetting, setResetting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadDna = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      let data;
      if (isAuthenticated) {
        data = await apiGetLuckyDna();
      } else {
        const guestToken = getOrCreateGuestSessionToken();
        data = await apiGetLuckyDna(guestToken);
      }
      setDna(data);
    } catch (err: unknown) {
      console.error("Lỗi khi tải Lucky DNA:", err);
      const msg = err instanceof Error ? err.message : "Không thể tải thông tin Lucky DNA.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isInitialized) {
      loadDna();
    }
  }, [isInitialized, loadDna]);

  const handleResetDna = async () => {
    if (!window.confirm("Bạn có chắc chắn muốn xóa hồ sơ Lucky DNA hiện tại? Lịch sử vé và tài khoản của bạn sẽ không bị ảnh hưởng, nhưng chân dung tính cách sẽ được tính toán lại từ đầu.")) {
      return;
    }

    try {
      setResetting(true);
      await apiResetLuckyDna();
      await loadDna();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Lỗi khi reset Lucky DNA.";
      alert(msg);
    } finally {
      setResetting(false);
    }
  };

  if (!isInitialized || loading) {
    return (
      <div className="flex flex-col items-center justify-center py-24 space-y-4 text-center">
        <RefreshCw className="w-8 h-8 text-rose-500 animate-spin" />
        <p className="text-sm font-semibold text-muted-foreground">
          Đang phân tích dữ liệu tính cách của bạn...
        </p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-md mx-auto py-16 px-4 text-center space-y-4">
        <AlertCircle className="w-12 h-12 text-destructive mx-auto" />
        <h2 className="text-lg font-bold text-foreground">Không thể tải Lucky DNA</h2>
        <p className="text-xs text-muted-foreground">{error}</p>
        <button
          onClick={loadDna}
          className="px-4 py-2 rounded-xl bg-muted border hover:bg-muted/80 text-xs font-bold transition-all"
        >
          Thử lại
        </button>
      </div>
    );
  }

  const isNotFormed = !dna || dna.status === "NotFormed" || dna.totalAnswers === 0;
  const isForming = dna?.status === "Forming";

  return (
    <div className="w-full max-w-2xl mx-auto space-y-6 py-6 px-4 animate-in fade-in duration-200">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-black tracking-tight text-foreground flex items-center gap-2.5">
            <Dna className="w-7 h-7 text-rose-500 animate-pulse" />
            <span>Lucky DNA</span>
          </h1>
          <p className="text-xs text-muted-foreground mt-0.5">
            Chân dung vui về phong cách sinh số được tổng hợp từ những lựa chọn của bạn.
          </p>
        </div>

        {isAuthenticated && !isNotFormed && (
          <button
            onClick={handleResetDna}
            disabled={resetting}
            className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-border hover:bg-muted text-muted-foreground hover:text-foreground text-xs font-bold transition-all disabled:opacity-50"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${resetting ? "animate-spin" : ""}`} />
            <span>Reset DNA</span>
          </button>
        )}
      </div>

      {/* Guest CTA Sign up Banner */}
      {!isAuthenticated && (
        <div className="p-4 rounded-2xl bg-gradient-to-r from-orange-600/10 to-amber-500/10 border border-orange-500/20 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div className="space-y-1">
            <h4 className="text-sm font-bold text-foreground flex items-center gap-1.5">
              <Sparkles className="w-4 h-4 text-orange-500" />
              <span>DNA tạm thời của Khách</span>
            </h4>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Bạn đang chơi dưới tư cách Khách. Hãy đăng nhập để lưu trữ vĩnh viễn và đồng bộ DNA của bạn.
            </p>
          </div>
          <Link
            href="/register?redirect=/my/lucky-dna"
            className="inline-flex items-center gap-1 px-4 py-2 rounded-xl bg-orange-600 hover:bg-orange-500 text-white text-xs font-bold transition-all shadow-md shadow-orange-600/10 shrink-0"
          >
            <UserPlus className="w-3.5 h-3.5" />
            <span>Lưu DNA của tôi</span>
          </Link>
        </div>
      )}

      {/* Case 1: Not Formed (0 answers) */}
      {isNotFormed && (
        <div className="p-8 text-center rounded-3xl bg-card border border-border/80 space-y-6">
          <div className="w-16 h-16 rounded-2xl bg-muted flex items-center justify-center mx-auto text-muted-foreground">
            <HelpCircle className="w-8 h-8" />
          </div>
          <div className="space-y-2 max-w-sm mx-auto">
            <h3 className="text-lg font-bold text-foreground">
              DNA của bạn chưa hình thành
            </h3>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Bịp lót chưa có đủ dữ liệu hành vi của bạn. Hãy chơi ít nhất một lượt Lucky Journey để bắt đầu phân tích phong cách tâm linh.
            </p>
          </div>
          <Link
            href="/play/POWER_655"
            className="inline-flex items-center gap-1.5 px-6 py-3 rounded-2xl bg-gradient-to-r from-orange-600 to-amber-500 hover:from-orange-500 hover:to-amber-400 text-white text-xs font-bold shadow-lg shadow-orange-500/20 active:scale-95 transition-all"
          >
            <span>Chơi Lucky Journey ngay</span>
            <ChevronRight className="w-4 h-4" />
          </Link>
        </div>
      )}

      {/* Case 2: Forming (1-4 answers) */}
      {isForming && dna && (
        <div className="space-y-6">
          <div className="p-5 rounded-2xl bg-muted/30 border border-dashed border-border/80 space-y-3">
            <div className="flex items-center gap-2 text-rose-500">
              <Info className="w-4 h-4" />
              <h4 className="text-xs font-bold uppercase tracking-wider">DNA đang hình thành...</h4>
            </div>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Bạn mới hoàn thành {dna.totalAnswers} câu hỏi. Hãy trả lời thêm ít nhất {5 - dna.totalAnswers} câu hỏi nữa qua Lucky Journey hoặc Daily Journey để mở khóa đầy đủ hình tượng nhân vật chính xác nhất của bạn!
            </p>
            {/* Progress indicator */}
            <div className="space-y-1">
              <div className="flex justify-between text-[10px] font-bold text-muted-foreground">
                <span>Tiến trình hoàn thiện DNA</span>
                <span>{dna.totalAnswers * 20}%</span>
              </div>
              <div className="w-full h-2 bg-muted rounded-full overflow-hidden">
                <div
                  className="h-full bg-gradient-to-r from-rose-500 to-orange-500 rounded-full transition-all duration-500"
                  style={{ width: `${dna.totalAnswers * 20}%` }}
                ></div>
              </div>
            </div>
          </div>

          {/* Simple Scores display */}
          <div className="p-6 rounded-2xl bg-card border border-border/80 space-y-4">
            <h3 className="text-sm font-bold text-foreground">Điểm số các khía cạnh ban đầu:</h3>
            <div className="space-y-3.5">
              {dna.allTraits
                .filter((t) => t.score > 0)
                .map((t) => (
                  <div key={t.traitCode} className="space-y-1">
                    <div className="flex justify-between text-xs">
                      <span className="font-semibold text-foreground">{t.traitName}</span>
                      <span className="font-black text-rose-500">{t.score}%</span>
                    </div>
                    <div className="w-full h-1.5 bg-muted rounded-full overflow-hidden">
                      <div
                        className="h-full bg-rose-500 rounded-full"
                        style={{ width: `${t.score}%` }}
                      ></div>
                    </div>
                  </div>
                ))}
            </div>
          </div>
        </div>
      )}

      {/* Case 3: Completed (5+ answers) */}
      {!isNotFormed && !isForming && dna && (
        <div className="space-y-6">
          {/* Archetype Profile Card */}
          <div className="p-6 sm:p-8 rounded-3xl bg-gradient-to-br from-card to-muted/20 border border-border/80 space-y-4 relative overflow-hidden shadow-sm">
            <div className="absolute top-0 right-0 w-24 h-24 bg-gradient-to-br from-rose-500/5 to-amber-500/5 rounded-full blur-2xl pointer-events-none"></div>

            <div className="space-y-2">
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-rose-500/10 text-rose-500 text-[10px] font-black uppercase tracking-wider border border-rose-500/20">
                <Sparkles className="w-3 h-3 animate-spin" />
                <span>Hình tượng chủ đạo</span>
              </span>
              <h2 className="text-xl sm:text-2xl font-black text-foreground tracking-tight">
                {dna.archetype}
              </h2>
              <p className="text-xs text-muted-foreground leading-relaxed">
                {dna.description}
              </p>
            </div>

            {/* Metadata info */}
            <div className="pt-3 border-t border-border flex items-center justify-between text-[10px] text-muted-foreground font-semibold">
              <span>Được phân tích từ {dna.totalAnswers} lựa chọn tâm linh</span>
              {dna.updatedAt && (
                <span>Cập nhật lúc {new Date(dna.updatedAt).toLocaleDateString("vi-VN")}</span>
              )}
            </div>
          </div>

          {/* Traits score list */}
          <div className="p-6 rounded-3xl bg-card border border-border/80 space-y-5">
            <h3 className="text-sm font-black text-foreground flex items-center gap-2">
              <Dna className="w-4 h-4 text-rose-500" />
              <span>Phổ chi tiết các luồng tính cách</span>
            </h3>

            <div className="space-y-4">
              {dna.allTraits.map((t) => (
                <div key={t.traitCode} className="space-y-1.5">
                  <div className="flex justify-between items-center text-xs">
                    <div className="flex items-center gap-1.5">
                      <span className="font-bold text-foreground">{t.traitName}</span>
                      <span className="text-[10px] text-muted-foreground">({t.sampleCount} câu)</span>
                    </div>
                    <span className="font-extrabold text-foreground">{t.score}%</span>
                  </div>
                  <div className="w-full h-2 bg-muted rounded-full overflow-hidden">
                    <div
                      className={`h-full rounded-full transition-all duration-500 ${
                        t.score >= 70
                          ? "bg-gradient-to-r from-rose-500 to-orange-500"
                          : t.score >= 40
                          ? "bg-gradient-to-r from-orange-400 to-amber-400"
                          : "bg-zinc-400"
                      }`}
                      style={{ width: `${t.score}%` }}
                    ></div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
