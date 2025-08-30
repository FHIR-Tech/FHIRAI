Tuyệt — dưới đây là **các lệnh terminal “đúng theo tài liệu”** để khởi tạo dự án Clean Architecture **.NET 9 template**, chọn **PostgreSQL**, và sinh **use cases** (CQRS) cho module **News** (Create/Update/Delete/Publish/Get/List).

> Theo README/NuGet của template: dùng `ca-sln` để tạo solution, tuỳ chọn `--client-framework` và `--database` (hỗ trợ `postgresql`). Dùng `ca-usecase` để sinh command/query trong `src/Application`. ([GitHub][1], [NuGet][2])

---

# 1) Cài template & tạo solution (Web API + PostgreSQL)

```bash
# Cài template Clean Architecture (bản .NET 9)
dotnet new install Clean.Architecture.Solution.Template

# Tạo solution Web API-only + PostgreSQL
dotnet new ca-sln -o MyApp -cf None --database postgresql
```

Sau đó chạy API:

```bash
cd MyApp/src/Web
dotnet run
```

([NuGet][2], [GitHub][1])

---

# 2) Sinh use cases cho tính năng **News**

> Lệnh này chạy trong thư mục `MyApp/src/Application`. Nó tạo sẵn **Command/Query + Handler + Validator** theo chuẩn của template. ([NuGet][2])

```bash
cd ../Application
```

## 2.1 Commands

```bash
# Tạo bản tin
dotnet new ca-usecase \
  --name CreateNews \
  --feature-name News \
  --usecase-type command \
  --return-type Guid

# Cập nhật bản tin
dotnet new ca-usecase \
  --name UpdateNews \
  --feature-name News \
  --usecase-type command \
  --return-type bool

# Xoá bản tin
dotnet new ca-usecase \
  --name DeleteNews \
  --feature-name News \
  --usecase-type command \
  --return-type bool

# Đổi trạng thái xuất bản
dotnet new ca-usecase \
  --name PublishNews \
  --feature-name News \
  --usecase-type command \
  --return-type bool
```

## 2.2 Queries

```bash
# Lấy chi tiết theo Id
dotnet new ca-usecase \
  --name GetNewsById \
  --feature-name News \
  --usecase-type query \
  --return-type NewsDto

# Danh sách có phân trang/lọc
dotnet new ca-usecase \
  --name GetNewsPaged \
  --feature-name News \
  --usecase-type query \
  --return-type 'PagedResult<NewsDto>'
```

> Tham số chuẩn của generator: `--name`, `--feature-name`, `--usecase-type (command|query)`, `--return-type`. Bạn có thể kiểm tra thêm bằng `dotnet new ca-usecase --help`. ([NuGet][2])

---

# 3) Khởi tạo Entity + DbContext + Migration (PostgreSQL)

> Template đã cấu hình sẵn EF Core cho provider bạn chọn. Với **PostgreSQL**, chỉ cần tạo entity + migration + update DB. (Nếu cần cú pháp `dotnet ef`, xem docs EF Core chuẩn của .NET).

Tạo migration & cập nhật DB:

```bash
cd ../Web
dotnet tool restore  # đảm bảo dotnet-ef tool sẵn sàng

# Tạo migration (đặt tên tuỳ ý, ví dụ "Init_News")
dotnet ef migrations add Init_News \
  --project ../Infrastructure \
  --startup-project .

# Cập nhật DB
dotnet ef database update \
  --project ../Infrastructure \
  --startup-project .
```

---

# 4) Chạy ứng dụng

```bash
dotnet run
```

---

## Ghi chú nhanh

* Tham số hợp lệ khi tạo solution (template .NET 9):

  * `-cf|--client-framework` = `Angular | React | None`
  * `--database` hỗ trợ `postgresql | sqlite | sqlserver`
  * Lệnh mẫu và phần “Use Case generator” đều có trong README/NuGet của template. ([GitHub][1], [NuGet][2])
* Nếu bạn muốn **target .NET 8** sau khi tạo bằng template .NET 9: đổi `<TargetFramework>net9.0</TargetFramework>` → `net8.0` trong các `.csproj` và hạ EFCore/Npgsql về 8.x. (Không bắt buộc nếu bạn để .NET 9.)

Bạn muốn mình xuất thêm **bộ lệnh sinh DTO/Mapping + ví dụ tham số cho GetNewsPaged** (filter theo `Title`, `Category`, `IsPublished`, `date range`) để dán vào Handler cho chạy ngay không?

[1]: https://github.com/jasontaylordev/CleanArchitecture?utm_source=chatgpt.com "GitHub - jasontaylordev/CleanArchitecture: Clean Architecture Solution Template for ASP.NET Core"
[2]: https://www.nuget.org/packages/Clean.Architecture.Solution.Template?utm_source=chatgpt.com "NuGet Gallery | Clean.Architecture.Solution.Template 9.0.12"
