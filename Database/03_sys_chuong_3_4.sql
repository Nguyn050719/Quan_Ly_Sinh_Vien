-- Ch??ng 3:
ALTER SESSION SET NLS_LANGUAGE = 'AMERICAN';
ALTER SESSION SET NLS_TERRITORY = 'AMERICA';
SELECT value FROM v$option WHERE parameter = 'Oracle Label Security';
--PH?N 1: Cài ð?t Cõ s? h? t?ng B?o m?t (Tablespace, Profile, User DB)
-- 1. T?o Tablespace (Nõi ch?a d? li?u riêng)
CREATE TABLESPACE TBS_QLSV 
DATAFILE 'C:\APP09\ORACLE\BASE\ORADATA\ORCLSV\DATAFILE\qlsv_data.dbf' 
SIZE 100M AUTOEXTEND ON NEXT 50M MAXSIZE 2048M;

-- 2. T?o Profile (Quy ð?nh chính sách m?t kh?u)
CREATE PROFILE PRO_SINHVIEN LIMIT
  FAILED_LOGIN_ATTEMPTS 3         -- Sai 3 l?n khóa
  PASSWORD_LIFE_TIME 30           -- M?t kh?u s?ng 30 ngày
  PASSWORD_LOCK_TIME 1            -- Khóa 1 ngày n?u vi ph?m
  SESSIONS_PER_USER 5;            -- Gi?i h?n session (Yêu c?u v? Session)ofile (Quy ð?nh chính sách m?t kh?u)

-- 3. T?o User và Role Database
-- T?o Role
CREATE ROLE ROLE_ADMIN;
CREATE ROLE ROLE_SINHVIEN;
CREATE ROLE ROLE_GIANGVIEN;

-- Gán quy?n cõ b?n cho Role
GRANT CREATE SESSION TO ROLE_SINHVIEN;
GRANT CREATE SESSION, CREATE TABLE TO ROLE_ADMIN;

