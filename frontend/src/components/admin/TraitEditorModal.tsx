"use client";

import React, { useState, useEffect } from "react";
import { AdminTrait, CreateTraitRequest, UpdateTraitRequest } from "@/types/admin";
import { X, Loader2 } from "lucide-react";

interface TraitEditorModalProps {
  trait: AdminTrait | null;
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreateTraitRequest | UpdateTraitRequest) => Promise<void>;
}

export function TraitEditorModal({ trait, isOpen, onClose, onSave }: TraitEditorModalProps) {
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState("Personality");
  const [isActive, setIsActive] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEditing = !!trait;

  useEffect(() => {
    if (trait) {
      setCode(trait.code);
      setName(trait.name);
      setDescription(trait.description || "");
      setCategory(trait.category || "Personality");
      setIsActive(trait.isActive);
    } else {
      setCode("");
      setName("");
      setDescription("");
      setCategory("Personality");
      setIsActive(true);
    }
    setError(null);
  }, [trait, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!isEditing && !code.trim()) {
      setError("Vui lòng nhập mã thuộc tính (Code)");
      return;
    }
    if (!name.trim()) {
      setError("Vui lòng nhập tên thuộc tính");
      return;
    }

    try {
      setLoading(true);
      if (isEditing) {
        await onSave({
          name: name.trim(),
          description: description.trim() || null,
          category: category.trim() || null,
          isActive
        });
      } else {
        await onSave({
          code: code.trim(),
          name: name.trim(),
          description: description.trim() || null,
          category: category.trim() || null,
          isActive
        });
      }
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Đã xảy ra lỗi khi lưu");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/75 p-4 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="flex w-full max-w-lg flex-col rounded-2xl border border-zinc-800 bg-zinc-900 shadow-2xl overflow-hidden">
        <div className="flex items-center justify-between border-b border-zinc-800 px-6 py-4 bg-zinc-950/60">
          <h3 className="font-bold text-zinc-100">{isEditing ? "Chỉnh sửa Thuộc tính" : "Thêm Thuộc tính mới"}</h3>
          <button onClick={onClose} className="rounded-lg p-1 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="rounded-lg bg-red-500/10 p-3 text-xs text-red-400 border border-red-500/20">
              {error}
            </div>
          )}

          <div>
            <label className="block text-xs font-semibold text-zinc-300 mb-1.5">
              Mã thuộc tính (Code) {!isEditing && <span className="text-red-400">*</span>}
            </label>
            <input
              type="text"
              disabled={isEditing}
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="VD: RiskTolerance, Intuition"
              className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none disabled:opacity-50 font-mono"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-zinc-300 mb-1.5">
              Tên thuộc tính <span className="text-red-400">*</span>
            </label>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="VD: Liều lĩnh, Trực giác"
              className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-zinc-300 mb-1.5">Phân loại (Category)</label>
            <input
              type="text"
              value={category}
              onChange={(e) => setCategory(e.target.value)}
              placeholder="VD: Personality, Emotion, Social"
              className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-zinc-300 mb-1.5">Mô tả ý nghĩa</label>
            <textarea
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Mô tả tác động của thuộc tính tới việc chọn số..."
              className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2.5 text-sm text-zinc-100 placeholder-zinc-500 focus:border-amber-500 focus:outline-none resize-none"
            />
          </div>

          <div className="flex items-center gap-2 pt-2">
            <input
              type="checkbox"
              id="traitActive"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              className="h-4 w-4 rounded border-zinc-700 bg-zinc-950 text-amber-500 focus:ring-amber-500"
            />
            <label htmlFor="traitActive" className="text-sm font-medium text-zinc-300">
              Kích hoạt thuộc tính
            </label>
          </div>

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
              {isEditing ? "Lưu thay đổi" : "Tạo thuộc tính"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}