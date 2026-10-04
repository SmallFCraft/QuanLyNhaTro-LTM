# GIỚI THIỆU ĐỀ TÀI
## XÂY DỰNG HỆ THỐNG QUẢN LÝ PHÒNG TRỌ PHƯỜNG NGŨ HÀNH SƠN

---

## 1. GIỚI THIỆU

Phường Ngũ Hành Sơn (TP. Đà Nẵng) là địa bàn có mật độ phòng trọ cao, phục vụ đông đảo sinh viên các trường Đại học Kinh tế – Đại học Đà Nẵng, VKU, FPT Đà Nẵng cùng người lao động ngoại tỉnh. Nhu cầu quản lý phòng trọ tại đây không chỉ là ghi chép danh sách người ở, mà là một quy trình vận hành khép kín theo tháng: khách tự đăng ký tài khoản và nhận phòng qua mã QR / PIN bảo mật, lập hợp đồng, chốt chỉ số điện nước, lập hóa đơn, thu tiền, kiểm soát phân quyền động và định kỳ xuất hồ sơ tạm trú phục vụ công tác kiểm tra của công an phường.

Đề tài xây dựng một hệ thống quản lý phòng trọ theo mô hình **Client – Server** trên giao thức **TCP/IP** thuần (.NET 8.0):
- **Máy khách (Client):** Ứng dụng desktop WinForms lưu trữ và nhúng **Microsoft Edge WebView2**, hiển thị giao diện người dùng web hiện đại (HTML5, CSS3, JavaScript không phụ thuộc framework ngoài), hoạt động hoàn toàn offline từ thư mục tài nguyên cục bộ `Assets/wwwroot/`. Mọi tương tác giao diện được chuyển đổi thành lời gọi API JSON qua cầu nối `window.bridge.call(action, payload)` gửi qua socket TCP tới Server.
- **Máy chủ (Server):** Ứng dụng Console .NET 8.0 sử dụng `TcpListener` lắng nghe trên cổng `8888`, tiếp nhận đa kết nối đồng thời (mỗi máy khách một Task/luồng độc lập), thực thi toàn bộ logic nghiệp vụ, ma trận phân quyền động (RBAC), quản lý giao dịch và truy vấn trực tiếp cơ sở dữ liệu **MySQL 8.0**. Client hoàn toàn không truy cập cơ sở dữ liệu trực tiếp — đây chính là yêu cầu trọng tâm của môn Lập Trình Mạng.

Hệ thống hướng đến giải quyết trọn vẹn quy trình nghiệp vụ phòng trọ, thể hiện đầy đủ các nội dung kỹ thuật cốt lõi của môn học: thiết kế giao thức ứng dụng JSON tự định nghĩa trên TCP, xử lý đồng thời đa kết nối, phân quyền vai trò bảo mật, và đảm bảo tính nhất quán dữ liệu ở tầng cơ sở dữ liệu (ACID, row locking `FOR UPDATE`, idempotency).

## 2. ĐẶT VẤN ĐỀ

Hiện nay, phần lớn chủ trọ tại phường Ngũ Hành Sơn vẫn quản lý bằng sổ tay hoặc các file Excel rời rạc. Cách làm này bộc lộ nhiều bất cập trong vận hành thực tế:

- **Sai sót chỉ số điện nước.** Chỉ số cuối kỳ của tháng trước không được chuyển tự động thành chỉ số đầu kỳ của tháng sau, dẫn đến nhập tay chồng chéo và tính tiền sai.
- **Thất lạc hợp đồng & rủi ro pháp lý.** Hợp đồng giấy không có cơ chế cảnh báo hết hạn; việc chấm dứt hợp đồng tùy tiện trước thời hạn vi phạm quy định thông báo trước ít nhất 30 ngày theo Luật Nhà ở 2023.
- **Quy trình bàn giao phòng thủ công.** Chủ trọ phải trực tiếp có mặt giao chìa khóa, nhập liệu thủ công thông tin người thuê dẫn đến chậm trễ và sai lệch dữ liệu cá nhân.
- **Khó tra cứu lịch sử thanh toán.** Thông tin thu tiền nằm rải rác, không đối chiếu được ai còn nợ tháng nào, số tiền bao nhiêu.
- **Mất nhiều thời gian tổng hợp danh sách khai báo tạm trú** cho công an phường theo quy định đăng ký lưu trú.

Về mặt kỹ thuật, đề tài đặt ra bài toán trọng tâm của môn Lập Trình Mạng: **làm thế nào để nhiều máy khách thuộc các vai trò khác nhau (Chủ trọ, Quản lý, Công an, Khách thuê) cùng thao tác trên một cơ sở dữ liệu dùng chung mà dữ liệu vẫn nhất quán**, không xảy ra tranh chấp hay ghi đè lẫn nhau — ví dụ hai máy cùng thêm một số phòng, hoặc hai khách cùng nhận một phòng trống qua QR.

## 3. MỤC TIÊU ĐỀ TÀI

Hệ thống được xây dựng với các mục tiêu cụ thể sau:

