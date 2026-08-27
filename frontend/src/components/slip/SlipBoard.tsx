"use client";

import React, { useState } from "react";
import Link from "next/link";
import { Game } from "@/types/game";
import { useSlipStore } from "@/stores/useSlipStore";
import { useLuckyJourneyStore } from "@/stores/useLuckyJourneyStore";
import { useMixedBuilderStore } from "@/stores/useMixedBuilderStore";
import { useAuthStore } from "@/stores/useAuthStore";
import { SlipLineRow } from "./SlipLineRow";
import { LineEditorModal } from "./LineEditorModal";
import { BulkGenerateModal } from "./BulkGenerateModal";
import { LuckyJourneyModal } from "@/components/lucky/LuckyJourneyModal";
import { MixedBuilderModal } from "@/components/mixed/MixedBuilderModal";
import { SaveSlipPromptModal } from "./SaveSlipPromptModal";
import { useDailyJourneyStore } from "@/stores/useDailyJourneyStore";
import { DailyJourneyModal } from "@/components/lucky/DailyJourneyModal";
import { useLuckyRemixStore } from "@/stores/useLuckyRemixStore";
import { LuckyRemixModal } from "@/components/lucky/LuckyRemixModal";
import { generateThanTaiLine, apiSaveSlip, apiGetTodayDailyJourney, apiQuickRemix } from "@/lib/api";
import { getOrCreateGuestSessionToken } from "@/lib/utils";
import {
  Sparkles,
  RotateCcw,
  Ticket,
  Calendar,
  Layers,
  HelpCircle,
  Share2,
  Heart,
  CheckCircle2,
  AlertCircle,
  Play,
  Eye,
  ArrowRight
} from "lucide-react";

interface SlipBoardProps {
  game: Game;
}

