-- Schema Quản Lý Phòng Trọ — Ngũ Hành Sơn
-- MySQL 8.0 / InnoDB / UTF8MB4
-- Mỗi statement kết thúc bằng marker dòng riêng "-- statement" để KhoiTaoSchema tách và chạy tuần tự.

CREATE DATABASE IF NOT EXISTS quanly_phongtro_nhs
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
-- statement

USE quanly_phongtro_nhs;
-- statement

-- Tài khoản đăng nhập của Chủ trọ / Quản lý / Công an phường.
CREATE TABLE IF NOT EXISTS tai_khoan (
    id INT AUTO_INCREMENT PRIMARY KEY,
    ten_dang_nhap VARCHAR(50) NOT NULL UNIQUE,
    mat_khau_hash VARCHAR(255) NOT NULL,
    ho_ten VARCHAR(100) NOT NULL,
    vai_tro ENUM('ChuTro', 'QuanLy', 'CongAn', 'KhachThue') NOT NULL DEFAULT 'ChuTro',
    ngay_tao DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

-- Ma trận quyền động theo vai trò. Chủ trọ (ChuTro) KHÔNG lưu ở đây —
-- quyền Chủ trọ bypass cứng trong code để không bao giờ tự khóa mình khỏi hệ thống.
CREATE TABLE IF NOT EXISTS quyen_vai_tro (
    vai_tro VARCHAR(20) NOT NULL,
    hanh_dong VARCHAR(50) NOT NULL,
    ngay_tao DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (vai_tro, hanh_dong)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS phong (
    id INT AUTO_INCREMENT PRIMARY KEY,
    so_phong VARCHAR(20) NOT NULL UNIQUE,
    gia_thue DECIMAL(12, 2) NOT NULL,
    so_nguoi_toi_da INT NOT NULL DEFAULT 2,
    trang_thai ENUM('Trong', 'DaThue', 'BaoTri') DEFAULT 'Trong',
    mo_ta VARCHAR(255) NULL,
    ngay_tao DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_phong_gia_thue CHECK (gia_thue > 0),
    CONSTRAINT chk_phong_so_nguoi CHECK (so_nguoi_toi_da > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS khach_thue (
    id INT AUTO_INCREMENT PRIMARY KEY,
    phong_id INT NULL,
    ho_ten VARCHAR(100) NOT NULL,
    ngay_sinh DATE NOT NULL,
    cccd VARCHAR(20) NOT NULL UNIQUE,
    mat_khau_hash VARCHAR(255) NULL,
    so_dien_thoai VARCHAR(20) NOT NULL,
    que_quan VARCHAR(150) NOT NULL,
    noi_lam_viec VARCHAR(150) NULL,
    da_dang_ky_tam_tru BOOLEAN DEFAULT FALSE,
    ngay_tao DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_khach_thue_phong FOREIGN KEY (phong_id) REFERENCES phong(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS hop_dong (
    id INT AUTO_INCREMENT PRIMARY KEY,
    phong_id INT NOT NULL,
    nguoi_dai_dien_id INT NOT NULL,
    ngay_bat_dau DATE NOT NULL,
    ngay_ket_thuc DATE NOT NULL,
    gia_thue DECIMAL(12, 2) NOT NULL,
    tien_coc DECIMAL(12, 2) NOT NULL DEFAULT 0,
    trang_thai ENUM('HieuLuc', 'HetHan', 'ChamDut') DEFAULT 'HieuLuc',
    ghi_chu TEXT NULL,
    ngay_tao DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_hop_dong_ngay CHECK (ngay_ket_thuc > ngay_bat_dau),
    CONSTRAINT fk_hop_dong_phong FOREIGN KEY (phong_id) REFERENCES phong(id),
    CONSTRAINT fk_hop_dong_nguoi_dai_dien FOREIGN KEY (nguoi_dai_dien_id) REFERENCES khach_thue(id),
    INDEX idx_hop_dong_phong_trang_thai (phong_id, trang_thai)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS chi_so_dien_nuoc (
    id INT AUTO_INCREMENT PRIMARY KEY,
    phong_id INT NOT NULL,
    ky_cuoc VARCHAR(7) NOT NULL,
    dien_cu INT NOT NULL,
    dien_moi INT NOT NULL,
    gia_dien DECIMAL(10, 2) NOT NULL DEFAULT 3500,
    nuoc_cu INT NOT NULL,
    nuoc_moi INT NOT NULL,
    gia_nuoc DECIMAL(10, 2) NOT NULL DEFAULT 10000,
    ngay_ghi DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_chi_so_dien CHECK (dien_moi >= dien_cu),
    CONSTRAINT chk_chi_so_nuoc CHECK (nuoc_moi >= nuoc_cu),
    CONSTRAINT fk_chi_so_phong FOREIGN KEY (phong_id) REFERENCES phong(id),
    UNIQUE KEY uq_phong_ky (phong_id, ky_cuoc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS hoa_don (
    id INT AUTO_INCREMENT PRIMARY KEY,
    phong_id INT NOT NULL,
    hop_dong_id INT NOT NULL,
    ky_cuoc VARCHAR(7) NOT NULL,
    tien_phong DECIMAL(12, 2) NOT NULL,
    tien_dien DECIMAL(12, 2) NOT NULL,
    tien_nuoc DECIMAL(12, 2) NOT NULL,
    phi_khac DECIMAL(12, 2) NOT NULL DEFAULT 0,
    tong_tien DECIMAL(12, 2) NOT NULL,
    trang_thai ENUM('ChuaThu', 'DaThu') DEFAULT 'ChuaThu',
    ngay_dong DATETIME NULL,
    ngay_tao DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_hoa_don_phong FOREIGN KEY (phong_id) REFERENCES phong(id),
    CONSTRAINT fk_hoa_don_hop_dong FOREIGN KEY (hop_dong_id) REFERENCES hop_dong(id),
    UNIQUE KEY uq_hoa_don_phong_ky (phong_id, ky_cuoc),
    INDEX idx_hoa_don_trang_thai_ky (trang_thai, ky_cuoc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement
