"use client";

import React, { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { Game } from "@/types/game";
import { fetchGameByCode, fetchActiveGames } from "@/lib/api";
import { useSlipStore } from "@/stores/useSlipStore";
import { SlipBoard } from "@/components/slip/SlipBoard";
import { ArrowLeft, Loader2, AlertCircle } from "lucide-react";

export default function PlayGamePage() {
  const params = useParams();
  const router = useRouter();
  const gameCode = typeof params.gameCode === "string" ? params.gameCode : "";

  const [game, setGame] = useState<Game | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const initSlipForGame = useSlipStore((state) => state.initSlipForGame);

  useEffect(() => {
    let isMounted = true;

    async function loadGame() {
      if (!gameCode) return;
      setIsLoading(true);
      setError(null);

      try {
        let loadedGame = await fetchGameByCode(gameCode);

        // Fallback: search in active games list
        if (!loadedGame) {
          const allGames = await fetchActiveGames();
          loadedGame = allGames.find(
            (g) => g.code.toUpperCase() === gameCode.toUpperCase()
          ) || null;
        }

        if (!isMounted) return;

        if (loadedGame) {
          setGame(loadedGame);
          initSlipForGame(loadedGame);
        } else {
          setError(`Không tìm thấy trò chơi có mã "${gameCode}".`);
        }
      } catch (err: unknown) {
        if (!isMounted) return;
        const msg = err instanceof Error ? err.message : "Lỗi khi tải thông tin trò chơi";
        setError(msg);
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadGame();

    return () => {
      isMounted = false;
    };
  }, [gameCode, initSlipForGame]);

  return (
    <div className="container max-w-5xl mx-auto px-4 py-6 sm:py-8 space-y-6">
      {/* Back Navigation Bar */}
      <div className="flex items-center justify-between">
        <Link
          href="/"
          className="inline-flex items-center gap-2 text-xs sm:text-sm font-bold text-muted-foreground hover:text-foreground transition-colors group"
        >
          <ArrowLeft className="w-4 h-4 transition-transform group-hover:-translate-x-1" />
          <span>Về trang danh mục trò chơi</span>
        </Link>
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="flex flex-col items-center justify-center py-24 space-y-4 text-center">
          <Loader2 className="w-8 h-8 text-rose-500 animate-spin" />
          <p className="text-sm font-semibold text-muted-foreground">
            Đang tải thông tin trò chơi và phiếu số...
          </p>
        </div>
      )}

      {/* Error State */}
      {!isLoading && error && (
        <div className="p-6 rounded-3xl bg-destructive/10 border border-destructive/30 text-destructive text-center space-y-4 max-w-md mx-auto">
          <AlertCircle className="w-8 h-8 mx-auto" />
          <h3 className="font-extrabold text-base">{error}</h3>
          <button
            type="button"
            onClick={() => router.push("/")}
            className="px-4 py-2 rounded-xl bg-destructive text-destructive-foreground font-bold text-xs shadow hover:opacity-90 transition-opacity"
          >
            Quay lại trang chủ
          </button>
        </div>
      )}

      {/* Main Slip Board */}
      {!isLoading && game && <SlipBoard game={game} />}
    </div>
  );
}
