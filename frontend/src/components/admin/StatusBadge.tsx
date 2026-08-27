"use client";

import React from "react";

interface StatusBadgeProps {
  isActive: boolean;
  activeLabel?: string;
  inactiveLabel?: string;
  size?: "sm" | "md";
}

export function StatusBadge({
  isActive,
  activeLabel = "Hoạt động",
  inactiveLabel = "Tạm dừng",
  size = "sm"
}: StatusBadgeProps) {
  const sizeClasses = size === "sm" ? "px-2 py-0.5 text-xs" : "px-3 py-1 text-sm";

  if (isActive) {
    return (
      <span className={`inline-flex items-center gap-1.5 rounded-full font-medium bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 ${sizeClasses}`}>
        <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 animate-pulse" />
        {activeLabel}
      </span>
    );
  }

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full font-medium bg-zinc-700/30 text-zinc-400 border border-zinc-700/50 ${sizeClasses}`}>
      <span className="h-1.5 w-1.5 rounded-full bg-zinc-400" />
      {inactiveLabel}
    </span>
  );
}