1. **Tập trung hóa dữ liệu:** Toàn bộ dữ liệu về phòng trọ, người thuê, hợp đồng, điện nước, hóa đơn và ma trận phân quyền lưu trữ tập trung tại máy chủ MySQL duy nhất, đảm bảo tính toàn vẹn và an toàn.
2. **Đúng chuẩn Lập Trình Mạng:** Máy khách (Client) hoàn toàn không có chuỗi kết nối (connection string) tới MySQL. Mọi thao tác đều gửi qua kết nối socket TCP tới Server; Server xác thực phiên, kiểm tra ma trận quyền, thực thi nghiệp vụ và trả kết quả.
3. **Phục vụ đa máy khách đồng thời:** Server phục vụ nhiều Client kết nối cùng lúc mà dữ liệu vẫn nhất quán thông qua cơ chế Transaction, row-level locking (`SELECT ... FOR UPDATE`) và các ràng buộc duy nhất (`UNIQUE`).
4. **Quy trình nhận phòng số hóa bằng QR & PIN:** Khách thuê tự đăng ký tài khoản từ xa; sau khi chủ trọ lập hợp đồng chờ, khách tự dùng camera webcam/ảnh QR/mã PIN 8 số để nhận phòng và kích hoạt hợp đồng một cách an toàn.
5. **Tuân thủ pháp luật về nhà ở:** Tự động hóa cảnh báo hợp đồng sắp hết hạn trong 30 ngày; ràng buộc lý do pháp lý hợp lệ và thời hạn bàn giao dự kiến (≥ 30 ngày) khi chấm dứt trước hạn theo Luật Nhà ở 2023 (Điều 172).

## 4. ĐỐI TƯỢNG SỬ DỤNG VÀ PHÂN QUYỀN

Hệ thống phục vụ bốn nhóm đối tượng người dùng:

| Vai trò | Mô tả | Phạm vi quyền |
|---|---|---|
| **Chủ trọ** (`ChuTro`) | Người sở hữu và điều hành cơ sở phòng trọ | Toàn quyền trên toàn hệ thống: quản lý phòng, người thuê, hợp đồng, điện nước, hóa đơn, thống kê doanh thu; **quản lý ma trận phân quyền động** (bật/tắt quyền cho các vai trò khác) |
| **Quản lý trọ** (`QuanLy`) | Người được chủ trọ ủy quyền vận hành hàng ngày | Thực hiện các nghiệp vụ: thêm/sửa phòng, người thuê, chốt điện nước, lập hóa đơn, thu tiền, xem mã QR/PIN hợp đồng theo danh mục quyền được Chủ trọ cấp |
| **Công an phường** (`CongAn`) | Cán bộ phụ trách quản lý cư trú, an ninh trật tự địa bàn | **Chỉ xem (Read-only)**: tra cứu danh sách người thuê, thông tin tạm trú, tình trạng từng phòng, xem và xuất lịch sử cư trú phục vụ kiểm tra hành chính; mọi hành vi ghi dữ liệu đều bị Server từ chối |
| **Khách thuê** (`KhachThue`) | Người thuê phòng trọ | Tự đăng ký tài khoản từ xa; nhận phòng qua quét mã QR / nhập PIN; xem thông tin phòng, bảng kê chi tiết cước tháng này, lịch sử hóa đơn và trạng thái đóng tiền của chính mình |

Server kiểm tra vai trò và token phiên đăng nhập ở tầng điều phối yêu cầu (`DieuPhoiYeuCau.cs`) trước khi thực thi bất kỳ thao tác nào (quy tắc BR-14, BR-15, BR-16).

## 5. CÁC CHỨC NĂNG CHÍNH

**1. Quản lý phòng trọ:**
- Thêm, sửa, xóa phòng với thông tin số phòng, giá thuê, sức chứa tối đa và mô tả.
- Hệ thống tự động theo dõi trạng thái mỗi phòng: `Trong` (Trống), `DaThue` (Đang thuê), `BaoTri` (Bảo trì). Số người đang ở thực tế được đếm tự động từ bảng người thuê.
- Chỉ cho phép xóa phòng khi phòng đang trống (không có người ở và không có hợp đồng hiệu lực).
- **Cảnh báo treo hợp đồng:** Tự động gắn thẻ cảnh báo cam `Treo HĐ (N người)` khi phòng đang có người ở nhưng không còn hợp đồng hiệu lực nào (do hợp đồng vừa bị chấm dứt hoặc khách chưa ký HĐ).

**2. Quản lý người thuê & Tự đăng ký tài khoản:**
- Hồ sơ người thuê gồm: họ tên, ngày sinh, CCCD (12 chữ số duy nhất), số điện thoại, quê quán, nơi làm việc/học tập, trạng thái tạm trú.
- Khách thuê có thể **tự đăng ký tài khoản từ xa** qua form đăng ký (`DANG_KY`) mà không cần chủ trọ tạo trước. Tài khoản mới tạo ở trạng thái chờ nhận phòng (`phong_id = NULL`).
- Chủ trọ có thể thêm trực tiếp người thuê vào phòng còn sức chứa, chuyển phòng hoặc xác nhận trả phòng (`phong_id = NULL`).

