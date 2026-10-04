# HỆ THỐNG QUẢN LÝ PHÒNG TRỌ PHƯỜNG NGŨ HÀNH SƠN — MÔ TẢ CHỨC NĂNG VÀ GIAO DIỆN
---

## 1. Ai sử dụng hệ thống

| Người dùng | Vai trò trong thực tế | Quyền trong hệ thống |
|---|---|---|
| Chủ trọ | Người sở hữu một cơ sở phòng trọ | Toàn quyền trên toàn hệ thống: phòng, người thuê, hợp đồng, điện nước, hóa đơn, thống kê; **quản lý ma trận phân quyền động** cho các vai trò khác |
| Quản lý trọ | Người được chủ trọ ủy quyền vận hành hàng ngày | Thực hiện các nghiệp vụ vận hành theo danh mục quyền do Chủ trọ cấp (thêm/sửa phòng, người thuê, chốt điện nước, lập hóa đơn, thu tiền, xem QR/PIN nhận phòng) |
| Công an phường | Cán bộ theo dõi lưu trú, tạm trú | Màn hình riêng ở chế độ **chỉ xem**: tra cứu công dân, tình trạng phòng, tạm trú và lịch sử biến động cư trú; xuất được file phục vụ kiểm tra nhưng không sửa được bất cứ gì (máy chủ chặn mọi thao tác ghi — quy tắc BR-16) |
| Khách thuê | Người đang ở trọ hoặc chuẩn bị nhận phòng | Tự đăng ký tài khoản từ xa; tự nhận phòng bằng mã QR / mã PIN 8 số; xem hóa đơn, bảng kê cước tháng này và lịch sử đóng tiền của phòng mình |

---

## 2. Danh sách màn hình

### 2.1. Màn hình đăng nhập và đăng ký

Màn hình đầu tiên khi mở phần mềm, dùng chung cho mọi vai trò. Người dùng đăng nhập bằng tài khoản được cấp — khách thuê đăng nhập bằng số căn cước công dân của mình.

Sau khi đăng nhập, hệ thống tự nhận biết người đó là chủ trọ, quản lý, công an phường hay khách thuê, và chỉ mở đúng giao diện người đó được phép dùng. Nhập sai mật khẩu quá 5 lần liên tiếp, tài khoản bị tạm khóa 1 phút để chống dò mật khẩu.

Cùng màn hình này có **tab "Đăng ký" dành cho khách thuê**: khách tự nhập họ tên, ngày sinh, CCCD 12 số, số điện thoại, quê quán, nơi học tập/làm việc và mật khẩu (tối thiểu 6 ký tự). Sau khi đăng ký thành công, hệ thống chuyển sang tab đăng nhập và điền sẵn CCCD để khách đăng nhập. Tài khoản mới tạo ở trạng thái **chờ nhận phòng** (chưa gán phòng nào).

### 2.2. Thanh trạng thái kết nối

Một dải trạng thái nằm ở cuối màn hình, luôn cho biết máy đang kết nối tốt với máy chủ hay đã mất kết nối. Địa chỉ máy chủ do kỹ thuật cài đặt sẵn từ đầu, người dùng không cần nhập gì. Khi mất kết nối, phần mềm hiện thông báo rõ ràng và không bị treo, kết nối lại là dùng tiếp.

### 2.3. Màn hình tổng quan

Dành cho: chủ trọ, quản lý trọ.

Vừa đăng nhập, chủ trọ nhìn thấy ngay bức tranh toàn cảnh việc kinh doanh: tổng số phòng, bao nhiêu phòng trống, bao nhiêu phòng đang cho thuê, tổng số người đang ở, tháng này đã thu được bao nhiêu và còn nợ bao nhiêu.

Bên dưới là hai danh sách cần hành động ngay: các phòng còn nợ tiền cần đôn đốc, và các hợp đồng sắp hết hạn trong 30 ngày.

### 2.4. Màn hình phòng

Dành cho: chủ trọ, quản lý trọ (công an phường chỉ xem trong tab Lưu trú).

Danh sách toàn bộ phòng với số phòng, giá thuê, sức chứa, số người đang ở thực tế và trạng thái (trống, đang thuê, bảo trì). Số người đang ở được đếm trực tiếp từ danh sách người thuê nên luôn đúng, không phải nhập tay.

