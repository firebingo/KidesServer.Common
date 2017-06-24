var baseUrl = window.location.href.split('/');
baseUrl.length = 3;
baseUrl = baseUrl.join('/');

var counts = { messageList: 10 };
var sortOrders = { messageList: "messageCount" };
var isDesc = { messageList: true };
var filterInput = { messageList: '' };
var loadFuncs = { messageList: loadMessageList };

function getJSON(url, callback, errorCallback, data) {
	$.ajax({
		dataType : "json",
		url : url,
		data : data,
		success : callback,
		error : errorCallback
	});
};

var messageListArea = undefined;
var messageListTable = undefined;
var messageListAreaLoading = undefined;

function loadMessageList() {
	messageListAreaLoading.innerHTML = "<span>Loading...</span>";
	getJSON(("https://server.icebingo.io:25563/api/v1/message-count/list/?count=" + counts['messageList'] +
	"&serverId=229596738615377920&sort=" + sortOrders['messageList'] + "&isDesc=" + isDesc['messageList']) + 
	(filterInput['messageList'] ? ("&userFilter=" + filterInput['messageList']) : ''), 
	messageListSucccess, messageListFailure, '');
}

function messageListSucccess(resp) {
	messageListAreaLoading.innerHTML = "";
	messageListTable.innerHTML = buildMessageList(resp.results);
}

function messageListFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error."
	}
	message = resp.responseJSON.Message;
	if(!message) {
		message = "There was an error in handling an error."
	}
	messageListAreaLoading.innerHTML = "<span>" + message + "</span>";
}

function buildMessageList(data) {
	var html = '\
	<tr class="list-table-header-row">\
		<th class="list-table-header header-sortable" onclick="changeSort(\'messageList\', \'userName\')">User' + getSortArrow('messageList', 'userName') +'</th>\
		<th class="list-table-header header-sortable" onclick="changeSort(\'messageList\', \'messageCount\')">Message Count' + getSortArrow('messageList', 'messageCount') +'</th>\
		<th class="list-table-header">Roles</th>\
	</tr>';
	for(var i = 0; i < data.length; ++i) {
		var item = data[i];
		html += '\
		<tr' + (item.isDeleted ? ' class="user-removed-row"' : '') + '">\
			<td class="list-table-cell cell-clickable"\
			onclick="">' + item.userName + '</td>\
			<td class="list-table-cell">' + item.messageCount + '</td>\
			<td class="list-table-cell">' + item.role + '</td>\
		</tr>\
		'
	}
	html+= '\
	<tr class="list-table-footer">\
		<td class="footer-left">\
			<span>Limit: </span>\
			<select id="message-list-limit-dd" onchange="changeLimit(\'messageList\', \'message-list-limit-dd\')">\
				<option value="10"' + (counts['messageList'] == 10 ? 'selected="selected"' : '' ) + '>10</option>\
				<option value="25"' + (counts['messageList'] == 25 ? 'selected="selected"' : '' ) + '>25</option>\
				<option value="50"' + (counts['messageList'] == 50 ? 'selected="selected"' : '' ) + '>50</option>\
			</select>\
		</td>\
		<td class="footer-mid"></td>\
		<td class="footer-right">\
			<input id="message-list-limit-filter" placeholder="Filter by name" value="' + filterInput['messageList'] + '"/>\
			<button onclick="changeFilter(\'messageList\', \'message-list-limit-filter\')">Filter</button>\
		</td>\
	</div>';
	return html;
}

function changeLimit(tableType, id) {
	var dd = document.getElementById(id);
	if(!dd) return;
	var limit = parseInt(dd.options[dd.selectedIndex].value);
	if(limit > 100) { limit = 100; }
	if(limit < 0) { limit = 1; }
	counts[tableType] = limit;
	loadFuncs[tableType]();
}

function changeFilter(tableType, id) {
	var input = document.getElementById(id);
	if(!input) return;
	filterInput['messageList'] = input.value;
	loadFuncs[tableType]();
}

function changeSort(tableType, field) {
	if(sortOrders[tableType] == field) {
		isDesc[tableType] = !isDesc[tableType];
	} else {
		sortOrders[tableType] = field;
	}
	loadFuncs[tableType]();
}

function getSortArrow(tableType, field) {
	if(sortOrders[tableType] == field) {
		if(isDesc[tableType]) {
			return ' ▼';
		} else {
			return ' ▲';
		}
	}	
	return '';
}

window.onload = function () {
	messageListArea = document.getElementById('message-table-area');
	messageListAreaLoading = document.getElementById('message-table-loading');
	messageListTable = document.getElementById('message-list-table');
	loadFuncs['messageList']();
}