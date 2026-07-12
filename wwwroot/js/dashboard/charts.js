/* charts.js - Chart.js initializer and dynamic update controller */

document.addEventListener('DOMContentLoaded', () => {
    // 1. READ CONFIG DATASETS INJECTED FROM RAZOR
    const revenuePoints = window.chartRevenueData || [];
    const occupancyConfig = window.chartOccupancyData || { occupiedCount: 19, availableCount: 4, maintenanceCount: 1, occupancyPercentage: 79 };

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Helper: Map data coordinates
    const months = revenuePoints.map(p => p.month);
    const revenueValues = revenuePoints.map(p => parseFloat(p.revenue));
    const expenseValues = revenuePoints.map(p => parseFloat(p.expense));

    // 2. REVENUE LINE & AREA CHART
    const lineCtx = document.getElementById('revenueLineChart');
    let revenueChartInstance = null;

    const initRevenueChart = () => {
        if (!lineCtx) return;
        
        revenueChartInstance = new Chart(lineCtx, {
            type: 'line',
            data: {
                labels: months,
                datasets: [
                    {
                        label: 'Doanh thu',
                        data: revenueValues,
                        borderColor: '#4f46e5', // indigo-600
                        backgroundColor: 'rgba(79, 70, 229, 0.05)',
                        fill: true,
                        tension: 0.35,
                        borderWidth: 2.5,
                        pointBackgroundColor: '#4f46e5',
                        pointHoverRadius: 6,
                        pointHoverBorderColor: '#ffffff',
                        pointHoverBorderWidth: 2,
                    },
                    {
                        label: 'Chi phí',
                        data: expenseValues,
                        borderColor: '#64748b', // slate-500
                        backgroundColor: 'rgba(100, 116, 139, 0.02)',
                        fill: true,
                        tension: 0.35,
                        borderWidth: 2,
                        pointBackgroundColor: '#64748b',
                        pointHoverRadius: 5,
                        pointHoverBorderColor: '#ffffff',
                        pointHoverBorderWidth: 2,
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            font: { family: 'Be Vietnam Pro', size: 11, weight: '600' },
                            usePointStyle: true,
                            pointStyle: 'circle',
                            padding: 16
                        }
                    },
                    tooltip: {
                        enabled: true,
                        backgroundColor: '#0f172a', // slate-900
                        titleFont: { family: 'Be Vietnam Pro', size: 12, weight: '700' },
                        bodyFont: { family: 'Be Vietnam Pro', size: 12, weight: '500' },
                        padding: 10,
                        cornerRadius: 12,
                        callbacks: {
                            label: function(context) {
                                return ` ${context.dataset.label}: ${context.raw}M ₫`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            font: { family: 'Be Vietnam Pro', size: 10, weight: '500' },
                            color: '#64748b'
                        }
                    },
                    y: {
                        grid: { color: '#f1f5f9' },
                        ticks: {
                            font: { family: 'Be Vietnam Pro', size: 10, weight: '500' },
                            color: '#64748b',
                            callback: function(value) { return value + 'M ₫'; }
                        }
                    }
                },
                animation: prefersReducedMotion ? { duration: 0 } : {
                    duration: 1000,
                    easing: 'easeOutQuad'
                }
            }
        });
    };

    // 3. OCCUPANCY DONUT CHART
    const donutCtx = document.getElementById('occupancyDonutChart');
    let occupancyChartInstance = null;

    const initOccupancyChart = () => {
        if (!donutCtx) return;

        occupancyChartInstance = new Chart(donutCtx, {
            type: 'doughnut',
            data: {
                labels: ['Đang thuê', 'Còn trống', 'Bảo trì'],
                datasets: [{
                    data: [
                        occupancyConfig.occupiedCount,
                        occupancyConfig.availableCount,
                        occupancyConfig.maintenanceCount
                    ],
                    backgroundColor: [
                        '#10b981', // emerald-500
                        '#cbd5e1', // slate-300
                        '#f59e0b'  // amber-500
                    ],
                    borderWidth: 3,
                    borderColor: '#ffffff',
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '80%',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        enabled: true,
                        backgroundColor: '#0f172a',
                        titleFont: { family: 'Be Vietnam Pro', size: 12, weight: '700' },
                        bodyFont: { family: 'Be Vietnam Pro', size: 12, weight: '500' },
                        padding: 10,
                        cornerRadius: 12,
                        callbacks: {
                            label: function(context) {
                                return ` ${context.label}: ${context.raw} phòng`;
                            }
                        }
                    }
                },
                animation: prefersReducedMotion ? { duration: 0 } : {
                    animateRotate: true,
                    animateScale: false,
                    duration: 1000,
                    easing: 'easeOutQuad'
                }
            }
        });
    };

    // 4. INTERSECTION OBSERVER FOR DRAW TRIGGERS ON SCROLL
    const chartContainers = document.querySelectorAll('.relative canvas');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const canvasId = entry.target.id;
                if (canvasId === 'revenueLineChart' && !revenueChartInstance) {
                    initRevenueChart();
                } else if (canvasId === 'occupancyDonutChart' && !occupancyChartInstance) {
                    initOccupancyChart();
                }
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    chartContainers.forEach(container => observer.observe(container));

    // Cleanup routines on unload
    window.addEventListener('unload', () => {
        if (revenueChartInstance) revenueChartInstance.destroy();
        if (occupancyChartInstance) occupancyChartInstance.destroy();
    });
});