export function SlipBoard({ game }: SlipBoardProps) {
  const {
    slip,
    activeLineLabel,
    isLineEditorOpen,
    isBulkModalOpen,
    openLineEditor,
    closeLineEditor,
    openBulkModal,
    closeBulkModal,
    setLineNumbers,
    resetLine,
    clearSlip,
    applyBulkLines
  } = useSlipStore();

  const { isAuthenticated } = useAuthStore();
  const openLuckyJourney = useLuckyJourneyStore((state) => state.openJourney);
  const openMixedBuilder = useMixedBuilderStore((state) => state.openBuilder);

  const [editorInitialTab, setEditorInitialTab] = useState<"manual" | "thantai">("manual");
  const [copied, setCopied] = useState(false);
  const [isSavePromptOpen, setIsSavePromptOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [saveStatus, setSaveStatus] = useState<{ type: "success" | "error"; message: string; slipId?: string } | null>(null);

  const openDailyJourney = useDailyJourneyStore((state) => state.openJourney);
  const isDailyOpen = useDailyJourneyStore((state) => state.isOpen);
  const [dailyStatus, setDailyStatus] = useState<"NotStarted" | "InProgress" | "Completed">("NotStarted");
  const [loadingDaily, setLoadingDaily] = useState(true);

  const fetchDaily = React.useCallback(async () => {
    try {
      setLoadingDaily(true);
      const guestToken = getOrCreateGuestSessionToken();
      const res = await apiGetTodayDailyJourney(game.code, guestToken);
      if (res) {
        setDailyStatus(res.status);
      } else {
        setDailyStatus("NotStarted");
      }
    } catch (err: unknown) {
      console.error(err);
    } finally {
      setLoadingDaily(false);
    }
  }, [game.code]);

  React.useEffect(() => {
    fetchDaily();
  }, [fetchDaily, isDailyOpen]);

  const activeLine = slip.lines.find(
    (l) => l.lineLabel.toUpperCase() === activeLineLabel?.toUpperCase()
  ) || slip.lines[0];

  const completedCount = slip.lines.filter((l) => l.status === "Complete").length;

  const handleOpenEditorForLine = (lineLabel: string, initialTab: "manual" | "thantai" = "manual") => {
    setEditorInitialTab(initialTab);
    openLineEditor(lineLabel);
  };

  const handleOpenLuckyForLine = (lineLabel: string) => {
    openLuckyJourney(game, lineLabel);
  };

  const handleOpenMixedForLine = (lineLabel: string) => {
    const targetLine = slip.lines.find(
      (l) => l.lineLabel.toUpperCase() === lineLabel.toUpperCase()
    );
    openMixedBuilder(game, lineLabel, targetLine?.numbers);
  };

  const handleQuickThanTaiForLine = async (lineLabel: string) => {
    try {
      const res = await generateThanTaiLine({
        gameCode: game.code,
        strategy: "PureRandom"
      });

      setLineNumbers(
        lineLabel,
        res.numbers,
        "Complete",
        res.strategy,
        res.commentary
      );
    } catch (err) {
      console.error("Lỗi sinh số Thần Tài nhanh:", err);
    }
  };

  const handleQuickRemixForLine = async (lineLabel: string) => {
    const targetLine = slip.lines.find(
      (l) => l.lineLabel.toUpperCase() === lineLabel.toUpperCase()
    );
    if (!targetLine) return;

    try {
      const res = await apiQuickRemix({
        gameCode: game.code,
        currentNumbers: targetLine.numbers.map((n) => ({
          value: n.value,
          poolIndex: n.poolIndex,
          source: n.source,
          metadataJson: n.metadataJson,
          isLocked: n.isLocked
        }))
      });

      setLineNumbers(
        lineLabel,
        res.numbers,
        "Complete",
        res.strategy,
        res.commentary
      );
    } catch (err: unknown) {
      console.error("Lỗi Quick Remix:", err);
      const msg = err instanceof Error ? err.message : "Lỗi khi thực hiện Quick Remix.";
      alert(msg);
    }
  };

  const handleLuckyRemixForLine = async (lineLabel: string) => {
    const targetLine = slip.lines.find(
      (l) => l.lineLabel.toUpperCase() === lineLabel.toUpperCase()
    );
    if (!targetLine) return;

    try {
      await useLuckyRemixStore.getState().openRemix(game, lineLabel, targetLine.numbers);
    } catch (err: unknown) {
      console.error("Lỗi Lucky Remix:", err);
    }
  };

  const handleShareSlip = () => {
    const textToCopy = `Bịp lót Ticket [${game.name}] - Mã: ${slip.slipCode}\n` +
      slip.lines
        .filter((l) => l.status === "Complete")
        .map((l) => `${l.lineLabel}: ${l.numbers.map((n) => n.formatted).join(" ")}`)
        .join("\n") +
      `\nCơ hội để nát hơn cùng biplot.vn!`;

    navigator.clipboard.writeText(textToCopy);
    setCopied(true);
    setTimeout(() => setCopied(false), 2500);
  };

  const handleSaveSlipClick = async () => {
    if (!isAuthenticated) {
      setIsSavePromptOpen(true);
      return;
    }

    const completedLines = slip.lines.filter(
      (l) => l.status === "Complete" && l.numbers.length > 0
    );

    if (completedLines.length === 0) {
      setSaveStatus({
        type: "error",
        message: "Bạn chưa hoàn thành dòng nào để lưu. Hãy tạo ít nhất 1 dòng số!"
      });
      setTimeout(() => setSaveStatus(null), 3500);
      return;
    }

    try {
      setIsSaving(true);
      const saved = await apiSaveSlip({
        gameCode: game.code,
        slipCode: slip.slipCode,
        title: `Vé ${game.name} - ${slip.slipCode}`,
        lines: completedLines.map((l) => ({
          lineLabel: l.lineLabel,
          numbers: l.numbers.map((n) => ({
            value: n.value,
            poolIndex: n.poolIndex,
            source: n.source,
            metadataJson: n.metadataJson
          }))
        }))
      });

      setSaveStatus({
        type: "success",
        message: `Đã lưu thành công ${completedLines.length} dòng vào Phiếu của tôi!`,
        slipId: saved.id
      });
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : "Không thể lưu vé lúc này.";
      setSaveStatus({
        type: "error",
        message: errorMsg
      });
    } finally {
      setIsSaving(false);
      setTimeout(() => setSaveStatus(null), 5000);
    }
  };

  return (
    <div className="w-full max-w-3xl mx-auto space-y-6">
      {/* Save Notification Toast */}
      {saveStatus && (
        <div
          className={`flex items-center justify-between gap-3 p-4 rounded-2xl border animate-in slide-in-from-top-2 duration-200 ${
            saveStatus.type === "success"
              ? "bg-emerald-50 dark:bg-emerald-950/40 border-emerald-300 dark:border-emerald-800/60 text-emerald-800 dark:text-emerald-200"
              : "bg-red-50 dark:bg-red-950/40 border-red-300 dark:border-red-800/60 text-red-800 dark:text-red-200"
          }`}
        >
          <div className="flex items-center gap-2.5 text-xs font-semibold">
            {saveStatus.type === "success" ? (
              <CheckCircle2 className="w-5 h-5 text-emerald-500 flex-shrink-0" />
            ) : (
              <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
            )}
            <span>{saveStatus.message}</span>
          </div>

          {saveStatus.type === "success" && (
            <Link
              href="/my/slips"
              className="px-3 py-1 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-bold transition-colors whitespace-nowrap"
            >
              Xem danh sách
            </Link>
          )}
        </div>
      )}

      {/* Daily Journey Banner */}
      {!loadingDaily && (
        <div className="p-4 sm:p-5 rounded-3xl bg-gradient-to-r from-rose-600/10 via-orange-500/10 to-amber-500/10 border border-orange-500/20 shadow-sm flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div className="space-y-1">
            <div className="flex items-center gap-2">
              <Sparkles className="w-4 h-4 text-orange-500" />
              <h4 className="text-sm font-black text-foreground">Số Phận Hôm Nay (Daily Journey)</h4>
              {dailyStatus === "Completed" ? (
                <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20">
                  Đã hoàn thành
                </span>
              ) : dailyStatus === "InProgress" ? (
                <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-amber-500/10 text-amber-600 dark:text-amber-400 border border-amber-500/20 animate-pulse">
                  Đang chơi
                </span>
              ) : (
                <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-zinc-500/10 text-zinc-500 border border-zinc-500/20">
                  Chưa chơi
                </span>
              )}
            </div>
            <p className="text-xs text-muted-foreground leading-relaxed">
              {dailyStatus === "Completed"
                ? "Bạn đã giải mã bộ số định mệnh ngày hôm nay. Áp dụng ngay vào dòng D để mua vé!"
                : "Hành trình tâm linh duy nhất trong ngày giúp bạn chọn ra bộ số độc nhất."}
            </p>
          </div>

          <button
            type="button"
            onClick={() => openDailyJourney(game)}
            className="inline-flex items-center gap-1.5 px-4.5 py-2.5 rounded-xl bg-zinc-950 hover:bg-zinc-900 text-white dark:bg-zinc-800 dark:hover:bg-zinc-700 text-xs font-bold transition-all shadow shrink-0 active:scale-95"
          >
            {dailyStatus === "Completed" ? (
              <>
                <Eye className="w-3.5 h-3.5" />
                <span>Xem lại hành trình</span>
              </>
            ) : dailyStatus === "InProgress" ? (
              <>
                <Play className="w-3.5 h-3.5 fill-current" />
                <span>Chơi tiếp</span>
              </>
            ) : (
              <>
                <Sparkles className="w-3.5 h-3.5 text-amber-400 animate-pulse" />
                <span>Bắt đầu chơi</span>
              </>
            )}
            <ArrowRight className="w-3.5 h-3.5" />
          </button>
        </div>
      )}

      {/* Top Controls Bar */}
      <div className="flex items-center justify-between gap-2 flex-wrap">
        <div className="flex items-center gap-2">
          <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-primary/10 text-primary border border-primary/20 text-xs font-bold">
            <Layers className="w-3.5 h-3.5" />
            Đã điền {completedCount} / 6 dòng
          </span>
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          {/* Save Slip Action */}
          <button
            type="button"
            disabled={isSaving || completedCount === 0}
            onClick={handleSaveSlipClick}
            className={`inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl text-xs sm:text-sm font-bold shadow-md transition-all ${
              completedCount > 0
                ? "bg-rose-600 hover:bg-rose-500 text-white shadow-rose-600/25 active:scale-95 cursor-pointer"
                : "bg-zinc-200 dark:bg-zinc-800 text-zinc-400 cursor-not-allowed opacity-60"
            }`}
          >
            <Heart className={`w-4 h-4 ${isSaving ? "animate-pulse" : "fill-current"}`} />
            <span>{isSaving ? "Đang lưu..." : "Lưu phiếu"}</span>
          </button>

          <button
            type="button"
            onClick={openBulkModal}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-gradient-to-r from-orange-600 to-amber-600 hover:from-orange-500 hover:to-amber-500 text-white font-bold text-xs sm:text-sm shadow-md shadow-orange-600/20 active:scale-95 transition-all"
          >
            <Sparkles className="w-4 h-4" />
            <span>Thần Tài cả phiếu</span>
          </button>

          <button
            type="button"
            onClick={clearSlip}
            className="inline-flex items-center gap-1 px-3 py-2 rounded-xl border border-border hover:bg-muted text-muted-foreground hover:text-foreground text-xs font-medium transition-colors"
            title="Làm mới toàn bộ phiếu"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">Xóa trắng</span>
          </button>

          {completedCount > 0 && (
            <button
              type="button"
              onClick={handleShareSlip}
              className="inline-flex items-center gap-1 px-3 py-2 rounded-xl border border-border hover:bg-muted text-muted-foreground hover:text-foreground text-xs font-medium transition-colors"
              title="Sao chép phiếu số"
            >
              <Share2 className="w-3.5 h-3.5 text-orange-500" />
              <span>{copied ? "Đã chép!" : "Chia sẻ"}</span>
            </button>
          )}
        </div>
      </div>

      {/* Ticket Board Container */}
      <div className="relative rounded-3xl bg-card border border-border/80 shadow-xl overflow-hidden">
        {/* Ticket Top Decorative Pattern */}
        <div className="h-3 bg-gradient-to-r from-rose-600 via-orange-500 to-amber-500"></div>

        {/* Ticket Header */}
        <div className="px-5 sm:px-8 py-5 border-b border-dashed border-border/80 bg-muted/20 flex items-center justify-between flex-wrap gap-3">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-10 h-10 rounded-2xl bg-gradient-to-br from-rose-500 to-amber-500 text-white shadow-md shadow-rose-500/20">
              <Ticket className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h2 className="text-lg sm:text-xl font-black text-foreground tracking-tight">
                  {game.name}
                </h2>
                <span className="text-[10px] font-extrabold uppercase px-2 py-0.5 rounded-full bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-500/20">
                  {game.code}
                </span>
              </div>
              <p className="text-xs text-muted-foreground">{game.tagline || game.description}</p>
            </div>
          </div>

          <div className="text-right flex flex-col items-end gap-0.5">
            <span className="font-mono font-black text-xs sm:text-sm text-foreground bg-muted px-2.5 py-1 rounded-lg border border-border">
              {slip.slipCode}
            </span>
            <span className="text-[11px] text-muted-foreground flex items-center gap-1">
              <Calendar className="w-3 h-3" />
              {new Date().toLocaleDateString("vi-VN")}
            </span>
          </div>
        </div>

        {/* 6 Lines Container (A, B, C, D, E, F) */}
        <div className="p-4 sm:p-6 space-y-3">
          {slip.lines.map((line) => (
            <SlipLineRow
              key={line.lineLabel}
              line={line}
              game={game}
              onOpenEditor={(tab) => handleOpenEditorForLine(line.lineLabel, tab)}
              onOpenLucky={() => handleOpenLuckyForLine(line.lineLabel)}
              onOpenMixed={() => handleOpenMixedForLine(line.lineLabel)}
              onQuickThanTai={() => handleQuickThanTaiForLine(line.lineLabel)}
              onQuickRemix={() => handleQuickRemixForLine(line.lineLabel)}
              onLuckyRemix={() => handleLuckyRemixForLine(line.lineLabel)}
              onReset={() => resetLine(line.lineLabel)}
            />
          ))}
        </div>

        {/* Ticket Footer */}
        <div className="px-5 sm:px-8 py-4 border-t border-dashed border-border/80 bg-muted/30 flex items-center justify-between text-xs text-muted-foreground flex-wrap gap-2">
          <div className="flex items-center gap-1.5 font-medium italic">
            <HelpCircle className="w-3.5 h-3.5 text-amber-500" />
            <span>Tagline: &quot;Cơ hội để nát hơn cùng số phận&quot;</span>
          </div>

          <span className="text-[11px]">
            Phiếu tạo giải trí độc lập • Không cam kết trúng thưởng
          </span>
        </div>
      </div>

      {/* Save Slip Prompt Modal for Guests */}
      <SaveSlipPromptModal
        isOpen={isSavePromptOpen}
        gameCode={game.code}
        onClose={() => setIsSavePromptOpen(false)}
      />

      {/* Line Editor Modal (Manual / Than Tai tabs) */}
      {isLineEditorOpen && activeLine && (
        <LineEditorModal
          isOpen={isLineEditorOpen}
          onClose={closeLineEditor}
          game={game}
          line={activeLine}
          initialTab={editorInitialTab}
          onSaveLine={(numbers, strategy, commentary) => {
            setLineNumbers(
              activeLine.lineLabel,
              numbers,
              "Complete",
              strategy,
              commentary
            );
          }}
        />
      )}

      {/* Lucky Journey Modal */}
      <LuckyJourneyModal
        game={game}
        onSaveToSlipLine={(lineLabel, numbers, commentary) => {
          setLineNumbers(lineLabel, numbers, "Complete", undefined, commentary);
        }}
      />

      {/* Mixed Builder Modal */}
      <MixedBuilderModal
        game={game}
        onSaveToSlipLine={(lineLabel, numbers) => {
          setLineNumbers(lineLabel, numbers, "Complete");
        }}
      />

      {/* Bulk Generate Modal */}
      {isBulkModalOpen && (
        <BulkGenerateModal
          isOpen={isBulkModalOpen}
          onClose={closeBulkModal}
          game={game}
          slip={slip}
          onSuccess={(lines) => {
            applyBulkLines(lines);
          }}
        />
      )}

      {/* Daily Journey Modal */}
      <DailyJourneyModal
        game={game}
        onSaveToSlipLine={(lineLabel, numbers, commentary) => {
          setLineNumbers(lineLabel, numbers, "Complete", undefined, commentary);
        }}
      />

      {/* Lucky Remix Modal */}
      <LuckyRemixModal
        game={game}
        onSaveToSlipLine={(lineLabel, numbers, commentary) => {
          setLineNumbers(lineLabel, numbers, "Complete", undefined, commentary);
        }}
      />
    </div>
  );
}
