import { SlipNumber, RandomStrategy } from "./slip";
import { QuestionDto, RevealedNumberDto } from "./lucky";

export type SlotStatus = "Empty" | "Selecting" | "Revealing" | "Completed";

export interface MixedSlot {
  slotIndex: number;
  poolIndex: number;
  poolName: string;
  isSpecial: boolean;
  number: SlipNumber | null;
  status: SlotStatus;
}

export interface GenerateRandomSlotRequest {
  gameCode: string;
  poolIndex: number;
  strategy?: RandomStrategy;
  excludedNumbers?: number[];
}

export interface GenerateRandomSlotResponse {
  number: SlipNumber;
  strategy: RandomStrategy;
  strategyName: string;
  commentary: string;
}

export interface GetMixedLuckyQuestionRequest {
  gameCode: string;
  poolIndex: number;
  isClimaxStep?: boolean;
  recentQuestionIds?: number[];
  recentThemeIds?: number[];
}

export interface GetMixedLuckyQuestionResponse {
  question: QuestionDto;
}

export interface AnswerMixedLuckySlotRequest {
  gameCode: string;
  poolIndex: number;
  questionId: number;
  choiceId: number;
  excludedNumbers?: number[];
  previousNumbersInLine?: number[];
}

export interface AnswerMixedLuckySlotResponse {
  revealedNumber: RevealedNumberDto;
}

export interface FillRemainderRequest {
  gameCode: string;
  strategy?: RandomStrategy;
  existingNumbers: SlipNumber[];
}

export interface FillRemainderResponse {
  gameCode: string;
  strategy: RandomStrategy;
  numbers: SlipNumber[];
  commentary: string;
}
