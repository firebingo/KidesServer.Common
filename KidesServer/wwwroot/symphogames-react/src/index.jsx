import React from "react";
import ReactDOM from "react-dom";
import './less/main.less';

const Index = () => {
	return <div className="test">Hello React!</div>;
};

ReactDOM.render(<Index />, document.getElementById("content"));