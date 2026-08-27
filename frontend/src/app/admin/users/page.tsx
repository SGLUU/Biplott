"use client";

import React, { useState, useEffect } from "react";
import { AdminUser } from "@/types/admin";
import { getAdminUsers, setAdminUserStatus } from "@/lib/adminApi";
import { useAuthStore } from "@/stores/useAuthStore";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import {
  Users,
  Search,
  Power,
  RefreshCw,
  Loader2,
  ChevronLeft,
  ChevronRight,
  ShieldCheck,
  Bookmark
} from "lucide-react";

export default function UsersManagementPage() {
  const { user: currentUser } = useAuthStore();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(15);
  const [search, setSearch] = useState("");
  const [isActiveFilter, setIsActiveFilter] = useState<string>("all");
  const [roleFilter, setRoleFilter] = useState<string>("");
  const [loading, setLoading] = useState(true);

  const [userToToggle, setUserToToggle] = useState<AdminUser | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const fetchUsers = React.useCallback(async () => {
    try {
      setLoading(true);
      const activeParam = isActiveFilter === "all" ? undefined : isActiveFilter === "active";
      const result = await getAdminUsers(
        page,
        pageSize,
        search.trim() || undefined,
        activeParam,
        roleFilter || undefined
      );
      setUsers(result.items);
      setTotalCount(result.totalCount);
    } catch (err: unknown) {
      console.error("Lỗi tải người dùng:", err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, isActiveFilter, roleFilter]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchUsers();
  };

  const handleStatusToggleClick = (user: AdminUser) => {
    if (user.id === currentUser?.id && user.isActive) {
      alert("Bạn không thể tự vô hiệu hóa tài khoản Quản trị viên của chính mình.");
      return;
    }
    setUserToToggle(user);
  };

  const handleConfirmStatusToggle = async () => {
    if (!userToToggle) return;
    try {
      setActionLoading(true);
      await setAdminUserStatus(userToToggle.id, !userToToggle.isActive);
      setUserToToggle(null);
      fetchUsers();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Lỗi thay đổi trạng thái tài khoản");
    } finally {
      setActionLoading(false);
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-zinc-100 flex items-center gap-2.5">
          <Users className="h-6 w-6 text-blue-400" /> Quản lý Tài khoản Người dùng
        </h1>
        <p className="mt-1 text-sm text-zinc-400">
          Xem danh sách người chơi, phân quyền Quản trị viên và kiểm soát trạng thái hoạt động tài khoản.
        </p>
      </div>

      {/* Filters Bar */}
      <div className="flex flex-col sm:flex-row gap-3">
        <form onSubmit={handleSearch} className="relative flex-1">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-500" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tìm theo email hoặc tên hiển thị..."
            className="w-full rounded-xl border border-zinc-800 bg-zinc-900/80 pl-10 pr-4 py-2 text-sm text-zinc-200 placeholder-zinc-500 focus:border-amber-500 focus:outline-none"
          />
        </form>

        <select
          value={roleFilter}
          onChange={(e) => {
            setRoleFilter(e.target.value);
            setPage(1);
          }}
          className="rounded-xl border border-zinc-800 bg-zinc-900 px-4 py-2 text-sm text-zinc-300 focus:border-amber-500 focus:outline-none"
        >
          <option value="">Tất cả vai trò</option>
          <option value="Admin">Chỉ Quản trị viên (Admin)</option>
          <option value="User">Người chơi thông thường (User)</option>
        </select>

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
          <option value="inactive">Bị vô hiệu hóa</option>
        </select>

        <button
          onClick={fetchUsers}
          className="inline-flex items-center gap-1.5 rounded-xl border border-zinc-800 bg-zinc-900 px-3.5 py-2 text-xs font-medium text-zinc-300 hover:bg-zinc-800"
        >
          <RefreshCw className={`h-3.5 w-3.5 ${loading ? "animate-spin" : ""}`} /> Làm mới
        </button>
      </div>

      {/* Users Table */}
      <div className="rounded-2xl border border-zinc-800 bg-zinc-900/40 overflow-hidden shadow-xl">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-zinc-300">
            <thead className="border-b border-zinc-800 bg-zinc-950/60 text-xs font-semibold text-zinc-400 uppercase tracking-wider">
              <tr>
                <th className="px-5 py-3.5">Người dùng</th>
                <th className="px-5 py-3.5">Email</th>
                <th className="px-5 py-3.5 text-center">Vai trò</th>
                <th className="px-5 py-3.5 text-center">Vé đã lưu</th>
                <th className="px-5 py-3.5 text-center">Ngày đăng ký</th>
                <th className="px-5 py-3.5 text-center">Trạng thái</th>
                <th className="px-5 py-3.5 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-800/60">
              {loading ? (
                <tr>
                  <td colSpan={7} className="px-5 py-12 text-center text-zinc-400">
                    <Loader2 className="mx-auto h-6 w-6 animate-spin text-amber-500 mb-2" />
                    Đang tải danh sách người dùng...
                  </td>
                </tr>
              ) : users.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-5 py-12 text-center text-zinc-500">
                    Không tìm thấy người dùng nào.
                  </td>
                </tr>
              ) : (
                users.map((u) => {
                  const isCurrent = u.id === currentUser?.id;
                  const isAdmin = u.roles.includes("Admin");

                  return (
                    <tr key={u.id} className="hover:bg-zinc-800/30 transition">
                      <td className="px-5 py-4 font-medium text-zinc-100">
                        <div className="flex items-center gap-2">
                          <span>{u.displayName}</span>
                          {isCurrent && (
                            <span className="rounded bg-zinc-800 px-1.5 py-0.5 text-[10px] text-zinc-400">
                              (Bạn)
                            </span>
                          )}
                        </div>
                      </td>
                      <td className="px-5 py-4 font-mono text-xs text-zinc-400">
                        {u.email}
                      </td>
                      <td className="px-5 py-4 text-center">
                        {isAdmin ? (
                          <span className="inline-flex items-center gap-1 rounded-full bg-amber-500/10 px-2.5 py-0.5 text-xs font-bold text-amber-400 border border-amber-500/20">
                            <ShieldCheck className="h-3 w-3" /> Admin
                          </span>
                        ) : (
                          <span className="rounded-full bg-zinc-800 px-2.5 py-0.5 text-xs text-zinc-400">
                            User
                          </span>
                        )}
                      </td>
                      <td className="px-5 py-4 text-center text-xs font-bold text-zinc-200">
                        <span className="inline-flex items-center gap-1">
                          <Bookmark className="h-3 w-3 text-emerald-400" />
                          {u.savedSlipsCount}
                        </span>
                      </td>
                      <td className="px-5 py-4 text-center text-xs text-zinc-400">
                        {new Date(u.createdAt).toLocaleDateString("vi-VN")}
                      </td>
                      <td className="px-5 py-4 text-center">
                        <StatusBadge
                          isActive={u.isActive}
                          activeLabel="Hoạt động"
                          inactiveLabel="Vô hiệu hóa"
                          size="sm"
                        />
                      </td>
                      <td className="px-5 py-4 text-right">
                        <button
                          onClick={() => handleStatusToggleClick(u)}
                          disabled={isCurrent && u.isActive}
                          className={`rounded-lg p-1.5 transition ${
                            u.isActive
                              ? "text-emerald-400 hover:bg-emerald-500/10 hover:text-red-400"
                              : "text-zinc-500 hover:bg-zinc-800 hover:text-emerald-400"
                          } ${isCurrent && u.isActive ? "opacity-30 cursor-not-allowed" : ""}`}
                          title={
                            isCurrent && u.isActive
                              ? "Không thể tự vô hiệu hóa chính mình"
                              : u.isActive
                              ? "Vô hiệu hóa tài khoản (Thu hồi token)"
                              : "Kích hoạt lại tài khoản"
                          }
                        >
                          <Power className="h-4 w-4" />
                        </button>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Footer */}
        <div className="flex items-center justify-between border-t border-zinc-800 px-5 py-3 text-xs text-zinc-400 bg-zinc-950/40">
          <span>Tổng số: <strong className="text-zinc-200">{totalCount}</strong> người dùng</span>
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

      {/* Confirm status toggle */}
      <ConfirmDialog
        isOpen={!!userToToggle}
        title={userToToggle?.isActive ? "Vô hiệu hóa tài khoản" : "Kích hoạt lại tài khoản"}
        message={
          userToToggle?.isActive
            ? `Bạn có chắc chắn muốn vô hiệu hóa tài khoản của '${userToToggle?.displayName}' (${userToToggle?.email})? Mọi phiên đăng nhập hiện tại sẽ bị thu hồi token ngay lập tức và người dùng sẽ không thể đăng nhập cho đến khi được kích hoạt lại.`
            : `Bạn có muốn kích hoạt lại quyền đăng nhập cho tài khoản '${userToToggle?.displayName}' (${userToToggle?.email})?`
        }
        isDestructive={userToToggle?.isActive}
        isLoading={actionLoading}
        confirmLabel={userToToggle?.isActive ? "Vô hiệu hóa" : "Kích hoạt"}
        onConfirm={handleConfirmStatusToggle}
        onCancel={() => setUserToToggle(null)}
      />
    </div>
  );
}