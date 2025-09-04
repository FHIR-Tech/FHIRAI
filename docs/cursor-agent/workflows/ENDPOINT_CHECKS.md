Quy tắc KIỂM TRA ENDPOINT (on‑demand) — dùng .http (VS Code REST Client)
For more info on HTTP files go to https://aka.ms/vs/httpfile

# 🎯 Mục tiêu & Phạm vi
- Chuẩn hoá **cách chạy kiểm tra endpoint** khi được yêu cầu sau các thay đổi mapping/schema.
- **Tự động hóa** việc kiểm tra với script và template chuẩn.

# 🧰 Công cụ thực hiện (chọn 1 hoặc nhiều)
- **VS Code REST Client**: mở `.http` và chạy theo `@name`.
- **curl** (smoke test nhanh): `curl -k https://localhost:7121/api/WeatherForecasts`.
- **Khởi động API cục bộ**: `dotnet run --project src/web --launch-profile FHIRAI.Web` (**HTTPS 5001**, **HTTP 5000**) với mod background.

# 🚀 Quy trình kiểm tra nhanh (Automated)

## Bước 1: Khởi động API
```bash
# Kiểm tra port đang sử dụng
lsof -i :5001

# Khởi động API (nếu chưa chạy)
dotnet run --project src/web --launch-profile FHIRAI.Web
```

## Bước 2: Chạy script test tự động với curl hoặc đọc tệp `.http` và chạy theo `@name`

## Bước 3: Kiểm tra kết quả
Script sẽ trả về:
- ✅ **PASS**: Endpoint hoạt động tốt
- ❌ **FAIL**: Endpoint có lỗi (status code sai, JSON invalid)
- ⏭️ **SKIP**: Endpoint chưa implement (như /health, /metadata)

**Kỳ vọng**: Tất cả FHIR resources phải **PASS** (Search, Create, Read).

# 📄 Template chuẩn cho tệp .http

## Template chính: `scripts\http\_TEMPLATE.http`
Sử dụng template này để tạo tệp .http mới cho resource mới:

## Quy tắc sử dụng template:
1. **Triển khai đầy đủ** Endpoint thực (Auth/login, Auth/register, ...)
2. **Bổ sung payload** hợp lệ cho create/update
3. **Thêm test cases** cụ thể cho resource (validation, conditional operations)

# 🌍 Biến môi trường — cách khai báo & ưu tiên
REST Client hỗ trợ 3 lớp biến (độ ưu tiên từ cao đến thấp):
1) **Biến cục bộ trong file `.http`** (đầu file) → **cao nhất**.  
2) **Biến inline theo request** (ít dùng).

> **Quy ước quan trọng**: Nếu cần bổ sung biến như `{Email}`, `{Password}`, `{idSystem}`, `idValue` thì **khai báo tại đầu tệp `.http`** để minh bạch và dễ cập nhật.

## 1) Khai báo biến cục bộ (đầu file `.http`)
Ví dụ đầu file **`Csharp.Api/Http/Patient.http`**:
```http
@Web_HostAddress = https://localhost:5001
@BearerToken = <YourToken>

# Biến test cụ thể cho file này (ghi đè env nếu trùng tên)
@Email = administrator@localhost
@Password = Administrator1!
@idSystem = https://example.org/mrn
@idValue  = MRN-00001
```

# 📄 Quy tắc tổ chức HTTP files
- **Không** dồn tất cả vào một file; **mỗi tài nguyên 1 file**: `scripts/Http/Auth.http`, `.../Fhir.http`, v.v.
- **Sử dụng template chuẩn**: Copy từ `_TEMPLATE.http` và customize
- Đặt `@name` cho từng request để chạy/canary dễ dàng: `### @name auth_login`, `### @name auth_register`, ...
- **Không hard‑code URL**; dùng biến `{Web_HostAddress}`.
- **Headers FHIR chuẩn**: `Accept: application/fhir+json`, `Content-Type: application/fhir+json`

# 🧪 Ví dụ request (theo yêu cầu)
Ví dụ REST endpoint chung (không FHIR):
```http
### @name product_search
GET {Web_HostAddress}/api/products?name=phone&page=1&pageSize=20
Authorization: Bearer {{BearerToken}}
Accept: application/json
```

