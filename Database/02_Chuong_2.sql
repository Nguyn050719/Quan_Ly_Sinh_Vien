-- B?ng SinhVien
CREATE TABLE SinhVien (
    MaSV VARCHAR2(10) PRIMARY KEY,
    HoTen VARCHAR2(100),
    NgaySinh DATE,
    SDT VARCHAR2(15),
    DiaChi VARCHAR2(200),
    Email VARCHAR2(50),
    TenDangNhap VARCHAR2(50),
    MatKhau VARCHAR2(256)
);

-- B?ng MonHoc
CREATE TABLE MonHoc (
    MaMH VARCHAR2(10) PRIMARY KEY,
    TenMH VARCHAR2(100),
    SoTinChi NUMBER
);

-- B?ng Lop
CREATE TABLE Lop (
    MaLop VARCHAR2(10) PRIMARY KEY,
    TenLop VARCHAR2(50),
    SiSo NUMBER
);

-- B?ng Diem
CREATE TABLE Diem (
    MaDiem VARCHAR2(10) PRIMARY KEY,
    DiemSo FLOAT(2),
    FK_SV_Diem VARCHAR2(10),
    FK_MH_Diem VARCHAR2(10),
    CONSTRAINT fk_diem_sinhvien FOREIGN KEY (FK_SV_Diem) REFERENCES SinhVien(MaSV),
    CONSTRAINT fk_diem_monhoc FOREIGN KEY (FK_MH_Diem) REFERENCES MonHoc(MaMH)
);

-- B?ng Diem_SinhVien (b?ng trung gian cho quan h? Many-to-Many gi?a SinhVien và Diem)
CREATE TABLE Diem_SinhVien (
    FK_SV_DiemSV VARCHAR2(10),
    FK_Diem_DiemSV VARCHAR2(10),
    CONSTRAINT pk_diem_sinhvien PRIMARY KEY (FK_SV_DiemSV, FK_Diem_DiemSV),
    CONSTRAINT fk_diem_sinhvien_sv FOREIGN KEY (FK_SV_DiemSV) REFERENCES SinhVien(MaSV),
    CONSTRAINT fk_diem_sinhvien_diem FOREIGN KEY (FK_Diem_DiemSV) REFERENCES Diem(MaDiem)
);

-- B?ng Diem_MonHoc (b?ng trung gian cho quan h? Many-to-Many gi?a Diem và MonHoc)
CREATE TABLE Diem_MonHoc (
    FK_Diem_DMH VARCHAR2(10),
    FK_MH_DMH VARCHAR2(10),
    CONSTRAINT pk_diem_monhoc PRIMARY KEY (FK_Diem_DMH, FK_MH_DMH),
    CONSTRAINT fk_diem_monhoc_diem FOREIGN KEY (FK_Diem_DMH) REFERENCES Diem(MaDiem),
    CONSTRAINT fk_diem_monhoc_monhoc FOREIGN KEY (FK_MH_DMH) REFERENCES MonHoc(MaMH)
);

-- B?ng SinhVien_Lop (b?ng trung gian cho quan h? Many-to-Many gi?a SinhVien và Lop)
CREATE TABLE SinhVien_Lop (
    FK_SV_SL VARCHAR2(10),
    FK_Lop_SL VARCHAR2(10),
    CONSTRAINT pk_sinhvien_lop PRIMARY KEY (FK_SV_SL, FK_Lop_SL),
    CONSTRAINT fk_sinhvien_lop_sv FOREIGN KEY (FK_SV_SL) REFERENCES SinhVien(MaSV),
    CONSTRAINT fk_sinhvien_lop_lop FOREIGN KEY (FK_Lop_SL) REFERENCES Lop(MaLop)
);

-- Cập nhật bảng Diem:
ALTER TABLE Diem DROP CONSTRAINT fk_diem_sinhvien;
ALTER TABLE Diem 
ADD CONSTRAINT fk_diem_sinhvien FOREIGN KEY (FK_SV_Diem)
REFERENCES SinhVien(MaSV) ON DELETE CASCADE;

