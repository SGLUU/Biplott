# ✅ TIÊU CHÍ NGHIỆM THU CHỨC NĂNG (ACCEPTANCE CRITERIA)
# Dự án: Bịp lót — *Cơ hội để nát hơn*

---

## 1. PHÂN HỆ QUY TẮC GAME & PHIẾU VÉ (GAMES & SLIPS)

### AC 1.1: Tính linh hoạt của Game Đa Pool (Multi-Pool Support)
- **Kịch bản:** Chọn game Power 6/55, Mega 6/45 hoặc Lotto 5/35.
- **Given:** Người dùng đang ở màn hình chọn Game.
- **When:** Người dùng chọn **Lotto 5/35**.
- **Then:**
  - Hệ thống hiển thị 2 khu vực chọn số riêng biệt: Dãy chính (5 số từ 01-35) và Số đặc biệt (1 số từ 01-12).
  - Không cho phép chọn quá 5 số ở Dãy chính hoặc quá 1 số ở Số đặc biệt.
  - Số ở dãy đặc biệt có thể trùng giá trị với số ở dãy chính mà vẫn hợp lệ.

### AC 1.2: Quản lý Phiếu vé 6 dòng (Lines A to F)
- **Kịch bản:** Người dùng tạo một hoặc nhiều dòng trên phiếu.
- **Given:** Người dùng đang xem Phiếu vé trống.
- **When:** Người dùng chỉ hoàn thành Dòng A và Dòng C (bỏ trống B, D, E, F).
- **Then:**
  - Phiếu vẫn được coi là hợp lệ để lưu hoặc tải ảnh chia sẻ.
  - Dòng A và C có trạng thái `Complete`, các dòng còn lại hiển thị trạng thái `Empty`.

---

## 2. PHÂN HỆ CÁC CHẾ ĐỘ SINH SỐ (GENERATION MODES)

### AC 2.1: Chế độ Lucky Journey (Hành trình Vận mệnh)
- **Kịch bản:** Mở từng con số tương tác qua câu hỏi.
- **Given:** Người chơi đang ở vị trí số thứ 3 của Dòng A (đã có 2 số trước đó).
- **When:** Người chơi trả lời câu hỏi hiển thị trên màn hình.
- **Then:**
  - Ngay khi chọn đáp án, hệ thống chạy animation mở thưởng và hiển thị ngay con số thứ 3.
  - Con số được mở bắt buộc nằm trong dải số hợp lệ và **không trùng** với 2 số đã mở trước đó.
  - Cùng 1 câu trả lời ở 2 thời điểm khác nhau sẽ mang lại các con số khác nhau (tính phi đơn định / Non-deterministic).
  - Hệ thống tự động chuyển sang câu hỏi thứ 4 với chủ đề khác để tránh nhàm chán.

### AC 2.2: Chế độ Thần Tài (Random Strategies)
- **Kịch bản:** Sinh nhanh bộ số theo phong cách.
- **Given:** Người chơi chọn dòng B và chọn phong cách "Balanced".
- **When:** Người chơi bấm nút "Xin số Thần Tài".
- **Then:**
  - Hệ thống trả về đủ 6 số hợp lệ với tỷ lệ chẵn/lẻ và nửa thấp/nửa cao cân đối.
  - Mỗi con số được gán cờ `Source: Random` kèm metadata `Strategy: Balanced`.

### AC 2.3: Chế độ Mixed (Hỗn hợp nguồn tạo số)
- **Kịch bản:** Kết hợp tự chọn, Lucky và Thần Tài trên cùng một dòng.
- **Given:** Người dùng tự chọn 2 số bằng tay (Manual: 08, 17).
- **When:** Người dùng dùng Lucky Journey cho 2 số tiếp theo và Thần Tài cho 2 số cuối.
- **Then:**
  - Dòng vé hiển thị đầy đủ 6 số.
  - Khi xem chi tiết, từng con số hiển thị đúng nguồn gốc xuất xứ (`08: Manual`, `17: Manual`, `24: Lucky`, `31: Lucky`, `39: Random`, `44: Random`).

