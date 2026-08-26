"use client";

import React from "react";
import { Game } from "@/types/game";
import { RevealedNumberDto } from "@/types/lucky";
import { Sparkles } from "lucide-react";

interface JourneyProgressBarProps {
  game: Game;
  currentStep: number;
  totalSteps: number;
  completedNumbers: RevealedNumberDto[];
  isClimaxStep: boolean;
}

export function JourneyProgressBar({
  game,
  currentStep,
  totalSteps,
  completedNumbers,
  isClimaxStep
}: JourneyProgressBarProps) {
  const isMultiPool = game.pools.length > 1;
  const pool0 = game.pools.find((p) => p.poolIndex === 0);
  const pool1 = game.pools.find((p) => p.poolIndex === 1);

  const mainCount = pool0?.pickCount || (totalSteps - (pool1?.pickCount || 0));
  const specialCount = pool1?.pickCount || 0;

  return (
    <div className="w-full space-y-3">
      {/* Header Info */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          {isClimaxStep ? (
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-gradient-to-r from-amber-500/20 to-yellow-500/20 text-amber-600 dark:text-amber-400 border border-amber-500/30 text-xs font-black animate-pulse">
              <Sparkles className="w-3.5 h-3.5" />
              🔮 LỰA CHỌN CUỐI CÙNG — SỐ ĐẶC BIỆT
            </span>
          ) : (
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-primary/10 text-primary border border-primary/20 text-xs font-black">
              🍀 SỐ {Math.min(currentStep, totalSteps)} / {totalSteps}
            </span>
          )}
        </div>

        <span className="text-xs text-muted-foreground font-semibold">
          {completedNumbers.length} / {totalSteps} con số đã mở
        </span>
      </div>

      {/* Progress Dots / Number Balls Container */}
      <div className="flex items-center gap-2 flex-wrap sm:flex-nowrap p-3 rounded-2xl bg-muted/40 border border-border/60">
        {/* Main Pool Dots */}
        {Array.from({ length: mainCount }).map((_, idx) => {
          const stepNumber = idx + 1;
          const revealed = completedNumbers[idx];
          const isCurrent = currentStep === stepNumber && !revealed;

          return (
            <div
              key={`main-step-${idx}`}
              className={`
                relative flex-1 min-w-[36px] h-10 sm:h-11 rounded-xl flex items-center justify-center font-black text-xs sm:text-sm transition-all duration-300
                ${
                  revealed
                    ? "bg-gradient-to-br from-rose-600 to-amber-600 text-white shadow-md shadow-rose-500/20 scale-100"
                    : isCurrent
                    ? "border-2 border-rose-500 bg-rose-500/10 text-rose-600 dark:text-rose-400 ring-2 ring-rose-500/30 ring-offset-1 ring-offset-background animate-pulse"
                    : "border border-dashed border-border/80 text-muted-foreground/40 bg-card/60"
                }
              `}
            >
              {revealed ? (
                <span>{revealed.formatted}</span>
              ) : (
                <span className="text-[11px] opacity-70">#{stepNumber}</span>
              )}
            </div>
          );
        })}

        {/* Multi-Pool Special Pool Separator & Dots */}
        {isMultiPool && specialCount > 0 && (
          <>
            <span className="text-amber-500 font-black text-sm px-0.5 select-none">+</span>
            {Array.from({ length: specialCount }).map((_, idx) => {
              const stepNumber = mainCount + idx + 1;
              const revealed = completedNumbers[mainCount + idx];
              const isCurrent = currentStep === stepNumber && !revealed;

              return (
                <div
                  key={`special-step-${idx}`}
                  className={`
                    relative w-12 sm:w-14 h-10 sm:h-11 rounded-xl flex items-center justify-center font-black text-xs sm:text-sm transition-all duration-300
                    ${
                      revealed
                        ? "bg-gradient-to-br from-amber-400 to-yellow-500 text-stone-950 shadow-md shadow-yellow-500/30 border-2 border-yellow-200"
                        : isCurrent
                        ? "border-2 border-amber-400 bg-amber-400/15 text-amber-600 dark:text-amber-400 ring-2 ring-amber-400/40 ring-offset-1 ring-offset-background animate-bounce"
                        : "border-2 border-dashed border-amber-500/40 bg-amber-500/5 text-amber-500/50"
                    }
                  `}
                >
                  {revealed ? (
                    <span className="flex items-center gap-0.5">
                      {revealed.formatted}
                      <span className="text-[10px] text-yellow-800">★</span>
                    </span>
                  ) : (
                    <span className="text-[11px] font-extrabold flex items-center gap-0.5">
                      ★ #{stepNumber}
                    </span>
                  )}
                </div>
              );
            })}
          </>
        )}
      </div>
    </div>
  );
}
