var counts = { messageList: 10, roleMesList: 10, emojiList: 10, wordList: 10 };
var sortOrders = { messageList: "messageCount", roleMesList: "messageCount", emojiList: "emojiCount", wordList: 'count', statUCnt: 1, statUnU: 1 };
var isDesc = { messageList: true, roleMesList: true, emojiList: true, wordList: true };
var filterInput = { messageList: '', roleMesList: '', emojiList: '', emojiListId: '', wordList: '', wordListId: '', wordListFloor: '', wordListEnglish: false };
var loadFuncs = { messageList: loadMessageList, userInfo: loadUserInfo, 
				  roleMesList: loadRoleMessageList, emojiList: loadEmojiList, 
				  emojiListId: loadEmojiList, wordList: loadWordList,
				  wordListId: loadWordList, wordListFloor: loadWordList,
				  wordListEnglish: loadWordList, stats: loadStats};
var serverId = '229596738615377920';
var placeholderAvatar = 'https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png';
var genericErrorArea = undefined;
var loadProgress = 0;
var endLoadCount = 5;

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
	loadProgress++;
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
	loadProgress++;
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
	//userInfoArea.style.display = "none";
	//userTableArea.innerHTML = "";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/user-info/?userId=" + id + 
	'&serverId=' + serverId), userInfoSuccess, userInfoFailure, '');
}

function userInfoSuccess(resp) {
	userTableArea.innerHTML = buildUserInfo(resp);
	buildUserDensityChart(resp.messageDensity);
	messageListAreaLoading.innerHTML = "";
	userInfoArea.style.display = "flex";
	setTimeout(function() { userInfoArea.style.opacity = "1"; });
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
	<div class="info-table" style="width: ' + 900 + ';">\
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
		width: 880,
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
	loadProgress++;
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
	loadProgress++;
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
	loadProgress++;
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
	loadProgress++;
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
	loadProgress++;
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
			<td class="list-table-cell"><img id="emoji-' + item.emojiId + '" onerror="emojiOnError(\'' + item.emojiId + '\')" class="emoji-table-img ' 
			+ (item.emojiId == '' ? 'hide-if-total' : '') + '" src="' + item.emojiImg.replace('.png', '.gif') + '"/></td>\
			<td class="list-table-cell"\>' + item.emojiName + '</td>\
			<td class="list-table-cell">' + item.rank + '</td>\
			<td class="list-table-cell">' + item.useCount + '</td>\
		</tr>';
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

function emojiOnError(id)
{
	var imageEl = document.getElementById('emoji-' + id);
	if(imageEl) {
		if(imageEl.src.indexOf('.gif') !== -1) {
			imageEl.src = imageEl.src.replace('.gif', '.png');
		}
	}
}
//#endregion

//#region Word Count List
var wordListArea = null;
var wordListAreaLoading = null;
var wordListTable = null;

function loadWordList() {
	if(wordListLoad && wordListLoad.parentNode) {
		wordListLoad.parentNode.removeChild(wordListLoad);
	}
	wordListLoad = undefined;
	wordListAreaLoading.innerHTML = "<span>Loading...</span>";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/word-count/list/?count=" + counts['wordList'] +
	"&serverId=" + serverId + "&start=0" + "&sort=" + sortOrders['wordList'] + "&isDesc=" + isDesc['wordList']) + 
	(filterInput['wordList'] ? ("&wordFilter=" + encodeURIComponent(filterInput['wordList'])) : '') + 
	(filterInput['wordListFloor'] ? ("&lengthFloor=" + filterInput['wordListFloor']) : '') +
	"&includeTotal=true&userFilterId=" + filterInput['wordListId'] + "&englishOnly=" + filterInput['wordListEnglish'], 
	wordListSucccess, wordListFailure, '');
}

function wordListSucccess(resp) {
	wordListAreaLoading.innerHTML = "";
	wordListTable.innerHTML = buildWordList(resp.results);
}

