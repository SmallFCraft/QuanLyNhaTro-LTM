# BÁO CÁO ĐỀ TÀI
## XÂY DỰNG HỆ THỐNG QUẢN LÝ PHÒNG TRỌ PHƯỜNG NGŨ HÀNH SƠN

**Môn học:** Lập Trình Mạng
**Mô hình:** Ứng dụng Client – Server trên giao thức TCP/IP

---

## 1. ĐẶT VẤN ĐỀ

Phường Ngũ Hành Sơn (TP. Đà Nẵng) có mật độ phòng trọ cao, phục vụ đông đảo sinh viên các trường Đại học Kinh tế – Đà Nẵng, VKU, FPT Đà Nẵng và người lao động ngoại tỉnh. Hiện nay, phần lớn chủ trọ vẫn quản lý bằng sổ tay hoặc file Excel rời rạc, dẫn đến nhiều bất cập: sót hoặc sai chỉ số điện nước, thất lạc hợp đồng, khó tra cứu lịch sử thanh toán, và mất nhiều thời gian tổng hợp danh sách khai báo tạm trú cho công an phường.

Từ thực tế đó, nhóm lựa chọn đề tài xây dựng hệ thống quản lý phòng trọ theo mô hình Client – Server, hướng đến giải quyết trọn vẹn quy trình: từ quản lý phòng, thành viên, hợp đồng cho đến chốt điện nước và lập hóa đơn thanh toán.

## 2. MỤC TIÊU ĐỀ TÀI

Hệ thống được xây dựng với các mục tiêu sau:

Thứ nhất, tập trung toàn bộ dữ liệu về phòng trọ, người thuê, hợp đồng, điện nước và hóa đơn về một máy chủ duy nhất, đảm bảo dữ liệu thống nhất và an toàn.

Thứ hai, đúng trọng tâm môn Lập Trình Mạng: máy khách (Client) hoàn toàn không truy cập trực tiếp cơ sở dữ liệu. Mọi yêu cầu đều được đóng gói và gửi qua kết nối TCP đến máy chủ (Server); Server xử lý nghiệp vụ, truy vấn cơ sở dữ liệu và trả kết quả về cho Client.

Thứ ba, Server phải phục vụ được nhiều Client kết nối đồng thời mà dữ liệu vẫn nhất quán, không xảy ra tranh chấp hay ghi đè lẫn nhau.

## 3. CÔNG NGHỆ SỬ DỤNG

| Thành phần | Lựa chọn | Lý do |
|---|---|---|
| Ứng dụng Client | C# WinForms (.NET 8.0) + WebView2 nhúng web cục bộ | Khung desktop native chuẩn của .NET, nhúng Microsoft Edge WebView2 hiển thị giao diện HTML5/CSS3/JS từ `Assets/wwwroot/` chạy offline, có Canvas giải mã QR và WebRTC điều khiển camera |
| Ứng dụng Server | C# Console App (.NET 8.0), thư viện TcpListener | Nằm trong chuẩn .NET, đúng tinh thần lập trình mạng |
| Giao thức trao đổi | TCP/IP, cổng 8888, gói tin văn bản dạng JSON | TCP đảm bảo tin cậy, đúng thứ tự; JSON dễ đọc, dễ mở rộng |
| Cơ sở dữ liệu | MySQL 8.0 trên Laragon (cổng 3306) | Nhẹ, miễn phí, quen thuộc với nhóm |
| Truy vấn dữ liệu | ADO.NET (MySqlConnector) | Thư viện chuẩn của hệ sinh thái .NET |
| Thư viện QR | `qrcode.min.js` (sinh mã) và `jsQR.min.js` (giải mã) | File cục bộ phía Client, hoạt động offline, mã nguồn mở (MIT) |

## 4. PHẠM VI ĐỀ TÀI

### 4.1. Trong phạm vi

Hệ thống quản lý trọn vẹn vòng đời vận hành một cơ sở phòng trọ: quản lý phòng (thêm, sửa, xóa, theo dõi trạng thái, cảnh báo Treo HĐ); quản lý người thuê và tự đăng ký tài khoản từ xa; quản lý hợp đồng thuê từ lúc lập đến khi chấm dứt, bao gồm cả quy trình bàn giao nhận phòng số hóa bằng mã QR / PIN 8 số; ghi chỉ số điện nước hàng tháng và tự động tính tiền; lập hóa đơn tổng hợp và xác nhận thanh toán; thống kê doanh thu – công nợ – công suất phòng; quản lý ma trận phân quyền động (RBAC) theo vai trò; và tra cứu cùng xuất danh sách người thuê, lịch sử cư trú phục vụ công tác kiểm tra của công an phường. Về mặt mạng, hệ thống triển khai đầy đủ kiến trúc Client – Server qua TCP, hỗ trợ nhiều máy khách đồng thời.

### 4.2. Ngoài phạm vi

Đề tài không bao gồm: thanh toán trực tuyến qua cổng ngân hàng/VNPAY/Momo; ứng dụng di động native trên điện thoại; gửi thông báo tự động qua email/SMS; và quản lý phân tán nhiều cơ sở trọ (hệ thống phục vụ tập trung một cơ sở phòng trọ, số phòng là duy nhất toàn hệ thống).

## 5. ĐỐI TƯỢNG SỬ DỤNG VÀ KỊCH BẢN VẬN HÀNH

