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
| Ứng dụng Client | C# WinForms (Visual Studio) | Giao diện desktop trực quan, phát triển nhanh |
| Ứng dụng Server | C# Console App, thư viện TcpListener | Nằm trong chuẩn .NET, đúng tinh thần lập trình mạng |
| Giao thức trao đổi | TCP/IP, cổng 8888, gói tin văn bản dạng JSON | TCP đảm bảo tin cậy, đúng thứ tự; JSON dễ đọc, dễ mở rộng |
| Cơ sở dữ liệu | MySQL trên Laragon (cổng 3306) | Nhẹ, miễn phí, quen thuộc với nhóm |
| Truy vấn dữ liệu | ADO.NET (MySqlConnector) | Thư viện chuẩn của hệ sinh thái .NET |

## 4. PHẠM VI ĐỀ TÀI

### 4.1. Trong phạm vi

Hệ thống quản lý trọn vẹn vòng đời vận hành một khu trọ: quản lý phòng (thêm, sửa, xóa, theo dõi trạng thái); quản lý người thuê và hồ sơ cá nhân từng thành viên; quản lý hợp đồng thuê từ lúc lập đến khi chấm dứt; ghi chỉ số điện nước hàng tháng và tự động tính tiền; lập hóa đơn tổng hợp và xác nhận thanh toán; thống kê doanh thu – công nợ – công suất phòng; xuất danh sách người thuê phục vụ khai báo tạm trú tại phường. Về mặt mạng, hệ thống triển khai đầy đủ kiến trúc Client – Server qua TCP, hỗ trợ nhiều máy khách đồng thời.

### 4.2. Ngoài phạm vi

Đề tài không bao gồm: thanh toán trực tuyến hay tích hợp cổng thanh toán điện tử; ứng dụng trên điện thoại; gửi thông báo tự động qua email/SMS; hệ thống phân quyền nhiều vai trò phức tạp (hệ thống chỉ phục vụ một vai trò duy nhất là chủ trọ).

## 5. ĐỐI TƯỢNG SỬ DỤNG VÀ KỊCH BẢN VẬN HÀNH

Hệ thống có một nhóm người dùng duy nhất là **chủ trọ**. Chủ trọ vận hành Server trên máy chính (cài đặt Laragon và MySQL) và có thể mở Client trên nhiều máy trong cùng mạng LAN để thao tác.

Kịch bản vận hành điển hình trong một tháng như sau: đầu tháng, chủ trọ lập hợp đồng cho khách mới và đưa thành viên vào phòng. Cuối tháng, chủ trọ ghi chỉ số điện nước từng phòng; hệ thống tự tính số tiêu thụ và thành tiền. Sau đó chủ trọ lập hóa đơn cho từng phòng — hệ thống tự gộp tiền phòng, tiền điện, tiền nước và phí phát sinh (rác, wifi) thành tổng phải thu. Khi khách trả tiền, chủ trọ xác nhận thanh toán; các phòng chưa trả được liệt kê để đôn đốc. Định kỳ, chủ trọ xuất danh sách người đang ở để nộp công an phường Ngũ Hành Sơn theo quy định tạm trú.

## 6. CÁC CHỨC NĂNG CHÍNH CỦA HỆ THỐNG

**Quản lý phòng trọ.** Chủ trọ thêm, sửa, xóa phòng cùng các thông tin số phòng, giá thuê, sức chứa. Hệ thống tự theo dõi trạng thái mỗi phòng (trống, đang thuê, bảo trì) và hiển thị số thành viên hiện tại — số này được đếm trực tiếp từ danh sách người thuê nên luôn khớp với thực tế. Phòng chỉ được xóa khi đang trống.

**Quản lý người thuê.** Mỗi thành viên ở trọ có một hồ sơ gồm họ tên, ngày sinh, CCCD, số điện thoại, quê quán và nơi học tập/làm việc. Chủ trọ gán người thuê vào phòng (có kiểm tra sức chứa), chuyển phòng hoặc ghi nhận trả phòng. Hệ thống còn lưu cờ "đã đăng ký tạm trú" cho từng người, phục vụ công tác quản lý lưu trú của phường.

