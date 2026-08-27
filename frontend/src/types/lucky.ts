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
  isLocked?: boolean;
}

export interface StartJourneyRequest {
  gameCode: string;
  lineLabel: string;
  recentQuestionIds?: number[];
  recentThemeIds?: number[];
  guestSessionToken?: string;
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

// Matches DailyJourneyStatus values from backend
export type DailyJourneyStatus = "NotStarted" | "InProgress" | "Completed";

// Matches backend DailyJourneyDto (returned from GET /api/v1/daily-journeys)
export interface DailyJourneyStatusResponse {
  journeyId: string;
  gameCode: string;
  dailyDate: string;
  status: DailyJourneyStatus;
  currentStep: number;
  totalSteps: number;
  numbers: RevealedNumberDto[];
  activeQuestion?: QuestionDto;
}

export interface TraitScore {
  traitCode: string;
  traitName: string;
  score: number;
  sampleCount: number;
}

export interface LuckyDna {
  status: "NotFormed" | "Forming" | "Completed";
  totalAnswers: number;
  archetype: string;
  description: string;
  topTraits: TraitScore[];
  allTraits: TraitScore[];
  updatedAt?: string;
}
