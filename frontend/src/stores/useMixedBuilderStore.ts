import { create } from "zustand";
import { Game } from "@/types/game";
import { SlipNumber, RandomStrategy } from "@/types/slip";
import { MixedSlot } from "@/types/mixed";
import { QuestionDto, RevealedNumberDto } from "@/types/lucky";
import {
  generateMixedRandomSlot,
  getMixedLuckyQuestion,
  answerMixedLuckySlot,
  fillMixedRemainder
} from "@/lib/api";

interface MixedBuilderState {
  isOpen: boolean;
  game: Game | null;
  lineLabel: string;
  slots: MixedSlot[];
  activeSlotIndex: number | null;
  activePickerMode: "source" | "manual" | "random" | "lucky" | null;

  luckyQuestion: QuestionDto | null;
  luckyRevealed: RevealedNumberDto | null;
  isGenerating: boolean;
  error: string | null;

  openBuilder: (game: Game, lineLabel: string, initialNumbers?: SlipNumber[]) => void;
  selectSlot: (slotIndex: number) => void;
  setPickerMode: (mode: "source" | "manual" | "random" | "lucky" | null) => void;
  closePicker: () => void;

  setManualNumber: (slotIndex: number, val: number) => void;
  generateRandomForSlot: (slotIndex: number, strategy: RandomStrategy) => Promise<void>;
  startLuckyForSlot: (slotIndex: number) => Promise<void>;
  answerLuckyForSlot: (slotIndex: number, choiceId: number) => Promise<void>;
  applyLuckyRevealed: (slotIndex: number) => void;

  clearSlot: (slotIndex: number) => void;
  fillRemainder: (strategy?: RandomStrategy) => Promise<void>;
  resetAllSlots: () => void;
  closeBuilder: () => void;
}

function buildInitialSlots(game: Game, initialNumbers?: SlipNumber[]): MixedSlot[] {
  const slots: MixedSlot[] = [];
  let slotIndex = 0;

  for (const pool of game.pools) {
    const isSpecial = pool.poolIndex === 1;
    const existingInPool = initialNumbers
      ? initialNumbers.filter((n) => n.poolIndex === pool.poolIndex)
      : [];

    for (let i = 0; i < pool.pickCount; i++) {
      const existingNum = existingInPool[i] || null;
      slots.push({
        slotIndex: slotIndex++,
        poolIndex: pool.poolIndex,
        poolName: pool.name,
        isSpecial,
        number: existingNum,
        status: existingNum ? "Completed" : "Empty"
      });
    }
  }

  return slots;
}

