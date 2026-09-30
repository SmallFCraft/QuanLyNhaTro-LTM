# HỆ THỐNG QUẢN LÝ PHÒNG TRỌ PHƯỜNG NGŨ HÀNH SƠN — MÔ TẢ CHỨC NĂNG VÀ GIAO DIỆN
---

## 1. Ai sử dụng hệ thống

| Người dùng | Vai trò trong thực tế | Quyền trong hệ thống |
|---|---|---|
| Chủ trọ | Người sở hữu một hoặc nhiều khu trọ | Toàn quyền trên mọi khu: xem, thêm, sửa, xóa, thu tiền, phân công người quản lý |
| Quản lý trọ | Người được chủ trọ thuê vận hành một khu cụ thể | Làm mọi việc như chủ trọ nhưng chỉ trong khu mình phụ trách, không thấy khu khác |
| Công an phường | Cán bộ theo dõi lưu trú, tạm trú — tạm vắng | Màn hình riêng ở chế độ chỉ xem: tra cứu công dân, tình trạng phòng, tạm trú và lịch sử biến động; xuất được file phục vụ kiểm tra nhưng không sửa được bất cứ gì |
| Khách thuê | Người đang ở trọ | Chỉ xem hóa đơn và lịch sử đóng tiền của phòng mình |

---

## 2. Danh sách màn hình

### 2.1. Màn hình đăng nhập

Màn hình đầu tiên khi mở phần mềm. Mỗi người dùng đăng nhập bằng tài khoản được cấp — khách thuê đăng nhập bằng số căn cước công dân của mình.

Sau khi đăng nhập, hệ thống tự nhận biết người đó là chủ trọ, quản lý, công an phường hay khách thuê, và chỉ mở đúng các màn hình người đó được phép dùng. Nhập sai mật khẩu quá 5 lần liên tiếp, tài khoản bị tạm khóa 1 phút để chống dò mật khẩu.

### 2.2. Thanh trạng thái kết nối

Một dải trạng thái nằm ở cuối màn hình, luôn cho biết máy đang kết nối tốt với máy chủ hay đã mất kết nối. Địa chỉ máy chủ do kỹ thuật cài đặt sẵn từ đầu, người dùng không cần nhập gì. Khi mất kết nối, phần mềm hiện thông báo rõ ràng và không bị treo, kết nối lại là dùng tiếp.

### 2.3. Màn hình tổng quan

Dành cho: chủ trọ, quản lý trọ.

Vừa đăng nhập, chủ trọ nhìn thấy ngay bức tranh toàn cảnh việc kinh doanh: tổng số phòng, bao nhiêu phòng trống, bao nhiêu phòng đang cho thuê, tổng số người đang ở, tháng này đã thu được bao nhiêu và còn nợ bao nhiêu.

Bên dưới là hai danh sách cần hành động ngay: các phòng còn nợ tiền cần đôn đốc, và các hợp đồng sắp hết hạn trong 30 ngày. Chủ trọ có nhiều khu thì xem gộp tất cả, hoặc chọn xem riêng từng khu để so sánh khu nào cho thuê tốt hơn.

### 2.4. Màn hình khu trọ

Dành cho: chủ trọ (quản lý và công an phường chỉ xem).

Chủ trọ khai báo từng khu mình sở hữu: tên khu, địa chỉ trong phường. Mỗi phòng thuộc về một khu. Tại đây chủ trọ cũng tạo tài khoản cho người quản lý và phân công mỗi người phụ trách khu nào, thu hồi khi cần. Khu còn phòng thì không được xóa để tránh mất dữ liệu.

### 2.5. Màn hình phòng

Dành cho: chủ trọ, quản lý trọ (công an phường chỉ xem).

Danh sách toàn bộ phòng với số phòng, giá thuê, sức chứa, số người đang ở thực tế và trạng thái (trống, đang thuê, bảo trì). Số người đang ở được đếm trực tiếp từ danh sách người thuê nên luôn đúng, không phải nhập tay.