Chủ trọ thêm phòng mới, sửa giá hoặc sức chứa, lọc xem riêng phòng trống để chào khách. Phòng đang có người ở hoặc còn hợp đồng thì hệ thống không cho xóa, và mỗi lần xóa đều hỏi xác nhận trước.

**Cảnh báo treo hợp đồng:** nếu phòng đang có người ở nhưng không còn hợp đồng hiệu lực nào (do vừa bị chấm dứt, hoặc khách vào ở chưa ký hợp đồng), phòng đó được gắn thẻ màu cam **"Treo HĐ (N người)"** thay cho trạng thái "Đang thuê", giúp chủ trọ nhận biết ngay và kịp xử lý.

### 2.5. Màn hình người thuê

Dành cho: chủ trọ, quản lý trọ (công an phường chỉ xem trong tab Lưu trú).

Hồ sơ mỗi người ở gồm họ tên, ngày sinh, căn cước công dân, số điện thoại, quê quán, nơi học tập hoặc làm việc. Mỗi số căn cước chỉ tồn tại một lần trong toàn hệ thống nên không bị trùng người.

Chủ trọ thêm người mới vào phòng còn chỗ trống — phòng vừa có người sẽ tự chuyển sang trạng thái đang thuê. Khi khách chuyển phòng, hệ thống kiểm tra phòng mới còn chỗ mới cho chuyển; khi khách trả phòng, liên kết được gỡ và phòng hết người tự trở về trạng thái trống.

Khi lập hợp đồng, chủ trọ có thể chọn cả **khách đã tự đăng ký nhưng chưa được gán phòng** làm người đại diện, để mở đường cho quy trình bàn giao phòng từ xa (xem mục 2.8).

Mỗi hồ sơ có ghi chú đã đăng ký tạm trú hay chưa. Cuối kỳ, chủ trọ bấm một nút để xuất danh sách người đang ở ra file, nộp cho công an phường theo quy định.

### 2.6. Màn hình hợp đồng

Dành cho: chủ trọ, quản lý trọ (công an phường không tham gia).

Mỗi phòng chỉ có tối đa một hợp đồng còn hiệu lực tại một thời điểm, do một người đại diện đứng tên. Hợp đồng ghi ngày bắt đầu, ngày kết thúc, giá thuê và tiền cọc.

Hệ thống liệt kê tất cả hợp đồng kèm số ngày còn lại, sắp xếp hợp đồng gần hết hạn lên đầu và tô nổi bật những hợp đồng còn dưới 30 ngày để chủ trọ chủ động liên hệ tái ký. Chủ trọ gia hạn bằng cách cập nhật ngày kết thúc mới.

**Chấm dứt hợp đồng** tuân thủ Luật Nhà ở 2023:
- Chấm dứt được cả hợp đồng đang hiệu lực lẫn hợp đồng đang **chờ nhận phòng** (khách đã đăng ký nhưng chưa quét QR).
- Nếu hợp đồng **còn thời hạn**, hệ thống bắt buộc nhập **lý do chấm dứt** hợp pháp (theo Điều 172 Luật Nhà ở 2023: nợ tiền thuê từ 03 tháng trở lên, dùng sai mục đích, tự ý cơi nới cải tạo, cho thuê lại không phép, gây mất trật tự đã lập biên bản đến lần thứ ba...).
- Hệ thống gợi ý **ngày bàn giao dự kiến** tối thiểu 30 ngày kể từ hôm nay (đúng quy định phải thông báo trước ít nhất 30 ngày).
- Ngày thông báo, ngày bàn giao dự kiến và lý do được lưu vết vào ghi chú hợp đồng.
- **Hệ thống không tự ý đuổi khách**: khách vẫn được giữ trong phòng cho tới khi chủ trọ thực hiện thủ tục "Trả phòng" ở tab Người thuê.

### 2.7. Màn hình bàn giao phòng (QR / PIN)

Dành cho: chủ trọ, quản lý trọ.

Trong quy trình bàn giao phòng không tiếp xúc, chủ trọ lập hợp đồng ở trạng thái **"Chờ nhận phòng"** thay vì kích hoạt ngay. Khi đó hệ thống tự sinh:
- **Mã QR bảo mật 128-bit** sinh bằng thuật toán ngẫu nhiên có kiểm soát (CSPRNG).
- **Mã PIN 8 chữ số** ngẫu nhiên phục vụ nhập tay khi không có máy quét.

