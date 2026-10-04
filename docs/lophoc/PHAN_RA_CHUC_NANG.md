# XÁC ĐỊNH CHỨC NĂNG, NHÓM, ĐỐI TƯỢNG & PHÂN CÔNG 5 NGƯỜI

Tài liệu gốc: [GIOI_THIEU_DE_TAI.md](GIOI_THIEU_DE_TAI.md) (FR-01→FR-30, BR-01→BR-19).
Tài liệu giao diện người dùng: [XAC_DINH_CHUC_NANG_MENU_GIAO_DIEN.md](XAC_DINH_CHUC_NANG_MENU_GIAO_DIEN.md).

---

## 1. Cây chức năng nguyên tử (phân rã đến mức không thể chia nhỏ thêm)

Tiêu chí dừng: mỗi nút lá (mức 4: `.x.y`) là **1 thao tác nguyên tử** (1 trường nhập/kiểm tra, 1 phép tính đơn, 1 truy vấn hoặc 1 lệnh ghi dữ liệu).

```
HệThốngTrọ_NgũHànhSơn/
├── A. HạTầng_Mạng_CSDL [FR-21, FR-22, NFR-01..05, 08, 09]
│   ├── A1. KếtNối_Client_Server [FR-21]
│   │   ├── A1.1 Đọc cấu hình kết nối từ client
│   │   │   ├── A1.1.1 Đọc địa chỉ IP máy chủ (mặc định 127.0.0.1)
│   │   │   ├── A1.1.2 Đọc cổng TCP từ cấu hình (mặc định 8888)
│   │   │   └── A1.1.3 Khởi tạo TcpClient theo cấu hình socket
│   │   ├── A1.2 Trạng thái kết nối
│   │   │   ├── A1.2.1 Bật cờ/đèn xanh khi kết nối TCP thành công
│   │   │   └── A1.2.2 Cập nhật nhãn trạng thái trên thanh đáy ứng dụng
│   │   └── A1.3 Xử lý mất kết nối [NFR-08]
│   │       ├── A1.3.1 Bắt ngoại lệ SocketException khi mất luồng TCP
│   │       ├── A1.3.2 Hiển thị hộp thoại cảnh báo mất kết nối máy chủ
│   │       └── A1.3.3 Giữ nguyên màn hình hiện tại, không làm sập ứng dụng
│   ├── A2. GiaoThức_TCP_JSON [NFR-03]
│   │   ├── A2.1 Đóng gói yêu cầu (Client)
│   │   │   ├── A2.1.1 Gán tên hành động vào trường Action (29 actions tiếng Việt không dấu)
│   │   │   ├── A2.1.2 Đính kèm mã phiên vào trường Token
│   │   │   ├── A2.1.3 Tuần tự hóa dữ liệu gửi sang chuỗi JSON ở trường Data
│   │   │   └── A2.1.4 Thêm ký tự xuống dòng `\n` vào cuối gói tin
│   │   ├── A2.2 Nhận và bóc gói tin (Server)
│   │   │   ├── A2.2.1 Đọc luồng byte đến khi gặp ký tự `\n`
│   │   │   ├── A2.2.2 Giải mã byte sang chuỗi UTF-8 trọn gói
│   │   │   └── A2.2.3 Giải tuần tự hóa JSON thành đối tượng Request
│   │   └── A2.3 Đóng gói phản hồi (Server)
│   │       ├── A2.3.1 Gán cờ thành công/thất bại vào trường Success
│   │       ├── A2.3.2 Gán thông báo tiếng Việt vào trường Message
│   │       ├── A2.3.3 Tuần tự hóa kết quả trả về vào trường Data
│   │       └── A2.3.4 Thêm ký tự `\n` và gửi ngược lại Client
│   └── A3. ĐaClient_ĐồngThời [FR-22, NFR-02, NFR-04, NFR-05]
│       ├── A3.1 Lắng nghe và cấp phát luồng
│       │   ├── A3.1.1 TcpListener mở cổng 8888 và chờ kết nối tới
│       │   ├── A3.1.2 Chấp nhận kết nối và tạo TcpClient mới
│       │   └── A3.1.3 Khởi tạo một Task/luồng xử lý độc lập cho Client đó
│       ├── A3.2 Quản lý vòng đời kết nối & Log có cấu trúc
│       │   ├── A3.2.1 Duy trì luồng đọc lặp cho đến khi Client ngắt
│       │   ├── A3.2.2 Ghi log ServerLog ra console: thời gian, endpoint, action, duration, OK/FAIL
│       │   └── A3.2.3 Dọn dẹp tài nguyên và đóng luồng khi Client thoát
│       └── A3.3 Đảm bảo nhất quán dữ liệu phía CSDL
│           ├── A3.3.1 Áp dụng ràng buộc UNIQUE cho số phòng và CCCD
│           ├── A3.3.2 Mở giao dịch (MySqlTransaction) cho thao tác ghi nhiều bảng
│           └── A3.3.3 Khóa dòng ghi của InnoDB (`FOR UPDATE`) tránh hai Client ghi đè
├── B. XácThực_PhânQuyền [FR-23, FR-24, FR-28, FR-30, NFR-06, NFR-07]
│   ├── B1. ĐăngNhập_HợpNhất [FR-23]
│   │   ├── B1.1 Tiếp nhận thông tin đăng nhập
│   │   │   ├── B1.1.1 Nhập tên tài khoản (hoặc CCCD đối với khách thuê)
│   │   │   ├── B1.1.2 Nhập mật khẩu
│   │   │   └── B1.1.3 Gửi yêu cầu `DANG_NHAP` sang máy chủ
│   │   ├── B1.2 Xác thực tài khoản máy chủ
│   │   │   ├── B1.2.1 Truy vấn tài khoản nội bộ trong bảng `tai_khoan`
│   │   │   ├── B1.2.2 Nếu không có, truy vấn tài khoản khách trong bảng `khach_thue`
│   │   │   ├── B1.2.3 Băm mật khẩu nhập vào và so khớp với `mat_khau_hash`
│   │   │   ├── B1.2.4 Đăng nhập sai: tăng bộ đếm số lần sai trong bộ nhớ
│   │   │   └── B1.2.5 Đăng nhập đúng: xóa bộ đếm số lần sai
│   │   ├── B1.3 Chặn dò mật khẩu (Lockout) [NFR-07]
│   │   │   ├── B1.3.1 Kiểm tra nếu số lần sai liên tiếp đạt 5 lần
│   │   │   ├── B1.3.2 Ghi mốc thời gian tạm khóa 1 phút
│   │   │   └── B1.3.3 Từ chối đăng nhập và báo số giây còn lại nếu trong thời gian khóa
│   │   └── B1.4 Khởi tạo phiên làm việc & Điều hướng
│   │       ├── B1.4.1 Sinh chuỗi token ngẫu nhiên không trùng lặp
│   │       ├── B1.4.2 Lưu token kèm mã người dùng và vai trò vào SessionStore
│   │       └── B1.4.3 Trả về Client: Token, Họ tên, Vai trò để client tự mở đúng shell giao diện
│   ├── B2. KháchThuê_TựĐăngKý [FR-24 / BR-17]
│   │   ├── B2.1 Nhập form đăng ký tài khoản từ xa
│   │   │   ├── B2.1.1 Nhập họ tên, ngày sinh, quê quán, nơi làm việc
│   │   │   ├── B2.1.2 Nhập số CCCD (kiểm tra đúng 12 chữ số)
│   │   │   ├── B2.1.3 Nhập số điện thoại liên hệ
│   │   │   └── B2.1.4 Nhập mật khẩu tự chọn (kiểm tra độ dài >= 6 ký tự)
│   │   └── B2.2 Xử lý đăng ký phía Server (`DANG_KY`)
│   │       ├── B2.2.1 Route công khai (Public Route) bỏ qua kiểm tra token phiên
│   │       ├── B2.2.2 Kiểm tra trùng lặp số CCCD trong bảng `khach_thue`
│   │       ├── B2.2.3 Băm mật khẩu bằng `PasswordHasher.Hash`
│   │       ├── B2.2.4 Lưu hồ sơ mới vào bảng `khach_thue` với `phong_id = NULL`
│   │       └── B2.2.5 Điền sẵn CCCD sang ô đăng nhập để khách đăng nhập nhận phòng
│   ├── B3. MaTrận_PhânQuyền_Động [FR-28 / BR-15]
│   │   ├── B3.1 Đọc ma trận quyền (`PHAN_QUYEN_LAY_MA_TRAN`)
│   │   │   ├── B3.1.1 Chủ trọ yêu cầu xem ma trận phân quyền
│   │   │   ├── B3.1.2 Đọc toàn bộ danh sách (vai_tro, hanh_dong) từ bảng `quyen_vai_tro`
│   │   │   └── B3.1.3 Trả về danh mục quyền theo từng nhóm chức năng (Hợp đồng, Phòng, Điện nước...)
│   │   ├── B3.2 Cập nhật quyền theo vai trò (`PHAN_QUYEN_CAP_NHAT_VAI_TRO`)
│   │   │   ├── B3.2.1 Chủ trọ tick/bỏ tick các quyền cho Quản lý, Công an, Khách thuê
│   │   │   ├── B3.2.2 Ghi đè danh sách hành động được phép vào bảng `quyen_vai_tro`
│   │   │   └── B3.2.3 Cập nhật ngay bộ nhớ đệm `MaTranPhanQuyen` mà không cần khởi động lại Server
│   │   └── B3.3 Tự động gieo bù quyền qua sổ cái
│   │       ├── B3.3.1 Tạo bảng sổ cái `quyen_mac_dinh_da_ap_dung` nếu chưa có
│   │       ├── B3.3.2 Đối chiếu danh mục quyền mặc định code với sổ cái trong 1 query
│   │       └── B3.3.3 Gieo bù batch các quyền mới xuất hiện mà không khôi phục quyền Chủ trọ đã thu hồi
│   └── B4. Chặn_Vai_ChỉXem [FR-30 / BR-16]
│       ├── B4.1 Nhận diện thao tác ghi (Thêm / Sửa / Xóa / Kích hoạt)
│       ├── B4.2 Kiểm tra vai trò của phiên có phải Công an phường hay không
│       └── B4.3 Nếu là Công an phường gửi thao tác ghi: máy chủ từ chối và báo lỗi rõ ràng
├── C. PhòngTrọ [FR-01, FR-02, FR-03 / BR-01, BR-12]
│   ├── C1. ThêmPhòng [FR-01 / BR-01]
│   │   ├── C1.1 Nhập thông tin phòng
│   │   │   ├── C1.1.1 Nhập số phòng
│   │   │   ├── C1.1.2 Nhập giá thuê tháng
│   │   │   ├── C1.1.3 Nhập sức chứa tối đa (người)
│   │   │   └── C1.1.4 Nhập mô tả phòng (tầng, tiện ích)
│   │   ├── C1.2 Kiểm tra hợp lệ phía Server
│   │   │   ├── C1.2.1 Kiểm tra số phòng không được để trống
│   │   │   ├── C1.2.2 Kiểm tra giá thuê > 0
│   │   │   ├── C1.2.3 Kiểm tra sức chứa > 0
│   │   │   └── C1.2.4 Kiểm tra trùng số phòng trên toàn hệ thống (bắt lỗi MySQL 1062)
│   │   └── C1.3 Lưu phòng mới
│   │       ├── C1.3.1 Đặt trạng thái ban đầu là Trống (Trong)
│   │       └── C1.3.2 Ghi bản ghi vào bảng `phong`
│   ├── C2. Sửa_XóaPhòng [FR-02 / BR-12]
│   │   ├── C2.1 Sửa thông tin phòng
│   │   │   ├── C2.1.1 Chọn phòng cần sửa
│   │   │   ├── C2.1.2 Cập nhật giá thuê, sức chứa, trạng thái, mô tả
│   │   │   └── C2.1.3 Lưu thay đổi vào bảng `phong`
│   │   └── C2.2 Xóa phòng [BR-12]
│   │       ├── C2.2.1 Chọn phòng cần xóa
│   │       ├── C2.2.2 Kiểm tra số người đang ở qua khóa ngoại `khach_thue`
│   │       ├── C2.2.3 Kiểm tra hợp đồng còn hiệu lực qua bảng `hop_dong`
│   │       ├── C2.2.4 Nếu có người hoặc có hợp đồng: báo lỗi từ chối xóa
│   │       └── C2.2.5 Nếu phòng trống hoàn toàn: hiển thị hộp thoại xác nhận và xóa
│   └── C3. Xem_DanhSách_CảnhBáo [FR-03, FR-04]
│       ├── C3.1 Hiển thị lưới danh sách phòng
│       │   ├── C3.1.1 Đọc toàn bộ phòng từ bảng `phong` (`PHONG_LAY_TAT_CA`)
│       │   ├── C3.1.2 Đếm số người đang ở thực tế cho từng phòng
│       │   └── C3.1.3 Lọc danh sách theo trạng thái (Trống / Đang thuê / Bảo trì)
│       └── C3.2 Cảnh báo phòng treo hợp đồng
│           ├── C3.2.1 Đọc song song danh sách hợp đồng hiệu lực (`HOP_DONG_LAY_TAT_CA`)
│           ├── C3.2.2 Phát hiện phòng có người ở (`soNguoiHienTai > 0`) nhưng không có HĐ HieuLuc
│           └── C3.2.3 Gắn thẻ cảnh báo cam `Treo HĐ (N người)` nổi bật trên giao diện
├── D. NgườiThuê [FR-05, FR-06, FR-07, FR-08 / BR-02, BR-03]
│   ├── D1. ThêmNgườiThuê [FR-05 / BR-02, BR-03]
│   │   ├── D1.1 Nhập hồ sơ người thuê
│   │   │   ├── D1.1.1 Nhập họ tên
│   │   │   ├── D1.1.2 Nhập ngày sinh
│   │   │   ├── D1.1.3 Nhập số CCCD (12 chữ số)
│   │   │   ├── D1.1.4 Nhập số điện thoại liên lạc
│   │   │   ├── D1.1.5 Nhập quê quán và nơi học tập/làm việc
│   │   │   └── D1.1.6 Chọn phòng trọ gán vào
│   │   ├── D1.2 Kiểm tra hợp lệ phía Server
│   │   │   ├── D1.2.1 Kiểm tra trùng CCCD trên toàn hệ thống (bắt lỗi MySQL 1062)
│   │   │   ├── D1.2.2 Đếm số người hiện tại trong phòng mục tiêu
│   │   │   └── D1.2.3 Kiểm tra số người hiện tại < Sức chứa tối đa (BR-02)
│   │   └── D1.3 Lưu hồ sơ người thuê
│   │       ├── D1.3.1 Băm mật khẩu (mặc định 6 số cuối CCCD nếu để trống)
│   │       ├── D1.3.2 Ghi bản ghi vào bảng `khach_thue`
│   │       └── D1.3.3 Cập nhật trạng thái phòng sang Đang thuê (`DaThue`)
│   ├── D2. Sửa_XóaNgườiThuê [FR-06]
│   │   ├── D2.1 Sửa hồ sơ người thuê
│   │   │   ├── D2.1.1 Chọn người thuê cần cập nhật
│   │   │   ├── D2.1.2 Cập nhật thông tin cá nhân hoặc đặt lại mật khẩu
│   │   │   └── D2.1.3 Lưu thay đổi vào bảng `khach_thue`
│   │   └── D2.2 Xóa hồ sơ người thuê
│   │       ├── D2.2.1 Chọn người thuê cần xóa
│   │       ├── D2.2.2 Kiểm tra trường `phong_id`: nếu khác NULL thì từ chối xóa
│   │       └── D2.2.3 Nếu đã trả phòng (`phong_id = NULL`): thực hiện xóa vĩnh viễn
│   ├── D3. ChuyểnPhòng_TrảPhòng [FR-07]
│   │   ├── D3.1 Chuyển phòng
│   │   │   ├── D3.1.1 Chọn phòng mới cho người thuê
│   │   │   ├── D3.1.2 Kiểm tra phòng mới còn sức chứa
│   │   │   ├── D3.1.3 Cập nhật `phong_id` mới cho người thuê
│   │   │   └── D3.1.4 Nếu phòng cũ hết sạch người: chuyển trạng thái phòng cũ về Trống
│   │   └── D3.2 Trả phòng (`KHACH_THUE_TRA_PHONG`)
│   │       ├── D3.2.1 Chọn người thuê xác nhận trả phòng
│   │       ├── D3.2.2 Cập nhật `phong_id = NULL` trong bảng `khach_thue`
│   │       └── D3.2.3 Nếu phòng không còn ai: chuyển trạng thái phòng về Trống
│   └── D4. XuấtHồSơ_TạmTrú [FR-08]
│       ├── D4.1 Lọc danh sách người đang cư trú thực tế
│       └── D4.2 Xuất file danh sách tạm trú phục vụ nộp cơ quan công an
├── E. HợpĐồng_QR_CheckIn [FR-09, FR-10, FR-11, FR-25, FR-26 / BR-04..06, BR-18, BR-19]
│   ├── E1. LậpHợpĐồng [FR-09, FR-25 / BR-04, BR-05, BR-06, BR-18]
│   │   ├── E1.1 Chọn thông tin hợp đồng
│   │   │   ├── E1.1.1 Chọn phòng cần lập hợp đồng
│   │   │   ├── E1.1.2 Chọn người đại diện (khách trong phòng hoặc khách chờ gán phòng)
│   │   │   ├── E1.1.3 Nhập ngày bắt đầu và ngày kết thúc (> ngày bắt đầu)
│   │   │   ├── E1.1.4 Nhập giá thuê thỏa thuận và tiền đặt cọc
│   │   │   └── E1.1.5 Chọn hình thức bàn giao: Trực tiếp (`HieuLuc`) hoặc Chờ nhận phòng QR (`ChoNhanPhong`)
│   │   ├── E1.2 Kiểm tra điều kiện ở Server
│   │   │   ├── E1.2.1 Khóa dòng phòng `SELECT ... FOR UPDATE`
│   │   │   ├── E1.2.2 Kiểm tra phòng chưa có hợp đồng HieuLuc hoặc ChoNhanPhong nào khác (BR-04)
│   │   │   └── E1.2.3 Nếu bàn giao trực tiếp: kiểm tra người đại diện đã ở trong phòng (BR-05)
│   │   └── E1.3 Sinh mã và lưu hợp đồng
│   │       ├── E1.3.1 Nếu là ChoNhanPhong: sinh mã QR CSPRNG 128-bit và mã PIN 8 số ngẫu nhiên
│   │       ├── E1.3.2 Lưu bản ghi vào bảng `hop_dong`
│   │       └── E1.3.3 Nếu là HieuLuc: cập nhật phòng sang trạng thái DaThue
│   ├── E2. BànGiao_InPhiếu_QR [FR-25 / BR-18]
│   │   ├── E2.1 Xem mã QR & PIN (`HOP_DONG_SINH_QR`)
│   │   │   ├── E2.1.1 Chủ trọ bấm "Xem QR / PIN..." trên toolbar hợp đồng
│   │   │   ├── E2.1.2 Server kiểm tra hợp đồng đang ở trạng thái ChoNhanPhong
│   │   │   └── E2.1.3 Trả về mã QR payload và mã PIN 8 số
│   │   └── E2.2 Hiển thị & In phiếu bàn giao
│   │       ├── E2.2.1 Vẽ mã QR lên modal bằng thư viện `qrcode.min.js`
│   │       ├── E2.2.2 Tải ảnh QR định dạng PNG về máy tính
│   │       └── E2.2.3 Kích hoạt in phiếu QR/PIN an toàn qua iframe ẩn (tránh bị WebView2 chặn popup)
│   ├── E3. KháchThuê_NhậnPhòng_QR [FR-26 / BR-19]
│   │   ├── E3.1 Thu thập mã nhận phòng phía Client
│   │   │   ├── E3.1.1 Cách 1: Tải file ảnh QR từ máy, giải mã qua canvas + `jsQR.min.js`
│   │   │   ├── E3.1.2 Cách 2: Bật camera webcam quét trực tiếp thời gian thực (loop 300ms)
│   │   │   └── E3.1.3 Cách 3: Nhập tay mã PIN 8 số dự phòng
│   │   └── E3.2 Kích hoạt nhận phòng ở Server (`KHACH_THUE_NHAN_PHONG_QR`)
│   │       ├── E3.2.1 Mở Transaction, tìm hợp đồng ChoNhanPhong khớp token hoặc PIN (`FOR UPDATE`)
│   │       ├── E3.2.2 Kiểm tra đúng người đại diện (CCCD từ phiên trùng `nguoi_dai_dien_id`)
│   │       ├── E3.2.3 Khóa phòng và kiểm tra phòng còn sức chứa
│   │       ├── E3.2.4 Gán phòng cho khách: `UPDATE khach_thue SET phong_id = @phongId`
│   │       ├── E3.2.5 Kích hoạt hợp đồng: `UPDATE hop_dong SET trang_thai='HieuLuc', ma_qr_token=NULL, ma_pin=NULL`
│   │       ├── E3.2.6 Cập nhật phòng: `UPDATE phong SET trang_thai='DaThue' WHERE trang_thai <> 'BaoTri'`
│   │       └── E3.2.7 Commit Transaction, tải lại giao diện dashboard khách thuê
│   ├── E4. GiaHạn_ChấmDứt [FR-10]
│   │   ├── E4.1 Gia hạn hợp đồng (`HOP_DONG_GIA_HAN`)
│   │   │   ├── E4.1.1 Chọn hợp đồng đang có hiệu lực (HieuLuc)
│   │   │   ├── E4.1.2 Nhập ngày kết thúc mới (> ngày kết thúc hiện tại)
│   │   │   └── E4.1.3 Cập nhật trường `ngay_ket_thuc` trong CSDL
│   │   └── E4.2 Chấm dứt hợp đồng tuân thủ Luật Nhà ở 2023 (`HOP_DONG_CHAM_DUT`)
│   │       ├── E4.2.1 Chọn hợp đồng cần thanh lý (HieuLuc hoặc ChoNhanPhong)
│   │       ├── E4.2.2 Nếu hợp đồng còn hạn: bắt buộc nhập lý do chấm dứt hợp pháp
│   │       ├── E4.2.3 Gợi ý và thiết lập ngày bàn giao dự kiến (mặc định hôm nay + 30 ngày)
│   │       ├── E4.2.4 Lưu vết ngày thông báo và lý do vào `ghi_chu`: `[TB: ... | Bàn giao: ...] <lý do>`
│   │       ├── E4.2.5 Cập nhật trạng thái thành `ChamDut`, xóa mã QR/PIN nếu là hợp đồng chờ
│   │       └── E4.2.6 Giữ nguyên khách trong phòng (không auto-evict) chờ làm thủ tục trả phòng thực tế
│   └── E5. CảnhBáo_HếtHạn [FR-11]
│       ├── E5.1 Tính số ngày còn lại của từng hợp đồng so với ngày hiện tại
│       └── E5.2 Lọc và tô màu cảnh báo hợp đồng sắp hết hạn trong vòng 30 ngày
├── F. ĐiệnNước [FR-12, FR-13 / BR-07, BR-08]
│   ├── F1. ChốtChỉSố [FR-12 / BR-07, BR-08]
│   │   ├── F1.1 Chọn phòng trọ và tháng chốt (yyyy-MM)
│   │   ├── F1.2 Tự điền chỉ số cũ từ bản ghi chốt của kỳ liền trước (`DIEN_NUOC_LAY_KY_TRUOC`)
│   │   ├── F1.3 Nhập chỉ số điện mới và chỉ số nước mới
│   │   ├── F1.4 Nhập đơn giá điện, nước áp dụng cho kỳ
│   │   ├── F1.5 Chọn hình thức tính tiền nước (theo khối hoặc khoán theo số người)
│   │   ├── F1.6 Kiểm tra hợp lệ: số mới >= số cũ, đơn giá > 0, chưa chốt trùng tháng
│   │   └── F1.7 Ghi bản ghi vào bảng `chi_so_dien_nuoc` (`DIEN_NUOC_GHI_SO`)
│   └── F2. TínhTiền_ĐiệnNước [FR-13]
│       ├── F2.1 Tính sản lượng tiêu thụ điện và nước
│       ├── F2.2 Tính thành tiền điện = Sản lượng điện × Đơn giá điện
│       ├── F2.3 Tính thành tiền nước theo khối hoặc theo số người × Đơn giá nước
│       └── F2.4 Hiển thị xem trước số tiền tức thì trên giao diện
├── G. HóaĐơn_ThanhToán [FR-14, FR-15, FR-16, FR-17, FR-27 / BR-08..11, BR-14]
│   ├── G1. LậpHóaĐơn [FR-14 / BR-08, BR-09, BR-10, BR-13]
│   │   ├── G1.1 Kiểm tra điều kiện: phòng có HĐ hiệu lực, đã chốt điện nước, chưa lập HĐ tháng này
│   │   ├── G1.2 Tập hợp các khoản: tiền phòng (từ HĐ), tiền điện, tiền nước, phí dịch vụ khác
│   │   ├── G1.3 Tính tổng tiền hóa đơn = Tổng các thành phần chi phí
│   │   └── G1.4 Lưu hóa đơn trạng thái `ChuaThu` vào bảng `hoa_don` (`HOA_DON_TAO`)
│   ├── G2. Xem_LịchSử_HóaĐơn [FR-15]
│   │   ├── G2.1 Lọc hóa đơn theo tháng hoặc theo phòng (`HOA_DON_LAY_TAT_CA`)
│   │   └── G2.2 Xem bảng kê chi tiết từng khoản cấu thành hóa đơn
│   ├── G3. XácNhận_ThanhToán [FR-16 / BR-11]
│   │   ├── G3.1 Chọn hóa đơn chưa thu cần xác nhận
│   │   ├── G3.2 Cập nhật trạng thái `DaThu` và lưu thời điểm thu tiền `ngay_dong = NOW()` (`HOA_DON_THANH_TOAN`)
│   │   └── G3.3 Khóa bất biến: không cho phép sửa đổi hay xóa hóa đơn đã thu tiền ở tầng CSDL
│   ├── G4. QuảnLý_CôngNợ [FR-17]
│   │   ├── G4.1 Lọc danh sách các phòng còn nợ tiền cước theo tháng
│   │   └── G4.2 Tổng hợp số tiền nợ đọng cần đôn đốc thu hồi
│   └── G5. KháchThuê_TraCứuCước [FR-27 / BR-14]
│       ├── G5.1 Server tự suy phòng từ `UserId` phiên đăng nhập của khách (`HOA_DON_CUA_TOI`)
│       ├── G5.2 Hiển thị bảng kê chi tiết cước tháng này kèm trạng thái nộp tiền
│       ├── G5.3 Nút sao chép cú pháp chuyển khoản ngân hàng tự động vào clipboard
│       └── G5.4 Tra cứu lịch sử hóa đơn các tháng trước có phân trang server-side
├── H. ThốngKê_LưuTrú_CôngAn [FR-18, FR-19, FR-20, FR-29]
│   ├── H1. ThốngKê_VậnHành [FR-18, FR-19]
│   │   ├── H1.1 Tính tỷ lệ lấp đầy phòng và tổng số người đang cư trú
│   │   ├── H1.2 Thống kê doanh thu đã thu và công nợ theo từng tháng
│   │   └── H1.3 Hiển thị khối KPI trực quan trên bảng điều khiển tổng quan
│   ├── H2. TraCứu_Nhanh [FR-20]
│   │   └── H2.1 Tìm kiếm không phân biệt hoa thường theo tên, số phòng, CCCD, số điện thoại
│   └── H3. NghiệpVụ_CôngAn_Phường [FR-29]
│       ├── H3.1 Tra cứu danh sách công dân đăng ký tạm trú / lưu trú trên địa bàn
│       ├── H3.2 Tra cứu tình trạng cư trú theo từng số phòng
│       ├── H3.3 Xem lịch sử biến động cư trú (`LICH_SU_CU_TRU_LAY`)
│       └── H3.4 Xuất file báo cáo lịch sử cư trú phục vụ kiểm tra hành chính (`XUAT_LICH_SU_CU_TRU`)
```

