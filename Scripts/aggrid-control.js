/**
 * AgGridControl
 * ag-Grid Community 를 래핑하는 커스텀 컨트롤 JavaScript 클래스
 *
 * 사용법:
 *   // 서버에서 자동 생성되는 초기화 스크립트 예시:
 *   window['myGridId'] = new AgGridControl({
 *     gridInstanceId : 'myGridId',
 *     columns        : [...],          // ag-Grid columnDefs
 *     rowData        : [...],          // 초기 데이터
 *     rowSelection   : 'multiple',
 *     enablePagination: false,
 *     pageSize       : 20,
 *     onNewClick     : 'handleNew',    // 전역 함수명
 *     onSaveClick    : 'handleSave',
 *     onDeleteClick  : 'handleDelete',
 *     excelFileName  : 'export',
 *   });
 *
 *   // 외부에서 API 접근
 *   var grid = window['myGridId'];
 *   grid.setColumns([...]);
 *   grid.setRowData([...]);
 *   grid.getSelectedRows();
 *   grid.getGridApi();
 */
;(function (global) {
    'use strict';

    // ------------------------------------------------------------------
    // 생성자
    // ------------------------------------------------------------------
    function AgGridControl(config) {
        this.config      = config || {};
        this.instanceId  = config.gridInstanceId || ('grid_' + Date.now());
        this.gridApi     = null;
        this._container  = null;
        this._pendingData = null;
        this._pendingCols = null;

        this._init();
    }

    // ------------------------------------------------------------------
    // 초기화
    // ------------------------------------------------------------------
    AgGridControl.prototype._init = function () {
        var self = this;

        // DOM 준비 후 초기화
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () { self._createGrid(); });
        } else {
            self._createGrid();
        }
    };

    AgGridControl.prototype._createGrid = function () {
        var self   = this;
        var cfg    = this.config;
        var id     = this.instanceId;

        // 컨테이너: {clientId}_grid 규칙으로 찾음
        // instanceId가 grid_xxx 형태이므로 그대로 사용 불가 → wrapper 내 _grid 패턴
        var container = document.getElementById(id + '_grid')
            || document.querySelector('[id$="' + id.replace('grid_', '') + '_grid"]');

        if (!container) {
            console.error('[AgGridControl] 그리드 컨테이너를 찾을 수 없습니다: ' + id + '_grid');
            return;
        }
        this._container = container;

        var columns = cfg.columns || [];
        var rowData = cfg.rowData || [];

        // ag-Grid 옵션
        var gridOptions = {
            columnDefs             : columns,
            rowData                : rowData,
            rowSelection           : cfg.rowSelection || 'multiple',
            animateRows            : cfg.animateRows !== false,
            enableRangeSelection   : cfg.enableRangeSelection || false,
            suppressRowClickSelection: cfg.suppressRowClickSelection || false,
            defaultColDef: {
                sortable   : true,
                filter     : true,
                resizable  : true,
                minWidth   : 60,
            },
            pagination             : cfg.enablePagination || false,
            paginationPageSize     : cfg.pageSize || 20,
            suppressCellFocus      : false,
            domLayout              : 'normal',

            // 이벤트
            onGridReady: function (params) {
                self.gridApi = params.api;
                // 보류 중 데이터/컬럼 적용
                if (self._pendingCols) {
                    self.gridApi.setGridOption('columnDefs', self._pendingCols);
                    self._pendingCols = null;
                }
                if (self._pendingData) {
                    self.gridApi.setGridOption('rowData', self._pendingData);
                    self._pendingData = null;
                }
                // 컬럼 자동 맞춤
                self.gridApi.sizeColumnsToFit();
                // 사용자 콜백
                self._callUserFn(cfg.onGridReady, params);
            },

            onRowClicked: function (event) {
                self._callUserFn(cfg.onRowClick, event);
            },

            onSelectionChanged: function (event) {
                self._callUserFn(cfg.onRowSelected, {
                    event       : event,
                    selectedRows: self.getSelectedRows()
                });
            },

            onCellValueChanged: function (event) {
                self._callUserFn(cfg.onCellValueChanged, event);
            },

            // 창 크기 변경 시 컬럼 재조정
            onFirstDataRendered: function () {
                if (self.gridApi) self.gridApi.sizeColumnsToFit();
            },
        };

        // ag-Grid 인스턴스 생성
        if (typeof agGrid === 'undefined') {
            console.error('[AgGridControl] ag-Grid 라이브러리가 로드되지 않았습니다.');
            return;
        }

        // ag-Grid v28 이하: new agGrid.Grid(...)
        // ag-Grid v31 이상: agGrid.createGrid(...)
        if (typeof agGrid.createGrid === 'function') {
            this.gridApi = agGrid.createGrid(container, gridOptions);
        } else {
            new agGrid.Grid(container, gridOptions);
            // v28 이하는 onGridReady 내에서 this.gridApi 가 설정됨
        }

        // 창 크기 변경 시 컬럼 자동 맞춤
        window.addEventListener('resize', function () {
            if (self.gridApi) {
                setTimeout(function () { self.gridApi.sizeColumnsToFit(); }, 100);
            }
        });
    };

    // ------------------------------------------------------------------
    // 툴바 버튼 핸들러 (ASCX 버튼의 onclick에서 호출)
    // ------------------------------------------------------------------
    AgGridControl.prototype.handleNew = function () {
        this._callUserFn(this.config.onNewClick, { gridApi: this.gridApi });
    };

    AgGridControl.prototype.handleSave = function () {
        this._callUserFn(this.config.onSaveClick, { gridApi: this.gridApi });
    };

    AgGridControl.prototype.handleDelete = function () {
        var selected = this.getSelectedRows();
        if (selected.length === 0) {
            alert('삭제할 행을 선택해 주세요.');
            return;
        }
        this._callUserFn(this.config.onDeleteClick, {
            gridApi      : this.gridApi,
            selectedRows : selected
        });
    };

    AgGridControl.prototype.handleExcel = function () {
        if (this.config.onExcelClick) {
            this._callUserFn(this.config.onExcelClick, { gridApi: this.gridApi });
        } else {
            this.exportToExcel();
        }
    };

    AgGridControl.prototype.handleSearch = function (value) {
        if (!this.gridApi) return;
        // v31+: setGridOption, v28: setQuickFilter
        if (typeof this.gridApi.setGridOption === 'function') {
            this.gridApi.setGridOption('quickFilterText', value);
        } else if (typeof this.gridApi.setQuickFilter === 'function') {
            this.gridApi.setQuickFilter(value);
        }
    };

    /**
     * 커스텀 버튼 핸들러
     * @param {string} btnId   - 버튼 ID
     * @param {string} fnName  - 호출할 전역 함수명
     */
    AgGridControl.prototype.handleCustomButton = function (btnId, fnName) {
        if (fnName) {
            this._callUserFn(fnName, { gridApi: this.gridApi, buttonId: btnId });
        }
    };

    // ------------------------------------------------------------------
    // 공개 API
    // ------------------------------------------------------------------

    /**
     * 컬럼을 동적으로 변경합니다.
     * @param {Array} columnDefs - ag-Grid columnDefs 배열
     */
    AgGridControl.prototype.setColumns = function (columnDefs) {
        if (!this.gridApi) {
            this._pendingCols = columnDefs;
            return;
        }
        if (typeof this.gridApi.setGridOption === 'function') {
            this.gridApi.setGridOption('columnDefs', columnDefs);
        } else if (typeof this.gridApi.setColumnDefs === 'function') {
            this.gridApi.setColumnDefs(columnDefs);
        }
        this.gridApi.sizeColumnsToFit();
    };

    /**
     * 행 데이터를 교체합니다. DataTable → JSON 변환은 서버에서 처리 후 전달.
     * @param {Array} rowData - 객체 배열
     */
    AgGridControl.prototype.setRowData = function (rowData) {
        if (!this.gridApi) {
            this._pendingData = rowData;
            return;
        }
        if (typeof this.gridApi.setGridOption === 'function') {
            this.gridApi.setGridOption('rowData', rowData);
        } else if (typeof this.gridApi.setRowData === 'function') {
            this.gridApi.setRowData(rowData);
        }
    };

    /**
     * JSON 문자열로 행 데이터를 설정합니다.
     * @param {string} jsonString
     */
    AgGridControl.prototype.setRowDataFromJson = function (jsonString) {
        try {
            var data = typeof jsonString === 'string' ? JSON.parse(jsonString) : jsonString;
            this.setRowData(data);
        } catch (e) {
            console.error('[AgGridControl] JSON 파싱 오류:', e);
        }
    };

    /**
     * 행 데이터 추가 (트랜잭션 방식, 변경 추적 가능)
     * @param {Array|Object} rows
     */
    AgGridControl.prototype.addRows = function (rows) {
        if (!this.gridApi) return;
        var addData = Array.isArray(rows) ? rows : [rows];
        this.gridApi.applyTransaction({ add: addData });
    };

    /**
     * 행 데이터 업데이트 (트랜잭션)
     * @param {Array|Object} rows
     */
    AgGridControl.prototype.updateRows = function (rows) {
        if (!this.gridApi) return;
        var updateData = Array.isArray(rows) ? rows : [rows];
        this.gridApi.applyTransaction({ update: updateData });
    };

    /**
     * 행 데이터 삭제 (트랜잭션)
     * @param {Array|Object} rows
     */
    AgGridControl.prototype.removeRows = function (rows) {
        if (!this.gridApi) return;
        var removeData = Array.isArray(rows) ? rows : [rows];
        this.gridApi.applyTransaction({ remove: removeData });
    };

    /**
     * 선택된 행을 반환합니다.
     * @returns {Array}
     */
    AgGridControl.prototype.getSelectedRows = function () {
        if (!this.gridApi) return [];
        return this.gridApi.getSelectedRows() || [];
    };

    /**
     * 선택된 행의 노드를 반환합니다.
     * @returns {Array}
     */
    AgGridControl.prototype.getSelectedNodes = function () {
        if (!this.gridApi) return [];
        return this.gridApi.getSelectedNodes() || [];
    };

    /**
     * 모든 행 데이터를 반환합니다.
     * @returns {Array}
     */
    AgGridControl.prototype.getAllRowData = function () {
        if (!this.gridApi) return [];
        var rows = [];
        this.gridApi.forEachNode(function (node) { rows.push(node.data); });
        return rows;
    };

    /**
     * Excel (.xlsx) 파일로 내보냅니다. (SheetJS 필요)
     * SheetJS 미로드 시 CSV로 대체합니다.
     * @param {string} [fileName]
     */
    AgGridControl.prototype.exportToExcel = function (fileName) {
        var name = fileName || this.config.excelFileName || 'export';

        if (typeof XLSX !== 'undefined') {
            // SheetJS를 이용한 xlsx 내보내기
            var data = this.getAllRowData();
            if (data.length === 0) { alert('내보낼 데이터가 없습니다.'); return; }

            var ws = XLSX.utils.json_to_sheet(data);
            var wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');

            // 헤더명을 컬럼 headerName으로 교체
            this._applyExcelHeaders(ws);

            XLSX.writeFile(wb, name + '.xlsx');
        } else {
            // SheetJS 없을 경우 ag-Grid 기본 CSV 내보내기
            if (!this.gridApi) return;
            this.gridApi.exportDataAsCsv({ fileName: name + '.csv' });
        }
    };

    /** @private 엑셀 헤더를 컬럼 headerName으로 교체 */
    AgGridControl.prototype._applyExcelHeaders = function (ws) {
        var cols = this.config.columns || [];
        if (cols.length === 0 || !ws['!ref']) return;

        var range = XLSX.utils.decode_range(ws['!ref']);
        for (var c = range.s.c; c <= range.e.c; c++) {
            var headerCell = XLSX.utils.encode_cell({ r: 0, c: c });
            if (ws[headerCell]) {
                var fieldName = ws[headerCell].v;
                var colDef = cols.find(function (col) { return col.field === fieldName; });
                if (colDef && colDef.headerName) {
                    ws[headerCell].v = colDef.headerName;
                }
            }
        }
    };

    /**
     * CSV 파일로 내보냅니다.
     * @param {string} [fileName]
     */
    AgGridControl.prototype.exportToCsv = function (fileName) {
        if (!this.gridApi) return;
        this.gridApi.exportDataAsCsv({
            fileName: (fileName || this.config.excelFileName || 'export') + '.csv'
        });
    };

    /**
     * 그리드 데이터를 새로고침합니다 (캐시 초기화).
     */
    AgGridControl.prototype.refresh = function () {
        if (this.gridApi) this.gridApi.refreshCells({ force: true });
    };

    /**
     * 모든 행의 선택을 해제합니다.
     */
    AgGridControl.prototype.deselectAll = function () {
        if (this.gridApi) this.gridApi.deselectAll();
    };

    /**
     * 모든 행을 선택합니다.
     */
    AgGridControl.prototype.selectAll = function () {
        if (this.gridApi) this.gridApi.selectAll();
    };

    /**
     * 컬럼 너비를 그리드 크기에 맞춥니다.
     */
    AgGridControl.prototype.sizeColumnsToFit = function () {
        if (this.gridApi) this.gridApi.sizeColumnsToFit();
    };

    /**
     * 모든 컬럼 너비를 컨텐츠 크기에 맞춥니다.
     */
    AgGridControl.prototype.autoSizeAllColumns = function () {
        if (!this.gridApi) return;
        if (typeof this.gridApi.autoSizeAllColumns === 'function') {
            this.gridApi.autoSizeAllColumns(false);
        }
    };

    /**
     * ag-Grid API 객체를 반환합니다.
     * @returns {Object}
     */
    AgGridControl.prototype.getGridApi = function () {
        return this.gridApi;
    };

    /**
     * 필터를 초기화합니다.
     */
    AgGridControl.prototype.clearFilters = function () {
        if (this.gridApi) this.gridApi.setFilterModel(null);
    };

    /**
     * 현재 필터 상태를 반환합니다.
     */
    AgGridControl.prototype.getFilterModel = function () {
        return this.gridApi ? this.gridApi.getFilterModel() : null;
    };

    /**
     * 필터를 적용합니다.
     * @param {Object} filterModel
     */
    AgGridControl.prototype.setFilterModel = function (filterModel) {
        if (this.gridApi) this.gridApi.setFilterModel(filterModel);
    };

    /**
     * 특정 행으로 스크롤합니다.
     * @param {number} rowIndex
     */
    AgGridControl.prototype.ensureRowVisible = function (rowIndex) {
        if (this.gridApi) this.gridApi.ensureIndexVisible(rowIndex, 'middle');
    };

    /**
     * 그리드의 총 행 수를 반환합니다.
     * @returns {number}
     */
    AgGridControl.prototype.getRowCount = function () {
        if (!this.gridApi) return 0;
        return this.gridApi.getDisplayedRowCount
            ? this.gridApi.getDisplayedRowCount()
            : 0;
    };

    // ------------------------------------------------------------------
    // 내부 유틸리티
    // ------------------------------------------------------------------

    /**
     * 전역 함수명 문자열로 함수를 찾아 호출합니다.
     * @private
     */
    AgGridControl.prototype._callUserFn = function (fnName, arg) {
        if (!fnName) return;
        var fn = null;

        // 점 표기법 지원: "obj.method"
        if (fnName.indexOf('.') !== -1) {
            var parts = fnName.split('.');
            fn = global;
            for (var i = 0; i < parts.length; i++) {
                fn = fn[parts[i]];
                if (!fn) break;
            }
        } else {
            fn = global[fnName];
        }

        if (typeof fn === 'function') {
            fn.call(null, arg);
        } else {
            console.warn('[AgGridControl] 콜백 함수를 찾을 수 없습니다: ' + fnName);
        }
    };

    // ------------------------------------------------------------------
    // 전역 등록
    // ------------------------------------------------------------------
    global.AgGridControl = AgGridControl;

}(window));
