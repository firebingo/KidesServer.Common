var baseUrl = window.location.href.split('/');
baseUrl.length = 3;
baseUrl = baseUrl.join('/');

var counts = { messageList: 10, roleMesList: 10, emojiList: 10 };
var sortOrders = { messageList: "messageCount", roleMesList: "messageCount", emojiList: "emojiCount" };
var isDesc = { messageList: true, roleMesList: true, emojiList: true };
var filterInput = { messageList: '', roleMesList: '', emojiList: '', emojiListId: '' };
var loadFuncs = { messageList: loadMessageList, userInfo: loadUserInfo, roleMesList: loadRoleMessageList, emojiList: loadEmojiList, emojiListId: loadEmojiList };
var serverId = '229596738615377920';
var placeholderAvatar = 'https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png';
var genericErrorArea = undefined;

function getJSON(url, callback, errorCallback, data) {
	$.ajax({
		dataType : "json",
		url : url,
		data : data,
		success : callback,
		error : errorCallback
	});
};

//#region Message List
var messageListArea = undefined;
var messageListTable = undefined;
var messageListAreaLoading = undefined;

function loadMessageList() {
	messageListAreaLoading.innerHTML = "<span>Loading...</span>";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/message-count/list/?count=" + counts['messageList'] +
	"&serverId=" + serverId + "&start=0" + "&sort=" + sortOrders['messageList'] + "&isDesc=" + isDesc['messageList']) + 
	(filterInput['messageList'] ? ("&userFilter=" + encodeURIComponent(filterInput['messageList'])) : '') + "&includeTotal=true", 
	messageListSucccess, messageListFailure, '');
}

function messageListSucccess(resp) {
	messageListAreaLoading.innerHTML = "";
	messageListTable.innerHTML = buildMessageList(resp.results);
}

function messageListFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
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
		<tr class="' + (item.isDeleted ? 'user-removed-row ' : '') + (item.isBanned ? 'user-banned-row' : '') + '">\
			<td class="list-table-cell cell-clickable"\
			onclick="loadUserInfo(\'' + item.userId + '\')">' + item.userName + '</td>\
			<td class="list-table-cell">' + item.rank + '</td>\
			<td class="list-table-cell">' + item.messageCount + '</td>\
			<td class="list-table-cell">' + item.role + '</td>\
		</tr>\
		';
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
	</tr>';
	return html;
}

//#endregion

//#region User Info
var userInfoArea = undefined;
var userTableArea = undefined;
var loadingUserInfo = false;

function loadUserInfo(id) {
	if(!id || id == -1) { return; }
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
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
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
					<div><span class="bold-text">UserID:</span><span>' + (data.userId) + '</span></div>\
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

//#endregion

//#region Role Message List
var roleMessageListArea = undefined;
var roleMessageListTable = undefined;
var roleMessageListAreaLoading = undefined;
var roleList = undefined;
var seletedRoleId = '229598038438445056'; //Lydian Student role id

function loadRoleList() {	
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/roles/?serverId=" + serverId), roleListSuccess, roleListFailure, '');
}

function roleListSuccess(resp) {
	roleList = resp.results;
	loadFuncs['roleMesList']();
}

function roleListFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
	}
	genericErrorArea.innerHTML = "<span>" + message + "</span>";
	roleMessageListAreaLoading.innerHTML = "<span>Failed to load role list</span>";
}

function loadRoleMessageList() {
	roleMessageListAreaLoading.innerHTML = "<span>Loading...</span>";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/message-count/list/?count=" + counts['roleMesList'] +
	"&serverId=" + serverId + "&start=0" + "&sort=" + sortOrders['roleMesList'] + "&isDesc=" + isDesc['roleMesList']) + 
	(filterInput['roleMesList'] ? ("&userFilter=" + encodeURIComponent(filterInput['roleMesList'])) : '') + '&roleId=' + seletedRoleId + "&includeTotal=true", 
	roleMesListSucccess, roleMesListFailure, '');
}

function roleMesListSucccess(resp) {
	roleMessageListAreaLoading.innerHTML = "";
	roleMessageListTable.innerHTML = buildMessageRoleList(resp.results);
}

function roleMesListFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
	}
	roleMessageListAreaLoading.innerHTML = "<span>" + message + "</span>";
}

function buildMessageRoleList(data) {
	var html = '\
	<tr class="list-table-header-row">\
		<th class="list-table-header header-sortable" onclick="changeSort(\'roleMesList\', \'userName\')">User' + getSortArrow('roleMesList', 'userName') +'</th>\
		<th class="list-table-header">Rank</th>\
		<th class="list-table-header header-sortable" onclick="changeSort(\'roleMesList\', \'messageCount\')">Message Count' + getSortArrow('roleMesList', 'messageCount') +'</th>\
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
		';
	}
	html+= '\
	<tr class="list-table-footer">\
		<td class="footer-left">\
			<span>Limit: </span>\
			<select id="role-message-list-limit-dd" onchange="changeLimit(\'roleMesList\', \'role-message-list-limit-dd\')">\
				<option value="10"' + (counts['roleMesList'] == 10 ? 'selected="selected"' : '' ) + '>10</option>\
				<option value="25"' + (counts['roleMesList'] == 25 ? 'selected="selected"' : '' ) + '>25</option>\
				<option value="50"' + (counts['roleMesList'] == 50 ? 'selected="selected"' : '' ) + '>50</option>\
			</select>\
			<select id="role-message-list-role-dd" onchange="changeSelectedRole()">';
	
	for(var i = 0; i < roleList.length; ++i) {
		var item = roleList[i];
		if(!item.isEveryone) {
			html += '<option style="color:' + item.roleColor + ' ;" value="' + item.roleId + '" ' + (seletedRoleId == item.roleId ? 'selected="selected"' : '') + '>' + item.roleName + '</option>';
		}
	}
			
	html+=	'</select>\
		</td>\
		<td class="footer-mid"></td>\
		<td class="footer-mid"></td>\
		<td class="footer-right">\
			<input id="role-message-list-limit-filter" placeholder="Filter by name" value="' + filterInput['roleMesList'] + '"/>\
			<button onclick="changeFilter(\'roleMesList\', \'role-message-list-limit-filter\')">Filter</button>\
		</td>\
	</tr>';
	return html;
}

