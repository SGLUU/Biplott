"use client";

import React, { useState } from "react";
import { Game } from "@/types/game";
import { Slip, SlipLine, RandomStrategy } from "@/types/slip";
import { generateThanTaiSlip } from "@/lib/api";
import {
  X,
  Sparkles,
  Dice5,
  Scale,
  Compass,
  Zap,
  CheckCircle2,
  AlertTriangle,
  Loader2
} from "lucide-react";

interface BulkGenerateModalProps {
  isOpen: boolean;
  onClose: () => void;
  game: Game;
  slip: Slip;
  onSuccess: (lines: SlipLine[]) => void;
}

export function BulkGenerateModal({
  isOpen,
  onClose,
  game,
  slip,
  onSuccess
}: BulkGenerateModalProps) {
  const [selectedStrategy, setSelectedStrategy] = useState<RandomStrategy>("Balanced");
  const [fillMode, setFillMode] = useState<"EmptyOnly" | "All">("EmptyOnly");
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  if (!isOpen) return null;

  const hasManualLines = slip.lines.some(
    (l) => l.status === "Complete" && l.numbers.some((n) => n.source === "Manual")
  );

  const completedLinesCount = slip.lines.filter((l) => l.status === "Complete").length;

  const handleGenerate = async () => {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const res = await generateThanTaiSlip({
        gameCode: game.code,
        strategy: selectedStrategy,
        fillMode: fillMode,
        existingLines: slip.lines
      });

      onSuccess(res.lines);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Lỗi khi sinh cả phiếu Thần Tài";
      setErrorMessage(msg);
    } finally {
      setIsLoading(false);
    }
  };

  const strategies: {
    id: RandomStrategy;
    name: string;
    description: string;
    icon: React.ReactNode;
  }[] = [
    {
      id: "PureRandom",
      name: "Pure Random",
      description: "Ngẫu nhiên thuần khiết, ngẫu nhiên bảo mật CSPRNG.",
      icon: <Dice5 className="w-5 h-5 text-rose-500" />
    },
    {
      id: "Balanced",
      name: "Balanced (Khuyên dùng)",
      description: "Cân bằng chẵn lẻ và cao thấp, vuông tròn âm dương.",
      icon: <Scale className="w-5 h-5 text-amber-500" />
    },
    {
      id: "Spread",
      name: "Spread",
      description: "Phân tán đều khắp các phân vùng dải số.",
      icon: <Compass className="w-5 h-5 text-orange-500" />
    },
    {
      id: "Surprise",
      name: "Surprise",
      description: "Dãy số mang cấu trúc độc lạ, phá cách.",
      icon: <Zap className="w-5 h-5 text-yellow-500" />
    }
  ];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-background/80 backdrop-blur-sm animate-in fade-in duration-200">
      <div
        className="relative w-full max-w-lg bg-card border border-border rounded-3xl shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
        role="dialog"
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border/60 bg-gradient-to-r from-rose-500/10 via-amber-500/10 to-transparent">
          <div className="flex items-center gap-2.5">
            <div className="p-2 rounded-xl bg-gradient-to-br from-rose-600 to-amber-600 text-white shadow-md shadow-rose-500/20">
              <Sparkles className="w-5 h-5" />
            </div>
            <div>
              <h3 className="font-extrabold text-base sm:text-lg text-foreground">
                Thần Tài cả phiếu
              </h3>
              <p className="text-xs text-muted-foreground">{game.name}</p>
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

        {/* Error Alert */}
        {errorMessage && (
          <div className="mx-5 mt-3 p-3 rounded-xl bg-destructive/10 border border-destructive/30 text-destructive text-xs font-semibold">
            {errorMessage}
          </div>
        )}

        {/* Body */}
        <div className="p-5 sm:p-6 space-y-5 overflow-y-auto max-h-[70vh]">
          {/* Strategy Selection */}
          <div className="space-y-2.5">
            <label className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
              1. Chọn phong cách sinh số:
            </label>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
              {strategies.map((strat) => {
                const isActive = selectedStrategy === strat.id;
                return (
                  <button
                    key={strat.id}
                    type="button"
                    onClick={() => setSelectedStrategy(strat.id)}
                    className={`
                      flex items-start gap-2.5 p-3 rounded-2xl border text-left transition-all
                      ${
                        isActive
                          ? "bg-gradient-to-br from-rose-500/10 to-amber-500/10 border-rose-500 shadow-sm ring-1 ring-rose-500/30"
                          : "bg-card border-border hover:border-primary/40 hover:bg-muted/30"
                      }
                    `}
                  >
                    <div className="p-1.5 rounded-lg bg-background border border-border flex-shrink-0 mt-0.5">
                      {strat.icon}
                    </div>
                    <div>
                      <h4 className="font-extrabold text-xs text-foreground mb-0.5">
                        {strat.name}
                      </h4>
                      <p className="text-[11px] text-muted-foreground line-clamp-2">
                        {strat.description}
                      </p>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Fill Mode Selection */}
          <div className="space-y-2.5">
            <label className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
              2. Chọn phạm vi áp dụng:
            </label>

            <div className="space-y-2">
              <label
                className={`
                  flex items-start gap-3 p-3 rounded-2xl border cursor-pointer transition-all
                  ${
                    fillMode === "EmptyOnly"
                      ? "bg-primary/5 border-primary ring-1 ring-primary/20"
                      : "bg-card border-border hover:bg-muted/30"
                  }
                `}
              >
                <input
                  type="radio"
                  name="fillMode"
                  value="EmptyOnly"
                  checked={fillMode === "EmptyOnly"}
                  onChange={() => setFillMode("EmptyOnly")}
                  className="mt-1 text-primary focus:ring-primary"
                />
                <div className="text-xs">
                  <span className="font-bold text-foreground block">
                    Chỉ điền các dòng còn trống (Khuyên dùng)
                  </span>
                  <span className="text-muted-foreground">
                    Giữ nguyên {completedLinesCount} dòng bạn đã chọn hoặc hoàn thành trước đó.
                  </span>
                </div>
              </label>

              <label
                className={`
                  flex items-start gap-3 p-3 rounded-2xl border cursor-pointer transition-all
                  ${
                    fillMode === "All"
                      ? "bg-rose-500/5 border-rose-500 ring-1 ring-rose-500/20"
                      : "bg-card border-border hover:bg-muted/30"
                  }
                `}
              >
                <input
                  type="radio"
                  name="fillMode"
                  value="All"
                  checked={fillMode === "All"}
                  onChange={() => setFillMode("All")}
                  className="mt-1 text-rose-500 focus:ring-rose-500"
                />
                <div className="text-xs">
                  <span className="font-bold text-foreground block">
                    Tạo lại toàn bộ 6 dòng (A đến F)
                  </span>
                  <span className="text-muted-foreground">
                    Thay thế toàn bộ phiếu bằng 6 bộ số ngẫu nhiên mới, không trùng lặp.
                  </span>
                </div>
              </label>
            </div>
          </div>

          {/* Warning if overwriting manual lines */}
          {fillMode === "All" && hasManualLines && (
            <div className="p-3 rounded-xl bg-amber-500/10 border border-amber-500/30 text-amber-700 dark:text-amber-300 text-xs flex items-center gap-2">
              <AlertTriangle className="w-4 h-4 flex-shrink-0" />
              <span>
                Cảnh báo: Bạn đang có dòng số tự chọn. Chế độ &quot;Tạo lại toàn bộ&quot; sẽ ghi đè các dòng này.
              </span>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-2 px-5 py-4 border-t border-border/60 bg-muted/20">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 rounded-xl text-xs sm:text-sm font-semibold border border-border hover:bg-muted text-foreground transition-colors"
          >
            Hủy
          </button>
          <button
            type="button"
            onClick={handleGenerate}
            disabled={isLoading}
            className="px-5 py-2 rounded-xl text-xs sm:text-sm font-bold text-white bg-gradient-to-r from-rose-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 shadow-md shadow-rose-600/20 active:scale-95 transition-all flex items-center gap-2"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Đang sinh số...</span>
              </>
            ) : (
              <>
                <CheckCircle2 className="w-4 h-4" />
                <span>Xác nhận sinh cả phiếu</span>
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