-- Cập nhật bảng Diem_SinhVien:
ALTER TABLE Diem_SinhVien DROP CONSTRAINT fk_diem_sinhvien_sv;
ALTER TABLE Diem_SinhVien 
ADD CONSTRAINT fk_diem_sinhvien_sv FOREIGN KEY (FK_SV_DiemSV)
REFERENCES SinhVien(MaSV) ON DELETE CASCADE;

-- Cập nhật bảng SinhVien_Lop:
ALTER TABLE SinhVien_Lop DROP CONSTRAINT fk_sinhvien_lop_sv;
ALTER TABLE SinhVien_Lop 
ADD CONSTRAINT fk_sinhvien_lop_sv FOREIGN KEY (FK_SV_SL)
REFERENCES SinhVien(MaSV) ON DELETE CASCADE;
-- Cập nhật bảng Diem:
ALTER TABLE Diem DROP CONSTRAINT fk_diem_monhoc;
ALTER TABLE Diem 
ADD CONSTRAINT fk_diem_monhoc FOREIGN KEY (FK_MH_Diem)
REFERENCES MonHoc(MaMH) ON DELETE CASCADE;

-- Cập nhật bảng Diem_MonHoc:
ALTER TABLE Diem_MonHoc DROP CONSTRAINT fk_diem_monhoc_monhoc;
ALTER TABLE Diem_MonHoc 
ADD CONSTRAINT fk_diem_monhoc_monhoc FOREIGN KEY (FK_MH_DMH)
REFERENCES MonHoc(MaMH) ON DELETE CASCADE;

-- Cập nhật bảng SinhVien_Lop:
ALTER TABLE SinhVien_Lop DROP CONSTRAINT fk_sinhvien_lop_lop;
ALTER TABLE SinhVien_Lop 
ADD CONSTRAINT fk_sinhvien_lop_lop FOREIGN KEY (FK_Lop_SL)
REFERENCES Lop(MaLop) ON DELETE CASCADE;

-- Cập nhật bảng Diem_SinhVien:
ALTER TABLE Diem_SinhVien DROP CONSTRAINT fk_diem_sinhvien_diem;
ALTER TABLE Diem_SinhVien 
ADD CONSTRAINT fk_diem_sinhvien_diem FOREIGN KEY (FK_Diem_DiemSV)
REFERENCES Diem(MaDiem) ON DELETE CASCADE;

-- Cập nhật bảng Diem_MonHoc:
ALTER TABLE Diem_MonHoc DROP CONSTRAINT fk_diem_monhoc_diem;
ALTER TABLE Diem_MonHoc 
ADD CONSTRAINT fk_diem_monhoc_diem FOREIGN KEY (FK_Diem_DMH)
REFERENCES Diem(MaDiem) ON DELETE CASCADE;
-- Chýõng 2:
-- 1. M? r?ng c?t SDT: V? m? hóa AES s? t?o ra chu?i k? t? dài hõn s? ði?n tho?i b?nh thý?ng
ALTER TABLE SinhVien MODIFY SDT VARCHAR2(200);

-- 2. Thêm c?t Avatar: Ki?u BLOB ð? ch?a d? li?u h?nh ?nh (M? hóa Lai)
ALTER TABLE SinhVien ADD Avatar BLOB;

-- 3. Thêm c?t EncryptedKey: Ch?a khóa AES ð? ðý?c m? hóa b?ng RSA (M? hóa Lai)
ALTER TABLE SinhVien ADD EncryptedKey VARCHAR2(1000);

-- 4. Thêm c?t ChuKySo: Ch?a ch? k? RSA xác th?c sinh viên (M? hóa B?t ð?i x?ng)
ALTER TABLE SinhVien ADD ChuKySo VARCHAR2(2000);

COMMIT;

-- Ch?y test d? li?u 
SELECT MaSV, TenDangNhap, MatKhau, SDT, EncryptedKey, ChuKySo 
FROM SinhVien 
ORDER BY MaSV DESC;


SELECT Avatar FROM SinhVien
