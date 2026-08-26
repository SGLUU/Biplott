"use client";

import React from "react";
import { MixedSlot } from "@/types/mixed";
import { NumberBall } from "@/components/slip/NumberBall";
import { Plus, User, Sparkles, Dices, Edit2 } from "lucide-react";

interface MixedSlotCardProps {
  slot: MixedSlot;
  isActive: boolean;
  onSelect: () => void;
}

export function MixedSlotCard({ slot, isActive, onSelect }: MixedSlotCardProps) {
  const isCompleted = slot.status === "Completed" && slot.number !== null;

  return (
    <button
      type="button"
      onClick={onSelect}
      className={`
        group relative flex flex-col items-center justify-between p-3 rounded-2xl border-2 transition-all duration-200 cursor-pointer min-h-[110px] w-full
        ${
          isActive
            ? "border-rose-500 bg-rose-500/10 shadow-lg shadow-rose-500/20 ring-2 ring-rose-500/30 scale-102"
            : isCompleted
            ? slot.isSpecial
              ? "border-amber-400 bg-amber-500/5 hover:border-amber-400 hover:shadow-md"
              : "border-border/80 bg-card hover:border-primary/50 hover:shadow-md"
            : slot.isSpecial
            ? "border-dashed border-amber-500/40 bg-amber-500/5 hover:border-amber-500 text-amber-600"
            : "border-dashed border-border/80 bg-muted/20 hover:border-primary/60 hover:bg-muted/40 text-muted-foreground"
        }
      `}
    >
      {/* Top Slot Header */}
      <div className="flex items-center justify-between w-full text-[11px] font-bold">
        <span className={slot.isSpecial ? "text-amber-500 font-black flex items-center gap-0.5" : "text-muted-foreground"}>
          {slot.isSpecial ? "★ Đặc biệt" : `Ô #${slot.slotIndex + 1}`}
        </span>

        {isCompleted && (
          <span className="opacity-0 group-hover:opacity-100 text-[10px] text-muted-foreground flex items-center gap-0.5 transition-opacity">
            <Edit2 className="w-2.5 h-2.5" /> Sửa
          </span>
        )}
      </div>

      {/* Center Body (NumberBall or Empty Plus) */}
      <div className="my-1">
        {isCompleted && slot.number ? (
          <NumberBall
            value={slot.number.value}
            poolIndex={slot.poolIndex}
            source={slot.number.source}
            isSpecial={slot.isSpecial}
            size="md"
          />
        ) : (
          <div
            className={`
              w-10 h-10 rounded-full border-2 border-dashed flex items-center justify-center transition-transform group-hover:scale-110
              ${slot.isSpecial ? "border-amber-400 text-amber-500" : "border-muted-foreground/30 text-muted-foreground"}
            `}
          >
            <Plus className="w-5 h-5" />
          </div>
        )}
      </div>

      {/* Bottom Source Badge */}
      <div className="w-full text-center">
        {isCompleted && slot.number ? (
          <span
            className={`
              inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-extrabold border
              ${
                slot.number.source === "Manual"
                  ? "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20"
                  : slot.number.source === "Lucky"
                  ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20"
                  : "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20"
              }
            `}
          >
            {slot.number.source === "Manual" && <User className="w-2.5 h-2.5" />}
            {slot.number.source === "Lucky" && <Sparkles className="w-2.5 h-2.5 text-emerald-500" />}
            {slot.number.source === "Random" && <Dices className="w-2.5 h-2.5" />}
            <span>
              {slot.number.source === "Manual"
                ? "Tự chọn"
                : slot.number.source === "Lucky"
                ? "Lucky"
                : "Thần Tài"}
            </span>
          </span>
        ) : (
          <span className="text-[10px] font-medium text-muted-foreground/70">
            Chạm để chọn
          </span>
        )}
      </div>
    </button>
  );
}
