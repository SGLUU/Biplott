"use client";

import { useEffect } from "react";
import { useGameStore } from "@/stores/useGameStore";
import { HeroSection } from "@/components/game/HeroSection";
import { GameCard } from "@/components/game/GameCard";
import { Dices, RefreshCw, AlertTriangle, CheckCircle2 } from "lucide-react";

export default function HomePage() {
  const { games, isLoading, error, loadGames, selectedGame, setSelectedGame } = useGameStore();

  useEffect(() => {
    loadGames();
  }, [loadGames]);

  return (
    <div className="flex flex-col items-center gap-10">
      {/* Hero Section */}
      <HeroSection />

      {/* Game Selection Arena */}
      <section className="w-full">
        <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4 mb-6 border-b border-zinc-200 dark:border-zinc-800 pb-4">
          <div>
            <div className="flex items-center gap-2 text-rose-600 dark:text-rose-400 font-bold text-xs uppercase tracking-widest mb-1">
              <Dices className="w-4 h-4" />
              <span>Chọn thể thức thử vận may</span>
            </div>
            <h2 className="text-2xl sm:text-3xl font-extrabold text-zinc-900 dark:text-zinc-50 tracking-tight">
              Danh mục trò chơi
            </h2>
          </div>

          {/* Backend Status Indicator */}
          <div className="flex items-center gap-2 self-start sm:self-auto">
            {isLoading ? (
              <span className="inline-flex items-center gap-1.5 text-xs text-zinc-500">
                <RefreshCw className="w-3.5 h-3.5 animate-spin text-orange-500" />
                Đang nạp dữ liệu từ Backend...
              </span>
            ) : error ? (
              <button
                onClick={loadGames}
                className="inline-flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 rounded-xl bg-rose-50 text-rose-700 dark:bg-rose-950/60 dark:text-rose-300 border border-rose-200 dark:border-rose-800 hover:bg-rose-100 transition-colors"
              >
                <AlertTriangle className="w-3.5 h-3.5" />
                <span>Thử kết nối lại</span>
              </button>
            ) : (
              <span className="inline-flex items-center gap-1.5 text-xs font-medium text-emerald-600 dark:text-emerald-400 px-2.5 py-1 rounded-full bg-emerald-50 dark:bg-emerald-950/50 border border-emerald-200 dark:border-emerald-800">
                <CheckCircle2 className="w-3.5 h-3.5" />
                <span>Backend API Online ({games.length} games)</span>
              </span>
            )}
          </div>
        </div>

        {/* Loading State Skeleton */}
        {isLoading && games.length === 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="p-6 rounded-3xl bg-zinc-100 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 animate-pulse space-y-4 h-80"
              >
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-2xl bg-zinc-200 dark:bg-zinc-800" />
                  <div className="space-y-1.5 flex-1">
                    <div className="h-4 bg-zinc-200 dark:bg-zinc-800 rounded w-1/2" />
                    <div className="h-3 bg-zinc-200 dark:bg-zinc-800 rounded w-3/4" />
                  </div>
                </div>
                <div className="h-12 bg-zinc-200 dark:bg-zinc-800 rounded-xl" />
                <div className="h-20 bg-zinc-200 dark:bg-zinc-800 rounded-xl" />
              </div>
            ))}
          </div>
        )}

        {/* Error State Banner */}
        {error && games.length === 0 && (
          <div className="p-8 rounded-3xl bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800/50 text-center flex flex-col items-center gap-3 max-w-lg mx-auto">
            <AlertTriangle className="w-8 h-8 text-rose-500" />
            <h3 className="font-bold text-rose-900 dark:text-rose-200">Không thể kết nối đến Backend API</h3>
            <p className="text-xs text-rose-700 dark:text-rose-300">
              Vui lòng đảm bảo backend ASP.NET Core đang chạy tại <code>http://localhost:5000</code> hoặc kiểm tra Docker.
            </p>
            <button
              onClick={loadGames}
              className="mt-2 px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold shadow-md transition-colors"
            >
              Thử lại ngay
            </button>
          </div>
        )}

        {/* Game Cards Grid */}
        {games.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {games.map((game) => (
              <GameCard
                key={game.id || game.code}
                game={game}
                isSelected={selectedGame?.code === game.code}
                onSelect={(g) => setSelectedGame(g)}
              />
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
