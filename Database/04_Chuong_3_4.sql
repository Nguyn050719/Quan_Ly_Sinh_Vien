--PH?N 2: Cài ð?t ?ng d?ng Phân quy?n (App Permission)
-- Thêm c?t VaiTro
ALTER TABLE QuanLySinhVien.SinhVien ADD VaiTro VARCHAR2(20);
select * from Diem_SinhVien
-- Thêm d? li?u m?u ð? test phân quy?n trên C#
-- Gi? s? SV001 là Admin

-- 1. XÓA D? LI?U C? (Ð? tránh l?i trùng l?p khi ch?y nhi?u l?n)
DELETE FROM SinhVien_Lop;
DELETE FROM Diem_MonHoc;
DELETE FROM Diem_SinhVien;
DELETE FROM Diem;
DELETE FROM SinhVien;
DELETE FROM MonHoc;
DELETE FROM Lop;
COMMIT;
DELETE FROM SinhVien; COMMIT;
-- 2. THÊM D? LI?U B?NG L?P
INSERT INTO Lop (MaLop, TenLop, SiSo) VALUES ('L01', 'ATTT_K14', 50);
INSERT INTO Lop (MaLop, TenLop, SiSo) VALUES ('L02', 'QTM_K14', 45);

-- 3. THÊM D? LI?U MÔN H?C
INSERT INTO MonHoc (MaMH, TenMH, SoTinChi) VALUES ('MH001', 'Bao Mat Co So Du Lieu', 3);
INSERT INTO MonHoc (MaMH, TenMH, SoTinChi) VALUES ('MH002', 'Quan Tri Mang', 4);
INSERT INTO MonHoc (MaMH, TenMH, SoTinChi) VALUES ('MH003', 'Tri Tue Nhan Tao', 3);

-- 4. THÊM SINH VIÊN (Bao g?m c? Admin và SV thý?ng)
-- Lýu ?: M?t kh?u ð? là '123' (V? code AuthManager.cs c?a b?n ðang return password chýa bãm)
-- Admin: Có quy?n qu?n l?
INSERT INTO SinhVien (MaSV, HoTen, NgaySinh, SDT, DiaChi, Email, TenDangNhap, MatKhau, VaiTro) 
VALUES ('ADMIN01', 'Quan Tri Vien', TO_DATE('1990-01-01', 'YYYY-MM-DD'), '0909123456', 'Hanoi', 'admin@school.edu', 'admin', '123', 'ADMIN');

-- Sinh viên 1: H?c l?p ATTT
INSERT INTO SinhVien (MaSV, HoTen, NgaySinh, SDT, DiaChi, Email, TenDangNhap, MatKhau, VaiTro) 
VALUES ('SV001', 'Nguyen Van An', TO_DATE('2003-05-10', 'YYYY-MM-DD'), '0901234567', 'TPHCM', 'an.nv@school.edu', 'sv001', '123', 'SV');

-- Sinh viên 2: H?c l?p M?ng (Ð? test VPD - SV001 không ðý?c th?y ði?m SV002)
INSERT INTO SinhVien (MaSV, HoTen, NgaySinh, SDT, DiaChi, Email, TenDangNhap, MatKhau, VaiTro) 
VALUES ('SV002', 'Tran Thi Binh', TO_DATE('2003-08-20', 'YYYY-MM-DD'), '0908765432', 'Danang', 'binh.tt@school.edu', 'sv002', '123', 'SV');

-- 5. PHÂN L?P CHO SINH VIÊN
INSERT INTO SinhVien_Lop (FK_SV_SL, FK_Lop_SL) VALUES ('SV001', 'L01');
INSERT INTO SinhVien_Lop (FK_SV_SL, FK_Lop_SL) VALUES ('SV002', 'L02');

-- 6. NH?P ÐI?M (Quan tr?ng ð? test VPD và OLS)
-- Ði?m c?a SV001 (An)
select * from DIEM;
INSERT INTO Diem (MaDiem, DiemSo, FK_SV_Diem, FK_MH_Diem) VALUES ('D001', 9.5, 'SV001', 'MH001'); -- Ði?m cao (Test OLS: Confidential)
INSERT INTO Diem (MaDiem, DiemSo, FK_SV_Diem, FK_MH_Diem) VALUES ('D002', 6.0, 'SV001', 'MH002'); -- Ði?m th?p (Test OLS: Public)
COMMIT;
-- Ði?m c?a SV002 (Binh)
INSERT INTO Diem (MaDiem, DiemSo, FK_SV_Diem, FK_MH_Diem) VALUES ('D003', 9.0, 'SV002', 'MH001');
INSERT INTO Diem (MaDiem, DiemSo, FK_SV_Diem, FK_MH_Diem) VALUES ('D004', 7.5, 'SV002', 'MH003');