-- T?o User k?t n?i cho ?ng d?ng (User này b?n ðang dùng trong C#)
DROP USER QuanLySinhVien CASCADE;
CREATE USER QuanLySinhVien IDENTIFIED BY 123    
DEFAULT TABLESPACE TBS_QLSV
PROFILE PRO_SINHVIEN; -- Áp d?ng Profile v?a t?o

DROP USER SV001 CASCADE;
CREATE USER SV001 IDENTIFIED BY 123    
DEFAULT TABLESPACE TBS_QLSV
PROFILE PRO_SINHVIEN;
GRANT CREATE SESSION TO SV001;

-- Gán quy?n cho User ?ng d?ng
GRANT CONNECT, RESOURCE TO QuanLySinhVien;
GRANT UNLIMITED TABLESPACE TO QuanLySinhVien;

--PH?N 2: Cài ð?t ?ng d?ng Phân quy?n (App Permission)

--PH?N 3: B?o m?t m?c nâng cao (FGA / VPD / OLS)
-- 1. T?o Context
CREATE OR REPLACE CONTEXT CTX_QLSV USING Pkg_Security;

-- 2. T?o Package qu?n l? Context (Ð? set UserID khi login)
CREATE OR REPLACE PACKAGE Pkg_Security AS
  PROCEDURE Set_User(p_user IN VARCHAR2);
END;
/

CREATE OR REPLACE PACKAGE BODY Pkg_Security AS
  PROCEDURE Set_User(p_user IN VARCHAR2) IS
  BEGIN
    DBMS_SESSION.SET_IDENTIFIER(p_user); -- Ð?t ð?nh danh session
  END;
END;
/

-- 3. Vi?t hàm Policy cho b?ng Diem
CREATE OR REPLACE FUNCTION Fn_VPD_Diem (
  p_schema IN VARCHAR2,
  p_object IN VARCHAR2
) RETURN VARCHAR2 AS
  v_user VARCHAR2(50);
BEGIN
  -- L?y User hi?n t?i t? session
  v_user := SYS_CONTEXT('USERENV', 'CLIENT_IDENTIFIER');
  
  -- N?u là Admin (ho?c chýa set user - trý?ng h?p ð?c bi?t) th? xem h?t
  -- Lýu ?: Logic này c?n kh?p v?i user admin b?n quy ð?nh
  IF v_user = 'admin' THEN 
    RETURN NULL; -- Không filter g? c?
  ELSE
    -- N?u là SV, ch? hi?n d?ng có FK_SV_Diem trùng v?i M? SV c?a h?
    -- Gi? s? TenDangNhap trùng v?i MaSV ð? ðõn gi?n hóa
    RETURN 'FK_SV_Diem = ''' || v_user || ''''; 
  END IF;
END;
/

-- 4. Áp d?ng Policy vào b?ng Diem
BEGIN
  DBMS_RLS.ADD_POLICY (
    object_schema => 'QuanLySinhVien',
    object_name   => 'Diem',
    policy_name   => 'VPD_XEM_DIEM',
    function_schema => 'QuanLySinhVien',
    policy_function => 'Fn_VPD_Diem',
    statement_types => 'SELECT'
  );
END;
/

--2. Cài ð?t OLS (Oracle Label Security) - Gán nh?n b?o m?t
--Setup OLS (Ch?y quy?n SYS)
-- Thêm c?t ch?a nh?n OLS vào b?ng Diem
BEGIN
  SA_SYSDBA.CREATE_POLICY(policy_name => 'ACCESS_DIEM', column_name => 'OLS_LABEL');
END;
/

-- T?o các Level b?o m?t
EXEC SA_COMPONENTS.CREATE_LEVEL('ACCESS_DIEM', 1000, 'PUB', 'Public');
EXEC SA_COMPONENTS.CREATE_LEVEL('ACCESS_DIEM', 2000, 'CONF', 'Confidential');

-- Áp d?ng vào b?ng Diem
BEGIN
  SA_POLICY_ADMIN.APPLY_TABLE_POLICY (
    policy_name => 'ACCESS_DIEM',
    schema_name => 'QuanLySinhVien',
    table_name  => 'Diem',
    table_options => 'READ_CONTROL, WRITE_CONTROL'
  );
END;
/

-- Ch?y l?nh này b?ng user ch? s? h?u b?ng (QuanLySinhVien) ho?c SYS
ALTER TABLE QuanLySinhVien.Diem DROP COLUMN OLS_LABEL;
BEGIN
  SA_POLICY_ADMIN.REMOVE_TABLE_POLICY(
    policy_name => 'ACCESS_DIEM',
    schema_name => 'QuanLySinhVien',
    table_name  => 'Diem'
  );
END;
/

-- Xóa Policy c? ði
BEGIN
  SA_SYSDBA.DROP_POLICY(policy_name => 'ACCESS_DIEM', drop_column => TRUE);
EXCEPTION 
  WHEN OTHERS THEN NULL; -- B? qua l?i n?u policy không t?n t?i
END;
/

--Dán nh?n d? li?u (Data Labeling)
-- C?p quy?n cho User DB ð? th?c hi?n update nh?n
GRANT ACCESS_DIEM_DBA TO QuanLySinhVien;

-- Ch?y b?ng User QuanLySinhVien ho?c SYS
-- Dán nh?n CONF cho ði?m cao
UPDATE QuanLySinhVien.Diem 
SET OLS_LABEL = CHAR_TO_LABEL('ACCESS_DIEM', 'CONF')
WHERE DiemSo >= 8.0;

-- Dán nh?n PUB cho ði?m th?p
UPDATE QuanLySinhVien.Diem 
SET OLS_LABEL = CHAR_TO_LABEL('ACCESS_DIEM', 'PUB')
WHERE DiemSo < 8.0;

--Gán nh?n cho User Quy ð?nh ai ðý?c xem nh?n nào.
-- User GIANGVIEN (n?u b?n t?o thêm user này) ðý?c xem h?t (CONF)
EXEC SA_USER_ADMIN.SET_USER_LABELS('ACCESS_DIEM', 'QuanLySinhVien', 'CONF', 'CONF', 'CONF', 'CONF');
-- ? ðây ta set cho QuanLySinhVien là CONF ð? App C# ch?y ðý?c, 
-- sau ðó dùng logic App ð? ?n hi?n. 
-- N?u mu?n demo chu?n OLS, b?n ph?i t?o User DB riêng cho t?ng SV.

SELECT MaDiem, FK_SV_Diem, DiemSo, LABEL_TO_CHAR(OLS_LABEL) as Nhan_Bao_Mat 
FROM QuanLySinhVien.Diem;

-- 1. T?m m? c?a ð? s?a
BEGIN 
   SA_POLICY_ADMIN.REMOVE_TABLE_POLICY('ACCESS_DIEM', 'QuanLySinhVien', 'Diem');
   SA_POLICY_ADMIN.APPLY_TABLE_POLICY (
     policy_name => 'ACCESS_DIEM', schema_name => 'QuanLySinhVien', table_name  => 'Diem',
     table_options => 'NO_CONTROL'
   );
END;
/

-- 2. Dán nh?n l?i (Ch?c ch?n ch?y d?ng này)
UPDATE QuanLySinhVien.Diem SET OLS_LABEL = CHAR_TO_LABEL('ACCESS_DIEM', 'CONF') WHERE DiemSo >= 8.0;
UPDATE QuanLySinhVien.Diem SET OLS_LABEL = CHAR_TO_LABEL('ACCESS_DIEM', 'PUB') WHERE DiemSo < 8.0;
COMMIT;

-- 3. Khóa c?a l?i
BEGIN
   SA_POLICY_ADMIN.REMOVE_TABLE_POLICY('ACCESS_DIEM', 'QuanLySinhVien', 'Diem');
   SA_POLICY_ADMIN.APPLY_TABLE_POLICY (
     policy_name => 'ACCESS_DIEM', schema_name => 'QuanLySinhVien', table_name  => 'Diem',
     table_options => 'READ_CONTROL, WRITE_CONTROL'
   );
END;
/

-- ki?m th?
SELECT table_name, tablespace_name 
FROM user_tables 
WHERE table_name IN ('SINHVIEN', 'DIEM');

-- Ch?y b?ng quy?n SYS ho?c User s? h?u b?ng
SELECT MaDiem, DiemSo, LABEL_TO_CHAR(OLS_LABEL) AS Nhan_Bao_Mat 
FROM QuanLySinhVien.Diem;

rollback;


-- Chýõng 4:
-- 1. Ki?m tra xem Audit ð? b?t chýa
SHOW PARAMETER AUDIT_TRAIL;
-- N?u value là 'NONE' th? ph?i ch?y d?ng dý?i và kh?i ð?ng l?i Oracle (nhýng thý?ng m?c ð?nh là DB)

-- 2. B?t ch? ð? giám sát phiên làm vi?c (Session)
AUDIT SESSION;

-- 3. Giám sát các hành ð?ng ðãng nh?p thành công & th?t b?i
AUDIT CREATE SESSION WHENEVER SUCCESSFUL;
AUDIT CREATE SESSION WHENEVER NOT SUCCESSFUL;

SELECT username, extended_timestamp, action_name, returncode FROM dba_audit_trail WHERE username='QUANLYSINHVIEN' ORDER BY extended_timestamp DESC;

-- 1. B?t giám sát hành ð?ng Select trên b?ng SinhVien
AUDIT SELECT ON QuanLySinhVien.SinhVien BY ACCESS;

-- 2. B?t giám sát hành ð?ng Select trên b?ng Diem (n?u chýa b?t)
AUDIT SELECT ON QuanLySinhVien.Diem BY ACCESS;


SELECT 
    EXTENDED_TIMESTAMP as ThoiGian,
    USERNAME as DB_User,      -- <--- Ð? S?A: Tên c?t ðúng là USERNAME
    CLIENT_ID as App_User,    -- Tên SV001/Admin s? hi?n ? ðây
    OBJ_NAME as DoiTuong,     -- Tên b?ng (SinhVien/Diem)
    ACTION_NAME as HanhDong,  -- SELECT/INSERT...
    SQL_TEXT                  -- Câu l?nh SQL c? th?
FROM DBA_AUDIT_TRAIL
WHERE USERNAME = 'QUANLYSINHVIEN'
  AND CLIENT_ID IS NOT NULL   -- Ch? l?y nh?ng d?ng có tên SV c? th?
ORDER BY EXTENDED_TIMESTAMP DESC;