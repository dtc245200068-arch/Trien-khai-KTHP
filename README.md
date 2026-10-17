# HỆ THỐNG QUẢN LÝ KHO HÀNG - KHOFLOW

## 1. Giới thiệu

KhoFlow là hệ thống quản lý kho hàng được xây dựng phục vụ bài thi kết thúc học phần
**Triển khai và Quản trị Hệ thống Phần mềm**.

Hệ thống hỗ trợ các nghiệp vụ:

- Đăng nhập và phân quyền
- Quản lý danh mục
- Quản lý nhà cung cấp
- Quản lý sản phẩm
- Nhập kho
- Xuất kho
- Theo dõi tồn kho
- Xem lịch sử nhập/xuất kho

## 2. Công nghệ sử dụng

- ASP.NET Core MVC
- Entity Framework Core
- MySQL 8.0
- phpMyAdmin
- Docker
- Docker Compose
- Nginx Reverse Proxy
- Prometheus
- Grafana
- Loki
- Promtail

## 3. Kiến trúc hệ thống

Người dùng
        |
        v
Nginx Reverse Proxy
        |
        v
ASP.NET Core MVC
        |
        v
MySQL
        |
        v
phpMyAdmin

Hệ thống giám sát:
Prometheus + Grafana

Hệ thống log tập trung:
Promtail + Loki + Grafana

## 4. Cấu trúc dự án

```text
inventory-final-exam/
├── InventoryManagement/     # Source ASP.NET Core MVC
├── nginx/                   # Cấu hình Nginx
├── docker-compose.yml       # Docker Compose
├── .env.example             # Mẫu biến môi trường
├── .gitignore
└── README.md
