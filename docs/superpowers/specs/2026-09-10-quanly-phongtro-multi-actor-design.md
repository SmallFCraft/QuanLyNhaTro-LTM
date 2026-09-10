# DESIGN: PHÂN VAI HAI TÁC NHÂN — "NHÀ TRỘ VĂN MINH" (DELTA SRS 2026-09-09)

**Ngày:** 2026-09-10
**Cơ sở:** Bổ sung cho `2026-09-09-quanly-phongtro-srs-design.md` — không thay thế.
**Mục tiêu:** Nâng hệ thống từ 1 tác nhân (chủ trọ) lên 2 tác nhân (chủ trọ + người thuê), đồng bộ 3 tài liệu (báo cáo môn học, SRS, plan) và vá các gap đã phát hiện giữa báo cáo ↔ SRS ↔ plan.

---

## 1. BỐI CẢNH & QUYẾT ĐỊNH ĐÃ CHỐT

- Trạng thái code: Task 1–2 của plan đã xong thật (4 project, Shared đủ DTO + 18 action, protocol tests PASS, build OK với SDK 10.0.401). Task 3–11 chưa có code → thời điểm tốt để chèn phân vai mà không phải đập code cũ.
- Quyết định của người phụ trách (2026-09-10):
  1. **2 tác nhân:** Chủ trọ (Landlord) + người thuê (Tenant). Không làm vai công an phường — xuất tạm trú vẫn là chức năng của chủ trọ.
  2. **Mục tiêu:** Báo cáo môn học + demo bảo vệ theo hướng đa tác nhân.
  3. **Plan cũ được giữ**, vá + bổ sung; không viết lại toàn bộ.
  4. **Vá gap rẻ:** thêm `TENANT_DELETE`, `CONTRACT_RENEW`, `CONTRACT_GET_ALL`, lọc hóa đơn theo phòng, khóa tài khoản 5 lần sai. `SEARCH_FAST` (US-20) ghi rõ deferred.

---

## 2. TÁC NHÂN & PHÂN QUYỀN (SERVER ENFORCED)

### 2.1. Vai trò

| Vai | Đăng nhập | Quyền |
|---|---|---|
| **Landlord** (chủ trọ) | Bảng `users` (username/password như cũ) | Toàn bộ action trừ `INVOICE_GET_MINE` |
| **Tenant** (người thuê) | Bảng `tenants` — username = CCCD, mật khẩu từ cột `password_hash` mới | Chỉ `AUTH_LOGIN` + `INVOICE_GET_MINE` |

### 2.2. Luồng đăng nhập (1 action duy nhất `AUTH_LOGIN`)

1. Server thử bảng `users` trước; không khớp thì thử bảng `tenants` theo `username = id_card`.
2. Thành công → token session + `LoginResult(Token, FullName, Role)`. `LoginResult` thêm trường `Role` (`Landlord` | `Tenant`).
3. `SessionStore` lưu `(userId, Role)` thay vì chỉ `userId`.
4. Sai thông tin: trả chung một thông báo cho cả 2 bảng, không tiết lộ bảng nào tồn tại.

### 2.3. Ma trận quyền trong `RequestRouter`

- Bảng tĩnh `Action → AllowedRoles`. Mọi action (trừ `AUTH_LOGIN`) kiểm tra token VÀ role trước khi gọi service. Tenant gọi action khác → `Success=false`, `"Không có quyền."`
- Client không tự quyết định quyền — chỉ ẩn/hiện menu theo `Role` cho UX.

### 2.4. Khóa tài khoản (US-23)

- Đếm sai liên tiếp **theo username** trong bộ nhớ (in-memory, reset khi đăng nhập thành công).
- Sai quá 5 lần liên tiếp → từ chối đăng nhập trong **1 phút**, thông báo thời gian còn lại. Đếm theo username kể cả khi tài khoản không tồn tại (chống dò tên).

**Quy tắc nghiệp vụ mới — BR-14:** Người thuê chỉ được xem hóa đơn của phòng mình đang ở (server suy ra `room_id` từ session, không nhận `RoomId` từ client).

---

## 3. THAY ĐỔI SCHEMA (1 THAY ĐỔI DUY NHẤT)

