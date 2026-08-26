# 🗄️ DATABASE RULES — BỊP LÓT

## 1. HỆ QUẢN TRỊ CSDL & CÔNG NGHỆ
- **Hệ quản trị CSDL:** Microsoft SQL Server 2022+ chạy trong Docker Container.
- **Phương pháp tiếp cận:** Entity Framework Core Code-First Migrations.
- **Chuẩn kết nối:** Luôn đặt `TrustServerCertificate=True` trong môi trường Dev/Docker.

## 2. QUY ƯỚC ĐẶT TÊN & KIỂU DỮ LIỆU
- **Tên Bảng:** PascalCase số nhiều (`Games`, `Questions`, `Slips`, `UserQuestionHistories`).
- **Tên Cột:** PascalCase (`Id`, `CreatedAt`, `QuestionType`, `IsActive`).
- **Khóa chính:**
  - `int IDENTITY(1,1)` cho các bảng danh mục cấu hình (`Games`, `Themes`, `Traits`, `Questions`, `QuestionChoices`).
  - `uniqueidentifier` (`Guid`) cho các bảng dữ liệu giao dịch (`Slips`, `SlipLines`, `SlipLineNumbers`).
  - `bigint IDENTITY(1,1)` cho các bảng log lịch sử tăng trưởng nhanh (`UserQuestionHistories`).
- **Thời gian:** Sử dụng kiểu `datetime2` với giá trị mặc định UTC: `sysutcdatetime()`.
- **Chuỗi văn bản:** Dùng `nvarchar` (hỗ trợ Tiếng Việt có dấu đầy đủ).

## 3. TỐI ƯU HIỆU NĂNG & INDEXING
- **Đánh Index chiến lược:**
  - Index trên `GuestSessionToken` và `UserId` trong bảng `Slips` và `UserQuestionHistories`.
  - Composite Index `(UserId, AnsweredAt DESC)` để phục vụ Novelty Engine truy vấn nhanh lịch sử câu hỏi gần đây của người dùng.
- **Dữ liệu linh hoạt:** Sử dụng cột `nvarchar(max)` lưu JSON (`MetadataJson`, `ValueJson`) cho các thuộc tính mở rộng mà không cần thay đổi cấu trúc bảng.

## 4. QUY TẮC MIGRATION & SEED DATA
- Tuyệt đối không sửa trực tiếp file Migration cũ sau khi đã áp dụng vào Database; luôn tạo Migration mới.
- Khởi tạo dữ liệu mẫu (`DbInitializer` / `DbSeeder`) bao gồm:
  - 3 trò chơi cơ bản: `Power 6/55`, `Mega 6/45`, `Lotto 5/35`.
  - Danh mục Themes (`Công sở`, `Tài chính`, `Tình duyên`, `Tâm linh meme`...).
  - Danh mục Traits (`ChaosEnergy`, `RiskTolerance`, `SpiritualVibe`...).
  - Ngân hàng câu hỏi mẫu ban đầu.
