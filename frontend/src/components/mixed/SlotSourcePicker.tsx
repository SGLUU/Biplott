"use client";

import React from "react";
import { Game } from "@/types/game";
import { MixedSlot } from "@/types/mixed";
import { RandomStrategy } from "@/types/slip";
import { NumberBall } from "@/components/slip/NumberBall";
import { QuestionCard } from "@/components/lucky/QuestionCard";
import {
  User,
  Sparkles,
  Dices,
  Trash2,
  X,
  ArrowLeft,
  Loader2,
  CheckCircle2,
  Quote
} from "lucide-react";

interface SlotSourcePickerProps {
  game: Game;
  slot: MixedSlot;
  pickerMode: "source" | "manual" | "random" | "lucky" | null;
  excludedInPool: number[];
  luckyQuestion: import("@/types/lucky").QuestionDto | null;
  luckyRevealed: import("@/types/lucky").RevealedNumberDto | null;
  isGenerating: boolean;
  onSetPickerMode: (mode: "source" | "manual" | "random" | "lucky" | null) => void;
  onClose: () => void;
  onSelectManualNumber: (val: number) => void;
  onSelectRandomStrategy: (strategy: RandomStrategy) => void;
  onStartLucky: () => void;
  onAnswerLucky: (choiceId: number) => void;
  onApplyLuckyRevealed: () => void;
  onClearSlot: () => void;
}