export const useMixedBuilderStore = create<MixedBuilderState>((set, get) => ({
  isOpen: false,
  game: null,
  lineLabel: "A",
  slots: [],
  activeSlotIndex: null,
  activePickerMode: null,
  luckyQuestion: null,
  luckyRevealed: null,
  isGenerating: false,
  error: null,

  openBuilder: (game, lineLabel, initialNumbers) => {
    const slots = buildInitialSlots(game, initialNumbers);
    set({
      isOpen: true,
      game,
      lineLabel,
      slots,
      activeSlotIndex: null,
      activePickerMode: null,
      luckyQuestion: null,
      luckyRevealed: null,
      isGenerating: false,
      error: null
    });
  },

  selectSlot: (slotIndex) => {
    const slot = get().slots[slotIndex];
    if (!slot) return;

    set({
      activeSlotIndex: slotIndex,
      activePickerMode: "source",
      luckyQuestion: null,
      luckyRevealed: null,
      error: null
    });
  },

  setPickerMode: (mode) => set({ activePickerMode: mode, error: null }),

  closePicker: () => {
    set({
      activeSlotIndex: null,
      activePickerMode: null,
      luckyQuestion: null,
      luckyRevealed: null,
      error: null
    });
  },

  setManualNumber: (slotIndex, val) => {
    const { slots } = get();
    const slot = slots[slotIndex];
    if (!slot) return;

    const updatedNum: SlipNumber = {
      value: val,
      formatted: val.toString().padStart(2, "0"),
      poolIndex: slot.poolIndex,
      source: "Manual"
    };

    const newSlots = slots.map((s, idx) =>
      idx === slotIndex ? { ...s, number: updatedNum, status: "Completed" as const } : s
    );

    set({
      slots: newSlots,
      activeSlotIndex: null,
      activePickerMode: null,
      error: null
    });
  },

  generateRandomForSlot: async (slotIndex, strategy) => {
    const { game, slots } = get();
    const slot = slots[slotIndex];
    if (!game || !slot) return;

    set({ isGenerating: true, error: null });

    // Exclude other numbers in the same pool
    const excludedNumbers = slots
      .filter((s, idx) => idx !== slotIndex && s.poolIndex === slot.poolIndex && s.number !== null)
      .map((s) => s.number!.value);

    try {
      const res = await generateMixedRandomSlot({
        gameCode: game.code,
        poolIndex: slot.poolIndex,
        strategy,
        excludedNumbers
      });

      const newSlots = slots.map((s, idx) =>
        idx === slotIndex ? { ...s, number: res.number, status: "Completed" as const } : s
      );

      set({
        slots: newSlots,
        isGenerating: false,
        activeSlotIndex: null,
        activePickerMode: null
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Không thể sinh số Thần Tài";
      set({ error: msg, isGenerating: false });
    }
  },

  startLuckyForSlot: async (slotIndex) => {
    const { game, slots } = get();
    const slot = slots[slotIndex];
    if (!game || !slot) return;

    set({ isGenerating: true, activePickerMode: "lucky", luckyQuestion: null, luckyRevealed: null, error: null });

    try {
      const res = await getMixedLuckyQuestion({
        gameCode: game.code,
        poolIndex: slot.poolIndex,
        isClimaxStep: slot.isSpecial
      });

      set({
        luckyQuestion: res.question,
        isGenerating: false
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Không thể tải câu hỏi Lucky";
      set({ error: msg, isGenerating: false });
    }
  },

  answerLuckyForSlot: async (slotIndex, choiceId) => {
    const { game, slots, luckyQuestion } = get();
    const slot = slots[slotIndex];
    if (!game || !slot || !luckyQuestion) return;

    set({ isGenerating: true, error: null });

    const excludedNumbers = slots
      .filter((s, idx) => idx !== slotIndex && s.poolIndex === slot.poolIndex && s.number !== null)
      .map((s) => s.number!.value);

    const previousInLine = slots
      .filter((s, idx) => idx !== slotIndex && s.number !== null)
      .map((s) => s.number!.value);

    try {
      const res = await answerMixedLuckySlot({
        gameCode: game.code,
        poolIndex: slot.poolIndex,
        questionId: luckyQuestion.id,
        choiceId,
        excludedNumbers,
        previousNumbersInLine: previousInLine
      });

      set({
        luckyRevealed: res.revealedNumber,
        isGenerating: false
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Lỗi khi mở số Lucky";
      set({ error: msg, isGenerating: false });
    }
  },

  applyLuckyRevealed: (slotIndex) => {
    const { slots, luckyRevealed } = get();
    const slot = slots[slotIndex];
    if (!slot || !luckyRevealed) return;

    const luckyNum: SlipNumber = {
      value: luckyRevealed.value,
      formatted: luckyRevealed.formatted,
      poolIndex: luckyRevealed.poolIndex,
      source: "Lucky",
      metadataJson: luckyRevealed.metadataJson
    };

    const newSlots = slots.map((s, idx) =>
      idx === slotIndex ? { ...s, number: luckyNum, status: "Completed" as const } : s
    );

    set({
      slots: newSlots,
      activeSlotIndex: null,
      activePickerMode: null,
      luckyQuestion: null,
      luckyRevealed: null
    });
  },

  clearSlot: (slotIndex) => {
    const { slots } = get();
    const newSlots = slots.map((s, idx) =>
      idx === slotIndex ? { ...s, number: null, status: "Empty" as const } : s
    );
    set({
      slots: newSlots,
      activeSlotIndex: null,
      activePickerMode: null,
      error: null
    });
  },

  fillRemainder: async (strategy = "Balanced") => {
    const { game, slots } = get();
    if (!game) return;

    set({ isGenerating: true, error: null });

    const existingNumbers = slots
      .filter((s) => s.number !== null)
      .map((s) => s.number!);

    try {
      const res = await fillMixedRemainder({
        gameCode: game.code,
        strategy,
        existingNumbers
      });

      // Distribute the filled numbers back into slots per pool
      const poolGroups: Record<number, SlipNumber[]> = {};
      for (const num of res.numbers) {
        if (!poolGroups[num.poolIndex]) poolGroups[num.poolIndex] = [];
        poolGroups[num.poolIndex].push(num);
      }

      const newSlots = slots.map((s) => {
        if (s.number !== null) return s; // preserve existing slot
        const availableInPool = poolGroups[s.poolIndex] || [];
        const nextNum = availableInPool.find((n) =>
          !slots.some((other) => other.number?.value === n.value && other.poolIndex === s.poolIndex)
        ) || availableInPool.shift() || null;

        const slotStatus: import("@/types/mixed").SlotStatus = nextNum ? "Completed" : "Empty";
        return {
          ...s,
          number: nextNum,
          status: slotStatus
        };
      });

      set({
        slots: newSlots,
        isGenerating: false,
        activeSlotIndex: null,
        activePickerMode: null
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Lỗi khi điền số tự động";
      set({ error: msg, isGenerating: false });
    }
  },

  resetAllSlots: () => {
    const { game } = get();
    if (!game) return;
    set({
      slots: buildInitialSlots(game),
      activeSlotIndex: null,
      activePickerMode: null,
      error: null
    });
  },

  closeBuilder: () => {
    set({
      isOpen: false,
      activeSlotIndex: null,
      activePickerMode: null,
      luckyQuestion: null,
      luckyRevealed: null
    });
  }
}));