---

## 2. Xác định nhóm chức năng và đối tượng sử dụng

| Nhóm chức năng | Tên nhóm | Mô tả tóm tắt | Đối tượng sử dụng chính |
|---|---|---|---|
| **A** | Hạ tầng mạng & CSDL | Kết nối TCP, giao thức JSON 29 actions, đa luồng, ACID, log console | Hệ thống, Kỹ thuật viên |
| **B** | Xác thực & Phân quyền | Đăng nhập hợp nhất, khách tự đăng ký, ma trận phân quyền động RBAC, lockout | Tất cả vai trò |
| **C** | Quản lý phòng trọ | Danh mục phòng, giá, sức chứa, trạng thái, cảnh báo Treo HĐ | Chủ trọ, Quản lý (Công an xem) |
| **D** | Quản lý người thuê | Hồ sơ người thuê, CCCD, gán phòng, chuyển/trả phòng, xuất tạm trú | Chủ trọ, Quản lý (Công an xem) |
| **E** | Hợp đồng & QR Check-In | Lập HĐ, sinh QR/PIN 128-bit, khách nhận phòng webcam/PIN, chấm dứt Luật Nhà ở | Chủ trọ, Quản lý, Khách thuê |
| **F** | Quản lý điện, nước | Ghi chỉ số tháng, tự điền số cũ, tính tiền theo khối hoặc theo người | Chủ trọ, Quản lý (Công an xem) |
| **G** | Hóa đơn & Thanh toán | Lập hóa đơn, xác nhận thu tiền, quản lý công nợ, khách tra cứu phân trang | Chủ trọ, Quản lý, Khách thuê |
| **H** | Thống kê & Lưu trú Công an | KPI vận hành, doanh thu, tra cứu nhanh, kiểm tra và xuất lịch sử cư trú | Chủ trọ, Quản lý, Công an phường |

