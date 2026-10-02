# GIỚI THIỆU ĐỀ TÀI
## XÂY DỰNG HỆ THỐNG QUẢN LÝ PHÒNG TRỌ PHƯỜNG NGŨ HÀNH SƠN

---

## 1. GIỚI THIỆU

Phường Ngũ Hành Sơn (TP. Đà Nẵng) là địa bàn có mật độ phòng trọ cao, phục vụ đông đảo sinh viên các trường Đại học Kiến Trúc – Đà Nẵng, VKU, FPT Đà Nẵng cùng người lao động ngoại tỉnh. Nhu cầu quản lý phòng trọ tại đây không chỉ là ghi chép danh sách người ở, mà là một quy trình vận hành khép kín theo tháng: lập hợp đồng cho khách mới, chốt chỉ số điện nước, lập hóa đơn, thu tiền và định kỳ xuất danh sách khai báo tạm trú cho công an phường.

Đề tài xây dựng một hệ thống quản lý phòng trọ theo mô hình **Client – Server** trên giao thức **TCP/IP**. Máy khách (Client) là ứng dụng desktop WinForms cung cấp giao diện thao tác; máy chủ (Server) là ứng dụng console lắng nghe kết nối, xử lý toàn bộ nghiệp vụ và truy vấn cơ sở dữ liệu MySQL. Client hoàn toàn không truy cập cơ sở dữ liệu trực tiếp — đây chính là yêu cầu trọng tâm của môn Lập Trình Mạng.

Hệ thống hướng đến giải quyết trọn vẹn quy trình nghiệp vụ của một khu trọ, đồng thời thể hiện đầy đủ các nội dung kỹ thuật của môn học: thiết kế giao thức ứng dụng riêng chạy trên TCP, xử lý đa kết nối đồng thời phía Server, và quản lý tính nhất quán dữ liệu khi nhiều máy khách cùng truy cập.

## 2. ĐẶT VẤN ĐỀ

Hiện nay, phần lớn chủ trọ tại phường Ngũ Hành Sơn vẫn quản lý bằng sổ tay hoặc các file Excel rời rạc. Cách làm này bộc lộ nhiều bất cập trong vận hành thực tế:

- **Sai sót chỉ số điện nước.** Chỉ số cuối kỳ của tháng trước không được chuyển tự động thành chỉ số đầu kỳ của tháng sau, dẫn đến nhập tay chồng chéo và tính tiền sai.
- **Thất lạc hợp đồng.** Hợp đồng giấy không có cơ chế cảnh báo hết hạn, chủ trọ thường phát hiện muộn khi khách đã ở quá hạn.
- **Khó tra cứu lịch sử thanh toán.** Thông tin thu tiền nằm rải rác, không đối chiếu được ai còn nợ tháng nào, số tiền bao nhiêu.
- **Mất nhiều thời gian tổng hợp danh sách khai báo tạm trú** cho công an phường theo quy định lưu trú.

Về mặt kỹ thuật, đề tài đặt ra bài toán trọng tâm của môn Lập Trình Mạng: **làm thế nào để nhiều máy khách cùng thao tác trên một cơ sở dữ liệu dùng chung mà dữ liệu vẫn nhất quán**, không xảy ra tranh chấp hay ghi đè lẫn nhau — ví dụ hai máy cùng thêm một số phòng, hoặc cùng lập hợp đồng cho một phòng đang trống.

Từ thực tế đó, nhóm lựa chọn đề tài xây dựng hệ thống quản lý phòng trọ theo mô hình Client – Server, hướng đến giải quyết trọn vẹn quy trình: từ quản lý phòng, thành viên, hợp đồng cho đến chốt điện nước và lập hóa đơn thanh toán.

## 3. MỤC TIÊU ĐỀ TÀI

Hệ thống được xây dựng với các mục tiêu sau:

**Thứ nhất, tập trung hóa dữ liệu.** Tập trung toàn bộ dữ liệu về phòng trọ, người thuê, hợp đồng, điện nước và hóa đơn về một máy chủ duy nhất, đảm bảo dữ liệu thống nhất và an toàn, chấm dứt tình trạng mỗi máy giữ một bản Excel riêng.

**Thứ hai, đúng trọng tâm môn Lập Trình Mạng.** Máy khách (Client) hoàn toàn không truy cập trực tiếp cơ sở dữ liệu. Mọi yêu cầu đều được đóng gói và gửi qua kết nối TCP đến máy chủ (Server); Server xử lý nghiệp vụ, truy vấn cơ sở dữ liệu và trả kết quả về cho Client.

