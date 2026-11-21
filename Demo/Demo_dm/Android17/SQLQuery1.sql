/* =====================================================================
   Android17 - Website bán điện thoại và phụ kiện (SQL Server / LocalDB)
   Script tạo database + bảng của cửa hàng + dữ liệu danh mục, sản phẩm.

   LƯU Ý QUAN TRỌNG:
   - Script này XÓA và TẠO LẠI database Android17 mỗi lần chạy.
   - Các bảng đăng nhập của ASP.NET Core Identity (AspNetUsers, AspNetRoles...)
     KHÔNG nằm trong script này. Chúng được tạo bằng lệnh Update-Database
     trong Visual Studio. Nếu chạy lại script này thì phải chạy lại
     Update-Database.
   - Tài khoản (admin, khách) sẽ được tạo bằng code khi chạy ứng dụng.
   ===================================================================== */
USE master;
GO
IF DB_ID(N'Android17') IS NOT NULL
BEGIN
    ALTER DATABASE Android17 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Android17;
END
GO
CREATE DATABASE Android17;
GO
USE Android17;
GO

/* ---------- 1. TẠO BẢNG ---------- */

/* Hồ sơ khách hàng. Tài khoản đăng nhập nằm ở AspNetUsers (Identity);
   IdentityUserId chính là AspNetUsers.Id của người đó. */
CREATE TABLE NguoiDung (
    NDID           INT IDENTITY(1,1) PRIMARY KEY,
    IdentityUserId NVARCHAR(450) NOT NULL,
    HoTen          NVARCHAR(100) NOT NULL,
    SDT            NVARCHAR(20)  NULL,
    DChi           NVARCHAR(255) NULL,
    TG_TaoTK       DATETIME NOT NULL CONSTRAINT DF_NguoiDung_TG_TaoTK DEFAULT GETDATE(),
    CONSTRAINT UQ_NguoiDung_IdentityUserId UNIQUE (IdentityUserId)
);

CREATE TABLE LoaiHang (
    LoaiID  INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL,
    MoTa    NVARCHAR(255) NULL
);

CREATE TABLE NhanHang (
    NhanHangID  INT IDENTITY(1,1) PRIMARY KEY,
    TenNhanHang NVARCHAR(100) NOT NULL
);

CREATE TABLE SanPham (
    SPID       INT IDENTITY(1,1) PRIMARY KEY,
    TenSP      NVARCHAR(200) NOT NULL,
    LoaiID     INT NOT NULL,
    NhanHangID INT NOT NULL,
    Gia        DECIMAL(18,0) NOT NULL,
    GiaGoc     DECIMAL(18,0) NULL,
    TonKho     INT NOT NULL CONSTRAINT DF_SanPham_TonKho DEFAULT 0,
    Anh        NVARCHAR(255) NOT NULL,
    MoTaNgan   NVARCHAR(500) NULL,
    ThongSo    NVARCHAR(MAX) NULL,
    NoiBat     BIT NOT NULL CONSTRAINT DF_SanPham_NoiBat DEFAULT 0,  -- 1: hiện ở khu "Sản phẩm nổi bật"
    DangBan    BIT NOT NULL CONSTRAINT DF_SanPham_DangBan DEFAULT 1, -- 1: đang bán, 0: đã ẩn
    CONSTRAINT FK_SanPham_LoaiHang FOREIGN KEY (LoaiID)     REFERENCES LoaiHang(LoaiID),
    CONSTRAINT FK_SanPham_NhanHang FOREIGN KEY (NhanHangID) REFERENCES NhanHang(NhanHangID),
    CONSTRAINT CK_SanPham_Gia    CHECK (Gia > 0),
    CONSTRAINT CK_SanPham_TonKho CHECK (TonKho >= 0)
);

CREATE TABLE GioHang (
    GioID  INT IDENTITY(1,1) PRIMARY KEY,
    NDID   INT NOT NULL,
    SPID   INT NOT NULL,
    SLuong INT NOT NULL,
    CONSTRAINT FK_GioHang_NguoiDung FOREIGN KEY (NDID) REFERENCES NguoiDung(NDID),
    CONSTRAINT FK_GioHang_SanPham   FOREIGN KEY (SPID) REFERENCES SanPham(SPID),
    CONSTRAINT UQ_GioHang_ND_SP UNIQUE (NDID, SPID),
    CONSTRAINT CK_GioHang_SLuong CHECK (SLuong > 0)
);

CREATE TABLE DonHang (
    DHID            INT IDENTITY(1,1) PRIMARY KEY,
    NDID            INT NOT NULL,
    NgayTaoDon      DATETIME NOT NULL CONSTRAINT DF_DonHang_NgayTaoDon DEFAULT GETDATE(),
    NguoiNhan       NVARCHAR(100) NOT NULL,
    SDTNguoiNhan    NVARCHAR(20)  NOT NULL,
    DChi            NVARCHAR(255) NOT NULL,
    GhiChu          NVARCHAR(255) NULL,
    PThucThanhToan  NVARCHAR(50)  NOT NULL,
    TongTienSP      DECIMAL(18,0) NOT NULL,
    PhiVanChuyen    DECIMAL(18,0) NOT NULL,
    ThanhTien       DECIMAL(18,0) NOT NULL,
    TrangThai       INT NOT NULL CONSTRAINT DF_DonHang_TrangThai DEFAULT 0, -- 0 Chờ xác nhận, 1 Đã xác nhận, 2 Đang giao, 3 Hoàn thành, 4 Đã hủy
    TG_XacNhan      DATETIME NULL,
    NgayGiaoDuKien  DATETIME NULL,
    TG_DaGiao       DATETIME NULL,
    CONSTRAINT FK_DonHang_NguoiDung FOREIGN KEY (NDID) REFERENCES NguoiDung(NDID),
    CONSTRAINT CK_DonHang_TrangThai CHECK (TrangThai BETWEEN 0 AND 4)
);

CREATE TABLE DonHangChiTiet (
    CTID      INT IDENTITY(1,1) PRIMARY KEY,
    DHID      INT NOT NULL,
    SPID      INT NOT NULL,
    SLuong    INT NOT NULL,
    GiaLucDat DECIMAL(18,0) NOT NULL,
    CONSTRAINT FK_DHChiTiet_DonHang FOREIGN KEY (DHID) REFERENCES DonHang(DHID),
    CONSTRAINT FK_DHChiTiet_SanPham FOREIGN KEY (SPID) REFERENCES SanPham(SPID),
    CONSTRAINT CK_DHChiTiet_SLuong CHECK (SLuong > 0)
);
GO

CREATE INDEX IX_SanPham_Loai     ON SanPham(LoaiID);
CREATE INDEX IX_SanPham_NhanHang ON SanPham(NhanHangID);
CREATE INDEX IX_DonHang_ND       ON DonHang(NDID);
CREATE INDEX IX_DHChiTiet_DH     ON DonHangChiTiet(DHID);
GO

/* ---------- 2. DỮ LIỆU MẪU (danh mục, hãng, sản phẩm) ---------- */
INSERT INTO LoaiHang (TenLoai, MoTa) VALUES
(N'Điện thoại', N'Điện thoại thông minh các hãng'),
(N'Ốp lưng', N'Ốp lưng, bao da bảo vệ điện thoại'),
(N'Sạc và cáp', N'Củ sạc, cáp sạc các loại'),
(N'Tai nghe', N'Tai nghe có dây và không dây'),
(N'Phụ kiện khác', N'Pin dự phòng, sạc không dây, giá đỡ...');

INSERT INTO NhanHang (TenNhanHang) VALUES
(N'Apple'), (N'Samsung'), (N'Xiaomi'), (N'OPPO'), (N'Anker'), (N'Baseus');