Hệ thống phục vụ bốn nhóm người dùng phân quyền rõ rệt: **Chủ trọ** (toàn quyền, quản lý phân quyền động), **Quản lý trọ** (thực hiện nghiệp vụ vận hành hằng ngày theo quyền được cấp), **Công an phường** (chỉ xem, tra cứu và xuất dữ liệu kiểm tra cư trú), và **Khách thuê** (tự đăng ký tài khoản, quét QR nhận phòng, xem cước cá nhân).

Kịch bản vận hành điển hình trong một tháng:
- **Đầu tháng / Khi có khách mới:** Khách thuê tự đăng ký tài khoản từ xa trên ứng dụng; chủ trọ lập hợp đồng ở trạng thái chờ nhận phòng và gửi mã QR kèm mã PIN 8 số cho khách; khách tự dùng webcam hoặc tải ảnh QR để nhận phòng, hệ thống tự động kích hoạt hợp đồng và gán phòng trong 1 giao dịch an toàn.
- **Cuối tháng:** Chủ trọ hoặc quản lý ghi chỉ số điện nước từng phòng; hệ thống tự tính số tiêu thụ và thành tiền (nước hỗ trợ tính theo khối hoặc khoán theo người). Sau đó lập hóa đơn cước tháng cho từng phòng. Khách thuê có thể đăng nhập xem chi tiết bảng kê cước của mình và bấm sao chép nội dung chuyển khoản nhanh.
- **Thu tiền & Báo cáo:** Khi khách đóng tiền, chủ trọ xác nhận thanh toán; các phòng chưa nộp được lọc danh sách để đôn đốc. Định kỳ, công an phường đăng nhập tra cứu tình trạng cư trú và xuất file báo cáo phục vụ công tác quản lý địa bàn.

## 6. CÁC CHỨC NĂNG CHÍNH CỦA HỆ THỐNG

**Quản lý phòng trọ.** Chủ trọ thêm, sửa, xóa phòng cùng các thông tin số phòng, giá thuê, sức chứa. Hệ thống tự theo dõi trạng thái mỗi phòng (trống, đang thuê, bảo trì) và hiển thị số thành viên hiện tại — số này được đếm trực tiếp từ danh sách người thuê nên luôn khớp với thực tế. Phòng chỉ được xóa khi đang trống. Nếu phòng có người ở nhưng không còn hợp đồng hiệu lực nào, hệ thống tự động gắn thẻ cảnh báo cam `Treo HĐ` để chủ trọ kịp thời xử lý.

**Quản lý người thuê & Tự đăng ký.** Mỗi thành viên ở trọ có một hồ sơ gồm họ tên, ngày sinh, CCCD (12 số duy nhất), số điện thoại, quê quán và nơi học tập/làm việc. Khách thuê có thể tự đăng ký tài khoản từ xa (`DANG_KY`) với mật khẩu ≥ 6 ký tự. Chủ trọ có thể gán người thuê vào phòng (kiểm tra sức chứa), chuyển phòng hoặc ghi nhận trả phòng. Hệ thống lưu cờ "đã đăng ký tạm trú" cho từng người.

**Quản lý hợp đồng & Nhận phòng QR/PIN.** Mỗi phòng đang cho thuê có tối đa một hợp đồng còn hiệu lực. Hệ thống hỗ trợ lập hợp đồng chờ nhận phòng (`ChoNhanPhong`) tự sinh mã QR bảo mật 128-bit CSPRNG và mã PIN 8 số; chủ trọ có thể in phiếu bàn giao trực tiếp. Khách thuê dùng camera hoặc nhập PIN để nhận phòng tự động. Khi chấm dứt hợp đồng còn hạn, hệ thống bắt buộc nhập lý do và ngày bàn giao dự kiến (≥ 30 ngày) tuân thủ Điều 172 Luật Nhà ở 2023.

**Quản lý điện, nước.** Hàng tháng, chủ trọ nhập chỉ số đầu kỳ và cuối kỳ cho từng phòng. Chỉ số đầu kỳ được hệ thống tự điền bằng chỉ số cuối kỳ của tháng trước, tránh nhập tay sai sót. Tiền điện và tiền nước do Server tính theo công thức: số tiêu thụ nhân đơn giá, với đơn giá được lưu riêng cho từng kỳ.

**Hóa đơn và thanh toán.** Hóa đơn tháng được lập theo phòng, tổng hợp tiền phòng, tiền điện, tiền nước và phí khác; mỗi phòng chỉ có một hóa đơn cho mỗi tháng. Hóa đơn chuyển từ trạng thái chưa thanh toán sang đã thanh toán kèm mốc thời gian thu tiền, làm cơ sở thống kê công nợ và doanh thu. Khách thuê có màn hình riêng xem bảng kê cước và lịch sử hóa đơn phân trang từ server.

**Ma trận phân quyền động (RBAC).** Chủ trọ quản lý danh mục quyền của Quản lý trọ, Công an và Khách thuê thông qua giao diện ma trận quyền thời gian thực (`quyen_vai_tro`). Sổ cái `quyen_mac_dinh_da_ap_dung` tự động gieo bù quyền mới khi nâng cấp hệ thống mà không hồi sinh các quyền đã bị Chủ trọ chủ động thu hồi.