**3. Quản lý hợp đồng & Nhận phòng tự động bằng QR / PIN:**
- Mỗi phòng có tối đa một hợp đồng có hiệu lực (`HieuLuc` hoặc `ChoNhanPhong`).
- Hỗ trợ 2 hình thức lập hợp đồng:
  + *Bàn giao trực tiếp:* Hợp đồng có hiệu lực ngay (`HieuLuc`), người đại diện bắt buộc đã ở trong phòng.
  + *Chờ nhận phòng bằng QR:* Hợp đồng tạo ở trạng thái `ChoNhanPhong`, hệ thống sinh tự động mã QR bảo mật 128-bit (CSPRNG) và mã PIN 8 số ngẫu nhiên. Chủ trọ có thể xem mã hoặc in phiếu bàn giao trực tiếp từ giao diện.
- Khách thuê đăng nhập tài khoản của mình, quét mã QR qua webcam/tải ảnh hoặc nhập mã PIN 8 số. Server thực thi nhận phòng trong **1 transaction duy nhất**: kiểm tra đúng người đại diện, phòng còn sức chứa, tự gán phòng cho khách, kích hoạt hợp đồng thành `HieuLuc`, cập nhật phòng thành `DaThue` và xóa bỏ mã token/PIN để chống quét lại (replay attack).
- **Chấm dứt hợp đồng tuân thủ Luật Nhà ở 2023:** Khi chấm dứt hợp đồng còn thời hạn, hệ thống bắt buộc nhập lý do chấm dứt (nợ tiền 3 tháng, vi phạm nội quy...), tự động thiết lập ngày bàn giao dự kiến (≥ 30 ngày) và lưu vết ngày thông báo vào ghi chú hợp đồng.

**4. Quản lý chỉ số điện, nước:**
- Hàng tháng, chủ trọ hoặc quản lý nhập chỉ số điện và nước cho từng phòng.
- Hệ thống tự động điền chỉ số đầu kỳ bằng chỉ số cuối kỳ của tháng trước liền kề, ngăn chặn nhập sai lệch.
- Hỗ trợ tính tiền nước linh hoạt: theo khối (`Khoi`) hoặc khoán theo số người (`Nguoi`).
- Server tự động tính lượng tiêu thụ và thành tiền theo đơn giá cấu hình riêng cho từng kỳ.

**5. Lập hóa đơn và thanh toán:**
- Hóa đơn tháng được tạo theo phòng, tự động tổng hợp tiền phòng, tiền điện, tiền nước và phụ phí (rác, mạng...). Mỗi phòng chỉ có duy nhất một hóa đơn cho mỗi kỳ cước (`yyyy-MM`).
- Xác nhận thanh toán chuyển trạng thái từ `ChuaThu` sang `DaThu` kèm mốc thời gian thực; hóa đơn đã thu tiền là bất biến ở tầng CSDL (không được sửa/xóa).
- Khách thuê xem được chi tiết bảng kê cước kỳ này, hỗ trợ nút sao chép nhanh cú pháp chuyển khoản ngân hàng, và xem lịch sử hóa đơn phân trang từ server.

**6. Ma trận phân quyền động (Dynamic RBAC):**
- Chủ trọ quản lý quyền hạn của Quản lý trọ, Công an và Khách thuê thông qua giao diện ma trận quyền thời gian thực (`PHAN_QUYEN_LAY_MA_TRAN`, `PHAN_QUYEN_CAP_NHAT_VAI_TRO`).
- Bảng `quyen_vai_tro` lưu trữ các hành động được phép cho từng vai trò.
- Sổ cái `quyen_mac_dinh_da_ap_dung` tự động gieo bù các quyền mới khi cập nhật phiên bản phần mềm mà không hồi sinh các quyền đã bị Chủ trọ chủ động thu hồi.

**7. Tra cứu và xuất hồ sơ phục vụ công an:**
- Công an phường tra cứu danh sách người thuê, tình trạng lưu trú và lịch sử cư trú.
- Hỗ trợ xuất danh sách tạm trú và lịch sử cư trú ra định dạng file phục vụ công tác kiểm tra hành chính địa bàn.

**8. Kết nối mạng và Giao thức TCP:**
- Client kết nối tới Server qua socket TCP (mặc định `127.0.0.1:8888`), gói tin JSON có ký tự phân cách dòng `\n`.
- Hiển thị trạng thái kết nối trực quan; khi mất kết nối mạng, ứng dụng thông báo rõ ràng mà không bị treo hay mất dữ liệu giao diện.
- Server ghi log console có cấu trúc (`ServerLog.cs`) ghi nhận endpoint, action, thời gian xử lý (ms) và trạng thái OK/FAIL.

## 6. ĐIỂM MỚI VÀ ĐẶC TRƯNG KỸ THUẬT

