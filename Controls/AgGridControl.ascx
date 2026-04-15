<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AgGridControl.ascx.cs" Inherits="WebApp.Controls.AgGridControl" %>
<div class="aggrid-wrapper" id="<%=ClientID%>_wrapper">

    <%-- ===== 툴바 ===== --%>
    <div class="aggrid-toolbar" id="<%=ClientID%>_toolbar">

        <%-- 좌측 버튼 그룹 --%>
        <div class="aggrid-toolbar-left">

            <% if (ShowNewButton) { %>
            <button type="button"
                    class="btn-toolbar btn-new"
                    id="<%=ClientID%>_btnNew"
                    title="신규 행 추가"
                    onclick="window['<%=GridInstanceId%>'] && window['<%=GridInstanceId%>'].handleNew(); return false;">
                <span class="btn-icon">
                    <svg viewBox="0 0 16 16" width="14" height="14" fill="currentColor">
                        <path d="M8 1a.5.5 0 0 1 .5.5v6h6a.5.5 0 0 1 0 1h-6v6a.5.5 0 0 1-1 0v-6h-6a.5.5 0 0 1 0-1h6v-6A.5.5 0 0 1 8 1z"/>
                    </svg>
                </span>
                <%=NewButtonText%>
            </button>
            <% } %>

            <% if (ShowSaveButton) { %>
            <button type="button"
                    class="btn-toolbar btn-save"
                    id="<%=ClientID%>_btnSave"
                    title="변경 내용 저장"
                    onclick="window['<%=GridInstanceId%>'] && window['<%=GridInstanceId%>'].handleSave(); return false;">
                <span class="btn-icon">
                    <svg viewBox="0 0 16 16" width="14" height="14" fill="currentColor">
                        <path d="M2 1a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V2a1 1 0 0 0-1-1H9.5a1 1 0 0 0-1 1v4.5h2a.5.5 0 0 1 .354.854l-2.5 2.5a.5.5 0 0 1-.708 0l-2.5-2.5A.5.5 0 0 1 5.5 6.5h2V2a2 2 0 0 1 2-2H14a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2h2.5a.5.5 0 0 1 0 1H2z"/>
                    </svg>
                </span>
                <%=SaveButtonText%>
            </button>
            <% } %>

            <% if (ShowDeleteButton) { %>
            <button type="button"
                    class="btn-toolbar btn-delete"
                    id="<%=ClientID%>_btnDelete"
                    title="선택 행 삭제"
                    onclick="window['<%=GridInstanceId%>'] && window['<%=GridInstanceId%>'].handleDelete(); return false;">
                <span class="btn-icon">
                    <svg viewBox="0 0 16 16" width="14" height="14" fill="currentColor">
                        <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/>
                        <path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>
                    </svg>
                </span>
                <%=DeleteButtonText%>
            </button>
            <% } %>

            <% if (ShowExcelButton) { %>
            <button type="button"
                    class="btn-toolbar btn-excel"
                    id="<%=ClientID%>_btnExcel"
                    title="Excel 파일로 내보내기"
                    onclick="window['<%=GridInstanceId%>'] && window['<%=GridInstanceId%>'].handleExcel(); return false;">
                <span class="btn-icon">
                    <svg viewBox="0 0 16 16" width="14" height="14" fill="currentColor">
                        <path d="M14 4.5V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2h5.5L14 4.5zm-3 0A1.5 1.5 0 0 1 9.5 3V1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h-2z"/>
                        <path d="M5 6h6v1H5V6zm0 2h6v1H5V8zm0 2h3v1H5v-1z"/>
                    </svg>
                </span>
                <%=ExcelButtonText%>
            </button>
            <% } %>

            <%-- 커스텀 버튼 --%>
            <% foreach (var btn in CustomButtons) { %>
            <% if (!btn.Hidden) { %>
            <button type="button"
                    class="<%=btn.CssClass%>"
                    id="<%=btn.Id%>"
                    title="<%=btn.Title ?? btn.Text%>"
                    onclick="window['<%=GridInstanceId%>'] && window['<%=GridInstanceId%>'].handleCustomButton('<%=btn.Id%>', '<%=btn.OnClick%>'); return false;">
                <% if (!string.IsNullOrEmpty(btn.IconHtml)) { %>
                <span class="btn-icon"><%=btn.IconHtml%></span>
                <% } %>
                <%=btn.Text%>
            </button>
            <% } %>
            <% } %>
        </div>

        <%-- 우측 검색창 --%>
        <% if (ShowSearchBox) { %>
        <div class="aggrid-toolbar-right">
            <div class="aggrid-search-box">
                <svg class="search-icon" viewBox="0 0 16 16" width="14" height="14" fill="currentColor">
                    <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.099zm-5.242 1.656a5.5 5.5 0 1 1 0-11 5.5 5.5 0 0 1 0 11z"/>
                </svg>
                <input type="text"
                       id="<%=ClientID%>_searchInput"
                       class="search-input"
                       placeholder="<%=SearchPlaceholder%>"
                       oninput="window['<%=GridInstanceId%>'] && window['<%=GridInstanceId%>'].handleSearch(this.value)" />
                <button type="button" class="search-clear"
                        onclick="document.getElementById('<%=ClientID%>_searchInput').value=''; window['<%=GridInstanceId%>'] && window['<%=GridInstanceId%>'].handleSearch('');"
                        title="검색 초기화">
                    &times;
                </button>
            </div>
        </div>
        <% } %>
    </div>

    <%-- ===== 그리드 컨테이너 ===== --%>
    <div class="aggrid-grid-container ag-theme-alpine"
         id="<%=ClientID%>_grid"
         style="height: <%=GridHeight%>; width: 100%;">
    </div>

</div>