function changeSelectedRole() {
	var dd = document.getElementById('role-message-list-role-dd');
	if(!dd) return;
	var id = dd.options[dd.selectedIndex].value;
	seletedRoleId = id;
	loadFuncs['roleMesList']();
}

//#endregion

//#region Emoji Count List
var emojiListArea = null;
var emojiListAreaLoading = null;
var emojiListTable = null;

function loadEmojiList() {
	emojiListAreaLoading.innerHTML = "<span>Loading...</span>";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/emoji-count/list/?count=" + counts['emojiList'] +
	"&serverId=" + serverId + "&start=0" + "&sort=" + sortOrders['emojiList'] + "&isDesc=" + isDesc['emojiList']) + 
	(filterInput['emojiList'] ? ("&nameFilter=" + filterInput['emojiList']) : '') + "&includeTotal=true&userFilterId=" + filterInput['emojiListId'], 
	emojiListSucccess, emojiListFailure, '');
}

function emojiListSucccess(resp) {
	emojiListAreaLoading.innerHTML = "";
	emojiListTable.innerHTML = buildEmojiList(resp.results);
}

function emojiListFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
	}
	emojiListAreaLoading.innerHTML = "<span>" + message + "</span>";
}

function buildEmojiList(data) {
	var html = '\
	<tr class="list-table-header-row">\
		<th class="list-table-header"></th>\
		<th class="list-table-header header-sortable" onclick="changeSort(\'emojiList\', \'emojiName\')">Name' + getSortArrow('emojiList', 'emojiName') +'</th>\
		<th class="list-table-header">Rank</th>\
		<th class="list-table-header header-sortable" onclick="changeSort(\'emojiList\', \'emojiCount\')">Use Count' + getSortArrow('emojiList', 'emojiCount') +'</th>\
	</tr>';
	for(var i = 0; i < data.length; ++i) {
		var item = data[i];
		html += '\
		<tr>\
			<td class="list-table-cell"><img class="emoji-table-img ' + (item.emojiId == '' ? 'hide-if-total' : '') + '" src="' + item.emojiImg + '"/></td>\
			<td class="list-table-cell"\>' + item.emojiName + '</td>\
			<td class="list-table-cell">' + item.rank + '</td>\
			<td class="list-table-cell">' + item.useCount + '</td>\
		</tr>\
		';
	}
	html+= '\
	<tr class="list-table-footer">\
		<td class="footer-left">\
			<span>Limit: </span>\
			<select id="emoji-list-limit-dd" onchange="changeLimit(\'emojiList\', \'emoji-list-limit-dd\')">\
				<option value="10"' + (counts['emojiList'] == 10 ? 'selected="selected"' : '' ) + '>10</option>\
				<option value="25"' + (counts['emojiList'] == 25 ? 'selected="selected"' : '' ) + '>25</option>\
				<option value="50"' + (counts['emojiList'] == 50 ? 'selected="selected"' : '' ) + '>50</option>\
			</select>\
		</td>\
		<td class="footer-mid">\
			<div class="footer-container">\
				<input id="emoji-list-name-filter" placeholder="Filter by name" value="' + filterInput['emojiList'] + '"/>\
				<button onclick="changeFilter(\'emojiList\', \'emoji-list-name-filter\')">Filter</button>\
			</div>\
		</td>\
		<td class="footer-mid"></td>\
		<td class="footer-right">\
			<div class="footer-container">\
				<input id="emoji-list-id-filter" placeholder="Filter by UserID" value="' + filterInput['emojiListId'] + '"/>\
				<button onclick="changeFilter(\'emojiListId\', \'emoji-list-id-filter\')">Filter</button>\
			</div>\
		</td>\
	</tr>';
	return html;
}
//#endregion

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
	filterInput[tableType] = input.value;
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
	genericErrorArea = document.getElementById('generic-error');
	messageListArea = document.getElementById('message-table-area');
	messageListAreaLoading = document.getElementById('message-table-loading');
	messageListTable = document.getElementById('message-list-table');
	userInfoArea = document.getElementById('user-info');
	userTableArea = document.getElementById('user-info-table-area');
	roleMessageListArea = document.getElementById('role-message-table-area');
	roleMessageListTable = document.getElementById('role-message-list-table');
	roleMessageListAreaLoading = document.getElementById('role-message-table-loading');
	emojiListArea = document.getElementById('emoji-table-area');
	emojiListAreaLoading = document.getElementById('emoji-table-loading');
	emojiListTable = document.getElementById('emoji-list-table');
	loadFuncs['messageList']();
	loadFuncs['emojiList']();
	loadRoleList();
}