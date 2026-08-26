import React from "react";
import { SlipLine, SlipNumber } from "@/types/slip";
import { Game } from "@/types/game";
import { NumberBall } from "./NumberBall";
import { Edit3, RotateCw, Trash2, Sparkles, PlusCircle, Layers } from "lucide-react";

interface SlipLineRowProps {
  line: SlipLine;
  game: Game;
  onOpenEditor: (initialTab?: "manual" | "thantai") => void;
  onOpenLucky: () => void;
  onOpenMixed: () => void;
  onQuickThanTai: () => void;
  onReset: () => void;
}

export function SlipLineRow({
  line,
  game,
  onOpenEditor,
  onOpenLucky,
  onOpenMixed,
  onQuickThanTai,
  onReset
}: SlipLineRowProps) {
  const isComplete = line.status === "Complete" && line.numbers.length > 0;
  const isEmpty = line.numbers.length === 0;

  // Group numbers by pool
  const pool0Numbers = line.numbers.filter((n) => n.poolIndex === 0);
  const pool1Numbers = line.numbers.filter((n) => n.poolIndex === 1);

  // Determine required counts
  const pool0 = game.pools.find((p) => p.poolIndex === 0);
  const pool1 = game.pools.find((p) => p.poolIndex === 1);

  const pool0Expected = pool0?.pickCount || 6;
  const pool1Expected = pool1?.pickCount || 0;

  // Derive mode badge from number sources
  const uniqueSources = Array.from(new Set(line.numbers.map((n) => n.source)));
  const isMixed = uniqueSources.length > 1;

  return (
    <div
      className={`
        group relative rounded-2xl border p-3 sm:p-4 transition-all duration-200
        ${isComplete ? "bg-card border-border/80 shadow-sm hover:border-primary/40 hover:shadow-md" : "bg-muted/30 border-dashed border-border/70"}
      `}
    >
      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        {/* Left: Line Label & Balls */}
        <div className="flex items-center gap-3 sm:gap-4 overflow-x-auto pb-1 md:pb-0 scrollbar-none">
          {/* Label Badge */}
          <div className="flex-shrink-0 flex items-center justify-center w-8 h-8 sm:w-9 sm:h-9 rounded-xl bg-gradient-to-br from-rose-500/20 to-orange-500/20 border border-rose-500/30 text-rose-600 dark:text-rose-400 font-black text-sm sm:text-base shadow-inner">
            {line.lineLabel}
          </div>

          {/* Balls Container */}
          {isEmpty ? (
            <div className="flex items-center gap-1.5 sm:gap-2">
              {Array.from({ length: pool0Expected }).map((_, idx) => (
                <div
                  key={`p0-empty-${idx}`}
                  className="w-8 h-8 sm:w-10 sm:h-10 rounded-full border-2 border-dashed border-muted-foreground/30 flex items-center justify-center text-muted-foreground/40 text-xs font-bold select-none"
                >
                  --
                </div>
              ))}

              {pool1Expected > 0 && (
                <>
                  <span className="text-muted-foreground/40 font-bold px-0.5">+</span>
                  {Array.from({ length: pool1Expected }).map((_, idx) => (
                    <div
                      key={`p1-empty-${idx}`}
                      className="w-8 h-8 sm:w-10 sm:h-10 rounded-full border-2 border-dashed border-amber-500/40 bg-amber-500/5 flex items-center justify-center text-amber-500/50 text-xs font-bold select-none"
                    >
                      ★
                    </div>
                  ))}
                </>
              )}

              <span className="hidden lg:inline text-xs text-muted-foreground italic ml-2">
                (Chưa chọn số)
              </span>
            </div>
          ) : (
            <div className="flex items-center gap-1.5 sm:gap-2 flex-wrap">
              {/* Pool 0 numbers */}
              {pool0Numbers.map((num: SlipNumber) => (
                <NumberBall
                  key={`p0-${num.value}`}
                  value={num.value}
                  poolIndex={0}
                  source={num.source}
                  size="md"
                />
              ))}

              {/* Pool 1 separator & numbers */}
              {pool1Numbers.length > 0 && (
                <>
                  <span className="text-amber-500 font-black text-sm px-1">+</span>
                  {pool1Numbers.map((num: SlipNumber) => (
                    <NumberBall
                      key={`p1-${num.value}`}
                      value={num.value}
                      poolIndex={1}
                      source={num.source}
                      isSpecial
                      size="md"
                    />
                  ))}
                </>
              )}
            </div>
          )}
        </div>

        {/* Right: Actions & Tags */}
        <div className="flex items-center justify-between md:justify-end gap-2 pt-2 md:pt-0 border-t md:border-t-0 border-border/40">
          {/* Metadata / Source tag */}
          {isComplete && (
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground pr-1">
              {isMixed ? (
                <span className="inline-flex items-center gap-1 rounded-full bg-purple-500/10 text-purple-600 dark:text-purple-400 px-2.5 py-0.5 text-[11px] font-black border border-purple-500/20 shadow-sm">
                  <span>🧩</span>
                  <span>Mixed</span>
                </span>
              ) : uniqueSources[0] === "Manual" ? (
                <span className="inline-flex items-center gap-1 rounded-full bg-blue-500/10 text-blue-600 dark:text-blue-400 px-2 py-0.5 text-[11px] font-semibold border border-blue-500/20">
                  Tự chọn
                </span>
              ) : uniqueSources[0] === "Lucky" ? (
                <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 px-2 py-0.5 text-[11px] font-semibold border border-emerald-500/20">
                  <Sparkles className="w-3 h-3 text-emerald-500" />
                  Lucky Journey
                </span>
              ) : (
                <span className="inline-flex items-center gap-1 rounded-full bg-amber-500/10 text-amber-600 dark:text-amber-400 px-2 py-0.5 text-[11px] font-semibold border border-amber-500/20">
                  <Sparkles className="w-3 h-3" />
                  {line.strategy || "Thần Tài"}
                </span>
              )}
            </div>
          )}

          {/* Action buttons */}
          <div className="flex items-center gap-1.5 ml-auto">
            {isEmpty ? (
              <>
                <button
                  type="button"
                  onClick={onOpenMixed}
                  className="inline-flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-purple-600 via-indigo-600 to-blue-600 hover:from-purple-500 hover:to-blue-500 text-white px-3 py-1.5 text-xs font-black transition-all shadow-sm shadow-purple-600/20 active:scale-95 cursor-pointer"
                  title="Tự xây từng số với nguồn tùy biến (Manual, Thần Tài, Lucky)"
                >
                  <Layers className="w-3.5 h-3.5" />
                  <span>Tự xây</span>
                </button>
                <button
                  type="button"
                  onClick={onOpenLucky}
                  className="inline-flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-rose-600 via-orange-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 text-white px-2.5 py-1.5 text-xs font-bold transition-all shadow-sm shadow-rose-600/25 active:scale-95"
                >
                  <Sparkles className="w-3.5 h-3.5" />
                  <span>Lucky</span>
                </button>
                <button
                  type="button"
                  onClick={() => onOpenEditor("manual")}
                  className="inline-flex items-center gap-1.5 rounded-xl bg-card border border-border hover:border-primary hover:bg-primary/5 text-foreground px-2.5 py-1.5 text-xs font-semibold transition-all shadow-sm active:scale-95"
                >
                  <PlusCircle className="w-3.5 h-3.5 text-rose-500" />
                  <span>Tự chọn</span>
                </button>
                <button
                  type="button"
                  onClick={onQuickThanTai}
                  className="inline-flex items-center gap-1.5 rounded-xl bg-muted hover:bg-muted/80 text-muted-foreground hover:text-foreground px-2.5 py-1.5 text-xs font-semibold transition-all shadow-sm active:scale-95"
                  title="Thần Tài ngẫu nhiên nhanh"
                >
                  <RotateCw className="w-3.5 h-3.5" />
                  <span className="hidden sm:inline">Thần Tài</span>
                </button>
              </>
            ) : (
              <>
                <button
                  type="button"
                  onClick={onOpenMixed}
                  className="inline-flex items-center gap-1 rounded-lg bg-purple-500/10 hover:bg-purple-500/20 text-purple-600 dark:text-purple-400 px-2.5 py-1.5 text-xs font-bold transition-colors"
                  title="Chỉnh sửa chi tiết từng ô số"
                >
                  <Layers className="w-3.5 h-3.5" />
                  <span className="hidden sm:inline">Tự xây</span>
                </button>
                <button
                  type="button"
                  onClick={() => onOpenEditor("manual")}
                  className="inline-flex items-center gap-1 rounded-lg bg-muted/60 hover:bg-muted text-foreground px-2.5 py-1.5 text-xs font-medium transition-colors"
                  title="Sửa nhanh bộ số"
                >
                  <Edit3 className="w-3.5 h-3.5" />
                  <span className="hidden sm:inline">Sửa</span>
                </button>
                <button
                  type="button"
                  onClick={onQuickThanTai}
                  className="inline-flex items-center gap-1 rounded-lg bg-amber-500/10 hover:bg-amber-500/20 text-amber-600 dark:text-amber-400 px-2.5 py-1.5 text-xs font-medium transition-colors"
                  title="Tạo lại ngẫu nhiên"
                >
                  <RotateCw className="w-3.5 h-3.5" />
                  <span className="hidden sm:inline">Tạo lại</span>
                </button>
                <button
                  type="button"
                  onClick={onReset}
                  className="inline-flex items-center justify-center w-8 h-8 rounded-lg hover:bg-destructive/10 text-muted-foreground hover:text-destructive transition-colors"
                  title="Xóa dòng này"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