**Thứ ba, phục vụ nhiều Client đồng thời.** Server phải phục vụ được nhiều Client kết nối cùng lúc mà dữ liệu vẫn nhất quán, không xảy ra tranh chấp hay ghi đè lẫn nhau.

**Thứ tư, giải quyết trọn vẹn quy trình nghiệp vụ.** Hệ thống bao phủ vòng đời vận hành một khu trọ từ lúc phòng trống, đưa người vào, lập hợp đồng, chốt điện nước, lập hóa đơn tới khi thu tiền và thanh lý hợp đồng.

## 4. ĐỐI TƯỢNG SỬ DỤNG VÀ PHÂN QUYỀN

Hệ thống phục vụ bốn nhóm người dùng, phản ánh mô hình thực tế: một chủ đầu tư sở hữu nhiều khu trọ ở các khu vực khác nhau trong phường, mỗi khu có thể có người quản lý được phân công, trong khi công an phường cần theo dõi lưu trú nhưng không được can thiệp dữ liệu.

| Vai trò | Mô tả | Phạm vi quyền |
|---|---|---|
| **Chủ trọ** (Owner) | Người sở hữu một hoặc nhiều khu trọ tại các khu vực trong phường Ngũ Hành Sơn | Toàn quyền trên mọi khu: phòng, người thuê, hợp đồng, điện nước, hóa đơn, thống kê; tạo tài khoản và phân công quản lý trọ theo khu |
| **Quản lý trọ** (QuanLy) | Người được chủ trọ ủy quyền vận hành một khu trọ cụ thể | Thao tác đầy đủ (thêm/sửa, chốt chỉ số, lập hóa đơn, xác nhận thu) nhưng **chỉ trong khu được phân công**; không thấy dữ liệu các khu khác; không được phân quyền cho người khác |
| **Công an phường** (Chỉ xem) | Cán bộ phụ trách an ninh trật tự và quản lý lưu trú của phường | **Chỉ xem**: danh sách người thuê, thông tin tạm trú – tạm vắng, tình trạng từng phòng, từng khu; màn hình gần giống chủ trọ/quản lý trọ nhưng mọi hành động ghi đều bị Server từ chối |
| **Khách thuê** (KhachThue) | Người đang ở trong khu trọ | Tra cứu hóa đơn và lịch sử thanh toán của phòng mình |

Server kiểm tra vai trò và phạm vi khu của từng phiên đăng nhập trước khi xử lý mọi yêu cầu (quy tắc BR-14, BR-15).

## 5. CÁC CHỨC NĂNG CHÍNH

**Quản lý phòng trọ.** Chủ trọ thêm, sửa, xóa phòng cùng các thông tin số phòng, giá thuê, sức chứa. Hệ thống tự theo dõi trạng thái mỗi phòng (trống, đang thuê, bảo trì) và hiển thị số thành viên hiện tại — số này được đếm trực tiếp từ danh sách người thuê nên luôn khớp với thực tế. Phòng chỉ được xóa khi đang trống.

**Quản lý người thuê.** Mỗi thành viên ở trọ có một hồ sơ gồm họ tên, ngày sinh, CCCD, số điện thoại, quê quán và nơi học tập/làm việc. Chủ trọ gán người thuê vào phòng (có kiểm tra sức chứa), chuyển phòng hoặc ghi nhận trả phòng. Hệ thống còn lưu cờ "đã đăng ký tạm trú" cho từng người, phục vụ công tác quản lý lưu trú của phường.

**Quản lý hợp đồng.** Mỗi phòng đang cho thuê có đúng một hợp đồng còn hiệu lực, ký bởi một người đại diện (bắt buộc là thành viên của phòng). Hợp đồng lưu thời hạn, giá thuê và tiền cọc. Hệ thống cảnh báo các hợp đồng sắp hết hạn trong 30 ngày và hỗ trợ gia hạn hoặc chấm dứt có ghi nhận lý do.

**Quản lý điện, nước.** Hàng tháng, chủ trọ nhập chỉ số đầu kỳ và cuối kỳ cho từng phòng. Chỉ số đầu kỳ được hệ thống tự điền bằng chỉ số cuối kỳ của tháng trước, tránh nhập tay sai sót. Tiền điện và tiền nước do Server tính theo công thức: số tiêu thụ nhân đơn giá, với đơn giá được lưu riêng cho từng kỳ.

