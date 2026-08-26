# ⚙️ BACKEND RULES — BỊP LÓT

## 1. TECH STACK & CÔNG NGHỆ
- **Framework:** ASP.NET Core 10 Web API (.NET 10 LTS), C#.
- **ORM:** Entity Framework Core 10 (SQL Server Provider).
- **Authentication:** ASP.NET Core Identity + JWT Bearer Tokens.
- **Validation:** `FluentValidation` tích hợp tự động qua Filter.
- **Documentation:** `Swashbuckle.AspNetCore` / `Scalar` (Swagger UI).
- **Tài liệu Excel/CSV:** `ClosedXML` / `CsvHelper` cho tính năng Bulk Import.

## 2. KIẾN TRÚC MODULAR MONOLITH / CLEAN ARCHITECTURE
Giải pháp Backend được tổ chức thành các project C# rõ ràng:
- `Biplott.Api`: Chứa Controllers, Middlewares, Dependency Injection container, Swagger config.
- `Biplott.Core`: Chứa Domain Entities, Value Objects, Enums, Domain Interfaces (Không phụ thuộc vào bất kỳ thư viện ngoài nào).
- `Biplott.Application`: Chứa Use Cases, Services, DTOs, Mappings, Logic của **Lucky Engine** và **Novelty Engine**.
- `Biplott.Infrastructure`: Chứa EF Core DbContext, Migrations, Repositories, Identity Services, File Importers.

## 3. NGUYÊN TẮC THIẾT KẾ CODE & THUẬT TOÁN
- **Lucky Engine không bao giờ tĩnh:**
  - Không hard-code ánh xạ cố định (như màu sắc $\rightarrow$ số).
  - Thuật toán phải sử dụng `Trait Vectors`, `Candidate Scoring`, `Weighted Random` và nguồn ngẫu nhiên an toàn `RandomNumberGenerator`.
- **Game Engine đa Pool:**
  - Xử lý các trò chơi thông qua trừu tượng `GamePool`, không giả định mọi game chỉ có 1 dải số đơn lẻ.
- **Xử lý Ngoại lệ tập trung (Global Exception Handling):**
  - Mọi lỗi được bắt và chuẩn hóa theo tiêu chuẩn **RFC 7807 Problem Details**. Không để lộ Stack Trace thô ra môi trường Production.
- **Logging & Tracing:**
  - Sử dụng Serilog / ILogger có cấu trúc để ghi log các thao tác sinh số và import dữ liệu.
