# Quản Lý Phòng Trọ Ngũ Hành Sơn — Hướng Dẫn Chạy Demo

Đồ án môn **Lập Trình Mạng**: hệ thống quản lý phòng trọ 3 tầng (Client WinForms → Server .NET 8 → MySQL) qua TCP/IP tự viết. Client **không bao giờ** mở kết nối database; mọi thao tác đi qua giao thức JSON một dòng tới Server.

---

## 1. Kiến trúc

```
┌────────────────────┐   JSON Lines / TCP 8888   ┌─────────────────────┐   ADO.NET   ┌──────────────┐
│  WinForms Client   │ ────────────────────────► │  Server .NET 8      │ ──────────► │  MySQL 8.0   │
│  (net8.0-windows)  │ ◄──────────────────────── │  Console App        │             │  Laragon     │
│  KHÔNG chạm DB     │   ResponsePacket(JSON)    │  Router→Service→Repo│             │  cổng 3306   │
└────────────────────┘                           └─────────────────────┘             └──────────────┘
        Form1 + 8 UserControl                        RequestRouter
        TcpClientService                             PermissionMatrix (BR-14)
        App.config: ServerHost/ServerPort            SessionStore, AuthService
```

- **Client** chỉ gửi `RequestPacket` và hiển thị kết quả; mọi business rule BR-01..BR-14 enforce tại **Server**.
- **Server** sở hữu toàn bộ nghiệp vụ, transaction MySQL, phân quyền 2 vai.
- **MySQL** là nguồn dữ liệu duy nhất; ràng buộc UNIQUE/CHECK/FK bảo vệ tầng cuối.

---

## 2. Yêu cầu môi trường

| Thành phần | Phiên bản / ghi chú |
|---|---|
| Windows | 11 (hoặc 10+) — WinForms chỉ chạy Windows |
| .NET SDK | 8.0 |
| MySQL | 8.0.30 qua **Laragon** |
| Cổng | `3306` (MySQL) và `8888` (Server TCP) phải trống |
| Công cụ | Visual Studio 2022 hoặc `dotnet` CLI |

Cấu hình kết nối nằm ở `QuanLyTro.Server/appsettings.json`:

```json
{
  "ConnectionString": "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User ID=root;Password=;SslMode=None;",
  "Port": 8888
}
```

> `User ID=root;Password=` là mặc định của Laragon. Đổi mật khẩu root thì sửa luôn chuỗi này.

---

## 3. Chạy lần đầu (đúng thứ tự)

**Bước 1 — Bật Laragon / MySQL** (Start All, đảm bảo MySQL 8.0.30 đang chạy ở 3306).

**Bước 2 — Tạo schema** (idempotent, chạy lại được nhiều lần):

```bash
dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --initialize-only
```

Kết quả in ra: `Database initialized.`

**Bước 3 — Tạo tài khoản chủ trọ** (xem mục 4 — schema **không** tự seed user; repo chưa có lệnh chính thức, làm theo thủ tục an toàn ở mục 4):

```bash
# 1) Sinh hash (xem mục 4 để biết cách chạy đoạn C# này)
#    → pbkdf2-sha256$210000$<salt>$<key>
# 2) Chèn vào users bằng mysql CLI của Laragon:
"E:/Apps/laragon/bin/mysql/mysql-8.0.30-winx64/bin/mysql.exe" -h 127.0.0.1 -P 3306 -u root quanly_phongtro_nhs \
  -e "INSERT INTO users (username, password_hash, full_name) VALUES ('admin', '<HASH>', 'Chủ Trọ Demo') ON DUPLICATE KEY UPDATE password_hash = VALUES(password_hash), full_name = VALUES(full_name);"
```

**Bước 4 — Chạy Server** (giữ terminal này mở):

```bash
dotnet run --project "QuanLyTro/QuanLyTro.Server"
```

Kết quả: `QuanLyTro Server (.NET 8) - TCP Port 8888` + `Server đang lắng nghe tại 0.0.0.0:8888`.

**Bước 5 — Chạy Client** (terminal khác):

```bash
dotnet run --project "QuanLyTro"
```

Cửa sổ đăng nhập hiện ra → nhập tài khoản ở mục 4.

---

## 4. Tài khoản demo

| Vai | Đăng nhập | Mật khẩu |
|---|---|---|
| Chủ trọ (Landlord) | `admin` | `admin-pass` |
| Người thuê (Tenant) | số CCCD | mặc định **6 số cuối CCCD** (chủ trọ để trống ô mật khẩu khi thêm hồ sơ) |

### Sự thật về seed admin (quan trọng)