**Hóa đơn và thanh toán.** Hóa đơn tháng được lập theo phòng, tổng hợp tiền phòng, tiền điện, tiền nước và phí khác; mỗi phòng chỉ có một hóa đơn cho mỗi tháng. Hóa đơn chuyển từ trạng thái chưa thanh toán sang đã thanh toán kèm mốc thời gian thu tiền, làm cơ sở thống kê công nợ và doanh thu.

**Thống kê và tra cứu.** Hệ thống tổng hợp công suất phòng (tỷ lệ lấp đầy), tổng người đang ở, doanh thu đã thu và chưa thu theo tháng; hỗ trợ tra cứu nhanh theo tên, số phòng, CCCD và xuất danh sách khai báo tạm trú.

**Kết nối mạng.** Client kết nối tới Server theo địa chỉ IP và cổng đọc từ cấu hình (mặc định `127.0.0.1:8888`), hiển thị trạng thái kết nối, thông báo rõ ràng khi mất kết nối mà không làm treo ứng dụng.

**Đăng nhập và phân quyền.** Người dùng đăng nhập bằng tài khoản được cấp; hệ thống xác định vai trò và phạm vi khu trọ của phiên làm việc. Mật khẩu lưu dưới dạng băm; sai quá 5 lần liên tiếp thì bị tạm khóa một phút.

**Quản lý nhiều khu trọ.** Chủ trọ khai báo từng khu trọ (tên khu, địa chỉ trong phường) và gán phòng vào khu. Bảng tổng quan hợp nhất số liệu toàn bộ các khu, đồng thời lọc xem riêng từng khu. Chủ trọ tạo tài khoản quản lý trọ và phân công phụ trách theo khu.

**Tra cứu phục vụ công an phường.** Vai công an phường mở màn hình tra cứu người thuê và tình trạng lưu trú của mọi khu trong phường, xem danh sách tạm trú – tạm vắng và xuất được danh sách phục vụ kiểm tra, nhưng không có bất kỳ thao tác ghi nào — Server từ chối mọi yêu cầu thay đổi dữ liệu từ vai này.

## 6. ĐIỂM MỚI CỦA ĐỀ TÀI

**Giao thức ứng dụng tự thiết kế trên TCP.** Hệ thống không dùng sẵn một giao thức tầng ứng dụng nào mà tự định nghĩa gói tin văn bản dạng JSON: mỗi gói kết thúc bằng ký tự xuống dòng để phân định ranh giới gói trên luồng TCP. Yêu cầu gồm ba trường `Action` – `Token` – `Data`; phản hồi gồm ba trường `Success` – `Message` – `Data`. Cách làm này giải quyết đúng bài toán đóng gói và phân đoạn thông điệp của môn học, đồng thời dễ đọc và dễ mở rộng hành động mới.

**Nghiệp vụ thực thi hoàn toàn ở Server.** Client chỉ gửi yêu cầu và hiển thị kết quả, không chứa logic nghiệp vụ. Mọi quy tắc ràng buộc đều do Server kiểm tra trước khi ghi dữ liệu — Client không được phép quyết định tính hợp lệ của nghiệp vụ. Điều này khiến hệ thống an toàn trước việc sửa đổi Client và đúng với kiến trúc ba tầng.

**Chống tranh chấp dữ liệu ở hai tầng.** Tính nhất quán khi nhiều Client thao tác đồng thời được bảo đảm bằng: (a) ràng buộc duy nhất ở tầng cơ sở dữ liệu — hai Client cùng thêm một số phòng thì chỉ một lần thành công; (b) giao dịch (giao_dich) với cơ chế khóa của InnoDB khi thao tác ghi liên quan nhiều bảng — hoặc thành công toàn bộ, hoặc không ghi gì.

**Mô hình một Client — một luồng.** Luồng chính lắng nghe và chấp nhận kết nối; mỗi kết nối mới được bàn giao cho một luồng xử lý độc lập, tồn tại suốt vòng đời của Client đó. Server phục vụ nhiều Client thật sự song song thay vì xử lý tuần tự.

**Tự động hóa chuỗi tính toán.** Chỉ số đầu kỳ tự lấy bằng chỉ số cuối kỳ tháng trước; tiền điện, tiền nước và tổng tiền hóa đơn do Server tự tính từ số tiêu thụ và đơn giá của kỳ. Việc tính toán được đẩy về phía Server để tránh sai sót tính tay và tránh mỗi Client tính một kết quả.