1. **Giao thức ứng dụng tự thiết kế 29 hành động tiếng Việt không dấu:** Hệ thống định nghĩa giao thức JSON đóng gói qua TCP với 29 hành động (`ActionNames.cs`) bao phủ toàn diện nghiệp vụ (xác thực, phòng, người thuê, hợp đồng, QR check-in, điện nước, hóa đơn, báo cáo, phân quyền).
2. **Kiến trúc WebView2 hiện đại nhúng trong WinForms:** Kết hợp ưu thế hiệu năng native của ứng dụng desktop C# WinForms với khả năng trình diễn linh hoạt, thẩm mỹ của Web chuẩn (HTML5 Canvas giải mã QR, WebRTC `getUserMedia` điều khiển camera thời gian thực, responsive UI), hoàn toàn chạy offline không cần máy chủ web hay Node.js.
3. **Quy trình nhận phòng số hóa khép kín (QR / PIN Code):** Tích hợp thư viện tạo mã QR (`qrcode.min.js`) và giải mã ảnh/webcam (`jsQR.min.js`) phía máy khách, kết hợp mã hóa ngẫu nhiên CSPRNG 128-bit phía Server, tự động hóa toàn bộ quá trình bàn giao phòng không tiếp xúc.
4. **Bảo đảm an toàn giao dịch ACID:** Toàn bộ chuỗi thao tác phức tạp (chấm dứt hợp đồng, trả phòng, nhận phòng qua QR) đều được bọc trong MySQL Transaction với cơ chế khóa dòng `SELECT ... FOR UPDATE`, triệt tiêu hoàn toàn race condition khi nhiều máy khách thao tác cùng lúc.
5. **Cơ chế phân quyền động thích ứng:** Không đóng cứng quyền trong mã nguồn, cho phép Chủ trọ cấu hình linh hoạt danh mục quyền của từng vai trò và lưu bền vững vào CSDL.
6. **Tuân thủ quy định pháp luật thực tế:** Tích hợp các ràng buộc pháp lý của Luật Nhà ở 2023 (Điều 172) vào quy trình vận hành hợp đồng (thời hạn báo trước ≥ 30 ngày, bắt buộc lý do hợp pháp khi hủy trước hạn).

## 7. YÊU CẦU HỆ THỐNG

### 7.1. Yêu cầu chức năng