Chủ trọ thêm phòng mới, sửa giá hoặc sức chứa, lọc xem riêng phòng trống để chào khách. Phòng đang có người ở hoặc còn hợp đồng thì hệ thống không cho xóa, và mỗi lần xóa đều hỏi xác nhận trước.

### 2.6. Màn hình người thuê

Dành cho: chủ trọ, quản lý trọ (công an phường chỉ xem).

Hồ sơ mỗi người ở gồm họ tên, ngày sinh, căn cước công dân, số điện thoại, quê quán, nơi học tập hoặc làm việc. Mỗi số căn cước chỉ tồn tại một lần trong toàn hệ thống nên không bị trùng người.

Chủ trọ thêm người mới vào phòng còn chỗ trống — phòng vừa có người sẽ tự chuyển sang trạng thái đang thuê. Khi khách chuyển phòng, hệ thống kiểm tra phòng mới còn chỗ mới cho chuyển; khi khách trả phòng, liên kết được gỡ và phòng hết người tự trở về trạng thái trống.

Mỗi hồ sơ có ghi chú đã đăng ký tạm trú hay chưa. Cuối kỳ, chủ trọ bấm một nút để xuất danh sách người đang ở ra file, nộp cho công an phường theo quy định.

### 2.7. Màn hình hợp đồng

Dành cho: chủ trọ, quản lý trọ (công an phường chỉ xem).

Mỗi phòng đang cho thuê có đúng một hợp đồng còn hiệu lực, do một người trong phòng đứng tên đại diện. Hợp đồng ghi ngày bắt đầu, ngày kết thúc, giá thuê và tiền cọc.

Hệ thống liệt kê tất cả hợp đồng kèm số ngày còn lại, sắp xếp hợp đồng gần hết hạn lên đầu và tô nổi bật những hợp đồng còn dưới 30 ngày để chủ trọ chủ động liên hệ tái ký. Chủ trọ gia hạn bằng cách cập nhật ngày kết thúc mới, hoặc chấm dứt hợp đồng có ghi lý do và việc hoàn trả tiền cọc.

### 2.8. Màn hình điện, nước

Dành cho: chủ trọ, quản lý trọ.

Cuối tháng, chủ trọ chọn tháng chốt và nhập chỉ số mới từng phòng. Chỉ số đầu kỳ được tự điền bằng chỉ số cuối kỳ của tháng trước nên không phải nhập lại và không sợ nhầm. Nếu chỉ số mới nhập thấp hơn chỉ số cũ, hệ thống báo lỗi ngay. Đơn giá điện, nước được lưu riêng cho từng tháng nên tháng nào tăng giá vẫn tính đúng tháng đó.

Trước khi lưu, màn hình hiện ngay số điện, nước tiêu thụ và thành tiền để đối chiếu. Mọi phép tính do máy chủ thực hiện nên mọi máy tính đều ra một kết quả.

### 2.9. Màn hình hóa đơn và thu tiền

Dành cho: chủ trọ, quản lý trọ.

Hóa đơn mỗi tháng được lập theo từng phòng, tự gộp tiền phòng, tiền điện, tiền nước và các phí khác thành tổng phải thu. Phòng nào chưa chốt điện nước hoặc chưa có hợp đồng thì chưa lập được hóa đơn. Mỗi phòng mỗi tháng chỉ có một hóa đơn, lập trùng hệ thống từ chối.

Danh sách hóa đơn hiển thị rõ hóa đơn nào đã thu, hóa đơn nào chưa, lọc được theo tháng. Khi khách trả tiền, chủ trọ bấm xác nhận thu tiền, hệ thống ghi lại thời điểm thu. Hóa đơn đã thu thì không sửa, không xóa được nữa. Danh sách phòng còn nợ theo tháng giúp chủ trọ biết còn phải thu bao nhiêu, của ai.

