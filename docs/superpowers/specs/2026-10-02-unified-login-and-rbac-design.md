# Thiết kế: Trang đăng nhập 1 form chung & Phân quyền động 4 tác nhân (RBAC)

**Ngày:** 2026-10-02  
**Mục tiêu:** 
1. Tinh gọn trang đăng nhập `auth/index.html` thành đúng 1 form gồm Username + Password (bỏ hoàn toàn bộ chọn radio vai trò). Đăng nhập thành công tự động nhận diện role từ CSDL và điều hướng tới giao diện tác nhân tương ứng.
2. Thiết lập 4 vai trò rõ ràng:
   - **Chủ trọ (`Landlord`)**: Cấp cao nhất, toàn quyền hệ thống, xem thống kê doanh thu, quản lý và cấp quyền động cho Quản lý / Công an / Người thuê.
   - **Quản lý (`Manager`)**: Quản lý vận hành khu trọ (phòng, người thuê, hợp đồng, điện nước, thu tiền...) theo đúng các quyền do Chủ trọ phân quyền trong CSDL.
   - **Người thuê (`Tenant`)**: Đăng nhập bằng CCCD, chỉ xem hóa đơn cá nhân và lịch sử của mình.
   - **Công an phường (`Police`)**: Quản lý tạm trú, tra cứu nhân khẩu, xuất báo cáo nhân khẩu (chỉ đọc).
3. Chủ trọ có giao diện trực quan để bật/tắt (gán/thu hồi) từng quyền cho từng vai trò, lưu vào CSDL và đồng bộ tới TCP Server runtime.

---

## 1. Cấu trúc CSDL (MySQL)

### 1.1. Cập nhật ENUM vai trò trong bảng `users`
- Cột `role` chuyển thành: `ENUM('Landlord', 'Manager', 'Police', 'Tenant') NOT NULL DEFAULT 'Landlord'`.
- Tự động di chuyển an toàn bằng migration `ALTER TABLE users MODIFY COLUMN role ...`.

