export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface AdminDashboardMetrics {
  totalUsers: number;
  totalSavedSlips: number;
  totalThemes: number;
  totalQuestions: number;
  activeQuestions: number;
  inactiveQuestions: number;
  totalChoices: number;
  totalTraits: number;
  recentUsers: AdminUser[];
  recentQuestions: AdminQuestionList[];
}

export interface AdminTheme {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  sortOrder: number;
  isActive: boolean;
  questionsCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateThemeRequest {
  code: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  sortOrder?: number;
  isActive?: boolean;
}

export interface UpdateThemeRequest {
  name: string;
  description?: string | null;
  icon?: string | null;
  sortOrder?: number;
  isActive?: boolean;
}

export interface AdminTrait {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  category?: string | null;
  isActive: boolean;
  choicesCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTraitRequest {
  code: string;
  name: string;
  description?: string | null;
  category?: string | null;
  isActive?: boolean;
}

export interface UpdateTraitRequest {
  name: string;
  description?: string | null;
  category?: string | null;
  isActive?: boolean;
}

export type QuestionType = "SingleChoice" | "ThisOrThat" | "Scenario" | "QuickInstinct";

export interface AdminChoiceTrait {
  traitId: number;
  traitCode: string;
  traitName: string;
  weight: number;
}

export interface AdminChoice {
  id: number;
  questionId: number;
  content: string;
  subContent?: string | null;
  mediaUrl?: string | null;
  orderIndex: number;
  isActive: boolean;
  choiceTraits: AdminChoiceTrait[];
}

export interface AdminQuestionList {
  id: number;
  themeId: number;
  themeCode: string;
  themeName: string;
  questionType: QuestionType;
  content: string;
  subtitle?: string | null;
  mediaUrl?: string | null;
  isActive: boolean;
  viewCount: number;
  choicesCount: number;
  activeChoicesCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface AdminQuestionDetail {
  id: number;
  themeId: number;
  themeCode: string;
  themeName: string;
  questionType: QuestionType;
  content: string;
  subtitle?: string | null;
  mediaUrl?: string | null;
  isActive: boolean;
  viewCount: number;
  choices: AdminChoice[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateChoiceTraitRequest {
  traitId: number;
  weight: number;
}

export interface CreateChoiceRequest {
  content: string;
  subContent?: string | null;
  mediaUrl?: string | null;
  orderIndex?: number;
  isActive: boolean;
  choiceTraits: CreateChoiceTraitRequest[];
}

export interface CreateQuestionRequest {
  themeId: number;
  questionType: QuestionType;
  content: string;
  subtitle?: string | null;
  mediaUrl?: string | null;
  isActive: boolean;
  choices: CreateChoiceRequest[];
}

export interface UpdateQuestionRequest {
  themeId: number;
  questionType: QuestionType;
  content: string;
  subtitle?: string | null;
  mediaUrl?: string | null;
  isActive: boolean;
  choices: CreateChoiceRequest[];
}

export interface QuestionFilterParams {
  page?: number;
  pageSize?: number;
  search?: string;
  themeId?: number;
  questionType?: QuestionType;
  isActive?: boolean;
  sortBy?: string;
}

// Bulk Import
export interface ImportRowError {
  rowIndex: number;
  field: string;
  message: string;
}

export interface ImportTraitPreview {
  traitCode: string;
  weight: number;
}

export interface ImportChoicePreview {
  content: string;
  subContent?: string | null;
  traits: ImportTraitPreview[];
}

export interface ImportQuestionPreview {
  rowIndex: number;
  themeCode: string;
  questionType: string;
  content: string;
  subtitle?: string | null;
  choices: ImportChoicePreview[];
  isValid: boolean;
  errors: string[];
}

export interface ImportValidationResult {
  isValid: boolean;
  totalRows: number;
  validCount: number;
  invalidCount: number;
  errors: ImportRowError[];
  previewItems: ImportQuestionPreview[];
  importSessionId: string;
}

export interface ImportConfirmResponse {
  success: boolean;
  importedQuestionsCount: number;
  importedChoicesCount: number;
  message: string;
}

// Engine Configs
export interface LuckyEngineConfig {
  baseWeight: number;
  traitAffinityMultiplier: number;
  noiseMagnitude: number;
  minWeight: number;
}

export interface NoveltyEngineConfig {
  baseWeight: number;
  neverSeenBonus: number;
  recentlySeenPenalty: number;
  repeatedThemePenalty: number;
  recentThemePenalty: number;
  questionTypeDiversityBonus: number;
  climaxDestinyThemeBoost: number;
  climaxQuickInstinctBoost: number;
}

export interface RandomEngineConfig {
  balancedMaxDeviation: number;
  spreadMinPartitions: number;
  enableSurpriseOutliers: boolean;
}

export interface AdminSettings {
  lucky: LuckyEngineConfig;
  novelty: NoveltyEngineConfig;
  random: RandomEngineConfig;
  updatedAt: string;
}

// Users
export interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  savedSlipsCount: number;
}