function buildWordList(data) {
	var html = '\
	<tr class="list-table-header-row">\
		<th class="list-table-header header-sortable" onclick="changeSort(\'wordList\', \'word\')">Name' + getSortArrow('wordList', 'word') +'</th>\
		<th class="list-table-header">Rank</th>\
		<th class="list-table-header header-sortable" onclick="changeSort(\'wordList\', \'count\')">Use Count' + getSortArrow('wordList', 'count') +'</th>\
	</tr>';
	for(var i = 0; i < data.length; ++i) {
		var item = data[i];
		html += '\
		<tr>\
			<td class="list-table-cell"\>' + item.word + '</td>\
			<td class="list-table-cell">' + item.rank + '</td>\
			<td class="list-table-cell">' + item.useCount + '</td>\
		</tr>\
		';
	}
	html+= '\
	<tr class="list-table-footer first-row">\
		<td class="footer-left">\
			<span>Limit: </span>\
			<select id="word-list-limit-dd" onchange="changeLimit(\'wordList\', \'word-list-limit-dd\')">\
				<option value="10"' + (counts['wordList'] == 10 ? 'selected="selected"' : '' ) + '>10</option>\
				<option value="25"' + (counts['wordList'] == 25 ? 'selected="selected"' : '' ) + '>25</option>\
				<option value="50"' + (counts['wordList'] == 50 ? 'selected="selected"' : '' ) + '>50</option>\
			</select>\
			<input id="word-list-english-filter" type="checkbox" value="' + 
			filterInput['wordList'] + '" onclick="changeFilterCheck(\'wordListEnglish\', \'word-list-english-filter\')" ' + 
			(filterInput['wordListEnglish'] == true ? ' checked' : '') + '/>\
			<span>English Only</span>\
		</td>\
		<td class="footer-mid">\
		</td>\
		<td class="footer-right">\
			<div class="footer-container">\
				<input id="word-list-name-filter" placeholder="Filter by word" value="' + filterInput['wordList'] + '"/>\
				<button onclick="changeFilter(\'wordList\', \'word-list-name-filter\')">Filter</button>\
			</div>\
		</td>\
	</tr>\
	<tr class="list-table-footer second-row">\
		<td class="footer-left">\
			<div class="footer-container">\
				<input id="word-list-floor-filter" placeholder="Word Min Length" value="' + filterInput['wordListFloor'] + '"/>\
				<button onclick="changeFilter(\'wordListFloor\', \'word-list-floor-filter\')">Filter</button>\
			</div>\
		</td>\
		<td class="footer-mid">\
		</td>\
		<td class="footer-right">\
			<div class="footer-container">\
				<input id="word-list-id-filter" placeholder="Filter by UserID" value="' + filterInput['wordListId'] + '"/>\
				<button onclick="changeFilter(\'wordListId\', \'word-list-id-filter\')">Filter</button>\
			</div>\
		</td>\
	</tr>';
	return html;
}

function wordListFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
	}
	wordListAreaLoading.innerHTML = "<span>" + message + "</span>";
}
//#endregion

//#region stats
var serverStats = undefined;
var statsUserCount = undefined;
var statsUniqueUserCount = undefined;

function loadStats() {
	loadUserCountStats();
	loadUniqueUserStats();
}

function changeStatDateGroup(type, increase) {
	if(increase) {
		if(sortOrders[type] < 4) {
			sortOrders[type]++;
		} else {
			return;
		}
	} else {
		if(sortOrders[type] > 0) {
			sortOrders[type]--;
		} else {
			return;
		}
	}
	
	if(type === 'statUCnt') {
		document.getElementById('stat-u-cnt-down').disabled = false;
		document.getElementById('stat-u-cnt-up').disabled = false;
		if(sortOrders[type] === 0) {
			document.getElementById('stat-u-cnt-down').disabled = true;
		} else if(sortOrders[type] === 4) {
			document.getElementById('stat-u-cnt-up').disabled = true;
		}
		loadUserCountStats();
	} else if(type === 'statUnU') {
		document.getElementById('stat-un-u-down').disabled = false;
		document.getElementById('stat-un-u-up').disabled = false;
		if(sortOrders[type] === 0) {
			document.getElementById('stat-un-u-down').disabled = true;
		} else if(sortOrders[type] === 4) {
			document.getElementById('stat-un-u-up').disabled = true;
		}
		loadUniqueUserStats();
	}
}

function loadUserCountStats() {
	var stDate = new Date();
	stDate = setDateForStat(stDate, 'statUCnt');
	document.getElementById("user-count-stat-loading").innerHTML = "<span>Loading...</span>";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/stats/?serverId=" + serverId +
	"&type=0&startDate=" + stDate.toISOString()+ "&dateGroup=" + sortOrders['statUCnt']), 
	statUserCountSuccess, statUserCountFailure, '');
}

function statUserCountSuccess(resp) {
	document.getElementById('user-count-stat-chart').innerHTML = "";
	buildStatValueChart(resp.results, 'user-count-stat-chart', 'User Count', 'statUCnt');
	document.getElementById("user-count-stat-loading").innerHTML = getStringForDateGroup(sortOrders['statUCnt']);;
	loadProgress++;
}

function statUserCountFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
	}
	document.getElementById("user-count-stat-loading").innerHTML = "<span>" + message + "</span>";
	loadProgress++;
}

