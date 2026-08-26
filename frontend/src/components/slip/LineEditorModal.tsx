"use client";

import React, { useState, useEffect } from "react";
import { Game } from "@/types/game";
import { SlipLine, SlipNumber, RandomStrategy } from "@/types/slip";
import { NumberBall } from "./NumberBall";
import { validateSlipLine, generateThanTaiLine } from "@/lib/api";
import {
  X,
  Sparkles,
  CheckCircle2,
  AlertCircle,
  RotateCcw,
  Dice5,
  Scale,
  Compass,
  Zap,
  Loader2
} from "lucide-react";

interface LineEditorModalProps {
  isOpen: boolean;
  onClose: () => void;
  game: Game;
  line: SlipLine;
  onSaveLine: (
    numbers: SlipNumber[],
    strategy?: RandomStrategy,
    commentary?: string
  ) => void;
  initialTab?: "manual" | "thantai";
}

export function LineEditorModal({
  isOpen,
  onClose,
  game,
  line,
  onSaveLine,
  initialTab = "manual"
}: LineEditorModalProps) {
  const [activeTab, setActiveTab] = useState<"manual" | "thantai">(initialTab);

  // Manual Selection state per pool
  const [selectedNumbersByPool, setSelectedNumbersByPool] = useState<Record<number, number[]>>({
    0: [],
    1: []
  });

  // Than Tai strategy selection state
  const [selectedStrategy, setSelectedStrategy] = useState<RandomStrategy>("PureRandom");
  const [generatedResult, setGeneratedResult] = useState<{
    numbers: SlipNumber[];
    commentary: string;
  } | null>(null);

  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Initialize state when modal opens or line changes
  useEffect(() => {
    if (isOpen) {
      setActiveTab(initialTab);
      setErrorMessage(null);

      // Load existing line numbers into manual selection state
      const initialPool0 = line.numbers.filter((n) => n.poolIndex === 0).map((n) => n.value);
      const initialPool1 = line.numbers.filter((n) => n.poolIndex === 1).map((n) => n.value);

      setSelectedNumbersByPool({
        0: initialPool0,
        1: initialPool1
      });

      if (line.strategy) {
        setSelectedStrategy(line.strategy);
      }

      if (line.numbers.length > 0) {
        setGeneratedResult({
          numbers: line.numbers,
          commentary: line.commentary || ""
        });
      } else {
        setGeneratedResult(null);
      }
    }
  }, [isOpen, line, initialTab]);

  if (!isOpen) return null;

  const pool0 = game.pools.find((p) => p.poolIndex === 0) || {
    id: 1,
    poolIndex: 0,
    name: "Dãy số chính",
    minNumber: 1,
    maxNumber: 55,
    pickCount: 6,
    allowDuplicates: false,
    badgeColor: "#EF4444"
  };

  const pool1 = game.pools.find((p) => p.poolIndex === 1);

  const selectedPool0 = selectedNumbersByPool[0] || [];
  const selectedPool1 = selectedNumbersByPool[1] || [];

  const isPool0Complete = selectedPool0.length === pool0.pickCount;
  const isPool1Complete = !pool1 || selectedPool1.length === pool1.pickCount;
  const isManualComplete = isPool0Complete && isPool1Complete;

  const handleToggleNumber = (poolIndex: number, val: number, maxCount: number) => {
    setErrorMessage(null);
    setSelectedNumbersByPool((prev) => {
      const current = prev[poolIndex] || [];
      if (current.includes(val)) {
        return { ...prev, [poolIndex]: current.filter((x) => x !== val) };
      }
      if (current.length >= maxCount) {
        return prev; // Reached pick count
      }
      return { ...prev, [poolIndex]: [...current, val] };
    });
  };

  const handleClearSelection = () => {
    setSelectedNumbersByPool({ 0: [], 1: [] });
    setErrorMessage(null);
  };

  const handleSaveManual = async () => {
    if (!isManualComplete) {
      setErrorMessage("Vui lòng chọn đủ số lượng cho tất cả các tập số.");
      return;
    }

    setIsLoading(true);
    setErrorMessage(null);

    const formattedNumbers: SlipNumber[] = [
      ...selectedPool0.sort((a, b) => a - b).map((val) => ({
        value: val,
        formatted: val.toString().padStart(2, "0"),
        poolIndex: 0,
        source: "Manual" as const
      })),
      ...selectedPool1.sort((a, b) => a - b).map((val) => ({
        value: val,
        formatted: val.toString().padStart(2, "0"),
        poolIndex: 1,
        source: "Manual" as const
      }))
    ];

    try {
      const validationRes = await validateSlipLine({
        gameCode: game.code,
        lineLabel: line.lineLabel,
        numbers: formattedNumbers
      });

      if (!validationRes.isValid) {
        setErrorMessage(validationRes.errors.join(". ") || "Bộ số không hợp lệ theo quy tắc trò chơi.");
        setIsLoading(false);
        return;
      }

      onSaveLine(formattedNumbers, undefined, undefined);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Lỗi xác thực bộ số";
      setErrorMessage(msg);
    } finally {
      setIsLoading(false);
    }
  };

  const handleGenerateThanTai = async (strat: RandomStrategy) => {
    setSelectedStrategy(strat);
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const res = await generateThanTaiLine({
        gameCode: game.code,
        strategy: strat
      });

      setGeneratedResult({
        numbers: res.numbers,
        commentary: res.commentary
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Không thể sinh số Thần Tài";
      setErrorMessage(msg);
    } finally {
      setIsLoading(false);
    }
  };

  const handleApplyThanTai = () => {
    if (!generatedResult || generatedResult.numbers.length === 0) {
      setErrorMessage("Chưa có bộ số nào được sinh ra.");
      return;
    }

    onSaveLine(
      generatedResult.numbers,
      selectedStrategy,
      generatedResult.commentary
    );
    onClose();
  };

  const strategies: {
    id: RandomStrategy;
    name: string;
    description: string;
    icon: React.ReactNode;
    badge: string;
  }[] = [
    {
      id: "PureRandom",
      name: "Pure Random",
      description: "Ngẫu nhiên tuyệt đối, vạn sự tùy duyên, số nào cũng có cơ hội.",
      icon: <Dice5 className="w-5 h-5 text-rose-500" />,
      badge: "Thuần khiết"
    },
    {
      id: "Balanced",
      name: "Balanced",
      description: "Cân bằng âm dương, tỷ lệ chẵn/lẻ và cao/thấp hài hòa.",
      icon: <Scale className="w-5 h-5 text-amber-500" />,
      badge: "Cân bằng"
    },
    {
      id: "Spread",
      name: "Spread",
      description: "Phân tán đều khắp các phân vùng, trải rộng dải số may mắn.",
      icon: <Compass className="w-5 h-5 text-orange-500" />,
      badge: "Rải đều"
    },
    {
      id: "Surprise",
      name: "Surprise",
      description: "Cấu trúc độc lạ bất ngờ, phá vỡ logic tính toán thông thường.",
      icon: <Zap className="w-5 h-5 text-yellow-500" />,
      badge: "Độc lạ"
    }
  ];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-background/80 backdrop-blur-sm animate-in fade-in duration-200">
      <div
        className="relative w-full max-w-2xl max-h-[90vh] bg-card border border-border rounded-3xl shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
        role="dialog"
      >
        {/* Modal Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border/60 bg-muted/20">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-8 h-8 rounded-xl bg-gradient-to-br from-rose-500 to-amber-500 text-white font-black text-sm shadow">
              {line.lineLabel}
            </div>
            <div>
              <h3 className="font-extrabold text-base sm:text-lg text-foreground">
                Tạo bộ số dòng {line.lineLabel}
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

        {/* Tab Selector */}
        <div className="flex border-b border-border/60 bg-muted/40 p-1.5 gap-1.5">
          <button
            type="button"
            onClick={() => {
              setActiveTab("manual");
              setErrorMessage(null);
            }}
            className={`
              flex-1 py-2 px-3 rounded-xl text-xs sm:text-sm font-bold transition-all
              ${activeTab === "manual" ? "bg-card text-foreground shadow-sm border border-border" : "text-muted-foreground hover:text-foreground"}
            `}
          >
            ✍️ Tự chọn số (Manual)
          </button>
          <button
            type="button"
            onClick={() => {
              setActiveTab("thantai");
              setErrorMessage(null);
              if (!generatedResult) {
                handleGenerateThanTai(selectedStrategy);
              }
            }}
            className={`
              flex-1 py-2 px-3 rounded-xl text-xs sm:text-sm font-bold transition-all flex items-center justify-center gap-1.5
              ${activeTab === "thantai" ? "bg-gradient-to-r from-rose-600 to-amber-600 text-white shadow-sm" : "text-muted-foreground hover:text-foreground"}
            `}
          >
            <Sparkles className="w-4 h-4" />
            🎲 Thần Tài Random
          </button>
        </div>

        {/* Error Alert */}
        {errorMessage && (
          <div className="mx-5 mt-3 p-3 rounded-xl bg-destructive/10 border border-destructive/30 text-destructive text-xs font-semibold flex items-center gap-2">
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            <span>{errorMessage}</span>
          </div>
        )}

        {/* Modal Body */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6">
          {activeTab === "manual" ? (
            <div className="space-y-6">
              {/* Pool 0: Main Pool */}
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="w-2.5 h-2.5 rounded-full bg-rose-500"></span>
                    <h4 className="text-sm font-bold text-foreground">{pool0.name}</h4>
                    <span className="text-xs text-muted-foreground">
                      (từ {pool0.minNumber.toString().padStart(2, "0")} đến {pool0.maxNumber.toString().padStart(2, "0")})
                    </span>
                  </div>
                  <span
                    className={`text-xs font-extrabold px-2.5 py-0.5 rounded-full border ${
                      isPool0Complete
                        ? "bg-emerald-500/10 text-emerald-600 border-emerald-500/30"
                        : "bg-muted text-muted-foreground border-border"
                    }`}
                  >
                    Đã chọn {selectedPool0.length} / {pool0.pickCount}
                  </span>
                </div>

                {/* Number Grid for Pool 0 */}
                <div className="grid grid-cols-7 sm:grid-cols-10 gap-1.5 sm:gap-2 p-3 rounded-2xl bg-muted/20 border border-border/50">
                  {Array.from(
                    { length: pool0.maxNumber - pool0.minNumber + 1 },
                    (_, i) => pool0.minNumber + i
                  ).map((val) => {
                    const isSelected = selectedPool0.includes(val);
                    return (
                      <NumberBall
                        key={`pool0-btn-${val}`}
                        value={val}
                        poolIndex={0}
                        size="sm"
                        interactive
                        selected={isSelected}
                        disabled={!isSelected && isPool0Complete}
                        onClick={() => handleToggleNumber(0, val, pool0.pickCount)}
                      />
                    );
                  })}
                </div>
              </div>

              {/* Pool 1: Special Pool (if exists) */}
              {pool1 && (
                <div className="space-y-3 pt-2 border-t border-border/50">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <span className="w-2.5 h-2.5 rounded-full bg-amber-400"></span>
                      <h4 className="text-sm font-bold text-amber-600 dark:text-amber-400">
                        {pool1.name}
                      </h4>
                      <span className="text-xs text-muted-foreground">
                        (từ {pool1.minNumber.toString().padStart(2, "0")} đến {pool1.maxNumber.toString().padStart(2, "0")})
                      </span>
                    </div>
                    <span
                      className={`text-xs font-extrabold px-2.5 py-0.5 rounded-full border ${
                        isPool1Complete
                          ? "bg-emerald-500/10 text-emerald-600 border-emerald-500/30"
                          : "bg-muted text-muted-foreground border-border"
                      }`}
                    >
                      Đã chọn {selectedPool1.length} / {pool1.pickCount}
                    </span>
                  </div>

                  {/* Number Grid for Pool 1 */}
                  <div className="grid grid-cols-6 sm:grid-cols-6 gap-2 p-3 rounded-2xl bg-amber-500/5 border border-amber-500/20">
                    {Array.from(
                      { length: pool1.maxNumber - pool1.minNumber + 1 },
                      (_, i) => pool1.minNumber + i
                    ).map((val) => {
                      const isSelected = selectedPool1.includes(val);
                      return (
                        <NumberBall
                          key={`pool1-btn-${val}`}
                          value={val}
                          poolIndex={1}
                          isSpecial
                          size="md"
                          interactive
                          selected={isSelected}
                          disabled={!isSelected && isPool1Complete}
                          onClick={() => handleToggleNumber(1, val, pool1.pickCount)}
                        />
                      );
                    })}
                  </div>
                </div>
              )}
            </div>
          ) : (
            <div className="space-y-6">
              {/* Strategy Picker Grid */}
              <div className="space-y-3">
                <label className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Chọn phong cách Thần Tài:
                </label>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                  {strategies.map((strat) => {
                    const isStratActive = selectedStrategy === strat.id;
                    return (
                      <button
                        key={strat.id}
                        type="button"
                        onClick={() => handleGenerateThanTai(strat.id)}
                        className={`
                          flex items-start gap-3 p-3.5 rounded-2xl border text-left transition-all
                          ${
                            isStratActive
                              ? "bg-gradient-to-br from-rose-500/10 to-amber-500/10 border-rose-500/60 shadow-md ring-1 ring-rose-500/30"
                              : "bg-card border-border hover:border-primary/40 hover:bg-muted/30"
                          }
                        `}
                      >
                        <div className="p-2 rounded-xl bg-background border border-border/80 shadow-sm flex-shrink-0 mt-0.5">
                          {strat.icon}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center justify-between gap-1 mb-1">
                            <span className="font-extrabold text-sm text-foreground">
                              {strat.name}
                            </span>
                            <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-muted text-muted-foreground border">
                              {strat.badge}
                            </span>
                          </div>
                          <p className="text-xs text-muted-foreground line-clamp-2 leading-relaxed">
                            {strat.description}
                          </p>
                        </div>
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Generated Result Preview */}
              {generatedResult && (
                <div className="p-4 sm:p-5 rounded-2xl bg-gradient-to-br from-muted/60 to-muted/20 border border-border/80 space-y-3.5 animate-in fade-in">
                  <div className="flex items-center justify-between">
                    <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground flex items-center gap-1.5">
                      <Sparkles className="w-3.5 h-3.5 text-amber-500" />
                      Bộ số Thần Tài đề xuất:
                    </span>
                    <button
                      type="button"
                      onClick={() => handleGenerateThanTai(selectedStrategy)}
                      disabled={isLoading}
                      className="text-xs font-bold text-rose-500 hover:text-rose-600 flex items-center gap-1 transition-colors"
                    >
                      <RotateCcw className={`w-3.5 h-3.5 ${isLoading ? "animate-spin" : ""}`} />
                      Quay số lại
                    </button>
                  </div>

                  {/* Balls display */}
                  <div className="flex items-center justify-center gap-2 flex-wrap py-2">
                    {generatedResult.numbers.map((num: SlipNumber) => (
                      <NumberBall
                        key={`gen-ball-${num.poolIndex}-${num.value}`}
                        value={num.value}
                        poolIndex={num.poolIndex}
                        isSpecial={num.poolIndex === 1}
                        size="lg"
                      />
                    ))}
                  </div>

                  {/* Commentary card */}
                  {generatedResult.commentary && (
                    <div className="p-3 rounded-xl bg-card/90 border border-border/60 text-xs text-foreground/90 italic text-center leading-relaxed">
                      &ldquo;{generatedResult.commentary}&rdquo;
                    </div>
                  )}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Modal Footer Actions */}
        <div className="flex items-center justify-between px-5 py-4 border-t border-border/60 bg-muted/20">
          {activeTab === "manual" ? (
            <>
              <button
                type="button"
                onClick={handleClearSelection}
                className="text-xs font-bold text-muted-foreground hover:text-foreground flex items-center gap-1 px-2.5 py-1.5 rounded-lg hover:bg-muted transition-colors"
              >
                <RotateCcw className="w-3.5 h-3.5" />
                <span>Xóa chọn</span>
              </button>

              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-4 py-2 rounded-xl text-xs sm:text-sm font-semibold border border-border hover:bg-muted text-foreground transition-colors"
                >
                  Hủy
                </button>
                <button
                  type="button"
                  onClick={handleSaveManual}
                  disabled={!isManualComplete || isLoading}
                  className={`
                    px-5 py-2 rounded-xl text-xs sm:text-sm font-bold text-white transition-all flex items-center gap-2
                    ${
                      isManualComplete && !isLoading
                        ? "bg-gradient-to-r from-rose-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 shadow-md shadow-rose-600/20 active:scale-95"
                        : "bg-muted text-muted-foreground cursor-not-allowed opacity-60"
                    }
                  `}
                >
                  {isLoading ? (
                    <>
                      <Loader2 className="w-4 h-4 animate-spin" />
                      <span>Đang kiểm tra...</span>
                    </>
                  ) : (
                    <>
                      <CheckCircle2 className="w-4 h-4" />
                      <span>Hoàn tất dòng {line.lineLabel}</span>
                    </>
                  )}
                </button>
              </div>
            </>
          ) : (
            <>
              <button
                type="button"
                onClick={() => handleGenerateThanTai(selectedStrategy)}
                disabled={isLoading}
                className="text-xs font-bold text-rose-500 hover:text-rose-600 flex items-center gap-1 px-2.5 py-1.5 rounded-lg hover:bg-rose-500/10 transition-colors"
              >
                <RotateCcw className={`w-3.5 h-3.5 ${isLoading ? "animate-spin" : ""}`} />
                <span>Sinh số khác</span>
              </button>

              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-4 py-2 rounded-xl text-xs sm:text-sm font-semibold border border-border hover:bg-muted text-foreground transition-colors"
                >
                  Hủy
                </button>
                <button
                  type="button"
                  onClick={handleApplyThanTai}
                  disabled={!generatedResult || isLoading}
                  className={`
                    px-5 py-2 rounded-xl text-xs sm:text-sm font-bold text-white transition-all flex items-center gap-2
                    ${
                      generatedResult && !isLoading
                        ? "bg-gradient-to-r from-rose-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 shadow-md shadow-rose-600/20 active:scale-95"
                        : "bg-muted text-muted-foreground cursor-not-allowed opacity-60"
                    }
                  `}
                >
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Áp dụng vào dòng {line.lineLabel}</span>
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
