import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import { Game } from "@/types/game";
import { Slip, SlipLine, SlipNumber, SlipLineStatus, RandomStrategy } from "@/types/slip";

const STANDARD_LINE_LABELS = ["A", "B", "C", "D", "E", "F"];

function createEmptyLines(): SlipLine[] {
  return STANDARD_LINE_LABELS.map((label) => ({
    lineLabel: label,
    status: "Empty" as SlipLineStatus,
    numbers: []
  }));
}

function generateSlipCode(): string {
  const randomNum = Math.floor(10000 + Math.random() * 90000);
  return `BIP-${randomNum}`;
}

interface SlipState {
  game: Game | null;
  slip: Slip;
  activeLineLabel: string | null;
  isLineEditorOpen: boolean;
  isBulkModalOpen: boolean;
  isLoading: boolean;
  error: string | null;

  initSlipForGame: (game: Game) => void;
  setLineNumbers: (
    lineLabel: string,
    numbers: SlipNumber[],
    status?: SlipLineStatus,
    strategy?: RandomStrategy,
    commentary?: string
  ) => void;
  resetLine: (lineLabel: string) => void;
  clearSlip: () => void;
  openLineEditor: (lineLabel: string) => void;
  closeLineEditor: () => void;
  openBulkModal: () => void;
  closeBulkModal: () => void;
  applyBulkLines: (lines: SlipLine[]) => void;
  setError: (error: string | null) => void;
  setLoading: (isLoading: boolean) => void;
}

export const useSlipStore = create<SlipState>()(
  persist(
    (set, get) => ({
      game: null,
      slip: {
        gameCode: "",
        slipCode: generateSlipCode(),
        lines: createEmptyLines(),
        createdAt: new Date().toISOString()
      },
      activeLineLabel: null,
      isLineEditorOpen: false,
      isBulkModalOpen: false,
      isLoading: false,
      error: null,

      initSlipForGame: (game: Game) => {
        const currentSlip = get().slip;
        if (currentSlip.gameCode === game.code && currentSlip.lines.length === 6) {
          // Same game, preserve current draft
          set({ game });
          return;
        }

        // New game or empty, initialize clean slip
        set({
          game,
          slip: {
            gameCode: game.code,
            slipCode: generateSlipCode(),
            lines: createEmptyLines(),
            createdAt: new Date().toISOString()
          },
          error: null
        });
      },

      setLineNumbers: (lineLabel, numbers, status = "Complete", strategy, commentary) => {
        set((state) => {
          const updatedLines = state.slip.lines.map((line) => {
            if (line.lineLabel.toUpperCase() === lineLabel.toUpperCase()) {
              return {
                ...line,
                numbers,
                status,
                strategy: strategy || line.strategy,
                commentary: commentary || line.commentary
              };
            }
            return line;
          });

          return {
            slip: {
              ...state.slip,
              lines: updatedLines
            }
          };
        });
      },

      resetLine: (lineLabel) => {
        set((state) => {
          const updatedLines = state.slip.lines.map((line) => {
            if (line.lineLabel.toUpperCase() === lineLabel.toUpperCase()) {
              return {
                lineLabel: line.lineLabel,
                status: "Empty" as SlipLineStatus,
                numbers: [],
                strategy: undefined,
                commentary: undefined
              };
            }
            return line;
          });

          return {
            slip: {
              ...state.slip,
              lines: updatedLines
            }
          };
        });
      },

      clearSlip: () => {
        set((state) => ({
          slip: {
            gameCode: state.game?.code || "",
            slipCode: generateSlipCode(),
            lines: createEmptyLines(),
            createdAt: new Date().toISOString()
          }
        }));
      },

      openLineEditor: (lineLabel) => {
        set({ activeLineLabel: lineLabel, isLineEditorOpen: true });
      },

      closeLineEditor: () => {
        set({ isLineEditorOpen: false, activeLineLabel: null });
      },

      openBulkModal: () => {
        set({ isBulkModalOpen: true });
      },

      closeBulkModal: () => {
        set({ isBulkModalOpen: false });
      },

      applyBulkLines: (lines) => {
        set((state) => ({
          slip: {
            ...state.slip,
            lines
          },
          isBulkModalOpen: false
        }));
      },

      setError: (error) => set({ error }),
      setLoading: (isLoading) => set({ isLoading })
    }),
    {
      name: "biplott_slip_storage",
      storage: createJSONStorage(() => sessionStorage),
      partialize: (state) => ({
        game: state.game,
        slip: state.slip
      })
    }
  )
);