**Kiểm soát truy cập.** Mật khẩu không lưu dạng văn bản thuần mà lưu dưới dạng băm; cơ chế tạm khóa một phút sau năm lần đăng nhập sai liên tiếp chặn được hành vi dò mật khẩu.

**Phân quyền theo vai và theo khu.** Hệ thống tách vai trò (chủ trọ, quản lý trọ, công an phường, khách thuê) khỏi phạm vi dữ liệu (khu trọ được phép truy cập). Việc kiểm tra được thực thi ở Server chứ không ở giao diện, nên một Client bị sửa đổi cũng không vượt được quyền: quản lý trọ không đọc được khu khác, công an phường không ghi được bất kỳ dữ liệu nào.

**Mô hình dữ liệu nhiều khu.** Thực thể khu trọ đứng trên phòng, cho phép một chủ đầu tư quản lý tập trung nhiều cơ sở ở các khu vực khác nhau trong phường, đồng thời vẫn tách bạch số liệu doanh thu và công suất theo từng khu.

## 7. YÊU CẦU HỆ THỐNG

### 7.1. Yêu cầu chức năng

| Mã | Tên yêu cầu | Mô tả | Vai trò | Ưu tiên |
|---|---|---|---|---|
| FR-01 | Thêm phòng mới | Thêm phòng với số phòng, giá thuê, sức chứa; phòng mới mặc định trạng thái trống; số phòng không trùng; giá thuê và sức chứa là số dương | Chủ trọ | Bắt buộc |
| FR-02 | Sửa hoặc xóa phòng | Sửa giá thuê, sức chứa, trạng thái; chỉ xóa được phòng trống (không có người thuê, không có hợp đồng còn hiệu lực); thao tác xóa phải xác nhận | Chủ trọ | Bắt buộc |
| FR-03 | Xem danh sách và trạng thái phòng | Lưới hiển thị số phòng, giá, sức chứa, số người hiện tại, trạng thái; số người hiện tại đếm từ danh sách người thuê; lọc theo trạng thái | Chủ trọ | Bắt buộc |
| FR-04 | Màn hình tổng quan | Hiển thị tổng phòng, phòng trống, phòng đang thuê, tổng người ở, tổng đã thu và chưa thu của tháng hiện tại | Chủ trọ | Nên có |
| FR-05 | Thêm người thuê và gán vào phòng | Hồ sơ gồm họ tên, ngày sinh, CCCD, SĐT, quê quán, nơi học tập/làm việc; CCCD bắt buộc và không trùng; chỉ gán được vào phòng còn chỗ; phòng có người thì trạng thái tự chuyển thành đang thuê | Chủ trọ | Bắt buộc |
| FR-06 | Sửa hoặc xóa hồ sơ người thuê | Sửa toàn bộ thông tin cá nhân; chỉ xóa được người đã trả phòng | Chủ trọ | Bắt buộc |
| FR-07 | Chuyển phòng, trả phòng | Chuyển phòng chỉ chấp nhận khi phòng đích còn chỗ; trả phòng thì bỏ liên kết phòng, nếu phòng trống hẳn thì trạng thái về "trống" | Chủ trọ | Bắt buộc |
| FR-08 | Xuất danh sách khai báo tạm trú | Xuất file CSV/Excel gồm họ tên, ngày sinh, CCCD, quê quán, số phòng; chỉ gồm người đang ở | Chủ trọ | Có thể có |
| FR-09 | Lập hợp đồng thuê | Chọn phòng đang trống và người đại diện thuộc phòng đó; nhập ngày bắt đầu, ngày kết thúc, giá thuê, tiền cọc; ngày kết thúc phải sau ngày bắt đầu; một phòng chỉ có một hợp đồng còn hiệu lực; lập xong phòng tự chuyển sang đang thuê | Chủ trọ | Bắt buộc |
| FR-10 | Gia hạn hoặc chấm dứt hợp đồng | Gia hạn cập nhật ngày kết thúc mới; chấm dứt lưu lý do và ghi chú hoàn trả cọc; chỉ thao tác trên hợp đồng còn hiệu lực | Chủ trọ | Bắt buộc |
| FR-11 | Theo dõi hợp đồng sắp hết hạn | Danh sách gồm phòng, người đại diện, ngày hết hạn, số ngày còn lại; sắp xếp theo ngày hết hạn tăng dần | Chủ trọ | Nên có |
| FR-12 | Nhập chỉ số điện nước hàng tháng | Chọn phòng và tháng chốt; chỉ số cũ tự lấy bằng chỉ số mới của kỳ trước nếu có; từ chối nếu chỉ số mới nhỏ hơn chỉ số cũ; đơn giá điện, nước lưu riêng theo từng kỳ | Chủ trọ | Bắt buộc |
| FR-13 | Tự động tính tiền điện nước | Tiền điện bằng số tiêu thụ nhân đơn giá điện, tiền nước tương tự; hiển thị số tiêu thụ và thành tiền ngay trước khi lưu; phép tính do Server thực hiện | Server | Bắt buộc |
| FR-14 | Lập hóa đơn tháng | Tự gom các khoản theo hợp đồng và số liệu điện nước của tháng; tổng tiền là tổng các khoản; mỗi phòng chỉ một hóa đơn mỗi tháng, trùng thì từ chối; phòng chưa chốt điện nước không thể lập hóa đơn | Chủ trọ | Bắt buộc |
| FR-15 | Xem chi tiết và lịch sử hóa đơn | Danh sách hóa đơn kèm trạng thái đã/chưa thanh toán; xem chi tiết từng khoản kèm chỉ số và đơn giá | Chủ trọ | Bắt buộc |
| FR-16 | Xác nhận thanh toán | Chuyển trạng thái và lưu thời gian xác nhận; hóa đơn đã thanh toán không được sửa hay xóa | Chủ trọ | Bắt buộc |
| FR-17 | Danh sách phòng còn nợ | Liệt kê phòng, người đại diện, tháng, số tiền còn nợ; lọc theo tháng | Chủ trọ | Nên có |
| FR-18 | Thống kê hiện trạng | Tỷ lệ lấp đầy và tổng người đang ở; số liệu tính trực tiếp từ cơ sở dữ liệu; hiển thị số liệu và biểu đồ đơn giản | Chủ trọ | Nên có |
| FR-19 | Thống kê doanh thu theo tháng | Chọn khoảng tháng; bảng thống kê tổng thu và chưa thu, có dòng tổng cộng | Chủ trọ | Nên có |
| FR-20 | Tra cứu nhanh | Tìm kiếm theo tên, số phòng, CCCD; xử lý ở Server; không phân biệt chữ hoa chữ thường; hỗ trợ tìm một phần CCCD và số điện thoại | Chủ trọ | Nên có |
| FR-21 | Kết nối Client đến Server | Kết nối tới IP/cổng đọc từ cấu hình (mặc định `127.0.0.1:8888`); hiển thị trạng thái kết nối; mất kết nối thì thông báo rõ ràng, ứng dụng không bị treo; mỗi gói tin kết thúc bằng ký tự xuống dòng | Chủ trọ | Bắt buộc |
| FR-22 | Nhiều Client cùng lúc | Server phục vụ nhiều kết nối đồng thời, mỗi kết nối một luồng xử lý; hai Client cùng tạo một phòng thì chỉ một lần thành công; Client khác thấy dữ liệu mới sau khi làm mới | Server | Bắt buộc |
| FR-23 | Đăng nhập và phân quyền | Đăng nhập bằng tài khoản được cấp; hệ thống xác định vai trò và phạm vi khu trọ của phiên; mật khẩu lưu dạng băm; sai quá 5 lần liên tiếp bị tạm khóa một phút | Tất cả vai trò | Bắt buộc |
| FR-24 | Quản lý khu trọ | Thêm, sửa, xóa khu trọ (tên khu, địa chỉ trong phường); gán phòng vào khu; chỉ xóa được khu không còn phòng | Chủ trọ | Bắt buộc |
| FR-25 | Phân công quản lý trọ theo khu | Chủ trọ tạo tài khoản quản lý trọ và gán phụ trách một hoặc nhiều khu; thu hồi phân công khi cần | Chủ trọ | Bắt buộc |
| FR-26 | Giới hạn phạm vi dữ liệu theo khu | Quản lý trọ chỉ đọc và ghi được dữ liệu thuộc khu được phân công; mọi yêu cầu vượt phạm vi bị Server từ chối | Server | Bắt buộc |
| FR-27 | Tổng quan hợp nhất nhiều khu | Bảng tổng quan gộp số liệu toàn bộ khu của chủ trọ; lọc xem riêng từng khu; so sánh công suất và doanh thu giữa các khu | Chủ trọ | Nên có |
| FR-28 | Tra cứu lưu trú cho công an phường | Xem danh sách người thuê, tình trạng phòng và thông tin tạm trú – tạm vắng của mọi khu trong phường; tra cứu theo tên, CCCD, số phòng; xuất danh sách phục vụ kiểm tra | Công an phường | Bắt buộc |
| FR-29 | Chặn thao tác ghi của vai chỉ xem | Mọi yêu cầu thêm, sửa, xóa từ vai công an phường bị Server từ chối kèm thông báo rõ ràng | Server | Bắt buộc |
| FR-30 | Khách thuê tra cứu hóa đơn của mình | Khách thuê đăng nhập bằng CCCD, chỉ xem được hóa đơn và lịch sử thanh toán của phòng mình | Khách thuê | Nên có |