function loadUniqueUserStats() {
	var stDate = new Date();
	stDate = setDateForStat(stDate, 'statUnU');
	document.getElementById("unique-user-stat-loading").innerHTML = "<span>Loading...</span>";
	getJSON(("https://server.icebingo.io:25563/api/v1/discord/stats/?serverId=" + serverId +
	"&type=1&startDate=" + stDate.toISOString() + "&dateGroup=" + sortOrders['statUnU']), 
	statUniqueUserSuccess, statUniqueUserFailure, '');
}

function statUniqueUserSuccess(resp) {
	document.getElementById('unique-user-stat-chart').innerHTML = "";
	buildStatValueChart(resp.results, 'unique-user-stat-chart', 'Unique Users', 'statUnU');
	document.getElementById("unique-user-stat-loading").innerHTML = getStringForDateGroup(sortOrders['statUnU']);
	loadProgress++;
}

function statUniqueUserFailure(resp) {
	var message = undefined;
	if(!resp.responseJSON) {
		message = "There was an error in handling an error.";
	} else {
		message = resp.responseJSON.Message;
	}
	if(!message) {
		message = "There was an error in handling an error.";
	}
	document.getElementById("unique-user-stat-loading").innerHTML = "<span>" + message + "</span>";
	loadProgress++;
}

function setDateForStat(date, type) {
	switch(sortOrders[type]) {
		case 0:
			date.setHours(date.getHours()-24);
			break;
		case 1:
			date.setDate(date.getDate()-12);
			break;
		case 2:
			date.setDate(date.getDate()-56);
			break;
		case 3:
			date.setMonth(date.getMonth()-6)
			break;
		case 4:
			date.setYear(date.getYear()-6)
			break;
	}
	
	return date;
}

function getStringForDateGroup(group) {
	switch(group) {
		case 0:
			return "Hour";
		case 1:
			return "Day";
		case 2:
			return "Week";
		case 3:
			return "Month";
		case 4:
			return "Year";
	}
}

function buildStatValueChart(data, elementId, valueTitle, type) {
	var chartData = new google.visualization.DataTable();
	chartData.addColumn('string', 'Date');
	chartData.addColumn('number', valueTitle);
	var rowsToAdd = [];
	for(var i = 0; i < data.length; ++i) {
		var date = getStatDateFormat(data[i].date, type);
		rowsToAdd.push([date, data[i].statValue]);
	}
	chartData.addRows(rowsToAdd);
	
	var options = {
		title: '',
		width: statsUniqueUserCount.offsetWidth-10,
		height: 300,
		vAxis: { format: 'decimal', gridlines: {color: '#818181'}, baselineColor: '#818181' },
		hAxis: { },
		legend: 'none',
		backgroundColor: '#393939',
		colors: ['#738bd7']
	};
	var chart = new google.visualization.LineChart(document.getElementById(elementId));
	chart.draw(chartData, options);
	var textBlocks = $('#' + elementId).find("text");
	for(var i = 0; i < textBlocks.length; ++i) {
		textBlocks.attr("fill", 'rgba(255,255,255,.7)');
	}
}

function getStatDateFormat(date, type) {
	var ret = "";
	switch(sortOrders[type]) {
		case 0:
			ret = moment(date).format('Do, hh A');
			break;
		case 1:
			ret = moment(date).format('MMM, Do');
			break;
		case 2:
			ret = "Week " + moment(date).format('ww');
			break;
		case 3:
			ret = moment(date).format('YYYY, MMM');
			break;
		case 4:
			ret = moment(date).format('YYYY');
			break;
	}
	
	return ret;
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

function changeFilterCheck(tableType, id) {
	var input = document.getElementById(id);
	if(!input) return;
	filterInput[tableType] = input.checked;
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
	google.charts.setOnLoadCallback(onGoogleLoaded);
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
	wordListArea = document.getElementById('word-table-area');
	wordListAreaLoading = document.getElementById('word-table-loading');
	wordListTable = document.getElementById('word-list-table');
	wordListLoad = document.getElementById('word-counts-load');
	serverStats = document.getElementById('server-stats');
	statsUserCount = document.getElementById('user-count-stat');
	statsUniqueUserCount = document.getElementById('unique-user-stat');
	loadFuncs['messageList']();
	loadFuncs['emojiList']();
	loadRoleList();
	setTimeout(checkLoaded, 250);
	
	function onGoogleLoaded() {
		loadFuncs['stats']();
	}
	
	function checkLoaded() {
		if(loadProgress >= endLoadCount) {
			document.getElementById('fade-parent').style.opacity = "1";
			document.getElementById('loading-parent').style.display = "none";
		} else {
			setTimeout(checkLoaded, 250);
		}
	}
}