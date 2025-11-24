# 🧥 Uniqlo-Inspired Web Store (ASP.NET MVC)

## 🏗️ Giới thiệu
Dự án **Uniqlo-Inspired Web Store** là một website thương mại điện tử mô phỏng theo phong cách của **Uniqlo**, được phát triển bằng **ASP.NET MVC**.  
Mục tiêu: xây dựng hệ thống bán hàng online với thiết kế tối giản, tốc độ cao, trải nghiệm người dùng mượt mà.

---

## 🚀 Công nghệ sử dụng
| Thành phần | Công nghệ |
|-------------|------------|
| Backend | ASP.NET MVC (.NET Framework / .NET 6+) |
| Frontend | HTML5, CSS3, Bootstrap, jQuery |
| CSDL | SQL Server |
| ORM | Entity Framework |
| IDE | Visual Studio 2022 |
| Version Control | Git + GitHub |

---

## ✨ Tính năng chính
- 🛍️ **Trang chủ** hiển thị sản phẩm, banner, danh mục.
- 👕 **Trang chi tiết sản phẩm** với ảnh, mô tả, giá, kích thước.
- 🛒 **Giỏ hàng** (Cart) – thêm / xóa / cập nhật sản phẩm.
- 👤 **Đăng nhập & đăng ký tài khoản** (authentication cơ bản).
- 💳 **Thanh toán demo** (mô phỏng đơn hàng).
- 🔍 **Tìm kiếm và lọc sản phẩm** theo danh mục, giá, giới tính.
- 🧩 **Trang admin** quản lý sản phẩm, đơn hàng, người dùng.

---

## ⚙️ Cấu trúc thư mục
```plaintext
├── Controllers/
│   ├── HomeController.cs
│   ├── ProductController.cs
│   ├── CartController.cs
│   └── AccountController.cs
├── Models/
│   ├── Product.cs
│   ├── User.cs
│   └── Order.cs
├── Views/
│   ├── Home/
│   ├── Product/
│   ├── Cart/
│   └── Shared/
├── Content/
│   ├── css/
│   ├── images/
│   └── js/
├── App_Data/
├── web.config
└── README.md
Hướng dẫn cài đặt

Clone repo:

git clone https://github.com/<tên-user>/<tên-repo>.git


Mở bằng Visual Studio
→ Chọn Restore NuGet Packages
→ Chạy project bằng Ctrl + F5

Cấu hình database trong file web.config:

<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=.;Initial Catalog=UniqloStore;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>


Chạy migration (nếu dùng EF Code First):

Update-Database

🧠 Định hướng phát triển

✅ Responsive UI (tối ưu cho mobile)

✅ Hoàn thiện module thanh toán

🚧 Tích hợp API vận chuyển (Giao Hàng Nhanh, VNPost)

🚧 Triển khai Azure / AWS Hosting

🚧 Tối ưu SEO & performance

👥 Team & Credit

Leader / Dev chính: Minh

UI Inspiration: Uniqlo Official Website

Framework: ASP.NET MVC

📜 Giấy phép

Dự án mang tính học tập / demo – không sử dụng cho mục đích thương mại.