### 7.2. Yêu cầu phi chức năng

| Mã | Nhóm | Yêu cầu | Tiêu chí đo lường |
|---|---|---|---|
| NFR-01 | Hiệu năng | Thời gian phản hồi yêu cầu trong mạng LAN ở mức chấp nhận được | Mỗi yêu cầu không vượt quá 500 ms |
| NFR-02 | Hiệu năng | Server phục vụ nhiều Client cùng lúc | Mỗi Client một luồng xử lý độc lập; không Client nào phải chờ Client khác hoàn tất |
| NFR-03 | Tin cậy | Gói tin trên luồng TCP được phân định rõ ràng | Mỗi gói kết thúc bằng ký tự xuống dòng; Server đọc trọn vẹn gói |
| NFR-04 | Tin cậy | Thao tác ghi liên quan nhiều bảng không để dữ liệu nửa vời | Chạy trong một giao dịch: hoặc thành công toàn bộ, hoặc không ghi gì |
| NFR-05 | Nhất quán | Nhiều Client ghi cùng dữ liệu không ghi đè nhau | Ràng buộc duy nhất ở cơ sở dữ liệu + khóa khi ghi; hai Client cùng thêm một số phòng chỉ một lần thành công |
| NFR-06 | Bảo mật | Mật khẩu không lưu dạng văn bản thuần | Mật khẩu lưu dưới dạng băm |
| NFR-07 | Bảo mật | Chặn dò mật khẩu | Sai quá 5 lần liên tiếp thì tạm khóa đăng nhập một phút |
| NFR-08 | Khả dụng | Client không treo khi mất kết nối | Tắt Server đột ngột: Client hiển thị thông báo mất kết nối, ứng dụng không bị sập |
| NFR-09 | Bảo trì | Tách bạch trách nhiệm giữa các tầng | Kiến trúc ba tầng: trình diễn (Client) – nghiệp vụ (Server) – dữ liệu (MySQL); Client không chứa logic nghiệp vụ |
| NFR-10 | Bảo mật | Phân quyền không thể vượt qua từ phía Client | Mọi yêu cầu đều bị Server kiểm tra vai trò và phạm vi khu trước khi xử lý; sửa đổi Client cũng không đọc/ghi được ngoài quyền |

