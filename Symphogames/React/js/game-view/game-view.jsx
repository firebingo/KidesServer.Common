import React from "react";
import ReactDOM from "react-dom";
import { Redirect } from "react-router-dom";
import { Translate } from "react-localize-redux";
import { Map } from "./map.jsx";
import { TurnControls } from "./turn-controls.jsx";

class GameView extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			error: null,
			isLoaded: false,
			gameInfo: {},
			hasSubmittedTurn: false,
			redirectToLogin: false
		};
		this.submitTurn = this.submitTurn.bind(this);
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
				return (
					<Translate>
						{translate => (
							<div className="centered">{translate('ERROR', { msg: error.message })}</div>
						)}
					</Translate>
				);
			} else if (!isLoaded) {
				return (
					<Translate>
						{translate => (
							<div className="centered">{translate('LOADING')}</div>
						)}
					</Translate>
				);
			} else {
				return (
					<div className="centered flex-column">
						<TurnControls gameInfo={gameInfo} hasSubmittedTurn={hasSubmittedTurn} submitTurn={submitTurn} />
						<Map gameInfo={gameInfo} />
					</div>
				);
			}
		}
	}

	submitTurn(e, action) {
		hasSubmittedTurn = true;
	}
}

export default GameView;