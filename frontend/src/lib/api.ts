import { ApiResponse, Game } from "@/types/game";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export async function fetchActiveGames(): Promise<Game[]> {
  try {
    // Try /api/v1/games first, then fallback to /api/games
    const url = `${API_BASE_URL}/api/v1/games`;
    const res = await fetch(url, {
      next: { revalidate: 30 },
      headers: {
        "Accept": "application/json"
      }
    });

    if (!res.ok) {
      // Fallback
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
