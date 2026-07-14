# Entity Relationship Diagram

```mermaid
erDiagram
    APPLICATION_USER ||--o{ BOARDING_HOUSE : owns
    APPLICATION_USER o|--o| TENANT_PROFILE : login_for
    APPLICATION_USER ||--o{ NOTIFICATION : receives
    BOARDING_HOUSE ||--o{ ROOM : contains
    BOARDING_HOUSE ||--o{ PROPERTY_SERVICE : defines
    ROOM ||--o{ CONTRACT : history
    TENANT_PROFILE ||--o{ CONTRACT : represents
    CONTRACT ||--o{ CONTRACT_MEMBER : has
    TENANT_PROFILE ||--o{ CONTRACT_MEMBER : joins
    ROOM ||--o{ ROOM_TENANT : residence_history
    TENANT_PROFILE ||--o{ ROOM_TENANT : resides
    CONTRACT ||--o{ CONTRACT_SERVICE : configures
    PROPERTY_SERVICE ||--o{ CONTRACT_SERVICE : snapshots
    ROOM ||--o{ METER_READING : meters
    CONTRACT ||--o{ METER_READING : billed_for
    CONTRACT ||--o{ INVOICE : billed
    ROOM ||--o{ INVOICE : snapshots_room
    INVOICE ||--|{ INVOICE_DETAIL : contains
    INVOICE ||--o{ PAYMENT : receives
    ROOM ||--o{ MAINTENANCE_REQUEST : concerns
    TENANT_PROFILE ||--o{ MAINTENANCE_REQUEST : submits
    MAINTENANCE_REQUEST ||--o{ MAINTENANCE_COMMENT : discusses
    APPLICATION_USER ||--o{ AUDIT_LOG : performs
    APPLICATION_USER ||--o{ UPLOADED_FILE : uploads
    UPLOADED_FILE o|--o{ PAYMENT : evidence
```

`RoomTenant` là lịch sử cư trú thực tế; `ContractMember` là quan hệ pháp lý với hợp đồng. Tenant chỉ có tối đa một `RoomTenant` chưa có `MoveOutDate`; service và transaction đảm bảo invariant này. Contract và invoice giữ snapshot giá để thay đổi catalog không sửa lịch sử.
