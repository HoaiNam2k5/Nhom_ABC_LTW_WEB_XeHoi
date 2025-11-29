# 🚗 WebCar - Website Quản Lý & Mua Bán Xe Hơi

<p align="center">
  <img src="https://img.shields.io/badge/.NET%20Framework-4.8-blue?style=for-the-badge&logo=dotnet" alt=".NET Framework">
  <img src="https://img.shields.io/badge/ASP.NET%20MVC-5.2.9-purple?style=for-the-badge" alt="ASP.NET MVC">
  <img src="https://img.shields.io/badge/Bootstrap-5.2.3-blueviolet?style=for-the-badge&logo=bootstrap" alt="Bootstrap">
  <img src="https://img.shields.io/badge/SQL%20Server-2019+-red?style=for-the-badge&logo=microsoftsqlserver" alt="SQL Server">
</p>

> 🎓 **Đồ án môn Lập Trình Web** - Website quản lý, giới thiệu và mua bán xe hơi trực tuyến

---

## 📋 Mục lục

- [Giới thiệu](#-giới-thiệu)
- [Tính năng chính](#-tính-năng-chính)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Hướng dẫn cài đặt](#-hướng-dẫn-cài-đặt)
- [Hướng dẫn sử dụng](#-hướng-dẫn-sử-dụng)
- [Cấu trúc thư mục](#-cấu-trúc-thư-mục)
- [Thành viên nhóm](#-thành-viên-nhóm)
- [License](#-license)
- [Liên hệ](#-liên-hệ)

---

## 📖 Giới thiệu

**WebCar** là một ứng dụng web hoàn chỉnh được xây dựng để phục vụ việc **quản lý, giới thiệu và mua bán xe hơi trực tuyến**. Dự án được phát triển như một đồ án môn học Lập Trình Web, thể hiện khả năng áp dụng các kiến thức về:

- Phát triển web với ASP.NET MVC
- Thiết kế và quản trị cơ sở dữ liệu SQL Server
- Xây dựng giao diện người dùng responsive với Bootstrap
- Tích hợp các công nghệ frontend và backend

### 🎯 Mục tiêu dự án

- Xây dựng một website thương mại điện tử hoàn chỉnh cho lĩnh vực xe hơi
- Áp dụng kiến thức từ môn Lập Trình Web và Hệ quản trị CSDL
- Thể hiện kỹ năng sử dụng HTML, CSS, JavaScript, C#, SQL Server
- Phát triển hệ thống phân quyền người dùng đầy đủ

---

## ✨ Tính năng chính

### 👤 Dành cho Khách hàng
- 🔍 **Xem danh sách xe** - Duyệt qua các loại xe với phân trang
- 🔎 **Tìm kiếm & Lọc** - Tìm kiếm xe theo tên, hãng, giá, năm sản xuất
- 📝 **Xem chi tiết xe** - Thông tin chi tiết về từng chiếc xe
- 🛒 **Đặt hàng** - Đặt mua xe trực tuyến
- 📋 **Quản lý đơn hàng** - Xem và theo dõi trạng thái đơn hàng
- 👤 **Tài khoản cá nhân** - Đăng ký, đăng nhập, quản lý thông tin

### 👨‍💼 Dành cho Quản trị viên (Admin/Manager)
- ➕ **Quản lý xe** - Thêm, sửa, xóa thông tin xe
- 📦 **Quản lý đơn hàng** - Cập nhật trạng thái đơn hàng
- 👥 **Quản lý người dùng** - Phân quyền và quản lý tài khoản
- 📊 **Theo dõi hoạt động** - Xem audit log hoạt động hệ thống

---

## 🛠 Công nghệ sử dụng

### Backend
| Công nghệ | Phiên bản | Mô tả |
|-----------|-----------|-------|
| .NET Framework | 4.8 | Nền tảng phát triển |
| ASP.NET MVC | 5.2.9 | Framework web |
| Entity Framework | 6.2.0 | ORM cho cơ sở dữ liệu |
| C# | - | Ngôn ngữ lập trình |

### Frontend
| Công nghệ | Phiên bản | Mô tả |
|-----------|-----------|-------|
| Bootstrap | 5.2.3 | Framework CSS responsive |
| jQuery | 3.7.0 | Thư viện JavaScript |
| Bootstrap Icons | 1.11.3 | Bộ icon |
| HTML5/CSS3 | - | Ngôn ngữ đánh dấu và định dạng |

### Database
| Công nghệ | Phiên bản | Mô tả |
|-----------|-----------|-------|
| SQL Server | 2019+ | Hệ quản trị CSDL |

### Thư viện khác
- **Newtonsoft.Json** (13.0.3) - Xử lý JSON
- **jQuery Validation** (1.19.5) - Validate form
- **Microsoft Web Optimization** (1.1.3) - Bundling & Minification

---

## 💻 Yêu cầu hệ thống

### Phần mềm cần thiết
- **Visual Studio 2019** hoặc cao hơn (khuyến nghị VS 2022)
- **SQL Server 2019** hoặc cao hơn (hoặc SQL Server Express)
- **SQL Server Management Studio (SSMS)** - Công cụ quản lý database
- **.NET Framework 4.8** - Runtime

### Yêu cầu phần cứng (tối thiểu)
- **RAM:** 4GB (khuyến nghị 8GB)
- **Ổ cứng:** 10GB dung lượng trống
- **CPU:** Intel Core i3 hoặc tương đương

### Trình duyệt hỗ trợ
- Google Chrome (khuyến nghị)
- Microsoft Edge
- Mozilla Firefox
- Safari

---

## 🚀 Hướng dẫn cài đặt

### Bước 1: Clone Repository

```bash
git clone https://github.com/HoaiNam2k5/Nhom_ABC_LTW_WEB_XeHoi.git
cd Nhom_ABC_LTW_WEB_XeHoi
```

### Bước 2: Thiết lập Database

1. Mở **SQL Server Management Studio (SSMS)**
2. Kết nối đến SQL Server instance của bạn
3. Mở và thực thi file `CarSaleScrip.sql` hoặc `Carsale.sql` để tạo database

```sql
-- Chạy script tạo database
-- File: CarSaleScrip.sql hoặc Carsale.sql
```

### Bước 3: Cấu hình Connection String

1. Mở file `WebCar/WebCar/Web.config`
2. Cập nhật connection string theo cấu hình SQL Server của bạn:

```xml
<connectionStrings>
  <add name="CARSALE_DB" 
       connectionString="Data Source=TEN_SERVER\SQLEXPRESS;Initial Catalog=CARSALE_DB;User ID=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> ⚠️ **Lưu ý:** Thay `TEN_SERVER` bằng tên SQL Server instance của bạn và `YOUR_PASSWORD` bằng mật khẩu.

### Bước 4: Mở và Chạy Dự án

1. Mở file `WebCar/WebCar.sln` bằng **Visual Studio**
2. Restore NuGet Packages: `Tools → NuGet Package Manager → Restore NuGet Packages`
3. Build Solution: `Ctrl + Shift + B` hoặc `Build → Build Solution`
4. Chạy ứng dụng: `F5` hoặc `Debug → Start Debugging`

---

## 📘 Hướng dẫn sử dụng

### Đăng ký & Đăng nhập

1. Truy cập trang web
2. Click vào **"Đăng ký"** để tạo tài khoản mới
3. Điền đầy đủ thông tin: Họ tên, Email, Số điện thoại, Mật khẩu, Địa chỉ
4. Click **"Đăng nhập"** và nhập email/mật khẩu

### Tìm kiếm và Xem xe

1. Truy cập trang **"Sản phẩm"** hoặc **"Danh sách xe"**
2. Sử dụng bộ lọc để tìm kiếm theo:
   - 🔤 Tên xe
   - 🏭 Hãng xe
   - 💰 Khoảng giá (Min - Max)
   - 📅 Năm sản xuất
3. Click vào xe để xem **chi tiết**

### Đặt hàng

1. Xem chi tiết xe muốn mua
2. Click **"Đặt hàng"** (cần đăng nhập)
3. Nhập số lượng, địa chỉ giao hàng, số điện thoại
4. Xác nhận đơn hàng

### Quản lý đơn hàng

1. Truy cập **"Đơn hàng của tôi"**
2. Xem danh sách và trạng thái đơn hàng
3. Có thể hủy đơn hàng nếu còn ở trạng thái chờ xử lý

### Chức năng Admin

1. Đăng nhập với tài khoản Admin
2. Truy cập **Dashboard Admin**
3. Quản lý:
   - 🚗 Thêm/Sửa/Xóa xe
   - 📦 Cập nhật trạng thái đơn hàng
   - 👥 Quản lý người dùng
   - 📊 Xem báo cáo và audit log

---

## 📁 Cấu trúc thư mục

```
Nhom_ABC_LTW_WEB_XeHoi/
│
├── 📂 WebCar/                          # Solution folder
│   ├── 📄 WebCar.sln                   # Visual Studio Solution file
│   │
│   └── 📂 WebCar/                      # Main project folder
│       ├── 📂 App_Start/               # Cấu hình khởi động ứng dụng
│       │   ├── BundleConfig.cs
│       │   ├── FilterConfig.cs
│       │   └── RouteConfig.cs
│       │
│       ├── 📂 Controllers/             # Controllers (MVC)
│       │   ├── AccountController.cs    # Xử lý đăng nhập/đăng ký
│       │   ├── AdminController.cs      # Quản trị hệ thống
│       │   ├── AuditController.cs      # Audit log
│       │   ├── ErrorController.cs      # Xử lý lỗi
│       │   ├── HomeController.cs       # Trang chủ
│       │   ├── OrderController.cs      # Quản lý đơn hàng
│       │   └── ProductController.cs    # Quản lý sản phẩm (xe)
│       │
│       ├── 📂 Models/                  # Data Models (Entity Framework)
│       │   ├── ACCOUNT_ROLE.cs         # Model phân quyền
│       │   ├── AUDIT_LOG.cs            # Model audit log
│       │   ├── CAR.cs                  # Model xe
│       │   ├── CUSTOMER.cs             # Model khách hàng
│       │   ├── FEEDBACK.cs             # Model phản hồi
│       │   ├── ORDER.cs                # Model đơn hàng
│       │   ├── ORDER_DETAIL.cs         # Model chi tiết đơn hàng
│       │   └── ViewModels/             # View Models
│       │
│       ├── 📂 Views/                   # Views (Razor)
│       │   ├── 📂 Account/             # Views đăng nhập/đăng ký
│       │   ├── 📂 Admin/               # Views quản trị
│       │   ├── 📂 Audit/               # Views audit log
│       │   ├── 📂 Home/                # Views trang chủ
│       │   ├── 📂 Order/               # Views đơn hàng
│       │   ├── 📂 Product/             # Views sản phẩm
│       │   ├── 📂 Shared/              # Layout và Partial views
│       │   └── _ViewStart.cshtml
│       │
│       ├── 📂 Content/                 # CSS, Fonts
│       │   ├── 📂 CSS/                 # Custom CSS
│       │   ├── Site.css                # Main stylesheet
│       │   └── bootstrap*.css          # Bootstrap CSS
│       │
│       ├── 📂 Scripts/                 # JavaScript files
│       │
│       ├── 📂 Data/                    # Data Access Layer
│       │
│       ├── 📂 Filters/                 # Custom Filters (Authorization)
│       │
│       ├── 📂 images/                  # Hình ảnh
│       │
│       ├── 📂 video/                   # Video
│       │
│       ├── 📄 Web.config               # Cấu hình ứng dụng
│       ├── 📄 Global.asax              # Application lifecycle
│       ├── 📄 packages.config          # NuGet packages
│       └── 📄 WebCar.csproj            # Project file
│
├── 📄 CarSaleScrip.sql                 # Script tạo database (đầy đủ)
├── 📄 Carsale.sql                      # Script database (phiên bản khác)
└── 📄 README.md                        # File hướng dẫn này
```

---

## 👥 Thành viên nhóm

### Nhóm ABC - Lập Trình Web

| STT | Họ và Tên | Vai trò | Nhiệm vụ |
|-----|-----------|---------|----------|
| 1 | Thành viên 1 | Team Leader | Quản lý dự án, phát triển Backend |
| 2 | Thành viên 2 | Developer | Phát triển Frontend, UI/UX |
| 3 | Thành viên 3 | Developer | Database, Testing |

> 📚 **Môn học:** Lập Trình Web  
> 🏫 **Trường:** [Đại Học Công Thương TP.HCM]  
> 📅 **Năm học:** 2023-2026

---

## 📄 License

Dự án này được phát triển cho mục đích học tập và nghiên cứu.

```
MIT License

Copyright (c) 2024 Nhóm ABC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
```

---

## 📞 Liên hệ

### 📧 Email
- **Email dự án:** [Namtqn2@gmail.com]

### 🔗 Repository
- **GitHub:** [https://github.com/HoaiNam2k5/Nhom_ABC_LTW_WEB_XeHoi](https://github.com/HoaiNam2k5/Nhom_ABC_LTW_WEB_XeHoi)

### 🤝 Đóng góp

Mọi đóng góp đều được hoan nghênh! Để đóng góp:

1. **Fork** repository này
2. Tạo **branch** mới: `git checkout -b feature/TenTinhNang`
3. **Commit** thay đổi: `git commit -m 'Thêm tính năng mới'`
4. **Push** lên branch: `git push origin feature/TenTinhNang`
5. Tạo **Pull Request**

### ⭐ Ủng hộ dự án

Nếu dự án hữu ích, hãy cho chúng tôi một ⭐ trên GitHub!

---

<p align="center">
  <b>🚗 WebCar - Nhóm ABC - LTW 2024 🚗</b><br>
  Made with ❤️ by Team ABC
</p>

