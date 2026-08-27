import { ApiResponse, Game } from "@/types/game";
import {
  GenerateLineRequest,
  GenerateLineResponse,
  GenerateSlipRequest,
  GenerateSlipResponse,
  ValidateLineRequest,
  ValidateLineResponse
} from "@/types/slip";
import {
  StartJourneyRequest,
  StartJourneyResponse,
  AnswerStepRequest,
  AnswerStepResponse,
  DailyJourneyStatusResponse,
  LuckyDna
} from "@/types/lucky";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export async function fetchActiveGames(): Promise<Game[]> {
  try {
    const url = `${API_BASE_URL}/api/v1/games`;
    const res = await fetch(url, {
      next: { revalidate: 30 },
      headers: { "Accept": "application/json" }
    });

    if (!res.ok) {
      const fallbackRes = await fetch(`${API_BASE_URL}/api/games`, {
        cache: "no-store",
        headers: { "Accept": "application/json" }
      });
      if (!fallbackRes.ok) {
        throw new Error(`API returned status ${fallbackRes.status}`);
      }
      const data: ApiResponse<Game[]> = await fallbackRes.json();
      return data.data || [];
    }

    const data: ApiResponse<Game[]> = await res.json();
    return data.data || [];
  } catch (error) {
    console.error("Error fetching games from Backend API:", error);
    throw error;
  }
}

export async function fetchGameByCode(code: string): Promise<Game | null> {
  try {
    const url = `${API_BASE_URL}/api/v1/games/${encodeURIComponent(code)}`;
    const res = await fetch(url, {
      headers: { "Accept": "application/json" }
    });

    if (!res.ok) {
      const fallbackRes = await fetch(`${API_BASE_URL}/api/games/${encodeURIComponent(code)}`, {
        headers: { "Accept": "application/json" }
      });
      if (!fallbackRes.ok) return null;
      const data: ApiResponse<Game> = await fallbackRes.json();
      return data.data || null;
    }

    const data: ApiResponse<Game> = await res.json();
    return data.data || null;
  } catch (error) {
    console.error(`Error fetching game ${code}:`, error);
    return null;
  }
}