### 1.2. Bảng mới `role_permissions`
Lưu ma trận quyền động cho các vai trò cấp dưới:
```sql
CREATE TABLE IF NOT EXISTS role_permissions (
    role VARCHAR(20) NOT NULL,
    action VARCHAR(50) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (role, action)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 1.3. Dữ liệu hạt giống mặc định (Seed)
- **Tài khoản mặc định**:
  - `landlord` / `landlord` (Chủ trọ - toàn quyền)
  - `manager` / `manager` (Quản lý - vận hành phòng trọ)
  - `police` / `police` (Công an phường)
  - `100000000001` / `100000000001` (Khách thuê demo)
- **Quyền mặc định của Manager**: Toàn bộ thao tác nghiệp vụ cơ sở (phòng, người thuê, hợp đồng, điện nước, hóa đơn, xem thống kê). Không có quyền quản lý phân quyền hệ thống.
- **Quyền mặc định của Police**: Các quyền đọc tạm trú, danh sách phòng, người thuê, lịch sử thường trú.
- **Quyền mặc định của Tenant**: `INVOICE_GET_MINE`.

---

## 2. Shared Protocol & Enums

### 2.1. `UserRole` Enum
```csharp
public enum UserRole
{
    Landlord = 0,
    Tenant = 1,
    Police = 2,
    Manager = 3
}
```

### 2.2. TCP Action Names mới
Thêm vào `ActionNames.cs`:
- `PERMISSION_GET_MATRIX`: Lấy toàn bộ danh sách action và quyền hiện tại theo từng role.
- `PERMISSION_UPDATE_ROLE`: Cập nhật danh sách action cho phép của 1 role (Chỉ Landlord mới được gọi).

### 2.3. DTOs
```csharp
public sealed record RolePermissionsDto(string Role, List<string> Actions);
public sealed record UpdateRolePermissionsRequest(string Role, List<string> Actions);
```

---

## 3. Server Architecture & Permission Enforce

### 3.1. `PermissionMatrix` cải tiến (Dynamic Hybrid Cache)
- **Chủ trọ (`Landlord`)**: Luôn `true` cho mọi action quản trị (trừ `INVOICE_GET_MINE` của riêng khách thuê). Cơ chế bypass cứng trong code đảm bảo Chủ trọ không bao giờ bị khóa ngoài hệ thống.
- **Manager / Police / Tenant**: Kiểm tra dựa trên bộ nhớ đệm `ConcurrentDictionary<string, HashSet<UserRole>>` nạp từ bảng `role_permissions`.
- Khi Chủ trọ gọi `PERMISSION_UPDATE_ROLE`:
  1. Kiểm tra session của người gọi phải là `UserRole.Landlord`.
  2. Lưu thay đổi vào bảng `role_permissions`.
  3. Reload lại cache của `PermissionMatrix` ngay lập tức. Mọi kết nối TCP tiếp theo áp dụng quyền mới ngay.

### 3.2. `AuthService`
- Hỗ trợ đăng nhập tìm `users` (Landlord, Manager, Police) trước, sau đó tìm `tenants` (Tenant).
- Trả về `UserRole` chính xác cho client.

---

## 4. Giao diện người dùng (WebView2 / Web Assets)

### 4.1. Trang Đăng nhập (`QuanLyTro/Assets/wwwroot/auth/`)
- Xóa bỏ `div.roleseg` (3 radio buttons).
- Form duy nhất:
  - Input `lu`: Tên đăng nhập (Chủ trọ, Quản lý, Công an hoặc số CCCD người thuê).
  - Input `lp`: Mật khẩu.
  - Nút Đăng nhập.
- `auth.js`: Gửi `AUTH_LOGIN`, nhận role (`0: Landlord`, `1: Tenant`, `2: Police`, `3: Manager`).
  - `Landlord` -> `../landlord/index.html?role=Landlord&u=...`
  - `Manager` -> `../landlord/index.html?role=Manager&u=...` (dùng chung shell quản lý nhưng phân quyền tab theo quyền được cấp)
  - `Tenant` -> `../tenant/index.html?u=...`
  - `Police` -> `../police/index.html?u=...`

### 4.2. Giao diện Chủ trọ & Quản lý (`QuanLyTro/Assets/wwwroot/landlord/`)
- Nếu đăng nhập là `Landlord`:
  - Huy hiệu hiển thị: `Chủ trọ (Toàn quyền)`.
  - Có thêm tab **"Phân quyền" (`perms`)**.
- Nếu đăng nhập là `Manager`:
  - Huy hiệu hiển thị: `Quản lý cơ sở`.
  - Ẩn hoàn toàn tab "Phân quyền".
  - Các tab khác (Phòng, Khách thuê, Hóa đơn,...) tự động ẩn/mờ nếu role Manager bị Chủ trọ tắt quyền tương ứng trên Server.

### 4.3. Màn hình Phân quyền (`tab-perms` trong Landlord)
- Chọn vai trò cần phân quyền: **Quản lý khu trọ** / **Công an** / **Người thuê**.
- Danh sách các nhóm quyền dạng checklist switch trực quan:
  - *Quản lý Phòng trọ* (Xem danh sách, Thêm, Sửa, Xóa)
  - *Quản lý Khách thuê* (Xem danh sách, Thêm khách, Sửa thông tin, Trả phòng, Xóa)
  - *Quản lý Hợp đồng* (Tạo hợp đồng, Gia hạn, Chấm dứt)
  - *Quản lý Điện nước* (Ghi chỉ số)
  - *Quản lý Hóa đơn* (Tạo hóa đơn, Thu tiền/Thanh toán)
  - *Báo cáo & Tạm trú* (Xem thống kê doanh thu, Xuất hồ sơ tạm trú)
- Nút "Lưu phân quyền": Gửi `PERMISSION_UPDATE_ROLE` lên Server, hiển thị Toast thông báo thành công.

---

## 5. Kế hoạch kiểm thử tự động (Unit & Integration Tests)

1. `PermissionMatrixTests`:
   - Test Landlord có đầy đủ quyền.
   - Test Manager mặc định được cấp quyền vận hành nhưng không được gọi `PERMISSION_UPDATE_ROLE`.
   - Test cập nhật quyền động: khi thu hồi quyền `RoomDelete` của Manager, gọi thử `IsAllowed` trả về `false`.
2. `AuthTests`:
   - Đăng nhập thành công với 4 tài khoản demo (`landlord`, `manager`, `police`, `100000000001`).
   - Kiểm tra vai trò trả về chính xác theo enum `UserRole`.
3. `TcpRoundTripTests`:
   - Kiểm tra `PERMISSION_GET_MATRIX` và `PERMISSION_UPDATE_ROLE`.
   - Kiểm tra Manager bị chặn `403/Fail("Không có quyền.")` khi thực hiện hành động đã bị Chủ trọ thu hồi.
