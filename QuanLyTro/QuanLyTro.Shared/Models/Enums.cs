namespace QuanLyTro.Shared.Models;

/// <summary>
/// Vai trò người dùng trong hệ thống.
/// Thứ tự giữ nguyên giá trị số gốc (0..3) — đổi thứ tự sẽ phá dữ liệu cũ đã lưu theo số.
/// </summary>
public enum VaiTroNguoiDung
{
    ChuTro = 0,
    KhachThue = 1,
    CongAn = 2,
    QuanLy = 3,
}

/// <summary>
/// Trạng thái phòng trọ.
/// </summary>
public enum TrangThaiPhong
{
    Trong,
    DaThue,
    BaoTri,
}

/// <summary>
/// Trạng thái hợp đồng thuê. `ChoNhanPhong` = đã lập, đang chờ người đại diện quét QR/PIN nhận phòng (BR-18).
/// </summary>
public enum TrangThaiHopDong
{
    HieuLuc,
    HetHan,
    ChamDut,
    ChoNhanPhong,
}

/// <summary>
/// Trạng thái hóa đơn tiền trọ.
/// </summary>
public enum TrangThaiHoaDon
{
    ChuaThu,
    DaThu,
}
