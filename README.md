# 🎲 Bịp lót

> **Tagline:** *Cơ hội để nát hơn*  
> **Brand Identity:** Vui vẻ • Châm biếm nhẹ • Hiện đại • Premium • Mobile-First  
> **Palette:** Đỏ (`Red`) • Cam (`Orange`) • Vàng (`Yellow`)

---

## 📌 1. Giới thiệu dự án

**Bịp lót** là một ứng dụng web giải trí độc lập (không liên kết với bất kỳ đơn vị xổ số nào, không sao chép logo chính thức) được xây dựng nhằm mang lại tiếng cười và trải nghiệm tạo số độc đáo, châm biếm văn minh cho người chơi các loại hình xổ số phổ biến (Power 6/55, Mega 6/45, Lotto 5/35).

Trọng tâm của Bịp lót không phải là "tuyên bố tăng xác suất trúng" (xổ số về mặt toán học là ngẫu nhiên độc lập), mà là **trải nghiệm sinh số mang đậm tính cá nhân hoá và giải trí (Lucky Journey, Thần Tài, Mixed Engine)**, kết hợp với Content hài hước, đánh trúng tâm lý cuộc sống, công sở, tình duyên và "tâm linh vui vẻ".

---

## 🛠️ 2. Tech Stack & Kiến trúc

| Thành phần | Công nghệ lựa chọn | Ghi chú |
| :--- | :--- | :--- |
| **Frontend** | Next.js (App Router), TypeScript, Tailwind CSS, shadcn/ui, Lucide Icons, Framer Motion | Tối ưu SEO, SSR, animation mượt mà, Mobile-first, Hỗ trợ Light / Dark / System |
| **Backend** | ASP.NET Core 10 Web API (.NET 10 LTS), C#, Entity Framework Core 10 | Hiệu năng cao, Type-safe, Kiến trúc Modular Monolith sạch sẽ, RESTful API |
| **Database** | Microsoft SQL Server | Relational data chặt chẽ, tối ưu truy vấn JSON cho Traits và Metadata |
| **Authentication** | ASP.NET Core Identity + JWT Token | Bảo mật chuẩn công nghiệp, không tự chế thuật toán băm mật khẩu |
| **Container & Dev** | Docker, Docker Compose | Khởi chạy toàn bộ hệ thống bằng 1 lệnh duy nhất |

---

## 🌟 3. Tính năng cốt lõi (V1)

1. **Đa dạng thể thức game linh hoạt:**
   - **Power 6/55:** 6 số từ 01 - 55.
   - **Mega 6/45:** 6 số từ 01 - 45.
   - **Lotto 5/35:** 5 số chính (01 - 35) + 1 số đặc biệt (01 - 12).
2. **Phiếu số chuyên nghiệp (A - F):**
   - Hỗ trợ tạo tối đa 6 bộ số trên một phiếu (A, B, C, D, E, F).
   - Cho phép tạo lẻ từng bộ hoặc đồng loạt nhiều bộ.
3. **4 Chế độ tạo số thông minh:**
   - **Manual:** Tự tay chọn từng số trên lưới bóng.
   - **Thần Tài (Random):** 4 phong cách (*Pure Random, Balanced, Spread, Surprise*).
   - **Lucky Journey (Interactive Storytelling):** Trả lời từng câu hỏi trắc nghiệm/tình huống hài hước để lật mở từng con số bí ẩn ngay tức thì.
   - **Mixed:** Tự do kết hợp các nguồn tạo số trong cùng 1 bộ (lưu vết nguồn gốc ở cấp độ từng con số: *Manual, Lucky, Random*).
4. **Lucky Engine & Novelty Engine:**
   - Không ánh xạ cứng (không có chuyện `Xanh = 17`). Sinh số dựa trên phân tích Trait vector, bối cảnh, trọng số xác suất động (Weighted Random) và đa dạng hoá nội dung.
   - Chống lặp câu hỏi, cân bằng chủ đề (Themes) cho từng người chơi.
5. **Trải nghiệm Guest & Thành viên:**
   - Người dùng vãng lai (Guest) vào chơi ngay lập tức không cần đăng ký.
   - Đăng nhập để lưu trữ phiếu, quản lý lịch sử, yêu thích và đồng bộ dữ liệu mượt mà.
6. **Hệ thống Quản trị (Admin CMS):**
   - Quản lý danh mục Games, Themes, Questions, Choices, Traits.
   - Hỗ trợ Import nội dung hàng loạt bằng Excel/CSV/JSON.
   - Cấu hình trọng số và hệ số phạt cho Lucky Engine.

---

## 📂 4. Đề xuất cấu trúc thư mục Repository

