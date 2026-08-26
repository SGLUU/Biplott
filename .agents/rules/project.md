# 📐 PROJECT RULES — BỊP LÓT

## 1. TỔNG QUAN KIẾN TRÚC & NGUYÊN TẮC
- **Tên dự án:** Bịp lót (*Cơ hội để nát hơn*).
- **Mô hình kiến trúc:** **Modular Monolith** — Frontend (Next.js) và Backend (ASP.NET Core Web API) tách rời thành 2 ứng dụng độc lập, giao tiếp thông qua RESTful API chuẩn.
- **Tiêu chí phát triển:** Đơn giản, thực dụng, dễ bảo trì, dễ mở rộng, KHÔNG sử dụng Microservices, KHÔNG over-engineer.
- **Ngôn ngữ V1:** Toàn bộ giao diện người dùng và nội dung bằng **Tiếng Việt**.

## 2. QUY ƯỚC PHÁT TRIỂN & CỘNG TÁC
- **Không tự ý triển khai code ngoài phạm vi được phê duyệt:** Luôn tuân theo tài liệu đặc tả trong thư mục `docs/`.
- **Bảo toàn tính toàn vẹn dữ liệu:** Không xóa hay sửa đổi các tập tin tài liệu đã thống nhất trừ khi có yêu cầu thay đổi rõ ràng.
- **Không hard-code giá trị logic:** Các tham số về Game, Chủ đề, Câu hỏi, Trọng số Trait phải được đọc từ Cơ sở dữ liệu và qua API.
- **Mobile-First là ưu tiên hàng đầu:** Mọi component và trải nghiệm tạo số phải hoạt động mượt mà trên màn hình cảm ứng di động.

## 3. CẤU TRÚC PHÂN CHIA THƯ MỤC CHUẨN
- `docs/`: Chứa toàn bộ tài liệu đặc tả và thiết kế kỹ thuật.
- `.agents/rules/`: Chứa các bộ quy tắc chuẩn cho AI và Lập trình viên.
- `frontend/`: Ứng dụng Next.js (App Router, TypeScript, Tailwind CSS, shadcn/ui).
- `backend/`: Ứng dụng ASP.NET Core Web API (C#, Entity Framework Core, SQL Server).
- `docker-compose.yml`: Điều phối môi trường phát triển và triển khai cục bộ.
