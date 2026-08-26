import { NumberSource, SlipLineStatus, SlipNumber } from "./slip";

export interface SaveSlipNumberDto {
  value: number;
  poolIndex: number;
  source: NumberSource;
  metadataJson?: string;
}

export interface SaveSlipLineDto {
  lineLabel: string;
  numbers: SaveSlipNumberDto[];
}

export interface SaveSlipRequest {
  gameCode: string;
  slipCode?: string;
  title?: string;
  isFavorite?: boolean;
  lines: SaveSlipLineDto[];
}

export interface SavedSlipLineSummary {
  lineLabel: string;
  numbers: SlipNumber[];
  derivedMode: "Manual" | "Random" | "Lucky" | "Mixed";
}

export interface SavedSlipSummary {
  id: string;
  gameCode: string;
  gameName: string;
  slipCode: string;
  title?: string;
  isFavorite: boolean;
  completedLineCount: number;
  createdAt: string;
  lines: SavedSlipLineSummary[];
}

export interface LuckyStoryItem {
  lineLabel: string;
  numberValue: number;
  formatted: string;
  poolIndex: number;
  themeName: string;
  questionText: string;
  choiceText: string;
  explanation: string;
  dominantTrait?: string;
}

export interface SavedSlipLineDetail {
  id: string;
  lineLabel: string;
  status: SlipLineStatus;
  numbers: SlipNumber[];
  derivedMode: "Manual" | "Random" | "Lucky" | "Mixed";
}

export interface SavedSlipDetail {
  id: string;
  gameCode: string;
  gameName: string;
  slipCode: string;
  title?: string;
  isFavorite: boolean;
  createdAt: string;
  updatedAt: string;
  lines: SavedSlipLineDetail[];
  luckyStories: LuckyStoryItem[];
}

export interface UserActivityItem {
  id: string;
  gameCode: string;
  gameName: string;
  activityType: string;
  title: string;
  summary: string;
  numbersJson?: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}
