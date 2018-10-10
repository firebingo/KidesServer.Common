import React from "react";
import ReactDOM from "react-dom";
import "./less/main.less";
import { GameView } from "./js/game-view/game-view.jsx"

let inGame = false;
const Index = () => {
	return (
		<div className="centered" style={{display: "flex", flexDirection: "column"}}>
			<input placeholder="Name"></input>
			<input placeholder="Game Id"></input>
			<button onClick={joinGame}>Join</button>
		</div>
	);
};

function renderView() {
	if (!inGame) {
		ReactDOM.render(<Index />, document.getElementById("content"));
	} else {
		ReactDOM.render(<GameView />, document.getElementById("content"));
	}
}

function joinGame() {
	inGame = true;
	renderView();
}

renderView();