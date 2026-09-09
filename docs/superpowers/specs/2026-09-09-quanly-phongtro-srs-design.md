# SOFTWARE REQUIREMENTS SPECIFICATION (SRS) & SYSTEM DESIGN
## HỆ THỐNG QUẢN LÝ PHÒNG TRỌ PHƯỜNG NGŨ HÀNH SƠN (CLIENT - SERVER TCP/IP)

**Môn học:** Lập Trình Mạng  
**Kiến trúc:** 3-tier Client - Server qua TCP/IP  
**Target Runtime:** .NET 8.0 (`net8.0`, `net8.0-windows`)  
**Database:** MySQL 8.0 (Laragon port 3306), Database: `quanly_phongtro_nhs`  

---

## 1. MỤC TIÊU & PHẠM VI HỆ THỐNG

### 1.1. Mục tiêu
- **Tập trung hóa:** Dữ liệu phòng trọ, khách thuê, hợp đồng, điện nước, hóa đơn lưu tập trung tại MySQL Server.
- **Tuân thủ lập trình mạng:** Client WinForms KHÔNG kết nối trực tiếp MySQL. Toàn bộ thao tác gửi qua giao thức TCP/IP đến Server xử lý.
- **Đa luồng (Multi-threading):** Server phục vụ đồng thời nhiều Client, dữ liệu đảm bảo tính toàn vẹn (ACID transactions, DB Unique Constraints).

### 1.2. Phạm vi
- **Trong phạm vi:** Quản lý phòng trọ, người thuê, hợp đồng, chỉ số điện nước, hóa đơn & thanh toán, thống kê doanh thu/công nợ, xuất file danh sách tạm trú (CSV/Excel) nộp công an phường Ngũ Hành Sơn.
- **Ngoài phạm vi:** Cổng thanh toán trực tuyến, app mobile, SMS/Email gateway.

---

## 2. QUY TẮC NGHIỆP VỤ (BUSINESS RULES - ENFORCED AT SERVER)

| Mã | Quy tắc nghiệp vụ | Chi tiết thực thi |
|---|---|---|
| **BR-01** | Số phòng duy nhất | Kiểm tra `RoomNumber` không trùng trong bảng `rooms`. |
| **BR-02** | Giới hạn sức chứa | Số người thuê hiện tại trong phòng `<= MaxOccupants`. Từ chối thêm người nếu đã đủ. |
| **BR-03** | CCCD duy nhất | Căn cước công dân của người thuê là duy nhất trên toàn hệ thống. |
| **BR-04** | 1 Hợp đồng hiệu lực / phòng | Một phòng chỉ có tối đa 1 hợp đồng ở trạng thái `Active`. |
| **BR-05** | Đại diện là thành viên phòng | Người đại diện ký hợp đồng bắt buộc phải đang ở trong chính phòng đó. |
| **BR-06** | Ngày hợp đồng hợp lệ | `EndDate > StartDate`. |
| **BR-07** | Chỉ số điện nước tăng dần | `NewElectricity >= OldElectricity` và `NewWater >= OldWater`. |
| **BR-08** | 1 bản ghi điện nước / tháng | Mỗi phòng chỉ chốt điện nước 1 lần trong 1 tháng (`BillingMonth`). |
| **BR-09** | Điều kiện lập hóa đơn | Phòng phải có hợp đồng `Active` và đã chốt điện nước của tháng đó. |
| **BR-10** | Công thức tính tiền hóa đơn | `TotalAmount = RoomPrice + (NewElec - OldElec)*ElecRate + (NewWater - OldWater)*WaterRate + OtherFees`. |
| **BR-11** | Bất biến sau thanh toán | Hóa đơn có trạng thái `Paid` KHÔNG được phép sửa đổi hoặc xóa. |
| **BR-12** | Điều kiện xóa phòng | Chỉ được xóa phòng khi phòng có trạng thái `Available` (không có người ở, không có hợp đồng active). |
| **BR-13** | Tính toàn vẹn giao dịch | Các thao tác tạo hợp đồng, trả phòng, lập hóa đơn phải chạy trong `MySqlTransaction`. |

---

## 3. THIẾT KẾ CƠ SỞ DỮ LIỆU (DATABASE SCHEMA - 3NF)

