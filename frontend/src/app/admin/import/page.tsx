"use client";

import React, { useState } from "react";
import { ImportValidationResult, ImportConfirmResponse } from "@/types/admin";
import {
  validateImportFile,
  confirmImport,
  downloadImportTemplate,
  ImportTemplateFormat
} from "@/lib/adminApi";
import {
  UploadCloud,
  FileSpreadsheet,
  Download,
  CheckCircle2,
  AlertCircle,
  Loader2,
  FileText,
  RefreshCw
} from "lucide-react";

export default function BulkImportPage() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [validating, setValidating] = useState(false);
  const [validationResult, setValidationResult] = useState<ImportValidationResult | null>(null);
  const [importing, setImporting] = useState(false);
  const [importResponse, setImportResponse] = useState<ImportConfirmResponse | null>(null);
  const [generalError, setGeneralError] = useState<string | null>(null);
  const [downloadingTemplate, setDownloadingTemplate] =
  useState<ImportTemplateFormat | null>(null);

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setSelectedFile(file);
    setValidationResult(null);
    setImportResponse(null);
    setGeneralError(null);

    // Auto validate upon file select
    try {
      setValidating(true);
      const result = await validateImportFile(file);
      setValidationResult(result);
    } catch (err: unknown) {
      setGeneralError(err instanceof Error ? err.message : "Lỗi khi kiểm tra tệp tin");
    } finally {
      setValidating(false);
    }
  };

  const handleConfirmImport = async () => {
    if (!validationResult || validationResult.validCount === 0) return;

    try {
      setImporting(true);
      setGeneralError(null);
      const res = await confirmImport(validationResult.importSessionId, validationResult.previewItems);
      setImportResponse(res);
    } catch (err: unknown) {
      setGeneralError(err instanceof Error ? err.message : "Lỗi khi nhập dữ liệu vào cơ sở dữ liệu");
    } finally {
      setImporting(false);
    }
  };

  const handleReset = () => {
    setSelectedFile(null);
    setValidationResult(null);
    setImportResponse(null);
    setGeneralError(null);
  };

  const handleDownloadTemplate = async (format: ImportTemplateFormat) => {
    try {
      setDownloadingTemplate(format);
      setGeneralError(null);

      const { blob, fileName } = await downloadImportTemplate(format);

      const objectUrl = URL.createObjectURL(blob);

      const link = document.createElement("a");
      link.href = objectUrl;
      link.download = fileName;

      document.body.appendChild(link);
      link.click();
      link.remove();

      window.setTimeout(() => {
        URL.revokeObjectURL(objectUrl);
      }, 0);
    } catch (err: unknown) {
      setGeneralError(
        err instanceof Error
          ? err.message
          : "Không thể tải file mẫu."
      );
    } finally {
      setDownloadingTemplate(null);
    }
  };

  return (
    <div className="space-y-8 max-w-5xl">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-zinc-100 flex items-center gap-2.5">
          <UploadCloud className="h-6 w-6 text-amber-400" /> Nhập Nội dung Hàng loạt (Bulk Import)
        </h1>
        <p className="mt-1 text-sm text-zinc-400">
          Nhập câu hỏi, lựa chọn và gán trọng số thuộc tính hàng loạt từ file CSV, Excel (.xlsx) hoặc JSON.
        </p>
      </div>

      {/* Download Templates Banner */}
      <div className="rounded-2xl border border-zinc-800 bg-zinc-900/60 p-6 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h3 className="text-sm font-bold text-zinc-200 flex items-center gap-2">
            <Download className="h-4 w-4 text-amber-400" /> Tải file mẫu chuẩn (Sample Templates)
          </h3>
          <p className="mt-1 text-xs text-zinc-400">
            Tải mẫu cấu trúc chuẩn để điền dữ liệu đúng định dạng trước khi tải lên.
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2.5">
          <button
  type="button"
  onClick={() => handleDownloadTemplate("csv")}
  disabled={downloadingTemplate !== null}
  className="inline-flex items-center gap-1.5 rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-xs font-semibold text-zinc-300 hover:bg-zinc-800 hover:text-amber-400 transition disabled:cursor-not-allowed disabled:opacity-50"
>
  {downloadingTemplate === "csv" ? (
    <Loader2 className="h-4 w-4 animate-spin" />
  ) : (
    <FileText className="h-4 w-4" />
  )}
  Mẫu CSV (.csv)
</button>
          <button
  type="button"
  onClick={() => handleDownloadTemplate("xlsx")}
  disabled={downloadingTemplate !== null}
  className="inline-flex items-center gap-1.5 rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-xs font-semibold text-zinc-300 hover:bg-zinc-800 hover:text-emerald-400 transition disabled:cursor-not-allowed disabled:opacity-50"
>
  {downloadingTemplate === "xlsx" ? (
    <Loader2 className="h-4 w-4 animate-spin" />
  ) : (
    <FileSpreadsheet className="h-4 w-4" />
  )}
  Mẫu Excel (.xlsx)
</button>
          <button
  type="button"
  onClick={() => handleDownloadTemplate("json")}
  disabled={downloadingTemplate !== null}
  className="inline-flex items-center gap-1.5 rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-xs font-semibold text-zinc-300 hover:bg-zinc-800 hover:text-blue-400 transition disabled:cursor-not-allowed disabled:opacity-50"
>
  {downloadingTemplate === "json" ? (
    <Loader2 className="h-4 w-4 animate-spin" />
  ) : (
    <FileText className="h-4 w-4" />
  )}
  Mẫu JSON (.json)
</button>
        </div>
      </div>

      {/* Upload Zone */}
      {!importResponse && (
        <div className="rounded-2xl border-2 border-dashed border-zinc-800 hover:border-amber-500/50 bg-zinc-900/30 p-8 text-center transition">
          <input
            type="file"
            id="bulkFileInput"
            accept=".csv, .xlsx, .xls, .json"
            onChange={handleFileSelect}
            className="hidden"
          />
          <label htmlFor="bulkFileInput" className="cursor-pointer flex flex-col items-center">
            <div className="rounded-full bg-amber-500/10 p-4 text-amber-400 ring-1 ring-amber-500/20 mb-3">
              <UploadCloud className="h-8 w-8" />
            </div>
            <span className="text-sm font-bold text-zinc-200">
              {selectedFile ? selectedFile.name : "Nhấp để chọn file hoặc kéo thả vào đây"}
            </span>
            <span className="mt-1 text-xs text-zinc-500">
              Hỗ trợ tệp định dạng .CSV, .XLSX (Excel) hoặc .JSON (tối đa 10MB)
            </span>
          </label>

          {validating && (
            <div className="mt-4 flex items-center justify-center gap-2 text-xs text-amber-400">
              <Loader2 className="h-4 w-4 animate-spin" />
              Đang phân tích cú pháp và kiểm tra hợp lệ dữ liệu...
            </div>
          )}
        </div>
      )}

      {/* Error display */}
      {generalError && (
        <div className="rounded-2xl border border-red-500/20 bg-red-500/10 p-4 text-xs text-red-400 flex items-start gap-3">
          <AlertCircle className="h-5 w-5 shrink-0 text-red-400" />
          <div>
            <p className="font-bold">Đã có lỗi xảy ra</p>
            <p className="mt-0.5">{generalError}</p>
          </div>
        </div>
      )}

      {/* Success View */}
      {importResponse && (
        <div className="rounded-2xl border border-emerald-500/30 bg-emerald-500/10 p-8 text-center space-y-4">
          <div className="mx-auto rounded-full bg-emerald-500/20 p-4 text-emerald-400 w-fit">
            <CheckCircle2 className="h-10 w-10" />
          </div>
          <h2 className="text-xl font-bold text-emerald-300">Nhập dữ liệu thành công!</h2>
          <p className="text-sm text-zinc-300">{importResponse.message}</p>
          <div className="flex justify-center gap-3 pt-2">
            <button
              onClick={handleReset}
              className="rounded-xl bg-zinc-800 px-4 py-2 text-xs font-bold text-zinc-200 hover:bg-zinc-700"
            >
              Nhập thêm tệp khác
            </button>
            <a
              href="/admin/questions"
              className="rounded-xl bg-amber-500 px-4 py-2 text-xs font-bold text-zinc-950 hover:bg-amber-400"
            >
              Xem danh sách câu hỏi
            </a>
          </div>
        </div>
      )}

      {/* Validation Result Preview */}
      {validationResult && !importResponse && (
        <div className="space-y-6">
          {/* Summary stats */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="rounded-xl border border-zinc-800 bg-zinc-900/60 p-4 text-center">
              <span className="text-xs text-zinc-400">Tổng số dòng</span>
              <div className="text-2xl font-bold text-zinc-100 mt-1">{validationResult.totalRows}</div>
            </div>
            <div className="rounded-xl border border-emerald-500/20 bg-emerald-500/10 p-4 text-center">
              <span className="text-xs text-emerald-400">Hợp lệ (Sẵn sàng nhập)</span>
              <div className="text-2xl font-bold text-emerald-300 mt-1">{validationResult.validCount}</div>
            </div>
            <div className="rounded-xl border border-red-500/20 bg-red-500/10 p-4 text-center">
              <span className="text-xs text-red-400">Không hợp lệ (Lỗi)</span>
              <div className="text-2xl font-bold text-red-400 mt-1">{validationResult.invalidCount}</div>
            </div>
          </div>

          {/* Error breakdown if any */}
          {validationResult.errors.length > 0 && (
            <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-5 space-y-3">
              <div className="flex items-center gap-2 text-xs font-bold text-red-400">
                <AlertCircle className="h-4 w-4" /> Danh sách lỗi phát hiện ({validationResult.errors.length} lỗi):
              </div>
              <ul className="space-y-1.5 text-xs text-red-300 list-disc list-inside max-h-48 overflow-y-auto">
                {validationResult.errors.map((err, idx) => (
                  <li key={idx}>
                    <strong>Dòng {err.rowIndex} ({err.field}):</strong> {err.message}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Action to confirm */}
          <div className="flex items-center justify-between pt-2">
            <button
              type="button"
              onClick={handleReset}
              className="inline-flex items-center gap-2 rounded-xl bg-zinc-800 px-4 py-2.5 text-xs font-bold text-zinc-300 hover:bg-zinc-700"
            >
              <RefreshCw className="h-3.5 w-3.5" /> Hủy & Tải file khác
            </button>

            <button
              type="button"
              onClick={handleConfirmImport}
              disabled={importing || validationResult.validCount === 0}
              className="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-6 py-2.5 text-xs font-extrabold text-zinc-950 hover:bg-amber-400 transition shadow-lg shadow-amber-500/20 disabled:opacity-50"
            >
              {importing && <Loader2 className="h-4 w-4 animate-spin" />}
              Xác nhận Nhập ({validationResult.validCount} câu hỏi hợp lệ)
            </button>
          </div>

          {/* Preview rows table */}
          <div className="rounded-2xl border border-zinc-800 bg-zinc-900/40 overflow-hidden shadow-xl">
            <div className="px-5 py-3 border-b border-zinc-800 text-xs font-bold text-zinc-300">
              Xem trước dữ liệu phân tích ({validationResult.previewItems.length} mục)
            </div>
            <div className="overflow-x-auto max-h-96">
              <table className="w-full text-left text-xs text-zinc-300">
                <thead className="border-b border-zinc-800 bg-zinc-950/80 sticky top-0 text-[11px] font-semibold text-zinc-400 uppercase">
                  <tr>
                    <th className="px-4 py-2.5">Dòng</th>
                    <th className="px-4 py-2.5">Chủ đề</th>
                    <th className="px-4 py-2.5">Loại</th>
                    <th className="px-4 py-2.5">Nội dung câu hỏi</th>
                    <th className="px-4 py-2.5">Số lựa chọn</th>
                    <th className="px-4 py-2.5 text-center">Trạng thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-zinc-800/60">
                  {validationResult.previewItems.map((q, idx) => (
                    <tr key={idx} className={q.isValid ? "hover:bg-zinc-800/20" : "bg-red-500/5 hover:bg-red-500/10"}>
                      <td className="px-4 py-3 font-mono text-zinc-500">#{q.rowIndex}</td>
                      <td className="px-4 py-3 font-mono text-amber-400">{q.themeCode}</td>
                      <td className="px-4 py-3">{q.questionType}</td>
                      <td className="px-4 py-3 font-medium text-zinc-200 max-w-xs truncate">{q.content}</td>
                      <td className="px-4 py-3">{q.choices.length} lựa chọn</td>
                      <td className="px-4 py-3 text-center">
                        {q.isValid ? (
                          <span className="rounded bg-emerald-500/10 px-2 py-0.5 text-[10px] font-bold text-emerald-400">
                            HỢP LỆ
                          </span>
                        ) : (
                          <span className="rounded bg-red-500/10 px-2 py-0.5 text-[10px] font-bold text-red-400">
                            LỖI
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}