export async function generateThanTaiLine(req: GenerateLineRequest): Promise<GenerateLineResponse> {
  const url = `${API_BASE_URL}/api/v1/than-tai/generate-line`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Lỗi sinh số Thần Tài (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<GenerateLineResponse> = await res.json();
  return data.data;
}

export async function generateThanTaiSlip(req: GenerateSlipRequest): Promise<GenerateSlipResponse> {
  const url = `${API_BASE_URL}/api/v1/than-tai/generate-slip`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Lỗi sinh cả phiếu Thần Tài (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<GenerateSlipResponse> = await res.json();
  return data.data;
}

export async function validateSlipLine(req: ValidateLineRequest): Promise<ValidateLineResponse> {
  const url = `${API_BASE_URL}/api/v1/slips/validate-line`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    return {
      isValid: false,
      errors: [`Lỗi kết nối máy chủ (${res.status}): ${errorBody}`]
    };
  }

  const data: ApiResponse<ValidateLineResponse> = await res.json();
  return data.data;
}

// ==========================================
// Phase 2B: Lucky Journey API Methods
// ==========================================

export async function startLuckyJourney(req: StartJourneyRequest): Promise<StartJourneyResponse> {
  const url = `${API_BASE_URL}/api/v1/lucky/journeys/start`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Không thể khởi tạo Lucky Journey (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<StartJourneyResponse> = await res.json();
  return data.data;
}

export async function answerLuckyStep(
  journeyId: string,
  req: AnswerStepRequest
): Promise<AnswerStepResponse> {
  const url = `${API_BASE_URL}/api/v1/lucky/journeys/${encodeURIComponent(journeyId)}/answer`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Lỗi xử lý đáp án (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<AnswerStepResponse> = await res.json();
  return data.data;
}

export async function cancelLuckyJourney(journeyId: string): Promise<void> {
  try {
    const url = `${API_BASE_URL}/api/v1/lucky/journeys/${encodeURIComponent(journeyId)}/cancel`;
    await fetch(url, {
      method: "POST",
      headers: { "Accept": "application/json" }
    });
  } catch (err) {
    console.warn("Lỗi khi hủy Lucky Journey session:", err);
  }
}

// ==========================================
// Phase 2C: Mixed Mode API Methods
// ==========================================

export async function generateMixedRandomSlot(
  req: import("@/types/mixed").GenerateRandomSlotRequest
): Promise<import("@/types/mixed").GenerateRandomSlotResponse> {
  const url = `${API_BASE_URL}/api/v1/mixed/generate-random-slot`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Lỗi sinh số Thần Tài cho ô (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<import("@/types/mixed").GenerateRandomSlotResponse> = await res.json();
  return data.data;
}

export async function getMixedLuckyQuestion(
  req: import("@/types/mixed").GetMixedLuckyQuestionRequest
): Promise<import("@/types/mixed").GetMixedLuckyQuestionResponse> {
  const url = `${API_BASE_URL}/api/v1/mixed/lucky-question`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Lỗi lấy câu hỏi Lucky cho ô (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<import("@/types/mixed").GetMixedLuckyQuestionResponse> = await res.json();
  return data.data;
}

export async function answerMixedLuckySlot(
  req: import("@/types/mixed").AnswerMixedLuckySlotRequest
): Promise<import("@/types/mixed").AnswerMixedLuckySlotResponse> {
  const url = `${API_BASE_URL}/api/v1/mixed/lucky-answer`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Lỗi xử lý đáp án Lucky cho ô (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<import("@/types/mixed").AnswerMixedLuckySlotResponse> = await res.json();
  return data.data;
}

export async function fillMixedRemainder(
  req: import("@/types/mixed").FillRemainderRequest
): Promise<import("@/types/mixed").FillRemainderResponse> {
  const url = `${API_BASE_URL}/api/v1/mixed/fill-remainder`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.text();
    throw new Error(`Lỗi điền số còn lại (${res.status}): ${errorBody}`);
  }

  const data: ApiResponse<import("@/types/mixed").FillRemainderResponse> = await res.json();
  return data.data;
}

export async function checkBackendHealth(): Promise<{ status: string; ok: boolean }> {
  try {
    const res = await fetch(`${API_BASE_URL}/health`, {
      cache: "no-store",
      headers: { "Accept": "application/json" }
    });
    const text = await res.text();
    return { status: text, ok: res.ok };
  } catch {
    return { status: "Unreachable", ok: false };
  }
}

// ----------------------------------------------------
// Phase 3: Auth & User Slip APIs
// ----------------------------------------------------

let currentAccessToken: string | null = null;

export function setAuthToken(token: string | null) {
  currentAccessToken = token;
}

export function getAuthToken(): string | null {
  return currentAccessToken;
}

function getAuthHeaders(): Record<string, string> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    "Accept": "application/json"
  };
  if (currentAccessToken) {
    headers["Authorization"] = `Bearer ${currentAccessToken}`;
  }
  return headers;
}

export async function apiRegister(req: import("@/types/auth").RegisterRequest): Promise<import("@/types/auth").AuthResponse> {
  const url = `${API_BASE_URL}/api/v1/auth/register`;
  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json", "Accept": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || `Đăng ký thất bại (${res.status})`);
  }

  setAuthToken(data.data.accessToken);
  return data.data;
}

export async function apiLogin(req: import("@/types/auth").LoginRequest): Promise<import("@/types/auth").AuthResponse> {
  const url = `${API_BASE_URL}/api/v1/auth/login`;
  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json", "Accept": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || `Đăng nhập thất bại (${res.status})`);
  }

  setAuthToken(data.data.accessToken);
  return data.data;
}

export async function apiRefreshToken(): Promise<import("@/types/auth").AuthResponse> {
  const url = `${API_BASE_URL}/api/v1/auth/refresh`;
  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json", "Accept": "application/json" },
    credentials: "include",
    body: JSON.stringify({})
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    setAuthToken(null);
    throw new Error(data.message || "Phiên đăng nhập đã hết hạn.");
  }

  setAuthToken(data.data.accessToken);
  return data.data;
}

export async function apiLogout(): Promise<void> {
  const url = `${API_BASE_URL}/api/v1/auth/logout`;
  try {
    await fetch(url, {
      method: "POST",
      headers: getAuthHeaders(),
      credentials: "include"
    });
  } finally {
    setAuthToken(null);
  }
}

export async function apiGetCurrentUser(): Promise<import("@/types/auth").UserDto> {
  const url = `${API_BASE_URL}/api/v1/auth/me`;
  const res = await fetch(url, {
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể tải thông tin người dùng.");
  }
  return data.data;
}

export async function apiSaveSlip(req: import("@/types/savedSlip").SaveSlipRequest): Promise<import("@/types/savedSlip").SavedSlipSummary> {
  const url = `${API_BASE_URL}/api/v1/user/slips`;
  const res = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || `Lưu vé thất bại (${res.status})`);
  }
  return data.data;
}

