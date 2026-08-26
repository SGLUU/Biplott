import { Game } from "@/types/game";
import { ArrowRight, Sparkles, Layers } from "lucide-react";

interface GameCardProps {
  game: Game;
  isSelected?: boolean;
  onSelect?: (game: Game) => void;
}

export function GameCard({ game, isSelected, onSelect }: GameCardProps) {
  // Determine gradient style based on Game Code
  const isPower = game.code.includes("655");
  const isMega = game.code.includes("645");
  const isLotto = game.code.includes("535");

  const badgeGradient = isPower
    ? "from-rose-600 to-rose-500 text-white"
    : isMega
    ? "from-orange-500 to-amber-500 text-white"
    : "from-amber-500 to-yellow-400 text-zinc-950";

  const cardBorder = isSelected
    ? "ring-2 ring-rose-500 border-rose-500 dark:border-rose-400 shadow-xl shadow-rose-500/10"
    : "border-zinc-200/80 dark:border-zinc-800/80 hover:border-zinc-300 dark:hover:border-zinc-700 shadow-sm hover:shadow-md";

  return (
    <div
      onClick={() => onSelect?.(game)}
      className={`relative flex flex-col justify-between p-6 rounded-3xl bg-white dark:bg-zinc-900/90 border transition-all duration-300 cursor-pointer group ${cardBorder}`}
    >
      <div>
        {/* Top Header & Badge */}
        <div className="flex items-center justify-between gap-3 mb-4">
          <div className="flex items-center gap-2.5">
            <div className={`w-10 h-10 rounded-2xl bg-gradient-to-br ${badgeGradient} flex items-center justify-center font-black text-sm shadow-md`}>
              {game.code.split("_")[1] || "BIP"}
            </div>
            <div>
              <h3 className="font-extrabold text-lg text-zinc-900 dark:text-zinc-50 group-hover:text-rose-600 dark:group-hover:text-rose-400 transition-colors">
                {game.name}
              </h3>
              {game.tagline && (
                <p className="text-xs font-medium text-zinc-500 dark:text-zinc-400">
                  {game.tagline}
                </p>
              )}
            </div>
          </div>

          {isLotto && (
            <span className="text-[11px] font-bold px-2 py-0.5 rounded-full bg-amber-50 dark:bg-amber-950/60 text-amber-700 dark:text-amber-300 border border-amber-200 dark:border-amber-800/60">
              Đa tập số (2 Pools)
            </span>
          )}
        </div>

        {/* Description */}
        <p className="text-sm text-zinc-600 dark:text-zinc-300 mb-5 leading-relaxed">
          {game.description}
        </p>

        {/* Pools Breakdown */}
        <div className="space-y-2 mb-6">
          <div className="text-[11px] font-bold uppercase tracking-wider text-zinc-600 dark:text-zinc-400 flex items-center gap-1.5">
            <Layers className="w-3.5 h-3.5" />
            <span>Quy tắc tập số (Multi-Pool Rule):</span>
          </div>

          <div className="grid grid-cols-1 gap-2">
            {game.pools?.map((pool) => (
              <div
                key={pool.id || pool.poolIndex}
                className="flex items-center justify-between p-2.5 rounded-xl bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-100 dark:border-zinc-800 text-xs"
              >
                <div className="flex items-center gap-2">
                  <div
                    className="w-2.5 h-2.5 rounded-full shrink-0"
                    style={{ backgroundColor: pool.badgeColor || (pool.poolIndex === 0 ? "#EF4444" : "#FACC15") }}
                  />
                  <span className="font-semibold text-zinc-800 dark:text-zinc-200">
                    {pool.name}
                  </span>
                </div>
                <div className="flex items-center gap-2 font-mono text-[11px] font-medium text-zinc-600 dark:text-zinc-300">
                  <span className="px-1.5 py-0.5 rounded bg-zinc-200/70 dark:bg-zinc-700/70">
                    Chọn {pool.pickCount} số
                  </span>
                  <span>
                    ({pool.minNumber < 10 ? `0${pool.minNumber}` : pool.minNumber} - {pool.maxNumber < 10 ? `0${pool.maxNumber}` : pool.maxNumber})
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* CTA Button */}
      <button
        className="w-full mt-auto flex items-center justify-center gap-2 py-3 px-4 rounded-2xl bg-zinc-900 hover:bg-rose-600 text-white dark:bg-zinc-800 dark:hover:bg-rose-600 font-semibold text-sm transition-all duration-300 shadow-md group-hover:translate-x-0.5"
      >
        <Sparkles className="w-4 h-4 text-amber-400" />
        <span>Tạo số ngay</span>
        <ArrowRight className="w-4 h-4 text-zinc-400 group-hover:text-white transition-colors" />
      </button>
    </div>
  );
}