**Tra cứu phục vụ công an phường.** Công an phường tra cứu danh sách người thuê, tình trạng tạm trú và biến động cư trú ở chế độ chỉ xem; xuất file báo cáo phục vụ kiểm tra; Server từ chối mọi yêu cầu ghi dữ liệu từ vai trò này.

## 7. USER STORY

Đầu mục (Epic) được chia theo nhóm chức năng. Mỗi user story kèm độ ưu tiên: **Must** — bắt buộc có để hệ thống vận hành; **Should** — nên có, tăng giá trị sử dụng; **Could** — có thể bổ sung khi dư thời gian.

### 7.1. Epic 1 — Quản lý phòng trọ

**US-01 — Thêm phòng mới** (Must)
> Là chủ trọ, tôi muốn thêm phòng mới với số phòng, giá thuê và sức chứa để khai thác thêm phòng.

Tiêu chí chấp nhận: phòng mới mặc định ở trạng thái trống; số phòng không được trùng; giá thuê và sức chứa là số dương; danh sách phòng cập nhật ngay sau khi thêm.

**US-02 — Sửa hoặc xóa phòng** (Must, phụ thuộc US-01)
> Là chủ trọ, tôi muốn sửa thông tin hoặc xóa phòng để dữ liệu luôn đúng hiện trạng.

Tiêu chí chấp nhận: sửa được giá thuê, sức chứa, trạng thái; chỉ xóa được phòng trống (không có người thuê, không có hợp đồng còn hiệu lực); thao tác xóa phải được xác nhận.

**US-03 — Xem danh sách và trạng thái phòng** (Must, phụ thuộc US-01)
> Là chủ trọ, tôi muốn xem toàn bộ phòng kèm trạng thái để nắm nhanh hiện trạng khu trọ.

Tiêu chí chấp nhận: lưới hiển thị số phòng, giá, sức chứa, số người hiện tại, trạng thái; số người hiện tại được đếm từ danh sách người thuê; lọc được theo trạng thái.

**US-04 — Màn hình tổng quan** (Should, phụ thuộc US-03)
> Là chủ trọ, tôi muốn thấy ngay tổng phòng, phòng trống, người đang ở và tình hình thu chi tháng này ngay khi mở ứng dụng.

Tiêu chí chấp nhận: hiển thị tổng phòng, phòng trống, phòng đang thuê, tổng người ở, tổng đã thu và chưa thu của tháng hiện tại.

### 7.2. Epic 2 — Quản lý người thuê

**US-05 — Thêm người thuê và gán vào phòng** (Must, phụ thuộc US-01)
> Là chủ trọ, tôi muốn lưu hồ sơ từng thành viên ở trọ và gán vào phòng để quản lý người ở chặt chẽ.

Tiêu chí chấp nhận: hồ sơ gồm họ tên, ngày sinh, CCCD, SĐT, quê quán, nơi học tập/làm việc; CCCD bắt buộc và không trùng; chỉ gán được vào phòng còn chỗ, quá chỗ thì hệ thống từ chối với thông báo rõ ràng; khi phòng có người ở, trạng thái tự chuyển thành đang thuê.

**US-06 — Sửa hoặc xóa hồ sơ người thuê** (Must, phụ thuộc US-05)
> Là chủ trọ, tôi muốn cập nhật hồ sơ khi thông tin thay đổi.

Tiêu chí chấp nhận: sửa được toàn bộ thông tin cá nhân; chỉ xóa được người đã trả phòng.

**US-07 — Chuyển phòng, trả phòng** (Must, phụ thuộc US-05)
> Là chủ trọ, tôi muốn chuyển người thuê sang phòng khác hoặc ghi nhận trả phòng để cập nhật hiện trạng.

Tiêu chí chấp nhận: chuyển phòng chỉ chấp nhận khi phòng đích còn chỗ; trả phòng thì bỏ liên kết phòng và nếu phòng trống hẳn thì trạng thái về "trống".

**US-08 — Xuất danh sách khai báo tạm trú** (Could, phụ thuộc US-05)
> Là chủ trọ, tôi muốn xuất danh sách người đang ở để nộp công an phường Ngũ Hành Sơn.

Tiêu chí chấp nhận: xuất được file CSV/Excel gồm họ tên, ngày sinh, CCCD, quê quán, số phòng; chỉ gồm người đang ở.

### 7.3. Epic 3 — Quản lý hợp đồng

**US-09 — Lập hợp đồng thuê** (Must, phụ thuộc US-01, US-05)
> Là chủ trọ, tôi muốn lập hợp đồng với người đại diện cho một phòng để có căn cứ thu tiền và ràng buộc hai bên.

Tiêu chí chấp nhận: chọn phòng đang trống và người đại diện thuộc phòng đó; nhập ngày bắt đầu, ngày kết thúc, giá thuê, tiền cọc; ngày kết thúc phải sau ngày bắt đầu; một phòng chỉ có một hợp đồng còn hiệu lực tại một thời điểm; lập hợp đồng xong phòng tự chuyển sang đang thuê.

**US-10 — Gia hạn hoặc chấm dứt hợp đồng** (Must, phụ thuộc US-09)
> Là chủ trọ, tôi muốn gia hạn khi hết hạn hoặc chấm dứt khi khách rời đi.