| Mã | Tên yêu cầu | Mô tả | Vai trò | Ưu tiên |
|---|---|---|---|---|
| FR-01 | Thêm phòng mới | Thêm phòng với số phòng, giá thuê, sức chứa; phòng mới mặc định trạng thái Trống; số phòng là duy nhất toàn hệ thống | Chủ trọ, Quản lý | Bắt buộc |
| FR-02 | Sửa hoặc xóa phòng | Sửa giá thuê, sức chứa, trạng thái; chỉ xóa được phòng trống (không có người thuê, không có hợp đồng hiệu lực) | Chủ trọ, Quản lý | Bắt buộc |
| FR-03 | Xem danh sách và trạng thái phòng | Lưới hiển thị số phòng, giá, sức chứa, số người ở, trạng thái; cảnh báo thẻ "Treo HĐ" nếu có người nhưng thiếu hợp đồng | Tất cả | Bắt buộc |
| FR-04 | Màn hình tổng quan | Hiển thị KPI: tổng phòng, phòng trống, phòng đang thuê, tổng người ở, doanh thu đã thu và còn nợ | Chủ trọ, Quản lý | Bắt buộc |
| FR-05 | Thêm người thuê vào phòng | Hồ sơ gồm họ tên, ngày sinh, CCCD (12 số duy nhất), SĐT, quê quán, nơi làm việc; kiểm tra sức chứa phòng | Chủ trọ, Quản lý | Bắt buộc |
| FR-06 | Sửa hoặc xóa hồ sơ người thuê | Sửa thông tin cá nhân; chỉ xóa được người đã trả phòng (phong_id là NULL) | Chủ trọ, Quản lý | Bắt buộc |
| FR-07 | Chuyển phòng, trả phòng | Chuyển phòng chỉ chấp nhận khi phòng đích còn chỗ; trả phòng đặt phong_id về NULL, phòng hết người tự về Trống | Chủ trọ, Quản lý | Bắt buộc |
| FR-08 | Xuất hồ sơ tạm trú | Xuất danh sách thông tin người thuê đang lưu trú để nộp công an phường | Chủ trọ, Quản lý | Bắt buộc |
| FR-09 | Lập hợp đồng thuê trực tiếp | Chọn phòng và người đại diện trong phòng; nhập ngày bắt đầu, ngày kết thúc (> ngày bắt đầu), giá thuê, tiền cọc | Chủ trọ, Quản lý | Bắt buộc |
| FR-10 | Gia hạn hoặc chấm dứt hợp đồng | Gia hạn tăng ngày kết thúc; chấm dứt trước hạn bắt buộc nhập lý do và ngày bàn giao (≥ 30 ngày) | Chủ trọ, Quản lý | Bắt buộc |
| FR-11 | Theo dõi hợp đồng sắp hết hạn | Danh sách hợp đồng sắp hết hạn trong 30 ngày, tô màu cảnh báo trực quan | Chủ trọ, Quản lý | Bắt buộc |
| FR-12 | Nhập chỉ số điện nước | Chọn phòng và tháng chốt; tự điền chỉ số cũ từ kỳ trước; từ chối nếu số mới < số cũ; đơn giá lưu riêng theo kỳ | Chủ trọ, Quản lý | Bắt buộc |
| FR-13 | Tự động tính tiền điện nước | Tiền điện/nước tính theo số tiêu thụ nhân đơn giá (nước hỗ trợ khoán theo người); tính toán ở Server | Server | Bắt buộc |
| FR-14 | Lập hóa đơn tháng | Tự gộp tiền phòng, điện, nước, phí khác; mỗi phòng chỉ 1 hóa đơn/tháng; phòng chưa chốt điện nước không thể lập HĐ | Chủ trọ, Quản lý | Bắt buộc |
| FR-15 | Xem chi tiết & lịch sử hóa đơn | Danh sách hóa đơn kèm trạng thái Đã thu / Chưa thu; xem chi tiết từng thành phần chi phí | Tất cả | Bắt buộc |
| FR-16 | Xác nhận thanh toán hóa đơn | Đổi trạng thái sang Đã thu kèm thời gian thực; hóa đơn đã thu không được sửa đổi hay xóa | Chủ trọ, Quản lý | Bắt buộc |
| FR-17 | Lọc danh sách phòng còn nợ | Lọc các phòng chưa nộp tiền theo tháng để đôn đốc thu cước | Chủ trọ, Quản lý | Bắt buộc |
| FR-18 | Thống kê hiện trạng & công suất | Tỷ lệ lấp đầy phòng, cơ cấu phòng trống / đang thuê / bảo trì theo thời gian thực | Chủ trọ, Quản lý | Bắt buộc |
| FR-19 | Thống kê doanh thu theo tháng | Bảng tổng hợp doanh thu đã thu và công nợ theo từng tháng | Chủ trọ, Quản lý | Bắt buộc |
| FR-20 | Tra cứu nhanh người thuê & phòng | Tìm kiếm theo tên, số phòng, CCCD, số điện thoại; tìm kiếm không phân biệt hoa thường | Tất cả | Bắt buộc |
| FR-21 | Kết nối TCP Client - Server | Kết nối TCP tới Server theo IP:Port cấu hình; hiển thị trạng thái kết nối; không treo app khi mất mạng | Tất cả | Bắt buộc |
| FR-22 | Xử lý đa máy khách đồng thời | Server cấp Task xử lý độc lập cho từng client; transaction và khóa dòng chống xung đột dữ liệu | Server | Bắt buộc |
| FR-23 | Đăng nhập tài khoản & Lockout | Đăng nhập một form tự nhận diện vai trò; băm mật khẩu; tạm khóa 1 phút khi sai quá 5 lần | Tất cả | Bắt buộc |
| FR-24 | Khách thuê tự đăng ký tài khoản | Người thuê tự tạo tài khoản qua `DANG_KY` (không cần token), mật khẩu ≥ 6 ký tự, hồ sơ lưu phong_id = NULL | Khách thuê | Bắt buộc |
| FR-25 | Lập hợp đồng chờ nhận phòng & QR | Hợp đồng trạng thái `ChoNhanPhong` sinh mã QR CSPRNG 128-bit + PIN 8 số; xem và in phiếu nhận phòng | Chủ trọ, Quản lý | Bắt buộc |
| FR-26 | Nhận phòng qua QR / Webcam / PIN | Khách thuê quét mã QR (webcam/tải ảnh) hoặc nhập PIN 8 số; tự kích hoạt HĐ và gán phòng trong 1 giao dịch | Khách thuê | Bắt buộc |
| FR-27 | Khách thuê tra cứu cước cá nhân | Khách thuê xem cước tháng này, nút copy chuyển khoản nhanh, xem lịch sử hóa đơn phân trang từ server | Khách thuê | Bắt buộc |
| FR-28 | Quản lý ma trận phân quyền động | Chủ trọ xem và cập nhật ma trận quyền của từng vai trò (`quyen_vai_tro`); sổ cái tự động gieo bù quyền mới | Chủ trọ | Bắt buộc |
| FR-29 | Tra cứu cư trú cho công an phường | Màn hình riêng chỉ xem: tra cứu người thuê, tình trạng phòng, xem và xuất lịch sử biến động cư trú | Công an phường | Bắt buộc |
| FR-30 | Chặn thao tác ghi của vai chỉ xem | Server từ chối mọi yêu cầu thêm/sửa/xóa từ vai Công an phường hoặc vai trò không được cấp quyền | Server | Bắt buộc |

### 7.2. Yêu cầu phi chức năng