-- 7. LIÊN K?T B?NG TRUNG GIAN (Ð? query JOIN ho?t ð?ng ðúng)
INSERT INTO Diem_MonHoc (FK_Diem_DMH, FK_MH_DMH) VALUES ('D001', 'MH001');
INSERT INTO Diem_MonHoc (FK_Diem_DMH, FK_MH_DMH) VALUES ('D002', 'MH002');
INSERT INTO Diem_MonHoc (FK_Diem_DMH, FK_MH_DMH) VALUES ('D003', 'MH001');
INSERT INTO Diem_MonHoc (FK_Diem_DMH, FK_MH_DMH) VALUES ('D004', 'MH003');

INSERT INTO Diem_SinhVien (FK_SV_DiemSV, FK_Diem_DiemSV) VALUES ('SV001', 'D001');
INSERT INTO Diem_SinhVien (FK_SV_DiemSV, FK_Diem_DiemSV) VALUES ('SV001', 'D002');
INSERT INTO Diem_SinhVien (FK_SV_DiemSV, FK_Diem_DiemSV) VALUES ('SV002', 'D003');
INSERT INTO Diem_SinhVien (FK_SV_DiemSV, FK_Diem_DiemSV) VALUES ('SV002', 'D004');

COMMIT;

UPDATE SinhVien 
SET MatKhau = 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3' 
WHERE MaSV = 'ADMIN01';

-- C?p nh?t luôn cho SV001 và SV002 ð? test cho ti?n
UPDATE SinhVien 
SET MatKhau = 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3' 
WHERE MaSV IN ('SV001', 'SV002');

UPDATE SinhVien 
SET EncryptedKey = 'RCOsm8GnieSbyssaeuWdcfxknpuScrk88Oey3uBB/YNf/GyISWQ3c6FUgKFk6pk8o8H1zcgxfYhQJtqTPr3wl4sHRV6mZsIWqAtK/KAL1rgX1sqPcdX3Vdg8FJcJQEXDnsDNbjrNOtQFJDooex8wgI8ka6GXnfCZOHRq91nFNTnXXZ8JjXKQbRAeYpZCB5YBIGrxvOilf4A3BrKFFiMttpIOOUmZAWOFgwZqbAI9zUKWZII3wqwCXj77sz6RY18nrqjWNu8ztmG7je4F1STAjDPjytMnHYjaoX1zNPVugPGi4vrc3tJGeEWICY7AC32Ih0t9kJ9zW2xv2yPASo3ysA==' 
WHERE MaSV = 'ADMIN01';

UPDATE SinhVien 
SET ChuKySo = 'kC3Nmgiz48ajkXWU9eudseYPCvK4/jNUyr3DXtiUwyO1wUUSr4C6QyVk0Au96WeaQ4aPMOKJCq6hJ3oIsWTnwxwsH2QDR9kdppBEF0yEvK9mN7Jeu3JoKGWR+V1eaK8BjbzEfocYVM0jDwuNOfXE4cEGcfJ3eYRqeW79UnA7aT/08tblbq8LghN/IZCQwgOFX3WfUhZmY2HxL4l+GglWqVtGCveK3hD3Sp0eJoMmGE7X5Z7K4KjqufNt2QWUvkc2llfV1VSGEj/vXXmA0E1JzcFmsi1MsSgv6lJxWDygNqobIlVrSi7Ffvty3ZpCxgtIRkSdsrDO1rh8biOtEAwgMQ==' 
WHERE MaSV = 'ADMIN01';
-- 1. Gi? l?p SV001 ðãng nh?p
EXEC Pkg_Security.Set_User('SV001');

-- 2. Xem b?ng ði?m (Lúc này Oracle t? ð?ng filter ng?m)
SELECT * FROM Diem;

-- 1. Gi? l?p SV002 ðãng nh?p
EXEC Pkg_Security.Set_User('SV002');

-- 2. Xem b?ng ði?m
SELECT * FROM Diem;

-- 1. Gi? l?p Admin
EXEC Pkg_Security.Set_User('admin');
-- 2. Xem b?ng ði?m
SELECT * FROM Diem;

SELECT * FROM SinhVien;

SELECT MaDiem, DiemSo, LABEL_TO_CHAR(OLS_LABEL) AS Nhan_Bao_Mat 
FROM QuanLySinhVien.Diem;

SELECT 
    MaDiem, 
    DiemSo, 
    LABEL_TO_CHAR(OLS_LABEL) AS Nhan_Bao_Mat 
FROM Diem;