Tiêu chí chấp nhận: gia hạn cập nhật ngày kết thúc mới; chấm dứt lưu lý do và ghi chú hoàn trả cọc; chỉ thao tác trên hợp đồng còn hiệu lực.

**US-11 — Theo dõi hợp đồng sắp hết hạn** (Should, phụ thuộc US-09)
> Là chủ trọ, tôi muốn thấy các hợp đồng hết hạn trong 30 ngày tới để chủ động liên hệ khách.

Tiêu chí chấp nhận: danh sách gồm phòng, người đại diện, ngày hết hạn, số ngày còn lại; sắp xếp theo ngày hết hạn tăng dần.

### 7.4. Epic 4 — Quản lý điện, nước

**US-12 — Nhập chỉ số điện nước hàng tháng** (Must, phụ thuộc US-01)
> Là chủ trọ, tôi muốn nhập chỉ số điện nước đầu kỳ và cuối kỳ theo từng phòng, từng tháng.

Tiêu chí chấp nhận: chọn phòng và tháng chốt; chỉ số cũ tự lấy bằng chỉ số mới của kỳ trước nếu có; hệ thống từ chối nếu chỉ số mới nhỏ hơn chỉ số cũ; đơn giá điện, nước lưu riêng theo từng kỳ.

**US-13 — Tự động tính tiền điện nước** (Must, phụ thuộc US-12)
> Là chủ trọ, tôi muốn hệ thống tự tính tiền để tránh sai sót tính tay.

Tiêu chí chấp nhận: tiền điện bằng số tiêu thụ nhân đơn giá điện, tiền nước tương tự; hiển thị số tiêu thụ và thành tiền ngay trước khi lưu; phép tính do Server thực hiện.

### 7.5. Epic 5 — Hóa đơn và thanh toán

**US-14 — Lập hóa đơn tháng** (Must, phụ thuộc US-09, US-12)
> Là chủ trọ, tôi muốn lập hóa đơn tháng cho từng phòng đang thuê, gồm tiền phòng, điện, nước và phí khác.

Tiêu chí chấp nhận: hệ thống tự gom các khoản theo hợp đồng và số liệu điện nước của tháng; tổng tiền là tổng các khoản; mỗi phòng chỉ có một hóa đơn mỗi tháng, trùng thì hệ thống từ chối; phòng chưa chốt điện nước không thể lập hóa đơn.

**US-15 — Xem chi tiết và lịch sử hóa đơn** (Must, phụ thuộc US-14)
> Là chủ trọ, tôi muốn tra cứu hóa đơn theo phòng hoặc tháng để đối chiếu với khách.

Tiêu chí chấp nhận: danh sách hóa đơn kèm trạng thái đã/chưa thanh toán; xem được chi tiết từng khoản kèm chỉ số và đơn giá.

**US-16 — Xác nhận thanh toán** (Must, phụ thuộc US-14)
> Là chủ trọ, tôi muốn đánh dấu hóa đơn đã thu tiền và lưu thời điểm thu để theo dõi công nợ.

Tiêu chí chấp nhận: chuyển trạng thái và lưu thời gian xác nhận; hóa đơn đã thanh toán không được sửa hay xóa.

**US-17 — Danh sách phòng còn nợ** (Should, phụ thuộc US-14)
> Là chủ trọ, tôi muốn xem các phòng còn nợ để đôn đốc thu tiền.

Tiêu chí chấp nhận: liệt kê phòng, người đại diện, tháng, số tiền còn nợ; lọc được theo tháng.

### 7.6. Epic 6 — Thống kê và tra cứu

**US-18 — Thống kê hiện trạng** (Should, phụ thuộc US-03, US-05)
> Là chủ trọ, tôi muốn xem tỷ lệ lấp đầy và tổng người đang ở để đánh giá công suất.

Tiêu chí chấp nhận: số liệu tính trực tiếp từ cơ sở dữ liệu; hiển thị số liệu và biểu đồ đơn giản.

**US-19 — Thống kê doanh thu theo tháng** (Should, phụ thuộc US-14)
> Là chủ trọ, tôi muốn xem tổng thu và chưa thu theo từng tháng để quản lý tài chính.

Tiêu chí chấp nhận: chọn được khoảng tháng; bảng thống kê có dòng tổng cộng.

**US-20 — Tra cứu nhanh** (Should)
> Là chủ trọ, tôi muốn tìm kiếm theo tên, số phòng, CCCD để tra cứu nhanh.

Tiêu chí chấp nhận: tìm kiếm xử lý ở Server; không phân biệt chữ hoa chữ thường; hỗ trợ tìm một phần của CCCD và số điện thoại.

### 7.7. Epic 7 — Kết nối mạng & Đăng nhập (trọng tâm môn học)

**US-21 — Kết nối Client đến Server** (Must)
> Là người dùng, tôi muốn Client kết nối đến Server theo địa chỉ IP và cổng cấu hình được, để có thể làm việc từ các máy trong mạng LAN.

Tiêu chí chấp nhận: kết nối TCP tới 127.0.0.1:8888; hiển thị trạng thái kết nối; mất kết nối thì thông báo rõ ràng, ứng dụng không bị treo; mỗi gói tin được kết thúc bằng ký tự xuống dòng `\n` để Server đọc trọn vẹn.

**US-22 — Nhiều Client cùng lúc** (Must, phụ thuộc US-21)
> Là người dùng, tôi muốn nhiều máy thao tác cùng lúc mà dữ liệu vẫn nhất quán.