export function SlotSourcePicker({
  game,
  slot,
  pickerMode,
  excludedInPool,
  luckyQuestion,
  luckyRevealed,
  isGenerating,
  onSetPickerMode,
  onClose,
  onSelectManualNumber,
  onSelectRandomStrategy,
  onStartLucky,
  onAnswerLucky,
  onApplyLuckyRevealed,
  onClearSlot
}: SlotSourcePickerProps) {
  if (!pickerMode) return null;

  const pool = game.pools.find((p) => p.poolIndex === slot.poolIndex) || game.pools[0];
  const minNum = pool?.minNumber || 1;
  const maxNum = pool?.maxNumber || 55;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-background/80 backdrop-blur-sm animate-in fade-in duration-150">
      <div
        className="relative w-full max-w-lg bg-card border border-border/80 rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh] animate-in zoom-in-95 duration-150"
        role="dialog"
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border/60 bg-muted/20">
          <div className="flex items-center gap-2">
            {pickerMode !== "source" && (
              <button
                type="button"
                onClick={() => onSetPickerMode("source")}
                className="p-1 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground mr-1"
                title="Quay lại"
              >
                <ArrowLeft className="w-4 h-4" />
              </button>
            )}
            <div>
              <h4 className="font-extrabold text-sm sm:text-base text-foreground">
                {slot.isSpecial ? "★ Cài đặt Số đặc biệt" : `Cài đặt Ô #${slot.slotIndex + 1}`}
              </h4>
              <p className="text-xs text-muted-foreground">{slot.poolName}</p>
            </div>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex items-center justify-center w-8 h-8 rounded-full hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Content Body */}
        <div className="flex-1 overflow-y-auto p-5 space-y-4">
          {/* ================= MODE: SOURCE SELECTION ================= */}
          {pickerMode === "source" && (
            <div className="space-y-3">
              <p className="text-xs font-semibold text-muted-foreground">
                Bạn muốn tạo con số này theo cách nào?
              </p>

              <div className="grid grid-cols-1 gap-2.5">
                {/* 1. Tự chọn */}
                <button
                  type="button"
                  onClick={() => onSetPickerMode("manual")}
                  className="flex items-center gap-3.5 p-4 rounded-2xl border border-border hover:border-blue-500/60 bg-card hover:bg-blue-500/5 text-left transition-all shadow-sm group"
                >
                  <div className="w-10 h-10 rounded-xl bg-blue-500/10 border border-blue-500/20 text-blue-600 dark:text-blue-400 flex items-center justify-center flex-shrink-0 group-hover:scale-105 transition-transform">
                    <User className="w-5 h-5" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <span className="font-bold text-sm text-foreground block">
                      Tự chọn số (Manual)
                    </span>
                    <span className="text-xs text-muted-foreground block">
                      Tự tay bấm chọn số theo trực giác hoặc ngày sinh
                    </span>
                  </div>
                </button>

                {/* 2. Thần Tài */}
                <button
                  type="button"
                  onClick={() => onSetPickerMode("random")}
                  className="flex items-center gap-3.5 p-4 rounded-2xl border border-border hover:border-amber-500/60 bg-card hover:bg-amber-500/5 text-left transition-all shadow-sm group"
                >
                  <div className="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/20 text-amber-600 dark:text-amber-400 flex items-center justify-center flex-shrink-0 group-hover:scale-105 transition-transform">
                    <Dices className="w-5 h-5" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <span className="font-bold text-sm text-foreground block">
                      Thần Tài gánh (Random)
                    </span>
                    <span className="text-xs text-muted-foreground block">
                      Ngẫu nhiên bảo mật với 4 chiến lược chọn lọc
                    </span>
                  </div>
                </button>

                {/* 3. Lucky Journey */}
                <button
                  type="button"
                  onClick={onStartLucky}
                  className="flex items-center gap-3.5 p-4 rounded-2xl border border-border hover:border-emerald-500/60 bg-card hover:bg-emerald-500/5 text-left transition-all shadow-sm group"
                >
                  <div className="w-10 h-10 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-600 dark:text-emerald-400 flex items-center justify-center flex-shrink-0 group-hover:scale-105 transition-transform">
                    <Sparkles className="w-5 h-5" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <span className="font-bold text-sm text-foreground block">
                      Lucky Journey (1 câu hỏi)
                    </span>
                    <span className="text-xs text-muted-foreground block">
                      Trả lời 1 câu trắc nghiệm để số phận tiết lộ con số
                    </span>
                  </div>
                </button>
              </div>

              {/* Clear Slot action if completed */}
              {slot.number !== null && (
                <div className="pt-2 border-t border-border/60">
                  <button
                    type="button"
                    onClick={onClearSlot}
                    className="w-full flex items-center justify-center gap-2 p-2.5 rounded-xl border border-destructive/30 bg-destructive/5 hover:bg-destructive/10 text-destructive text-xs font-bold transition-colors"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                    <span>Xóa con số hiện tại khỏi ô này</span>
                  </button>
                </div>
              )}
            </div>
          )}

          {/* ================= MODE: MANUAL NUMBER GRID ================= */}
          {pickerMode === "manual" && (
            <div className="space-y-3">
              <p className="text-xs text-muted-foreground font-medium">
                Chọn một con số trong dải <span className="font-bold text-foreground">{minNum} - {maxNum}</span> (các số đã dùng trong dòng này đã bị vô hiệu):
              </p>

              <div className="grid grid-cols-6 sm:grid-cols-8 gap-2 p-2 rounded-2xl bg-muted/20 border border-border/60 max-h-64 overflow-y-auto">
                {Array.from({ length: maxNum - minNum + 1 }).map((_, idx) => {
                  const val = minNum + idx;
                  const isExcluded = excludedInPool.includes(val);
                  const isCurrent = slot.number?.value === val;

                  return (
                    <button
                      key={`manual-num-${val}`}
                      type="button"
                      disabled={isExcluded}
                      onClick={() => onSelectManualNumber(val)}
                      className={`
                        h-10 rounded-xl font-bold text-xs flex items-center justify-center transition-all cursor-pointer
                        ${
                          isCurrent
                            ? "bg-rose-600 text-white shadow-md shadow-rose-600/30 scale-105 ring-2 ring-rose-500"
                            : isExcluded
                            ? "bg-muted/40 text-muted-foreground/30 border border-transparent cursor-not-allowed line-through"
                            : "bg-card border border-border/80 hover:border-primary hover:bg-primary/10 text-foreground hover:scale-105 active:scale-95"
                        }
                      `}
                    >
                      {val.toString().padStart(2, "0")}
                    </button>
                  );
                })}
              </div>
            </div>
          )}

          {/* ================= MODE: RANDOM STRATEGY ================= */}
          {pickerMode === "random" && (
            <div className="space-y-3">
              <p className="text-xs text-muted-foreground font-medium">
                Chọn phong cách Thần Tài để sinh 1 số cho ô này:
              </p>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                {[
                  { key: "PureRandom", name: "🎲 Pure Random", desc: "Ngẫu nhiên toán học thuần túy" },
                  { key: "Balanced", name: "⚖️ Balanced", desc: "Cân bằng chẵn - lẻ & cao - thấp" },
                  { key: "Spread", name: "📐 Spread", desc: "Trải rộng, phân tán khoảng cách" },
                  { key: "Surprise", name: "⚡ Surprise", desc: "Phá vỡ quy luật, tạo bất ngờ" }
                ].map((st) => (
                  <button
                    key={st.key}
                    type="button"
                    disabled={isGenerating}
                    onClick={() => onSelectRandomStrategy(st.key as RandomStrategy)}
                    className="p-3.5 rounded-2xl border border-border hover:border-amber-500 bg-card hover:bg-amber-500/5 text-left transition-all group"
                  >
                    <span className="font-bold text-xs text-foreground block group-hover:text-amber-500">
                      {st.name}
                    </span>
                    <span className="text-[11px] text-muted-foreground block mt-0.5">
                      {st.desc}
                    </span>
                  </button>
                ))}
              </div>

              {isGenerating && (
                <div className="flex items-center justify-center gap-2 py-3 text-xs font-bold text-amber-500 animate-pulse">
                  <Loader2 className="w-4 h-4 animate-spin" />
                  <span>Thần Tài đang quay cầu...</span>
                </div>
              )}
            </div>
          )}

          {/* ================= MODE: LUCKY SINGLE QUESTION ================= */}
          {pickerMode === "lucky" && (
            <div className="space-y-4">
              {luckyRevealed ? (
                <div className="flex flex-col items-center justify-center text-center space-y-4 py-2 animate-in fade-in">
                  <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 text-xs font-extrabold border border-emerald-500/20">
                    <CheckCircle2 className="w-3.5 h-3.5" />
                    <span>Con số Lucky đã được tiết lộ!</span>
                  </div>

                  <div className="scale-125 my-1">
                    <NumberBall
                      value={luckyRevealed.value}
                      poolIndex={luckyRevealed.poolIndex}
                      source="Lucky"
                      isSpecial={slot.isSpecial}
                      size="lg"
                    />
                  </div>

                  <div className="relative max-w-sm p-3.5 rounded-2xl bg-muted/60 border border-border/80 text-xs italic text-foreground/90 font-medium">
                    <Quote className="w-4 h-4 text-emerald-500/40 absolute -top-2 left-3" />
                    &ldquo;{luckyRevealed.explanation}&rdquo;
                  </div>

                  <button
                    type="button"
                    onClick={onApplyLuckyRevealed}
                    className="w-full inline-flex items-center justify-center gap-2 py-3 rounded-2xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-extrabold text-sm shadow-md shadow-emerald-600/20 active:scale-95 transition-all"
                  >
                    <Sparkles className="w-4 h-4" />
                    <span>Dùng số này cho Ô #{slot.slotIndex + 1}</span>
                  </button>
                </div>
              ) : luckyQuestion ? (
                <QuestionCard
                  question={luckyQuestion}
                  onSelectChoice={(choiceId) => onAnswerLucky(choiceId)}
                  isSubmitting={isGenerating}
                  selectedChoiceId={null}
                />
              ) : (
                <div className="flex flex-col items-center justify-center py-10 space-y-2">
                  <Loader2 className="w-6 h-6 text-emerald-500 animate-spin" />
                  <p className="text-xs text-muted-foreground font-semibold">
                    Đang chọn câu hỏi phù hợp...
                  </p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
