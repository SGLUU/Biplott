export type NumberSource = "Manual" | "Lucky" | "Random";
export type SlipLineStatus = "Empty" | "Partial" | "Complete";
export type RandomStrategy = "PureRandom" | "Balanced" | "Spread" | "Surprise";

export interface SlipNumber {
  value: number;
  formatted: string;
  poolIndex: number;
  source: NumberSource;
  metadataJson?: string;
  isLocked?: boolean;
}

export interface SlipLine {
  lineLabel: string; // "A", "B", "C", "D", "E", "F"
  status: SlipLineStatus;
  numbers: SlipNumber[];
  strategy?: RandomStrategy;
  commentary?: string;
}

export interface Slip {
  gameCode: string;
  slipCode: string;
  lines: SlipLine[];
  createdAt?: string;
}

export interface GenerateLineRequest {
  gameCode: string;
  strategy?: RandomStrategy;
  excludedNumbers?: number[];
  currentNumbers?: SlipNumber[];
}

export interface GenerateLineResponse {
  strategy: RandomStrategy;
  strategyName: string;
  numbers: SlipNumber[];
  commentary: string;
}

export interface ValidateLineRequest {
  gameCode: string;
  lineLabel: string;
  numbers: { value: number; poolIndex: number; source?: NumberSource }[];
}

export interface ValidateLineResponse {
  isValid: boolean;
  errors: string[];
}

export interface GenerateSlipRequest {
  gameCode: string;
  strategy?: RandomStrategy;
  fillMode: "EmptyOnly" | "All";
  existingLines?: SlipLine[];
}

export interface GenerateSlipResponse {
  gameCode: string;
  strategy: RandomStrategy;
  lines: SlipLine[];
  commentary: string;
}