### 2.10. Màn hình hóa đơn của tôi

Dành cho: khách thuê.

Khách thuê đăng nhập bằng căn cước công dân sẽ thấy duy nhất màn hình này: thông tin phòng mình đang ở, hóa đơn từng tháng với chi tiết tiền phòng, tiền điện, tiền nước, và lịch sử đã đóng hay còn nợ. Khách thuê chỉ xem được phòng của mình, không thấy phòng khác, không sửa được gì.

### 2.11. Màn hình thống kê và tra cứu

Dành cho: chủ trọ, quản lý trọ.

Ba nội dung chính. Thứ nhất, hiện trạng kinh doanh: tỉ lệ phòng đã lấp đầy, tổng số người đang ở, kèm biểu đồ đơn giản dễ nhìn. Thứ hai, doanh thu theo tháng: chọn khoảng thời gian, xem bảng đã thu và chưa thu từng tháng có dòng tổng cộng. Thứ ba, tra cứu nhanh: gõ tên, số phòng, số căn cước hoặc số điện thoại là tìm ra ngay, không phân biệt chữ hoa chữ thường.

### 2.12. Màn hình tra cứu lưu trú (dành riêng Công an phường)

Dành cho: công an phường (chỉ đọc, giao diện riêng 3 tab).

Cán bộ công an phường đăng nhập bằng tài khoản được cấp và chỉ thấy màn hình này:
- **Tab Công dân (PM-02, PM-03):** 4 thẻ số liệu (tổng phòng, người lưu trú, đã đăng ký, chưa đăng ký); tìm kiếm theo số CCCD, họ tên (không phân biệt hoa thường) hoặc số điện thoại; xem hồ sơ chi tiết và phòng đang ở.
- **Tab Tạm trú (PM-04):** lọc riêng các trường hợp chưa đăng ký tạm trú theo từng phòng; nút xuất nhanh danh sách ra file CSV / Excel phục vụ công tác kiểm tra hành chính.
- **Tab Biến động (PM-05):** chọn khoảng ngày (từ ngày → đến ngày), lọc theo loại biến động (vào, ra, chuyển phòng); xem trước 10 dòng đầu; xuất file (CSV / Excel / PDF) kèm ghi log xuất file về máy chủ để lưu vết.

Toàn bộ nút thao tác ghi đều bị khóa; máy chủ chặn mọi lệnh sửa đổi dữ liệu từ vai này (quy tắc BR-16).

---

## 3. Tóm tắt phân quyền theo màn hình

| Màn hình | Chủ trọ | Quản lý trọ | Công an phường | Khách thuê |
|---|---|---|---|---|
| Đăng nhập, trạng thái kết nối | Dùng | Dùng | Dùng | Dùng |
| Tổng quan | Toàn bộ khu | Khu mình | Không | Không |
| Khu trọ | Thêm, sửa, xóa, phân công | Xem khu mình | Xem | Không |
| Phòng | Toàn quyền | Trong khu mình | Xem (trong tab Lưu trú) | Không |
| Người thuê | Toàn quyền | Trong khu mình | Xem (trong tab Lưu trú) | Không |
| Hợp đồng | Toàn quyền | Trong khu mình | Không | Không |
| Điện, nước | Toàn quyền | Trong khu mình | Không | Không |
| Hóa đơn và thu tiền | Toàn quyền | Trong khu mình | Không | Không |
| Hóa đơn của tôi | Không | Không | Không | Chỉ phòng mình |
| Thống kê và tra cứu | Toàn quyền | Trong khu mình | Không | Không |
| Tra cứu lưu trú (3 tab) | Không | Dùng chung một phần | Toàn quyền tra cứu (chỉ đọc) | Không |

Mọi giới hạn trên đều do máy chủ kiểm tra và chặn, không phụ thuộc vào giao diện — kể cả khi giao diện bị sửa đổi thì yêu cầu vượt quyền vẫn bị từ chối.
