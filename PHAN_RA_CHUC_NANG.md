# XÁC ĐỊNH CHỨC NĂNG, NHÓM, ĐỐI TƯỢNG & PHÂN CÔNG 5 NGƯỜI

Tài liệu gốc: [GIOI_THIEU_DE_TAI.md](GIOI_THIEU_DE_TAI.md) (FR-01→FR-30, BR-01→BR-18).
Tài liệu giao diện người dùng: [XAC_DINH_CHUC_NANG_MENU_GIAO_DIEN.md](XAC_DINH_CHUC_NANG_MENU_GIAO_DIEN.md).

---

## 1. Cây chức năng nguyên tử (phân rã đến mức không thể chia nhỏ thêm)

Tiêu chí dừng: mỗi nút lá (mức 4: `.x.y`) là **1 thao tác nguyên tử** (1 trường nhập/kiểm tra, 1 phép tính đơn, 1 truy vấn hoặc 1 lệnh ghi dữ liệu).

```
HệThốngTrọ_NgũHànhSơn/
├── A. HạTầng_Mạng_CSDL [FR-21, FR-22, NFR-01..05, 08, 09]
│   ├── A1. KếtNối_Client_Server [FR-21]
│   │   ├── A1.1 Đọc cấu hình kết nối từ file
│   │   │   ├── A1.1.1 Đọc địa chỉ IP máy chủ từ App.config
│   │   │   ├── A1.1.2 Đọc cổng TCP từ App.config (mặc định 8888)
│   │   │   └── A1.1.3 Khởi tạo TcpClient theo cấu hình đã đọc
│   │   ├── A1.2 Trạng thái kết nối
│   │   │   ├── A1.2.1 Bật cờ/đèn xanh khi kết nối TCP thành công
│   │   │   └── A1.2.2 Cập nhật nhãn trạng thái trên thanh đáy ứng dụng
│   │   └── A1.3 Xử lý mất kết nối [NFR-08]
│   │       ├── A1.3.1 Bắt ngoại lệ SocketException khi mất luồng TCP
│   │       ├── A1.3.2 Hiển thị hộp thoại cảnh báo mất kết nối máy chủ
│   │       └── A1.3.3 Giữ nguyên màn hình hiện tại, không làm sập ứng dụng
│   ├── A2. GiaoThức_TCP_JSON [NFR-03]
│   │   ├── A2.1 Đóng gói yêu cầu (Client)
│   │   │   ├── A2.1.1 Gán tên hành động vào trường Action
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
│       │   ├── A3.1.1 TcpListener mở cổng và chờ kết nối tới
│       │   ├── A3.1.2 Chấp nhận kết nối và tạo TcpClient mới
│       │   └── A3.1.3 Khởi tạo một luồng xử lý độc lập cho Client đó
│       ├── A3.2 Quản lý vòng đời kết nối
│       │   ├── A3.2.1 Duy trì luồng đọc lặp cho đến khi Client ngắt
│       │   └── A3.2.2 Dọn dẹp tài nguyên và đóng luồng khi Client thoát
│       └── A3.3 Đảm bảo nhất quán dữ liệu phía CSDL
│           ├── A3.3.1 Áp dụng ràng buộc UNIQUE cho số phòng và CCCD
│           ├── A3.3.2 Mở giao dịch (Transaction) cho thao tác ghi nhiều bảng
│           └── A3.3.3 Khóa dòng ghi của InnoDB tránh hai Client ghi đè
├── B. XácThực_PhânQuyền [FR-23, FR-26, FR-29, NFR-06, NFR-07, NFR-10]
│   ├── B1. ĐăngNhập [FR-23]
│   │   ├── B1.1 Tiếp nhận thông tin đăng nhập
│   │   │   ├── B1.1.1 Nhập tên tài khoản (hoặc CCCD đối với khách thuê)
│   │   │   ├── B1.1.2 Nhập mật khẩu
│   │   │   └── B1.1.3 Gửi yêu cầu đăng nhập sang máy chủ
│   │   ├── B1.2 Xác thực tài khoản máy chủ
│   │   │   ├── B1.2.1 Truy vấn tài khoản nội bộ trong bảng `users`
│   │   │   ├── B1.2.2 Nếu không có, truy vấn tài khoản khách trong bảng `tenants`
│   │   │   ├── B1.2.3 Băm mật khẩu nhập vào và so khớp với `password_hash`
│   │   │   ├── B1.2.4 Đăng nhập sai: tăng bộ đếm số lần sai trong bộ nhớ
│   │   │   └── B1.2.5 Đăng nhập đúng: xóa bộ đếm số lần sai
│   │   ├── B1.3 Chặn dò mật khẩu (Lockout) [NFR-07]
│   │   │   ├── B1.3.1 Kiểm tra nếu số lần sai liên tiếp đạt 5 lần
│   │   │   ├── B1.3.2 Ghi mốc thời gian tạm khóa 1 phút
│   │   │   └── B1.3.3 Từ chối đăng nhập và báo số giây còn lại nếu trong thời gian khóa
│   │   └── B1.4 Khởi tạo phiên làm việc
│   │       ├── B1.4.1 Sinh chuỗi token ngẫu nhiên không trùng lặp
│   │       ├── B1.4.2 Lưu token kèm mã người dùng, vai trò và phạm vi khu vào SessionStore
│   │       └── B1.4.3 Trả về Client: Token, Họ tên, Vai trò
│   ├── B2. Chặn_PhạmVi_Khu [FR-26 / BR-15]
│   │   ├── B2.1 Xác định phạm vi khu của yêu cầu
│   │   │   ├── B2.1.1 Trích xuất token từ gói tin yêu cầu
│   │   │   ├── B2.1.2 Tra cứu danh sách mã khu được phép thao tác từ phiên
│   │   │   └── B2.1.3 Đọc mã khu của đối tượng dữ liệu được gửi kèm
│   │   └── B2.2 Thực thi giới hạn phạm vi
│   │       ├── B2.2.1 Nếu vai trò là Chủ trọ: cho phép toàn bộ khu
│   │       ├── B2.2.2 Nếu vai trò là Quản lý: so khớp mã khu với danh sách được phân công
│   │       └── B2.2.3 Không khớp khu: trả về lỗi từ chối quyền truy cập
│   └── B3. Chặn_Vai_ChỉXem [FR-29 / BR-16]
│       ├── B3.1 Nhận diện thao tác ghi (Thêm / Sửa / Xóa)
│       ├── B3.2 Kiểm tra vai trò của phiên có phải Công an phường hay không
│       └── B3.3 Nếu là Công an phường gửi thao tác ghi: máy chủ từ chối và báo lỗi rõ ràng
├── C. KhuTrọ_PhânCông [FR-24, FR-25, FR-27]
│   ├── C1. QuảnLý_KhuTrọ [FR-24 / BR-17]
│   │   ├── C1.1 Thêm khu trọ
│   │   │   ├── C1.1.1 Nhập tên khu trọ
│   │   │   ├── C1.1.2 Nhập địa chỉ khu trọ trong phường
│   │   │   ├── C1.1.3 Nhập ghi chú khu trọ
│   │   │   ├── C1.1.4 Kiểm tra trùng tên khu trọ trên toàn hệ thống
│   │   │   └── C1.1.5 Lưu bản ghi khu trọ mới vào bảng `areas`
│   │   ├── C1.2 Sửa thông tin khu trọ
│   │   │   ├── C1.2.1 Chọn khu trọ cần sửa
│   │   │   ├── C1.2.2 Cập nhật tên mới, địa chỉ mới, ghi chú mới
│   │   │   └── C1.2.3 Lưu cập nhật vào cơ sở dữ liệu
│   │   ├── C1.3 Xóa khu trọ [BR-17]
│   │   │   ├── C1.3.1 Chọn khu trọ cần xóa
│   │   │   ├── C1.3.2 Đếm số phòng trực thuộc khu trọ đó
│   │   │   ├── C1.3.3 Nếu số phòng > 0: báo lỗi từ chối xóa
│   │   │   └── C1.3.4 Nếu số phòng = 0: thực hiện xóa khu trọ
│   │   └── C1.4 Gán phòng vào khu trọ
│   │       ├── C1.4.1 Chọn danh sách phòng
│   │       └── C1.4.2 Cập nhật khóa ngoại `area_id` cho các phòng được chọn
│   ├── C2. PhânCông_QuảnLý [FR-25]
│   │   ├── C2.1 Tạo tài khoản Quản lý trọ
│   │   │   ├── C2.1.1 Nhập tên đăng nhập, họ tên, mật khẩu ban đầu
│   │   │   ├── C2.1.2 Băm mật khẩu và lưu vào bảng `users` với vai trò Manager
│   │   │   └── C2.1.3 Báo tạo tài khoản thành công
│   │   ├── C2.2 Gán khu phụ trách
│   │   │   ├── C2.2.1 Chọn tài khoản Quản lý trọ
│   │   │   ├── C2.2.2 Tích chọn một hoặc nhiều khu trọ phân công
│   │   │   └── C2.2.3 Lưu các cặp (user_id, area_id) vào bảng `assignments`
│   │   ├── C2.3 Thu hồi phân công
│   │   │   ├── C2.3.1 Chọn bản ghi phân công cần hủy
│   │   │   └── C2.3.2 Xóa bản ghi tương ứng khỏi bảng `assignments`
│   │   └── C2.4 Tra cứu danh sách phân công
│   │       └── C2.4.1 Đọc toàn bộ bảng phân công kèm tên người quản lý và tên khu
│   └── C3. TổngQuan_NhiềuKhu [FR-27, FR-04]
│       ├── C3.1 Hợp nhất số liệu toàn bộ khu
│       │   ├── C3.1.1 Tính tổng số phòng toàn bộ các khu
│       │   ├── C3.1.2 Tính tổng số phòng trống và đang thuê
│       │   ├── C3.1.3 Tính tổng số người đang lưu trú toàn hệ thống
│       │   └── C3.1.4 Tính tổng tiền đã thu và còn nợ của tháng hiện tại
│       ├── C3.2 Lọc số liệu riêng từng khu
│       │   ├── C3.2.1 Chọn khu trọ từ danh sách thả xuống
│       │   └── C3.2.2 Tính và hiển thị lại các chỉ số trên theo riêng khu đã chọn
│       └── C3.3 So sánh giữa các khu trọ
│           ├── C3.3.1 Tính tỉ lệ lấp đầy của từng khu
│           ├── C3.3.2 Tính doanh thu từng khu
│           └── C3.3.3 Hiển thị bảng so sánh đối chiếu giữa các khu
├── D. PhòngTrọ [FR-01, FR-02, FR-03]
│   ├── D1. ThêmPhòng [FR-01 / BR-01, BR-14]
│   │   ├── D1.1 Nhập thông tin phòng
│   │   │   ├── D1.1.1 Nhập số phòng
│   │   │   ├── D1.1.2 Nhập giá thuê tháng
│   │   │   ├── D1.1.3 Nhập sức chứa tối đa (người)
│   │   │   ├── D1.1.4 Chọn khu trọ trực thuộc
│   │   │   └── D1.1.5 Nhập mô tả/ghi chú phòng
│   │   ├── D1.2 Kiểm tra hợp lệ dữ liệu
│   │   │   ├── D1.2.1 Kiểm tra số phòng không được để trống, tối đa 20 ký tự
│   │   │   ├── D1.2.2 Kiểm tra giá thuê phải là số > 0
│   │   │   ├── D1.2.3 Kiểm tra sức chứa là số nguyên từ 1 đến 50
│   │   │   └── D1.2.4 Kiểm tra số phòng không được trùng trong cùng khu trọ
│   │   └── D1.3 Lưu phòng mới
│   │       ├── D1.3.1 Đặt trạng thái ban đầu mặc định là Trống
│   │       └── D1.3.2 Lưu bản ghi vào bảng `rooms`
│   ├── D2. Sửa_Xóa_Phòng [FR-02 / BR-12]
│   │   ├── D2.1 Sửa thông tin phòng
│   │   │   ├── D2.1.1 Chọn phòng cần sửa từ danh sách
│   │   │   ├── D2.1.2 Cập nhật giá thuê mới
│   │   │   ├── D2.1.3 Cập nhật sức chứa mới (không nhỏ hơn số người đang ở thực tế)
│   │   │   ├── D2.1.4 Cập nhật trạng thái phòng (Trống / Đang thuê / Bảo trì)
│   │   │   └── D2.1.5 Lưu bản ghi cập nhật
│   │   └── D2.2 Xóa phòng [BR-12]
│   │       ├── D2.2.1 Chọn phòng cần xóa
│   │       ├── D2.2.2 Hiển thị hộp thoại xác nhận xóa
│   │       ├── D2.2.3 Kiểm tra phòng có người thuê đang ở hay không
│   │       ├── D2.2.4 Kiểm tra phòng có hợp đồng còn hiệu lực hay không
│   │       ├── D2.2.5 Nếu còn người hoặc còn hợp đồng: từ chối xóa và báo lỗi
│   │       └── D2.2.6 Nếu phòng trống hoàn toàn: xóa bản ghi phòng khỏi bảng `rooms`
│   └── D3. Xem_DanhSách_Phòng [FR-03]
│       ├── D3.1 Lấy dữ liệu danh sách phòng
│       │   ├── D3.1.1 Truy vấn danh sách phòng theo khu được phép
│       │   ├── D3.1.2 Đếm số người đang ở thực tế từ bảng `tenants` cho từng phòng
│       │   └── D3.1.3 Ghép trạng thái, giá thuê, sức chứa thành dòng hiển thị
│       ├── D3.2 Lọc danh sách phòng
│       │   ├── D3.2.1 Lọc phòng theo trạng thái: Tất cả / Trống / Đang thuê / Bảo trì
│       │   └── D3.2.2 Lọc phòng theo khu trọ
│       └── D3.3 Tìm kiếm nhanh phòng
│           └── D3.3.1 Gõ số phòng để lọc ngay trên bảng hiển thị
├── E. NgườiThuê_TạmTrú [FR-05, FR-06, FR-07, FR-08, FR-28]
│   ├── E1. Thêm_GánPhòng [FR-05 / BR-02, BR-03]
│   │   ├── E1.1 Nhập hồ sơ cá nhân
│   │   │   ├── E1.1.1 Nhập họ và tên
│   │   │   ├── E1.1.2 Chọn ngày tháng năm sinh
│   │   │   ├── E1.1.3 Nhập số CCCD (bắt buộc)
│   │   │   ├── E1.1.4 Nhập số điện thoại liên hệ
│   │   │   ├── E1.1.5 Nhập quê quán (tỉnh/thành, quận/huyện)
│   │   │   └── E1.1.6 Nhập nơi học tập hoặc nơi làm việc
│   │   ├── E1.2 Kiểm tra hợp lệ hồ sơ
│   │   │   ├── E1.2.1 Kiểm tra định dạng và tính duy nhất của CCCD trên toàn hệ thống
│   │   │   └── E1.2.2 Kiểm tra họ tên và số điện thoại hợp lệ
│   │   ├── E1.3 Gán vào phòng ở
│   │   │   ├── E1.3.1 Chọn phòng còn chỗ
│   │   │   ├── E1.3.2 Đếm số người hiện tại của phòng, so sánh với sức chứa tối đa
│   │   │   └── E1.3.3 Nếu đủ chỗ: gán `room_id` cho người thuê mới
│   │   ├── E1.4 Khởi tạo mật khẩu tra cứu cho khách
│   │   │   ├── E1.4.1 Nhận mật khẩu do chủ trọ đặt (nếu có)
│   │   │   ├── E1.4.2 Nếu để trống: lấy 6 số cuối của CCCD làm mật khẩu mặc định
│   │   │   └── E1.4.3 Băm mật khẩu và lưu vào trường `password_hash`
│   │   └── E1.5 Cập nhật trạng thái phòng
│   │       └── E1.5.1 Tự động chuyển trạng thái phòng sang Đang thuê
│   ├── E2. Sửa_Xóa_HồSơ [FR-06]
│   │   ├── E2.1 Sửa thông tin cá nhân người thuê
│   │   │   ├── E2.1.1 Chọn người thuê cần sửa
│   │   │   ├── E2.1.2 Cho phép sửa họ tên, ngày sinh, SĐT, quê quán, nơi làm/học
│   │   │   ├── E2.1.3 Cho phép đặt lại mật khẩu tra cứu mới
│   │   │   └── E2.1.4 Lưu bản ghi thông tin đã cập nhật
│   │   └── E2.2 Xóa hồ sơ người thuê
│   │       ├── E2.2.1 Chọn người thuê cần xóa
│   │       ├── E2.2.2 Kiểm tra người thuê đã trả phòng chưa (`room_id IS NULL`)
│   │       ├── E2.2.3 Kiểm tra người thuê có đang đứng tên hợp đồng còn hiệu lực không
│   │       ├── E2.2.4 Nếu chưa trả phòng hoặc đang đứng tên HĐ: từ chối xóa
│   │       └── E2.2.5 Nếu đã trả phòng hoàn toàn: xóa bản ghi khỏi bảng `tenants`
│   ├── E3. ChuyểnPhòng_TrảPhòng [FR-07]
│   │   ├── E3.1 Chuyển phòng cho người thuê
│   │   │   ├── E3.1.1 Chọn người thuê cần chuyển
│   │   │   ├── E3.1.2 Chọn phòng đích muốn chuyển đến
│   │   │   ├── E3.1.3 Kiểm tra phòng đích còn chỗ trống hay không
│   │   │   ├── E3.1.4 Cập nhật `room_id` mới cho người thuê
│   │   │   ├── E3.1.5 Cập nhật trạng thái phòng đích thành Đang thuê
│   │   │   └── E3.1.6 Kiểm tra phòng cũ: nếu hết người thì cập nhật về Trống
│   │   └── E3.2 Ghi nhận khách trả phòng
│   │       ├── E3.2.1 Chọn người thuê trả phòng
│   │       ├── E3.2.2 Gỡ liên kết phòng (gán `room_id = NULL`)
│   │       ├── E3.2.3 Đếm lại số người còn lại của phòng cũ
│   │       └── E3.2.4 Nếu phòng cũ không còn người nào: chuyển trạng thái phòng về Trống
│   └── E4. QuảnLý_TạmTrú [FR-08, FR-28]
│       ├── E4.1 Quản lý trạng thái tạm trú
│       │   ├── E4.1.1 Tích chọn cờ Đã đăng ký tạm trú cho từng người
│       │   └── E4.1.2 Lưu cờ `is_temporary_registered` vào CSDL
│       ├── E4.2 Xuất danh sách khai báo tạm trú [FR-08]
│       │   ├── E4.2.1 Lọc danh sách người đang ở thực tế (`room_id IS NOT NULL`)
│       │   ├── E4.2.2 Kết xuất các cột: Họ tên, Ngày sinh, CCCD, Quê quán, Số phòng, Khu
│       │   └── E4.2.3 Xuất dữ liệu ra file định dạng CSV/Excel
│       └── E4.3 Tra cứu lưu trú phục vụ Công an phường [FR-28]
│           ├── E4.3.1 Mở màn hình tra cứu danh sách lưu trú toàn phường
│           ├── E4.3.2 Tìm kiếm người thuê theo họ tên
│           ├── E4.3.3 Tìm kiếm người thuê theo số CCCD (cho phép tìm một phần)
│           ├── E4.3.4 Tìm kiếm theo số phòng hoặc theo từng khu trọ
│           └── E4.3.5 Xuất báo cáo danh sách kiểm tra lưu trú
├── F. HợpĐồngThuê [FR-09, FR-10, FR-11]
│   ├── F1. LậpHợpĐồng [FR-09 / BR-04, BR-05, BR-06, BR-13]
│   │   ├── F1.1 Nhập thông tin hợp đồng
│   │   │   ├── F1.1.1 Chọn phòng cần lập hợp đồng
│   │   │   ├── F1.1.2 Chọn người đại diện ký hợp đồng từ danh sách thành viên của phòng
│   │   │   ├── F1.1.3 Chọn ngày bắt đầu hợp đồng
│   │   │   ├── F1.1.4 Chọn ngày kết thúc hợp đồng
│   │   │   ├── F1.1.5 Nhập giá thuê theo thỏa thuận hợp đồng
│   │   │   └── F1.1.6 Nhập số tiền đặt cọc
│   │   ├── F1.2 Kiểm tra quy tắc nghiệp vụ
│   │   │   ├── F1.2.1 Kiểm tra ngày kết thúc phải sau ngày bắt đầu [BR-06]
│   │   │   ├── F1.2.2 Kiểm tra người đại diện bắt buộc phải thuộc phòng đó [BR-05]
│   │   │   ├── F1.2.3 Kiểm tra phòng không có hợp đồng nào khác đang hiệu lực [BR-04]
│   │   │   └── F1.2.4 Kiểm tra giá thuê và tiền cọc phải >= 0
│   │   └── F1.3 Lưu hợp đồng (giao dịch an toàn) [BR-13]
│   │       ├── F1.3.1 Mở Transaction cơ sở dữ liệu
│   │       ├── F1.3.2 Ghi bản ghi hợp đồng mới với trạng thái Active
│   │       ├── F1.3.3 Cập nhật trạng thái phòng sang Đang thuê
│   │       └── F1.3.4 Commit giao dịch
│   ├── F2. GiaHạn_ChấmDứt [FR-10]
│   │   ├── F2.1 Gia hạn hợp đồng
│   │   │   ├── F2.1.1 Chọn hợp đồng đang có hiệu lực (Active)
│   │   │   ├── F2.1.2 Nhập ngày kết thúc mới
│   │   │   ├── F2.1.3 Kiểm tra ngày kết thúc mới phải sau ngày kết thúc cũ
│   │   │   └── F2.1.4 Cập nhật trường `end_date` của hợp đồng
│   │   └── F2.2 Chấm dứt hợp đồng
│   │       ├── F2.2.1 Chọn hợp đồng cần thanh lý
│   │       ├── F2.2.2 Nhập lý do chấm dứt hợp đồng
│   │       ├── F2.2.3 Nhập ghi chú hoàn trả/cấn trừ tiền cọc
│   │       ├── F2.2.4 Cập nhật trạng thái hợp đồng thành Terminated
│   │       └── F2.2.5 Lưu lý do và ghi chú cọc vào bản ghi hợp đồng
│   └── F3. CảnhBáo_HếtHạn [FR-11]
│       ├── F3.1 Tính thời hạn còn lại của hợp đồng
│       │   ├── F3.1.1 Lấy ngày kết thúc của từng hợp đồng Active so với ngày hiện tại
│       │   └── F3.1.2 Tính ra số ngày còn lại
│       ├── F3.2 Hiển thị danh sách sắp hết hạn
│       │   ├── F3.2.1 Sắp xếp hợp đồng theo số ngày còn lại tăng dần
│       │   └── F3.2.2 Lọc các hợp đồng có số ngày còn lại <= 30 ngày
│       └── F3.3 Báo hiệu trực quan
│           └── F3.3.1 Tô màu đỏ cảnh báo dòng hợp đồng sắp hết hạn trên bảng
├── G. ĐiệnNước [FR-12, FR-13]
│   ├── G1. ChốtChỉSố [FR-12 / BR-07, BR-08]
│   │   ├── G1.1 Chọn kỳ ghi nhận
│   │   │   ├── G1.1.1 Chọn khu trọ
│   │   │   ├── G1.1.2 Chọn phòng trọ
│   │   │   └── G1.1.3 Chọn tháng chốt theo định dạng yyyy-MM
│   │   ├── G1.2 Lấy chỉ số đầu kỳ tự động
│   │   │   ├── G1.2.1 Truy vấn bản ghi chốt số của kỳ tháng liền trước
│   │   │   ├── G1.2.2 Tự điền chỉ số điện cũ = chỉ số điện mới kỳ trước
│   │   │   └── G1.2.3 Tự điền chỉ số nước cũ = chỉ số nước mới kỳ trước
│   │   ├── G1.3 Nhập chỉ số cuối kỳ và đơn giá
│   │   │   ├── G1.3.1 Nhập chỉ số điện mới
│   │   │   ├── G1.3.2 Nhập chỉ số nước mới
│   │   │   ├── G1.3.3 Nhập đơn giá điện áp dụng cho kỳ này (đồng/kWh)
│   │   │   └── G1.3.4 Nhập đơn giá nước áp dụng cho kỳ này (đồng/m³)
│   │   ├── G1.4 Kiểm tra tính hợp lệ [BR-07, BR-08]
│   │   │   ├── G1.4.1 Kiểm tra chỉ số điện mới không được nhỏ hơn chỉ số điện cũ
│   │   │   ├── G1.4.2 Kiểm tra chỉ số nước mới không được nhỏ hơn chỉ số nước cũ
│   │   │   ├── G1.4.3 Kiểm tra đơn giá điện và đơn giá nước phải > 0
│   │   │   └── G1.4.4 Kiểm tra phòng chưa từng chốt số trong tháng này (tránh ghi đè)
│   │   └── G1.5 Lưu chỉ số chốt
│   │       └── G1.5.1 Ghi bản ghi vào bảng `utility_readings`
│   └── G2. TínhTiền_ĐiệnNước [FR-13]
│       ├── G2.1 Tính lượng tiêu thụ
│       │   ├── G2.1.1 Lượng điện tiêu thụ = Chỉ số điện mới - Chỉ số điện cũ
│       │   └── G2.1.2 Lượng nước tiêu thụ = Chỉ số nước mới - Chỉ số nước cũ
│       ├── G2.2 Tính thành tiền tại máy chủ
│       │   ├── G2.2.1 Tiền điện = Lượng điện tiêu thụ × Đơn giá điện kỳ
│       │   └── G2.2.2 Tiền nước = Lượng nước tiêu thụ × Đơn giá nước kỳ
│       └── G2.3 Hiển thị xem trước
│           ├── G2.3.1 Hiển thị số lượng và thành tiền điện tức thì trên giao diện
│           └── G2.3.2 Hiển thị số lượng và thành tiền nước tức thì trên giao diện
├── H. HóaĐơn_ThanhToán [FR-14, FR-15, FR-16, FR-17, FR-30]
│   ├── H1. LậpHóaĐơn [FR-14 / BR-08, BR-09, BR-10, BR-13]
│   │   ├── H1.1 Kiểm tra điều kiện lập hóa đơn [BR-09]
│   │   │   ├── H1.1.1 Kiểm tra phòng có hợp đồng đang hiệu lực không
│   │   │   ├── H1.1.2 Kiểm tra phòng đã chốt điện nước của tháng cần lập chưa
│   │   │   └── H1.1.3 Kiểm tra phòng đã có hóa đơn trong tháng này chưa [BR-08]
│   │   ├── H1.2 Tập hợp các khoản tiền [BR-10]
│   │   │   ├── H1.2.1 Lấy tiền phòng từ giá thuê trên hợp đồng đang hiệu lực
│   │   │   ├── H1.2.2 Lấy thành tiền điện từ bản ghi chốt điện nước của tháng
│   │   │   ├── H1.2.3 Lấy thành tiền nước từ bản ghi chốt điện nước của tháng
│   │   │   ├── H1.2.4 Nhập các khoản phí dịch vụ khác (rác, wifi, vệ sinh) nếu có
│   │   │   └── H1.2.5 Tính tổng tiền = Tiền phòng + Tiền điện + Tiền nước + Phí khác
│   │   └── H1.3 Lưu hóa đơn mới
│   │       ├── H1.3.1 Đặt trạng thái ban đầu là Chưa thanh toán (Unpaid)
│   │       ├── H1.3.2 Đặt thời gian thu `paid_at = NULL`
│   │       └── H1.3.3 Lưu bản ghi vào bảng `invoices`
│   ├── H2. Xem_LịchSử_HóaĐơn [FR-15]
│   │   ├── H2.1 Lấy danh sách hóa đơn
│   │   │   ├── H2.1.1 Lọc hóa đơn theo tháng (yyyy-MM)
│   │   │   ├── H2.1.2 Lọc hóa đơn theo phòng trọ
│   │   │   └── H2.1.3 Lọc theo trạng thái: Tất cả / Đã thanh toán / Chưa thanh toán
│   │   └── H2.2 Xem chi tiết hóa đơn
│   │       ├── H2.2.1 Xem bảng kê chi tiết từng khoản: phòng, điện, nước, phí khác
│   │       └── H2.2.2 Xem chỉ số đầu/cuối và đơn giá điện nước kèm theo
│   ├── H3. XácNhận_ThanhToán [FR-16 / BR-11]
│   │   ├── H3.1 Thực hiện thu tiền
│   │   │   ├── H3.1.1 Chọn hóa đơn cần xác nhận thanh toán
│   │   │   ├── H3.1.2 Kiểm tra hóa đơn phải đang ở trạng thái Unpaid
│   │   │   ├── H3.1.3 Nếu đã thanh toán rồi: từ chối xử lý
│   │   │   ├── H3.1.4 Cập nhật trạng thái sang Đã thanh toán (Paid)
│   │   │   └── H3.1.5 Lưu mốc thời gian thu tiền `paid_at = NOW()`
│   │   └── H3.2 Bảo vệ tính bất biến [BR-11]
│   │       ├── H3.2.1 Khóa chức năng chỉnh sửa đối với hóa đơn đã Paid
│   │       └── H3.2.2 Khóa chức năng xóa đối với hóa đơn đã Paid
│   ├── H4. QuảnLý_CôngNợ [FR-17]
│   │   ├── H4.1 Lọc danh sách phòng còn nợ
│   │   │   ├── H4.1.1 Truy vấn các hóa đơn có trạng thái Unpaid
│   │   │   └── H4.1.2 Lọc danh sách theo tháng được chọn
│   │   └── H4.2 Tổng hợp thông tin đôn đốc
│   │       ├── H4.2.1 Liệt kê: Số phòng, Tên người đại diện, Số điện thoại, Tháng nợ, Số tiền
│   │       └── H4.2.2 Tính tổng tiền nợ đọng cần thu trong kỳ
│   └── H5. KháchThuê_TraCứu [FR-30 / BR-18]
│       ├── H5.1 Xác định danh tính khách thuê
│       │   ├── H5.1.1 Đọc mã người thuê từ phiên đăng nhập CCCD
│       │   └── H5.1.2 Máy chủ tự suy ra mã phòng khách đang ở (không nhận mã phòng từ máy khách)
│       ├── H5.2 Xem hóa đơn phòng mình
│       │   ├── H5.2.1 Hiển thị hóa đơn tháng hiện tại của phòng
│       │   ├── H5.2.2 Xem chi tiết tiền phòng, chỉ số điện, chỉ số nước, tiền dịch vụ
│       │   └── H5.2.3 Xem trạng thái đã đóng tiền hay chưa
│       └── H5.3 Xem lịch sử thanh toán
│           └── H5.3.1 Xem danh sách các tháng trước đã thanh toán kèm ngày giờ thu
└── I. ThốngKê_BáoCáo_TraCứu [FR-18, FR-19, FR-20]
    ├── I1. ThốngKê_HiệnTrạng [FR-18]
    │   ├── I1.1 Tính toán chỉ số vận hành
    │   │   ├── I1.1.1 Đếm tổng số phòng đang quản lý
    │   │   ├── I1.1.2 Đếm số phòng đang có người thuê
    │   │   ├── I1.1.3 Đếm số phòng còn trống
    │   │   ├── I1.1.4 Tính tỉ lệ lấp đầy = (Số phòng đang thuê / Tổng số phòng) × 100%
    │   │   └── I1.1.5 Đếm tổng số người đang lưu trú thực tế
    │   └── I1.2 Trực quan hóa hiện trạng
    │       ├── I1.2.1 Hiển thị các khối số liệu nổi bật trên bảng điều khiển
    │       └── I1.2.2 Vẽ biểu đồ tròn/cột thể hiện cơ cấu phòng (Trống vs Đang thuê)
    ├── I2. ThốngKê_DoanhThu [FR-19]
    │   ├── I2.1 Chọn khoảng thời gian thống kê
    │   │   ├── I2.1.1 Chọn tháng bắt đầu thống kê
    │   │   └── I2.1.2 Chọn tháng kết thúc thống kê
    │   ├── I2.2 Tổng hợp số liệu tài chính theo tháng
    │   │   ├── I2.2.1 Tính tổng tiền đã thu trong từng tháng (tổng tiền các hóa đơn Paid)
    │   │   └── I2.2.2 Tính tổng tiền còn nợ trong từng tháng (tổng tiền các hóa đơn Unpaid)
    │   └── I2.3 Bảng tổng hợp tài chính
    │       ├── I2.3.1 Hiển thị danh sách các tháng với hai cột: Đã thu và Còn nợ
    │       └── I2.3.2 Tính dòng tổng cộng cho toàn bộ khoảng thời gian đã chọn
    └── I3. TraCứu_Nhanh [FR-20]
        ├── I3.1 Tiếp nhận từ khóa tìm kiếm
        │   ├── I3.1.1 Nhập từ khóa tự do vào ô tìm kiếm nhanh
        │   └── I3.1.2 Gửi chuỗi tìm kiếm về máy chủ
        └── I3.2 Thực thi tìm kiếm đa tiêu chí phía máy chủ
            ├── I3.2.1 Tìm kiếm không phân biệt hoa thường theo Họ tên người thuê
            ├── I3.2.2 Tìm kiếm khớp một phần theo Số căn cước công dân
            ├── I3.2.3 Tìm kiếm khớp một phần theo Số điện thoại liên hệ
            ├── I3.2.4 Tìm kiếm theo Số phòng
            └── I3.2.5 Trả về danh sách kết quả phù hợp kèm phòng và khu tương ứng
```

