import React from "react";
import { NumberSource } from "@/types/slip";

interface NumberBallProps {
  value: number;
  poolIndex?: number;
  source?: NumberSource;
  size?: "sm" | "md" | "lg";
  badgeColor?: string;
  isSpecial?: boolean;
  selected?: boolean;
  disabled?: boolean;
  interactive?: boolean;
  onClick?: () => void;
}

export function NumberBall({
  value,
  poolIndex = 0,
  source,
  size = "md",
  badgeColor,
  isSpecial = false,
  selected = false,
  disabled = false,
  interactive = false,
  onClick
}: NumberBallProps) {
  const formatted = value.toString().padStart(2, "0");

  const sizeClasses = {
    sm: "w-8 h-8 text-xs font-bold",
    md: "w-10 h-10 text-sm font-black md:w-11 md:h-11 md:text-base",
    lg: "w-12 h-12 text-base font-black md:w-14 md:h-14 md:text-lg"
  }[size];

  // Base colors for Pool 0 vs Special Pool 1
  const isSpecialPool = isSpecial || poolIndex === 1;

  let bgClass = "bg-gradient-to-br from-rose-500 via-orange-500 to-amber-500 text-white shadow-md shadow-rose-500/20";
  if (isSpecialPool) {
    bgClass = "bg-gradient-to-br from-amber-400 via-yellow-400 to-yellow-600 text-stone-900 shadow-md shadow-yellow-500/30 font-black border-2 border-yellow-200";
  }

  // If used in interactive grid
  if (interactive) {
    if (selected) {
      if (isSpecialPool) {
        bgClass = "bg-gradient-to-br from-amber-400 to-yellow-500 text-stone-950 ring-2 ring-yellow-400 ring-offset-2 ring-offset-background scale-105 shadow-lg shadow-yellow-500/40";
      } else {
        bgClass = "bg-gradient-to-br from-rose-600 via-orange-600 to-amber-600 text-white ring-2 ring-rose-500 ring-offset-2 ring-offset-background scale-105 shadow-lg shadow-rose-500/40";
      }
    } else {
      bgClass = "bg-card text-card-foreground border border-border/80 hover:border-primary/50 hover:bg-muted/80";
    }
  }

  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onClick}
      className={`
        relative inline-flex items-center justify-center rounded-full select-none
        transition-all duration-200 ease-out
        ${sizeClasses}
        ${bgClass}
        ${interactive ? "cursor-pointer active:scale-95" : "cursor-default pointer-events-none"}
        ${disabled ? "opacity-40 cursor-not-allowed hover:bg-card" : ""}
      `}
      style={badgeColor && selected ? { backgroundColor: badgeColor } : undefined}
      title={
        isSpecialPool
          ? `Số đặc biệt: ${formatted}${source ? ` (${source})` : ""}`
          : `Số chính: ${formatted}${source ? ` (${source})` : ""}`
      }
      data-source={source}
    >
      <span className="tracking-tighter drop-shadow-sm">{formatted}</span>

      {/* Small special pool indicator */}
      {isSpecialPool && !interactive && (
        <span className="absolute -top-1 -right-1 flex h-3 w-3 items-center justify-center rounded-full bg-stone-900 text-[8px] text-yellow-300 font-extrabold shadow">
          ★
        </span>
      )}
    </button>
  );
}
