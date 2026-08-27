"use client";

import React, { useState, useEffect } from "react";
import {
  AdminQuestionDetail,
  CreateQuestionRequest,
  UpdateQuestionRequest,
  QuestionType,
  AdminTheme,
  AdminTrait
} from "@/types/admin";
import { getAdminThemes, getAllActiveTraits } from "@/lib/adminApi";
import { X, Plus, Trash2, Loader2, Sparkles } from "lucide-react";

interface QuestionEditorModalProps {
  question: AdminQuestionDetail | null;
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreateQuestionRequest | UpdateQuestionRequest) => Promise<void>;
}

interface LocalChoice {
  id?: number;
  content: string;
  subContent: string;
  orderIndex: number;
  isActive: boolean;
  choiceTraits: { traitId: number; weight: number }[];
}

export function QuestionEditorModal({ question, isOpen, onClose, onSave }: QuestionEditorModalProps) {
  const [themeId, setThemeId] = useState<number>(0);
  const [questionType, setQuestionType] = useState<QuestionType>("SingleChoice");
  const [content, setContent] = useState("");
  const [subtitle, setSubtitle] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [choices, setChoices] = useState<LocalChoice[]>([]);

  const [themes, setThemes] = useState<AdminTheme[]>([]);
  const [availableTraits, setAvailableTraits] = useState<AdminTrait[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEditing = !!question;

  const loadMetadata = React.useCallback(async () => {
    try {
      const [themesRes, traitsRes] = await Promise.all([
        getAdminThemes(1, 100, undefined, true),
        getAllActiveTraits()
      ]);
      setThemes(themesRes.items);
      setAvailableTraits(traitsRes);
      if (!isEditing && themesRes.items.length > 0 && themeId === 0) {
        setThemeId(themesRes.items[0].id);
      }
    } catch (err: unknown) {
      console.error("Error loading metadata:", err);
    }
  }, [isEditing, themeId]);

  useEffect(() => {
    if (isOpen) {
      loadMetadata();
    }
  }, [isOpen, loadMetadata]);

  useEffect(() => {
    if (question) {
      setThemeId(question.themeId);
      setQuestionType(question.questionType);
      setContent(question.content);
      setSubtitle(question.subtitle || "");
      setIsActive(question.isActive);
      setChoices(
        question.choices.map((c, idx) => ({
          id: c.id,
          content: c.content,
          subContent: c.subContent || "",
          orderIndex: c.orderIndex ?? idx,
          isActive: c.isActive,
          choiceTraits: c.choiceTraits.map((ct) => ({ traitId: ct.traitId, weight: ct.weight }))
        }))
      );
    } else {
      setThemeId(themes[0]?.id || 0);
      setQuestionType("SingleChoice");
      setContent("");
      setSubtitle("");
      setIsActive(true);
      setChoices([
        { content: "", subContent: "", orderIndex: 0, isActive: true, choiceTraits: [] },
        { content: "", subContent: "", orderIndex: 1, isActive: true, choiceTraits: [] }
      ]);
    }
    setError(null);
  }, [question, isOpen, themes]);

  if (!isOpen) return null;

  const handleAddChoice = () => {
    setChoices([
      ...choices,
      {
        content: "",
        subContent: "",
        orderIndex: choices.length,
        isActive: true,
        choiceTraits: []
      }
    ]);
  };

  const handleRemoveChoice = (index: number) => {
    if (choices.length <= 2) {
      setError("Câu hỏi cần duy trì ít nhất 2 lựa chọn.");
      return;
    }
    setChoices(choices.filter((_, i) => i !== index));
  };

  const handleChoiceChange = (index: number, field: keyof LocalChoice, value: string | number | boolean) => {
    const next = [...choices];
    next[index] = { ...next[index], [field]: value };
    setChoices(next);
  };

  const handleAddTraitToChoice = (choiceIndex: number, traitId: number) => {
    const choice = choices[choiceIndex];
    if (choice.choiceTraits.some((t) => t.traitId === traitId)) return;
    const next = [...choices];
    next[choiceIndex] = {
      ...choice,
      choiceTraits: [...choice.choiceTraits, { traitId, weight: 0.8 }]
    };
    setChoices(next);
  };

  const handleRemoveTraitFromChoice = (choiceIndex: number, traitId: number) => {
    const choice = choices[choiceIndex];
    const next = [...choices];
    next[choiceIndex] = {
      ...choice,
      choiceTraits: choice.choiceTraits.filter((t) => t.traitId !== traitId)
    };
    setChoices(next);
  };

  const handleTraitWeightChange = (choiceIndex: number, traitId: number, weight: number) => {
    const choice = choices[choiceIndex];
    const next = [...choices];
    next[choiceIndex] = {
      ...choice,
      choiceTraits: choice.choiceTraits.map((t) =>
        t.traitId === traitId ? { ...t, weight: Math.min(1.0, Math.max(0.0, weight)) } : t
      )
    };
    setChoices(next);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!themeId) {
      setError("Vui lòng chọn Chủ đề.");
      return;
    }
    if (!content.trim()) {
      setError("Vui lòng nhập nội dung câu hỏi.");
      return;
    }

    const activeChoices = choices.filter((c) => c.isActive && c.content.trim());
    if (isActive && activeChoices.length < 2) {
      setError("Câu hỏi đang kích hoạt phải có ít nhất 2 lựa chọn có nội dung và đang hoạt động.");
      return;
    }

    const payloadChoices = choices.map((c, idx) => ({
      content: c.content.trim(),
      subContent: c.subContent.trim() || null,
      orderIndex: idx,
      isActive: c.isActive,
      choiceTraits: c.choiceTraits.map((ct) => ({
        traitId: ct.traitId,
        weight: ct.weight
      }))
    }));

    try {
      setLoading(true);
      await onSave({
        themeId,
        questionType,
        content: content.trim(),
        subtitle: subtitle.trim() || null,
        isActive,
        choices: payloadChoices
      });
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Đã xảy ra lỗi khi lưu câu hỏi");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/75 p-4 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="flex w-full max-w-3xl flex-col rounded-2xl border border-zinc-800 bg-zinc-900 shadow-2xl overflow-hidden max-h-[92vh]">
        {/* Modal Header */}
        <div className="flex items-center justify-between border-b border-zinc-800 px-6 py-4 bg-zinc-950/60">
          <h3 className="font-bold text-zinc-100">
            {isEditing ? `Chỉnh sửa Câu hỏi #${question.id}` : "Tạo Câu hỏi mới"}
          </h3>
          <button onClick={onClose} className="rounded-lg p-1 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200">
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Modal Body */}
        <form onSubmit={handleSubmit} className="overflow-y-auto p-6 space-y-6">
          {error && (
            <div className="rounded-lg bg-red-500/10 p-3 text-xs text-red-400 border border-red-500/20">
              {error}
            </div>
          )}

          {/* Top row: Theme & Question Type */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-zinc-300 mb-1.5">
                Chủ đề (Theme) <span className="text-red-400">*</span>
              </label>
              <select
                value={themeId}
                onChange={(e) => setThemeId(parseInt(e.target.value))}
                className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
              >
                {themes.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name} ({t.code})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-semibold text-zinc-300 mb-1.5">
                Loại câu hỏi (Question Type) <span className="text-red-400">*</span>
              </label>
              <select
                value={questionType}
                onChange={(e) => setQuestionType(e.target.value as QuestionType)}
                className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
              >
                <option value="SingleChoice">SingleChoice (Trắc nghiệm đơn)</option>
                <option value="ThisOrThat">ThisOrThat (Chọn 1 trong 2)</option>
                <option value="Scenario">Scenario (Tình huống)</option>
                <option value="QuickInstinct">QuickInstinct (Trực giác nhanh)</option>
              </select>
            </div>
          </div>

          {/* Question text */}
          <div>
            <label className="block text-xs font-semibold text-zinc-300 mb-1.5">
              Nội dung câu hỏi <span className="text-red-400">*</span>
            </label>
            <textarea
              rows={2}
              required
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="VD: Sếp giao deadline gấp lúc 17h30 chiều thứ Sáu, bạn sẽ..."
              className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none resize-none"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-zinc-300 mb-1.5">Phụ đề / Ngữ cảnh gợi ý</label>
            <input
              type="text"
              value={subtitle}
              onChange={(e) => setSubtitle(e.target.value)}
              placeholder="VD: Chọn phản ứng bản năng nhất của bạn"
              className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
            />
          </div>

          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              id="qActive"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              className="h-4 w-4 rounded border-zinc-700 bg-zinc-950 text-amber-500 focus:ring-amber-500"
            />
            <label htmlFor="qActive" className="text-sm font-medium text-zinc-300">
              Kích hoạt câu hỏi trong hệ thống
            </label>
          </div>

          {/* Choices Section */}
          <div className="space-y-4 pt-2 border-t border-zinc-800">
            <div className="flex items-center justify-between">
              <h4 className="text-sm font-bold text-zinc-200">Danh sách Lựa chọn & Thuộc tính (Traits)</h4>
              <button
                type="button"
                onClick={handleAddChoice}
                className="inline-flex items-center gap-1.5 rounded-lg bg-amber-500/10 px-3 py-1.5 text-xs font-semibold text-amber-400 border border-amber-500/20 hover:bg-amber-500/20 transition"
              >
                <Plus className="h-3.5 w-3.5" /> Thêm lựa chọn
              </button>
            </div>

            <div className="space-y-3">
              {choices.map((choice, cIdx) => (
                <div
                  key={cIdx}
                  className="rounded-xl border border-zinc-800 bg-zinc-950/60 p-4 space-y-3 relative group"
                >
                  <div className="flex items-center justify-between gap-3">
                    <span className="text-xs font-bold text-amber-400/90">Lựa chọn #{cIdx + 1}</span>
                    <div className="flex items-center gap-2">
                      <label className="flex items-center gap-1.5 text-xs text-zinc-400">
                        <input
                          type="checkbox"
                          checked={choice.isActive}
                          onChange={(e) => handleChoiceChange(cIdx, "isActive", e.target.checked)}
                          className="h-3.5 w-3.5 rounded border-zinc-700 bg-zinc-900 text-amber-500"
                        />
                        Hoạt động
                      </label>
                      <button
                        type="button"
                        onClick={() => handleRemoveChoice(cIdx)}
                        className="rounded p-1 text-zinc-500 hover:text-red-400 hover:bg-zinc-800 transition"
                        title="Xóa lựa chọn"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                    <input
                      type="text"
                      required
                      value={choice.content}
                      onChange={(e) => handleChoiceChange(cIdx, "content", e.target.value)}
                      placeholder="Nội dung lựa chọn..."
                      className="w-full rounded-lg border border-zinc-800 bg-zinc-900 px-3 py-2 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
                    />
                    <input
                      type="text"
                      value={choice.subContent}
                      onChange={(e) => handleChoiceChange(cIdx, "subContent", e.target.value)}
                      placeholder="Ghi chú phụ (tùy chọn)..."
                      className="w-full rounded-lg border border-zinc-800 bg-zinc-900 px-3 py-2 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
                    />
                  </div>

                  {/* Choice Traits */}
                  <div className="pt-2 border-t border-zinc-800/60">
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-[11px] font-semibold text-zinc-400 flex items-center gap-1">
                        <Sparkles className="h-3 w-3 text-amber-400" /> Trọng số thuộc tính (0.0 - 1.0):
                      </span>
                      {/* Trait picker */}
                      <select
                        onChange={(e) => {
                          const val = parseInt(e.target.value);
                          if (val > 0) {
                            handleAddTraitToChoice(cIdx, val);
                            e.target.value = "0";
                          }
                        }}
                        defaultValue="0"
                        className="rounded bg-zinc-900 px-2 py-1 text-xs text-zinc-300 border border-zinc-800 focus:outline-none"
                      >
                        <option value="0">+ Gắn thuộc tính...</option>
                        {availableTraits
                          .filter((at) => !choice.choiceTraits.some((ct) => ct.traitId === at.id))
                          .map((at) => (
                            <option key={at.id} value={at.id}>
                              {at.name} ({at.code})
                            </option>
                          ))}
                      </select>
                    </div>

                    <div className="space-y-2">
                      {choice.choiceTraits.map((ct) => {
                        const traitInfo = availableTraits.find((t) => t.id === ct.traitId);
                        return (
                          <div
                            key={ct.traitId}
                            className="flex items-center justify-between gap-3 rounded-lg bg-zinc-900/90 px-3 py-1.5 text-xs border border-zinc-800"
                          >
                            <span className="font-medium text-zinc-200 min-w-[100px]">
                              {traitInfo?.name || `Trait #${ct.traitId}`}
                            </span>
                            <div className="flex items-center gap-2 flex-1 max-w-[200px]">
                              <input
                                type="range"
                                min="0.0"
                                max="1.0"
                                step="0.05"
                                value={ct.weight}
                                onChange={(e) =>
                                  handleTraitWeightChange(cIdx, ct.traitId, parseFloat(e.target.value))
                                }
                                className="w-full accent-amber-500"
                              />
                              <span className="font-mono text-[11px] text-amber-400 w-8 text-right">
                                {ct.weight.toFixed(2)}
                              </span>
                            </div>
                            <button
                              type="button"
                              onClick={() => handleRemoveTraitFromChoice(cIdx, ct.traitId)}
                              className="text-zinc-500 hover:text-red-400"
                            >
                              <X className="h-3.5 w-3.5" />
                            </button>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Form Actions */}
          <div className="flex justify-end gap-3 pt-4 border-t border-zinc-800">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="rounded-lg bg-zinc-800 px-4 py-2 text-sm font-medium text-zinc-300 hover:bg-zinc-700"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-lg bg-amber-500 px-5 py-2 text-sm font-bold text-zinc-950 hover:bg-amber-400 disabled:opacity-50"
            >
              {loading && <Loader2 className="h-4 w-4 animate-spin" />}
              {isEditing ? "Lưu câu hỏi" : "Tạo câu hỏi"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}