```sql
ALTER TABLE tenants ADD COLUMN password_hash VARCHAR(255) NULL AFTER is_temporary_registered;
```

- Cập nhật cả `database/schema.sql` trong plan (Task 3) và §3.3 SRS.
- Mật khẩu người thuê do **chủ trọ đặt** khi thêm/sửa hồ sơ. Để trống → server băm **6 số cuối CCCD** làm mật khẩu mặc định.
  `# ponytail: mật khẩu mặc định = 6 số cuối CCCD cho demo nhanh; đổi thành bắt buộc nhập + bắt đổi lần đầu đăng nhập nếu chạy thật.`
- Băm bằng đúng `PasswordHasher` PBKDF2 đã có trong plan Task 4 — không thêm cơ chế mới.
- Tenant không có mật khẩu (`password_hash IS NULL`) → không đăng nhập được.

---

## 4. THAY ĐỔI GIAO THỨC: 18 → 22 ACTION

### 4.1. Action mới (4)

| Action | Payload vào | Trả ra | Phục vụ |
|---|---|---|---|
| `INVOICE_GET_MINE` | `{}` (server suy từ token) | `List<InvoiceDto>` hóa đơn phòng đang ở | US-24 |
| `TENANT_DELETE` | `{ TenantId }` | `bool` — chỉ xóa người **đã trả phòng** (`room_id IS NULL`); người đã ký hợp đồng bị FK chặn → thông báo rõ | US-06 |
| `CONTRACT_RENEW` | `{ ContractId, NewEndDate }` | `bool` — chỉ hợp đồng `Active`; `NewEndDate > EndDate` hiện tại; giữ BR-06 | US-10 |
| `CONTRACT_GET_ALL` | `{}` | `List<ContractDto>` kèm số phòng + tên người đại diện, sắp xếp `end_date` tăng dần; Client tự lọc sắp hết hạn 30 ngày | US-11 |

### 4.2. Action sửa payload (2)

- `INVOICE_GET_ALL`: `{ BillingMonth?, RoomId? }` — cả hai tùy chọn (ít nhất một); phục vụ US-15 (lọc theo phòng) và US-17 (lọc còn nợ phía client theo `Status`).
- `REPORT_SUMMARY`: giữ `{ BillingMonth }`. US-19 (khoảng tháng) làm bằng cách client gọi nhiều tháng rồi cộng dòng tổng — ghi chú deferred phần server-range.

### 4.3. Bản vá SRS §4.2/§4.3 (đã lệch code thực tế)

SRS §4.2 mô tả `Data` là chuỗi JSON lồng (string escape). Code và plan Task 2 đã dùng **`JsonElement`**. Cập nhật ví dụ SRS:

```json
{"Action":"ROOM_ADD","Token":"...","Data":{"RoomNumber":"P101","Price":2500000,"MaxOccupants":2,"Description":"Phòng lầu 1"}}
```

Packet đầy đủ luôn 1 dòng, kết thúc `\n` (giữ nguyên framing).

---

## 5. UI CLIENT (WinForms)

- `FrmLogin` (shell `Form1`): đăng nhập chung cho cả 2 vai; sau login, `Role` quyết định menu.
  - **Landlord:** tab đầy đủ như SRS §5 (Rooms, Tenants, Contracts, Utilities, Invoices, Reports).
  - **Tenant:** 1 tab duy nhất "Hóa đơn của tôi" — grid hóa đơn phòng đang ở, breakdown từng khoản, trạng thái đã/chưa trả. Không thấy nút thao tác ghi.
- Thêm/sửa người thuê (tab Tenants của chủ trọ): thêm 1 ô nhập mật khẩu (để trống = 6 số cuối CCCD).
- Hợp đồng: nút "Gia hạn" (prompt ngày kết thúc mới) bên cạnh "Chấm dứt"; thêm cột hiển thị số ngày còn lại, tô đậm < 30 ngày.

---

## 6. ĐỒNG BỘ TÀI LIỆU (BẮT BUỘC — demo phải khớp báo cáo)

### 6.1. `BAO_CAO_USER_STORY.md`