---

## 2. Xác định nhóm chức năng và đối tượng sử dụng

**Ký hiệu đối tượng:**
- **CT**: Chủ trọ (toàn quyền quản lý).
- **QL**: Quản lý trọ (toàn quyền trong khu được phân công).
- **CA**: Công an phường (chỉ xem thông tin lưu trú, cấm ghi).
- **KH**: Khách thuê (chỉ tra cứu hóa đơn phòng mình).
- **SV**: Máy chủ (tự động xử lý tính toán, giao dịch, phân quyền).

| Nhóm chức năng | Mã FR bao hàm | Đối tượng sử dụng | Cơ chế kiểm soát tại Server |
|---|---|---|---|
| **A. Hạ tầng mạng & CSDL** | FR-21, FR-22, NFR-01→05, 08, 09 | Toàn bộ vai trò + SV | Cấp luồng riêng mỗi kết nối; Transaction InnoDB; gói JSON có đuôi `\n`. |
| **B. Xác thực & Phân quyền** | FR-23, FR-26, FR-29, NFR-06, 07, 10 | Toàn bộ vai trò + SV | Băm mật khẩu PBKDF2; khóa 1 phút sau 5 lần sai; chặn theo AreaId; từ chối ghi với CA. |
| **C. Khu trọ & Phân công** | FR-24, FR-25, FR-27, (FR-04) | CT (ghi/phân công), QL (xem khu mình), CA (xem) | Chỉ Owner được thêm/sửa/xóa khu và phân công; chỉ xóa khu khi không còn phòng (BR-17). |
| **D. Quản lý phòng trọ** | FR-01, FR-02, FR-03 | CT, QL (khu mình), CA (chỉ xem), SV | Số phòng duy nhất trong khu (BR-01, BR-14); chỉ xóa phòng trống (BR-12). |
| **E. Người thuê & Tạm trú** | FR-05, FR-06, FR-07, FR-08, FR-28 | CT, QL (khu mình), CA (xem lưu trú), SV | CCCD duy nhất hệ thống (BR-03); kiểm tra sức chứa phòng (BR-02); chỉ xóa khi đã trả phòng. |
| **F. Quản lý hợp đồng** | FR-09, FR-10, FR-11 | CT, QL (khu mình), CA (chỉ xem), SV | 1 phòng chỉ 1 HĐ hiệu lực (BR-04); người ký phải thuộc phòng (BR-05); ngày KT > ngày BĐ (BR-06). |
| **G. Điện, nước** | FR-12, FR-13 | CT, QL (khu mình), CA (xem chỉ số), SV | Tự điền số cũ; từ chối số mới < số cũ (BR-07); Server tự nhân đơn giá kỳ (BR-10). |
| **H. Hóa đơn & Thu tiền** | FR-14, FR-15, FR-16, FR-17, FR-30 | CT, QL (khu mình), KH (chỉ phòng mình), SV | 1 phòng 1 HĐ 1 hóa đơn/tháng (BR-08); cần chốt điện nước (BR-09); HĐ đã thu bất biến (BR-11, BR-18). |
| **I. Thống kê & Tra cứu** | FR-18, FR-19, FR-20 | CT (toàn khu), QL (khu mình), CA (tra cứu lưu trú) | Máy chủ tính toán tổng hợp; tìm kiếm LIKE không phân biệt hoa thường. |