---

## 3. Phân công trách nhiệm nhóm 5 thành viên

Dự án được phân chia thành 5 module độc lập, có ranh giới giao tiếp rõ ràng thông qua hợp đồng giao thức TCP (ActionNames, RequestPacket, ResponsePacket):

```
                   ┌──────────────────────────────────────────────┐
                   │               MÁY CHỦ SERVER                │
                   │     TcpListener Server (Cổng 8888)           │
                   └──────────────────────┬───────────────────────┘
                                          │ Giao thức JSON qua TCP
       ┌──────────────────┬───────────────┴──────────────┬──────────────────┐
       ▼                  ▼                              ▼                  ▼
┌──────────────┐  ┌──────────────┐                ┌──────────────┐  ┌──────────────┐
│  Thành viên 1 │  │  Thành viên 2 │                │  Thành viên 3 │  │  Thành viên 4 │
│  Hạ tầng mạng │  │  Phòng trọ,   │                │  Điện nước,   │  │  Khách thuê, │
│  & Phân quyền │  │  Người thuê & │                │  Hóa đơn &    │  │  Tự đăng ký &│
│  (Nhóm A, B)  │  │  Hợp đồng     │                │  Công nợ      │  │  QR Check-in │
│              │  │  (Nhóm C, D, E)│                │  (Nhóm F, G)  │  │  (Nhóm B2,E3)│
└──────────────┘  └──────────────┘                └──────────────┘  └──────────────┘
                                          ▲
                                          │ Phối hợp kiểm thử
                                  ┌───────┴──────┐
                                  │ Thành viên 5 │
                                  │ Thống kê,    │
                                  │ Công an lưu  │
                                  │ trú & Test   │
                                  │ (Nhóm H)     │
                                  └──────────────┘
```

