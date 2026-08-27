"use client";

import React, { useState, useEffect } from "react";
import { AdminTheme, CreateThemeRequest, UpdateThemeRequest } from "@/types/admin";
import {
  getAdminThemes,
  createAdminTheme,
  updateAdminTheme,
  setAdminThemeStatus,
  deleteAdminTheme
} from "@/lib/adminApi";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { ThemeEditorModal } from "@/components/admin/ThemeEditorModal";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import {
  Palette,
  Plus,
  Search,
  Edit2,
  Trash2,
  Power,
  RefreshCw,
  Loader2,
  ChevronLeft,
  ChevronRight
} from "lucide-react";

export default function ThemesPage() {
  const [themes, setThemes] = useState<AdminTheme[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(15);
  const [search, setSearch] = useState("");
  const [isActiveFilter, setIsActiveFilter] = useState<string>("all");
  const [loading, setLoading] = useState(true);

  // Modals state
  const [selectedTheme, setSelectedTheme] = useState<AdminTheme | null>(null);
  const [editorOpen, setEditorOpen] = useState(false);
  const [themeToDelete, setThemeToDelete] = useState<AdminTheme | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const fetchThemes = React.useCallback(async () => {
    try {
      setLoading(true);
      const activeParam = isActiveFilter === "all" ? undefined : isActiveFilter === "active";
      const result = await getAdminThemes(page, pageSize, search.trim() || undefined, activeParam);
      setThemes(result.items);
      setTotalCount(result.totalCount);
    } catch (err: unknown) {
      console.error("Không thể tải danh sách chủ đề:", err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, isActiveFilter]);

  useEffect(() => {
    fetchThemes();
  }, [fetchThemes]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchThemes();
  };

  const handleSaveTheme = async (data: CreateThemeRequest | UpdateThemeRequest) => {
    if (selectedTheme) {
      await updateAdminTheme(selectedTheme.id, data as UpdateThemeRequest);
    } else {
      await createAdminTheme(data as CreateThemeRequest);
    }
    fetchThemes();
  };

  const handleToggleStatus = async (theme: AdminTheme) => {
    try {
      await setAdminThemeStatus(theme.id, !theme.isActive);
      fetchThemes();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Lỗi thay đổi trạng thái");
    }
  };

  const handleDeleteConfirm = async () => {
    if (!themeToDelete) return;
    try {
      setActionLoading(true);
      await deleteAdminTheme(themeToDelete.id);
      setThemeToDelete(null);
      fetchThemes();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Lỗi khi xóa chủ đề");
    } finally {
      setActionLoading(false);
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-zinc-100 flex items-center gap-2.5">
            <Palette className="h-6 w-6 text-amber-400" /> Quản lý Chủ đề Câu hỏi
          </h1>
          <p className="mt-1 text-sm text-zinc-400">
            Tạo và phân loại các chủ đề tâm linh/châm biếm cho Lucky Journey.
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedTheme(null);
            setEditorOpen(true);
          }}
          className="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-4 py-2.5 text-xs font-bold text-zinc-950 hover:bg-amber-400 transition shadow-lg shadow-amber-500/10"
        >
          <Plus className="h-4 w-4" /> Thêm chủ đề mới
        </button>
      </div>

      {/* Filters & Search */}
      <div className="flex flex-col sm:flex-row gap-3">
        <form onSubmit={handleSearch} className="relative flex-1">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-500" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tìm theo tên hoặc mã chủ đề..."
            className="w-full rounded-xl border border-zinc-800 bg-zinc-900/80 pl-10 pr-4 py-2 text-sm text-zinc-200 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
          />
        </form>

        <select
          value={isActiveFilter}
          onChange={(e) => {
            setIsActiveFilter(e.target.value);
            setPage(1);
          }}
          className="rounded-xl border border-zinc-800 bg-zinc-900 px-4 py-2 text-sm text-zinc-300 focus:border-amber-500 focus:outline-none"
        >
          <option value="all">Tất cả trạng thái</option>
          <option value="active">Đang hoạt động</option>
          <option value="inactive">Đã tạm dừng</option>
        </select>

        <button
          onClick={fetchThemes}
          className="inline-flex items-center gap-1.5 rounded-xl border border-zinc-800 bg-zinc-900 px-3.5 py-2 text-xs font-medium text-zinc-300 hover:bg-zinc-800"
        >
          <RefreshCw className={`h-3.5 w-3.5 ${loading ? "animate-spin" : ""}`} /> Làm mới
        </button>
      </div>

      {/* Table */}
      <div className="rounded-2xl border border-zinc-800 bg-zinc-900/40 overflow-hidden shadow-xl">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-zinc-300">
            <thead className="border-b border-zinc-800 bg-zinc-950/60 text-xs font-semibold text-zinc-400 uppercase tracking-wider">
              <tr>
                <th className="px-5 py-3.5">Mã (Code)</th>
                <th className="px-5 py-3.5">Tên chủ đề</th>
                <th className="px-5 py-3.5">Mô tả</th>
                <th className="px-5 py-3.5 text-center">Thứ tự</th>
                <th className="px-5 py-3.5 text-center">Số câu hỏi</th>
                <th className="px-5 py-3.5 text-center">Trạng thái</th>
                <th className="px-5 py-3.5 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-800/60">
              {loading ? (
                <tr>
                  <td colSpan={7} className="px-5 py-12 text-center text-zinc-400">
                    <Loader2 className="mx-auto h-6 w-6 animate-spin text-amber-500 mb-2" />
                    Đang tải dữ liệu...
                  </td>
                </tr>
              ) : themes.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-5 py-12 text-center text-zinc-500">
                    Không tìm thấy chủ đề nào phù hợp.
                  </td>
                </tr>
              ) : (
                themes.map((theme) => (
                  <tr key={theme.id} className="hover:bg-zinc-800/30 transition">
                    <td className="px-5 py-4 font-mono text-xs font-bold text-amber-400">
                      {theme.code}
                    </td>
                    <td className="px-5 py-4 font-medium text-zinc-100">
                      <div className="flex items-center gap-2">
                        {theme.icon && <span>{theme.icon}</span>}
                        <span>{theme.name}</span>
                      </div>
                    </td>
                    <td className="px-5 py-4 text-xs text-zinc-400 max-w-xs truncate">
                      {theme.description || "—"}
                    </td>
                    <td className="px-5 py-4 text-xs text-center font-mono text-zinc-400">
                      {theme.sortOrder}
                    </td>
                    <td className="px-5 py-4 text-xs text-center font-bold text-zinc-200">
                      {theme.questionsCount}
                    </td>
                    <td className="px-5 py-4 text-center">
                      <StatusBadge isActive={theme.isActive} size="sm" />
                    </td>
                    <td className="px-5 py-4 text-right">
                      <div className="flex items-center justify-end gap-1.5">
                        <button
                          onClick={() => handleToggleStatus(theme)}
                          className={`rounded-lg p-1.5 transition ${
                            theme.isActive
                              ? "text-emerald-400 hover:bg-emerald-500/10"
                              : "text-zinc-500 hover:bg-zinc-800 hover:text-zinc-300"
                          }`}
                          title={theme.isActive ? "Tạm dừng chủ đề" : "Kích hoạt chủ đề"}
                        >
                          <Power className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => {
                            setSelectedTheme(theme);
                            setEditorOpen(true);
                          }}
                          className="rounded-lg p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-amber-400 transition"
                          title="Chỉnh sửa"
                        >
                          <Edit2 className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setThemeToDelete(theme)}
                          className="rounded-lg p-1.5 text-zinc-500 hover:bg-red-500/10 hover:text-red-400 transition"
                          title="Xóa chủ đề"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Footer */}
        <div className="flex items-center justify-between border-t border-zinc-800 px-5 py-3 text-xs text-zinc-400 bg-zinc-950/40">
          <span>Tổng số: <strong className="text-zinc-200">{totalCount}</strong> chủ đề</span>
          <div className="flex items-center gap-2">
            <span>Trang {page} / {totalPages}</span>
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1}
              className="rounded p-1 text-zinc-400 hover:bg-zinc-800 disabled:opacity-30"
            >
              <ChevronLeft className="h-4 w-4" />
            </button>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page >= totalPages}
              className="rounded p-1 text-zinc-400 hover:bg-zinc-800 disabled:opacity-30"
            >
              <ChevronRight className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Theme Editor Modal */}
      <ThemeEditorModal
        theme={selectedTheme}
        isOpen={editorOpen}
        onClose={() => setEditorOpen(false)}
        onSave={handleSaveTheme}
      />

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!themeToDelete}
        title="Xác nhận xóa chủ đề"
        message={`Bạn có chắc chắn muốn xóa chủ đề '${themeToDelete?.name}' (${themeToDelete?.code})? Nếu đã có câu hỏi liên kết, hệ thống sẽ yêu cầu bạn chuyển sang trạng thái Tạm dừng.`}
        isDestructive={true}
        isLoading={actionLoading}
        confirmLabel="Xóa chủ đề"
        onConfirm={handleDeleteConfirm}
        onCancel={() => setThemeToDelete(null)}
      />
    </div>
  );
}