## 8. MA TRẬN CHỨC NĂNG

Ký hiệu: **●** đảm nhiệm chính · **○** tham gia · **X** chỉ được xem, không được ghi · **–** không tham gia

| Chức năng | Chủ trọ | Quản lý trọ | Công an phường | Khách thuê | Server | Cơ sở dữ liệu |
|---|---|---|---|---|---|---|
| Quản lý khu trọ | ● | X khu được phân công | X | – | ● kiểm tra phạm vi | ○ danh mục khu |
| Quản lý phòng (thêm/sửa/xóa/xem) | ● | ● trong khu mình | X | – | ● kiểm tra quy tắc, định tuyến | ○ ràng buộc duy nhất số phòng trong khu |
| Quản lý người thuê | ● | ● trong khu mình | X | – | ● kiểm tra sức chứa | ○ ràng buộc duy nhất CCCD |
| Quản lý hợp đồng | ● | ● trong khu mình | X | – | ● kiểm tra hiệu lực, giao dịch | ○ lưu vết, khóa ngoại |
| Quản lý điện nước | ● | ● trong khu mình | X chỉ số | – | ● tự điền chỉ số cũ, tính tiền | ○ ràng buộc duy nhất (phòng, tháng) |
| Hóa đơn và thanh toán | ● | ● trong khu mình | – | X hóa đơn của mình | ● gom khoản, tính tổng | ○ ràng buộc duy nhất (phòng, tháng) |
| Thống kê và tra cứu | ● toàn bộ khu | ● khu mình | X lưu trú | – | ● tổng hợp, lọc theo quyền | ○ truy vấn tổng hợp |
| Phân công quản lý trọ theo khu | ● | – | – | – | ● kiểm tra quyền chủ trọ | ○ bảng phân công |
| Tra cứu tạm trú – tạm vắng | ● | ● khu mình | X | – | ● lọc theo quyền | ○ truy vấn người đang ở |
| Kết nối mạng | ● cấu hình đọc từ file | ● | ● | ● | ● lắng nghe TCP, cấp luồng riêng | – |
| Đăng nhập và phân quyền | ● | ● | ● | ● bằng CCCD | ● kiểm tra băm, vai trò, phạm vi khu, tạm khóa | ○ tài khoản băm, vai trò, phân công |
| Giao diện và trạng thái | ● thao tác, theo dõi | ● | X | X | ○ trả thông điệp lỗi tiếng Việt | – |