**Quản lý hợp đồng.** Mỗi phòng đang cho thuê có đúng một hợp đồng còn hiệu lực, ký bởi một người đại diện (bắt buộc là thành viên của phòng). Hợp đồng lưu thời hạn, giá thuê và tiền cọc. Hệ thống cảnh báo các hợp đồng sắp hết hạn trong 30 ngày và hỗ trợ gia hạn hoặc chấm dứt có ghi nhận lý do.

**Quản lý điện, nước.** Hàng tháng, chủ trọ nhập chỉ số đầu kỳ và cuối kỳ cho từng phòng. Chỉ số đầu kỳ được hệ thống tự điền bằng chỉ số cuối kỳ của tháng trước, tránh nhập tay sai sót. Tiền điện và tiền nước do Server tính theo công thức: số tiêu thụ nhân đơn giá, với đơn giá được lưu riêng cho từng kỳ.

**Hóa đơn và thanh toán.** Hóa đơn tháng được lập theo phòng, tổng hợp tiền phòng, tiền điện, tiền nước và phí khác; mỗi phòng chỉ có một hóa đơn cho mỗi tháng. Hóa đơn chuyển từ trạng thái chưa thanh toán sang đã thanh toán kèm mốc thời gian thu tiền, làm cơ sở thống kê công nợ và doanh thu.

**Thống kê và tra cứu.** Hệ thống tổng hợp công suất phòng (tỷ lệ lấp đầy), tổng người đang ở, doanh thu đã thu và chưa thu theo tháng; hỗ trợ tra cứu nhanh theo tên, số phòng, CCCD và xuất danh sách khai báo tạm trú.

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

### 7.7. Epic 7 — Kết nối mạng (trọng tâm môn học)

**US-21 — Kết nối Client đến Server** (Must)
> Là chủ trọ, tôi muốn Client kết nối đến Server theo địa chỉ IP và cổng cấu hình được, để có thể làm việc từ máy khác trong mạng LAN.

Tiêu chí chấp nhận: có màn hình cấu hình IP/cổng, mặc định 127.0.0.1:8888; hiển thị trạng thái kết nối; mất kết nối thì thông báo rõ ràng, ứng dụng không bị treo; mỗi gói tin được kết thúc bằng ký tự xuống dòng để Server đọc trọn vẹn.

**US-22 — Nhiều Client cùng lúc** (Must, phụ thuộc US-21)
> Là chủ trọ, tôi muốn nhiều máy thao tác cùng lúc mà dữ liệu vẫn nhất quán.

Tiêu chí chấp nhận: Server phục vụ nhiều kết nối đồng thời, mỗi kết nối một luồng xử lý; hai Client cùng tạo một phòng thì chỉ một lần thành công; Client khác thấy dữ liệu mới sau khi làm mới.

**US-23 — Đăng nhập chủ trọ** (Could, phụ thuộc US-21)
> Là chủ trọ, tôi muốn đăng nhập bằng tài khoản quản lý để người lạ không mở được ứng dụng.

Tiêu chí chấp nhận: mật khẩu lưu dưới dạng băm; sai quá 5 lần liên tiếp bị tạm khóa một phút.

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
| BR-13 | Các thao tác ghi liên quan nhiều bảng (hợp đồng, hóa đơn) chạy trong một giao dịch (giao_dich) — hoặc thành công toàn bộ, hoặc không ghi gì |

## 9. THIẾT KẾ DỮ LIỆU

Cơ sở dữ liệu `quanly_phongtro_nhs` gồm 6 bảng chính, thiết kế ở dạng chuẩn hóa 3NF, dùng khóa ngoại đảm bảo toàn vẹn và ràng buộc duy nhất chặn trùng lặp ngay từ tầng cơ sở dữ liệu:

**Bảng tai_khoan** — tài khoản quản trị: tên đăng nhập (duy nhất), mật khẩu đã băm, họ tên.

**Bảng phong** — phòng trọ: số phòng (duy nhất), giá thuê, sức chứa tối đa, trạng thái (trống / đang thuê / bảo trì), mô tả.

**Bảng khach_thue** — người thuê: họ tên, ngày sinh, CCCD (duy nhất), số điện thoại, quê quán, nơi học tập/làm việc, cờ đăng ký tạm trú, khóa ngoại về phòng đang ở (cho phép rỗng khi chưa gán phòng).

**Bảng hop_dong** — hợp đồng: khóa ngoại về phòng và về người đại diện, ngày bắt đầu, ngày kết thúc, giá thuê, tiền cọc, trạng thái (còn hiệu lực / hết hạn / chấm dứt), ghi chú.

**Bảng chi_so_dien_nuoc** — chỉ số điện nước: khóa ngoại về phòng, tháng chốt (định dạng năm-tháng), chỉ số điện và nước cũ/mới, đơn giá điện và nước của kỳ. Ràng buộc duy nhất trên cặp (phòng, tháng) chặn ghi trùng kỳ.

**Bảng hoa_don** — hóa đơn: khóa ngoại về phòng và về hợp đồng, tháng lập, các khoản tiền phòng, điện, nước, phí khác, tổng tiền, trạng thái thanh toán, thời điểm thanh toán. Ràng buộc duy nhất trên cặp (phòng, tháng) đảm bảo không có hai hóa đơn cùng kỳ.

## 10. KIẾN TRÚC VÀ GIAO THỨC TRAO ĐỔI

### 10.1. Kiến trúc ba tầng

Hệ thống tổ chức theo kiến trúc ba tầng, tách bạch trách nhiệm giữa giao diện, xử lý nghiệp vụ và lưu trữ:

- **Tầng trình diễn (Client WinForms):** các màn hình quản lý phòng, người thuê, hợp đồng, điện nước, hóa đơn và thống kê. Client không chứa logic nghiệp vụ, chỉ gửi yêu cầu và hiển thị kết quả.
- **Tầng nghiệp vụ (Server):** lắng nghe kết nối qua TcpListener tại cổng 8888. Với mỗi Client, Server cấp phát một luồng xử lý riêng (ClientHandler), nhận gói yêu cầu, định tuyến đến bộ điều khiển tương ứng (phòng, người thuê, hợp đồng, điện nước, hóa đơn). Toàn bộ quy tắc nghiệp vụ nằm tại đây.
- **Tầng dữ liệu (MySQL):** Server truy cập cơ sở dữ liệu qua ADO.NET, tổ chức thành các Repository cho từng nhóm đối tượng.

### 10.2. Giao thức ứng dụng trên TCP

Client và Server trao đổi bằng gói tin văn bản dạng JSON, mỗi gói kết thúc bằng ký tự xuống dòng để phân định ranh giới gói tin trên luồng TCP. Một yêu cầu gồm ba trường: tên hành động (Action), mã phiên đăng nhập (Token) và dữ liệu (Data). Phản hồi của Server gồm ba trường: thành công hay thất bại (Success), thông điệp (Message) và dữ liệu kết quả (Data).

Hệ thống định nghĩa 18 hành động, nhóm theo chức năng: đăng nhập; bốn hành động quản lý phòng (lấy danh sách, thêm, sửa, xóa); bốn hành động quản lý người thuê (lấy theo phòng, thêm, sửa, trả phòng); hai hành động hợp đồng (lập, chấm dứt); hai hành động điện nước (chốt chỉ số, lấy chỉ số kỳ trước); ba hành động hóa đơn (lập, lấy danh sách, xác nhận thanh toán); và hai hành động báo cáo (thống kê tổng hợp, xuất danh sách tạm trú). Danh sách chi tiết được trình bày trong phụ lục kỹ thuật.

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