`schema.sql` chỉ tạo **cấu trúc bảng**, **không** chèn sẵn tài khoản `admin`. Database Laragon hiện tại có `admin/admin-pass` là do được **đặt tay trong lúc kiểm thử E2E**, không phải hành vi tự động khi khởi tạo. Vì vậy máy mới cài cần tạo tài khoản thủ công như bước 3.

**Cách tạo đúng, an toàn** (dùng lại `PasswordHasher` của Server, không tự băm):

1. Tạo project console tạm dùng `QuanLyTro.Server` làm tham chiếu (hoặc chạy trong LINQPad/`dotnet script`) và in hash:

   ```csharp
   Console.WriteLine(QuanLyTro.Server.Security.PasswordHasher.Hash("admin-pass"));
   // → pbkdf2-sha256$210000$<salt>$<key>
   ```

2. Dán hash đó vào lệnh bước 3 (thay `<HASH>`). Lệnh `INSERT ... ON DUPLICATE KEY UPDATE` chạy lại chỉ đặt lại mật khẩu — không tạo trùng.

> Đừng tự viết PBKDF2 tay hoặc dùng hash công cụ khác — định dạng `pbkdf2-sha256$iterations$salt$key` phải khớp `PasswordHasher.Verify` thì đăng nhập mới qua. Không nhúng hash thô nào vào tài liệu này.

> Nếu chỉ muốn đổi mật khẩu admin đã tồn tại: chạy lại bước 3 với hash mới.

---

## 5. 22 action TCP & ma trận phân quyền

Server enforce tại `RequestRouter` → `PermissionMatrix.IsAllowed(action, role)` (BR-14). Mọi action ngoài `AUTH_LOGIN` đều cần token hợp lệ; thiếu/sai token → `"Phiên đăng nhập không hợp lệ hoặc đã hết hạn."`; sai vai → `"Không có quyền."`.

| # | Action | Payload vào | Trả ra | Landlord | Tenant |
|---|---|---|---|:---:|:---:|
| 1 | `AUTH_LOGIN` | `{Username, Password}` | `LoginResult{Token, FullName, Role}` | ✅ | ✅ |
| 2 | `ROOM_GET_ALL` | `{}` | `List<RoomDto>` | ✅ | — |
| 3 | `ROOM_ADD` | `RoomDto` | `RoomDto` | ✅ | — |
| 4 | `ROOM_UPDATE` | `RoomDto` | `bool` | ✅ | — |
| 5 | `ROOM_DELETE` | `{RoomId}` | `bool` | ✅ | — |
| 6 | `TENANT_GET_BY_ROOM` | `{RoomId}` | `List<TenantDto>` | ✅ | — |
| 7 | `TENANT_ADD` | `TenantDto` (+`plainPassword`) | `TenantDto` | ✅ | — |
| 8 | `TENANT_UPDATE` | `TenantDto` (+`plainPassword`) | `bool` | ✅ | — |
| 9 | `TENANT_CHECKOUT` | `{TenantId}` | `bool` | ✅ | — |
| 10 | `TENANT_DELETE` | `{TenantId}` | `bool` | ✅ | — |
| 11 | `CONTRACT_CREATE` | `ContractDto` | `ContractDto` | ✅ | — |
| 12 | `CONTRACT_TERMINATE` | `{ContractId, Notes}` | `bool` | ✅ | — |
| 13 | `CONTRACT_RENEW` | `{ContractId, NewEndDate}` | `bool` | ✅ | — |
| 14 | `CONTRACT_GET_ALL` | `{}` | `List<ContractListItem>` | ✅ | — |
| 15 | `UTILITY_GET_PREVIOUS` | `{RoomId, BillingMonth?}` | `UtilityReadingDto?` | ✅ | — |
| 16 | `UTILITY_RECORD` | `UtilityReadingDto` | `UtilityReadingDto` | ✅ | — |
| 17 | `INVOICE_CREATE` | `{RoomId, BillingMonth, OtherFees}` | `InvoiceDto` | ✅ | — |
| 18 | `INVOICE_GET_ALL` | `{BillingMonth?, RoomId?}` | `List<InvoiceDto>` | ✅ | — |
| 19 | `INVOICE_PAY` | `{InvoiceId}` | `bool` | ✅ | — |
| 20 | `INVOICE_GET_MINE` | `{}` (server suy từ token) | `List<InvoiceDto>` | — | ✅ |
| 21 | `REPORT_SUMMARY` | `{BillingMonth}` | `SummaryReportDto` | ✅ | — |
| 22 | `EXPORT_RESIDENCE` | `{}` | `List<ResidenceExportDto>` | ✅ | — |

**BR-14:** `INVOICE_GET_MINE` suy `room_id` từ `tenantId` trong phiên, **không** nhận `RoomId` từ client — người thuê không xem được hóa đơn phòng khác.

---

## 6. Luồng demo tháng (SRS §7)