---

## 3. PHÂN HỆ NOVELTY ENGINE (CHỐNG LẶP NỘI DUNG)

### AC 3.1: Chống lặp câu hỏi trong phiên
- **Given:** Người chơi đang trả lời câu hỏi để tạo 6 số cho Dòng A.
- **When:** Hệ thống lấy câu hỏi tiếp theo từ Backend.
- **Then:** Tuyệt đối không xuất hiện lại bất kỳ câu hỏi nào đã được trả lời trong Dòng A.

### AC 3.2: Đa dạng hóa Chủ đề (Theme Diversity)
- **Given:** Người chơi vừa trả lời một câu hỏi thuộc chủ đề `Chuyện công sở`.
- **When:** Hệ thống chọn câu hỏi tiếp theo.
- **Then:** Trọng số xác suất chọn câu hỏi thuộc chủ đề `Chuyện công sở` bị giảm $70\%$, ưu tiên chuyển sang chủ đề khác (Tình duyên, Tài chính, Tâm linh).

---

## 4. PHÂN HỆ TÀI KHOẢN & ĐỒNG BỘ DỮ LIỆU (AUTH & DATA MIGRATION)

### AC 4.1: Chơi ngay không cần đăng nhập (Guest Instant Play)
- **Given:** Người dùng lần đầu tiên truy cập website mà chưa đăng ký tài khoản.
- **When:** Người dùng tạo phiếu vé ở bất kỳ chế độ nào.
- **Then:**
  - Không có bất kỳ popup bắt buộc đăng nhập nào chặn ngang trải nghiệm.
  - Phiếu được lưu tạm vào `LocalStorage` và lưu vào Database với `GuestSessionToken`.

### AC 4.2: Tự động hợp nhất dữ liệu khi Đăng ký / Đăng nhập
- **Given:** Khách đã tạo 2 phiếu vé khi chưa đăng nhập.
- **When:** Khách bấm Đăng ký tài khoản mới và đăng nhập thành công.
- **Then:**
  - Hệ thống tự động gửi lệnh `sync-guest` để chuyển quyền sở hữu 2 phiếu vé sang `UserId` mới.
  - Danh sách phiếu xuất hiện đầy đủ trong mục "Lịch sử & Phiếu đã lưu" của tài khoản.

---

## 5. PHÂN HỆ QUẢN TRỊ VIÊN & BULK IMPORT (ADMIN CMS)

### AC 5.1: Import hàng loạt câu hỏi qua Excel / CSV
- **Given:** Admin truy cập trang `/admin/content/import`.
- **When:** Admin tải lên file `questions.xlsx` chứa 200 câu hỏi.
- **Then:**
  - Hệ thống chạy chế độ kiểm tra trước (Dry-run).
  - Nếu có dòng lỗi (thiếu cột, sai định dạng), hiển thị danh sách dòng lỗi chi tiết và không lưu vào DB.
  - Nếu toàn bộ file hợp lệ, cho phép bấm "Xác nhận Import" để nạp toàn bộ 200 câu hỏi vào CSDL trong 1 Transaction.

---

## 6. PHÂN HỆ TRIỂN KHAI & CONTAINER (DOCKER)

### AC 6.1: Khởi động hệ thống bằng 1 lệnh duy nhất
- **Given:** Môi trường đã cài đặt Docker và Docker Compose.
- **When:** Chạy lệnh `docker compose up -d`.
- **Then:**
  - Cả 3 containers (`sqlserver`, `backend`, `frontend`) khởi động thành công và đạt trạng thái `healthy`.
  - Frontend truy cập được tại `http://localhost:3000`.
  - Backend API và Swagger UI truy cập được tại `http://localhost:5000/swagger`.
  - Dữ liệu Database được duy trì liên tục qua Docker Volume.
