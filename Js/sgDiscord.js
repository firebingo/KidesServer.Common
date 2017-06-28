var baseUrl = window.location.href.split('/');
baseUrl.length = 3;
baseUrl = baseUrl.join('/');

var counts = { messageList: 10 };
var sortOrders = { messageList: "messageCount" };
var isDesc = { messageList: true };
var filterInput = { messageList: '' };
var loadFuncs = { messageList: loadMessageList, userInfo: loadUserInfo };
var serverId = '229596738615377920';
var placeholderAvatar = 'https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png';

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
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/message-count/list/?count=" + counts['messageList'] +
	"&serverId=" + serverId + "&sort=" + sortOrders['messageList'] + "&isDesc=" + isDesc['messageList']) + 
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
		<th class="list-table-header">Rank</th>\
		<th class="list-table-header header-sortable" onclick="changeSort(\'messageList\', \'messageCount\')">Message Count' + getSortArrow('messageList', 'messageCount') +'</th>\
		<th class="list-table-header">Roles</th>\
	</tr>';
	for(var i = 0; i < data.length; ++i) {
		var item = data[i];
		html += '\
		<tr class="' + (item.isDeleted ? 'user-removed-row' : '') + (item.isBanned ? 'user-banned-row' : '') + '">\
			<td class="list-table-cell cell-clickable"\
			onclick="loadUserInfo(\'' + item.userId + '\')">' + item.userName + '</td>\
			<td class="list-table-cell">' + item.rank + '</td>\
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
		<td class="footer-mid"></td>\
		<td class="footer-right">\
			<input id="message-list-limit-filter" placeholder="Filter by name" value="' + filterInput['messageList'] + '"/>\
			<button onclick="changeFilter(\'messageList\', \'message-list-limit-filter\')">Filter</button>\
		</td>\
	</div>';
	return html;
}

var userInfoArea = undefined;
var userTableArea = undefined;
var loadingUserInfo = false;

function loadUserInfo(id) {
	if(loadingUserInfo) { return; }
	loadingUserInfo = true;
	messageListAreaLoading.innerHTML = "<span>Loading...</span>";
	userInfoArea.style.display = "none";
	userTableArea.innerHTML = "";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/user-info/?userId=" + id + 
	'&serverId=' + serverId), userInfoSuccess, userInfoFailure, '');
}

function userInfoSuccess(resp) {
	userTableArea.innerHTML = buildUserInfo(resp);
	buildUserDensityChart(resp.messageDensity);
	messageListAreaLoading.innerHTML = "";
	userInfoArea.style.display = "flex";
	loadingUserInfo = false;
}

function userInfoFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error."
	}
	message = resp.responseJSON.Message;
	if(!message) {
		message = "There was an error in handling an error."
	}
	messageListAreaLoading.innerHTML = "<span>" + message + "</span>";
	loadingUserInfo = false;
}

function buildUserInfo(data) {
	var jDate = moment(data.joinedDate);
	var html = '\
	<div class="info-table" style="width: ' + messageListTable.offsetWidth + '">\
		<div class="info-table-row">\
			<div class="avatar-cell"><img src="' + (data.avatarUrl ? data.avatarUrl : placeholderAvatar) + '"/></div>\
			<div class="info-cell">\
				<div class="info-cell-area">\
					<div><span class="bold-text">Username:</span><span class="' + 
					(data.isDeleted ? 'user-removed' : '') + (data.isBanned ? 'user-banned' : '') + '">' + data.userName + '</span></div>\
					<div><span class="bold-text">Nickname:</span><span>' + (data.nickName ? data.nickName : 'None') + '</span></div>\
					<div><span class="bold-text">Joined At:</span><span>' + jDate.format('MMMM Do YYYY, h:mm a') + '</span></div>' +
					(data.isBot ? '<div><span class="bold-text">Bot</span></div>' : '') +
					'<div><span class="bold-text">Roles:</span><span>' + (data.role ? data.role : 'None') + '</span></div>\
				</div>\
			</div>\
		</div>\
		<div class="info-table-row" id="user-info-chart-area">\
		</div>\
	</div>';
	return html;
}

function buildUserDensityChart(data) {
	var chartData = new google.visualization.DataTable();
	chartData.addColumn('string', 'Date');
	chartData.addColumn('number', 'Message Count');
	var rowsToAdd = [];
	for(var i = data.length-1; i > -1; --i) {
		var date = moment.utc(data[i].date).format('MMMM, YYYY');
		rowsToAdd.push([date, data[i].messageCount]);
	}
	chartData.addRows(rowsToAdd);
	
	var options = {
		'title': 'Message Counts by Month',
		width: messageListTable.offsetWidth - 20,
		vAxis: { format: 'decimal', gridlines: {color: '#818181'}, baselineColor: '#818181' },
		hAxis: { textPosition: 'none' },
		legend: 'none',
		backgroundColor: '#393939',
		colors: ['#738bd7']
	};
	var chart = new google.visualization.ColumnChart(document.getElementById('user-info-chart-area'));
	chart.draw(chartData, options);
	var textBlocks = $('#user-info-chart-area').find("text");
	for(var i = 0; i < textBlocks.length; ++i) {
		textBlocks.attr("fill", 'rgba(255,255,255,.7)');
	}
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
	google.charts.load('current', {'packages':['corechart']});
	messageListArea = document.getElementById('message-table-area');
	messageListAreaLoading = document.getElementById('message-table-loading');
	messageListTable = document.getElementById('message-list-table');
	userInfoArea = document.getElementById('user-info');
	userTableArea = document.getElementById('user-info-table-area');
	loadFuncs['messageList']();
}