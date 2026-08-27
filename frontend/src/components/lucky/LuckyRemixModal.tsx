"use client";

import React, { useState } from "react";
import { Game } from "@/types/game";
import { SlipNumber } from "@/types/slip";
import { useLuckyRemixStore } from "@/stores/useLuckyRemixStore";
import { sortSlipNumbers } from "@/lib/utils";
import { JourneyProgressBar } from "./JourneyProgressBar";
import { QuestionCard } from "./QuestionCard";
import { NumberRevealView } from "./NumberRevealView";
import { LuckyCompleteView } from "./LuckyCompleteView";
import { X, Sparkles, AlertCircle } from "lucide-react";

interface LuckyRemixModalProps {
  game: Game;
  onSaveToSlipLine: (
    lineLabel: string,
    numbers: SlipNumber[],
    commentary?: string
  ) => void;
}

export function LuckyRemixModal({
  game,
  onSaveToSlipLine
}: LuckyRemixModalProps) {
  const {
    isOpen,
    lineLabel,
    currentStep,
    totalSteps,
    isClimaxStep,
    currentQuestion,
    selectedChoiceId,
    revealedNumber,
    completedNumbers,
    isSubmitting,
    isRevealed,
    isCompleted,
    journeyCommentary,
    error,
    submitChoice,
    proceedToNextStep,
    closeRemix,
    resetRemix
  } = useLuckyRemixStore();

  const [showCancelConfirm, setShowCancelConfirm] = useState(false);

  if (!isOpen) return null;

  const handleClose = () => {
    if (completedNumbers.length > 0 && !isCompleted) {
      setShowCancelConfirm(true);
    } else {
      closeRemix();
    }
  };

  const handleConfirmCancel = () => {
    setShowCancelConfirm(false);
    closeRemix();
  };

  const handleApplyCompleted = () => {
    const formattedNumbers: SlipNumber[] = sortSlipNumbers(
      completedNumbers.map((n) => ({
        value: n.value,
        formatted: (n.value < 10 ? "0" : "") + n.value,
        poolIndex: n.poolIndex,
        source: n.source,
        metadataJson: n.metadataJson,
        isLocked: n.isLocked
      }))
    );

    onSaveToSlipLine(lineLabel, formattedNumbers, journeyCommentary || undefined);
    resetRemix();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-background/85 backdrop-blur-md animate-in fade-in duration-200">
      <div
        className="relative w-full max-w-2xl max-h-[92vh] bg-card border border-border/80 rounded-3xl shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
        role="dialog"
      >
        {/* Top Decorative Line */}
        <div className="h-2 bg-gradient-to-r from-rose-600 via-orange-500 to-amber-500" />

        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border/60 bg-muted/20">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-8 h-8 rounded-xl bg-gradient-to-br from-rose-500 to-amber-500 text-white font-black text-xs shadow">
              🔄
            </div>
            <div>
              <div className="flex items-center gap-1.5">
                <Sparkles className="w-4 h-4 text-amber-500" />
                <h3 className="font-extrabold text-base sm:text-lg text-foreground">
                  Lucky Remix — Dòng {lineLabel}
                </h3>
              </div>
              <p className="text-xs text-muted-foreground">{game.name}</p>
            </div>
          </div>

          <button
            type="button"
            onClick={handleClose}
            className="flex items-center justify-center w-8 h-8 rounded-full hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Error banner */}
        {error && (
          <div className="mx-5 mt-3 p-3 rounded-2xl bg-destructive/10 border border-destructive/30 text-destructive text-xs font-semibold flex items-center gap-2">
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            <span>{error}</span>
          </div>
        )}

        {/* Cancel Confirmation Dialog Overlay */}
        {showCancelConfirm && (
          <div className="absolute inset-0 z-20 bg-background/90 backdrop-blur-sm flex flex-col items-center justify-center p-6 text-center space-y-4">
            <AlertCircle className="w-10 h-10 text-amber-500" />
            <h4 className="text-lg font-bold text-foreground">
              Dừng hành trình Lucky Remix?
            </h4>
            <p className="text-xs text-muted-foreground max-w-sm">
              Bạn đang trả lời giữa chừng. Nếu thoát bây giờ, các con số mới giải mã sẽ không được lưu vào vé.
            </p>
            <div className="flex items-center gap-3 pt-2">
              <button
                type="button"
                onClick={() => setShowCancelConfirm(false)}
                className="px-4 py-2 rounded-xl border border-border text-xs font-bold hover:bg-muted text-foreground"
              >
                Tiếp tục
              </button>
              <button
                type="button"
                onClick={handleConfirmCancel}
                className="px-4 py-2 rounded-xl bg-destructive text-destructive-foreground text-xs font-bold shadow hover:opacity-90"
              >
                Hủy bỏ
              </button>
            </div>
          </div>
        )}

        {/* Modal Body Scroll Area */}
        <div className="flex-1 overflow-y-auto p-5 sm:p-6 space-y-6">
          {!isCompleted && (
            <JourneyProgressBar
              game={game}
              currentStep={currentStep}
              totalSteps={totalSteps}
              completedNumbers={completedNumbers.map((n) => ({
                ...n,
                formatted: (n.value < 10 ? "0" : "") + n.value
              }))}
              isClimaxStep={isClimaxStep}
            />
          )}

          {isCompleted ? (
            <LuckyCompleteView
              lineLabel={lineLabel}
              gameName={game.name}
              completedNumbers={completedNumbers.map((n) => ({
                ...n,
                formatted: (n.value < 10 ? "0" : "") + n.value
              }))}
              journeyCommentary={journeyCommentary || "Hành trình Lucky Remix hoàn tất!"}
              onApply={handleApplyCompleted}
            />
          ) : isRevealed && revealedNumber ? (
            <NumberRevealView
              revealedNumber={{
                ...revealedNumber,
                formatted: (revealedNumber.value < 10 ? "0" : "") + revealedNumber.value
              }}
              isCompleted={isCompleted}
              onProceed={proceedToNextStep}
            />
          ) : currentQuestion ? (
            <QuestionCard
              question={currentQuestion}
              isSubmitting={isSubmitting}
              selectedChoiceId={selectedChoiceId}
              onSelectChoice={submitChoice}
            />
          ) : (
            <div className="flex items-center justify-center py-20 text-muted-foreground text-sm font-semibold">
              {!error && "Đang nạp hành trình..."}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
