import {
  AdminDashboardMetrics,
  AdminTheme,
  CreateThemeRequest,
  UpdateThemeRequest,
  AdminTrait,
  CreateTraitRequest,
  UpdateTraitRequest,
  AdminQuestionList,
  AdminQuestionDetail,
  CreateQuestionRequest,
  UpdateQuestionRequest,
  QuestionFilterParams,
  ImportValidationResult,
  ImportConfirmResponse,
  ImportQuestionPreview,
  AdminSettings,
  AdminUser,
  PagedResult
} from "@/types/admin";
import { ApiResponse } from "@/types/game";
import { getAuthToken } from "./api";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

function getAuthHeaders(): Record<string, string> {
  const token = getAuthToken();
  const headers: Record<string, string> = {
    "Accept": "application/json"
  };
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  return headers;
}

// ----------------------------------------------------
// 1. Dashboard
// ----------------------------------------------------
export async function getAdminDashboard(): Promise<AdminDashboardMetrics> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/dashboard`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminDashboardMetrics> = await res.json();
  if (!res.ok || !data.success) {
    throw new Error(data.message || `Lỗi tải Dashboard (${res.status})`);
  }
  return data.data;
}

// ----------------------------------------------------
// 2. Themes
// ----------------------------------------------------
export async function getAdminThemes(page = 1, pageSize = 20, search?: string, isActive?: boolean): Promise<PagedResult<AdminTheme>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString()
  });
  if (search) params.append("search", search);
  if (isActive !== undefined) params.append("isActive", isActive.toString());

  const res = await fetch(`${API_BASE_URL}/api/v1/admin/themes?${params.toString()}`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<PagedResult<AdminTheme>> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tải danh sách chủ đề");
  return data.data;
}

export async function getAdminThemeById(id: number): Promise<AdminTheme> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/themes/${id}`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminTheme> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Không tìm thấy chủ đề");
  return data.data;
}

export async function createAdminTheme(req: CreateThemeRequest): Promise<AdminTheme> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/themes`, {
    method: "POST",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });
  const data: ApiResponse<AdminTheme> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tạo chủ đề");
  return data.data;
}

export async function updateAdminTheme(id: number, req: UpdateThemeRequest): Promise<AdminTheme> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/themes/${id}`, {
    method: "PUT",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });
  const data: ApiResponse<AdminTheme> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi cập nhật chủ đề");
  return data.data;
}

export async function setAdminThemeStatus(id: number, isActive: boolean): Promise<AdminTheme> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/themes/${id}/status`, {
    method: "PATCH",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ isActive })
  });
  const data: ApiResponse<AdminTheme> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi đổi trạng thái chủ đề");
  return data.data;
}

export async function deleteAdminTheme(id: number): Promise<void> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/themes/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<string> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi xóa chủ đề");
}

// ----------------------------------------------------
// 3. Traits
// ----------------------------------------------------
export async function getAdminTraits(page = 1, pageSize = 20, search?: string, isActive?: boolean): Promise<PagedResult<AdminTrait>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString()
  });
  if (search) params.append("search", search);
  if (isActive !== undefined) params.append("isActive", isActive.toString());

  const res = await fetch(`${API_BASE_URL}/api/v1/admin/traits?${params.toString()}`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<PagedResult<AdminTrait>> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tải danh sách thuộc tính");
  return data.data;
}

export async function getAllActiveTraits(): Promise<AdminTrait[]> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/traits/active`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminTrait[]> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tải thuộc tính");
  return data.data;
}

export async function createAdminTrait(req: CreateTraitRequest): Promise<AdminTrait> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/traits`, {
    method: "POST",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });
  const data: ApiResponse<AdminTrait> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tạo thuộc tính");
  return data.data;
}

export async function updateAdminTrait(id: number, req: UpdateTraitRequest): Promise<AdminTrait> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/traits/${id}`, {
    method: "PUT",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });
  const data: ApiResponse<AdminTrait> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi cập nhật thuộc tính");
  return data.data;
}

export async function setAdminTraitStatus(id: number, isActive: boolean): Promise<AdminTrait> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/traits/${id}/status`, {
    method: "PATCH",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ isActive })
  });
  const data: ApiResponse<AdminTrait> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi đổi trạng thái thuộc tính");
  return data.data;
}

export async function deleteAdminTrait(id: number): Promise<void> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/traits/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<string> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi xóa thuộc tính");
}

// ----------------------------------------------------
// 4. Questions
// ----------------------------------------------------
export async function getAdminQuestions(params: QuestionFilterParams): Promise<PagedResult<AdminQuestionList>> {
  const qParams = new URLSearchParams({
    page: (params.page || 1).toString(),
    pageSize: (params.pageSize || 20).toString()
  });
  if (params.search) qParams.append("search", params.search);
  if (params.themeId) qParams.append("themeId", params.themeId.toString());
  if (params.questionType) qParams.append("questionType", params.questionType);
  if (params.isActive !== undefined) qParams.append("isActive", params.isActive.toString());
  if (params.sortBy) qParams.append("sortBy", params.sortBy);

  const res = await fetch(`${API_BASE_URL}/api/v1/admin/questions?${qParams.toString()}`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<PagedResult<AdminQuestionList>> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tải danh sách câu hỏi");
  return data.data;
}

export async function getAdminQuestionById(id: number): Promise<AdminQuestionDetail> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/questions/${id}`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminQuestionDetail> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Không tìm thấy câu hỏi");
  return data.data;
}

