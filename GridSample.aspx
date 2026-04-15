<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GridSample.aspx.cs" Inherits="WebApp.GridSample" %>
<%@ Register TagPrefix="ctrl" TagName="AgGridControl" Src="~/Controls/AgGridControl.ascx" %>
<!DOCTYPE html>
<html lang="ko">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>AgGrid 커스텀 컨트롤 샘플</title>

    <%-- ag-Grid Community CSS --%>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/ag-grid-community@28.2.1/dist/styles/ag-grid.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/ag-grid-community@28.2.1/dist/styles/ag-theme-alpine.css" />

    <%-- 커스텀 컨트롤 CSS --%>
    <link rel="stylesheet" href="Styles/aggrid-control.css" />

    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Malgun Gothic', sans-serif;
            font-size: 14px;
            background-color: #f0f2f5;
            color: #333;
        }

        .page-header {
            background: linear-gradient(135deg, #1a237e 0%, #283593 100%);
            color: #fff;
            padding: 20px 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        }
        .page-header h1 { font-size: 22px; font-weight: 600; }
        .page-header p  { font-size: 13px; color: rgba(255,255,255,0.75); margin-top: 4px; }

        .page-content { padding: 24px 30px; max-width: 1400px; margin: 0 auto; }

        .section-card {
            background  : #fff;
            border-radius: 8px;
            box-shadow  : 0 1px 4px rgba(0,0,0,0.08);
            margin-bottom: 28px;
            overflow    : hidden;
        }
        .section-title {
            display     : flex;
            align-items : center;
            gap         : 10px;
            padding     : 14px 20px;
            font-size   : 15px;
            font-weight : 600;
            border-bottom: 1px solid #e9ecef;
            background  : #f8f9fa;
        }
        .section-badge {
            font-size   : 11px;
            font-weight : 500;
            padding     : 2px 8px;
            border-radius: 20px;
            background  : #e7f3ff;
            color       : #0d6efd;
        }
        .section-body { padding: 16px 20px; }

        /* 상태 정보 패널 */
        .status-panel {
            display     : flex;
            gap         : 16px;
            flex-wrap   : wrap;
            margin-top  : 12px;
        }
        .status-item {
            background  : #f8f9fa;
            border      : 1px solid #dee2e6;
            border-radius: 6px;
            padding     : 8px 14px;
            font-size   : 13px;
        }
        .status-item label { color: #6c757d; margin-right: 6px; }
        .status-item span  { font-weight: 600; color: #333; }

        /* 로그 박스 */
        .event-log {
            margin-top  : 12px;
            background  : #1a1a2e;
            color       : #a8d8a8;
            font-family : 'Courier New', monospace;
            font-size   : 12px;
            padding     : 12px;
            border-radius: 6px;
            height      : 120px;
            overflow-y  : auto;
            line-height : 1.6;
        }
        .event-log .log-time { color: #6c757d; }
        .event-log .log-info { color: #a8d8a8; }
        .event-log .log-warn { color: #ffc107; }
        .event-log .log-error{ color: #f87171; }
    </style>
</head>
<body>

<div class="page-header">
    <h1>AgGrid 커스텀 컨트롤 샘플</h1>
    <p>ASP.NET Framework 4.7.2 + ag-Grid Community 28 | DataTable &amp; JSON 데이터소스 지원</p>
</div>

<div class="page-content">

    <%-- ============================================================
         섹션 1: 사원 그리드 (DataTable 데이터소스)
         ============================================================ --%>
    <div class="section-card">
        <div class="section-title">
            사원 관리
            <span class="section-badge">DataTable 데이터소스</span>
        </div>
        <div class="section-body">
            <%-- 컨트롤 선언: 외부에서 버튼 표시 여부 및 콜백 함수명 설정 --%>
            <ctrl:AgGridControl
                ID="EmployeeGrid"
                runat="server"
                GridId="employeeGrid"
                GridHeight="420px"
                ShowNewButton="true"
                ShowSaveButton="true"
                ShowDeleteButton="true"
                ShowExcelButton="true"
                ShowSearchBox="true"
                NewButtonText="신규"
                SaveButtonText="저장"
                DeleteButtonText="삭제"
                ExcelButtonText="엑셀"
                RowSelection="multiple"
                EnablePagination="false"
                ExcelFileName="employee_list"
                OnNewClick="handleEmployeeNew"
                OnSaveClick="handleEmployeeSave"
                OnDeleteClick="handleEmployeeDelete"
                OnRowClick="handleEmployeeRowClick"
                OnRowSelected="handleEmployeeRowSelected"
                OnCellValueChanged="handleEmployeeCellChanged"
                OnGridReady="handleEmployeeGridReady" />

            <%-- 선택 정보 패널 --%>
            <div class="status-panel">
                <div class="status-item">
                    <label>전체 행 수:</label>
                    <span id="empTotalCount">-</span>
                </div>
                <div class="status-item">
                    <label>선택된 행:</label>
                    <span id="empSelectedCount">0</span>
                </div>
                <div class="status-item">
                    <label>마지막 클릭:</label>
                    <span id="empLastClick">-</span>
                </div>
            </div>

            <%-- 이벤트 로그 --%>
            <div class="event-log" id="empEventLog">
                <span class="log-info">그리드 초기화 대기중...</span>
            </div>
        </div>
    </div>

    <%-- ============================================================
         섹션 2: 제품 그리드 (JSON 데이터소스, 버튼 일부 숨김)
         ============================================================ --%>
    <div class="section-card">
        <div class="section-title">
            제품 목록
            <span class="section-badge">JSON 데이터소스</span>
            <span class="section-badge" style="background:#fff3cd;color:#856404;">신규/저장 버튼 숨김 예시</span>
        </div>
        <div class="section-body">
            <%-- 코드비하인드에서 ShowSaveButton=false, ShowDeleteButton=false 처리 --%>
            <ctrl:AgGridControl
                ID="ProductGrid"
                runat="server"
                GridId="productGrid"
                ShowNewButton="true"
                ShowExcelButton="true"
                ShowSearchBox="true"
                NewButtonText="제품 추가"
                ExcelFileName="product_list"
                RowSelection="single"
                OnNewClick="handleProductNew"
                OnRowClick="handleProductRowClick"
                OnGridReady="handleProductGridReady" />

            <div class="status-panel">
                <div class="status-item">
                    <label>선택 제품:</label>
                    <span id="productSelected">-</span>
                </div>
                <div class="status-item">
                    <label>가격:</label>
                    <span id="productPrice">-</span>
                </div>
            </div>
        </div>
    </div>

</div><%-- /page-content --%>

<%-- ============================================================
     ag-Grid Community JS
     ============================================================ --%>
<script src="https://cdn.jsdelivr.net/npm/ag-grid-community@28.2.1/dist/ag-grid-community.min.js"></script>
<%-- SheetJS (xlsx) - 엑셀 내보내기용 --%>
<script src="https://cdn.jsdelivr.net/npm/xlsx@0.18.5/dist/xlsx.full.min.js"></script>
<%-- 커스텀 컨트롤 JS --%>
<script src="Scripts/aggrid-control.js"></script>

<%-- ============================================================
     페이지별 렌더러 & 포매터 & 이벤트 핸들러
     ============================================================ --%>
<script>
// ----------------------------------------------------------------
// 유틸리티
// ----------------------------------------------------------------
function logEvent(logId, type, message) {
    var log = document.getElementById(logId);
    if (!log) return;
    var time = new Date().toLocaleTimeString('ko-KR');
    var line = document.createElement('div');
    line.innerHTML = '<span class="log-time">[' + time + ']</span> '
                   + '<span class="log-' + type + '">' + message + '</span>';
    log.appendChild(line);
    log.scrollTop = log.scrollHeight;
}

function formatNumber(n) {
    return Number(n).toLocaleString('ko-KR');
}

// ----------------------------------------------------------------
// ag-Grid 값 포매터 (전역 함수로 등록)
// ----------------------------------------------------------------
function salaryFormatter(params) {
    if (params.value == null) return '';
    return formatNumber(params.value) + ' 원';
}

function priceFormatter(params) {
    if (params.value == null) return '';
    return formatNumber(params.value) + ' 원';
}

// ----------------------------------------------------------------
// ag-Grid 셀 렌더러
// ----------------------------------------------------------------
function statusRenderer(params) {
    if (!params.value) return '';
    var colorMap = { '재직': '#198754', '수습': '#0d6efd', '휴직': '#fd7e14', '퇴직': '#dc3545' };
    var color = colorMap[params.value] || '#6c757d';
    return '<span style="display:inline-block;padding:2px 8px;border-radius:12px;'
         + 'font-size:11px;font-weight:600;background:' + color + '20;color:' + color + '">'
         + params.value + '</span>';
}

function ratingRenderer(params) {
    if (params.value == null) return '';
    var val = parseFloat(params.value);
    var stars = '';
    for (var i = 1; i <= 5; i++) {
        stars += '<span style="color:' + (i <= Math.round(val) ? '#ffc107' : '#dee2e6') + '">&#9733;</span>';
    }
    return stars + ' <span style="font-size:11px;color:#6c757d;">(' + val.toFixed(1) + ')</span>';
}

// ----------------------------------------------------------------
// ag-Grid 셀 스타일 함수
// ----------------------------------------------------------------
function statusCellStyle(params) {
    return { textAlign: 'center' };
}

function stockCellStyle(params) {
    if (params.value == null) return null;
    var stock = parseInt(params.value);
    if (stock <= 30)  return { color: '#dc3545', fontWeight: '600' };
    if (stock <= 100) return { color: '#fd7e14' };
    return { color: '#198754' };
}

// ================================================================
// 사원 그리드 이벤트 핸들러
// ================================================================
function handleEmployeeGridReady(params) {
    var count = window.employeeGrid ? window.employeeGrid.getRowCount() : 0;
    document.getElementById('empTotalCount').textContent = count;
    logEvent('empEventLog', 'info', '그리드 준비 완료. 총 ' + count + '개 행 로드됨.');
}

function handleEmployeeNew(arg) {
    logEvent('empEventLog', 'info', '[신규] 버튼 클릭 - 새 행 추가 처리');
    // 예시: 빈 행 추가
    var newRow = {
        empId: 'E' + String(Date.now()).slice(-3),
        name: '', dept: '', position: '', salary: 0,
        joinDate: new Date().toISOString().slice(0, 10),
        email: '', status: '수습'
    };
    window.employeeGrid.addRows(newRow);
    document.getElementById('empTotalCount').textContent = window.employeeGrid.getRowCount();
}

function handleEmployeeSave(arg) {
    var all = window.employeeGrid.getAllRowData();
    logEvent('empEventLog', 'info', '[저장] 버튼 클릭 - ' + all.length + '개 행 저장 처리 (실제 구현 필요)');
    alert('저장 처리: ' + all.length + '개 행\n(실제 구현에서는 Ajax 또는 PostBack으로 처리)');
}

function handleEmployeeDelete(arg) {
    var rows = arg.selectedRows;
    logEvent('empEventLog', 'warn', '[삭제] ' + rows.length + '개 행 삭제 요청');
    if (!confirm(rows.length + '개 행을 삭제하시겠습니까?')) return;
    window.employeeGrid.removeRows(rows);
    document.getElementById('empTotalCount').textContent = window.employeeGrid.getRowCount();
    document.getElementById('empSelectedCount').textContent = '0';
    logEvent('empEventLog', 'warn', '[삭제] 완료');
}

function handleEmployeeRowClick(event) {
    if (!event || !event.data) return;
    document.getElementById('empLastClick').textContent =
        event.data.name + ' (' + event.data.empId + ')';
}

function handleEmployeeRowSelected(arg) {
    var count = arg.selectedRows ? arg.selectedRows.length : 0;
    document.getElementById('empSelectedCount').textContent = count;
    if (count > 0) {
        logEvent('empEventLog', 'info', '[선택] ' + count + '개 행 선택됨');
    }
}

function handleEmployeeCellChanged(event) {
    logEvent('empEventLog', 'info',
        '[편집] ' + event.colDef.headerName + ' : "' + event.oldValue + '" → "' + event.newValue + '"');
}

// 커스텀 버튼 핸들러
function handlePrint(arg) {
    logEvent('empEventLog', 'info', '[인쇄] 버튼 클릭');
    window.print();
}

function handleRefresh(arg) {
    logEvent('empEventLog', 'info', '[새로고침] 데이터 새로고침 (실제 구현에서는 Ajax 호출)');
    window.employeeGrid.refresh();
}

// ================================================================
// 제품 그리드 이벤트 핸들러
// ================================================================
function handleProductGridReady(params) {
    // 제품 그리드 준비 완료 시 추가 작업
}

function handleProductNew(arg) {
    alert('제품 추가 폼을 여세요.\n(실제 구현에서는 모달 또는 폼 페이지로 이동)');
}

function handleProductRowClick(event) {
    if (!event || !event.data) return;
    document.getElementById('productSelected').textContent = event.data.productName;
    document.getElementById('productPrice').textContent = formatNumber(event.data.price) + ' 원';
}
</script>

</body>
</html>