```text
biplott/
├── .agents/                    # Bộ quy tắc dành cho AI và pair-programming
│   └── rules/
│       ├── project.md          # Quy tắc chung toàn dự án
│       ├── frontend.md         # Quy tắc chuẩn cho Next.js & UI
│       ├── backend.md          # Quy tắc chuẩn cho ASP.NET Core Web API
│       └── database.md         # Quy tắc chuẩn cho SQL Server & EF Core
├── docs/                       # Toàn bộ tài liệu đặc tả chi tiết
│   ├── PRODUCT_SPEC.md         # Đặc tả sản phẩm và định vị thương hiệu
│   ├── GAME_RULES.md           # Quy tắc trò chơi và cấu trúc phiếu số
│   ├── UX_FLOW.md              # Sơ đồ luồng trải nghiệm người dùng
│   ├── LUCKY_ENGINE.md         # Thuật toán sinh số Lucky & Novelty Engine
│   ├── CONTENT_SYSTEM.md       # Hệ thống câu hỏi, traits và cơ chế Import
│   ├── DATABASE_SPEC.md        # Thiết kế CSDL, ERD và Schema
│   ├── API_SPEC.md             # Đặc tả RESTful API Contracts
│   ├── UI_DESIGN.md            # Thiết kế giao diện, Design Tokens & Component
│   ├── SECURITY.md             # Chính sách bảo mật, Auth & Validation
│   ├── ROADMAP.md              # Lộ trình triển khai qua các Phase
│   └── ACCEPTANCE_CRITERIA.md  # Tiêu chí nghiệm thu chức năng
├── frontend/                   # Ứng dụng Next.js (Sẽ scaffold ở Phase 1)
│   ├── public/
│   ├── src/
│   │   ├── app/                # Next.js App Router
│   │   ├── components/         # UI Components (shadcn/ui + custom)
│   │   ├── hooks/              # Custom React Hooks
│   │   ├── lib/                # Utilities & API Client
│   │   ├── stores/             # Zustand / State management
│   │   └── types/              # TypeScript Types & DTOs
│   ├── Dockerfile
│   └── package.json
├── backend/                    # ASP.NET Core Solution (Sẽ scaffold ở Phase 1)
│   ├── src/
│   │   ├── Biplott.Api/        # Controllers, Middlewares, DI Setup
│   │   ├── Biplott.Core/       # Domain Entities, Interfaces, Enums
│   │   ├── Biplott.Application/# Use Cases, DTOs, Lucky Engine Logic
│   │   └── Biplott.Infrastructure/ # EF Core DbContext, Migrations, Repositories
│   ├── tests/
│   │   ├── Biplott.Core.Tests/
│   │   └── Biplott.Application.Tests/
│   ├── Dockerfile
│   └── Biplott.sln
├── docker-compose.yml          # Docker Compose orchestration
├── docker-compose.override.yml # Local development overrides
├── .env.example                # Biến môi trường mẫu
└── README.md                   # File tổng quan dự án
```

---

## 📖 5. Hệ thống tài liệu đặc tả (Documentation Index)

Vui lòng đọc các tài liệu chi tiết trong thư mục `docs/` để nắm rõ toàn bộ kiến trúc:

- 📄 [**docs/PRODUCT_SPEC.md**](./docs/PRODUCT_SPEC.md) - Định vị, tính năng, chân dung người dùng.
- 📄 [**docs/GAME_RULES.md**](./docs/GAME_RULES.md) - Chi tiết luật chơi, cấu trúc Pool, quản lý phiếu A-F.
- 📄 [**docs/UX_FLOW.md**](./docs/UX_FLOW.md) - Hành trình người dùng (Guest, User, Admin) và Wireflows.
- 📄 [**docs/LUCKY_ENGINE.md**](./docs/LUCKY_ENGINE.md) - Toán học và giải thuật Lucky Journey & Novelty Engine.
- 📄 [**docs/CONTENT_SYSTEM.md**](./docs/CONTENT_SYSTEM.md) - 9 dạng câu hỏi, hệ thống Traits, cơ chế Import bulk.
- 📄 [**docs/DATABASE_SPEC.md**](./docs/DATABASE_SPEC.md) - Mô hình dữ liệu, bảng, khoá ngoại, Indexing.
- 📄 [**docs/API_SPEC.md**](./docs/API_SPEC.md) - Chi tiết REST Endpoints, request/response models.
- 📄 [**docs/UI_DESIGN.md**](./docs/UI_DESIGN.md) - Design Tokens, bảng màu Đỏ/Cam/Vàng, Component Specs.
- 📄 [**docs/SECURITY.md**](./docs/SECURITY.md) - Bảo mật ASP.NET Identity, Rate Limiting, CORS, Data Protection.
- 📄 [**docs/ROADMAP.md**](./docs/ROADMAP.md) - Lộ trình thực hiện từng bước từ V1 đến tương lai.
- 📄 [**docs/ACCEPTANCE_CRITERIA.md**](./docs/ACCEPTANCE_CRITERIA.md) - Tiêu chí kiểm thử và nghiệm thu chức năng.

---

## ⚡ 6. Hướng dẫn nhanh cho AI Agents & Developers

Các quy tắc dự án được thiết lập tại `.agents/rules/`:
- [`.agents/rules/project.md`](./.agents/rules/project.md): Quy ước kiến trúc tổng thể và giao tiếp.
- [`.agents/rules/frontend.md`](./.agents/rules/frontend.md): Quy chuẩn Next.js, Tailwind và Component.
- [`.agents/rules/backend.md`](./.agents/rules/backend.md): Quy chuẩn C#, ASP.NET Core và Clean Architecture.
- [`.agents/rules/database.md`](./.agents/rules/database.md): Quy chuẩn SQL Server và EF Core Migrations.
