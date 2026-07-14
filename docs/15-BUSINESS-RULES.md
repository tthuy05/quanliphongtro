# Business rules

## Property và room

- Boarding house có đúng một owner; record có lịch sử chỉ deactive.
- `RoomCode` unique không phân biệt hoa/thường trong một property; trùng ở property khác được phép.
- Room status: `Available`, `Occupied`, `Maintenance`, `Inactive`. Chỉ Available nhận contract active; không có hai contract active/overlap; room có lịch sử không hard delete.

## Tenant và contract

- Phone và email được validate; identity number không hiện ở list/log và được mask khi cần.
- Tenant profile có thể chưa gắn account; một tenant chỉ có một residence active.
- Contract end > start, tiền không âm, due day 1..28. Draft có thể sửa; activate cần room Available và dùng transaction, sau đó room Occupied. End kiểm tra invoice còn nợ; move-out đóng residence và đưa room Available. Cancel/end giữ lịch sử.

## Service và meter

- Calculation type: `FixedPerRoom`, `PerPerson`, `PerUnit`, `Custom`; price không âm.
- ContractService và InvoiceDetail giữ snapshot tên, đơn vị, loại tính, giá và lượng thỏa thuận. `FixedPerRoom` dùng lượng 1; `PerPerson` dùng số thành viên active; `PerUnit`/`Custom` dùng `ContractService.Quantity`; giá có thể override tại hợp đồng.
- Một reading/type/room/kỳ; current >= previous; previous luôn đọc từ database. Reading phải gắn active contract. Reading đã dùng cho invoice paid không sửa trực tiếp.

## Invoice

- Một invoice không-cancel cho contract/kỳ. Invoice mới chỉ chứa chi phí của kỳ hiện tại; nợ các invoice cũ được giữ ở ledger riêng và chỉ hiển thị như `account debt`, không roll-forward vào `TotalAmount`, để không double-count. Mọi thành phần không âm và remaining không âm.
- Status `Draft`, `Issued`, `PartiallyPaid`, `Paid`, `Overdue`, `Cancelled`; overdue được suy ra/cập nhật từ due date và remaining.
- Issue/cancel có audit; cancel bắt buộc lý do; paid không sửa/xóa. Bulk generation xử lý từng contract nhưng chống duplicate bằng constraint và transaction.

## Payment

- Amount > 0; confirmed total không vượt remaining; partial được hỗ trợ, overpayment không hỗ trợ.
- Tenant chỉ submit evidence cho invoice của contract mình. Owner của property mới confirm/reject. Confirm cập nhật paid/remaining/status trong một transaction. Confirmed payment là immutable; correction cần workflow audit riêng.
- Upload chỉ JPEG/PNG/WebP, tối đa 5 MB mặc định; tên lưu do server sinh.

## Maintenance và notification

- Tenant chỉ tạo request cho room residence active. Priority/status là enum. Owner chỉ cập nhật request property mình; chuyển Completed đặt `CompletedAt`, rời Completed xóa timestamp chỉ qua transition hợp lệ.
- Notification luôn gắn user; chỉ user đó đọc/mark read.