Chủ trọ bấm nút **"Xem QR / PIN…"** trên thanh công cụ để xem mã QR, mã PIN, **tải ảnh QR về máy** và **in phiếu bàn giao** để đưa cho khách.

Mỗi phòng chỉ được mở một hợp đồng chờ nhận phòng tại một thời điểm, nên chủ trọ không thể vô tình hứa cùng một phòng cho hai khách.

### 2.8. Màn hình nhận phòng của khách thuê

Dành cho: khách thuê (chỉ hiển thị khi khách **chưa được gán vào phòng nào**).

Khi khách đăng nhập mà hệ thống xác nhận chưa có phòng, thay vì vào thẳng màn hình hóa đơn, khách thấy màn hình **"Nhận phòng trọ"** với ba cách thức:

1. **Tải ảnh QR từ máy** — chọn ảnh khách đã chụp; hệ thống đọc mã ngay trên máy.
2. **Quét webcam trực tiếp** — bật camera, đưa mã QR vào khung hình; hệ thống tự nhận diện trong khoảng 1 phần 3 giây.
3. **Nhập mã PIN 8 số** — nhập tay dòng PIN chủ trọ đưa, phù hợp khi camera lỗi.

Sau khi nhận phòng thành công, hệ thống hiển thị lời chúc mừng kèm số phòng và ngày bắt đầu hợp đồng, rồi chuyển thẳng sang màn hình hóa đơn. Màn hình có nút **"Kiểm tra lại trạng thái"** để khách tự làm mới nếu vừa được chủ trọ gán phòng bởi cách khác.

Quy trình nhận phòng thực hiện an toàn ở tầng máy chủ: kiểm tra đúng người đại diện, kiểm tra phòng còn sức chứa, gán phòng, kích hoạt hợp đồng và **xóa mã QR/PIN ngay trong cùng giao dịch** — nhờ đó mã chỉ dùng được đúng một lần, không thể quét lại hoặc dùng lại lần hai.

### 2.9. Màn hình điện, nước

Dành cho: chủ trọ, quản lý trọ.

Cuối tháng, chủ trọ chọn tháng chốt và nhập chỉ số mới từng phòng. Chỉ số đầu kỳ được tự điền bằng chỉ số cuối kỳ của tháng trước nên không phải nhập lại và không sợ nhầm. Nếu chỉ số mới nhập thấp hơn chỉ số cũ, hệ thống báo lỗi ngay. Đơn giá điện, nước được lưu riêng cho từng tháng nên tháng nào tăng giá vẫn tính đúng tháng đó.

Trước khi lưu, màn hình hiện ngay số điện, nước tiêu thụ và thành tiền để đối chiếu. Mọi phép tính do máy chủ thực hiện nên mọi máy tính đều ra một kết quả.

### 2.10. Màn hình hóa đơn và thu tiền

Dành cho: chủ trọ, quản lý trọ.

Hóa đơn mỗi tháng được lập theo từng phòng, tự gộp tiền phòng, tiền điện, tiền nước và các phí khác thành tổng phải thu. Phòng nào chưa chốt điện nước hoặc chưa có hợp đồng thì chưa lập được hóa đơn. Mỗi phòng mỗi tháng chỉ có một hóa đơn, lập trùng hệ thống từ chối.

Danh sách hóa đơn hiển thị rõ hóa đơn nào đã thu, hóa đơn nào chưa, lọc được theo tháng. Khi khách trả tiền, chủ trọ bấm xác nhận thu tiền, hệ thống ghi lại thời điểm thu. Hóa đơn đã thu thì không sửa, không xóa được nữa. Danh sách phòng còn nợ theo tháng giúp chủ trọ biết còn phải thu bao nhiêu, của ai.

### 2.11. Màn hình hóa đơn của tôi

Dành cho: khách thuê.

Khách thuê đăng nhập bằng căn cước công dân sẽ thấy ba tab: **Tổng quan** (thông tin phòng đang ở, trạng thái hợp đồng), **Kỳ này** (bảng kê chi tiết tiền phòng, tiền điện, tiền nước, phí khác, tổng cộng) và **Lịch sử cước** (danh sách hóa đơn các tháng trước, phân trang từ máy chủ). Có nút **sao chép nội dung chuyển khoản** để khách chuyển tiền nhanh.

Khách thuê chỉ xem được phòng của mình, không thấy phòng khác, không sửa được gì. Trang tự động làm mới mỗi 30 giây nhưng **không tự chuyển tab** đang khách đang xem.

