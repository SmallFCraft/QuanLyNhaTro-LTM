-- Schema Quản Lý Phòng Trọ — Ngũ Hành Sơn
-- MySQL 8.0 / InnoDB / UTF8MB4
-- Mỗi statement kết thúc bằng marker "-- statement" để SchemaInitializer tách và chạy tuần tự.

CREATE DATABASE IF NOT EXISTS quanly_phongtro_nhs
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
-- statement

USE quanly_phongtro_nhs;
-- statement

CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS rooms (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_number VARCHAR(20) NOT NULL UNIQUE,
    price DECIMAL(12, 2) NOT NULL,
    max_occupants INT NOT NULL DEFAULT 2,
    status ENUM('Available', 'Rented', 'Maintenance') DEFAULT 'Available',
    description VARCHAR(255) NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_rooms_price CHECK (price > 0),
    CONSTRAINT chk_rooms_max_occupants CHECK (max_occupants > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS tenants (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NULL,
    full_name VARCHAR(100) NOT NULL,
    dob DATE NOT NULL,
    id_card VARCHAR(20) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NULL,
    phone VARCHAR(20) NOT NULL,
    hometown VARCHAR(150) NOT NULL,
    workplace VARCHAR(150) NULL,
    is_temporary_registered BOOLEAN DEFAULT FALSE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_tenants_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS contracts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NOT NULL,
    representative_tenant_id INT NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    rental_price DECIMAL(12, 2) NOT NULL,
    deposit_amount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    status ENUM('Active', 'Expired', 'Terminated') DEFAULT 'Active',
    notes TEXT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_contracts_dates CHECK (end_date > start_date),
    CONSTRAINT fk_contracts_room FOREIGN KEY (room_id) REFERENCES rooms(id),
    CONSTRAINT fk_contracts_representative FOREIGN KEY (representative_tenant_id) REFERENCES tenants(id),
    INDEX idx_contracts_room_status (room_id, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS utility_readings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NOT NULL,
    billing_month VARCHAR(7) NOT NULL,
    old_electricity INT NOT NULL,
    new_electricity INT NOT NULL,
    electricity_rate DECIMAL(10, 2) NOT NULL DEFAULT 3500,
    old_water INT NOT NULL,
    new_water INT NOT NULL,
    water_rate DECIMAL(10, 2) NOT NULL DEFAULT 10000,
    recorded_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_utility_electricity CHECK (new_electricity >= old_electricity),
    CONSTRAINT chk_utility_water CHECK (new_water >= old_water),
    CONSTRAINT fk_utility_room FOREIGN KEY (room_id) REFERENCES rooms(id),
    UNIQUE KEY uq_room_month (room_id, billing_month)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

CREATE TABLE IF NOT EXISTS invoices (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id INT NOT NULL,
    contract_id INT NOT NULL,
    billing_month VARCHAR(7) NOT NULL,
    room_amount DECIMAL(12, 2) NOT NULL,
    electricity_amount DECIMAL(12, 2) NOT NULL,
    water_amount DECIMAL(12, 2) NOT NULL,
    other_fees DECIMAL(12, 2) NOT NULL DEFAULT 0,
    total_amount DECIMAL(12, 2) NOT NULL,
    status ENUM('Unpaid', 'Paid') DEFAULT 'Unpaid',
    paid_at DATETIME NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_invoices_room FOREIGN KEY (room_id) REFERENCES rooms(id),
    CONSTRAINT fk_invoices_contract FOREIGN KEY (contract_id) REFERENCES contracts(id),
    UNIQUE KEY uq_invoice_room_month (room_id, billing_month),
    INDEX idx_invoices_status_month (status, billing_month)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement
