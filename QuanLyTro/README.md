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
        Form1 + WebView2 (Assets/wwwroot)            DieuPhoiYeuCau
        TcpClientService                             MaTranPhanQuyen (BR-14)
        App.config: ServerHost/ServerPort            SessionStore, XacThucService
```

- **Client** chỉ gửi `RequestPacket` và hiển thị kết quả; mọi business rule BR-01..BR-16 enforce tại **Server**.
- **Server** sở hữu toàn bộ nghiệp vụ, giao_dich MySQL, phân quyền 4 vai trò.
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
  "ConnectionString": "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User ID=root;MatKhau=;SslMode=None;",
  "Port": 8888
}
```

> `User ID=root;MatKhau=` là mặc định của Laragon. Đổi mật khẩu root thì sửa luôn chuỗi này.

---

## 3. Chạy lần đầu (đúng thứ tự)

**Bước 1 — Bật Laragon / MySQL** (Start All, đảm bảo MySQL 8.0.30 đang chạy ở 3306).

> Mọi lệnh `dotnet` dưới đây dùng đường dẫn tính từ **thư mục gốc repository** (`LapTrinhMang/`), không phải từ `QuanLyTro/`.

**Bước 2 — Tạo schema** (idempotent, chạy lại được nhiều lần):

```bash
dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --initialize-only
```

Kết quả in ra: `Database initialized.`

**Bước 3 — Tạo tài khoản chủ trọ** (xem mục 4 — schema **không** tự seed user; repo chưa có lệnh chính thức, làm theo thủ tục an toàn ở mục 4):

```bash
# 1) Sinh hash (xem mục 4 để biết cách chạy đoạn C# này)
#    → pbkdf2-sha256$210000$<salt>$<key>
# 2) Chèn vào tai_khoan bằng mysql CLI của Laragon.
#    BẮT BUỘC --default-character-set=utf8mb4, nếu không CLI Windows dùng cp850 và
#    tên tiếng Việt bị hỏng thành "Ch? Tr? Demo".
"E:/Apps/laragon/bin/mysql/mysql-8.0.30-winx64/bin/mysql.exe" --default-character-set=utf8mb4 -h 127.0.0.1 -P 3306 -u root quanly_phongtro_nhs \
  -e "INSERT INTO tai_khoan (tenDangNhap, mat_khau_hash, ho_ten, vai_tro) VALUES ('admin', '<HASH>', 'Chủ Trọ Demo', 'ChuTro') ON DUPLICATE KEY UPDATE mat_khau_hash = VALUES(mat_khau_hash), ho_ten = VALUES(ho_ten), vai_tro = 'ChuTro';"
```

> **Muốn demo thêm vai Công an phường (BR-16)?** Sinh một hash khác (mật khẩu khác,
> vẫn dùng lại `PasswordHasher` ở mục 4) rồi chạy INSERT kiểu y hệt:

