# 🎨 FRONTEND RULES — BỊP LÓT

## 1. TECH STACK & CÔNG NGHỆ
- **Framework:** Next.js (App Router, React 19 / 18, TypeScript).
- **Styling:** Tailwind CSS, `tailwind-merge`, `clsx`.
- **UI Components:** `shadcn/ui` (dựa trên Radix UI primitives).
- **Icons:** `lucide-react`.
- **Animation:** `framer-motion` (ưu tiên hiệu ứng lật thẻ 3D, bóng nổ, tia sáng mở số).
- **State Management:** `zustand` cho Client State (Phiếu vé tạm, Trạng thái đang chơi dòng A-F), `@tanstack/react-query` cho Server State.

## 2. QUY CHUẨN THIẾT KẾ & GIAO DIỆN
- **Màu thương hiệu:** Bắt buộc tuân thủ bộ màu Đỏ (`#E11D48`), Cam (`#F97316`), Vàng (`#FACC15`).
- **Hỗ trợ giao diện Theme:** Tích hợp `next-themes` hỗ trợ 3 chế độ: `Light`, `Dark`, `System`.
- **Thiết kế Mobile-First:** Sử dụng các breakpoint chuẩn `sm:`, `md:`, `lg:` của Tailwind. Đảm bảo kích thước bấm tối thiểu $44\text{px} \times 44\text{px}$ trên di động.
- **Tone & Voice:** Thể hiện sự vui vẻ, châm biếm hài hước, hiện đại, không lòe loẹt kiểu sòng bài cờ bạc.

## 3. CẤU TRÚC THƯ MỤC `frontend/src/`
```text
src/
├── app/                  # Next.js App Router (layout, page, route handlers)
│   ├── (public)/         # Trang chính, chơi game, xem phiếu
│   ├── (auth)/           # Trang đăng nhập / đăng ký modal
│   ├── admin/            # Trang quản trị nội dung & bulk import
│   └── globals.css       # Biến màu CSS & Tailwind styles
├── components/
│   ├── ui/               # shadcn/ui base components (Button, Dialog, Drawer, Input)
│   ├── game/             # LottoBall, QuestionCard, ModeSwitcher, BallGrid
│   ├── slip/             # SlipTicket, SlipLineRow, ShareModal
│   ├── layout/           # Header, Footer, BottomNav, ThemeToggle
│   └── shared/           # ErrorBoundary, LoadingSpinner
├── hooks/                # Custom React hooks (useLuckyEngine, useSlipStore, useAuth)
├── lib/                  # Tiện ích axios/fetch client, định dạng số tiền, ngày tháng
├── stores/               # Zustand stores (slipStore, gameStore, authStore)
└── types/                # TypeScript interfaces và API DTOs
```

## 4. NGUYÊN TẮC LẬP TRÌNH REACT / TYPESCRIPT
- **Strict TypeScript:** Luôn khai báo type rõ ràng, không dùng `any`.
- **Component nhỏ & Đơn trách nhiệm:** Tách các component lớn thành các sub-components nhỏ dễ tái sử dụng và kiểm thử.
- **Xử lý trạng thái Loading & Error:** Mọi thao tác gọi API sinh số đều phải có skeleton/spinner và xử lý lỗi lịch sự, không làm treo giao diện.