### 2.12. Màn hình thống kê và tra cứu

Dành cho: chủ trọ, quản lý trọ.

Ba nội dung chính. Thứ nhất, hiện trạng kinh doanh: tỉ lệ phòng đã lấp đầy, tổng số người đang ở. Thứ hai, doanh thu theo tháng: chọn khoảng thời gian, xem bảng đã thu và chưa thu từng tháng có dòng tổng cộng. Thứ ba, tra cứu nhanh: gõ tên, số phòng, số căn cước hoặc số điện thoại là tìm ra ngay, không phân biệt chữ hoa chữ thường.

### 2.13. Màn hình phân quyền

Dành riêng cho: chủ trọ.

Chủ trọ xem và điều chỉnh quyền của từng vai trò theo từng nhóm chức năng: Quản lý (bao gồm quyền xem mã QR/PIN nhận phòng), Công an phường (tra cứu, xuất hồ sơ lưu trú), Khách thuê (xem hóa đơn của chính mình, tự nhận phòng). Thay đổi có hiệu lực ngay lập tức mà không cần khởi động lại máy chủ.

Khi nâng cấp phần mềm bổ sung chức năng mới, hệ thống tự động cấp quyền mặc định cho các vai trò, nhưng **không** tự khôi phục những quyền chủ trọ đã chủ động thu hồi trước đó.

### 2.14. Màn hình tra cứu lưu trú (dành riêng Công an phường)

Dành cho: công an phường (chỉ đọc, giao diện riêng 3 tab).

Cán bộ công an phường đăng nhập bằng tài khoản được cấp và chỉ thấy màn hình này:
- **Tab Công dân:** thẻ số liệu (tổng phòng, người lưu trú, đã đăng ký, chưa đăng ký); tìm kiếm theo số CCCD, họ tên (không phân biệt hoa thường) hoặc số điện thoại; xem hồ sơ chi tiết và phòng đang ở.
- **Tab Tạm trú:** lọc riêng các trường hậu cần chưa đăng ký tạm trú theo từng phòng; xuất danh sách ra file phục vụ công tác kiểm tra hành chính.
- **Tab Biến động:** chọn khoảng ngày, lọc theo loại biến động (vào, ra, chuyển phòng); xem trước và xuất file báo cáo lịch sử cư trú.

Toàn bộ nút thao tác ghi đều bị khóa; máy chủ chặn mọi lệnh sửa đổi dữ liệu từ vai này (quy tắc BR-16). Ngoài ra, công an **không** được xem danh sách khách chưa được gán phòng — chỉ chủ trọ và quản lý mới có quyền xem nhóm này.

---

## 3. Tóm tắt phân quyền theo màn hình

| Màn hình | Chủ trọ | Quản lý trọ | Công an phường | Khách thuê |
|---|---|---|---|---|
| Đăng nhập / Đăng ký | Dùng | Dùng | Dùng | Dùng (+ tab đăng ký) |
| Trạng thái kết nối | Dùng | Dùng | Dùng | Dùng |
| Tổng quan | Toàn quyền | Theo phân quyền | Không | Không |
| Phòng | Toàn quyền | Theo phân quyền | Xem (trong tab Lưu trú) | Không |
| Người thuê | Toàn quyền | Theo phân quyền | Xem (trong tab Lưu trú) | Không |
| Hợp đồng | Toàn quyền | Theo phân quyền | Không | Không |
| Bàn giao QR / PIN | Toàn quyền (sinh & in phiếu) | Theo phân quyền | Không | Không |
| Nhận phòng (QR / webcam / PIN) | Không | Không | Không | Tự thực hiện |
| Điện, nước | Toàn quyền | Theo phân quyền | Không | Không |
| Hóa đơn và thu tiền | Toàn quyền | Theo phân quyền | Không | Không |
| Hóa đơn của tôi | Không | Không | Không | Chỉ phòng mình |
| Thống kê và tra cứu | Toàn quyền | Theo phân quyền | Không | Không |
| Phân quyền | Toàn quyền | Không | Không | Không |
| Tra cứu lưu trú (3 tab) | Không | Không | Toàn quyền tra cứu (chỉ đọc) | Không |

Mọi giới hạn trên đều do máy chủ kiểm tra và chặn, không phụ thuộc vào giao diện — kể cả khi giao diện bị sửa đổi thì yêu cầu vượt quyền vẫn bị từ chối.