| Mã | Nhóm | Yêu cầu | Tiêu chí đo lường |
|---|---|---|---|
| NFR-01 | Hiệu năng | Thời gian phản hồi trong mạng LAN | Mỗi yêu cầu xử lý không quá 300 ms; tải trang ban đầu < 1 giây |
| NFR-02 | Hiệu năng | Đa kết nối song song | Mỗi Client một Task độc lập; không có tình trạng nghẽn hàng đợi kết nối |
| NFR-03 | Tin cậy | Phân định gói tin TCP rõ ràng | Mỗi gói JSON kết thúc bằng ký tự `\n`; bóc tách trọn vẹn không phân mảnh |
| NFR-04 | Tin cậy | Toàn vẹn dữ liệu đa bảng (ACID) | Các chuỗi nghiệp vụ (nhận phòng QR, chốt cước, trả phòng) bọc trong 1 Transaction |
| NFR-05 | Nhất quán | Chống ghi đè đồng thời | Ràng buộc UNIQUE CSDL kết hợp `SELECT ... FOR UPDATE` ở tầng Repository |
| NFR-06 | Bảo mật | Bảo vệ mật khẩu | Mật khẩu lưu dạng băm (`PasswordHasher`); không log mật khẩu hay token ra console |
| NFR-07 | Bảo mật | Chống tấn công dò mật khẩu (Brute Force) | Tạm khóa đăng nhập 1 phút sau 5 lần sai liên tiếp |
| NFR-08 | Khả dụng | Ứng dụng client ổn định | Mất kết nối TCP thì hiện thông báo rõ ràng, không sập hay đơ giao diện |
| NFR-09 | Khả dụng | Hoạt động offline-first | Giao diện WebView2 nạp từ file local, không cần mạng Internet ngoài LAN |
| NFR-10 | Bảo trì | Kiểm thử tự động đầy đủ | Bộ test tự động kiểm thử toàn diện (> 230 test cases) đạt 100% pass |

## 8. MA TRẬN CHỨC NĂNG VÀ QUY TẮC NGHIỆP VỤ

### 8.1. Ma trận chức năng theo vai trò

Ký hiệu: **●** Toàn quyền (đọc/ghi) · **○** Thao tác theo phân quyền · **X** Chỉ xem (Read-only) · **–** Không tham gia

| Chức năng | Chủ trọ | Quản lý trọ | Công an phường | Khách thuê | Server | Cơ sở dữ liệu |
|---|---|---|---|---|---|---|
| Quản lý phòng trọ | ● | ○ | X | – | ● Kiểm tra quy tắc xóa/sức chứa | `phong` |
| Quản lý người thuê | ● | ○ | X | – | ● Kiểm tra CCCD duy nhất | `khach_thue` |
| Hợp đồng & Chấm dứt | ● | ○ | X | – | ● Ràng buộc ngày, lý do, transaction | `hop_dong` |
| Sinh mã QR & PIN nhận phòng | ● | ○ | – | – | ● Sinh CSPRNG 128-bit + PIN 8 số | `hop_dong` |
| Nhận phòng QR/Webcam/PIN | – | – | – | ● | ● Xác thực đại diện, gán phòng, tự hủy mã | `hop_dong`, `khach_thue`, `phong` |
| Khách thuê tự đăng ký | – | – | – | ● | ● Public route, kiểm tra CCCD, băm mật khẩu | `khach_thue` |
| Quản lý điện nước | ● | ○ | X | – | ● Tự điền số cũ, tính tiền | `chi_so_dien_nuoc` |
| Lập hóa đơn & Thu tiền | ● | ○ | – | – | ● Kiểm tra 1 HĐ/tháng, khóa bất biến | `hoa_don` |
| Khách thuê tra cứu cước | – | – | – | ● | ● Suy phòng từ phiên, phân trang | `hoa_don` |
| Thống kê & Báo cáo | ● | ○ | X | – | ● Tổng hợp dữ liệu | Nhiều bảng |
| Quản lý phân quyền động | ● | – | – | – | ● Ma trận quyền, sổ cái migration | `quyen_vai_tro`, `quyen_mac_dinh_da_ap_dung` |
| Tra cứu cư trú công an | – | – | ● | – | ● Chặn ghi, truy vấn biến động | `khach_thue`, `hop_dong` |
| Kết nối mạng & Giao thức | ● | ● | ● | ● | ● TCP Listener, định tuyến 29 action | – |

### 8.2. Danh mục quy tắc nghiệp vụ (BR-01..BR-19)

Mọi quy tắc dưới đây **đều được Server kiểm tra và thực thi** trước khi ghi dữ liệu:

| Mã | Quy tắc | Yêu cầu liên quan |
|---|---|---|
| BR-01 | Số phòng là duy nhất trong toàn hệ thống | FR-01 |
| BR-02 | Số thành viên một phòng không được vượt quá sức chứa tối đa của phòng | FR-05, FR-07, FR-26 |
| BR-03 | CCCD của người thuê là duy nhất trong toàn hệ thống (12 chữ số) | FR-05, FR-24 |
| BR-04 | Một phòng chỉ có tối đa một hợp đồng còn hiệu lực (`HieuLuc` hoặc `ChoNhanPhong`) tại một thời điểm | FR-09, FR-25 |
| BR-05 | Người đại diện ký hợp đồng trực tiếp phải là người đang ở trong phòng đó; với hợp đồng chờ nhận phòng, người đại diện chỉ cần tồn tại trong hệ thống | FR-09, FR-25 |
| BR-06 | Ngày kết thúc hợp đồng phải sau ngày bắt đầu; gia hạn phải chọn ngày mới sau ngày kết thúc hiện tại | FR-09, FR-10 |
| BR-07 | Chỉ số mới của điện và nước không được nhỏ hơn chỉ số cũ | FR-12 |
| BR-08 | Mỗi phòng chỉ có một bản ghi chỉ số điện nước và một hóa đơn cho mỗi kỳ cước (`yyyy-MM`) | FR-12, FR-14 |
| BR-09 | Phòng phải có hợp đồng còn hiệu lực và đã chốt điện nước thì mới được lập hóa đơn tháng | FR-14 |
| BR-10 | Tổng tiền hóa đơn bằng tiền phòng cộng tiền điện, tiền nước và phí dịch vụ phát sinh | FR-14 |
| BR-11 | Hóa đơn đã thanh toán (`DaThu`) là bất biến, không được sửa đổi hay xóa ở tầng CSDL | FR-16 |
| BR-12 | Chỉ được phép xóa phòng khi phòng đang trống hoàn toàn (0 người ở và 0 hợp đồng hiệu lực) | FR-02 |
| BR-13 | Các thao tác ghi liên quan nhiều bảng chạy trong một giao dịch (Transaction): thành công toàn bộ hoặc rollback | FR-09, FR-14, FR-26 |
| BR-14 | Tra cứu hóa đơn của khách thuê (`HOA_DON_CUA_TOI`) suy phòng từ `UserId` của phiên, không nhận `phongId` từ client; hỗ trợ phân trang server-side | FR-27 |
| BR-15 | Ma trận phân quyền động kiểm soát quyền của từng vai trò; Chủ trọ bypass cứng; quyền mới tự động gieo bù mà không làm mất cấu hình đã thu hồi | FR-28 |
| BR-16 | Vai Công an phường là tài khoản chỉ xem; Server chặn mọi yêu cầu thêm/sửa/xóa từ vai trò này | FR-29, FR-30 |
| BR-17 | Khách thuê tự đăng ký tài khoản từ xa qua `DANG_KY` là hành động công khai (không cần token phiên), hồ sơ ban đầu có `phong_id = NULL` | FR-24 |
| BR-18 | Hợp đồng trạng thái `ChoNhanPhong` sinh mã QR bảo mật 128-bit CSPRNG và mã PIN 8 số ngẫu nhiên; chỉ hiển thị mã khi hợp đồng chưa được kích hoạt | FR-25 |
| BR-19 | Khách thuê nhận phòng qua `KHACH_THUE_NHAN_PHONG_QR` (quét webcam, ảnh QR hoặc PIN); Server kiểm tra đúng đại diện, kích hoạt hợp đồng và xóa token/PIN một lần để chống quét lại | FR-26 |

---

## PHỤ LỤC A: BẢNG HÀNH ĐỘNG GIAO THỨC TCP (29 ACTIONS)

Hệ thống sử dụng chuẩn giao thức JSON trên TCP. Mỗi yêu cầu gồm `Action`, `Token`, `Data`; phản hồi gồm `Success`, `Message`, `Data`. Toàn bộ 29 hành động chuẩn được định nghĩa tại `ActionNames.cs`:

