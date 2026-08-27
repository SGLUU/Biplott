import { create } from "zustand";
import { Game } from "@/types/game";
import { SlipNumber } from "@/types/slip";
import { QuestionDto, RevealedNumberDto } from "@/types/lucky";
import { apiStartLuckyRemix, apiAnswerLuckyRemixStep } from "@/lib/api";

interface LuckyRemixState {
  isOpen: boolean;
  journeyId: string | null;
  game: Game | null;
  lineLabel: string;
  currentStep: number;
  totalSteps: number;
  currentPoolIndex: number;
  currentPoolName: string;
  isClimaxStep: boolean;

  currentQuestion: QuestionDto | null;
  selectedChoiceId: number | null;
  revealedNumber: RevealedNumberDto | null;
  nextQuestion: QuestionDto | null;
  completedNumbers: RevealedNumberDto[];
  journeyCommentary: string | null;

  isSubmitting: boolean;
  isRevealed: boolean;
  isCompleted: boolean;
  error: string | null;

  openRemix: (game: Game, lineLabel: string, currentNumbers: SlipNumber[]) => Promise<void>;
  submitChoice: (choiceId: number) => Promise<void>;
  proceedToNextStep: () => void;
  closeRemix: () => void;
  resetRemix: () => void;
}

export const useLuckyRemixStore = create<LuckyRemixState>((set, get) => ({
  isOpen: false,
  journeyId: null,
  game: null,
  lineLabel: "A",
  currentStep: 1,
  totalSteps: 6,
  currentPoolIndex: 0,
  currentPoolName: "Dãy số chính",
  isClimaxStep: false,

  currentQuestion: null,
  selectedChoiceId: null,
  revealedNumber: null,
  nextQuestion: null,
  completedNumbers: [],
  journeyCommentary: null,

  isSubmitting: false,
  isRevealed: false,
  isCompleted: false,
  error: null,

  openRemix: async (game, lineLabel, currentNumbers) => {
    // Clear out client-side metadata to keep clean payload
    const payloadNumbers = currentNumbers.map((n) => ({
      value: n.value,
      poolIndex: n.poolIndex,
      source: n.source,
      metadataJson: n.metadataJson,
      isLocked: n.isLocked
    }));

    set({
      isOpen: true,
      game,
      lineLabel,
      journeyId: null,
      currentStep: 1,
      totalSteps: 6,
      currentQuestion: null,
      selectedChoiceId: null,
      revealedNumber: null,
      nextQuestion: null,
      completedNumbers: [],
      journeyCommentary: null,
      isSubmitting: true,
      isRevealed: false,
      isCompleted: false,
      error: null
    });

    try {
      const res = await apiStartLuckyRemix({
        gameCode: game.code,
        lineLabel,
        currentNumbers: payloadNumbers
      });

      set({
        journeyId: res.journeyId,
        currentStep: res.currentStep,
        totalSteps: res.totalSteps,
        currentPoolIndex: res.currentPoolIndex,
        currentPoolName: res.currentPoolName,
        isClimaxStep: res.isClimaxStep,
        currentQuestion: res.firstQuestion,
        completedNumbers: [], // fresh remix starts with no completed numbers
        isSubmitting: false
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Không thể khởi động Lucky Remix";
      set({ error: msg, isSubmitting: false });
    }
  },

  submitChoice: async (choiceId) => {
    const { journeyId, currentQuestion } = get();
    if (!journeyId || !currentQuestion) return;

    set({ selectedChoiceId: choiceId, isSubmitting: true, error: null });

    try {
      const res = await apiAnswerLuckyRemixStep(journeyId, {
        questionId: currentQuestion.id,
        choiceId
      });

      const updatedCompleted = res.completedNumbers || [...get().completedNumbers, res.revealedNumber];

      set({
        isSubmitting: false,
        isRevealed: true,
        revealedNumber: res.revealedNumber,
        completedNumbers: updatedCompleted,
        isCompleted: res.isCompleted,
        nextQuestion: res.nextQuestion,
        journeyCommentary: res.journeyCommentary,
        currentPoolIndex: res.currentPoolIndex,
        currentPoolName: res.currentPoolName,
        isClimaxStep: res.isClimaxStep
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Lỗi khi xử lý lựa chọn";
      set({ error: msg, isSubmitting: false });
    }
  },

  proceedToNextStep: () => {
    const { nextQuestion, currentStep } = get();
    if (!nextQuestion) return;

    set({
      currentQuestion: nextQuestion,
      nextQuestion: null,
      revealedNumber: null,
      selectedChoiceId: null,
      isRevealed: false,
      currentStep: currentStep + 1
    });
  },

  closeRemix: () => {
    set({ isOpen: false });
  },

  resetRemix: () => {
    set({
      isOpen: false,
      journeyId: null,
      currentQuestion: null,
      revealedNumber: null,
      nextQuestion: null,
      completedNumbers: [],
      isRevealed: false,
      isCompleted: false,
      error: null
    });
  }
}));
