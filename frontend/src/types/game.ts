export interface GamePool {
  id: number;
  poolIndex: number;
  name: string;
  minNumber: number;
  maxNumber: number;
  pickCount: number;
  allowDuplicates: boolean;
  badgeColor?: string;
}

export interface Game {
  id: number;
  code: string;
  name: string;
  description: string;
  tagline?: string;
  iconUrl?: string;
  isActive: boolean;
  sortOrder: number;
  pools: GamePool[];
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  timestamp: string;
}