export async function createAdminQuestion(req: CreateQuestionRequest): Promise<AdminQuestionDetail> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/questions`, {
    method: "POST",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });
  const data: ApiResponse<AdminQuestionDetail> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tạo câu hỏi");
  return data.data;
}

export async function updateAdminQuestion(id: number, req: UpdateQuestionRequest): Promise<AdminQuestionDetail> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/questions/${id}`, {
    method: "PUT",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(req)
  });
  const data: ApiResponse<AdminQuestionDetail> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi cập nhật câu hỏi");
  return data.data;
}

export async function duplicateAdminQuestion(id: number): Promise<AdminQuestionDetail> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/questions/${id}/duplicate`, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminQuestionDetail> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi nhân bản câu hỏi");
  return data.data;
}

export async function setAdminQuestionStatus(id: number, isActive: boolean): Promise<AdminQuestionDetail> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/questions/${id}/status`, {
    method: "PATCH",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ isActive })
  });
  const data: ApiResponse<AdminQuestionDetail> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi đổi trạng thái câu hỏi");
  return data.data;
}

export async function deleteAdminQuestion(id: number): Promise<void> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/questions/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<string> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi xóa câu hỏi");
}

// ----------------------------------------------------
// 5. Import
// ----------------------------------------------------
export async function validateImportFile(file: File): Promise<ImportValidationResult> {
  const formData = new FormData();
  formData.append("file", file);

  const token = getAuthToken();
  const headers: Record<string, string> = { "Accept": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE_URL}/api/v1/admin/import/validate`, {
    method: "POST",
    headers,
    credentials: "include",
    body: formData
  });
  const data: ApiResponse<ImportValidationResult> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi phân tích tệp tin");
  return data.data;
}

export async function confirmImport(sessionId?: string, items?: ImportQuestionPreview[]): Promise<ImportConfirmResponse> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/import/confirm`, {
    method: "POST",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ importSessionId: sessionId, items })
  });
  const data: ApiResponse<ImportConfirmResponse> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi nhập dữ liệu");
  return data.data;
}

export function getTemplateDownloadUrl(format: "csv" | "xlsx" | "json"): string {
  return `${API_BASE_URL}/api/v1/admin/import/template?format=${format}`;
}

// ----------------------------------------------------
// 6. Settings
// ----------------------------------------------------
export async function getAdminSettings(): Promise<AdminSettings> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/settings`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminSettings> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tải cấu hình");
  return data.data;
}

export async function updateAdminSettings(settings: AdminSettings): Promise<AdminSettings> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/settings`, {
    method: "PUT",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(settings)
  });
  const data: ApiResponse<AdminSettings> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi cập nhật cấu hình");
  return data.data;
}

export async function resetAdminSettings(): Promise<AdminSettings> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/settings/reset`, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminSettings> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi khôi phục cấu hình");
  return data.data;
}

// ----------------------------------------------------
// 7. Users
// ----------------------------------------------------
export async function getAdminUsers(page = 1, pageSize = 20, search?: string, isActive?: boolean, role?: string): Promise<PagedResult<AdminUser>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString()
  });
  if (search) params.append("search", search);
  if (isActive !== undefined) params.append("isActive", isActive.toString());
  if (role) params.append("role", role);

  const res = await fetch(`${API_BASE_URL}/api/v1/admin/users?${params.toString()}`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<PagedResult<AdminUser>> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi tải danh sách người dùng");
  return data.data;
}

export async function getAdminUserById(id: string): Promise<AdminUser> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/users/${encodeURIComponent(id)}`, {
    headers: getAuthHeaders(),
    credentials: "include"
  });
  const data: ApiResponse<AdminUser> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Không tìm thấy người dùng");
  return data.data;
}

export async function setAdminUserStatus(id: string, isActive: boolean): Promise<AdminUser> {
  const res = await fetch(`${API_BASE_URL}/api/v1/admin/users/${encodeURIComponent(id)}/status`, {
    method: "PATCH",
    headers: { ...getAuthHeaders(), "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ isActive })
  });
  const data: ApiResponse<AdminUser> = await res.json();
  if (!res.ok || !data.success) throw new Error(data.message || "Lỗi đổi trạng thái người dùng");
  return data.data;
}