### Chi tiết phân công từng thành viên

#### Thành viên 1: Kiến trúc hạ tầng mạng, Giao thức TCP & Phân quyền động (Trưởng nhóm)
- **Phụ trách:** Nhóm chức năng A, B (A1, A2, A3, B1, B3, B4).
- **Công việc cụ thể:**
  + Xây dựng `TcpListenerServer`, `ClientHandler`, cơ chế cấp Task đa luồng xử lý đồng thời.
  + Thiết kế giao thức `RequestPacket`, `ResponsePacket`, xử lý phân tách gói tin theo ký tự xuống dòng `\n`.
  + Xây dựng `SessionStore`, cơ chế khóa dò mật khẩu (Lockout 5 lần).
  + Cài đặt ma trận phân quyền động (`quyen_vai_tro`, `MaTranPhanQuyen`), sổ cái gieo quyền tự động `quyen_mac_dinh_da_ap_dung`.
  + Xây dựng cơ chế Console ServerLog có cấu trúc.
  + Khung giao diện WinForms WebView2 host (`Form1.cs`), cầu nối JavaScript bridge (`window.bridge.call`).

#### Thành viên 2: Quản lý Phòng trọ, Người thuê & Hợp đồng thuê phòng
- **Phụ trách:** Nhóm chức năng C, D, E (C1, C2, C3, D1, D2, D3, D4, E1, E4, E5).
- **Công việc cụ thể:**
  + Xây dựng CSDL và Repository cho các bảng `phong`, `khach_thue`, `hop_dong`.
  + Thực thi các ràng buộc nghiệp vụ: số phòng duy nhất (BR-01), sức chứa tối đa (BR-02), CCCD duy nhất (BR-03), chỉ xóa phòng trống (BR-12).
  + Xây dựng nghiệp vụ lập hợp đồng, gia hạn và chấm dứt hợp đồng tuân thủ Luật Nhà ở 2023 (bắt buộc lý do, ngày bàn giao ≥ 30 ngày).
  + Xây dựng tính năng cảnh báo thẻ cam `Treo HĐ` trên giao diện danh sách phòng.
  + Thiết kế giao diện tab Phòng, tab Người thuê, tab Hợp đồng cho shell Chủ trọ/Quản lý.

