/* dashboard.js - Coordinator file for dashboard operations */

document.addEventListener('DOMContentLoaded', () => {
    // Log demo activation status
    console.log('%c Trọ Sinh Viên Dashboard initialized successfully! ', 'background: #4f46e5; color: #fff; font-weight: bold; padding: 4px; border-radius: 4px;');
    
    // Check local month filter transitions
    const monthSelectEl = document.querySelector('select[x-model="pageState"]');
    if (monthSelectEl) {
        monthSelectEl.addEventListener('change', (e) => {
            const selectedState = e.target.value;
            // Notify client stack
            if (selectedState === 'loading') {
                window.showToast('Đang tải dữ liệu mô phỏng...', 'info');
            } else if (selectedState === 'empty') {
                window.showToast('Chuyển đổi dữ liệu trống thành công', 'warning');
            } else {
                window.showToast('Khôi phục cấu hình dữ liệu nhà trọ!', 'success');
            }
        });
    }
});
