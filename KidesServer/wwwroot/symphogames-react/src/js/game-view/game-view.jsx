import React from "react";
import ReactDOM from "react-dom";
import { Redirect } from "react-router-dom";
import { Map } from "./map.jsx";

export class GameView extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			error: null,
			isLoaded: false,
			gameInfo: {},
			redirectToLogin: false
		};
	}

	componentDidMount() {
		const accessData = JSON.parse(sessionStorage.getItem("accessData"));
		if (!accessData) {
			this.setState({
				redirectToLogin: true
			});
			return;
		}
		fetch(`/api/v1/symphogames/current-player-game-info?gameId=${accessData.gameId}&playerId=${accessData.playerId}&accessGuid=${accessData.accessGuid}`)
			.then(res => res.json())
			.then(
				(result) => {
					this.setState({
						isLoaded: true,
						gameInfo: result
					});
				},
				(error) => {
					this.setState({
						isLoaded: true,
						error
					});
				});
	}

	render() {
		const { error, isLoaded, gameInfo, redirectToLogin } = this.state;
		if (redirectToLogin) {
			return <Redirect to="/" />;
		} else {
			if (error) {
				return <div className="centered">Error: {error.message}</div>;
			} else if (!isLoaded) {
				return <div className="centered">Loading...</div>
			} else {
				return (
					<div className="centered">
						<Map gameInfo={gameInfo} />
					</div>
				);
			}
		}
	}
}