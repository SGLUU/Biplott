import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import { Game } from "@/types/game";
import { QuestionDto, RevealedNumberDto } from "@/types/lucky";
import { startLuckyJourney, answerLuckyStep, cancelLuckyJourney } from "@/lib/api";

interface LuckyJourneyState {
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

  // Novelty history context
  recentQuestionIds: number[];
  recentThemeIds: number[];

  openJourney: (game: Game, lineLabel: string) => Promise<void>;
  submitChoice: (choiceId: number) => Promise<void>;
  proceedToNextStep: () => void;
  cancelJourney: () => Promise<void>;
  closeJourney: () => void;
  resetJourney: () => void;
}

export const useLuckyJourneyStore = create<LuckyJourneyState>()(
  persist(
    (set, get) => ({
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

      recentQuestionIds: [],
      recentThemeIds: [],

      openJourney: async (game, lineLabel) => {
        set({
          isOpen: true,
          game,
          lineLabel,
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
          const res = await startLuckyJourney({
            gameCode: game.code,
            lineLabel,
            recentQuestionIds: get().recentQuestionIds.slice(-20),
            recentThemeIds: get().recentThemeIds.slice(-10)
          });

          // Update recent tracking
          const updatedRecentQuestions = [...get().recentQuestionIds, res.firstQuestion.id].slice(-30);
          const updatedRecentThemes = [...get().recentThemeIds, res.firstQuestion.themeId].slice(-20);

          set({
            journeyId: res.journeyId,
            currentStep: res.currentStep,
            totalSteps: res.totalSteps,
            currentPoolIndex: res.currentPoolIndex,
            currentPoolName: res.currentPoolName,
            isClimaxStep: res.isClimaxStep,
            currentQuestion: res.firstQuestion,
            isSubmitting: false,
            recentQuestionIds: updatedRecentQuestions,
            recentThemeIds: updatedRecentThemes
          });
        } catch (err: unknown) {
          const msg = err instanceof Error ? err.message : "Không thể bắt đầu Lucky Journey";
          set({ error: msg, isSubmitting: false });
        }
      },

      submitChoice: async (choiceId) => {
        const { journeyId, currentQuestion, recentQuestionIds, recentThemeIds } = get();
        if (!journeyId || !currentQuestion) return;

        set({ selectedChoiceId: choiceId, isSubmitting: true, error: null });

        try {
          const res = await answerLuckyStep(journeyId, {
            questionId: currentQuestion.id,
            choiceId,
            recentQuestionIds: recentQuestionIds.slice(-20),
            recentThemeIds: recentThemeIds.slice(-10)
          });

          const updatedCompleted = res.completedNumbers || [...get().completedNumbers, res.revealedNumber];

          let updatedRecentQ = recentQuestionIds;
          let updatedRecentT = recentThemeIds;

          if (res.nextQuestion) {
            updatedRecentQ = [...recentQuestionIds, res.nextQuestion.id].slice(-30);
            updatedRecentT = [...recentThemeIds, res.nextQuestion.themeId].slice(-20);
          }

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
            isClimaxStep: res.isClimaxStep,
            recentQuestionIds: updatedRecentQ,
            recentThemeIds: updatedRecentT
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

      cancelJourney: async () => {
        const { journeyId } = get();
        if (journeyId) {
          await cancelLuckyJourney(journeyId);
        }
        set({ isOpen: false, journeyId: null, isRevealed: false, isCompleted: false });
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
    }),
    {
      name: "biplott_lucky_history",
      storage: createJSONStorage(() => sessionStorage),
      partialize: (state) => ({
        recentQuestionIds: state.recentQuestionIds,
        recentThemeIds: state.recentThemeIds
      })
    }
  )
);