INSERT INTO SanPham (TenSP, LoaiID, NhanHangID, Gia, GiaGoc, TonKho, Anh, MoTaNgan, ThongSo, NoiBat) VALUES
(N'iPhone 15 Pro Max 256GB', 1, 1, 29990000, 34990000, 15, N'iphone-15-pro-max.jpg', N'Khung titan, chip A17 Pro, camera 48MP', N'Màn hình: 6.7 inch Super Retina XDR
Chip: Apple A17 Pro
RAM: 8GB
Bộ nhớ: 256GB
Camera sau: 48MP + 12MP + 12MP
Pin: 4422 mAh', 1),
(N'iPhone 15 128GB', 1, 1, 19990000, 22990000, 25, N'iphone-15.jpg', N'Dynamic Island, camera 48MP, cổng USB-C', N'Màn hình: 6.1 inch Super Retina XDR
Chip: Apple A16 Bionic
RAM: 6GB
Bộ nhớ: 128GB
Camera sau: 48MP + 12MP
Pin: 3349 mAh', 1),
(N'iPhone 14 128GB', 1, 1, 16490000, 19990000, 20, N'iphone-14.jpg', N'Hiệu năng ổn định, camera kép đẹp', N'Màn hình: 6.1 inch OLED
Chip: Apple A15 Bionic
RAM: 6GB
Bộ nhớ: 128GB
Camera sau: 12MP + 12MP
Pin: 3279 mAh', 0),
(N'Samsung Galaxy S24 Ultra 256GB', 1, 2, 27990000, 33990000, 12, N'galaxy-s24-ultra.jpg', N'Bút S Pen, camera 200MP, tích hợp AI', N'Màn hình: 6.8 inch Dynamic AMOLED 2X
Chip: Snapdragon 8 Gen 3
RAM: 12GB
Bộ nhớ: 256GB
Camera sau: 200MP + 50MP + 12MP + 10MP
Pin: 5000 mAh', 1),
(N'Samsung Galaxy S24 128GB', 1, 2, 18990000, 22990000, 18, N'galaxy-s24.jpg', N'Nhỏ gọn, mạnh mẽ, có Galaxy AI', N'Màn hình: 6.2 inch Dynamic AMOLED 2X
Chip: Exynos 2400
RAM: 8GB
Bộ nhớ: 128GB
Camera sau: 50MP + 12MP + 10MP
Pin: 4000 mAh', 1),
(N'Samsung Galaxy A55 5G 128GB', 1, 2, 9990000, 11990000, 30, N'galaxy-a55.jpg', N'Khung kim loại, pin lâu, giá dễ tiếp cận', N'Màn hình: 6.6 inch Super AMOLED
Chip: Exynos 1480
RAM: 8GB
Bộ nhớ: 128GB
Camera sau: 50MP + 12MP + 5MP
Pin: 5000 mAh', 0),
(N'Xiaomi 14 12GB/256GB', 1, 3, 19990000, 22990000, 10, N'xiaomi-14.jpg', N'Camera Leica, sạc nhanh 90W', N'Màn hình: 6.36 inch AMOLED
Chip: Snapdragon 8 Gen 3
RAM: 12GB
Bộ nhớ: 256GB
Camera sau: 50MP Leica x3
Pin: 4610 mAh', 1),
(N'Xiaomi Redmi Note 13 Pro 8GB/256GB', 1, 3, 6990000, 7990000, 40, N'redmi-note-13-pro.jpg', N'Camera 200MP trong tầm giá', N'Màn hình: 6.67 inch AMOLED 120Hz
Chip: Snapdragon 7s Gen 2
RAM: 8GB
Bộ nhớ: 256GB
Camera sau: 200MP + 8MP + 2MP
Pin: 5100 mAh', 0),
(N'Xiaomi Redmi 13C 4GB/128GB', 1, 3, 2990000, 3490000, 50, N'redmi-13c.jpg', N'Điện thoại giá rẻ cho nhu cầu cơ bản', N'Màn hình: 6.74 inch IPS 90Hz
Chip: Helio G85
RAM: 4GB
Bộ nhớ: 128GB
Camera sau: 50MP
Pin: 5000 mAh', 0),
(N'OPPO Reno11 5G 8GB/256GB', 1, 4, 10490000, 11990000, 22, N'oppo-reno11.jpg', N'Chuyên gia chân dung, thiết kế mỏng nhẹ', N'Màn hình: 6.7 inch AMOLED 120Hz
Chip: Dimensity 7050
RAM: 8GB
Bộ nhớ: 256GB
Camera sau: 50MP + 32MP + 8MP
Pin: 5000 mAh', 1),
(N'OPPO A78 8GB/256GB', 1, 4, 5990000, 6990000, 35, N'oppo-a78.jpg', N'Sạc nhanh 67W, màn hình mượt', N'Màn hình: 6.43 inch AMOLED 90Hz
Chip: Snapdragon 680
RAM: 8GB
Bộ nhớ: 256GB
Camera sau: 50MP + 2MP
Pin: 5000 mAh', 0),
(N'OPPO Find N3 Flip 12GB/256GB', 1, 4, 22990000, 24990000, 6, N'oppo-find-n3-flip.jpg', N'Điện thoại gập vỏ sò thời thượng', N'Màn hình: 6.8 inch AMOLED gập
Chip: Dimensity 9200
RAM: 12GB
Bộ nhớ: 256GB
Camera sau: 50MP + 48MP + 32MP
Pin: 4300 mAh', 0),
(N'Ốp lưng silicone iPhone 15 Pro Max', 2, 1, 1290000, NULL, 60, N'op-iphone-15-pro-max.jpg', N'Chất liệu silicone mềm, ôm máy', N'Tương thích: iPhone 15 Pro Max
Chất liệu: Silicone
Hỗ trợ sạc MagSafe: Có', 0),
(N'Ốp lưng chống sốc Galaxy S24 Ultra', 2, 2, 490000, 690000, 45, N'op-galaxy-s24-ultra.jpg', N'Chống sốc chuẩn quân đội, viền nâng camera', N'Tương thích: Galaxy S24 Ultra
Chất liệu: TPU + PC
Chống sốc: Có', 0),
(N'Ốp lưng trong suốt Baseus iPhone 15', 2, 6, 250000, 350000, 80, N'op-baseus-iphone-15.jpg', N'Trong suốt, chống ố vàng', N'Tương thích: iPhone 15
Chất liệu: TPU cao cấp
Chống ố vàng: Có', 0),
(N'Củ sạc nhanh Anker 20W', 3, 5, 390000, 490000, 100, N'cu-sac-anker-20w.jpg', N'Sạc nhanh USB-C PD 20W', N'Công suất: 20W
Cổng: USB-C
Chuẩn sạc: Power Delivery
Bảo hành: 18 tháng', 1),
(N'Củ sạc nhanh Baseus GaN 65W', 3, 6, 790000, 990000, 50, N'cu-sac-baseus-65w.jpg', N'Sạc đa thiết bị, nhỏ gọn nhờ công nghệ GaN', N'Công suất: 65W
Cổng: 2 USB-C + 1 USB-A
Công nghệ: GaN
Bảo hành: 12 tháng', 0),
(N'Cáp sạc Baseus USB-C to Lightning 1m', 3, 6, 190000, 250000, 120, N'cap-baseus-lightning.jpg', N'Bện dù chống đứt, sạc nhanh 20W', N'Chiều dài: 1m
Đầu cáp: USB-C to Lightning
Công suất tối đa: 20W', 0),
(N'Cáp sạc Anker USB-C to USB-C 100W', 3, 5, 290000, NULL, 90, N'cap-anker-usb-c.jpg', N'Hỗ trợ sạc laptop và điện thoại', N'Chiều dài: 1.8m
Đầu cáp: USB-C to USB-C
Công suất tối đa: 100W', 0),
(N'Tai nghe AirPods Pro 2 USB-C', 4, 1, 5990000, 6790000, 25, N'airpods-pro-2.jpg', N'Chống ồn chủ động, âm thanh không gian', N'Chip: Apple H2
Chống ồn: Có (ANC)
Pin: 6 giờ (30 giờ với hộp sạc)
Cổng sạc: USB-C', 1),
(N'Tai nghe Samsung Galaxy Buds2 Pro', 4, 2, 3990000, 4990000, 28, N'galaxy-buds2-pro.jpg', N'Âm thanh Hi-Fi 24-bit, chống ồn thông minh', N'Chống ồn: Có (ANC)
Pin: 5 giờ (18 giờ với hộp sạc)
Kháng nước: IPX7
Kết nối: Bluetooth 5.3', 0),
(N'Tai nghe Xiaomi Redmi Buds 5', 4, 3, 890000, 1190000, 60, N'redmi-buds-5.jpg', N'Chống ồn 46dB, giá hợp lý', N'Chống ồn: Có (ANC 46dB)
Pin: 10 giờ (40 giờ với hộp sạc)
Kết nối: Bluetooth 5.3', 0),
(N'Pin dự phòng Anker 10000mAh', 5, 5, 690000, 850000, 70, N'pin-anker-10000.jpg', N'Mỏng nhẹ, sạc nhanh 20W', N'Dung lượng: 10000 mAh
Công suất: 20W
Cổng: USB-C + USB-A', 0),
(N'Pin dự phòng Baseus 20000mAh 65W', 5, 6, 1490000, 1790000, 30, N'pin-baseus-20000.jpg', N'Sạc được cả laptop, có màn hình hiển thị', N'Dung lượng: 20000 mAh
Công suất: 65W
Cổng: 2 USB-C + 1 USB-A', 0),
(N'Sạc không dây Baseus 15W', 5, 6, 450000, 590000, 55, N'sac-khong-day-baseus.jpg', N'Sạc không dây chuẩn Qi, đế nghiêng tiện lợi', N'Công suất: 15W
Chuẩn: Qi
Tương thích: iPhone, Samsung, Xiaomi', 0),
(N'Giá đỡ điện thoại ô tô Baseus', 5, 6, 320000, NULL, 65, N'gia-do-oto-baseus.jpg', N'Gắn cửa gió, xoay 360 độ', N'Chất liệu: Hợp kim + silicone
Gắn: Cửa gió điều hòa
Tương thích: Mọi điện thoại 4.7 - 7 inch', 0);
GO

/* ---------- 3. KIỂM TRA NHANH SỐ DÒNG ---------- */
SELECT N'NguoiDung' AS Bang, COUNT(*) AS SoDong FROM NguoiDung
UNION ALL SELECT N'LoaiHang', COUNT(*) FROM LoaiHang
UNION ALL SELECT N'NhanHang', COUNT(*) FROM NhanHang
UNION ALL SELECT N'SanPham', COUNT(*) FROM SanPham
UNION ALL SELECT N'GioHang', COUNT(*) FROM GioHang
UNION ALL SELECT N'DonHang', COUNT(*) FROM DonHang
UNION ALL SELECT N'DonHangChiTiet', COUNT(*) FROM DonHangChiTiet;
GO

SELECT * FROM LoaiHang;
SELECT * FROM NhanHang;
SELECT * FROM SanPham;