| Nhóm | Tên hành động | Ý nghĩa nghiệp vụ |
|---|---|---|
| **Xác thực** | `DANG_NHAP` | Đăng nhập hệ thống bằng tên tài khoản/CCCD và mật khẩu |
| | `DANG_KY` | Khách thuê tự đăng ký tài khoản mới từ xa (public route, không cần token) |
| **Phòng trọ** | `PHONG_LAY_TAT_CA` | Lấy danh sách toàn bộ phòng và số người hiện tại |
| | `PHONG_THEM` | Thêm phòng trọ mới |
| | `PHONG_CAP_NHAT` | Sửa thông tin phòng (giá thuê, sức chứa, trạng thái) |
| | `PHONG_XOA` | Xóa phòng trọ (chỉ cho phép khi phòng trống) |
| **Người thuê** | `KHACH_THUE_THEO_PHONG` | Lấy danh sách người thuê theo phòng (hoặc khách chờ khi `phongId=0`) |
| | `KHACH_THUE_THEM` | Thêm hồ sơ người thuê vào phòng |
| | `KHACH_THUE_CAP_NHAT` | Cập nhật thông tin cá nhân người thuê |
| | `KHACH_THUE_TRA_PHONG` | Ghi nhận khách trả phòng, giải phóng liên kết phòng |
| | `KHACH_THUE_XOA` | Xóa vĩnh viễn hồ sơ người thuê đã trả phòng |
| **Hợp đồng** | `HOP_DONG_TAO` | Lập hợp đồng mới (trực tiếp hoặc chờ nhận phòng QR) |
| | `HOP_DONG_GIA_HAN` | Gia hạn ngày kết thúc hợp đồng đang hiệu lực |
| | `HOP_DONG_CHAM_DUT` | Chấm dứt hợp đồng (bắt buộc lý do và ngày bàn giao khi còn hạn) |
| | `HOP_DONG_LAY_TAT_CA` | Lấy danh sách toàn bộ hợp đồng |
| | `HOP_DONG_SINH_QR` | Lấy lại mã QR và mã PIN của hợp đồng chờ nhận phòng |
| **Nhận phòng QR** | `KHACH_THUE_NHAN_PHONG_QR` | Khách gửi mã QR/PIN để kích hoạt hợp đồng và nhận phòng một lần |
| **Điện nước** | `DIEN_NUOC_LAY_KY_TRUOC` | Lấy chỉ số điện nước kỳ trước liền kề |
| | `DIEN_NUOC_GHI_SO` | Chốt chỉ số điện nước kỳ hiện tại |
| **Hóa đơn** | `HOA_DON_TAO` | Lập hóa đơn cước tháng cho phòng |
| | `HOA_DON_LAY_TAT_CA` | Lấy danh sách hóa đơn theo kỳ cước hoặc theo phòng |
| | `HOA_DON_THANH_TOAN` | Xác nhận hóa đơn đã thanh toán (DaThu) |
| | `HOA_DON_CUA_TOI` | Khách thuê tra cứu danh sách hóa đơn của mình (hỗ trợ phân trang) |
| **Báo cáo & Tạm trú** | `BAO_CAO_TONG_QUAN` | Lấy số liệu KPI tổng quan khu trọ |
| | `XUAT_HO_SO_TAM_TRU` | Xuất danh sách thông tin người ở phục vụ đăng ký tạm trú |
| | `LICH_SU_CU_TRU_LAY` | Lấy lịch sử biến động cư trú theo thời gian |
| | `XUAT_LICH_SU_CU_TRU` | Xuất file báo cáo lịch sử cư trú phục vụ kiểm tra công an |
| **Phân quyền** | `PHAN_QUYEN_LAY_MA_TRAN` | Lấy ma trận quyền động hiện tại của hệ thống |
| | `PHAN_QUYEN_CAP_NHAT_VAI_TRO` | Chủ trọ cập nhật danh mục quyền cho từng vai trò |

---

## PHỤ LỤC B: MÔ HÌNH DỮ LIỆU CSDL (7 BẢNG CHUẨN)

Hệ thống tổ chức cơ sở dữ liệu MySQL 8.0 gồm 7 bảng cốt lõi (tất cả định danh bằng tiếng Việt không dấu theo chuẩn `snake_case`):

1. **`tai_khoan`:** Tài khoản nội bộ của Chủ trọ, Quản lý, Công an (`id`, `ten_dang_nhap`, `mat_khau_hash`, `ho_ten`, `vai_tro`, `ngay_tao`).
2. **`quyen_vai_tro`:** Ma trận phân quyền động (`vai_tro`, `hanh_dong`, `ngay_tao`) với khóa chính kết hợp `(vai_tro, hanh_dong)`.
3. **`phong`:** Danh mục phòng trọ (`id`, `so_phong` UNIQUE, `gia_thue`, `so_nguoi_toi_da`, `trang_thai`, `mo_ta`, `ngay_tao`).
4. **`khach_thue`:** Hồ sơ người thuê kiêm tài khoản đăng nhập khách (`id`, `phong_id`, `ho_ten`, `ngay_sinh`, `cccd` UNIQUE, `mat_khau_hash`, `so_dien_thoai`, `que_quan`, `noi_lam_viec`, `da_dang_ky_tam_tru`, `ngay_tao`).
5. **`hop_dong`:** Hợp đồng thuê phòng (`id`, `phong_id`, `nguoi_dai_dien_id`, `ngay_bat_dau`, `ngay_ket_thuc`, `gia_thue`, `tien_coc`, `trang_thai`, `ghi_chu`, `ma_qr_token` UNIQUE, `ma_pin`, `ngay_tao`).
6. **`chi_so_dien_nuoc`:** Chỉ số tiêu thụ hàng tháng (`id`, `phong_id`, `ky_cuoc`, `dien_cu`, `dien_moi`, `gia_dien`, `nuoc_cu`, `nuoc_moi`, `gia_nuoc`, `hinh_thuc_nuoc`, `so_nguoi_nuoc`, `ngay_ghi`) với ràng buộc duy nhất `uq_phong_ky(phong_id, ky_cuoc)`.
7. **`hoa_don`:** Hóa đơn thanh toán tháng (`id`, `phong_id`, `hop_dong_id`, `ky_cuoc`, `tien_phong`, `tien_dien`, `tien_nuoc`, `phi_khac`, `tong_tien`, `trang_thai`, `ngay_dong`, `ngay_tao`) với ràng buộc duy nhất `uq_hoa_don_phong_ky(phong_id, ky_cuoc)`.
