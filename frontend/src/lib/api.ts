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
  AnswerStepResponse
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