Thứ tự thao tác trên Client (đăng nhập `admin`):

1. **Phòng** → Thêm phòng (số phòng, giá, sức chứa).
2. **Người thuê** → Thêm hồ sơ, chọn phòng vừa tạo; để trống mật khẩu = 6 số cuối CCCD.
3. **Hợp đồng** → Lập hợp đồng chọn phòng + người đại diện (phải đang ở phòng đó).
4. **Điện nước** → Chốt chỉ số tháng `yyyy-MM` (hệ thống tự gợi ý chỉ số cũ kỳ trước).
5. **Hóa đơn** → Lập hóa đơn tháng đó — Server tự tính `RoomPrice + (NewElec−OldElec)×ElecRate + (NewWater−OldWater)×WaterRate + OtherFees` (BR-10).
6. **Hóa đơn** → nút **Thu** xác nhận thanh toán (hóa đơn chuyển `Paid`).
7. **Thống kê** → xem doanh thu/công nợ tháng + **Xuất DS tạm trú** ra CSV.

Tenant đăng nhập bằng CCCD chỉ thấy tab **"Hóa đơn của tôi"** — read-only, đúng hóa đơn phòng mình.

---

## 7. Chi tiết giao thức mạng

- **Framing:** mỗi packet là **một dòng JSON UTF-8** kết thúc `\n`. Dùng `ReadLineAsync`/`WriteLineAsync` để không dính gói.
- **Giới hạn packet:** **1 MiB** một dòng.
- **Timeout client:** **10 giây** mỗi lần gửi/nhận.
- **Kết nối:** long-lived — Client giữ một kết nối TCP suốt phiên (mất kết nối → phải đăng nhập lại, đúng thiết kế).
- **Đồng bộ gửi:** `SemaphoreSlim(1,1)` serialize mọi request trên một kết nối, tránh hai luồng UI ghi đan xen.
- **Định dạng:** tên trường JSON camelCase; không BOM; `Data` là object JSON thật (không phải chuỗi escape).

```json
{"action":"ROOM_ADD","token":"<token>","data":{"roomNumber":"P101","price":2500000,"maxOccupants":2}}
{"success":true,"message":"Thành công.","data":{"id":1,"roomNumber":"P101"}}
```

---

## 8. Hạn chế đã biết

- **Client và Tests target `net8.0-windows`** → toàn bộ suite **không build/chạy được trên Linux CI**. Chấp nhận: đồ án demo trên Windows.
- **US-20** (tra cứu nhanh) deferred — không nằm trong demo.
- **US-19** khoảng tháng: Server chỉ trả 1 tháng; Client gọi nhiều tháng rồi cộng.
- Token session **in-memory**: Server khởi động lại = phải đăng nhập lại.
- `appsettings.json` hardcode mật khẩu rỗng của Laragon — chỉ dùng cho demo cục bộ.

---

## 9. Kiểm thử & bằng chứng

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --nologo
dotnet build "QuanLyTro/QuanLyTro.slnx" --nologo
```

| Hạng mục | Kết quả (2026-09-11) |
|---|---|
| Test toàn suite | **127 PASS, 0 FAIL** (117 trước Task 12 + 10 acceptance) |
| Build solution | **0 Warning(s), 0 Error(s)** |
| Coverage (line) | **27.3%** (5.522 / 20.221 dòng) — collector `Code Coverage` của Visual Studio; `XPlat Code Coverage` không khả dụng vì project chưa tham chiếu `coverlet.collector` |

Lệnh đo coverage:

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --collect:"Code Coverage" --nologo
```

Artifact `.coverage` nằm trong `QuanLyTro/QuanLyTro.Tests/TestResults/<guid>/` (đã bị `.gitignore`). Lưu ý con số này tính cả mã WinForms UI (khó test tự động) nên thấp hơn tỉ lệ phủ thực tế của tầng nghiệp vụ Server — `RequestRouter`, `PermissionMatrix`, các `Service`/`Repository` đều có test acceptance chạy MySQL thật.

**Acceptance test** (`AcceptanceTests.cs`, 10 test, MySQL thật + router thật):

- Luồng tháng SRS §7 đầu-cuối (tạo phòng → tenant → HĐ → điện nước → hóa đơn → thu → báo cáo → xuất tạm trú), assert DB sau từng bước.
- Ma trận phân quyền đủ 22 action qua `RequestRouter`; tenant bị chặn `"Không có quyền."`; landlord bị chặn `INVOICE_GET_MINE`.
- BR-14 room-isolation của `INVOICE_GET_MINE` (seed 2 phòng + 2 hóa đơn).
- Khóa đăng nhập US-23 với clock injectable (không sleep thật).
- BR-04, BR-05, BR-06, BR-09, BR-10, BR-11 trên MySQL thật.
