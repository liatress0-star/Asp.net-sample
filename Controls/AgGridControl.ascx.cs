using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebApp.Controls
{
    // -----------------------------------------------------------------------
    // 툴바 커스텀 버튼 정의
    // -----------------------------------------------------------------------
    public class ToolbarButton
    {
        public string Id { get; set; }
        public string Text { get; set; }
        /// <summary>버튼 앞에 붙일 HTML 아이콘/텍스트 (예: "&lt;i class='icon'&gt;&lt;/i&gt;")</summary>
        public string IconHtml { get; set; }
        public string CssClass { get; set; } = "btn-toolbar btn-custom";
        /// <summary>클릭 시 호출할 전역 JS 함수명 (예: "handlePrint")</summary>
        public string OnClick { get; set; }
        public bool Hidden { get; set; } = false;
        public string Title { get; set; }
    }

    // -----------------------------------------------------------------------
    // 그리드 컬럼 정의 (ag-Grid columnDefs 매핑)
    // -----------------------------------------------------------------------
    public class GridColumnDef
    {
        public string Field { get; set; }
        public string HeaderName { get; set; }
        public int? Width { get; set; }
        public int? MinWidth { get; set; }
        public int? MaxWidth { get; set; }
        public bool Sortable { get; set; } = true;
        /// <summary>true=기본필터, "agTextColumnFilter","agNumberColumnFilter","agDateColumnFilter" 등</summary>
        public object Filter { get; set; } = true;
        public bool Resizable { get; set; } = true;
        public bool Editable { get; set; } = false;
        /// <summary>셀 렌더러 함수명 또는 내장 렌더러 이름</summary>
        public string CellRenderer { get; set; }
        /// <summary>셀 클래스명 또는 클래스 함수명</summary>
        public string CellClass { get; set; }
        /// <summary>헤더 정렬: "left","center","right"</summary>
        public string HeaderClass { get; set; }
        /// <summary>컬럼 타입: "numericColumn","dateColumn" 등</summary>
        public string Type { get; set; }
        public bool CheckboxSelection { get; set; } = false;
        public bool HeaderCheckboxSelection { get; set; } = false;
        public bool Hide { get; set; } = false;
        /// <summary>컬럼 고정: "left" 또는 "right"</summary>
        public string Pinned { get; set; }
        /// <summary>값 포매터 JS 함수명</summary>
        public string ValueFormatter { get; set; }
        /// <summary>셀 스타일 JS 함수명 또는 고정 스타일 객체 JSON</summary>
        public string CellStyle { get; set; }
        public bool FloatingFilter { get; set; } = false;
        public bool Flex { get; set; } = false;
        public int FlexValue { get; set; } = 1;
    }

    // -----------------------------------------------------------------------
    // AgGridControl 유저 컨트롤
    // -----------------------------------------------------------------------
    public partial class AgGridControl : UserControl
    {
        private List<ToolbarButton> _customButtons = new List<ToolbarButton>();
        private List<GridColumnDef> _columns = new List<GridColumnDef>();
        private object _dataSource;
        private string _gridInstanceId;

        // -------------------------------------------------------------------
        // 툴바 버튼 표시 여부
        // -------------------------------------------------------------------
        public bool ShowNewButton { get; set; } = true;
        public bool ShowSaveButton { get; set; } = true;
        public bool ShowDeleteButton { get; set; } = true;
        public bool ShowExcelButton { get; set; } = true;
        public bool ShowSearchBox { get; set; } = true;

        // -------------------------------------------------------------------
        // 툴바 버튼 텍스트 (다국어 대응)
        // -------------------------------------------------------------------
        public string NewButtonText { get; set; } = "신규";
        public string SaveButtonText { get; set; } = "저장";
        public string DeleteButtonText { get; set; } = "삭제";
        public string ExcelButtonText { get; set; } = "엑셀";
        public string SearchPlaceholder { get; set; } = "검색어를 입력하세요...";
        public string ExcelFileName { get; set; } = "export";

        // -------------------------------------------------------------------
        // JS 콜백 (전역 함수명을 문자열로 지정)
        // -------------------------------------------------------------------
        /// <summary>신규 버튼 클릭 시 호출할 전역 JS 함수명</summary>
        public string OnNewClick { get; set; }
        /// <summary>저장 버튼 클릭 시 호출할 전역 JS 함수명</summary>
        public string OnSaveClick { get; set; }
        /// <summary>삭제 버튼 클릭 시 호출할 전역 JS 함수명</summary>
        public string OnDeleteClick { get; set; }
        /// <summary>엑셀 버튼 클릭 시 호출할 전역 JS 함수명 (미지정 시 내부 엑셀 내보내기 실행)</summary>
        public string OnExcelClick { get; set; }
        /// <summary>행 클릭 시 호출할 전역 JS 함수명</summary>
        public string OnRowClick { get; set; }
        /// <summary>행 선택 변경 시 호출할 전역 JS 함수명</summary>
        public string OnRowSelected { get; set; }
        /// <summary>셀 값 변경 시 호출할 전역 JS 함수명</summary>
        public string OnCellValueChanged { get; set; }
        /// <summary>그리드 준비 완료 시 호출할 전역 JS 함수명</summary>
        public string OnGridReady { get; set; }

        // -------------------------------------------------------------------
        // 그리드 옵션
        // -------------------------------------------------------------------
        public string GridHeight { get; set; } = "500px";
        /// <summary>행 선택 모드: "single" | "multiple"</summary>
        public string RowSelection { get; set; } = "multiple";
        public bool EnablePagination { get; set; } = false;
        public int PageSize { get; set; } = 20;
        public bool AnimateRows { get; set; } = true;
        public bool EnableRangeSelection { get; set; } = false;
        public bool SuppressRowClickSelection { get; set; } = false;

        /// <summary>외부에서 사용할 JS 변수명 (window[GridId]로 접근 가능)</summary>
        public string GridId { get; set; }

        // -------------------------------------------------------------------
        // 컬럼 컬렉션
        // -------------------------------------------------------------------
        public List<GridColumnDef> Columns
        {
            get => _columns;
            set => _columns = value ?? new List<GridColumnDef>();
        }

        // -------------------------------------------------------------------
        // 데이터 소스 (DataTable 또는 JSON 문자열 지원)
        // -------------------------------------------------------------------
        public object DataSource
        {
            get => _dataSource;
            set => _dataSource = value;
        }

        // -------------------------------------------------------------------
        // 커스텀 버튼 컬렉션
        // -------------------------------------------------------------------
        public List<ToolbarButton> CustomButtons => _customButtons;

        // -------------------------------------------------------------------
        // 고유 그리드 인스턴스 ID (JS 전역 변수명으로 사용)
        // -------------------------------------------------------------------
        public string GridInstanceId
        {
            get
            {
                if (_gridInstanceId == null)
                {
                    string safeId = !string.IsNullOrEmpty(GridId) ? GridId
                        : "grid_" + ClientID.Replace("-", "_").Replace("$", "_");
                    _gridInstanceId = safeId;
                }
                return _gridInstanceId;
            }
        }

        // -------------------------------------------------------------------
        // 공개 메서드
        // -------------------------------------------------------------------

        /// <summary>그리드 컬럼을 일괄 설정합니다.</summary>
        public void SetColumns(List<GridColumnDef> columns)
        {
            _columns = columns ?? new List<GridColumnDef>();
        }

        /// <summary>단일 컬럼을 추가합니다.</summary>
        public void AddColumn(GridColumnDef column)
        {
            if (column != null) _columns.Add(column);
        }

        /// <summary>DataTable 또는 JSON 문자열 형태의 데이터를 설정합니다.</summary>
        public void SetDataSource(object dataSource)
        {
            _dataSource = dataSource;
        }

        /// <summary>커스텀 툴바 버튼을 추가합니다.</summary>
        public void AddCustomButton(ToolbarButton button)
        {
            if (button != null) _customButtons.Add(button);
        }

        // -------------------------------------------------------------------
        // 라이프사이클
        // -------------------------------------------------------------------
        protected void Page_Load(object sender, EventArgs e) { }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RegisterStartupScript();
        }

        // -------------------------------------------------------------------
        // 내부 구현
        // -------------------------------------------------------------------
        private void RegisterStartupScript()
        {
            var columnsJson = BuildColumnsJson();
            var rowDataJson = BuildRowDataJson();
            var configJson = BuildConfigJson();

            var sb = new StringBuilder();
            sb.AppendLine("(function () {");
            sb.AppendLine($"  var _cols = {columnsJson};");
            sb.AppendLine($"  var _data = {rowDataJson};");
            sb.AppendLine($"  var _cfg  = {configJson};");
            sb.AppendLine($"  _cfg.columns = _cols;");
            sb.AppendLine($"  _cfg.rowData = _data;");
            sb.AppendLine($"  window['{GridInstanceId}'] = new AgGridControl(_cfg);");
            sb.AppendLine("})();");

            var script = sb.ToString();
            var scriptKey = GridInstanceId + "_init";

            if (ScriptManager.GetCurrent(Page) != null)
                ScriptManager.RegisterStartupScript(this, GetType(), scriptKey, script, true);
            else
                Page.ClientScript.RegisterStartupScript(GetType(), scriptKey, script, true);
        }

        private string BuildColumnsJson()
        {
            var colList = new List<Dictionary<string, object>>();
            foreach (var col in _columns)
            {
                var d = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(col.Field)) d["field"] = col.Field;
                if (!string.IsNullOrEmpty(col.HeaderName)) d["headerName"] = col.HeaderName;
                if (col.Width.HasValue) d["width"] = col.Width.Value;
                if (col.MinWidth.HasValue) d["minWidth"] = col.MinWidth.Value;
                if (col.MaxWidth.HasValue) d["maxWidth"] = col.MaxWidth.Value;
                d["sortable"] = col.Sortable;
                d["filter"] = col.Filter ?? (object)true;
                d["resizable"] = col.Resizable;
                if (col.Editable) d["editable"] = col.Editable;
                if (col.CheckboxSelection) d["checkboxSelection"] = col.CheckboxSelection;
                if (col.HeaderCheckboxSelection) d["headerCheckboxSelection"] = col.HeaderCheckboxSelection;
                if (col.Hide) d["hide"] = col.Hide;
                if (!string.IsNullOrEmpty(col.Pinned)) d["pinned"] = col.Pinned;
                if (!string.IsNullOrEmpty(col.Type)) d["type"] = col.Type;
                if (!string.IsNullOrEmpty(col.CellClass)) d["cellClass"] = col.CellClass;
                if (!string.IsNullOrEmpty(col.HeaderClass)) d["headerClass"] = col.HeaderClass;
                if (!string.IsNullOrEmpty(col.FloatingFilter.ToString()) && col.FloatingFilter) d["floatingFilter"] = col.FloatingFilter;
                if (col.Flex) d["flex"] = col.FlexValue;

                // JS 함수명은 __fn__ 접두사로 마킹 후 직렬화 후 치환
                if (!string.IsNullOrEmpty(col.ValueFormatter)) d["valueFormatter"] = "__fn__" + col.ValueFormatter;
                if (!string.IsNullOrEmpty(col.CellRenderer)) d["cellRenderer"] = "__fn__" + col.CellRenderer;
                if (!string.IsNullOrEmpty(col.CellStyle)) d["cellStyle"] = "__fn__" + col.CellStyle;

                colList.Add(d);
            }

            var json = JsonConvert.SerializeObject(colList, Formatting.None);
            // "__fn__" 마킹된 값은 따옴표 없이 JS 함수 참조로 변환
            json = System.Text.RegularExpressions.Regex.Replace(
                json, "\"__fn__([^\"]+)\"", "$1");
            return json;
        }

        private string BuildRowDataJson()
        {
            if (_dataSource == null) return "[]";

            if (_dataSource is string jsonStr)
            {
                var trimmed = jsonStr.Trim();
                return (trimmed.StartsWith("[") || trimmed.StartsWith("{")) ? trimmed : "[]";
            }

            if (_dataSource is DataTable dt)
                return DataTableToJson(dt);

            return "[]";
        }

        private string DataTableToJson(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var d = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    d[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                rows.Add(d);
            }
            return JsonConvert.SerializeObject(rows, Formatting.None);
        }

        private string BuildConfigJson()
        {
            var cfg = new Dictionary<string, object>
            {
                ["gridInstanceId"] = GridInstanceId,
                ["rowSelection"] = RowSelection,
                ["enablePagination"] = EnablePagination,
                ["pageSize"] = PageSize,
                ["animateRows"] = AnimateRows,
                ["enableRangeSelection"] = EnableRangeSelection,
                ["suppressRowClickSelection"] = SuppressRowClickSelection,
                ["excelFileName"] = ExcelFileName,
            };

            // JS 콜백 함수명 (비어있으면 null)
            if (!string.IsNullOrWhiteSpace(OnNewClick)) cfg["onNewClick"] = OnNewClick;
            if (!string.IsNullOrWhiteSpace(OnSaveClick)) cfg["onSaveClick"] = OnSaveClick;
            if (!string.IsNullOrWhiteSpace(OnDeleteClick)) cfg["onDeleteClick"] = OnDeleteClick;
            if (!string.IsNullOrWhiteSpace(OnExcelClick)) cfg["onExcelClick"] = OnExcelClick;
            if (!string.IsNullOrWhiteSpace(OnRowClick)) cfg["onRowClick"] = OnRowClick;
            if (!string.IsNullOrWhiteSpace(OnRowSelected)) cfg["onRowSelected"] = OnRowSelected;
            if (!string.IsNullOrWhiteSpace(OnCellValueChanged)) cfg["onCellValueChanged"] = OnCellValueChanged;
            if (!string.IsNullOrWhiteSpace(OnGridReady)) cfg["onGridReady"] = OnGridReady;

            return JsonConvert.SerializeObject(cfg, Formatting.None);
        }
    }
}
