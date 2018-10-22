import React from "react";
import ReactDOM from "react-dom";
import { LocalizeProvider } from "react-localize-redux";
import "./less/main.less";
import Main from "./main.jsx"

const App = props => (
	<LocalizeProvider>
		<Main />
	</LocalizeProvider>
);

function renderView() {
	ReactDOM.render(<App />, document.getElementById("content"));
}

renderView();