Ví dụ FHIR (Patient search + read):
```http
### @name patient_search
GET {Web_HostAddress}/fhir/Patient?_count=10
Authorization: Bearer {{BearerToken}}
Accept: application/fhir+json

### @name patient_read
GET {Web_HostAddress}/fhir/Patient/{patientId}
Authorization: Bearer {{BearerToken}}
Accept: application/fhir+json
```

> Với request có **body** (create/update), thêm: `Content-Type: application/fhir+json`.  
> Khi update theo version: `If-Match: W/"{patientVid}"`.

# 🚦 Quy trình kiểm tra nhanh (Patient)
1) **Smoke test**: `GET {Web_HostAddress}/Patient` → kỳ vọng `200 OK` + body hợp lệ (Bundle hoặc danh sách).  
2) **Luồng chính**:  
   - `patient_search` → `patient_create` (`Prefer: return=representation`) → `patient_read` → `patient_update` (ETag: `If-Match`) → `patient_delete` (nếu có).  
3) **Ghi nhận**: `Location` khi tạo; `meta.versionId` tăng sau update; `resourceType` khớp.

# 🧰 Cách chạy
- **VS Code REST Client**: mở `.http` và chạy theo `@name` (khuyến nghị).  
- **curl (smoke)**: `curl -k https://localhost:5001/fhir/Patient`.

# 🧪 Báo cáo kết quả (mẫu)
| Step | Name | Status | resourceType | id | versionId | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | patient_search | 200 | Bundle | — | — |  |
| 2 | patient_create | 201/200 | Patient | 123 | 1 | Có Location |
| 3 | patient_read | 200 | Patient | 123 | 1 |  |
| 4 | patient_update | 200/204 | Patient | 123 | 2 | ETag OK |
| 5 | patient_delete | 200/204 | — | — | — | (nếu áp dụng) |

## Cách mở rộng script cho resource mới:
1. **Thêm test case** vào script:
```bash
# Test new resource
print_status "INFO" "Testing NewResource endpoints..."
test_search "NewResource" "NewResource search"
test_create "NewResource" '{"resourceType":"NewResource",...}' "NewResource create"
```

2. **Tạo tệp .http** từ template:
```bash
cp scripts/Http/_TEMPLATE.http scripts/Http/NewResource.http
# Edit và customize
```

## Kết quả script:
- **Colored output**: ✅ PASS, ❌ FAIL, ⏭️ SKIP
- **JSON validation**: Kiểm tra response có phải JSON hợp lệ
- **FHIR validation**: Kiểm tra `resourceType`, `Bundle.type`
- **Summary report**: Tổng kết Passed/Failed/Skipped

# 🧯 Troubleshooting
- **Server/port**: chạy `dotnet run --launch-profile https`, kiểm tra `launchSettings.json`.  
- **SSL dev cert**: `dotnet dev-certs https --trust` hoặc tắt verify (dev) trong REST Client.  
- **Proxy/Firewall**: cấu hình/tạm vô hiệu hoá khi test cục bộ.  
- **Biến thiếu/không khớp**: kiểm tra phần **đầu file `.http`**.
- **Script lỗi**: Kiểm tra port 5001 đang chạy, dừng và thử khởi động lại.

# 🔐 Bảo mật
- **Không commit** token/secret thật; dùng variable per-env, hoặc secret manager cho non-dev.  
- `.http` có thể chứa token dev **tạm thời**, nhưng không push token sản xuất.

# 🔄 Quy trình Sửa lỗi & Vòng lặp phản hồi
> Sau khi chạy kiểm tra endpoint, nếu phát hiện lỗi hoặc sai lệch dữ liệu, **phải** thực hiện quy trình dưới đây.  
> Lưu ý: trước **mỗi thay đổi**, **đọc lại** rule tương ứng để bảo đảm tuân thủ.

