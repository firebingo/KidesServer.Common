import React from "react";
import ReactDOM from "react-dom";
import { Redirect } from "react-router-dom";
import { withTranslation } from 'react-i18next';
import Map from "./map.jsx";
import TurnControls from "./turn-controls.jsx";

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
                        hasSubmittedTurn: result.playerInfo.thisPlayer.hasSubmittedTurn,
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
        const { t, i18n } = this.props;
		const { error, isLoaded, gameInfo, redirectToLogin } = this.state;
		if (redirectToLogin) {
			return <Redirect to="/" />;
		} else {
			if (error) {
				return (
					<div className="centered">{t('ERROR', { msg: error.message })}</div>
				);
			} else if (!isLoaded) {
				return (
					<div className="centered">{t('LOADING')}</div>
				);
			} else {
				return (
                    <div className="centered flex-column">
                        <TurnControls gameInfo={gameInfo} hasSubmittedTurn={this.state.hasSubmittedTurn} submitTurn={this.submitTurn} />
						<Map gameInfo={gameInfo} />
					</div>
				);
			}
		}
	}

    submitTurn(e, action) {
        e.preventDefault();
        const accessData = JSON.parse(sessionStorage.getItem("accessData"));
        fetch(`/api/v1/symphogames/submit-turn?gameId=${accessData.gameId}&playerId=${accessData.playerId}&accessGuid=${accessData.accessGuid}`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(action)
            })
            .then(res => res.json())
            .then(
                (result) => {
                    this.setState({ hasSubmittedTurn: true });
                },
                (error) => {

                });
    }
}

export default withTranslation()(GameView);