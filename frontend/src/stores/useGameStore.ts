import { create } from "zustand";
import { Game } from "@/types/game";
import { fetchActiveGames } from "@/lib/api";

interface GameState {
  games: Game[];
  selectedGame: Game | null;
  isLoading: boolean;
  error: string | null;
  loadGames: () => Promise<void>;
  setSelectedGame: (game: Game) => void;
}

export const useGameStore = create<GameState>((set) => ({
  games: [],
  selectedGame: null,
  isLoading: false,
  error: null,
  loadGames: async () => {
    set({ isLoading: true, error: null });
    try {
      const data = await fetchActiveGames();
      set({ games: data, selectedGame: data[0] || null, isLoading: false });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : "Không thể kết nối đến máy chủ";
      set({ error: message, isLoading: false });
    }
  },
  setSelectedGame: (game: Game) => set({ selectedGame: game }),
}));