---

## 3. Phân công công việc chi tiết cho 5 người

### Phân bổ tổng quan

```
[Nhóm 5 thành viên]
 ├── P1 (Trưởng nhóm): Hạ tầng mạng TCP + Khung CSDL + Điều phối kiến trúc
 ├── P2: Xác thực tài khoản + Quản lý nhiều khu trọ + Phân quyền 4 vai
 ├── P3: Quản lý phòng trọ + Quản lý hồ sơ người thuê + Thủ tục tạm trú
 ├── P4: Vận hành hợp đồng + Chốt điện nước + Lập & Thu hóa đơn
 └── P5: Bảng điều khiển tổng quan + Thống kê báo cáo + Kiểm thử tích hợp
```

### Bảng phân công chi tiết từng người

| Thành viên | Trách nhiệm chính | Mã yêu cầu đảm nhiệm | Các file / module phụ trách | Sản phẩm bàn giao cụ thể |
|---|---|---|---|---|
| **P1**<br>*(Trưởng nhóm)* | **Hạ tầng mạng TCP, Kiến trúc Server & CSDL** | FR-21, FR-22<br>NFR-01→05<br>NFR-08, 09 | `TcpListenerServer.cs`<br>`ClientHandler.cs`<br>`RequestRouter.cs`<br>`Database.cs`<br>`SchemaInitializer.cs`<br>`schema.sql` | 1. Máy chủ TCP console chạy đa luồng ổn định.<br>2. Bộ định tuyến gói tin JSON đóng ngắt bằng `\n`.<br>3. CSDL MySQL 8 bảng hoàn chỉnh với ràng buộc toàn vẹn, giao dịch khóa dòng InnoDB. |
| **P2** | **Xác thực, Phân quyền & Quản lý khu trọ** | FR-23, FR-24<br>FR-25, FR-26<br>FR-27, FR-29 | `AuthService.cs`<br>`SessionStore.cs`<br>`PermissionMatrix.cs`<br>`PasswordHasher.cs`<br>`AreaRepository.cs`<br>Giao diện Đăng nhập + Khu trọ | 1. Hệ thống đăng nhập băm mật khẩu, tự khóa 1 phút khi sai 5 lần.<br>2. Kiểm soát phạm vi dữ liệu theo khu cho vai Quản lý trọ.<br>3. Bộ lọc chặn toàn bộ lệnh ghi đối với vai Công an phường.<br>4. Màn hình quản lý danh mục khu trọ và phân công. |
| **P3** | **Quản lý phòng trọ, Người thuê & Tạm trú** | FR-01→FR-08<br>FR-28<br>BR-01, 02, 03, 12 | `RoomService.cs`<br>`TenantService.cs`<br>`RoomRepository.cs`<br>`TenantRepository.cs`<br>`RoomsForm.cs`<br>`TenantsForm.cs` | 1. Quản lý phòng trọ: thêm, sửa, xóa an toàn (chỉ xóa phòng trống), đếm người ở thực tế.<br>2. Quản lý hồ sơ người thuê, kiểm tra duy nhất CCCD, kiểm tra sức chứa khi gán/chuyển.<br>3. Màn hình tra cứu lưu trú cho Công an phường và xuất file CSV tạm trú. |
| **P4** | **Chuỗi tiền: Hợp đồng, Điện nước & Hóa đơn** | FR-09→FR-17<br>FR-30<br>BR-04→11, 13, 18 | `ContractService.cs`<br>`UtilityService.cs`<br>`InvoiceService.cs`<br>`ContractsForm.cs`<br>`UtilitiesForm.cs`<br>`InvoicesForm.cs`<br>`MyInvoicesForm.cs` | 1. Lập hợp đồng ràng buộc đại diện phòng, cảnh báo hợp đồng sắp hết hạn trong 30 ngày.<br>2. Chốt chỉ số điện nước tự điền số cũ, máy chủ tự tính tiền theo đơn giá kỳ.<br>3. Lập hóa đơn tổng hợp theo tháng, thu tiền lưu thời gian, khóa bất biến hóa đơn đã thu.<br>4. Màn hình riêng cho Khách thuê tự tra cứu hóa đơn phòng mình. |
| **P5** | **Bảng điều khiển, Báo cáo & Kiểm thử** | FR-04, FR-18<br>FR-19, FR-20<br>Tích hợp hệ thống | `ReportService.cs`<br>`ReportRepository.cs`<br>`DashboardForm.cs`<br>`ReportsForm.cs`<br>`Form1.cs`<br>`QuanLyTro.Tests` | 1. Bảng điều khiển trung tâm với các thẻ số liệu KPI và danh sách đôn đốc nợ.<br>2. Màn hình thống kê doanh thu theo khoảng tháng và vẽ biểu đồ lấp đầy.<br>3. Chức năng tra cứu nhanh không phân biệt hoa thường.<br>4. Bộ kịch bản kiểm thử tích hợp (Unit test + Integration test) và tài liệu nghiệm thu. |

### Kế hoạch phối hợp và thứ tự triển khai

1. **Tuần 1 (P1 khởi động nền móng):** P1 hoàn thành mô hình CSDL MySQL và khung máy chủ `TcpListenerServer` + giao thức đóng gói gói tin.
2. **Tuần 2 (P2 dựng khung bảo mật):** P2 phát triển module xác thực `AuthService`, cấp token, bảng ma trận phân quyền `PermissionMatrix` và phân công khu trọ.
3. **Tuần 3 & 4 (P3 & P4 xây dựng nghiệp vụ song song):**
   - P3 hoàn thành các chức năng Phòng và Người thuê.
   - P4 hoàn thành chuỗi nghiệp vụ Hợp đồng -> Điện nước -> Hóa đơn.
4. **Tuần 5 (P5 hoàn thiện giao diện tổng hợp & Kiểm thử):** P5 kết nối dữ liệu lên Dashboard, hoàn thiện chức năng Thống kê/Báo cáo và chạy toàn bộ bộ kiểm thử tự động trên nhiều máy trạm đồng thời.

---

*Ghi chú kỹ thuật đã chốt: Không xây dựng màn hình nhập IP máy chủ trên giao diện (nạp tự động từ file cấu hình `App.config`).*