```bash
"E:/Apps/laragon/bin/mysql/mysql-8.0.30-winx64/bin/mysql.exe" --default-character-set=utf8mb4 -h 127.0.0.1 -P 3306 -u root quanly_phongtro_nhs \
  -e "INSERT INTO tai_khoan (tenDangNhap, mat_khau_hash, ho_ten, vai_tro) VALUES ('police_nhs', '<POLICE_HASH>', 'Công An Phường', 'CongAn') ON DUPLICATE KEY UPDATE mat_khau_hash = VALUES(mat_khau_hash), ho_ten = VALUES(ho_ten), vai_tro = 'CongAn';"
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

Bộ dữ liệu mẫu tạo bằng `dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --seed-demo`.
Mật khẩu **trùng tên đăng nhập** cho mọi tài khoản:

| Vai | Đăng nhập | Mật khẩu |
|---|---|---|
| Chủ trọ (ChuTro) | `landlord` | `landlord` |
| Công an phường (CongAn) | `police` | `police` |
| Người thuê (KhachThue) | `100000000001` (CCCD) | `100000000001` |

Dữ liệu mẫu: 3 phòng (`P201` đang cho thuê, `P202`/`P203` trống), 1 người thuê,
1 hợp đồng HieuLuc, 1 kỳ chỉ số điện nước và 1 hóa đơn `ChuaThu` 2.245.000 đ cho kỳ hiện tại.
Seeder idempotent (chạy lại chỉ ghi đè dòng mẫu) và nằm trong dải id 7001-7009.

### Tài khoản khác trong DB cục bộ

`schema.sql` chỉ tạo **cấu trúc bảng**, **không** chèn sẵn tài khoản. Lệnh `--seed-demo`
là cách chính thức tạo dữ liệu mẫu.

> Đừng tự viết PBKDF2 tay hoặc dùng hash từ công cụ khác — định dạng
> `pbkdf2-sha256$iterations$salt$key` phải khớp `PasswordHasher.Verify` thì đăng nhập mới qua.
> `DuLieuMau` dùng chính `PasswordHasher` nên không thể lệch định dạng.

> Người thuê tự tạo qua UI (không dùng seeder) mặc định có mật khẩu là **6 số cuối CCCD**
> khi chủ trọ để trống ô mật khẩu.

---

## 5. 26 hanh_dong TCP & ma trận phân quyền

Server enforce tại `DieuPhoiYeuCau` → `MaTranPhanQuyen.IsAllowed(hanh_dong, vai_tro)` (BR-14). Mọi hanh_dong ngoài `DANG_NHAP` đều cần token hợp lệ; thiếu/sai token → `"Phiên đăng nhập không hợp lệ hoặc đã hết hạn."`; sai vai → `"Không có quyền."`.

| # | Action | Payload vào | Trả ra | ChuTro | KhachThue |
|---|---|---|---|:---:|:---:|
| 1 | `DANG_NHAP` | `{TenDangNhap, MatKhau}` | `KetQuaDangNhap{Token, HoTen, VaiTro}` | ✅ | ✅ |
| 2 | `PHONG_LAY_TAT_CA` | `{}` | `List<PhongDto>` | ✅ | — |
| 3 | `PHONG_THEM` | `PhongDto` | `PhongDto` | ✅ | — |
| 4 | `PHONG_CAP_NHAT` | `PhongDto` | `bool` | ✅ | — |
| 5 | `PHONG_XOA` | `{PhongId}` | `bool` | ✅ | — |
| 6 | `KHACH_THUE_THEO_PHONG` | `{PhongId}` | `List<KhachThueDto>` | ✅ | — |
| 7 | `KHACH_THUE_THEM` | `KhachThueDto` (+`plainPassword`) | `KhachThueDto` | ✅ | — |
| 8 | `KHACH_THUE_CAP_NHAT` | `KhachThueDto` (+`plainPassword`) | `bool` | ✅ | — |
| 9 | `KHACH_THUE_TRA_PHONG` | `{TenantId}` | `bool` | ✅ | — |
| 10 | `KHACH_THUE_XOA` | `{TenantId}` | `bool` | ✅ | — |
| 11 | `HOP_DONG_TAO` | `HopDongDto` | `HopDongDto` | ✅ | — |
| 12 | `HOP_DONG_CHAM_DUT` | `{HopDongId, GhiChu}` | `bool` | ✅ | — |
| 13 | `HOP_DONG_GIA_HAN` | `{HopDongId, NewEndDate}` | `bool` | ✅ | — |
| 14 | `HOP_DONG_LAY_TAT_CA` | `{}` | `List<MucHopDongItem>` | ✅ | — |
| 15 | `DIEN_NUOC_LAY_KY_TRUOC` | `{PhongId, KyCuoc?}` | `ChiSoDienNuocDto?` | ✅ | — |
| 16 | `DIEN_NUOC_GHI_SO` | `ChiSoDienNuocDto` | `ChiSoDienNuocDto` | ✅ | — |
| 17 | `HOA_DON_TAO` | `{PhongId, KyCuoc, PhiKhac}` | `HoaDonDto` | ✅ | — |
| 18 | `HOA_DON_LAY_TAT_CA` | `{KyCuoc?, PhongId?}` | `List<HoaDonDto>` | ✅ | — |
| 19 | `HOA_DON_THANH_TOAN` | `{InvoiceId}` | `bool` | ✅ | — |
| 20 | `HOA_DON_CUA_TOI` | `{}` (server suy từ token) | `List<HoaDonDto>` | — | ✅ |
| 21 | `BAO_CAO_TONG_QUAN` | `{KyCuoc}` | `BaoCaoTongQuanDto` | ✅ | — |
| 22 | `XUAT_HO_SO_TAM_TRU` | `{}` | `List<XuatHoSoTamTruDto>` | ✅ | — |

**BR-14:** `HOA_DON_CUA_TOI` suy `phong_id` từ `khachThueId` trong phiên, **không** nhận `PhongId` từ client — người thuê không xem được hóa đơn phòng khác.

---

## 6. Luồng demo tháng (SRS §7)

Thứ tự thao tác trên Client (đăng nhập `admin`):

1. **Phòng** → Thêm phòng (số phòng, giá, sức chứa).
2. **Người thuê** → Thêm hồ sơ, chọn phòng vừa tạo; để trống mật khẩu = 6 số cuối CCCD.
3. **Hợp đồng** → Lập hợp đồng chọn phòng + người đại diện (phải đang ở phòng đó).
4. **Điện nước** → Chốt chỉ số tháng `yyyy-MM` (hệ thống tự gợi ý chỉ số cũ kỳ trước).
5. **Hóa đơn** → Lập hóa đơn tháng đó — Server tự tính `RoomPrice + (NewElec−OldElec)×ElecRate + (NuocMoi−NuocCu)×GiaNuoc + PhiKhac` (BR-10).
6. **Hóa đơn** → nút **Thu** xác nhận thanh toán (hóa đơn chuyển `DaThu`).
7. **Thống kê** → xem doanh thu/công nợ tháng + **Xuất DS tạm trú** ra CSV.

KhachThue đăng nhập bằng CCCD chỉ thấy tab **"Hóa đơn của tôi"** — read-only, đúng hóa đơn phòng mình.

---

## 7. Chi tiết giao thức mạng

- **Framing:** mỗi packet là **một dòng JSON UTF-8** kết thúc `\n`. Dùng `ReadLineAsync`/`WriteLineAsync` để không dính gói.
- **Giới hạn packet:** **1 MiB** một dòng.
- **Timeout client:** **10 giây** mỗi lần gửi/nhận.
- **Kết nối:** long-lived — Client giữ một kết nối TCP suốt phiên (mất kết nối → phải đăng nhập lại, đúng thiết kế).
- **Đồng bộ gửi:** `SemaphoreSlim(1,1)` serialize mọi request trên một kết nối, tránh hai luồng UI ghi đan xen.
- **Định dạng:** tên trường JSON camelCase; không BOM; `Data` là object JSON thật (không phải chuỗi escape).

```json
{"hanh_dong":"PHONG_THEM","token":"<token>","data":{"soPhong":"P101","gia_thue":2500000,"soNguoiToiDa":2}}
{"success":true,"message":"Thành công.","data":{"id":1,"soPhong":"P101"}}
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

Artifact `.coverage` nằm trong `QuanLyTro/QuanLyTro.Tests/TestResults/<guid>/` (đã bị `.gitignore`). Lưu ý con số này tính cả mã WinForms UI (khó test tự động) nên thấp hơn tỉ lệ phủ thực tế của tầng nghiệp vụ Server — `DieuPhoiYeuCau`, `MaTranPhanQuyen`, các `Service`/`Repository` đều có test acceptance chạy MySQL thật.

**Acceptance test** (`AcceptanceTests.cs`, 10 test, MySQL thật + router thật):

- Luồng tháng SRS §7 đầu-cuối (tạo phòng → tenant → HĐ → điện nước → hóa đơn → thu → báo cáo → xuất tạm trú), assert DB sau từng bước.
- Ma trận phân quyền đủ 26 hanh_dong qua `DieuPhoiYeuCau`; tenant bị chặn `"Không có quyền."`; landlord bị chặn `HOA_DON_CUA_TOI`.
- BR-14 room-isolation của `HOA_DON_CUA_TOI` (seed 2 phòng + 2 hóa đơn).
- Khóa đăng nhập US-23 với clock injectable (không sleep thật).
- BR-04, BR-05, BR-06, BR-09, BR-10, BR-11 trên MySQL thật.
