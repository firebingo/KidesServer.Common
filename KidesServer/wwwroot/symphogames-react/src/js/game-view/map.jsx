import React from "react";
import ReactDOM from "react-dom";
import { baseResolution, baseMapSize } from "../globals.js";

export class Map extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			error: null
		};
		this.renderCanvasGrid = this.renderCanvasGrid.bind(this);
		this.onResize = this.onResize.bind(this);
	}

	componentDidMount() {
		this.renderCanvasGrid();
		this.onResize();
		window.addEventListener('resize', this.onResize);
	}

	componentWillUnmount() {
		window.removeEventListener('resize', this.onResize);
	}

	render() {
		return (
			<canvas id="gameMap" width="1024" height="1024"></canvas>
		);
	}

	renderCanvasGrid() {
		const { gameInfo } = this.props;
		const c = document.getElementById("gameMap");
		const ctx = c.getContext("2d");
		const size = gameInfo.mapInfo.map.size;
		ctx.fillStyle = "white";
		ctx.fillRect(0, 0, c.width, c.height);
		ctx.strokeStyle = 'rgba(0, 0, 0, 128)';
		for (let i = 0; i < size.x; ++i) {
			ctx.beginPath();
			const co = Math.ceil(1024 / size.x) * (i + 1);
			ctx.moveTo(co, 0);
			ctx.lineTo(co, 1024);
			ctx.stroke();
		}
		for (let i = 0; i < size.y; ++i) {
			ctx.beginPath();
			const co = Math.ceil(1024 / size.y) * (i + 1);
			ctx.moveTo(0, co);
			ctx.lineTo(1024, co);
			ctx.stroke();
		}
	}

	onResize(ev) {
		const newScale = (window.innerHeight / baseResolution.y); 
		const c = document.getElementById("gameMap");
		c.style = `width: ${baseMapSize.x * newScale}; height: ${baseMapSize.y * newScale};`;
	}
}