Tiêu chí chấp nhận: Server phục vụ nhiều kết nối đồng thời, mỗi kết nối một luồng xử lý độc lập; hai Client cùng tạo một phòng thì chỉ một lần thành công; transaction và row locking chống xung đột.

**US-23 — Đăng nhập hợp nhất** (Must, phụ thuộc US-21)
> Là người dùng (Chủ trọ, Quản lý, Công an, Khách thuê), tôi muốn đăng nhập chung trên một form duy nhất và hệ thống tự động nhận diện vai trò chuyển hướng vào đúng giao diện làm việc.

Tiêu chí chấp nhận: mật khẩu lưu dưới dạng băm; sai quá 5 lần liên tiếp bị tạm khóa một phút; tự chuyển vào shell tương ứng với quyền hạn.

### 7.8. Epic 8 — Tự đăng ký & Nhận phòng QR/PIN (BR-17, BR-18, BR-19)

**US-24 — Khách thuê tự đăng ký tài khoản** (Must, phụ thuộc US-21)
> Là khách thuê mới, tôi muốn tự đăng ký tài khoản từ xa bằng CCCD và mật khẩu để chủ động làm thủ tục nhận phòng.

Tiêu chí chấp nhận: form đăng ký kiểm tra CCCD 12 số, mật khẩu ≥ 6 ký tự; gửi lên server qua `DANG_KY` (hành động công khai không cần token); tài khoản mới tạo có `phong_id = NULL`.

**US-25 — Lập hợp đồng chờ nhận phòng & sinh mã QR / PIN** (Must, phụ thuộc US-09)
> Là chủ trọ, tôi muốn lập hợp đồng ở trạng thái chờ nhận phòng để cấp mã QR và PIN cho khách tự vào phòng.

Tiêu chí chấp nhận: hợp đồng tạo ở trạng thái `ChoNhanPhong`; server tự sinh mã token 128-bit CSPRNG và mã PIN 8 số; chủ trọ có thể xem mã và in phiếu bàn giao trực tiếp từ giao diện.

**US-26 — Khách thuê nhận phòng qua QR / Webcam / PIN** (Must, phụ thuộc US-24, US-25)
> Là khách thuê, tôi muốn quét mã QR hoặc nhập PIN 8 số để hoàn tất nhận phòng mà không cần chờ chủ trọ đưa chìa khóa tận tay.

Tiêu chí chấp nhận: hỗ trợ 3 cách (tải file ảnh QR, quét webcam trực tiếp, nhập mã PIN 8 số); server kiểm tra đúng đại diện, kích hoạt hợp đồng thành `HieuLuc`, gán phòng cho khách, phòng chuyển sang `DaThue`, và xóa mã token/PIN để chống quét lại.

### 7.9. Epic 9 — Ma trận phân quyền động (RBAC)

**US-27 — Tùy biến phân quyền cho các vai trò** (Must, phụ thuộc US-23)
> Là chủ trọ, tôi muốn bật/tắt quyền hạn của Quản lý trọ, Công an và Khách thuê thông qua giao diện ma trận để linh hoạt phân công công việc.

Tiêu chí chấp nhận: ma trận hiển thị danh mục quyền theo từng nhóm; chủ trọ tick chọn và lưu thay đổi có hiệu lực ngay lập tức mà không cần khởi động lại Server; quyền mới tự động gieo bù qua sổ cái khi nâng cấp hệ thống.

### 7.10. Epic 10 — Chấm dứt hợp đồng tuân thủ Luật Nhà ở 2023

**US-28 — Ràng buộc lý do và ngày bàn giao khi chấm dứt trước hạn** (Must, phụ thuộc US-10)
> Là chủ trọ, tôi muốn khi chấm dứt hợp đồng trước hạn phải ghi rõ lý do pháp lý và ngày bàn giao dự kiến (≥ 30 ngày) để đúng quy định Điều 172 Luật Nhà ở 2023.

Tiêu chí chấp nhận: hợp đồng còn hạn bắt buộc nhập lý do chấm dứt và chọn ngày bàn giao dự kiến; thông tin được lưu vết tự động vào ghi chú hợp đồng.

**US-29 — Cảnh báo phòng treo hợp đồng** (Should, phụ thuộc US-03, US-28)
> Là chủ trọ, tôi muốn nhìn thấy phòng nào đang có người ở nhưng không còn hợp đồng hiệu lực để kịp thời xử lý thủ tục trả phòng hoặc tái ký.

Tiêu chí chấp nhận: trên bảng danh sách phòng, các phòng có người nhưng 0 hợp đồng hiệu lực được gắn thẻ cam `Treo HĐ` cảnh báo nổi bật.

## 8. QUY TẮC NGHIỆP VỤ

Các quy tắc dưới đây là ràng buộc bắt buộc, **đều được Server kiểm tra và thực thi** trước khi ghi dữ liệu — Client không được phép quyết định tính hợp lệ của nghiệp vụ:

| Mã | Quy tắc |
|---|---|
| BR-01 | Số phòng là duy nhất trong toàn hệ thống |
| BR-02 | Số thành viên một phòng không vượt quá sức chứa của phòng |
| BR-03 | CCCD của người thuê là duy nhất trong toàn hệ thống |
| BR-04 | Một phòng chỉ có tối đa một hợp đồng còn hiệu lực tại một thời điểm |
| BR-05 | Người đại diện ký hợp đồng phải là thành viên của chính phòng đó |
| BR-06 | Ngày kết thúc hợp đồng phải sau ngày bắt đầu |
| BR-07 | Chỉ số mới của điện và nước không được nhỏ hơn chỉ số cũ |
| BR-08 | Mỗi phòng chỉ có một bản ghi điện nước và một hóa đơn cho mỗi tháng |
| BR-09 | Phòng phải có hợp đồng còn hiệu lực và đã chốt điện nước thì mới được lập hóa đơn |
| BR-10 | Tổng tiền hóa đơn bằng tiền phòng cộng tiền điện, tiền nước và phí khác |
| BR-11 | Hóa đơn đã thanh toán không được sửa đổi hay xóa |
| BR-12 | Chỉ xóa được phòng khi phòng đang trống |
| BR-13 | Các thao tác ghi liên quan nhiều bảng (hợp đồng, hóa đơn, nhận phòng) chạy trong một giao dịch (giao_dich) — hoặc thành công toàn bộ, hoặc không ghi gì |
| BR-14 | Tra cứu hóa đơn của khách thuê suy phòng từ token phiên, không nhận phòng từ client; phân trang ở Server |
| BR-15 | Ma trận phân quyền động kiểm soát quyền từng vai trò; quyền mới tự động gieo bù mà không hồi sinh quyền đã bị thu hồi |
| BR-16 | Vai Công an phường chỉ xem — Server từ chối mọi yêu cầu ghi dữ liệu từ vai trò này |
| BR-17 | Khách thuê tự đăng ký tài khoản từ xa (hành động công khai, không cần token); hồ sơ mới có phong_id rỗng |
| BR-18 | Hợp đồng chờ nhận phòng sinh mã QR 128-bit CSPRNG và mã PIN 8 số; mã chỉ hiển thị khi hợp đồng chưa kích hoạt |
| BR-19 | Nhận phòng qua QR/PIN: kiểm tra đúng người đại diện, kích hoạt hợp đồng và xóa token/PIN một lần chống quét lại |

## 9. THIẾT KẾ DỮ LIỆU

Cơ sở dữ liệu `quanly_phongtro_nhs` gồm 7 bảng cốt lõi, thiết kế ở dạng chuẩn hóa 3NF, dùng khóa ngoại đảm bảo toàn vẹn và ràng buộc duy nhất chặn trùng lặp ngay từ tầng cơ sở dữ liệu:

**Bảng tai_khoan** — tài khoản Chủ trọ, Quản lý, Công an: tên đăng nhập (duy nhất), mật khẩu đã băm, họ tên, vai trò (ChuTro / QuanLy / CongAn).

**Bảng quyen_vai_tro** — ma trận phân quyền động: cặp (vai trò, hành động) cho biết vai trò đó được phép gọi hành động nào. Bảng là nguồn duy nhất nạp vào bộ kiểm tra quyền lúc Server khởi động.

**Bảng phong** — phòng trọ: số phòng (duy nhất), giá thuê, sức chứa tối đa, trạng thái (trống / đang thuê / bảo trì), mô tả.

**Bảng khach_thue** — người thuê kiêm tài khoản đăng nhập khách: họ tên, ngày sinh, CCCD (duy nhất), mật khẩu đã băm, số điện thoại, quê quán, nơi học tập/làm việc, cờ đăng ký tạm trú, khóa ngoại về phòng đang ở (cho phép rỗng khi khách tự đăng ký hoặc đã trả phòng).

**Bảng hop_dong** — hợp đồng: khóa ngoại về phòng và về người đại diện, ngày bắt đầu, ngày kết thúc, giá thuê, tiền cọc, trạng thái (chờ nhận phòng / còn hiệu lực / hết hạn / chấm dứt), ghi chú, mã QR (duy nhất) và mã PIN dùng cho bàn giao phòng.

**Bảng chi_so_dien_nuoc** — chỉ số điện nước: khóa ngoại về phòng, tháng chốt (định dạng năm-tháng), chỉ số điện và nước cũ/mới, đơn giá điện và nước của kỳ, hình thức tính nước (theo khối / khoán theo người). Ràng buộc duy nhất trên cặp (phòng, tháng) chặn ghi trùng kỳ.

**Bảng hoa_don** — hóa đơn: khóa ngoại về phòng và về hợp đồng, tháng lập, các khoản tiền phòng, điện, nước, phí khác, tổng tiền, trạng thái thanh toán, thời điểm thanh toán. Ràng buộc duy nhất trên cặp (phòng, tháng) đảm bảo không có hai hóa đơn cùng kỳ.

## 10. KIẾN TRÚC VÀ GIAO THỨC TRAO ĐỔI

### 10.1. Kiến trúc ba tầng

Hệ thống tổ chức theo kiến trúc ba tầng, tách bạch trách nhiệm giữa giao diện, xử lý nghiệp vụ và lưu trữ:

- **Tầng trình diễn (Client WinForms + WebView2):** ứng dụng WinForms nhúng Microsoft Edge WebView2, tải giao diện HTML/CSS/JavaScript thuần từ thư mục cục bộ `Assets/wwwroot/` chia thành các shell giao diện chuyên biệt cho từng vai trò (`chutro`, `congan`, `khachthue`, `auth`). Client không chứa logic nghiệp vụ, giao tiếp thông qua cầu nối `window.bridge.call(action, payload)` gửi dữ liệu JSON qua socket TCP tới Server.
- **Tầng nghiệp vụ (Server):** ứng dụng Console .NET 8.0 lắng nghe kết nối qua TcpListener tại cổng 8888. Với mỗi Client, Server cấp phát một luồng xử lý riêng (ClientHandler), nhận gói yêu cầu, kiểm tra phiên đăng nhập và ma trận quyền (`MaTranPhanQuyen`), rồi định tuyến đến service tương ứng.
- **Tầng dữ liệu (MySQL):** Server truy cập cơ sở dữ liệu qua ADO.NET (MySqlConnector), tổ chức thành các Repository cho từng nhóm đối tượng.

### 10.2. Giao thức ứng dụng trên TCP

Client và Server trao đổi bằng gói tin văn bản dạng JSON, mỗi gói kết thúc bằng ký tự xuống dòng `\n` để phân định ranh giới gói tin trên luồng TCP. Một yêu cầu gồm ba trường: tên hành động (Action), mã phiên đăng nhập (Token) và dữ liệu (Data). Phản hồi của Server gồm ba trường: thành công hay thất bại (Success), thông điệp (Message) và dữ liệu kết quả (Data).

Hệ thống định nghĩa 29 hành động chuẩn bằng tiếng Việt không dấu (`ActionNames.cs`): xác thực và tự đăng ký (DANG_NHAP, DANG_KY); bốn hành động phòng (lấy danh sách, thêm, sửa, xóa); năm hành động người thuê (lấy theo phòng, thêm, sửa, trả phòng, xóa); năm hành động hợp đồng (lập, gia hạn, chấm dứt, lấy danh sách, sinh QR); nhận phòng QR/PIN; hai hành động điện nước (chốt số, lấy số kỳ trước); bốn hành động hóa đơn (lập, danh sách, thanh toán, hóa đơn của tôi); bốn hành động báo cáo & lưu trú (tổng quan, xuất tạm trú, lấy lịch sử cư trú, xuất lịch sử cư trú); và hai hành động phân quyền (lấy ma trận, cập nhật quyền).

---

## PHỤ LỤC: BẢNG HÀNH ĐỘNG GIAO THỨC (29 ACTIONS)

| Nhóm | Hành động | Ý nghĩa |
|---|---|---|
| **Xác thực** | `DANG_NHAP` | Đăng nhập tài khoản, tự nhận diện vai trò |
| | `DANG_KY` | Khách thuê tự đăng ký tài khoản từ xa (không cần token) |
| **Phòng** | `PHONG_LAY_TAT_CA` | Lấy danh sách toàn bộ phòng và số người hiện tại |
| | `PHONG_THEM` | Thêm phòng trọ mới |
| | `PHONG_CAP_NHAT` | Sửa thông tin phòng (giá thuê, sức chứa, trạng thái) |
| | `PHONG_XOA` | Xóa phòng trọ (chỉ cho phép khi phòng trống) |
| **Người thuê** | `KHACH_THUE_THEO_PHONG` | Lấy danh sách người thuê theo phòng (hoặc khách chờ nhận phòng) |
| | `KHACH_THUE_THEM` | Thêm hồ sơ người thuê vào phòng |
| | `KHACH_THUE_CAP_NHAT` | Cập nhật thông tin cá nhân người thuê |
| | `KHACH_THUE_TRA_PHONG` | Ghi nhận khách trả phòng, đưa phong_id về NULL |
| | `KHACH_THUE_XOA` | Xóa vĩnh viễn hồ sơ người thuê đã trả phòng |
| **Hợp đồng** | `HOP_DONG_TAO` | Lập hợp đồng mới (trực tiếp hoặc chờ nhận phòng QR) |
| | `HOP_DONG_GIA_HAN` | Gia hạn ngày kết thúc hợp đồng đang hiệu lực |
| | `HOP_DONG_CHAM_DUT` | Chấm dứt hợp đồng (bắt buộc lý do và ngày bàn giao khi còn hạn) |
| | `HOP_DONG_LAY_TAT_CA` | Lấy danh sách toàn bộ hợp đồng |
| | `HOP_DONG_SINH_QR` | Lấy lại mã QR và mã PIN của hợp đồng chờ nhận phòng |
| **Nhận phòng QR** | `KHACH_THUE_NHAN_PHONG_QR` | Khách quét QR / nhập PIN để kích hoạt hợp đồng và nhận phòng |
| **Điện nước** | `DIEN_NUOC_LAY_KY_TRUOC` | Lấy chỉ số điện nước kỳ trước liền kề |
| | `DIEN_NUOC_GHI_SO` | Chốt chỉ số điện nước kỳ hiện tại |
| **Hóa đơn** | `HOA_DON_TAO` | Lập hóa đơn cước tháng cho phòng |
| | `HOA_DON_LAY_TAT_CA` | Lấy danh sách hóa đơn theo kỳ cước hoặc theo phòng |
| | `HOA_DON_THANH_TOAN` | Xác nhận hóa đơn đã thanh toán (DaThu) |
| | `HOA_DON_CUA_TOI` | Khách thuê tra cứu danh sách hóa đơn của mình (có phân trang) |
| **Báo cáo & Tạm trú** | `BAO_CAO_TONG_QUAN` | Lấy số liệu KPI tổng quan cơ sở trọ |
| | `XUAT_HO_SO_TAM_TRU` | Xuất danh sách thông tin người ở phục vụ đăng ký tạm trú |
| | `LICH_SU_CU_TRU_LAY` | Lấy lịch sử biến động cư trú theo thời gian |
| | `XUAT_LICH_SU_CU_TRU` | Xuất file báo cáo lịch sử cư trú phục vụ kiểm tra công an |
| **Phân quyền** | `PHAN_QUYEN_LAY_MA_TRAN` | Lấy ma trận quyền động hiện tại của hệ thống |
| | `PHAN_QUYEN_CAP_NHAT_VAI_TRO` | Chủ trọ cập nhật danh mục quyền cho từng vai trò |

