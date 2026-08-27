"use client";

import React, { useState, useEffect } from "react";
import {
  AdminQuestionList,
  AdminQuestionDetail,
  CreateQuestionRequest,
  UpdateQuestionRequest,
  QuestionType,
  AdminTheme
} from "@/types/admin";
import {
  getAdminQuestions,
  getAdminQuestionById,
  createAdminQuestion,
  updateAdminQuestion,
  duplicateAdminQuestion,
  setAdminQuestionStatus,
  deleteAdminQuestion,
  getAdminThemes
} from "@/lib/adminApi";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { QuestionEditorModal } from "@/components/admin/QuestionEditorModal";
import { QuestionPreviewModal } from "@/components/admin/QuestionPreviewModal";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import {
  HelpCircle,
  Plus,
  Search,
  Edit2,
  Copy,
  Eye,
  Trash2,
  Power,
  RefreshCw,
  Loader2,
  ChevronLeft,
  ChevronRight
} from "lucide-react";

export default function QuestionsPage() {
  const [questions, setQuestions] = useState<AdminQuestionList[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(15);
  const [search, setSearch] = useState("");
  const [selectedThemeId, setSelectedThemeId] = useState<number | undefined>(undefined);
  const [selectedType, setSelectedType] = useState<QuestionType | undefined>(undefined);
  const [selectedStatus, setSelectedStatus] = useState<string>("all");
  const [sortBy] = useState<string>("updatedAt_desc");
  const [loading, setLoading] = useState(true);

  const [themes, setThemes] = useState<AdminTheme[]>([]);

  // Modals state
  const [selectedQuestionDetail, setSelectedQuestionDetail] = useState<AdminQuestionDetail | null>(null);
  const [editorOpen, setEditorOpen] = useState(false);
  const [previewQuestion, setPreviewQuestion] = useState<AdminQuestionDetail | null>(null);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [questionToDelete, setQuestionToDelete] = useState<AdminQuestionList | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const loadThemes = async () => {
    try {
      const res = await getAdminThemes(1, 100);
      setThemes(res.items);
    } catch (err: unknown) {
      console.error("Lỗi tải danh sách chủ đề:", err);
    }
  };

  const fetchQuestions = React.useCallback(async () => {
    try {
      setLoading(true);
      const isActiveParam = selectedStatus === "all" ? undefined : selectedStatus === "active";
      const result = await getAdminQuestions({
        page,
        pageSize,
        search: search.trim() || undefined,
        themeId: selectedThemeId,
        questionType: selectedType,
        isActive: isActiveParam,
        sortBy
      });
      setQuestions(result.items);
      setTotalCount(result.totalCount);
    } catch (err: unknown) {
      console.error("Lỗi tải câu hỏi:", err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, selectedThemeId, selectedType, selectedStatus, sortBy]);

  useEffect(() => {
    loadThemes();
  }, []);

  useEffect(() => {
    fetchQuestions();
  }, [fetchQuestions]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchQuestions();
  };

  const handleOpenCreate = () => {
    setSelectedQuestionDetail(null);
    setEditorOpen(true);
  };

  const handleOpenEdit = async (q: AdminQuestionList) => {
    try {
      const detail = await getAdminQuestionById(q.id);
      setSelectedQuestionDetail(detail);
      setEditorOpen(true);
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Không thể tải chi tiết câu hỏi");
    }
  };

  const handleOpenPreview = async (q: AdminQuestionList) => {
    try {
      const detail = await getAdminQuestionById(q.id);
      setPreviewQuestion(detail);
      setPreviewOpen(true);
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Không thể mở xem trước câu hỏi");
    }
  };

  const handleSaveQuestion = async (data: CreateQuestionRequest | UpdateQuestionRequest) => {
    if (selectedQuestionDetail) {
      await updateAdminQuestion(selectedQuestionDetail.id, data as UpdateQuestionRequest);
    } else {
      await createAdminQuestion(data as CreateQuestionRequest);
    }
    fetchQuestions();
  };

  const handleDuplicate = async (q: AdminQuestionList) => {
    try {
      setActionLoading(true);
      await duplicateAdminQuestion(q.id);
      fetchQuestions();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Lỗi khi nhân bản câu hỏi");
    } finally {
      setActionLoading(false);
    }
  };

  const handleToggleStatus = async (q: AdminQuestionList) => {
    try {
      await setAdminQuestionStatus(q.id, !q.isActive);
      fetchQuestions();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Lỗi đổi trạng thái câu hỏi");
    }
  };

  const handleDeleteConfirm = async () => {
    if (!questionToDelete) return;
    try {
      setActionLoading(true);
      await deleteAdminQuestion(questionToDelete.id);
      setQuestionToDelete(null);
      fetchQuestions();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Lỗi khi xóa câu hỏi");
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
            <HelpCircle className="h-6 w-6 text-amber-400" /> Ngân hàng Câu hỏi Lucky Journey
          </h1>
          <p className="mt-1 text-sm text-zinc-400">
            Quản lý kho câu hỏi tương tác, các lựa chọn và phân bổ trọng số thuộc tính tâm lý.
          </p>
        </div>
        <button
          onClick={handleOpenCreate}
          className="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-4 py-2.5 text-xs font-bold text-zinc-950 hover:bg-amber-400 transition shadow-lg shadow-amber-500/10"
        >
          <Plus className="h-4 w-4" /> Thêm câu hỏi mới
        </button>
      </div>

      {/* Filters Bar */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
        {/* Search */}
        <form onSubmit={handleSearch} className="relative lg:col-span-2">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-500" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tìm theo nội dung câu hỏi..."
            className="w-full rounded-xl border border-zinc-800 bg-zinc-900/80 pl-10 pr-4 py-2 text-sm text-zinc-200 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
          />
        </form>

        {/* Theme filter */}
        <select
          value={selectedThemeId ?? 0}
          onChange={(e) => {
            const val = parseInt(e.target.value);
            setSelectedThemeId(val === 0 ? undefined : val);
            setPage(1);
          }}
          className="rounded-xl border border-zinc-800 bg-zinc-900 px-3.5 py-2 text-xs text-zinc-300 focus:border-amber-500 focus:outline-none"
        >
          <option value="0">Tất cả chủ đề</option>
          {themes.map((t) => (
            <option key={t.id} value={t.id}>
              {t.name}
            </option>
          ))}
        </select>

        {/* Question Type */}
        <select
          value={selectedType ?? ""}
          onChange={(e) => {
            const val = e.target.value;
            setSelectedType(val === "" ? undefined : (val as QuestionType));
            setPage(1);
          }}
          className="rounded-xl border border-zinc-800 bg-zinc-900 px-3.5 py-2 text-xs text-zinc-300 focus:border-amber-500 focus:outline-none"
        >
          <option value="">Tất cả loại câu hỏi</option>
          <option value="SingleChoice">SingleChoice (Trắc nghiệm)</option>
          <option value="ThisOrThat">ThisOrThat (1 trong 2)</option>
          <option value="Scenario">Scenario (Tình huống)</option>
          <option value="QuickInstinct">QuickInstinct (Trực giác)</option>
        </select>

        {/* Status & Sort */}
        <div className="flex gap-2">
          <select
            value={selectedStatus}
            onChange={(e) => {
              setSelectedStatus(e.target.value);
              setPage(1);
            }}
            className="flex-1 rounded-xl border border-zinc-800 bg-zinc-900 px-3 py-2 text-xs text-zinc-300 focus:border-amber-500 focus:outline-none"
          >
            <option value="all">Trạng thái: Tất cả</option>
            <option value="active">Hoạt động</option>
            <option value="inactive">Tạm dừng</option>
          </select>

          <button
            onClick={fetchQuestions}
            className="rounded-xl border border-zinc-800 bg-zinc-900 px-3 py-2 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200"
            title="Làm mới"
          >
            <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
          </button>
        </div>
      </div>

      {/* Questions Table */}
      <div className="rounded-2xl border border-zinc-800 bg-zinc-900/40 overflow-hidden shadow-xl">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-zinc-300">
            <thead className="border-b border-zinc-800 bg-zinc-950/60 text-xs font-semibold text-zinc-400 uppercase tracking-wider">
              <tr>
                <th className="px-5 py-3.5">ID</th>
                <th className="px-5 py-3.5">Chủ đề</th>
                <th className="px-5 py-3.5">Nội dung câu hỏi</th>
                <th className="px-5 py-3.5">Loại</th>
                <th className="px-5 py-3.5 text-center">Lựa chọn</th>
                <th className="px-5 py-3.5 text-center">Lượt xem</th>
                <th className="px-5 py-3.5 text-center">Trạng thái</th>
                <th className="px-5 py-3.5 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-800/60">
              {loading ? (
                <tr>
                  <td colSpan={8} className="px-5 py-12 text-center text-zinc-400">
                    <Loader2 className="mx-auto h-6 w-6 animate-spin text-amber-500 mb-2" />
                    Đang tải danh sách câu hỏi...
                  </td>
                </tr>
              ) : questions.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-5 py-12 text-center text-zinc-500">
                    Không tìm thấy câu hỏi nào.
                  </td>
                </tr>
              ) : (
                questions.map((q) => (
                  <tr key={q.id} className="hover:bg-zinc-800/30 transition">
                    <td className="px-5 py-4 font-mono text-xs text-zinc-500">#{q.id}</td>
                    <td className="px-5 py-4">
                      <span className="rounded-full bg-amber-500/10 px-2.5 py-0.5 text-xs font-semibold text-amber-400 border border-amber-500/20 whitespace-nowrap">
                        {q.themeName || q.themeCode}
                      </span>
                    </td>
                    <td className="px-5 py-4 font-medium text-zinc-100 max-w-md">
                      <div className="line-clamp-2">{q.content}</div>
                      {q.subtitle && (
                        <div className="text-xs text-zinc-500 italic mt-0.5 truncate">{q.subtitle}</div>
                      )}
                    </td>
                    <td className="px-5 py-4 text-xs font-mono text-zinc-400 whitespace-nowrap">
                      {q.questionType}
                    </td>
                    <td className="px-5 py-4 text-xs text-center font-bold">
                      <span className={q.activeChoicesCount < 2 ? "text-red-400" : "text-zinc-200"}>
                        {q.activeChoicesCount} / {q.choicesCount}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-xs text-center font-mono text-zinc-400">
                      {q.viewCount}
                    </td>
                    <td className="px-5 py-4 text-center">
                      <StatusBadge isActive={q.isActive} size="sm" />
                    </td>
                    <td className="px-5 py-4 text-right">
                      <div className="flex items-center justify-end gap-1.5">
                        <button
                          onClick={() => handleOpenPreview(q)}
                          className="rounded-lg p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-amber-300 transition"
                          title="Xem trước giao diện người chơi"
                        >
                          <Eye className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleToggleStatus(q)}
                          className={`rounded-lg p-1.5 transition ${
                            q.isActive
                              ? "text-emerald-400 hover:bg-emerald-500/10"
                              : "text-zinc-500 hover:bg-zinc-800 hover:text-zinc-300"
                          }`}
                          title={q.isActive ? "Tạm dừng câu hỏi" : "Kích hoạt câu hỏi"}
                        >
                          <Power className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleDuplicate(q)}
                          className="rounded-lg p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-blue-400 transition"
                          title="Nhân bản câu hỏi (Clone)"
                        >
                          <Copy className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleOpenEdit(q)}
                          className="rounded-lg p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-amber-400 transition"
                          title="Chỉnh sửa"
                        >
                          <Edit2 className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setQuestionToDelete(q)}
                          className="rounded-lg p-1.5 text-zinc-500 hover:bg-red-500/10 hover:text-red-400 transition"
                          title="Xóa câu hỏi"
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
          <span>Tổng số: <strong className="text-zinc-200">{totalCount}</strong> câu hỏi</span>
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

      {/* Modals */}
      <QuestionEditorModal
        question={selectedQuestionDetail}
        isOpen={editorOpen}
        onClose={() => setEditorOpen(false)}
        onSave={handleSaveQuestion}
      />

      <QuestionPreviewModal
        question={previewQuestion}
        isOpen={previewOpen}
        onClose={() => setPreviewOpen(false)}
      />

      <ConfirmDialog
        isOpen={!!questionToDelete}
        title="Xác nhận xóa câu hỏi"
        message={`Bạn có chắc chắn muốn xóa câu hỏi #${questionToDelete?.id} ("${questionToDelete?.content}")? Nếu người dùng đã từng trả lời câu hỏi này trong lịch sử vé, hệ thống sẽ tự động chuyển sang trạng thái Vô hiệu hóa để bảo toàn dữ liệu snapshot.`}
        isDestructive={true}
        isLoading={actionLoading}
        confirmLabel="Xóa câu hỏi"
        onConfirm={handleDeleteConfirm}
        onCancel={() => setQuestionToDelete(null)}
      />
    </div>
  );
}