using System;
using System.Collections.Generic;
using System.Data;
using WebApp.Controls;

namespace WebApp
{
    public partial class GridSample : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ConfigureGrid();
            }
        }

        private void ConfigureGrid()
        {
            // -------------------------------------------------------
            // 1. 컬럼 설정
            // -------------------------------------------------------
            var columns = new List<GridColumnDef>
            {
                new GridColumnDef
                {
                    Field                   = "empId",
                    HeaderName              = "사원번호",
                    Width                   = 100,
                    CheckboxSelection       = true,
                    HeaderCheckboxSelection = true,
                    Pinned                  = "left"
                },
                new GridColumnDef
                {
                    Field      = "name",
                    HeaderName = "이름",
                    Width      = 120,
                    Editable   = true,
                },
                new GridColumnDef
                {
                    Field      = "dept",
                    HeaderName = "부서",
                    Width      = 130,
                    Filter     = "agTextColumnFilter",
                },
                new GridColumnDef
                {
                    Field      = "position",
                    HeaderName = "직급",
                    Width      = 100,
                },
                new GridColumnDef
                {
                    Field      = "salary",
                    HeaderName = "연봉",
                    Width      = 120,
                    Type       = "numericColumn",
                    Filter     = "agNumberColumnFilter",
                    ValueFormatter = "salaryFormatter",   // JS 포매터 함수명
                },
                new GridColumnDef
                {
                    Field      = "joinDate",
                    HeaderName = "입사일",
                    Width      = 110,
                    Filter     = "agDateColumnFilter",
                },
                new GridColumnDef
                {
                    Field      = "email",
                    HeaderName = "이메일",
                    MinWidth   = 200,
                    Flex       = true,
                    FlexValue  = 1,
                },
                new GridColumnDef
                {
                    Field      = "status",
                    HeaderName = "상태",
                    Width      = 90,
                    CellRenderer = "statusRenderer",   // JS 렌더러 함수명
                    CellStyle    = "statusCellStyle",   // JS 스타일 함수명
                }
            };

            EmployeeGrid.SetColumns(columns);

            // -------------------------------------------------------
            // 2. 데이터 소스 설정 (DataTable 예시)
            // -------------------------------------------------------
            EmployeeGrid.DataSource = CreateSampleDataTable();

            // -------------------------------------------------------
            // 3. 커스텀 버튼 추가
            // -------------------------------------------------------
            EmployeeGrid.AddCustomButton(new ToolbarButton
            {
                Id       = "btnPrint",
                Text     = "인쇄",
                IconHtml = "<svg viewBox='0 0 16 16' width='14' height='14' fill='currentColor'><path d='M2.5 8a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1z'/><path d='M5 1a2 2 0 0 0-2 2v2H2a2 2 0 0 0-2 2v3a2 2 0 0 0 2 2h1v1a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2v-1h1a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-1V3a2 2 0 0 0-2-2H5zM4 3a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1v2H4V3zm1 5a2 2 0 0 0-2 2v1H2a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-1v-1a2 2 0 0 0-2-2H5zm7 2v3a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1v-3a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1z'/></svg>",
                CssClass = "btn-toolbar btn-custom",
                OnClick  = "handlePrint",
                Title    = "현재 그리드 인쇄"
            });

            EmployeeGrid.AddCustomButton(new ToolbarButton
            {
                Id       = "btnRefresh",
                Text     = "새로고침",
                IconHtml = "<svg viewBox='0 0 16 16' width='14' height='14' fill='currentColor'><path d='M11.534 7h3.932a.25.25 0 0 1 .192.41l-1.966 2.36a.25.25 0 0 1-.384 0l-1.966-2.36a.25.25 0 0 1 .192-.41zm-11 2h3.932a.25.25 0 0 0 .192-.41L2.692 6.23a.25.25 0 0 0-.384 0L.342 8.59A.25.25 0 0 0 .534 9z'/><path fill-rule='evenodd' d='M8 3c-1.552 0-2.94.707-3.857 1.818a.5.5 0 1 1-.771-.636A6.002 6.002 0 0 1 13.917 7H12.9A5.002 5.002 0 0 0 8 3zM3.1 9a5.002 5.002 0 0 0 8.757 2.182.5.5 0 1 1 .771.636A6.002 6.002 0 0 1 2.083 9H3.1z'/></svg>",
                CssClass = "btn-toolbar btn-custom",
                OnClick  = "handleRefresh",
                Title    = "데이터 새로고침"
            });
        }

        // -------------------------------------------------------
        // 샘플 DataTable 생성
        // -------------------------------------------------------
        private DataTable CreateSampleDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("empId",    typeof(string));
            dt.Columns.Add("name",     typeof(string));
            dt.Columns.Add("dept",     typeof(string));
            dt.Columns.Add("position", typeof(string));
            dt.Columns.Add("salary",   typeof(int));
            dt.Columns.Add("joinDate", typeof(string));
            dt.Columns.Add("email",    typeof(string));
            dt.Columns.Add("status",   typeof(string));

            var data = new object[,]
            {
                { "E001", "김민준", "개발팀",   "팀장",   85000000, "2018-03-05", "minjun.kim@company.com",   "재직" },
                { "E002", "이서연", "기획팀",   "대리",   48000000, "2020-07-12", "seoyeon.lee@company.com",  "재직" },
                { "E003", "박지호", "개발팀",   "사원",   38000000, "2022-01-10", "jiho.park@company.com",    "재직" },
                { "E004", "최수아", "인사팀",   "과장",   60000000, "2017-09-20", "sua.choi@company.com",     "재직" },
                { "E005", "정태양", "영업팀",   "사원",   36000000, "2023-03-15", "taeyang.jung@company.com", "수습" },
                { "E006", "윤하은", "개발팀",   "대리",   52000000, "2019-11-01", "haeun.yoon@company.com",   "재직" },
                { "E007", "임재원", "디자인팀", "팀장",   78000000, "2016-05-25", "jaewon.lim@company.com",   "재직" },
                { "E008", "강나연", "마케팅팀", "과장",   62000000, "2018-08-08", "nayeon.kang@company.com",  "재직" },
                { "E009", "조현우", "개발팀",   "사원",   40000000, "2021-06-14", "hyunwoo.cho@company.com",  "재직" },
                { "E010", "송미래", "기획팀",   "팀장",   80000000, "2015-02-28", "mirae.song@company.com",   "재직" },
                { "E011", "한도윤", "영업팀",   "대리",   50000000, "2020-04-03", "doyun.han@company.com",    "재직" },
                { "E012", "오예린", "인사팀",   "사원",   35000000, "2023-08-21", "yerin.oh@company.com",     "수습" },
                { "E013", "신준혁", "개발팀",   "부장",   95000000, "2013-10-17", "junhyuk.shin@company.com", "재직" },
                { "E014", "류지아", "디자인팀", "대리",   49000000, "2021-02-08", "jia.ryu@company.com",      "재직" },
                { "E015", "문세진", "마케팅팀", "사원",   37000000, "2022-11-30", "sejin.moon@company.com",   "휴직" },
            };

            for (int i = 0; i < data.GetLength(0); i++)
            {
                dt.Rows.Add(
                    data[i, 0], data[i, 1], data[i, 2], data[i, 3],
                    data[i, 4], data[i, 5], data[i, 6], data[i, 7]
                );
            }

            return dt;
        }

        // -------------------------------------------------------
        // 예시: JSON 데이터 소스로 두 번째 그리드 설정
        // -------------------------------------------------------
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // JSON 문자열 방식 예시 - 두 번째 그리드(ProductGrid)에 적용
                var jsonData = @"[
                    {""productId"":""P001"",""productName"":""노트북 Pro 14"",   ""category"":""전자기기"",""price"":1850000,""stock"":45,""rating"":4.5},
                    {""productId"":""P002"",""productName"":""무선 마우스"",      ""category"":""주변기기"",""price"":  45000,""stock"":230,""rating"":4.2},
                    {""productId"":""P003"",""productName"":""기계식 키보드"",    ""category"":""주변기기"",""price"": 135000,""stock"": 88,""rating"":4.7},
                    {""productId"":""P004"",""productName"":""4K 모니터 27"",    ""category"":""전자기기"",""price"": 720000,""stock"": 32,""rating"":4.4},
                    {""productId"":""P005"",""productName"":""USB-C 허브"",      ""category"":""주변기기"",""price"":  68000,""stock"":175,""rating"":4.1},
                    {""productId"":""P006"",""productName"":""웹캠 Full HD"",    ""category"":""주변기기"",""price"":  95000,""stock"": 62,""rating"":4.3},
                    {""productId"":""P007"",""productName"":""태블릿 10.9"",     ""category"":""전자기기"",""price"": 980000,""stock"": 28,""rating"":4.6},
                    {""productId"":""P008"",""productName"":""블루투스 스피커"", ""category"":""음향기기"",""price"": 159000,""stock"":110,""rating"":4.0},
                    {""productId"":""P009"",""productName"":""노이즈캔슬링 이어폰"",""category"":""음향기기"",""price"":325000,""stock"": 56,""rating"":4.8},
                    {""productId"":""P010"",""productName"":""스마트 충전기"",   ""category"":""액세서리"",""price"":  38000,""stock"":310,""rating"":3.9}
                ]";

                ProductGrid.SetColumns(new List<GridColumnDef>
                {
                    new GridColumnDef { Field = "productId",   HeaderName = "제품코드",  Width = 100,
                                        CheckboxSelection = true, HeaderCheckboxSelection = true },
                    new GridColumnDef { Field = "productName", HeaderName = "제품명",    MinWidth = 180, Flex = true, FlexValue = 1 },
                    new GridColumnDef { Field = "category",    HeaderName = "카테고리",  Width = 120 },
                    new GridColumnDef { Field = "price",       HeaderName = "가격(원)",  Width = 130,
                                        Type = "numericColumn", ValueFormatter = "priceFormatter" },
                    new GridColumnDef { Field = "stock",       HeaderName = "재고",      Width = 80,
                                        Type = "numericColumn", CellStyle = "stockCellStyle" },
                    new GridColumnDef { Field = "rating",      HeaderName = "평점",      Width = 90,
                                        CellRenderer = "ratingRenderer" },
                });

                ProductGrid.DataSource = jsonData;
                ProductGrid.ShowDeleteButton = false;
                ProductGrid.ShowSaveButton = false;
                ProductGrid.GridHeight = "360px";
                ProductGrid.ExcelFileName = "product_list";
            }
        }
    }
}
