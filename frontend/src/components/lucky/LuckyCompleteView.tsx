"use client";

import React from "react";
import { RevealedNumberDto } from "@/types/lucky";
import { NumberBall } from "@/components/slip/NumberBall";
import { CheckCircle2, Sparkles } from "lucide-react";

interface LuckyCompleteViewProps {
  lineLabel: string;
  gameName: string;
  completedNumbers: RevealedNumberDto[];
  journeyCommentary?: string | null;
  onApply: () => void;
}

export function LuckyCompleteView({
  lineLabel,
  gameName,
  completedNumbers,
  journeyCommentary,
  onApply
}: LuckyCompleteViewProps) {
  return (
    <div className="flex flex-col items-center justify-center text-center space-y-6 py-2 animate-in fade-in duration-300">
      {/* Top Badge */}
      <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-gradient-to-r from-emerald-500/20 to-teal-500/20 text-emerald-600 dark:text-emerald-400 border border-emerald-500/30 text-xs font-black">
        <CheckCircle2 className="w-4 h-4" />
        <span>HÀNH TRÌNH ĐÃ HOÀN TẤT</span>
      </div>

      <div className="space-y-1">
        <h3 className="text-xl sm:text-3xl font-black text-foreground tracking-tight">
          Bộ số Lucky Dòng {lineLabel}
        </h3>
        <p className="text-xs sm:text-sm text-muted-foreground font-medium">
          {gameName} • Mỗi con số sinh ra từ một quyết định thật lòng của bạn
        </p>
      </div>

      {/* Main Balls Presentation */}
      <div className="p-4 sm:p-6 rounded-3xl bg-gradient-to-br from-muted/60 via-card to-muted/30 border border-border/80 shadow-xl flex items-center justify-center gap-2.5 sm:gap-3 flex-wrap">
        {completedNumbers.map((num, idx) => (
          <NumberBall
            key={`completed-ball-${idx}-${num.value}`}
            value={num.value}
            poolIndex={num.poolIndex}
            source="Lucky"
            isSpecial={num.poolIndex === 1}
            size="lg"
          />
        ))}
      </div>

      {/* Story Summary Grid */}
      <div className="w-full max-w-lg space-y-2 text-left">
        <h4 className="text-xs font-bold uppercase tracking-wider text-muted-foreground px-1">
          Hành trình các lựa chọn:
        </h4>
        <div className="space-y-1.5 max-h-48 overflow-y-auto pr-1">
          {completedNumbers.map((num, idx) => (
            <div
              key={`story-item-${idx}`}
              className="flex items-center justify-between p-2.5 rounded-xl bg-card border border-border/60 text-xs"
            >
              <div className="flex items-center gap-2 min-w-0 pr-2">
                <span className="font-mono font-bold text-rose-500 flex-shrink-0">
                  #{idx + 1}
                </span>
                <div className="truncate">
                  <span className="font-bold text-foreground block truncate">
                    {num.themeName || "Chủ đề"}
                  </span>
                  <span className="text-[11px] text-muted-foreground truncate block">
                    {num.choiceText ? `"${num.choiceText}"` : num.explanation}
                  </span>
                </div>
              </div>

              <div className="flex-shrink-0">
                <NumberBall
                  value={num.value}
                  poolIndex={num.poolIndex}
                  source="Lucky"
                  isSpecial={num.poolIndex === 1}
                  size="sm"
                />
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Commentary */}
      {journeyCommentary && (
        <p className="text-xs text-muted-foreground italic max-w-md">
          {journeyCommentary}
        </p>
      )}

      {/* Apply Button */}
      <button
        type="button"
        onClick={onApply}
        className="w-full max-w-md inline-flex items-center justify-center gap-2 px-8 py-3.5 rounded-2xl bg-gradient-to-r from-rose-600 via-orange-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 text-white font-extrabold text-sm sm:text-base shadow-xl shadow-rose-600/25 active:scale-95 transition-all"
      >
        <Sparkles className="w-5 h-5 text-yellow-300" />
        <span>Áp dụng bộ số này vào Dòng {lineLabel}</span>
      </button>
    </div>
  );
}