### 10.3. Xử lý đa kết nối

Server sử dụng mô hình "một Client — một luồng": luồng chính lắng nghe và chấp nhận kết nối, mỗi kết nối mới được bàn giao cho một luồng xử lý độc lập vòng đời đến khi Client ngắt kết nối. Tính nhất quán dữ liệu giữa các Client được bảo đảm ở hai tầng: tầng cơ sở dữ liệu với các ràng buộc duy nhất (hai Client cùng thêm một số phòng thì chỉ một lần thành công) và tầng giao dịch với cơ chế khóa của InnoDB khi ghi dữ liệu liên quan nhiều bảng.

## 11. KẾ HOẠCH KIỂM THỬ

Hệ thống được kiểm thử theo các nhóm tình huống chính sau:

**Kiểm thử nghiệp vụ:** thêm phòng trùng số phòng phải bị từ chối; thêm người thứ ba vào phòng sức chứa hai người phải bị từ chối; thêm người trùng CCCD phải bị từ chối; lập hợp đồng thứ hai cho phòng đang còn hiệu lực phải bị từ chối; nhập chỉ số điện mới nhỏ hơn chỉ số cũ phải bị từ chối; lập hóa đơn trùng phòng trùng tháng phải bị từ chối; tổng tiền hóa đơn phải bằng đúng tổng các khoản.

**Kiểm thử luồng nghiệp vụ trọn vẹn:** kịch bản "phòng trống → đưa người vào → lập hợp đồng → chốt điện nước → lập hóa đơn → thanh toán" chạy thông suốt, trạng thái phòng và hóa đơn biến đổi đúng tại từng bước; kịch bản "trả phòng → phòng về trạng thái trống → xóa phòng được".

**Kiểm thử mạng:** hai Client thao tác đồng thời (cùng thêm một phòng) chỉ một lần thành công; tắt Server đột ngột thì Client hiển thị thông báo mất kết nối và không bị sập ứng dụng; thời gian phản hồi mỗi yêu cầu trong mạng LAN không vượt quá 500 ms.

## 12. KẾT LUẬN

Đề tài "Xây dựng hệ thống quản lý phòng trọ phường Ngũ Hành Sơn" vận dụng đầy đủ kiến thức môn Lập Trình Mạng vào một bài toán thực tiễn: kiến trúc Client – Server qua TCP/IP, thiết kế giao thức ứng dụng riêng, xử lý đa luồng phía Server và quản lý tính nhất quán dữ liệu khi nhiều máy truy cập đồng thời. Đồng thời, hệ thống giải quyết trọn vẹn nhu cầu thực tế của chủ trọ: từ quản lý phòng, thành viên, hợp đồng đến chốt điện nước, lập hóa đơn và khai báo tạm trú với công an phường.

Hướng phát triển sau khi hoàn thành phần cốt lõi: gửi thông báo hóa đơn qua Zalo/email cho người thuê, mở rộng vai trò người thuê được tự tra cứu hóa đơn, và thống kê trực quan bằng biểu đồ theo năm.

---

## PHỤ LỤC: BẢNG HÀNH ĐỘNG GIAO THỨC

| Nhóm | Hành động | Ý nghĩa |
|---|---|---|
| Xác thực | LOGIN | Đăng nhập tài khoản chủ trọ |
| Phòng | PHONG_LAY_TAT_CA / PHONG_THEM / PHONG_CAP_NHAT / PHONG_XOA | Lấy danh sách, thêm, sửa, xóa phòng |
| Người thuê | KHACH_THUE_THEO_PHONG / KHACH_THUE_THEM / KHACH_THUE_CAP_NHAT / KHACH_THUE_TRA_PHONG | Lấy thành viên theo phòng, thêm, sửa, trả phòng |
| Hợp đồng | HOP_DONG_TAO / HOP_DONG_CHAM_DUT | Lập hợp đồng, chấm dứt hợp đồng |
| Điện nước | DIEN_NUOC_GHI_SO / DIEN_NUOC_LAY_KY_TRUOC | Chốt chỉ số kỳ, lấy chỉ số kỳ trước |
| Hóa đơn | HOA_DON_TAO / HOA_DON_LAY_TAT_CA / HOA_DON_THANH_TOAN | Lập hóa đơn, danh sách, xác nhận thanh toán |
| Báo cáo | BAO_CAO_TONG_QUAN / XUAT_HO_SO_TAM_TRU | Thống kê tổng hợp, xuất danh sách tạm trú |