Tên CSDL: `quanly_phongtro_nhs` (MySQL / InnoDB / UTF8MB4)

### 3.1. Bảng `users` (Tài khoản quản trị chủ trọ)
```sql
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### 3.2. Bảng `rooms` (Danh mục phòng trọ)
```sql
CREATE TABLE rooms (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_number VARCHAR(20) NOT NULL UNIQUE,
    price DECIMAL(12, 2) NOT NULL,
    max_occupants INT NOT NULL DEFAULT 2,
    status ENUM('Available', 'Rented', 'Maintenance') DEFAULT 'Available',
    description VARCHAR(255) NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### 3.3. Bảng `tenants` (Người thuê trọ)
```sql
CREATE TABLE tenants (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NULL,
    full_name VARCHAR(100) NOT NULL,
    dob DATE NOT NULL,
    id_card VARCHAR(20) NOT NULL UNIQUE,
    phone VARCHAR(20) NOT NULL,
    hometown VARCHAR(150) NOT NULL,
    workplace VARCHAR(150) NULL,
    is_temporary_registered BOOLEAN DEFAULT FALSE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE SET NULL
);
```

### 3.4. Bảng `contracts` (Hợp đồng thuê phòng)
```sql
CREATE TABLE contracts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NOT NULL,
    representative_tenant_id INT NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    rental_price DECIMAL(12, 2) NOT NULL,
    deposit_amount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    status ENUM('Active', 'Expired', 'Terminated') DEFAULT 'Active',
    notes TEXT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (room_id) REFERENCES rooms(id),
    FOREIGN KEY (representative_tenant_id) REFERENCES tenants(id)
);
```

### 3.5. Bảng `utility_readings` (Chỉ số điện nước)
```sql
CREATE TABLE utility_readings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NOT NULL,
    billing_month VARCHAR(7) NOT NULL, -- Format: 'YYYY-MM'
    old_electricity INT NOT NULL,
    new_electricity INT NOT NULL,
    electricity_rate DECIMAL(10, 2) NOT NULL DEFAULT 3500,
    old_water INT NOT NULL,
    new_water INT NOT NULL,
    water_rate DECIMAL(10, 2) NOT NULL DEFAULT 10000,
    recorded_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_room_month (room_id, billing_month),
    FOREIGN KEY (room_id) REFERENCES rooms(id)
);
```

### 3.6. Bảng `invoices` (Hóa đơn hàng tháng)
```sql
CREATE TABLE invoices (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NOT NULL,
    contract_id INT NOT NULL,
    billing_month VARCHAR(7) NOT NULL, -- Format: 'YYYY-MM'
    room_amount DECIMAL(12, 2) NOT NULL,
    electricity_amount DECIMAL(12, 2) NOT NULL,
    water_amount DECIMAL(12, 2) NOT NULL,
    other_fees DECIMAL(12, 2) NOT NULL DEFAULT 0, -- Wifi, garbage, etc.
    total_amount DECIMAL(12, 2) NOT NULL,
    status ENUM('Unpaid', 'Paid') DEFAULT 'Unpaid',
    paid_at DATETIME NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_invoice_room_month (room_id, billing_month),
    FOREIGN KEY (room_id) REFERENCES rooms(id),
    FOREIGN KEY (contract_id) REFERENCES contracts(id)
);
```

---

## 4. GIAO THỨC TRUYỀN THÔNG TCP/IP & JSON PACKET

### 4.1. Khung gói tin (Framing)
Mỗi gói tin gửi qua TCP là chuỗi JSON mã hóa UTF-8 kết thúc bằng ký tự xuống dòng `\n`.  
Client và Server sử dụng `StreamReader.ReadLineAsync()` và `StreamWriter.WriteLineAsync()` đảm bảo không bị dính gói hay phân mảnh tin.

### 4.2. Cấu trúc gói tin Yêu cầu (Request)
```json
{
  "Action": "ROOM_ADD",
  "Token": "session-token-or-empty",
  "Data": "{\"RoomNumber\":\"P101\",\"Price\":2500000,\"MaxOccupants\":2,\"Description\":\"Phòng lầu 1\"}"
}
```

### 4.3. Cấu trúc gói tin Phản hồi (Response)
```json
{
  "Success": true,
  "Message": "Thêm phòng thành công.",
  "Data": "{\"Id\":1,\"RoomNumber\":\"P101\"}"
}
```

### 4.4. Danh mục Action & Payload
1. `AUTH_LOGIN` -> In: `{ Username, Password }` | Out: `{ Token, FullName }`
2. `ROOM_GET_ALL` -> In: `{}` | Out: `List<RoomDto>`
3. `ROOM_ADD` -> In: `RoomDto` | Out: `RoomDto`
4. `ROOM_UPDATE` -> In: `RoomDto` | Out: `bool`
5. `ROOM_DELETE` -> In: `{ RoomId }` | Out: `bool`
6. `TENANT_GET_BY_ROOM` -> In: `{ RoomId }` | Out: `List<TenantDto>`
7. `TENANT_ADD` -> In: `TenantDto` | Out: `TenantDto`
8. `TENANT_UPDATE` -> In: `TenantDto` | Out: `bool`
9. `TENANT_CHECKOUT` -> In: `{ TenantId }` | Out: `bool`
10. `CONTRACT_CREATE` -> In: `ContractDto` | Out: `ContractDto`
11. `CONTRACT_TERMINATE` -> In: `{ ContractId, Notes }` | Out: `bool`
12. `UTILITY_GET_PREVIOUS` -> In: `{ RoomId }` | Out: `{ OldElectricity, OldWater }`
13. `UTILITY_RECORD` -> In: `UtilityReadingDto` | Out: `UtilityReadingDto`
14. `INVOICE_CREATE` -> In: `CreateInvoiceDto` | Out: `InvoiceDto`
15. `INVOICE_GET_ALL` -> In: `{ BillingMonth }` | Out: `List<InvoiceDto>`
16. `INVOICE_PAY` -> In: `{ InvoiceId }` | Out: `bool`
17. `REPORT_SUMMARY` -> In: `{ BillingMonth }` | Out: `SummaryReportDto`
18. `EXPORT_RESIDENCE` -> In: `{}` | Out: `List<ResidenceExportDto>`

---

## 5. THIẾT KẾ CẤU TRÚC CODE SOLUTION (.NET 8)

```
QuanLyTro/
├── QuanLyTro.sln / QuanLyTro.slnx
│
├── QuanLyTro.Shared/               [Class Library .NET 8]
│   ├── Models/                     (Entity DTOs)
│   ├── Protocol/                   (NetworkPacket, NetworkResponse, ActionNames)
│   └── Helpers/                    (JsonHelper)
│
├── QuanLyTro.Server/               [Console App .NET 8]
│   ├── Config/                     (ServerConfig: Port 8888, MySQL connection string)
│   ├── Data/                       (DatabaseContext: ADO.NET connection factory)
│   ├── Repositories/               (RoomRepo, TenantRepo, ContractRepo, UtilityRepo, InvoiceRepo, UserRepo)
│   ├── Services/                   (BusinessLogic: Validation BR-01 -> BR-13)
│   ├── Network/                    (TcpListenerServer, ClientHandler multi-threaded)
│   └── Program.cs
│
└── QuanLyTro.Client/               [WinForms App .NET 8-windows]
    ├── Network/                    (TcpClientService: Connect, SendRequestAsync, OnDisconnected)
    ├── Forms/
    │   ├── FrmLogin.cs             (Màn hình đăng nhập tài khoản chủ trọ)
    │   ├── FrmMain.cs              (Dashboard tổng quan + điều hướng các tab)
    │   ├── FrmRooms.cs             (Quản lý danh sách, thêm, sửa, xóa phòng)
    │   ├── FrmTenants.cs           (Hồ sơ người thuê, gán phòng, trả phòng)
    │   ├── FrmContracts.cs         (Lập hợp đồng, thanh lý, gia hạn)
    │   ├── FrmUtilities.cs         (Ghi chỉ số điện nước hàng tháng)
    │   ├── FrmInvoices.cs          (Lập hóa đơn, xác nhận thanh toán, in/xem)
    │   └── FrmReports.cs           (Thống kê doanh thu, công nợ, xuất DS tạm trú CSV)
    └── Program.cs
```