--Ki?m th?
SELECT table_name, tablespace_name 
FROM user_tables 
WHERE table_name IN ('SINHVIEN', 'DIEM');

ROLLBACK;

UPDATE QuanLySinhVien.Diem 
SET OLS_LABEL = CHAR_TO_LABEL('ACCESS_DIEM', 'CONF')
WHERE DiemSo >= 8.0;

-- Dán nh?n PUB cho ði?m th?p
UPDATE QuanLySinhVien.Diem 
SET OLS_LABEL = CHAR_TO_LABEL('ACCESS_DIEM', 'PUB')
WHERE DiemSo < 8.0;

--chýõng 4
 CREATE TABLE NhatKyHeThong (
    ID NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    TaiKhoan VARCHAR2(50),
    HanhDong VARCHAR2(100), -- Ví d?: "Ðãng nh?p", "Xem ði?m"
    ThoiGian TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    TrangThai VARCHAR2(50), -- "Thành công", "Th?t b?i"
    GhiChu VARCHAR2(255)
);

select * from SinhVien;
-- Ki?m tra gi? hi?n t?i
SELECT SYSTIMESTAMP FROM DUAL; 
-- (Ví d?: 10:00:00)

-- Xóa m?t sinh viên
INSERT INTO SinhVien (MaSV, HoTen, NgaySinh, SDT, DiaChi, Email, TenDangNhap, MatKhau, VaiTro) 
VALUES ('SV01', 'Nguy?n Vãn T', TO_DATE('1990-01-01', 'YYYY-MM-DD'), '0909123456', 'Hanoi', 'admin@school.edu', 'sv01', '123', 'SV');
DELETE FROM SinhVien WHERE MaSV = 'SV01';
COMMIT;

-- Ki?m tra l?i: M?t tiêu r?i
SELECT * FROM SinhVien WHERE MaSV = 'SV01';


-- Xem l?i d? li?u cách ðây 15 phút (trý?c khi xóa)
SELECT * FROM SinhVien AS OF TIMESTAMP (SYSTIMESTAMP - INTERVAL '15' MINUTE)
WHERE MaSV = 'SV01';

-- Khôi ph?c l?i d?ng ð? xóa t? quá kh?
INSERT INTO SinhVien
(SELECT * FROM SinhVien AS OF TIMESTAMP (SYSTIMESTAMP - INTERVAL '1' MINUTE)
 WHERE MaSV = 'SV01');

COMMIT;

select * from NhatKyHeThong;

-- 1. T?o d? li?u nháp
INSERT INTO SinhVien (MaSV, HoTen, TenDangNhap) VALUES ('SV_RESTORE', 'Test Phuc Hoi', 'test');
COMMIT;
SELECT * FROM SinhVien;

-- 2. Ch? 5 giây (ð? Oracle ghi nh?n timestamp)
EXEC DBMS_LOCK.SLEEP(5);

-- 3. Xóa nó ði
DELETE FROM SinhVien WHERE MaSV = 'SV_RESTORE';
COMMIT;

-- 4. Ki?m tra: Ð? m?t tiêu (CH?P H?NH 1: M?t d? li?u)
SELECT * FROM SinhVien WHERE MaSV = 'SV_RESTORE';

-- 5. Xem l?i quá kh? (Flashback) - Lùi l?i 10 giây trý?c
-- (CH?P H?NH 2: Th?y d? li?u c? hi?n ra)
SELECT * FROM SinhVien AS OF TIMESTAMP (SYSTIMESTAMP - INTERVAL '10' SECOND)
WHERE MaSV = 'SV_RESTORE';

-- 6. Khôi ph?c l?i

INSERT INTO SinhVien
(SELECT * FROM SinhVien AS OF TIMESTAMP (SYSTIMESTAMP - INTERVAL '10' SECOND)
 WHERE MaSV = 'SV_RESTORE');
 SELECT * FROM SinhVien WHERE MaSV = 'SV_RESTORE';
COMMIT;

-- 7. Ki?m tra k?t qu? (CH?P H?NH 3: D? li?u ð? v?)
-- 1. Gi? l?p vi?c App g?i tên ngý?i dùng xu?ng (Set Context th? công)
EXEC Pkg_Security.Set_User('SV001'); 

-- 2. Th?c hi?n hành ð?ng SELECT (Ð? kích ho?t Audit)
-- (Lýu ?: B?n ph?i ch?c ch?n ð? b?t AUDIT SELECT ON SINHVIEN trý?c ðó b?ng SYS)
SELECT * FROM SinhVien;

-- 3. Ð?i sang user Admin và làm l?i thao tác
EXEC Pkg_Security.Set_User('ADMIN');
SELECT * FROM SinhVien;