#### Thành viên 3: Quản lý Điện nước, Lập hóa đơn & Theo dõi công nợ
- **Phụ trách:** Nhóm chức năng F, G (F1, F2, G1, G2, G3, G4).
- **Công việc cụ thể:**
  + Xây dựng CSDL và Repository cho bảng `chi_so_dien_nuoc` và `hoa_don`.
  + Thực thi các quy tắc: tự điền số cũ (BR-07), mỗi phòng 1 hóa đơn/tháng (BR-08), điều kiện có HĐ hiệu lực và chốt cước (BR-09), tính tổng tiền (BR-10).
  + Khóa bất biến hóa đơn đã thanh toán ở tầng CSDL (BR-11).
  + Xây dựng thuật toán tính tiền điện, tiền nước (hỗ trợ tính theo khối hoặc khoán theo số người).
  + Xây dựng giao diện tab Điện nước, tab Hóa đơn, bộ lọc phòng nợ cước.

#### Thành viên 4: Quy trình Tự đăng ký & Nhận phòng số hóa QR / PIN
- **Phụ trách:** Nhóm chức năng B2, E2, E3, G5.
- **Công việc cụ thể:**
  + Xây dựng route công khai `DANG_KY` cho phép khách thuê tự đăng ký tài khoản không cần token.
  + Tích hợp thuật toán CSPRNG sinh mã token QR 128-bit và mã PIN 8 số trong `HopDongService`.
  + Tích hợp thư viện `qrcode.min.js` hiển thị và in phiếu QR/PIN bàn giao phòng.
  + Xây dựng giao diện nhận phòng phía khách thuê: tải ảnh QR, quét webcam trực tiếp qua HTML5 Canvas và `jsQR.min.js`, nhập mã PIN 8 số.
  + Xây dựng transaction nhận phòng `KHACH_THUE_NHAN_PHONG_QR` an toàn (khóa HĐ, gán phòng, tự hủy token/PIN chống replay).
  + Xây dựng màn hình tra cứu cước cá nhân của khách thuê (suy phòng từ phiên, phân trang server-side, nút copy chuyển khoản).

#### Thành viên 5: Thống kê KPI, Nghiệp vụ Công an & Đảm bảo chất lượng (QA/Tester)
- **Phụ trách:** Nhóm chức năng H (H1, H2, H3), công tác kiểm thử và tài liệu.
- **Công việc cụ thể:**
  + Xây dựng module thống kê công suất lấp đầy, doanh thu thu cước và nợ đọng theo tháng.
  + Xây dựng shell giao diện chuyên biệt cho Công an phường (`congan/index.html`): tra cứu công dân, tạm trú, xuất lịch sử biến động cư trú.
  + Đảm bảo cơ chế chặn quyền ghi đối với vai Công an phường (BR-16).
  + Xây dựng bộ kiểm thử tự động (Unit Test, Integration Test trên MSTest) bao phủ toàn bộ 242 test cases.
  + Duy trì công cụ kiểm tra tính đồng bộ giao diện `tools/xcheck.js`.