1. §4.2 "Ngoài phạm vi": bỏ câu "hệ thống chỉ phục vụ một vai trò duy nhất là chủ trọ" → thay bằng: hệ thống phục vụ 2 vai trò — chủ trọ và người thuê (người thuê chỉ xem hóa đơn của mình).
2. §7.7: `US-23` đổi **Could → Must** (đã có đăng nhập + khóa tài khoản); giữ nguyên tiêu chí khóa 5 lần.
3. Thêm **US-24** (Must, phụ thuộc US-23): *"Là người thuê, tôi muốn đăng nhập bằng CCCD và xem hóa đơn/công nợ của phòng mình."* — tiêu chí: chỉ thấy hóa đơn phòng đang ở; không thao tác ghi được; server từ chối action ngoài vai.
4. §10.2: "18 hành động" → "22 hành động"; Phụ lục bổ sung 4 action mới vào đúng nhóm.
5. §10.1/§10.3: đổi "một Client — một luồng" → "mỗi kết nối một luồng xử lý độc lập (async Task)" — khớp plan Task 8, tránh hội đồng chất vấn `new Thread`.
6. Thêm **BR-14** vào bảng §8: người thuê chỉ xem hóa đơn phòng mình.
7. US-20 (tra cứu nhanh): giữ Should, thêm ghi chú *"deferred — không nằm trong phạm vi demo"*.

### 6.2. SRS `2026-09-09-quanly-phongtro-srs-design.md`

1. §3.3: thêm cột `password_hash` vào bảng `tenants`.
2. §2: thêm BR-14.
3. §4.2/§4.3: sửa ví dụ `Data` thành JSON thật (mục 4.3 ở trên).
4. §4.4: cập nhật danh sách 22 action + payload `INVOICE_GET_ALL` mới.
5. §5: bổ sung vai tenant vào cây Form (`FrmLogin` dùng chung, tab "Hóa đơn của tôi").

### 6.3. Plan `2026-09-09-quanly-phongtro-net8.md`

1. Renumber: Task 3–9 cũ → 4–10; chèn task mới **"Task 5: Phân vai người thuê"** (schema `password_hash`, `AuthService` 2 bảng + lockout, permission matrix router, `INVOICE_GET_MINE`, UI tenant, tests); Task 10–11 cũ → 11–12.
2. Task cũ về password/login (Task 4 mới): thêm lockout 5 lần/1 phút.
3. Task schema (Task 3 mới): thêm cột `password_hash` vào `schema.sql`.
4. Task UI (Task 11 mới): thêm nút Gia hạn, TENANT_DELETE, ô mật khẩu tenant, tab hóa đơn tenant.
5. Task test cuối (Task 12 mới): thêm test matrix BR-14, US-24, lockout; bỏ câu "Không thêm US-23" — US-23 giờ Must.
6. Sửa Task 9 cũ (connection shell): Form1 hiện danh sách tab theo Role.

---

## 7. KIỂM THỬ BỔ SUNG

- **Auth:** login đúng 2 vai; sai password; sai username không tồn tại; 5 lần sai → khóa 1 phút (dùng clock injectable để test không chờ thật); tenant login sau khi chủ trọ đặt mật khẩu.
- **Phân quyền:** tenant gọi `ROOM_GET_ALL`/`INVOICE_PAY`/… → `"Không có quyền."`; landlord gọi `INVOICE_GET_MINE` → từ chối (dùng `INVOICE_GET_ALL`).
- **BR-14:** `INVOICE_GET_MINE` chỉ trả hóa đơn phòng đang ở; sau trả phòng → danh sách rỗng.
- **Gap vá:** `TENANT_DELETE` chặn người đang ở và người đã ký hợp đồng; `CONTRACT_RENEW` chặn ngày lùi; `CONTRACT_GET_ALL` sắp xếp đúng; `INVOICE_GET_ALL` lọc theo phòng/tháng.

## 8. NGOÀI PHẠM VI (GHI RÕ TRONG BÁO CÁO)

- Vai công an phường, tra cứu khách vãng lai (`SEARCH_FAST`/US-20), khoảng tháng server-side cho `REPORT_SUMMARY` (US-19 dùng cộng client-side), lưu token khi client mất kết nối (mất kết nối = đăng nhập lại).
