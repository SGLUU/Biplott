import { NumberSource } from "./slip";

export type QuestionType =
  | "SingleChoice"
  | "ThisOrThat"
  | "Scenario"
  | "QuickInstinct"
  | "Slider"
  | "VisualChoice"
  | "BlindChoice"
  | "Ranking"
  | "SymbolChoice";

export interface ChoiceDto {
  id: number;
  content: string;
  subContent?: string;
  mediaUrl?: string;
  orderIndex: number;
}

export interface QuestionDto {
  id: number;
  themeId: number;
  themeCode: string;
  themeName: string;
  themeIcon?: string;
  questionType: QuestionType;
  content: string;
  subtitle?: string;
  mediaUrl?: string;
  choices: ChoiceDto[];
}

export interface RevealedNumberDto {
  value: number;
  formatted: string;
  poolIndex: number;
  source: NumberSource;
  explanation: string;
  dominantTrait?: string;
  themeName?: string;
  questionText?: string;
  choiceText?: string;
  metadataJson?: string;
}

export interface StartJourneyRequest {
  gameCode: string;
  lineLabel: string;
  recentQuestionIds?: number[];
  recentThemeIds?: number[];
}

export interface StartJourneyResponse {
  journeyId: string;
  gameCode: string;
  lineLabel: string;
  currentStep: number;
  totalSteps: number;
  currentPoolIndex: number;
  currentPoolName: string;
  isClimaxStep: boolean;
  firstQuestion: QuestionDto;
}

export interface AnswerStepRequest {
  questionId: number;
  choiceId: number;
  recentQuestionIds?: number[];
  recentThemeIds?: number[];
}

export interface AnswerStepResponse {
  journeyId: string;
  revealedNumber: RevealedNumberDto;
  currentStep: number;
  totalSteps: number;
  currentPoolIndex: number;
  currentPoolName: string;
  isClimaxStep: boolean;
  isCompleted: boolean;
  nextQuestion?: QuestionDto;
  completedNumbers?: RevealedNumberDto[];
  journeyCommentary?: string;
}
