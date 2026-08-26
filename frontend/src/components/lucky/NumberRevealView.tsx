"use client";

import React from "react";
import { RevealedNumberDto } from "@/types/lucky";
import { NumberBall } from "@/components/slip/NumberBall";
import { ArrowRight, Sparkles, Quote } from "lucide-react";

interface NumberRevealViewProps {
  revealedNumber: RevealedNumberDto;
  isCompleted: boolean;
  onProceed: () => void;
}

export function NumberRevealView({
  revealedNumber,
  isCompleted,
  onProceed
}: NumberRevealViewProps) {
  const isSpecial = revealedNumber.poolIndex === 1;

  return (
    <div className="flex flex-col items-center justify-center text-center space-y-6 py-4 animate-in fade-in zoom-in-95 duration-300">
      {/* Top Banner */}
      <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-gradient-to-r from-rose-500/10 via-orange-500/10 to-amber-500/10 border border-orange-500/30 text-orange-600 dark:text-orange-400 text-xs font-black">
        <Sparkles className="w-4 h-4 text-amber-500 animate-spin" />
        <span>VẬN MỆNH ĐÃ TIẾT LỘ CON SỐ!</span>
      </div>

      {/* Choice summary */}
      {revealedNumber.choiceText && (
        <p className="text-xs sm:text-sm text-muted-foreground font-medium max-w-md">
          Từ lựa chọn: <span className="font-extrabold text-foreground">&ldquo;{revealedNumber.choiceText}&rdquo;</span>
        </p>
      )}

      {/* Revealed Ball Container with Glow */}
      <div className="relative my-2">
        <div className="absolute inset-0 rounded-full bg-gradient-to-tr from-rose-500 to-amber-400 blur-2xl opacity-40 animate-pulse pointer-events-none" />
        <div className="scale-125 sm:scale-150 transform transition-transform">
          <NumberBall
            value={revealedNumber.value}
            poolIndex={revealedNumber.poolIndex}
            source="Lucky"
            isSpecial={isSpecial}
            size="lg"
          />
        </div>
      </div>

      {/* Dominant Trait Badge */}
      {revealedNumber.dominantTrait && (
        <div className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-muted border border-border text-xs font-bold text-muted-foreground">
          <span>✨ Thuộc tính:</span>
          <span className="text-foreground">{revealedNumber.dominantTrait}</span>
        </div>
      )}

      {/* Explanation Box */}
      <div className="relative max-w-lg p-4 sm:p-5 rounded-3xl bg-gradient-to-br from-muted/80 via-card to-muted/40 border border-border/80 shadow-md">
        <Quote className="w-6 h-6 text-rose-500/30 absolute -top-3 left-4" />
        <p className="text-xs sm:text-sm text-foreground/90 font-medium italic leading-relaxed pt-1">
          &ldquo;{revealedNumber.explanation}&rdquo;
        </p>
      </div>

      {/* Proceed Button */}
      <button
        type="button"
        onClick={onProceed}
        className="mt-2 inline-flex items-center justify-center gap-2 px-8 py-3.5 rounded-2xl bg-gradient-to-r from-rose-600 via-orange-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 text-white font-extrabold text-sm sm:text-base shadow-lg shadow-rose-600/30 active:scale-95 transition-all"
      >
        <span>{isCompleted ? "Xem tổng kết toàn bộ phiếu 🎉" : "Tiếp tục câu tiếp theo"}</span>
        <ArrowRight className="w-5 h-5" />
      </button>
    </div>
  );
}