export async function apiGetUserSlips(
  page = 1,
  pageSize = 10,
  isFavorite = false
): Promise<import("@/types/savedSlip").PagedResult<import("@/types/savedSlip").SavedSlipSummary>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    isFavorite: isFavorite.toString()
  });
  const url = `${API_BASE_URL}/api/v1/user/slips?${params.toString()}`;
  const res = await fetch(url, {
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể tải danh sách vé.");
  }
  return data.data;
}

export async function apiGetSlipDetail(id: string): Promise<import("@/types/savedSlip").SavedSlipDetail> {
  const url = `${API_BASE_URL}/api/v1/user/slips/${encodeURIComponent(id)}`;
  const res = await fetch(url, {
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể tải chi tiết vé.");
  }
  return data.data;
}

export async function apiToggleFavoriteSlip(id: string): Promise<{ slipId: string; isFavorite: boolean }> {
  const url = `${API_BASE_URL}/api/v1/user/slips/${encodeURIComponent(id)}/favorite`;
  const res = await fetch(url, {
    method: "PUT",
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể cập nhật yêu thích.");
  }
  return data.data;
}

export async function apiDeleteSlip(id: string): Promise<void> {
  const url = `${API_BASE_URL}/api/v1/user/slips/${encodeURIComponent(id)}`;
  const res = await fetch(url, {
    method: "DELETE",
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể xóa vé.");
  }
}

export async function apiGetUserHistory(
  page = 1,
  pageSize = 20
): Promise<import("@/types/savedSlip").PagedResult<import("@/types/savedSlip").UserActivityItem>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString()
  });
  const url = `${API_BASE_URL}/api/v1/user/history?${params.toString()}`;
  const res = await fetch(url, {
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể tải lịch sử hoạt động.");
  }
  return data.data;
}

// ==========================================
// Phase 5: Lucky DNA, Daily Journey & Remix
// ==========================================

export async function apiGetLuckyDna(guestSessionToken?: string): Promise<LuckyDna> {
  const params = new URLSearchParams();
  if (guestSessionToken) {
    params.append("guestSessionToken", guestSessionToken);
  }
  const url = `${API_BASE_URL}/api/v1/lucky-dna?${params.toString()}`;
  const res = await fetch(url, {
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể tải Lucky DNA.");
  }
  return data.data as LuckyDna;
}

export async function apiResetLuckyDna(): Promise<boolean> {
  const url = `${API_BASE_URL}/api/v1/lucky-dna/reset`;
  const res = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include"
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể reset Lucky DNA.");
  }
  return data.data;
}

export async function apiGetTodayDailyJourney(gameCode: string, guestSessionToken?: string): Promise<DailyJourneyStatusResponse | null> {
  const params = new URLSearchParams({ gameCode });
  if (guestSessionToken) {
    params.append("guestSessionToken", guestSessionToken);
  }
  const url = `${API_BASE_URL}/api/v1/daily-journeys?${params.toString()}`;
  const res = await fetch(url, {
    headers: getAuthHeaders(),
    credentials: "include"
  });

  if (res.status === 404) return null;

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể tải Daily Journey.");
  }
  return data.data as DailyJourneyStatusResponse;
}

export async function apiStartDailyJourney(req: StartJourneyRequest): Promise<StartJourneyResponse> {
  const url = `${API_BASE_URL}/api/v1/daily-journeys/start`;
  const res = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể bắt đầu Daily Journey.");
  }
  return data.data;
}

export async function apiAnswerDailyStep(journeyId: string, req: AnswerStepRequest): Promise<AnswerStepResponse> {
  const url = `${API_BASE_URL}/api/v1/daily-journeys/${encodeURIComponent(journeyId)}/answer`;
  const res = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Lỗi lưu câu trả lời Daily Journey.");
  }
  return data.data;
}

export async function apiQuickRemix(req: unknown): Promise<GenerateLineResponse> {
  const url = `${API_BASE_URL}/api/v1/remix/quick`;
  const res = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể thực hiện Quick Remix.");
  }
  return data.data;
}

export async function apiStartLuckyRemix(req: unknown): Promise<StartJourneyResponse> {
  const url = `${API_BASE_URL}/api/v1/remix/lucky/start`;
  const res = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Không thể khởi động Lucky Remix.");
  }
  return data.data;
}

export async function apiAnswerLuckyRemixStep(journeyId: string, req: AnswerStepRequest): Promise<AnswerStepResponse> {
  const url = `${API_BASE_URL}/api/v1/remix/lucky/${encodeURIComponent(journeyId)}/answer`;
  const res = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(req)
  });

  const data = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || "Lỗi lưu câu trả lời Lucky Remix.");
  }
  return data.data;
}


