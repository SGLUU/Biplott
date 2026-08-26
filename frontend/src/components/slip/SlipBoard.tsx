"use client";

import React from "react";
import { Game } from "@/types/game";
import { useSlipStore } from "@/stores/useSlipStore";
import { useLuckyJourneyStore } from "@/stores/useLuckyJourneyStore";
import { useMixedBuilderStore } from "@/stores/useMixedBuilderStore";
import { SlipLineRow } from "./SlipLineRow";
import { LineEditorModal } from "./LineEditorModal";
import { BulkGenerateModal } from "./BulkGenerateModal";
import { LuckyJourneyModal } from "@/components/lucky/LuckyJourneyModal";
import { MixedBuilderModal } from "@/components/mixed/MixedBuilderModal";
import { generateThanTaiLine } from "@/lib/api";
import {
  Sparkles,
  RotateCcw,
  Ticket,
  Calendar,
  Layers,
  HelpCircle,
  Share2
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

  const openLuckyJourney = useLuckyJourneyStore((state) => state.openJourney);
  const openMixedBuilder = useMixedBuilderStore((state) => state.openBuilder);

  const [editorInitialTab, setEditorInitialTab] = React.useState<"manual" | "thantai">("manual");
  const [copied, setCopied] = React.useState(false);

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

  return (
    <div className="w-full max-w-3xl mx-auto space-y-6">
      {/* Top Controls Bar */}
      <div className="flex items-center justify-between gap-2 flex-wrap">
        <div className="flex items-center gap-2">
          <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-primary/10 text-primary border border-primary/20 text-xs font-bold">
            <Layers className="w-3.5 h-3.5" />
            Đã điền {completedCount} / 6 dòng
          </span>
          {completedCount > 0 && (
            <span className="text-xs text-muted-foreground hidden sm:inline">
              (Có thể in hoặc chia sẻ vé ngay)
            </span>
          )}
        </div>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={openBulkModal}
            className="inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-gradient-to-r from-rose-600 to-amber-600 hover:from-rose-500 hover:to-amber-500 text-white font-bold text-xs sm:text-sm shadow-md shadow-rose-600/20 active:scale-95 transition-all"
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
              <Share2 className="w-3.5 h-3.5 text-rose-500" />
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
    </div>
  );
}
