"use client";

import React from "react";
import { Game } from "@/types/game";
import { SlipNumber } from "@/types/slip";
import { useMixedBuilderStore } from "@/stores/useMixedBuilderStore";
import { MixedSlotCard } from "./MixedSlotCard";
import { SlotSourcePicker } from "./SlotSourcePicker";
import {
  X,
  Sparkles,
  Dices,
  RotateCcw,
  AlertCircle,
  CheckCircle2,
  HelpCircle
} from "lucide-react";

interface MixedBuilderModalProps {
  game: Game;
  onSaveToSlipLine: (lineLabel: string, numbers: SlipNumber[]) => void;
}

export function MixedBuilderModal({ game, onSaveToSlipLine }: MixedBuilderModalProps) {
  const {
    isOpen,
    lineLabel,
    slots,
    activeSlotIndex,
    activePickerMode,
    luckyQuestion,
    luckyRevealed,
    isGenerating,
    error,
    selectSlot,
    setPickerMode,
    closePicker,
    setManualNumber,
    generateRandomForSlot,
    startLuckyForSlot,
    answerLuckyForSlot,
    applyLuckyRevealed,
    clearSlot,
    fillRemainder,
    resetAllSlots,
    closeBuilder
  } = useMixedBuilderStore();

  if (!isOpen) return null;

  const totalSlots = slots.length;
  const completedSlots = slots.filter((s) => s.status === "Completed" && s.number !== null);
  const isAllCompleted = completedSlots.length === totalSlots && totalSlots > 0;

  // Active slot details
  const activeSlot = activeSlotIndex !== null ? slots[activeSlotIndex] : null;
  const excludedInActivePool = activeSlot
    ? slots
        .filter(
          (s, idx) =>
            idx !== activeSlotIndex &&
            s.poolIndex === activeSlot.poolIndex &&
            s.number !== null
        )
        .map((s) => s.number!.value)
    : [];

  const handleApplyCompletedLine = () => {
    if (!isAllCompleted) return;

    const formattedNumbers: SlipNumber[] = slots.map((s) => s.number!);
    onSaveToSlipLine(lineLabel, formattedNumbers);
    closeBuilder();
  };

  // Group slots by pool for display
  const pool0Slots = slots.filter((s) => s.poolIndex === 0);
  const pool1Slots = slots.filter((s) => s.poolIndex === 1);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-background/85 backdrop-blur-md animate-in fade-in duration-200">
      <div
        className="relative w-full max-w-3xl max-h-[92vh] bg-card border border-border/80 rounded-3xl shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
        role="dialog"
      >
        {/* Top Decorative Line */}
        <div className="h-2 bg-gradient-to-r from-blue-600 via-rose-500 to-amber-500" />

        {/* Header */}
        <div className="flex items-center justify-between px-5 sm:px-6 py-4 border-b border-border/60 bg-muted/20">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-8 h-8 sm:w-9 sm:h-9 rounded-xl bg-gradient-to-br from-blue-500 via-purple-500 to-rose-500 text-white font-black text-sm shadow">
              {lineLabel}
            </div>
            <div>
              <div className="flex items-center gap-1.5">
                <span className="text-base">🧩</span>
                <h3 className="font-extrabold text-base sm:text-lg text-foreground">
                  Tự xây bộ số — Dòng {lineLabel}
                </h3>
              </div>
              <p className="text-xs text-muted-foreground">
                {game.name} • Tùy biến nguồn gốc cho từng con số
              </p>
            </div>
          </div>

          <button
            type="button"
            onClick={closeBuilder}
            className="flex items-center justify-center w-8 h-8 rounded-full hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Quick Toolbar */}
        <div className="flex items-center justify-between px-5 sm:px-6 py-3 border-b border-border/40 bg-card/60 flex-wrap gap-2">
          <div className="flex items-center gap-2">
            <span
              className={`
                inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold border
                ${
                  isAllCompleted
                    ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/30"
                    : "bg-primary/10 text-primary border-primary/20"
                }
              `}
            >
              {isAllCompleted ? (
                <CheckCircle2 className="w-3.5 h-3.5" />
              ) : (
                <Sparkles className="w-3.5 h-3.5" />
              )}
              <span>
                {completedSlots.length} / {totalSlots} ô đã hoàn thành
              </span>
            </span>
          </div>

          <div className="flex items-center gap-2">
            <button
              type="button"
              disabled={isGenerating || isAllCompleted}
              onClick={() => fillRemainder("Balanced")}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-400 hover:to-orange-400 text-stone-950 font-extrabold text-xs shadow-sm shadow-amber-500/20 active:scale-95 disabled:opacity-50 transition-all cursor-pointer"
              title="Thần Tài sẽ tự động chọn các ô còn trống"
            >
              <Dices className="w-3.5 h-3.5" />
              <span>Thần Tài điền phần còn lại</span>
            </button>

            <button
              type="button"
              onClick={resetAllSlots}
              className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-xl border border-border hover:bg-muted text-muted-foreground hover:text-foreground text-xs font-medium transition-colors"
              title="Xóa tất cả các ô"
            >
              <RotateCcw className="w-3 h-3" />
              <span>Làm mới</span>
            </button>
          </div>
        </div>

        {/* Error banner */}
        {error && (
          <div className="mx-5 sm:mx-6 mt-3 p-3 rounded-2xl bg-destructive/10 border border-destructive/30 text-destructive text-xs font-semibold flex items-center gap-2">
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            <span>{error}</span>
          </div>
        )}

        {/* Modal Body: Slots Grid */}
        <div className="flex-1 overflow-y-auto p-5 sm:p-7 space-y-6">
          {/* Main Pool Slots */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-xs">
              <span className="font-extrabold uppercase tracking-wider text-muted-foreground">
                {game.pools[0]?.name || "Dãy số chính"} ({pool0Slots.length} số)
              </span>
              <span className="text-[11px] text-muted-foreground">
                Dải số: {game.pools[0]?.minNumber} - {game.pools[0]?.maxNumber}
              </span>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-2.5">
              {pool0Slots.map((slot) => (
                <MixedSlotCard
                  key={`slot-${slot.slotIndex}`}
                  slot={slot}
                  isActive={activeSlotIndex === slot.slotIndex}
                  onSelect={() => selectSlot(slot.slotIndex)}
                />
              ))}
            </div>
          </div>

          {/* Special Pool Slots (if exists) */}
          {pool1Slots.length > 0 && (
            <div className="space-y-2 pt-2 border-t border-dashed border-border/80">
              <div className="flex items-center justify-between text-xs">
                <span className="font-extrabold uppercase tracking-wider text-amber-600 dark:text-amber-400 flex items-center gap-1">
                  <span>★</span> {game.pools[1]?.name || "Số đặc biệt"} ({pool1Slots.length} số)
                </span>
                <span className="text-[11px] text-muted-foreground">
                  Dải số: {game.pools[1]?.minNumber} - {game.pools[1]?.maxNumber} (Độc lập với dãy chính)
                </span>
              </div>

              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-2.5">
                {pool1Slots.map((slot) => (
                  <MixedSlotCard
                    key={`slot-special-${slot.slotIndex}`}
                    slot={slot}
                    isActive={activeSlotIndex === slot.slotIndex}
                    onSelect={() => selectSlot(slot.slotIndex)}
                  />
                ))}
              </div>
            </div>
          )}

          {/* Guide hint */}
          <div className="flex items-start gap-2 p-3 rounded-2xl bg-muted/30 border border-border/60 text-xs text-muted-foreground">
            <HelpCircle className="w-4 h-4 text-amber-500 flex-shrink-0 mt-0.5" />
            <p>
              Chạm vào bất kỳ ô nào để <span className="font-semibold text-foreground">Tự chọn</span>, nhờ <span className="font-semibold text-foreground">Thần Tài</span>, hoặc trả lời 1 câu trắc nghiệm <span className="font-semibold text-foreground">Lucky</span>. Bạn có thể thay đổi hoặc xóa từng số bất kỳ lúc nào.
            </p>
          </div>
        </div>

        {/* Footer Actions */}
        <div className="px-5 sm:px-6 py-4 border-t border-border/60 bg-muted/20 flex items-center justify-between gap-3 flex-wrap">
          <button
            type="button"
            onClick={closeBuilder}
            className="px-4 py-2.5 rounded-xl border border-border hover:bg-muted text-muted-foreground hover:text-foreground text-xs font-bold transition-colors"
          >
            Hủy bỏ
          </button>

          <button
            type="button"
            disabled={!isAllCompleted}
            onClick={handleApplyCompletedLine}
            className={`
              inline-flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl font-extrabold text-xs sm:text-sm shadow-md transition-all
              ${
                isAllCompleted
                  ? "bg-gradient-to-r from-rose-600 via-orange-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 text-white shadow-rose-600/25 active:scale-95 cursor-pointer"
                  : "bg-muted text-muted-foreground border border-border/60 cursor-not-allowed opacity-50"
              }
            `}
          >
            <Sparkles className="w-4 h-4" />
            <span>Dùng bộ số này vào Dòng {lineLabel}</span>
          </button>
        </div>
      </div>

      {/* Active Slot Source Picker Submodal */}
      {activeSlot && activePickerMode && (
        <SlotSourcePicker
          game={game}
          slot={activeSlot}
          pickerMode={activePickerMode}
          excludedInPool={excludedInActivePool}
          luckyQuestion={luckyQuestion}
          luckyRevealed={luckyRevealed}
          isGenerating={isGenerating}
          onSetPickerMode={setPickerMode}
          onClose={closePicker}
          onSelectManualNumber={(val) => setManualNumber(activeSlot.slotIndex, val)}
          onSelectRandomStrategy={(strategy) => generateRandomForSlot(activeSlot.slotIndex, strategy)}
          onStartLucky={() => startLuckyForSlot(activeSlot.slotIndex)}
          onAnswerLucky={(choiceId) => answerLuckyForSlot(activeSlot.slotIndex, choiceId)}
          onApplyLuckyRevealed={() => applyLuckyRevealed(activeSlot.slotIndex)}
          onClearSlot={() => clearSlot(activeSlot.slotIndex)}
        />
      )}
    </div>
  );
}
