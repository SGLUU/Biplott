import { create } from "zustand";
import { Game } from "@/types/game";
import { QuestionDto, RevealedNumberDto } from "@/types/lucky";
import { apiGetTodayDailyJourney, apiStartDailyJourney, apiAnswerDailyStep } from "@/lib/api";
import { getOrCreateGuestSessionToken } from "@/lib/utils";

interface DailyJourneyState {
  isOpen: boolean;
  journeyId: string | null;
  game: Game | null;
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

  openJourney: (game: Game) => Promise<void>;
  submitChoice: (choiceId: number) => Promise<void>;
  proceedToNextStep: () => void;
  closeJourney: () => void;
  resetJourney: () => void;
}

export const useDailyJourneyStore = create<DailyJourneyState>((set, get) => ({
  isOpen: false,
  journeyId: null,
  game: null,
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

  openJourney: async (game) => {
    set({
      isOpen: true,
      game,
      journeyId: null,
      currentStep: 1,
      totalSteps: game.pools.reduce((sum, p) => sum + p.pickCount, 0),
      currentPoolIndex: 0,
      currentPoolName: game.pools[0]?.name || "Dãy số chính",
      isClimaxStep: false,
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
      const guestToken = getOrCreateGuestSessionToken();
      // Try to load today's daily journey first to resume or show completed
      const todayJourney = await apiGetTodayDailyJourney(game.code, guestToken);

      if (todayJourney) {
        if (todayJourney.status === "Completed") {
          set({
            journeyId: todayJourney.journeyId.toString(),
            currentStep: todayJourney.totalSteps,
            isCompleted: true,
            completedNumbers: todayJourney.numbers,
            isSubmitting: false,
            journeyCommentary: "Bạn đã hoàn thành hành trình hôm nay!"
          });
          return;
        }

        // Resume in-progress journey
        set({
          journeyId: todayJourney.journeyId.toString(),
          currentStep: todayJourney.currentStep,
          totalSteps: todayJourney.totalSteps,
          currentQuestion: todayJourney.activeQuestion,
          completedNumbers: todayJourney.numbers,
          isSubmitting: false
        });
        return;
      }

      // Start new
      const res = await apiStartDailyJourney({
        gameCode: game.code,
        lineLabel: "D", // D for Daily
        guestSessionToken: guestToken
      });

      set({
        journeyId: res.journeyId,
        currentStep: res.currentStep,
        totalSteps: res.totalSteps,
        currentPoolIndex: res.currentPoolIndex,
        currentPoolName: res.currentPoolName,
        isClimaxStep: res.isClimaxStep,
        currentQuestion: res.firstQuestion,
        isSubmitting: false
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Không thể bắt đầu Daily Journey";
      set({ error: msg, isSubmitting: false });
    }
  },

  submitChoice: async (choiceId) => {
    const { journeyId, currentQuestion } = get();
    if (!journeyId || !currentQuestion) return;

    set({ selectedChoiceId: choiceId, isSubmitting: true, error: null });

    try {
      const res = await apiAnswerDailyStep(journeyId, {
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

  closeJourney: () => {
    set({ isOpen: false });
  },

  resetJourney: () => {
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