### 8.1. Ma trận truy vết yêu cầu chức năng

| Mã yêu cầu | User Story | Epic |
|---|---|---|
| FR-01 | US-01 | Quản lý phòng trọ |
| FR-02 | US-02 | Quản lý phòng trọ |
| FR-03 | US-03 | Quản lý phòng trọ |
| FR-04 | US-04 | Quản lý phòng trọ |
| FR-05 | US-05 | Quản lý người thuê |
| FR-06 | US-06 | Quản lý người thuê |
| FR-07 | US-07 | Quản lý người thuê |
| FR-08 | US-08 | Quản lý người thuê |
| FR-09 | US-09 | Quản lý hợp đồng |
| FR-10 | US-10 | Quản lý hợp đồng |
| FR-11 | US-11 | Quản lý hợp đồng |
| FR-12 | US-12 | Quản lý điện, nước |
| FR-13 | US-13 | Quản lý điện, nước |
| FR-14 | US-14 | Hóa đơn và thanh toán |
| FR-15 | US-15 | Hóa đơn và thanh toán |
| FR-16 | US-16 | Hóa đơn và thanh toán |
| FR-17 | US-17 | Hóa đơn và thanh toán |
| FR-18 | US-18 | Thống kê và tra cứu |
| FR-19 | US-19 | Thống kê và tra cứu |
| FR-20 | US-20 | Thống kê và tra cứu |
| FR-21 | US-21 | Kết nối mạng |
| FR-22 | US-22 | Kết nối mạng |
| FR-23 | US-23 | Kết nối mạng |
| FR-24 | US-24 | Quản lý nhiều khu trọ |
| FR-25 | US-25 | Quản lý nhiều khu trọ |
| FR-26 | US-26 | Phân quyền theo vai và theo khu |
| FR-27 | US-27 | Quản lý nhiều khu trọ |
| FR-28 | US-28 | Tra cứu lưu trú cho công an phường |
| FR-29 | US-29 | Phân quyền theo vai và theo khu |
| FR-30 | US-30 | Khách thuê tra cứu hóa đơn |

### 8.2. Ma trận quy tắc nghiệp vụ

Mọi quy tắc dưới đây **đều được Server kiểm tra và thực thi** trước khi ghi dữ liệu — Client không được phép quyết định tính hợp lệ của nghiệp vụ.

