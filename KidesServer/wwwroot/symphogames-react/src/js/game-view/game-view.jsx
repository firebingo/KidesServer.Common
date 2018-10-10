import React from "react";
import ReactDOM from "react-dom";
import { Map } from "./map.jsx";

export class GameView extends React.Component {
	render() {
		return (
			<div>
				<Map />
			</div>
		);
	}
}