## Bước 0 — Bắt buộc đọc lại rule liên quan
- Quy tắc Domain/EF & FHIR mapping (R4/R5, cardinality, datatype, binding).  
- Quy tắc AutoMapper + exception mapping.  
- Quy tắc kiểm tra endpoint (tài liệu **hiện tại**).  
- Quy ước API REST nội bộ (nếu có).  
- Quy tắc lai FHIR↔API (nếu sử dụng).

> Không tiến hành thay đổi khi **chưa đọc lại** các rule trên.

## Bước 1 — Chẩn đoán (triage)
1) Xác định **điểm lỗi**: request `@name`, status code, error body (OperationOutcome nếu FHIR).  
2) Phân loại lỗi:  
   - **Mapping/Serialization** (DTO ↔ Domain ↔ FHIR).  
   - **Schema/Entity/EF** (thiếu trường, cardinality sai, precision/length).  
   - **Headers/Protocol** (Accept/Content‑Type/ETag/Prefer sai).  
   - **Dữ liệu test/biến env** (thiếu `patientId`, `patientVid`, `idSystem`, `idValue`…).  
3) Thu thập bằng chứng: request + response (ẩn token), trích đoạn log.

## Bước 2 — Xác định phạm vi thay đổi
- Nếu do **mapping AutoMapper** → sửa trong *Profile* theo **exception mapping** được phép.  
- Nếu do **schema Domain/EF** → cập nhật theo `domain-entity.mdc` (cardinality, datatype…), tạo migration mới, **không** chỉnh migration cũ.  
- Nếu do **headers/endpoint** → sửa `.http`/controller nhưng vẫn tuân thủ `endpoint-checks.mdc`.  

## Bước 3 — Soạn **Spec Note** trước khi sửa
- Nêu **FHIR element path** liên quan (R4; R5 nếu áp dụng), **cardinality**, **datatype**, **binding** (nếu có).  
- Lý do thay đổi (bug root cause) và **chiến lược tương thích** (nếu R5).  
- Ảnh hưởng migration (nếu có) và bước chuyển dữ liệu.

## Bước 4 — Thực thi thay đổi có kiểm soát
- Sửa code **tối thiểu cần thiết**.  
- Tuân thủ **guardrail "không suy diễn"**; chỉ thêm trường có trong R4 hoặc Extension hợp lệ.  
- Viết/điều chỉnh **unit tests** (round‑trip, null vs [] semantics).

## Bước 5 — Chạy lại kiểm tra endpoint
- **Lặp lại** **Quy trình nhanh — Patient** (hoặc tài nguyên liên quan).  
- Ghi nhật ký: tên request, status, `resourceType`, `id`, `versionId`, ghi chú fix.

## Bước 6 — Hoàn tất & PR checklist
- Cập nhật: **Spec Note** trong PR, liên kết đến rule đã đọc lại.  
- Đính kèm trích xuất **log endpoint** sau khi pass.  
- Đảm bảo tất cả checkbox trong **Definition of Done** đạt.

# 📋 Bugfix Checklist (bắt buộc tick trước khi merge)
- [ ] ĐÃ đọc lại: `domain-entity.mdc`, `automapper-rules.mdc`, `endpoint-checks.mdc`, `api.mdc`, `api-fhir-hybrid.mdc`.  
- [ ] ĐÃ tạo **Spec Note** (FHIR R4; R5 nếu có).  
- [ ] Thay đổi **không suy diễn**, phù hợp R4/Extension hợp lệ.  
- [ ] Unit tests pass; `AssertConfigurationIsValid()` không lỗi.  
- [ ] Endpoint checks pass (Search/Create/Read/Update/ETag/Delete or History).  
- [ ] Log kết quả & tài liệu hoá kèm PR.

# ✅ Definition of Done (Endpoint Checks)
- **Script tự động pass**: Tất cả FHIR resources (11+ tests) phải PASS
- Có **log kết quả** cho: Search, Create (Prefer), Read, Update (ETag), Delete/History (nếu dùng).  
- Status hợp lệ; body FHIR hợp lệ (`resourceType` đúng, `versionId` tăng sau update).  
- Mapping mới hoạt động (round‑trip đúng DTO FHIR).  
- Mọi **exception mapping** có **Spec Note** kèm theo PR.
- **Template chuẩn** được sử dụng cho tệp .http mới