| Mã | Quy tắc | Yêu cầu liên quan |
|---|---|---|
| BR-01 | Số phòng là duy nhất trong khu trọ chứa nó (không cần duy nhất toàn hệ thống khi có nhiều khu) | FR-01 |
| BR-02 | Số thành viên một phòng không vượt quá sức chứa của phòng | FR-05, FR-07 |
| BR-03 | CCCD của người thuê là duy nhất trong toàn hệ thống | FR-05 |
| BR-04 | Một phòng chỉ có tối đa một hợp đồng còn hiệu lực tại một thời điểm | FR-09 |
| BR-05 | Người đại diện ký hợp đồng phải là thành viên của chính phòng đó | FR-09 |
| BR-06 | Ngày kết thúc hợp đồng phải sau ngày bắt đầu | FR-09, FR-10 |
| BR-07 | Chỉ số mới của điện và nước không được nhỏ hơn chỉ số cũ | FR-12 |
| BR-08 | Mỗi phòng chỉ có một bản ghi điện nước và một hóa đơn cho mỗi tháng | FR-12, FR-14 |
| BR-09 | Phòng phải có hợp đồng còn hiệu lực và đã chốt điện nước thì mới được lập hóa đơn | FR-14 |
| BR-10 | Tổng tiền hóa đơn bằng tiền phòng cộng tiền điện, tiền nước và phí khác | FR-14 |
| BR-11 | Hóa đơn đã thanh toán không được sửa đổi hay xóa | FR-16 |
| BR-12 | Chỉ xóa được phòng khi phòng đang trống | FR-02 |
| BR-13 | Các thao tác ghi liên quan nhiều bảng chạy trong một giao dịch — hoặc thành công toàn bộ, hoặc không ghi gì | FR-09, FR-14 |
| BR-14 | Mỗi bản ghi (phòng, hợp đồng, hóa đơn, chỉ số) thuộc đúng một khu trọ; số phòng chỉ cần duy nhất trong khu, không cần duy nhất toàn hệ thống | FR-01, FR-24 |
| BR-15 | Quản lý trọ chỉ đọc và ghi được dữ liệu thuộc khu được phân công; mọi yêu cầu vượt phạm vi khu bị từ chối ở Server | FR-26 |
| BR-16 | Vai công an phường chỉ được đọc; mọi yêu cầu ghi từ vai này bị từ chối ở Server | FR-28, FR-29 |
| BR-17 | Chỉ xóa được khu trọ khi khu không còn phòng nào | FR-24 |
| BR-18 | Khách thuê chỉ đọc được hóa đơn gắn với phòng mình đang ở | FR-30 |

---

## PHỤ LỤC A: BẢNG HÀNH ĐỘNG GIAO THỨC

| Nhóm | Hành động | Ý nghĩa |
|---|---|---|
| Xác thực | LOGIN | Đăng nhập tài khoản; trả về vai trò và phạm vi khu được phép truy cập |
| Khu trọ | AREA_GET_ALL / AREA_ADD / AREA_UPDATE / AREA_DELETE | Lấy danh sách, thêm, sửa, xóa khu trọ |
| Phân công | ASSIGN_GET / ASSIGN_SET | Lấy và gán quản lý trọ phụ trách khu |
| Phòng | PHONG_LAY_TAT_CA / PHONG_THEM / PHONG_CAP_NHAT / PHONG_XOA | Lấy danh sách, thêm, sửa, xóa phòng |
| Người thuê | KHACH_THUE_THEO_PHONG / KHACH_THUE_THEM / KHACH_THUE_CAP_NHAT / KHACH_THUE_TRA_PHONG | Lấy thành viên theo phòng, thêm, sửa, trả phòng |
| Hợp đồng | HOP_DONG_TAO / HOP_DONG_CHAM_DUT | Lập hợp đồng, chấm dứt hợp đồng |
| Điện nước | DIEN_NUOC_GHI_SO / DIEN_NUOC_LAY_KY_TRUOC | Chốt chỉ số kỳ, lấy chỉ số kỳ trước |
| Hóa đơn | HOA_DON_TAO / HOA_DON_LAY_TAT_CA / HOA_DON_THANH_TOAN | Lập hóa đơn, danh sách, xác nhận thanh toán |
| Báo cáo | BAO_CAO_TONG_QUAN / XUAT_HO_SO_TAM_TRU | Thống kê tổng hợp, xuất danh sách tạm trú |

---

## PHỤ LỤC B: MÔ HÌNH DỮ LIỆU MỞ RỘNG

Bổ sung hai bảng so với mô hình cốt lõi (tổng cộng 8 bảng):

**Bảng areas — khu trọ:** tên khu (duy nhất trong hệ thống), địa chỉ trong phường, ghi chú. Mỗi phòng `phong` có khóa ngoại về khu; BR-01 điều chỉnh thành "số phòng duy nhất trong khu".

**Bảng assignments — phân công quản lý:** khóa ngoại về tài khoản quản lý trọ và về khu, ghi nhận quản lý trọ phụ trách những khu nào. Vai và quyền của chủ trọ không phụ thuộc bảng này.

Vai trò người dùng lưu trên bảng `tai_khoan` mở rộng trường `vai_tro` (Owner / QuanLy / CongAn / KhachThue); khách thuê dùng tài khoản gắn với hồ sơ người thuê tương ứng.

---

